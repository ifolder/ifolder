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

		private static readonly string DefaultConfigFile = "Simias";
		private static readonly string ConfigFileExtension = ".remoting";

		private static readonly string RemotingSection = "Remoting";
		private static readonly string HostKey = "Published Host";

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
		/// <param name="config">A Simias configuration object.</param>
		public static void Configure(Configuration config)
		{
			lock(typeof(SimiasRemoting))
			{
				// only configure once
				if (SimiasRemoting.config == null)
				{
					string storePath = config.StorePath;
					string name;
					
					try
					{
						// process name
						name = Assembly.GetEntryAssembly().GetName().Name;
					}
					catch
					{
						// note: this is only known to happen inside of NUnit
						name = "UnknownProcess";
					}

					// config file
					string configFile = Path.Combine(storePath, name + ConfigFileExtension);

					// bootstrap config
					if (!File.Exists(configFile))
					{
						string bootStrapFile = Path.Combine(SimiasSetup.sysconfdir,
							DefaultConfigFile + ConfigFileExtension);

						if (File.Exists(bootStrapFile))
						{
							File.Copy(bootStrapFile, configFile);
						}
					}

					try
					{
						// remoting configuration
						RemotingConfiguration.Configure(configFile);

						// save remoting parameters
						XmlDocument doc = new XmlDocument();
						doc.Load(configFile);

						// port
						XmlNode node = doc.SelectSingleNode("//channel[@port]");
						XmlNode attr = node.Attributes.GetNamedItem("port");
						SimiasRemoting.port = int.Parse(attr.Value);

						// report port
						log.Debug("Server Port: {0}", SimiasRemoting.port);
					}
					catch(Exception e)
					{
						throw new ApplicationException(String.Format(
							"Unable to read required configuration file: {0}",
							configFile), e);
					}

					// save configuration object
					SimiasRemoting.config = config;
				}
			}
		}

		/// <summary>
		/// Generate a service url for the given end point.
		/// </summary>
		/// <param name="endPoint"></param>
		/// <returns></returns>
		public static Uri GetServiceUrl(string endPoint)
		{
			UriBuilder ub = new UriBuilder(ServiceUri);
			ub.Path = endPoint;

			return ub.Uri;
		}

		/// <summary>
		/// The host that is published for remote machines to connect
		/// to the localhost.
		/// </summary>
		public static string Host
		{
			get { return Get(HostKey, MyDns.GetHostName()); }
			set { Set(HostKey, value); }
		}

		/// <summary>
		/// The remoting service uri for remote machines.
		/// </summary>
		public static Uri ServiceUri
		{
			get { return (new UriBuilder("http", Host, port)).Uri; }
		}

		/// <summary>
		/// Get a property from the remoting section of the configuration file.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private static string Get(string key, string defaultValue)
		{
			CheckConfig();

			return config.Get(RemotingSection, key, defaultValue);
		}

		/// <summary>
		/// Set a property in the remoting section of the configuration file.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private static void Set(string key, string value)
		{
			CheckConfig();

			config.Set(RemotingSection, key, value);
		}

		/// <summary>
		/// Check that the Configure() method has been called first.
		/// </summary>
		/// <remarks>
		/// We have to be passed the configuration file to give the
		/// calling application the chance to place the config file
		/// in a custom location.
		///	</remarks>
		private static void CheckConfig()
		{
			if (SimiasRemoting.config == null)
			{
				throw new Exception("SimiasRemoting.Configure() must be called first to initialize the SimiasRemoting class.");
			}
		}
	}
}
