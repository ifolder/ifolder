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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using System.Reflection;

using Novell.Security.SecureSink;
using Novell.Security.SecureSink.SecurityProvider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

using Simias;
using Simias.Storage;
using Simias.Sniffer;

namespace Simias.Channels
{
	/// <summary>
	/// Simias Channel Factory
	/// </summary>
	public class SimiasChannelFactory
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SimiasChannelFactory));

		private static SimiasChannelFactory singleton;

		private ArrayList channels;
		private int index;

		static SimiasChannelFactory()
		{
#if DEBUG && !MONO
			// send the errors back on debug
			RemotingConfiguration.CustomErrorsEnabled(false);
#endif
			// TODO: remove or update
			string config = "remoting.config";

			try
			{
				RemotingConfiguration.Configure(config);
			}
			catch
			{
				config = "Not Found";
			}

			log.Debug("Remoting Configuration File: {0}", config);
		}

		/// <summary>
		/// The singleton instance of the SimiasChannelFactory.
		/// </summary>
		/// <returns></returns>
		public static SimiasChannelFactory GetInstance()
		{
			if (singleton == null)
			{
				lock(typeof(SimiasChannelFactory))
				{
					if (singleton == null)
					{
						singleton = new SimiasChannelFactory();
					}
				}
			}
			
			return singleton;
		}

		private SimiasChannelFactory()
		{
			channels = new ArrayList();
			index = 0;
		}
		
		/// <summary>
		/// Get a Simias channel
		/// </summary>
		/// <param name="store"></param>
		/// <param name="scheme"></param>
		/// <param name="sinks"></param>
		/// <returns></returns>
		public SimiasChannel GetChannel(Store store, string scheme, SimiasChannelSinks sinks)
		{
			return GetChannel(store, scheme, sinks, 0);
		}
			
		/// <summary>
		/// Get a Simias channel
		/// </summary>
		/// <param name="store"></param>
		/// <param name="scheme"></param>
		/// <param name="sinks"></param>
		/// <param name="port"></param>
		/// <returns></returns>
		public SimiasChannel GetChannel(Store store, string scheme, SimiasChannelSinks sinks, int port)
		{
			SimiasChannel result = null;

			lock(this)
			{
				if (port != 0)
				{
					foreach(SimiasChannel sc in channels)
					{
						if (sc.Port == port)
						{
							result = sc.Open();

							if (sc.Sinks != sinks)
							{
								throw new ApplicationException(
									"Another channel already exists on the requested port with different sinks.");
							}

							log.Debug("Channel Opened: {0}", sc.Name);

							break;
						}
					}
				}
				
				// channel needs to be created
				if (result == null)
				{
					// name
					string name = String.Format("Simias Channel (port: {0}, index: {1})", port, ++index);

					// setup channel properties
					ListDictionary props = new ListDictionary();
					props.Add("name", name);
					props.Add("port", port);

					// provider notes
					// server providers: security sink -> monitor sink -> formatter sink
					// client providers: formatter sink -> monitor sink -> security sink

					// setup format providers
					IClientChannelSinkProvider clientProvider = null;
					IServerChannelSinkProvider serverProvider = null;

					if ((sinks & SimiasChannelSinks.Soap) > 0)
					{
						// soap
						clientProvider = new SoapClientFormatterSinkProvider();

						serverProvider = new SoapServerFormatterSinkProvider();
#if !MONO
						(serverProvider as SoapServerFormatterSinkProvider).TypeFilterLevel = TypeFilterLevel.Full;
#endif
					}
					else
					{
						// binary
						clientProvider = new BinaryClientFormatterSinkProvider();

						serverProvider = new BinaryServerFormatterSinkProvider();
#if !MONO
						(serverProvider as BinaryServerFormatterSinkProvider).TypeFilterLevel = TypeFilterLevel.Full;
#endif
					}

					// setup monitor providers
					if ((sinks & SimiasChannelSinks.Sniffer) > 0)
					{
						IServerChannelSinkProvider serverMonitorProvider = new SnifferServerChannelSinkProvider();
						serverMonitorProvider.Next = serverProvider;
						serverProvider = serverMonitorProvider;

						IClientChannelSinkProvider clientMonitorProvider = new SnifferClientChannelSinkProvider();
						clientProvider.Next = clientMonitorProvider;
					}

					// setup security providers
					if ((sinks & SimiasChannelSinks.Security) > 0)
					{
						/* TODO: add back
						ISecurityServerFactory securityServerFactory = (ISecurityServerFactory) new RsaSecurityServerFactory(store.KeyStore);
						IServerChannelSinkProvider serverSecurityProvider = (IServerChannelSinkProvider) new SecureServerSinkProvider(securityServerFactory, SecureServerSinkProvider.MsgSecurityLevel.privacy);
						serverSecurityProvider.Next = serverProvider;
						serverProvider = serverSecurityProvider;

						ISecurityClientFactory[] secClientFactories = new ISecurityClientFactory[1];
						secClientFactories[0] = (ISecurityClientFactory) new RsaSecurityClientFactory(store.KeyStore);
						IClientChannelSinkProvider clientSecureProvider = (IClientChannelSinkProvider) new SecureClientSinkProvider(secClientFactories);
						

						// TODO: fix with cleaner solution
						if (clientProvider.Next != null)
						{
							clientProvider.Next.Next = clientSecureProvider;
						}
						else
						{
							clientProvider.Next = clientSecureProvider;
						}
						*/
					}

					// create channel
					IChannel channel;

					if (scheme == "http")
                    {
                        // http channel
                        channel = new HttpChannel(props, clientProvider, serverProvider);
                    }
                    else
                    {
                        // tcp channel
                        channel = new TcpChannel(props, clientProvider, serverProvider);
                    }

					// register channel
					ChannelServices.RegisterChannel(channel);

					result = (new SimiasChannel(this, channel, name, port, sinks)).Open();

					// add channel
					channels.Add(result);

					log.Debug("Channel Registered: {0} ({1})", name, sinks);
				}
			}
	
			return result;
		}
		
		internal void ReleaseChannel(SimiasChannel channel)
		{
			lock(this)
			{
				channels.Remove(channel);
				ChannelServices.UnregisterChannel(channel.Channel);
			}

			log.Debug("Channel Unregistered: {0}", channel.Name);
		}
	}
}
