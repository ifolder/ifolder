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

		private static readonly string proxyPropertyName = "Proxy";
		
		private static ulong index = 0;

		static SimiasChannelFactory()
		{
#if DEBUG
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
		/// Hidden constructor
		/// </summary>
		private SimiasChannelFactory()
		{
		}

		/// <summary>
		/// Get a Simias channel
		/// </summary>
		/// <param name="uri">Channel URI</param>
		/// <param name="sinks">Channel Sinks</param>
		/// <returns>A Simias Channel</returns>
		public static SimiasChannel Create(Uri uri, SimiasChannelSinks sinks)
		{
			return Create(uri, sinks, false);
		}

		/// <summary>
		/// Get a Simias channel
		/// </summary>
		/// <param name="uri">Channel URI</param>
		/// <param name="sinks">Channel Sinks</param>
		/// <param name="server">Server Channel?</param>
		/// <returns>A Simias Channel</returns>
		public static SimiasChannel Create(Uri uri, SimiasChannelSinks sinks, bool server)
		{
			IChannel channel = null;
			string name = null;

			lock(typeof(SimiasChannelFactory))
			{
				// name
				name = String.Format("Simias Channel {0}", ++index);
			}

			// setup channel properties
			ListDictionary props = new ListDictionary();
			props.Add("name", name);
			
			// server properties
			if (server)
			{
				props.Add("port", uri.Port);
				//props.Add("useIpAddress", true);
				
				//props.Add("bindTo", uri.Host);
			}
			
			// client properties
			else
			{
				props.Add("port", 0);
				props.Add("clientConnectionLimit", 5);

				// proxy
				string proxyName = null;
				string proxyPort = null;
				if (GetProxy(ref proxyName, ref proxyPort))
				{
					props.Add("proxyName", proxyName);
					props.Add("proxyPort", proxyPort);
				}

				// TODO: why doesn't this work?
				//props.Add("timeout", TimeSpan.FromSeconds(30).Milliseconds);

			}

			// common properties
			//props.Add("machineName", uri.Host);

			// provider notes
			// server providers: security sink -> monitor sink -> formatter sink
			// client providers: formatter sink -> monitor sink -> security sink

			// server providers
			if (server)
			{
				IServerChannelSinkProvider serverProvider = null;

				// setup format provider
				if ((sinks & SimiasChannelSinks.Soap) > 0)
				{
					// soap
					serverProvider = new SoapServerFormatterSinkProvider();
					(serverProvider as SoapServerFormatterSinkProvider).TypeFilterLevel = TypeFilterLevel.Full;
				}
				else
				{
					// binary
					serverProvider = new BinaryServerFormatterSinkProvider();
					(serverProvider as BinaryServerFormatterSinkProvider).TypeFilterLevel = TypeFilterLevel.Full;
				}

				// setup monitor provider
				if ((sinks & SimiasChannelSinks.Sniffer) > 0)
				{
					IServerChannelSinkProvider serverMonitorProvider = new SnifferServerChannelSinkProvider();
					serverMonitorProvider.Next = serverProvider;
					serverProvider = serverMonitorProvider;
				}

				// setup security provider
				if ((sinks & SimiasChannelSinks.Security) > 0)
				{
					/* TODO: add back
					ISecurityServerFactory securityServerFactory = (ISecurityServerFactory) new RsaSecurityServerFactory(store.KeyStore);
					IServerChannelSinkProvider serverSecurityProvider = (IServerChannelSinkProvider) new SecureServerSinkProvider(securityServerFactory, SecureServerSinkProvider.MsgSecurityLevel.privacy);
					serverSecurityProvider.Next = serverProvider;
					serverProvider = serverSecurityProvider;
					*/
				}

				// create channel
				if (uri.Scheme.ToLower() == "http")
				{
					// http channel
					channel = new HttpServerChannel(props, serverProvider);
				}
				else
				{
					// tcp channel
					channel = new TcpServerChannel(props, serverProvider);
				}
			}

			// client providers
			else
			{
				IClientChannelSinkProvider clientProvider = null;

				// setup security provider
				if ((sinks & SimiasChannelSinks.Security) > 0)
				{
					/* TODO: add back
					ISecurityClientFactory[] secClientFactories = new ISecurityClientFactory[1];
					secClientFactories[0] = (ISecurityClientFactory) new RsaSecurityClientFactory(store.KeyStore);
					clientProvider = (IClientChannelSinkProvider) new SecureClientSinkProvider(secClientFactories);
					*/
				}

				// setup monitor provider
				if ((sinks & SimiasChannelSinks.Sniffer) > 0)
				{
					IClientChannelSinkProvider clientMonitorProvider = new SnifferClientChannelSinkProvider();
					clientMonitorProvider.Next = clientProvider;
					clientProvider = clientMonitorProvider;
				}

				// setup format provider
				if ((sinks & SimiasChannelSinks.Soap) > 0)
				{
					// soap
					IClientChannelSinkProvider clientFormatProvider = new SoapClientFormatterSinkProvider();
					clientFormatProvider.Next = clientProvider;
					clientProvider = clientFormatProvider;
				}
				else
				{
					// binary
					IClientChannelSinkProvider clientFormatProvider = new BinaryClientFormatterSinkProvider();
					clientFormatProvider.Next = clientProvider;
					clientProvider = clientFormatProvider;
				}


				// create channel
				if (uri.Scheme.ToLower() == "http")
				{
					// http channel
					channel = new HttpClientChannel(props, clientProvider);
				}
				else
				{
					// tcp channel
					channel = new TcpClientChannel(props, clientProvider);
				}
			}

			return new SimiasChannel(channel);
		}

		/// <summary>
		/// Get the proxy setting from the local database.
		/// </summary>
		/// <param name="proxyName">On return, contains the name of the proxy to use.</param>
		/// <param name="proxyPort">On return, contains the value of the port to use.</param>
		/// <returns>This method returns <b>true</b> if the setting is to be used; otherwise, <b>false</b>.</returns>
		public static bool GetProxy(ref string proxyName, ref string proxyPort)
		{
			Collection localDb = Store.GetStore().GetDatabaseObject();

			Property p = localDb.Properties.GetSingleProperty(proxyPropertyName);

			if (p != null)
			{
				// The proxy is stored in the format "n,proxy:port" ... 
				// if n == 3 use the proxy, if n == 1 don't use the proxy.
				string proxyValue = p.Value.ToString();
				return ParseProxyValue(proxyValue, ref proxyName, ref proxyPort);
			}

			return false;
		}

		/// <summary>
		/// Set the proxy setting in the local database.
		/// </summary>
		/// <param name="useProxy">Set to <b>true</b> to use the proxy setting; otherwise, <b>false</b>.</param>
		/// <param name="proxyName">The name of the proxy to store.</param>
		/// <param name="proxyPort">The port of the proxy to store.</param>
		public static void SetProxy(bool useProxy, string proxyName, string proxyPort)
		{
			Collection localDb = Store.GetStore().GetDatabaseObject();

			if ((proxyName != null) && (!proxyName.Equals(String.Empty)) &&
				(proxyPort != null) && (!proxyPort.Equals(String.Empty)))
			{
				Property p = new Property(proxyPropertyName, (useProxy ? "3," : "1,") + proxyName + ":" + proxyPort);
				p.LocalProperty = true;

				localDb.Properties.ModifyProperty(p);
			}
			else
			{
				localDb.Properties.DeleteSingleProperty(proxyPropertyName);
			}

			localDb.Commit();
		}

		/// <summary>
		/// Set the proxy setting in the local database.
		/// </summary>
		/// <param name="proxyValue">The string representing the proxy setting.</param>
		public static void SetProxy(string proxyValue)
		{
			string proxyName = null;
			string proxyPort = null;
			bool useProxy = ParseProxyValue(proxyValue, ref proxyName, ref proxyPort);
			SetProxy(useProxy, proxyName, proxyPort);
		}

		private static bool ParseProxyValue(string proxyValue, ref string proxyName, ref string proxyPort)
		{
			// The proxy is stored in the format "n,proxy:port" ... 
			// if n == 3 use the proxy, if n == 1 don't use the proxy.
			bool useProxy = proxyValue.StartsWith("3");

			// Remove "n," from the beginning of the string.
			proxyValue = proxyValue.Substring(2);

			// Parse to get the proxy and port value.
			int index = proxyValue.IndexOf( ':' );
			if ( index != -1 )
			{
				proxyName = proxyValue.Substring( 0, index );
				proxyPort = proxyValue.Substring( index + 1 );
				return useProxy;
			}

			return false;
		}
	}
}
