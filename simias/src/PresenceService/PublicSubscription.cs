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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
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
using Simias.POBox;

namespace Simias.Presence
{
	/// <summary>
	/// Multi-Cast DNS Location Provider
	/// </summary>
	public class PublicSubscription
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(PublicSubscription));

		public static readonly string NAME = "_public-collection._tcp.local";


		/// <summary>
		/// Static Constructor
		/// </summary>
		public PublicSubscription()
		{
		}




		/// <summary>
		/// Returns all public subscriptions
		/// </summary>
		/// <returns>An Array of SubscriptionInfo objects.</returns>
		public static SubscriptionInfo[] GetSubscriptions()
		{
			IRemoteFactory factory = null;

			try
			{
				TcpChannel chnl = new TcpChannel();
				ChannelServices.RegisterChannel(chnl);
			}
			catch(Exception e){}

			factory = (IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");

			ArrayList subArray = new ArrayList();

			if(factory != null)
			{
				IResourceQuery query = factory.GetQueryInstance();
				if(query != null)
				{
					Ptr[] recordPtrs = null;
					if (query.GetPtrResourcesByName(NAME, out recordPtrs) == 0)
					{
						TextStrings ts = null;
						HostAddress ha = null;
						ServiceLocation sl = null;

						foreach(Ptr recordPtr in recordPtrs)
						{
							if(recordPtr != null)
							{
								bool isPublic = false;
								SubscriptionInfo si = new SubscriptionInfo();

								if(query.GetTextStringsByName(recordPtr.Target,
											ref ts) == 0)
								{
									// parse out strings and add them
									// to the subscription info
									foreach(string strval in ts.GetTextStrings())
									{
										string[] pair = 
											Regex.Split(strval, "(=)");
										
										if(pair[0] == "domain")
											si.DomainID = pair[2];
										else if(pair[0] == "name")
											si.SubscriptionCollectionName = 
														pair[2];
										else if(pair[0] == "id")
											si.SubscriptionCollectionID =
														pair[2];
										else if(pair[0] == "hasDirNode")
										{
											if(pair[2] == "true")
												si.SubscriptionCollectionHasDirNode = true;
											else
												si.SubscriptionCollectionHasDirNode = true;
										}
										else if(pair[0] == "public")
										{
											if(pair[2] == "true")
												isPublic = true;
										}
									}
								}

								if(query.GetServiceByName(recordPtr.Target,
											ref sl) == 0)
								{
									if(query.GetHostByName(sl.Target,
												ref ha) == 0)
									{
										string url = "http://" + ha.PrefAddress + ":" + sl.Port + "/PostOffice.rem";

										si.POServiceUrl = new Uri(url);
										if(isPublic)
											subArray.Add(si);
									}
								}
							}
						}
					}
				}
			}

		    return (SubscriptionInfo[])subArray.ToArray(typeof(SubscriptionInfo));
		}




		/// <summary>
		/// Adds a public subcription for the collection
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		public static void AddSubscription(string collection)
		{
			IRemoteFactory factory = null;

			try
			{
				TcpChannel chnl = new TcpChannel();
				ChannelServices.RegisterChannel(chnl);
			}
			catch(Exception e){}

			factory = (IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");

			IResourceRegistration register = factory.GetRegistrationInstance();

			IResourceQuery query = factory.GetQueryInstance();

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(collection);

			if (collection != null)
			{
				SyncCollection sc = new SyncCollection(c);

				// service
				string service = String.Format("{0}.{1}", sc.Name, NAME);
				string host;

				// ptr
				register.RegisterPointer(NAME, service);

				Roster roster = store.GetDomain(sc.Domain).GetRoster(store);
				SyncCollection rostersc = new SyncCollection(roster);

				HostAddress ha = null;

				if (query.GetDefaultHost(ref ha) == 0)
				{
					host = ha.Name;
				}
				else
				{
					// TODO: should rely on default host
					host = MyDns.GetHostName() + ".local";

					try
					{
						register.RegisterHost(host, rostersc.MasterUrl.Host);
					}
					catch(Exception e)
					{
						log.Debug(e.Message);
						throw new ApplicationException("Badd stuff");
					}
				}

				// service location
				if (register.RegisterServiceLocation(host,
							service, rostersc.MasterUrl.Port, 0, 0) == 0)
				{
					ArrayList list = new ArrayList();

					list.Add(String.Format("name={0}", sc.Name));
					list.Add(String.Format("domain={0}", sc.Domain));
					list.Add(String.Format("id={0}", sc.ID));
					list.Add("public=true");
					if(sc.GetRootDirectory() != null)
						list.Add("hasDirNode=true");
					else
						list.Add("hasDirNode=false");

					register.RegisterTextStrings(service, (string[]) list.ToArray(typeof(string)));
				}
				else
				{
					log.Debug("Failed to register {0} with Reunion.", service);
				}
				sc.Dispose();
			}
		}

		/// <summary>
		/// Remove a public subscription
		/// </summary>
		/// <param name="collection">The collection ID.</param>
		public static void RemoveSubscription(string collection)
		{
			IRemoteFactory factory = null;

			try
			{
				TcpChannel chnl = new TcpChannel();
				ChannelServices.RegisterChannel(chnl);
			}
			catch(Exception e){}

			factory = (IRemoteFactory) Activator.GetObject(
					typeof(IRemoteFactory),
					"tcp://localhost:8091/mDnsRemoteFactory.tcp");
			string host = MyDns.GetHostName() + ".local";
			IResourceRegistration register = factory.GetRegistrationInstance();

			Store store = Store.GetStore();

			Collection c = store.GetCollectionByID(collection);

			string service = String.Format("{0}.{1}", c.Name, NAME);

			register.DeregisterTextStrings(service);
			register.DeregisterServiceLocation(host, service);
			register.DeregisterPointer(NAME, service);
		}
	}
}
