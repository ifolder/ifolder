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
	/// <param name="collection"></param>
	/// <param name="nodeCount"></param>
	/// <param name="maxBytesToSend"></param>
	public static void CalculateSendSize(Collection collection, out uint nodeCount, out ulong maxBytesToSend)
	{
		maxBytesToSend = 0;
		nodeCount = 0;
		Log.Spew("starting to calculate size to send to master for collection {0}", collection.Name);

		// TODO: this call can leave tombstones on the server. see note in Dredger
		new Dredger(collection, false); 

		// TODO: would be nice to have the database find all nodes for which MasterIncarnation != LocalIncarnation
		foreach (ShallowNode sn in collection)
		{
			Node node = new Node(collection, sn);
			if (node.MasterIncarnation != node.LocalIncarnation)
			{
				long fileSize = 0;

				BaseFileNode bfn = null;
				if (collection.IsType(node, typeof(FileNode).Name))
					bfn = new FileNode(node);
				else if (collection.IsType(node, typeof(StoreFileNode).Name))
					bfn = new StoreFileNode(node);

				if (bfn != null)
					fileSize = new FileInfo(bfn.GetFullPath(collection)).Length;

				// TODO: would be nice to have a faster to get serializable-size rather than actually serializing it.
				MemoryStream ms = new MemoryStream();
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(ms, node);

				Log.Spew("Adding node {0} to send size: fileSize = {1}, nodeSize = {2}", node.Name, fileSize, ms.Length );
				maxBytesToSend += (ulong)fileSize + (ulong)ms.Length;
				nodeCount++;
			}
		}
	}

}

//===========================================================================
}
