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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;
using System.Security.Principal;
using System.Threading;
using System.Diagnostics;
using Simias;
//using Simias.Storage.Provider.Flaim;

namespace Simias.Storage.Provider
{	
	/// <summary>
	/// Represents a Collection Store Provider.
	/// </summary>
	public abstract class Provider
	{
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(Provider));

		#region Static Methods.

		/// <summary>
		/// Connect to the default provider.
		/// </summary>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(out bool created)
		{
			return Connect(new ProviderConfig(), out created);
		}

		/// <summary>
		/// Connect to the store using the provided configuration.
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="created"></param>
		/// <returns></returns>
		public static IProvider Connect(ProviderConfig conf, out bool created)
		{
			string path = conf.Path;
			string assembly = conf.Assembly;
			string providerType = conf.TypeName;
			created = false;
			IProvider provider = null;

			// Load the assembly and find our provider.
			Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assembly));
			object[] args = {conf};
			object[] activationAttrs = null;
			
			provider = (IProvider)pAssembly.CreateInstance(providerType, false, 0, null, args, null, activationAttrs);
			if (provider != null)
			{
				try
				{
					provider.OpenStore();
				}
				catch
				{
					provider.CreateStore();
					// Set the Provider in the configuration.
					conf.Assembly = assembly;
					conf.TypeName = providerType;
					created = true;
					logger.Info("Created new store {0}.", provider.StoreDirectory);
				}
			}
			
			return (provider);
		}

		/// <summary>
		/// Called to remove the Database path.
		/// </summary>
		public static void Delete(string path)
		{
			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
				logger.Info("Deleted store {0}.", path);
			}
		}
		
		#endregion
	}

	/// <summary>
	/// Store configuration class.
	/// </summary>
	public class ProviderConfig
	{
		private const string CFG_Section = "StoreProvider";
		private const string CFG_Path = "Path";
		private const string CFG_Assembly = "Assembly";
		private const string CFG_TypeName = "Type";
		private const string CFG_Version = "Version";
		private const string StoreName = ".simias";

		Simias.Configuration conf;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ProviderConfig()
		{
			conf = new Simias.Configuration();
			Path = conf.StorePath;
		}

		/// <summary>
		/// Constructor that takes a path.
		/// </summary>
		/// <param name="path">The path to the Configuration.</param>
		public ProviderConfig(string path)
		{
			conf = new Simias.Configuration(path);
			Path = conf.StorePath;
		}

		/// <summary>
		/// Constructor that takes a Simias.Configuration object.
		/// </summary>
		/// <param name="sConf">The the simias configuration object.</param>
		public ProviderConfig(Configuration sConf)
		{
			conf = sConf;
			Path = conf.StorePath;
		}

		/// <summary>
		/// Gets and Sets the DB Path.
		/// </summary>
		public string Path
		{
			get
			{
				string path = conf.StorePath;
				return (conf.Get(CFG_Section, CFG_Path, path));
			}
			set
			{
				conf.Set(CFG_Section, CFG_Path, value);
			}
		}

		/// <summary>
		/// Gets and Sets the Provider Version.
		/// </summary>
		public string Version
		{
			get
			{
				string version = "0";
				return (conf.Get(CFG_Section, CFG_Version, version));
			}
			set
			{
				conf.Set(CFG_Section, CFG_Version, value);
			}
		}

		/// <summary>
		/// Gets and Sets the Assembly that implements the provider instance used.
		/// </summary>
		public string Assembly
		{
			get
			{
				string assembly = "SqliteProvider.dll";
				return (conf.Get(CFG_Section, CFG_Assembly, assembly));
			}
			set
			{
				conf.Set(CFG_Section, CFG_Assembly, value);
			}
		}

		/// <summary>
		/// Gets and Sets the Class Type of the implemented provider.
		/// </summary>
		public string TypeName
		{
			get
			{
				string providerType = "Simias.Storage.Provider.Sqlite.SqliteProvider";
				return (conf.Get(CFG_Section, CFG_TypeName, providerType));
			}
			set
			{
				conf.Set(CFG_Section, CFG_TypeName, value);
			}
		}
	}
}
