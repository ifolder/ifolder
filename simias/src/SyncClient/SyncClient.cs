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
*  Author: Russ Young
* 
***********************************************************************/


using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using Simias;
using Simias.Storage;
using Simias.Service;
using Simias.Event;


namespace Simias.Sync
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class SyncClient : Simias.Service.IThreadService
	{
		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncClient));
		Configuration		conf;
		Store				store;
		Hashtable			collections;
		EventSubscriber		storeEvents;
		Queue				syncQueue;
		ManualResetEvent	queueEvent;
		Thread				syncThread;
		bool				shuttingDown;
		bool				paused;

		public static void Main()
		{
			SyncClient sc = new SyncClient();
			Configuration.CreateDefaultConfig(Environment.CurrentDirectory);
			sc.Start(Configuration.GetConfiguration());
			sc.syncThread.Join();
		}

		public SyncClient()
		{
		}

		public void TimeToSync(object collectionClient)
		{
			lock (syncQueue)
			{
				syncQueue.Enqueue(collectionClient);
				queueEvent.Set();
			}
		}

		private void StartSync()
		{
			while (!shuttingDown)
			{
				// Wait for something to be added to the queue.
				queueEvent.WaitOne();

				// Now loop until the queue is emptied.
				while (true)
				{
					CollectionSyncClient cClient;
					lock (syncQueue)
					{
						if (syncQueue.Count == 0)
						{
							queueEvent.Reset();
							break;
						}

						cClient = (CollectionSyncClient)syncQueue.Dequeue();
					}
					if (!shuttingDown || !paused)
					{
						// Sync this collection now.
						log.Info("Starting Sync for {0}.", cClient);
						cClient.SyncNow();
						log.Info("Finished Sync for {0}.", cClient);
						cClient.Reschedule();
					}
				}
			}
		}

		#region IThreadService Members

		/// <summary>
		/// Starts the Sync client.
		/// </summary>
		/// <param name="conf">The configuration object to use.</param>
		public void Start(Configuration conf)
		{
			shuttingDown = false;
			paused = false;
			this.conf = conf;
			store = Store.GetStore();

			// Subscribe for node events so that we can add new CollectionSyncClients.
			storeEvents = new EventSubscriber();
			storeEvents.NodeCreated +=new NodeEventHandler(storeEvents_NodeCreated);
			storeEvents.NodeDeleted +=new NodeEventHandler(storeEvents_NodeDeleted);

			syncQueue = new Queue();
			queueEvent = new ManualResetEvent(false);
			collections = new Hashtable();

			lock (collections)
			{
				foreach(ShallowNode sn in store)
				{
					collections.Add(sn.ID, new CollectionSyncClient(sn.ID, new TimerCallback(TimeToSync)));
				}
			}

			// Start the Sync Thread.
			syncThread = new Thread(new ThreadStart(StartSync));
			syncThread.Start();
		}

		public void Resume()
		{
			foreach(CollectionSyncClient cClient in collections)
			{
				cClient.Reschedule();
			}
		}

		public void Pause()
		{
			paused = true;
		}

		public void Custom(int message, string data)
		{
		}

		public void Stop()
		{
			shuttingDown = true;
			lock (syncQueue)
			{
				queueEvent.Set();
			}
			syncThread.Join();
			storeEvents.Dispose();
		}

		#endregion

		private void storeEvents_NodeCreated(NodeEventArgs args)
		{
			if (args.ID == args.Collection)
			{
				lock (collections)
				{
					if (!collections.Contains(args.ID))
					{
						collections.Add(args.ID, new CollectionSyncClient(args.ID, new TimerCallback(TimeToSync)));
					}
				}
			}
		}

		private void storeEvents_NodeDeleted(NodeEventArgs args)
		{
			if (args.ID == args.Collection)
			{
				lock (collections)
				{
					collections.Remove(args.ID);
				}
			}
		}
	}

	public class CollectionSyncClient
	{
		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CollectionSyncClient));
		SimiasSyncService service;
		Store			store;
		SyncCollection	collection;
		Timer			timer;
		TimerCallback	callback;
		Hashtable		nodesFromServer;
		Hashtable		filesFromServer;
		Hashtable		nodesToServer;
		Hashtable		filesToServer;
		Hashtable		killOnClient;
		FileWatcher		fileMonitor;
		bool			stopping;
		Rights			rights;
		string			serverContext;
		string			clientContext;
		static int		BATCH_SIZE = 10;
		bool			HadErrors;
		private const string	ServerCLContextProp = "ServerCLContext";
		private const string	ClientCLContextProp = "ClientCLContext";
	
		
		/// <summary>
		/// Construct a new CollectionSyncClient.
		/// </summary>
		/// <param name="nid">The node ID.</param>
		/// <param name="callback">Callback that is called when collection should be synced.</param>
		internal CollectionSyncClient(string nid, TimerCallback callback)
		{
			store = Store.GetStore();
			collection = new SyncCollection(store.GetCollectionByID(nid));
			this.callback = callback;
			stopping = false;

			switch(collection.Role)
			{
				case SyncCollectionRoles.Master:
				case SyncCollectionRoles.Local:
				default:
					timer = new Timer(callback, this, Timeout.Infinite, Timeout.Infinite);				
					break;

				case SyncCollectionRoles.Slave:
					Initialize();
					int delay = collection.Interval == Timeout.Infinite ? Timeout.Infinite : 0;
					timer = new Timer(callback, this, delay, Timeout.Infinite);
					break;
			}
		}

		~CollectionSyncClient()
		{
			Dispose(true);
		}

		public void Dispose()
		{
			Dispose(false);
		}

		private void Dispose(bool inFinalizer)
		{
			if (!inFinalizer)
			{
				GC.SuppressFinalize(this);
			}
			if (timer != null)
			{
				timer.Dispose();
				timer = null;
			}
		}

		private void Initialize()
		{
			fileMonitor = new FileWatcher(collection, false);
			nodesFromServer = new Hashtable();
			filesFromServer = new Hashtable();
			nodesToServer = new Hashtable();
			filesToServer = new Hashtable();
			killOnClient = new Hashtable();
			serverContext = null;
			clientContext = null;
		}

		internal void Reschedule()
		{
			timer.Change(collection.Interval, Timeout.Infinite);
		}

		internal void Stop()
		{
			stopping = true;
		}

		internal void SyncNow()
		{
			HadErrors = false;
			// Refresh the collection.
			collection.Refresh();
			
			// Sync the file system with the local store.
			fileMonitor.CheckForFileChanges();

			// Setup the url to the server.
			service = new SimiasSyncService();
			UriBuilder hostUrl = new UriBuilder(service.Url);
			hostUrl.Host = collection.MasterUrl.Host;
			if (collection.MasterUrl.Port != 0)
				hostUrl.Port = collection.MasterUrl.Port;
			service.Url = hostUrl.ToString();

			rights = service.Start(collection.ID, collection.StoreReference.GetUserIDFromDomainID(collection.Domain));
			
			// Now lets determine the files that need to be synced.
			GetNodesToSync();

			service.Stop();
		}

		private void GetNodesToSync()
		{
			SyncNodeStamp[] sstamps;
			NodeStamp[] cstamps;
			bool more;
			GetChangeLogContext(out serverContext, out clientContext);
			bool gotServerChanges = service.GetChangedNodeStamps(ref serverContext, out sstamps, out more);
			bool gotClientChanges = GetChangedNodeStamps(out cstamps, ref clientContext, out more);

			if (gotServerChanges && gotClientChanges)
			{
				ProcessChangedNodeStamps(sstamps, cstamps);
			}
			else
			{
				sstamps =  service.GetAllNodeStamps();
				if (sstamps == null)
				{
					log.Debug("Server Failure: could not get server nodestamps");
					return;
				}
				cstamps =  GetNodeStamps();
				ReconcileAllNodeStamps(sstamps, cstamps);
			}
			//ExecuteSync();
			SetChangeLogContext(serverContext, clientContext, !HadErrors);
		}

		/// <summary>
		/// Set the cookie for the changelog.
		/// Used to get the next set of changes.
		/// </summary>
		internal void SetChangeLogContext(string serverContext, string clientContext, bool persist)
		{
			this.serverContext = serverContext;
			this.clientContext = clientContext;
	
			if (persist)
			{
				if (serverContext != null)
				{
					Property sc = new Property(ServerCLContextProp, serverContext);
					sc.LocalProperty = true;
					collection.Properties.ModifyProperty(sc);
				}
				if (clientContext != null)
				{
					Property cc = new Property(ClientCLContextProp, clientContext);
					cc.LocalProperty = true;
					collection.Properties.ModifyProperty(cc);
				}
				collection.Commit();
			}
		}

		private void GetChangeLogContext(out string serverContext, out string clientContext)
		{
			if (this.serverContext == null)
			{
				Property sc = collection.Properties.GetSingleProperty(ServerCLContextProp);
				if (sc != null)
				{
					this.serverContext = sc.Value.ToString();
				}
			}
			if (this.clientContext == null)
			{
				Property cc = collection.Properties.GetSingleProperty(ClientCLContextProp);
				if (cc != null)
				{
					this.clientContext = cc.Value.ToString();
				}
			}
			serverContext = this.serverContext;
			clientContext = this.clientContext;
		}

		/// <summary>
		/// Returns information about all of the nodes in the collection.
		/// The nodes can be used to determine what nodes need to be synced.
		/// </summary>
		/// <returns>Array of NodeStamps</returns>
		private NodeStamp[] GetNodeStamps()
		{
			log.Debug("GetNodeStamps start");
			ArrayList stampList = new ArrayList();
			foreach (ShallowNode sn in collection)
			{
				Node node;
				try
				{
					node = new Node(collection, sn);
					if (collection.HasCollisions(node))
						continue;
					NodeStamp stamp = new NodeStamp();
					stamp.localIncarn = node.LocalIncarnation;
					stamp.masterIncarn = node.MasterIncarnation;
					stamp.id = node.ID;
					stamp.type = node.Type;
					stamp.changeType = ChangeLogRecord.ChangeLogOp.Unknown;
					stampList.Add(stamp);
				}
				catch (Storage.DoesNotExistException)
				{
					log.Debug("Node: Name:{0} ID:{1} Type:{2} no longer exists.", sn.Name, sn.ID, sn.Type);
					continue;
				}
			}
			return (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
		}
			
		/// <summary>
		/// Get the changes from the change log.
		/// </summary>
		/// <param name="nodes">returns the list of changes.</param>
		/// <param name="context">The context handed back from the last call.</param>
		/// <returns>false the call failed.</returns>
		internal bool GetChangedNodeStamps(out NodeStamp[] nodes, ref string context, out bool more)
		{
			log.Debug("GetChangedNodeStamps Start");
			EventContext eventContext;
			ArrayList changeList = null;
			ArrayList stampList = new ArrayList();

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			nodes = null;
			more = false;
		
			try
			{
				// Read the cookie from the last sync and then get the changes since then.
				if (context != null)
				{
					eventContext = new EventContext(context);
					more = logReader.GetEvents(eventContext, out changeList);
					foreach( ChangeLogRecord rec in changeList )
					{
						// Make sure the events are not for local only changes.
						if (((NodeEventArgs.EventFlags)rec.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
						{
							NodeStamp stamp = new NodeStamp();
							stamp.localIncarn = rec.SlaveRev;
							stamp.masterIncarn = rec.MasterRev;
							stamp.id = rec.EventID;
							stamp.changeType = rec.Operation;
							stamp.type = rec.Type.ToString();
							stampList.Add(stamp);
						}
					}
					log.Debug("Found {0} changed nodes.", stampList.Count);
					nodes = (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
					context = eventContext.ToString();
					return true;
				}
			}
			catch
			{
			}
			// The cookie is invalid.  Get a valid cookie and save it for the next sync.
			eventContext = logReader.GetEventContext();
			if (eventContext != null)
				context = eventContext.ToString();
			return false;
		}
	
		void ProcessChangedNodeStamps(SyncNodeStamp[] sstamps, NodeStamp[] cstamps)
		{
			for (int i = 0; i < sstamps.Length; ++i)
			{
				switch (sstamps[i].ChangeType)
				{
					case SyncChangeType.Changed:
					case SyncChangeType.Created:
					case SyncChangeType.Renamed:
						// Make sure the node has been changed.
						Node oldNode = collection.GetNodeByID(sstamps[i].ID);
						if (oldNode == null || oldNode.MasterIncarnation != sstamps[i].Incarnation)
						{
							GetNodeFromServer(sstamps[i]);
						}
						break;
					case SyncChangeType.Deleted:
						if (!killOnClient.Contains(sstamps[i].ID))
							killOnClient.Add(sstamps[i].ID, sstamps[i].BaseType);
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
							PutNodeToServer(cstamps[i]);
						break;
				}
			}
		}

		void GetNodeFromServer(SyncNodeStamp stamp)
		{
			if (stamp.BaseType == NodeTypes.BaseFileNodeType)
			{
				if (!filesFromServer.Contains(stamp.ID))
					filesFromServer.Add(stamp.ID, stamp.BaseType);
			}
			else
			{
				if (!nodesFromServer.Contains(stamp.ID))
				{
					nodesFromServer.Add(stamp.ID, stamp.BaseType);
				}
			}
		}

		void PutNodeToServer(NodeStamp stamp)
		{
			if (stamp.masterIncarn == stamp.localIncarn)
			{
				// This node has not changed.
				return;
			}
			if (stamp.type == NodeTypes.BaseFileNodeType)
			{
				if (!filesToServer.Contains(stamp.id))
					filesToServer.Add(stamp.id, stamp.type);
			}
			else
			{
				if (!nodesToServer.Contains(stamp.id))
				{
					nodesToServer.Add(stamp.id, stamp.type);
				}
			}
		}

		void ReconcileAllNodeStamps(SyncNodeStamp[] sstamps, NodeStamp[] cstamps)
		{
			int sCount = sstamps.Length, cCount = cstamps.Length;
			Hashtable tempTable = new Hashtable();

			log.Debug("Server has {0} nodes. Client has {1} nodes", sCount, cCount);

			// Add all of the server nodes to the hashtable and then we can reconcile them
			// against the client nodes.
			foreach (SyncNodeStamp sStamp in sstamps)
			{
				tempTable.Add(sStamp.ID, sStamp);
			}

			foreach (NodeStamp cStamp in cstamps)
			{
				SyncNodeStamp sStamp = (SyncNodeStamp)tempTable[cStamp.id];
				if (sStamp == null)
				{
					// If the Master Incarnation is not 0 then this node has been deleted on the server.
					if (cStamp.masterIncarn != 0)
					{
						if (!killOnClient.Contains(cStamp.id))
							killOnClient.Add(cStamp.id, cStamp.type);
					}
					else
					{
						// The node is on the client but not the server send it to the server.
						PutNodeToServer(cStamp);
					}
				}
				else
				{
					// The node is on both the server and the client.  Check which way the node
					// should go.
					if (cStamp.localIncarn != cStamp.masterIncarn)
					{
						// The file has been changed locally if the master is correct, push this file.
						if (cStamp.masterIncarn == sStamp.Incarnation)
						{
							PutNodeToServer(cStamp);
						}
						else
						{
							// This will be a conflict get the servers latest revision.
							GetNodeFromServer(sStamp);
						}
					}
					else
					{
						// The node has not been changed locally see if we need to get the node.
						if (cStamp.masterIncarn != sStamp.Incarnation)
						{
							GetNodeFromServer(sStamp);
						}
					}
					tempTable.Remove(cStamp.id);
				}
			}

			// Now any nodes left are on the server but not on the client.
			foreach(SyncNodeStamp sStamp in tempTable)
			{
				GetNodeFromServer(sStamp);
			}
		}

		void ExecuteSync()
		{
			ProcessKillOnClient();
			ProcessNodesFromServer();
			//ProcessFilesFromServer();
			ProcessNodesToServer();
			//ProcessFilesToServer();
		}

		void ProcessKillOnClient()
		{
			// remove deleted nodes from client
			if (killOnClient.Count > 0 && ! stopping)
			{
				string[] idList = new string[killOnClient.Count];
				killOnClient.Keys.CopyTo(idList, 0);
				foreach (string id in idList)
				{
					try
					{
						Node node = collection.GetNodeByID(id);
						if (node == null)
						{
							log.Debug("Ignoring attempt to delete non-existent node {0}", id);
							continue;
						}

						log.Info("Deleting {0}", node.Name);
						// If this is a collision node then delete the collision file.
						if (collection.HasCollisions(node))
						{
							// BUGBUG what about an update conflict.
							Conflict conflict = new Conflict(collection, node);
							if (conflict.IsFileNameConflict)
							{
								File.Delete(conflict.FileNameConflictPath);
							}
						}
						else
						{
							BaseFileNode bfn = node as BaseFileNode;
							if (bfn != null)
								File.Delete(bfn.GetFullPath(collection));
							else
							{
								DirNode dn = node as DirNode;
								if (dn != null)
									Directory.Delete(dn.GetFullPath(collection), true);
							}
						}
						
						// Do a deep delete.
						Node[] deleted = collection.Delete(node, PropertyTags.Parent);
						collection.Commit(deleted);

						killOnClient.Remove(id);
					}
					catch
					{
						// Try to delete the next node.
						HadErrors = true;
					}
				}
			}
		}

		void ProcessNodesFromServer()
		{
			SyncNode[] updates = null;

			// get small nodes and files from server
			if (nodesFromServer.Count > 0)
			{
				string[] nodeIDs = new string[nodesFromServer.Count];
				nodesFromServer.Keys.CopyTo(nodeIDs, 0);
				
				// Now get the nodes in groups of BATCH_SIZE.
				int offset = 0;
				while (offset < nodeIDs.Length && !stopping)
				{
					int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
					string[] batchIDs = new string[batchCount];
					Array.Copy(nodeIDs, offset, batchIDs, 0, batchCount);
					offset += batchCount;

					updates = service.GetNonFileNodes(batchIDs);

					StoreNodes(updates);
				}
			}
		}

		public void StoreNodes(SyncNode [] nodes)
		{
			Node[]		commitList = new Node[nodes.Length];

			// Try to commit all the nodes at once.
			int i = 0;
			foreach (SyncNode sn in nodes)
			{
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(sn.node);
				Node node = new Node(xNode);
				collection.ImportNode(node, true, 0);
				commitList[i++] = node;
			}
			try
			{
				collection.Commit(commitList);
				foreach ( Node node in commitList)
				{
					nodesFromServer.Remove(node.ID);
				}
			}
			catch
			{
				// If we get here we need to try to commit the nodes one at a time.
				foreach (Node node in commitList)
				{
					try
					{
						collection.Commit(node);
						nodesFromServer.Remove(node.ID);
					}
					catch (CollisionException)
					{
						try
						{
							// The current node failed because of a collision.
							Node cNode = collection.CreateCollision(node, false);
							collection.Commit(cNode);
							nodesFromServer.Remove(cNode.ID);
						}
						catch
						{
							HadErrors = true;
						}
					}
					catch
					{
						// Handle any other errors.
						HadErrors = true;
					}
				}
			}
		}

		void ProcessNodesToServer()
		{
			// get small nodes and files from server
			if (nodesToServer.Count == 0)
			{
				return;
			}
			string[] nodeIDs = new string[nodesToServer.Count];
			nodesToServer.Keys.CopyTo(nodeIDs, 0);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length && !stopping)
			{
				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				for (int i = offset; i < offset + batchCount; ++ i)
				{
					Node node = collection.GetNodeByID(nodeIDs[i]);
					if (node != null)
					{
						nodes[i - offset] = node;
						SyncNode snode = new SyncNode();
						snode.node = node.Properties.ToString(true);
						snode.expectedIncarn = node.MasterIncarnation;
						updates[i - offset] = snode;
					}
				}

				offset += batchCount;

				SyncNodeStatus[] nodeStatus = service.PutNonFileNodes(updates);
					
				for (int i = 0; i < nodes.Length; ++ i)
				{
					Node node = nodes[i];
					SyncNodeStatus status = nodeStatus[i];
					switch (status.status)
					{
						case SyncStatus.Success:
							if (collection.IsType(node, NodeTypes.TombstoneType))
							{
								collection.Commit(collection.Delete(node));
							}
							else
							{
								node.SetMasterIncarnation(node.LocalIncarnation);
								collection.Commit(node);
							}
							nodesToServer.Remove(node.ID);
							break;
						case SyncStatus.UpdateConflict:
						case SyncStatus.FileNameConflict:
							// The file has been changed on the server lets get it next pass.
							log.Debug("Skipping update of node {0} due to {1} on server",
								status.nodeID, status.status);
									
							nodesFromServer.Add(node.ID, node.Type);
							nodesToServer.Remove(node.ID);
							break;
						default:
							log.Debug("Skipping update of node {0} due to {1} on server",
								status.nodeID, status.status);
							HadErrors = true;
							break;
					}
				}
			}
		}
	}
}

	/*
	/// <summary>
	/// client side top level class for SynkerA style sychronization
	/// </summary>
	public class SynkerWorkerA: SyncCollectionWorker
	{
		//TODO: why is the base master and slave not protected instead of private?
		SynkerServiceA ss;
		FileWatcher	fileWatcher;
	
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
		bool		stopping;

		/// <summary>
		/// public constructor which accepts real or proxy objects specifying master and collection
		/// </summary>
		public SynkerWorkerA(SyncCollection collection): base(collection)
		{
			ops = new SyncOps(collection, false);
			fileWatcher = new FileWatcher(collection, false);
		}

		/// <summary>
		/// used to perform one synchronization pass on one collection
		/// </summary>
		public override void DoSyncWork(SyncCollectionService service)
		{
			// save service
			ss = (SynkerServiceA)service;
			HadErrors = false;

			// Get a fresh copy of the collection.
			collection.Refresh();

			// Run the dredger
			fileWatcher.CheckForFileChanges();

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

		
		
		static string[] MoveIdsToArray(ArrayList idList)
		{
			string[] ids = (string[])idList.ToArray(typeof(string));
			idList.Clear();
			return ids;
		}

		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rights"></param>
		/// <param name="sstamps"></param>
		/// <param name="cstamps"></param>
		
		

		/// <summary>
		/// Deletes the Nodes in the killOnClient table.
		/// </summary>
		

		void ProcessDirsFromServer()
		{
			ProcessSmallFromServer(dirsFromServer);
		}
	
		void ProcessSmallFromServer()
		{
			ProcessSmallFromServer(smallFromServer);
		}

		

		void ProcessDirsToServer()
		{
			// push up directories.
			ProcessSmallToServer(dirsToServer);
		}
	
		void ProcessSmallToServer()
		{
			// push up small nodes and files to server
			ProcessSmallToServer(smallToServer);
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
						{
							largeToServer.Remove(node.ID);
							continue;
						}

						int totalSize;
						byte[] data = outNode.ReadChunk(NodeChunk.MaxSize, out totalSize);
						if (!ss.WriteLargeNode(node, data))
						{
							Log.log.Debug("Could not write large node {0}", node.Name);
							HadErrors = true;
							continue;
						}

						while (true)
						{
							data = outNode.ReadChunk(NodeChunk.MaxSize, out totalSize);
							if (data != null && totalSize >= NodeChunk.MaxSize)
								ss.WriteLargeNode(data, 0, false);
							else
								break;
						}

						NodeStatus status = ss.WriteLargeNode(data, stamp.masterIncarn, true);
						if (status == NodeStatus.Complete || status == NodeStatus.FileNameConflict)
						{
							ops.UpdateIncarn(node.ID, node.LocalIncarnation);
							largeToServer.Remove(node.ID);
						}
						else
						{
							HadErrors = true;
							Log.log.Debug("skipping update of incarnation for large node {0} due to {1}", node.Name, status);
						}
					}
					catch
					{
						// Ignore: We'll get this file next sync pass.
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
						inNode.BlowChunk(nc.data);
						while (nc.totalSize >= NodeChunk.MaxSize)
						{
							nc.data = ss.ReadLargeNode(NodeChunk.MaxSize);
							inNode.BlowChunk(nc.data);
							nc.totalSize = nc.data.Length;
						}
						NodeStatus status = inNode.Complete(stamp.masterIncarn);
						if (status == NodeStatus.Complete || 
							status == NodeStatus.FileNameConflict ||
							status == NodeStatus.UpdateConflict)
						{
							largeFromServer.Remove(nc.node.ID);
						}
						else
						{
							Log.log.Debug("failed to update large node {0} from master, status {1}", nc.node.Name, status);
							HadErrors = true;
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

*/