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
using System.IO;
using System.Xml;

namespace Simias.Client
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public sealed class Configuration
	{
		#region Class Members
		internal const string SectionTag = "section";
		internal const string SettingTag = "setting";
		internal const string NameAttr = "name";
		internal const string ValueAttr = "value";
		private const string DefaultSection = "SimiasDefault";
		private const string DefaultFileName = "Simias.config";
		private const string storeProvider = "StoreProvider";
		private const string storeProviderPath = "Path";

		private const string clientBootStrapFileName = "simias-client-bootstrap.config";

		private string configFilePath;
		private XmlDocument configDoc;
		#endregion

		#region Properties
		/// <summary>
		/// Called to get the path where Simias.config is installed.
		/// </summary>
		public string ConfigPath
		{
			get { return this.configFilePath; }
		}

		/// <summary>
		/// Called to get the path where simias is installed.
		/// </summary>
		public string StorePath
		{
			get 
			{ 
				string path = Get(storeProvider, storeProviderPath);
				return (path != null) ? fixupPath(path) : DefaultPath; 
			}
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
		public Configuration()
		{
			// Check to see if the file exists.
			configFilePath = Path.Combine( DefaultPath, DefaultFileName );
			if ( !File.Exists( configFilePath ) )
			{
				string bootStrapFilePath = Path.Combine( SimiasSetup.sysconfdir, clientBootStrapFileName );
				if ( !File.Exists( bootStrapFilePath ) )
				{
					throw new ApplicationException( String.Format( "Cannot locate {0} or {1}", configFilePath, bootStrapFilePath ) );
				}

				// Copy the bootstrap file to the configuration file.
				File.Copy( bootStrapFilePath, configFilePath );
			}

			// Load the configuration document from the file.
			configDoc = new XmlDocument();
			configDoc.Load(configFilePath);
		}
		#endregion

		#region Private Methods
		private XmlElement GetSection(string section)
		{
			return (XmlElement)configDoc.DocumentElement.SelectSingleNode(String.Format("//section[@name='{0}']", section));
		}

		private XmlElement GetKey(string section, string key)
		{
			XmlElement keyElement = null;
			XmlElement sectionElement = GetSection(section);
			if (sectionElement != null)
			{
				string str = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
				keyElement = (XmlElement)sectionElement.SelectSingleNode(str);
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
		/// </summary>
		/// <param name="section">The section where the key is stored.</param>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement or a null if the section or key does not exist.</returns>
		public XmlElement GetElement(string section, string key)
		{
			return GetKey(section, key);
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="key">The key to get the value for.</param>
		/// <returns>The value as a string or a null if the key does not exist.</returns>
		public string Get(string key)
		{
			return Get(DefaultSection, key);
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="section">The section where the key exists.</param>
		/// <param name="key">The key to get the value for.</param>
		/// <returns>The value as a string or a null if the section and key do not exist.</returns>
		public string Get(string section, string key)
		{
			string keyValue = null;

			XmlElement keyElement = GetKey(section, key);
			if (keyElement != null)
			{
				keyValue = keyElement.GetAttribute(ValueAttr);
			}

			return keyValue;
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
			return KeyExists(section, key);
		}
		#endregion
	}
}


