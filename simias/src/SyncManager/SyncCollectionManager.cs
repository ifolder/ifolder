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
using System.IO;

using Simias;
using Simias.Storage;
using Simias.Location;
using Simias.Domain;
using Simias.Channels;

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
		private SimiasChannel channel;
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
			store = Store.GetStore();

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
						// check for the server
						Storage.Domain domain = store.GetDomain(store.DefaultDomain);
						Roster roster = domain.GetRoster(store);
						SyncCollection sc = new SyncCollection(roster);

						// only use register on client (workgroup) machines
						if ((domain.ID == Storage.Domain.WorkGroupDomainID) || (sc.Role == SyncCollectionRoles.Slave))
						{
							// register with the location service
							syncManager.Location.Register(collection.ID);
						}
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
			log.Debug("Stopping {0} - Locking Manager", collection.Name);

			lock(this)
			{
				log.Debug("Stopping {0} - Dispose Channel", collection.Name);

				// release channel
				if (channel != null)
				{
					channel.Dispose();
					channel = null;
				}
				
				log.Debug("Stopping {0} - Stop the Worker", collection.Name);

				// stop worker
				working = false;
				stopSleepEvent.Set();
	
				log.Debug("Stopping {0} - Notify the Sync Engine", collection.Name);

				// send a stop message
				try
				{
					worker.StopSyncWork();
				}
				catch
				{
					// ignore
				}

				log.Debug("Stopping {0} - Join the Sync Engine", collection.Name);

				// join the sync engine
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
			log.Debug("Sync Work {0} - Initiated", collection.Name);

			while(working)
			{
				// huge try/catch for Mono exceptions
				try
				{
					// get permission from sync manager
					syncManager.ReadyToWork();

					log.Debug("Sync Work {0} - Starting", collection.Name);

					try
					{
						// check master
						if (collection.CreateMaster)
						{
							log.Debug("Sync Work {0} - Creating the Master", collection.Name);

							DomainAgent dAgent = new DomainAgent(syncManager.Config);

							dAgent.CreateMaster(collection);
						}
						else
						{
							// get the service URL
							string serviceUrl = collection.MasterUrl.ToString();
							log.Debug("Sync Work {0} - Service URL: {1}", collection.Name, serviceUrl);

							// check the channel
							if (channel == null)
							{
								// create channel
								channel = syncManager.ChannelFactory.GetChannel(store,
									collection.MasterUrl.Scheme, syncManager.ChannelSinks);
							}

							// get a proxy to the store service object
							log.Debug("Sync Work {0} - Connecting...", collection.Name);
							storeService = (SyncStoreService)Activator.GetObject(typeof(SyncStoreService), serviceUrl);
						
							// no store service
							if (storeService == null) throw new ApplicationException("No Sync Store Service");

							// ping the store
							log.Debug("Store Service Ping: {0}", storeService.Ping());

							// get a proxy to the collection service object
							log.Debug("Connecting to the Sync Collection Service...");
							service = storeService.GetCollectionService(collection.ID);
						
							// removed collection?
							if (service == null)
							{
								log.Debug("The collection is no longer on the server.");
								log.Debug("Removing collection from the client.");
							
								// delete the colection
								collection.Commit(collection.Delete());
							
								// stop the slave
								working = false;
								continue;
							}

							// ping the collection
							log.Debug("Collection Service Ping: {0}", service.Ping());

							// get the collection worker
							log.Debug("Creating a Sync Worker Object...");
							if (worker == null)
							{
								worker = syncManager.LogicFactory.GetCollectionWorker(collection);
							}
						
							// bad worker
							if (worker == null) throw new ApplicationException("No Sync Collection Worker");

							// do the work
							log.Debug("Sync Work {0} - Worker Start", collection.Name);
							worker.DoSyncWork(service);
							log.Debug("Sync Work {0} - Worker Done", collection.Name);
						}
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");

						if (working)
						{
							// reset worker
							worker = null;

							try
							{
								// try the location service on an exception
								log.Debug("Querying the Location Service...");

								// find the URL with the location service
								Uri locationUrl = syncManager.Location.Locate(collection.ID);

								// update the URL
								if ((locationUrl != null) && (locationUrl != collection.MasterUrl))
								{
									// try the location service on an exception
									log.Debug("Updating Master Service Url...");

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
					}
					catch
					{
						log.Debug("Caught unknown exception!");
					}
					finally
					{
						storeService = null;
						service = null;
					}

					log.Info("Finished Sync Cycle: {0}", collection.Name);

					// finish with sync manager
					syncManager.DoneWithWork();

					log.Debug("Sync Work {0} - Sleeping", collection.Name);

					// sleep
					stopSleepEvent.WaitOne(TimeSpan.FromSeconds(collection.Interval), true);

					log.Debug("Sync Work {0} - Done Sleeping", collection.Name);
				}
				catch(Exception e)
				{
					// TODO: clean-up
					try
					{
						log.Debug(e, "Ignored");
					}
					catch
					{
						File.CreateText(Path.Combine(Configuration.GetConfiguration().StorePath, "double.exception")).Close();
					}
				}
			}
		}
	}
}
