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
*                 $Author: Bruce Getter <bgetter@novell.com> 
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
