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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Threading;
using System.Runtime.Remoting;
using System.Diagnostics;

using Simias;
using Simias.Storage;
using Simias.Location;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Manager
	/// </summary>
	public class SyncCollectionManager
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncCollectionManager));

		private SyncManager syncManager;
		private SyncStoreManager storeManager;
		private Store store;
		private SyncCollection collection;
		private SyncChannel channel;
		private SyncStoreService storeService;
		private SyncCollectionService service;
		private SyncCollectionWorker worker;
		private Thread syncWorkerThread;
		private bool working;
		private ManualResetEvent stopWorkingEvent = new ManualResetEvent(true);

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="storeManager"></param>
		/// <param name="id"></param>
		public SyncCollectionManager(SyncStoreManager storeManager, string id)
		{
			this.syncManager = storeManager.Manager;
			this.storeManager = storeManager;
			
			// open store and collection
			// note: the store provider requires that we open a new store for each thread
			store = new Store(syncManager.Config);
			collection = new SyncCollection(store.GetCollectionByID(id));
			Debug.Assert(collection != null);

			// note: we need to revert any internal impersonations
			store.Revert();
		}

		/// <summary>
		/// Start the sync collection manager.
		/// </summary>
		public void Start()
		{
			try
			{
				switch(collection.Role)
				{
					case SyncCollectionRoles.Master:
						// register with the location service
						syncManager.Location.Register(collection.ID);
						break;

					case SyncCollectionRoles.Slave:
						// start the slave
						StartSlave();
						break;

					case SyncCollectionRoles.Local:
					default:
						// ?
						break;
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start collection manager.");

				throw e;
			}
		}

		/// <summary>
		/// Stop the sync collection manager.
		/// </summary>
		public void Stop()
		{
			try
			{
				switch(collection.Role)
				{
					case SyncCollectionRoles.Master:
						// unregister with the location service
						syncManager.Location.Unregister(collection.ID);
						break;

					case SyncCollectionRoles.Slave:
						// stop the master
						StopSlave();
						break;

					case SyncCollectionRoles.Local:
					default:
						// ?
						break;
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop collection manager.");

				throw e;
			}
		}

		internal SyncCollectionService GetService()
		{
			// refresh collection
			collection.Refresh();
			
			// service
			return syncManager.LogicFactory.GetCollectionService(collection);
		}

		private void StartSlave()
		{
			lock(this)
			{
				// create channel
				channel = syncManager.ChannelFactory.GetChannel(store, syncManager.ChannelSinks);
				
				// create worker thread
				syncWorkerThread = new Thread(new ThreadStart(this.DoSyncWork));
				syncWorkerThread.Priority = ThreadPriority.BelowNormal;
				working = true;
				stopWorkingEvent.Reset();
				syncWorkerThread.Start();
			}

			log.Debug("{0} Url: {1}", collection.Name, collection.ServiceUrl);
		}

		private void StopSlave()
		{
			lock(this)
			{
				// release channel
				if (channel != null)
				{
					channel.Dispose();
					channel = null;
				}
				
				// stop worker
				stopWorkingEvent.Set();
				working = false;
	
				// send a stop message
				try
				{
					worker.StopSyncWork();
				}
				catch
				{
					// ignore
				}

				try
				{
					syncWorkerThread.Join();
				}
				catch
				{
					// ignore
				}
			}
		}

		private void DoSyncWork()
		{
			while(working)
			{
				// get permission from sync manager
				syncManager.ReadyToWork();

				log.Info("Starting Sync Cycle: {0}", collection.Name);

				// TODO: the remoting connection is currently being created with each sync interval,
				// once we have more confidence in remoting the connection should be created less often
				try
				{
					// get the service URL
					string serviceUrl = collection.ServiceUrl;
					log.Debug("Sync Store Service URL: {0}", serviceUrl);

					// get a proxy to the store service object
					log.Debug("Connecting to the Sync Store Service...");
					storeService = (SyncStoreService)Activator.GetObject(typeof(SyncStoreService), collection.ServiceUrl);
					if (storeService == null) throw new ApplicationException("No Sync Store Service");

					// get a proxy to the collection service object
					log.Debug("Connecting to the Sync Collection Service...");
					service = storeService.GetCollectionService(collection.ID);
					if (service == null) throw new ApplicationException("No Sync Collection Service");

					// get the collection worker
					log.Debug("Creating a Sync Worker Object...");
					worker = syncManager.LogicFactory.GetCollectionWorker(service, collection);
					if (worker == null) throw new ApplicationException("No Sync Collection Worker");

					// do the work
					log.Debug("Starting the Sync Worker...");
					worker.DoSyncWork();
				}
				catch(Exception e)
				{
					log.Debug(e, "Ignored");

					try
					{
						// try the location service on an exception
						log.Debug("Querying the Location Service...");

						// find the URL with the location service
						Uri locationUri = syncManager.Location.Locate(collection.ID);

						// update the URL
						if (locationUri != null) collection.MasterUri = locationUri;
					}
					catch(Exception e2)
					{
						log.Debug(e2, "Ignored");
					}
				}
				finally
				{
					storeService = null;
					service = null;
					worker = null;
				}

				log.Info("Finished Sync Cycle: {0}", collection.Name);

				// finish with sync manager
				syncManager.DoneWithWork();

				// sleep
				stopWorkingEvent.WaitOne(TimeSpan.FromSeconds(collection.Interval), true);
			}
		}
	}
}
