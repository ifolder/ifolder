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

/* delta sync thought: it would be easy to store the overall md5 of the file
 * as a property for other uses, perhaps even some security use.
 */
 

namespace Simias.Sync
{

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

	// total size of all streams, expected to be used for sync progress meter
	// if -1, this node is not derived from BaseFileNode.
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
// TODO: should this just be a nested class in ForkChunk
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
	internal string relativePath;
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
// various useful Sync operations that are called by client and server
internal class SyncOps
{
	Collection collection;
	bool onServer;

	//TODO: remove after verifying that 'node as BaseFileNode' works for this
	internal static BaseFileNode CastToBaseFileNode(Collection collection, Node node)
	{
		if (collection.IsType(node, typeof(FileNode).Name))
			return new FileNode(node);
		if (collection.IsType(node, typeof(StoreFileNode).Name))
			return new StoreFileNode(node);
		return null;
	}

	//TODO: remove after verifying that 'node as DirNode' works for this
	internal static DirNode CastToDirNode(Collection collection, Node node)
	{
		if (collection.IsType(node, typeof(DirNode).Name))
			return new DirNode(node);
		return null;
	}

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

		foreach (ShallowNode sn in collection)
		{
			Node node = new Node(collection, sn);
			bool tombstone = collection.IsType(node, NodeTypes.TombstoneType);
			if (onServer && tombstone)
				continue;
			NodeStamp stamp = new NodeStamp();
			stamp.localIncarn = tombstone? UInt64.MaxValue: node.LocalIncarnation;

			//DEBUG start
			if (node.Properties.GetSingleProperty( PropertyTags.MasterIncarnation ) == null)
			{
				Log.Spew("Node {0} has no MasterIncarnation", node.Name);
				stamp.masterIncarn = node.LocalIncarnation; //just a guess
			}
			else
			//DEBUG end
				stamp.masterIncarn = node.MasterIncarnation;

			stamp.id = new Nid(node.ID);
			stamp.name = node.Name;

			//TODO: another place to handle multiple forks
			BaseFileNode bfn = CastToBaseFileNode(collection, node);
			if (bfn == null)
				stamp.streamsSize = -1;
			else
				stamp.streamsSize = new FileInfo(bfn.GetFullPath(collection)).Length;
			
			stampList.Add(stamp);
		}

		stampList.Sort();
		Log.Spew("Found {0} nodes in {1}", stampList.Count, collection.Name);
		return (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
	}

	/// <summary>
	/// deletes a list of nodes from a collection and deals with
	/// tombstones, files, and directories
	/// </summary>
	public void DeleteNode(Nid nid, bool whackFile)
	{
		Node node = collection.GetNodeByID(nid);
		if (node == null)
		{
			Log.Spew("ignoring attempt to delete non-existent node {0}", nid);
			return;
		}

		if (whackFile)
		{
			//TODO: another place to handle multiple forks
			BaseFileNode bfn = CastToBaseFileNode(collection, node);
			if (bfn != null)
				File.Delete(bfn.GetFullPath(collection));
			else
			{
				DirNode dn = CastToDirNode(collection, node);
				if (dn != null)
					Directory.Delete(dn.GetFullPath(collection), true);
			}
		}

		Node[] deleted = collection.Delete(node, PropertyTags.Parent);
		collection.Commit(deleted);

		/* TODO: right now we never leave tombstones on the server. Fix this
		 * such that we only leave tombstones when this collection has an
		 * upstream master.
		 */
		if (onServer)
			collection.Commit(collection.Delete(deleted));
	}

	// make the path use '/' seperators since these are accepted on Create on all current filesystems
	string GetDirNodePath(Node node)
	{
		DirNode dn = CastToDirNode(collection, node);
		if (dn == null)
			return null;
		for (string path = dn.Name;; path = dn.Name + '/' + path)
			if ((dn = dn.GetParent(collection)) == null)
				return path;
	}

	/// <summary>
	/// returns a node chunk
	/// </summary>
	public NodeChunk GetSmallNode(Nid nid)
	{
		OutgoingNode ogn = new OutgoingNode(collection);
		NodeChunk chunk;
		chunk.totalSize = 0;
		chunk.forkChunks = null;
		chunk.expectedIncarn = 0;
		chunk.relativePath = null;
		if ((chunk.node = ogn.Start(nid)) != null)
		{
			chunk.relativePath = GetDirNodePath(chunk.node);
			if ((chunk.forkChunks = ogn.ReadChunks(NodeChunk.MaxSize, out chunk.totalSize)) == null)
				Log.Assert(chunk.totalSize == 0);
			else if (chunk.totalSize >= NodeChunk.MaxSize)
				/* the file grew larger than a SmallNode should handle,
				 * indicate client should retry as large node
				 */
				chunk.forkChunks = null;
		}

		//Log.Spew("chunk: {0}, expIncarn {1}, totalSize {2}, forkCount {3}, relPath {4}",
		//		chunk.node.Name, chunk.expectedIncarn, chunk.totalSize,
		//		chunk.forkChunks == null? -1: chunk.forkChunks.Length, chunk.relativePath);
		return chunk;
	}

	/// <summary>
	/// returns array of nodes with stamp, metadata and file contents
	/// this one takes Nids and does not fill in the expectedIncarn
	/// in the NodeChunks -- only called on the server
	/// </summary>
	public NodeChunk[] GetSmallNodes(Nid[] nids)
	{
		NodeChunk[] chunks = new NodeChunk[nids.Length];
		uint i = 0;
		foreach (Nid nid in nids)
			chunks[i++] = GetSmallNode(nid);
		
		return chunks;
	}

	/// <summary>
	/// returns array of nodes with stamp, metadata and file contents
	/// this one takes NodeStamps and fills in the expectedIncarn
	/// in the NodeChunks from the masterIncarn in the NodeStamp
	///    -- only makes sense to be called on the client
	/// </summary>
	public NodeChunk[] GetSmallNodes(ArrayList stampsA, ArrayList stampsB)
	{
		if (stampsA.Count == 0 && stampsB.Count == 0)
			return null;
		NodeChunk[] chunks = new NodeChunk[stampsA.Count + stampsB.Count];
		uint i = 0;
		foreach (NodeStamp stamp in stampsA)
		{
			chunks[i] = GetSmallNode(stamp.id);
			chunks[i++].expectedIncarn = stamp.masterIncarn;
		}
		foreach (NodeStamp stamp in stampsB)
		{
			chunks[i] = GetSmallNode(stamp.id);
			chunks[i++].expectedIncarn = stamp.masterIncarn;
		}
		return chunks;
	}

	/// <summary>
	/// returns nodes were not updated
	/// </summary>
    public RejectedNode[] PutSmallNodes(NodeChunk[] nodeChunks)
	{
		IncomingNode inNode = new IncomingNode(collection, onServer);
		ArrayList rejects = new ArrayList();
		Log.Spew("PutSmallNodes() {0}", nodeChunks.Length);
		foreach (NodeChunk nc in nodeChunks)
		{
			if (nc.forkChunks == null && nc.totalSize >= NodeChunk.MaxSize)
			{
				Log.Spew("skipping update of node {0}, retry next sync", nc.node.Name);
				continue;
			}
			if (collection.IsType(nc.node, NodeTypes.TombstoneType))
			{
				Log.Assert(onServer); // should not get tombstones on client from server
				DeleteNode((Nid)nc.node.ID, true);
				continue;
			}
			inNode.Start(nc.node, nc.relativePath);
			inNode.BlowChunks(nc.forkChunks);
			NodeStatus status = inNode.Complete(nc.expectedIncarn);
			if (status != NodeStatus.Complete && status != NodeStatus.FileNameConflict)
				rejects.Add(new RejectedNode((Nid)nc.node.ID, status));
		}
		return rejects.Count == 0? null: (RejectedNode[])rejects.ToArray(typeof(RejectedNode));
	}

	/// <summary>
	/// only called on the client after the node has been updated on the
	/// server. Just sets the MasterIncarnation to the LocalIncarnation
	/// </summary>
	public void UpdateIncarn(Nid nid, ulong masterIncarn)
	{
		Log.Assert(!onServer);
		Node node = collection.GetNodeByID(nid);
		node.IncarnationUpdate = masterIncarn;
		collection.Commit(node);
	}
}

//===========================================================================
}
