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
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
[Serializable]
public struct Nid
{
	private string g;

	// run string through Guid constructor to throw exception if bad format
	public Nid(string s)
	{
		try { g = new Guid(s).ToString(); }
		catch (FormatException) { Log.Spew("'{0}' is not a valid guid", s); throw; }
	}

	public static explicit operator Nid(string s) { return new Nid(s); }
	public override bool Equals(object o) { return CompareTo(o) == 0; }
	public static bool operator==(Nid a, Nid b) { return a.Equals(b); }
	public static bool operator!=(Nid a, Nid b) { return !a.Equals(b); }
	public override string ToString() { return g; }
	public override int GetHashCode() { return g.GetHashCode(); }
	public bool Valid() { return Valid(g); }
	public void Validate() { Log.Assert(Valid(g)); }
	static public void Validate(string g) { Log.Assert(Valid(g)); }

	static public bool Valid(string g)
	{
		try { return String.Compare(new Nid(g).ToString(), g, true) == 0; }
		catch (FormatException) { return false; }
	}

	public int CompareTo(object obj)
	{
		if (!(obj is Nid))
			throw new ArgumentException("object is not Nid");
		return String.Compare(((Nid)obj).g, g, true);
	}
}

[Serializable]
public struct NodeStamp: IComparable
{
	public Nid id;

	// if localIncarn == UInt64.MaxValue, node is a tombstone
	public ulong localIncarn, masterIncarn;

	// total size of all streams
	public ulong streamsSize;

	public string name; //just for debug

	public int CompareTo(object obj)
	{
		if (obj == null)
			Log.Spew("obj is null");
		if (!(obj is NodeStamp))
			throw new ArgumentException("object is not NodeStamp");
		return id.CompareTo(((NodeStamp)obj).id);
	}
}

[Serializable]
public struct NodeChunk
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

	public bool Start(Nid nid, out NodeStamp stamp, out string metaData, out string relativePath)
	{
		nid.Validate();
		if (fs != null)
		{
			fs.Close();
			fs = null;
		}
		Log.Assert(collection != null && collection.LocalStore != null);
		stamp = new NodeStamp();
		Node node = collection.GetNodeById(nid.ToString());
		Log.Assert(collection != null && nid.Valid() && collection.LocalStore != null);
		if (node == null)
		{
			Log.Spew("ignoring attempt to start outgoing sync for non-existent node {0}", nid);
			metaData = null;
			relativePath = null;
			return false;
		}

		metaData = collection.LocalStore.ExportSingleNodeToXml(collection, nid.ToString()).OuterXml;
		Log.Assert(metaData != null);

		stamp.id = nid;
		stamp.localIncarn = node.LocalIncarnation;
		stamp.masterIncarn = node.MasterIncarnation;
		stamp.name = node.Name;
		relativePath = null;
		foreach (FileSystemEntry fse in node.GetFileSystemEntryList())
		{
			relativePath = fse.RelativePath;
			if (fse.IsFile)
				fs = ((FileEntry)fse).Open(FileMode.Open, FileAccess.Read, FileShare.Read);
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
		Log.Assert(bytesRead == buffer.Length);
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
			fi = new FileInfo(Path.Combine(dirName, ".simias." + stamp.id));
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
		doc.LoadXml(metaData);
		Log.Spew("importing {0} {1} to collection {2}", stamp.name, stamp.id, collection.Name);
		Node node = collection.LocalStore.ImportSingleNodeFromXml(collection, doc);
		Log.Assert(node.Id == stamp.id.ToString());
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
		{
			//Log.Spew("updating metadata only for {0}", node.Name);
		}
		else
		{
			fs.Close();
			//Log.Spew("placing file {0}", fileName);
			fs = null;
			if (File.Exists(fileName))
			{
				Log.Spew("File {0} exists, rejecting incoming node", fileName);
				fi.Delete();
				return false;
			}
			fi.MoveTo(fileName);
			fi = null;
			foreach (FileSystemEntry fse in node.GetFileSystemEntryList())
			{
				File.SetLastWriteTime(fileName, fse.LastWriteTime);
				break; //TODO: handle multiple streams 
			}
			
		}
		
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
			NodeStamp stamp = new NodeStamp();
			stamp.localIncarn = tombstone? UInt64.MaxValue: node.LocalIncarnation;
			stamp.masterIncarn = node.MasterIncarnation;
			stamp.id = new Nid(node.Id);
			stamp.streamsSize = 0;
			stamp.name = node.Name;
			foreach (FileEntry fe in node.GetFileSystemEntryList())
			{
				if (fe != null)
					stamp.streamsSize += (ulong)fe.Length;
			}
			stampList.Add(stamp);
			//Log.Spew("listing stamp for {0} Nid '{1}'", stamp.name, stamp.id.g);
		}

		stampList.Sort();
		Log.Spew("Found {0} nodes in {1}", stampList.Count, collection.Name);
		return (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
	}

	/// <summary>
	/// deletes a list of nodes from a collection and removes the tombstones
	/// </summary>
	public void DeleteNodes(Nid[] nodes)
	{
		foreach (Nid nid in nodes)
		{
			Node node = collection.GetNodeById(nid.ToString());
			if (node == null)
			{
				Log.Spew("ignoring attempt to delete non-existent node {0}", nid);
				continue;
			}
			foreach(FileSystemEntry fse in node.GetFileSystemEntryList())
				fse.Delete(true);

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
	public NodeChunk[] GetSmallNodes(Nid[] nids)
	{
		uint i = 0;
		SyncOutgoingNode outNode = new SyncOutgoingNode(collection);
		NodeChunk[] nodes = new NodeChunk[nids.Length];
		foreach (Nid nid in nids)
		{
			if (outNode.Start(nid, out nodes[i].stamp, out nodes[i].metaData, out nodes[i].relativePath))
			{
				nodes[i].data = outNode.GetChunk(NodeChunk.MaxSize);
				if (nodes[i].data == null)
					Log.Assert(nodes[i].stamp.streamsSize == 0);
				else if (nodes[i].data.Length >= NodeChunk.MaxSize)
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
	/// returns array of Nids that were not updated due to collisions
	/// </summary>
	public Nid[] PutSmallNodes(NodeChunk[] nodes)
	{
		SyncIncomingNode inNode = new SyncIncomingNode(collection, onServer);
		ArrayList rejects = new ArrayList();
		foreach (NodeChunk sn in nodes)
		{
			if (!onServer && sn.data == null && sn.stamp.streamsSize >= NodeChunk.MaxSize)
			{
				Log.Spew("skipping update of node {0} because server requests retry", sn.stamp.name);
				continue;
			}

			inNode.Start(sn.stamp, sn.relativePath);
			inNode.Append(sn.data);
			if (!inNode.Complete(sn.metaData))
				rejects.Add(sn.stamp.id);
		}
		return (Nid[])rejects.ToArray(typeof(Nid));
	}

	/// <summary>
	/// only called on the client after the node has been updated on the
	/// server. Just sets the MasterIncarnation
	/// </summary>
	public void UpdateIncarn(NodeStamp stamp)
	{
		Log.Assert(!onServer);

		Node node = collection.GetNodeById(stamp.id.ToString());
		Log.Assert(node.MasterIncarnation == stamp.masterIncarn);
		if (stamp.localIncarn != node.LocalIncarnation)
			Log.Spew("Local node {0} has been updated again, skipping update of master incarnation number", node.Name);
		else
			node.UpdateIncarnation(stamp.masterIncarn + 1);
	}
}

//===========================================================================
}
