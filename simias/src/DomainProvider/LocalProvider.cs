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
 *  Author: Brady Anderson <banderso@novell.com>
 *			Mike Lasky <mlasky@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using Simias;
using Simias.Authentication;
using Simias.Service;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;

// shorty
using SCodes = Simias.Authentication.StatusCodes;

namespace Simias
{
	/// <summary>
	/// Implementation of the DomainProvider Interface for the local domain.
	/// </summary>
	public class LocalProvider : IDomainProvider, IThreadService
	{
		#region Class Members

		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( LocalProvider ) );

		/// <summary>
		/// String used to identify domain provider.
		/// </summary>
		static private string providerName = "Local Domain Provider";
		static private string providerDescription = "Domain Provider for Simias Local Domain";

		/// <summary>
		/// Session tag used to identify session state information.
		/// </summary>
		static private string localSessionTag = "local";

		/// <summary>
		/// Store object.
		/// </summary>
		static private Store store = Store.GetStore();

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
		public Authentication.Status Authenticate( Domain domain, HttpContext httpContext )
		{
			// Assume failure
			Simias.Authentication.Status status = new Simias.Authentication.Status( SCodes.InvalidCredentials );

			// Requires session support
			if ( httpContext.Session != null )
			{
				// State should be 1
				string memberID = httpContext.Request.Headers[ "local-member" ];
				if ( ( memberID == null ) || ( memberID == String.Empty ) )
				{
					return status;
				}

				// Get the member that was specified as belonging to this domain.
				Member member = domain.GetMemberByID( memberID );
				if ( member == null )
				{
					return status;
				}

				status.UserName = member.Name;
				status.UserID = member.UserID;

				LocalSession localSession = httpContext.Session[ localSessionTag ] as LocalSession;
				if ( localSession == null )
				{
					localSession = new LocalSession();
					localSession.MemberID = member.UserID;
					localSession.State = 1;

					// Fixme
					localSession.OneTimePassword = DateTime.UtcNow.Ticks.ToString();
					RSACryptoServiceProvider publicKey = member.PublicKey;
					if ( publicKey != null )
					{
						byte[] oneTime = new UTF8Encoding().GetBytes( localSession.OneTimePassword );
						byte[] encryptedText = publicKey.Encrypt( oneTime, false );

						// Set the encrypted one time password in the response
						httpContext.Response.AddHeader( "local-secret", Convert.ToBase64String( encryptedText ) );
						httpContext.Session[ localSessionTag ] = localSession;
					}
				}
				else if ( status.UserID == localSession.MemberID )
				{
					// State should be 1
					string encodedSecret = httpContext.Request.Headers[ "local-secret" ];
					if ( ( encodedSecret != null ) && ( encodedSecret != String.Empty ) )
					{
						UTF8Encoding utf8 = new UTF8Encoding();
						string decodedString = utf8.GetString( Convert.FromBase64String( encodedSecret ) );

						// Check it...
						if ( decodedString.Equals( localSession.OneTimePassword ) )
						{
							status.statusCode = SCodes.Success;
							localSession.State = 2;
						}
					}
					else
					{
						// Fixme
						localSession.OneTimePassword = DateTime.UtcNow.Ticks.ToString();
						localSession.State = 1;

						RSACryptoServiceProvider publicKey = member.PublicKey;
						if ( publicKey != null )
						{
							try
							{
								byte[] oneTime = new UTF8Encoding().GetBytes( localSession.OneTimePassword );
								byte[] encryptedText = publicKey.Encrypt( oneTime, false );

								// Set the encrypted one time password in the response
								httpContext.Response.AddHeader( "local-secret", Convert.ToBase64String( encryptedText ) );
							}
							catch( Exception encr )
							{
								log.Debug( encr.Message );
								log.Debug( encr.StackTrace );
							}
						}
					}
				}
			}

			return status;
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
			return ( domainID == store.LocalDomain ) ? true : false;
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

		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">Configuration file object that indicates which Collection Store to use.</param>
		public void Start( Configuration conf )
		{
			// Register with the domain provider service.
			DomainProvider.RegisterProvider( this );
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			// Unregister with the domain provider service.
			DomainProvider.Unregister( this );
		}

		/// <summary>
		/// Pauses a service's execution.
		/// </summary>
		public void Pause()
		{
		}

		/// <summary>
		/// Resumes a paused service. 
		/// </summary>
		public void Resume()
		{
		}

		/// <summary>
		/// Custom.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom( int message, string data )
		{
		}

		#endregion

		#region LocalSession
		/// <summary>
		/// Session state object used to hold state to authenticate local requests.
		/// </summary>
		private class LocalSession
		{
			#region Class Members

			/// <summary>
			/// Session state variables used to authenticate local requests.
			/// </summary>
			public string MemberID;
			public string OneTimePassword;
			public int State;

			#endregion
		}
		#endregion
	}
}
