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
*                 $Author:  Calvin Gaisford <cgaisford@novell.com> 
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
		/// <summary>
		/// </summary>
		public long AvailableSpace;
		/// <summary>
		/// </summary>
		public long Limit;
		/// <summary>
		/// </summary>
		public long UsedSpace;

		/// <summary>
		/// </summary>
		public DiskSpace()
		{
		}

		/// <summary>
        /// COnstructor
		/// </summary>
		private DiskSpace(long spaceUsed, long limit)
		{
			this.UsedSpace = spaceUsed;
			this.Limit = limit;

			long spaceAvailable = limit - spaceUsed;
			this.AvailableSpace = ( spaceAvailable < 0 ) ? 0 : spaceAvailable;
		}

		private DiskSpace(long spaceUsed, long limit, long spaceAvail)
		{
			this.UsedSpace = spaceUsed;
			this.Limit = limit;

			this.AvailableSpace = spaceAvail;
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
			long limit = 0;
                        long UsedSpaceOnServer = 0;
                        Store store = Store.GetStore();
	                Domain domain = store.GetDomainForUser(UserID);
			Simias.Policy.Rule rule = null;
			if(domain == null)
				throw new Exception("Unable to access domain");

			Simias.Storage.Member simMem = domain.GetMemberByID(UserID);
			if(simMem == null)
				throw new Exception("Invalid UserID");
            Property DiskLimitProp = simMem.Properties.GetSingleProperty(Simias.Policy.DiskSpaceQuota.DiskSpaceQuotaPolicyID);
            Property DiskSpaceUsedProp = simMem.Properties.GetSingleProperty(Simias.Policy.DiskSpaceQuota.UsedDiskSpaceOnServer);

            if(DiskLimitProp != null)
            {
		if(DiskLimitProp.Type == Syntax.String)
	                rule = new Simias.Policy.Rule(DiskLimitProp.Value as string);
		else
	                rule = new Simias.Policy.Rule(DiskLimitProp.Value);
                limit = (long)rule.Operand;
            }

            if(DiskSpaceUsedProp != null)
            {
                UsedSpaceOnServer = (long)DiskSpaceUsedProp.Value;
            }
            return new DiskSpace(UsedSpaceOnServer, limit);

			/*Simias.DomainServices.DomainAgent da = new Simias.DomainServices.DomainAgent();
			long spaceUsed = da.GetDomainDiskSpaceForMember( UserID, out limit );
			return new DiskSpace( spaceUsed, limit );*/
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
			long limit;
			long avail;

			Simias.DomainServices.DomainAgent da = new Simias.DomainServices.DomainAgent();
			long spaceUsed = da.GetDomainDiskSpaceForCollection( iFolderID, out limit, out avail );
			return new DiskSpace( spaceUsed, limit, avail );
		}




		/// <summary>
		/// WebMethod that sets the disk space limit for a member
		/// </summary>
		/// <param name = "UserID">
		/// The ID of the member to set the disk space limit
		/// </param>
		/// <param name = "limit"></param>
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
		/// <param name = "limit"></param>
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
