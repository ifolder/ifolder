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
using Simias.Sync.Http;
using Simias.Storage;
using Simias.Service;
using Simias.Event;
using Simias.Client;
using Simias.Client.Event;
using Simias.DomainServices;


namespace Simias.Sync
{
	#region StartSyncStatus

	/// <summary>
	/// 
	/// </summary>
	public enum StartSyncStatus : byte
	{
		/// <summary>
		/// We need to sync.
		/// </summary>
		Success,
		/// <summary>
		/// There is nothing to do.
		/// </summary>
		NoWork,
		/// <summary>
		/// The collection was not found.
		/// </summary>
		NotFound,
		/// <summary>
		/// Someone is sync-ing now come back latter.
		/// </summary>
		Busy,
		/// <summary>
		/// The user is not authenticated.
		/// </summary>
		AccessDenied,
		/// <summary>
		/// The collection is locked.
		/// </summary>
		Locked,
		/// <summary>
		/// The user has not been authenticated.
		/// </summary>
		UserNotAuthenticated,
	};

	#endregion

	#region SyncOperation

	/// <summary>
	/// Sync operation.
	/// </summary>
	public enum SyncOperation : byte
	{
		/// <summary>
		/// The node exists but no log record has been created.
		/// Do a brute force sync.
		/// </summary>
		Unknown,
		/// <summary>
		/// Node object was created.
		/// </summary>
		Create,
		/// <summary>
		/// Node object was deleted.
		/// </summary>
		Delete,
		/// <summary>
		/// Node object was changed.
		/// </summary>
		Change,
		/// <summary>
		/// Node object was renamed.
		/// </summary>
		Rename
	};

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
		Queue				priorityQueue;
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
				CollectionSyncClient cc = collectionClient as CollectionSyncClient;
				if (cc.HighPriority)
				{
					if (!priorityQueue.Contains(cc))
						priorityQueue.Enqueue(cc);
				}
				else
				{
					if (!syncQueue.Contains(collectionClient))
						syncQueue.Enqueue(collectionClient);
				}
				queueEvent.Set();
			}
		}

		#region static methods

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
						if (priorityQueue.Count != 0)
						{
							cClient = priorityQueue.Dequeue() as CollectionSyncClient;
						}
						else
						{
							if (syncQueue.Count == 0)
							{
								queueEvent.Reset();
								break;
							}

							cClient = syncQueue.Dequeue() as CollectionSyncClient;
						}
					}
					if (!shuttingDown || !paused)
					{
						bool serverWasConnected = cClient.Connected;
						// Sync this collection now.
						if (serverWasConnected)
							log.Info("{0} : Starting Sync.", cClient);
						try
						{
							cClient.SyncNow();
							log.Info("{0} : Finished Sync.", cClient);
						}
						catch (NeedCredentialsException ex)
						{
							log.Debug(ex.Message);
						}
						catch (Exception ex)
						{
							if (!cClient.Connected)
							{
								if (serverWasConnected)
									log.Info("Server for {0} is unavailable", cClient);
							}
							else
							{
								log.Error(ex, "Finished Sync. Error =  {0}", ex.Message);
							}
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
			priorityQueue = new Queue();
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
			foreach(CollectionSyncClient cClient in collections.Values)
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
			foreach(CollectionSyncClient cClient in collections.Values)
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
			lock (collections)
			{
				foreach(CollectionSyncClient cClient in collections.Values)
				{
					cClient.Stop();
				}
				collections.Clear();
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
		HttpSyncProxy	service;
		SyncWorkArray	workArray;
		Store			store;
		Collection		collection;
		bool			queuedChanges;
		bool			serverAlive = true;
		StartSyncStatus	serverStatus;
		Timer			timer;
		TimerCallback	callback;
		FileWatcher		fileMonitor;
		bool			stopping;
		Access.Rights	rights;
		string			serverContext;
		string			clientContext;
		static int		BATCH_SIZE = 50;
		private const string	ServerCLContextProp = "ServerCLContext";
		private const string	ClientCLContextProp = "ClientCLContext";
		int				nodesToSync = 0;
		static int		initialSyncDelay = 10 * 1000; // 10 seconds.
		DateTime		syncStartTime; // Time stamp when sync was called.
		const int		timeSlice = 3; //Timeslice in minutes.
		SyncFile		syncFile;
		
		/// <summary>
		/// Returns true if we should yield our timeslice.
		/// </summary>
		bool Yield
		{
			get 
			{	
				if (stopping)
					return true;
				TimeSpan ts = DateTime.Now - syncStartTime;
				if (ts.Minutes > timeSlice)
					return true;
				return false;
			}
		}

		internal bool HighPriority
		{
			get 
			{ 
				// If the server is not responding make sure that we are not high priority.
				if (serverAlive)
					return collection.Priority == 0 ? true : false; 
				else return false;
			}
		}

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
			collection = store.GetCollectionByID(nid);
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
		/// Get whether the server is connected.
		/// </summary>
		internal bool Connected
		{
			get { return serverAlive; }
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
					seconds = 60;
				}
				else if (workArray.Count != 0 && nodesToSync > workArray.Count)
				{
					seconds = 0;
				}
				else 
				{
					seconds = collection.Interval;
					if (serverStatus == StartSyncStatus.Busy)
					{
						// Reschedule to sync within 1/12 of the scheduled sync time, but no less than 10 seconds.
						seconds = new Random().Next(seconds / 12) + 10;
						seconds = seconds > 30 ? 30 : seconds;
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
				if (syncFile != null)
					syncFile.Stop = true;
			}
		}

		/// <summary>
		/// Get the number of bytes to sync.
		/// </summary>
		/// <param name="fileCount">Returns the number of files to be synced.</param>
		internal void GetSyncCount(out uint fileCount)
		{
			fileCount = (uint)workArray.Count;
		}

		/// <summary>
		/// Called to synchronize this collection.
		/// </summary>
		internal void SyncNow()
		{
			syncStartTime = DateTime.Now;
			queuedChanges = false;
			serverAlive = false;
			serverStatus = StartSyncStatus.Success;
			// Refresh the collection.
			collection.Refresh();

			// Make sure the master exists.
			if (collection.CreateMaster)
			{
				new DomainAgent().CreateMaster(collection);
			}
			
			// Only syncronize local changes when we have finished with the 
			// Server side changes.
			if (workArray == null || workArray.DownCount == 0)
				fileMonitor.CheckForFileChanges();
			if (collection.Role != SyncRoles.Slave)
			{
				serverAlive = true;
				return;
			}

			// We may have just created or deleted nodes wait for the events to settle.
			Thread.Sleep(500);

			// Setup the url to the server.
			string userID = store.GetUserIDFromDomainID(collection.Domain);
			string userName = collection.GetMemberByID(userID).Name;
			service = new HttpSyncProxy(collection, userName, userID);

			SyncNodeInfo[] cstamps;
			
			// Get the current sync state.
			string tempClientContext;
			string tempServerContext;
			GetChangeLogContext(out tempServerContext, out tempClientContext);
			bool gotClientChanges = this.GetChangedNodeInfoArray(out cstamps, ref tempClientContext);

			// Setup the SyncStartInfo.
			StartSyncInfo si = new StartSyncInfo();
			si.CollectionID = collection.ID;
			si.Context = tempServerContext;
			si.ChangesOnly = gotClientChanges | !workArray.Complete;
			si.ClientHasChanges = si.ChangesOnly;
			
			// Start the Sync pass and save the rights.
			try
			{
				service.StartSync(ref si);
			}
			catch (Exception ex)
			{
				service = null;
				throw ex;
			}
			
			serverAlive = true;

			eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Action.StartSync, true));

			try
			{

				tempServerContext = si.Context;
				workArray.SetAccess = rights = si.Access;
				
				serverStatus = si.Status;
				switch (si.Status)
				{
					case StartSyncStatus.AccessDenied:
						new EventPublisher().RaiseEvent(
							new NodeEventArgs(
								"Sync", collection.ID, collection.ID, 
								collection.BaseType, EventType.NoAccess, 
								0, DateTime.Now, collection.MasterIncarnation,
								collection.LocalIncarnation, 0)); 
						log.Info("The user no longer has rights.");
						collection.Commit(collection.Delete());
						break;
					case StartSyncStatus.Locked:
						log.Info("The collection is locked");
						break;
					case StartSyncStatus.Busy:
						log.Info("The server is busy");
						break;
					case StartSyncStatus.NotFound:
						new EventPublisher().RaiseEvent(
							new NodeEventArgs(
								"Sync", collection.ID, collection.ID, 
								collection.BaseType, EventType.NoAccess, 
								0, DateTime.Now, collection.MasterIncarnation, 
								collection.LocalIncarnation, 0)); 
						log.Info("The collection no longer exists");
						// The collection does not exist or we do not have rights.
						collection.Commit(collection.Delete());
						break;
					case StartSyncStatus.NoWork:
						log.Debug("No work to do");
						break;
					case StartSyncStatus.UserNotAuthenticated:
						log.Debug("The user could not be authenticated");
						break;
					case StartSyncStatus.Success:
					switch (rights)
					{
						case Access.Rights.Deny:
							break;
						case Access.Rights.Admin:
						case Access.Rights.ReadOnly:
						case Access.Rights.ReadWrite:
							try
							{
								while (true)
								{
									int filesFromServer;
									// Now lets determine the files that need to be synced.
									if (si.ChangesOnly)
									{
										// We only need to look at the changed nodes.
										ProcessChangedNodeStamps(cstamps, ref tempServerContext);
										cstamps = null;
									}
									else
									{
										// We don't have any state. So do a full sync.
										ReconcileAllNodeStamps();
									}
									filesFromServer = workArray.DownCount;
									queuedChanges = true;
									ExecuteSync();
									if (!si.ChangesOnly)
										break;
									if (filesFromServer == 0 || filesFromServer == workArray.DownCount)
										break;
								}
							}
							finally
							{
								bool status = workArray.Complete;
								if (queuedChanges)
								{
									// Save the sync state.
									SetChangeLogContext(tempServerContext, tempClientContext, status);
								}
								// End the sync.
								service.EndSync();
							}
							break;
					}
						break;
				}
			}
			finally
			{
				eventPublisher.RaiseEvent(new CollectionSyncEventArgs(collection.Name, collection.ID, Action.StopSync, workArray.Complete));
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
		private void InitializeSlave(object collectionClient)
		{
			int delay;
			if (collection.MasterIncarnation == 0) delay = initialSyncDelay;
			else delay = collection.Interval == Timeout.Infinite ? Timeout.Infinite : initialSyncDelay;
			
			// If the master has not been created. Do it now.
			try
			{
				workArray = new SyncWorkArray(collection);
				serverContext = null;
				clientContext = null;
				timer = new Timer(callback, this, delay, Timeout.Infinite);
			}
			catch
			{
				timer = new Timer(new TimerCallback(InitializeSlave), this, delay, Timeout.Infinite);
			}
		}

		/// <summary>
		/// Initializes the instance.
		/// </summary>
		private void Initialize()
		{
			fileMonitor = new FileWatcher(collection, false);
			switch(collection.Role)
			{
				case SyncRoles.Master:
				case SyncRoles.Local:
				default:
					timer = new Timer(callback, this, initialSyncDelay, Timeout.Infinite);				
					break;

				case SyncRoles.Slave:
					InitializeSlave(this);
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
			serverContext = this.serverContext != null ? this.serverContext : "";
			clientContext = this.clientContext != null ? this.clientContext : "";
		}

		/// <summary>
		/// Returns information about all of the nodes in the collection.
		/// The nodes can be used to determine what nodes need to be synced.
		/// </summary>
		/// <returns>Array of NodeStamps</returns>
		private SyncNodeInfo[] GetNodeInfoArray()
		{
			log.Debug("GetNodeInfoArray start");
			ArrayList infoList = new ArrayList();
			foreach (ShallowNode sn in collection)
			{
				Node node;
				try
				{
					node = new Node(collection, sn);
					if (collection.HasCollisions(node))
						continue;
					infoList.Add(new SyncNodeInfo(node));
				}
				catch (Storage.DoesNotExistException)
				{
					log.Debug("Node: Name:{0} ID:{1} Type:{2} no longer exists.", sn.Name, sn.ID, sn.Type);
					continue;
				}
			}
			log.Debug("GetNodeInfoArray returning {0} nodes", infoList.Count);
			return (SyncNodeInfo[])infoList.ToArray(typeof(SyncNodeInfo));
		}
			
		/// <summary>
		/// Get the changes from the change log.
		/// </summary>
		/// <param name="nodes">returns the list of changes.</param>
		/// <param name="context">The context handed back from the last call.</param>
		/// <returns>false the call failed. The context is initialized.</returns>
		private bool GetChangedNodeInfoArray(out SyncNodeInfo[] nodes, ref string context)
		{
			log.Debug("GetChangedNodes Start");
			EventContext eventContext;
			ArrayList changeList = null;
			ArrayList infoList = new ArrayList();

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
							infoList.Add(new SyncNodeInfo(rec));
						}
					}
					log.Debug("Found {0} changed nodes.", infoList.Count);
					nodes = (SyncNodeInfo[])infoList.ToArray(typeof(SyncNodeInfo));
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
		/// <param name="cstamps">The client changes.</param>
		/// <param name="context">The sync context.</param>
		private void ProcessChangedNodeStamps(SyncNodeInfo[] cstamps, ref string context)
		{
			SyncNodeInfo[] infoList;
			string tempContext;
			while ((infoList = service.GetNextInfoList(out tempContext)) != null)
			{
				foreach(SyncNodeInfo nodeInfo in infoList)
				{
					workArray.AddNodeFromServer(nodeInfo);
				}
			}

			if (cstamps != null)
			{
				for (int i = 0; i < cstamps.Length; ++i)
				{
					workArray.AddNodeToServer(cstamps[i]);
				}
			}
		
			if (tempContext != null)
				context = tempContext;
			
		}

		
		/// <summary>
		/// Determines which nodes need to by synced.
		/// This is done by comparing all nodes on the server with all nodes on the client.
		/// </summary>
		private void ReconcileAllNodeStamps()
		{
			SyncNodeInfo[] cstamps = GetNodeInfoArray();
			SyncNodeInfo[] sstamps;
			// Clear the current work because we are doing a full sync.
			workArray.Clear();
			
			Hashtable tempTable = new Hashtable();

			// Add all of the server nodes to the hashtable and then we can reconcile them
			// against the client nodes.
			string context;
			while ((sstamps = service.GetNextInfoList(out context)) != null)
			{
				foreach (SyncNodeInfo sStamp in sstamps)
				{
					tempTable.Add(sStamp.ID, sStamp);
				}
			}
			log.Debug("Server has {0} nodes. Client has {1} nodes", tempTable.Count, cstamps.Length);

			foreach (SyncNodeInfo cStamp in cstamps)
			{
				SyncNodeInfo sStamp = (SyncNodeInfo)tempTable[cStamp.ID];
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
					if (cStamp.NodeType == SyncNodeType.Tombstone)
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
			foreach(SyncNodeInfo sStamp in tempTable.Values)
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
			if (workArray.DownCount != 0)
			{
				// Get the updates from the server.
				ProcessDeleteOnClient();
				ProcessNodesFromServer();
				// Make sure that we have all subdirs down before we 
				// start on files.
				if (ProcessDirsFromServer())
					ProcessFilesFromServer();
			}
			if (workArray.UpCount != 0)
			{
				// Push the updates from the client.
				ProcessDeleteOnServer();
				ProcessNodesToServer();
				// Make sure that we put the subdirs up before the files.
				if (ProcessDirsToServer())
					ProcessFilesToServer();
			}
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
				if (Yield)
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
						{
							if (File.Exists(conflictPath))
								File.Delete(conflictPath);
						}
					}
						
					DirNode dn = node as DirNode;
					if (dn != null)
					{
						string fullPath = dn.GetFullPath(collection); 
						if (Directory.Exists(fullPath))
							Directory.Delete(fullPath, true);
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
							try
							{
								eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, true, node.Name, 0, 0, 0, Direction.Downloading));
								FileInfo fi = new FileInfo(bfn.GetFullPath(collection));
								if (rights == Access.Rights.ReadOnly)
									fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly;
								fi.Delete();
							}
							catch {}
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
				if (Yield)
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
				if (sn.node != null && sn.node.Length != 0)
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
					workArray.RemoveNodeFromServer(sn.ID);
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
		/// <returns>true if successful.</returns>
		private bool ProcessDirsFromServer()
		{
			bool status = true;
			SyncNode[] updates = null;

			// get small nodes and files from server
			string[] nodeIDs = workArray.DirsFromServer();
			if (nodeIDs.Length == 0)
				return true;
			log.Info("Downloading {0} Directories from server", nodeIDs.Length);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (Yield)
				{
					return false;
				}

				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				try
				{
					string[] batchIDs = new string[batchCount];
					Array.Copy(nodeIDs, offset, batchIDs, 0, batchCount);
					
					updates = service.GetDirs(batchIDs);

					foreach (SyncNode snode in updates)
					{
						if (!StoreDir(snode))
							status = false;
					}
				}
				catch
				{
				}
				offset += batchCount;
			}
			return status;
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
		/// <returns>ture if successful.</returns>
		private bool StoreDir(SyncNode snode)
		{
			try
			{
				if (snode.node != null && snode.node.Length != 0)
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
								try
								{
									Directory.Move(oldPath, path);
								}
								catch (IOException)
								{
									// This directory has already been moved by the parent move.
								}
							}
						}
					}

					if (!Directory.Exists(path))
					{
						try
						{
							Directory.CreateDirectory(path);
						}
						catch
						{
							// Create a collision.
							node = Conflict.CreateNameConflict(collection, node, node.GetFullPath(collection)) as DirNode;
						}
					}
					collection.Commit(node);
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.Directory, false, node.Name, 0, 0, 0, Direction.Downloading));
				}
				workArray.RemoveNodeFromServer(snode.ID);
				return true;
			}
			catch 
			{
				return false;
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
					if (Yield)
					{
						return;
					}

					HttpClientInFile file = new HttpClientInFile(collection, nodeID, service);
					if (file.Open(rights == Access.Rights.ReadOnly ? true : false))
					{
						bool success = false;
						try
						{
							lock (this) {syncFile = file;}
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
							lock (this) {syncFile = null;}
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
				catch (DirectoryNotFoundException)
				{
					// The directory has been deleted.
					workArray.RemoveNodeFromServer(nodeID);
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
								try
								{
									collection.Commit(collection.Delete(node));
								}
								catch
								{
									// The node does not exist. just ignore the error.
								}
							}
							workArray.RemoveNodeToServer(status.nodeID);
						}
						else if(status.status == SyncStatus.Locked)
							break;
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
				if (Yield)
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
							updates[i - offset] = new SyncNode(node);
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
									
								SyncNodeInfo ns = new SyncNodeInfo(node);
								ns.MasterIncarnation++;
								workArray.AddNodeFromServer(ns);
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.Locked:
								// The collection is locked exit.
								i = nodes.Length;
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

		/// <summary>
		/// Copy subdirs to the server.
		/// </summary>
		/// <returns>true if successful.</returns>
		private bool ProcessDirsToServer()
		{
			bool bStatus = true;
			// get small nodes and files from server
			string[] nodeIDs = workArray.DirsToServer();
			if (nodeIDs.Length == 0)
				return true;
			log.Info("Uploading {0} Directories To server", nodeIDs.Length);
				
			// Now get the nodes in groups of BATCH_SIZE.
			int offset = 0;
			while (offset < nodeIDs.Length)
			{
				if (Yield)
				{
					return false;
				}

				int batchCount = nodeIDs.Length - offset < BATCH_SIZE ? nodeIDs.Length - offset : BATCH_SIZE;
				SyncNode[] updates = new SyncNode[batchCount];
				Node[] nodes = new Node[batchCount];
				try
				{
					for (int i = offset; i < offset + batchCount; ++ i)
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						if (node != null & !collection.HasCollisions(node))
						{
							log.Info("Uploading Directory {0} to server", node.Name);
							nodes[i - offset] = node;
							updates[i - offset] = new SyncNode(node);
						}
						else
						{
							// The node no longer exists or has a collision.
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
									
								SyncNodeInfo ns = new SyncNodeInfo(node);
								ns.MasterIncarnation++;
								workArray.AddNodeFromServer(ns);
								workArray.RemoveNodeToServer(node.ID);
								break;
							case SyncStatus.Locked:
								// The collection is locked.
								i = nodes.Length;
								bStatus = false;
								break;
							default:
								log.Debug("Failed update of node {0} due to {1} on server",
									status.nodeID, status.status);
								bStatus = false;
								break;
						}
					}
				}
				catch
				{
					bStatus = false;
				}
				offset += batchCount;
			}
			return bStatus;
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
					if (Yield)
					{
						return;
					}

					BaseFileNode node = collection.GetNodeByID(nodeID) as BaseFileNode;
					if (node != null)
					{
						if (collection.HasCollisions(node))
						{
							// We have a collision do not sync.
							workArray.RemoveNodeFromServer(nodeID);
						}
						HttpClientOutFile file = new HttpClientOutFile(collection, node, service);
						SyncStatus status = file.Open();
						if (status == SyncStatus.Success)
						{
							bool success = false;
							try
							{
								lock (this) {syncFile = file;}
								log.Info("Uploading File {0} to server", file.Name);
								success = file.UploadFile();
							}
							finally
							{
								SyncNodeStatus syncStatus = file.Close(success);
								lock (this) {syncFile = null;}
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
									case SyncStatus.UpdateConflict:
										// Since we had a conflict we need to get the conflict node down.
										workArray.RemoveNodeToServer(nodeID);
										SyncNodeInfo ns = new SyncNodeInfo(node);
										ns.MasterIncarnation++;
										workArray.AddNodeFromServer(ns);
										log.Info("Failed Uploading File {0} : reason {1}", file.Name, syncStatus.status.ToString());
										break;
								}
							}
						}
						else if (status == SyncStatus.FileNameConflict)
						{
							// Since we had a conflict we need to set the conflict.
							BaseFileNode conflictNode = Conflict.CreateNameConflict(collection, node, node.GetFullPath(collection)) as BaseFileNode;
							collection.Commit(conflictNode);
							workArray.RemoveNodeToServer(nodeID);
							log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
							break;
						}
						else
						{
							log.Info("Failed Uploading File {0} : reason {1}", file.Name, status.ToString());
							if (status == SyncStatus.Locked)
								return;
						}
					}
				}
				catch (FileNotFoundException ex)
				{
					// The file no longer exists.
					workArray.RemoveNodeFromServer(nodeID);
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
		Access.Rights	rights;
		bool			sparseReplica = false;
		
		internal class nodeTypeEntry : IComparable
		{
			static int				counter = 0;
			internal string			ID;
			internal SyncNodeType	Type;
			int						EntryNumber;
            
			internal nodeTypeEntry(string ID, SyncNodeType type)
			{
				this.ID = ID;
				this.Type = type;
				this.EntryNumber = Interlocked.Increment(ref counter);
			}

			#region IComparable Members

			public int CompareTo(object obj)
			{
				nodeTypeEntry se = obj as nodeTypeEntry;
				return EntryNumber.CompareTo(se.EntryNumber);
			}

			#endregion
		}

		
		/// <summary>
		/// Array of items to sync.
		/// </summary>
		/// <param name="collection">The collection.</param>
		internal SyncWorkArray(Collection collection)
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
		internal void AddNodeFromServer(SyncNodeInfo stamp)
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
						nodesFromServer[stamp.ID] = new nodeTypeEntry(stamp.ID, SyncNodeType.Tombstone);
					}
					else
					{
						nodesFromServer[stamp.ID] = new nodeTypeEntry(stamp.ID, stamp.NodeType);
					}
				}
			}
		}

		/// <summary>
		/// Determins how this node should be sent to the server.
		/// </summary>
		/// <param name="stamp">The SyncNodeStamp describing this node.</param>
		internal void AddNodeToServer(SyncNodeInfo stamp)
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
						nodesToServer[stamp.ID] = new nodeTypeEntry(stamp.ID, SyncNodeType.Tombstone);
					}
				}
				else if (rights == Access.Rights.ReadOnly)
				{
					Log.log.Debug("Failed Uploading Node (ReadOnly rights)");
				
					// If this node exists on the server.
					if (stamp.MasterIncarnation != 0)
					{
						// We need to get this node from the server.
						stamp.Operation = SyncOperation.Change;
						stamp.LocalIncarnation = stamp.MasterIncarnation + 1;
						AddNodeFromServer(stamp);
					}
				}
				else
				{
					if (stamp.Operation == SyncOperation.Delete)
					{
						nodesToServer[stamp.ID] = new nodeTypeEntry(stamp.ID, SyncNodeType.Tombstone);
					}
					else
					{
						nodesToServer[stamp.ID] = new nodeTypeEntry(stamp.ID, stamp.NodeType);
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
		/// <param name="oType">The Type of objects to return.</param>
		/// <returns></returns>
		private string[] FromServer(SyncNodeType oType)
		{
			ArrayList na = new ArrayList();
			bool haveCollection = false;
			
			// Lets sync the collection first if it is in our list.
			if (oType == SyncNodeType.Generic)
			{
				haveCollection = nodesFromServer.Contains(collection.ID);
				if (haveCollection)
					na.Add(collection.ID);
			}
			
			nodeTypeEntry[] entryArray = new nodeTypeEntry[nodesFromServer.Count];
			nodesFromServer.Values.CopyTo(entryArray, 0);
			Array.Sort(entryArray);
			
			foreach (nodeTypeEntry entry in entryArray)
			{
				if (entry.Type == oType)
				{
					if (haveCollection && entry.ID == collection.ID)
						haveCollection = false;
					else
						na.Add(entry.ID);
				}
			}

			return (string[])na.ToArray(typeof(string));
		}

		/// <summary>
		/// Get an array of the IDs to delete on the client.
		/// </summary>
		/// <returns></returns>
		internal string[] DeletesFromServer()
		{
			return FromServer(SyncNodeType.Tombstone);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] GenericsFromServer()
		{
			return FromServer(SyncNodeType.Generic);
		}

		/// <summary>
		/// Get an array of the IDs of the Directory Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DirsFromServer()
		{
			return FromServer(SyncNodeType.Directory);
		}

		/// <summary>
		/// Get an array of the IDs of the File Nodes to retrieve from the server.
		/// </summary>
		/// <returns></returns>
		internal string[] FilesFromServer()
		{
			return FromServer(SyncNodeType.File);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to push up to the server.
		/// </summary>
		/// <param name="oType">The Type of objects to return.</param>
		/// <returns></returns>
		private string[] ToServer(SyncNodeType oType)
		{
			ArrayList na = new ArrayList();
			bool haveCollection = false;
			
			// Lets sync the collection first if it is in our list.
			if (oType == SyncNodeType.Generic)
			{
				haveCollection = nodesToServer.Contains(collection.ID);
				if (haveCollection)
					na.Add(collection.ID);
			}

			nodeTypeEntry[] entryArray = new nodeTypeEntry[nodesToServer.Count];
			nodesToServer.Values.CopyTo(entryArray, 0);
			Array.Sort(entryArray);
			
			foreach (nodeTypeEntry entry in entryArray)
			{
				if (entry.Type == oType)
				{
					if (haveCollection && entry.ID == collection.ID)
						haveCollection = false;
					else
						na.Add(entry.ID);
				}
			}

			return (string[])na.ToArray(typeof(string));
		}

		/// <summary>
		/// Get an array of the IDs to delete on the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DeletesToServer()
		{
			return ToServer(SyncNodeType.Tombstone);
		}

		/// <summary>
		/// Get an array of the IDs of the Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] GenericsToServer()
		{
			return ToServer(SyncNodeType.Generic);
		}

		/// <summary>
		/// Get an array of the IDs of the Directory Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] DirsToServer()
		{
			return ToServer(SyncNodeType.Directory);
		}

		/// <summary>
		/// Get an array of the IDs of the File Nodes to push up to the server.
		/// </summary>
		/// <returns></returns>
		internal string[] FilesToServer()
		{
			return ToServer(SyncNodeType.File);
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
			if ((oldNode == null && !sparseReplica) || oldNode.MasterIncarnation != MasterIncarnation)
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
		internal Access.Rights SetAccess
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
