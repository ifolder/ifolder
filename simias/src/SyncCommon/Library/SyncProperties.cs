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

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Properties
	/// </summary>
	public class SyncProperties : IDisposable
	{
		/// <summary>
		/// The suggested sync host or ip address for the current machine.
		/// </summary>
		private static readonly string DefaultHost = MyDns.GetHostName();
		private static readonly string HostPropertyName = "Sync Host";

		/// <summary>
		/// The suggested sync port for the current machine.
		/// </summary>
		private static readonly int DefaultPort = 6436;
		private static readonly string PortPropertyName = "Sync Port";

		/// <summary>
		/// The suggested sync logic factory for syncing.
		/// </summary>
		private static readonly Type DefaultLogicFactory = typeof(SyncLogicFactory);
		private static readonly string LogicFactoryPropertyName = "Sync Logic Factory";
		
		/// <summary>
		/// The suggested sync interval in seconds.
		/// </summary>
		private static readonly int DefaultInterval = 5;
		private static readonly string IntervalPropertyName = "Sync Interval";

		/// <summary>
		/// The suggested sync channel sinks.
		/// </summary>
		private static readonly SyncChannelSinks DefaultChannelSinks =
#if DEBUG
			SyncChannelSinks.Binary | SyncChannelSinks.Monitor; // | SyncChannelSinks.Security;
#else
			SyncChannelSinks.Binary; // | SyncChannelSinks.Security;
#endif
		private static readonly string ChannelSinksPropertyName = "Sync Channel Sinks";

		// fields
		private Configuration config;
		private Store store;
		private Collection localDb;

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncProperties(Configuration config)
		{
			this.config = config;

			store = new Store(config);
			localDb = store.GetDatabaseObject();
		}

		#region IDisposable Members

		public void Dispose()
		{
			// dispose the store
			if (store != null)
			{
				store.Dispose();
				store = null;
			}
		}

		#endregion

		#region Property Methods

		/// <summary>
		/// Get a property value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <returns>The value of the property.</returns>
		private object GetProperty(string name)
		{
			return GetProperty(name, null);
		}

		/// <summary>
		/// Get a poperty value from the base node.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">A default value to return if the property has no value.</param>
		/// <returns>The property value, if it exists, or the default value.</returns>
		private object GetProperty(string name, object value)
		{
			object result = value;

			Property p = localDb.Properties.GetSingleProperty(name);

			if (p != null)
			{
				result = p.Value;
			}

			return result;
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		private void SetProperty(string name, object value)
		{
			SetProperty(name, value, false);
		}

		/// <summary>
		/// Set the value of the given property of the base node.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The new property value.</param>
		/// <param name="local">Is this a local only property? (non-synced)</param>
		private void SetProperty(string name, object value, bool local)
		{
			if (value != null)
			{
				Property p = new Property(name, value);
				p.LocalProperty = local;

				localDb.Properties.ModifyProperty(p);
			}
			else
			{
				localDb.Properties.DeleteSingleProperty(name);
			}

			localDb.Commit();
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// The Simias configuration object.
		/// </summary>
		public Configuration Config
		{
			get { return config; }
		}

		/// <summary>
		/// The sync host.
		/// </summary>
		public string Host
		{
			get { return (string)GetProperty(HostPropertyName, DefaultHost); }
			set { SetProperty(HostPropertyName, value, true); }
		}

		/// <summary>
		/// The sync port.
		/// </summary>
		public int Port
		{
			get { return (int)GetProperty(PortPropertyName, DefaultPort); }
			set { SetProperty(PortPropertyName, value, true); }
		}

		/// <summary>
		/// The sync interval in seconds.
		/// </summary>
		public int Interval
		{
			get { return (int)GetProperty(IntervalPropertyName, DefaultInterval); }
			set { SetProperty(IntervalPropertyName, value, true); }
		}

		/// <summary>
		/// The sync logic factory.
		/// </summary>
		public Type LogicFactory
		{
			get { return Type.GetType((string)GetProperty(LogicFactoryPropertyName, DefaultLogicFactory.AssemblyQualifiedName)); }
			set { SetProperty(LogicFactoryPropertyName, value.AssemblyQualifiedName, true); }
		}

		/// <summary>
		/// The sync channel sinks.
		/// </summary>
		public SyncChannelSinks ChannelSinks
		{
			get { return (SyncChannelSinks)GetProperty(ChannelSinksPropertyName, DefaultChannelSinks); }
			set { SetProperty(ChannelSinksPropertyName, value, true); }
		}

		#endregion
	}
}
