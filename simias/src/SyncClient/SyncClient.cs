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
using System.Net;
using Simias;
using Simias.Storage;
using Simias.Service;
using Simias.Event;


namespace Simias.Sync
{
	/// <summary>
	/// Class that manages the synchronization for all collection.
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


		/// <summary>
		/// Called by The CollectionSyncClient when it is time to run another sync pass.
		/// </summary>
		/// <param name="collectionClient">The client that is ready to sync.</param>
		public void TimeToSync(object collectionClient)
		{
			lock (syncQueue)
			{
				// Queue the sync.
				syncQueue.Enqueue(collectionClient);
				queueEvent.Set();
			}
		}

		/// <summary>
		/// The main synchronization thread.
		/// </summary>
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
						log.Info("{0} : Starting Sync.", cClient);
						try
						{
							cClient.SyncNow();
							log.Info("{0} : Finished Sync.", cClient);
						}
						catch (Exception ex)
						{
							log.Debug(ex, "Finished Sync. Error =  {0}", ex.Message);
						}
						try
						{
							cClient.Reschedule();
						}
						catch { /* If we could not reschedule this collection will no longer sync. */};
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

		/// <summary>
		/// Resumes a paused sync.
		/// </summary>
		public void Resume()
		{
			foreach(CollectionSyncClient cClient in collections)
			{
				cClient.Reschedule();
			}
		}

		/// <summary>
		/// Pauses the synchronization service.
		/// </summary>
		public void Pause()
		{
			paused = true;
		}

		/// <summary>
		/// Used to send custom control messages to the service.
		/// Not used.
		/// </summary>
		/// <param name="message">The custom message.</param>
		/// <param name="data">The data of the message.</param>
		public void Custom(int message, string data)
		{
		}

		/// <summary>
		/// Called to stop the synchronization service.
		/// </summary>
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

		/// <summary>
		/// Called when a node is created.
		/// </summary>
		/// <param name="args">Arguments of the create.</param>
		private void storeEvents_NodeCreated(NodeEventArgs args)
		{
			// If the ID matched the Collection this is a collection.
			if (args.ID == args.Collection)
			{
				lock (collections)
				{
					// Add this to the collections to sync if not already added.
					if (!collections.Contains(args.ID))
					{
						collections.Add(args.ID, new CollectionSyncClient(args.ID, new TimerCallback(TimeToSync)));
					}
				}
			}
		}

		/// <summary>
		/// Called when a Node is deleted.
		/// </summary>
		/// <param name="args">Arguments of the delete.</param>
		private void storeEvents_NodeDeleted(NodeEventArgs args)
		{
			// If the ID matched the Collection this is a collection.
			if (args.ID == args.Collection)
			{
				lock (collections)
				{
					// Remove the CollectionSyncClient.
					CollectionSyncClient client = (CollectionSyncClient)collections[args.ID];
					if (client != null)
					{
						client.Stop();
						collections.Remove(args.ID);
						client.Dispose();
					}
				}
			}
		}
	}

	/// <summary>
	/// Class that implements the synchronization logic for a collection.
	/// </summary>
	public class CollectionSyncClient
	{
		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CollectionSyncClient));
		SimiasSyncService service;
		Store			store;
		SyncCollection	collection;
		byte[]			buffer;
		Timer			timer;
		TimerCallback	callback;
		Hashtable		killOnClient;
		Hashtable		nodesFromServer;
		Hashtable		dirsFromServer;
		Hashtable		filesFromServer;
		Hashtable		killOnServer;
		Hashtable		nodesToServer;
		Hashtable		dirsToServer;
		Hashtable		filesToServer;
		FileWatcher		fileMonitor;
		bool			stopping;
		Rights			rights;
		string			serverContext;
		string			clientContext;
		int				MAX_XFER_SIZE = 1024 * 64;
		static int		BATCH_SIZE = 50;
		bool			SyncComplete;
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
					int delay;
					if (collection.MasterIncarnation == 0) delay = 0;
					else delay = collection.Interval == Timeout.Infinite ? Timeout.Infinite : 0;
					timer = new Timer(callback, this, delay, Timeout.Infinite);
					break;
			}
		}

		~CollectionSyncClient()
		{
			Dispose(true);
		}

		/// <summary>
		/// Called to dispose the CollectionSyncClient.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// Called to dispose the CollectionSyncClient.
		/// </summary>
		/// <param name="inFinalizer">True if called from the finalizer.</param>
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

		/// <summary>
		/// Returns the name of the collection.
		/// </summary>
		/// <returns>Name of the collection.</returns>
		public override string ToString()
		{
			return collection.Name;
		}


		/// <summary>
		/// Initializes the instance.
		/// </summary>
		private void Initialize()
		{
			// If the master has not been created. Do it now.
			if (collection.CreateMaster)
			{
				new Simias.Domain.DomainAgent(Configuration.GetConfiguration()).CreateMaster(collection);
			}

			fileMonitor = new FileWatcher(collection, false);
			killOnClient = new Hashtable();
			nodesFromServer = new Hashtable();
			dirsFromServer = new Hashtable();
			filesFromServer = new Hashtable();
			killOnServer = new Hashtable();
			nodesToServer = new Hashtable();
			dirsToServer = new Hashtable();
			filesToServer = new Hashtable();
			serverContext = null;
			clientContext = null;
			buffer = new byte[MAX_XFER_SIZE];
		}

		/// <summary>
		/// Called to schedule a sync operation a the set sync Interval.
		/// </summary>
		internal void Reschedule()
		{
			if (!stopping)
				timer.Change(collection.Interval * 1000, Timeout.Infinite);
		}

		/// <summary>
		/// Called to stop this instance from sync-ing.
		/// </summary>
		internal void Stop()
		{
			timer.Dispose();
			stopping = true;
		}

		/// <summary>
		/// Called to synchronize this collection.
		/// </summary>
		internal void SyncNow()
		{
			SyncComplete = true;
			// Refresh the collection.
			collection.Refresh();
			
			// Sync the file system with the local store.
			fileMonitor.CheckForFileChanges();

			// We may have just created or deleted nodes wait for the events to settle.
			Thread.Sleep(500);

			// Setup the url to the server.
			service = new SimiasSyncService();
			service.CookieContainer = new CookieContainer();
			UriBuilder hostUrl = new UriBuilder(service.Url);
			hostUrl.Host = collection.MasterUrl.Host;
			// BUGBUG this need to be put back 
			if (collection.MasterUrl.Port != 0 && collection.MasterUrl.Port != 6436)
				hostUrl.Port = collection.MasterUrl.Port;
			service.Url = hostUrl.ToString();

			SyncNodeStamp[] sstamps;
			NodeStamp[]		cstamps;
			
			// Get the current sync state.
			GetChangeLogContext(out serverContext, out clientContext);
			bool gotClientChanges = GetChangedNodeStamps(out cstamps, ref clientContext);

			// Setup the SyncStartInfo.
			SyncStartInfo si = new SyncStartInfo();
			si.CollectionID = collection.ID;
			si.Context = serverContext;
			si.ChangesOnly = gotClientChanges;
			si.ClientHasChanges = (gotClientChanges && cstamps.Length == 0) ? false : true;
			
			// Start the Sync pass and save the rights.
			sstamps = service.Start(ref si, store.GetUserIDFromDomainID(collection.Domain));

			serverContext = si.Context;
			rights = si.Access;

			switch (si.Status)
			{
				case SyncColStatus.Busy:
					// BUGBUG. we may need to back off.
					// Re-queue this to run.
					callback(this);
					break;
					break;
				case SyncColStatus.NotFound:
					// The collection does not exist or we do not have rights.
					collection.Delete();
					// Delete the tombstone.
					collection.Delete();
					break;
				case SyncColStatus.NoWork:
					break;
				case SyncColStatus.Success:
				switch (rights)
					{
						case Rights.Deny:
							break;
						case Rights.Admin:
						case Rights.ReadOnly:
						case Rights.ReadWrite:
							try
							{
								// Now lets determine the files that need to be synced.
								if (si.ChangesOnly)
								{
									// We only need to look at the changed nodes.
									ProcessChangedNodeStamps(sstamps, cstamps);
								}
								else
								{
									// We don't have any state. So do a full sync.
									cstamps =  GetNodeStamps();
									ReconcileAllNodeStamps(sstamps, cstamps);
								}
								ExecuteSync();
							}
							finally
							{
								// Save the sync state.
								SetChangeLogContext(serverContext, clientContext, SyncComplete);
								// End the sync.
								service.Stop();
							}
							break;
					}
					break;
			}
		}

		/// <summary>
		/// Called to calculate the nodes that need to be synced.
		/// </summary>
		private void GetNodesToSync(SyncStartInfo si, SyncNodeStamp[] sstamps, NodeStamp[] cstamps)
		{
			
		}

		/// <summary>
		/// Set the new context strings that are used by ChangeLog.
		/// If persist is true save the contexts to the store.
		/// </summary>
		/// <param name="serverContext">The server context.</param>
		/// <param name="clientContext">The client context.</param>
		/// <param name="persist">Persist the changes if true.</param>
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

		/// <summary>
		/// Get the new context strings that are used by ChangeLog.
		/// </summary>
		/// <param name="serverContext">The server context.</param>
		/// <param name="clientContext">The client context.</param>
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
		/// <returns>false the call failed. The context is initialized.</returns>
		internal bool GetChangedNodeStamps(out NodeStamp[] nodes, ref string context)
		{
			log.Debug("GetChangedNodeStamps Start");
			EventContext eventContext;
			ArrayList changeList = null;
			ArrayList stampList = new ArrayList();

			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			nodes = null;
			bool more = true;
		
			try
			{
				// Read the cookie from the last sync and then get the changes since then.
				if (context != null)
				{
					eventContext = new EventContext(context);
					while (more)
					{
						more = logReader.GetEvents(eventContext, out changeList);
						foreach( ChangeLogRecord rec in changeList )
						{
							// Make sure the events are not for local only changes.
							if ((((NodeEventArgs.EventFlags)rec.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0) &&
								(rec.MasterRev != rec.SlaveRev))
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
	
		/// <summary>
		/// Using the change node stamps, determine what sync work needs to be done.
		/// </summary>
		/// <param name="sstamps">The server changes.</param>
		/// <param name="cstamps">The client changes.</param>
		private void ProcessChangedNodeStamps(SyncNodeStamp[] sstamps, NodeStamp[] cstamps)
		{
			for (int i = 0; i < sstamps.Length; ++i)
			{
				GetNodeFromServer(sstamps[i]);
			}

			for (int i = 0; i < cstamps.Length; ++i)
			{
				PutNodeToServer(cstamps[i]);
			}
		}

		/// <summary>
		/// Determins how this node should be retrieved from the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		void GetNodeFromServer(SyncNodeStamp stamp)
		{
			if (stamp.Operation == SyncOperation.Delete)
			{
				killOnClient[stamp.ID] = stamp.BaseType;
			}
			else
			{
				// If it is scheduled to be deleted remove from delete list.
				if (killOnClient.Contains(stamp.ID))
					killOnClient.Remove(stamp.ID);

				// Make sure the node has been changed.
				Node oldNode = collection.GetNodeByID(stamp.ID);
				if (oldNode == null || oldNode.MasterIncarnation != stamp.Incarnation)
				{
					if (stamp.BaseType == NodeTypes.FileNodeType || stamp.BaseType == NodeTypes.StoreFileNodeType)
					{
						// This is a file.
						filesFromServer[stamp.ID] = stamp.BaseType;
					}
					else if (stamp.BaseType == NodeTypes.DirNodeType)
					{
						// This node represents a directory.
						dirsFromServer[stamp.ID] = stamp.BaseType;
					}
					else
					{
						// This is a generic node.
						nodesFromServer[stamp.ID] = stamp.BaseType;
					}
				}
			}
		}

		/// <summary>
		/// Determins how this node should be sent to the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		void PutNodeToServer(NodeStamp stamp)
		{
			if (stamp.type == NodeTypes.TombstoneType)
			{
				killOnServer[stamp.id] = stamp.type;
			}
			else
			{
				// Remove from the delete list if it exists.
				if (killOnServer.Contains(stamp.id))
					killOnServer.Remove(stamp.id);

				if (stamp.masterIncarn != stamp.localIncarn)
				{
					if (stamp.type == NodeTypes.FileNodeType || stamp.type == NodeTypes.StoreFileNodeType)
					{
						// This node is a file.
						filesToServer[stamp.id] = stamp.type;
					}
					else if (stamp.type == NodeTypes.DirNodeType)
					{
						// This node is a directory.
						dirsToServer[stamp.id] = stamp.type;
					}
					else
					{
						// This is a generic node.
						nodesToServer[stamp.id] = stamp.type;
					}
				}
			}
		}

		/// <summary>
		/// Determines which nodes need to by synced.
		/// This is done by comparing all nodes on the server with all nodes on the client.
		/// </summary>
		/// <param name="sstamps">The server nodes.</param>
		/// <param name="cstamps">The client nodes.</param>
		void ReconcileAllNodeStamps(SyncNodeStamp[] sstamps, NodeStamp[] cstamps)
		{
			// Clear all the table because we are doing a full sync.
			killOnClient.Clear();
			nodesFromServer.Clear();
			dirsFromServer.Clear();
			filesFromServer.Clear();
			killOnServer.Clear();
			nodesToServer.Clear();
			dirsToServer.Clear();
			filesToServer.Clear();
		
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
						killOnClient[cStamp.id] = cStamp.type;
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
					if (cStamp.type == NodeTypes.TombstoneType)
					{
						// This node has been deleted on the client.
						killOnServer[cStamp.id] = cStamp.type;
					}
					else if (cStamp.localIncarn != cStamp.masterIncarn)
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

			// Now Get any nodes left, that are on the server but not on the client.
			foreach(SyncNodeStamp sStamp in tempTable.Values)
			{
				GetNodeFromServer(sStamp);
			}
		}

		/// <summary>
		/// Do the work that is needed to synchronize the client and server.
		/// </summary>
		void ExecuteSync()
		{
			ProcessKillOnClient();
			ProcessNodesFromServer();
			ProcessDirsFromServer();
			ProcessFilesFromServer();
			ProcessKillOnServer();
			ProcessNodesToServer();
			ProcessDirsToServer();
			ProcessFilesToServer();
		}

		/// <summary>
		/// Delete the nodes on the client.
		/// </summary>
		void ProcessKillOnClient()
		{
			// remove deleted nodes from client
			if (killOnClient.Count > 0)
			{
				string[] idList = new string[killOnClient.Count];
				killOnClient.Keys.CopyTo(idList, 0);
				foreach (string id in idList)
				{
					if (stopping)
					{
						SyncComplete = false;
						return;
					}
					try
					{
						Node node = collection.GetNodeByID(id);
						if (node == null)
						{
							log.Debug("Ignoring attempt to delete non-existent node {0}", id);
							killOnClient.Remove(id);
							continue;
						}

						log.Info("Deleting {0} on client", node.Name);
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

						// Now delete the tombstones.
						collection.Commit(deleted);

						killOnClient.Remove(id);
					}
					catch
					{
						// Try to delete the next node.
						SyncComplete = false;
					}
				}
			}
		}

		/// <summary>
		/// Copy the generic nodes from the server.
		/// </summary>
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
				while (offset < nodeIDs.Length)
				{
					if (stopping)
					{
						SyncComplete = false;
						return;
					}
					int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
					try
					{
						string[] batchIDs = new string[batchCount];
						Array.Copy(nodeIDs, offset, batchIDs, 0, batchCount);
						updates = service.GetNodes(batchIDs);
						StoreNodes(updates);
					}
					catch
					{
						SyncComplete = false;
					}
					offset += batchCount;
				}
			}
		}

		/// <summary>
		/// Save the nodes from the server in the local store.
		/// </summary>
		/// <param name="nodes"></param>
		public void StoreNodes(SyncNode [] nodes)
		{
			Node[]		commitList = new Node[nodes.Length];

			// Try to commit all the nodes at once.
			int i = 0;
			foreach (SyncNode sn in nodes)
			{
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(sn.node);
				Node node = Node.NodeFactory(store, xNode);
				log.Info("Updating {0} {1} from server", node.Name, node.Type);
				Import(node);
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
						if (node != null)
						{
							collection.Commit(node);
							nodesFromServer.Remove(node.ID);
						}
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
							SyncComplete = false;
						}
					}
					catch
					{
						// Handle any other errors.
						SyncComplete = false;
					}
				}
			}
		}

		/// <summary>
		/// Get the directory nodes from the server.
		/// </summary>
		void ProcessDirsFromServer()
		{
			SyncNode[] updates = null;

			// get small nodes and files from server
			if (dirsFromServer.Count > 0)
			{
				string[] nodeIDs = new string[dirsFromServer.Count];
				dirsFromServer.Keys.CopyTo(nodeIDs, 0);
				
				// Now get the nodes in groups of BATCH_SIZE.
				int offset = 0;
				while (offset < nodeIDs.Length)
				{
					if (stopping)
					{
						SyncComplete = false;
						return;
					}

					int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
					try
					{
						string[] batchIDs = new string[batchCount];
						Array.Copy(nodeIDs, offset, batchIDs, 0, batchCount);
					
						updates = service.GetDirs(batchIDs);

						foreach (SyncNode snode in updates)
						{
							StoreDir(snode);
						}
					}
					catch
					{
						SyncComplete = false;
					}
					offset += batchCount;
				}
			}
		}

		/// <summary>
		/// Called to import a node.
		/// </summary>
		/// <param name="node"></param>
		void Import(Node node)
		{
			collection.ImportNode(node, false, 0);
			node.IncarnationUpdate = node.LocalIncarnation;
		}

		/// <summary>
		/// Store the directory node in the local store also create the directory.
		/// </summary>
		/// <param name="snode">The node to store.</param>
		void StoreDir(SyncNode snode)
		{
			try
			{
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(snode.node);
				DirNode node = (DirNode)Node.NodeFactory(store, xNode);
				log.Info("Updating {0} {1} from server", node.Name, node.Type);
				Import(node);
			
				// Get the old node to see if the node was renamed.
				DirNode oldNode = collection.GetNodeByID(node.ID) as DirNode;
				string path;
				if (node.IsRoot)
				{
					path = oldNode.GetFullPath(collection);
				}
				else
				{
					path = node.GetFullPath(collection);
					if (oldNode != null)
					{
						// We already have this node look for a rename.
						string oldPath = oldNode.GetFullPath(collection);
						if (oldPath != path)
						{
							Directory.Move(oldPath, path);
						}
					}
				}

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				collection.Commit(node);
				dirsFromServer.Remove(node.ID);
			}
			catch 
			{
				SyncComplete = false;
			}
		}

		/// <summary>
		/// Get the file nodes from the server.
		/// </summary>
		void ProcessFilesFromServer()
		{
			if (filesFromServer.Count == 0)
				return;

			string[] nodeIDs = new string[filesFromServer.Count];
			filesFromServer.Keys.CopyTo(nodeIDs, 0);

			foreach (string nodeID in nodeIDs)
			{
				if (stopping)
				{
					SyncComplete = false;
					return;
				}
				// Get the node.
				SyncNode sn = service.GetFileNode(nodeID);
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(sn.node);
				Node node = new Node(xNode);
				Import(node);
				
				// Now get the file.
				string fPath = "temp";
				long offset = 0;
				FileStream stream = File.Create(fPath);
				int bytesRead;
				while ((bytesRead = service.Read(offset, buffer.Length, out buffer)) != 0)
				{
					stream.Write(buffer, 0, bytesRead);
					offset += bytesRead;
				}
			}
		}

		/// <summary>
		/// Delete nodes from the server.
		/// </summary>
		void ProcessKillOnServer()
		{
			// remove deleted nodes from server
			if (killOnServer.Count > 0 && ! stopping)
			{
				try
				{
					string[] idList = new string[killOnServer.Count];
					killOnServer.Keys.CopyTo(idList, 0);
					SyncNodeStatus[] nodeStatus = service.DeleteNodes(idList);
					foreach (SyncNodeStatus status in nodeStatus)
					{
						try
						{
							if (status.status == SyncStatus.Success)
							{
								Node node = collection.GetNodeByID(status.nodeID);
								if (node != null)
								{
									log.Info("Deleting {0} from server", node.Name);
									// Delete the tombstone.
									collection.Commit(collection.Delete(node));
								}
								killOnServer.Remove(status.nodeID);
							}
						}
						catch 
						{
							SyncComplete = false;
						}
					}
				}
				catch
				{
					SyncComplete = false;
				}
			}
		}

		/// <summary>
		/// Upload the nodes to the server.
		/// </summary>
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
			while (offset < nodeIDs.Length)
			{
				if (stopping)
				{
					SyncComplete = false;
					return;
				}
				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				for (int i = offset; i < offset + batchCount; ++ i)
				{
					Node node = collection.GetNodeByID(nodeIDs[i]);
					if (node != null)
					{
						log.Info("Updating {0} {1} to server", node.Name, node.Type);
						nodes[i - offset] = node;
						SyncNode snode = new SyncNode();
						snode.node = node.Properties.ToString(true);
						snode.expectedIncarn = node.MasterIncarnation;
						updates[i - offset] = snode;
					}
				}

				offset += batchCount;

				SyncNodeStatus[] nodeStatus = service.PutNodes(updates);
					
				for (int i = 0; i < nodes.Length; ++ i)
				{
					Node node = nodes[i];
					SyncNodeStatus status = nodeStatus[i];
					switch (status.status)
					{
						case SyncStatus.Success:
							node.SetMasterIncarnation(node.LocalIncarnation);
							collection.Commit(node);
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
							SyncComplete = false;
							break;
					}
				}
			}
		}

		void ProcessDirsToServer()
		{
			// get small nodes and files from server
			if (dirsToServer.Count == 0)
			{
				return;
			}
			string[] nodeIDs = new string[dirsToServer.Count];
			dirsToServer.Keys.CopyTo(nodeIDs, 0);
				
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
						log.Info("Updating {0} {1} to server", node.Name, node.Type);
						nodes[i - offset] = node;
						SyncNode snode = new SyncNode();
						snode.node = node.Properties.ToString(true);
						snode.expectedIncarn = node.MasterIncarnation;
						updates[i - offset] = snode;
					}
				}

				offset += batchCount;

				SyncNodeStatus[] nodeStatus = service.PutDirs(updates);
					
				for (int i = 0; i < nodes.Length; ++ i)
				{
					Node node = nodes[i];
					SyncNodeStatus status = nodeStatus[i];
					switch (status.status)
					{
						case SyncStatus.Success:
							node.SetMasterIncarnation(node.LocalIncarnation);
							collection.Commit(node);
							dirsToServer.Remove(node.ID);
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
							SyncComplete = false;
							break;
					}
				}
			}
		}

		void ProcessFilesToServer()
		{
		}
	}
}