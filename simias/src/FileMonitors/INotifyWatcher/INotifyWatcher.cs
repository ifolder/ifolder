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
 *  Author: banderso@novell.com
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Threading;
using System.Web;

using Simias;
using Simias.Client;
using Simias.Client.Event;
using Simias.Event;
using Simias.Storage;
using Simias.Sync;
using Simias.Web;

namespace Simias.FileMonitor.INotifyWatcher
{
	/// <summary>
	/// INotifyWatcher Manager
	/// </summary>
	public class Manager : IDisposable
	{
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(typeof(Simias.FileMonitor.INotifyWatcher.Manager));
			
		private AutoResetEvent	stopEvent;
		private bool			started = false;
		private	bool			stop = false;
		private Configuration	config;
		private EventSubscriber subscriber;
		private Thread			watcherThread = null;
		private Store			store;

		private void OnCreated(NodeEventArgs args)
		{
			log.Info(args.ID);

			try
			{
				Collection newCollection = store.GetCollectionByID(args.ID);
				DirNode dirNode = newCollection.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = 
						Simias.Web.SharedCollection.GetUriPath(dirNode.GetFullPath(newCollection));
					log.Info("Watching collection: " + colPath.ToString());
								
					// Add this puppy
				}
			}
			catch(Exception e)
			{
				log.Info(e.Message);
				log.Info(e.StackTrace);
			}
		}

		private void OnDeleted(NodeEventArgs args)
		{
			log.Info(args.ID);

			try
			{
				Collection cCollection = store.GetCollectionByID(args.ID);
				DirNode dirNode = cCollection.GetRootDirectory();
				if(dirNode != null)
				{
					Uri colPath = 
						Simias.Web.SharedCollection.GetUriPath(dirNode.GetFullPath(cCollection));
					log.Info("Removing watched collection: " + colPath.ToString());
								
					// remove this puppy
				}
			}
			catch(Exception e)
			{
				log.Info(e.Message);
				log.Info(e.StackTrace);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config">Simias configuration</param>
		public Manager(Configuration config)
		{
			this.config = config;
			
			// store
			store = Store.GetStore();
		}

		/// <summary>
		/// Start the Watcher.
		/// </summary>
		public void Start()
		{
			log.Info("INotifyWatcher.Manager.Start called");

			try
			{
				lock(this)
				{
					if (started == false)
					{
						// For testing
						Thread.Sleep(30000);

						/*
							Get all the current collections that contain file or
							directory nodes and start watching.
							
							Also, subscribe to a new collection event so we can watch
							any new ones that come into the system	
						*/

						ICSList collections = 
							store.GetCollectionsByDomain(store.DefaultDomain);

						foreach(ShallowNode shallowNode in collections)
						{
							Collection cCollection = store.GetCollectionByID(shallowNode.ID);
							DirNode dirNode = cCollection.GetRootDirectory();
							if(dirNode != null)
							{
								Uri colPath = 
									Simias.Web.SharedCollection.GetUriPath(dirNode.GetFullPath(cCollection));
								log.Info("Watching collection: " + colPath.ToString());
								
								// Add this puppy
							}
						}

						// Subscribe to all events from all collections
						subscriber = new EventSubscriber();
						subscriber.Enabled = true;
					
						//subscriber.NodeTypeFilter = NodeTypes.NodeType;
						subscriber.NodeCreated += new NodeEventHandler(OnCreated);
						subscriber.NodeDeleted += new NodeEventHandler(OnDeleted);
					}
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to start the INotifyWatcher service.");
				throw e;
			}
		}

		/// <summary>
		/// Stop the Domain Watcher Thread.
		/// </summary>
		public void Stop()
		{
			log.Info("INotifyWatcher.Manager.Stop called");
			try
			{
				lock(this)
				{
					// Set state and then signal the event
					this.stop = true;
				}
			}
			catch(Exception e)
			{
				log.Error(e, "Unable to stop INotifyWatcher.");
				throw e;
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
	}
}
