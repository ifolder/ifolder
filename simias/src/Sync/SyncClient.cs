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
using Simias.Authentication;
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
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				sc.GetSyncCount(out fileCount);
			else
			{
				fileCount = 0;
			}
		}

		/// <summary>
		/// Call to schedule the collection to sync.
		/// </summary>
		/// <param name="collectionID">The collection to sync.</param>
		public static void ScheduleSync(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				sc.Reschedule(true);
		}

		/// <summary>
		/// Get the last time the Collection was in sync.
		/// </summary>
		/// <param name="collectionID">The collection to query.</param>
		/// <returns>The last time the collection was in sync.</returns>
		public static DateTime GetLastSyncTime(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				return sc.GetLastSyncTime();
			return DateTime.MinValue;
		}

		/// <summary>
		/// Stop the 
		/// </summary>
		/// <param name="collectionID"></param>
		public static void Stop(string collectionID)
		{
			CollectionSyncClient sc = GetCollectionSyncClient(collectionID);
			if (sc != null)
				sc.Stop();
		}

		#endregion

		#region private methods.

		/// <summary>
		/// Gets the CollectionSyncClient object for the specified collection.
		/// </summary>
		/// <param name="collectionID"></param>
		/// <returns></returns>
		private static CollectionSyncClient GetCollectionSyncClient(string collectionID)
		{
			CollectionSyncClient sc;
			lock (collections)
			{
				sc = (CollectionSyncClient)collections[collectionID];
			}
			return sc;
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
							cClient.Reschedule(false);
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
				cClient.Reschedule(true);
			}
		}

		/// <summary>
		/// Pauses the synchronization service.
		/// </summary>
		public void Pause()
		{
			paused = true;
			foreach(CollectionSyncClient cClient in collections)
			{
				cClient.Stop();
			}
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
			foreach(CollectionSyncClient cClient in collections)
			{
				cClient.Stop();
			}
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
		SyncWorkArray	workArray;
		Store			store;
		SyncCollection	collection;
		bool			queuedChanges;
		bool			serverAlive;
		SyncColStatus	serverStatus;
		Timer			timer;
		TimerCallback	callback;
		FileWatcher		fileMonitor;
		bool			stopping;
		Rights			rights;
		string			serverContext;
		string			clientContext;
		static int		BATCH_SIZE = 50;
		private const string	ServerCLContextProp = "ServerCLContext";
		private const string	ClientCLContextProp = "ClientCLContext";
		int				nodesToSync;

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
			Initialize();
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
		/// <param name="SyncNow">If true schedule now.</param>
		internal void Reschedule(bool SyncNow)
		{
			if (!stopping)
			{
                int seconds;
				if (SyncNow)
				{
					seconds = 0;
				}
				else if (!serverAlive)
				{
					seconds = 10;
				}
				else 
				{
					int nodesLeft = workArray.Count;
					if (nodesLeft != 0 && nodesLeft < nodesToSync)
					{
						seconds = 0;
					}
					else
					{
						seconds = collection.Interval;
						if (serverStatus == SyncColStatus.Busy)
						{
							// Reschedule to sync within 1/12 of the scheduled sync time, but no less than 2 seconds.
							seconds = new Random().Next(seconds / 12) + 2;
							seconds = seconds > 30 ? 30 : seconds;
						}
					}
				}
				timer.Change(seconds * 1000, Timeout.Infinite);
			}
		}

		/// <summary>
		/// Get the last time the collection was in sync.
		/// </summary>
		/// <returns></returns>
		internal DateTime GetLastSyncTime()
		{
			Property cc = collection.Properties.GetSingleProperty(ClientCLContextProp);
			if (cc != null)
			{
				return (new EventContext(cc.Value.ToString()).TimeStamp);
			}
			return DateTime.MinValue;
		}

		/// <summary>
		/// Called to stop this instance from sync-ing.
		/// </summary>
		internal void Stop()
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
			lock (this)
			{
				stopping = true;
			}
		}

		/// <summary>
		/// Get the number of bytes to sync.
		/// </summary>
		/// <param name="fileCount">Returns the number of files to be synced.</param>
		internal void GetSyncCount(out uint fileCount)
		{
			fileCount = (uint)workArray.UpCount;
		}

		/// <summary>
		/// Called to synchronize this collection.
		/// </summary>
		internal void SyncNow()
		{
			lock (this)
			{
				stopping = false;
			}
			queuedChanges = false;
			serverAlive = false;
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

			// credentials
			Credentials cSimiasCreds = new Credentials(collection.ID);
			service.Credentials = cSimiasCreds.GetCredentials();

			SyncNodeStamp[] sstamps;
			NodeStamp[]		cstamps;
			
			// Get the current sync state.
			GetChangeLogContext(out serverContext, out clientContext);
			bool gotClientChanges = GetChangedNodeStamps(out cstamps, ref clientContext);

			// Setup the SyncStartInfo.
			SyncStartInfo si = new SyncStartInfo();
			si.CollectionID = collection.ID;
			si.Context = serverContext;
			si.ChangesOnly = gotClientChanges | !workArray.Complete;
			si.ClientHasChanges = si.ChangesOnly;
			
			// Start the Sync pass and save the rights.
			sstamps = service.Start(si, store.GetUserIDFromDomainID(collection.Domain), out si);
			serverAlive = true;

			eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Action.StartSync, true));

			serverContext = si.Context;
			workArray.SetAccess = rights = si.Access;
				
			serverStatus = si.Status;
			switch (si.Status)
			{
				case SyncColStatus.AccessDenied:
					log.Info("Failed authentication");
					break;
				case SyncColStatus.Busy:
					log.Info("The server is busy");
					break;
				case SyncColStatus.NotFound:
					log.Info("The collection no longer exists");
					// The collection does not exist or we do not have rights.
					collection.Commit(collection.Delete());
					break;
				case SyncColStatus.NoWork:
					log.Debug("No work to do");
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
							bool status = workArray.Complete;
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
		/// Called to reinitilize if we failed.
		/// This will happen when we are disconected.
		/// </summary>
		/// <param name="collectionClient"></param>
		private void RetryInit(object collectionClient)
		{
			Initialize();
		}

		/// <summary>
		/// Initializes the instance.
		/// </summary>
		private void Initialize()
		{
			fileMonitor = new FileWatcher(collection, false);
			switch(collection.Role)
			{
				case SyncCollectionRoles.Master:
				case SyncCollectionRoles.Local:
				default:
					timer = new Timer(callback, this, Timeout.Infinite, Timeout.Infinite);				
					break;

				case SyncCollectionRoles.Slave:
					int delay;
					if (collection.MasterIncarnation == 0) delay = 0;
					else delay = collection.Interval == Timeout.Infinite ? Timeout.Infinite : 0;
			
					// If the master has not been created. Do it now.
					try
					{
						if (collection.CreateMaster)
						{
							new Simias.Domain.DomainAgent(Configuration.GetConfiguration()).CreateMaster(collection);
						}
						workArray = new SyncWorkArray(collection);
						serverContext = null;
						clientContext = null;
						timer = new Timer(callback, this, delay, Timeout.Infinite);
					}
					catch
					{
						timer = new Timer(new TimerCallback(RetryInit), this, delay, Timeout.Infinite);
					}
					break;
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
				workArray.AddNodeFromServer(sstamps[i]);
			}

			for (int i = 0; i < cstamps.Length; ++i)
			{
				workArray.AddNodeToServer(cstamps[i]);
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
			// Clear the current work because we are doing a full sync.
			workArray.Clear();
			
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
						cStamp.Operation = SyncOperation.Delete;
						cStamp.MasterIncarnation = 0;
						workArray.AddNodeFromServer(cStamp);
					}
					else
					{
						// The node is on the client but not the server send it to the server.
						workArray.AddNodeToServer(cStamp);
					}
				}
				else
				{
					// The node is on both the server and the client.  Check which way the node
					// should go.
					if (cStamp.BaseType == NodeTypes.TombstoneType)
					{
						// This node has been deleted on the client.
						workArray.AddNodeToServer(cStamp);
					}
					else if (cStamp.LocalIncarnation != cStamp.MasterIncarnation)
					{
						// The file has been changed locally if the master is correct, push this file.
						if (cStamp.MasterIncarnation == sStamp.LocalIncarnation)
						{
							workArray.AddNodeToServer(cStamp);
						}
						else
						{
							// This will be a conflict get the servers latest revision.
							workArray.AddNodeFromServer(sStamp);
						}
					}
					else
					{
						// The node has not been changed locally see if we need to get the node.
						if (cStamp.MasterIncarnation != sStamp.LocalIncarnation)
						{
							workArray.AddNodeFromServer(sStamp);
						}
					}
					tempTable.Remove(cStamp.ID);
				}
			}

			// Now Get any nodes left, that are on the server but not on the client.
			foreach(SyncNodeStamp sStamp in tempTable.Values)
			{
				workArray.AddNodeFromServer(sStamp);
			}
		}

		/// <summary>
		/// Do the work that is needed to synchronize the client and server.
		/// </summary>
		private void ExecuteSync()
		{
			nodesToSync = workArray.Count;
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
			
			// remove deleted nodes from client
			string[] idList = workArray.DeletesFromServer();
			if (idList.Length == 0)
				return;
			log.Info("Deleting {0} nodes on client", idList.Length);
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
						workArray.RemoveNodeFromServer(id);
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
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, true, node.Name, 0, 0, 0, Direction.Downloading));
						Node[] deleted = collection.Delete(node, PropertyTags.Parent);
						collection.Commit(deleted);
						collection.Commit(deleted);
					}
					else
					{
						BaseFileNode bfn = node as BaseFileNode;
						if (bfn != null)
						{
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, true, node.Name, 0, 0, 0, Direction.Downloading));
							FileInfo fi = new FileInfo(bfn.GetFullPath(collection));
							if (rights == Rights.ReadOnly)
								fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly;
							fi.Delete();
						}
						else
						{
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Unknown, true, node.Name, 0, 0, 0, Direction.Downloading));
						}
						collection.Delete(node);
						collection.Commit(node);
						collection.Commit(node);
					}
					workArray.RemoveNodeFromServer(id);
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
			SyncNode[] updates = null;

			// get small nodes and files from server
			string[] nodeIDs = workArray.GenericsFromServer();
			if (nodeIDs.Length == 0)
				return;
			log.Info("Downloading {0} Nodes from server", nodeIDs.Length);
				
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
					workArray.RemoveNodeFromServer(sn.nodeID);
				}
			}
			try
			{
				commitList = (Node[])commitArray.ToArray(typeof(Node));
				collection.Commit(commitList);
				foreach ( Node node in commitList)
				{
					if (node != null)
						workArray.RemoveNodeFromServer(node.ID);
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
						workArray.RemoveNodeFromServer(node.ID);
					}
					catch (CollisionException)
					{
						try
						{
							// The current node failed because of a collision.
							Node cNode = collection.CreateCollision(node, false);
							collection.Commit(cNode);
							workArray.RemoveNodeFromServer(cNode.ID);
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
			
			SyncNode[] updates = null;

			// get small nodes and files from server
			string[] nodeIDs = workArray.DirsFromServer();
			if (nodeIDs.Length == 0)
				return;
			log.Info("Downloading {0} Directories from server", nodeIDs.Length);
				
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
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, node.Name, 0, 0, 0, Direction.Downloading));
				}
				workArray.RemoveNodeFromServer(snode.nodeID);
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
			
			string[] nodeIDs = workArray.FilesFromServer();
			if (nodeIDs.Length == 0)
				return;
			log.Info("Downloading {0} Files from server", nodeIDs.Length);
			
			foreach (string nodeID in nodeIDs)
			{
				try
				{
					if (stopping)
					{
						return;
					}

					ClientInFile file = new ClientInFile(collection, nodeID, new WsServerReadFile(service));
					if (file.Open(rights == Rights.ReadOnly ? true : false))
					{
						bool success = false;
						try
						{
							log.Info("Downloading File {0} from server", file.Name);
							success = file.DownLoadFile();
						}
						catch (Exception ex)
						{
							Log.log.Debug(ex, "Failed Download before close");
						}
						finally
						{
							success = file.Close(success);
							if (success)
							{
								workArray.RemoveNodeFromServer(nodeID);
							}
							else
							{
								log.Info("Failed Downloading File {0}", file.Name);
							}
						}
					}
					else
					{
						// There is no file to pull down.
						workArray.RemoveNodeFromServer(nodeID);
					}
				}
				catch (Exception ex)
				{
					Log.log.Debug(ex, "Failed Downloading File during close");
				}
			}
		}

		/// <summary>
		/// Delete nodes from the server.
		/// </summary>
		private void ProcessDeleteOnServer()
		{
			// remove deleted nodes from server
			try
			{
				string[] idList = workArray.DeletesToServer();
				if (idList.Length == 0)
					return;
				log.Info("Deleting {0} Nodes on server", idList.Length);
				
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
								eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Unknown, true, node.Name, 0, 0, 0, Direction.Uploading));
								log.Info("Deleting {0} from server", node.Name);
								// Delete the tombstone.
								collection.Commit(collection.Delete(node));
							}
							workArray.RemoveNodeToServer(status.nodeID);
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
			string[] nodeIDs = workArray.GenericsToServer();
			if (nodeIDs.Length == 0)
				return;
			log.Info("Uploading {0} Nodes To server", nodeIDs.Length);
				
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
							workArray.RemoveNodeToServer(nodeIDs[i]);
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
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.UpdateConflict:
							case SyncStatus.FileNameConflict:
								// The file has been changed on the server lets get it next pass.
								log.Debug("Skipping update of node {0} due to {1} on server",
									status.nodeID, status.status);
									
								NodeStamp ns = new NodeStamp(node);
								ns.MasterIncarnation++;
								workArray.AddNodeFromServer(ns);
								workArray.RemoveNodeToServer(node.ID);
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
			string[] nodeIDs = workArray.DirsToServer();
			if (nodeIDs.Length == 0)
				return;
			log.Info("Uploading {0} Directories To server", nodeIDs.Length);
				
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
							workArray.RemoveNodeToServer(nodeIDs[i]);
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
								eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, node.Name, 0, 0, 0, Direction.Uploading));
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.UpdateConflict:
							case SyncStatus.FileNameConflict:
								// The file has been changed on the server lets get it next pass.
								log.Debug("Failed update of node {0} due to {1} on server",
									status.nodeID, status.status);
									
								NodeStamp ns = new NodeStamp(node);
								ns.MasterIncarnation++;
								workArray.AddNodeFromServer(ns);
								workArray.RemoveNodeToServer(node.ID);
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
			string[] nodeIDs = workArray.FilesToServer();
			if (nodeIDs.Length == 0)
				return;

			log.Info("Uploading {0} Files To server", nodeIDs.Length);
			
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
						SyncStatus status = file.Open();
						if (status == SyncStatus.Success)
						{
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
										workArray.RemoveNodeToServer(nodeID);
										break;
									case SyncStatus.InProgess:
									case SyncStatus.InUse:
									case SyncStatus.ServerFailure:
										log.Info("Failed Uploading File {0} : reason {1}", file.Name, syncStatus.status.ToString());
										break;
									case SyncStatus.FileNameConflict:
									case SyncStatus.UpdateConflict:
										// Since we had a conflict we need to get the conflict node down.
										workArray.RemoveNodeToServer(nodeID);
										NodeStamp ns = new NodeStamp(node);
										ns.MasterIncarnation++;
										workArray.AddNodeFromServer(ns);
										log.Info("Failed Uploading File {0} : reason {1}", file.Name, syncStatus.status.ToString());
										break;
								}
							}
						}
						else
						{
							log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
						}
					}
				}
				catch (Exception ex)
				{
					Log.log.Debug(ex, "Failed Uploading File");
				}
			}
		}

		#endregion
	}

	internal class SyncWorkArray
	{
		Collection		collection;
		Hashtable		nodesFromServer;
		Hashtable		nodesToServer;
		Rights			rights;
		
		/// <summary>
		/// Node type enum.
		/// </summary>
		enum WorkType
		{
			Generic = 0,
			Directory,
			File,
			Delete,
		}

		/// <summary>
		/// Array of items to sync.
		/// </summary>
		/// <param name="collection">The collection.</param>
		internal SyncWorkArray(SyncCollection collection)
		{
			this.collection = collection;
			nodesFromServer = new Hashtable();
			nodesToServer = new Hashtable();
		}

		/// <summary>
		/// Clear all of the current work to do.
		/// </summary>
		internal void Clear()
		{
			nodesFromServer.Clear();
			nodesToServer.Clear();
		}

		/// <summary>
		/// Determins how this node should be retrieved from the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		internal void AddNodeFromServer(SyncNodeStamp stamp)
		{
			// Make sure the node does not exist in the nodesToServer table.
			if (NodeHasChanged(stamp.ID, stamp.LocalIncarnation) || stamp.Operation == SyncOperation.Delete)
			{
				if (nodesToServer.Contains(stamp.ID))
				{
					nodesToServer.Remove(stamp.ID);
				}
				else
				{
					if (stamp.Operation == SyncOperation.Delete)
					{
						nodesFromServer[stamp.ID] = WorkType.Delete;
					}
					else if (stamp.BaseType == NodeTypes.FileNodeType || stamp.BaseType == NodeTypes.StoreFileNodeType)
					{
						// This is a file.
						nodesFromServer[stamp.ID] = WorkType.File;
					}
					else if (stamp.BaseType == NodeTypes.DirNodeType)
					{
						// This node represents a directory.
						nodesFromServer[stamp.ID] = WorkType.Directory;
					}
					else
					{
						// This is a generic node.
						nodesFromServer[stamp.ID] = WorkType.Generic;
					}
				}
			}
		}

		/// <summary>
		/// Determins how this node should be sent to the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		internal void AddNodeToServer(NodeStamp stamp)
		{
			if (stamp.MasterIncarnation != stamp.LocalIncarnation)
			{
				if (nodesFromServer.Contains(stamp.ID))
				{
					// This node has changed on the server we have a collision that we need to get.
					// Unless this is a delete.
					if (stamp.Operation == SyncOperation.Delete)
					{
						RemoveNodeFromServer(stamp.ID);
						nodesToServer[stamp.ID] = WorkType.Delete;
					}
				}
				else if (rights == Rights.ReadOnly)
				{
					// We need to get this node from the server.
					stamp.Operation = SyncOperation.Change;
					stamp.LocalIncarnation = stamp.MasterIncarnation + 1;
					AddNodeFromServer(stamp);
				}
				else
				{
					if (stamp.BaseType == NodeTypes.TombstoneType || stamp.Operation == SyncOperation.Delete)
					{
						nodesToServer[stamp.ID] = WorkType.Delete;
					}
					else if (stamp.BaseType == NodeTypes.FileNodeType || stamp.BaseType == NodeTypes.StoreFileNodeType)
					{
						// This node is a file.
						nodesToServer[stamp.ID] = WorkType.File;
					}
					else if (stamp.BaseType == NodeTypes.DirNodeType)
					{
						// This node is a directory.
						nodesToServer[stamp.ID] = WorkType.Directory;
					}
					else
					{
						// This is a generic node.
						nodesToServer[stamp.ID] = WorkType.Generic;
					}
				}
			}
		}

		/// <summary>
		/// Remove the node from the work table.
		/// </summary>
		/// <param name="nodeID">The node to remove.</param>
		internal void RemoveNodeFromServer(string nodeID)
		{
			nodesFromServer.Remove(nodeID);
		}

		/// <summary>
		/// Remove the node from the work table.
		/// </summary>
		/// <param name="nodeID">The node to remove.</param>
		internal void RemoveNodeToServer(string nodeID)
		{
			nodesToServer.Remove(nodeID);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to retrieve from the server.
		/// </summary>
		/// <param name="wType">The Type of work to return.</param>
		/// <returns></returns>
		private string[] FromServer(WorkType wType)
		{
			ArrayList na = new ArrayList();
			foreach (DictionaryEntry de in nodesFromServer)
			{
				if ((WorkType)de.Value == wType)
					na.Add(de.Key);
			}

			return (string[])na.ToArray(typeof(string));
		}

		/// <summary>
		/// Get an array of the IDs to delete on the client.
		/// </summary>
		/// <returns></returns>
		internal string[] DeletesFromServer()
		{
			return FromServer(WorkType.Delete);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] GenericsFromServer()
		{
			return FromServer(WorkType.Generic);
		}

		/// <summary>
		/// Get an array of the IDs of the Directory Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DirsFromServer()
		{
			return FromServer(WorkType.Directory);
		}

		/// <summary>
		/// Get an array of the IDs of the File Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] FilesFromServer()
		{
			return FromServer(WorkType.File);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to push up to the server.
		/// </summary>
		/// <param name="wType">The Type of work to return.</param>
		/// <returns></returns>
		private string[] ToServer(WorkType wType)
		{
			ArrayList na = new ArrayList();
			foreach (DictionaryEntry de in nodesToServer)
			{
				if ((WorkType)de.Value == wType)
					na.Add(de.Key);
			}

			return (string[])na.ToArray(typeof(string));
		}

		/// <summary>
		/// Get an array of the IDs to delete on the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DeletesToServer()
		{
			return ToServer(WorkType.Delete);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] GenericsToServer()
		{
			return ToServer(WorkType.Generic);
		}

		/// <summary>
		/// Get an array of the IDs of the Directory Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DirsToServer()
		{
			return ToServer(WorkType.Directory);
		}

		/// <summary>
		/// Get an array of the IDs of the File Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] FilesToServer()
		{
			return ToServer(WorkType.File);
		}

		/// <summary>
		/// Checks to see if the node needs updating.
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="MasterIncarnation"></param>
		/// <returns></returns>
		bool NodeHasChanged(string nodeID, ulong MasterIncarnation)
		{
			Node oldNode = collection.GetNodeByID(nodeID);
			if (oldNode == null || oldNode.MasterIncarnation != MasterIncarnation)
				return true;
			return false;
		}

		/// <summary>
		/// Gets if the work is complete.
		/// </summary>
		internal bool Complete
		{
			get
			{
				int count = 0;
				count += nodesFromServer.Count;
				count += nodesToServer.Count;
				
				return count == 0 ? true : false;
			}
		}

		/// <summary>
		/// Set the Access that is allowed to the collection.
		/// </summary>
		internal Rights SetAccess
		{
			set
			{
				rights = value;
			}
		}

		/// <summary>
		/// Get the number of nodes that need to be synced.
		/// </summary>
		internal int Count
		{
			get
			{
				return nodesToServer.Count + nodesFromServer.Count;
			}
		}

		/// <summary>
		/// Get the number of node to sync up to the server.
		/// </summary>
		internal int UpCount
		{
			get
			{
				return nodesToServer.Count;
			}
		}

		/// <summary>
		/// Get the number of node to sync down from the server.
		/// </summary>
		internal int DownCount
		{
			get
			{
				return nodesFromServer.Count;
			}
		}
	}
	#endregion
}
