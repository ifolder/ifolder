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

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Manager
	/// </summary>
	public class SyncCollectionManager
	{
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

		public SyncCollectionManager(SyncStoreManager storeManager, string id)
		{
			this.syncManager = storeManager.Manager;
			this.storeManager = storeManager;
			
			// open store and collection
			// note: the store provider requires that we open a new store for each thread
			store = new Store(syncManager.Config);
			collection = new SyncCollection(store.GetCollectionByID(id));
			Debug.Assert(collection != null);

			// check sync properties
			CheckProperties();
		}

		private void CheckProperties()
		{
			// collection port default
			if ((collection.MasterUri == null) || (collection.Interval == -1))
			{
				// set the host
				if (collection.MasterUri == null) collection.MasterUri = syncManager.MasterUri;
				
				// set the sync interval
				if (collection.Interval == -1) collection.Interval = syncManager.SyncInterval;
				
				// save the defaults
				collection.Commit();
			}
		}

		public void Start()
		{
			try
			{
				switch(collection.Role)
				{
					case SyncCollectionRoles.Master:
						// ?
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
				MyTrace.WriteLine(e);

				throw e;
			}
		}

		public void Stop()
		{
			try
			{
				switch(collection.Role)
				{
					case SyncCollectionRoles.Master:
						// ?
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
				MyTrace.WriteLine(e);

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
				working = true;
				syncWorkerThread.Start();
			}

			MyTrace.WriteLine("{0} Url: {1}", collection.Name, collection.ServiceUrl);
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

				MyTrace.WriteLine("Sync Cycle Starting: {0}", collection.Name);

				// TODO: the remoting connection is currently being created with each sync interval,
				// once we have more confidence in remoting the connection should be created less often
				try
				{
					// Find the url with the location service
					UriBuilder serviceUri = new UriBuilder(collection.ServiceUrl);
					Uri locationUri = syncManager.Location.Locate(collection.ID);

					if (locationUri != null)
					{
						serviceUri.Host = locationUri.Host;
						serviceUri.Port = locationUri.Port;
					}

					string serviceUrl = serviceUri.ToString();

					MyTrace.WriteLine("Sync Store Service URL: {0}", serviceUrl);

					// get a proxy to the store service object
					MyTrace.WriteLine("Connecting to the Sync Store Service...");
					storeService = (SyncStoreService)Activator.GetObject(typeof(SyncStoreService), collection.ServiceUrl);
					Debug.Assert(storeService != null);

					// get a proxy to the collection service object
					MyTrace.WriteLine("Connecting to the Sync Collection Service...");
					service = storeService.GetCollectionService(collection.ID);
					Debug.Assert(service != null);

					// debug
					MyTrace.WriteLine("Pinging the Sync Collection Service...");
					MyTrace.WriteLine(service.Ping().ToString());

					// get the collection worker
					MyTrace.WriteLine("Creating a Sync Worker Object...");
					worker = syncManager.LogicFactory.GetCollectionWorker(service, collection);
					Debug.Assert(worker != null);

					// do the work
					MyTrace.WriteLine("Starting the Sync Worker...");
					worker.DoSyncWork();
				}
				catch(Exception e)
				{
					MyTrace.WriteLine(e);
				}
				finally
				{
					storeService = null;
					service = null;
					worker = null;
				}

				MyTrace.WriteLine("Sync Cycle Finished: {0}", collection.Name);

				// finish with sync manager
				syncManager.DoneWithWork();

				// sleep
				if (working) Thread.Sleep(TimeSpan.FromSeconds(collection.Interval));
			}
		}
	}
}
