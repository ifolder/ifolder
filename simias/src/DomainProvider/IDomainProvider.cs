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
using System.Web;

using Simias.Authentication;
using Simias.Storage;

namespace Simias
{
	/// <summary>
	/// Interface for a Domain Provider Service.
	/// </summary>
	public interface IDomainProvider
	{
		#region Properties
		/// <summary>
		/// Gets the name of the domain provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the description of the domain provider.
		/// </summary>
		string Description { get; }
		#endregion

		#region Public Methods
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
		Authentication.Status Authenticate( Domain domain, HttpContext httpContext );

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="domainID">The identifier for the domain from
		/// where the collection has been deleted.</param>
		/// <param name="collectionID">Identifier of the collection that
		/// is being deleted.</param>
		void DeleteLocation( string domainID, string collectionID );

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		void FindCloseDomainMembers( Object searchContext );

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		bool FindFirstDomainMembers( string domainID, out Object searchContext, out Member[] memberList, out int total, int count );

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, out Object searchContext, out Member[] memberList, out int total, int count );

		/// <summary>
		/// Continues the search for all domain members started by calling the FindFirstDomainMembers method.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		bool FindNextDomainMembers( ref Object searchContext, out Member[] memberList, int count );

		/// <summary>
		/// Continues the search for domain members from a previous cursor.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		bool FindPreviousDomainMembers( ref Object searchContext, out Member[] memberList, int count );

		/// <summary>
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		bool OwnsDomain( string domainID );

		/// <summary>
		/// Informs the domain provider that the specified member object is about to be
		/// committed to the domain's member list. This allows an opportunity for the 
		/// domain provider to add any domain specific attributes to the member object.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <param name="member">Member object that is about to be committed to the domain's member list.</param>
		void PreCommit( string domainID, Member member );

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		Uri ResolveLocation( string domainID );

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
		Uri ResolveLocation( string domainID, string collectionID );

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
		Uri ResolveLocation( string domainID, string userID, string collectionID );

		/// <summary>
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		Uri ResolvePOBoxLocation( string domainID, string userID );
		#endregion
	}
}
