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
/// struct to represent the minimal information that the sync code would need
/// to know about a node to determine if it needs to be synced.
/// </summary>
[Serializable]
public struct NodeStamp: IComparable
{
	public string id;

	// if localIncarn == UInt64.MaxValue, node is a tombstone
	public ulong localIncarn, masterIncarn;

	// total size of all streams, expected to be used for sync progress meter
	// if -1, this node is not derived from BaseFileNode.
	public long streamsSize;

	/// <summary>
	/// 
	/// </summary>
	public bool isDir;

	/// <summary>
	/// 
	/// </summary>
	public ChangeLogRecord.ChangeLogOp changeType;

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
/// a chunk of data about a particular incarnation of a node
/// </summary>
[Serializable]
public struct NodeChunk
{
	internal Node node;
	public const int MaxSize = 128 * 1024;
	public string nodeString;
	public ulong expectedIncarn;
	public int totalSize;
	public string relativePath;
	public byte[] data;
}

//---------------------------------------------------------------------------
/// <summary>
/// node collision information
/// </summary>
[Serializable]
public struct RejectedNode
{
	public string nid;
	public NodeStatus status;
	internal RejectedNode(string nid, NodeStatus status)
	{
		this.nid = nid;
		this.status = status;
	}
}

//---------------------------------------------------------------------------
// various useful Sync operations that are called by client and server
internal class SyncOps
{
	SyncCollection collection;
	bool onServer;
	private const string ServerChangeLogCookieProp = "ServerChangeLogCookie";
	private const string ClientChangeLogCookieProp = "ClientChangeLogCookie";
	private string clientCookie = null;
	private string serverCookie = null;
	EventContext eventCookie;

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

	public SyncOps(SyncCollection collection, bool onServer)
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
			Node node;

			try
			{
				node = new Node(collection, sn);
				if (collection.HasCollisions(node))
					continue;
			}
			catch (Storage.DoesNotExistException e)
			{
				Log.Spew(e.Message);
				Log.Spew("Node: {0} - ID: {1} - Type: {2} no longer exists.", sn.Name, sn.ID, sn.Type );
				continue;
			}

			NodeStamp stamp = OutgoingNode.GetOutNodeStamp(collection, ref node, ChangeLogRecord.ChangeLogOp.Unknown);
			
			stampList.Add(stamp);
		}

		stampList.Sort();
        //Log.Spew("Found {0} nodes in {1}", stampList.Count, collection.Name);
		return (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
	}

	/// <summary>
	/// deletes a list of nodes from a collection and deals with
	/// tombstones, files, and directories
	/// </summary>
	public void DeleteNode(string nid, bool whackFile)
	{
		Node node = collection.GetNodeByID(nid);
		if (node == null)
		{
			Log.Spew("ignoring attempt to delete non-existent node {0}", nid);
			return;
		}

		Log.log.Info("Deleting {0}", node.Name);
		if (whackFile)
		{
			// If this is a collision node then delete the collision file.
			if (collection.HasCollisions(node))
			{
				Conflict conflict = new Conflict(collection, node);
				if (conflict.IsFileNameConflict)
				{
					File.Delete(conflict.FileNameConflictPath);
				}
			}
			else
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
		}

		// Do a deep delete.
		Node[] deleted = collection.Delete(node, PropertyTags.Parent);
		collection.Commit(deleted);
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
	public NodeChunk GetSmallNode(string nid)
	{
		OutgoingNode ogn = new OutgoingNode(collection);
		NodeChunk chunk = new NodeChunk();
		chunk.totalSize = 0;
		chunk.data = null;
		chunk.expectedIncarn = 0;
		chunk.relativePath = null;
		if ((chunk.node = ogn.Start(nid)) != null)
		{
			Log.log.Info("Synchronizing {0} to {1}", chunk.node.Name, onServer ? "client" : "server");
			chunk.relativePath = GetDirNodePath(chunk.node);
			if ((chunk.data = ogn.ReadChunk(NodeChunk.MaxSize, out chunk.totalSize)) == null)
				Log.Assert(chunk.totalSize == 0);
			else if (chunk.totalSize >= NodeChunk.MaxSize)
			{
				/* the file grew larger than a SmallNode should handle,
				 * indicate client should retry as large node
				 */
				chunk.data = null;
			}
		}
		return chunk;
	}

	/// <summary>
	/// returns array of nodes with stamp, metadata and file contents
	/// this one takes Nids and does not fill in the expectedIncarn
	/// in the NodeChunks -- only called on the server
	/// </summary>
	public NodeChunk[] GetSmallNodes(string[] nids)
	{
		NodeChunk[] chunks = new NodeChunk[nids.Length];
		uint i = 0;
		foreach (string nid in nids)
		{
			chunks[i++] = GetSmallNode(nid);
		}	
		return chunks;
	}

	/// <summary>
	/// returns array of nodes with stamp, metadata and file contents
	/// this one takes NodeStamps and fills in the expectedIncarn
	/// in the NodeChunks from the masterIncarn in the NodeStamp
	///    -- only makes sense to be called on the client
	/// </summary>
	public NodeChunk[] GetSmallNodes(Hashtable stamps)
	{
		if (stamps.Count == 0)
			return null;
		NodeChunk[] chunks = new NodeChunk[stamps.Count];
		uint i = 0;
		foreach (NodeStamp stamp in stamps.Values)
		{
			chunks[i] = GetSmallNode(stamp.id);
			chunks[i].expectedIncarn = chunks[i].node.MasterIncarnation;
			i++;
		}
		return chunks;
	}

	/// <summary>
	/// Commits the node to the local system.
	/// </summary>
	/// <param name="nc">The node to commit.</param>
	/// <returns></returns>
	public NodeStatus PutSmallNode(NodeChunk nc)
	{
		IncomingNode inNode = new IncomingNode(collection, onServer);
		if (nc.data == null && nc.totalSize >= NodeChunk.MaxSize)
		{
			Log.Spew("skipping update of node {0}, retry next sync", nc.node.Name);
			return NodeStatus.ServerFailure;
		}
		if (collection.IsType(nc.node, NodeTypes.TombstoneType))
		{
			//Log.Assert(onServer); // should not get tombstones on client from server
			DeleteNode(nc.node.ID, true);
			return NodeStatus.Complete;
		}
		
		Log.log.Info("Synchronizing {0} from {1}", nc.node.Name, onServer ? "client" : "server");
		inNode.Start(nc.node, nc.relativePath);
		inNode.BlowChunk(nc.data);
		NodeStatus status = inNode.Complete(nc.expectedIncarn);
		return status == NodeStatus.FileNameConflict ? NodeStatus.Complete : status;
	}

	/// <summary>
	/// returns nodes were not updated
	/// </summary>
    public RejectedNode[] PutSmallNodes(NodeChunk[] nodeChunks)
	{
		ArrayList rejects = new ArrayList();
		Log.Spew("PutSmallNodes() {0}", nodeChunks.Length);
		foreach (NodeChunk nc in nodeChunks)
		{
			if (nc.node != null)
			{
				NodeStatus status = PutSmallNode(nc);
				if (status != NodeStatus.Complete)
				{
					rejects.Add(new RejectedNode(nc.node.ID, status));
				}
			}
		}
		return rejects.Count == 0? null: (RejectedNode[])rejects.ToArray(typeof(RejectedNode));
	}

	/// <summary>
	/// only called on the client after the node has been updated on the
	/// server. Just sets the MasterIncarnation to the LocalIncarnation
	/// </summary>
	public void UpdateIncarn(string nid, ulong masterIncarn)
	{
		Log.Assert(!onServer);
        Node node = collection.GetNodeByID(nid);
		node.SetMasterIncarnation(masterIncarn);
		collection.Commit(node);
		//node = collection.GetNodeByID(nid);
		//Log.Assert(node.MasterIncarnation == masterIncarn);
		//Log.Spew("Updated master incarn to {0} on {1}", masterIncarn, node.Name);
	}

	/// <summary>
	/// Get the changes from the change log.
	/// </summary>
	/// <param name="nodes">returns the list of changes.</param>
	/// <param name="cookie">The cookie handed back the last call.</param>
	/// <returns>false the call failed.</returns>
	internal bool GetChangedNodeStamps(out NodeStamp[] nodes, ref string cookie)
	{
		nodes = null;
		ArrayList changeList = null;
		ArrayList stampList = new ArrayList();

		// Create a change log reader.
		ChangeLogReader logReader = new ChangeLogReader( collection );
		bool more = true;
		
		try
		{
			// Read the cookie from the last sync and then get the changes since then.
			if (cookie == null)
			{
				eventCookie = logReader.GetEventContext();
				if (eventCookie != null)
					cookie = eventCookie.ToString();
				return false;
			}
			else
			{
				eventCookie = new EventContext(cookie);
				while (more)
				{
					more = logReader.GetEvents(eventCookie, out changeList);
					foreach( ChangeLogRecord rec in changeList )
					{
						// Make sure the events are not for local only changes.
						if (((NodeEventArgs.EventFlags)rec.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
						{
							if (onServer)
							{
								NodeStamp stamp = new NodeStamp();
								stamp.localIncarn = rec.SlaveRev;
								stamp.masterIncarn = rec.MasterRev;
								stamp.id = rec.EventID;
								stamp.changeType = rec.Operation;
								stamp.streamsSize = rec.FileLength;
								stampList.Add(stamp);
							}
							else
							{
								Node node = collection.GetNodeByID(rec.EventID);
								if (node != null)
								{
									NodeStamp stamp = OutgoingNode.GetOutNodeStamp(collection, ref node, rec.Operation);
									stampList.Add(stamp);
								}
								else if (rec.Operation == ChangeLogRecord.ChangeLogOp.Deleted)
								{
									NodeStamp stamp = new NodeStamp();
									stamp.localIncarn = UInt64.MaxValue;
									stamp.masterIncarn = 0;
									stamp.id = rec.EventID;
									stamp.changeType = rec.Operation;
									stampList.Add(stamp);
								}
							}
						}
					}
				}
			}
			Log.Spew("Found {0} changed nodes in {1}", stampList.Count, collection.Name);
			nodes = (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
			cookie = eventCookie.ToString();
			return true;
		}
		catch
		{
			// The cookie is invalid.  Get a valid cookie and save it for the next sync.
			eventCookie = logReader.GetEventContext();
			if (eventCookie != null)
				cookie = eventCookie.ToString();
			return false;
		}
	}

	/// <summary>
	/// Set the cookie for the changelog.
	/// Used to get the next set of changes.
	/// </summary>
	internal void SetChangeLogCookies(string serverCookie, string clientCookie, bool persist)
	{
		this.serverCookie = serverCookie;
		this.clientCookie = clientCookie;
	
		if (persist)
		{
			if (serverCookie != null)
			{
				Property sc = new Property(ServerChangeLogCookieProp, serverCookie);
				sc.LocalProperty = true;
				collection.Properties.ModifyProperty(sc);
			}
			if (clientCookie != null)
			{
				Property cc = new Property(ClientChangeLogCookieProp, clientCookie);
				cc.LocalProperty = true;
				collection.Properties.ModifyProperty(cc);
			}
			collection.Properties.State = PropertyList.PropertyListState.Internal;
			collection.Commit();
		}
	}

	internal void GetChangeLogCookies(out string serverCookie, out string clientCookie)
	{
		if (this.serverCookie == null)
		{
			Property sc = collection.Properties.GetSingleProperty(ServerChangeLogCookieProp);
			if (sc != null)
			{
				this.serverCookie = sc.Value.ToString();
			}
		}
		if (this.clientCookie == null)
		{
			Property cc = collection.Properties.GetSingleProperty(ClientChangeLogCookieProp);
			if (cc != null)
			{
				this.clientCookie = cc.Value.ToString();
			}
		}
		serverCookie = this.serverCookie;
		clientCookie = this.clientCookie;
	}
}

//===========================================================================
}
