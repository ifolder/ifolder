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
using Simias;
using Simias.Storage;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists only to represent DiskSpaceQuota
	/// </summary>
	[Serializable]
	public class DiskSpace 
	{
		public long AvailableSpace;
		public long Limit;
		public long UsedSpace;

		public DiskSpace()
		{
		}

		public DiskSpace(Simias.Policy.DiskSpaceQuota quota)
		{
			this.AvailableSpace = quota.AvailableSpace;
			this.Limit = quota.Limit;
			this.UsedSpace = quota.UsedSpace;
		}




		/// <summary>
		/// WebMethod that gets the DiskSpaceQuota for a given member
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to get the DiskSpaceQuota
		/// </param>
		/// <returns>
		/// DiskSpaceQuota for the specified member
		/// </returns>
		public static DiskSpace GetMemberDiskSpace( string UserID )
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomainForUser(UserID);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member simMem = domain.GetMemberByID(UserID);
			if(simMem == null)
				throw new Exception("Invalid UserID");

			Simias.Policy.DiskSpaceQuota squota =
				Simias.Policy.DiskSpaceQuota.Get(simMem);
			if(squota == null)
				throw new Exception("Unable to get Disk Space Quota");

			return new DiskSpace(squota);
		}




		/// <summary>
		/// WebMethod that gets the DiskSpaceQuota for a given iFolder
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to get the DiskSpaceQuota
		/// </param>
		/// <returns>
		/// DiskSpaceQuota for the specified iFolder
		/// </returns>
		public static DiskSpace GetiFolderDiskSpace( string iFolderID )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");


			DiskSpace space = new DiskSpace();
			space.UsedSpace = col.StorageSize;
			space.Limit = Simias.Policy.DiskSpaceQuota.GetLimit(col);
			space.AvailableSpace = space.Limit - space.UsedSpace;
			if(space.AvailableSpace < 0)
				space.AvailableSpace = 0;

			return space;
		}




		/// <summary>
		/// WebMethod that sets the disk space limit for a member
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to set the disk space limit
		/// </param>
		public static void SetUserDiskSpaceLimit( string UserID, long limit )
		{
			Store store = Store.GetStore();

			Domain domain = store.GetDomainForUser(UserID);
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member simMem = domain.GetMemberByID(UserID);
			if(simMem == null)
				throw new Exception("Invalid UserID");

			Simias.Policy.DiskSpaceQuota.Set(simMem, limit);
		}




		/// <summary>
		/// WebMethod that sets the disk space limit for an iFolder 
		/// </summary>
		/// <param name = "iFolderID">
		/// The ID of the iFolder to set the disk space limit
		/// </param>
		public static void SetiFolderDiskSpaceLimit( string iFolderID, 
													long limit )
		{
			Store store = Store.GetStore();

			Collection col = store.GetCollectionByID(iFolderID);
			if(col == null)
				throw new Exception("Invalid iFolderID");

			Simias.Policy.DiskSpaceQuota.Set(col, limit);
		}
	}
}
