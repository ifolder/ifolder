/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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

namespace Simias.Sync
{
	/// <summary>
	/// Sync Channel Factory
	/// </summary>
	public class SyncChannelFactory
	{
		private static SyncChannelFactory singleton;

		private ArrayList channels;
		private int index;

		static SyncChannelFactory()
		{
#if DEBUG && !MONO
			// send the errors back on debug
			RemotingConfiguration.CustomErrorsEnabled(false);
#endif
		}

		public static SyncChannelFactory GetInstance()
		{
			if (singleton == null)
			{
				lock(typeof(SyncChannelFactory))
				{
					if (singleton == null)
					{
						singleton = new SyncChannelFactory();
					}
				}
			}
			
			return singleton;
		}

		private SyncChannelFactory()
		{
			channels = new ArrayList();
			index = 0;
		}
		
		public SyncChannel GetChannel(SyncStore store)
		{
			return GetChannel(store, 0);
		}
		
		public SyncChannel GetChannel(SyncStore store, SyncChannelFormatters formatter)
		{
			return GetChannel(store, 0, formatter);
		}
		
		public SyncChannel GetChannel(SyncStore store, int port)
		{
			return GetChannel(store, port, SyncChannelFormatters.Binary);
		}
		
		public SyncChannel GetChannel(SyncStore store, int port, SyncChannelFormatters formatter)
		{
			SyncChannel result = null;

			lock(this)
			{
				if (port != 0)
				{
					foreach(SyncChannel sc in channels)
					{
						if (sc.Port == port)
						{
							result = sc.Open();

							if (sc.Formatter != formatter)
							{
								throw new ApplicationException(
									"Another channel already exists on the requested port with a different formatter.");
							}

							MyTrace.WriteLine("Channel Opened: {0}", sc.Name);

							break;
						}
					}
				}
				
				// channel needs to be created
				if (result == null)
				{
					// name
					string name = String.Format("Sync Channel (port: {0}, index: {1})", port, ++index);

					// setup channel properties
					ListDictionary props = new ListDictionary();
					props.Add("name", name);
					props.Add("port", port);

					// setup providers
					IClientChannelSinkProvider clientProvider = null;
					IServerChannelSinkProvider serverProvider = null;

					if (formatter == SyncChannelFormatters.Soap)
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

#if DEBUG
					IServerChannelSinkProvider debugProvider = new DebugServerChannelSinkProvider();
					debugProvider.Next = serverProvider;
					serverProvider = debugProvider;
#endif
					// The server chain order should be as follows: 
					// SecureServerProvider->DebugProvider->ServerProvider.
					ISecurityServerFactory securityServerFactory = (ISecurityServerFactory) new RsaSecurityServerFactory(store.BaseStore.KeyStore);
					IServerChannelSinkProvider secureServerProvider = (IServerChannelSinkProvider) new SecureServerSinkProvider(securityServerFactory, SecureServerSinkProvider.MsgSecurityLevel.privacy);
					secureServerProvider.Next = serverProvider;

					// The client chain order should be as follows:
					// ClientProvider->ClientSecureProvider.
					ISecurityClientFactory[] secClientFactories = new ISecurityClientFactory[1];
					secClientFactories[0] = (ISecurityClientFactory) new RsaSecurityClientFactory(store.BaseStore.KeyStore);
					IClientChannelSinkProvider clientSecureProvider = (IClientChannelSinkProvider) new SecureClientSinkProvider(secClientFactories);
					clientProvider.Next = clientSecureProvider;

					// create channel
					IChannel channel;

					// http channel
					channel = new HttpChannel(props,
						clientProvider, secureServerProvider);

					// register channel
					ChannelServices.RegisterChannel(channel);

					result = (new SyncChannel(this, channel, name, port, formatter)).Open();

					// add channel
					channels.Add(result);

					MyTrace.WriteLine("Channel Registered: {0} ({1})", name, formatter);
				}
			}
	
			return result;
		}
		
		internal void ReleaseChannel(SyncChannel channel)
		{
			lock(this)
			{
				channels.Remove(channel);
				ChannelServices.UnregisterChannel(channel.Channel);
			}

			MyTrace.WriteLine("Channel Unregistered: {0}", channel.Name);
		}
	}
}
