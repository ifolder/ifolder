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
using System.Threading;

using Simias.Storage;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// client side top level class for SynkerA style sychronization
/// </summary>
public class SynkerWorkerA: SyncCollectionWorker
{
	//TODO: why is the base master and slave not protected instead of private?
	SynkerServiceA ss;
	SyncCollection collection;

	ArrayList largeFromServer, smallFromServer, nonFileFromServer;
	ArrayList largeToServer, smallToServer, nonFileToServer;
	ArrayList killOnClient;

	/// <summary>
	/// public constructor which accepts real or proxy objects specifying master and collection
	/// </summary>
	public SynkerWorkerA(SynkerServiceA master, SyncCollection slave): base(master, slave)
	{
		ss = master;
		collection = slave;
	}

	/// <summary>
	/// used to perform one synchronization pass on one collection
	/// </summary>
	public override void DoSyncWork()
	{
		Log.Spew("-------- starting sync pass for collection {0}", collection.Name);
		try
		{
			DoOneSyncPass();
		}
		catch (Exception e)
		{
			Log.Spew("Uncaught exception in DoSyncWork: {0}", e.Message);
			Log.Spew(e.StackTrace);
		}
		catch { Log.Spew("Uncaught foreign exception in DoSyncWork"); }
		Log.Spew("-------- end of sync pass for collection {0}", collection.Name);
	}

	void PutNodeToServer(ref NodeStamp stamp, string message)
	{
		// TODO: deal with small files in pages, right now we just limit the
		// first small file page and then consider everything a large file
		Log.Spew("{0} {1} incarn {2} {3}", stamp.id, stamp.name, stamp.localIncarn, message);
		Log.Assert(stamp.streamsSize >= -1);
		if (stamp.streamsSize == -1)
			nonFileToServer.Add(stamp);
		else if (stamp.streamsSize >= NodeChunk.MaxSize || smallFromServer.Count > 100)
			largeToServer.Add(stamp);
		else
			smallToServer.Add(stamp);
	}

	void GetNodeFromServer(ref NodeStamp stamp, string message)
	{
		// TODO: deal with small files in pages, right now we just limit the
		// first small file page and then consider everything a large file
		Log.Spew("{0} {1} incarn {2} {3}", stamp.id, stamp.name, stamp.localIncarn, message);
		Log.Assert(stamp.streamsSize >= -1);
		if (stamp.streamsSize == -1)
			nonFileFromServer.Add(stamp);
		else if (stamp.streamsSize >= NodeChunk.MaxSize || smallFromServer.Count > 100)
			largeFromServer.Add(stamp);
		else
			smallFromServer.Add(stamp);
	}

	static Nid[] MoveIdsToArray(ArrayList idList)
	{
		Nid[] ids = (Nid[])idList.ToArray(typeof(Nid));
		idList.Clear();
		return ids;
	}

	/// <summary>
	/// used to perform one synchronization pass on one collection
	/// </summary>
	void DoOneSyncPass()
	{
		Access.Rights rights = ss.Start();
		if (rights == Access.Rights.Deny)
		{
			Log.Error("Sync with collection {0} denied", collection.Name);
			return;
		}

		//NodeChunk[] testchunk = ss.CollectionChunk;
		//Log.Spew("Syncing with Collection {0} {1}", testchunk[0].node.Name, testchunk[0].expectedIncarn);

		//TODO: we don't know the previous state of collection identity, will this always work?
		//collection.StoreReference.Revert();

		//new Dredger(collection, false);

		IncomingNode inNode = new IncomingNode(collection, false);
		OutgoingNode outNode = new OutgoingNode(collection);
		SyncOps ops = new SyncOps(collection, false);
		largeFromServer = new ArrayList();
		smallFromServer = new ArrayList();
		nonFileFromServer = new ArrayList();
		largeToServer = new ArrayList();
		smallToServer = new ArrayList();
		nonFileToServer = new ArrayList();
		killOnClient = new ArrayList();

		NodeStamp[] sstamps = ss.GetNodeStamps();
		NodeStamp[] cstamps = ops.GetNodeStamps();

		if (sstamps == null)
		{
			Log.Error("Server Failure: could not get nodestamps");
			return;
		}

		int si = 0, ci = 0;
		int sCount = sstamps.Length, cCount = cstamps.Length;

		Log.Spew("Got {0} nodes in chunk from server, {1} from client", sCount, cCount);

		while (si < sCount || ci < cCount)
		{
			Log.Assert(si <= sCount && ci <= cCount);
			if (si == sCount || ci < cCount && cstamps[ci].CompareTo(sstamps[si]) < 0)
			{
				// node ci exists on client but not server
				if (cstamps[ci].masterIncarn == 0
						&& cstamps[ci].localIncarn != UInt64.MaxValue
						&& rights != Access.Rights.ReadOnly)
					PutNodeToServer(ref cstamps[ci], "is new on the client, send to server");
				else
				{
					Log.Spew("{1} {0} has been killed or synced before or is RO, but is not on the server, just kill it locally",
							cstamps[ci].name, cstamps[ci].id);
					killOnClient.Add(cstamps[ci].id);
				}
				ci++;
			}
			else if (ci == cCount || cstamps[ci].CompareTo(sstamps[si]) > 0)
			{
				// node si exists on server but not client
				GetNodeFromServer(ref sstamps[si++], "exists on server but not client, get it");
			}
			else
			{
				Log.Assert(ci < cCount && si < sCount && cstamps[ci].CompareTo(sstamps[si]) == 0);
				if (cstamps[ci].localIncarn == UInt64.MaxValue)
				{
					if (rights == Access.Rights.ReadOnly)
					{
						ops.DeleteNode(cstamps[ci].id, false);
						GetNodeFromServer(ref sstamps[si], "reconstitute from server after illegal delete");
					}
					else
					{
						Log.Assert(cstamps[ci].streamsSize == -1);
						PutNodeToServer(ref cstamps[ci], "is local tombstone, delete on server");
					}
				}
				else if (sstamps[si].localIncarn != cstamps[ci].masterIncarn)
				{
					Log.Assert(sstamps[si].localIncarn > cstamps[ci].masterIncarn);
					GetNodeFromServer(ref sstamps[si], "has changed on server, get it");
				}
				else if (cstamps[ci].localIncarn != cstamps[ci].masterIncarn)
				{
					if (rights == Access.Rights.ReadOnly)
					{
						ops.DeleteNode(cstamps[ci].id, false);
						GetNodeFromServer(ref sstamps[si], "reconstitute from server after illegal update");
					}
					else
					{
						Log.Assert(cstamps[ci].localIncarn > cstamps[ci].masterIncarn);
						PutNodeToServer(ref cstamps[ci], "has changed locally, send to server");
					}
				}
				ci++;
				si++;
			}
		}

		// remove deleted nodes from client
		foreach (Nid nid in killOnClient)
			ops.DeleteNode(nid, true);

		// get nonFiles and small files from server
		NodeChunk[] updates = null;
		if (nonFileFromServer.Count > 0 || smallFromServer.Count > 0)
		{
			int i = 0;
			Nid[] ids = new Nid[nonFileFromServer.Count + smallFromServer.Count];
			foreach (NodeStamp stamp in nonFileFromServer)
				ids[i++] = stamp.id;
			foreach (NodeStamp stamp in smallFromServer)
				ids[i++] = stamp.id;
			updates = ss.GetSmallNodes(ids);
			i = 0;
			foreach (NodeStamp stamp in nonFileFromServer)
			{
				Log.Assert(updates[i].node.ID == stamp.id);
				updates[i++].expectedIncarn = stamp.masterIncarn;
			}
			foreach (NodeStamp stamp in smallFromServer)
			{
				Log.Assert(updates[i].node.ID == stamp.id);
				updates[i++].expectedIncarn = stamp.masterIncarn;
			}
		}
		if (updates != null && updates.Length > 0)
			ops.PutSmallNodes(updates);

		// push up nonFiles and small files to server
		if ((updates = ops.GetSmallNodes(nonFileToServer, smallToServer)) != null)
		{
			RejectedNode[] rejects = ss.PutSmallNodes(updates);
			if (rejects != null)
				foreach (NodeChunk nc in updates)
				{
					bool updateIncarn = true;
					foreach (RejectedNode reject in rejects)
					{
						if (reject.nid == nc.node.ID)
						{
							Log.Spew("skipping update of incarnation for small node {0} due to {1} on server",
										reject.nid, reject.status);
							updateIncarn = false;
							break;
						}
					}
					if (updateIncarn == true)
					{
						if (collection.IsType(nc.node, NodeTypes.TombstoneType))
							ops.DeleteNode((Nid)nc.node.ID, false);
						else
							ops.UpdateIncarn((Nid)nc.node.ID, nc.node.LocalIncarnation);
					}
				}
		}

		// push up large files
		foreach (NodeStamp stamp in largeToServer)
		{
			Node node;
			if ((node = outNode.Start(stamp.id)) == null)
				continue;

			int totalSize;
			ForkChunk[] chunks = outNode.ReadChunks(NodeChunk.MaxSize, out totalSize);
			if (!ss.WriteLargeNode(node, chunks))
			{
				Log.Spew("Could not write large node {0}", node.Name);
				continue;
			}
			if (chunks == null || totalSize < NodeChunk.MaxSize)
				chunks = null;
			else
				while (true)
				{
					chunks = outNode.ReadChunks(NodeChunk.MaxSize, out totalSize);
					if (chunks == null || totalSize < NodeChunk.MaxSize)
						break;
					ss.WriteLargeNode(chunks, 0, false);
				}
			NodeStatus status = ss.WriteLargeNode(chunks, stamp.masterIncarn, true);
			if (status == NodeStatus.Complete || status == NodeStatus.FileNameConflict)
				ops.UpdateIncarn((Nid)node.ID, node.LocalIncarnation);
			else
				Log.Spew("skipping update of incarnation for large node {0} due to {1}", node.Name, status);
		}

		// get large files from server
		foreach (NodeStamp stamp in largeFromServer)
		{
			NodeChunk nc = ss.ReadLargeNode(stamp.id, NodeChunk.MaxSize);
			inNode.Start(nc.node, null);
			inNode.BlowChunks(nc.forkChunks);
			while (nc.totalSize >= NodeChunk.MaxSize)
			{
				nc.forkChunks = ss.ReadLargeNode(NodeChunk.MaxSize);
				inNode.BlowChunks(nc.forkChunks);
				nc.totalSize = 0;
				foreach (ForkChunk chunk in nc.forkChunks)
					nc.totalSize += chunk.data.Length;
			}
			NodeStatus status = inNode.Complete(stamp.masterIncarn);
			if (status != NodeStatus.Complete)
				Log.Spew("failed to update large node {0} from master, status {1}", stamp.name, status);
		}
	}
}

//===========================================================================
}
