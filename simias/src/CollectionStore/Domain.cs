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

		/// <summary>
		/// Well known identitifer for workgroup.
		/// </summary>
		static public string WorkGroupDomainID = "363051d1-8841-4c7b-a1dd-71abbd0f4ada";

		/// <summary>
		/// Friendly name for the workgroup domain.
		/// </summary>
		static public string WorkGroupDomainName = "WorkGroup";
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Domain object.
		/// </summary>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		internal Domain( string domainName, string domainID ) :
			this ( domainName, domainID, null )
		{
		}

		/// <summary>
		/// Constructor for creating a new Domain object.
		/// </summary>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known unique identifier for the domain.</param>
		/// <param name="description">String that describes this domain.</param>
		internal Domain( string domainName, string domainID, string description ) :
			base ( domainName, domainID, NodeTypes.DomainType )
		{
			// Add the description attribute.
			if ( ( description != null ) && ( description.Length > 0 ) )
			{
				properties.AddNodeProperty( PropertyTags.Description, description );
			}
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
	}
}
