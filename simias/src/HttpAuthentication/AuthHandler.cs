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
 *  Author: Todd Throne - tthrone@novell.com
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Services;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.Security.Web.AuthenticationService;
using Simias.Storage;

namespace Simias.Security.Web
{
	/// <summary>
	/// HttpModule used for Simias authentication
	/// </summary>
	public sealed class AuthenticationModule : IHttpModule
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( AuthenticationModule ) );

		/// <summary>
		/// Session tag used to store session information.
		/// </summary>
		private static readonly string sessionTag = "simias";

		// Response header set by the Http Authentication Module
		public readonly static string DomainIDHeader = "Domain-ID";

		/// <summary>
		/// Enabled if ssl required is set in the web.config.
		/// </summary>
		private bool sslRequired = true;

		/// <summary>
		/// Default ssl port.
		/// </summary>
		private int sslPort = 443;

		/// <summary>
		/// Handle to the store.
		/// </summary>
		private Store store = null;

		/// <summary>
		/// Contains services specified in the web.config that require no
		/// authentication. These may include entire web services, http
		/// handlers and individual web service methods.
		/// </summary>
		private Hashtable unauthenticatedServices = new Hashtable();

		#endregion

		#region Properties
		/// <summary>
		/// Gets the store handle for the store.
		/// 
		/// NOTE: It seems that calling Store.GetStore() statically, in the
		/// constructor or in the Init() call causes the Mac client to fail
		/// because it cannot find the Flaim libraries.
		/// </summary>
		private Store StoreReference
		{
			get 
			{ 
				lock ( this )
				{
					if ( store == null )
					{
						store = Store.GetStore();
					}
				}

				return store;
			}
		}
		#endregion

		#region Private Methods

		/// <summary>
		/// Occurs when ASP.NET acquires the current state (for example, session state) 
		/// associated with the current request.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="eventArgs">An EventArgs that contains the event data.</param>
		private void OnAcquireRequestState( Object source, EventArgs eventArgs ) 
		{
			// Get the context for the current request.
			HttpContext context = HttpContext.Current;

			// See if this request requires authentication.
			string webService = Path.GetFileName( context.Request.FilePath );
			if ( unauthenticatedServices.ContainsKey( webService ) == false )
			{
				// See if this request method requires authentication.
				string soapMethod = context.Request.Headers[ "SOAPAction" ];
				if ( ( soapMethod == null ) || ( unauthenticatedServices.ContainsKey( soapMethod ) == false ) )
				{
					if ( context.Session != null )
					{
						// See if the user has a session from a previous login.
						Session simiasSession = context.Session[ sessionTag ] as Session;
						if ( simiasSession != null )
						{
							context.User = simiasSession.User;
							if ( context.User.Identity.IsAuthenticated )
							{
								// The user is authenticated, set it as the current principal on this thread.
								Thread.CurrentPrincipal = context.User;
							}
							else
							{
								// The user is not authenticated on this session. See if there are
								// credentials specified on the request.
								VerifyPrincipalFromRequest( context );
							}
						}
						else
						{
							// A simias session from a previous login does not exist. See if there
							// are credentials specified on the request.
							VerifyPrincipalFromRequest( context );
						}
					}
					else
					{
						// There is no session setup. Authenticate every time.
						VerifyPrincipalFromRequest( context );
					}
				}
			}
		}

		/// <summary>
		/// Occurs when a security module has established the identity of the user.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="eventArgs">An EventArgs that contains the event data.</param>
		private void OnAuthenticateRequest( Object source, EventArgs eventArgs ) 
		{
			// There is no way to access session information from the OnAuthenticateRequest
			// event unless we implement our own cookie/session management.
			// 
			// We will handle authentication from our OnAcquireRequestState event instead
			// so that we can take advantage of the SessionStateModule implementation.

			// Verify that we are on a secure connection, if not redirect to https
			HttpContext context = HttpContext.Current;
			if ( ( context.Request.Url.IsLoopback == false ) && 
				 ( context.Request.IsSecureConnection == false ) && 
				 ( sslRequired == true ) ) 
			{
				// Redirect over https
				UriBuilder redirectedUri = new UriBuilder( context.Request.Url.ToString() );
				redirectedUri.Scheme = "https";
				redirectedUri.Port = sslPort;

				log.Debug( redirectedUri.Uri.ToString() );

				// You must have an SSL certificate configured on your web server for this to work
				context.Response.Redirect( redirectedUri.Uri.ToString() );
			}
		}

		/// <summary>
		/// Occurs as the first event in the HTTP pipeline chain of execution when ASP.NET 
		/// responds to a request.
		/// </summary>
		/// <param name="source">The source of the event.</param>
		/// <param name="eventArgs">An EventArgs that contains the event data.</param>
		private void OnBeginRequest( Object source, EventArgs eventArgs ) 
		{
			HttpApplication app = source as HttpApplication;
			string physicalPath = app.Request.PhysicalPath;

			if ( ( app.Request.Path.IndexOf( '\\' ) >= 0 ) || 
				( Path.GetFullPath( physicalPath ) != physicalPath ) )
			{
				log.Debug( "AuthenticationModule.OnBeginRequest - Security attack detected!!" );
				throw new HttpException( 404, "Not Found" );
			}
		}

		/// <summary>
		/// Parses the web.config appSettings values for AuthNotRequired.
		/// </summary>
		/// <param name="parseString">String that contains the service names.</param>
		private void ParseAuthNotRequiredServices( string parseString )
		{
			string[] services = parseString.Split( new char[] { ',' } );
			foreach ( string s in services )
			{
				// Web methods contain a ':'.
				if ( s.IndexOf( ':' ) != -1 )
				{
					// The format for a web method is as follows:
					// [ 0 ] - Web method.
					// [ 1 ] - Web class.
					// [ 2 ] - Web assembly.
					string[] values = s.Split( new char[] { ':' } );
					string webMethod = values[ 0 ].Trim();
					string webClass = values[ 1 ].Trim();
					string webAssembly = values[ 2 ].Trim();

					// Load the assembly that contains the web class.
					Assembly assembly = AppDomain.CurrentDomain.Load( webAssembly );
					if ( assembly != null )
					{
						// Get the class type information.
						Type type = assembly.GetType( webClass );
						if ( type != null )
						{
							// Get the custome attributes for this class.
							object[] customAttrs = type.GetCustomAttributes( typeof( WebServiceAttribute ), false );
							if ( customAttrs.Length == 1 )
							{
								// Add the namespace string exactly as it would occur in the HTTP header
								// SOAPAction parameter.
								WebServiceAttribute wsa = customAttrs[ 0 ] as WebServiceAttribute;
								string nss = String.Format( "\"{0}/{1}\"", wsa.Namespace.TrimEnd( new char[] {'/'} ), webMethod );
								unauthenticatedServices.Add( nss, null );
							}
						}
					}
				}
				else
				{
					// This is a web service or an http handler.
					unauthenticatedServices.Add( s.Trim(), null );
				}
			}
		}

		/// <summary>
		/// Tries to authenticate the current request if authorization headers are present.
		/// </summary>
		/// <param name="context">HttpContext that represents the request.</param>
		private void VerifyPrincipalFromRequest( HttpContext context )
		{
			// Get the Domain ID.
			string domainID = context.Request.Headers.Get( Http.DomainIDHeader );
			if ( domainID == null )
			{
				if ( StoreReference.IsEnterpriseServer )				
				{
					// If this is an enterprise server use the default domain.
					domainID = StoreReference.DefaultDomain;
				}
				else if ( context.Request.Url.IsLoopback )
				{
					// If this address is loopback, set the local domain in the HTTP context.
					domainID = StoreReference.LocalDomain;
				}
			}

			// Try and authenticate the request.
			if ( domainID != null )
			{
				if ( Http.GetMember( domainID, context ) != null )
				{
					// Set the session to never expire on the local web service.
					if ( domainID == StoreReference.LocalDomain )
					{
						// Set to a very long time.
						context.Session.Timeout = 60 * 24 * 365;
					}
					else
					{
						// Set all other sessions to 10 minutes.
						context.Session.Timeout = 10;
					}
				}
			}
			else
			{
				string realm = 
					StoreReference.IsEnterpriseServer ? 
						StoreReference.GetDomain( store.DefaultDomain ).Name : 
						Environment.MachineName;

				context.Response.StatusCode = 401;
				context.Response.StatusDescription = "Unauthorized";
				context.Response.AddHeader( "WWW-Authenticate", String.Concat( "Basic realm=\"", realm, "\"" ) );
				context.ApplicationInstance.CompleteRequest();
			}
		}

		#endregion

		#region IHttpModule Members

		/// <summary>
		/// Initializes a module and prepares it to handle requests.
		/// </summary>
		/// <param name="app">An HttpApplication that provides access to the methods, 
		/// properties, and events common to all application objects within an ASP.NET 
		/// application </param>
		public void Init( HttpApplication app ) 
		{
			log.Debug( "AuthenticationModule Init()" );

			// Register for the interesting events in the HTTP life-cycle.
			app.BeginRequest += new EventHandler( OnBeginRequest );
			app.AuthenticateRequest += new EventHandler( OnAuthenticateRequest );
			app.AcquireRequestState += new EventHandler( OnAcquireRequestState );

			// Get the application settings from the web.config.
			NameValueCollection AppSettings = ConfigurationSettings.AppSettings;
			string[] settings = AppSettings.GetValues( "SimiasRequireSSL" );
			if ( ( settings != null ) && ( settings[ 0 ] != null ) )
			{
				if ( String.Compare( settings[ 0 ], "no", true ) == 0 )
				{
					sslRequired = false;
				}
			}

			// Get the ssl port setting.
			settings = AppSettings.GetValues( "SimiasSSLPort" );
			if ( ( settings != null ) && ( settings[ 0 ] != null ) )
			{
				sslPort = Convert.ToInt32( settings[ 0 ] );
			}

			// Get the services that do not need authentication.
			settings = AppSettings.GetValues( "SimiasAuthNotRequired" );
			if ( ( settings != null ) && ( settings[ 0 ] != null ) )
			{
				ParseAuthNotRequiredServices( settings[ 0 ] );
			}
		}

		/// <summary>
		/// Disposes of the resources (other than memory) used by the module that 
		/// implements IHttpModule. 
		/// </summary>
		public void Dispose() 
		{
		}

		#endregion
	}
}
