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

//---------------------------------------------------------------------------
/// <summary>
/// valid states of a node update attempt
/// TODO: requiring a comment on every enum member is counter-productive. How to fix?
///       Do we even still need this enum?
/// </summary>
[Serializable]
public enum NodeStatus
{
	/// <summary> node update was successful </summary>
	Complete,

	/// <summary> node update was aborted due to update from other client </summary>
	UpdateCollision,

	/// <summary> node update was aborted due to other node referencing same file </summary>
	//FileSystemEntryCollision,

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
	public static implicit operator string(Nid n) { return n.g; }

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
	internal long streamsSize;

	internal string name; //just for debug

	/// <summary> implement some convenient operator overloads </summary>
	public int CompareTo(object obj)
	{
		if (obj == null || !(obj is NodeStamp))
			throw new ArgumentException("object is not NodeStamp");
		return id.CompareTo(((NodeStamp)obj).id);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// a chunk of data from a particular data stream (fork) of a node
/// </summary>
[Serializable]
public struct ForkChunk
{
	internal const string DataForkName = "data";
	internal string name;
	internal byte[] data;
}

//---------------------------------------------------------------------------
/// <summary>
/// a chunk of data about a particular incarnation of a node
/// </summary>
[Serializable]
public struct NodeChunk
{
	internal const int MaxSize = 128 * 1024;
	internal Node node;
	internal ulong expectedIncarn;
	internal int totalSize;
	internal ForkChunk[] forkChunks;
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
	//internal string[] forkNameList;
	Collection collection;
	class Fork { public string name; public Stream stream; };
	ArrayList forkList;

	public SyncOutgoingNode(Collection collection)
	{
		this.collection = collection;
	}

	public Node Start(Nid nid)
	{
		nid.Validate();
		Node node = collection.GetNodeByID(nid);
		forkList = null;
		if (node == null)
		{
			Log.Spew("ignoring attempt to start outgoing sync for non-existent node {0}", nid);
			return null;
		}

		BaseFileNode bfn = node as BaseFileNode;
		if (bfn != null)
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
				fork.stream = File.Open(bfn.GetFullPath(collection), FileMode.Open, FileAccess.Read, FileShare.Read);
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
		return (ForkChunk[])chunks.ToArray(typeof(ForkChunk));
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
	Node node;
	FileInfo fileInfo;
	class Fork { public string name; public Stream stream; };
	ArrayList forkList = null;
	string parentPath = null;

	public SyncIncomingNode(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
	}

	void CleanUp()
	{
		if (forkList != null)
			foreach (Fork fork in forkList)
				if (fork.stream != null)
					fork.stream.Close();
		forkList = null;
		if (fileInfo != null)
			fileInfo.Delete();
		fileInfo = null;
		parentPath = null;
	}

	~SyncIncomingNode() { CleanUp(); }

	public void Start(Node node)
	{
		CleanUp();
		this.node = node;
		if (collection.IsType(node, typeof(DirNode).Name) && !((DirNode)node).IsRoot
				|| collection.IsType(node, typeof(FileNode).Name))
		{
			Property p = node.Properties.GetSingleProperty(PropertyTags.Parent);
			Relationship rship = p == null? null: (p.Value as Relationship);
			if (rship == null)
				throw new ApplicationException("file or dir node has no parent");
			DirNode pn = collection.GetNodeByID(rship.NodeID) as DirNode;
			if (pn == null)
				throw new ApplicationException("parent is not DirNode");
			parentPath = pn.GetFullPath(collection);
		}
		else
			parentPath = collection.ManagedPath;
	}

	public void BlowChunks(ForkChunk[] chunks)
	{
		foreach (ForkChunk chunk in chunks)
		{
			bool done = false;

			if (forkList == null)
				forkList = new ArrayList();
			else
				foreach (Fork fork in forkList)
					if (chunk.name == fork.name)
					{
						fork.stream.Write(chunk.data, 0, chunk.data.Length);
						done = true;
						break;
					}

			if (!done)
			{
				/* TODO: handle multiple forks (streams), EAs, etc. For right now
				 * this is just a guess at how to do it, stubbed to support
				 * SyncOutgoingNode.
				 */
				Log.Assert(chunk.name == ForkChunk.DataForkName);

				if (fileInfo == null)
					fileInfo = new FileInfo(Path.Combine(parentPath, String.Format(".simias.{0}", node.ID)));
				Fork fork = new Fork();
				fork.name = chunk.name;
				fork.stream = fileInfo.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
				fork.stream.Write(chunk.data, 0, chunk.data.Length);
				forkList.Add(fork);
			}
		}
	}

	public NodeStatus Complete(ulong expectedIncarn)
	{
		Log.Spew("importing {0} {1} to collection {2}", node.Name, node.ID, collection.Name);

		/* TODO: this is a problem because we cannot lock curNode until commit
		 *       could ImportNode do this for us?
		 */
		Node curNode = collection.GetNodeByID(node.ID);
		if (curNode != null && curNode.LocalIncarnation != expectedIncarn)
		{
			if (onServer)
			{
				Log.Spew("Rejecting update for node {0} due to update collision on server", node.Name);
				CleanUp();
				return NodeStatus.UpdateCollision;
			}
			//TODO: handle collision here
			Log.Spew("Collision: overwriting local node {0} with node from server", node.Name);
		}

		//TODO: check here for whether the proposed file name is available, i.e. make current and proposed LocalFileName
		string path = Path.Combine(parentPath, node.Name);
		if (collection.IsType(node, typeof(DirNode).Name))
			Directory.CreateDirectory(path);
		else if (!collection.IsType(node, typeof(BaseFileNode).Name))
			Log.Assert(forkList == null);
		else
		{
			foreach (Fork fork in forkList)
			{
				fork.stream.Close();
				fork.stream = null;
			}
			Log.Spew("placing file {0}", path);
			File.Delete(path); //TODO: delete current LocalFileName
			fileInfo.MoveTo(path); //TODO: moved to proposed LocalFileName
			FileNode fnode = (FileNode)node;
			fileInfo.LastWriteTime = fnode.LastWriteTime;
			fileInfo.CreationTime = fnode.CreationTime;
			fileInfo = null;
		}

		/* TODO: here is where we should commit or abort,
		 *     returning NodeStatus.Complete or NodeStatus.UpdateCollision
		 */
		collection.ImportNode(node);
		node.IncarnationUpdate = node.LocalIncarnation;
		collection.Commit(node);
		return NodeStatus.Complete;
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
			bool tombstone = collection.IsTombstone(node);
			if (onServer && tombstone)
				continue;
			NodeStamp stamp = new NodeStamp();
			stamp.localIncarn = tombstone? UInt64.MaxValue: node.LocalIncarnation;
			stamp.masterIncarn = node.MasterIncarnation;
			stamp.id = new Nid(node.ID);
			stamp.streamsSize = 0;
			stamp.name = node.Name;
			BaseFileNode bfn = node as BaseFileNode;

			//TODO: another place to handle multiple forks
			if (bfn != null)
			{
				FileInfo fi = new FileInfo(bfn.GetFullPath(collection));
				stamp.streamsSize += fi.Length;
			}
			
			stampList.Add(stamp);
		}

		stampList.Sort();
		Log.Spew("Found {0} nodes in {1}", stampList.Count, collection.Name);
		return (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
	}


	void DeleteNode(Node node)
	{
		Node[] deleted = collection.Delete(node, PropertyTags.Parent);
		collection.Commit(deleted);
		if (onServer)
		{
			foreach (Node del in deleted)
				collection.Delete(del); // don't leave a tombstone on the server
			collection.Commit(deleted);
		}
	}

	/// <summary>
	/// deletes a list of nodes from a collection and removes the tombstones
	/// </summary>
	public void DeleteNodes(Nid[] nids)
	{
		foreach (Nid nid in nids)
		{
			Node node = collection.GetNodeByID(nid);
			if (node == null)
				Log.Spew("ignoring attempt to delete non-existent node {0}", nid);
			else
			{
				//TODO: another place to handle multiple forks
				BaseFileNode bfn = node as BaseFileNode;
				if (bfn != null)
					File.Delete(bfn.GetFullPath(collection));

				DeleteNode(node);
			}
		}
	}

	/// <summary>
	/// deletes a node that is colliding with another node due to a FileSystemEntry collision.
	/// </summary>
	public void DeleteSpuriousNode(Nid nid)
	{
		Node node = collection.GetNodeByID(nid);
		if (node == null)
			Log.Spew("ignoring attempt to delete non-existent spurious node {0}", nid);
		else
		{
			Log.Spew("deleting spurious node {0}", node.Name);
			DeleteNode(node);
		}
	}

	/// <summary>
	/// returns array of nodes with stamp, metadata and file contents
	/// </summary>
	public NodeChunk[] GetSmallNodes(Nid[] nids)
	{
		SyncOutgoingNode ogn = new SyncOutgoingNode(collection);
		NodeChunk[] chunks = new NodeChunk[nids.Length];
		uint i = 0;
		foreach (Nid nid in nids)
		{
			if ((chunks[i].node = ogn.Start(nid)) != null)
			{
				chunks[i].forkChunks = ogn.ReadChunks(NodeChunk.MaxSize, out chunks[i].totalSize);
				if (chunks[i].forkChunks == null)
					Log.Assert(chunks[i].totalSize == 0);
				else if (chunks[i].totalSize >= NodeChunk.MaxSize)
				{
					/* the file grew larger than a SmallNode should handle,
					 * tell client to add this to large node list
					 */
					chunks[i].forkChunks = null;
				}
			}
			++i;
		}
		return chunks;
	}

	/// <summary>
	/// returns nodes were not updated
	/// </summary>
    public RejectedNode[] PutSmallNodes(NodeChunk[] nodeChunks)
	{
		SyncIncomingNode inNode = new SyncIncomingNode(collection, onServer);
		ArrayList rejects = new ArrayList();
		Log.Spew("PutSmallNodes() {0}", nodeChunks.Length);
		foreach (NodeChunk nc in nodeChunks)
		{
			if (nc.forkChunks == null && nc.totalSize >= NodeChunk.MaxSize)
			{
				Log.Spew("skipping update of node {0}, retry next sync", nc.node.Name);
				continue;
			}
			inNode.Start(nc.node);
			inNode.BlowChunks(nc.forkChunks);
			NodeStatus status = inNode.Complete(nc.expectedIncarn);
			if (status != NodeStatus.Complete)
				rejects.Add(new RejectedNode((Nid)nc.node.ID, status));
		}
		return (RejectedNode[])rejects.ToArray(typeof(RejectedNode));
	}

	/// <summary>
	/// only called on the client after the node has been updated on the
	/// server. Just sets the MasterIncarnation to the LocalIncarnation
	/// </summary>
	public void UpdateIncarn(Nid nid)
	{
		Log.Assert(!onServer);
		for (bool done = false; !done;)
		{
			Node node = collection.GetNodeByID(nid);

			//TODO: review this

			//NOTE: caller already has node, why not use it?
			
			//Log.Assert(node.MasterIncarnation == stamp.masterIncarn);
			//bool wasUpdatedAgain = stamp.localIncarn != node.LocalIncarnation;
			//if (!(done = node.UpdateIncarnation(stamp.localIncarn)))
			//	node.Rollback();
			//else if (wasUpdatedAgain)
			//	node.Commit(); // increment LocalIncarnation

			node.IncarnationUpdate = node.LocalIncarnation;
			collection.Commit(node);
		}
	}
}

//===========================================================================
}
