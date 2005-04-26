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

using Simias;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Storage.Tests
{
	/// <summary>
	/// Class used to test functionality.
	/// </summary>
	public class DomainServerAddressTests
	{
		#region Class Members

		private Store store;

		#endregion

		#region Properties
		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Handle to the store.</param>
		public DomainServerAddressTests( Store store )
		{
			this.store = store;
		}

		#endregion

		#region Private Methods

		private Domain CreateDomain()
		{
			// Create a new domain
			Domain domain = 
				new Domain(
					store, 
					"DomainServerAddressTest", 
					Guid.NewGuid().ToString(), 
					"Test domain", 
					SyncRoles.Slave, 
					Domain.ConfigurationType.ClientServer );

			domain.SetType( domain, "Enterprise" );

			Member member = new Member( "Test Owner", Guid.NewGuid().ToString(), Access.Rights.Admin );
			member.IsOwner = true;
			domain.Commit( new Node[] { domain, member } );

			// Set a default host address.
			DomainProvider.SetHostLocation( domain.ID, new Uri( "http://localhost/simias10" ) );
			return domain;
		}

		private void SetHostAddress( string domainID )
		{
			// Get the current address for this domain.
			Uri currentAddress = DomainProvider.ResolveLocation( domainID );
			if ( currentAddress != null )
			{
				UriBuilder ub = new UriBuilder( currentAddress );
				ub.Host = "mylocalhost";
				DomainProvider.SetHostLocation( domainID, ub.Uri );
			}

			currentAddress = DomainProvider.ResolveLocation( domainID );
			if ( ( currentAddress == null ) || ( currentAddress.Host != "mylocalhost" ) )
			{
				throw new ApplicationException( "Did not set host address." );
			}
		}

		private void SetHostAddressWithPort( string domainID )
		{
			// Get the current address for this domain.
			Uri currentAddress = DomainProvider.ResolveLocation( domainID );
			if ( currentAddress != null )
			{
				UriBuilder ub = new UriBuilder( currentAddress );
				ub.Host = "mylocalhost";
				ub.Port = 8086;
				DomainProvider.SetHostLocation( domainID, ub.Uri );
			}

			currentAddress = DomainProvider.ResolveLocation( domainID );
			if ( ( currentAddress == null ) || ( currentAddress.Host != "mylocalhost" ) || ( currentAddress.Port != 8086 ) )
			{
				throw new ApplicationException( "Did not set host address." );
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Runs the collection owner tests.
		/// </summary>
		public void RunTests()
		{
			// Create a domain.
			Domain domain = CreateDomain();

			// Set a new host address.
			SetHostAddress( domain.ID );

			// Set a new host address with a port.
			SetHostAddressWithPort( domain.ID );
		}

		#endregion
	}
}
