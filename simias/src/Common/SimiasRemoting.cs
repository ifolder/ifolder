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
using System.Xml;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Diagnostics;

namespace Simias
{
	/// <summary>
	/// Simias remoting configuration class.
	/// </summary>
	public class SimiasRemoting
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SimiasRemoting));

		private static readonly string DefaultConfigFile = "Simias.remoting";
		private static readonly string ConfigFileExtension = ".remoting";

		private static readonly string RemotingSection = "Remoting";
		private static readonly string HostKey = "Public Host";

		private static Configuration config = null;
		private static int port = 0;

		/// <summary>
		/// Default Constructor
		/// </summary>
		private SimiasRemoting()
		{
		}

		/// <summary>
		/// Configure remoting to a specific Simias store.
		/// </summary>
		/// <param name="configuration">A Simias configuration object.</param>
		public static void Configure(Configuration config)
		{
			lock(typeof(SimiasRemoting))
			{
				// only configure once
				if (SimiasRemoting.config == null)
				{
					string storePath = config.StorePath;

					// process name
					string name = Assembly.GetEntryAssembly().GetName().Name;

					// config file
					string configFile = Path.Combine(storePath, name + ConfigFileExtension);

					// bootstrap config
					if (!File.Exists(configFile))
					{
						string bootStrapFile = Path.Combine(SimiasSetup.sysconfdir, DefaultConfigFile);

						if (File.Exists(bootStrapFile))
						{
							File.Copy(bootStrapFile, configFile);
						}
					}

					// remoting configuration
					RemotingConfiguration.Configure(configFile);

					// save remoting parameters
					XmlDocument doc = new XmlDocument();
					doc.Load(configFile);

					XmlNode node = doc.SelectSingleNode("//channel[@port]");
					XmlNode attr = node.Attributes.GetNamedItem("port");
					SimiasRemoting.port = int.Parse(attr.Value);

					log.Debug("Server Port: {0}", SimiasRemoting.port);

					// save configuration object
					SimiasRemoting.config = config;
				}
			}
		}

		public static Uri GetServiceUrl(string endPoint)
		{
			UriBuilder ub = new UriBuilder(ServiceUri);
			ub.Path = endPoint;

			return ub.Uri;
		}

		public static string Host
		{
			get 
			{ 
				if(config == null)
					return MyDns.GetHostName();
				else
					return Get(HostKey, MyDns.GetHostName()); 
			}
			set { Set(HostKey, value); }
		}

		public static Uri ServiceUri
		{
			get { return (new UriBuilder("http", Host, port)).Uri; }
		}

		private static string Get(string key, string defaultValue)
		{
			Debug.Assert(config != null);

			return config.Get(RemotingSection, key, defaultValue);
		}

		private static void Set(string key, string value)
		{
			Debug.Assert(config != null);

			config.Set(RemotingSection, key, value);
		}
	}
}
