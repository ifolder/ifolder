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
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

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
			POBoxStatus			status = POBoxStatus.UnknownError;
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();

			log.Debug( "AcceptedSubscription - called" );
			log.Debug( "  subscription ID: " + subMsg.SubscriptionID );
			log.Debug( "  collection ID: " + subMsg.SharedCollectionID );

			// Verify the caller
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );

			try
			{
				/* FIXME:: uncomment when everybody is running with authentication
				if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
				{
					log.Error( "Specified \"toIdentity\" is not the caller" );
					return( POBoxStatus.UnknownIdentity );
				}
				*/

				// open the post office box
				poBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
				if ( poBox != null )
				{
					// check that the message has already not been posted
					IEnumerator e = 
						poBox.Search(
						Message.MessageIDProperty, 
						subMsg.SubscriptionID,
						SearchOp.Equal).GetEnumerator();
					ShallowNode sn = null;
					if (e.MoveNext())
					{
						sn = (ShallowNode) e.Current;
					}

					if ( sn == null )
					{
						log.Debug( "AcceptedSubscription - Subscription does not exist" );

						// See if the toIdentity already exists in the memberlist
						// of the shared collection

						Collection sharedCollection = store.GetCollectionByID( subMsg.SharedCollectionID );
						if ( sharedCollection != null )
						{
							Simias.Storage.Member toMember = 
								sharedCollection.GetMemberByID( subMsg.ToID );
							if ( toMember != null )
							{
								log.Debug( "  already a member!" );
								status = POBoxStatus.Success;
							}
							else
							{
								status = POBoxStatus.UnknownSubscription;
							}
						}
						else
						{
							status = POBoxStatus.UnknownCollection;
						}
					}
					else
					{
						//
						// Subscription exists in the inviters PO box
						// 
						Subscription cSub = new Subscription( poBox, sn );

						// Identities need to match up
						if ( subMsg.FromID == cSub.FromIdentity )
						{
							if ( subMsg.ToID == cSub.ToIdentity )
							{
								cSub.Accept( store, cSub.SubscriptionRights );
								poBox.Commit( cSub );
								status = POBoxStatus.Success;
							}
							else
							{
								log.Debug( "  to identity does not match" );
								status = POBoxStatus.UnknownIdentity;
							}
						}
						else
						{
							log.Debug( "  from identity does not match" );
							status = POBoxStatus.UnknownIdentity;
						}
					}
				}
				else
				{
					status = POBoxStatus.UnknownPOBox;
				}
			}
			catch(Exception e)
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
			}
			
			log.Debug( "AcceptedSubscription exit  status: " + status.ToString() );
			return status;
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="subMsg"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public POBoxStatus DeclinedSubscription( SubscriptionMsg subMsg )
		{
			POBoxStatus	status = POBoxStatus.UnknownError;
			Simias.POBox.POBox	toPOBox;
			Store				store = Store.GetStore();
		
			log.Debug( "DeclinedSubscription - called" );
			log.Debug( "  subscription ID: " + subMsg.SubscriptionID );

			// Verify the caller
			log.Debug( "  current Principal: " + Thread.CurrentPrincipal.Identity.Name );
			/* FIXME:: uncomment when everybody is running with authentication
			if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
			{
				log.Error( "Specified \"toIdentity\" is not the caller" );
				return POBoxStatus.UnknownIdentity;
			}
			*/

			// open the post office box of the From user
			toPOBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.ToID );
			if ( toPOBox == null )
			{
				log.Debug( "DeclinedSubscription - PO Box not found" );
				return POBoxStatus.UnknownPOBox;
			}

			// Get the subscription from the caller's PO box
			IEnumerator e = 
				toPOBox.Search(
				Message.MessageIDProperty, 
				subMsg.SubscriptionID,
				SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if ( e.MoveNext() )
			{
				sn = (ShallowNode) e.Current;
			}

			if ( sn == null )
			{
				log.Debug(
					"DeclinedSubscription - Subscription: " +
					subMsg.SubscriptionID +
					" does not exist");

				//
				// See if the toIdentity already exists in the memberlist
				// of the shared collection.  If he has already accepted
				// he can't decline from another machine
				//

				Collection sharedCollection = store.GetCollectionByID( subMsg.SharedCollectionID );
				if ( sharedCollection == null )
				{
					log.Debug( "DeclinedSubscription - shared collection does not exist" );
					return POBoxStatus.UnknownCollection;
				}

				Simias.Storage.Member cMember = sharedCollection.GetMemberByID( subMsg.ToID );
				if ( cMember != null )
				{
					log.Debug("DeclinedSubscription - already a member returning success");
					return POBoxStatus.Success;
				}

				return POBoxStatus.UnknownSubscription;
			}

			// get the subscription object
			Subscription cSub = new Subscription( toPOBox, sn );

			// Identities need to match up
			if ( subMsg.FromID != cSub.FromIdentity )
			{
				log.Debug( "DeclinedSubscription - Identity does not match" );
				return POBoxStatus.UnknownIdentity;
			}

			if ( subMsg.ToID != cSub.ToIdentity )
			{
				log.Debug( "DeclinedSubscription - Identity does not match" );
				return POBoxStatus.UnknownIdentity;
			}

			// Validate the shared collection
			Collection cCol = store.GetCollectionByID( cSub.SubscriptionCollectionID );
			if ( cCol == null )
			{
				// FIXEME:: Do we want to still try and cleanup the subscriptions?
				log.Debug( "DeclinedSubscription - Collection not found" );
				return POBoxStatus.UnknownCollection;
			}

			//
			// Actions taken when a subscription is declined
			//
			// If I'm the owner of the shared collection then the
			// decline is treated as a delete of the shared collection
			// so we must.
			// 1) Delete all the subscriptions in all members PO boxes
			// 2) Delete the shared collection itself
			//
			// If I'm already a member of the shared collection but not
			// the owner.
			// 1) Remove myself from the member list of the shared collection
			// 2) Delete my subscription to the shared collection
			//
			// If I'm not yet a member of the shared collection but declined
			// an invitation from another user in the system.  In this case
			// the From and To identies will be different.
			// 1) Delete my subscription to the shared collection.
			// 2) Set the state of the subscription in the inviter's PO
			//    Box to "declined".
			//

			Simias.Storage.Member toMember = cCol.GetMemberByID( subMsg.ToID );
			if ( toMember == null )
			{
				log.Debug( "  handling case where identity is declining a subscription" );
				// I am not a member of this shared collection and want to
				// decline the subscription.

				// open the post office box of the from and decline the subscription
				Simias.POBox.POBox fromPOBox = 
					Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
				if ( fromPOBox != null )
				{
					Subscription cFromMemberSub = 
						fromPOBox.GetSubscriptionByCollectionID( cCol.ID );
					if( cFromMemberSub != null )
					{
						cFromMemberSub.Decline();
						fromPOBox.Commit( cFromMemberSub );
					}
				}

				// Remove the subscription from the "toIdentity" PO box
				toPOBox.Delete( cSub );
				toPOBox.Commit( toPOBox.Delete( cSub ) );
			}
			else
				if ( toMember.IsOwner == true )
			{
				// Am I the owner of the shared collection?
				log.Debug( "  handling case where identity is owner of collection" );

				ICSList memberlist = cCol.GetMemberList();
				foreach( ShallowNode sNode in memberlist )
				{
					Simias.Storage.Member cMember =	
						new Simias.Storage.Member( cCol, sNode );

					// Get the member's POBox
					Simias.POBox.POBox memberPOBox = 
						Simias.POBox.POBox.FindPOBox(
						store, 
						cCol.Domain, 
						cMember.UserID );
					if ( memberPOBox != null )
					{
						// Search for the matching subscription
						Subscription memberSub = 
							memberPOBox.GetSubscriptionByCollectionID( cCol.ID );
						if( memberSub != null )
						{
							memberPOBox.Delete( memberSub );
							memberPOBox.Commit( memberSub );
						}
					}
				}

				//
				// If the collection has unmanaged files we need
				// to delete those as well
				// FIXME:: this may need to be queued off asynchronously
				// since it could take a long time to delete a directory
				// with thousands of files
				//

				try
				{
					DirNode dirNode = cCol.GetRootDirectory();
					if( dirNode != null )
					{
						Directory.Delete( dirNode.GetFullPath( cCol ), true );
					}
				}
				catch{}

				// Delete the shared collection itself
				cCol.Commit( cCol.Delete() );
			}
			else
			{
				// Am I a member of the shared collection?
				log.Debug( "  handling case where identity is a member of the collection" );
				cCol.Commit( cCol.Delete( toMember ) );

				// Remove the subscription from the "toIdentity" PO box
				Subscription cMemberSub = 
					toPOBox.GetSubscriptionByCollectionID( cCol.ID );
				if( cMemberSub != null )
				{
					toPOBox.Commit( toPOBox.Delete( cMemberSub ) );
				}

				if ( subMsg.FromID != subMsg.ToID )
				{
					// open the post office box of the From user
					Simias.POBox.POBox fromPOBox = 
						Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID ); 
					if ( fromPOBox != null )
					{
						// Remove the subscription from the "fromIdentity" PO box
						Subscription cFromMemberSub = 
							fromPOBox.GetSubscriptionByCollectionID( cCol.ID );
						if( cFromMemberSub != null )
						{
							fromPOBox.Commit( fromPOBox.Delete( cFromMemberSub ) );
						}
					}
				}
			}

			log.Debug("DeclinedSubscription - exit");
			return POBoxStatus.Success;;
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
			POBoxStatus	status = POBoxStatus.UnknownError;
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
		
			log.Debug( "Acksubscription - called" );
			log.Debug( "  subscription: " + subMsg.SubscriptionID );

			// Verify the caller
			log.Debug("  current Principal: " + Thread.CurrentPrincipal.Identity.Name);
			/*
			if ( subMsg.ToID != Thread.CurrentPrincipal.Identity.Name )
			{
				log.Error( "specified \"toIdentity\" is not the caller" );
				return POBoxStatus.UnknownIdentity;
			}
			*/

			// open the post office box
			poBox = Simias.POBox.POBox.FindPOBox( store, subMsg.DomainID, subMsg.FromID );
			if (poBox == null)
			{
				log.Debug("AckSubscription - PO Box not found");
				return POBoxStatus.UnknownPOBox;
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(
				Message.MessageIDProperty, 
				subMsg.SubscriptionID,
				SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if ( sn == null )
			{
				log.Debug("AckSubscription - Subscription does not exist.");

				//
				// See if the toIdentity already exists in the memberlist
				// of the shared collection
				//

				Collection sharedCollection = 
					store.GetCollectionByID( subMsg.SharedCollectionID );
				if ( sharedCollection == null )
				{
					log.Debug("AckSubscription - shared collection does not exist");
					return POBoxStatus.UnknownCollection;
				}

				Simias.Storage.Member toMember = sharedCollection.GetMemberByID( subMsg.ToID );
				if ( toMember != null )
				{
					log.Debug( "AckSubscription - already a member returning success" );
					return POBoxStatus.Success;
				}

				return POBoxStatus.UnknownSubscription;
			}

			// get the subscription object
			Subscription cSub = new Subscription( poBox, sn );

			// Identities need to match up
			if ( subMsg.FromID != cSub.FromIdentity )
			{
				log.Debug( "AckSubscription - Identity does not match" );
				return POBoxStatus.UnknownIdentity;
			}

			if ( subMsg.ToID != cSub.ToIdentity )
			{
				log.Debug( "AckSubscription - Identity does not match" );
				return POBoxStatus.UnknownIdentity;
			}

			//
			// Delete the subscription from the inviters PO box
			//

			cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Acknowledged;
			poBox.Commit( cSub );
			poBox.Commit( poBox.Delete(cSub) );

			log.Debug( "Acksubscription - exit" );
			return POBoxStatus.Success;;
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string subscriptionID)
		{
			SubscriptionInformation subInfo = null;

			Simias.POBox.POBox	poBox;
			Store store = Store.GetStore();

			log.Debug("GetSubscriptionInfo - called");
			log.Debug("  for subscription: " + subscriptionID);

			// open the post office box
			poBox =	Simias.POBox.POBox.FindPOBox(store, domainID, identityID);
			if (poBox == null)
			{
				log.Debug("GetSubscriptionInfo - PO Box not found");
				return(null);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(Message.MessageIDProperty, subscriptionID, SearchOp.Equal).GetEnumerator();
		
			ShallowNode sn = null;

			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("GetSubscriptionInfo - Subscription does not exist");
				return(null);
			}

			// generate the subscription info object and return it
			Subscription cSub = new Subscription(poBox, sn);

			// Validate the shared collection
			Collection cSharedCollection = store.GetCollectionByID(cSub.SubscriptionCollectionID);
			if (cSharedCollection == null)
			{
				log.Debug("GetSubscriptionInfo - Collection not found");
				return(null);
			}

			UriBuilder colUri = 
				new UriBuilder(
				this.Context.Request.Url.Scheme,
				this.Context.Request.Url.Host,
				this.Context.Request.Url.Port,
				this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ));

			log.Debug("  URI: " + colUri.ToString());
			subInfo = new SubscriptionInformation(colUri.ToString());
			subInfo.GenerateFromSubscription(cSub);

			log.Debug("GetSubscriptionInfo - exit");
			return subInfo;
		}

		/// <summary>
		/// Verify that a collection exists
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="collectionID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		VerifyCollection(string domainID, string collectionID)
		{
			POBoxStatus	wsStatus = POBoxStatus.UnknownCollection;
			Store store = Store.GetStore();

			log.Debug("VerifyCollection - called");
			log.Debug("  for collection: " + collectionID);


			// Validate the shared collection
			Collection cSharedCollection = store.GetCollectionByID(collectionID);
			if (cSharedCollection != null)
			{
				// Make sure the collection is not in a proxy state
				if (cSharedCollection.IsProxy == false)
				{
					wsStatus = POBoxStatus.Success;
				}
				else
				{
					log.Debug("  collection is in the proxy state");
				}
			}
			else
			{
				log.Debug("  collection not found");
			}

			log.Debug("VerifyCollection - exit");
			return wsStatus;
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
			/*
			if ( subMsg.FromID != Thread.CurrentPrincipal.Identity.Name )
			{
				return POBoxStatus.UnknownIdentity;
			}
			*/

			/*  FIXME::don't think we need this check anymore
			if (domainID == null || domainID == "")
			{
				domainID = store.DefaultDomain;
			}
			*/


			// Verify and get additional information about the "To" user
			Member toMember = cDomain.GetMemberByID( subMsg.ToID );
			if ( toMember == null )
			{
				log.Debug( "  specified \"toUserID\" does not exist in the domain!" );
				return POBoxStatus.UnknownIdentity;
			}

			// Don't check for the fromMember in the domain if this is workgroup.
			if ( cDomain.ConfigType != Domain.ConfigurationType.Workgroup )
			{
				Member fromMember = cDomain.GetMemberByID( subMsg.FromID );
				if ( fromMember == null )
				{
					log.Debug( "  specified \"fromUserID\" does not exist in the domain!" );
					return POBoxStatus.UnknownIdentity;
				}
			}

			// In peer-to-peer the collection won't exist 
			sharedCollection = store.GetCollectionByID( subMsg.SharedCollectionID ); 
			if ( sharedCollection == null )
			{
				log.Debug( "  shared collection does not exist" );
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

				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = subMsg.ToID;
				cSub.FromName = subMsg.FromName;
				cSub.FromIdentity = subMsg.FromID;
				cSub.SubscriptionRights = (Simias.Storage.Access.Rights) subMsg.AccessRights;
				cSub.MessageID = subMsg.SubscriptionID;

				string appPath = this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} );
				appPath += "/POBoxService.asmx";

				log.Debug("  application path: " + appPath);

				/*
				// TODO: Need to fix how we detect SSL. Waiting for a fix in mod_mono.
				try
				{
					UriBuilder poUri = 
						new UriBuilder(
						(this.Context.Request.Url.Port == 443) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp ,
						this.Context.Request.Url.Host,
						this.Context.Request.Url.Port,
						appPath);

					cSub.POServiceURL = poUri.Uri;
				}
				catch( Exception e1 )
				{
					log.Debug( "Failed creating POBox Uri" );
					log.Debug( e1.Message );
					log.Debug( e1.StackTrace );

					// For now we won't error out cause I don't think we need
					// URLs in the subscription itself
				}
				*/

				cSub.SubscriptionCollectionID = subMsg.SharedCollectionID;
				cSub.SubscriptionCollectionType = subMsg.SharedCollectionType;
				cSub.SubscriptionCollectionName = subMsg.SharedCollectionName;
				cSub.DomainID = cDomain.ID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

				/*
				try
				{
					// TODO: Need to fix how we detect SSL. Waiting for a fix in mod_mono.
					UriBuilder coUri = 
						new UriBuilder(
						(this.Context.Request.Url.Port == 443) ? Uri.UriSchemeHttps : Uri.UriSchemeHttp ,
						this.Context.Request.Url.Host,
						this.Context.Request.Url.Port,
						this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} ));

					cSub.SubscriptionCollectionURL = coUri.Uri.ToString();
					log.Debug( "  SubscriptionCollectionURL: " + cSub.SubscriptionCollectionURL );
				}
				catch( Exception e2 )
				{
					log.Debug( "Failed creating Collection Uri" );
					log.Debug( e2.Message );
					log.Debug( e2.StackTrace );
				}
				*/

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

		/// <summary>
		/// Return the Default Domain
		/// </summary>
		/// <param name="dummy">Dummy parameter so stub generators won't produce empty structures</param>
		/// <returns>default domain</returns>
		[WebMethod(EnableSession = true)]
		public string GetDefaultDomain(int dummy)
		{
			return(Store.GetStore().DefaultDomain);
		}
	}

	/// <summary>
	/// </summary>
	[Serializable]
	public class SubscriptionInformation
	{
		/// <summary>
		/// </summary>
		public string   Name;
		/// <summary>
		/// </summary>
		public string	MsgID;
		/// <summary>
		/// </summary>
		public string	FromID;
		/// <summary>
		/// </summary>
		public string	FromName;
		/// <summary>
		/// </summary>
		public string	ToID;
		/// <summary>
		/// </summary>
		public string	ToNodeID;
		/// <summary>
		/// </summary>
		public string	ToName;
		/// <summary>
		/// </summary>
		public int		AccessRights;

		/// <summary>
		/// </summary>
		public string	CollectionID;
		/// <summary>
		/// </summary>
		public string	CollectionName;
		/// <summary>
		/// </summary>
		public string	CollectionType;
		/// <summary>
		/// </summary>
		public string	CollectionUrl;

		/// <summary>
		/// </summary>
		public string	DirNodeID;
		/// <summary>
		/// </summary>
		public string	DirNodeName;

		/// <summary>
		/// </summary>
		public string	DomainID;
		/// <summary>
		/// </summary>
		public string	DomainName;

		/// <summary>
		/// </summary>
		public int		State;
		/// <summary>
		/// </summary>
		public int		Disposition;

		/// <summary>
		/// </summary>
		public SubscriptionInformation()
		{

		}

		/// <summary>
		/// </summary>
		public SubscriptionInformation(string collectionUrl)
		{
			this.CollectionUrl = collectionUrl;
		}

		internal void GenerateFromSubscription(Subscription cSub)
		{
			this.Name = cSub.Name;
			this.MsgID = cSub.MessageID;
			this.FromID = cSub.FromIdentity;
			this.FromName = cSub.FromName;
			this.ToID = cSub.ToIdentity;
			this.ToNodeID = cSub.ToMemberNodeID;
			this.ToName = cSub.ToName;
			this.AccessRights = (int) cSub.SubscriptionRights;

			this.CollectionID = cSub.SubscriptionCollectionID;
			this.CollectionName = cSub.SubscriptionCollectionName;
			this.CollectionType = cSub.SubscriptionCollectionType;

			//this.CollectionUrl = cSub.SubscriptionCollectionURL;

			this.DirNodeID = cSub.DirNodeID;
			this.DirNodeName = cSub.DirNodeName;

			this.DomainID = cSub.DomainID;
			this.DomainName = cSub.DomainName;

			this.State = (int) cSub.SubscriptionState;
			this.Disposition = (int) cSub.SubscriptionDisposition;
		}
	}
}
