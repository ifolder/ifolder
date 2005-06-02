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
 *  Author: Mike Lasky
 *
 ***********************************************************************/
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Simias.Client
{
	/// <summary>
	/// Summary description for WSInspection.
	/// </summary>
	public class WSInspection
	{
		#region Class Members
		/// <summary>
		/// Default name for WSIL document.
		/// </summary>
		private static string WSInspectionDocument = "inspection.wsil";

		/// <summary>
		/// WSIL specified XML tags.
		/// </summary>
		private static string WSIL_ServiceTag = "wsil:service";
		private static string WSIL_NameTag = "wsil:name";
		private static string WSIL_DescriptionTag = "description";
		private static string WSIL_LocationAttrTag = "location";

		/// <summary>
		/// Default ports used by http and https.
		/// </summary>
		private const int DefaultPort = 80;
		private const int SSLDefaultPort = 443;
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the URL for the specified service by using WS-Inspection.
		/// </summary>
		/// <param name="host">Address and optionally port of the host server.</param>
		/// <param name="serviceName">Service name to find URL for.</param>
		/// <param name="user">The user to be authenticated.</param>
		/// <param name="password">The password of the user.</param>
		/// <returns>A URL that references the specified service.</returns>
		static public Uri GetServiceUrl( string host, string serviceName, string user, string password )
		{
			Uri serviceUrl = null;
			HttpWebResponse response = null;
			// Build a credential from the user name and password.
			NetworkCredential myCred = new NetworkCredential( user, password ); 

			// Parse the host string in case there is a port specified.
			Uri parseUri = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + host );

			// Try 'https' first.
			int port = ( host.IndexOf( ':' ) != -1 ) ? parseUri.Port : DefaultPort; //SSLDefaultPort;
			UriBuilder wsUri = new UriBuilder( Uri.UriSchemeHttp, parseUri.Host, port, WSInspectionDocument );

			// Create the web request.
			WebRequest request = WebRequest.Create( wsUri.Uri );
			request.Credentials = myCred;
			request.PreAuthenticate = true;
			request.Timeout = 15 * 1000;
			
			bool retry = true;
			proxyRetry:
			try
			{
				// Get the response from the web server.
				response = request.GetResponse() as HttpWebResponse;
			}
			catch ( WebException we )
			{
				if ( ( we.Status == WebExceptionStatus.TrustFailure ) ||
					 ( we.Status == WebExceptionStatus.Timeout ) ||
					 ( we.Status == WebExceptionStatus.NameResolutionFailure ) )
				{
					throw we;	
				}
				else
				{
					response = we.Response as HttpWebResponse;
					if (response != null)
					{
						if (response.StatusCode == HttpStatusCode.Unauthorized && retry == true)
						{
							// This should be a free call we must be behind iChain.
							request = WebRequest.Create( response.ResponseUri );
							request.Credentials = myCred;
							request.PreAuthenticate = true;
							request.Timeout = 15 * 1000;
							retry = false;
							goto proxyRetry;
						}
					}
					
					response = null;
					CertPolicy.CertificateState cs = CertPolicy.GetCertificate(host);
					if (cs != null && !cs.Accepted)
					{
						// BUGBUG this is here to work around a mono bug.
						throw new WebException(we.Message, we, WebExceptionStatus.TrustFailure, we.Response);
					}
				}
			}
			
			// Make sure that there was an answer.
			if ( response != null )
			{
				try
				{
					// Get the stream associated with the response.
					Stream receiveStream = response.GetResponseStream();
					
					// Pipes the stream to a higher level stream reader with the required encoding format. 
					StreamReader readStream = new StreamReader( receiveStream, Encoding.UTF8 );
					try
					{
						XmlDocument document = new XmlDocument();
						document.Load( readStream );

						//Create an XmlNamespaceManager for resolving namespaces.
						XmlNamespaceManager nsmgr = new XmlNamespaceManager( document.NameTable );
						nsmgr.AddNamespace( "wsil", document.DocumentElement.NamespaceURI );

						// Search for the named service element.
						XmlNode serviceNode = document.DocumentElement.SelectSingleNode( WSIL_ServiceTag + "[" + WSIL_NameTag + "='" + "Domain Service" + "']", nsmgr );
						if ( serviceNode != null )
						{
							// Get the description node.
							XmlElement description = serviceNode[ WSIL_DescriptionTag ];
							if ( description != null )
							{
								// Get the uri location.
								string uriString = description.GetAttribute( WSIL_LocationAttrTag );
								if ( uriString != null )
								{
									// Fix up the URI if it is relative.
									if ( !uriString.ToLower().StartsWith( Uri.UriSchemeHttp ) )
									{
										Uri respUri = response.ResponseUri;
										UriBuilder urb = new UriBuilder( respUri.Scheme, respUri.Host, respUri.Port, uriString.TrimStart( new char[] { '/' } ) );
										serviceUrl = urb.Uri;
									}
									else
									{
										serviceUrl = new Uri( uriString );
									}
								}
							}
						}
					}
					finally
					{
						readStream.Close ();
					}
				}
				finally
				{
					response.Close ();
				}
			}

			return serviceUrl;
		}
		#endregion
	}
}
