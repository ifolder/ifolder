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
using System.Collections;
using System.Text.RegularExpressions;

using Mono.P2p.mDnsResponderApi;

using Simias.Storage;
using Simias.Sync;

namespace Simias.Location
{
	/// <summary>
	/// Multi-Cast DNS Location Provider
	/// </summary>
	public class mDnsLocationProvider : ILocationProvider
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(mDnsLocationProvider));

		public static readonly string SUFFIX = "_collection._tcp._local";

		private string host;

		private Configuration configuration;

		private IRemoteFactory factory;

		/// <summary>
		/// Static Constructor
		/// </summary>
		static mDnsLocationProvider()
		{
			// channel
			TcpClientChannel channel = new TcpClientChannel("mDnsClient",
				new BinaryClientFormatterSinkProvider());
			ChannelServices.RegisterChannel(channel);
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		public mDnsLocationProvider()
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
			Uri result = null;

			// query
			IResourceQuery query = factory.GetQueryInstance();

			ServiceLocation sl = null;
		
			string service = String.Format("{0}.{1}", collection, SUFFIX);

			if (query.GetServiceByName(service, ref sl) == 0)
			{
				log.Debug("Located {0} with Reunion.", service);

				int port = sl.Port;

				HostAddress ha = null;

				if (query.GetHostByName(sl.Target, ref ha) == 0)
				{
					TextStrings ts = null;

					if (query.GetTextStringsByName(sl.Target, ref ts) == 0)
					{
						// text strings
						ArrayList list = ts.GetTextStrings();
						string scheme = "http";

						foreach(string item in list)
						{
							string[] pair = Regex.Split(item, "(=)");

							if (pair[0] == "scheme")
							{
								scheme = pair[2];
							}
						}

						UriBuilder ub = new UriBuilder(scheme, ha.PrefAddress.ToString(), port,
							SyncStoreService.EndPoint);

						result = ub.Uri;

						log.Debug("Reunion URI for {0}: {1}", service, result);
					}
				}
			}
			
			return result;
		}

		/// <summary>
		/// Register a master collection.
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		public void Register(string collection)
		{
			// register
			IResourceRegistration register = factory.GetRegistrationInstance();

			// host
			IResourceQuery query = factory.GetQueryInstance();

			HostAddress ha = null;

			if (query.GetDefaultHost(ref ha) == 0)
			{
				host = ha.Name;
			}
			else
			{
				// TODO: should rely on default host
				host = MyDns.GetHostName() + ".local";

				register.RegisterHost(host, MyDns.GetHostName());
			}

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(collection);

			if (collection != null)
			{
				SyncCollection sc = new SyncCollection(c);
			
				// service
				string service = String.Format("{0}.{1}", sc.ID, SUFFIX);

				// ptr
				register.RegisterPointer(SUFFIX, service);
				
				// service location
				if (register.RegisterServiceLocation(host,
					service, sc.MasterUrl.Port, 0, 0) == 0)
				{
					log.Debug("Registered {0} with Reunion.", service);

					ArrayList list = new ArrayList();

					list.Add(String.Format("name={0}", sc.Name));
					list.Add(String.Format("domain={0}", sc.Domain));
					list.Add(String.Format("type={0}", sc.Type));
					list.Add(String.Format("scheme={0}", sc.MasterUrl.Scheme));

					register.RegisterTextStrings(service, (string[]) list.ToArray(typeof(string)));
				}
				else
				{
					log.Debug("Failed to register {0} with Reunion.", service);
				}
			}

		}

		/// <summary>
		/// Unregister a master collection.
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		public void Unregister(string collection)
		{
			IResourceRegistration register = factory.GetRegistrationInstance();

			string service = String.Format("{0}.{1}", collection, SUFFIX);

			register.DeregisterTextStrings(service);
			register.DeregisterServiceLocation(host, service);
			register.DeregisterPointer(SUFFIX, service);

			log.Debug("Unregistered {0} with Reunion.", service);
		}

		#endregion
	}
}
