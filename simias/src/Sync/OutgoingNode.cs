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
using System.Collections;
using System.IO;
//using System.Xml;
//using System.Diagnostics;

using Simias.Storage;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to dish out Node information in pieces.
/// </summary>
internal class OutgoingNode
{
	Collection collection;
	class Fork { public string name; public Stream stream; };
	ArrayList forkList;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="collection"></param>
	public OutgoingNode(Collection collection)
	{
		this.collection = collection;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="node"></param>
	/// <returns></returns>
	public static string GetOutNode(Collection collection, ref Node node)
	{
		string path;
		Conflict cf = new Conflict(collection, node);
		if (cf.IsUpdateConflict)
		{
			path = cf.UpdateConflictPath;
			node = cf.UpdateConflictNode;
		}
		else if (cf.IsFileNameConflict)
			path = cf.FileNameConflictPath;
		else
			path = cf.NonconflictedPath;
		return path;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="node"></param>
	/// <returns></returns>
	public static NodeStamp GetOutNodeStamp(Collection collection, ref Node node, ChangeLogRecord.ChangeLogOp changeType)
	{
		string path;
		NodeStamp stamp = new NodeStamp();
				
		Conflict cf = new Conflict(collection, node);
		if (cf.IsUpdateConflict)
		{
			path = cf.UpdateConflictPath;
			node = cf.UpdateConflictNode;
		}
		else if (cf.IsFileNameConflict)
			path = cf.FileNameConflictPath;
		else
			path = cf.NonconflictedPath;
		
		bool tombstone = collection.IsType(node, NodeTypes.TombstoneType);
		stamp.localIncarn = tombstone? UInt64.MaxValue: node.LocalIncarnation;
		stamp.masterIncarn = cf.IsUpdateConflict ? node.LocalIncarnation : node.MasterIncarnation;
		stamp.id = (Nid)node.ID;
		stamp.name = node.Name;
		stamp.isDir = collection.IsType(node, NodeTypes.DirNodeType);
		stamp.changeType = changeType;

		//TODO: another place to handle multiple forks
		try
		{
			stamp.streamsSize = path == null? -1: new FileInfo(path).Length;
		}
		catch (Exception e)
		{
			Log.Spew("Could not get file size of {0}: {1}", path, e);
			stamp.streamsSize = 0;
		}

		return stamp;
	}


	public Node Start(Nid nid)
	{
		nid.Validate();
		forkList = null;

		/* always construct a plain node instead of the one returned by
		 * GetNodeByID in case it is a Collection node which is not
		 * serializable (but a raw node is).
		 */
		Node node = collection.GetNodeByID(nid);
		// If the node does not exist or it has collisions do not sync.
		if (node == null || collection.HasCollisions(node))
		{
			string errorString;
			if (node == null)
				errorString = "Node does not exist";
			else
				errorString = "Node has collision";

			Log.log.Debug("ignoring attempt to start outgoing sync for node {0}. {1}", nid, errorString);
			return null;
		}

		node = new Node(node);
		
		string path = GetOutNode(collection, ref node);
		if (path != null)
		{
			/* TODO: handle multiple forks (streams), EAs, etc. For right now
			 * this is just a guess at how to do it. The idea is that we loop
			 * though all known streams, read them from the local file system
			 * or from those stored in the collection store area
			 * (similar to StoreFiles) if not supported by the local file system.
			 */
			forkList = new ArrayList();
			string forkName = ForkChunk.DataForkName;
			//foreach (string forkName in forkNameList)
			{
				Fork fork = new Fork();
				fork.name = forkName;
				fork.stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				forkList.Add(fork);
			}
		}
		return node;
	}

	public ForkChunk[] ReadChunks(int maxSize, out int totalSize)
	{
		if (maxSize < 4096)
			maxSize = 4096;
		ArrayList chunks = new ArrayList();
		totalSize = 0;
		if (forkList != null)
			foreach (Fork fork in forkList)
			{
				if (fork.name == null)
					continue;

				ForkChunk chunk = new ForkChunk();
				chunk.name = fork.name;
				long remaining = fork.stream.Length - fork.stream.Position;
				int chunkSize = remaining < maxSize? (int)remaining: maxSize;
				chunk.data = new byte[chunkSize];
				int bytesRead = fork.stream.Read(chunk.data, 0, chunkSize);
				Log.Assert(bytesRead == chunkSize);
				if (chunkSize < maxSize)
				{
					fork.stream.Close();
					fork.stream = null;
					fork.name = null;
				}
				totalSize += chunkSize;
				chunks.Add(chunk);
				if (totalSize >= maxSize)
					break;
			}
		return chunks.Count == 0? null: (ForkChunk[])chunks.ToArray(typeof(ForkChunk));
	}
}

//===========================================================================
}
