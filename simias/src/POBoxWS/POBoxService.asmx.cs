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
using System.Data;
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
	/// Summary description for Service1.
	/// </summary>
	/// 
	
	[WebService(Namespace="http://novell.com/simias/pobox/")]
	public class POBoxService : System.Web.Services.WebService
	{
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
		[WebMethod]
		public int Ping(int sleepFor)
		{
			Thread.Sleep(sleepFor * 1000);
			return 0;
		}

		/*
		/// <summary>
		/// Post a message
		/// </summary>
		/// <param name="user"></param>
		/// <param name="message">A message object</param>
		/// <returns>true if the message was posted</returns>
		[WebMethod]
		[SoapDocumentMethod]
		public bool Post(string user, Message message)
		{
			bool	result = false;
			Store	store = Store.GetStore();
			bool	workgroup = (message.DomainID == Simias.Storage.Domain.WorkGroupDomainID);
			string	userID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
			
			if ((userID == null) || (userID.Length == 0))
			{
				// Kludge: for now trust the client.  this need to be removed before shipping.
				userID = user;
			}

			Simias.POBox.POBox box = null;
			
			// temporary, in memory only, subscription object
			Subscription temp = new Subscription(message);
				
			// new subscription
			Subscription subscription = null;

			// create a new subscription object with some of the information from temp
			switch (temp.SubscriptionState)
			{
				case SubscriptionStates.Received:
					// Make sure the from field matches the authenticated userid.
					if (!workgroup && userID == temp.FromIdentity)
					{
						box = Simias.POBox.POBox.GetPOBox(store, message.DomainID, temp.ToIdentity);
						if (box != null)
						{
							subscription = new Subscription(Guid.NewGuid().ToString(), message);
							subscription.SubscriptionState =  SubscriptionStates.Received;
							result = true;
						}
					}
					break;
				
				case SubscriptionStates.Pending:
					if (workgroup)
					{
						box = Simias.POBox.POBox.GetPOBox(store, message.DomainID);
					}
					else
					{
						box = Simias.POBox.POBox.GetPOBox(store, message.DomainID, temp.FromIdentity);
					}
					if (box != null)
					{
						ICSList list = box.Search(Message.MessageIDProperty, message.MessageID, SearchOp.Equal);
			
						ICSEnumerator e = (ICSEnumerator)list.GetEnumerator();
						ShallowNode sn = null;

						if (e.MoveNext())
						{
							sn = (ShallowNode) e.Current;
						}

						if (sn != null)
						{
							subscription = new Subscription(box, sn);
							string ToID;
							if (workgroup)
							{
								ToID = temp.ToIdentity;
							}
							else
							{
								ToID = subscription.ToIdentity;
							}
					
							// Make sure the to field matches the authenticated user.
							if (userID == ToID 
							{
								subscription.SubscriptionState =  SubscriptionStates.Pending;
								subscription.ToName = temp.ToName;
								subscription.ToIdentity = temp.ToIdentity;
								if (workgroup)
								{
									subscription.ToPublicKey = temp.ToPublicKey;
								}
								
								result = true;

								// auto-accept if we have a key, it is not workgroup, and we have rights
								if ((subscription.SubscriptionKey != null)
									&& !workgroup && (subscription.SubscriptionRights != Access.Rights.Deny))
								{
									// accept
									subscription.Accept(store, subscription.SubscriptionRights);
								}
							}
						}
						else
						{
							subscription = new Subscription(
								Guid.NewGuid().ToString(), message);

							subscription.SubscriptionState = 
								SubscriptionStates.Pending;

							subscription.ToName = temp.ToName;
							subscription.ToIdentity = temp.ToIdentity;
							if (workgroup)
							{
								subscription.ToPublicKey = temp.ToPublicKey;
							}
								
							result = true;

							// auto-accept if we have a key, it is not workgroup, and we have rights
							if ((subscription.SubscriptionKey != null)
								&& !workgroup && (subscription.SubscriptionRights != Access.Rights.Deny))
							{
								// accept
								subscription.Accept(store, subscription.SubscriptionRights);
							}

							result = true;
						}
					}
					break;
			}
				
			// commit
			if (result && box != null)
			{
				box.Commit(subscription);
			}
			
			return result;
		}
		*/

		/// <summary>
		/// Acknowledge the subscription.
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		[WebMethod]
		[SoapDocumentMethod]
		public
		void
		AckSubscription(string domainID, string identityID, string messageID)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
			
			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, identityID);

			// check the post office box
			if (poBox == null)
			{
				throw new ApplicationException("PO Box not found.");
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
				throw new ApplicationException("Subscription does not exist.");
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);
			cSub.SubscriptionState = SubscriptionStates.Acknowledged;
			poBox.Commit(cSub);

			// TODO: remove the subscription object?
			poBox.Commit(poBox.Delete(cSub));
		}

		/// <summary>
		/// Get the subscription status
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		/// <returns></returns>
		[WebMethod]
		[SoapDocumentMethod]
		public
		SubscriptionStatus 
		GetSubscriptionStatus(string domainID, string identityID, string messageID)
		{
			Simias.POBox.POBox	poBox;
			Store store = Store.GetStore();

			// open the post office box
			poBox =
				(domainID == Simias.Storage.Domain.WorkGroupDomainID)
					? Simias.POBox.POBox.GetPOBox(store, domainID)
					: Simias.POBox.POBox.GetPOBox(store, domainID, identityID);
			
			// check the post office box
			if (poBox == null)
			{
				throw new ApplicationException("PO Box not found.");
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
				throw new ApplicationException("Subscription does not exists.");
			}

			// get the status object
			Subscription cSub = new Subscription(poBox, sn);
			SubscriptionStatus subStatus = cSub.GenerateStatus();
			return subStatus;
		}

		/// <summary>
		/// Get the subscription details.
		/// </summary>
		/// <param name="domain"></param>
		/// <param name="identity"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		[WebMethod]
		[SoapDocumentMethod]
		public 
		SubscriptionDetails 
		GetSubscriptionDetails(
			string			domainID, 
			string			identityID, 
			string			collectionID)
		{
			Store	store = Store.GetStore();
			SubscriptionDetails details = new SubscriptionDetails();

			// open the collection
			Collection c = store.GetCollectionByID(collectionID);

			// check the collection
			if (c == null)
			{
				throw new ApplicationException("Collection not found.");
			}

			SyncCollection sc = new SyncCollection(c);

			// details
			DirNode dn = sc.GetRootDirectory();
			details.DirNodeID = ( dn != null ) ? dn.ID : null;
			details.DirNodeName = ( dn != null ) ? dn.Name : null;
			details.CollectionUrl = sc.MasterUrl.ToString();

			return details;
		}

		/// <summary>
		/// Get the subscription information
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		/// <returns></returns>
		[WebMethod]
		[SoapDocumentMethod]
		public
		SubscriptionInformation 
		GetSubscriptionInfo(string domainID, string identityID, string messageID)
		{
			Simias.POBox.POBox	poBox;
			Store store = Store.GetStore();

			// open the post office box
			poBox =
				(domainID == Simias.Storage.Domain.WorkGroupDomainID)
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, identityID);
			
			// check the post office box
			if (poBox == null)
			{
				throw new ApplicationException("PO Box not found.");
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
				throw new ApplicationException("Subscription does not exists.");
			}

			// generate the subscription info object and return it
			Subscription cSub = new Subscription(poBox, sn);
			SubscriptionInformation subInfo = new SubscriptionInformation();
			subInfo.GenerateFromSubscription(cSub);
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
		public string Invite(
			string			domainID, 
			string			fromUserID,
			string			toUserID,
			string			sharedCollectionID,
			string			sharedCollectionType)
		{
			Collection			sharedCollection;
			Simias.POBox.POBox	poBox = null;
			Store				store = Store.GetStore();
			Subscription		cSub = null;

			// FIXME:  remove!
			Console.WriteLine("POBoxService::Invite called");
			Console.WriteLine("  DomainID:   " + domainID);
			Console.WriteLine("  fromUserID: " + fromUserID);
			Console.WriteLine("  toUserID:   " + toUserID);
			Console.WriteLine("  colID:      " + sharedCollectionID);

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

			try
			{
				poBox = 
					(domainID == Simias.Storage.Domain.WorkGroupDomainID)
						? POBox.POBox.GetPOBox(store, domainID)
						: POBox.POBox.GetPOBox(store, domainID, toUserID);

				string[] hostAndPort;
				char[] seps = {':'};
				hostAndPort = this.Context.Request.Url.Authority.Split(seps);

				cSub = new Subscription(sharedCollection.Name + " subscription", "Subscription", fromUserID);
				cSub.SubscriptionState = SubscriptionStates.Received;
				cSub.ToName = toMember.Name;
				cSub.ToIdentity = toUserID;
				cSub.FromName = fromMember.Name;
				cSub.FromIdentity = fromUserID;
				cSub.POServiceURL = new Uri("http://" + hostAndPort[0] + ":6436/PostOffice.rem");
				cSub.SubscriptionCollectionID = sharedCollection.ID;
				cSub.SubscriptionCollectionType = sharedCollectionType;
				cSub.SubscriptionCollectionName = sharedCollection.Name;
				//cSub.SubscriptionCollectionURL = "http://" + hostAndPort[0] + ":6436/SyncService.rem";
				cSub.DomainID = domainID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Outbound";  // ????

				SyncCollection sc = new SyncCollection(sharedCollection);
				cSub.SubscriptionCollectionURL = sc.MasterUrl.ToString();

				DirNode dirNode = sharedCollection.GetRootDirectory();
				if(dirNode != null)
				{
					cSub.DirNodeID = dirNode.ID;
					cSub.DirNodeName = dirNode.Name;
				}

				poBox.Commit(cSub);
				return(cSub.MessageID);
			}
			catch{}
			return("");
		}

		/// <summary>
		/// Set the subscription state
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="identityID"></param>
		/// <param name="messageID"></param>
		[WebMethod]
		[SoapDocumentMethod]
		public
		void 
		SetSubscriptionState(
			string				domainID, 
			string				identityID, 
			string				messageID,
			int					state)
		{
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();
			
			// open the post office box
			poBox = (domainID == Simias.Storage.Domain.WorkGroupDomainID) 
				? Simias.POBox.POBox.GetPOBox(store, domainID)
				: Simias.POBox.POBox.GetPOBox(store, domainID, identityID);

			// check the post office box
			if (poBox == null)
			{
				throw new ApplicationException("PO Box not found.");
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
				throw new ApplicationException("Subscription does not exist.");
			}

			// get the subscription object
			Subscription cSub = new Subscription(poBox, sn);
			cSub.SubscriptionState = (Simias.POBox.SubscriptionStates) state;
			poBox.Commit(cSub);
		}

		/// <summary>
		/// Subscribe to a shared collection
		/// </summary>
		/// <param name="domainID"></param>
		/// <param name="poBoxID"></param>
		/// <param name="ToUserID"></param>
		/// <param name="SubscriptionName"></param>
		/// <returns>true subscription ID - false empty string</returns>
		[WebMethod]
		public
		string 
		Subscribe(
			string			domainID, 
			string			fromUserID,
			string			fromUserAlias,
			string			fromUserPubKey,
			string			toUserName,
			string			toUserID,
			string			collectionID,
			string			subscriptionName)
		{
			Simias.POBox.POBox	poBox = null;
			Store			store = Store.GetStore();
			Subscription	cSub = null;
			Collection		sharedCollection;

			if (domainID == null || domainID == "")
			{
				domainID = store.DefaultDomain;
			}
			
			Simias.Storage.Domain	cDomain = store.GetDomain(domainID);
			if (cDomain == null)
			{
				throw new ApplicationException("Invalid Domain ID");
			}

			sharedCollection = store.GetCollectionByID(collectionID);
			if (sharedCollection == null)
			{
				throw new ApplicationException("Invalid shared collection ID");
			}

			// Verify the user
			if (fromUserID == null)
			{
				fromUserID = Guid.NewGuid().ToString();
				fromUserPubKey = Guid.NewGuid().ToString();
			}

			/*
			fromUserID = System.Threading.Thread.CurrentPrincipal.Identity.Name;
			if ((fromUserID == null) || (fromUserID.Length == 0))
			{
				//throw new ApplicationException("Invalid current user.");

				// Temp
			}
			*/

			try
			{
				poBox = 
					(domainID == Simias.Storage.Domain.WorkGroupDomainID)
					? POBox.POBox.GetPOBox(store, domainID)
					: POBox.POBox.GetPOBox(store, domainID, toUserID);

				char[] seps = {':'};
				string[] hostAndPort = this.Context.Request.Url.Authority.Split(seps);

				cSub = new Subscription(subscriptionName, "Subscription", fromUserID);
				cSub.SubscriptionState = SubscriptionStates.Pending;
				cSub.ToName = fromUserAlias;
				cSub.ToIdentity = fromUserID;
				cSub.FromName = toUserName;
				cSub.FromIdentity = toUserID;
				cSub.POServiceURL = new Uri("http://" + hostAndPort[0] + ":6436/PostOffice.rem");
				cSub.SubscriptionCollectionID = sharedCollection.ID;
				cSub.SubscriptionCollectionType = "iFolder";
				cSub.SubscriptionCollectionName = sharedCollection.Name;
				cSub.SubscriptionCollectionURL = "http://" + hostAndPort[0] + ":6436/SyncService.rem";
				cSub.DomainID = domainID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();

				if(domainID == Simias.Storage.Domain.WorkGroupDomainID)
				{
					//cSub.FromPublicKey = fromUserPubKey;
				}
								
				poBox.Commit(cSub);
				return(cSub.ID);
			}
			catch{}
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
		public string	MsgID;
		public string	FromID;
		public string	FromName;
		public string	ToID;
		public string	ToName;

		public string	CollectionID;
		public string	CollectionName;
		public string	CollectionType;
		public string	CollectionUrl;

		public string	DomainID;
		public string	DomainName;

		public int		State;

		public SubscriptionInformation()
		{

		}

		internal void GenerateFromSubscription(Subscription cSub)
		{
			this.MsgID = cSub.MessageID;
			this.FromID = cSub.FromIdentity;
			this.FromName = cSub.FromName;
			this.ToID = cSub.ToIdentity;
			this.ToName = cSub.ToName;

			this.CollectionID = cSub.SubscriptionCollectionID;
			this.CollectionName = cSub.SubscriptionCollectionName;
			this.CollectionType = cSub.SubscriptionCollectionType;
			this.CollectionUrl = cSub.SubscriptionCollectionURL;

			this.DomainID = cSub.DomainID;
			this.DomainName = cSub.DomainName;

			this.State = (int) cSub.SubscriptionState;
		}
	}
}
