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
using System.IO;
using System.Xml;

using Simias.Client;

namespace Simias
{
	/// <summary>
	/// Configuration class for simias components.
	/// </summary>
	public sealed class Configuration
	{
		#region Class Members

		private const string SectionTag = "section";
		private const string SettingTag = "setting";
		private const string NameAttr = "name";
		private const string ValueAttr = "value";
		private const string DefaultSection = "SimiasDefault";
		private const string DefaultFileName = "Simias.config";

		private XmlDocument configDoc;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="storePath">The directory path to the store.</param>
		/// <param name="isServer">True if running in a server configuration.</param>
		public Configuration( string storePath, bool isServer )
		{
			// The server's Simias.config file must always be in the data directory.
			string configFilePath = Path.Combine( storePath, DefaultFileName );
			
			if ( !isServer )
			{
				// See if there is an overriding Simias.config file in the client's data
				// directory. If not, then get the global copy.
				if ( !File.Exists( configFilePath ) )
				{
					configFilePath = Path.Combine( SimiasSetup.sysconfdir, DefaultFileName );
				}
			}

			// Check to see if the file already exists.
			if ( !File.Exists( configFilePath ) )
			{
				throw new SimiasException( String.Format( "Cannot locate configuration file: {0}", configFilePath ) );
			}

			// Load the configuration document from the file.
			configDoc = new XmlDocument();
			configDoc.Load( configFilePath );
		}

		#endregion

		#region Private Methods

		private XmlElement GetSection( string section )
		{
			string str = string.Format( "//section[@name='{0}']", section );
			return configDoc.DocumentElement.SelectSingleNode( str ) as XmlElement;
		}

		private XmlElement GetKey( string section, string key )
		{
			XmlElement keyElement = null;

			// Get the section that the key belongs to.
			XmlElement sectionElement = GetSection( section );
			if ( sectionElement != null )
			{
				string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key );
				keyElement = sectionElement.SelectSingleNode( str ) as XmlElement;
			}

			return keyElement;
		}

		private bool KeyExists( string section, string key )
		{
			return ( GetKey( section, key ) != null ) ? true : false;
		}

		private bool SectionExists( string section )
		{
			return ( GetSection( section ) != null ) ? true : false;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// Creates the key if does not exist.
		/// </summary>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement.</returns>
		public XmlElement GetElement( string key )
		{
			return GetElement( DefaultSection, key );
		}

		/// <summary>
		/// Returns the XmlElement for the specified key.  
		/// </summary>
		/// <param name="section">The section where the key is stored.</param>
		/// <param name="key">The key to return.</param>
		/// <returns>The key as an XmlElement if successful. Otherwise a null is returned.</returns>
		public XmlElement GetElement( string section, string key )
		{
			XmlElement element = GetKey( section, key );
			return ( element != null ) ? element.Clone() as XmlElement : null;
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="key">The key to get the value for.</param>
		/// <returns>The value as a string if successful. Otherwise a null is returned.</returns>
		public string Get( string key )
		{
			return Get( DefaultSection, key );
		}

		/// <summary>
		/// Returns the value for the specified key.
		/// </summary>
		/// <param name="section">The section where the key exists.</param>
		/// <param name="key">The key to get the value for.</param>
		/// <returns>The value as a string if successful. Otherwise a null is returned.</returns>
		public string Get( string section, string key )
		{
			XmlElement keyElement = GetKey( section, key );
			return ( keyElement != null ) ? keyElement.GetAttribute( ValueAttr ) : null;
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
		/// <param name="key">The key to set. If this parameter is null, then only the section
		/// is checked for existence.</param>
		/// <returns>True if the section and key exists, otherwise false is returned.</returns>
		public bool Exists( string section, string key )
		{
			return ( ( key != null ) && ( key != String.Empty ) ) ? KeyExists( section, key ) : SectionExists( section );
		}

		#endregion
	}
}


