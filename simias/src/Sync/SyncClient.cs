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
using Simias.Client;
using Simias.Client.Event;


namespace Simias.Sync.Client
{
	#region NodeStamp

	/// <summary>
	/// struct to represent the minimal information that the sync code would need
	/// to know about a node to determine if it needs to be synced.
	/// </summary>
	internal class NodeStamp: SyncNodeStamp, IComparable
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		internal NodeStamp(Node node)
		{
			this.ID = node.ID;
			this.LocalIncarnation = node.LocalIncarnation;
			this.MasterIncarnation = node.MasterIncarnation;
			this.BaseType = node.Type;
			this.Operation = SyncOperation.Unknown;
		}

		internal NodeStamp(ChangeLogRecord record)
		{
			this.ID = record.EventID;
			this.LocalIncarnation = record.SlaveRev;
			this.MasterIncarnation = record.MasterRev;
			this.BaseType = record.Type.ToString();
			switch (record.Operation)
			{
				case ChangeLogRecord.ChangeLogOp.Changed:
					this.Operation = SyncOperation.Change;
					break;
				case ChangeLogRecord.ChangeLogOp.Created:
					this.Operation = SyncOperation.Create;
					break;
				case ChangeLogRecord.ChangeLogOp.Deleted:
					this.Operation = SyncOperation.Delete;
					break;
				case ChangeLogRecord.ChangeLogOp.Renamed:
					this.Operation = SyncOperation.Rename;
					break;
				default:
					this.Operation = SyncOperation.Unknown;
					break;
			}
		}
		
		/// <summary> implement some convenient operator overloads </summary>
		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is NodeStamp))
				throw new ArgumentException("object is not NodeStamp");
			return ID.CompareTo(((NodeStamp)obj).ID);
		}
	}

	#endregion

	#region SyncClient

	/// <summary>
	/// Class that manages the synchronization for all collection.
	/// </summary>
	public class SyncClient : Simias.Service.IThreadService
	{
		#region Fields

		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncClient));
//		Configuration		conf;
		Store				store;
		static Hashtable	collections;
		EventSubscriber		storeEvents;
		Queue				syncQueue;
		ManualResetEvent	queueEvent;
		Thread				syncThread;
		bool				shuttingDown;
		bool				paused;

		#endregion

		#region public methods.

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
		/// Returns the number of bytes to sync to the server.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <param name="fileCount">Returns the number of nodes to sync to the server.</param>
		public static void GetCountToSync(string collectionID, out uint fileCount)
		{
			CollectionSyncClient sc;
			lock (collections)
			{
				sc = (CollectionSyncClient)collections[collectionID];
			}
			if (sc != null)
				sc.GetSyncCount(out fileCount);
			else
			{
				fileCount = 0;
			}
		}

		#endregion

		#region private methods.

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

		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the Sync client.
		/// </summary>
		/// <param name="conf">The configuration object to use.</param>
		public void Start(Configuration conf)
		{
			shuttingDown = false;
			paused = false;
//			this.conf = conf;
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
			syncThread.Priority = ThreadPriority.BelowNormal;
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
	}

	#endregion

	#region CollectionSyncClient

	/// <summary>
	/// Class that implements the synchronization logic for a collection.
	/// </summary>
	public class CollectionSyncClient
	{
		#region fields

		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(CollectionSyncClient));
		EventPublisher	eventPublisher = new EventPublisher();
		SimiasSyncService	service;
		Store			store;
		SyncCollection	collection;
		bool			queuedChanges;
		SyncColStatus	serverStatus;
//		byte[]			buffer;
		Timer			timer;
		TimerCallback	callback;
		Hashtable		DeleteOnClient;
		Hashtable		nodesFromServer;
		Hashtable		dirsFromServer;
		Hashtable		filesFromServer;
		Hashtable		DeleteOnServer;
		Hashtable		nodesToServer;
		Hashtable		dirsToServer;
		Hashtable		filesToServer;
		FileWatcher		fileMonitor;
		bool			stopping;
		Rights			rights;
		string			serverContext;
		string			clientContext;
//		int				MAX_XFER_SIZE = 1024 * 64;
		static int		BATCH_SIZE = 50;
		private const string	ServerCLContextProp = "ServerCLContext";
		private const string	ClientCLContextProp = "ClientCLContext";

		#endregion
	
		#region Constructor / Finalizer
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

		/// <summary>
		/// Finalizer.
		/// </summary>
		~CollectionSyncClient()
		{
			Dispose(true);
		}

		#endregion

		#region public Methods.

		/// <summary>
		/// Returns the name of the collection.
		/// </summary>
		/// <returns>Name of the collection.</returns>
		public override string ToString()
		{
			return collection.Name;
		}

		#endregion

		#region internal Methods.

		/// <summary>
		/// Called to dispose the CollectionSyncClient.
		/// </summary>
		internal void Dispose()
		{
			Dispose(false);
		}

		/// <summary>
		/// Called to schedule a sync operation a the set sync Interval.
		/// </summary>
		internal void Reschedule()
		{
			if (!stopping)
			{
				int seconds = collection.Interval;
				if (serverStatus == SyncColStatus.Busy)
				{
					// Reschedule to sync within 1/12 of the scheduled sync time, but no less than 2 seconds.
					seconds = new Random().Next(seconds / 12) + 2;
				}
				timer.Change(seconds * 1000, Timeout.Infinite);
			}
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
		/// Get the number of bytes to sync.
		/// </summary>
		/// <param name="fileCount">Returns the number of files to be synced.</param>
		internal void GetSyncCount(out uint fileCount)
		{
			fileCount = (uint)nodesToServer.Count;
			fileCount += (uint)dirsToServer.Count;
			fileCount += (uint)filesToServer.Count;
		}

		/// <summary>
		/// Called to synchronize this collection.
		/// </summary>
		internal void SyncNow()
		{
			lock (this)
			{
				queuedChanges = false;
				serverStatus = SyncColStatus.Success;
				// Refresh the collection.
				collection.Refresh();
			
				// Sync the file system with the local store.
				fileMonitor.CheckForFileChanges();

				// We may have just created or deleted nodes wait for the events to settle.
				Thread.Sleep(500);

				// Setup the url to the server.
				service = new SimiasSyncService();
				service.Url = collection.MasterUrl.ToString().TrimEnd('/') + service.Url.Substring(service.Url.LastIndexOf('/'));
				service.CookieContainer = new CookieContainer();
				
				SyncNodeStamp[] sstamps;
				NodeStamp[]		cstamps;
			
				// Get the current sync state.
				GetChangeLogContext(out serverContext, out clientContext);
				bool gotClientChanges = GetChangedNodeStamps(out cstamps, ref clientContext);

				// Setup the SyncStartInfo.
				SyncStartInfo si = new SyncStartInfo();
				si.CollectionID = collection.ID;
				si.Context = serverContext;
				si.ChangesOnly = gotClientChanges | !SyncComplete;
				si.ClientHasChanges = si.ChangesOnly;
			
				// Start the Sync pass and save the rights.
				sstamps = service.Start(si, store.GetUserIDFromDomainID(collection.Domain), out si);

				eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Action.StartSync, true));

				serverContext = si.Context;
				rights = si.Access;
				
				serverStatus = si.Status;
				switch (si.Status)
				{
					case SyncColStatus.Busy:
						break;
					case SyncColStatus.NotFound:
						// The collection does not exist or we do not have rights.
						collection.Commit(collection.Delete());
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
								queuedChanges = true;
								ExecuteSync();
							}
							finally
							{
								bool status = SyncComplete;
								if (queuedChanges)
								{
									// Save the sync state.
									SetChangeLogContext(serverContext, clientContext, status);
								}
								// End the sync.
								eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Action.StopSync, status));
								service.Stop();
							}
							break;
					}
						break;
				}
			}
		}

		#endregion

		#region private Methods

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
			if (fileMonitor != null)
			{
				fileMonitor.Dispose();
				fileMonitor = null;
			}
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
			DeleteOnClient = new Hashtable();
			nodesFromServer = new Hashtable();
			dirsFromServer = new Hashtable();
			filesFromServer = new Hashtable();
			DeleteOnServer = new Hashtable();
			nodesToServer = new Hashtable();
			dirsToServer = new Hashtable();
			filesToServer = new Hashtable();
			serverContext = null;
			clientContext = null;
//			buffer = new byte[MAX_XFER_SIZE];
		}

		private bool SyncComplete
		{
			get
			{
				int count = 0;
				count += DeleteOnClient.Count;
				count += nodesFromServer.Count;
				count += dirsFromServer.Count;
				count += filesFromServer.Count;
				count += DeleteOnServer.Count;
				count += nodesToServer.Count;
				count += dirsToServer.Count;
				count += filesToServer.Count;
			
				return count == 0 ? true : false;
			}
		}

		

		/// <summary>
		/// Set the new context strings that are used by ChangeLog.
		/// If persist is true save the contexts to the store.
		/// </summary>
		/// <param name="serverContext">The server context.</param>
		/// <param name="clientContext">The client context.</param>
		/// <param name="persist">Persist the changes if true.</param>
		private void SetChangeLogContext(string serverContext, string clientContext, bool persist)
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
					NodeStamp stamp = new NodeStamp(node);
					stampList.Add(stamp);
				}
				catch (Storage.DoesNotExistException)
				{
					log.Debug("Node: Name:{0} ID:{1} Type:{2} no longer exists.", sn.Name, sn.ID, sn.Type);
					continue;
				}
			}
			log.Debug("GetNodeStamps returning {0} stamps", stampList.Count);
			return (NodeStamp[])stampList.ToArray(typeof(NodeStamp));
		}
			
		/// <summary>
		/// Get the changes from the change log.
		/// </summary>
		/// <param name="nodes">returns the list of changes.</param>
		/// <param name="context">The context handed back from the last call.</param>
		/// <returns>false the call failed. The context is initialized.</returns>
		private bool GetChangedNodeStamps(out NodeStamp[] nodes, ref string context)
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
							NodeStamp stamp = new NodeStamp(rec);
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
				log.Debug("Could not get changes");
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
		private void GetNodeFromServer(SyncNodeStamp stamp)
		{
			if (stamp.Operation == SyncOperation.Delete)
			{
				DeleteOnClient[stamp.ID] = stamp.BaseType;
			}
			else
			{
				// If it is scheduled to be deleted remove from delete list.
				if (DeleteOnClient.Contains(stamp.ID))
					DeleteOnClient.Remove(stamp.ID);

				// Make sure the node has been changed.
				Node oldNode = collection.GetNodeByID(stamp.ID);
				if (oldNode == null || oldNode.MasterIncarnation != stamp.LocalIncarnation)
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
		private void PutNodeToServer(NodeStamp stamp)
		{
			if (stamp.BaseType == NodeTypes.TombstoneType || stamp.Operation == SyncOperation.Delete)
			{
				DeleteOnServer[stamp.ID] = stamp.BaseType;
			}
			else
			{
				// Remove from the delete list if it exists.
				if (DeleteOnServer.Contains(stamp.ID))
					DeleteOnServer.Remove(stamp.ID);

				if (stamp.MasterIncarnation != stamp.LocalIncarnation)
				{
					if (stamp.BaseType == NodeTypes.FileNodeType || stamp.BaseType == NodeTypes.StoreFileNodeType)
					{
						// This node is a file.
						filesToServer[stamp.ID] = stamp.BaseType;
					}
					else if (stamp.BaseType == NodeTypes.DirNodeType)
					{
						// This node is a directory.
						dirsToServer[stamp.ID] = stamp.BaseType;
					}
					else
					{
						// This is a generic node.
						nodesToServer[stamp.ID] = stamp.BaseType;
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
		private void ReconcileAllNodeStamps(SyncNodeStamp[] sstamps, NodeStamp[] cstamps)
		{
			// Clear all the tables because we are doing a full sync.
			DeleteOnClient.Clear();
			nodesFromServer.Clear();
			dirsFromServer.Clear();
			filesFromServer.Clear();
			DeleteOnServer.Clear();
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
				SyncNodeStamp sStamp = (SyncNodeStamp)tempTable[cStamp.ID];
				if (sStamp == null)
				{
					// If the Master Incarnation is not 0 then this node has been deleted on the server.
					if (cStamp.MasterIncarnation != 0)
					{
						DeleteOnClient[cStamp.ID] = cStamp.BaseType;
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
					if (cStamp.BaseType == NodeTypes.TombstoneType)
					{
						// This node has been deleted on the client.
						DeleteOnServer[cStamp.ID] = cStamp.BaseType;
					}
					else if (cStamp.LocalIncarnation != cStamp.MasterIncarnation)
					{
						// The file has been changed locally if the master is correct, push this file.
						if (cStamp.MasterIncarnation == sStamp.LocalIncarnation)
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
						if (cStamp.MasterIncarnation != sStamp.LocalIncarnation)
						{
							GetNodeFromServer(sStamp);
						}
					}
					tempTable.Remove(cStamp.ID);
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
		private void ExecuteSync()
		{
			// Get the updates from the server.
			ProcessDeleteOnClient();
			ProcessNodesFromServer();
			ProcessDirsFromServer();
			ProcessFilesFromServer();
			// Push the updates from the client.
			ProcessDeleteOnServer();
			ProcessNodesToServer();
			ProcessDirsToServer();
			ProcessFilesToServer();
		}

		/// <summary>
		/// Delete the nodes on the client.
		/// </summary>
		private void ProcessDeleteOnClient()
		{
			if (DeleteOnClient.Count == 0)
				return;
			
			// remove deleted nodes from client
			log.Info("Deleting {0} nodes on client", DeleteOnClient.Count);
			string[] idList = new string[DeleteOnClient.Count];
			DeleteOnClient.Keys.CopyTo(idList, 0);
			foreach (string id in idList)
			{
				if (stopping)
				{
					return;
				}
				try
				{
					Node node = collection.GetNodeByID(id);
					if (node == null)
					{
						log.Debug("Ignoring attempt to delete non-existent node {0}", id);
						DeleteOnClient.Remove(id);
						continue;
					}

					log.Info("Deleting {0} on client", node.Name);
					// If this is a collision node then delete the collision file.
					if (collection.HasCollisions(node))
					{
						Conflict conflict = new Conflict(collection, node);
						string conflictPath;
						if (conflict.IsFileNameConflict)
						{
							conflictPath = conflict.FileNameConflictPath;
						}
						else 
						{
							conflictPath = conflict.UpdateConflictPath;
						}
						if (conflictPath != null)
							File.Delete(conflictPath);
					}
						
					DirNode dn = node as DirNode;
					if (dn != null)
					{
						Directory.Delete(dn.GetFullPath(collection), true);
						Node[] deleted = collection.Delete(node, PropertyTags.Parent);
						collection.Commit(deleted);
						collection.Commit(deleted);
					}
					else
					{
						BaseFileNode bfn = node as BaseFileNode;
						if (bfn != null)
							File.Delete(bfn.GetFullPath(collection));
						collection.Delete(node);
						collection.Commit(node);
						collection.Commit(node);
					}
					DeleteOnClient.Remove(id);
				}
				catch
				{
					// Try to delete the next node.
				}
			}
		}

		/// <summary>
		/// Copy the generic nodes from the server.
		/// </summary>
		private void ProcessNodesFromServer()
		{
			if (nodesFromServer.Count == 0)
				return;

			SyncNode[] updates = null;

			// get small nodes and files from server
			log.Info("Downloading {0} Nodes from server", nodesFromServer.Count);
			string[] nodeIDs = new string[nodesFromServer.Count];
			nodesFromServer.Keys.CopyTo(nodeIDs, 0);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (stopping)
				{
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
				catch {}
				offset += batchCount;
			}
		}
	
		/// <summary>
		/// Save the nodes from the server in the local store.
		/// </summary>
		/// <param name="nodes"></param>
		private void StoreNodes(SyncNode [] nodes)
		{
			ArrayList	commitArray = new ArrayList();
			Node[]		commitList = null;

			// Try to commit all the nodes at once.
			foreach (SyncNode sn in nodes)
			{
				if (sn.node != null)
				{
					XmlDocument xNode = new XmlDocument();
					xNode.LoadXml(sn.node);
					Node node = Node.NodeFactory(store, xNode);
					log.Info("Importing {0} {1} from server", node.Type, node.Name);
					Import(node);
					commitArray.Add(node);
				}
				else
				{
					nodesFromServer.Remove(sn.nodeID);
				}
			}
			try
			{
				commitList = (Node[])commitArray.ToArray(typeof(Node));
				collection.Commit(commitList);
				foreach ( Node node in commitList)
				{
					if (node != null)
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
						}
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
						}
					}
					catch
					{
						// Handle any other errors.
					}
				}
			}
		}

		/// <summary>
		/// Get the directory nodes from the server.
		/// </summary>
		private void ProcessDirsFromServer()
		{
			if (dirsFromServer.Count == 0)
				return;

			SyncNode[] updates = null;

			// get small nodes and files from server
			log.Info("Downloading {0} Directories from server", dirsFromServer.Count);
			string[] nodeIDs = new string[dirsFromServer.Count];
			dirsFromServer.Keys.CopyTo(nodeIDs, 0);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (stopping)
				{
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
				}
				offset += batchCount;
			}
		}
	
		/// <summary>
		/// Called to import a node.
		/// </summary>
		/// <param name="node"></param>
		private void Import(Node node)
		{
			collection.ImportNode(node, false, 0);
			node.IncarnationUpdate = node.LocalIncarnation;
		}

		/// <summary>
		/// Store the directory node in the local store also create the directory.
		/// </summary>
		/// <param name="snode">The node to store.</param>
		private void StoreDir(SyncNode snode)
		{
			try
			{
				if (snode.node != null)
				{
					XmlDocument xNode = new XmlDocument();
					xNode.LoadXml(snode.node);
					DirNode node = (DirNode)Node.NodeFactory(store, xNode);
					log.Info("Importing Directory {0} from server", node.Name);
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
				}
				dirsFromServer.Remove(snode.nodeID);
			}
			catch 
			{
			}
		}

		/// <summary>
		/// Get the file nodes from the server.
		/// </summary>
		private void ProcessFilesFromServer()
		{
			if (filesFromServer.Count == 0)
				return;

			log.Info("Downloading {0} Files from server", filesFromServer.Count);
			string[] nodeIDs = new string[filesFromServer.Count];
			filesFromServer.Keys.CopyTo(nodeIDs, 0);

			foreach (string nodeID in nodeIDs)
			{
				try
				{
					if (stopping)
					{
						return;
					}

					ClientInFile file = new ClientInFile(collection, nodeID, new WsServerReadFile(service));
					file.Open();
					bool success = false;
					try
					{
						log.Info("Downloading File {0} from server", file.Name);
						success = file.DownLoadFile();
					}
					finally
					{
						success = file.Close(success);
						if (success)
						{
							filesFromServer.Remove(nodeID);
						}
						else
						{
							log.Info("Failed Downloading File {0}", file.Name);
						}
					}
				}
				catch 
				{
				}
			}
		}

		/// <summary>
		/// Delete nodes from the server.
		/// </summary>
		private void ProcessDeleteOnServer()
		{
			// remove deleted nodes from server
			if (DeleteOnServer.Count == 0)
				return;
		
			try
			{
				log.Info("Deleting {0} Nodes on server", DeleteOnServer.Count);
				string[] idList = new string[DeleteOnServer.Count];
				DeleteOnServer.Keys.CopyTo(idList, 0);
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
							DeleteOnServer.Remove(status.nodeID);
						}
					}
					catch 
					{
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>
		/// Upload the nodes to the server.
		/// </summary>
		private void ProcessNodesToServer()
		{
			// get small nodes and files from server
			if (nodesToServer.Count == 0)
				return;

			log.Info("Uploading {0} Nodes To server", nodesToServer.Count);
			string[] nodeIDs = new string[nodesToServer.Count];
			nodesToServer.Keys.CopyTo(nodeIDs, 0);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (stopping)
				{
					return;
				}
				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				try
				{
					for (int i = offset; i < offset + batchCount; ++ i)
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null)
						{
							log.Info("Uploading {0} {1} to server", node.Type, node.Name);
							nodes[i - offset] = node;
							SyncNode snode = new SyncNode();
							snode.nodeID = node.ID;
							snode.node = node.Properties.ToString(true);
							snode.expectedIncarn = node.MasterIncarnation;
							updates[i - offset] = snode;
						}
						else
						{
							nodesToServer.Remove(nodeIDs[i]);
						}
					}

					SyncNodeStatus[] nodeStatus = service.PutNodes(updates);
					
					for (int i = 0; i < nodes.Length; ++ i)
					{
						Node node = nodes[i];
						if (node == null)
							continue;
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
								break;
						}
					}
				}
				catch
				{
				}
				offset += batchCount;
			}
		}

		private void ProcessDirsToServer()
		{
			// get small nodes and files from server
			if (dirsToServer.Count == 0)
				return;
			
			log.Info("Uploading {0} Directories To server", dirsToServer.Count);
			string[] nodeIDs = new string[dirsToServer.Count];
			dirsToServer.Keys.CopyTo(nodeIDs, 0);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (stopping)
				{
					return;
				}

				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				try
				{
					for (int i = offset; i < offset + batchCount; ++ i)
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null)
						{
							log.Info("Uploading Directory {0} to server", node.Name);
							nodes[i - offset] = node;
							SyncNode snode = new SyncNode();
							snode.nodeID = node.ID;
							snode.node = node.Properties.ToString(true);
							snode.expectedIncarn = node.MasterIncarnation;
							updates[i - offset] = snode;
						}
						else
						{
							dirsToServer.Remove(nodeIDs[i]);
						}
					}

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
								log.Debug("Failed update of node {0} due to {1} on server",
									status.nodeID, status.status);
									
								nodesFromServer.Add(node.ID, node.Type);
								nodesToServer.Remove(node.ID);
								break;
							default:
								log.Debug("Failed update of node {0} due to {1} on server",
									status.nodeID, status.status);
								break;
						}
					}
				}
				catch
				{
				}
				offset += batchCount;
			}
		}

		private void ProcessFilesToServer()
		{
			if (filesToServer.Count == 0)
				return;

			log.Info("Uploading {0} Files To server", filesToServer.Count);
			string[] nodeIDs = new string[filesToServer.Count];
			filesToServer.Keys.CopyTo(nodeIDs, 0);

			foreach (string nodeID in nodeIDs)
			{
				try
				{
					if (stopping)
					{
						return;
					}

					BaseFileNode node = collection.GetNodeByID(nodeID) as BaseFileNode;
					if (node != null)
					{
						ClientOutFile file = new ClientOutFile(collection, node, new WsServerWriteFile(service));
						file.Open();
						bool success = false;
						try
						{
							log.Info("Uploading File {0} to server", file.Name);
							success = file.UploadFile();
						}
						finally
						{
							SyncNodeStatus syncStatus = file.Close(success);
							switch (syncStatus.status)
							{
								case SyncStatus.Success:
									filesToServer.Remove(nodeID);
									break;
								case SyncStatus.InProgess:
								case SyncStatus.InUse:
								case SyncStatus.ServerFailure:
									log.Info("Failed Uploading File {0}", file.Name);
									break;
								case SyncStatus.FileNameConflict:
								case SyncStatus.UpdateConflict:
									// Since we had a conflict we need to get the conflict node down.
									filesFromServer[nodeID] = node.Type;

									log.Info("Failed Uploading File {0}", file.Name);
									break;
							}
						}
					}
				}
				catch 
				{
				}
			}
		}

		#endregion
	}

	/*
	internal class SyncWork
	{
		internal class WorkNode
		{
			bool	ToServer;
			string	type;
		}

		SyncCollection	collection;
		Hashtable		nodesToServer = new Hashtable();
		Hashtable		nodesFromServer = new Hashtable();

		public SyncWork(SyncCollection collection)
		{
			this.collection = collection;
		}
	
		void AddNodeToServer(string nodeID, string type)
		{
			// Make sure we are not getting this node from the server.
			if (!nodesFromServer.Contains(nodeID))
			{
				nodesToServer[nodeID] = type;
			}
		}

		void AddNodeFromServer(string nodeID, string type)
		{
			if (nodesToServer.Contains(nodeID))
			{
				nodesToServer.Remove(nodeID);
			}
			nodesFromServer[nodeID] = type;
		}

		bool HasNodeChanged(string nodeID)
		{
			Node node = collection.GetNodeByID(nodeID);
			if (node
		}
	}
	*/
	#endregion
}
