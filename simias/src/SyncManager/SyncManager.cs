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
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

using Simias;
using Simias.Location;
using Simias.Channels;

namespace Simias.Sync
{
	/// <summary>
	/// Sync manager states.
	/// </summary>
	public enum SyncManagerStates
	{
		/// <summary>
		/// The sync manager is syncing (or transfering) files.
		/// </summary>
		Syncing,

		/// <summary>
		/// The sync manager is active.
		/// </summary>
		Active,

		/// <summary>
		/// They sync manager is idle.
		/// </summary>
		Idle,
	};

	/// <summary>
	/// The sync manager state has changed event handler.
	/// </summary>
	public delegate void ChangedSyncStateEventHandler(SyncManagerStates state);
	
	/// <summary>
	/// Sync Manager
	/// </summary>
	public class SyncManager : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncManager));

		/// <summary>
		/// Occurs when the sync state has changed.
		/// </summary>
		public event ChangedSyncStateEventHandler ChangedState;

		private enum RemotingStates
		{
			// The sync remoting state is good.
			Good,

			// The sync remoting state is ok.
			Ok,

			// The sync remoting state is bad.
			Bad,
		};

		private SyncProperties properties;
		private SyncStoreManager storeManager;
		private SyncLogicFactory logicFactory;
		private LocationService locationService;
		private Configuration config;

		private int active;
		private object activeLock = new object();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="config"></param>
		public SyncManager(Configuration config)
		{
			this.config = config;

			// properties
			this.properties = new SyncProperties(config);

			// logic factory
			logicFactory = (SyncLogicFactory)Activator.CreateInstance(properties.LogicFactory);

			// store
			storeManager = new SyncStoreManager(this);

			// no one is working
			active = 0;

			// create the location service
			locationService = new LocationService(config);
		}

		/// <summary>
		/// Start the sync manager
		/// </summary>
		public void Start()
		{
			lock(this)
			{
				// start the store manager
				storeManager.Start();
			}
		}

		/// <summary>
		/// Stop the sync manager
		/// </summary>
		public void Stop()
		{
			lock(this)
			{
				// stop the store manager
				storeManager.Stop();
			}
		}

		public void SyncCollectionNow(string id)
		{
			storeManager.SyncCollectionNow(id);
		}

		public void SyncAllNow()
		{
			storeManager.SyncAllNow();
		}

		/// <summary>
		/// Collection syncing gate method.
		/// </summary>
		internal void ReadyToWork()
		{
			log.Debug("Ready Work: {0}", active);

			lock(activeLock)
			{
				++active;

				if ((active == 1) && (ChangedState != null))
				{
					ChangedState(SyncManagerStates.Active);
				}
			}
		}

		/// <summary>
		/// Collection syncing gate method.
		/// </summary>
		internal void DoneWithWork()
		{
			lock(activeLock)
			{
				--active;
				
				if ((active == 0) && (ChangedState != null))
				{
					ChangedState(SyncManagerStates.Idle);
				}
			}

			log.Debug("Done Work: {0}", active);
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose the sync manager.
		/// </summary>
		public void Dispose()
		{
			// validate stop
			Stop();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The local service Url.
		/// </summary>
		public Uri ServiceUrl
		{
			get { return properties.ServiceUrl; }
		}
		
		/// <summary>
		/// The store path.
		/// </summary>
		public string StorePath
		{
			get { return config.StorePath; }
		}

		/// <summary>
		/// The default sync interval.
		/// </summary>
		public int SyncInterval
		{
			get { return properties.Interval; }
		}

		/// <summary>
		/// The sync store manager.
		/// </summary>
		public SyncStoreManager StoreManager
		{
			get { return storeManager; }
		}

		/// <summary>
		/// An enumeration of the sinks to include.
		/// </summary>
		public SimiasChannelSinks ChannelSinks
		{
			get { return properties.ChannelSinks; }
		}

		/// <summary>
		/// The default logic factory
		/// </summary>
		public SyncLogicFactory LogicFactory
		{
			get { return logicFactory; }
		}

		/// <summary>
		/// The default location service.
		/// </summary>
		public LocationService Location
		{
			get { return locationService; }
		}

		/// <summary>
		/// The configuration object.
		/// </summary>
		public Configuration Config
		{
			get { return config; }
		}

		#endregion
	}
}
