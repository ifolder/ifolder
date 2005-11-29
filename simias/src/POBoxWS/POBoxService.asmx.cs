/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright © Unpublished Work of Novell, Inc. All Rights Reserved.
 *
 *  THIS WORK IS AN UNPUBLISHED WORK AND CONTAINS CONFIDENTIAL,
 *  PROPRIETARY AND TRADE SECRET INFORMATION OF NOVELL, INC. ACCESS TO 
 *  THIS WORK IS RESTRICTED TO (I) NOVELL, INC. EMPLOYEES WHO HAVE A 
 *  NEED TO KNOW HOW TO PERFORM TASKS WITHIN THE SCOPE OF THEIR 
 *  ASSIGNMENTS AND (II) ENTITIES OTHER THAN NOVELL, INC. WHO HAVE 
 *  ENTERED INTO APPROPRIATE LICENSE AGREEMENTS. NO PART OF THIS WORK 
 *  MAY BE USED, PRACTICED, PERFORMED, COPIED, DISTRIBUTED, REVISED, 
 *  MODIFIED, TRANSLATED, ABRIDGED, CONDENSED, EXPANDED, COLLECTED, 
 *  COMPILED, LINKED, RECAST, TRANSFORMED OR ADAPTED WITHOUT THE PRIOR 
 *  WRITTEN CONSENT OF NOVELL, INC. ANY USE OR EXPLOITATION OF THIS 
 *  WORK WITHOUT AUTHORIZATION COULD SUBJECT THE PERPETRATOR TO 
 *  CRIMINAL AND CIVIL LIABILITY.  
 *
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;

namespace Simias.POBoxService.Web
{
	/// <summary>
	/// Status codes returned from POBoxService methods
	/// </summary>
	[Serializable]
	public enum POBoxStatus
	{
		/// <summary>
		/// The method was successful.
		/// </summary>
		Success,

		/// <summary>
		/// The specified PO Box was not found.
		/// </summary>
		UnknownPOBox,

		/// <summary>
		/// The specified identity was not found in the domain.
		/// </summary>
		UnknownIdentity,

		/// <summary>
		/// The specified collection was not found.
		/// </summary>
		UnknownCollection,

		/// <summary>
		/// The specified domain was not found.
		/// </summary>
		UnknownDomain,

		/// <summary>
		/// The suscription was in an invalid state for the method
		/// </summary>
		InvalidState,

		/// <summary>
		/// The access rights were invalid during an inviate
		/// </summary>
		InvalidAccessRights,

		/// <summary>
		/// An unknown error was realized.
		/// </summary>
		UnknownError
	};

	/// <summary>
	/// Object used for inviting, accepting/declining subscriptions etc.
	/// </summary>
	[Serializable]
	public class SubscriptionMsg
	{
		/// <summary>
		/// Domain to invite and accept on
		/// </summary>
		public string DomainID;

		/// <summary>
		/// The ID of the user who sent the subscription
		/// </summary>
		public string FromID;

		/// <summary>
		/// The name of the user who sent the subscription
		/// </summary>
		public string FromName;

		/// <summary>
		/// The ID of the user who received the subscription
		/// </summary>
		public string ToID;

		/// <summary>
		/// The ID of the originating subscription
		/// Subscription ID are consistent in both the
		/// sender's and receiver's PO Boxes
		/// </summary>
		public string SubscriptionID;

		/// <summary>
		/// The ID of the collection the sender is wanting
		/// to share.
		/// </summary>
		public string SharedCollectionID;

		/// <summary>
		/// The friendly name of the collection the sender
		/// is wanting to share
		/// </summary>
		public string SharedCollectionName;

		/// <summary>
		/// The type of collection the sender is wanting
		/// to share
		/// </summary>
		public string SharedCollectionType;

		/// <summary>
		/// If the shared collection contains a directory
		/// node, the id will be set on the invite
		/// </summary>
		public string DirNodeID;

		/// <summary>
		/// If the shared collection contains a directory
		/// node, the node's name will be set on the invite
		/// </summary>
		public string DirNodeName;

		/// <summary>
		/// Access rights the sender is wishing to grant
		/// to the receiver for the shared collection
		/// This member is really only valid on the
		/// invite method
		/// </summary>
		public int	AccessRights;
	};

	/// <summary>
	/// Summary description for POBoxService
	/// </summary>
	/// 
	[WebService(Namespace="http://novell.com/simias/pobox/")]
	public class POBoxService : System.Web.Services.WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBoxService));

		/// <summary>
		/// </summary>
		public POBoxService()
		{
		}

		/// <summary>
		/// Ping
		/// Method for clients to determine if POBoxService is
		/// up and running.
		/// </summary>
		/// <param name="sleepFor"></param>
		/// <returns>0</returns>
		[WebMethod(EnableSession = true)]
		public int Ping(int sleepFor)
		{
			Thread.Sleep(sleepFor * 1000);
			return 0;
		}

		/// <summary>
		/// Accept subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus AcceptedSubscription( SubscriptionMsg subMsg )
		{
			log.Debug( "DEBUG - Obsolete AcceptedSubscription called" );
			return POBoxStatus.Success;
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus DeclinedSubscription( SubscriptionMsg subMsg )
		{
			POBoxStatus status = POBoxStatus.Success;

			log.Debug( "DeclinedSubscription - called" );
			log.Debug( "  subscription ID: " + subMsg.SubscriptionID );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
			{
				log.Error( "  specified \"toIdentity\" is not the caller" );
				return POBoxStatus.UnknownIdentity;
			}

			// open the post office box of the To user
			Store store = Store.GetStore();
			POBox.POBox toPOBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.ToID );
			if ( toPOBox != null )
			{
				// Get the subscription from the caller's PO box
				ICSList list = toPOBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
				if ( list.Count != 0 )
				{
					// Get the subscription object
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription cSub = new Subscription( toPOBox, e.Current as ShallowNode );
					e.Dispose();

					// Remove the subscription from the "toIdentity" PO box
					log.Debug( "  removing subscription from toPOBox." );
					toPOBox.Commit( toPOBox.Delete( cSub ) );
				}
			}
			else
			{
				status = POBoxStatus.UnknownPOBox;
			}

			log.Debug( "DeclinedSubscription exit  status: " + status.ToString() );
			return status;
		}

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AckSubscription( SubscriptionMsg subMsg )
		{
			log.Debug( "DEBUG - Obsolete AckSubscription called" );
			return POBoxStatus.Success;
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		/// <param name="collectionID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string subscriptionID, string collectionID)
		{
			log.Debug( "DEBUG - Obsolete GetSubscriptionInfo called" );
			return null;
		}

		/// <summary>
		/// Invite a user to a shared collection
		/// </summary>
		/// <param name="subMsg"></param>
		/// <returns>True if successful. False if not.</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		Invite( SubscriptionMsg subMsg )
		{
			POBoxStatus status;
			Access.Rights rights;

			log.Debug( "Invite - called" );
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			try
			{
				// Verify the requested access rights.
				rights = ( Access.Rights )Enum.ToObject( typeof( Access.Rights ), subMsg.AccessRights ); 
			}
			catch
			{
				log.Debug( "  invalid access rights: {0}", subMsg.AccessRights );
				return POBoxStatus.InvalidAccessRights;
			}

			// Verify domain
			Store store = Store.GetStore();
			Domain domain = store.GetDomain( subMsg.DomainID );
			if ( domain != null )
			{
				// Verify and get additional information about the "To" user
				Member toMember = domain.GetMemberByID( subMsg.ToID );
				if ( toMember != null )
				{
					// Don't check for the fromMember in the domain if this is workgroup.
					if ( domain.ConfigType != Domain.ConfigurationType.Workgroup )
					{
						// In peer-to-peer the collection won't exist 
						Collection collection = store.GetCollectionByID( subMsg.SharedCollectionID ); 
						if ( collection != null )
						{
							// Verify that the from user exists in the collection which will also verify
							// them as being in the domain. Use the current principal because the
							// FromID can be spoofed.
							Member fromMember = collection.GetMemberByID( Thread.CurrentPrincipal.Identity.Name );
							if ( fromMember != null )
							{
								// Impersonate the caller so we obtain their access rights.
								collection.Impersonate( fromMember );
								try
								{
									// Add the new member to the collection.
									collection.Commit( new Member( toMember.Name, toMember.UserID, rights ) );
									status = POBoxStatus.Success;
								}
								catch ( Simias.Storage.AccessException )
								{
									log.Debug( "  caller {0} has invalid access rights.", fromMember.UserID );
									status = POBoxStatus.InvalidAccessRights;
								}
								catch ( Exception ex )
								{
									log.Debug( "  commit exception - {0}", ex.Message );
									status = POBoxStatus.UnknownError;
								}
								finally
								{
									collection.Revert();
								}

								// Remove the status subscription.
								if ( status == POBoxStatus.Success )
								{
									RemoveStatusSubscription( store, subMsg );
								}
							}
							else
							{
								log.Debug( "  sender {0} does not exist in the domain!", Thread.CurrentPrincipal.Identity.Name );
								status = POBoxStatus.UnknownIdentity;
							}
						}
						else
						{
							log.Debug( "  shared collection {0} does not exist on enterprise", subMsg.SharedCollectionID );
							status = POBoxStatus.UnknownCollection;
						}
					}
					else
					{
						// Look up the invitee's POBox.
						POBox.POBox poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.ToID );
						if ( poBox != null )
						{
							// Create the subscription to put into the invitee's POBox.
							Subscription subscription = new Subscription( 
								subMsg.SharedCollectionName + " subscription", 
								"Subscription", 
								subMsg.FromID );

							subscription.SubscriptionState = Simias.POBox.SubscriptionStates.Ready;
							subscription.ToName = toMember.Name;
							subscription.ToIdentity = subMsg.ToID;
							subscription.FromName = subMsg.FromName;
							subscription.FromIdentity = subMsg.FromID;
							subscription.SubscriptionRights = rights;
							subscription.MessageID = subMsg.SubscriptionID;
							subscription.SubscriptionCollectionID = subMsg.SharedCollectionID;
							subscription.SubscriptionCollectionType = subMsg.SharedCollectionType;
							subscription.SubscriptionCollectionName = subMsg.SharedCollectionName;
							subscription.DomainID = domain.ID;
							subscription.DomainName = domain.Name;
							subscription.SubscriptionKey = Guid.NewGuid().ToString();
							subscription.MessageType = "Outbound";
							subscription.DirNodeID = subMsg.DirNodeID;
							subscription.DirNodeName = subMsg.DirNodeName;

							try
							{
								poBox.Commit( subscription );
								status = POBoxStatus.Success;
							}
							catch ( Exception ex )
							{
								log.Debug( "  commit exception - {0}", ex.Message );
								status = POBoxStatus.UnknownError;
							}
						}
						else
						{
							log.Debug( "  cannot find toUser {0} POBox", subMsg.ToID );
							status = POBoxStatus.UnknownPOBox;
						}
					}
				}
				else
				{
					log.Debug( "  specified toUser {0} does not exist in the domain!", subMsg.ToID );
					status = POBoxStatus.UnknownIdentity;
				}
			}
			else
			{
				log.Debug( "  invalid Domain ID - {0}", subMsg.DomainID );
				status = POBoxStatus.UnknownDomain;
			}

			log.Debug( "Invite - exit" );
			return status;
		}

		/// <summary>
		/// Removes the status subscription from the caller's POBox.
		/// </summary>
		/// <param name="store">Store handle.</param>
		/// <param name="subMsg">Subscription information.</param>
		private void RemoveStatusSubscription( Store store, SubscriptionMsg subMsg )
		{
			// Look up the invitee's POBox.
			POBox.POBox poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.FromID );
			if ( poBox != null )
			{
				// Look for the status subscription that is associated with the inviter's subscription.		
				ICSList list = poBox.Search( Message.MessageIDProperty, subMsg.SubscriptionID, SearchOp.Equal);
				if ( list.Count > 0 )
				{
					// Get the subscription object
					ICSEnumerator e = list.GetEnumerator() as ICSEnumerator; e.MoveNext();
					Subscription subscription = new Subscription( poBox, e.Current as ShallowNode );
					e.Dispose();

					// Don't allow additional invitation events to occur.
					subscription.CascadeEvents = false;

					// Remove the subscription from the "fromIdentity" PO box
					poBox.Commit( poBox.Delete( subscription ) );
					log.Debug( "  removed subscription from fromPOBox." );
				}
			}
		}
	}

	/// <summary>
	/// </summary>
	[Serializable]
	public class SubscriptionInformation
	{
	}
}
