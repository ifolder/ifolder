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
/// <summary>
/// class to register guts of SynkerA-style synchronization
/// </summary>
public class SynkerA: SyncLogicFactory
{
	public override SyncCollectionService GetCollectionService(SyncCollection collection)
	{
		return new SynkerServiceA(collection);
	}

	public override SyncCollectionWorker GetCollectionWorker(SyncCollectionService master, SyncCollection slave)
	{
		return new SynkerWorkerA((SynkerServiceA)master, slave);
	}

	public override bool WatchFileSystem()
	{
		return false;
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

	public SynkerServiceA(SyncCollection collection): base(collection)
	{
		this.collection = collection.BaseCollection;
	}

	// TODO: fix identity and credential
	public bool Start()
	{
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

	public string Version
	{
		get { return "0.0.0"; }
	}

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
	/// returns array of Nids that were not updated due to collisions
	/// </summary>
	public Nid[] PutSmallNodes(SmallNode[] nodes)
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

	public SmallNode[] GetSmallNodes(Nid[] nids)
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

	public bool WriteLargeNode(NodeStamp stamp, string relativePath, byte[] data)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.Start(stamp, relativePath);
			inNode.Append(data);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	public bool WriteLargeNode(byte[] data, string metaData)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.Append(data);
			return metaData == null? true: inNode.Complete(metaData);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	public byte[] ReadLargeNode(Nid nid, int maxSize, out NodeStamp stamp, out string metaData, out string relativePath)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			return outNode.Start(nid, out stamp, out metaData, out relativePath)? outNode.GetChunk(maxSize): null;
		}
		catch (Exception e) { Log.Uncaught(e); }
		stamp = new NodeStamp();
		metaData = null;
		relativePath = null;
		return null;
	}

	// right now reusing small node struct for first chunk of large node
	public SmallNode ReadLargeNode(Nid nid, int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			//SmallNode[] sn = new SmallNode[1];
			//sn[0].data = outNode.Start(nid, out sn[0].stamp, out sn[0].metaData, out sn[0].relativePath)? outNode.GetChunk(maxSize): null;
			SmallNode sn;
			sn.data = outNode.Start(nid, out sn.stamp, out sn.metaData, out sn.relativePath)? outNode.GetChunk(maxSize): null;
			return sn;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return new SmallNode();
	}

	public byte[] ReadLargeNode(int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");
			return outNode.GetChunk(maxSize);
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
		if (stamp.streamsSize >= SmallNode.MaxSize || small.Count > 100)
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

		ArrayList addLargeFromServer = new ArrayList();
		ArrayList addSmallFromServer = new ArrayList();
		ArrayList updateLargeFromServer = new ArrayList();
		ArrayList updateSmallFromServer = new ArrayList();
		ArrayList killOnServer = new ArrayList();
		ArrayList killOnClient = new ArrayList();
		ArrayList addSmallToServer = new ArrayList();
		ArrayList addLargeToServer = new ArrayList();
		ArrayList updateSmallToServer = new ArrayList();
		ArrayList updateLargeToServer = new ArrayList();
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
					AddToUpdateList(cstamps[ci], addSmallToServer, addLargeToServer);
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
				AddToUpdateList(sstamps[si], addSmallFromServer, addLargeFromServer);
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
					AddToUpdateList(sstamps[si], updateSmallFromServer, updateLargeFromServer);
				}
				else if (cstamps[ci].localIncarn != cstamps[ci].masterIncarn)
				{
					Log.Assert(cstamps[ci].localIncarn > cstamps[ci].masterIncarn);
					Log.Spew("{2} '{0}' has changed, send incarn {1} to server", cstamps[ci].name, cstamps[ci].localIncarn, cstamps[ci].id);
					AddToUpdateList(cstamps[ci], updateSmallToServer, updateLargeToServer);
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

		// push up new small files
		SmallNode[] updates = null;
		ids = MoveIdsToArray(addSmallToServer);
		if (ids != null && ids.Length > 0)
			updates = ops.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
		{
			Nid[] rejectedUpdates = ss.PutSmallNodes(updates);
			foreach (SmallNode sn in updates)
				if (Array.IndexOf(rejectedUpdates, sn.stamp.id) == -1)
					ops.UpdateIncarn(sn.stamp);
				//TODO: else
				//TODO:	DeleteNode (not stream);
		}

		// push up modified small files
		updates = null;
		ids = MoveIdsToArray(updateSmallToServer);
		if (ids != null && ids.Length > 0)
			updates = ops.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
		{
			Nid[] rejectedUpdates = ss.PutSmallNodes(updates);
			foreach (SmallNode sn in updates)
				if (Array.IndexOf(rejectedUpdates, sn.stamp.id) == -1)
					ops.UpdateIncarn(sn.stamp);
		}

		// get new small files from server
		updates = null;
		ids = MoveIdsToArray(addSmallFromServer);
		if (ids != null && ids.Length > 0)
			updates = ss.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
			ops.PutSmallNodes(updates);

		// get modified small files from server
		updates = null;
		ids = MoveIdsToArray(updateSmallFromServer);
		if (ids != null && ids.Length > 0)
			updates = ss.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
			ops.PutSmallNodes(updates);

		// push up new and modified large files
		foreach (Nid nid in addLargeToServer)
		{
			NodeStamp stamp;
			string metaData, relativePath;
			if (!outNode.Start(nid, out stamp, out metaData, out relativePath))
				continue;
			byte[] data = outNode.GetChunk(SmallNode.MaxSize);
			ss.WriteLargeNode(stamp, relativePath, data);
			if (data == null || data.Length < SmallNode.MaxSize)
				data = null;
			else
				while (true)
				{
					data = outNode.GetChunk(SmallNode.MaxSize);
					if (data == null || data.Length < SmallNode.MaxSize)
						break;
					ss.WriteLargeNode(data, null);
				}
			if (ss.WriteLargeNode(data, metaData))
				ops.UpdateIncarn(stamp);
			else
				Log.Spew("skipping update of incarnation for large node {0} due to collision on server", stamp.name);
		}
		addLargeToServer.Clear();

		// get large files from server
		foreach (Nid nid in addLargeFromServer)
		{
			//SmallNode[] sna = ss.ReadLargeNode(nid, SmallNode.MaxSize);
			//if (sna == null)
			//{
			//	Log.Here();
			//	continue;
			//}
			//SmallNode sn = sna[0];
			SmallNode sn = ss.ReadLargeNode(nid, SmallNode.MaxSize);
			inNode.Start(sn.stamp, sn.relativePath);
			inNode.Append(sn.data);
			while (sn.data.Length == SmallNode.MaxSize)
			{
				sn.data = ss.ReadLargeNode(SmallNode.MaxSize);
				inNode.Append(sn.data);
			}
			if (!inNode.Complete(sn.metaData))
				Log.Spew("skipped update of large node {0} from server due to local collision", sn.stamp.name);
		}
		addLargeFromServer.Clear();
	}
}

//===========================================================================
}
