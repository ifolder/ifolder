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
using System.Xml;
using System.Diagnostics;

using Simias.Storage;
using Simias;

namespace Simias.Sync
{

/// <summary>
/// valid states of a node update attempt
/// TODO: requiring a comment on every enum member is counter-productive. How to fix?
/// </summary>
[Serializable]
public enum NodeStatus
{
	/// <summary> node update was successful </summary>
	Complete,

	/// <summary> node update was aborted due to update from other client </summary>
	UpdateCollision,

	/// <summary> node update was aborted due to other node referencing same file </summary>
	FileSystemEntryCollision,

	/// <summary> node update was probably unsuccessful, unhandled exception on the server </summary>
	ServerFailure,

	/// <summary> node update is in progress </summary>
	InProgess
};

//---------------------------------------------------------------------------
/// <summary>
/// struct to encapsulate a GUID string, provides a case-insensitive compare,
/// ensures proper format and characters, etc.
/// </summary>
[Serializable]
public struct Nid
{
	private string g;

	/// <summary>
	/// runs string through Guid constructor to throw exception if bad format
	/// </summary>
	public Nid(string s)
	{
		try { g = new Guid(s).ToString(); }
		catch (FormatException) { Log.Spew("'{0}' is not a valid guid", s); throw; }
	}

	/// <summary> implement some convenient operator overloads </summary>
	public static explicit operator Nid(string s) { return new Nid(s); }

	/// <summary> implement some convenient operator overloads </summary>
	public override bool Equals(object o) { return CompareTo(o) == 0; }

	/// <summary> implement some convenient operator overloads </summary>
	public static bool operator==(Nid a, Nid b) { return a.Equals(b); }

	/// <summary> implement some convenient operator overloads </summary>
	public static bool operator!=(Nid a, Nid b) { return !a.Equals(b); }

	/// <summary> implement some convenient operator overloads </summary>
	public override string ToString() { return g; }

	/// <summary> implement some convenient operator overloads </summary>
	public override int GetHashCode() { return g.GetHashCode(); }

	internal bool Valid() { return Valid(g); }
	internal void Validate() { Log.Assert(Valid(g)); }
	static internal void Validate(string g) { Log.Assert(Valid(g)); }

	static internal bool Valid(string g)
	{
		try { return String.Compare(new Nid(g).ToString(), g, true) == 0; }
		catch (FormatException) { return false; }
	}

	internal int CompareTo(object obj)
	{
		if (!(obj is Nid))
			throw new ArgumentException("object is not Nid");
		return String.Compare(((Nid)obj).g, g, true);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// struct to represent the minimal information that the sync code would need
/// to know about a node to determine if it needs to be synced.
/// </summary>
[Serializable]
public struct NodeStamp: IComparable
{
	internal Nid id;

	// if localIncarn == UInt64.MaxValue, node is a tombstone
	internal ulong localIncarn, masterIncarn;

	// total size of all streams
	internal ulong streamsSize;

	internal string name; //just for debug

	/// <summary> implement some convenient operator overloads </summary>
	public int CompareTo(object obj)
	{
		if (obj == null)
			Log.Spew("obj is null");
		if (!(obj is NodeStamp))
			throw new ArgumentException("object is not NodeStamp");
		return id.CompareTo(((NodeStamp)obj).id);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// a chunk of data that is part of a node data stream property
/// </summary>
[Serializable]
public class FseChunk
{
	internal string relativePath = null;
	internal byte[] data = null; // if null, IsDirectory
}

//---------------------------------------------------------------------------
/// <summary>
/// a chunk of data about a particular incarnation of a node
/// </summary>
[Serializable]
public struct NodeChunk
{
	internal const int MaxSize = 128 * 1024;
	internal NodeStamp stamp;
	internal string metaData;
	internal int totalSize;
	internal FseChunk[] fseChunks;
}


//---------------------------------------------------------------------------
/// <summary>
/// node collision information
/// </summary>
[Serializable]
public struct RejectedNode
{
	internal Nid nid;
	internal NodeStatus status;
	internal RejectedNode(Nid nid, NodeStatus status)
	{
		this.nid = nid;
		this.status = status;
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// class to dish out Node information in pieces.
/// </summary>
internal class SyncOutgoingNode
{
	Collection collection;

	class fseOut { public string relativePath = null; public Stream stream = null; }
	ArrayList fseList = null;

	public SyncOutgoingNode(Collection collection)
	{
		this.collection = collection;
	}

	public bool Start(Nid nid, out NodeStamp stamp, out string metaData)
	{
		nid.Validate();
		fseList = new ArrayList();
		stamp = new NodeStamp();
		metaData = null;
		Node node = collection.GetNodeById(nid.ToString());
		if (node == null)
		{
			Log.Spew("ignoring attempt to start outgoing sync for non-existent node {0}", nid);
			return false;
		}

		metaData = collection.LocalStore.ExportSingleNodeToXml(collection, nid.ToString()).OuterXml;
		stamp.id = nid;
		stamp.localIncarn = node.LocalIncarnation;
		stamp.masterIncarn = node.MasterIncarnation;
		stamp.name = node.Name;
		foreach (FileSystemEntry fse in node.GetFileSystemEntryList())
		{
			fseOut fseo = new fseOut();
			fseo.relativePath = fse.RelativePath;
			fseo.stream = fse.IsFile? ((FileEntry)fse).Open(FileMode.Open, FileAccess.Read, FileShare.Read): null;
			fseList.Add(fseo);
		}
		return true;
	}

	public FseChunk[] ReadChunks(int maxSize, out int totalSize)
	{
		if (maxSize < 4096)
			maxSize = 4096;
		ArrayList chunks = new ArrayList();
		totalSize = 0;
		foreach (fseOut fseo in fseList)
		{
			if (fseo.relativePath == null)
				continue;

			FseChunk chunk = new FseChunk();
			chunk.relativePath = fseo.relativePath;
			if (fseo.stream == null)
				fseo.relativePath = null;
			else
			{
				long remaining = fseo.stream.Length - fseo.stream.Position;
				int chunkSize = remaining < maxSize? (int)remaining: maxSize;
				byte[] buffer = new byte[chunkSize];
				int bytesRead = fseo.stream.Read(buffer, 0, buffer.Length);
				Log.Assert(bytesRead == buffer.Length);
				if (chunkSize < maxSize)
				{
					fseo.stream.Close();
					fseo.stream = null;
					fseo.relativePath = null;
				}
				chunk.data = buffer;
				totalSize += chunkSize;
			}
			chunks.Add(chunk);
			if (totalSize >= maxSize)
				break;
		}
		return (FseChunk[])chunks.ToArray(typeof(FseChunk));
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// class to receive Node information in pieces and commit it when done.
/// Complete() must be called to complete the file or the partial
/// file will be deleted by the destructor.
/// </summary>
internal class SyncIncomingNode
{
	Collection collection;
	bool onServer;

	int streamCount = 0;
	NodeStamp stamp;

	class FseIn { public string name = null, relName = null; public Stream stream = null; public FileInfo info = null; }
	ArrayList fseList = null;

	public SyncIncomingNode(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
	}

	void CleanUp()
	{
		if (fseList != null)
			foreach (FseIn fsei in fseList)
			{
				if (fsei.stream != null)
					fsei.stream.Close();
				if (fsei.info != null)
					fsei.info.Delete();
			}
		fseList = null;
	}

	~SyncIncomingNode() { CleanUp(); }

	public void Start(NodeStamp stamp)
	{
		CleanUp();
		this.stamp = stamp;
		streamCount = 0;
		fseList = new ArrayList();
	}

	public void WriteChunks(FseChunk[] chunks)
	{
		foreach (FseChunk chunk in chunks)
		{
			string name = Path.Combine(collection.DocumentRoot.LocalPath, chunk.relativePath);
			bool done = false;

			foreach (FseIn fsei in fseList)
				if (name == fsei.name)
				{
					if (fsei.stream != null)
						fsei.stream.Write(chunk.data, 0, chunk.data.Length);
					else
						Log.Assert(fsei.stream != null);
					done = true;
					break;
				}
			if (!done)
			{
				FseIn fsei = new FseIn();
				fsei.name = name;
				fsei.relName = chunk.relativePath;
				if (chunk.data != null)
				{
					string dirName = Path.GetDirectoryName(name);
					if (dirName.Length > 1)
						Directory.CreateDirectory(dirName);
					fsei.info = new FileInfo(Path.Combine(dirName, String.Format(".simias.{0}.{1}", stamp.id, ++streamCount)));
					fsei.stream = fsei.info.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
					fsei.stream.Write(chunk.data, 0, chunk.data.Length);
				}
				fseList.Add(fsei);
			}
		}
	}


	public NodeStatus Complete(string metaData)
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
				Log.Spew("Rejecting update for node {0} due to update collision on server", node.Name);
				CleanUp();
				return NodeStatus.UpdateCollision;
			}
		}
		else if (node.LocalIncarnation != node.MasterIncarnation)
			Log.Spew("Collision: overwriting local node {0} with node from server", node.Name);

		foreach (FseIn fsei in fseList)
			foreach (Node n in collection.LocalStore.GetNodesAssociatedWithPath(collection.Id, fsei.relName))
				if ((Nid)n.Id != (Nid)node.Id)
				{
					Log.Spew("Rejecting update for node {0} due to FileSystemEntry collision on server", node.Name);
					CleanUp();
					return NodeStatus.FileSystemEntryCollision;
				}

		foreach (FseIn fsei in fseList)
		{
			if (fsei.stream == null)
			{
				Log.Spew("creating directory {0}", fsei.name);
				Directory.CreateDirectory(fsei.name);
			}
			else
			{
				fsei.stream.Close();
				fsei.stream = null;
				Log.Spew("placing file {0}", fsei.name);
				File.Delete(fsei.name);
				fsei.info.MoveTo(fsei.name);
				fsei.info = null;
			}
		}

		foreach (FileSystemEntry fse in node.GetFileSystemEntryList())
		{
			if (fse.IsFile)
			{
				File.SetLastWriteTime(fse.FullName, fse.LastWriteTime);
				File.SetCreationTime(fse.FullName, fse.CreationTime);
				//File.SetLastAccessTime(fse.FullName, fse.LastAccessTime);
			}
		}

		Log.Assert(stamp.localIncarn > node.MasterIncarnation);
		return node.UpdateIncarnation(stamp.localIncarn)? NodeStatus.Complete: NodeStatus.UpdateCollision;
	}
}

//---------------------------------------------------------------------------
internal class SyncOps
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
			foreach (FileSystemEntry fse in node.GetFileSystemEntryList())
				if (fse.IsFile)
					stamp.streamsSize += (ulong)((FileEntry)fse).Length;
			stampList.Add(stamp);
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
				Log.Spew("ignoring attempt to delete non-existent node {0}", nid);
			else
			{
				foreach(FileSystemEntry fse in node.GetFileSystemEntryList())
					fse.Delete(true);
				node.Delete(true).Delete(); // delete node and tombstone
			}
		}
	}

	/// <summary>
	/// deletes a node that is colliding with another node due to a FileSystemEntry collision.
	/// </summary>
	public void DeleteSpuriousNode(Nid nid)
	{
		Node node = collection.GetNodeById(nid.ToString());
		if (node == null)
			Log.Spew("ignoring attempt to delete non-existent spurious node {0}", nid);
		else
		{
			Log.Spew("deleting spurious node {0}", node.Name);
			node.Delete(true).Delete(); // delete node and tombstone
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
			if (outNode.Start(nid, out nodes[i].stamp, out nodes[i].metaData))
			{
				nodes[i].fseChunks = outNode.ReadChunks(NodeChunk.MaxSize, out nodes[i].totalSize);
				if (nodes[i].fseChunks == null)
					Log.Assert(nodes[i].stamp.streamsSize == 0);
				else if (nodes[i].totalSize >= NodeChunk.MaxSize)
				{
					/* the file grew larger than a SmallNode should handle,
					 * tell client to add this to large node list
					 */
					nodes[i].fseChunks = null;
				}
			}
			++i;
		}
		return nodes;
	}

	/// <summary>
	/// returns nodes were not updated due to collisions
	/// </summary>
    public RejectedNode[] PutSmallNodes(NodeChunk[] nodeChunks)
	{
		SyncIncomingNode inNode = new SyncIncomingNode(collection, onServer);
		ArrayList rejects = new ArrayList();
		Log.Spew("PutSmallNodes() {0}", nodeChunks.Length);
		foreach (NodeChunk nc in nodeChunks)
		{
			if (!onServer && nc.fseChunks == null && nc.totalSize >= NodeChunk.MaxSize)
			{
				Log.Spew("skipping update of node {0} because server requests retry", nc.stamp.name);
				continue;
			}
			inNode.Start(nc.stamp);
			inNode.WriteChunks(nc.fseChunks);
			NodeStatus status = inNode.Complete(nc.metaData);
			if (status != NodeStatus.Complete)
				rejects.Add(new RejectedNode(nc.stamp.id, status));
		}
		return (RejectedNode[])rejects.ToArray(typeof(RejectedNode));
	}

	/// <summary>
	/// only called on the client after the node has been updated on the
	/// server. Just sets the MasterIncarnation
	/// </summary>
	public void UpdateIncarn(NodeStamp stamp)
	{
		Log.Assert(!onServer);
		for (bool done = false; !done;)
		{
			Node node = collection.GetNodeById(stamp.id.ToString());
			Log.Assert(node.MasterIncarnation == stamp.masterIncarn);
			bool wasUpdatedAgain = stamp.localIncarn != node.LocalIncarnation;
			if (!(done = node.UpdateIncarnation(stamp.localIncarn)))
				node.Rollback();
			else if (wasUpdatedAgain)
				node.Commit(); // increment LocalIncarnation
		}
	}
}

//===========================================================================
}
