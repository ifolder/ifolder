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

namespace Simias.Storage
{
	/// <summary>
	/// Represents an object that can be used as an alias to a Node object.
	/// </summary>
	public class LinkNode : Node
	{
		#region Constructor
		/// <summary>
		/// Constructor used to create a new LinkNode object.
		/// </summary>
		/// <param name="name">The name of this LinkNode object.</param>
		/// <param name="relationship">A relationship object set for another Node object.</param>
		public LinkNode( string name, Relationship relationship ) :
			this ( name, Guid.NewGuid().ToString(), relationship )
		{
		}

		/// <summary>
		/// Constructor used to create a new LinkNode object with a specified ID.
		/// </summary>
		/// <param name="name">The name of this LinkNode object.</param>
		/// <param name="nodeID">Globally unique identifier for the LinkNode object.</param>
		/// <param name="relationship">A relationship object set for another Node object.</param>
		public LinkNode( string name, string nodeID, Relationship relationship ) :
			base( name, nodeID, NodeTypes.LinkNodeType )
		{
			properties.AddProperty( PropertyTags.LinkReference, relationship );
		}

		/// <summary>
		/// Constructor for creating an existing LinkNode object.
		/// </summary>
		/// <param name="collection">Collection that the Node object belongs to.</param>
		/// <param name="node">Node object to create LinkNode object from.</param>
		protected LinkNode( Collection collection, Node node ) :
			base ( node )
		{
			if ( !collection.IsType( node, NodeTypes.LinkNodeType ) )
			{
				throw new ApplicationException( "Cannot construct object from specified type." );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the Node referenced by this LinkNode object.
		/// </summary>
		/// <param name="store">A reference to the Collection Store.</param>
		/// <returns>The node that is referenced by this LinkNode object or null if the link is broken.</returns>
		public Node GetReference( Store store )
		{
			Node node = null;

			// Get the pre-defined relationship property.
			Relationship reference = properties.FindSingleValue( PropertyTags.LinkReference ).Value as Relationship;
        
			// Get the referenced collection.
			Collection collection = store.GetCollectionByID( reference.CollectionID );
			if ( collection != null )
			{
				// Get the referenced Node object from the collection.
				node = collection.GetNodeByID( reference.NodeID );
			}

			return node;
		}
		#endregion
	}
}
