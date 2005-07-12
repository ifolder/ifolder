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
	/// SubscriptionService
	/// </summary>
	public class SubscriptionService
	{
		#region Class Members

		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SubscriptionService));
		private static readonly string	poServiceLabel = "/POService.asmx";

		/// <summary>
		/// Queue used to hold subscriptions for processing.
		/// </summary>
		private Queue subQueue = new Queue();

		/// <summary>
		/// Table used for quick lookup of subscription information.
		/// </summary>
		private Hashtable subTable = new Hashtable();

		/// <summary>
		/// Event used to signal thread that items have been placed on the queue.
		/// </summary>
		private AutoResetEvent subEvent = new AutoResetEvent( false );

		/// <summary>
		/// Post office box for this service.
		/// </summary>
		private POBox poBox;

		/// <summary>
		/// Tells the subscription thread to exit.
		/// </summary>
		private bool killThread = false;

		#endregion
		
		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="poBox">The POBox for this service.</param>
		public SubscriptionService( POBox poBox )
		{
			this.poBox = poBox;

			// Start the subscription service thread running.
			Thread thread = new Thread( new ThreadStart( Run ) );
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Adds subscription information to be processed if it isn't already
		/// being processed.
		/// </summary>
		/// <param name="qItem">A SubQItem object that will be placed on the queue.</param>
		/// <returns>True if the subscription was queued for processing. False
		/// if the subscription was already being processed.</returns>
		private bool QueueSubscription( SubQItem qItem )
		{
			bool exists = true;

			lock( typeof( SubscriptionService ) )
			{
				if ( !subTable.ContainsKey( qItem.Subscription.ID ) )
				{
					subQueue.Enqueue( qItem );
					subTable.Add( qItem.Subscription.ID, null );
					exists = false;
				}
			}

			return exists;
		}

		/// <summary>
		/// Dequeues the subscription information from the head of the queue.
		/// </summary>
		/// <param name="waitTime">The amount of time to wait before </param>
		/// <returns>A SubQItem object if one exists on the queue. Otherwise
		/// null is returned.</returns>
		private SubQItem DequeueSubscription( out int waitTime )
		{
			SubQItem qItem = null;
			waitTime = Timeout.Infinite;

			// Get a lock on the queue.
			lock( typeof( SubscriptionService ) )
			{
				int nextProcessTime = Int32.MaxValue;

				for ( int i = 0; i < subQueue.Count; ++i )
				{
					// Remove the item from the head of the queue.
					SubQItem tempItem = subQueue.Dequeue() as SubQItem;
					subTable.Remove( tempItem.Subscription.ID );

					// Check if this item is ready to be processed.
					TimeSpan ts = tempItem.ProcessTime - DateTime.Now;
					int tempProcessTime = Convert.ToInt32( ts.TotalMilliseconds );
					if ( tempProcessTime <= 0 )
					{
						// The item is ready to be processed.
						qItem = tempItem;
						break;
					}
					else
					{
						// This item needs to wait longer before being processed.
						// Put it on the tail of the queue.
						subQueue.Enqueue( tempItem );
						subTable.Add( tempItem.Subscription.ID, null );

						// Always take the lowest next process time.
						if ( nextProcessTime > tempProcessTime )
						{
							nextProcessTime = tempProcessTime;
						}
					}
				}

				// If there are still entries on the queue, but none being returned,
				// set the wait time for the next item process time.
				if ( ( subQueue.Count > 0 ) && ( qItem == null ) )
				{
					waitTime = nextProcessTime;
				}
			}

			return qItem;
		}

		/// <summary>
		/// Thread Run
		/// </summary>
		private void Run()
		{
			while ( !killThread )
			{
				try
				{
					int waitTime;

					// Get a subscription item.
					SubQItem qItem = DequeueSubscription( out waitTime );
					if ( qItem != null )
					{
						bool done = false;
						switch( qItem.Subscription.SubscriptionState )
						{
								// invited (master)
							case SubscriptionStates.Invited:
								done = DoInvited( qItem.Subscription );
								break;

								// replied (slave)
							case SubscriptionStates.Replied:
								done = DoReplied( qItem.Subscription );
								break;

								// delivered (slave)
							case SubscriptionStates.Delivered:
								done = DoDelivered( qItem.Subscription );
								break;
						}

						if (!done)
						{
							// Put the item back onto the queue and process the next item
							// that is on the queue.
							qItem.ProcessTime = DateTime.Now + TimeSpan.FromSeconds( 10 );
							QueueSubscription( qItem );
						}
					}
					else
					{
						// Sync the POBox before going to sleep.
						Sync.SyncClient.ScheduleSync( poBox.ID );

						// Wait for an item to be placed on the queue.
						subEvent.WaitOne( waitTime, true );
					}
				}
				catch( Exception e )
				{
					log.Debug( e, "Exception in subscription thread - Ignored" );
					Thread.Sleep( 10 * 1000 );
				}
			}
		}

		private bool DoInvited( Subscription subscription )
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
					WebState webState = new WebState( subscription.DomainID, subscription.FromIdentity );
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

			Collection cSharedCollection = store.GetCollectionByID( subscription.SubscriptionCollectionID );
			if ( cSharedCollection == null )
			{
				return result;
			}

			if ( cSharedCollection.Role == SyncRoles.Slave && cSharedCollection.MasterIncarnation == 0 )
			{
				log.Debug(
					"Failed POBoxService::Invite - collection: {0} hasn't sync'd to the server yet",
					subscription.SubscriptionCollectionID);

				return result;
			}

			//
			// Make sure the subscription node has sync'd up to the server as well
			//

			if ( poBox.Role == SyncRoles.Slave )
			{
				Node cNode = poBox.Refresh( subscription );
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
					return true;
				}

				POBoxStatus status = poService.Invite( subscription.GenerateSubscriptionMessage() );
				if ( status == POBoxStatus.Success )
				{
					log.Debug( "Successfully invited {0} to collection {1}.", subscription.FromName, subscription.SubscriptionCollectionName );
					subscription.SubscriptionState = SubscriptionStates.Posted;
					poBox.Commit( subscription );
					result = true;
				}
				else if ( status == POBoxStatus.InvalidAccessRights )
				{
					// The user did not have sufficient rights to send this invitation.
					// Delete the subscription and don't try to send it anymore.
					log.Info( "User {0} did not have sufficient rights to share collection {1}", me.Name, cSharedCollection.Name );
					poBox.Commit( poBox.Delete( subscription ) );
					result = true;
				}
				else if ( status == POBoxStatus.UnknownIdentity )
				{
					// The invited user no longer exists on the server roster
					// possibly due to scoping changes by the administrator
					log.Info( "User {0} does not exist in the server domain", me.Name );
					poBox.Commit( poBox.Delete( subscription ) );
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

		private bool DoReplied( Subscription subscription )
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
					log.Debug( "  added workgroup member {0}", subscription.FromName );
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
							result = CreateCollectionProxy( poBox, subscription );
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
							log.Debug( "  successfully declined subscription {0}", subscription.ID );
							result = true;
							break;
						}

						case POBoxStatus.AlreadyAccepted:
						{
							log.Debug( "  subscription already accepted from a different client." );

							// If the collection has already been accepted on another client, we cannot
							// automatically accept it here because we don't know where to root the
							// the collection in the file system.
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

		private bool DoDelivered( Subscription subscription )
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
								log.Debug( "  moving subscription to ready state." );
								subscription.SubscriptionState = SubscriptionStates.Ready;
								poBox.Commit( subscription );
								result = true;
							}
							else
							{
								log.Debug( "  failed acknowledging a subscription" );
								log.Debug( "  status = {0}", wsStatus.ToString() );
								log.Debug( "  deleting the local subscription" );

								poBox.Commit( poBox.Delete( subscription ) );
								result = true;
							}
						}
						break;
					}

					case POBoxStatus.AlreadyAccepted:
					{
						log.Debug( "  the subscription has already been accepted by another client." );
						result = CreateCollectionProxy( poBox, subscription );
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

		private bool CreateCollectionProxy( POBox poBox, Subscription subscription )
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
					log.Debug( "  created collection proxy." );
				}

				createdProxy = true;
			}

			return createdProxy;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds subscription information to be processed if it isn't already
		/// being processed.
		/// </summary>
		/// <param name="subscription">The Subscription to process.</param>
		/// <returns>True if the subscription was queued for processing. False
		/// if the subscription was already being processed.</returns>
		public bool QueueSubscription( Subscription subscription )
		{
			bool exists = QueueSubscription( new SubQItem( subscription ) );

			// If the subscription was queued, signal the thread to process it.
			if ( !exists )
			{
				log.Debug( "Queued subscription {0} for processing.", subscription.ID );
				subEvent.Set();
			}
			else
			{
				log.Debug( "Subscription {0} is already being processed.", subscription.ID );
			}

			return exists;
		}

		/// <summary>
		/// Stops the subscription service thread.
		/// </summary>
		public void Stop()
		{
			lock( typeof( SubscriptionService ) )
			{
				subQueue.Clear();
				subTable.Clear();
			}

			killThread = true;
			subEvent.Set();
			log.Debug( "Subscription service stopped." );
		}

		#endregion

		#region SubQItem Class

		/// <summary>
		/// Class used to keep track of subscription state items while on the
		/// subscription queue.
		/// </summary>
		private class SubQItem
		{
			#region Class Members

			private Subscription subscription;
			private DateTime processTime;

			#endregion

			#region Properties

			/// <summary>
			/// Gets the subscription for this instance.
			/// </summary>
			public Subscription Subscription
			{
				get { return subscription; }
			}

			/// <summary>
			/// Gets or set the wait time before processing this item.
			/// </summary>
			public DateTime ProcessTime
			{
				get { return processTime; }
				set { processTime = value; }
			}

			#endregion

			#region Constructor

			/// <summary>
			/// Initializes an instance of the object.
			/// </summary>
			/// <param name="subscription">The subscription to be serviced.</param>
			public SubQItem( Subscription subscription )
			{
				this.subscription = subscription;
				this.processTime = DateTime.Now;
			}

			#endregion
		}

		#endregion
	}
}
