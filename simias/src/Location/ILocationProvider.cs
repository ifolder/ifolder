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

namespace Simias.Location
{
	/// <summary>
	/// Interface for a Location Provider Service.
	/// </summary>
	public interface ILocationProvider
	{
		#region Properties
		/// <summary>
		/// Gets the name of the location provider.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the description of the location provider.
		/// </summary>
		string Description { get; }
		#endregion

		#region Public Methods
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
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		bool OwnsDomain( string domainID );

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
