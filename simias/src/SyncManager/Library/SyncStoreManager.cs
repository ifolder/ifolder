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
using System.Threading;
using System.Collections;
using System.Runtime.Remoting;

using Simias;
using Simais.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Store Manager
	/// </summary>
	public class SyncStoreManager : IDisposable
	{
		private SyncManager syncManager;
		private Store store;
		private SyncStoreService service;
		private StoreWatcher watcher;
		private SyncChannel channel;
		private Hashtable collectionManagers;

		public SyncStoreManager(SyncManager syncManager)
		{
			// TODO: remove or update
			{
				string config = "remoting.config";

				try
				{
					RemotingConfiguration.Configure(config);
				}
				catch
				{
					config = "Not Found";
				}

				MyTrace.WriteLine("Configuration File: {0}", config);
			}
			
			// save manager
			this.syncManager = syncManager;

			// store
			store = new Store(syncManager.Config);

			// collection managers
			collectionManagers = new Hashtable();

			// watcher
			watcher = new StoreWatcher(syncManager.StorePath);
			watcher.Created += new CreatedCollectionEventHandler(OnCreatedCollection);
			watcher.Deleted += new DeletedCollectionEventHandler(OnDeletedCollection);
		}

		public void Start()
		{
			try
			{
				lock(this)
				{
					int port = syncManager.MasterUri.Port;

					// create channel
					string name = String.Format("Store Service [{0}]", store.ID);

					channel = syncManager.ChannelFactory.GetChannel(store, syncManager.ChannelSinks, port);
				
					MyTrace.WriteLine("Starting Store Service: http://0.0.0.0:{0}/{1}", port, SyncStore.GetEndPoint(port));

					// create service
					service = new SyncStoreService(this);

					// marshal service
					RemotingServices.Marshal(service, SyncStore.GetEndPoint(port));
				
					// start collection managers
					foreach(SyncCollectionManager manager in collectionManagers.Values)
					{
						manager.Start();
					}

					// start the watcher
					watcher.Start();
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
				lock(this)
				{
					int port = syncManager.MasterUri.Port;

					MyTrace.WriteLine("Stopping Store Service: http://0.0.0.0:{0}/{1}", port, SyncStore.GetEndPoint(port));

					// stop watcher
					watcher.Stop();

					// release collections
					foreach(SyncCollectionManager manager in collectionManagers.Values)
					{
						manager.Stop();
					}

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
			catch(Exception e)
			{
				MyTrace.WriteLine(e);

				throw e;
			}
		}

		internal SyncCollectionService GetCollectionService(string id)
		{
			SyncCollectionService service = null;

			if (collectionManagers.Contains(id))
			{
				SyncCollectionManager scm = (SyncCollectionManager)collectionManagers[id];

				if (scm == null)
				{
					throw new ApplicationException("The sync collection manager was not found.");
				}

                service = scm.GetService();

				MyTrace.WriteLine("Serving Collection Service: {0}", service.Ping().ToString());
			}

			return service;
		}

		private void OnCreatedCollection(string id)
		{
			SyncCollectionManager manager;
			
			MyTrace.WriteLine("Creating Collection Manager: {0}", id);

			try
			{
				manager = new SyncCollectionManager(this, id);
				
				manager.Start();

				lock(collectionManagers.SyncRoot)
				{
					collectionManagers.Add(id, manager);
				}
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);
			}
		}

		private void OnDeletedCollection(string id)
		{
			SyncCollectionManager manager;

			MyTrace.WriteLine("Deleting Collection Manager: {0}", id);

			try
			{
				manager = (SyncCollectionManager)collectionManagers[id];
			
				manager.Stop();
				manager.Dispose();

				lock(collectionManagers.SyncRoot)
				{
					collectionManagers.Remove(id);
				}
			}
			catch(Exception e)
			{
				MyTrace.WriteLine(e);
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose of any open resources.
		/// </summary>
		public void Dispose()
		{
			// validate a stop
			Stop();

			lock(collectionManagers.SyncRoot)
			{
				// release collections
				foreach(SyncCollectionManager manager in collectionManagers.Values)
				{
					manager.Dispose();
				}

				collectionManagers.Clear();
			}

			watcher.Dispose();
			watcher = null;

			store.Dispose();
			store = null;
		}

		#endregion

		#region Properties

		public SyncManager Manager
		{
			get { return syncManager; }
		}

		public SyncStore Store
		{
			get { return store; }
		}

		#endregion
	}
}
