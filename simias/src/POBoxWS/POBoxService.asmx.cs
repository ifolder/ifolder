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
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

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
		/// The specified subscription was not found.
		/// </summary>
		UnknownSubscription,

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
		/// The subscription was already accepted by another client.
		/// </summary>
		AlreadyAccepted,

		/// <summary>
		/// The subscription was already denied by another client.
		/// </summary>
		AlreadyDeclined,

		/// <summary>
		/// The invitation has not moved to the posted state yet.
		/// </summary>
		NotPosted,

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
			return POBoxStatus.InvalidState;
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
			return POBoxStatus.InvalidState;
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
			POBoxStatus			status = POBoxStatus.UnknownError;
			Store				store = Store.GetStore();
			Collection			sharedCollection = null;
			Simias.POBox.POBox	poBox = null;
			Subscription		cSub = null;

			// Verify domain
			Domain cDomain = store.GetDomain( subMsg.DomainID );
			if ( cDomain == null )
			{
				log.Debug( "  invalid Domain ID!" );
				return POBoxStatus.UnknownDomain;
			}

			log.Debug( "Invite - called" );
			log.Debug( "  DomainID: " + subMsg.DomainID );
			log.Debug( "  FromUserID: " + subMsg.FromID );
			log.Debug( "  ToUserID: " + subMsg.ToID );

			// Verify the fromMember is the caller
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			// Verify and get additional information about the "To" user
			Member toMember = cDomain.GetMemberByID( subMsg.ToID );
			if ( toMember == null )
			{
				log.Debug( "  specified \"toUserID\" does not exist in the domain!" );
				return POBoxStatus.UnknownIdentity;
			}

			// In peer-to-peer the collection won't exist 
			sharedCollection = store.GetCollectionByID( subMsg.SharedCollectionID ); 
			if ( sharedCollection == null )
			{
				log.Debug( "  shared collection does not exist" );
			}

			// Don't check for the fromMember in the domain if this is workgroup.
			if ( cDomain.ConfigType != Domain.ConfigurationType.Workgroup )
			{
				Member fromMember = cDomain.GetMemberByID( subMsg.FromID );
				if ( fromMember != null )
				{
					// Check that the sender has sufficient rights to invite.
					if ( sharedCollection != null )
					{
						Member collectionMember = sharedCollection.GetMemberByID( fromMember.UserID );
						if ( ( collectionMember == null ) || ( collectionMember.Rights != Access.Rights.Admin ) )
						{
							log.Debug( " sender does not have rights to invite to this collection." );
							return POBoxStatus.InvalidAccessRights;
						}
					}
					else
					{
						// The collection must exist in enterprise.
						log.Debug( " shared collection does not exist on enterprise" );
						return POBoxStatus.UnknownCollection;
					}
				}
				else
				{
					log.Debug( "  specified \"fromUserID\" does not exist in the domain!" );
					return POBoxStatus.UnknownIdentity;
				}
			}

			if ( subMsg.AccessRights > (int) Simias.Storage.Access.Rights.Admin)
			{
				return POBoxStatus.InvalidAccessRights;
			}

			try
			{
				log.Debug( "  looking up POBox for: " + subMsg.ToID );
				poBox = POBox.POBox.GetPOBox( store, subMsg.DomainID, subMsg.ToID );

				cSub = 
					new Subscription( 
					subMsg.SharedCollectionName + " subscription", 
					"Subscription", 
					subMsg.FromID );

				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Ready;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = subMsg.ToID;
				cSub.FromName = subMsg.FromName;
				cSub.FromIdentity = subMsg.FromID;
				cSub.SubscriptionRights = (Simias.Storage.Access.Rights) subMsg.AccessRights;
				cSub.MessageID = subMsg.SubscriptionID;

				string appPath = this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} );
				appPath += "/POBoxService.asmx";

				log.Debug("  application path: " + appPath);

				cSub.SubscriptionCollectionID = subMsg.SharedCollectionID;
				cSub.SubscriptionCollectionType = subMsg.SharedCollectionType;
				cSub.SubscriptionCollectionName = subMsg.SharedCollectionName;
				cSub.DomainID = cDomain.ID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

				if ( sharedCollection != null )
				{
					DirNode dirNode = sharedCollection.GetRootDirectory();
					if( dirNode != null )
					{
						cSub.DirNodeID = dirNode.ID;
						cSub.DirNodeName = dirNode.Name;
					}
				}
				else
				{
					cSub.DirNodeID = subMsg.DirNodeID;
					cSub.DirNodeName = subMsg.DirNodeName;
				}

				poBox.Commit( cSub );
				status = POBoxStatus.Success;
			}
			catch(Exception e)
			{
				log.Error("  failed creating subscription");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			log.Debug( "Invite - exit" );
			return status;
		}
	}

	/// <summary>
	/// </summary>
	[Serializable]
	public class SubscriptionInformation
	{
	}
}
