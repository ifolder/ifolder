/***********************************************************************
 *  $RCSfile: Domain.cs,v $
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
using System.Threading;
using System.Web;
using System.Xml;

namespace iFolder.Client
{
	/// <summary>
	/// status codes returned by remote authentication modules
	/// </summary>
	public enum LoginStatus : uint
	{
		/// <summary>
		/// Successful authentication
		/// </summary>
		Success = 0x00000000,

		/// <summary>
		/// Successful authentication but within a grace login period
		/// </summary>
		SuccessInGrace = 0x00000001,

		/// <summary>
		/// The certificate is invalid.
		/// </summary>
		InvalidCertificate = 0x00000002,

		/// <summary>
		/// Invalid or Unknown user specified
		/// </summary>
		UnknownUser = 0x1f000001,

		/// <summary>
		/// Ambigous user - more than one user exists 
		/// </summary>
		AmbiguousUser = 0x1f000002,

		/// <summary>
		/// The credentials may have invalid characters etc.
		/// </summary>
		InvalidCredentials = 0x1f000003,

		/// <summary>
		/// Invalid password specified
		/// </summary>
		InvalidPassword = 0x1f000020,

		/// <summary>
		/// The account has been disabled by an administrator
		/// </summary>
		AccountDisabled = 0x1f000040,

		/// <summary>
		/// The account has been locked due to excessive login failures
		/// or possibly the grace logins have all been consumed
		/// </summary>
		AccountLockout = 0x1f000041,

		/// <summary>
		/// The simias account has been disabled by the administrator.
		/// </summary>
		LoginDisabled = 0x1f000042,

		/// <summary>
		/// The specified domain was unknown
		/// </summary>
		UnknownDomain = 0x1f000060,

		/// <summary>
		/// Authentication failed due to an internal exception
		/// </summary>
		InternalException = 0x1f000100,

		/// <summary>
		/// The authentication provider does not support the method
		/// </summary>
		MethodNotSupported = 0x1f000101,
	
		/// <summary>
		/// The operation timed out on the client request
		/// </summary>
		Timeout = 0x1f000102,

		/// <summary>
		/// The operation time out resolving the host
		/// or pinging an expected web service on the host
		/// </summary>
		UnknownHost = 0x1f000102,

		/// <summary>
		/// Authentication failed with an unknown reason
		/// </summary>
		Unknown = 0x1f001fff
	}

	/// <summary>
	/// A Domain object
	/// </summary>
	public class Domain
	{
		#region Domain Types
		
		/// <summary>
		/// User Information
		/// </summary>
		private string memberID = String.Empty;
		private NetworkCredential credentials = null;
		private bool authenticated = false;
		private bool savepassword = false;

		/// <summary>
		/// Domain Information
		/// </summary>
		private string name = String.Empty;
		private string id = String.Empty;
		private string description = String.Empty;
		private Host host = null;
		private bool defaultDomain = false;
		
		/// <summary>
		/// Web Service Paths
		/// </summary>
		private readonly static string LoginPath = "/simias10/Login.ashx";
		internal static string DomainServicePath = "/simias10/DomainService.asmx";
		internal static string DomainService = "/DomainService.asmx";

		/// <summary>
		/// Custom http request headers needed by Simias
		/// Authentication module
		/// </summary>
		public readonly static string DomainIDHeader = "Domain-ID";
		public readonly static string BasicEncodingHeader = "Basic-Encoding";

		/// <summary>
		/// Custom http response headers set by Simias
		/// Authentication module
		/// </summary>
		public readonly static string GraceTotalHeader = "Simias-Grace-Total";
		public readonly static string GraceRemainingHeader = "Simias-Grace-Remaining";
		public readonly static string SimiasErrorHeader = "Simias-Error";

		private static bool certificatePolicyInitialized = false;
		#endregion

		#region Properties
		/// <summary>
		/// True if the user was authenticated against
		/// the remote domain.
		/// False the user is not authenticated.
		/// </summary>
		public bool Authenticated
		{
			get 
			{ 
				return authenticated;
			}
		}
		
		internal NetworkCredential Credentials
		{
			get
			{
				return  this.credentials;
			}
		}
		
		/*
		internal iFolder.Client.Host Host
		{
			get
			{
				return this.host;
			}
		}
		*/
		
		/// <summary>
		/// Gets the friendly name of the domain
		/// </summary>
		public string Name
		{
			get 
			{ 
				return name;
			}
		}
		
		/// <summary>
		/// Default domain
		/// </summary>
		public bool Default
		{
			get 
			{ 
				return defaultDomain;
			}

			set
			{
				defaultDomain = value;
			}
		}
		
		/// <summary>
		/// Gets the unique id of the domain
		/// </summary>
		public string ID
		{
			get 
			{ 
				return id;
			}
		}
		
		/// <summary>
		/// Detailed descripion of the domain
		/// </summary>
		public string Description
		{
			get 
			{ 
				return description;
			}
		}
		
		/// <summary>
		/// Gets the Host the user logged into
		/// </summary>
		public Host	Host
		{
			get 
			{ 
				return host;
			}
		}

		/// <summary>
		/// Gets the configured user
		/// </summary>
		public string User
		{
			get 
			{ 
				return this.credentials.UserName;
			}
		}

		/// <summary>
		/// Gets the configured user's ID
		/// </summary>
		public string UserID
		{
			get 
			{ 
				return this.memberID;
			}
		}
		#endregion

		#region Constructors

		static Domain()
		{
			if ( Domain.certificatePolicyInitialized == false )
			{
				ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
				Domain.certificatePolicyInitialized = true;
			}
		}

		private Domain()
		{

		}

		private Domain( Host host, string username, string password )
		{
			this.host = host;
			this.credentials = new NetworkCredential( username, password );
		}
		
		private Domain( string domainID )
		{
			string path = GetDomainPathFromID( domainID );
			if ( path == null )
			{
				throw new UnknownDomainException( domainID );
			}

			this.ConfigFileToDomain( path );
		}
		#endregion
		
		#region Private Methods

		/// <summary>
		/// Private method to retrieve a saved domain
		/// configuration file and deserialize the
		/// contents back to the domain.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void ConfigFileToDomain( string configFile )
		{
			XmlDocument doc = new XmlDocument();
			doc.Load( configFile );

			// Make sure the document is valid and then process
			XmlNode rootNode = doc.FirstChild;

			// If there is an xml declaration attached to the document
			// move past it
			if ( rootNode.NodeType == XmlNodeType.XmlDeclaration )
			{
				rootNode = rootNode.NextSibling;
				if ( rootNode == null )
				{
					throw new ApplicationException( "Not a valid domain configuration" );
				}
			}

			if ( rootNode.NodeType != XmlNodeType.Element )
			{
				throw new ApplicationException( "Not a valid domain configuration" );
			}
		
			if ( rootNode.Name.ToLower() != "domain" )
			{
				throw new ApplicationException( "Not a valid domain configuration" );
			}

			// get the attributes
			foreach( XmlAttribute attr in rootNode.Attributes )
			{
				switch( attr.Name.ToLower() )
				{
					case "id":
					{
						this.id = attr.InnerText;
						break;
					}

					case "name":
					{
						this.name = attr.InnerText;
						break;
					}

					case "description":
					{
						this.description = attr.InnerText;
						break;
					}
				}
			}

			// Get member information, host information and default info
			foreach( XmlNode node in rootNode.ChildNodes )
			{
				switch( node.Name.ToLower() )
				{
					case "member":
					{
						string name = String.Empty;
						string pwd = String.Empty;

						XmlAttribute attr = node.Attributes[ "id" ];
						if ( attr != null )
						{
							this.memberID = attr.InnerText;
						}

						attr = node.Attributes[ "name" ];
						if ( attr != null )
						{
							name = attr.InnerText;
						}

						attr = node.Attributes[ "pwd" ];
						if ( attr != null )
						{
							this.savepassword = true;
							pwd = attr.InnerText;
						}

						this.credentials = new NetworkCredential( name, pwd );
						break;
					}

					case "host":
					{
						//this.host = new iFolder.Client.Host( node.InnerText );
						this.host = new Host( node.InnerText );
						break;
					}

					case "default":
					{
						this.defaultDomain = 
							( node.InnerText.ToLower() == "true" ) ? true : false;
						break;
					}
				}
			}
		}

		/// <summary>
		/// Private method to get domain information
		/// Assumes a login has performed and the private
		/// field memberID is valid.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void GetDomainInformation()
		{
			try
			{
				// Create the domain service web client object.
				DomainService domainService = SetupDomainService( true );
				DomainInfo info = domainService.GetDomainInfo( this.memberID );
				if ( info != null )
				{
					this.description = info.Description;
					this.name = info.Name;
					this.id = info.ID;
				}
			}
			catch ( WebException we )
			{
				/*
				if ( we.Status == WebExceptionStatus.TrustFailure )
				{
					hostUp = true;
				}
				*/

				throw we;
			}
			
			return;
		}

		/// <summary>
		/// Login to a remote domain using username and password
		/// Assumes a slave domain has been provisioned locally
		/// </summary>
		/// <param name="host">The uri to the host.</param>
		/// <param name="domainID">ID of the remote domain.</param>
		/// <param name="networkCredential">The credentials to authenticate with.</param>
		/// <param name="calledRecursive">True if called recursively.</param>
		/// <returns>
		/// The status of the remote authentication
		/// </returns>
		private LoginStatus RemoteLogin()
		{
			LoginStatus status = LoginStatus.Unknown;
			HttpWebResponse response = null;

			Uri loginUri = new Uri( host.Uri, Domain.LoginPath );
			HttpWebRequest request = WebRequest.Create( loginUri ) as HttpWebRequest;
//			WebState webState = new WebState( domainID );
//			webState.InitializeWebRequest( request, domainID );

			request.Credentials = this.credentials;
			//request.CookieContainer = this.host.cookies;
			request.PreAuthenticate = true;
			request.Headers.Add( Domain.DomainIDHeader, host.DomainID );

			request.Headers.Add(
				Domain.BasicEncodingHeader,
#if MONO
				// bht: Fix for Bug 73324 - Client fails to authenticate if LDAP
				// username has an international character in it.
				//
				// Mono converts the username and password to a byte array
				// without paying attention to the encoding.  In NLD, the
				// default encoding is UTF-8.  Without this fix, we ended up
				// sending the username and password in 1252 but the server
				// was attempting to decode it as UTF-8.  This fix forces the
				// username and password to be sent with Windows-1252 encoding
				// which properly gets decoded on the server.
				System.Text.Encoding.GetEncoding(1252).WebName );
#else
				System.Text.Encoding.Default.WebName );
#endif
			
			request.Method = "POST";
			request.ContentLength = 0;

			try
			{
				request.GetRequestStream().Close();
				response = request.GetResponse() as HttpWebResponse;
				if ( response != null )
				{
					//request.CookieContainer.Add( response.Cookies );
					//host.cookies.Add( response.Cookies );
					string grace = response.GetResponseHeader( GraceTotalHeader );
					if ( grace != null && grace != "" )
					{
						int totalGraceLogins = 0;
						int remainingGraceLogins = 0;

						status = LoginStatus.SuccessInGrace;
						totalGraceLogins = Convert.ToInt32( grace );

						grace = 
							response.GetResponseHeader( GraceRemainingHeader );
						if ( grace != null && grace != "" )
						{
							remainingGraceLogins = Convert.ToInt32( grace );
						}

						throw new LoginGraceException( "Login succeeded but is in a grace condition", totalGraceLogins, remainingGraceLogins );
					}
					else
					{
						status = LoginStatus.Success;
						this.authenticated = true;
					}
				}
			}
			catch( WebException webEx )
			{
				if ( webEx.Status == WebExceptionStatus.TrustFailure )
				{
					// The Certificate is invalid.
					status = LoginStatus.InvalidCertificate;
					throw new LoginException( LoginStatus.InvalidCertificate );
				}
				else
				{
					response = webEx.Response as HttpWebResponse;
					if (response != null)
					{
						//request.CookieContainer.Add( response.Cookies );
						
						// Look for our special header to give us more
						// information why the authentication failed
						string remoteStatus = 
							response.GetResponseHeader( Domain.SimiasErrorHeader );

						if ( remoteStatus != null && remoteStatus != "" )
						{
							if ( remoteStatus == LoginStatus.AccountDisabled.ToString() )
							{
								throw new LoginException( LoginStatus.AccountDisabled );
							}
							else if ( remoteStatus == LoginStatus.AccountLockout.ToString() )
							{
								throw new LoginException( LoginStatus.AccountLockout );
							}
							else if ( remoteStatus == LoginStatus.AmbiguousUser.ToString() )
							{
								throw new LoginException( LoginStatus.AmbiguousUser );
							}
							else if ( remoteStatus == LoginStatus.InternalException.ToString() )
							{
								throw new LoginException( LoginStatus.InternalException );
							}
							else if ( remoteStatus == LoginStatus.InvalidCertificate.ToString() )
							{
								throw new LoginException( LoginStatus.InvalidCertificate );
							}
							else if ( remoteStatus == LoginStatus.InvalidCredentials.ToString() )
							{
								throw new LoginException( LoginStatus.InvalidCredentials );
							}
							else if ( remoteStatus == LoginStatus.InvalidPassword.ToString() )
							{
								throw new LoginException( LoginStatus.InvalidPassword );
							}
							else if ( remoteStatus == LoginStatus.LoginDisabled.ToString() )
							{
								throw new LoginException( LoginStatus.LoginDisabled );
							}
							else if ( remoteStatus == LoginStatus.MethodNotSupported.ToString() )
							{
								throw new LoginException( LoginStatus.MethodNotSupported );
							}
							else if ( remoteStatus == LoginStatus.UnknownDomain.ToString() )
							{
								throw new LoginException( LoginStatus.UnknownDomain );
							}
							else if ( remoteStatus == LoginStatus.UnknownUser.ToString() )
							{
								throw new LoginException( LoginStatus.UnknownUser );
							}
							else
							{
								throw new LoginException( LoginStatus.Unknown );
							}
						}

							/*
							else if ( iFolderError == StatusCodes.InvalidCredentials.ToString() )
							{
								// This could have failed because of iChain.
								// Check for a via header.
								string viaHeader = response.Headers.Get("via");
								if (viaHeader != null && !calledRecursive)
								{
									// Try again.
									return Login(host, domainID, networkCredential, true);
								}
								status.statusCode = SCodes.InvalidCredentials;
							}
							else if ( iFolderError == StatusCodes.SimiasLoginDisabled.ToString() )
							{
								status.statusCode = SCodes.SimiasLoginDisabled;
							}
							*/
						else if ( response.StatusCode == HttpStatusCode.Unauthorized )
						{
							// This call is a free call on the server.
							// If we get a 401 we must have iChain between us.
							// The user was invalid.
							status = LoginStatus.UnknownUser;
							throw new LoginException( LoginStatus.UnknownUser );
						}
					}
					else
					{
						throw new LoginException( LoginStatus.Unknown );
					}
				}
			}
			catch( Exception ex )
			{
				//log.Debug( ex.Message );
				//log.Debug( ex.StackTrace );
			
				throw ex;
			}

			return status;
		}

		/// <summary>
		/// Private method to persist domain and user
		/// information into the private store.
		/// </summary>
		/// <param name="defaultDomain">True if this domain is default.</param>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void Persist()
		{
			XmlDocument doc = DomainToXmlDocument();

			// Build a path to the store/domain
			string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( fullPath == null || fullPath.Length == 0 )
			{
				fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			fullPath += Path.DirectorySeparatorChar.ToString() + Configuration.StoreComponent;
			if ( Directory.Exists( fullPath) == false )
			{
				Directory.CreateDirectory( fullPath );
			}

			fullPath += Path.DirectorySeparatorChar.ToString() + Configuration.DomainComponent;
			if ( Directory.Exists( fullPath) == false )
			{
				Directory.CreateDirectory( fullPath );
			}

			fullPath += Path.DirectorySeparatorChar.ToString() + this.ID;

			/*
			string fullPath = 
				String.Format( 
					"{0}{1}{2}{3}{4}{5}{6}", 
					Environment.SpecialFolder.LocalApplicationData, 
					Path.DirectorySeparatorChar.ToString(), 
					Configuration.StoreComponent,
					Path.DirectorySeparatorChar.ToString(), 
					Configuration.DomainComponent,
					Path.DirectorySeparatorChar.ToString(), 
					this.ID );
			*/		

			doc.Save( fullPath );
			return;
		}

		/// <summary>
		private XmlDocument DomainToXmlDocument()
		{
			XmlDocument doc = new XmlDocument();
			XmlNode n = doc.CreateXmlDeclaration( "1.0", "utf-8", "" );
			doc.AppendChild( n );

			XmlNode root = doc.CreateElement( "domain" );
			XmlAttribute attr = doc.CreateAttribute( "id" );
			attr.Value = this.id;
			root.Attributes.SetNamedItem( attr );

			attr = doc.CreateAttribute( "name" );
			attr.Value = this.name;
			root.Attributes.SetNamedItem( attr );

			if ( this.Description != null )
			{
				attr = doc.CreateAttribute( "description" );
				attr.Value = this.description;
				root.Attributes.SetNamedItem( attr );
			}

			XmlNode node;
			node = doc.CreateElement( "member" );
			attr = doc.CreateAttribute( "id" );
			attr.Value = this.memberID;
			node.Attributes.SetNamedItem( attr );
			attr = doc.CreateAttribute( "name" );
			attr.Value = this.credentials.UserName;
			node.Attributes.SetNamedItem( attr );

			if ( this.savepassword == true )
			{
				attr = doc.CreateAttribute( "pwd" );
				attr.Value = this.credentials.Password;
				node.Attributes.SetNamedItem( attr );
			}
			root.AppendChild( node );

			node = doc.CreateElement( "host" );
			node.InnerText = this.host.Address;
			root.AppendChild( node );

			if ( this.defaultDomain == true )
			{
				node = doc.CreateElement( "default" );
				node.InnerText = this.Default.ToString();
				root.AppendChild( node );
			}
			
			doc.AppendChild( root );
			return doc;
		}

		/// <summary>
		/// Private method to remove domain and user
		/// information from the private store.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private string GetDomainPathFromID( string domainID )
		{
			string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
			if ( fullPath == null || fullPath.Length == 0 )
			{
				fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
			}

			fullPath += 
				Path.DirectorySeparatorChar.ToString() + 
				Configuration.StoreComponent +
				Path.DirectorySeparatorChar.ToString() + 
				Configuration.DomainComponent + 
				Path.DirectorySeparatorChar.ToString() +
				domainID;
			if ( File.Exists( fullPath ) == true )
			{
				return fullPath;
			}

			return null;
		}

		/// <summary>
		/// Private method to remove domain and user
		/// information from the private store.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void Remove()
		{
			File.Delete( GetDomainPathFromID( this.ID ) );
			return;
		}
		
		/// <summary>
		/// Private method to provision a user into
		/// the specified domain.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private void ProvisionUser()
		{
			// Create the domain service web client object.
			DomainService domainService = SetupDomainService( true );
				
			// provision user
			ProvisionInfo provisionInfo = 
				domainService.ProvisionUser( credentials.UserName, credentials.Password );
			if (provisionInfo == null)
			{
				throw new UnknownUserException( this.credentials.UserName );
			}
			
			this.memberID = provisionInfo.UserID;
			return;
		}

		/// <summary>
		/// Private method to setup the DomainService
		/// proxy object.
		/// </summary>
		/// <returns>
		/// Throws an exception on failure
		/// </returns>
		private DomainService SetupDomainService( bool requiresCredentials )
		{
			// Create the domain service web client object.
			DomainService domainService = new DomainService();
			domainService.CookieContainer = this.host.cookies;
			domainService.Url = this.host.Address + Domain.DomainServicePath;
			//domainService.Proxy = ProxyState.GetProxyState( domainServiceUrl );
			
			if ( requiresCredentials == true )
			{
				domainService.Credentials = this.credentials;
				domainService.PreAuthenticate = true;
			}
		
			return domainService;
		}
		#endregion
		
		#region Public Methods
		/// <summary>
		/// Static method to add an iFolder domain to the locally
		/// configured domain set.  Adding also ensures the user
		/// is provisioned on the server.  The last step logs
		/// the user into the system.
		/// </summary>
		/// <param name="host">Host the caller is logging into</param>
		/// <param name="username">Friendly name of the user.</param>
		/// <param name="password">Password of the user.</param>
		/// <param name="defaultDomain">True sets this domain as default.</param>
		/// <param name="savePassword">True saves the password in the store.</param>
		/// <returns>Domain object</returns>
		static public Domain Add( Host host, string username, string password, bool defaultDomain, bool savePassword )
		{
			// First ping the host
			if ( host.Ping() == true )
			{
				Domain domain = new Domain( host, username, password );
				domain.savepassword = savePassword;
				domain.defaultDomain = defaultDomain;
				domain.ProvisionUser();
				domain.RemoteLogin();
				domain.GetDomainInformation();
				
				domain.Persist();
				return domain;
			}

			// Generate a host not available exception
			throw new LoginException( LoginStatus.UnknownHost );
		}

		/// <summary>
		/// Public method to login to a remote domain.
		/// </summary>
		/// <param name="password">Password of the configured user.</param>
		/// <returns>Failure - throws an exception</returns>
		public void Login( string password )
		{
			this.credentials.Password = password;

			if ( host.Ping() == true )
			{
				this.RemoteLogin();

				// bugbug if domain information has changed
				// persist the changes
				//domain.GetDomainInformation();

				return;
			}

			throw new LoginException( LoginStatus.UnknownHost );
		}
		
		/// <summary>
		/// Static method to get the locally
		/// configured default domain
		/// </summary>
		/// <returns>Domain object</returns>
		static public Domain GetDefaultDomain()
		{
			Domain defaultDomain = null;
			try
			{
				// Build a path to the store/domain
				string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
				if ( fullPath == null || fullPath.Length == 0 )
				{
					fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				}

				fullPath += 
					Path.DirectorySeparatorChar.ToString() + 
					Configuration.StoreComponent +
					Path.DirectorySeparatorChar.ToString() + 
					Configuration.DomainComponent;

				string[] files = Directory.GetFiles( fullPath );
				foreach( string domainConfig in files )
				{
					Domain domain = new Domain();
					domain.ConfigFileToDomain( domainConfig );
					if ( domain.Default == true )
					{
						defaultDomain = domain;
						break;
					}
				}
			}
			catch( System.Exception gd )
			{
				// log
			}

			return defaultDomain;
		}

		/// <summary>
		/// Static method to retrieve all the added/configured
		/// domains on this system.
		/// </summary>
		/// <param name="host">Host the caller is logging into</param>
		/// <param name="username">Friendly name of the user.</param>
		/// <param name="password">Password of the user.</param>
		/// <returns>Domain object</returns>
		static public Domain[] GetDomains()
		{
			ArrayList domains = new ArrayList();
			try
			{
				// Build a path to the store/domain
				string fullPath = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
				if ( fullPath == null || fullPath.Length == 0 )
				{
					fullPath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
				}

				fullPath += 
					Path.DirectorySeparatorChar.ToString() + 
					Configuration.StoreComponent +
					Path.DirectorySeparatorChar.ToString() + 
					Configuration.DomainComponent;

				// GetFiles returns file in full path format
				string[] files = Directory.GetFiles( fullPath );
				foreach( string domainConfig in files )
				{
					Domain domain = new Domain();
					domain.ConfigFileToDomain( domainConfig );
					domains.Add( domain );
				}
			}
			catch( System.Exception gd )
			{
				// log
			}

			return domains.ToArray( typeof( Domain ) ) as Domain[];
		}

		/// <summary>
		/// Static method to newup a Domain by ID
		/// </summary>
		/// <param name="domainID">Unique ID of the domain to newup</param>
		/// <returns>Domain object</returns>
		static public Domain GetDomainByID( string domainID )
		{
			return new Domain( domainID );
		}

		/// <summary>
		/// Static method to remove an iFolder domain from the
		/// local configuration store.
		/// </summary>
		/// <param name="host">Host the caller is logging into</param>
		/// <param name="username">Friendly name of the user.</param>
		/// <param name="password">Password of the user.</param>
		/// <returns>Domain object</returns>
		static public void Remove( string domainID )
		{
			Domain domain = new Domain( domainID );
			
			// bugbug logout first
			domain.Remove();
		}
		#endregion
	}
}	

