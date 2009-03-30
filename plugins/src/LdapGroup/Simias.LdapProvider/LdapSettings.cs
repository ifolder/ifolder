/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/


using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Simias;
using Simias.Storage;

namespace Simias.LdapProvider
{
	/// <summary>
	/// The LDAP directory type.
	/// </summary>
	public enum LdapDirectoryType
	{
		/// <summary>
		/// unknown
		/// </summary>
		Unknown,

		/// <summary>
		/// eDirectory
		/// </summary>
		eDirectory,

		/// <summary>
		/// Active Directory
		/// </summary>
		ActiveDirectory,

		/// <summary>
		/// OpenLDAP
		/// </summary>
		OpenLDAP
	}

	public class LdapSettings
    {
	    #region Fields
		private static readonly string SectionTag = "section";
		private static readonly string SettingTag = "setting";
		private static readonly string NameAttr = "name";
		private static readonly string ValueAttr = "value";

		private static readonly string ProxyPasswordFile = ".simias.ppf";

		private static readonly string LdapAuthenticationSection = "LdapAuthentication";
		public static readonly string UriKey = "LdapUri";
		public static readonly string ProxyDNKey = "ProxyDN";
		public static readonly string ProxyPasswordKey = "ProxyPassword";

		public static readonly string DomainSection = "EnterpriseDomain";
		public static readonly string OldDomainSection = "Domain";
		public static readonly string SimiasAdminDNKey = "AdminName";
		public static readonly string SimiasOldAdminDNKey = "AdminDN";

		private static readonly string LdapSystemBookSection = "LdapProvider";
		private static readonly string OldLdapSystemBookSection = "LdapSystemBook";
		private static readonly string SearchKey = "Search";
		public static readonly string XmlContextTag = "Context";
		private static readonly string XmlDNAttr = "dn";
		private static readonly string NamingAttributeKey = "NamingAttribute";

		private static readonly string IdentitySection = "Identity";
		private static readonly string AssemblyKey = "Assembly";
		private static readonly string ClassKey = "Class";

		private static readonly string DefaultUri = "ldaps://localhost/";

		public static readonly string DefaultNamingAttribute = "cn";

		public static readonly string UriSchemeLdap = "ldap";
		public static readonly int UriPortLdap = 389;
		public static readonly string UriSchemeLdaps = "ldaps";
		public static readonly int UriPortLdaps = 636;
		#endregion

		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		private enum ChangeMap : uint
		{
			unchanged = 0x00000000,
            uri = 0x00000001,
            scheme = 0x00000002,
            host = 0x00000004,
            port = 0x00000008,
            proxy = 0x00000010,
			password = 0x00000020,
			searchContexts = 0x00020000,
            syncInterval = 0x00040000,
            syncOnStart = 0x00080000,
	    	namingAttribute = 0x00100000
		}

        private ChangeMap settingChangeMap;
        private Uri uri = new Uri( DefaultUri );

		private string scheme;
        private string host;
        private int port;
		private LdapDirectoryType ldapType = LdapDirectoryType.Unknown;

        private string proxy = String.Empty;
		private string password = String.Empty;
		private string simiasAdmin = String.Empty;

		private XmlElement searchElement;

		private ArrayList searchContexts = new ArrayList();
		private string namingAttribute = DefaultNamingAttribute;

		private string storePath;
    
		#region Properties
		/// <summary>
		/// Gets the admin DN.
		/// </summary>
		public string AdminDN
		{
			get { return ( this.simiasAdmin ); }
		}

		/// <summary>
		/// Gets/sets the LDAP directory type.
		/// </summary>
		public LdapDirectoryType DirectoryType
		{
			get { return ldapType; }
			set { ldapType = value; }
		}

		/// <summary>
		/// Gets/sets the host.
		/// </summary>
		public string Host
		{
			get { return (this.host); }
			set
			{
				this.host = value;
				settingChangeMap |= ChangeMap.host;
			}
		}

		/// <summary>
		/// Gets/sets the naming attribute.
		/// </summary>
		public string NamingAttribute
		{
			get { return ( this.namingAttribute ); }
			set
			{
				this.namingAttribute = value;
				settingChangeMap |= ChangeMap.namingAttribute;
			}
		}

		/// <summary>
		/// Gets/sets the port.
		/// </summary>
		public int Port
		{
			get { return (this.port); }
			set
			{
				this.port = value;
				settingChangeMap |= ChangeMap.port;
			}
		}

		/// <summary>
		/// Gets/sets the proxy DN.
		/// </summary>
		public string ProxyDN
		{
			get { return ( this.proxy ); }
			set
			{
				this.proxy = value;
				settingChangeMap |= ChangeMap.proxy;
			}
		}

		/// <summary>
		/// Gets/sets the proxy password.
		/// </summary>
		public string ProxyPassword
		{
			get { return ( this.password ); }
			set
			{
				this.password = value;
				settingChangeMap |= ChangeMap.password;
			}
		}

		/// <summary>
		/// Gets/sets the scheme.
		/// </summary>
		public string Scheme
		{
			get { return (this.scheme); }
			set
			{
				this.scheme = value;
				settingChangeMap |= ChangeMap.scheme;
			}
		}

		/// <summary>
		/// Gets/sets the contexts that are searched when provisioning users.
		/// </summary>
		public IEnumerable SearchContexts
		{
			get { return ( ( IEnumerable ) this.searchContexts.Clone() ); }
			set
			{
				searchContexts.Clear();
				foreach ( string context in value )
				{
					searchContexts.Add( context );
				}

				settingChangeMap |= ChangeMap.searchContexts;
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if SSL is being used.
		/// </summary>
		public bool SSL
		{
			get { return ( this.Scheme.Equals( UriSchemeLdaps ) ? true : false ); }
			set { this.Scheme = value ? UriSchemeLdaps : UriSchemeLdap; }
		}

		/// <summary>
		/// Gets/sets the Uri of the LDAP server.
		/// </summary>
		public Uri Uri
		{
			get { return( this.uri ); }
			set
			{
				this.uri = value;
				this.scheme = uri.Scheme;
				this.host = uri.Host;
				if ( ( this.port = uri.Port ) == -1 )
				{
					this.port = SSL ? UriPortLdaps : UriPortLdap;
				}

				settingChangeMap |= ChangeMap.uri;
			}
		}
		#endregion

		#region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="storePath"></param>
        private LdapSettings( string storePath )
        {
			this.storePath = storePath;

			Configuration config = new Configuration( storePath, true );
            settingChangeMap = 0;

			string identity = config.Get( IdentitySection, AssemblyKey );
			if ( identity != null )
			{
				switch ( identity )
				{
					case "Simias.LdapProvider":
						ldapType = LdapDirectoryType.eDirectory;
						break;
					case "Simias.ADLdapProvider":
						ldapType = LdapDirectoryType.ActiveDirectory;
						break;
					case "Simias.OpenLdapProvider":
						ldapType = LdapDirectoryType.OpenLDAP;
						break;
				}
			}

			// <setting name="LdapUri" />
			string uriString = config.Get( LdapAuthenticationSection, UriKey );
			if ( uriString != null )
			{
				this.uri = new Uri( uriString );
			}

		    this.scheme = uri.Scheme;
			this.host = uri.Host;
			if ( ( this.port = uri.Port ) == -1 )
			{
				this.port = SSL ? UriPortLdaps : UriPortLdap;
			}

            string proxyString = config.Get( LdapAuthenticationSection, ProxyDNKey );
			if ( proxyString != null )
			{
				proxy = proxyString;
			}

			// Get the password from the file if it exists.
			this.password = GetProxyPasswordFromFile();

			string simiasAdminString = config.Get( DomainSection, SimiasAdminDNKey );
			if ( simiasAdminString != null )
			{
				simiasAdmin = simiasAdminString;
			}

		    // <setting name="Search" />
		    searchElement = config.GetElement( LdapSystemBookSection, SearchKey );
			if ( searchElement != null )
			{
				XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
				foreach( XmlElement contextNode in contextNodes )
				{
					searchContexts.Add( contextNode.GetAttribute( XmlDNAttr ) );
				}
			}

			string namingAttributeString = config.Get( LdapSystemBookSection, NamingAttributeKey );
			if ( namingAttributeString != null )
			{
				this.namingAttribute = namingAttributeString;
			}
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="storePath"></param>
		/// <param name="upgrade"></param>
        private LdapSettings( string storePath, bool upgrade )
        {
        	if(upgrade)
        	{
				this.storePath = storePath;
				
				Configuration config = new Configuration( storePath, true );
				settingChangeMap = 0;
				
				ldapType = LdapDirectoryType.eDirectory;
				
				// <setting name="LdapUri" />
				string uriString = config.Get( LdapAuthenticationSection, UriKey );
				if ( uriString != null )
				{
					this.uri = new Uri( uriString );
				}
				
				this.scheme = uri.Scheme;
				this.host = uri.Host;
				if ( ( this.port = uri.Port ) == -1 )
				{
					this.port = SSL ? UriPortLdaps : UriPortLdap;
				}
				
				string proxyString = config.Get( LdapAuthenticationSection, ProxyDNKey );
				if ( proxyString != null )
				{
					proxy = proxyString;
				}
				
				// Get the password from the file if it exists.
				this.password = GetProxyPasswordFromFile();
				
				string simiasAdminString = config.Get( OldDomainSection, SimiasOldAdminDNKey );
				if ( simiasAdminString != null )
				{
					simiasAdmin = simiasAdminString;
				}
				
				// <setting name="Search" />
				searchElement = config.GetElement( OldLdapSystemBookSection, SearchKey );
				if ( searchElement != null )
				{
					XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
					foreach( XmlElement contextNode in contextNodes )
					{
						searchContexts.Add( contextNode.GetAttribute( XmlDNAttr ) );
					}
				}
				
				string namingAttributeString = config.Get( OldLdapSystemBookSection, NamingAttributeKey );
				if ( namingAttributeString != null )
				{
					this.namingAttribute = namingAttributeString;
				}
        	}
				
        }
		#endregion

		#region Private Methods
        /// <summary>
        /// Get the proxy password from the store path
        /// </summary>
        /// <returns>encrypted password</returns>
		private string GetProxyPasswordFromFile()
		{
			string proxyPassword = String.Empty;
			string ppfPath = Path.Combine( storePath, ProxyPasswordFile );
			if ( File.Exists( ppfPath ) )
			{
				using ( StreamReader sr = File.OpenText( ppfPath ) )
				{
					// Password should be a single line of text.
					proxyPassword = sr.ReadLine();
				}
			}

			return proxyPassword;
		}

        /// <summary>
        /// writes the proxy password into a file inside store path
        /// </summary>
		private void SetProxyPasswordInFile()
		{
			string ppfPath = Path.Combine( storePath, ProxyPasswordFile );
			using ( StreamWriter sw = File.CreateText( ppfPath ) )
			{
				sw.WriteLine( password );
			}
		}

        /// <summary>
        /// Sets the value into config file
        /// </summary>
        /// <param name="document">Xml document </param>
        /// <param name="section">section to be written</param>
        /// <param name="key">key to be written</param>
        /// <param name="configValue">value to be written for key</param>
        /// <returns>true if write is successful</returns>
		private bool SetConfigValue( XmlDocument document, string section, string key, string configValue )
		{
			bool status = false;

			// Build an xpath for the setting.
			string str = string.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, section, SettingTag, key );
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode( str );
			if ( element != null )
			{
				element.SetAttribute( ValueAttr, configValue );
				status = true;
			}
			else
			{
				// The setting doesn't exist, so create it.
				element = document.CreateElement(SettingTag);
				element.SetAttribute(NameAttr, key);
				element.SetAttribute(ValueAttr, configValue);
				str = string.Format("//{0}[@{1}='{2}']", SectionTag, NameAttr, section);
				XmlElement eSection = (XmlElement)document.DocumentElement.SelectSingleNode(str);
				if ( eSection == null )
				{
					// If the section doesn't exist, create it.
					eSection = document.CreateElement( SectionTag );
					eSection.SetAttribute( NameAttr, section );
					document.DocumentElement.AppendChild( eSection );
				}

				eSection.AppendChild(element);
				status = true;
			}

			return status;
		}

        /// <summary>
        /// Get the search element from given Xml document
        /// </summary>
        /// <param name="document"></param>
        /// <returns>an Xml element representing the key/value pair</returns>
		private XmlElement GetSearchElement( XmlDocument document )
		{
			string str = String.Format( "//{0}[@{1}='{2}']/{3}[@{1}='{4}']", SectionTag, NameAttr, LdapSystemBookSection, SettingTag, SearchKey );
			XmlElement element = ( XmlElement )document.DocumentElement.SelectSingleNode( str );
			if ( element == null )
			{
				// The setting doesn't exist, so create it.
				element = document.CreateElement( SettingTag );
				element.SetAttribute( NameAttr, SearchKey );
				str = string.Format( "//{0}[@{1}='{2}']", SectionTag, NameAttr, LdapSystemBookSection );
				XmlElement eSection = ( XmlElement )document.DocumentElement.SelectSingleNode(str);
				if ( eSection == null )
				{
					// If the section doesn't exist, create it.
					eSection = document.CreateElement( SectionTag );
					eSection.SetAttribute( NameAttr, LdapSystemBookSection );
					document.DocumentElement.AppendChild( eSection );
				}

				eSection.AppendChild(element);
			}

			return element;
		}
		#endregion

		#region Public Methods
        /// <summary>
        /// Gets the ldapsetting 
        /// </summary>
        /// <param name="storePath">path of store</param>
        /// <param name="upgrade">whether upgrade is true or false</param>
        /// <returns>LdapSetting object</returns>
        public static LdapSettings Get( string storePath, bool upgrade )
        {
        	if(upgrade)
	            return ( new LdapSettings( storePath, upgrade ) );
		else
	            return ( new LdapSettings( storePath ) );
        }

        /// <summary>
        /// Gets LdapSetting
        /// </summary>
        /// <param name="storePath">path of the store</param>
        /// <returns>LdapSetting object</returns>
        public static LdapSettings Get( string storePath )
        {
            return ( new LdapSettings( storePath ) );
        }

        /// <summary>
        /// Commit into the database the ldapsetting
        /// </summary>
		public void Commit()
		{
			if (settingChangeMap != 0)
			{
				// Build a path to the Simias.config file.
				string configFilePath = 
					Path.Combine( storePath, Simias.Configuration.DefaultConfigFileName );

				// Load the configuration file into an xml document.
				XmlDocument configDoc = new XmlDocument();
				configDoc.Load( configFilePath );

				if ( ( settingChangeMap & ChangeMap.uri ) != ChangeMap.unchanged )
				{
					SetConfigValue( configDoc, LdapAuthenticationSection, UriKey, uri.ToString() );
				}
				else if ( ( settingChangeMap & ( ChangeMap.scheme | ChangeMap.host | ChangeMap.port ) ) != ChangeMap.unchanged )
				{
					UriBuilder ub = new UriBuilder( scheme, host, port );
					SetConfigValue( configDoc, LdapAuthenticationSection, UriKey, ub.Uri.ToString() );
				}

				if ( ( settingChangeMap & ChangeMap.proxy ) == ChangeMap.proxy )
				{
					SetConfigValue( configDoc, LdapAuthenticationSection, ProxyDNKey, proxy );
				}

				if ( ( settingChangeMap & ChangeMap.namingAttribute ) == ChangeMap.namingAttribute )
				{
					SetConfigValue( configDoc, LdapSystemBookSection, NamingAttributeKey, namingAttribute );
				}

				if ( ( settingChangeMap & ChangeMap.searchContexts ) == ChangeMap.searchContexts )
				{
					XmlElement searchElement = GetSearchElement( configDoc );
					if ( searchElement != null )
					{
						XmlNodeList contextNodes = searchElement.SelectNodes( XmlContextTag );
						foreach( XmlElement contextNode in contextNodes )
						{
							searchElement.RemoveChild( contextNode );
						}

						foreach( string dn in searchContexts )
						{
							XmlElement element = configDoc.CreateElement( XmlContextTag );
							element.SetAttribute( XmlDNAttr, dn );
							searchElement.AppendChild( element );
						}
					}
				}

				if ( ( settingChangeMap & ChangeMap.password ) == ChangeMap.password )
				{
					SetProxyPasswordInFile();
				}

				switch ( ldapType )
				{
					case LdapDirectoryType.ActiveDirectory:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.ADLdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.ADLdapProvider.User" );
						break;
					case LdapDirectoryType.eDirectory:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.LdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.LdapProvider.User" );
						break;
					case LdapDirectoryType.OpenLDAP:
						SetConfigValue( configDoc, IdentitySection, AssemblyKey, "Simias.OpenLdapProvider" );
						SetConfigValue( configDoc, IdentitySection, ClassKey, "Simias.OpenLdapProvider.User" );
						break;
					default:
						throw new Exception( "The LDAP directory type is unknown!" );
				}

				// Write the configuration file settings.
				XmlTextWriter xtw = new XmlTextWriter( configFilePath, Encoding.UTF8 );
				try
				{
					xtw.Formatting = Formatting.Indented;
					configDoc.WriteTo( xtw );
				}
				finally
				{
					xtw.Close();
				}
			}
		}
		#endregion
    }
}


