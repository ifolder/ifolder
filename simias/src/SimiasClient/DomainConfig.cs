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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Net;
using System.Xml;

namespace Simias.Client
{
	/// <summary>
	/// Class used to assist in configuring the domain.
	/// </summary>
	public class DomainConfig
	{
		#region Class Members
		/// <summary>
		/// Configuration file XML tags.
		/// </summary>
		private static string SectionTag = "DomainClient";
		private static string ServersTag = "Servers";
		private static string NameTag = "name";
		private static string IDTag = "id";
		private static string DescriptionTag = "description";
		private static string UriTag = "uri";
		private static string EnabledTag = "enabled";

		/// <summary>
		/// Holds a reference to the simias configuration file.
		/// </summary>
		private Configuration config = new Configuration();

		/// <summary>
		/// Identifier of the domain for which configuration information is associated with.
		/// </summary>
		private string domainID;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the domain ID.
		/// </summary>
		public string ID
		{
			get { return domainID; }
		}

		/// <summary>
		/// Gets the domain name.
		/// </summary>
		public string Name
		{
			get { return GetDomainAttribute( NameTag ); }
		}

		/// <summary>
		/// Gets the domain description.
		/// </summary>
		public string Description
		{
			get { return GetDomainAttribute( DescriptionTag ); }
		}

		/// <summary>
		/// Gets the domain host.
		/// </summary>
		public string Host
		{
			get	
			{ 
				Uri uri = ServiceUrl;
				return ( uri != null ) ? uri.Host : null; 
			}
		}

		/// <summary>
		/// Gets the domain port.
		/// </summary>
		public int Port
		{
			get	
			{ 
				Uri uri = ServiceUrl;
				return ( uri != null ) ? uri.Port : -1; 
			}
		}

		/// <summary>
		/// Gets whether the domain is enabled.
		/// </summary>
		public bool Enabled
		{
			get
			{
				string enableString = GetDomainAttribute( EnabledTag );
				return ( enableString != null ) ? Convert.ToBoolean( enableString ) : false;
			}
		}

		/// <summary>
		/// Gets the url scheme.
		/// </summary>
		public string Scheme
		{
			get 
			{ 
				Uri uri = ServiceUrl;
				return ( uri != null ) ? uri.Scheme : Uri.UriSchemeHttp; 
			}
		}

		/// <summary>
		/// Gets the domain url
		/// </summary>
		public Uri ServiceUrl
		{
			get 
			{ 
				string uriString = GetDomainAttribute( UriTag );
				return ( uriString != null ) ? new Uri( uriString ) : null;
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of this object.
		/// </summary>
		/// <param name="domainID"></param>
		public DomainConfig( string domainID )
		{
			this.domainID = domainID;
		}
		#endregion

		#region Private Methods
		private XmlElement GetServerElement( XmlElement rootElement )
		{
			XmlElement serverElement = null;
			foreach( XmlElement element in rootElement )
			{
				if ( element.GetAttribute( IDTag ) == domainID )
				{
					serverElement = element;
					break;
				}
			}

			return serverElement;
		}

		private string GetDomainAttribute( string tag )
		{
			XmlElement root = config.GetElement( SectionTag, ServersTag );
			XmlElement element = GetServerElement( root );
			return ( element != null ) ? element.GetAttribute( tag ) : null;
		}
		#endregion
	}
}
