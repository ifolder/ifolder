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

namespace Simias.Subscribe.Web
{
	/// <summary>
	/// Subscribe Web Service is used in the Simias workgroup model.
	/// Simias clients may use the subscribe methods to request access
	/// to a public collection.
	/// </summary>
	/// 
	
	[WebService(Namespace="http://novell.com/simias/subscribe/")]
	public class SubscribeService : System.Web.Services.WebService
	{
		public SubscribeService()
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
		/// Benign method clients can use to determine if the 
		/// SubscriptionService is up and running.
		/// </summary>
		/// <param name="sleepFor"></param>
		/// <returns>0</returns>
		[WebMethod]
		[SoapDocumentMethod]
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
		string
		Subscribe(
			string				fromID,
			string				fromAlias,
			string				fromCredentialInfo,
			string				sharedCollectionID)
		{
			Collection			sharedCollection;
			Simias.POBox.POBox	poBox;
			Store				store = Store.GetStore();

			// Verify domain
			Simias.Storage.Domain cDomain = 
				store.GetDomain(Simias.Storage.Domain.WorkGroupDomainID);
			if (cDomain == null)
			{
				throw new ApplicationException("Can't discover the workgroup domain ID");
			}

			// Get the shared collection the caller is requesting access to
			sharedCollection = store.GetCollectionByID(sharedCollectionID); 
			if (sharedCollection == null)
			{
				throw new ApplicationException("Invalid shared collection ID");
			}
			
			// Get the workgroup PO Box
			poBox = Simias.POBox.POBox.GetPOBox(store, Simias.Storage.Domain.WorkGroupDomainID);
			if (poBox == null)
			{
				throw new ApplicationException("Workgroup PO Box not found.");
			}

			try
			{
				Subscription cSub = 
					new Subscription(
							sharedCollection.Name + " subscription", 
							"Subscription", 
							fromID);
				cSub.SubscriptionState = Simias.POBox.SubscriptionStates.Received;
				//cSub.ToName = toMember.Name;
				//cSub.ToIdentity = toUserID;
				cSub.FromName = fromAlias;
				cSub.FromIdentity = fromID;

				// FIXME:
				string serviceUrl = 
					"http://" + 
					this.Context.Request.Url.Host +
					":" +
					this.Context.Request.Url.Port.ToString() +
					"/SubscribeService.asmx";

				cSub.POServiceURL = new Uri(serviceUrl);
				cSub.SubscriptionCollectionID = sharedCollection.ID;
				cSub.SubscriptionCollectionType = sharedCollection.Type;
				cSub.SubscriptionCollectionName = sharedCollection.Name;
				cSub.DomainID = Simias.Storage.Domain.WorkGroupDomainID;
				cSub.DomainName = cDomain.Name;
				cSub.SubscriptionKey = Guid.NewGuid().ToString();
				cSub.MessageType = "Inbound";
				cSub.SubscriptionCollectionURL = sharedCollection.MasterUrl.ToString();

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

		public string	DirNodeID;
		public string	DirNodeName;

		public string	DomainID;
		public string	DomainName;

		public int		State;
		public int		Disposition;

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

			this.DirNodeID = cSub.DirNodeID;
			this.DirNodeName = cSub.DirNodeName;

			this.DomainID = cSub.DomainID;
			this.DomainName = cSub.DomainName;

			this.State = (int) cSub.SubscriptionState;
			this.Disposition = (int) cSub.SubscriptionDisposition;
		}
	}
}
