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
using System.Threading;

namespace Simias
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public sealed class Configuration
	{
		private static readonly string RootElementTag = "configuration";
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";
		private static readonly string DefaultSection = "SimiasDefault";
		private static readonly string DefaultFileName = "simias.conf";

		private string storePath;
		
		private Mutex mutex = new Mutex(false, "SimiasConfigMutex");

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public Configuration() : this(null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="path">The path to the configuration file.</param>
		public Configuration(string path)
		{
			if (path == null)
			{
				path = DefaultPath;
			}
			else
			{
				path = fixupPath(path);
			}

			this.storePath = path;
			CreateDefaults();
		}

		/*
		internal Configuration(string path, bool remove)
		{
			this.storePath = path;

			if (!File.Exists(ConfigFilePath))
			{
				// create defaults
				CreateDefaults();
			}
		}
		*/

		/// <summary>
		/// 
		/// </summary>
		public void CreateDefaults()
		{
			mutex.WaitOne();
			try
			{
				// If the file does not exist look for defaults.
				if (!File.Exists(ConfigFilePath))
				{
					string bootStrapFile = Path.Combine(SimiasSetup.sysconfdir, DefaultFileName);
					if (File.Exists(bootStrapFile))
					{
						File.Copy(bootStrapFile, ConfigFilePath);
					}
					else
					{
						File.Create(ConfigFilePath).Close();
					}
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Called to get the path where simias is installed.
		/// </summary>
		public string StorePath
		{
			get { return storePath; }
		}

		private string ConfigFilePath
		{
			get { return Path.Combine(storePath, DefaultFileName); }
		}

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
			mutex.WaitOne();
			try
			{
				XmlElement keyElement = GetKey(section, key);
				return keyElement;
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Sets the modified element.  The element must have been retrieved from GetElement.
		/// </summary>
		/// <param name="keyElement">The element to save.</param>
		public void SetElement(XmlElement keyElement)
		{
			mutex.WaitOne();
			try
			{
				keyElement.OwnerDocument.Save(ConfigFilePath);
			}
			finally
			{
				mutex.ReleaseMutex();
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
			mutex.WaitOne();
			try
			{
				string keyValue = null;
				XmlElement keyElement = GetKey(section, key);
				keyValue = keyElement.GetAttribute(ValueAttr);
				if (keyValue == "")
				{
					keyElement.SetAttribute(ValueAttr, defaultValue);
					keyElement.OwnerDocument.Save(ConfigFilePath);
					keyValue = keyElement.GetAttribute(ValueAttr);
				}
				return keyValue;
			}
			finally
			{
				mutex.ReleaseMutex();
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
			mutex.WaitOne();
			try
			{
				XmlElement keyElement = GetKey(section, key);
				keyElement.SetAttribute(ValueAttr, keyValue);
				keyElement.OwnerDocument.Save(ConfigFilePath);
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		/// <summary>
		/// Checks for existence of a specified key.
		/// </summary>
		/// <param name="key">The key to check for existence.</param>
		/// <returns>True if the key exists, otherwise false is returned.</returns>
		public bool Exists( string key )
		{
			return Exists( DefaultSection, key );
		}

		/// <summary>
		/// Checks for existence of a specified section and key.
		/// </summary>
		/// <param name="section">The section for the tuple</param>
		/// <param name="key">The key to set.</param>
		/// <returns>True if the section and key exists, otherwise false is returned.</returns>
		public bool Exists( string section, string key )
		{
			mutex.WaitOne();
			try
			{
				return KeyExists(section, key);
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		// These two methods are going to read the XML document every
		// time they are called but it's a cheap way to have fresh data
		// and this is probably not called all the time
		private XmlElement GetSection(string section)
		{
			XmlElement docElement;
			XmlElement sectionElement;
			
			docElement = GetDocElement();

			string str = string.Format("//section[@name='{0}']", section);
			sectionElement = (XmlElement)docElement.SelectSingleNode(str);

			if(sectionElement == null)
			{
				// Create the Section node
				sectionElement = docElement.OwnerDocument.CreateElement(SectionTag);
				sectionElement.SetAttribute(NameAttr, section);
				docElement.AppendChild(sectionElement);				
			}

			return sectionElement;
		}

		private XmlElement GetKey(string section, string key)
		{
			XmlElement keyElement = null;
			XmlElement sectionElement = GetSection(section);
			string xpath = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
			keyElement = (XmlElement)sectionElement.SelectSingleNode(xpath);
			if (keyElement == null)
			{				
				keyElement = (XmlElement)sectionElement.OwnerDocument.CreateNode(XmlNodeType.Element, SettingTag, "");
				keyElement.SetAttribute(NameAttr, key);
				sectionElement.AppendChild(keyElement);
				keyElement.OwnerDocument.Save(ConfigFilePath);
			}

			return keyElement;
		}

		private bool KeyExists(string section, string key)
		{
			bool foundKey = false;
		
			string xpath = string.Format("//{0}[@{1}='{2}']", SectionTag, NameAttr, section);
			XmlElement sectionElement = (XmlElement)GetDocElement().SelectSingleNode(xpath);
			if(sectionElement != null)
			{
				xpath = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
				if(sectionElement.SelectSingleNode(xpath) != null)
				{
					foundKey = true;
				}
			}

			return foundKey;
		}

		private XmlElement GetDocElement()
		{
			XmlElement docElement = null;

			XmlDocument doc = new XmlDocument();

			try
			{
				doc.Load(ConfigFilePath);
			}
			catch
			{
				doc = new XmlDocument();
				docElement = doc.CreateElement(RootElementTag);
				doc.AppendChild(docElement);
			}

			docElement = doc.DocumentElement;

			return docElement;
		}

		#region Static Methods
		
		private static string DefaultPath
		{
			get
			{
				string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (path == null || path.Length == 0)
				{
					path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				}
				path = fixupPath(path);
				return (path);
			}
		}

		private static string fixupPath(string path)
		{
			if ((path.EndsWith(".simias") == false) &&
				(path.EndsWith(".simias/") == false) &&
				(path.EndsWith(@".simias\") == false))
			{
				path = Path.Combine(path, ".simias");
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}
		#endregion
	}
}


