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
using Simias.Event;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Store Manager
	/// </summary>
	public class SyncStoreManager : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncStoreManager));

		private Store store;
		private SyncStoreService service;
		private SyncManager syncManager;
		private SyncChannel channel;
		private Hashtable collectionManagers;
		private EventSubscriber subscriber;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="syncManager"></param>
		public SyncStoreManager(SyncManager syncManager)
		{
			// save manager
			this.syncManager = syncManager;

			// store
			store = new Store(syncManager.Config);

			// note: we need to revert any internal impersonations
			store.Revert();

			// collection managers
			collectionManagers = new Hashtable();

			// events
			subscriber = new EventSubscriber(syncManager.Config);
			subscriber.Enabled = false;
			subscriber.NodeTypeFilter = NodeTypes.CollectionType;
			subscriber.NodeCreated += new NodeEventHandler(OnCollectionCreated);
			subscriber.NodeDeleted += new NodeEventHandler(OnCollectionDeleted);
		}

		/// <summary>
		/// Start the sync store manager.
		/// </summary>
		public void Start()
		{
			try
			{
				lock(this)
				{
					// create service
					service = new SyncStoreService(this);

					// create channel
					string name = String.Format("Store Service [{0}]", store.ID);

					channel = syncManager.ChannelFactory.GetChannel(store, syncManager.ChannelSinks,
						syncManager.MasterUri.Port);
				
					log.Debug("Starting Store Service: {0}", service.ServiceUrl);

					// marshal service
					RemotingServices.Marshal(service, service.EndPoint);
				
					// start collection managers
					subscriber.Enabled = true;
					foreach(ShallowNode n in store)
					{
						AddCollectionManager(n.ID);
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start store manager.");

				throw e;
			}
		}

		/// <summary>
		/// Stop the sync store manager.
		/// </summary>
		public void Stop()
		{
			try
			{
				lock(this)
				{
					int port = syncManager.MasterUri.Port;

					if (service != null)
                    {
                        log.Debug("Stopping Store Service: {0}", service.ServiceUrl);
                    }

					// stop collection managers
					subscriber.Enabled = false;
					foreach(string id in new ArrayList(collectionManagers.Keys))
					{
						RemoveCollectionManager(id);
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
				log.Error(e, "Unable to stop store manager.");

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

				log.Debug("Serving Collection Service: {0}", service.Ping().ToString());
			}

			return service;
		}

		private void AddCollectionManager(string id)
		{
			SyncCollectionManager manager;
			
			lock(collectionManagers.SyncRoot)
			{
				if (!collectionManagers.Contains(id))
				{
					log.Debug("Adding Collection Manager: {0}", id);

					try
					{
						manager = new SyncCollectionManager(this, id);
				
						manager.Start();

						collectionManagers.Add(id, manager);
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");
					}
				}
			}
		}

		private void RemoveCollectionManager(string id)
		{
			SyncCollectionManager manager;
			
			lock(collectionManagers.SyncRoot)
			{
				if (collectionManagers.Contains(id))
				{
					log.Debug("Removing Collection Manager: {0}", id);

					try
					{
						manager = (SyncCollectionManager)collectionManagers[id];
			
						manager.Stop();

						collectionManagers.Remove(id);
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");
					}
				}
			}
		}

		private void OnCollectionCreated(NodeEventArgs args)
		{
			AddCollectionManager(args.ID);
		}

		private void OnCollectionDeleted(NodeEventArgs args)
		{
			RemoveCollectionManager(args.ID);
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose of any open resources.
		/// </summary>
		public void Dispose()
		{
			// validate a stop
			Stop();

			// clean-up store
			store.Dispose();
			store = null;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The sync manager object.
		/// </summary>
		public SyncManager Manager
		{
			get { return syncManager; }
		}

		/// <summary>
		/// The store object.
		/// </summary>
		public Store Store
		{
			get { return store; }
		}

		#endregion
	}
}
