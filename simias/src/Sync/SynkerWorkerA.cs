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
	
	Hashtable	largeFromServer = new Hashtable();
	Hashtable	smallFromServer = new Hashtable();
	Hashtable	dirsFromServer = new Hashtable();
	Hashtable	largeToServer = new Hashtable();
	Hashtable	smallToServer = new Hashtable();
	Hashtable	dirsToServer = new Hashtable();
    Hashtable	killOnClient = new Hashtable();
	SyncOps		ops;
	static int BATCH_SIZE = 10;
	bool		moreWork;
	bool		HadErrors;
			
	bool stopping;

	/// <summary>
	/// public constructor which accepts real or proxy objects specifying master and collection
	/// </summary>
	public SynkerWorkerA(SyncCollection collection): base(collection)
	{
		ops = new SyncOps(collection, false);
	}

	/// <summary>
	/// used to perform one synchronization pass on one collection
	/// </summary>
	public override void DoSyncWork(SyncCollectionService service)
	{
		// save service
		ss = (SynkerServiceA)service;
		HadErrors = false;

		// Run the dredger
		new Dredger(collection, false);

		stopping = false;
		Log.log.Debug("-------- starting sync pass for collection {0}", collection.Name);
		try
		{
			moreWork = true;
			while (moreWork && ! stopping)
			{
				DoOneSyncPass();
			}
		}
		catch (Exception e)
		{
			Log.log.Debug("Uncaught exception in DoSyncWork: {0}", e.Message);
			Log.log.Debug(e.StackTrace);
		}
		catch { Log.log.Debug("Uncaught foreign exception in DoSyncWork"); }
		Log.log.Debug("-------- end of sync pass for collection {0}", collection.Name);
	}

	/// <summary>
	/// Called to Stop the current sync.
	/// We will stop when we are at a good stopping point.
	/// </summary>
	public override void StopSyncWork()
	{
		stopping = true;
	}

	void PutNodeToServer(ref NodeStamp stamp, string message)
	{
		// TODO: deal with small files in pages, right now we just limit the
		// first small file page and then consider everything a large file
		Log.log.Debug("{0} {1} incarn {2} {3}", stamp.id, stamp.name, stamp.localIncarn, message);
		Log.Assert(stamp.streamsSize >= -1);

		if (stamp.isDir)
		{
			if (!dirsToServer.Contains(stamp.id))
				dirsToServer.Add(stamp.id, stamp);
		}
		else if (stamp.streamsSize == -1)
		{
			if (!smallToServer.Contains(stamp.id))
				smallToServer.Add(stamp.id, stamp);
		}
		else if (stamp.streamsSize >= NodeChunk.MaxSize)
		{
			if (!largeToServer.Contains(stamp.id))
				largeToServer.Add(stamp.id, stamp);
		}
		else
		{
			if (!smallToServer.Contains(stamp.id))
				smallToServer.Add(stamp.id, stamp);
		}
	}

	void GetNodeFromServer(ref NodeStamp stamp, string message)
	{
		// TODO: deal with small files in pages, right now we just limit the
		// first small file page and then consider everything a large file
		Log.log.Debug("{0} {1} incarn {2} {3}", stamp.id, stamp.name, stamp.localIncarn, message);
		Log.Assert(stamp.streamsSize >= -1);
		if (stamp.isDir)
		{
			if(!dirsFromServer.Contains(stamp.id))
				dirsFromServer.Add(stamp.id, stamp);

		}
		else if (stamp.streamsSize == -1)
		{
			if (!smallFromServer.Contains(stamp.id))
				smallFromServer.Add(stamp.id, stamp);
		}
		else if (stamp.streamsSize >= NodeChunk.MaxSize)
		{
			if (!largeFromServer.Contains(stamp.id))
				largeFromServer.Add(stamp.id, stamp);
		}
		else
		{
			if (!smallFromServer.Contains(stamp.id))
				smallFromServer.Add(stamp.id, stamp);
		}
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
		string userID = collection.StoreReference.GetUserIDFromDomainID(collection.Domain);
		Access.Rights rights = ss.Start(userID);
		try
		{
			if (rights == Access.Rights.Deny)
			{
				Log.log.Error("Sync with collection {0} denied", collection.Name);
				moreWork = false;
				return;
			}

			moreWork = true;
			while (moreWork && ! stopping)
			{
				NodeStamp[] sstamps;
				NodeStamp[] cstamps;
		
				string clientCookie, serverCookie;
				bool gotServerChanges, gotClientChanges;
				ops.GetChangeLogCookies(out serverCookie, out clientCookie);
				gotServerChanges = ss.GetChangedNodeStamps(out sstamps, ref serverCookie);
				gotClientChanges = ops.GetChangedNodeStamps(out cstamps, ref clientCookie);
		
				if (gotServerChanges && gotClientChanges)
				{
					if (sstamps.Length != 0 || cstamps.Length != 0)
						moreWork = true;
					else
						moreWork = false;

					ProcessChangedNodeStamps(rights, sstamps, cstamps);
					ExecuteSync();
					ops.SetChangeLogCookies(serverCookie, clientCookie, !HadErrors);
				}
				else
				{
					sstamps =  ss.GetNodeStamps();
					if (sstamps == null)
					{
						Log.log.Error("Server Failure: could not get nodestamps");
						return;
					}
					cstamps =  ops.GetNodeStamps();
					BruteForceSync(rights, sstamps, cstamps);
					ExecuteSync();
					ops.SetChangeLogCookies(serverCookie, clientCookie, !HadErrors);
					moreWork = false;
				}
			}
		}
		finally
		{
			ss.Stop();
		}
	}
		

	/// <summary>
	/// 
	/// </summary>
	/// <param name="rights"></param>
	/// <param name="sstamps"></param>
	/// <param name="cstamps"></param>
	void BruteForceSync(Access.Rights rights, NodeStamp[] sstamps, NodeStamp[] cstamps)
	{
		int si = 0, ci = 0;
		int sCount = sstamps.Length, cCount = cstamps.Length;

		Log.log.Debug("Got {0} nodes in chunk from server, {1} from client", sCount, cCount);

		while (si < sCount || ci < cCount && !stopping)
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
					Log.log.Debug("{1} {0} has been killed or synced before or is RO, but is not on the server, just kill it locally",
						cstamps[ci].name, cstamps[ci].id);
					if (!killOnClient.Contains(cstamps[ci].id))
						killOnClient.Add(cstamps[ci].id, cstamps[ci]);
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
					//Log.Spew("server incarn {0}, client incarn {1}, client thinks server is {2}",
					//		sstamps[si].localIncarn, cstamps[ci].localIncarn, cstamps[ci].masterIncarn);
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
	}


	void ProcessChangedNodeStamps(Access.Rights rights, NodeStamp[] sstamps, NodeStamp[] cstamps)
	{
		for (int i = 0; i < sstamps.Length; ++i)
		{
			switch (sstamps[i].changeType)
			{
				case ChangeLogRecord.ChangeLogOp.Changed:
				case ChangeLogRecord.ChangeLogOp.Created:
				case ChangeLogRecord.ChangeLogOp.Renamed:
					// Make sure the node has been changed.
					Node oldNode = collection.GetNodeByID(sstamps[i].id);
					if (oldNode == null || oldNode.MasterIncarnation != sstamps[i].localIncarn)
					{
						GetNodeFromServer(ref sstamps[i], "exists on server but not client, get it");
					}
					break;
				case ChangeLogRecord.ChangeLogOp.Deleted:
					if (!killOnClient.Contains(sstamps[i].id))
						killOnClient.Add(sstamps[i].id, sstamps[i]);
					break;
			}
		}

		for (int i = 0; i < cstamps.Length; ++i)
		{
			switch (cstamps[i].changeType)
			{
				case ChangeLogRecord.ChangeLogOp.Changed:
				case ChangeLogRecord.ChangeLogOp.Created:
				case ChangeLogRecord.ChangeLogOp.Deleted:
				case ChangeLogRecord.ChangeLogOp.Renamed:
					if (cstamps[i].localIncarn != cstamps[i].masterIncarn)
						PutNodeToServer(ref cstamps[i], "is new on the client, send to server");
					break;
			}
		}
	}


	void ExecuteSync()
	{
		if (stopping)
			return;
		
		ProcessKillOnClient();
		ProcessDirsFromServer();
		ProcessSmallFromServer();
		ProcessDirsToServer();
		ProcessSmallToServer();	
		ProcessLargeToServer();
		ProcessLargeFromServer();
	}

	/// <summary>
	/// Deletes the Nodes in the killOnClient table.
	/// </summary>
	void ProcessKillOnClient()
	{
		// remove deleted nodes from client
		if (killOnClient.Count > 0 && ! stopping)
		{
			DictionaryEntry[] killList = new DictionaryEntry[killOnClient.Count];
			killOnClient.CopyTo(killList, 0);
			foreach (DictionaryEntry entry in killList)
			{
				try
				{
					NodeStamp ns = (NodeStamp)entry.Value;
					ops.DeleteNode(ns.id, true);
					killOnClient.Remove(entry.Key);
				}
				catch
				{
					// Try to delete the next node.
					HadErrors = true;
				}
			}
		}
	}

	void ProcessDirsFromServer()
	{
		NodeChunk[] updates = null;

		// get directories from the server.
		if (dirsFromServer.Count > 0 && ! stopping)
		{
			NodeStamp[] dirStamps = new NodeStamp[dirsFromServer.Count];
			dirsFromServer.Values.CopyTo(dirStamps, 0);

			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < dirStamps.Length)
			{
				int batchCount = dirStamps.Length - offset < BATCH_SIZE ? dirStamps.Length - offset : BATCH_SIZE;
				Nid[] ids = new Nid[batchCount];
				for (int i = 0; i < batchCount; ++i)
				{
					ids[i] = dirStamps[offset + i].id;
				}
				
				updates = ss.GetSmallNodes(ids);

				if (updates != null && updates.Length > 0)
				{
					for (int i = 0; i < updates.Length; ++i)
					{
						try
						{
							// Set the expected incarnation.
							NodeStamp ns = (NodeStamp)dirsFromServer[(Nid)updates[i].node.ID];
							updates[i].expectedIncarn = ns.masterIncarn;

							if (ops.PutSmallNode(updates[i]) == NodeStatus.Complete)
							{
								// This was successful remove the node from the hashtable.
								dirsFromServer.Remove((Nid)updates[i].node.ID);
							}
							else
							{
								// We had an error.
								HadErrors = true;
							}
						}
						catch
						{
							// Try to store the next node.
						}
					}
				}
				offset += batchCount;
			}
		}
	}

	void ProcessSmallFromServer()
	{
		NodeChunk[] updates = null;

		// get small nodes and files from server
		if (smallFromServer.Count > 0 && !stopping)
		{
			NodeStamp[] smallStamps = new NodeStamp[smallFromServer.Count];
			smallFromServer.Values.CopyTo(smallStamps, 0);

			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < smallStamps.Length)
			{
				int batchCount = smallStamps.Length - offset < BATCH_SIZE ? smallStamps.Length - offset : BATCH_SIZE;
				Nid[] ids = new Nid[batchCount];
				for (int i = 0; i < batchCount; ++i)
				{
					ids[i] = smallStamps[offset + i].id;
				}
				
				updates = ss.GetSmallNodes(ids);
			
				if (updates != null && updates.Length > 0)
				{
					for (int i = 0; i < updates.Length; ++i)
					{
						try
						{
							// Set the expected incarnation.
							NodeStamp ns = (NodeStamp)smallFromServer[(Nid)updates[i].node.ID];
							updates[i].expectedIncarn = ns.masterIncarn;

							if (ops.PutSmallNode(updates[i]) == NodeStatus.Complete)
							{
								// This was successful remove the node from the hashtable.
								smallFromServer.Remove((Nid)updates[i].node.ID);
							}
							else
							{
								// We had an error.
								HadErrors = true;
							}
						}
						catch
						{
							// Try to store the next node.
						}
					}
				}
				offset += batchCount;
			}
		}
	}

	void ProcessDirsToServer()
	{
		NodeChunk[] updates = null;

		// push up directories.
		if ((updates = ops.GetSmallNodes(dirsToServer)) != null)
		{
			// Now put the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < updates.Length)
			{
				int batchCount = updates.Length - offset < BATCH_SIZE ? updates.Length - offset : BATCH_SIZE;
				try
				{
					NodeChunk[] nodeChunks = new NodeChunk[batchCount];

					for (int i = 0; i < batchCount; ++i)
					{
						nodeChunks[i] = updates[offset + i];
					}
			
					RejectedNode[] rejects = ss.PutSmallNodes(nodeChunks);

					foreach (NodeChunk nc in nodeChunks)
					{
						if (stopping)
							return;
						bool updateIncarn = true;
						if (rejects != null)
						{
							foreach (RejectedNode reject in rejects)
							{
								if (reject.nid == nc.node.ID)
								{
									Log.log.Debug("skipping update of incarnation for small node {0} due to {1} on server",
										reject.nid, reject.status);
									updateIncarn = false;
									HadErrors = true;
									break;
								}
							}
						}
						if (updateIncarn == true)
						{
							if (collection.IsType(nc.node, NodeTypes.TombstoneType))
								ops.DeleteNode((Nid)nc.node.ID, false);
							else
								ops.UpdateIncarn((Nid)nc.node.ID, nc.node.LocalIncarnation);
							dirsToServer.Remove((Nid)nc.node.ID);
						}
					}
				}
				catch
				{
					// Continu getting the rest of the nodes.
				}
				offset += batchCount;
			}
		}
	}

	void ProcessSmallToServer()
	{
		NodeChunk[] updates = null;

		// push up small nodes and files to server
		if ((updates = ops.GetSmallNodes(smallToServer)) != null)
		{
			// Now put the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < updates.Length)
			{
				int batchCount = updates.Length - offset < BATCH_SIZE ? updates.Length - offset : BATCH_SIZE;
				try
				{
					NodeChunk[] nodeChunks = new NodeChunk[batchCount];

					for (int i = 0; i < batchCount; ++i)
					{
						nodeChunks[i] = updates[offset + i];
					}
			
					RejectedNode[] rejects = ss.PutSmallNodes(nodeChunks);

					foreach (NodeChunk nc in nodeChunks)
					{
						if (stopping)
							return;
						bool updateIncarn = true;
						if (rejects != null)
						{
							foreach (RejectedNode reject in rejects)
							{
								if (reject.nid == nc.node.ID)
								{
									Log.log.Debug("skipping update of incarnation for small node {0} due to {1} on server",
										reject.nid, reject.status);
									updateIncarn = false;
									HadErrors = true;
									break;
								}
							}
						}
						if (updateIncarn == true)
						{
							if (collection.IsType(nc.node, NodeTypes.TombstoneType))
								ops.DeleteNode((Nid)nc.node.ID, false);
							else
								ops.UpdateIncarn((Nid)nc.node.ID, nc.node.LocalIncarnation);
							smallToServer.Remove((Nid)nc.node.ID);
						}
					}
				}
				catch
				{
					// Get these next sync pass.
				}
				offset += batchCount;
			}
		}
	}

	void ProcessLargeToServer()
	{
		OutgoingNode outNode = new OutgoingNode(collection);
		
		// push up large files
		if (largeToServer.Count != 0 && !stopping)
		{
			NodeStamp[] largeStamps = new NodeStamp[largeToServer.Count];
			largeToServer.Values.CopyTo(largeStamps, 0);

			foreach (NodeStamp stamp in largeStamps)
			{
				try
				{
					if (stopping)
						return;
					Node node;

					if ((node = outNode.Start(stamp.id)) == null)
						continue;

					int totalSize;
					ForkChunk[] chunks = outNode.ReadChunks(NodeChunk.MaxSize, out totalSize);
					if (!ss.WriteLargeNode(node, chunks))
					{
						Log.log.Debug("Could not write large node {0}", node.Name);
						HadErrors = true;
						continue;
					}
					if (chunks == null || totalSize < NodeChunk.MaxSize)
					{
						chunks = null;
					}
					else
					{
						while (true)
						{
							chunks = outNode.ReadChunks(NodeChunk.MaxSize, out totalSize);
							if (chunks == null || totalSize < NodeChunk.MaxSize)
								break;
							ss.WriteLargeNode(chunks, 0, false);
						}
					}
					NodeStatus status = ss.WriteLargeNode(chunks, stamp.masterIncarn, true);
					if (status == NodeStatus.Complete || status == NodeStatus.FileNameConflict)
					{
						ops.UpdateIncarn((Nid)node.ID, node.LocalIncarnation);
						largeToServer.Remove((Nid)node.ID);
					}
					else
					{
						HadErrors = true;
						Log.log.Debug("skipping update of incarnation for large node {0} due to {1}", node.Name, status);
					}
				}
				catch
				{
					// We'll get this file net sync pass.
				}
			}
		}
	}

	void ProcessLargeFromServer()
	{
		IncomingNode inNode = new IncomingNode(collection, false);
		
		// get large files from server
		if (largeFromServer.Count != 0 && !stopping)
		{
			NodeStamp[] largeStamps = new NodeStamp[largeFromServer.Count];
			largeFromServer.Values.CopyTo(largeStamps, 0);

			foreach (NodeStamp stamp in largeStamps)
			{
				try
				{
					if (stopping)
						return;
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
					{
						Log.log.Debug("failed to update large node {0} from master, status {1}", stamp.name, status);
						HadErrors = true;
					}
					else
					{
						largeFromServer.Remove((Nid)stamp.id);
					}
				}
				catch
				{
					// We'll get this node next pass.
				}
			}
		}
	}
}

//===========================================================================
}
