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

	/// <summary>
	/// SynkerA does not need another process to watch the file system yet, always return false
	/// </summary>
	public override bool WatchFileSystem()
	{
		return false;
	}

	/// <summary>
	/// query the sync collection service class type.
	/// </summary>
	/// <returns>The type object of the sync collection service class.</returns>
	public override Type GetCollectionServiceType()
	{
		return typeof(SynkerServiceA);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
public class SynkerServiceA: SyncCollectionService
{
	Collection collection;
	SyncOps ops;
	SyncIncomingNode inNode;
	SyncOutgoingNode outNode;

	/// <summary>
	/// public ctor 
	/// </summary>
	public SynkerServiceA(SyncCollection collection): base(collection)
	{
		this.collection = collection.BaseCollection;
	}

	/// <summary>
	/// start sync of this collection -- perform basic role checks and dredge server file system
	/// </summary>
	public bool Start()
	{
		// TODO: fix identity, get it from channel?
		try
		{
			collection.LocalStore.ImpersonateUser(Access.SyncOperatorRole);
		
			Log.Spew("dredging server at docRoot '{0}'", collection.DocumentRoot.LocalPath);
			new Dredger(collection, true);
			Log.Spew("done dredging server at docRoot '{0}'", collection.DocumentRoot.LocalPath);
			inNode = new SyncIncomingNode(collection, true);
			outNode = new SyncOutgoingNode(collection);
			ops = new SyncOps(collection, true);

			//store.Revert();
			//store.ImpersonateUser(userId);

			//TODO: this check should be before updating the collection from the file system
			if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
				return false;
			Log.Spew("session started");
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
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
	// TODO: better handle exceptions, return array of failed deletes?
	public bool DeleteNodes(Nid[] nodes)
	{
		try
		{
			Log.Spew("SyncSession.DeleteNodes() Count {0}", nodes.Length);

			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");
		
			ops.DeleteNodes(nodes);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	/// <summary>
	/// takes an array of small nodes. returns rejected nodes
	/// </summary>
	public RejectedNodes PutSmallNodes(NodeChunk[] nodes)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			Log.Spew("SyncSession.PutSmallNodes() Count {0}", nodes.Length);
			return ops.PutSmallNodes(nodes);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return new RejectedNodes();
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
	public bool WriteLargeNode(NodeStamp stamp, FseChunk[] fseChunks)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.Start(stamp);
			inNode.WriteChunks(fseChunks);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	/// <summary>
	/// takes next chunk of data for a large node, completes node if metadata is given
	/// </summary>
	public SyncIncomingNode.Status WriteLargeNode(FseChunk[] fseChunks, string metaData)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.WriteChunks(fseChunks);
			return metaData == null? SyncIncomingNode.Status.InProgess: inNode.Complete(metaData);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return SyncIncomingNode.Status.ServerFailure;
	}

	/// <summary>
	/// gets metadata and first chunk of data for a large node
	/// </summary>
	public NodeChunk ReadLargeNode(Nid nid, int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			NodeChunk nc = new NodeChunk();
			nc.fseChunks = outNode.Start(nid, out nc.stamp, out nc.metaData)? outNode.ReadChunks(maxSize, out nc.totalSize): null;
			return nc;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return new NodeChunk();
	}

	/// <summary>
	/// gets next chunks of data for a large node
	/// </summary>
	public FseChunk[] ReadLargeNode(int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

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
	Collection collection;

	public SynkerWorkerA(SynkerServiceA master, SyncCollection slave): base(master, slave)
	{
		ss = master;
		collection = slave.BaseCollection;
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
		ss.Start();

		Log.Spew("client connected to server version {0}", ss.Version);

		collection.LocalStore.ImpersonateUser(Access.SyncOperatorRole);

		new Dredger(collection, false);
		SyncIncomingNode inNode = new SyncIncomingNode(collection, false);
		SyncOutgoingNode outNode = new SyncOutgoingNode(collection);
		SyncOps ops = new SyncOps(collection, false);

		NodeStamp[] sstamps = ss.GetNodeStamps();
		NodeStamp[] cstamps = ops.GetNodeStamps();

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
				// file ci exists on client but not server
				if (cstamps[ci].masterIncarn == 0 && cstamps[ci].localIncarn != UInt64.MaxValue)
				{
					Log.Spew("{1} '{0}' is new on the client, send to server", cstamps[ci].name,  cstamps[ci].id);
					AddToUpdateList(cstamps[ci], smallToServer, largeToServer);
				}
				else
				{
					Log.Spew("{1} '{0}' has been killed or synced before, but is no longer on the server, just kill it locally",
							cstamps[ci].name, cstamps[ci].id);
					killOnClient.Add(cstamps[ci].id);
				}
				ci++;
			}
			else if (ci == cCount || cstamps[ci].CompareTo(sstamps[si]) > 0)
			{
				Log.Spew("{1} '{0}' exists on server, but not client (no tombstone either), get it", sstamps[si].name, sstamps[si].id);
				AddToUpdateList(sstamps[si], smallFromServer, largeFromServer);
				si++;
			}
			else
			{
				if (cstamps[ci].localIncarn == UInt64.MaxValue)
				{
					Log.Spew("{1} '{0}' is local tombstone, delete on server", sstamps[si].name, sstamps[si].id);
					killOnServer.Add(cstamps[ci].id);
				}
				else if (sstamps[si].localIncarn != cstamps[ci].masterIncarn)
				{
					Log.Assert(sstamps[si].localIncarn > cstamps[ci].masterIncarn);
					Log.Spew("{2} '{0}' has changed on server, get incarn {1}", sstamps[si].name, sstamps[si].localIncarn, sstamps[si].id);
					AddToUpdateList(sstamps[si], smallFromServer, largeFromServer);
				}
				else if (cstamps[ci].localIncarn != cstamps[ci].masterIncarn)
				{
					Log.Assert(cstamps[ci].localIncarn > cstamps[ci].masterIncarn);
					Log.Spew("{2} '{0}' has changed, send incarn {1} to server", cstamps[ci].name, cstamps[ci].localIncarn, cstamps[ci].id);
					AddToUpdateList(cstamps[ci], smallToServer, largeToServer);
				}
				ci++;
				si++;
			}
		}

		// remove files from client
		Nid[] ids = MoveIdsToArray(killOnClient);
		if (ids != null && ids.Length > 0)
			ops.DeleteNodes(ids);

		// remove files on server
		ids = MoveIdsToArray(killOnServer);
		if (ids != null && ids.Length > 0)
		{
			ss.DeleteNodes(ids);
			ops.DeleteNodes(ids); // remove tombstones from client
		}

		// push up small files
		NodeChunk[] updates = null;
		ids = MoveIdsToArray(smallToServer);
		if (ids != null && ids.Length > 0)
			updates = ops.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
		{
			RejectedNodes rejects = ss.PutSmallNodes(updates);
			foreach (NodeChunk nc in updates)
			{
				if (Array.IndexOf(rejects.updateCollisions, nc.stamp.id) == -1
						&& Array.IndexOf(rejects.fileSystemEntryCollisions, nc.stamp.id) == -1)
					ops.UpdateIncarn(nc.stamp);
			}
			foreach (Nid nid in rejects.fileSystemEntryCollisions)
				ops.DeleteSpuriousNode(nid);
		}

		// get small files from server
		updates = null;
		ids = MoveIdsToArray(smallFromServer);
		if (ids != null && ids.Length > 0)
			updates = ss.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
			ops.PutSmallNodes(updates);
		updates = null;

		// push up large files
		foreach (Nid nid in largeToServer)
		{
			NodeStamp stamp;
			string metaData;
			if (!outNode.Start(nid, out stamp, out metaData))
				continue;

			int totalSize;
			FseChunk[] chunks = outNode.ReadChunks(NodeChunk.MaxSize, out totalSize);
			if (!ss.WriteLargeNode(stamp, chunks))
			{
				Log.Spew("Could not write large node {0}", stamp.name);
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
					ss.WriteLargeNode(chunks, null);
				}
			SyncIncomingNode.Status status = ss.WriteLargeNode(chunks, metaData);
			switch (status)
			{
				case SyncIncomingNode.Status.Complete: ops.UpdateIncarn(stamp); break;
				case SyncIncomingNode.Status.UpdateCollision:
					Log.Spew("skipping update of incarnation for large node {0} due to update collision on server", stamp.name);
					break;
				case SyncIncomingNode.Status.FileSystemEntryCollision:
					ops.DeleteSpuriousNode(nid);
					break;
				default:
					Log.Spew("skipping update of incarnation for large node {0} due to update {1}", stamp.name, status.ToString());
					break;
			}
		}
		largeToServer.Clear();

		// get large files from server
		foreach (Nid nid in largeFromServer)
		{
			NodeChunk nc = ss.ReadLargeNode(nid, NodeChunk.MaxSize);
			inNode.Start(nc.stamp);
			inNode.WriteChunks(nc.fseChunks);
			while (nc.totalSize >= NodeChunk.MaxSize)
			{
				nc.fseChunks = ss.ReadLargeNode(NodeChunk.MaxSize);
				inNode.WriteChunks(nc.fseChunks);
				nc.totalSize = 0;
				foreach (FseChunk chunk in nc.fseChunks)
					nc.totalSize += chunk.data.Length;
			}
			SyncIncomingNode.Status status = inNode.Complete(nc.metaData);
			switch (status)
			{
				case SyncIncomingNode.Status.Complete: break;
				default:
					Log.Spew("skipping update of incarnation for large node {0} due to update {1}", nid.ToString(), status.ToString());
					break;
			}
		}
		largeFromServer.Clear();
	}
}

//===========================================================================
}
