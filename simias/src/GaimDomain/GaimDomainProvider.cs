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
 *  Author(s): 
 *      Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;

using Simias.Authentication;
using Simias.Security.Web;
using Simias.Storage;
using Simias.Sync;

using Simias.Gaim;

namespace Simias.Gaim
{
	/// <summary>
	/// Class to support Simias' IDomainProvider interface
	/// </summary>
	public class GaimDomainProvider : IDomainProvider
	{
		#region Class Members
		private string providerName = "Gaim Domain Provider";
		private string description = "Simias Domain Provider for the Gaim Workgroup Domain";
		private Hashtable searchContexts = new Hashtable();
		private static readonly ISimiasLog log = 
			SimiasLogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the location provider.
		/// </summary>
		public string Name
		{
			get { return( providerName ); }
		}

		/// <summary>
		/// Gets the description of the location provider.
		/// </summary>
		public string Description
		{
			get { return( description ); }
		}
		#endregion

		#region Constructors

		public GaimDomainProvider()
		{
			log.Debug( "Instance constructor called" );
			try
			{
			}
			catch( Exception e )
			{
				log.Debug( e.Message );
				log.Debug( e.StackTrace );
			}
		}

		#endregion

		#region IDomainProvider Implementation
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
		public Authentication.Status Authenticate( Simias.Storage.Domain domain, HttpContext httpContext )
		{
			return new Authentication.Status( Authentication.StatusCodes.Success );
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
			// GaimDomain does not keep location state
			// so wave it on through
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		public void FindCloseDomainMembers( Object searchContext )
		{
			if (searchContext == null) return;

			string searchContextID = (string)searchContext;
			if (searchContexts.Contains(searchContextID))
			{
				searchContexts.Remove(searchContextID);
			}
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, int count, out Object searchContext, out Member[] memberList, out int total )
		{
			return FindFirstDomainMembers(domainID, PropertyTags.FullName, String.Empty, SearchOp.Contains, count, out searchContext, out memberList, out total);
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="total">Receives the total number of objects found in the search.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, int count, out Object searchContext, out Member[] memberList, out int total )
		{
			// Ignore the domainID since we only ever have one domain to deal with

			bool bMoreEntries = false;
			ArrayList members = new ArrayList();
			ArrayList extraBuddies = new ArrayList();
			searchContext = null;
			total = 0;

			GaimBuddy[] buddies =
				GaimDomain.SearchForBuddies(mapSimiasAttribToGaim(attributeName),
											searchString,
											operation);
			if (buddies != null && buddies.Length > 0)
			{
				total = buddies.Length;
				foreach (GaimBuddy buddy in buddies)
				{
					if (members.Count < count)
					{
						// We haven't exceeded the requested search size

						string givenName;
						string familyName;
						GaimDomain.ParseGaimBuddyAlias(buddy.Alias, out givenName, out familyName);

						Member member = new Member(buddy.Name, Guid.NewGuid().ToString(),//  buddy.MungedID,
												   Simias.Storage.Access.Rights.ReadWrite,
												   givenName, familyName);
						member.FN = buddy.Alias;

						members.Add(member);
					}
					else
					{
						// Save the extra buddies for later
						extraBuddies.Add(buddy);
					}
				}
			}

			memberList = members.ToArray(typeof(Member)) as Member[];

			if (extraBuddies.Count > 0)
			{
				GaimDomainSearchContext newSearchContext = new GaimDomainSearchContext();
				newSearchContext.Buddies = extraBuddies;
				searchContext = newSearchContext.ID;
				lock (searchContexts.SyncRoot)
				{
					searchContexts.Add(searchContext, newSearchContext);
				}

				bMoreEntries = true;
			}

			return bMoreEntries;
		}

		/// <summary>
		/// Continues the search for all domain members started by calling the FindFirstDomainMembers method.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindNextDomainMembers( ref Object searchContext, int count, out Member[] memberList )
		{
			bool bMoreEntries = false;
			ArrayList members = new ArrayList();
			memberList = null;

			if (searchContext == null)
				throw new ArgumentNullException("searchContext cannot be null when calling FindNextDomainMembers");

			string searchContextID = (string)searchContext;
			lock (searchContexts.SyncRoot)
			{
				if (!searchContexts.Contains(searchContextID))
					return false;

				GaimDomainSearchContext gaimDomainSearchContext = (GaimDomainSearchContext)searchContexts[searchContextID];

				foreach (GaimBuddy buddy in gaimDomainSearchContext.Buddies)
				{
					if (members.Count < count)
					{
						string givenName;
						string familyName;
						GaimDomain.ParseGaimBuddyAlias(buddy.Alias, out givenName, out familyName);

						Member member = new Member(buddy.Name, Guid.NewGuid().ToString(), //buddy.MungedID,
							Simias.Storage.Access.Rights.ReadWrite,
							givenName, familyName);

						member.FN = buddy.Alias;

						members.Add(member);

						gaimDomainSearchContext.Buddies.Remove(buddy);
					}
					else
					{
						bMoreEntries = true;
						break;
					}
				}
			}

			if (members.Count > 0)
			{
				memberList = members.ToArray(typeof (Member)) as Member[];
			}

			if (!bMoreEntries)
			{
				// Cleanup the searchContext just in case the caller forgets to
				FindCloseDomainMembers(searchContext);
			}

			return bMoreEntries;
		}

		/// <summary>
		/// Continues the search for domain members from a previous cursor.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindPreviousDomainMembers( ref Object searchContext, int count, out Member[] memberList )
		{
			// FIXME: Implement FindPreviousDomainMembers()
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
			return ( domainID.ToLower() == Simias.Gaim.GaimDomain.ID ) ? true : false;
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
			log.Debug( "ResolveLocation(domainID) called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = GaimDomain.GetDomain();
					Member member = domain.GetMemberByID(domain.Owner.UserID);
					if ( member != null )
					{
						locationUri = MemberToUri(domain, member);
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
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
			log.Debug( "ResolveLocation called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = GaimDomain.GetDomain();
					Collection collection = Store.GetStore().GetCollectionByID( collectionID );
					Member member = collection.GetMemberByID(collection.Owner.UserID);
					if ( member != null )
					{
						locationUri = MemberToUri(domain, member);
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
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
			log.Debug( "ResolveLocation with userID called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = GaimDomain.GetDomain();
					Member member = domain.GetMemberByID(userID);
					if ( member != null )
					{
						locationUri = MemberToUri(domain, member);
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;
		}

		/// <summary>
		/// Returns the network location of where to the POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			log.Debug( "ResolvePOBoxLocation called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = GaimDomain.GetDomain();
					Member member = domain.GetMemberByID(userID);
					if ( member != null )
					{
						locationUri = MemberToUri(domain, member);
					}
				}
				catch ( Exception e )
				{
					log.Debug( e.Message );
					log.Debug( e.StackTrace );
				}
			}

			return locationUri;

		}

		#endregion

		#region Private Methods
		private Uri MemberToUri(Simias.Storage.Domain domain, Member member)
		{
			Uri locationUri = null;
			
			if (domain == null || member == null) return null;			

			// Since we store the Buddy's SimiasURL right in the Simias Store,
			// we can just retrieve the information from the database.
			Simias.Storage.PropertyList pList = member.Properties;
			Simias.Storage.Property p = pList.GetSingleProperty("Gaim:SimiasURL");
			if (p == null) return null;
			
			locationUri = new Uri((string) p.Value);

			return locationUri;
		}

		private string mapSimiasAttribToGaim(string attributeName)
		{
			switch(attributeName)
			{
				case "Given":
					return "ScreenName";
				case "Family":
				case "FN":
				default:
					return "Alias";
			}
		}
		#endregion
	}
}
