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
	public override SyncCollectionService GetCollectionService(
		SyncCollection collection)
	{
		return new SynkerServiceA(collection);
	}

	public override SyncCollectionWorker GetCollectionWorker(
		SyncCollectionService master, SyncCollection slave)
	{
		return new SynkerWorkerA((SynkerServiceA)master, slave);
	}

	public override bool WatchFileSystem()
	{
		return false;
	}
}

/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
public class SynkerServiceA: SyncCollectionService
{
	//TODO: why is the baseCollection not protected instead of private?
	Collection collection;
	Uri storePath;

	public SynkerServiceA(SyncCollection collection): base(collection)
	{
		this.collection = collection.BaseCollection;
		this.storePath = new Uri(collection.StorePath);
	}

	public SyncSession CreateSession()
	{
		// TODO: fix identity and credential
		// 
		return new SyncSession(storePath,
			collection.Owner, "novell", collection.Id, null);
	}
}

/// <summary>
/// client side top level class for SynkerA style sychronization
/// </summary>
internal class SynkerWorkerA: SyncCollectionWorker
{
	//TODO: why is the base master and slave not protected instead of private?
	SynkerServiceA master;
	Uri storeLocation;
	string collectionId;

	public SynkerWorkerA(SynkerServiceA master, SyncCollection slave): base(master, slave)
	{
		this.master = master;
		storeLocation = new Uri(slave.StorePath);
		collectionId = slave.ID;
	}

	public override void DoSyncWork()
	{
		SyncPass.Run(storeLocation, collectionId, master.CreateSession());
	}
}

//---------------------------------------------------------------------------
public class SyncSession: Session
{
	Collection collection;
	SyncOps ops;
	SyncIncomingNode inNode;
	SyncOutgoingNode outNode;

	public static Session SessionFactory(Uri storeLocation, string userId,
			string credential, string collectionId, ServStatusUpdate updater)
	{
		return new SyncSession(storeLocation, userId, credential, collectionId, updater);
	}

	public SyncSession(Uri storeLocation, string userId, string credential,
			string collectionId, ServStatusUpdate updater):
		base(userId, collectionId, updater)
	{
		// connect to the store
		Store store = Store.Connect(storeLocation);
		store.ImpersonateUser(Access.SyncOperatorRole, null);
		
		collection = store.GetCollectionById(collectionId);

		if (collection == null)
			throw new ApplicationException("No such collection");

		Log.Spew("dredging server at store {0}, docRoot '{1}'", storeLocation, collection.DocumentRoot.LocalPath);
		new Dredger(collection, true);
		inNode = new SyncIncomingNode(collection, true);
		outNode = new SyncOutgoingNode(collection);
		ops = new SyncOps(collection, true);

		//store.Revert();
		//store.ImpersonateUser(userId, credential);

		//TODO: this check should be before updating the collection from the file system
		if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
			throw new UnauthorizedAccessException("Current user cannot read this collection");
	}

	public NodeStamp[] GetNodeStamps()
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
			throw new UnauthorizedAccessException("Current user cannot read this collection");
		return ops.GetNodeStamps();
	}

	public override string Version
	{
		get { return "0.0.0"; }
	}

	public void DeleteNodes(NodeId[] nodes)
	{
		Log.Spew("SyncSession.DeleteNodes() Count {0}", nodes.Length);

		if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
			throw new UnauthorizedAccessException("Current user cannot modify this collection");
		
		ops.DeleteNodes(nodes);
	}

	/// <summary>
	/// returns array of NodeIds that were not updated due to collisions
	/// </summary>
	public NodeId[] PutSmallNodes(SmallNode[] nodes)
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
			throw new UnauthorizedAccessException("Current user cannot modify this collection");

		Log.Spew("SyncSession.PutSmallNodes() Count {0}", nodes.Length);
		return ops.PutSmallNodes(nodes);
	}

	public SmallNode[] GetSmallNodes(NodeId[] nids)
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadOnly))
			throw new UnauthorizedAccessException("Current user cannot read this collection");

		Log.Spew("SyncSession.GetSmallNodes() Count {0}", nids.Length);
		return ops.GetSmallNodes(nids);
	}

	public void WriteLargeNode(NodeStamp stamp, string relativePath, byte[] data)
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
			throw new UnauthorizedAccessException("Current user cannot modify this collection");

		inNode.Start(stamp, relativePath);
		inNode.Append(data);
	}

	public bool WriteLargeNode(byte[] data, string metaData)
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
			throw new UnauthorizedAccessException("Current user cannot modify this collection");

		inNode.Append(data);
		return metaData == null? true: inNode.Complete(metaData);
	}

	public byte[] ReadLargeNode(NodeId nid, int maxSize, out NodeStamp stamp, out string metaData, out string relativePath)
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
			throw new UnauthorizedAccessException("Current user cannot modify this collection");

		return outNode.Start(nid, out stamp, out metaData, out relativePath)? outNode.GetChunk(maxSize): null;
	}

	public byte[] ReadLargeNode(int maxSize)
	{
		if (!collection.IsAccessAllowed(Access.Rights.ReadWrite))
			throw new UnauthorizedAccessException("Current user cannot modify this collection");
		return outNode.GetChunk(maxSize);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// used to perform one synchronization pass on one collection
/// </summary>
public class SyncPass
{

	static void AddToUpdateList(NodeStamp stamp, ArrayList small, ArrayList large)
	{
		// TODO: send small files in pages, right now we just limit the
		// first small file page and then consider everything a large file
		if (stamp.streamsSize >= SmallNode.MaxSize || small.Count > 100)
			large.Add(stamp.id);
		else
			small.Add(stamp.id);
	}

	static NodeId[] MoveIdsToArray(ArrayList idList)
	{
		NodeId[] ids = (NodeId[])idList.ToArray(typeof(NodeId));
		idList.Clear();
		return ids;
	}

	public static void Run(Uri storeLocation, string collectionId, SyncSession ss)
	{
		Log.Spew("client connected to server version {0}", ss.Version);

		// connect to the store
		Store store = Store.Connect(storeLocation);
		store.ImpersonateUser(Access.SyncOperatorRole, null);
		
		Collection collection = store.GetCollectionById(collectionId);
		if (collection == null)
			throw new ArgumentException("unknown collection");

		new Dredger(collection, false);
		SyncIncomingNode inNode = new SyncIncomingNode(collection, false);
		SyncOutgoingNode outNode = new SyncOutgoingNode(collection);
		SyncOps ops = new SyncOps(collection, false);

		NodeStamp[] sstamps = ss.GetNodeStamps();
		NodeStamp[] cstamps = ops.GetNodeStamps();

		ArrayList getLargeFromServer = new ArrayList();
		ArrayList getSmallFromServer = new ArrayList();
		ArrayList killOnServer = new ArrayList();
		ArrayList killOnClient = new ArrayList();
		ArrayList sendSmallToServer = new ArrayList();
		ArrayList sendLargeToServer = new ArrayList();
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
			Debug.Assert(si <= sCount && ci <= cCount);
			if (si == sCount || ci < cCount && cstamps[ci].CompareTo(sstamps[si]) < 0)
			{
				// file ci exists on client but not server
				if (cstamps[ci].masterIncarn == 0 && cstamps[ci].localIncarn != UInt64.MaxValue)
				{
					Log.Spew("{1} '{0}' is new on the client, send to server", cstamps[ci].name,  cstamps[ci].id.s);
					AddToUpdateList(cstamps[ci], sendSmallToServer, sendLargeToServer);
				}
				else
				{
					Log.Spew("{1} '{0}' has been killed or synced before, but is no longer on the server, just kill it locally",
							cstamps[ci].name, cstamps[ci].id.s);
					killOnClient.Add(cstamps[ci].id);
				}
				ci++;
			}
			else if (ci == cCount || cstamps[ci].CompareTo(sstamps[si]) > 0)
			{
				Log.Spew("{1} '{0}' exists on server, but not client (no tombstone either), get it", sstamps[si].name, sstamps[si].id.s);
				AddToUpdateList(sstamps[si], getSmallFromServer, getLargeFromServer);
				si++;
			}
			else
			{
				if (cstamps[ci].localIncarn == UInt64.MaxValue)
				{
					Log.Spew("{1} '{0}' is local tombstone, delete on server", sstamps[si].name, sstamps[si].id.s);
					killOnServer.Add(cstamps[ci].id);
				}
				else if (sstamps[si].localIncarn != cstamps[ci].masterIncarn)
				{
					Debug.Assert(sstamps[si].localIncarn > cstamps[ci].masterIncarn);
					Log.Spew("{2} '{0}' has changed on server, get incarn {1}", sstamps[si].name, sstamps[si].localIncarn, sstamps[si].id.s);
					AddToUpdateList(sstamps[si], getSmallFromServer, getLargeFromServer);
				}
				else if (cstamps[ci].localIncarn != cstamps[ci].masterIncarn)
				{
					Debug.Assert(cstamps[ci].localIncarn > cstamps[ci].masterIncarn);
					Log.Spew("{2} '{0}' has changed, send incarn {1} to server", cstamps[ci].name, cstamps[ci].localIncarn, cstamps[ci].id.s);
					AddToUpdateList(cstamps[ci], sendSmallToServer, sendLargeToServer);
				}
				ci++;
				si++;
			}
		}

		// remove files from client
		NodeId[] ids = MoveIdsToArray(killOnClient);
		if (ids != null && ids.Length > 0)
			ops.DeleteNodes(ids);

		// remove files on server
		ids = MoveIdsToArray(killOnServer);
		if (ids != null && ids.Length > 0)
		{
			ss.DeleteNodes(ids);
			ops.DeleteNodes(ids); // remove tombstones from client
		}

		// push up new and modified small files
		SmallNode[] updates = null;
		ids = MoveIdsToArray(sendSmallToServer);
		if (ids != null && ids.Length > 0)
			updates = ops.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
		{
			NodeId[] rejectedUpdates = ss.PutSmallNodes(updates);
			foreach (SmallNode sn in updates)
				if (Array.IndexOf(rejectedUpdates, sn.stamp.id) == -1)
					ops.UpdateIncarn(sn.stamp);
		}

		// get small files from server
		updates = null;
		ids = MoveIdsToArray(getSmallFromServer);
		if (ids != null && ids.Length > 0)
			updates = ss.GetSmallNodes(ids);
		if (updates != null && updates.Length > 0)
			ops.PutSmallNodes(updates);

		// push up new and modified large files
		foreach (NodeId nid in sendLargeToServer)
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
				Log.Spew("skipping update of incarnation for large node {0} due to local collision", stamp.name);
		}
		sendLargeToServer.Clear();

		// get large files from server
		foreach (NodeId nid in getLargeFromServer)
		{
			NodeStamp stamp;
			string metaData, relativePath;
			byte[] data = ss.ReadLargeNode(nid, SmallNode.MaxSize, out stamp, out metaData, out relativePath);
			inNode.Start(stamp, relativePath);
			inNode.Append(data);
			while (data.Length == SmallNode.MaxSize)
			{
				data = ss.ReadLargeNode(SmallNode.MaxSize);
				inNode.Append(data);
			}
			if (!inNode.Complete(metaData))
				Log.Spew("skipped update of large node {0} from server due to local collision", stamp.name);
		}
		getLargeFromServer.Clear();
	}
}

//===========================================================================
}
