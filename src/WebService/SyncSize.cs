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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using Simias;
using Simias.Sync;

namespace Novell.iFolder.Web
{
	/// <summary>
	/// This class exists to pass the sync size of a collection via the
	/// WebService.
	/// </summary>
	[Serializable]
	public class SyncSize
	{
		/// <summary>
		/// The number of nodes to sync.
		/// </summary>
		public uint SyncNodeCount;

		/// <summary>
		/// The number of bytes to sync
		/// </summary>
		public ulong SyncByteCount;

		/// <summary>
		/// Constructs a SyncSize object.
		/// </summary>
		public SyncSize()
		{
		}

		/// <summary>
		/// Constructs a SyncSize object.
		/// </summary>
		/// <param name="SyncNodeCount">The number of nodes that need to be sync'd.</param>
		/// <param name="SyncByteCount">The number of bytes that need to be sync'd.</param>
		public SyncSize(uint SyncNodeCount, ulong SyncByteCount)
		{
			this.SyncNodeCount = SyncNodeCount;
			this.SyncByteCount = SyncByteCount;
		}
	}
}
