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
	/// Class to share and keep the client state for web communications.
	/// </summary>
	public class WebState
	{
		static string				userAgent = "Simias Client " 
			+ System.Reflection.Assembly.GetCallingAssembly().ImageRuntimeVersion 
			+ " OS=" 
			+ System.Environment.OSVersion.ToString();
		static CookieContainer		cookies = new CookieContainer();
		IWebProxy					proxy;
		NetworkCredential			credentials;

		/*
		/// <summary>
		/// Get a WebState object for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain ID.</param>
		/// <param name="memberID">The member the client is running as.</param>
		public WebState(string domainID, string memberID) :
			this()
		{
			// Get the credentials for this collection.
			credentials = new Credentials(domainID, memberID).GetCredentials();
			if (credentials == null)
			{
				new EventPublisher().RaiseEvent(new NeedCredentialsEventArgs(domainID, memberID));
				throw new NeedCredentialsException();
			}
		}
		*/

		/// <summary>
		/// Get a WebState object for the specified domain and collection.
		/// </summary>
		/// <param name="domainID">The domain ID.</param>
		/// <param name="memberID">The member the client is running as.</param>
		public WebState(string domainID, string collectionID) :
			this()
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
			this()
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
		public WebState()
		{
			// Now set the proxy.
			proxy = null;
		}
		
		/// <summary>
		/// Initialize the HttpWebRequest.
		/// </summary>
		/// <param name="request">The request to initialize.</param>
		public void InitializeWebRequest(HttpWebRequest request)
		{
			request.UserAgent = userAgent;
			request.Credentials = credentials;
			request.CookieContainer = cookies;
			// request.Proxy = proxy;
			request.PreAuthenticate = true;
		}

		/// <summary>
		/// Initialize the web service proxy stub.
		/// </summary>
		/// <param name="request">The client proxy to initialize</param>
		public void InitializeWebClient(HttpWebClientProtocol request)
		{
			request.UserAgent = userAgent;
			request.Credentials = credentials;
			request.CookieContainer = cookies;
			// request.Proxy = proxy;
			request.PreAuthenticate = true;
		}
	}
}
