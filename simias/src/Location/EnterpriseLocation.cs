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

using Simias.Client;
using Simias.Service;
using Simias.Storage;

namespace Simias.Location
{
	/// <summary>
	/// Implementation of the ILocationProvider Service for Simias Enterprise
	/// Server.
	/// </summary>
	public class EnterpriseLocation : ILocationProvider, IThreadService
	{
		#region Class Members
		/// <summary>
		/// String used to identify location provider.
		/// </summary>
		static private string providerName = "Enterprise Location Provider";
		static private string providerDescription = "Location Provider for Simias Enterprise";

		/// <summary>
		/// Store object.
		/// </summary>
		static private Store store = Store.GetStore();
		#endregion

		#region ILocationProvider Members

		/// <summary>
		/// Gets the name of the location provider.
		/// </summary>
		public string Name
		{
			get { return providerName; }
		}

		/// <summary>
		/// Gets the description of the location provider.
		/// </summary>
		public string Description
		{
			get { return providerDescription; }
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
		/// Determines if the provider claims ownership for the 
		/// specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of a domain.</param>
		/// <returns>True if the provider claims ownership for the 
		/// specified domain. Otherwise, False is returned.</returns>
		public bool OwnsDomain( string domainID )
		{
			Domain domain = store.GetDomain( domainID );
			return ( ( domain != null ) && domain.IsType( domain, "Enterprise" ) ) ? true : false;
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
			Domain domain = store.GetDomain( domainID );
			return ( OwnsDomain( domainID ) && ( domain != null ) ) ? domain.HostAddress : null;
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
			Domain domain = store.GetDomain( domainID );
			return ( OwnsDomain( domainID ) && ( domain != null ) ) ? domain.HostAddress : null;
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
			Domain domain = store.GetDomain( domainID );
			return ( OwnsDomain( domainID ) && ( domain != null ) ) ? domain.HostAddress : null;
		}
		#endregion

		#region IThreadService Members

		/// <summary>
		/// Starts the thread service.
		/// </summary>
		/// <param name="config">Configuration file object that indicates which Collection Store to use.</param>
		public void Start( Configuration conf )
		{
			// Register with the location service.
			Locate.RegisterProvider( this );
		}

		/// <summary>
		/// Stops the service from executing.
		/// </summary>
		public void Stop()
		{
			// Unregister with the location service.
			Locate.Unregister( this );
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
	}
}
