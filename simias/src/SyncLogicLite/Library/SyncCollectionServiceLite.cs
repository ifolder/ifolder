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
using System.IO;
using System.Xml;

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Service Lite
	/// </summary>
	public class SyncCollectionServiceLite : SyncCollectionService
	{
		SyncCollection collection;

		public SyncCollectionServiceLite(SyncCollection collection)
			: base(collection)
		{
			this.collection = collection;
		}

		public SyncNodeInfo[] GetNodeInfoArray()
		{
			return collection.GetNodeInfoArray();
		}

		public SyncPacket GetSyncPacket(string id)
		{
			SyncPacket packet = null;

			try
			{
				SyncNode node = collection.GetNode(id);

				packet = new SyncPacket(node);
			}
			catch(Exception e)
			{
				// ignore ?
				MyTrace.WriteLine(e);
			}

			return packet;
		}

		public ulong CommitSyncPacket(SyncPacket packet)
		{
			return CommitSyncPacket(packet, 0);
		}

		public ulong CommitSyncPacket(SyncPacket packet, ulong masterIncarnation)
		{
			ulong result = 0;

			try
			{
				result = SyncPacket.Commit(packet, collection, masterIncarnation);
			}
			catch(Exception e)
			{
				// ignore ?
				MyTrace.WriteLine(e);
			}

			return result;
		}

		public void DeleteNode(string id)
		{
			SyncNode node = collection.GetNode(id);

			MyTrace.WriteLine("Deleting Node: {0} ({1}.{2})", node.NodePath, node.MasterIncarnation, node.LocalIncarnation);

			node.Delete();
		}
	}
}
