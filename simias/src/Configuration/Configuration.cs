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

namespace Simias
{
	public sealed class Configuration
	{
		private static string basePath;

		private const string RootElementTag = "configuration";
		private const string SectionTag = "section";
		private const string SettingTag = "setting";
		private const string DefaultSection = "SimiasDefault";
		private const string DefaultInnerPath = ".simias";
		private const string DefaultFileName = "simias.conf";
			
		public static string BaseConfigPath
		{
			get
			{
				if(basePath == null)
				{
					basePath = Environment.GetFolderPath(
						Environment.SpecialFolder.LocalApplicationData);

					if(basePath == "")
					{
						basePath = Environment.GetFolderPath(
								Environment.SpecialFolder.Personal);
					}

					basePath += Path.DirectorySeparatorChar.ToString();
					basePath += DefaultInnerPath;


					DirectoryInfo dir = new DirectoryInfo(basePath);
					if (dir.Exists == false)
					{
						dir.Create();
					}
				}

				return basePath;
			}

			set
			{
				basePath = value;

				DirectoryInfo dir = new DirectoryInfo(basePath);
				if (dir.Exists == false)
				{
					dir.Create();
				}
			}
		}

		private static string ConfigFilePath
		{
			get
			{
				string fileName = BaseConfigPath + 
					Path.DirectorySeparatorChar.ToString() +
					DefaultFileName;

				return fileName;
			}
		}

		public static string Get(string key, string defaultValue)
		{
			return Get(DefaultSection, key, defaultValue);
		}

		public static string Get(string section, string key, 
				string defaultValue)
		{
			XmlNode valNode;

			XmlElement sectionElement = GetSection(section);
			XmlElement keyElement = sectionElement[key];
			if(keyElement == null)
			{
				keyElement = sectionElement.OwnerDocument.CreateElement(key);
				sectionElement.AppendChild(keyElement);
			}

			valNode = keyElement.FirstChild;
			if(valNode == null)
			{
				valNode = (XmlNode) 
					keyElement.OwnerDocument.CreateTextNode(defaultValue);
				keyElement.AppendChild(valNode);
				keyElement.OwnerDocument.Save(ConfigFilePath);
			}
			return valNode.Value;
		}

		public static void Set(string key, string value)
		{
			Set(DefaultSection, key, value);
		}

		public static void Set(string section, string key, string value)
		{
			XmlNode valNode;

			XmlElement sectionElement = GetSection(section);
			XmlElement keyElement = sectionElement[key];
			if(keyElement == null)
			{
				keyElement = sectionElement.OwnerDocument.CreateElement(key);
				sectionElement.AppendChild(keyElement);
			}

			valNode = keyElement.FirstChild;
			if(valNode == null)
			{
				valNode = (XmlNode) 
						keyElement.OwnerDocument.CreateTextNode(value);
				keyElement.AppendChild(valNode);
			}
			else
			{
				valNode.Value = value;
			}

			keyElement.OwnerDocument.Save(ConfigFilePath);
		}

		// These two methods are going to read the XML document every
		// time they are called but it's a cheap way to have fresh data
		// and this is probably not called all the time
		private static XmlElement GetSection(string section)
		{
			XmlElement sectionElement;
			XmlElement docElement;
			
			docElement = GetDocElement();

			sectionElement = docElement[section];

			if(sectionElement == null)
			{
				// Create the Section element cuase it don't exist
				sectionElement = 
						docElement.OwnerDocument.CreateElement(section);
				docElement.AppendChild(sectionElement);
			}
			return sectionElement;
		}

		private static XmlElement GetDocElement()
		{
			XmlElement docElement = null;

			XmlDocument doc = new XmlDocument();

			try
			{
				doc.Load(ConfigFilePath);
			}
			catch(Exception e)
			{
				doc = new XmlDocument();
				docElement = doc.CreateElement(RootElementTag);
				doc.AppendChild(docElement);
			}

			docElement = doc.DocumentElement;

			return docElement;
		}
	}
}


