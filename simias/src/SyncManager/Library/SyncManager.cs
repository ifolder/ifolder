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

namespace Simias.Sync
{
	/// <summary>
	/// Sync manager states.
	/// </summary>
	public enum SyncManagerStates
	{
		// The sync manager is syncing (or transfering) files.
		Syncing,

		// The sync manager is active.
		Active,

		// They sync manager is idle.
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
		private SyncChannelFactory channelFactory;
		private SyncStoreManager storeManager;
		private SyncLogicFactory logicFactory;
		private LocationService locationService;

		private int active;
		private object activeLock = new object();

		public SyncManager(): this(new SyncProperties())
		{
		}

		public SyncManager(SyncProperties properties)
		{
			// properties
			this.properties = (SyncProperties)properties.Clone();

			// logic factory
			logicFactory = (SyncLogicFactory)Activator.CreateInstance(properties.DefaultLogicFactory);

			// channel factory
			channelFactory = SyncChannelFactory.GetInstance();

			// store
			storeManager = new SyncStoreManager(this);

			// no one is working
			active = 0;

			// TODO: fix sync manager to use configuration
			// create the location service
			Configuration configuration = new Configuration(properties.StorePath);

			locationService = new LocationService(configuration);
		}

		public void Start()
		{
			lock(this)
			{
				// start the store manager
				storeManager.Start();
			}
		}

		public void Stop()
		{
			lock(this)
			{
				// stop the store manager
				storeManager.Stop();
			}
		}

		internal void ReadyToWork()
		{
			MyTrace.WriteLine("Ready Work: {0}", active);

			lock(activeLock)
			{
				++active;

				if ((active == 1) && (ChangedState != null))
				{
					ChangedState(SyncManagerStates.Active);
				}
			}
		}

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

			MyTrace.WriteLine("Done Work: {0}", active);
		}

		#region IDisposable Members

		public void Dispose()
		{
			// validate stop
			Stop();
		}

		#endregion

		#region Properties

		public Uri MasterUri
		{
			get
			{
				UriBuilder builder = new UriBuilder("http", properties.DefaultHost, properties.DefaultPort);
				return builder.Uri;
			}
		}
		
		public string StorePath
		{
			get { return properties.StorePath; }
		}

		public int SyncInterval
		{
			get { return properties.DefaultSyncInterval; }
		}

		public SyncStoreManager StoreManager
		{
			get { return storeManager; }
		}

		public SyncChannelFactory ChannelFactory
		{
			get { return channelFactory; }
		}

		public SyncChannelSinks ChannelSinks
		{
			get { return properties.DefaultChannelSinks; }
		}

		public SyncLogicFactory LogicFactory
		{
			get { return logicFactory; }
		}

		public LocationService Location
		{
			get { return locationService; }
		}

		#endregion
	}
}
