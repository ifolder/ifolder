/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Manager
	/// </summary>
	public class SyncCollectionManager : IDisposable
	{
		private SyncManager syncManager;
		private SyncStoreManager storeManager;
		private SyncStore store;
		private SyncCollection collection;
		private SyncCollectionService service;
		private CollectionWatcher watcher;
		private bool watching;
		private SyncChannel channel;
		private SyncCollectionWorker worker;

		private Thread syncWorkerThread;
		private bool working;

		public SyncCollectionManager(SyncStoreManager storeManager, string id)
		{
			this.syncManager = storeManager.Manager;
			this.storeManager = storeManager;
			
			// open store and collection
			// note: the store provider requires that we open a new store for each thread
			store = new SyncStore(syncManager.StorePath);
			collection = store.OpenCollection(id);
			Debug.Assert(collection != null);

			// check sync properties
			CheckProperties();

			// watcher
			watching = syncManager.LogicFactory.WatchFileSystem();
			
			if (watching)
			{
				watcher = new CollectionWatcher(syncManager.StorePath, id);
				watcher.ChangedFile += new ChangedFileEventHandler(OnChangedFile);
			}
		}

		private void CheckProperties()
		{
			// collection port default
			if ((collection.Port == -1) || (collection.Host == null) ||
				(collection.Interval == -1))
			{
				// set the port
				if (collection.Port == -1) collection.Port = syncManager.Port;
				
				// set the host
				if (collection.Host == null) collection.Host = syncManager.Host;
				
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
						// start the master
						StartMaster();
				
						// start the watcher
						if (watching) watcher.Start();
						break;

					case SyncCollectionRoles.Slave:
						// start the slave
						StartSlave();
				
						// start the watcher
						if (watching) watcher.Start();
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
						// stop watcher
						if (watching) watcher.Stop();

						// stop the master
						StopMaster();
						break;

					case SyncCollectionRoles.Slave:
						// stop watcher
						if (watching) watcher.Stop();

						// stop the master
						StopSlave();
						break;

					case SyncCollectionRoles.Local:
					default:
						break;
				}
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);

				throw e;
			}
		}

		private void StartMaster()
		{
			lock(this)
			{
				// create channel
				channel = syncManager.ChannelFactory.GetChannel(collection.Port);
				
				// service
				service = syncManager.LogicFactory.GetCollectionService(collection);

				// marshal service
				RemotingServices.Marshal(service, collection.EndPoint);

				MyTrace.WriteLine("{0} Service Url: http://0.0.0.0:{1}/{2}", collection.Name, collection.Port, collection.EndPoint);
			}
		}

		private void StopMaster()
		{
			lock(this)
			{
				// release service
				if (service != null)
				{
					RemotingServices.Disconnect(service);
					service = null;
				}

				// release channel
				if (channel != null)
				{
					channel.Dispose();
					channel = null;
				}
			}
		}

		private void StartSlave()
		{
			lock(this)
			{
				// create channel
				channel = syncManager.ChannelFactory.GetChannel();
				
				// get a proxy to the collection object
				service = (SyncCollectionService)Activator.GetObject(
					syncManager.LogicFactory.GetCollectionServiceType(), collection.Url);

				// get the collection worker
				worker = syncManager.LogicFactory.GetCollectionWorker(service, collection);

				// create worker thread
				syncWorkerThread = new Thread(new ThreadStart(this.DoSyncWork));
				working = true;
				syncWorkerThread.Start();
			}

			MyTrace.WriteLine("{0} Url: {1}", collection.Name, collection.Url);
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
				if (worker != null) worker.StopSyncWork();

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

				MyTrace.WriteLine("Sync Work Starting: {0} ({1})", collection.Name, collection.Url);

				try
				{
					MyTrace.WriteLine("Sync Collection Ping: {0}", service.Ping());

					worker.DoSyncWork();
				}
				catch(Exception e)
				{
					MyTrace.WriteLine(e);
				}

				MyTrace.WriteLine("Sync Work Finished: {0}", collection.Name);

				// finish with sync manager
				syncManager.DoneWithWork();

				// sleep
				if (working) Thread.Sleep(TimeSpan.FromSeconds(collection.Interval));
			}
		}

		private void OnChangedFile(string id)
		{
		}

		#region IDisposable Members

		public void Dispose()
		{
			// validate a stop
			Stop();

			watcher.Dispose();
			watcher = null;

			collection.Dispose();
			collection = null;
		}

		#endregion
	}
}
