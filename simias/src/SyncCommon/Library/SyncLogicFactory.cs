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
	/// Sync Logic Factory
	/// </summary>
	public class SyncLogicFactory
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncLogicFactory()
		{
		}

		/// <summary>
		/// Generate a collection service object.
		/// </summary>
		/// <param name="collection">A collection object for the sync collection service.</param>
		/// <returns>A new service object.</returns>
		public virtual SyncCollectionService GetCollectionService(SyncCollection collection)
		{
			return new SyncCollectionService(collection);
		}

		/// <summary>
		/// Get the collection service object type.
		/// </summary>
		/// <returns>A type object for the service class.</returns>
		public virtual Type GetCollectionServiceType()
		{
			return typeof(SyncCollectionService);
		}

		/// <summary>
		/// Generate a collection worker object.
		/// </summary>
		/// <param name="master">The master collection object.</param>
		/// <param name="slave">The slave collection object.</param>
		/// <returns>A new worker object.</returns>
		public virtual SyncCollectionWorker GetCollectionWorker(SyncCollectionService master, SyncCollection slave)
		{
			return new SyncCollectionWorker(master, slave);
		}

		/// <summary>
		/// Should the collection watcher be running?
		/// </summary>
		/// <returns>true, if the collection watcher should be started.</returns>
		public virtual bool WatchFileSystem()
		{
			return true;
		}
	}
}
