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
 *  Author: Dale Olds <olds@novell.com>
 *
 ***********************************************************************/
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Simias.Storage;
using Simias;
using Simias.Sync.Client;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to approximate amount of data that is out of sync with master
/// Note that this is worst-case of data that may need to be sent from
/// this collection to the master. It does not include data that may need
/// to be retrieved from the master. It also does not account for
/// delta-sync algorithms that may reduce what needs to be sent
/// </summary>
	public class SyncSize
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		/// <param name="nodeCount"></param>
		/// <param name="maxBytesToSend"></param>
		public static void CalculateSendSize(Collection col, out uint nodeCount, out ulong maxBytesToSend)
		{
			Log.log.Debug("starting to calculate size to send to master for collection {0}", col.Name);

			maxBytesToSend = 0;
			nodeCount = 0;

			maxBytesToSend = SyncClient.GetSizeToSync(col.ID, out nodeCount);
		}
	}

}
//===========================================================================
