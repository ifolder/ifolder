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

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Worker
	/// </summary>
	public class SyncCollectionWorker
	{
		protected SyncCollection collection;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncCollectionWorker(SyncCollection collection)
		{
			this.collection = collection;
		}

		/// <summary>
		/// Do the sync work.
		/// </summary>
		public virtual void DoSyncWork(SyncCollectionService service)
		{
		}

		/// <summary>
		/// A stop syncing request.
		/// </summary>
		public virtual void StopSyncWork()
		{
		}
	}
}
