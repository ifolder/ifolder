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
		Stream stream;
	
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
		/// <param name="changeType"></param>
		/// <returns></returns>
		public static NodeStamp GetOutNodeStamp(Collection collection, ref Node node, ChangeLogRecord.ChangeLogOp changeType)
		{
			NodeStamp stamp = new NodeStamp();
			stamp.localIncarn = node.LocalIncarnation;
			stamp.masterIncarn = node.MasterIncarnation;
			stamp.id = node.ID;
			stamp.type = node.BaseType;
			stamp.changeType = changeType;
			return stamp;
		}


		public Node Start(string nid)
		{
			stream = null;

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
				stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
			return node;
		}

		public byte[] ReadChunk(int maxSize, out int totalSize)
		{
			totalSize = 0;
			if (stream != null)
			{
				long remaining = stream.Length - stream.Position;
				int chunkSize = remaining < maxSize? (int)remaining: maxSize;
				byte[] data = new byte[chunkSize];
				int bytesRead = stream.Read(data, 0, chunkSize);
				Log.Assert(bytesRead == chunkSize);
				if (chunkSize < maxSize)
				{
					// We have read to the end of the file.
					stream.Close();
					stream = null;
				}
				totalSize += chunkSize;
				return data;
			}
			return null;
		}
	}

//===========================================================================
}
