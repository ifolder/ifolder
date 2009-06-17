/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Calvin Gaisford <cgaisford@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using Simias.Storage;
using Simias.POBox;
using Simias.Discovery;
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
		/// <summary>
		/// </summary>
		public static readonly string FilesDirName = "SimiasFiles";
		/// <summary>
		/// </summary>
		public static readonly string iFolderType = "iFolder";

		/// <summary>
		/// </summary>
		public string DomainID;
		/// <summary>
		/// </summary>
		public string ID;
		/// <summary>
		/// </summary>
		public ulong LocalIncarnation;
		/// <summary>
		/// </summary>
		public string ManagedPath;
		/// <summary>
		/// </summary>
		public string UnManagedPath;
		/// <summary>
		/// </summary>
		public ulong MasterIncarnation;
		/// <summary>
		/// </summary>
  	        public string Name;
		/// <summary>
		/// </summary>
		public string Owner;
		/// <summary>
		/// </summary>
		public string OwnerID;
		/// <summary>
		/// </summary>
		public int EffectiveSyncInterval;
		/// <summary>
		/// </summary>
		public int SyncInterval;
		/// <summary>
		/// </summary>
		public bool Synchronizable;
		/// <summary>
		/// </summary>
		public string Type;
		/// <summary>
		/// </summary>
		public string Description;
		/// <summary>
		/// </summary>
		public string State;
		/// <summary>
		/// </summary>
		public bool IsSubscription;
		/// <summary>
		/// </summary>
		public int EnumeratedState;
		/// <summary>
		/// </summary>
		public bool IsWorkgroup;
		/// <summary>
		/// </summary>
		public bool HasConflicts;
		/// <summary>
		/// </summary>
		public string CurrentUserID;
		/// <summary>
		/// </summary>
		public string CurrentUserRights;
		/// <summary>
		/// </summary>
		public string CollectionID;
		/// <summary>
		/// </summary>
		public string LastSyncTime;
		/// <summary>
		/// </summary>
		public string Role;

		/// <summary>
		/// </summary>
		public bool ssl;

		public string encryptionAlgorithm;

		public int MigratediFolder;
		
		/// <summary>
		/// whether collection is shared or not 
		/// </summary>
		public bool shared;

        public long iFolderSize;
		
		/// <summary>
		/// </summary>
		public iFolderWeb()
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Collcetion</param>
        public iFolderWeb(Collection collection, int infoToFetch)
        {
            Member tmpMember = null;

            //base information that is mandatory
            this.DomainID = collection.Domain;
            this.ID = collection.ID;
            this.CollectionID = collection.ID;
            this.LocalIncarnation = collection.LocalIncarnation;

            //infoToFetch is a bitmap, for now using it as a boolean
            if (infoToFetch > 0)
            {
                DirNode dirNode = collection.GetRootDirectory();
                if (dirNode != null)
                    this.UnManagedPath = dirNode.GetFullPath(collection);
                else
                    this.UnManagedPath = "";
                this.ManagedPath = collection.ManagedPath;
                this.MasterIncarnation = collection.MasterIncarnation;
                this.Name = collection.Name;
                DateTime lastSyncTime = Simias.Sync.SyncClient.GetLastSyncTime(collection.ID);
                if (collection.Role.Equals(SyncRoles.Master))
                {
                    this.LastSyncTime = string.Empty;
                    this.State = "Local";
                }
                else if (lastSyncTime.Equals(DateTime.MinValue))
                {
                    this.LastSyncTime = string.Empty;
                    this.State = "WaitSync";
                }
                else
                {
                    this.LastSyncTime = lastSyncTime.ToString();
                    this.State = collection.IsProxy ? "WaitSync" : "Local";
                }
                this.HasConflicts = collection.HasCollisions();

                tmpMember = collection.GetCurrentMember();

                this.CurrentUserID = tmpMember.UserID;
                this.CurrentUserRights = tmpMember.Rights.ToString();

                this.Role = collection.Role.ToString();
                this.ssl = collection.SSL;
                if (collection.EncryptionAlgorithm == null || collection.EncryptionAlgorithm == "")
                    this.encryptionAlgorithm = "";
                else
                    this.encryptionAlgorithm = collection.EncryptionAlgorithm;

                this.MigratediFolder = collection.MigratediFolder;

                tmpMember = collection.Owner;
                if (tmpMember != null)
                {
                    this.Owner = tmpMember.Name;
                    this.OwnerID = tmpMember.UserID;
                }
                else
                {
                    this.Owner = "Not available";
                    this.OwnerID = "0";
                }

                ICSList memberList;

                memberList = collection.GetMemberList();
                if (memberList.Count > 1)
                    this.shared = true;
                else
                    this.shared = false;
                this.iFolderSize = collection.StorageSize;

            }
        }

		/// <summary>
        /// Constructor
		/// </summary>
		public iFolderWeb(Collection collection)
		{
			Member tmpMember = null;
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
       			tmpMember = collection.Owner;
			
        		if (tmpMember != null)
			{
				this.OwnerID = tmpMember.UserID;
				Domain domain = Store.GetStore().GetDomain(this.DomainID);
				Member domainMember = domain.GetMemberByID(this.OwnerID);
				string fullName = domainMember.FN;
				this.Owner = (fullName != null) ? fullName : tmpMember.Name;
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

			tmpMember = collection.GetCurrentMember();

			this.CurrentUserID = tmpMember.UserID;
			this.CurrentUserRights = tmpMember.Rights.ToString();
			int EffectiveSync = tmpMember.EffectiveSyncPolicy(collection);
			if(EffectiveSync > 0)
			{
				this.EffectiveSyncInterval = EffectiveSync;
				//Simias.Policy.SyncInterval.Set(collection, EffectiveSync);
			}

			DateTime lastSyncTime = Simias.Sync.SyncClient.GetLastSyncTime(collection.ID);
			if (collection.Role.Equals(SyncRoles.Master))
			{
				this.LastSyncTime = string.Empty;
				this.State = "Local";
			}
			else if (lastSyncTime.Equals(DateTime.MinValue))
			{
				this.LastSyncTime = string.Empty;
				this.State = "WaitSync";
			}
			else
			{
				this.LastSyncTime = lastSyncTime.ToString();
				this.State = collection.IsProxy ? "WaitSync" : "Local";
			}

			this.Role = collection.Role.ToString();
			this.ssl = collection.SSL;
			if( collection.EncryptionAlgorithm == null || collection.EncryptionAlgorithm == "" )
				this.encryptionAlgorithm = "";
			else
				this.encryptionAlgorithm = collection.EncryptionAlgorithm;
			this.MigratediFolder = collection.MigratediFolder;
			ICSList memberList;

			memberList = collection.GetMemberList();
			if( memberList.Count >1)
				this.shared = true;
			else
				this.shared = false;
            this.iFolderSize = collection.StorageSize;
		}


		/// <summary>
        /// Constructor
		/// </summary>
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
			Domain domain = Store.GetStore().GetDomain(subscription.DomainID);
			if(domain != null)
			{
				Simias.Storage.Member member = domain.GetMemberByID(subscription.FromIdentity);
				if(member != null)
					this.Owner = member.FN;
			}

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

		/// <summary>
	    /// CollectionInfo from DiscoveryFrameWork
		/// </summary>
		public iFolderWeb(CollectionInfo c)
		{
			this.DomainID = c.DomainID;
			this.Name = c.Name;
			this.ID = c.ID;
			this.CollectionID = c.CollectionID;
			this.Description = c.Description;

                        // TODO : Remove the following  line when cleaning up POBox.
			this.IsSubscription = true;
			this.EnumeratedState = (int) SubscriptionStates.Ready;
			this.Owner = c.OwnerFullName;
			this.CurrentUserID = c.MemberUserID;

			Domain domain = Store.GetStore().GetDomain(c.DomainID);
			if(domain != null)
			{
				Simias.Storage.Member member = domain.GetMemberByID(c.OwnerID);
				if(member != null)
					this.Owner = member.FN;
			}

			this.OwnerID = c.OwnerID;
//			this.CurrentUserRights = "Admin";
			this.CurrentUserRights = c.UserRights;

			this.State = "WaitSync";
			if( c.encryptionAlgorithm != null)
				this.encryptionAlgorithm = c.encryptionAlgorithm;
			else
				this.encryptionAlgorithm = "";
			this.MigratediFolder = c.MigratediFolder;
            this.iFolderSize = c.Size;
		}

	}
}
