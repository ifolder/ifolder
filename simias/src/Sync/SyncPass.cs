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
/// class to register guts of SynkerA-style synchronization
/// </summary>
public class SynkerA: SyncLogicFactory
{
	/// <summary>
	/// creates a SynkerServiceA on the server
	/// </summary>
	public override SyncCollectionService GetCollectionService(SyncCollection collection)
	{
		return new SynkerServiceA(collection);
	}

	/// <summary>
	/// creates a SynkerWorkerA on the client
	/// </summary>
	public override SyncCollectionWorker GetCollectionWorker(SyncCollectionService master, SyncCollection slave)
	{
		return new SynkerWorkerA((SynkerServiceA)master, slave);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
public class SynkerServiceA: SyncCollectionService
{
	SyncCollection collection;
	SyncOps ops;
	SyncIncomingNode inNode;
	SyncOutgoingNode outNode;
	bool ignoreRights = false;

	/// <summary>
	/// public ctor 
	/// </summary>
	public SynkerServiceA(SyncCollection collection): base(collection)
	{
		this.collection = collection;
	}

	/// <summary>
	/// debug ctor 
	/// </summary>
	public SynkerServiceA(SyncCollection collection, bool ignoreRights): base(collection)
	{
		this.collection = collection;
		this.ignoreRights = ignoreRights;
	}

	/// <summary>
	/// start sync of this collection -- perform basic role checks and dredge server file system
	/// </summary>
	public Access.Rights Start()
	{
		Access.Rights rights = Access.Rights.Deny;
		
		string userID = null;

		try
		{
			userID = Thread.CurrentPrincipal.Identity.Name;
			Log.Spew("Sync session starting for {0}", userID);
		}
		catch (Exception e)
		{
			// kludge
			Log.Spew("could not get identity in sync start, error {0}", e.Message);
			ignoreRights = true;
		}

		//TODO: where did SyncOperatorRole go?
		if (ignoreRights || userID == String.Empty)
			userID = "unknown-user";
		//	userID = Access.SyncOperatorRole;
		collection.StoreReference.ImpersonateUser(userID);
		if (collection.IsAccessAllowed(Access.Rights.ReadOnly))
		{
			//TODO: where did SyncOperatorRole go?
			//collection.StoreReference.ImpersonateUser(Access.SyncOperatorRole);
			Log.Spew("dredging server for collection '{0}'", collection.Name);
			new Dredger(collection, true);
			Log.Spew("done dredging server for collection '{0}'", collection.Name);
			inNode = new SyncIncomingNode(collection, true);
			outNode = new SyncOutgoingNode(collection);
			ops = new SyncOps(collection, true);
			collection.StoreReference.Revert();
			rights = collection.GetUserAccess(userID);
		}
		
		return rights;
	}

	/// <summary>
	/// returns array of NodeStamps for all Nodes in this collection
	/// </summary>
	public NodeStamp[] GetNodeStamps()
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");
			return ops.GetNodeStamps();
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}

	/// <summary>
	/// simple version string, also useful to check remoting
	/// </summary>
	public string Version
	{
		get { return "0.0.0"; }
	}

	/// <summary>
	/// deletes specified nodes
	/// </summary>
	public bool DeleteNodes(Nid[] nids)
	{
		try
		{
			Log.Spew("SyncSession.DeleteNodes() Count {0}", nids.Length);

			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");
		
			ops.DeleteNodes(nids);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	/// <summary>
	/// takes an array of small nodes. returns rejected nodes
	/// </summary>
	public RejectedNode[] PutSmallNodes(NodeChunk[] nodes)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			Log.Spew("SyncSession.PutSmallNodes() Count {0}", nodes.Length);
			return ops.PutSmallNodes(nodes);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}

	/// <summary>
	/// gets an array of small nodes
	/// </summary>
	public NodeChunk[] GetSmallNodes(Nid[] nids)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			Log.Spew("SyncSession.GetSmallNodes() Count {0}", nids.Length);
			return ops.GetSmallNodes(nids);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}

	/// <summary>
	/// takes metadata and first chunk of data for a large node
	/// </summary>
	public bool WriteLargeNode(Node node, ForkChunk[] forkChunks)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.Start(node);
			inNode.BlowChunks(forkChunks);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	/// <summary>
	/// takes next chunk of data for a large node, completes node if done
	/// </summary>
	public NodeStatus WriteLargeNode(ForkChunk[] forkChunks, ulong expectedIncarn, bool done)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.BlowChunks(forkChunks);
			return done? inNode.Complete(expectedIncarn): NodeStatus.InProgess;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return NodeStatus.ServerFailure;
	}

	/// <summary>
	/// gets metadata and first chunk of data for a large node
	/// </summary>
	public NodeChunk ReadLargeNode(Nid nid, int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			NodeChunk nc = new NodeChunk();
			nc.forkChunks = (nc.node = outNode.Start(nid)) != null? outNode.ReadChunks(maxSize, out nc.totalSize): null;
			return nc;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return new NodeChunk();
	}

	/// <summary>
	/// gets next chunks of data for a large node
	/// </summary>
	public ForkChunk[] ReadLargeNode(int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			int unused;
			return outNode.ReadChunks(maxSize, out unused);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// client side top level class for SynkerA style sychronization
/// </summary>
public class SynkerWorkerA: SyncCollectionWorker
{
	//TODO: why is the base master and slave not protected instead of private?
	SynkerServiceA ss;
	SyncCollection collection;

	/// <summary>
	/// public constructor which accepts real or proxy objects specifying master and collection
	/// </summary>
	public SynkerWorkerA(SynkerServiceA master, SyncCollection slave): base(master, slave)
	{
		ss = master;
		collection = slave;
	}

	static void AddToUpdateList(NodeStamp stamp, ArrayList small, ArrayList large)
	{
		// TODO: send small files in pages, right now we just limit the
		// first small file page and then consider everything a large file
		if (stamp.streamsSize >= NodeChunk.MaxSize || small.Count > 100)
			large.Add(stamp.id);
		else
			small.Add(stamp.id);
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
	public override void DoSyncWork()
	{
	Log.Spew("-------- starting sync pass for collection {0}", collection.Name);
	try
	{
		Access.Rights rights = ss.Start();
		Log.Spew("-------- started sync pass for collection {0}", collection.Name);

		if (rights == Access.Rights.Deny)
		{
			Log.Error("Sync with collection {0} denied", collection.Name);
			return;
		}

		Log.Spew("-------- starting dredge for collection {0}", collection.Name);
		//TODO: what happened to SyncOperatorRole?
		//collection.StoreReference.ImpersonateUser(Access.SyncOperatorRole);
		new Dredger(collection, false);

		Log.Spew("-------- completed dredge for collection {0}", collection.Name);

		SyncIncomingNode inNode = new SyncIncomingNode(collection, false);
		SyncOutgoingNode outNode = new SyncOutgoingNode(collection);
		SyncOps ops = new SyncOps(collection, false);


		NodeStamp[] sstamps = ss.GetNodeStamps();
		Log.Spew("-------- got local node stamps for collection {0}", collection.Name);

		NodeStamp[] cstamps = ops.GetNodeStamps();
		Log.Spew("-------- got server node stamps for collection {0}", collection.Name);

		ArrayList largeFromServer = new ArrayList();
		ArrayList smallFromServer = new ArrayList();
		ArrayList killOnServer = new ArrayList();
		ArrayList killOnClient = new ArrayList();
		ArrayList smallToServer = new ArrayList();
		ArrayList largeToServer = new ArrayList();
		int si = 0, ci = 0;
		int sCount = sstamps.Length, cCount = cstamps.Length;

		Log.Spew("Got {0} NodeStamps from server, {1} from client", sCount, cCount);

		//Log.Spew("Got {0} NodeStamps from server", sCount);
		//foreach (NodeStamp stamp in sstamps)
		//	Log.Spew("{0} {1}", stamp.id.s, stamp.name);
		//Log.Spew("Got {0} NodeStamps from client", cCount);
		//foreach (NodeStamp stamp in cstamps)
		//	Log.Spew("{0} {1}", stamp.id.s, stamp.name);

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
				{
					Log.Spew("{1} '{0}' is new on the client, send to server", cstamps[ci].name,  cstamps[ci].id);
					AddToUpdateList(cstamps[ci], smallToServer, largeToServer);
				}
				else
				{
					Log.Spew("{1} '{0}' has been killed or synced before or is RO, but is not on the server, just kill it locally",
							cstamps[ci].name, cstamps[ci].id);
					killOnClient.Add(cstamps[ci].id);
				}
				ci++;
			}
			else if (ci == cCount || cstamps[ci].CompareTo(sstamps[si]) > 0)
			{
				// node si exists on server but not client
				Log.Spew("{1} '{0}' exists on server but not client, get it", sstamps[si].name, sstamps[si].id);
				AddToUpdateList(sstamps[si], smallFromServer, largeFromServer);
				si++;
			}
			else
			{
				if (cstamps[ci].localIncarn == UInt64.MaxValue)
				{
					if (rights == Access.Rights.ReadOnly)
					{
						ops.DeleteSpuriousNode(cstamps[ci].id);
						Log.Spew("{1} '{0}' reconstitute from server after illegal delete", cstamps[ci].name, cstamps[ci].id);
						AddToUpdateList(cstamps[ci], smallFromServer, largeFromServer);
					}
					else
					{
						Log.Spew("{1} '{0}' is local tombstone, delete on server", sstamps[si].name, sstamps[si].id);
						killOnServer.Add(cstamps[ci].id);
					}
				}
				else if (sstamps[si].localIncarn != cstamps[ci].masterIncarn)
				{
					Log.Assert(sstamps[si].localIncarn > cstamps[ci].masterIncarn);
					Log.Spew("{2} '{0}' has changed on server, get incarn {1}", sstamps[si].name, sstamps[si].localIncarn, sstamps[si].id);
					AddToUpdateList(sstamps[si], smallFromServer, largeFromServer);
				}
				else if (cstamps[ci].localIncarn != cstamps[ci].masterIncarn)
				{
					if (rights == Access.Rights.ReadOnly)
					{
						ops.DeleteSpuriousNode(cstamps[ci].id);
						Log.Spew("{1} '{0}' reconstitute from server after illegal update", cstamps[ci].name, cstamps[ci].id);
						AddToUpdateList(cstamps[ci], smallFromServer, largeFromServer);
					}
					else
					{
						Log.Assert(cstamps[ci].localIncarn > cstamps[ci].masterIncarn);
						Log.Spew("{2} '{0}' has changed, send incarn {1} to server", cstamps[ci].name, cstamps[ci].localIncarn, cstamps[ci].id);
						AddToUpdateList(cstamps[ci], smallToServer, largeToServer);
					}
				}
				ci++;
				si++;
			}
		}

		// remove deleted nodes from client
		Nid[] ids = MoveIdsToArray(killOnClient);
		if (ids != null && ids.Length > 0)
			ops.DeleteNodes(ids);

		// remove deleted nodes from server
		ids = MoveIdsToArray(killOnServer);
		if (ids != null && ids.Length > 0)
		{
			ss.DeleteNodes(ids);
			ops.DeleteNodes(ids); // remove tombstones from client
		}

		// get small files from server
		NodeChunk[] updates = null;
		ids = MoveIdsToArray(smallFromServer);
		if (ids != null && ids.Length > 0)
			updates = ss.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
			ops.PutSmallNodes(updates);
		updates = null;

		// push up small files
		updates = null;
		ids = MoveIdsToArray(smallToServer);
		if (ids != null && ids.Length > 0)
			updates = ops.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
		{
			RejectedNode[] rejects = ss.PutSmallNodes(updates);
			if (rejects == null)
				Log.Spew("null rejects list");
			else
			{
				foreach (NodeChunk nc in updates)
				{
					bool updateIncarn = true;
					foreach (RejectedNode reject in rejects)
					{
						if (reject.nid == nc.node.ID)
						{
							//if (reject.status == NodeStatus.FileSystemEntryCollision)
							//	ops.DeleteSpuriousNode(reject.nid);
							//else
								Log.Spew("skipping update of incarnation for small node {0} due to {1} on server",
										reject.nid, reject.status);
							updateIncarn = false;
							break;
						}
					}
					if (updateIncarn == true)
						ops.UpdateIncarn((Nid)nc.node.ID);
				}
			}
		}

		// push up large files
		foreach (Nid nid in largeToServer)
		{
			Node node;
			if ((node = outNode.Start(nid)) == null)
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
			//TODO: fill in expectedIncarn here
			NodeStatus status = ss.WriteLargeNode(chunks, 0, true);
			switch (status)
			{
				case NodeStatus.Complete: ops.UpdateIncarn(nid); break;
				//case NodeStatus.FileSystemEntryCollision: ops.DeleteSpuriousNode(nid); break;
				default: Log.Spew("skipping update of incarnation for large node {0} due to {1}", node.Name, status); break;
			}
		}
		largeToServer.Clear();

		// get large files from server
		foreach (Nid nid in largeFromServer)
		{
			NodeChunk nc = ss.ReadLargeNode(nid, NodeChunk.MaxSize);
			inNode.Start(nc.node);
			inNode.BlowChunks(nc.forkChunks);
			while (nc.totalSize >= NodeChunk.MaxSize)
			{
				nc.forkChunks = ss.ReadLargeNode(NodeChunk.MaxSize);
				inNode.BlowChunks(nc.forkChunks);
				nc.totalSize = 0;
				foreach (ForkChunk chunk in nc.forkChunks)
					nc.totalSize += chunk.data.Length;
			}
			//TODO: fill in expectedIncarn here
			NodeStatus status = inNode.Complete(0);
			switch (status)
			{
				case NodeStatus.Complete: break;
				default: Log.Spew("skipping update of incarnation for large node {0} due to update {1}", nid, status); break;
			}
		}
		largeFromServer.Clear();
	}
	catch (Exception e)
	{
		Log.Spew("Uncaught exception in DoSyncWork: {0}", e.Message);
		Log.Spew(e.StackTrace);
	}
	catch { Log.Spew("Uncaught foreign exception in DoSyncWork"); }
	Log.Spew("-------- end of sync pass for collection {0}", collection.Name);
	}
}

//===========================================================================
}
