/***********************************************************************
 *  ComponentConfiguration.cs - A Generic Configuration Class
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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 * 
 ***********************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Threading;

namespace Simias
{
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

		[Obsolete("Use SystemManager.Config to get the configuration object.", false)]
		public Configuration() : this(null)
		{
		}

		[Obsolete("Use SystemManager.Config to get the configuration object.", false)]
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
		}

		internal Configuration(string path, bool remove)
		{
			this.storePath = path;

			if (!File.Exists(ConfigFilePath))
			{
				// create defaults
				CreateDefaults();
			}
		}

		public void CreateDefaults()
		{
		}

		[Obsolete("Use .StorePath instead of .BasePath.", false)]
		public string BasePath
		{
			get { return storePath; }
		}

		public string StorePath
		{
			get { return storePath; }
		}

		private string ConfigFilePath
		{
			get { return Path.Combine(storePath, DefaultFileName); }
		}

		public string Get(string key, string defaultValue)
		{
			return Get(DefaultSection, key, defaultValue);
		}

		public string Get(string section, string key, string defaultValue)
		{
			string keyValue = null;

			try
			{
				mutex.WaitOne();
				XmlElement sectionElement = GetSection(section);
				string xpath = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
				XmlElement keyElement = (XmlElement)sectionElement.SelectSingleNode(xpath);
				if (keyElement == null)
				{				
					keyElement = (XmlElement)sectionElement.OwnerDocument.CreateNode(XmlNodeType.Element, SettingTag, "");
					keyElement.SetAttribute(NameAttr, key);
					sectionElement.AppendChild(keyElement);
				}

				keyValue = keyElement.GetAttribute(ValueAttr);
				if (keyValue == "")
				{
					keyElement.SetAttribute(ValueAttr, defaultValue);
					keyElement.OwnerDocument.Save(ConfigFilePath);
					keyValue = keyElement.GetAttribute(ValueAttr);
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}

			return keyValue;
		}

		public void Set(string key, string keyValue)
		{
			Set(DefaultSection, key, keyValue);
		}

		public void Set(string section, string key, string keyValue)
		{
			try
			{
				mutex.WaitOne();
				XmlElement sectionElement = GetSection(section);
				string xpath = string.Format("//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key);
				XmlElement keyElement = (XmlElement)sectionElement.SelectSingleNode(xpath);
				if (keyElement == null)
				{				
					keyElement = (XmlElement)sectionElement.OwnerDocument.CreateNode(XmlNodeType.Element, SettingTag, "");
					keyElement.SetAttribute(NameAttr, key);
					sectionElement.AppendChild(keyElement);
				}

				keyElement.SetAttribute(ValueAttr, keyValue);
				keyElement.OwnerDocument.Save(ConfigFilePath);
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
		
		[Obsolete("Stop using the DefaultPath.", false)]
		public static string DefaultPath
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

		[Obsolete("Stop using the fixupPath.", false)]
		public static string fixupPath(string path)
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


