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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Web;
using System.Net;
using System.Web.Services.Protocols;

using Simias.Storage;
using Simias.Authentication;
using Simias.Event;
using Simias.Client.Event;

namespace Simias
{
	/// <summary>
	/// Class to share and keep proxy state for web communications.
	/// </summary>
	public class ProxyState : IWebProxy
	{
		#region Class Members

		private IWebProxy webProxy;
		private static Hashtable proxyHash = new Hashtable();

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of the object.
		/// </summary>
		/// <param name="proxy">The proxy server address.</param>
		/// <param name="proxyUser">The user name for proxy authentication.</param>
		/// <param name="proxyPassword">The password for proxy authentication.</param>
		private ProxyState( Uri proxy, string proxyUser, string proxyPassword )
		{
			InitializeWebProxy( proxy, proxyUser, proxyPassword );
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the WebProxy object. 
		/// </summary>
		/// <param name="proxy">The proxy server address.</param>
		/// <param name="proxyUser">The user name for proxy authentication.</param>
		/// <param name="proxyPassword">The password for proxy authentication.</param>
		private void InitializeWebProxy( Uri proxy, string proxyUser, string proxyPassword )
		{
			if ( proxy != null )
			{
				if ( ( proxyUser == null ) || ( proxyUser == String.Empty ) )
				{
					webProxy = new WebProxy( proxy, false );
				}
				else
				{
					webProxy = new WebProxy( 
						proxy, 
						false, 
						new string[] {}, 
						new NetworkCredential( proxyUser, proxyPassword, proxy.ToString() ) );
				}
			}
			else
			{
				webProxy = GlobalProxySelection.GetEmptyWebProxy();
			}
		}

		/// <summary>
		/// Creates a proxy key to use for the specified host.
		/// </summary>
		/// <param name="server">Uri of the host.</param>
		/// <returns></returns>
		private static string ProxyKey( Uri server )
		{
			return new UriBuilder( server.Scheme, server.Host.ToLower(), server.Port ).Uri.ToString();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">The simias server address.</param>
		public static ProxyState AddProxyState( Uri server )
		{
			return AddProxyState( server, null, null, null );
		}

		/// <summary>
		/// Adds a ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">The simias server address.</param>
		/// <param name="proxy">The proxy server address.</param>
		/// <param name="proxyUser">The user name for proxy authentication.</param>
		/// <param name="proxyPassword">The password for proxy authentication.</param>
		public static ProxyState AddProxyState( Uri server, Uri proxy, string proxyUser, string proxyPassword )
		{
			lock( proxyHash )
			{
				string key = ProxyKey( server );
				ProxyState ps = proxyHash[ key ] as ProxyState;
				if ( ps != null )
				{
					ps.InitializeWebProxy( proxy, proxyUser, proxyPassword );
				}
				else
				{
					ps = new ProxyState( proxy, proxyUser, proxyPassword );
					proxyHash[ key ] = ps;
				}

				return ps;
			}
		}

		/// <summary>
		/// Deletes the specified ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">Address of the server to delete a ProxyState object for.</param>
		public static void DeleteProxyState( Uri server )
		{
			lock ( proxyHash )
			{
				proxyHash[ ProxyKey( server ) ] = null;
			}
		}

		/// <summary>
		/// Gets a ProxyState object for the specified server address.
		/// </summary>
		/// <param name="server">Address of the server to find a ProxyState object for.</param>
		/// <returns>A corresponding ProxyState object.</returns>
		public static ProxyState GetProxyState( Uri server )
		{
			ProxyState ps;

			lock ( proxyHash )
			{
				ps = proxyHash[ ProxyKey( server ) ] as ProxyState;
			}

			return ( ps == null ) ? AddProxyState( server ) : ps;
		}

		#endregion

		#region IWebProxy Members

		/// <summary>
		/// Returns the URI of a proxy.
		/// </summary>
		/// <param name="destination">A Uri specifying the requested Internet resource.</param>
		/// <returns>A Uri containing the URI of the proxy used to contact destination.</returns>
		public Uri GetProxy( Uri destination )
		{
			return webProxy.GetProxy( destination );
		}

		/// <summary>
		/// The credentials to submit to the proxy server for authentication.
		/// </summary>
		public ICredentials Credentials
		{
			get { return webProxy.Credentials; }
			set	{ webProxy.Credentials = value; }
		}

		/// <summary>
		/// Indicates that the proxy should not be used for the specified host.
		/// </summary>
		/// <param name="host">The Uri of the host to check for proxy use.</param>
		/// <returns>true if the proxy server should not be used for host; otherwise, false.</returns>
		public bool IsBypassed( Uri host )
		{
			return webProxy.IsBypassed( host );
		}

		#endregion
	}

	/// <summary>
	/// Class to share and keep the client state for web communications.
	/// </summary>
	public class WebState
	{
		static string				userAgent = "Simias Client " 
			+ System.Reflection.Assembly.GetCallingAssembly().ImageRuntimeVersion 
			+ " OS=" 
			+ System.Environment.OSVersion.ToString();
		NetworkCredential			credentials;
		static Hashtable			cookieHash = new Hashtable();

		/// <summary>
		/// Get a WebState object for the specified domain and collection.
		/// </summary>
		/// <param name="domainID">The domain ID.</param>
		/// <param name="memberID">The member the client is running as.</param>
		public WebState(string domainID, string collectionID) :
			this(domainID)
		{
			Member currentMember = Store.GetStore().GetDomain( domainID ).GetCurrentMember();
			if ( currentMember != null )
			{
				// Get the credentials for this collection.
				credentials = new Credentials( domainID, collectionID, currentMember.UserID ).GetCredentials();
			}

			if (credentials == null)
			{
				new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( domainID, collectionID ));
				throw new NeedCredentialsException();
			}
		}

		/// <summary>
		/// Get a WebState object for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain ID.</param>
		/// <param name="collectionID">The collection ID.</param>
		/// <param name="memberID">The member the client is running as.</param>
		public WebState(string domainID, string collectionID, string memberID) :
			this(domainID)
		{
			// Get the credentials for this collection.
			credentials = new Credentials( domainID, collectionID, memberID ).GetCredentials();
			if (credentials == null)
			{
				new EventPublisher().RaiseEvent( new NeedCredentialsEventArgs( domainID, collectionID ) );
				throw new NeedCredentialsException();
			}
		}

		/// <summary>
		/// Get a WebState with the specified credential.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		public WebState( string domainID )
		{
			lock( cookieHash )
			{
				if (!cookieHash.ContainsKey(domainID))
				{
					cookieHash[ domainID ] = new CookieContainer();
				}
			}
		}
		
		/// <summary>
		/// Initialize the HttpWebRequest.
		/// </summary>
		/// <param name="request">The request to initialize.</param>
		/// <param name="domainID">The identifier for the domain.</param>
		public void InitializeWebRequest(HttpWebRequest request, string domainID)
		{
			request.UserAgent = userAgent;
			request.Credentials = credentials;
			request.CookieContainer = cookieHash[ domainID ] as CookieContainer;
			request.Proxy = ProxyState.GetProxyState( request.RequestUri );
			request.PreAuthenticate = true;
		}

		/// <summary>
		/// Initialize the web service proxy stub.
		/// </summary>
		/// <param name="request">The client proxy to initialize</param>
		/// <param name="domainID">The identifier for the domain.</param>
		public void InitializeWebClient(HttpWebClientProtocol request, string domainID)
		{
			request.UserAgent = userAgent;
			request.Credentials = credentials;
			request.CookieContainer = cookieHash[ domainID ] as CookieContainer;
			request.Proxy = ProxyState.GetProxyState( new Uri( request.Url ) );
			request.PreAuthenticate = true;
		}

		/// <summary>
		/// Resets the WebState object.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		static public void ResetWebState( string domainID )
		{
			lock( cookieHash )
			{
				cookieHash[ domainID ] = new CookieContainer();
			}
		}
	}
}
