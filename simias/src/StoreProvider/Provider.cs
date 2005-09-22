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
using System.Text;
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
		/// Connect to the store using the provided configuration.
		/// </summary>
		/// <param name="conf"></param>
		/// <param name="created"></param>
		/// <returns></returns>
		public static IProvider Connect(ProviderConfig conf, out bool created)
		{
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
					created = true;
					logger.Info("Created new store {0}.", conf.Path);
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
		private const string StoreName = "simias";
		private string assembly = null;
		private string typeName = null;
		private Hashtable settingTable = new Hashtable();

		private Simias.Configuration conf;
		private string storePath;
		

		/// <summary>
		/// Default Constructor
		/// </summary>
		public ProviderConfig(Configuration config, string storePath)
		{
			this.conf = config;
			this.storePath = storePath;
		}

		/// <summary>
		/// Gets and Sets the DB Path.
		/// </summary>
		public string Path
		{
			get
			{
				return storePath;
			}
		}


		/// <summary>
		/// Gets and Sets the Assembly that implements the provider instance used.
		/// </summary>
		public string Assembly
		{
			get
			{
				lock(this)
				{
					if (assembly == null)
					{
						assembly = conf.Get(CFG_Section, CFG_Assembly);
						if ( assembly == null )
						{
							assembly = "SimiasLib.dll";
						}
					}
				}
				return assembly;
			}
		}

		/// <summary>
		/// Gets and Sets the Class Type of the implemented provider.
		/// </summary>
		public string TypeName
		{
			get
			{
				lock(this)
				{
					if (typeName == null)
					{
						typeName = conf.Get(CFG_Section, CFG_TypeName);
						if ( typeName == null )
						{
							typeName = "Simias.Storage.Provider.Flaim.FlaimProvider";
						}
					}
				}
				return typeName;
			}
		}

		/// <summary>
		/// Get the specified configuration setting.
		/// </summary>
		/// <param name="key">The setting to retrieve.</param>
		/// <param name="defaultValue">The default setting.</param>
		/// <returns>The stored setting.</returns>
		public string Get(string key, string defaultValue)
		{
			string setting;
			lock (this)
			{
				if (!settingTable.Contains(key))
				{
					setting = conf.Get(CFG_Section, key);
					if ( setting == null )
					{
						setting = defaultValue;
					}

					settingTable[key] = setting;
				}
			}
			return (string)settingTable[key];
		}
	}

	/// <summary>
	///
	/// </summary>
	public class CommitException : SimiasException
	{
		/// <summary>
		///
		/// </summary>
		public XmlDocument CreateDoc;
		/// <summary>
		///
		/// </summary>
		public XmlDocument DeleteDoc;

		/// <summary>
		///
		/// </summary>
		/// <param name="createDoc"></param>
		/// <param name="deleteDoc"></param>
		/// <param name="ex"></param>
		public CommitException(XmlDocument createDoc, XmlDocument deleteDoc, Exception ex) :
			base("Failed to commite Records", ex)
		{
			CreateDoc = createDoc;
			DeleteDoc = deleteDoc;
		}

		/// <summary>
		///
		/// </summary>
		public override string Message
		{
			get
			{
                StringBuilder sb = new StringBuilder();

				if (CreateDoc != null)
				{
					sb.Append("Failed to create:" + Environment.NewLine);
					XmlElement root = CreateDoc.DocumentElement;
					XmlNodeList recordList = root.SelectNodes(XmlTags.ObjectTag);
					foreach (XmlElement recordEl in recordList)
					{
						sb.Append(String.Format("{0}, {1}{2}", recordEl.GetAttribute(XmlTags.IdAttr), recordEl.GetAttribute(XmlTags.NameAttr), Environment.NewLine));
					}
				}

				if (DeleteDoc != null)
				{
					sb.Append("Failed to delete:" + Environment.NewLine);
					XmlElement root = DeleteDoc.DocumentElement;
					XmlNodeList recordList = root.SelectNodes(XmlTags.ObjectTag);
					foreach (XmlElement recordEl in recordList)
					{
						sb.Append(String.Format("{0}, {1}{2}", recordEl.GetAttribute(XmlTags.IdAttr), recordEl.GetAttribute(XmlTags.NameAttr), Environment.NewLine));
					}
				}
				return (sb.ToString());
			}
		}

	}
}
