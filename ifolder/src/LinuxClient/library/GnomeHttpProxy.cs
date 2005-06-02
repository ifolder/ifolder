/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: banderso@novell.com
 *
 ***********************************************************************/

using System;
using GConf;

namespace Novell.iFolder
{
	/// <summary>
	/// Class for getting the GConf proxy settings
	/// </summary>
	public class GnomeHttpProxy
	{
		private GConf.Client client;
		private string domainUrl;
		private bool bypass = false;
	
		// these strings represent our gconf paths
		static string GCONF_PROXY_PATH = "/system/http_proxy";
		static string GCONF_SPROXY_PATH = "/system/proxy";
		static string GCONF_IGNORE_HOSTS = GCONF_PROXY_PATH + "/ignore_hosts";
		static string GCONF_USE_AUTHENTICATION = GCONF_PROXY_PATH + "/use_authentication";
		static string GCONF_HOST = GCONF_PROXY_PATH + "/host";
		static string GCONF_PORT = GCONF_PROXY_PATH + "/port";
		static string GCONF_USE_PROXY = GCONF_PROXY_PATH + "/use_http_proxy";
		static string GCONF_USER = GCONF_PROXY_PATH + "/authentication_user";
		static string GCONF_PASSWORD = GCONF_PROXY_PATH + "/authentication_password";
		static string GCONF_SECURE_HOST = GCONF_SPROXY_PATH + "/secure_host";
		static string GCONF_SECURE_PORT = GCONF_SPROXY_PATH + "/secure_port";
	
		#region Properies
		/// <summary>
		/// Returns true if a proxy is set
		/// </summary>
		public bool IsProxySet
		{
			get
			{ 
				try
				{
					if ( bypass == false )
					{
						return (bool) client.Get( GCONF_USE_PROXY );
					}
				}
				catch{}
				return false;
			}
		}

		/// <summary>
		/// Returns true if a secure proxy is set
		/// </summary>
		public bool IsSecureProxySet
		{
			get
			{ 
				try
				{
					if ( bypass == false )
					{
						string shost = (string) client.Get( GCONF_SECURE_HOST );
						if ( shost != null && shost != "" )
						{
							return true;
						}
					}
				}
				catch{}
				return false;
			}
		}

		/// <summary>
		/// Returns true if proxy credentials are set
		/// </summary>
		public bool CredentialsSet
		{
			get
			{ 
				try
				{
					if ( bypass == false )
					{
						return (bool) client.Get( GCONF_USE_AUTHENTICATION );
					}
				}
				catch{}
				return false;
			}
		}
	
		/// <summary>
		/// If configured, returns the proxy host otherwise
		/// return null
		/// </summary>
		public string Host
		{
			get
			{ 
				try
				{
					if ( bypass == false && 
						(bool) client.Get( GCONF_USE_PROXY ) == true )
					{
						string host = (string) client.Get( GCONF_HOST );
						if ( host != null && host != "" )
						{
							int port = (int) client.Get( GCONF_PORT );
							if ( port != 0 )
							{
								return host + ":" + port.ToString();
							}
							
							return host;
						}
					}
				}
				catch{}
				return null;
			}
		}
	
		/// <summary>
		/// If configured, returns the username
		/// for authentication to the proxy otherwise
		/// returns null
		/// </summary>
		public string Username
		{
			get
			{ 
				try
				{
					if ( bypass == false &&
						(bool) client.Get( GCONF_USE_AUTHENTICATION ) == true )
					{
						return (string) client.Get( GCONF_USER );
					}
				}
				catch{}
				return null;
			}
		}
	
		/// <summary>
		/// If configured, returns the password
		/// for authentication to the proxy otherwise
		/// returns null
		/// </summary>
		public string Password
		{
			get
			{ 
				try
				{
					if ( bypass == false &&
						(bool) client.Get( GCONF_USE_AUTHENTICATION ) == true )
					{
						return (string) client.Get( GCONF_PASSWORD );
					}
				}
				catch{}
				return null;
			}
		}

		/// <summary>
		/// If configured, returns the secure proxy host otherwise
		/// return null
		/// </summary>
		public string SecureHost
		{
			get
			{ 
				try
				{
					if ( bypass == false )
					{
						string shost = (string) client.Get( GCONF_SECURE_HOST );
						if ( shost != null && shost != "" )
						{
							int sport = (int) client.Get( GCONF_SECURE_PORT );
							if ( sport != 0 )
							{
								return shost + ":" + sport.ToString();
							}
							
							return shost;
						}
					}
				}
				catch{}
				return null;
			}
		}
		#endregion
	
	
		/// <summary>
		/// Constructor - constains the target url
		/// which is checked against the bypass list
		/// If the target url exists in the bypass list
		/// proxy settings are ignored.
		/// </summary>
		public GnomeHttpProxy( string domainUrl )
		{
			client = new GConf.Client();
			this.domainUrl = domainUrl.ToLower();
			
			try
			{
				string[] bypassHosts = (string[]) client.Get( GCONF_IGNORE_HOSTS );
				foreach( string host in bypassHosts )
				{
					string normalizedHost = host.Replace( '/', ':' ).ToLower();
					if ( normalizedHost == this.domainUrl )
					{
						this.bypass = true;
						break;
					}
				}
			}
			catch( Exception e )
			{
				//Console.WriteLine( e.Message );
			}	
		}
	}
}

