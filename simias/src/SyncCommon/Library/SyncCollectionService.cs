/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Rob
 * 
 ***********************************************************************/

using System;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Service
	/// </summary>
	public class SyncCollectionService : MarshalByRefObject
	{
		private SyncCollection collection;

		public SyncCollectionService(SyncCollection collection)
		{
			MyTrace.WriteLine("Creating Sync Collection Service: {0}", collection.ID);
			this.collection = collection;
			
			// refresh the connection
			collection.Refresh();

			MyTrace.WriteLine("Refreshed Sync Collection Service: {0}", collection.ID);
		}

		public SyncCollectionInfo Ping()
		{
			SyncCollectionInfo info = new SyncCollectionInfo(collection);

			MyTrace.WriteLine("Preparing Ping Response: {0}", info);

			return info;
		}
	}
}
