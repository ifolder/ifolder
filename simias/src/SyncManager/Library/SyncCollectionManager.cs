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
using System.Runtime.Remoting.Channels;
using System.Diagnostics;

using Simias;
using Simias.Storage;
using Simias.Location;
using Simias.Domain;

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
		private AutoResetEvent stopSleepEvent = new AutoResetEvent(false);

        /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="storeManager"></param>
		/// <param name="id"></param>
		public SyncCollectionManager(SyncStoreManager storeManager, string id)
		{
			this.syncManager = storeManager.Manager;
			this.storeManager = storeManager;
			
            // open store
			// note: the store provider requires that we open a new store for each thread
			store = new Store(syncManager.Config);

            // note: we need to revert any internal impersonations
			store.Revert();

            // open collection			
			collection = new SyncCollection(store.GetCollectionByID(id));
			Debug.Assert(collection != null);
		}

		/// <summary>
		/// Start the sync collection manager.
		/// </summary>
		public void Start()
		{
			try
			{
                log.Debug("Starting {0} Collection Manager: {1}", collection.Role, collection.Name);
				
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
                log.Debug("Stopping {0} Collection Manager: {1}", collection.Role, collection.Name);
				
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

		internal void SyncNow()
		{
			if (collection.Role == SyncCollectionRoles.Slave)
			{
				stopSleepEvent.Set();
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
				// create worker thread
				syncWorkerThread = new Thread(new ThreadStart(this.DoSyncWork));
				syncWorkerThread.Priority = ThreadPriority.BelowNormal;
				working = true;
				syncWorkerThread.Start();
			}

			log.Debug("{0} Url: {1}", collection.Name, collection.MasterUrl);
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
				working = false;
				stopSleepEvent.Set();
	
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
					// check master
					if (collection.CreateMaster)
					{
						DomainAgent dAgent = new DomainAgent(syncManager.Config);

						log.Debug("Connecting to Domain Service: {0}", dAgent.ServiceUrl);

						// create channel
						SyncChannel domainChannel = syncManager.ChannelFactory.GetChannel(store,
							dAgent.ServiceUrl.Scheme, syncManager.ChannelSinks);

						// connect
						IDomainService dService = dAgent.Connect();

						string nodeID = null;
						string nodeName = null;

						DirNode dirNode = collection.GetRootDirectory();

						if (dirNode != null)
						{
							nodeID = dirNode.ID;
							nodeName = dirNode.Name;
						}

						string uriString = dService.CreateMaster(collection.ID, collection.Name, collection.Owner,
							nodeID, nodeName);

						if (uriString == null)
						{
							throw new ApplicationException("Unable to create remote master collection.");
						}

						Uri master = new Uri(uriString);

						log.Debug("Master URL from Domain Service: {0}", master);

						collection.MasterUrl = master;
						collection.CreateMaster = false;

						domainChannel.Dispose();
					}
					else
					{
						// get the service URL
						string serviceUrl = collection.MasterUrl.ToString();
						log.Debug("Sync Store Service URL: {0}", serviceUrl);

						// check the channel
						if (channel == null)
						{
							// create channel
							channel = syncManager.ChannelFactory.GetChannel(store,
								collection.MasterUrl.Scheme, syncManager.ChannelSinks);
						}

						// get a proxy to the store service object
						log.Debug("Connecting to the Sync Store Service...");
						storeService = (SyncStoreService)Activator.GetObject(typeof(SyncStoreService), serviceUrl);
						if (storeService == null) throw new ApplicationException("No Sync Store Service");

						// ping the store
						log.Debug("Store Service Ping: {0}", storeService.Ping());

						// get a proxy to the collection service object
						log.Debug("Connecting to the Sync Collection Service...");
						service = storeService.GetCollectionService(collection.ID);
						if (service == null) throw new ApplicationException("No Sync Collection Service");

						// ping the collection
						log.Debug("Collection Service Ping: {0}", service.Ping());

						// get the collection worker
						log.Debug("Creating a Sync Worker Object...");
						worker = syncManager.LogicFactory.GetCollectionWorker(service, collection);
						if (worker == null) throw new ApplicationException("No Sync Collection Worker");

						// do the work
						log.Debug("Starting the Sync Worker...");
						worker.DoSyncWork();
					}
				}
				catch(Exception e)
				{
					log.Debug(e, "Ignored");

					try
					{
						// try the location service on an exception
						log.Debug("Querying the Location Service...");

						// find the URL with the location service
						Uri locationUrl = syncManager.Location.Locate(collection.ID);

						// update the URL
						if ((locationUrl != null) && (locationUrl != collection.MasterUrl))
						{
							collection.MasterUrl = locationUrl;

							// clear channel
							if (channel != null)
							{
								channel.Dispose();
								channel = null;
							}
						}
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
				stopSleepEvent.WaitOne(TimeSpan.FromSeconds(collection.Interval), true);
			}
		}
	}
}
