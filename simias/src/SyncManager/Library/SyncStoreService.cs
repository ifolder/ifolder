/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Diagnostics;

namespace Simias.Sync
{
	/// <summary>
	/// Store Store Service
	/// </summary>
	public class SyncStoreService : MarshalByRefObject
	{
		private SyncStoreManager manager;

		/// <summary>
		/// Get a lifetime service object to control the lifetime policy.
		/// </summary>
		/// <returns>An ILease object used to control the lifetime policy.</returns>
		public override object InitializeLifetimeService()
		{
			// infinite lease time
			return null;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="manager">The sync store manager object.</param>
		public SyncStoreService(SyncStoreManager manager)
		{
			this.manager = manager;
		}

		/// <summary>
		/// Generate the sync store information.
		/// </summary>
		/// <returns></returns>
		public SyncStoreInfo Ping()
		{
			return new SyncStoreInfo(manager.Store);
		}

		/// <summary>
		/// Get the sync collection service.
		/// </summary>
		/// <param name="id">The collection id.</param>
		/// <returns>The sync collection service object.</returns>
		public SyncCollectionService GetCollectionService(string id)
		{
			return manager.GetCollectionService(id);
		}

		public static string GetEndPoint(int port)
		{
			return String.Format("SyncStoreService{0}.rem", port);
		}

		#region Properties
		
		public string EndPoint
		{
			get { return GetEndPoint(manager.Manager.MasterUri.Port); }
		}

		public Uri ServiceUrl
		{
			get
			{
				UriBuilder ub = new UriBuilder(manager.Manager.MasterUri);
				ub.Path = EndPoint;

				return ub.Uri;
			}
		}

		#endregion
	}
}
