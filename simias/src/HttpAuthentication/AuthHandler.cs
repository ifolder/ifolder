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
using System.Collections.Specialized;
using System.Configuration;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
//using Simias.Client;
//using Simias.Client.Authentication;

using Simias;
using Simias.Storage;
using Simias.Security.Web.AuthenticationService;
using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.Security.Web
{
	public delegate void AuthenticationEventHandler( object sender, AuthenticationEventArgs e );
}


namespace Simias.Security.Web
{
	public sealed class AuthenticationEventArgs : EventArgs 
	{
		private IPrincipal _IPrincipalUser;
		private HttpContext _Context;
		private string _User;
		private string _Password;

		public AuthenticationEventArgs(HttpContext context)
		{
			_Context = context;
		}

		public AuthenticationEventArgs(HttpContext context,
			string user, string password)
		{
			_Context = context;
			_User = user;
			_Password = password;
		}

		public  HttpContext Context 
		{ 
			get { return _Context;}
		}
		public IPrincipal Principal 
		{ 
			get { return _IPrincipalUser;} 
			set { _IPrincipalUser = value;}
		}
		public void Authenticate()
		{
			GenericIdentity i = new GenericIdentity(User);
			this.Principal = new GenericPrincipal(i, new String[0]);
		}
		public void Authenticate(string[] roles)
		{
			GenericIdentity i = new GenericIdentity(User);
			this.Principal = new GenericPrincipal(i, roles);
		}
		public string User 
		{
			get { return _User; }
			set { _User = value; }
		}
		public string Password
		{
			get { return _Password; }
			set { _Password = value; }
		}
		public bool HasCredentials 
		{
			get 
			{
				if ((_User == null) || (_Password == null))
					return false;
				return true;
			}
		}
	}
}

namespace Simias.Security.Web
{
	/// <summary>
	/// Internal class for managing and authenticating
	/// credentials
	/// 
	/// Note::Today we only support 'basic'
	/// </summary>
	internal class SimiasCredentials
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string domainID;
		private string username;
		private string password;
		private string authType;

		private readonly char[] colonDelimeter = {':'};
		private readonly char[] backDelimeter = {'\\'};

		#region Properties
		public string AuthType
		{
			get { return this.authType; }
			set { this.authType = value; }
		}

		public string DomainID
		{
			get { return this.domainID; }
			set { this.domainID = value; }
		}

		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		public string Username 
		{
			get { return this.username; }
			set { this.username = value; }
		}
		#endregion

		#region Constructors
		public SimiasCredentials()
		{
		}

		public SimiasCredentials( string username, string password )
		{
			this.username = username;
			this.password = password;
			this.authType = "basic";
		}
		#endregion

		#region Methods
		internal Simias.Authentication.Status Authenticate( Type iAuthService )
		{
			Simias.Authentication.Status authStatus = null;

			if ( this.username != null && this.password != null )
			{
				if ( this.authType == "basic" )
				{
					try
					{
						IAuthenticationService iAuth;
						iAuth = (IAuthenticationService) Activator.CreateInstance( iAuthService );
						authStatus = iAuth.AuthenticateByName( this.username, this.password );
						iAuth = null;
					}
					catch(Exception e)
					{
						log.Error( e.Message );
						log.Error( e.StackTrace );
						authStatus = new Simias.Authentication.Status( SCodes.InternalException );
						authStatus.ExceptionMessage = e.Message;
					}
				}
				else
				if ( this.authType == "ppk" )
				{
					//
					// FIXME:: for now let's just verify the user in the store and let him through
					//
					authStatus = new Simias.Authentication.Status( SCodes.Unknown );

					try
					{
						Simias.Storage.Domain domain = Store.GetStore().GetDomain( this.domainID );
						if ( domain != null )
						{
							Member member = domain.GetMemberByID( this.username );
							if ( member != null )
							{
								authStatus.UserID = member.UserID;
								authStatus.UserName = member.Name;
								authStatus.statusCode = SCodes.Success;
							}
							else
							{
								authStatus.statusCode = SCodes.UnknownUser;
							}
						}
						else
						{
							authStatus.statusCode = SCodes.UnknownDomain;
						}
					}
					catch( Exception e )
					{
						log.Debug( e.Message );
						log.Debug( e.StackTrace );
					}
				}
			}
			else
			{
				authStatus = new Simias.Authentication.Status( SCodes.InvalidCredentials );
			}

			return authStatus;
		}

		internal bool AuthorizationHeaderToCredentials( string authHeader )
		{
			bool returnStatus = false;

			//
   			// Make sure we are dealing with "Basic" credentials
   			//
   
			if ( authHeader.StartsWith( "Basic " ) )
			{
				//
				// The authHeader after the basic signature is encoded
				//
   
				authHeader = authHeader.Remove(0, 6);
				byte[] credential = System.Convert.FromBase64String( authHeader );
				string decodedCredential = 
					System.Text.Encoding.ASCII.GetString(
						credential, 
						0, 
						credential.Length);
   
				//
				// clients that newed up a NetCredential object with a URL
				// come though on the authorization line like:
				// http://domain:port/simias10/service.asmx\username:password
				//

				string[] credentials = decodedCredential.Split( this.backDelimeter );
				if (credentials.Length == 1)
				{
					credentials = decodedCredential.Split( this.colonDelimeter, 2 );
				}
				else
				if (credentials.Length >= 2)
				{
					credentials = 
						credentials[credentials.Length - 1].Split( colonDelimeter, 2 );
				}

				if (credentials.Length == 2)
				{
					// Check if this is a Rendezvous client attempting to authenticate
					if ( credentials[1].StartsWith( "@ppk@" ) )
					{
						this.authType = "ppk";
						this.password = credentials[1].Remove(0, 5);
					}
					else
					{
						this.authType = "basic";
						this.password = credentials[1];
					}

					this.username = credentials[0];
					returnStatus = true;
				}

				credentials = null;
			}

			return returnStatus;
		}

		public bool HasCredentials()
		{
			if ( this.username != null && this.password != null )
			{
				return true;
			}

			return false;
		}
		#endregion
	}

	/// <summary>
	/// HttpModule used for Simias authentication
	/// </summary>
	public sealed class AuthenticationModule : IHttpModule
	{
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private AuthenticationEventHandler m_eventHandler = null;

		private bool m_sslRequired = false;
		private string authenticationProvider;
		private string m_authenticationRealm;
		private readonly string sessionTag = "simias-user";
		private readonly string[] rolesArray = { "users" };
		//private Store store;

		private	Assembly authAssembly;
		private Type iAuthService;

		public void Init(System.Web.HttpApplication app) 
		{
			NameValueCollection AppSettings = System.Configuration.ConfigurationSettings.AppSettings;
			string [] settings;

			log.Debug( "AuthenticationModule Init()");

			//this.store = Store.GetStore();

			app.BeginRequest += new
				EventHandler( this.OnBeginRequest );

			app.AuthenticateRequest += new
				EventHandler( this.OnAuthenticateRequest );

			app.AcquireRequestState += new
				EventHandler( this.OnAcquireRequestState );

			app.ReleaseRequestState += new
				EventHandler( this.OnReleaseRequestState );

			app.EndRequest += new
				EventHandler( this.OnEndRequest );

			app.Disposed += new
				EventHandler( this.OnDisposed );

			settings = AppSettings.GetValues("require ssl");

			if (settings != null && settings[0] != null && settings[0].Equals("no"))
			{
				m_sslRequired = false;
			}

			// FIXME:: Change the realm to the server domain name
			settings = AppSettings.GetValues( "authentication realm" );
			if ( settings != null && settings[0] != null )
			{
				m_authenticationRealm = settings[0];
			}
			else
			{
				log.Debug( "Web.Config - \"authentication realm\" not configured" );
				throw new Exception( "Web.Config - \"authentication realm\" not configured" );
			}

			settings = AppSettings.GetValues( "authentication provider" );
			if (settings != null && settings[0] != null)
			{
				authenticationProvider = settings[0];
			}
			else
			{
				log.Debug( "Web.Config - \"authentication provider\" not configured" );
				throw new Exception("Web.Config - \"authentication provider\" not configured");
			}

			log.Debug( "Attempting to load: " + authenticationProvider );

			// Load the assembly
			authAssembly = AppDomain.CurrentDomain.Load( authenticationProvider );
   
			// Find the type implementing IAuthenticationService
			Type[] assemblyTypes = authAssembly.GetTypes();
			foreach ( Type t in assemblyTypes )
			{
				// Look for type that has the IAuthenticationServiceAttribute
				if (Attribute.GetCustomAttribute( t, typeof(IAuthenticationServiceAttribute )) != null )
				{
					iAuthService = t;
					break;
				}
			}

			if ( iAuthService == null )
			{
				throw new 
					Exception( "Web.Config - couldn't load authentication provider: " + authenticationProvider );
			}
		}

		public void Dispose() 
		{
		}

		public event AuthenticationEventHandler Authenticate 
		{
			add{ m_eventHandler += value; }
			remove{ m_eventHandler -= value; }
		}
 
		void OnAcquireRequestState(Object source, EventArgs eventArgs) 
		{
			bool challengeRequest = true;
			HttpApplication app = (HttpApplication)source;
			HttpContext context = HttpContext.Current;

			//log.Debug( "OnAcquireRequestState - called" );

			//
			// Check the session to see if we have already authenticated this user
			//

			if ( context.Session != null )
			{
				//log.Debug( "SessionTag: " + app.Context.Session.SessionID );
   				GenericPrincipal sessionPrincipal;
   
				sessionPrincipal = (GenericPrincipal)( context.Session[this.sessionTag] );
   				if (sessionPrincipal != null)
   				{
   					context.User = sessionPrincipal;
   					Thread.CurrentPrincipal = context.User;
   				}
				else
				{
					//log.Debug( "Session object " + sessionTag + " not set!" );
				}
   
   				if ( context.User.Identity.IsAuthenticated )
   				{
					challengeRequest = false;

					//log.Debug( "User is authenticated!" );
				    //log.Debug( "Context.User.Identity exists: " + context.User.Identity.Name );
   				}
   				else
   				{
					//log.Debug( "User is not authenticated" );
   
   					//
   					// Check for Authorization headers coming back
   					//
   
   					string[] encodedCredentials = context.Request.Headers.GetValues( "Authorization" );
					if ( encodedCredentials != null && encodedCredentials[0] != null )
					{
						bool success;

						SimiasCredentials creds = new SimiasCredentials();
						success = creds.AuthorizationHeaderToCredentials( encodedCredentials[0] );
						if ( success == true )
						{
							Simias.Authentication.Status authStatus;
							authStatus = creds.Authenticate( this.iAuthService );

							this.SetSimiasResponseHeaders( context, authStatus );

							if ( authStatus.statusCode == SCodes.Success ||
								authStatus.statusCode == SCodes.SuccessInGrace )
							{
								challengeRequest = false;
								this.SetLastLoginTime( creds, authStatus );
   
								//
								// Set the context user for this request
								//
   
								context.User = 
									new GenericPrincipal(
										new GenericIdentity(
											authStatus.UserID,
											"Basic authentication"), 
											rolesArray);
   
								//
								// Set the current user for this thread
								//
   
								Thread.CurrentPrincipal = context.User;
   							
								//
								// Save off the user principle so we can associate them
								// with their next request
								//
   
								context.Session.Add( sessionTag, context.User );
								context.SkipAuthorization = true;

								//log.Debug( "Authentication successful - session object set!" );

								if ( context.Items["simias-login"] != null )
								{
									log.Debug( "completing login request" );
									app.CompleteRequest();
								}
							}
						}
						else
						{
							log.Debug( "Invalid credentials" );
						}
   					}
   				}
			}
			else
			{
				//log.Debug( "No context.Session in OnAcquireRequestState" );

				// Check if this is our special URL
				if ( context.Items["simias-login"] != null )
				{
					string[] encodedCredentials = context.Request.Headers.GetValues("Authorization");
					if ( encodedCredentials != null && encodedCredentials[0] != null )
					{
						SimiasCredentials creds = new SimiasCredentials();
						bool success = 
							creds.AuthorizationHeaderToCredentials( encodedCredentials[0] );
						if ( success == true )
						{
							// Get the domain from the request header
							string[] domainID = 
								context.Request.Headers.GetValues(
									Simias.Security.Web.AuthenticationService.Login.DomainIDHeader );
							if ( domainID != null && domainID[0] != null && domainID[0] != "" )
							{
								creds.DomainID = domainID[0];
							}

							Simias.Authentication.Status authStatus = 
								creds.Authenticate( this.iAuthService );

							this.SetSimiasResponseHeaders( context, authStatus );

							if ( authStatus.statusCode == SCodes.Success ||
								authStatus.statusCode == SCodes.SuccessInGrace )
							{
								this.SetLastLoginTime( creds, authStatus );
							}

							app.CompleteRequest();
							challengeRequest = false;
						}
					}
				}
			}

			//
			// If no Authorization headers or authentication fails then
			// challenge the request
			//

			if (challengeRequest == true)
			{
				//
				// Force a 'Basic' popup dialog
				//

				Simias.Storage.Domain domain;
				string[] domainID = 
					context.Request.Headers.GetValues(
					Simias.Security.Web.AuthenticationService.Login.DomainIDHeader);
				if ( domainID != null && domainID[0] != null && domainID[0] != "" )
				{
					domain = Store.GetStore().GetDomain( domainID[0] );
				}
				else
				{
					Store lStore = Store.GetStore();
					domain = lStore.GetDomain( lStore.DefaultDomain );
				}

				context.Response.StatusCode = 401;
				context.Response.StatusDescription = "Unauthorized";
				context.Response.AddHeader( 
					"WWW-Authenticate", 
					String.Concat("Basic realm=\"", (domain != null) ? domain.Name : m_authenticationRealm, "\""));					

				/*
				context.Response.AddHeader( 
					"WWW-Authenticate", 
					String.Concat("Basic realm=\"", m_authenticationRealm, "\""));					
				*/

	            //
		        // Add to the session data to force a cookie with the 401.
			    // This will enable us to tell if cookies are supported on the client
				//

				// context.Session.Add("AuthRequest", authRequest);
				app.CompleteRequest();
			}
		}

		void OnAuthenticateRequest(Object source, EventArgs eventArgs) 
		{
			HttpContext context = HttpContext.Current;

			//log.Debug( "OnAuthenticateRequest called" );

			//
			// There is no way to access session information from the OnAuthenticateRequest
			// event unless we implement our own cookie/session management.
			// 
			// We will handle authentication from our OnAcquireRequestState event instead
			// so that we can take advantage of the SessionStateModule implementation.
			//

			//
			// Verify that we are on a secure connection, if not redirect to https
			//

			if (context.Request.IsSecureConnection == false
				&& m_sslRequired == true) 
			{
				//
				// Redirect over https
				//

				UriBuilder redirectedUri = new UriBuilder(context.Request.Url.ToString());
				redirectedUri.Scheme = "https";
				redirectedUri.Port = 443;

				log.Debug( redirectedUri.ToString() );

				//
				// You must have an SSL certificate configured on your web
				// server for this to work
				//

				context.Response.Redirect( redirectedUri.ToString() );
			}

			HttpCookieCollection cookies;
			Boolean incomingCookies = false;

			//
			// Store request cookie count information in the context items list
			// to be accessed during the OnAcquireRequestState event.
			// During the OnAcquireRequestState event the request cookie
			// information is bogus!
			//
            // This is done to determine if the client supports cookies
            //

			cookies = context.Request.Cookies;
			incomingCookies = (cookies.Count != 0);
            context.Items.Add("IncomingCookies", incomingCookies);

			//log.Debug( "Incoming cookie: " + incomingCookies.ToString() );
			return;
		}

		void OnBeginRequest(Object source, EventArgs eventArgs) 
		{
			HttpApplication app = (HttpApplication)source;
			HttpContext context = HttpContext.Current;

			//log.Debug( "OnBeginRequest called" );

            string physicalPath = app.Request.PhysicalPath;

            if (app.Request.Path.IndexOf('\\') >= 0 || 
				Path.GetFullPath(physicalPath) != physicalPath)
            {
				log.Debug( "AuthenticationModule.OnBeginRequest - Security attack detected!!" );
				throw new HttpException(404, "Not Found");
            }

			if ( Simias.Security.Web.AuthenticationService.Login.Path.ToLower() == 
				context.Request.Path.ToLower() )
			{
				context.Items.Add( "simias-login", "true" );
			}
		}

		void OnDisposed( Object source, EventArgs eventArgs ) 
		{
			//log.Debug( "OnDisposed - called" );
		}

		void OnEndRequest( Object source, EventArgs eventArgs ) 
		{
			//log.Debug( "OnEndRequest - called" );
		}

		void OnReleaseRequestState( Object source, EventArgs eventArgs ) 
		{
			//log.Debug( "OnReleaseRequestState - called" );
		}

		private void OnAuthenticate(AuthenticationEventArgs e)
		{
			//log.Debug( "OnAuthenticate - called" );

			if (m_eventHandler == null)
			{
				return;
			}

			//log.Debug( "Calling m_eventHandler in OnAuthenticate" );

			m_eventHandler(this, e);
			if (e.User != null)
			{
				e.Context.User = e.Principal;
			}
		}

		private void SetLastLoginTime( SimiasCredentials creds, Simias.Authentication.Status authStatus )
		{
			if ( creds.DomainID != null )
			{
				Simias.Storage.Domain domain = Store.GetStore().GetDomain( creds.DomainID );
				if ( domain != null )
				{
					Member member = domain.GetMemberByID( authStatus.UserID );
					Property p = new Property( "LastLogin", DateTime.Now );
					p.LocalProperty = true;
					member.Properties.ModifyProperty( p );
					domain.Commit( member );
				}
			}
		}

		private void SetSimiasResponseHeaders( HttpContext context, Simias.Authentication.Status authStatus )
		{
			switch ( authStatus.statusCode )
			{
				case SCodes.Success:
				{
					context.Response.StatusCode = 200;
					break;	
				}

				case SCodes.SuccessInGrace:
				{
					context.Response.StatusCode = 200;
					context.Response.AppendHeader(
						Login.GraceTotalHeader,
						authStatus.TotalGraceLogins.ToString() );

					context.Response.AppendHeader(
						Login.GraceRemainingHeader,
						authStatus.RemainingGraceLogins.ToString() );
					break;
				}

				case SCodes.AccountDisabled:
				{
					context.Response.StatusCode = 500;
					context.Response.AddHeader(
						Login.SimiasErrorHeader,
						StatusCodes.AccountDisabled.ToString() );
					break;
				}

				case SCodes.AccountLockout:
				{
					context.Response.StatusCode = 500;
					context.Response.AddHeader(
						Login.SimiasErrorHeader,
						StatusCodes.AccountLockout.ToString() );
					break;
				}

				case SCodes.AmbiguousUser:
				{
					context.Response.StatusCode = 500;
					context.Response.AddHeader(
						Login.SimiasErrorHeader,
						StatusCodes.AmbiguousUser.ToString() );
					break;
				}

				case SCodes.UnknownUser:
				{
					context.Response.StatusCode = 500;
					context.Response.AddHeader(
						Login.SimiasErrorHeader,
						StatusCodes.UnknownUser.ToString() );
					break;
				}

				case SCodes.Unknown:
				{
					context.Response.StatusCode = 500;
					context.Response.AddHeader(
						Login.SimiasErrorHeader,
						StatusCodes.Unknown.ToString() );
					break;
				}

				case SCodes.InvalidPassword:
				{
					context.Response.StatusCode = 500;
					context.Response.AppendHeader(
						Login.SimiasErrorHeader,
						StatusCodes.InvalidPassword.ToString() );
					break;
				}

				case SCodes.InternalException:
				{
					context.Response.StatusCode = 500;
					context.Response.AddHeader(
						Login.SimiasErrorHeader,
						StatusCodes.InternalException.ToString() );
					break;
				}

				default:
					context.Response.StatusCode = 401;
					break;
			}
		}
	}
}
