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
using Simias.Channels;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Properties
	/// </summary>
	public class SyncProperties : IDisposable
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncProperties));

		/// <summary>
		/// The suggested service url for the current machine.
		/// </summary>
		private static readonly Uri DefaultServiceUrl = (new UriBuilder("http",
			MyDns.GetHostName(), 6436, SyncStoreService.EndPoint)).Uri;
		
		private static readonly string ServiceUrlPropertyName = "Service Url";

		/// <summary>
		/// The suggested sync logic factory for syncing.
		/// </summary>
		private static readonly Type DefaultLogicFactory = typeof(SyncLogicFactory);
		private static readonly string LogicFactoryPropertyName = "Sync Logic Factory";
		
		/// <summary>
		/// The suggested channel sinks.
		/// </summary>
		private static readonly SimiasChannelSinks DefaultChannelSinks =
#if DEBUG
			SimiasChannelSinks.Binary; // | SimiasChannelSinks.Sniffer; // | SimiasChannelSinks.Security;
#else
			SimiasChannelSinks.Binary; // |SimiasChannelSinks.Security;
#endif
		private static readonly string ChannelSinksPropertyName = "Simias Channel Sinks";

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

			store = Store.GetStore();
			localDb = store.GetDatabaseObject();
		}

		#region IDisposable Members

		public void Dispose()
		{
			// dispose the store
			if (store != null)
			{
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

			// refresh the collection to get the latest information
			localDb.Refresh();

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
		public Uri ServiceUrl
		{
			//get { return new Uri((string)GetProperty(ServiceUrlPropertyName, DefaultServiceUrl.ToString())); }
			//set { SetProperty(ServiceUrlPropertyName, value.ToString(), true); }
			
			// TODO: save in the configuration temporarily
			get { return new Uri(config.Get("Sync", ServiceUrlPropertyName, DefaultServiceUrl.ToString())); }
			set { config.Set("Sync", ServiceUrlPropertyName, value.ToString()); }
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
		public SimiasChannelSinks ChannelSinks
		{
			get { return (SimiasChannelSinks)GetProperty(ChannelSinksPropertyName, DefaultChannelSinks); }
			set { SetProperty(ChannelSinksPropertyName, value, true); }
		}

		#endregion
	}
}
