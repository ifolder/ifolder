/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
using System.Xml;

using Simias.Client;
using Simias.Sync;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a Domain object in the Collection Store.
	/// </summary>
	public class Domain : Node
	{
		#region Class Members
		/// <summary>
		/// Configuration section name where enterprise key value pairs are stored.
		/// </summary>
		static internal string SectionName = "Domain";

		/// <summary>
		/// Configuration key name for an enterprise domain.
		/// </summary>
		static internal string EnterpriseName = "EnterpriseName";

		/// <summary>
		/// Configuration key name for an enterprise domain ID.
		/// </summary>
		static internal string EnterpriseID = "EnterpriseID";

		/// <summary>
		/// Configuration key name for an enterprise description.
		/// </summary>
		static internal string EnterpriseDescription = "EnterpriseDescription";
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the domain description.
		/// </summary>
		public string Description
		{
			get 
			{ 
				Property p = properties.GetSingleProperty( PropertyTags.Description );
				return ( p != null ) ? p.Value as String : null;
			}

			set { properties.ModifyNodeProperty( PropertyTags.Description, value ); }
		}

		/// <summary>
		/// Gets or sets the host address for the domain.
		/// </summary>
		public Uri HostAddress
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.HostAddress );
				return ( p != null ) ? p.Value as Uri : null;
			}

			set { properties.ModifyNodeProperty( PropertyTags.HostAddress, value ); }
		}

		/// <summary>
		/// The syncing role of the domain.
		/// </summary>
		public SyncRoles Role
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.SyncRole );
				return ( p != null ) ? ( SyncRoles )p.Value : SyncRoles.None;
			}

			set	
			{ 
				Property p = new Property( PropertyTags.SyncRole, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p );
			}
		}

		/// <summary>
		/// Gets the roster for this domain.
		/// </summary>
		public Roster Roster
		{
			get { return Store.GetStore().GetRoster( id ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Domain object.
		/// </summary>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="role">The synchronization role for this domain.</param>
		internal Domain( string domainName, string domainID, SyncRoles role ) :
			this ( domainName, domainID, null, role )
		{
		}

		/// <summary>
		/// Constructor for creating a new Domain object.
		/// </summary>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="description">String that describes this domain.</param>
		/// <param name="role">The synchronization role for this domain.</param>
		internal Domain( string domainName, string domainID, string description, SyncRoles role ) :
			base ( domainName, domainID, NodeTypes.DomainType )
		{
			// Add the description attribute.
			if ( ( description != null ) && ( description.Length > 0 ) )
			{
				properties.AddNodeProperty( PropertyTags.Description, description );
			}

			// Add the role attribute.
			Role = role;
		}

		/// <summary>
		/// Constructor that creates a Domain object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the Domain object from.</param>
		internal Domain( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.DomainType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.DomainType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a Domain object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Domain object from.</param>
		internal Domain( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.DomainType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.DomainType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a Domain object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the Domain object from.</param>
		internal Domain( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.DomainType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.DomainType, type ) );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the domain roster for this domain.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <returns>The Roster object associated with this domain. Null is returned if the call fails.</returns>
		[ Obsolete( "This method should no longer be used. Use the property Roster instead.", false ) ]
		public Roster GetRoster( Store store )
		{
			Roster roster = null;
			ICSList list = store.GetCollectionsByType( NodeTypes.RosterType );
			foreach( ShallowNode sn in list )
			{
				Roster r = new Roster( store, sn );
				if ( r.Domain == id )
				{
					roster = r;
					break;
				}
			}

			return roster;
		}

		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return this.Name;
		}
		#endregion
	}
}
