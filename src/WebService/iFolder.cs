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
	public class iFolderWeb
	{
		public static readonly string FilesDirName = "SimiasFiles";
		public static readonly string iFolderType = "iFolder";

		public string DomainID;
		public string ID;
		public ulong LocalIncarnation;
		public string ManagedPath;
		public string UnManagedPath;
		public ulong MasterIncarnation;
        public string Name;
		public string Owner;
		public string OwnerID;
		public int EffectiveSyncInterval;
		public int SyncInterval;
		public bool Synchronizable;
		public string Type;
		public string Description;
		public string State;
		public bool IsSubscription;
		public int EnumeratedState;
		public bool IsWorkgroup;
		public bool HasConflicts;
		public string CurrentUserID;
		public string CurrentUserRights;
		public string CollectionID;
		public string LastSyncTime;

		public iFolderWeb()
		{
		}

		public iFolderWeb(Collection collection)
		{
			this.DomainID = collection.Domain;
			this.ID = collection.ID;
			this.CollectionID = collection.ID;
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
			{
				this.Owner = collection.Owner.Name;
				this.OwnerID = collection.Owner.UserID;
			}
			else
			{
				this.Owner = "Not available";
				this.OwnerID = "0";
			}

			this.SyncInterval = 
				Simias.Policy.SyncInterval.GetInterval(collection);
			this.Synchronizable = collection.Synchronizable;
			this.Type = iFolderType;
			this.Description = "";
			this.IsSubscription = false;
			this.EnumeratedState = -1;

			// There is no longer a WorkGroup domain ... for now return false. We
			// probably need to change this to return a type instead.
			this.IsWorkgroup = false;
			this.HasConflicts = collection.HasCollisions();

			Member tmpMember = collection.GetCurrentMember();
			this.CurrentUserID = tmpMember.UserID;
			this.CurrentUserRights = tmpMember.Rights.ToString();

			Simias.Policy.SyncInterval si = Simias.Policy.SyncInterval.Get(tmpMember, collection);
			this.EffectiveSyncInterval = si.Interval;
			DateTime lastSyncTime = Simias.Sync.SyncClient.GetLastSyncTime(collection.ID);
			if (lastSyncTime.Equals(DateTime.MinValue))
			{
				this.LastSyncTime = "";
				this.State = "WaitSync";
			}
			else
			{
				this.LastSyncTime = lastSyncTime.ToString();
				this.State = collection.IsProxy ? "WaitSync" : "Local";
			}
		}


		public iFolderWeb(Subscription subscription)
		{
			this.DomainID = subscription.DomainID;
			this.Name = subscription.SubscriptionCollectionName;
			this.ID = subscription.ID;
			this.CollectionID = subscription.SubscriptionCollectionID;
			this.Description = subscription.CollectionDescription;
			this.IsSubscription = true;
			this.EnumeratedState = (int) subscription.SubscriptionState;
			this.Owner = subscription.FromName;
			this.OwnerID = subscription.FromIdentity;
			this.CurrentUserRights = subscription.SubscriptionRights.ToString();

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
