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
		private string basePath;
		private Mutex mutex = new Mutex(false, "SimiasConfigMutex");

		private const string RootElementTag = "configuration";
		private const string SectionTag = "section";
		private const string SettingTag = "setting";
		private const string NameAttr = "name";
		private const string ValueAttr = "value";
		private const string DefaultSection = "SimiasDefault";
		private const string DefaultFileName = "simias.conf";

		public Configuration() : this(null)
		{
		}

		public Configuration(string basePath)
		{
			if(basePath == null)
			{
				basePath = DefaultPath;
			}
			else
			{
				basePath = fixupPath(basePath);
			}

			this.basePath = basePath;
		}
			
		public string BasePath
		{
			get
			{
				return basePath;
			}
		}

		private string ConfigFilePath
		{
			get
			{
				return Path.Combine(BasePath, DefaultFileName);
			}
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
		/// <summary>
		/// Gets the default database path.
		/// </summary>
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

		public static string fixupPath(string path)
		{
			path = Path.Combine(path, ".simias");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			return path;
		}
		#endregion
	}
}


