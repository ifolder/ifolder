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
using System.Runtime.Remoting;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Service
	/// </summary>
	public class SyncCollectionService : MarshalByRefObject
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncCollectionService));

		protected SyncCollection collection;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="collection">The collection object.</param>
		public SyncCollectionService(SyncCollection collection)
		{
			log.Debug("Creating Sync Collection Service: {0}", collection.Name);
			
			this.collection = collection;
		}

		/// <summary>
		/// Generate the ping information for the collection.
		/// </summary>
		/// <returns></returns>
		public SyncCollectionInfo Ping()
		{
			log.Debug("About to new up a SyncCollectionInfo dude...");

			SyncCollectionInfo info = new SyncCollectionInfo(collection);

			log.Debug("Preparing Ping Response: {0}", info);

			return info;
		}

		/// <summary>
		/// Release this object on the server.
		/// </summary>
		public void Release()
		{
			RemotingServices.Disconnect(this);
		}
	}
}
