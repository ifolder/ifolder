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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using Mono.P2p.mDnsResponderApi;

using Simias.Storage;
using Simias.Sync;

namespace Simias.Location
{
	/// <summary>
	/// Multi-Cast DNS Location Provider
	/// </summary>
	public class MDnsLocationProvider : ILocationProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(MDnsLocationProvider));

		private static readonly string SUFFIX = "_collection._tcp._local";

		private Configuration configuration;

		private IRemoteFactory factory;

		/// <summary>
		/// Static Constructor
		/// </summary>
		static MDnsLocationProvider()
		{
			// channel
			TcpChannel channel = new TcpChannel();
			ChannelServices.RegisterChannel(channel);
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public MDnsLocationProvider()
		{
		}
		
		#region ILocationProvider Members

		/// <summary>
		/// Configure the location provider.
		/// </summary>
		/// <param name="configuration">The Simias configuration object.</param>
		public void Configure(Configuration configuration)
		{
			this.configuration = configuration;

			// factory
			factory = 
				(IRemoteFactory) Activator.GetObject(
				typeof(IRemoteFactory),
				"tcp://localhost:8091/mDnsRemoteFactory.tcp");
		}

		/// <summary>
		/// Locate the collection master.
		/// </summary>
		/// <param name="collection">The collection id.</param>
		/// <returns>A URI object containing the location of the collection master, or null.</returns>
		public Uri Locate(string collection)
		{
			// query
			IResourceQuery query = factory.GetQueryInstance();

			RPtr[] ptrs = null;

			if (query.GetPtrResourcesByName(SUFFIX, out ptrs) == 0)
			{
				foreach(RPtr ptr in ptrs)
				{
				}
			}

			return null;
		}

		/// <summary>
		/// Publish a master collection.
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		public void Publish(string collection)
		{
			// publish
			IResourceRegistration publish = factory.GetRegistrationInstance();

			// host
			IResourceQuery query = factory.GetQueryInstance();

			RHostAddress ha = null;
			string host = null;

			if (query.GetDefaultHost(ref ha) == 0)
			{
				host = ha.Name;
			}
			else
			{
				// TODO: should rely on default host
				host = MyDns.GetHostName() + ".local";

				publish.RegisterHost(host, MyDns.GetHostName());
			}

			Store store = new Store(configuration);

			Collection c = store.GetCollectionByID(collection);

			if (collection != null)
			{
				SyncCollection sc = new SyncCollection(c);
			
				// service
				string service = String.Format("{0}.{1}", sc.ID, SUFFIX);
				int port = sc.MasterUri.Port;

				if (publish.RegisterServiceLocation(host, service, port, 0, 0) == 0)
				{
					log.Debug("Published {0} with Reunion.", service);
				}
				else
				{
					log.Debug("Failed to publish {0} with Reunion.", service);
				}
			}
		}

		#endregion
	}
}
