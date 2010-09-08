/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Mahabaleshwar Asundi <amahabaleshwar@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System;
using System.Collections;
using System.Reflection;
using System.Net;
using System.Web;
using System.Text;
using System.Threading;

using Simias;
using Simias.Authentication;
using Simias.Client;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;

using SCodes = Simias.Authentication.StatusCodes;

namespace Simias.Identity
{
	/// <summary>
	/// Class used to keep track of outstanding searches.
	/// </summary>
	internal class SearchState : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Table used to keep track of outstanding search entries.
		/// </summary>
		static private Hashtable searchTable = new Hashtable();

		/// <summary>
		/// Indicates whether the object has been disposed.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Handle used to store and recall this context object.
		/// </summary>
		private string contextHandle = Guid.NewGuid().ToString();

		/// <summary>
		/// Identifier for the domain that is being searched.
		/// </summary>
		private string domainID;

		/// <summary>
		/// Object used to iteratively return the members from the domain.
		/// </summary>
		private ICSEnumerator enumerator;

		/// <summary>
		/// Total number of records contained in the search.
		/// </summary>
		private int totalRecords;

		/// <summary>
		/// The cursor for the caller.
		/// </summary>
		private int currentRecord = 0;

		/// <summary>
		/// The last count of records returned.
		/// </summary>
		private int previousCount = 0;
		#endregion

		#region Properties
		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Gets the context handle for this object.
		/// </summary>
		public string ContextHandle
		{
			get { return contextHandle; }
		}

		/// <summary>
		/// Gets or sets the current record.
		/// </summary>
		public int CurrentRecord
		{
			get { return currentRecord; }
			set { currentRecord = value; }
		}

		/// <summary>
		/// Gets the domain ID for the domain that is being searched.
		/// </summary>
		public string DomainID
		{
			get { return domainID; }
		}

		/// <summary>
		/// Gets or sets the last record count.
		/// </summary>
		public int LastCount
		{
			get { return previousCount; }
			set { previousCount = value; }
		}

		/// <summary>
		/// Gets the search iterator.
		/// </summary>
		public ICSEnumerator Enumerator
		{
			get { return enumerator; }
		}

		/// <summary>
		/// Gets the total number of records contained by this search.
		/// </summary>
		public int TotalRecords
		{
			get { return totalRecords; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes an instance of an object.
		/// </summary>
		/// <param name="domainID">Identifier for the domain that is being searched.</param>
		/// <param name="enumerator">Search iterator.</param>
		/// <param name="totalRecords">The total number of records contained in the search.</param>
		public SearchState( string domainID, ICSEnumerator enumerator, int totalRecords )
		{
			this.domainID = domainID;
			this.enumerator = enumerator;
			this.totalRecords = totalRecords;

			lock ( searchTable )
			{
				searchTable.Add( contextHandle, this );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Removes this SearchState object from the search table.
		/// </summary>
		private void RemoveSearchState()
		{
			lock ( searchTable )
			{
				// Remove the search context from the table and dispose it.
				searchTable.Remove( contextHandle );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Returns a search context object that contains the state information for an outstanding search.
		/// </summary>
		/// <param name="contextHandle">Context handle that refers to a specific search context object.</param>
		/// <returns>A SearchState object if a valid one exists, otherwise a null is returned.</returns>
		static public SearchState GetSearchState( string contextHandle )
		{
			lock ( searchTable )
			{
				return searchTable[ contextHandle ] as SearchState;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Allows for quick release of managed and unmanaged resources.
		/// Called by applications.
		/// </summary>
		public void Dispose()
		{
			RemoveSearchState();
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// Dispose( bool disposing ) executes in two distinct scenarios.
		/// If disposing equals true, the method has been called directly
		/// or indirectly by a user's code. Managed and unmanaged resources
		/// can be disposed.
		/// If disposing equals false, the method has been called by the 
		/// runtime from inside the finalizer and you should not reference 
		/// other objects. Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					enumerator.Dispose();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~SearchState()      
		{
			Dispose( false );
		}
		#endregion
	}

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
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

		private string domainID;
		private string username;
		private string password;
		private string authType;

		private readonly char[] colonDelimeter = {':'};
		private readonly char[] backDelimeter = {'\\'};
		#endregion

		#region Properties
        /// <summary>
        /// gets/sets authintaction type
        /// </summary>
		public string AuthType
		{
			get { return this.authType; }
			set { this.authType = value; }
		}

        /// <summary>
        /// gets/sets domain id
        /// </summary>
		public string DomainID
		{
			get { return this.domainID; }
			set { this.domainID = value; }
		}

        /// <summary>
        /// gets/sets the password
        /// </summary>
		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

        /// <summary>
        /// gets/sets username
        /// </summary>
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

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
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
					this.username = DecodeCreds(credentials[ 0 ], encodingName);
                                        this.password = DecodeCreds(credentials[ 1 ], encodingName);
					this.authType = "basic";
					returnStatus = true;
				}

				credentials = null;
			}

			return returnStatus;
		}

		/// <summary>
                /// Returns the decoded value of user creds if its encoded. Else will return the same [ Old Client ] .
                /// </summary>
                /// <returns>String - User Creds in String</returns>
                private string DecodeCreds(string creds, string encodingName)
                {
                        try
                        {
                                byte[] encodedCredsByteArray = Convert.FromBase64String(creds);
                                Encoding encoder = System.Text.Encoding.GetEncoding( encodingName );
                                return encoder.GetString(encodedCredsByteArray, 0, encodedCredsByteArray.Length);
                        }
                        catch(Exception ex)
                        {
				// Exception occurs when we try to decode string which is not encoded
                                // TODO : Find the right exception and catch it.
                                return creds;
                        }

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
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( MethodBase.GetCurrentMethod().DeclaringType );

                /// <summary>
                /// Sync wait method
                /// </summary>
		private static AutoResetEvent syncEvent = new AutoResetEvent( false );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Enterprise Authentication Provider";
		static private string providerDescription = "Authentication Provider for Simias Enterprise Server";

		/// <summary>
		/// Store object.
		/// </summary>
		private Store store = null;

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
			defaultBasicEncodingName = Store.Config.Get( Storage.Domain.SectionName, Storage.Domain.Encoding );
			if ( defaultBasicEncodingName == null )
			{
				defaultBasicEncodingName = "utf-8";
			}
			
			store = Store.GetStore();
		}

		#endregion

		#region Private Methods
		/// <summary>
		/// Authenticates the user by name and password
		/// </summary>
		/// <param name="DomainID">The identifier for the domain.</param>
		/// <param name="User">The user to authenticate.</param>
		/// <param name="Password">The user's password.</param>
		///<returns>
		/// Returns an authentication status object
		/// </returns>
		private
		Simias.Authentication.Status
		AuthenticateByName( string DomainID, string User, string Password )
		{
			Simias.Authentication.Status status = new Simias.Authentication.Status( SCodes.Unknown );
			status.DomainID = DomainID;
			status.UserName = User;

			try
			{
				// Verify the credentials passed in.
				if ( Simias.Identity.User.VerifyPassword( User, Password, status ) )
				{
					log.Info( "Authenticated User: {0}:{1} {2}", status.UserID, status.UserName, status.statusCode );
				}
				else
				{
					log.Info( "{0} : {1}", status.statusCode, status.UserName );
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
		/// <param name="Domain">Domain to authenticate to.</param>
		/// <param name="HttpCtx">HTTP-specific request information. This is passed as a parameter so that a domain 
		/// provider may modify the HTTP request by adding special headers as necessary.
		/// 
		/// NOTE: The domain provider must NOT end the HTTP request.
		/// </param>
		/// <returns>The status from the authentication.</returns>
		public
		Simias.Authentication.Status
		Authenticate( Simias.Storage.Domain Domain, HttpContext HttpCtx )
		{
			Simias.Authentication.Status authStatus;

			log.Debug( "Authenticate called" );

			try
			{
				// Check for an authorization header.
				string[] encodedCredentials = HttpCtx.Request.Headers.GetValues( "Authorization" );
				if ( ( encodedCredentials != null ) && ( encodedCredentials[0] != null ) )
				{
					// Get the basic encoding type from the http header.
					string[] encodingName = HttpCtx.Request.Headers.GetValues( "Basic-Encoding" );
					if ( ( encodingName == null ) || ( encodingName[0] == null ) )
					{
						// Use the specified default encoding.
						encodingName = new string[] { defaultBasicEncodingName };
					}
					// Get the credentials from the auth header.
					SimiasCredentials creds = new SimiasCredentials();
					if( creds.AuthorizationHeaderToCredentials( encodedCredentials[0], encodingName[0] ) )
					{
						// Valid credentials?
						if ( ( creds.Username != null ) && ( creds.Password != null ) )
						{
							// Only support basic.
							if ( creds.AuthType == "basic" )
							{
								Member member = Domain.GetMemberByName( creds.Username );
								if(member == null)
									member = Domain.GetMemberByDN( creds.Username );
                                                                if(member != null )
                                                                {
									if( Domain.IsLoginDisabled( member.UserID ) != true )
									{
										try
										{
											// Authenticate the user.
											authStatus = AuthenticateByName( Domain.ID, creds.Username, creds.Password );
											HostNode hNode = HostNode.GetLocalHost();
                                                                                        if(hNode.IsMasterHost != true)
                                                                                        for(int i =0; i < 10 ; i ++)
                                                                                        {
												log.Debug( "System Sync Status : " + Domain.SystemSyncStatus.ToString() );
                                                                                                if((Domain.SystemSyncStatus &
                                                                                                 (ulong) CollectionSyncClient.StateMap.CatalogSyncOnce) ==
                                                                                                 (ulong) CollectionSyncClient.StateMap.CatalogSyncOnce ||
													(CollectionSyncClient.ServerSyncStatus &
													CollectionSyncClient.StateMap.CatalogSyncOnce) ==
													CollectionSyncClient.StateMap.CatalogSyncOnce )
                                                                                                        break;
                                                                                                else
                                                                                                {
                                                                                                        syncEvent.WaitOne( 5000, false );
                                                                                                }

												if(i == 9)
                                                                                                       authStatus = new Simias.Authentication.Status( SCodes.InvalidCredentials );
                                                                                        }
											
											HostNode mNode = member.HomeServer;
                                                                                        log.Debug("id.Auth : localhost userid  is :"+hNode.UserID);
											Http.UserMoved = 0;
											if(mNode != null)
											{
												log.Debug("id.Auth : member's home server userid is :"+mNode.UserID);
                                                                                        	if (hNode.UserID != mNode.UserID)
                                                                                        	{
                                                                                               		log.Debug("id.Aith : sending useralreadymoved status back to client");
													Http.UserMoved = 1;
                                                                                        	}
											}
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
										log.Debug( "Login is disabled for user " + creds.Username );
										authStatus = new Simias.Authentication.Status( SCodes.SimiasLoginDisabled );
									}
								}
                                                                else
                                                                {
									log.Debug( creds.Username + " is not member of simias" );
                                                                        authStatus = new Simias.Authentication.Status( SCodes.InvalidCredentials );
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
			// See if there is a valid search context.
			SearchState searchState = SearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				searchState.Dispose();
			}
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
			return FindFirstDomainMembers( domainID, PropertyTags.FullName, String.Empty, SearchOp.Exists, count, out searchContext, out memberList, out total );
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
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;
			total = 0;

			// Start the search for the specific members of the domain.
			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				ICSList list = domain.Search( attributeName, searchString, operation );
				SearchState searchState = new SearchState( domainID, list.GetEnumerator() as ICSEnumerator, list.Count );
				searchContext = searchState.ContextHandle;
				total = list.Count;
				moreEntries = FindNextDomainMembers( ref searchContext, count, out memberList );
			}

			return moreEntries;
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
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			// See if there is a valid search context.
			SearchState searchState = SearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// See if entries are to be returned.
				if ( count > 0 )
				{
					// Get the domain being searched.
					Domain domain = store.GetDomain( searchState.DomainID );
					if ( domain != null )
					{
						// Allocate a list to hold the member objects.
						ArrayList tempList = new ArrayList( count );
						ICSEnumerator enumerator = searchState.Enumerator;
						while( ( count > 0 ) && enumerator.MoveNext() )
						{
							// The enumeration returns ShallowNode objects.
							ShallowNode sn = enumerator.Current as ShallowNode;
							if ( sn.Type == NodeTypes.MemberType )
							{
								tempList.Add( new Member( domain, sn ) );
								--count;
							}
						}

						if ( tempList.Count > 0 )
						{
							memberList = tempList.ToArray( typeof ( Member ) ) as Member[];
							searchState.CurrentRecord += memberList.Length;
							searchState.LastCount = memberList.Length;
							moreEntries = ( count == 0 ) ? true : false;
						}
					}
				}
				else
				{
					if ( searchState.CurrentRecord < searchState.TotalRecords )
					{
						moreEntries = true;
					}
				}
			}

			return moreEntries;
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
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			// See if there is a valid search context.
			SearchState searchState = SearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// Backup the current cursor, but don't go passed the first record.
				if ( searchState.CurrentRecord > 0 )
				{
					bool invalidIndex = false;
					int cursorIndex = ( searchState.CurrentRecord - ( searchState.LastCount + count ) );
					if ( cursorIndex < 0 )
					{
						invalidIndex = true;
						count = searchState.CurrentRecord - searchState.LastCount;
						cursorIndex = 0;
					}

					// Set the new index for the cursor.
					if ( searchState.Enumerator.SetCursor( Simias.Storage.Provider.IndexOrigin.SET, cursorIndex ) )
					{
						// Reset the current record.
						searchState.CurrentRecord = cursorIndex;

						// Complete the search.
						FindNextDomainMembers( ref searchContext, count, out memberList );

						if ( ( invalidIndex == false ) && ( memberList != null ) )
						{
							moreEntries = true;
						}
					}
				}
			}

			return moreEntries;
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
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			// See if there is a valid search context.
			SearchState searchState = SearchState.GetSearchState( searchContext );
			if ( searchState != null )
			{
				// Make sure that the specified offset is valid.
				if ( ( offset >= 0 ) && ( offset <= searchState.TotalRecords ) )
				{
					// Set the cursor to the specified offset.
					if ( searchState.Enumerator.SetCursor( Simias.Storage.Provider.IndexOrigin.SET, offset ) )
					{
						// Reset the current record.
						searchState.CurrentRecord = offset;

						// Complete the search.
						moreEntries = FindNextDomainMembers( ref searchContext, count, out memberList );
					}
				}
			}

			return moreEntries;
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
			
			Collection collection = store.GetSingleCollectionByType( "Enterprise" );
			if ( collection.ID == domainID )
			{
					log.Debug( "  returning true" );
					return true;
			}
			
			log.Debug( "  returning false" );
			return false;
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
			return ResolveLocation(domainID, domainID);
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
			// This is only called by a server return the private address.
			Uri hostAddress = null;
			Domain domain = store.GetDomain( domainID );
			if ( domain != null )
			{
				// Get the address from the host.
				HostNode host = domain.Host;
				if (host != null)
				{
					hostAddress = new Uri(host.PublicUrl);
				}
				else
				{
					Property p = domain.Properties.GetSingleProperty( PropertyTags.HostAddress );
					if ( p != null )
					{
						hostAddress = p.Value as Uri;
					}
				}
			}

			return hostAddress;
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
			return ResolvePOBoxLocation( domainID, userID );
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
			Uri hostAddress = null;
			
			Domain domain = store.GetDomain(domainID);
			Member member = domain.GetMemberByID(userID);
			if ( member != null )
			{
				// Get the address from the host.
				HostNode host = member.HomeServer;
				if (host != null)
				{
					hostAddress = new Uri(host.PrivateUrl);
				}
				else
				{
					// This is a single server system.
					hostAddress = ResolveLocation(domainID);
				}
			}
			return hostAddress;
		}

		/// <summary>
		/// Returns the network address of the host
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="hostID">The host to resolve.</param>
		/// <returns>A Uri object that contains the network location.</returns>
		public Uri ResolveHostAddress(string domainID, string hostID)
        {
			HostNode host = HostNode.GetHostByID(domainID, hostID);
			if (host != null)
			{
				return new Uri(host.PrivateUrl);
			}
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
