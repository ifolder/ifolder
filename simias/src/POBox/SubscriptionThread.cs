/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.Remoting;

using Simias;
using Simias.Authentication;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// SubscriptionThread
	/// </summary>
	public class SubscriptionThread
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SubscriptionThread));
		private static readonly string	poServiceLabel = "/POService.asmx";
		
		private POBox poBox;
		private Subscription subscription;
		private Hashtable threads;
		//private string poServiceUrl;

		/// <summary>
		/// Constructor
		/// </summary>
		public SubscriptionThread(POBox poBox, Subscription subscription, Hashtable threads)
		{
			this.poBox = poBox;
			this.subscription = subscription;
			this.threads = threads;
		}

		/// <summary>
		/// Thread Run
		/// </summary>
		public void Run()
		{
			bool done = false;

			try
			{
				while( done == false )
				{
					try
					{
						switch(subscription.SubscriptionState)
						{
							// invited (master)
							case SubscriptionStates.Invited:
								done = DoInvited();
								break;

							// TODO: fix and cleanup states
							//case SubscriptionStates.Pending:

							// replied (slave)
							case SubscriptionStates.Replied:
								done = DoReplied();
								break;

							// delivered (slave)
							case SubscriptionStates.Delivered:
								done = DoDelivered();
								break;

							default:
								break;
						}
					}
					catch(Exception e)
					{
						done = false;
						log.Debug(e, "Ignored");
					}

					if (!done)
					{
						Thread.Sleep(TimeSpan.FromSeconds(10));
					}
				}
			}
			catch(Exception e)
			{
				log.Debug(e, "Ignored");
			}
			finally
			{
				lock(threads.SyncRoot)
				{
					threads.Remove(subscription.ID);
				}
			}
		}

		private bool DoInvited()
		{
			bool result = false;
			Store store = Store.GetStore();

			log.Debug( "SubscriptionThread::DoInvited called" );
			POBoxService poService = new POBoxService();

			// Resolve the PO Box location for the invitee
			Uri poUri = DomainProvider.ResolvePOBoxLocation( subscription.DomainID, subscription.ToIdentity );
			if ( poUri != null )
			{
				poService.Url = poUri.ToString() + poServiceLabel;
				log.Debug( "Calling POService::Invite at: " + poService.Url );
			}
			else
			{
				log.Error( "  Could not resolve the PO Box location for: " + subscription.FromIdentity );
				return result;
			}

			//
			// If the Domain Type is "ClientServer" credentials are needed for
			// posting an invitation.  In workgroup, credentials aren't needed.
			//

			Domain domain = store.GetDomain( subscription.DomainID );
			if ( domain.ConfigType == Simias.Storage.Domain.ConfigurationType.ClientServer )
			{
				try
				{
					WebState webState = 
						new WebState( subscription.DomainID, subscription.FromIdentity );

					webState.InitializeWebClient( poService, domain.ID );
				}
				catch ( NeedCredentialsException )
				{
					log.Debug( "  no credentials - back to sleep" );
					return result;
				}
			}
			
			//
			// Make sure the shared collection has sync'd to the server before inviting
			//

			Collection cSharedCollection = 
				store.GetCollectionByID( subscription.SubscriptionCollectionID );
			if ( cSharedCollection == null )
			{
				return result;
			}

			if ( cSharedCollection.Role == SyncRoles.Slave &&
					cSharedCollection.MasterIncarnation == 0 )
			{
				log.Debug(
					"Failed POBoxService::Invite - collection: {0} hasn't sync'd to the server yet",
					subscription.SubscriptionCollectionID);

				return result;
			}

			//
			// Make sure the subscription node has sync'd up to the server as well
			//

			if ( this.poBox.Role == SyncRoles.Slave )
			{
				Node cNode = this.poBox.Refresh( subscription );
				if ( cNode.MasterIncarnation == 0 )
				{
					// force the PO box to be sync'd right away
					SyncClient.ScheduleSync( poBox.ID );

					log.Debug(
						"Failed POBoxService::Invite - inviter's subscription {0} hasn't sync'd to the server yet",
						subscription.MessageID);

					return result;
				}
			}
		
			// Remove location of the POBox service
			log.Debug( "Connecting to the Post Office Service : " + poService.Url );

			try
			{
				// Set the remote state to received.
				// And post the subscription to the server.
				Simias.Storage.Member me = poBox.GetCurrentMember();
				subscription.FromIdentity = me.UserID;
				subscription.FromName = me.Name;

				// Make sure that this user has sufficient rights to send this invitation.
				Simias.Storage.Member me2 = cSharedCollection.GetMemberByID( me.UserID );
				if ( ( me2 == null ) || ( me2.Rights != Access.Rights.Admin ) )
				{
					// The user did not have sufficient rights to send this invitation.
					// Delete the subscription and don't try to send it anymore.
					log.Info( "User {0} did not have sufficient rights to share collection {1}", me.Name, cSharedCollection.Name );
					poBox.Commit( poBox.Delete( subscription ) );
					Sync.SyncClient.ScheduleSync( poBox.ID );
					return true;
				}

				POBoxStatus status = poService.Invite( subscription.GenerateSubscriptionMessage() );
				if ( status == POBoxStatus.Success )
				{
					subscription.SubscriptionState = SubscriptionStates.Posted;
					poBox.Commit( subscription );
					Sync.SyncClient.ScheduleSync( poBox.ID );
					result = true;
				}
				else if ( status == POBoxStatus.InvalidAccessRights )
				{
					// The user did not have sufficient rights to send this invitation.
					// Delete the subscription and don't try to send it anymore.
					log.Info( "User {0} did not have sufficient rights to share collection {1}", me.Name, cSharedCollection.Name );
					poBox.Commit( poBox.Delete( subscription ) );
					Sync.SyncClient.ScheduleSync( poBox.ID );
					result = true;
				}
				else if ( status == POBoxStatus.UnknownIdentity )
				{
					// The invited user no longer exists on the server roster
					// possibly due to scoping changes by the administrator
					log.Info( "User {0} does not exist in the server domain", me.Name );
					poBox.Commit( poBox.Delete( subscription ) );
					Sync.SyncClient.ScheduleSync( poBox.ID );
					result = true;
				}
				else
				{
					log.Debug( "Failed the remote invite call -  Status: " + status.ToString() );
				}
			}
			catch
			{
				log.Debug( "Failed POBoxService::Invite - target: " + poService.Url );
			}

			return result;
		}

		private bool DoReplied()
		{
			log.Debug( "DoReplied" );
			log.Debug( "  calling the PO Box server to accept/reject subscription" );
			log.Debug( "  domainID: " + subscription.DomainID );
			log.Debug( "  fromID:   " + subscription.FromIdentity );
			log.Debug( "  toID:     " + subscription.ToIdentity );
			log.Debug( "  SubID:    " + subscription.MessageID );

			bool result = false;
			POBoxService poService = new POBoxService();
			POBoxStatus	wsStatus = POBoxStatus.UnknownError;

			// Resolve the PO Box location for the inviter
			Uri poUri = DomainProvider.ResolvePOBoxLocation( subscription.DomainID, subscription.FromIdentity );
			if ( poUri != null )
			{
				poService.Url = poUri.ToString() + poServiceLabel;
				log.Debug( "  connecting to the Post Office Service : " + poService.Url );
			}
			else
			{
				log.Debug( "  could not resolve the PO Box location for: " + subscription.FromIdentity );
				return result;
			}

			try
			{
				WebState webState = 
				new WebState(
					subscription.DomainID, 
					subscription.SubscriptionCollectionID, 
					subscription.ToIdentity);

				webState.InitializeWebClient(poService, subscription.DomainID);
			}
			catch (NeedCredentialsException)
			{
				log.Debug( "  no credentials - back to sleep" );
				return result;
			}

			// Add a new member to the domain if one doesn't exist already.  This is needed
			// for Workgroup Domains.  If the user doesn't exist, the upcoming call to
			// ResolvePOBoxLocation will fail.
			Domain domain = Store.GetStore().GetDomain( subscription.DomainID );
			if ( domain.ConfigType == Domain.ConfigurationType.Workgroup )
			{
				Member member = domain.GetMemberByName( subscription.FromName );
				if ( member == null )
				{
					member = new Member( subscription.FromName, subscription.FromIdentity, subscription.SubscriptionRights );
					domain.Commit( member );
				}
			}

			try
			{
				if ( subscription.SubscriptionDisposition == SubscriptionDispositions.Accepted )
				{
					log.Debug("  disposition is accepted!");

					SubscriptionMsg subMsg = subscription.GenerateSubscriptionMessage();
					wsStatus = poService.AcceptedSubscription( subMsg );
					switch ( wsStatus )
					{
						case POBoxStatus.Success:
						{
							log.Debug( "  successfully accepted subscription." );

							subscription.SubscriptionState = SubscriptionStates.Delivered;
							poBox.Commit( subscription );
							Sync.SyncClient.ScheduleSync( poBox.ID );
							break;
						}

						case POBoxStatus.NotPosted:
						{
							log.Debug( "  waiting for subscription to be posted." );
							break;
						}

						case POBoxStatus.AlreadyAccepted:
						{
							log.Debug( "  subscription already accepted from a different client." );
							result = CreateCollectionProxy();
							break;
						}

						case POBoxStatus.AlreadyDeclined:
						case POBoxStatus.UnknownCollection:
						case POBoxStatus.UnknownSubscription:
						case POBoxStatus.UnknownPOBox:
						case POBoxStatus.UnknownIdentity:
						{
							log.Debug( "  failed accepting a subscription" );
							log.Debug( "  status = {0}", wsStatus.ToString() );
							log.Debug( "  deleting the local subscription" );

							// Delete the subscription and return true so the thread 
							// controlling the subscription will die off.
							poBox.Commit( poBox.Delete( subscription ) );
							Sync.SyncClient.ScheduleSync( poBox.ID );
							result = true;
							break;
						}

						default:
						{
							log.Debug( "  failed Accepting a subscription.  Status: " + wsStatus.ToString());
							break;
						}
					}
				}
				else if ( subscription.SubscriptionDisposition == SubscriptionDispositions.Declined )
				{
					log.Debug("  disposition is declined");

					SubscriptionMsg subMsg = subscription.GenerateSubscriptionMessage();
					wsStatus = poService.DeclinedSubscription( subMsg );
					switch ( wsStatus )
					{
						case POBoxStatus.Success:
						{
							// This subscription is done!
							Sync.SyncClient.ScheduleSync( poBox.ID );
							result = true;
							break;
						}

						case POBoxStatus.AlreadyAccepted:
						{
							log.Debug( "  subscription already accepted from a different client." );

							// If the collection has already been accepted on another client, we cannot
							// automatically accept it here because we don't know where to root the
							// the collection in the file system. Just sync the subscription from the
							// server and force the client to re-accept.
							Sync.SyncClient.ScheduleSync( poBox.ID );
							result = true;
							break;
						}

						case POBoxStatus.AlreadyDeclined:
						case POBoxStatus.UnknownIdentity:
						case POBoxStatus.UnknownCollection:
						case POBoxStatus.UnknownPOBox:
						{
							log.Debug( "  failed declining a subscription" );
							log.Debug( "  status = {0}", wsStatus.ToString() );
							log.Debug( "  deleting the local subscription" );

							// Delete the subscription and return true so the thread 
							// controlling the subscription will die off.
							poBox.Commit( poBox.Delete( subscription ) );
							Sync.SyncClient.ScheduleSync( poBox.ID );
							result = true;
							break;
						}

						default:
						{
							log.Debug( "  failed declining a subscription.  Status: " + wsStatus.ToString());
							break;
						}
					}
				}
			}
			catch(Exception e)
			{
				log.Error("  DoReplied failed updating originator's PO box");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return result;
		}

		private bool DoDelivered()
		{
			bool result = false;
			POBoxService poService = new POBoxService();

			log.Debug("  calling the PO Box server to get subscription state" );
			log.Debug("  domainID: " + subscription.DomainID );
			log.Debug("  fromID:   " + subscription.FromIdentity );
			log.Debug("  SubID:    " + subscription.MessageID );

			// Resolve the PO Box location for the inviter
			Uri poUri = DomainProvider.ResolvePOBoxLocation( subscription.DomainID, subscription.FromIdentity );
			if ( poUri != null )
			{
				poService.Url = poUri.ToString() + poServiceLabel;
				log.Debug( "  connecting to the Post Office Service : " + poService.Url );
			}
			else
			{
				log.Debug( "  Could not resolve the PO Box location for: " + subscription.FromIdentity );
				return result;
			}

			try
			{
				WebState webState = 
					new WebState(
						subscription.DomainID, 
						subscription.SubscriptionCollectionID, 
						subscription.ToIdentity);

				webState.InitializeWebClient(poService, subscription.DomainID);
			}
			catch (NeedCredentialsException)
			{
				log.Debug( "  no credentials - back to sleep" );
				return result;
			}
			
			try
			{
				SubscriptionInformation subInfo =
					poService.GetSubscriptionInfo(
						subscription.DomainID,
						subscription.FromIdentity,
						subscription.MessageID,
						subscription.SubscriptionCollectionID );

				switch ( subInfo.Status )
				{
					case POBoxStatus.Success:
					{
						log.Debug( "  subInfo.FromName: " + subInfo.FromName );
						log.Debug( "  subInfo.ToName: " + subInfo.ToName );
						log.Debug( "  subInfo.State: " + subInfo.State.ToString() );
						log.Debug( "  subInfo.Disposition: " + subInfo.Disposition.ToString() );

						if ( subInfo.Disposition == ( int )SubscriptionDispositions.Accepted )
						{
							log.Debug( "  creating collection..." );

							// Check to make sure the collection proxy doesn't already exist.
							if ( poBox.StoreReference.GetCollectionByID( subscription.SubscriptionCollectionID ) == null )
							{
								SubscriptionDetails details = new SubscriptionDetails();
								details.DirNodeID = subInfo.DirNodeID;
								details.DirNodeName = subInfo.DirNodeName;

								subscription.SubscriptionRights = (Access.Rights)subInfo.AccessRights;
								subscription.AddDetails( details );
								poBox.Commit( subscription );
								Sync.SyncClient.ScheduleSync( poBox.ID );

								// create slave stub
								subscription.ToMemberNodeID = subInfo.ToNodeID;
								subscription.CreateSlave( poBox.StoreReference );
							}
				
							// acknowledge the message which removes the originator's subscription.
							SubscriptionMsg subMsg = subscription.GenerateSubscriptionMessage();
							POBoxStatus wsStatus = poService.AckSubscription( subMsg );
							if ( ( wsStatus == POBoxStatus.Success ) ||
									( wsStatus == POBoxStatus.AlreadyAccepted ) )
							{
								// done with the subscription - move to local subscription to the ready state
								subscription.SubscriptionState = SubscriptionStates.Ready;
								poBox.Commit( subscription );
								Sync.SyncClient.ScheduleSync( poBox.ID );
								result = true;
							}
							else
							{
								log.Debug( "  failed acknowledging a subscription" );
								log.Debug( "  status = {0}", wsStatus.ToString() );
								log.Debug( "  deleting the local subscription" );

								poBox.Commit( poBox.Delete( subscription ) );
								Sync.SyncClient.ScheduleSync( poBox.ID );
								result = true;
							}
						}
						break;
					}

					case POBoxStatus.AlreadyAccepted:
					{
						log.Debug( "  the subscription has already been accepted by another client." );
						result = CreateCollectionProxy();
						break;
					}

					case POBoxStatus.InvalidState:
					{
						log.Debug ( "  invalid state" );
						break;
					}

					default:
					{
						log.Debug( "  server error = {0}. Deleting subscription.", subInfo.Status.ToString() );
						poBox.Commit( poBox.Delete( subscription ) );
						Sync.SyncClient.ScheduleSync( poBox.ID );
						result = true;
						break;
					}
				}
			}
			catch(Exception e)
			{
				log.Error("  DoDelivered failed with an exception");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return result;
		}

		private bool CreateCollectionProxy()
		{
			bool createdProxy = false;

			// Refresh the subscription.
			subscription = poBox.Refresh( subscription ) as Subscription;

			// See if the member Node ID is specified yet.
			if ( subscription.ToMemberNodeID != null )
			{
				// Check to make sure the collection proxy doesn't already exist.
				if ( poBox.StoreReference.GetCollectionByID( subscription.SubscriptionCollectionID ) == null )
				{
					// create slave stub
					subscription.CreateSlave( poBox.StoreReference );
				}

				createdProxy = true;
			}

			// Sync the POBox to force the subscription to update from the server.
			Sync.SyncClient.ScheduleSync( poBox.ID  );
			return createdProxy;
		}
	}
}
