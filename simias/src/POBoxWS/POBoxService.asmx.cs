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
//using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.IO;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Simias.Web;
using System.Xml;
using System.Xml.Serialization;

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
		/// The specified identity was not found in the roster.
		/// </summary>
		UnknownIdentity,

		/// <summary>
		/// The specified subscription was not found.
		/// </summary>
		UnknownSubscription,

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

		private string versionEndpoint = "/simias10";
				
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
		[WebMethod]
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
		[WebMethod]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AcceptedSubscription(
			string				domainID, 
			string				fromIdentity, 
			string				toIdentity, 
			string				subscriptionID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();

			log.Info("POBoxService::AcceptedSubscription - called");
			log.Info("  subscription: " + subscriptionID);
			
			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, fromIdentity);

			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::AcceptedSubscription - PO Box not found");
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
				log.Debug("POBoxService::AcceptedSubscription - Subscription does not exist");
				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("POBoxService::AcceptedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("POBoxService::AcceptedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			// FIXME: need to match the caller's ID against the toIdentity

			cSub.Accept(store, cSub.SubscriptionRights);
			poBox.Commit(cSub);
			log.Info("POBoxService::AcceptedSubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Decline subscription
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="subscriptionID"></param>
		[WebMethod]
		[SoapDocumentMethod]
		public
		POBoxStatus
		DeclinedSubscription(
			string			domainID, 
			string			fromIdentity, 
			string			toIdentity, 
			string			subscriptionID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
			
			log.Info("POBoxService::DeclinedSubscription - called");
			log.Info("  subscription: " + subscriptionID);

			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, fromIdentity);

			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::DeclinedSubscription - PO Box not found");
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
				log.Debug("POBoxService::DeclinedSubscription - Subscription does not exist");
				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("POBoxService::DeclinedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("POBoxService::DeclinedSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			cSub.Decline();
			poBox.Commit(cSub);
			log.Info("POBoxService::DeclinedSubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		[WebMethod]
		[SoapDocumentMethod]
		public
		POBoxStatus
		AckSubscription(
			string			domainID, 
			string			fromIdentity, 
			string			toIdentity, 
			string			messageID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
			
			log.Info("POBoxService::Acksubscription - called");
			log.Info("  subscription: " + messageID);

			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, fromIdentity);

			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::AckSubscription - PO Box not found");
				return(POBoxStatus.UnknownPOBox);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(
					Message.MessageIDProperty, 
					messageID, 
					SearchOp.Equal).GetEnumerator();
			ShallowNode sn = null;
			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("POBoxService::AckSubscription - Subscription does not exist.");
				return(POBoxStatus.UnknownSubscription);
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);

			// Identities need to match up
			if (fromIdentity != cSub.FromIdentity)
			{
				log.Debug("POBoxService::AckSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			if (toIdentity != cSub.ToIdentity)
			{
				log.Debug("POBoxService::AckSubscription - Identity does not match");
				return(POBoxStatus.UnknownIdentity);
			}

			// FIXME: need to match the caller's ID against the toIdentity

			cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Acknowledged;
			poBox.Commit(cSub);
			poBox.Commit(poBox.Delete(cSub));

			log.Info("POBoxService::Acksubscription - exit");
			return(POBoxStatus.Success);
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		/// <returns>success:subinfo  failure:null</returns>
		[WebMethod]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string messageID)
		{
			Simias.POBox.POBox	poBox;
			Store store = Store.GetStore();

			log.Info("POBoxService::GetSubscriptionInfo - called");
			log.Info("  for subscription: " + messageID);

			// open the post office box
			poBox =
				(domainID == Simias.Storage.Domain.WorkGroupDomainID)
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, identityID);
			
			// check the post office box
			if (poBox == null)
			{
				log.Debug("POBoxService::GetSubscriptionInfo - PO Box not found");
				return(null);
			}

			// check that the message has already not been posted
			IEnumerator e = 
				poBox.Search(Message.MessageIDProperty, messageID, SearchOp.Equal).GetEnumerator();
			
			ShallowNode sn = null;

			if (e.MoveNext())
			{
				sn = (ShallowNode) e.Current;
			}

			if (sn == null)
			{
				log.Debug("POBoxService::GetSubscriptionInfo - Subscription does not exist");
				return(null);
			}

			// generate the subscription info object and return it
			Subscription cSub = new Subscription(poBox, sn);

			// Validate the shared collection
			Collection cSharedCollection = store.GetCollectionByID(cSub.SubscriptionCollectionID);
			if (cSharedCollection == null)
			{
				log.Debug("POBoxService::GetSubscriptionInfo - Collection not found");
				return(null);
			}

			// FIXME: got to be a better way
			string colUrl =
				(this.Context.Request.IsSecureConnection == true)
				? "https://" : "http://";

			colUrl +=
				this.Context.Request.Url.Host +
				":" +
				this.Context.Request.Url.Port.ToString();

			SubscriptionInformation subInfo = new SubscriptionInformation(colUrl);
			subInfo.GenerateFromSubscription(cSub);

			log.Info("POBoxService::GetSubscriptionInfo - exit");
			return subInfo;
		}

		/// <summary>
		/// Invite a user to a shared collection
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="fromUserID"></param>
		/// <param name="toUserID"></param>
		/// <param name="sharedCollectionID"></param>
		/// <param name="sharedCollectionType"></param>
		/// <returns>success subscription ID - failure empty string</returns>
		[WebMethod]
		[SoapDocumentMethod]
		public
		string 
		Invite(
			string			domainID, 
			string			fromUserID,
			string			toUserID,
			string			sharedCollectionID,
			string			sharedCollectionType,
			int				rights)
		{
			Collection			sharedCollection;
			Simias.POBox.POBox	poBox = null;
			Store				store = Store.GetStore();
			Subscription		cSub = null;

			log.Debug("POBoxService::Invite");

			if (domainID == null || domainID == "")
			{
				domainID = store.DefaultDomain;
			}

			// Verify domain
			Simias.Storage.Domain cDomain = store.GetDomain(domainID);
			if (cDomain == null)
			{
				throw new ApplicationException("Invalid Domain ID");
			}

			// Verify and get additional information about the "To" user
			Simias.Storage.Roster currentRoster = cDomain.GetRoster(store);
			if (currentRoster == null)
			{
				throw new ApplicationException("No member Roster exists for the specified Domain");
			}

			Member toMember = currentRoster.GetMemberByID(toUserID);
			if (toMember == null)
			{
				throw new ApplicationException("Specified \"toUserID\" does not exist in the Domain Roster");
			}

			Member fromMember = currentRoster.GetMemberByID(fromUserID);
			if (fromMember == null)
			{
				throw new ApplicationException("Specified \"fromUserID\" does not exist in the Domain Roster");
			}

			// FIXME:  Verify the fromMember is the caller

			sharedCollection = store.GetCollectionByID(sharedCollectionID); 
			if (sharedCollection == null)
			{
				throw new ApplicationException("Invalid shared collection ID");
			}

			if (rights > (int) Simias.Storage.Access.Rights.Admin)
			{
				throw new ApplicationException("Invalid access rights");
			}

			try
			{
				log.Debug("  looking up POBox for: " + toUserID);
			
				poBox = 
					(domainID == Simias.Storage.Domain.WorkGroupDomainID)
						? POBox.POBox.GetPOBox(store, domainID)
						: POBox.POBox.GetPOBox(store, domainID, toUserID);

				log.Debug("  newup subscription");
				cSub = new Subscription(sharedCollection.Name + " subscription", "Subscription", fromUserID);
				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = toUserID;
				cSub.FromName = fromMember.Name;
				cSub.FromIdentity = fromUserID;
				cSub.SubscriptionRights = (Simias.Storage.Access.Rights) rights;

				// FIXME: got to be a better way
				string serviceUrl =
					(this.Context.Request.IsSecureConnection == true)
						? "https://" : "http://";
				
				serviceUrl += 
						this.Context.Request.Url.Host +
						":" +
						this.Context.Request.Url.Port.ToString() +
						versionEndpoint +
						"/POBoxService.asmx";

				log.Debug("  newup service url: " + serviceUrl);
				cSub.POServiceURL = new Uri(serviceUrl);
				cSub.SubscriptionCollectionID = sharedCollection.ID;
				cSub.SubscriptionCollectionType = sharedCollectionType;
				cSub.SubscriptionCollectionName = sharedCollection.Name;
				cSub.DomainID = domainID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

				/*
				SyncCollection sc = new SyncCollection(sharedCollection);
				cSub.SubscriptionCollectionURL = sc.MasterUrl.ToString();
				*/
				
				// FIXME: got to be a better way
				cSub.SubscriptionCollectionURL =
					(this.Context.Request.IsSecureConnection == true)
						? "https://" : "http://";

				cSub.SubscriptionCollectionURL +=
					this.Context.Request.Url.Host +
					":" +
					this.Context.Request.Url.Port.ToString() +
					this.versionEndpoint;

				log.Debug("SubscriptionCollectionURL: " + cSub.SubscriptionCollectionURL);
				log.Debug("  getting the dir node"); 
				DirNode dirNode = sharedCollection.GetRootDirectory();
				if(dirNode != null)
				{
					cSub.DirNodeID = dirNode.ID;
					cSub.DirNodeName = dirNode.Name;
				}

				poBox.Commit(cSub);
				return(cSub.MessageID);
			}
			catch(Exception e)
			{
				log.Debug("  failed creating subscription");
				log.Debug(e.Message);
				log.Debug(e.StackTrace);
			}
			return("");
		}

		/// <summary>
		/// Return the Default Domain
		/// </summary>
		/// <param name="dummy">Dummy parameter so stub generators won't produce empty structures</param>
		/// <returns>default domain</returns>
		[WebMethod]
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
