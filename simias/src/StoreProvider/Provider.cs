/***********************************************************************
 *  Provider.cs - A helper class for providers.
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
 *  Author: Russ Young <ryoung@novell.com>
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
		#region Errors

		/// <summary>
		/// The defined errors.
		/// </summary>
		public enum Error
		{
			/// <summary>
			/// The call completed successfully.
			/// </summary>
			OK = 0,
			/// <summary>
			/// Invalid DataBase Version.
			/// </summary>
			Version = -1,
			/// <summary>
			/// Failed to create object.
			/// </summary>
			Create = -2,
			/// <summary>
			/// The Object already exists.
			/// </summary>
			Exists = -3,
			/// <summary>
			/// Invalid Object Format.
			/// </summary>
			Format = -4,
			/// <summary>
			/// Failed to delete object.
			/// </summary>
			Delete = -5,
			/// <summary>
			/// Transaction Error.
			/// </summary>
			Transaction = -6,
			/// <summary>
			/// Open DB Error.
			/// </summary>
			Open = -7,
		}

		#endregion

		#region Static Methods.
		/// <summary>
		/// Connect to the default provider.
		/// </summary>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(out bool created)
		{
			return LoadProvider(new Configuration(), out created);
		}

		/// <summary>
		/// Connect to the default provider.
		/// </summary>
		/// <param name="path">Path to the store.</param>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(string path, out bool created)
		{
			return (LoadProvider(new Configuration(path), out created));
		}
		
		/// <summary>
		/// Connect to the specified provider.
		/// </summary>
		/// <param name="assembly">Path to the assembly that contains the provider.</param>
		/// <param name="providerType">Name of the class that implements the IProvider interface.</param>
		/// <param name="path"></param>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		public static IProvider Connect(string assembly, string providerType, string path, out bool created)
		{
			Configuration conf = new Configuration(path);
			conf.Assembly = assembly;
			conf.TypeName = providerType;
			return (LoadProvider(conf, out created));
		}
		
		/// <summary>
		/// Connect to the specified provider.
		/// </summary>
		/// <param name="path">The path to the DB.</param>
		/// <param name="created">True if the Store was created.</param>
		/// <returns></returns>
		private static IProvider LoadProvider(Configuration conf, out bool created)
		{
			string path = conf.Path;
			string assembly = conf.Assembly;
			string providerType = conf.TypeName;
			//string path = DbPath;
			created = false;
			IProvider provider = null;

			// Load the assembly and find our provider.
			Assembly pAssembly = AppDomain.CurrentDomain.Load(Path.GetFileNameWithoutExtension(assembly));
			Type pType = null;
			Type[] types = pAssembly.GetExportedTypes();
			foreach (Type t in types)
			{
				if (t.FullName.Equals(providerType))
				{
					pType = t;
					break;
				}
			}

			// If we did not find our type return a null.
			if (pType == null)
			{
				return (null);
			}

			object[] args = {conf};
			object[] activationAttrs = null;
			
			/* This needs to be commented out till I can fix the service code.
			if (Environment.OSVersion.Platform.Equals(PlatformID.Win32NT))
			{
				// Try to create the mutex.
				bool createdMutex;
				string name = "FlaimService_Mutex";

				Mutex mutex = new Mutex(true, name, out createdMutex);
			
				if (!createdMutex)
				{
					// The service is running attach to the remote service.
					object[] parameters = {path};
					activationAttrs = new object[] {new UrlAttribute ("tcp://localhost:1234")};
					//RemotingConfiguration.RegisterActivatedClientType(pType, "tcp://localHost:1234");
				}
				else
				{
					mutex.ReleaseMutex();
				}
			}
			*/

			provider = (IProvider)pAssembly.CreateInstance(providerType, false, 0, null, args, null, activationAttrs);

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
				//DbPath = path;
				created = true;
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
			}
		}
		
		/// <summary>
		/// Gets the default database path.
		/// </summary>
		public static string DefaultPath
		{
			get
			{
				return Simias.Configuration.DefaultPath;
			}
		}
		
		#endregion
	}

	public class Configuration
	{
		private const string CFG_Section = "StoreProvider";
		private const string CFG_Path = "Path";
		private const string CFG_Assembly = "Assembly";
		private const string CFG_TypeName = "Type";
		private const string CFG_Version = "Version";
		private const string StoreName = ".simias";

		Simias.Configuration conf;

		internal Configuration()
		{
			conf = new Simias.Configuration();
		}

		internal Configuration(string path)
		{
			conf = new Simias.Configuration(path);
			Path = conf.BasePath;
		}

		/// <summary>
		/// Gets and Sets the DB Path.
		/// </summary>
		public string Path
		{
			get
			{
				string path = Simias.Configuration.DefaultPath;
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
