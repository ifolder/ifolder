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
 *			Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/
	 
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;

namespace Simias
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public sealed class Configuration
	{
		#region Class Members
		private const string RootElementTag = "configuration";
		private const string SectionTag = "section";
		private const string SettingTag = "setting";
		private const string NameAttr = "name";
		private const string ValueAttr = "value";
		private const string DefaultSection = "SimiasDefault";
		private const string DefaultFileName = "Simias.config";

		private const string enterpriseServer = "Enterprise";
		private const string serverBootStrapFileName = "simias-server-bootstrap.config";
		private const string clientBootStrapFileName = "simias-client-bootstrap.config";
		private const string storeProvider = "StoreProvider";
		private const string storeProviderPath = "Path";

		private static Configuration instance = null;

		private string configFilePath;
		private string storePath;
		private XmlDocument configDoc;
		#endregion

		#region Properties
		/// <summary>
		/// Called to get the path where simias is installed.
		/// </summary>
		public string StorePath
		{
			get { return storePath; }
		}

		private static string DefaultPath
		{
			get
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if ((path == null) || (path.Length == 0))
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}

				return fixupPath(path);
			}
		}
		#endregion
		
		#region Constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		private Configuration()
		{
			string bootStrapFilePath = null;
			configFilePath = Path.Combine( DefaultPath, DefaultFileName );

			// See if we are running as a client or server.
			NameValueCollection nvc = System.Configuration.ConfigurationSettings.AppSettings;
			if (nvc.Get( enterpriseServer ) != null )
			{
				// Enterprise server.
				bootStrapFilePath = Path.Combine( SimiasSetup.sysconfdir, serverBootStrapFileName );
			}
			else
			{
				// Client
				bootStrapFilePath = Path.Combine( SimiasSetup.sysconfdir, clientBootStrapFileName );
			}

			// Check to see if the file already exists.
			if ( !File.Exists( configFilePath ) )
			{
				if ( !File.Exists( bootStrapFilePath ) )
				{
					throw new SimiasException( String.Format( "Cannot locate {0} or {1}", configFilePath, bootStrapFilePath ) );
				}

				// Copy the bootstrap file to the configuration file.
				File.Copy( bootStrapFilePath, configFilePath );
			}

			// Load the configuration document from the file.
			configDoc = new XmlDocument();
			configDoc.Load(configFilePath);

			// Get or set the store path.
			storePath = fixupPath( Get( storeProvider, storeProviderPath, Path.GetDirectoryName( configFilePath ) ) );
		}
		#endregion

		#region Factory  Methods
		/// <summary>
		/// Gets the instance of the configuration object for this process.
		/// </summary>
		/// <returns>A reference to the configuration object.</returns>
		static public Configuration GetConfiguration()
		{
			lock (typeof(Configuration))
			{
				if (instance == null)
				{
					instance = new Configuration();
				}

				return instance;
			}
		}

		/// <summary>
		/// Creates the default instance of the configuration.
		/// </summary>
		/// <param name="path">Path to where the data store is.</param>
		/// <returns>A reference to the configuration object.</returns>
		[ Obsolete( "This method is obsolete. Use GetConfiguration() instead.", false ) ]
		static public Configuration CreateDefaultConfig(string path)
		{
			return GetConfiguration();
		}
		#endregion

		#region Private Methods
		private XmlElement GetSection(string section, ref bool changed)
		{
			string str = string.Format("//section[@name='{0}']", section);
			XmlElement sectionElement = (XmlElement)configDoc.DocumentElement.SelectSingleNode(str);
			if(sectionElement == null)
			{
				// Create the Section node
				sectionElement = configDoc.CreateElement(SectionTag);
				sectionElement.SetAttribute(NameAttr, section);
				configDoc.DocumentElement.AppendChild(sectionElement);
				changed = true;
			}

			return sectionElement;
		}

		private XmlElement GetKey(string section, string key, ref bool changed)
		{
			// Get the section that the key belongs to.
			XmlElement sectionElement = GetSection(section, ref changed);

			string str = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
			XmlElement keyElement = (XmlElement)sectionElement.SelectSingleNode(str);
			if (keyElement == null)
			{				
				// Create the key.
				keyElement = configDoc.CreateElement(SettingTag);
				keyElement.SetAttribute(NameAttr, key);
				sectionElement.AppendChild(keyElement);
				changed = true;
			}

			return keyElement;
		}

		private bool KeyExists(string section, string key)
		{
			bool foundKey = false;
		
			string str = string.Format("//{0}[@{1}='{2}']", SectionTag, NameAttr, section);
			XmlElement sectionElement = (XmlElement)configDoc.DocumentElement.SelectSingleNode(str);
			if(sectionElement != null)
			{
				str = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
				if(sectionElement.SelectSingleNode(str) != null)
				{
					foundKey = true;
				}
			}

			return foundKey;
		}

		private void UpdateConfigFile()
		{
			XmlTextWriter xtw = new XmlTextWriter(configFilePath, Encoding.ASCII);
			try
			{
				xtw.Formatting = Formatting.Indented;
				configDoc.WriteTo(xtw);
			}
			finally
			{
				xtw.Close();
			}
		}
		
		private static string fixupPath(string path)
		{
			if ((path.EndsWith("simias") == false) &&
				(path.EndsWith("simias/") == false) &&
				(path.EndsWith(@"simias\") == false))
			{
				path = Path.Combine(path, "simias");
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// Creates the key if does not exist.
		/// </summary>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement.</returns>
		public XmlElement GetElement(string key)
		{
			return GetElement(DefaultSection, key);
		}

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// Creates the key if does not exist.
		/// </summary>
		/// <param name="section">The section where the key is stored.</param>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement.</returns>
		public XmlElement GetElement(string section, string key)
		{
			lock(typeof(Configuration))
			{
				bool changed = false;
				XmlElement element = GetKey(section, key, ref changed);
				if (changed)
				{
					UpdateConfigFile();
				}

				return element.Clone() as XmlElement;
			}
		}

		/// <summary>
		/// Sets the modified element.  The element must have been retrieved from GetElement.
		/// </summary>
		/// <param name="section">Section that the key belongs in.</param>
		/// <param name="key">Key to set new element into.</param>
		/// <param name="newElement">The element to save.</param>
		public void SetElement(string section, string key, XmlElement newElement)
		{
			lock(typeof(Configuration))
			{
				bool changed = false;
				XmlElement keyElement = GetKey(section, key, ref changed);
				keyElement.InnerXml = newElement.InnerXml;
				UpdateConfigFile();
			}
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="key">The key to get the value for.</param>
		/// <param name="defaultValue">The default value if no value exists.</param>
		/// <returns>The value as a string.</returns>
		public string Get(string key, string defaultValue)
		{
			return Get(DefaultSection, key, defaultValue);
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="section">The section where the key exists.</param>
		/// <param name="key">The key to get the value for.</param>
		/// <param name="defaultValue">The default value if no value exists.</param>
		/// <returns>The value as a string.</returns>
		public string Get(string section, string key, string defaultValue)
		{
			lock(typeof(Configuration))
			{
				bool changed = false;
				XmlElement keyElement = GetKey(section, key, ref changed);
				string keyValue = keyElement.GetAttribute(ValueAttr);
				if (keyValue == string.Empty)
				{
					if (defaultValue != null )
					{
						keyElement.SetAttribute(ValueAttr, defaultValue);
						keyValue = defaultValue;
						changed = true;
					}
					else
					{
						keyValue = null;
					}
				}

				if (changed)
				{
					UpdateConfigFile();
				}

				return keyValue;
			}
		}

		/// <summary>
		/// Set a Key and value pair.
		/// </summary>
		/// <param name="key">The key to set.</param>
		/// <param name="keyValue">The value of the key.</param>
		public void Set(string key, string keyValue)
		{
			Set(DefaultSection, key, keyValue);
		}

		/// <summary>
		/// Set a key and value pair.
		/// </summary>
		/// <param name="section">The section for the tuple</param>
		/// <param name="key">The key to set.</param>
		/// <param name="keyValue">The value of the key.</param>
		public void Set(string section, string key, string keyValue)
		{
			lock(typeof(Configuration))
			{
				bool changed = false;
				XmlElement keyElement = GetKey(section, key, ref changed);
				keyElement.SetAttribute(ValueAttr, keyValue);
				UpdateConfigFile();
			}
		}

		/// <summary>
		/// Checks for existence of a specified key.
		/// </summary>
		/// <param name="key">The key to check for existence.</param>
		/// <returns>True if the key exists, otherwise false is returned.</returns>
		public bool Exists(string key)
		{
			return Exists(DefaultSection, key);
		}

		/// <summary>
		/// Checks for existence of a specified section and key.
		/// </summary>
		/// <param name="section">The section for the tuple</param>
		/// <param name="key">The key to set.</param>
		/// <returns>True if the section and key exists, otherwise false is returned.</returns>
		public bool Exists(string section, string key)
		{
			lock(typeof(Configuration))
			{
				return KeyExists(section, key);
			}
		}

		/// <summary>
		/// Deletes the specified key from the default section.
		/// </summary>
		/// <param name="key">Key to delete.</param>
		public void DeleteKey(string key)
		{
			DeleteKey(DefaultSection, key);
		}

		/// <summary>
		/// Deletes the specified key from the specified section.
		/// </summary>
		/// <param name="section">Section to delete key from.</param>
		/// <param name="key">Key to delete.</param>
		public void DeleteKey(string section, string key)
		{
			lock(typeof(Configuration))
			{
				// Check if the key exists.
				if (KeyExists(section, key))
				{
					bool changed = false;
					XmlElement sectionElement = GetSection(section, ref changed);
					XmlElement keyElement = GetKey(section, key, ref changed);
					sectionElement.RemoveChild(keyElement);
					UpdateConfigFile();
				}
			}
		}
		#endregion
	}
}


