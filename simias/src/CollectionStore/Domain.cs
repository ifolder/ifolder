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
	public class Domain : Collection
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

			set 
			{ 
				Property p = new Property( PropertyTags.HostAddress, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p ); 
			}
		}

		/// <summary>
		/// The syncing role of the domain.
		/// </summary>
		new public SyncRoles Role
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
		#endregion

		#region Constructors
		/// <param name="store">Store object.</param>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="description">String that describes this domain.</param>
		/// <param name="role">The synchronization role for this domain.</param>
		public Domain( Store store, string domainName, string domainID, string description, SyncRoles role ) :
			this ( store, domainName, domainID, description, role, null )
		{
		}

		/// <param name="store">Store object.</param>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="description">String that describes this domain.</param>
		/// <param name="role">The synchronization role for this domain.</param>
		/// <param name="locationAddress">The network address of the domain's location service.</param>
		public Domain( Store store, string domainName, string domainID, string description, SyncRoles role, Uri locationAddress ) :
			base ( store, domainName, domainID, NodeTypes.DomainType, domainID )
		{
			// Add the description attribute.
			if ( ( description != null ) && ( description.Length > 0 ) )
			{
				properties.AddNodeProperty( PropertyTags.Description, description );
			}

			// Add the location address.
			if ( locationAddress != null )
			{
				HostAddress = locationAddress;
			}

			// Add the role attribute.
			Role = role;
		}

		/// <summary>
		/// Constructor to create an existing Domain object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		public Domain( Store storeObject, Node node ) :
			base( storeObject, node )
		{
		}

		/// <summary>
		/// Constructor for creating an existing Domain object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Domain( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
		}

		/// <summary>
		/// Constructor to create an existing Domain object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal Domain( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Obtains the string representation of this instance.
		/// </summary>
		/// <returns>The friendly name of the domain.</returns>
		public override string ToString()
		{
			return Name;
		}
		#endregion
	}
}
