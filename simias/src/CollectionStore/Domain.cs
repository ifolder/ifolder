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
		/// Well known identitifer for workgroup.
		/// </summary>
		static public string WorkGroupDomainID = "363051D1-8841-4c7b-A1DD-71ABBD0F4ADA";

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
			base ( domainName, domainID, NodeTypes.DomainType )
		{
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
