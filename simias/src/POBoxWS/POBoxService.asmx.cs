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
		/// The suscription was in an invalid state for the method
		/// </summary>
		InvalidState,

		/// <summary>
		/// An unknown error was realized.
		/// </summary>
		UnknownError
	};

	/// <summary>
	/// Summary description for Service1.
	/// </summary>
	/// 
	
	[WebService(Namespace="http://novell.com/simias/pobox/")]
	public class POBoxService : System.Web.Services.WebService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POBoxService));

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
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AcceptedSubscription(
			string				domainID, 
			string				fromIdentity, 
			string				toIdentity, 
			string				subscriptionID,
			string				sharedCollectionID)
		{
			POBoxStatus			status = POBoxStatus.UnknownError;
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();

			log.Debug("AcceptedSubscription - called");
			log.Debug("  subscription ID: " + subscriptionID);
			log.Debug("  collection ID: " + sharedCollectionID);

			// Verify the caller
			log.Debug("  current Principal: " + Thread.CurrentPrincipal.Identity.Name);

			try
			{
				/* FIXME:: uncomment when everybody is running with authentication
				if (toIdentity != Thread.CurrentPrincipal.Identity.Name)
				{
					log.Error("Specified \"toIdentity\" is not the caller");
					return(POBoxStatus.UnknownIdentity);
				}
				*/

				// open the post office box
				poBox = Simias.POBox.POBox.FindPOBox(store, domainID, fromIdentity);
				if (poBox != null)
				{
					// check that the message has already not been posted
					IEnumerator e = 
						poBox.Search(
						Message.MessageIDProperty, 
						subscriptionID, 
						SearchOp.Equal).GetEnumerator();
					ShallowNode sn = null;
					if (e.MoveNext())
					{
						sn = (ShallowNode) e.Current;
					}

					if (sn == null)
					{
						log.Debug("AcceptedSubscription - Subscription does not exist");

						// See if the toIdentity already exists in the memberlist
						// of the shared collection

						Collection sharedCollection = store.GetCollectionByID(sharedCollectionID);
						if (sharedCollection != null)
						{
							Simias.Storage.Member toMember = 
								sharedCollection.GetMemberByID(toIdentity);
							if (toMember != null)
							{
								log.Debug("  already a member!");
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
						Subscription cSub = new Subscription(poBox, sn);

						// Identities need to match up
						if (fromIdentity == cSub.FromIdentity)
						{
							if (toIdentity == cSub.ToIdentity)
							{
								cSub.Accept(store, cSub.SubscriptionRights);
								poBox.Commit(cSub);
								status = POBoxStatus.Success;
							}
							else
							{
								log.Debug("  to identity does not match");
								status = POBoxStatus.UnknownIdentity;
							}
						}
						else
						{
							log.Debug("  from identity does not match");
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
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}
			
			log.Debug("AcceptedSubscription exit  status: " + status.ToString());
			return(status);
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		DeclinedSubscription(
			string			domainID, 
			string			fromIdentity, 
			string			toIdentity, 
			string			subscriptionID,
			string			sharedCollectionID)
		{
			Simias.POBox.POBox	toPOBox;
			Store				store = Store.GetStore();
			
			log.Debug("DeclinedSubscription - called");
			log.Debug("  subscription: " + subscriptionID);

			// Verify the caller
			log.Debug("  current Principal: " + Thread.CurrentPrincipal.Identity.Name);
			/* FIXME:: uncomment when everybody is running with authentication
			if (toIdentity != Thread.CurrentPrincipal.Identity.Name)
			{
				log.Error("Specified \"toIdentity\" is not the caller");
				return(POBoxStatus.UnknownIdentity);
			}
			*/

			// open the post office box of the From user
			toPOBox = Simias.POBox.POBox.FindPOBox(store, domainID, toIdentity);
			if (toPOBox == null)
			{
				log.Debug("DeclinedSubscription - PO Box not found");
				return(POBoxStatus.UnknownPOBox);
			}

			// Get the subscription from the caller's PO box
			IEnumerator e = 
				toPOBox.Search(
				Message.MessageIDProperty, 
				subscriptionID, 
				SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug(
					"DeclinedSubscription - Subscription: " +
					subscriptionID +
					" does not exist");

				//
				// See if the toIdentity already exists in the memberlist
				// of the shared collection.  If he has already accepted
				// he can't decline from another machine
				//

				Collection sharedCollection = store.GetCollectionByID(sharedCollectionID);
				if (sharedCollection == null)
				{
					log.Debug("DeclinedSubscription - shared collection does not exist");
					return(POBoxStatus.UnknownCollection);
				}

				Simias.Storage.Member cMember = sharedCollection.GetMemberByID(toIdentity);
				if (cMember != null)
				{
					log.Debug("DeclinedSubscription - already a member returning success");
					return(POBoxStatus.Success);
				}

				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(toPOBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("DeclinedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("DeclinedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			// Validate the shared collection
			Collection cCol = store.GetCollectionByID(cSub.SubscriptionCollectionID);
			if (cCol == null)
			{
				// FIXEME:: Do we want to still try and cleanup the subscriptions?
				log.Debug("DeclinedSubscription - Collection not found");
				return(POBoxStatus.UnknownCollection);
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

			Simias.Storage.Member toMember = cCol.GetMemberByID(toIdentity);
			if (toMember == null)
			{
				log.Debug("  handling case where identity is declining a subscription");
				// I am not a member of this shared collection and want to
				// decline the subscription.

				// open the post office box of the from and decline the subscription
				Simias.POBox.POBox fromPOBox = Simias.POBox.POBox.FindPOBox(store, domainID, fromIdentity);
				if (fromPOBox != null)
				{
					Subscription cFromMemberSub = 
						fromPOBox.GetSubscriptionByCollectionID(cCol.ID);
					if(cFromMemberSub != null)
					{
						cFromMemberSub.Decline();
						fromPOBox.Commit(cFromMemberSub);
					}
				}

				// Remove the subscription from the "toIdentity" PO box
				toPOBox.Delete(cSub);
				toPOBox.Commit(cSub);
			}
			else
			if (toMember.IsOwner == true)
			{
				// Am I the owner of the shared collection?
				log.Debug("  handling case where identity is owner of collection");

				ICSList memberlist = cCol.GetMemberList();
				foreach(ShallowNode sNode in memberlist)
				{
					Simias.Storage.Member cMember =	
						new Simias.Storage.Member(cCol, sNode);

					// Get the member's POBox
					Simias.POBox.POBox memberPOBox = 
						Simias.POBox.POBox.FindPOBox(
						store, 
						cCol.Domain, 
						cMember.UserID );
					if (memberPOBox != null)
					{
						// Search for the matching subscription
						Subscription memberSub = 
							memberPOBox.GetSubscriptionByCollectionID(cCol.ID);
						if(memberSub != null)
						{
							memberPOBox.Delete(memberSub);
							memberPOBox.Commit(memberSub);
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
					if(dirNode != null)
					{
						Directory.Delete(dirNode.GetFullPath(cCol), true);
					}
				}
				catch{}

				// Delete the shared collection itself
				cCol.Commit(cCol.Delete());
			}
			else
			{
				// Am I a member of the shared collection?
				log.Debug("  handling case where identity is a member of the collection");

				cCol.Delete(toMember);
				cCol.Commit(toMember);

				// Remove the subscription from the "toIdentity" PO box
				Subscription cMemberSub = 
					toPOBox.GetSubscriptionByCollectionID(cCol.ID);
				if(cMemberSub != null)
				{
					toPOBox.Delete(cMemberSub);
					toPOBox.Commit(cMemberSub);
				}

				if (fromIdentity != toIdentity)
				{
					// open the post office box of the From user
					Simias.POBox.POBox fromPOBox = 
						Simias.POBox.POBox.FindPOBox(store, domainID, toIdentity);
					if (fromPOBox != null)
					{
						// Remove the subscription from the "fromIdentity" PO box
						Subscription cFromMemberSub = 
							fromPOBox.GetSubscriptionByCollectionID(cCol.ID);
						if(cFromMemberSub != null)
						{
							fromPOBox.Delete(cFromMemberSub);
							fromPOBox.Commit(cFromMemberSub);
						}
					}
				}
			}

			log.Debug("DeclinedSubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AckSubscription(
			string			domainID, 
			string			fromIdentity, 
			string			toIdentity, 
			string			subscriptionID,
			string			sharedCollectionID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
			
			log.Debug("Acksubscription - called");
			log.Debug("  subscription: " + subscriptionID);

			// Verify the caller
			log.Debug("  current Principal: " + Thread.CurrentPrincipal.Identity.Name);
			/*
			if (toIdentity != Thread.CurrentPrincipal.Identity.Name)
			{
				log.Error("specified \"toIdentity\" is not the caller");
				return(POBoxStatus.UnknownIdentity);
			}
			*/

			// open the post office box
			poBox = Simias.POBox.POBox.FindPOBox(store, domainID, fromIdentity);
			if (poBox == null)
			{
				log.Debug("AckSubscription - PO Box not found");
				return(POBoxStatus.UnknownPOBox);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(
					Message.MessageIDProperty, 
					subscriptionID, 
					SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("AckSubscription - Subscription does not exist.");

				//
				// See if the toIdentity already exists in the memberlist
				// of the shared collection
				//

				Collection sharedCollection = store.GetCollectionByID(sharedCollectionID);
				if (sharedCollection == null)
				{
					log.Debug("AckSubscription - shared collection does not exist");
					return(POBoxStatus.UnknownCollection);
				}

				Simias.Storage.Member toMember = sharedCollection.GetMemberByID(toIdentity);
				if (toMember != null)
				{
					log.Debug("AckSubscription - already a member returning success");
					return(POBoxStatus.Success);
				}

				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("AckSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("AckSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			//
			// Delete the subscription from the inviters PO box
			//

			cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Acknowledged;
			poBox.Commit(cSub);
			poBox.Commit(poBox.Delete(cSub));

			log.Debug("Acksubscription - exit");
			return(POBoxStatus.Success);
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
			SubscriptionInformation subInfo = new SubscriptionInformation(colUri.ToString());
			subInfo.GenerateFromSubscription(cSub);

			log.Debug("GetSubscriptionInfo - exit");
			return subInfo;
		}

		/// <summary>
		/// Verify that a collection exists
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		POBoxStatus
		VerifyCollection(string domainID, string collectionID)
		{
			Store store = Store.GetStore();

			log.Debug("VerifyCollection - called");
			log.Debug("  for collection: " + collectionID);

			POBoxStatus	wsStatus = POBoxStatus.UnknownCollection;

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
			return(wsStatus);
		}

		/// <summary>
		/// Invite a user to a shared collection
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="fromUserID"></param>
		/// <param name="toUserID"></param>
		/// <param name="sharedCollectionID"></param>
		/// <param name="sharedCollectionType"></param>
		/// <param name="rights"></param>
		/// <param name="messageID"></param>
		/// <returns>True if successful. False if not.</returns>
		[WebMethod(EnableSession = true)]
		[SoapDocumentMethod]
		public
		bool
		Invite(
			string			domainID, 
			string			fromUserID,
			string			toUserID,
			string			sharedCollectionID,
			string			sharedCollectionType,
			int				rights,
			string			messageID)
		{
			bool				status = false;
			Collection			sharedCollection = null;
			Simias.POBox.POBox	poBox = null;
			Store				store = Store.GetStore();
			Subscription		cSub = null;

			log.Debug( "Invite - called" );
			log.Debug( "  DomainID: " + domainID );
			log.Debug( "  FromUserID: " + fromUserID );
			log.Debug( "  ToUserID: " + toUserID );

			// Verify the fromMember is the caller
			log.Debug("  current Principal: " + Thread.CurrentPrincipal.Identity.Name);
			/*
			if (fromUserID != Thread.CurrentPrincipal.Identity.Name)
			{
				throw new ApplicationException("Specified \"fromUserID\" is not the caller");
			}
			*/

			if (domainID == null || domainID == "")
			{
				domainID = store.DefaultDomain;
			}

			// Verify domain
			Simias.Storage.Domain cDomain = store.GetDomain( domainID );
			if (cDomain == null)
			{
				log.Debug( "  invalid Domain ID!" );
				throw new ApplicationException("Invalid Domain ID");
			}

			// Verify and get additional information about the "To" user
			Member toMember = cDomain.GetMemberByID(toUserID);
			if (toMember == null)
			{
				log.Debug( "  specified \"toUserID\" does not exist in the domain!" );
				throw new ApplicationException("Specified \"toUserID\" does not exist in the Domain");
			}

			Member fromMember = cDomain.GetMemberByID(fromUserID);
			if (fromMember == null)
			{
				log.Debug( "  specified \"fromUserID\" does not exist in the domain!" );
				throw new ApplicationException("Specified \"fromUserID\" does not exist in the Domain");
			}

			// In peer-to-peer the collection won't exist 
			sharedCollection = store.GetCollectionByID( sharedCollectionID ); 
			if ( sharedCollection == null )
			{
				log.Debug( "  shared collection does not exist" );
			}

			if (rights > (int) Simias.Storage.Access.Rights.Admin)
			{
				throw new ApplicationException("Invalid access rights");
			}

			try
			{
				log.Debug("  looking up POBox for: " + toUserID);
				poBox = POBox.POBox.GetPOBox( store, domainID, toUserID );
				if ( sharedCollection == null )
				{
					cSub = new Subscription( "Subscription from " + fromMember.Name, "Subscription", fromUserID );
				}
				else
				{
					cSub = new Subscription(sharedCollection.Name + " subscription", "Subscription", fromUserID );
				}
				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = toUserID;
				cSub.FromName = fromMember.Name;
				cSub.FromIdentity = fromUserID;
				cSub.SubscriptionRights = (Simias.Storage.Access.Rights) rights;
				cSub.MessageID = messageID;

				string appPath = this.Context.Request.ApplicationPath.TrimStart( new char[] {'/'} );
				appPath += "/POBoxService.asmx";

				log.Debug("  application path: " + appPath);

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
				}

				cSub.SubscriptionCollectionID = sharedCollectionID;
				cSub.SubscriptionCollectionType = sharedCollectionType;

				cSub.SubscriptionCollectionName = 
					( sharedCollection != null ) ? sharedCollection.Name : "proxy";

				cSub.DomainID = domainID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

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
 
				if ( sharedCollection != null )
				{
					DirNode dirNode = sharedCollection.GetRootDirectory();
					if( dirNode != null )
					{
						cSub.DirNodeID = dirNode.ID;
						cSub.DirNodeName = dirNode.Name;
					}
				}

				poBox.Commit( cSub );
				status = true;
			}
			catch(Exception e)
			{
				log.Error("  failed creating subscription");
				log.Error(e.Message);
				log.Error(e.StackTrace);
			}

			log.Debug("Invite - exit");
			return(status);
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

	[Serializable]
	public class SubscriptionInformation
	{
		public string   Name;
		public string	MsgID;
		public string	FromID;
		public string	FromName;
		public string	ToID;
		public string	ToNodeID;
		public string	ToName;
		public int		AccessRights;

		public string	CollectionID;
		public string	CollectionName;
		public string	CollectionType;
		public string	CollectionUrl;

		public string	DirNodeID;
		public string	DirNodeName;

		public string	DomainID;
		public string	DomainName;

		public int		State;
		public int		Disposition;

		public SubscriptionInformation()
		{

		}

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
