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
using Simias.Mail;

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

			//
			// If the Domain Type is "ClientServer" credentials are needed for
			// posting an invitation.  In workgroup, credentials aren't needed.
			//

			Domain domain = store.GetDomain( subscription.DomainID );
			if ( domain.ConfigType == Simias.Storage.Domain.ConfigurationType.ClientServer )
			{
				WebState webState = 
					new WebState( subscription.DomainID, subscription.SubscriptionCollectionID );
				try
				{
					webState.InitializeWebClient( poService );
				}
				catch ( NeedCredentialsException )
				{
					log.Debug( "  no credentials - back to sleep" );
					return result;
				}
			}
			
			// Resolve the PO Box location for the invitee
			Uri poUri = 
				Location.Locate.ResolvePOBoxLocation( subscription.DomainID, subscription.ToIdentity );
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

				POBoxStatus status = poService.Invite( subscription.GenerateSubscriptionMessage() );
				if ( status == POBoxStatus.Success )
				{
					// FIXME:: sync my PostOffice right now!
					subscription.SubscriptionState = SubscriptionStates.Posted;
					poBox.Commit( subscription );
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
			WebState webState = new WebState(subscription.DomainID, subscription.ToIdentity);
			try
			{
				webState.InitializeWebClient(poService);
			}
			catch (NeedCredentialsException)
			{
				log.Debug( "  no credentials - back to sleep" );
				return result;
			}
			
			POBoxStatus	wsStatus = POBoxStatus.UnknownError;

			// Resolve the PO Box location for the inviter
			Uri poUri = 
				Location.Locate.ResolvePOBoxLocation( subscription.DomainID, subscription.FromIdentity );
			if ( poUri != null )
			{
				poService.Url = poUri.ToString() + poServiceLabel;
			}
			else
			{
				log.Debug( "  Could not resolve the PO Box location for: " + subscription.FromIdentity );
				return false;
			}

			log.Debug( "  connecting to the Post Office Service : " + poService.Url );

			try
			{
				if ( subscription.SubscriptionDisposition == SubscriptionDispositions.Accepted )
				{
					log.Debug("  subscription accepted!");

					SubscriptionMsg subMsg = subscription.GenerateSubscriptionMessage();
					wsStatus = poService.AcceptedSubscription( subMsg );
					if ( wsStatus == POBoxStatus.Success )
					{
						subscription.SubscriptionState = SubscriptionStates.Delivered;
						poBox.Commit( subscription );
					}
					else
					if (wsStatus == POBoxStatus.UnknownSubscription)
					{
						log.Debug( "Failed accepting/declining a subscription" );
						log.Debug( "The subscription did not exist on the server" );
						log.Debug( "Deleting the local subscription" );

						poBox.Commit( poBox.Delete( subscription ) );

						// return true so the thread controlling the
						// subscription will die off
						result = true;
					}
					else
					{
						log.Debug(
							"Failed Accepting/Declining a subscription.  Status: " + 
							wsStatus.ToString());
					}
				}
				else
				if ( subscription.SubscriptionDisposition == SubscriptionDispositions.Declined )
				{
					log.Debug("  subscription declined");

					SubscriptionMsg subMsg = subscription.GenerateSubscriptionMessage();
					wsStatus = poService.DeclinedSubscription( subMsg );
					if (wsStatus == POBoxStatus.Success)
					{
						// This subscription is done!
						result = true;
					}
					else
					if ( wsStatus == POBoxStatus.UnknownCollection )
					{
						log.Debug( "Failed declining a subscription" );
						log.Debug( "The Collection did not exist on the server" );
						log.Debug( "Deleting the local subscription" );

						poBox.Commit( poBox.Delete( subscription ) );
						result = true;
					}
				}
			}
			catch(Exception e)
			{
				log.Error("DoReplied failed updating originator's PO box");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return result;
		}

		private bool DoDelivered()
		{
			bool result = false;

			//log.Debug("DoDelivered::Connecting to the Post Office Service : {0}", this.poServiceUrl);
			log.Debug("  calling the PO Box server to get subscription state" );
			log.Debug("  domainID: " + subscription.DomainID );
			log.Debug("  fromID:   " + subscription.FromIdentity );
			log.Debug("  SubID:    " + subscription.MessageID );

			POBoxService poService = new POBoxService();
			WebState webState = new WebState(subscription.DomainID, subscription.ToIdentity);
			try
			{
				webState.InitializeWebClient(poService);
			}
			catch (NeedCredentialsException)
			{
				log.Debug( "  no credentials - back to sleep" );
				return result;
			}
			
			// Resolve the PO Box location for the inviter
			Uri poUri = 
				Location.Locate.ResolvePOBoxLocation( subscription.DomainID, subscription.FromIdentity );
			if ( poUri != null )
			{
				poService.Url = poUri.ToString() + poServiceLabel;
			}
			else
			{
				log.Debug( "  Could not resolve the PO Box location for: " + subscription.FromIdentity );
				return result;
			}

			try
			{
				SubscriptionInformation subInfo =
					poService.GetSubscriptionInfo(
						subscription.DomainID,
						subscription.FromIdentity,
						subscription.MessageID);

				if ( subInfo != null )
				{
					log.Debug( "  subInfo.FromName: " + subInfo.FromName );
					log.Debug( "  subInfo.ToName: " + subInfo.ToName );
					log.Debug( "  subInfo.State: " + subInfo.State.ToString() );
					log.Debug( "  subInfo.Disposition: " + subInfo.Disposition.ToString() );
				}

				// update subscription
				if ( subInfo.State == (int) SubscriptionStates.Responded )
				{
					// create proxy
					if ( subInfo.Disposition == (int) SubscriptionDispositions.Accepted )
					{
						log.Debug( "Creating collection..." );

						// do not re-create the proxy
						if ( poBox.StoreReference.GetCollectionByID( subscription.SubscriptionCollectionID ) == null )
						{
							SubscriptionDetails details = new SubscriptionDetails();
							details.DirNodeID = subInfo.DirNodeID;
							details.DirNodeName = subInfo.DirNodeName;
							details.CollectionUrl = subInfo.CollectionUrl;

							log.Debug("Collection URL: " + subInfo.CollectionUrl);

							subscription.SubscriptionRights = 
								(Simias.Storage.Access.Rights) subInfo.AccessRights;

							// save details
							subscription.AddDetails( details );
							poBox.Commit( subscription );
					
							// create slave stub
							subscription.ToMemberNodeID = subInfo.ToNodeID;
							subscription.CreateSlave( poBox.StoreReference );
						}
						
						// acknowledge the message
						// which removes the originator's 
						SubscriptionMsg subMsg = subscription.GenerateSubscriptionMessage();
						POBoxStatus wsStatus = poService.AckSubscription( subMsg );
						if ( wsStatus == POBoxStatus.Success )
						{
							// done with the subscription - move to local subscription to the ready state
							subscription.SubscriptionState = SubscriptionStates.Ready;
							poBox.Commit( subscription );
						}
						else 
						if (wsStatus == POBoxStatus.UnknownSubscription)
						{
							log.Debug( "Failed acknowledging a subscription" );
							log.Debug( "The subscription did not exist on the server" );
							log.Debug( "Deleting the local subscription" );

							poBox.Commit( poBox.Delete( subscription ) );

							// return true so the thread controlling the
							// subscription will die off
							result = true;
						}
						else
						{
							log.Debug(
								"Failed Acking a subscription.  Status: " + 
								wsStatus.ToString());
						}
					}
					else
					{
						// Remove the subscription from the local PO box
						poBox.Commit( poBox.Delete( subscription ) );
					}

					// done
					result = true;
				}
			}
			catch(Exception e)
			{
				log.Error("SubscriptionThread::DoDelivered failed with an exception");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			return result;
		}
	}
}
