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
using System.Diagnostics;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Domain;
using Simias.Storage;
using Simias.Sync;

namespace Simias.POBox
{
	/// <summary>
	/// PO Manager
	/// </summary>
	public class POManager : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(POManager));

		private Store store;
		private Configuration config;
		private Hashtable boxManagers;
		private EventSubscriber subscriber;
//		private PostOffice service;
		private Uri serviceUrl = null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Simias configuration</param>
		public POManager(Configuration config)
		{
			this.config = config;
			
			// store
			store = Store.GetStore();

			// box managers
			boxManagers = new Hashtable();

			// events
			subscriber = new EventSubscriber();
			subscriber.Enabled = true;
			subscriber.NodeTypeFilter = NodeTypes.CollectionType;
			subscriber.NodeCreated += new NodeEventHandler(OnPOBoxCreated);
			subscriber.NodeDeleted += new NodeEventHandler(OnPOBoxDeleted);
		}
		/// <summary>
		/// Start the PO Box manager.
		/// </summary>
		public void Start()
		{
			try
			{
				lock(this)
				{
					log.Debug("Starting PO Service: {0}", ServiceUrl);

					// Get a list of all POBoxes.
					ICSList poBoxList = store.GetCollectionsByType(typeof(POBox).Name);
					foreach(ShallowNode sn in poBoxList)
					{
						// Get the domain for this POBox.
						POBox poBox = new POBox(store, sn);
						Simias.Storage.Domain domain = store.GetDomain(poBox.Domain);
						//if (domain.Role == SyncRoles.Slave)
						//{
							// start collection managers
							AddPOBoxManager(poBox.ID);
						//}
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start PO manager.");

				throw e;
			}
		}

		/// <summary>
		/// Stop the PO Box manager.
		/// </summary>
		public void Stop()
		{
			try
			{
				lock(this)
				{
					/*
					if (service != null)
					{
						log.Debug("Stopping PO Service: {0}", ServiceUrl);
					}
					*/

					// stop collection managers
					subscriber.Enabled = false;
					foreach(string id in new ArrayList(boxManagers.Keys))
					{
						RemovePOBoxManager(id);
					}

					/*
					// release service
					if (service != null)
					{
						RemotingServices.Disconnect(service);
						service = null;
					}
					*/
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop store manager.");

				throw e;
			}
		}

		private void AddPOBoxManager(string id)
		{
			POBoxManager manager;
			
			lock(boxManagers.SyncRoot)
			{
				if (!boxManagers.Contains(id))
				{
					log.Debug("Adding PO Box Manager: {0}", id);

					try
					{
						manager = new POBoxManager(this, id);
			
						manager.Start();

						boxManagers.Add(id, manager);
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");
					}
				}
			}
		}

		private void RemovePOBoxManager(string id)
		{
			POBoxManager manager;
			
			lock(boxManagers.SyncRoot)
			{
				if (boxManagers.Contains(id))
				{
					log.Debug("Removing PO Box Manager: {0}", id);

					try
					{
						manager = (POBoxManager)boxManagers[id];
			
						manager.Stop();

						manager.Dispose();

						boxManagers.Remove(id);
					}
					catch(Exception e)
					{
						log.Debug(e, "Ignored");
					}
				}
			}
		}

		private void OnPOBoxCreated(NodeEventArgs args)
		{
			Collection c = store.GetCollectionByID(args.ID);

			if (c.IsType(c, typeof(POBox).Name))
			{
				AddPOBoxManager(args.ID);
			}
		}

		private void OnPOBoxDeleted(NodeEventArgs args)
		{
			lock(boxManagers.SyncRoot)
			{
				if (boxManagers.Contains(args.ID))
				{
					// This POBox is being deleted. Call to get rid of the domain information.
					DomainAgent agent = new DomainAgent();
					agent.RemoveDomainInformation((boxManagers[args.ID] as POBoxManager).Domain);
					RemovePOBoxManager(args.ID);
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Stop();
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// Service URL
		/// </summary>
		public Uri ServiceUrl
		{
			get { return serviceUrl; }
			set { serviceUrl = value; }
		}

		/// <summary>
		/// Configuration object
		/// </summary>
		public Configuration Config
		{
			get { return config; }
		}

		#endregion
	}
}
