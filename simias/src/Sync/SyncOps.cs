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
 *  Author: Dale Olds <olds@novell.com>
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;

using Simias.Storage;
using Simias.Identity;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
[Serializable]
public struct NodeId
{
	public string s;

	public int CompareTo(object obj)
	{
		if (!(obj is NodeId))
			throw new ArgumentException("object is not NodeId");
		return String.Compare(((NodeId)obj).s, s, true);
	}
}

[Serializable]
public struct NodeStamp: IComparable
{
	public NodeId id;

	// if localIncarn == UInt64.MaxValue, node is a tombstone
	public ulong localIncarn, masterIncarn;

	// total size of all streams
	public ulong streamsSize;

	public string name; //just for debug

	public int CompareTo(object obj)
	{
		if (!(obj is NodeStamp))
			throw new ArgumentException("object is not NodeStamp");
		return String.Compare(((NodeStamp)obj).id.s, id.s, true);
	}

	public static void Clear(out NodeStamp stamp)
	{
		stamp.id.s = stamp.name = null;
		stamp.localIncarn = stamp.masterIncarn = stamp.streamsSize = 0;
	}
}

[Serializable]
public struct SmallNode
{
	public const int MaxSize = 128 * 1024;
	public NodeStamp stamp;
	public string metaData, relativePath;
	public byte[] data; //TODO: need to support multiple data streams
}

//---------------------------------------------------------------------------
/// <summary>
/// class to dish out Node information in pieces.
/// </summary>
public class SyncOutgoingNode
{
	Collection collection;
	Stream fs = null;

	public SyncOutgoingNode(Collection collection)
	{
		this.collection = collection;
	}

	public bool Start(NodeId nid, out NodeStamp stamp, out string metaData, out string relativePath)
	{
		if (fs != null)
		{
			fs.Close();
			fs = null;
		}
		Debug.Assert(collection != null && nid.s != null && collection.LocalStore != null);

		NodeStamp.Clear(out stamp);
		Node node = collection.GetNodeById(nid.s);
		Debug.Assert(collection != null && nid.s != null && collection.LocalStore != null);
		if (node == null)
		{
			Log.Spew("ignoring attempt to start outgoing sync for non-existent node {0}", nid.s);
			metaData = null;
			relativePath = null;
			return false;
		}

		metaData = collection.LocalStore.ExportSingleNodeToXml(collection, nid.s).OuterXml;
		Debug.Assert(metaData != null);

		//Node node = collection.GetNodeById(nid.s);
		stamp.id = nid;
		stamp.localIncarn = node.LocalIncarnation;
		stamp.masterIncarn = node.MasterIncarnation;
		stamp.name = node.Name;
		relativePath = null;
		foreach (NodeStream ns in node.GetStreamList())
		{
			relativePath = ns.RelativePath;
			fs = ns.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
			break; //TODO: handle multiple streams
		}
		return true;
	}

	public byte[] GetChunk(int MaxSize)
	{
		if (fs == null)
			return null;
		long remaining = fs.Length - fs.Position;
		int chunkSize = remaining < MaxSize? (int)remaining: MaxSize;
		byte[] buffer = new byte[chunkSize];
		int bytesRead = fs.Read(buffer, 0, buffer.Length);
		Debug.Assert(bytesRead == buffer.Length);
		if (chunkSize < MaxSize)
		{
			fs.Close();
			fs = null;
		}
		return buffer;
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// class to receive Node information in pieces and commit it when done.
/// Complete() must be called to complete the file or the partial
/// file will be deleted by the destructor.
/// </summary>
public class SyncIncomingNode
{
	string fileName = null;
	FileInfo fi = null;
	FileStream fs = null;
	Collection collection;
	NodeStamp stamp;
	bool onServer;

	public SyncIncomingNode(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
	}

	public void Start(NodeStamp stamp, string relativePath)
	{
		CleanUp();
		this.collection = collection;
		this.stamp = stamp;
		if (relativePath != null)
		{
			fileName = Path.Combine(collection.DocumentRoot.LocalPath, relativePath);
			string dirName = Path.GetDirectoryName(fileName);
			Directory.CreateDirectory(dirName);
			fi = new FileInfo(Path.Combine(dirName, ".simias." + stamp.id.s));
			fs = fi.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
		}
	}

	public void Append(byte[] data)
	{
		if (data != null && fs != null)
			fs.Write(data, 0, data.Length);
	}

	public bool Complete(string metaData)
	{
		XmlDocument doc = new XmlDocument();
		if (doc == null)
		{
			Log.Spew("No doc created by XmlDocument!?");
			return false;
		}
		if (metaData == null)
		{
			Log.Spew("skipping node, no metadata given");
			return false;
		}
		doc.LoadXml(metaData);
		Log.Spew("importing {0} {1} to collection {2}", stamp.name, stamp.id.s, collection.Name);
		//Log.Spew("       {0}", metaData);
		Node node = collection.LocalStore.ImportSingleNodeFromXml(collection, doc);
		Log.Assert(node.Id == stamp.id.s);
		if (onServer)
		{
			if (node.LocalIncarnation != stamp.masterIncarn)
			{
				Log.Spew("Rejecting update for node {0} due to collision on server", node.Name);
				CleanUp();
				return false;
			}
		}
		else if (node.LocalIncarnation != node.MasterIncarnation)
			Log.Spew("Collision: overwriting local node {0} with node from server", node.Name);

		if (fi == null)
			Log.Spew("updating metadata only for {0}", node.Name);
		else
		{
			fs.Close();
			Log.Spew("placing file {0}", fileName);
			fs = null;
			if (File.Exists(fileName))
			{
				Log.Error("File {0} exists, removing it to place incoming file", fileName);
				File.Delete(fileName);
			}
			fi.MoveTo(fileName);
			fi = null;
			foreach (NodeStream ns in node.GetStreamList())
			{
				File.SetLastWriteTime(fileName, ns.LastWriteTime);
				break; //TODO: handle multiple streams 
			}
			
		}
		//Log.Spew("  metadata:   {0}", metaData);

		/* TODO: If the node was a directory, we need to create it in
		 * the file system. Like the dredger looking for deleted files,
		 * we cannot rely on NodeStream.RelativePath to do it, so we
		 * call the dredger to make the name.
		 */
		if (node.Type == Dredger.NodeTypeDir)
		{
			string dirName = Dredger.FullPath(collection.DocumentRoot.LocalPath, node);
			if (File.Exists(dirName))
			{
				Log.Error("File {0} exists, removing it to place incoming file", dirName);
				File.Delete(dirName);
			}
			Log.Spew("creating directory {0}", dirName);
			Directory.CreateDirectory(dirName);
		}

		node.Commit();
		if (onServer)
			Log.Assert(node.LocalIncarnation == stamp.masterIncarn + 1);
		else
		{
			Log.Assert(stamp.localIncarn > node.MasterIncarnation);
			node.UpdateIncarnation(stamp.localIncarn);
			Log.Assert(node.LocalIncarnation == node.MasterIncarnation);
			Log.Assert(node.LocalIncarnation == stamp.localIncarn);
		}
		return true;
	}

	void CleanUp()
	{
		if (fs != null) fs.Close(); fs = null;
		if (fi != null) fi.Delete(); fi = null;
		fileName = null;
	}

	~SyncIncomingNode() { CleanUp(); }
}

//---------------------------------------------------------------------------
public class SyncOps
{
	Collection collection;
	bool onServer;

	public SyncOps(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
	}

	/// <summary>
	/// returns information about a set of nodes so that the sync code can detect collisions and which nodes are out of sync
	/// </summary>
	public NodeStamp[] GetNodeStamps()
	{
		ArrayList stampList = new ArrayList();

		foreach (Node node in collection)
		{
			bool tombstone = node.IsTombstone;
			if (onServer && tombstone)
				continue;
			NodeStamp stamp;
			stamp.localIncarn = tombstone? UInt64.MaxValue: node.LocalIncarnation;
			stamp.masterIncarn = node.MasterIncarnation;
			stamp.id.s = node.Id;
			stamp.streamsSize = 0;
			stamp.name = node.Name;
			foreach (NodeStream ns in node.GetStreamList())
				stamp.streamsSize += (ulong)ns.Length;
			stampList.Add(stamp);
		}

		NodeStamp[] stampArray = (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
		Array.Sort(stampArray);
		Log.Spew("Found {0} nodes in {1}", stampArray.Length, collection.Name);
		return stampArray;
	}

	/// <summary>
	/// deletes a list of nodes from a collection and removes the tombstones
	/// </summary>
	public void DeleteNodes(NodeId[] nodes)
	{
		foreach (NodeId nid in nodes)
		{
			Node node = collection.GetNodeById(nid.s);
			if (node == null)
			{
				Log.Spew("ignoring attempt to delete non-existent node {0}", nid.s);
				continue;
			}
			foreach(NodeStream ns in node.GetStreamList())
				ns.Delete(true);

			/* TODO: If the node was a directory, we need to delete it from
			 * the file system. Like the dredger looking for deleted files,
			 * we cannot rely on NodeStream.RelativePath to do it, so we
			 * call the dredger to make the name.
			 */
			if (node.Type == Dredger.NodeTypeDir)
			{
				string dirName = Dredger.FullPath(collection.DocumentRoot.LocalPath, node);
				Log.Spew("deleting directory {0} and all contents and subdirectories", dirName);
				Directory.Delete(dirName, true);
			}
			node.Delete(true);
			node.Delete(); // delete node and tombstone
		}
	}

	/// <summary>
	/// returns array of nodes with stamp, metadata and file contents
	/// </summary>
	public SmallNode[] GetSmallNodes(NodeId[] nids)
	{
		uint i = 0;
		SyncOutgoingNode outNode = new SyncOutgoingNode(collection);
		SmallNode[] nodes = new SmallNode[nids.Length];
		foreach (NodeId nid in nids)
		{
			if (outNode.Start(nid, out nodes[i].stamp, out nodes[i].metaData, out nodes[i].relativePath))
			{
				nodes[i].data = outNode.GetChunk(SmallNode.MaxSize);
				if (nodes[i].data == null)
					Debug.Assert(nodes[i].stamp.streamsSize == 0);
				else if (nodes[i].data.Length >= SmallNode.MaxSize)
				{   // the file grew larger than a SmallNode should handle, tell client to retry
					nodes[i].stamp.streamsSize = (ulong)nodes[i].data.Length;
					nodes[i].data = null;
				}
			}
			++i;
		}
		return nodes;
	}

	/// <summary>
	/// returns array of NodeIds that were not updated due to collisions
	/// </summary>
	public NodeId[] PutSmallNodes(SmallNode[] nodes)
	{
		SyncIncomingNode inNode = new SyncIncomingNode(collection, onServer);
		ArrayList rejects = new ArrayList();
		foreach (SmallNode sn in nodes)
		{
			if (!onServer && sn.data == null && sn.stamp.streamsSize >= SmallNode.MaxSize)
			{
				Log.Spew("skipping update of node {0} because server requests retry", sn.stamp.name);
				continue;
			}

			inNode.Start(sn.stamp, sn.relativePath);
			inNode.Append(sn.data);
			if (!inNode.Complete(sn.metaData))
				rejects.Add(sn.stamp.id);
		}
		return (NodeId[])rejects.ToArray(typeof(NodeId));
	}

	/// <summary>
	/// only called on the client after the node has been updated on the
	/// server. Just sets the MasterIncarnation
	/// </summary>
	public void UpdateIncarn(NodeStamp stamp)
	{
		Debug.Assert(!onServer);

		Node node = collection.GetNodeById(stamp.id.s);
		Debug.Assert(node.MasterIncarnation == stamp.masterIncarn);
		if (stamp.localIncarn != node.LocalIncarnation)
			Log.Spew("Local node {0} has been updated again, skipping update of master incarnation number", node.Name);
		else
			node.UpdateIncarnation(stamp.masterIncarn + 1);
	}
}

//===========================================================================
}
