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

using Simias.Client;
using Simias.Storage;

namespace Simias.Location
{
	/// <summary>
	/// Class that determines the network location of remote Collections.
	/// </summary>
	public class Locate
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Locate ) );

		/// <summary>
		/// Table used to keep track of domain-to-location provider mappings.
		/// </summary>
		static private Hashtable locationProviderTable = new Hashtable();

		/// <summary>
		/// List that holds the registered providers.
		/// </summary>
		static private Hashtable registeredProviders = new Hashtable();
		#endregion

		#region Private Methods
		/// <summary>
		/// Searches the list of registered location providers and asks if
		/// any claim ownership of the specified domain.
		/// 
		/// NOTE: The Location lock must be held before making this call.
		/// 
		/// </summary>
		/// <param name="domainID">Identifier of domain to claim ownership
		/// for.</param>
		/// <returns>An ILocationProvider object for the provider that claims
		/// the specified domain. A null is returned if no provider claims the
		/// domain.</returns>
		static private ILocationProvider GetDomainProvider( string domainID )
		{
			ILocationProvider provider = null;

			log.Debug( "Searching for location provider that claims {0} domain.", Store.GetStore().GetDomain( domainID ).Name );

			foreach( ILocationProvider ilp in registeredProviders.Values )
			{
				// See if the provider claims this domain.
				if ( ilp.OwnsDomain( domainID ) )
				{
					log.Debug( "Provider {0} claims domain {1}", ilp.Name, Store.GetStore().GetDomain( domainID ).Name );
					locationProviderTable.Add( domainID, ilp.Name );
					provider = ilp;
					break;
				}
			}

			return provider;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Indicates to the provider that the specified collection has
		/// been deleted and a mapping is no longer required.
		/// </summary>
		/// <param name="collection">Collection that is being deleted.</param>
		static public void DeleteLocation( Collection collection )
		{
			ILocationProvider ilp = null;

			lock ( typeof( Locate ) )
			{
				// See if there is a provider mapping for this domain.
				string ilpName = locationProviderTable[ collection.Domain ]as string;
				ilp = ( ilpName != null ) ? registeredProviders[ ilpName ] as ILocationProvider : GetDomainProvider( collection.Domain );
			}

			// Is there a provider for this domain?
			if ( ilp != null )
			{
				log.Debug( "Deleting location for collection {0}.", collection.Name );
				ilp.DeleteLocation( collection.Domain, collection.ID );
			}
		}

		/// <summary>
		/// Registers the specified location provider with the location service.
		/// </summary>
		/// <param name="provider">An ILocationProvider interface object.</param>
		static public void RegisterProvider( ILocationProvider provider )
		{
			lock ( typeof( Locate ) )
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
			ILocationProvider ilp = null;
			Uri networkLocation = null;

			log.Debug( "Resolving location for collection {0}.", collection.Name );

			lock ( typeof( Locate) )
			{
				// See if there is a provider mapping for this domain.
				// See if there is a provider mapping for this domain.
				string ilpName = locationProviderTable[ collection.Domain ]as string;
				ilp = ( ilpName != null ) ? registeredProviders[ ilpName ] as ILocationProvider : GetDomainProvider( collection.Domain );
			}

			// Is there a provider for this domain?
			if ( ilp != null )
			{
				// See if the provider already knows about this collection.
				networkLocation = ilp.ResolveLocation( collection.Domain, collection.ID );
				if ( ( networkLocation == null ) && !collection.IsHosted )
				{
					// This is a new collection, resolve the location that it should be created.
					log.Debug( "Collection {0} is new.", collection.Name );
					networkLocation = ilp.ResolveLocation( collection.Domain, collection.Owner.UserID, collection.ID );
				}
			}

			log.Debug( "Location for collection {0} is {1}.", collection.Name, ( networkLocation != null ) ? networkLocation.ToString() : "not found" );
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
			ILocationProvider ilp = null;
			Uri networkLocation = null;

			log.Debug( "Resolving location for POBox for user {0}.", userID );

			lock ( typeof( Locate ) )
			{
				// See if there is a provider mapping for this domain.
				// See if there is a provider mapping for this domain.
				string ilpName = locationProviderTable[ domainID ]as string;
				ilp = ( ilpName != null ) ? registeredProviders[ ilpName ] as ILocationProvider : GetDomainProvider( domainID );
			}

			// Is there a provider for this domain?
			if ( ilp != null )
			{
				networkLocation = ilp.ResolvePOBoxLocation( domainID, userID );
			}

			return networkLocation;
		}

		/// <summary>
		/// Unregisters this location provider from the location service.
		/// </summary>
		/// <param name="provider">Location provider to unregister.</param>
		static public void Unregister( ILocationProvider provider )
		{
			lock ( typeof ( Locate ) )
			{
				log.Debug( "Unregistering location provider {0}.", provider.Name );

				// Remove the location provider from the list.
				registeredProviders.Remove( provider.Name );

				// Remove all domain mappings for this provider.
				string[] domainList = new string[ locationProviderTable.Count ];
				locationProviderTable.Keys.CopyTo( domainList, 0 );
				foreach( string domainID in domainList )
				{
					// Is this mapping for the specified provider?
					if ( locationProviderTable[ domainID ] as string == provider.Name )
					{
						log.Debug( "Removing mapping for domain {0}.", domainID );
						locationProviderTable.Remove( domainID );
					}
				}
			}
		}
		#endregion
	}
}
