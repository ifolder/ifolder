/***********************************************************************
 *  $RCSfile: Host.cs,v $
 * 
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author: Brady Anderson (banderso@novell.com)
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Web;

namespace iFolder.Client
{
	/// <summary>
	/// A Host object
	/// </summary>
	public class Host
	{
		#region Private Types
		private Uri uri;
		private string url;
		private string domainID;
		private readonly string wsInspectionDocument = "/inspection.wsil";
		
		/// <summary>
		/// WSIL specified XML tags.
		/// </summary>
		private readonly string WSIL_ServiceTag = "wsil:service";
		private readonly string WSIL_NameTag = "wsil:name";
		private readonly string WSIL_DescriptionTag = "description";
		private readonly string WSIL_LocationAttrTag = "location";

		/// <summary>
		/// Web State
		/// Note: Internal access for domain and ifolder usage
		/// </summary>
		internal CookieContainer cookies = new CookieContainer();
		internal NetworkCredential credentials = null;

		/// <summary>
		/// Private Address is mostly used for communication
		/// between hosts.  Leave this internal for now but
		/// we can probably remove the type alltogether since
		/// it shouldn't used by a client.
		/// </summary>
		internal string PrivateAddress;

		private char[] trimChars = new char[] { '/' };

		#endregion
		
		#region Properties
		/// <summary>
		/// Public Address
		/// </summary>
		public string Address
		{
			get 
			{ 
				if ( uri != null )
				{
					return uri.ToString().TrimEnd( trimChars );
				}
				
				return "";
			}
		}

		/// <summary>
		/// Domain ID
		/// Note: only valid after a successful ping
		/// </summary>
		public string DomainID
		{
			get 
			{ 
				return this.domainID;
			}
		}
		
		/// <summary>
		/// True - Master domain exists on this host
		/// False - Slave domain exists on this host
		/// </summary>
		public bool Master
		{
			get 
			{ 
				return true;
			}
		}

		/// <summary>
		/// Public URI Address
		/// </summary>
		public Uri Uri
		{
			get 
			{ 
				return uri;
			}
		}
		#endregion
		
		#region Constructors
		public Host( string url )
		{
			this.url = url;
			
			try 
			{	
				if ( url.StartsWith( Uri.UriSchemeHttp ) || url.StartsWith( Uri.UriSchemeHttps ) )
				{
					this.uri = new Uri( url.TrimEnd( trimChars ) ); 
				}
				else
				{
					this.uri = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + url.TrimEnd( trimChars ) );
				}
			}
			catch 
			{
				this.uri = new Uri( Uri.UriSchemeHttp + Uri.SchemeDelimiter + url.TrimEnd( trimChars ) );
			}
		}
		#endregion
		
		#region Private Methods
		/// <summary>
		/// Gets the URL for the specified service by using WS-Inspection.
		/// </summary>
		/// <param name="host">Address and optionally port of the host server.</param>
		/// <param name="serviceName">Service name to find URL for.</param>
		/// <param name="user">The user to be authenticated.</param>
		/// <param name="password">The password of the user.</param>
		/// <returns>A URL that references the specified service.</returns>
		//private Uri GetServiceUrl( string host, string serviceName, string user, string password )
		private Uri GetServiceUrl()
		{
			Uri serviceUrl = null;
			HttpWebResponse response = null;
			CookieContainer cks = new CookieContainer();
			Uri tempUri = new Uri( this.uri.ToString().TrimEnd( trimChars ) + wsInspectionDocument );

			// Create the web request.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( tempUri );
			
			bool retry = true;
			proxyRetry:

			request.Timeout = 15 * 1000;
			request.CookieContainer = cks;
			//request.Proxy = ProxyState.GetProxyState( request.RequestUri );

			try
			{
				// Get the response from the web server.
				response = request.GetResponse() as HttpWebResponse;

				// Mono has a bug where it doesn't set the cookies in the cookie jar.
				cks.Add( response.Cookies );
			}
			catch ( WebException we )
			{
				IsTrustFailure( this.url, we );
				if ( ( we.Status == WebExceptionStatus.Timeout ) ||
					 ( we.Status == WebExceptionStatus.NameResolutionFailure ) )
				{
					throw we;	
				}
				else
				{
					response = we.Response as HttpWebResponse;
					if (response != null)
					{
						// Mono has a bug where it doesn't set the cookies in the cookie jar.
						cks.Add( response.Cookies );
						if (response.StatusCode == HttpStatusCode.Unauthorized && retry == true)
						{
							// This should be a free call we must be behind iChain.
							request = (HttpWebRequest)WebRequest.Create( response.ResponseUri );
							retry = false;
							goto proxyRetry;
						}
					}
					response = null;
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
                                        
										// Check to see if we need to use ssl.
										// Make the request and see if we get redirected 302;
										// Create the web request.
										request = (HttpWebRequest)WebRequest.Create( serviceUrl );
										request.CookieContainer = cks;
										//request.Proxy = ProxyState.GetProxyState( request.RequestUri );
										response.Close();
										try
										{
											response = request.GetResponse() as HttpWebResponse;
											serviceUrl = response.ResponseUri;
										}
										catch( WebException wex )
										{
											IsTrustFailure( this.url, wex );
											response = wex.Response as HttpWebResponse;
											if( response != null )
											{
												if( response.StatusCode == HttpStatusCode.Unauthorized )
												{
													if( response.Headers.Get( "Simias-Error" ) != null )
													{
														// This is expected because this service requires authentication.
														serviceUrl = response.ResponseUri;
													}
												}
											}
										}
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

		#region Private Static Methods
		static void IsTrustFailure( string host, WebException we )
		{
			if (we.Status == WebExceptionStatus.TrustFailure )
			{
				throw we;	
			}
			
			/*
			CertPolicy.CertificateState cs = CertPolicy.GetCertificate( host );
			if ( cs != null && !cs.Accepted )
			{
				// BUGBUG this is here to work around a mono bug.
				throw new WebException( we.Message, we, WebExceptionStatus.TrustFailure, we.Response );
			}
			*/
		}
		#endregion
		
		#region Public Methods
		public bool Ping()
		{
			bool hostUp = false;
			Uri pingUri = null;
			
			try
			{
				try
				{
					pingUri = this.GetServiceUrl();
					if ( pingUri != null )
					{
						this.uri = new UriBuilder( pingUri.Scheme, pingUri.Host, pingUri.Port ).Uri;
					}
				} 
				catch{}

				if ( pingUri == null )
				{
					pingUri = 
						new Uri( this.uri.ToString().TrimEnd( trimChars ) + Domain.DomainServicePath );
				}

				// Create the domain service web client object.
				DomainService domainService = new DomainService();
				domainService.CookieContainer = cookies;
				domainService.Url = pingUri.ToString();
				//domainService.Proxy = ProxyState.GetProxyState( domainServiceUrl );

				this.domainID = domainService.GetDomainID();
				if ( this.domainID != null )
				{
					// Mono has a bug where it doesn't set the cookies in the cookie jar.
					//cookies.Add( response.Cookies );

					hostUp = true;
				}
			}
			catch ( WebException we )
			{
				if ( we.Status == WebExceptionStatus.TrustFailure )
				{
					hostUp = true;
				}
			}
			catch{}

			return hostUp;		
		}
		#endregion
	}
}	
