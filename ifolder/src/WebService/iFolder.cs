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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using Simias.Storage;
using Simias.POBox;
using Simias.Sync;
using System.Xml;
using System.Xml.Serialization;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists only to represent an iFolder and should only be
	/// used in association with the iFolderWebService class.
	/// </summary>
	[Serializable]
	public class iFolder
	{
		public static readonly string FilesDirName = "SimiasFiles";
		public static readonly string iFolderType = "iFolder";

		public string Domain;
		public string DomainIdentity;
		public string ID;
		public ulong LocalIncarnation;
		public string ManagedPath;
		public string UnManagedPath;
		public ulong MasterIncarnation;
        public string Name;
		public string Owner;
		public int SyncInterval;
		public bool Synchronizable;
		public string Type;
		public string Description;
		public string State;
		public bool IsSubscription;
		public int EnumeratedState;
		public bool IsWorkgroup;
		public bool HasCollisions;
		public string CurrentUserID;

		public iFolder()
		{
		}

		public iFolder(Collection collection)
		{
			this.Domain = collection.Domain;
			this.DomainIdentity = collection.Domain;
			this.ID = collection.ID;
			this.LocalIncarnation = collection.LocalIncarnation;
			DirNode dirNode = collection.GetRootDirectory();
			if(dirNode != null)
				this.UnManagedPath = dirNode.GetFullPath(collection);
			else
				this.UnManagedPath = "";
			this.ManagedPath = collection.ManagedPath;
			this.MasterIncarnation = collection.MasterIncarnation;
			this.Name = collection.Name;
			if(collection.Owner != null)
				this.Owner = collection.Owner.Name;
			else
				this.Owner = "Not available";

			this.SyncInterval = 
				Simias.Policy.SyncInterval.GetInterval(collection);
			this.Synchronizable = collection.Synchronizable;
			this.Type = iFolderType;
			this.Description = "";
			this.State = "Local";
			this.IsSubscription = false;
			this.EnumeratedState = -1;
			this.IsWorkgroup = 
				(collection.Domain == Simias.Storage.Domain.WorkGroupDomainID);
			this.HasCollisions = collection.HasCollisions();
			this.CurrentUserID = collection.GetCurrentMember().UserID;
		}


		public iFolder(Subscription subscription)
		{
			this.Name = subscription.SubscriptionCollectionName;
			this.ID = subscription.ID;
			this.Description = subscription.CollectionDescription;
			this.IsSubscription = true;
			this.EnumeratedState = (int) subscription.SubscriptionState;

			if(	(subscription.SubscriptionState == 
								SubscriptionStates.Ready) ||
						(subscription.SubscriptionState == 
								SubscriptionStates.Received) )
			{
				this.State = "Available";
			}
			else if(subscription.SubscriptionState == 
									SubscriptionStates.Replied)
			{
				this.State = "WaitConnect";
			}
			else if(subscription.SubscriptionState == 
								SubscriptionStates.Delivered)
			{
				this.State = "WaitSync";
			}
/*			else if(	(subscription.SubscriptionState == 
								SubscriptionStates.Replied) ||
						(subscription.SubscriptionState == 
								SubscriptionStates.Delivered) ||
						(subscription.SubscriptionState == 
								SubscriptionStates.Pending) ||
						(subscription.SubscriptionState == 
								SubscriptionStates.Responded) ||
						(subscription.SubscriptionState == 
								SubscriptionStates.Acknowledged) )
*/
			else
			{
				this.State = "Unknown";
			}
		}
	}
}
