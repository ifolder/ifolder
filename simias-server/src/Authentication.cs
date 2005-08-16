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
 *  Author: Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Reflection;
using System.Web;

using Simias;
using Simias.Authentication;
using Simias.Service;
using Simias.Storage;

using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.Server
{
	/// <summary>
	/// Internal class for managing and authenticating enterprise credentials
	/// 
	/// Note: Today we only support 'basic'
	/// </summary>
	internal class SimiasCredentials
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		private string domainID;
		private string username;
		private string password;
		private string authType;

		private readonly char[] colonDelimeter = {':'};
		private readonly char[] backDelimeter = {'\\'};
		#endregion

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

		#region Public Methods
		/// <summary>
		/// Gets the credentials from an encoded authorization header.
		/// </summary>
		/// <param name="authHeader"></param>
		/// <param name="encodingName">The name of the code paged used to encode the credentials.</param>
		/// <returns></returns>
		public bool AuthorizationHeaderToCredentials( string authHeader, string encodingName )
		{
			bool returnStatus = false;

			// Make sure we are dealing with "Basic" credentials
			if ( authHeader.StartsWith( "Basic " ) )
			{
				// The authHeader after the basic signature is encoded
				authHeader = authHeader.Remove( 0, 6 );
				byte[] credential = System.Convert.FromBase64String( authHeader );

				System.Text.Encoding encoder = null;
				try
				{
					// Use the specified codepage to decode the basic credentials.
					encoder = System.Text.Encoding.GetEncoding( encodingName );
				}
				catch ( Exception ex )
				{
					// The specified code page is not supported on this machine. Use
					// the default codepage.
					log.Info( "Code page: {0} is not supported on this machine.", encodingName );
					encoder = System.Text.Encoding.Default;
					log.Debug( ex, "Cannot load codepage: {0}. Using code page: {1}.", encodingName, encoder.EncodingName );
				}

				string decodedCredential = encoder.GetString( credential, 0, credential.Length );
   
				// Clients that newed up a NetCredential object with a URL
				// come though on the authorization line like:
				// http://domain:port/simias10/service.asmx\username:password

				string[] credentials = decodedCredential.Split( this.backDelimeter );
				if ( credentials.Length == 1 )
				{
					credentials = decodedCredential.Split( this.colonDelimeter, 2 );
				}
				else if ( credentials.Length >= 2 )
				{
					credentials = credentials[ credentials.Length - 1 ].Split( colonDelimeter, 2 );
				}

				if ( credentials.Length == 2 )
				{
					this.username = credentials[ 0 ];
					this.password = credentials[ 1 ];
					this.authType = "basic";
					returnStatus = true;
				}

				credentials = null;
			}

			return returnStatus;
		}

		/// <summary>
		/// Returns whether the object has credentials.
		/// </summary>
		/// <returns></returns>
		public bool HasCredentials()
		{
			return ( ( this.username != null ) && ( this.password != null ) ) ? true : false;
		}
		#endregion
	}

	/// <summary>
	/// Implementation of the IDomainProvider Service for SimpleServer.
	/// </summary>
	public class Authentication : IDomainProvider
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "SimiasServer Authentication Provider";
		static private string providerDescription = "Authentication Provider for Simias Server";

		/// <summary>
		/// Store object.
		/// </summary>
		private Store store = Store.GetStore();

		/// <summary>
		/// The default encoding to use for decoding the basic credential set.
		/// </summary>
		private string defaultBasicEncodingName;
		#endregion

		#region Constructor

		/// <summary>
		/// Initializes an instance of this object.
		/// </summary>
		public Authentication()
		{
			Configuration config = Configuration.GetConfiguration();
			defaultBasicEncodingName = config.Get( Storage.Domain.SectionName, Storage.Domain.Encoding, "iso-8859-1" );
		}

		#endregion

		#region Private Methods
		/// <summary>
		/// Authenticates the user by name and password
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="user">The user to authenticate.</param>
		/// <param name="password">The user's password.</param>
		///<returns>
		/// Returns an authentication status object
		/// </returns>
		private Simias.Authentication.Status AuthenticateByName( string domainID, string user, string password )
		{
			Simias.Authentication.Status status = new Simias.Authentication.Status( SCodes.Unknown );

			try
			{
				// First verify the user exists in the SimpleServer domain
				Simias.Storage.Domain domain = store.GetDomain( domainID );
				if ( domain != null )
				{
					Simias.Storage.Member member = domain.GetMemberByName( user );
					if ( member != null )
					{
						Property pwd = member.Properties.GetSingleProperty( "SS:PWD" );
						if ( pwd != null )
						{
							if (password == ( string ) pwd.Value)
							{
								status.statusCode = SCodes.Success;
								status.UserID = member.UserID;
								status.UserName = member.Name;
							}
							else
							{
								status.statusCode = SCodes.InvalidCredentials;
							}
						}
					}
					else
					{
						log.Debug( "Unknown user: " + user + " attempted to authenticate" );
						status.statusCode = SCodes.UnknownUser;
					}
				}
				else
				{
					log.Debug( "Failed to instantiate the domain" );
				}
			}
			catch( Exception authEx )
			{
				log.Debug( authEx.Message );
				log.Debug( authEx.StackTrace );

				status.statusCode = SCodes.InternalException;
				status.ExceptionMessage = authEx.Message;
			}

			return status;
		}
		#endregion

		#region IDomainProvider Members

		/// <summary>
		/// Gets the name of the domain provider.
		/// </summary>
		public string Name
		{
			get { return providerName; }
		}

		/// <summary>
		/// Gets the description of the domain provider.
		/// </summary>
		public string Description
		{
			get { return providerDescription; }
		}

		/// <summary>
		/// Performs authentication to the specified domain.
		/// </summary>
		/// <param name="domain">Domain to authenticate to.</param>
		/// <param name="httpContext">HTTP-specific request information. This is passed as a parameter so that a domain 
		/// provider may modify the HTTP request by adding special headers as necessary.
		/// 
		/// NOTE: The domain provider must NOT end the HTTP request.
		/// </param>
		/// <returns>The status from the authentication.</returns>
		public Simias.Authentication.Status Authenticate( Simias.Storage.Domain domain, HttpContext httpContext )
		{
			Simias.Authentication.Status authStatus;

			log.Debug( "Authenticate called" );

			try
			{
				// Check for an authorization header.
				string[] encodedCredentials = httpContext.Request.Headers.GetValues( "Authorization" );
				if ( ( encodedCredentials != null ) && ( encodedCredentials[0] != null ) )
				{
					// Get the basic encoding type from the http header.
					string[] encodingName = httpContext.Request.Headers.GetValues( "Basic-Encoding" );
					if ( ( encodingName == null ) || ( encodingName[0] == null ) )
					{
						// Use the specified default encoding.
						encodingName = new string[] { defaultBasicEncodingName };
					}

					// Get the credentials from the auth header.
					SimiasCredentials creds = new SimiasCredentials();
					bool success = creds.AuthorizationHeaderToCredentials( encodedCredentials[0], encodingName[0] );
					if ( success )
					{
						// Valid credentials?
						if ( ( creds.Username != null ) && ( creds.Password != null ) )
						{
							// Only support basic.
							if ( creds.AuthType == "basic" )
							{
								try
								{
									// Authenticate the user.
									authStatus = AuthenticateByName( domain.ID, creds.Username, creds.Password );
								}
								catch( Exception e )
								{
									log.Error( e.Message );
									log.Error( e.StackTrace );
									authStatus = new Simias.Authentication.Status( SCodes.InternalException );
									authStatus.ExceptionMessage = e.Message;
								}
							}
							else
							{
								authStatus = new Simias.Authentication.Status( SCodes.MethodNotSupported );
							}
						}
						else
						{
							authStatus = new Simias.Authentication.Status( SCodes.InvalidCredentials );
						}
					}
					else
					{
						authStatus = new Simias.Authentication.Status( SCodes.InvalidCredentials );
					}
				}
				else
				{
					authStatus = new Simias.Authentication.Status( SCodes.InvalidCredentials );
				}
			}
			catch ( Exception e )
			{
				log.Error( e.Message );
				log.Error( e.StackTrace );
				authStatus = new Simias.Authentication.Status( SCodes.InternalException );
				authStatus.ExceptionMessage = e.Message;
			}

			return authStatus;
		}

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="domainID">The identifier for the domain from
		/// where the collection has been deleted.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being deleted.</param>
		/// <summary>
		public void DeleteLocation( string domainID, string collectionID )
		{
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		public void FindCloseDomainMembers( string searchContext )
		{
		}

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, int count, out string searchContext, out Member[] memberList, out int total )
		{
			searchContext = null;
			memberList = null;
			total = 0;
			return false;
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Name of attribute to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, int count, out string searchContext, out Member[] memberList, out int total )
		{
			searchContext = null;
			memberList = null;
			total = 0;
			return false;
		}

		/// <summary>
		/// Continues the search for domain members from the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindNextDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			memberList = null;
			return false;
		}

		/// <summary>
		/// Continues the search for domain members previous to the current record location.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindPreviousDomainMembers( ref string searchContext, int count, out Member[] memberList )
		{
			memberList = null;
			return false;
		}

		/// <summary>
		/// Continues the search for domain members from the specified record location.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="offset">Record offset to return members from.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindSeekDomainMembers( ref string searchContext, int offset, int count, out Member[] memberList )
		{
			memberList = null;
			return false;
		}

		/// <summary>
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		public bool OwnsDomain( string domainID )
		{
			log.Debug( "OwnsDomain called" );
			log.Debug( "  with domain: " + domainID );

			Simias.Server.Domain thisDomain = new Simias.Server.Domain( false );
			Simias.Storage.Domain ssDomain = thisDomain.GetSimiasServerDomain( false );
			if ( ssDomain != null )
			{
				log.Debug( "  this SimpleServer domain is: " + ssDomain.ID );
				if ( ssDomain.ID == domainID )
				{
					log.Debug( "  returning true" );
					return true;
				}
			}

			log.Debug( "Returning false" );
			return false;

			/*
			Simias.Storage.Domain domain = store.GetDomain( domainID );
			return ( ( domain != null ) && domain.IsType( domain, "Enterprise" ) ) ? true : false;
			*/
		}

		/// <summary>
		/// Informs the domain provider that the specified member object is about to be
		/// committed to the domain's member list. This allows an opportunity for the 
		/// domain provider to add any domain specific attributes to the member object.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <param name="member">Member object that is about to be committed to the domain's member list.</param>
		public void PreCommit( string domainID, Member member )
		{
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that the
		/// collection belongs to.</param>
		/// <param name="collectionID">Identifier of the collection to
		/// find the network location for.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string collectionID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network location of where to create a collection.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will own the 
		/// collection.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being created.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolveLocation( string domainID, string userID, string collectionID )
		{
			return null;
		}

		/// <summary>
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			return null;
		}

		/// <summary>
		/// Sets a new host address for the domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain for network address
		/// to be changed.</param>
		/// <param name="hostLocation">A Uri object containing the new network
		/// address for the domain.</param>
		public void SetHostLocation( string domainID, Uri hostLocation )
		{
			// Not needed by this implementation.
		}
		#endregion
	}
}
