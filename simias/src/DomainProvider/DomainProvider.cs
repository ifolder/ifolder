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
using System.Collections;
using System.Web;

using Simias.Client;
using Simias.Storage;

namespace Simias
{
	/// <summary>
	/// Class that implements the domain provider functionality.
	/// </summary>
	public class DomainProvider
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( DomainProvider ) );

		/// <summary>
		/// Table used to keep track of domain provider mappings.
		/// </summary>
		static private Hashtable domainProviderTable = new Hashtable();

		/// <summary>
		/// List that holds the registered providers.
		/// </summary>
		static private Hashtable registeredProviders = new Hashtable();
		#endregion

		#region Private Methods
		/// <summary>
		/// Searches the list of registered location providers and asks if
		/// any claim ownership of the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of domain to claim ownership for.</param>
		/// <returns>An IDomainProvider object for the provider that claims
		/// the specified domain. A null is returned if no provider claims the
		/// domain.</returns>
		static private IDomainProvider GetDomainProvider( string domainID )
		{
			IDomainProvider provider = null;

			log.Debug( "Searching for domain provider that claims {0} domain.", domainID );

			lock ( typeof( DomainProvider ) )
			{
				// See if there is a provider mapping for this domain.
				string idpName = domainProviderTable[ domainID ] as string;
				if ( idpName != null )
				{
					// There is a domain mapping already set.
					provider = registeredProviders[ idpName ] as IDomainProvider;
					log.Debug( "Provider {0} is already registered for domain {1}", provider.Name, domainID );
				}
				else
				{
					// Search for an owner for this domain.
					foreach( IDomainProvider idp in registeredProviders.Values )
					{
						// See if the provider claims this domain.
						if ( idp.OwnsDomain( domainID ) )
						{
							log.Debug( "Provider {0} claims domain {1}", idp.Name, domainID );
							domainProviderTable.Add( domainID, idp.Name );
							provider = idp;
							break;
						}
					}
				}
			}

			return provider;
		}
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
		static public Authentication.Status Authenticate( Domain domain, HttpContext httpContext )
		{
			IDomainProvider idp = GetDomainProvider( domain.ID );
			if ( idp == null )
			{
				throw new DoesNotExistException( "The specified domain does not exist." );
			}

			return idp.Authenticate( domain, httpContext );
		}

		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="collection">Collection that is being deleted.</param>
		static public void DeleteLocation( Collection collection )
		{
			IDomainProvider idp = GetDomainProvider( collection.Domain );
			if ( idp != null )
			{
				log.Debug( "Deleting location for collection {0}.", collection.Name );
				idp.DeleteLocation( collection.Domain, collection.ID );
			}
		}

		/// <summary>
		/// End the search for domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain.</param>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers or
		/// FindNextDomainMembers methods.</param>
		static public void FindCloseDomainMembers( string domainID, object searchContext )
		{
			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				log.Debug( "Closing search on domain {0}.", domainID );
				idp.FindCloseDomainMembers( searchContext );
			}
		}

		/// <summary>
		/// Starts a search for all domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindFirstDomainMembers( string domainID, out object searchContext, out Member[] memberList, int count )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindFirstDomainMembers( domainID, out searchContext, out memberList, count );
			}

			return moreEntries;
		}

		/// <summary>
		/// Starts a search for a specific set of domain members.
		/// </summary>
		/// <param name="domainID">The identifier of the domain to search for members in.</param>
		/// <param name="attributeName">Attribute name to search.</param>
		/// <param name="searchString">String that contains a pattern to search for.</param>
		/// <param name="operation">Type of search operation to perform.</param>
		/// <param name="searchContext">Receives a provider specific search context object. This object must be serializable.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		public bool FindFirstDomainMembers( string domainID, string attributeName, string searchString, SearchOp operation, out object searchContext, out Member[] memberList, int count )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			searchContext = null;
			memberList = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindFirstDomainMembers( domainID, attributeName, searchString, operation, out searchContext, out memberList, count );
			}

			return moreEntries;
		}

		/// <summary>
		/// Continues the search for all domain members started by calling the FindFirstDomainMembers method.
		/// </summary>
		/// <param name="searchContext">Domain provider specific search context returned by FindFirstDomainMembers method.</param>
		/// <param name="memberList">Receives an array object that contains the domain Member objects.</param>
		/// <param name="count">Maximum number of member objects to return.</param>
		/// <returns>True if there are more domain members. Otherwise false is returned.</returns>
		static public bool FindNextDomainMembers( string domainID, ref object searchContext, out Member[] memberList, int count )
		{
			bool moreEntries = false;

			// Initialize the outputs.
			memberList = null;

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				moreEntries = idp.FindNextDomainMembers( ref searchContext, out memberList, count );
			}

			return moreEntries;
		}

		/// <summary>
		/// Registers the specified domain provider with the domain provider service.
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void RegisterProvider( IDomainProvider provider )
		{
			lock ( typeof( DomainProvider ) )
			{
				log.Debug( "Registering provider {0}.", provider.Name );
				registeredProviders.Add( provider.Name, provider );
			}
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// collection.
		/// </summary>
		/// <param name="collection">Collection to find the network 
		/// location for.</param>
		/// <returns>A Uri object that contains the network location.
		/// If the network location could not be determined, a null
		/// is returned.</returns>
		static public Uri ResolveLocation( Collection collection )
		{
			Uri networkLocation = null;

			log.Debug( "Resolving location for collection {0}.", collection.Name );

			IDomainProvider idp = GetDomainProvider( collection.Domain );
			if ( idp != null )
			{
				// See if the provider already knows about this collection.
				networkLocation = idp.ResolveLocation( collection.Domain, collection.ID );
				if ( ( networkLocation == null ) && !collection.IsHosted )
				{
					// This is a new collection, resolve the location that it should be created.
					log.Debug( "Collection {0} is new.", collection.Name );
					networkLocation = idp.ResolveLocation( collection.Domain, collection.Owner.UserID, collection.ID );
				}
			}

			log.Debug( "Location for collection {0} is {1}.", collection.Name, ( networkLocation != null ) ? networkLocation.ToString() : "not found" );
			return networkLocation;
		}

		/// <summary>
		/// Returns the network location for the the specified
		/// domain.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		static public Uri ResolveLocation( string domainID )
		{
			Uri networkLocation = null;

			log.Debug( "Resolving location for domain {0}.", domainID );

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				// See if the provider already knows about this collection.
				networkLocation = idp.ResolveLocation( domainID );
			}

			log.Debug( "Location for domain {0} is {1}.", domainID, ( networkLocation != null ) ? networkLocation.ToString() : "not found" );
			return networkLocation;
		}

		/// <summary>
		/// Returns the network location of where to the specified user's POBox is located.
		/// </summary>
		/// <param name="domainID">Identifier of the domain where a 
		/// collection is to be created.</param>
		/// <param name="userID">The member that will owns the POBox.</param>
		/// <returns>A Uri object that contains the network location.
		/// </returns>
		static public Uri ResolvePOBoxLocation( string domainID, string userID )
		{
			Uri networkLocation = null;

			log.Debug( "Resolving location for POBox for user {0}.", userID );

			IDomainProvider idp = GetDomainProvider( domainID );
			if ( idp != null )
			{
				networkLocation = idp.ResolvePOBoxLocation( domainID, userID );
			}

			return networkLocation;
		}

		/// <summary>
		/// Unregisters this domain provider from the domain provider service.
		/// </summary>
		/// <param name="provider">Domain provider to unregister.</param>
		static public void Unregister( IDomainProvider provider )
		{
			lock ( typeof ( DomainProvider ) )
			{
				log.Debug( "Unregistering domain provider {0}.", provider.Name );

				// Remove the domain provider from the list.
				registeredProviders.Remove( provider.Name );

				// Remove all domain mappings for this provider.
				string[] domainList = new string[ domainProviderTable.Count ];
				domainProviderTable.Keys.CopyTo( domainList, 0 );
				foreach( string domainID in domainList )
				{
					// Is this mapping for the specified provider?
					if ( domainProviderTable[ domainID ] as string == provider.Name )
					{
						log.Debug( "Removing mapping for domain {0}.", domainID );
						domainProviderTable.Remove( domainID );
					}
				}
			}
		}
		#endregion
	}
}
