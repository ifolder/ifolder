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
 *      Brady Anderson <banderso@novell.com>
 *      Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.Text.RegularExpressions;

using Simias.Storage;
using Simias.Sync;

namespace Simias.Location
{
	/// <summary>
	/// Class to support Simias' plugin Location Provider interface
	/// </summary>
	public class GaimProvider : ILocationProvider
	{
		#region Class Members
		private string providerName = "Gaim Domain Provider";
		private string description = "Simias Location provider which uses the user's Gaim Buddy List to resolve objects";
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

		public GaimProvider()
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

		private Uri MemberIDToUri( string memberID )
		{
			Uri locationUri = null;

			locationUri = new Uri( "http://127.0.0.1:8086/simias10" );

			return locationUri;
		}

		#region ILocationProvider Members

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
			log.Debug( "DeleteLocation called" );
			
			// GaimDomain does not keep location state
			// so wave it on through

			return;
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
					Collection collection = Store.GetStore().GetCollectionByID( collectionID );
					locationUri = MemberIDToUri( collection.Owner.UserID );
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
					Collection collection = Store.GetStore().GetCollectionByID( collectionID );
					Member member = collection.GetMemberByID( userID );
					if ( member != null )
					{
						locationUri = MemberIDToUri( member.UserID );
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
			log.Debug( "ResolveLocation with userID called" );

			Uri locationUri = null;
			if( domainID.ToLower() == Simias.Gaim.GaimDomain.ID )
			{
				try
				{
					Simias.Storage.Domain domain = Store.GetStore().GetDomain( Simias.Gaim.GaimDomain.ID );
					Member member = domain.GetMemberByID( userID );
					if ( member != null )
					{
						locationUri = MemberIDToUri( member.UserID );
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
	}
}
