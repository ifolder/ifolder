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
using System.IO;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents the Collision container where Nodes that need user intervention to
	/// resolve changes are kept.
	/// </summary>
	public class Collision : Collection
	{
		#region Class Members
		/// <summary>
		/// Well known identifier for the collision object.
		/// </summary>
		public const string CollisionID = "6CFF1F68-79A6-43c6-B9E6-9866CBA684EF";
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for this object that creates a Collision object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="ownerGuid">Owner identifier of this object.</param>
		/// <param name="domainName">Name of the domain that this address book belongs to.</param>
		internal Collision( Store storeObject, string ownerGuid, string domainName  ) :
			base ( storeObject, "CollisionContainer", CollisionID, NodeTypes.CollisionType, ownerGuid, domainName )
		{
			Synchronizable = false;
		}

		/// <summary>
		/// Constructor to create an existing Collision object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this object belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		internal Collision( Store storeObject, Node node ) :
			base( storeObject, node )
		{
			if ( type != NodeTypes.CollisionType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.CollisionType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing Collision object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this object belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		internal Collision( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
			if ( type != NodeTypes.CollisionType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.CollisionType, type ) );
			}
		}

		/// <summary>
		/// Constructor to create an existing Collision object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this object belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal Collision( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
			if ( type != NodeTypes.CollisionType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.CollisionType, type ) );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates a Node object that represents the collision of the specified Node object with another instance.
		/// </summary>
		/// <param name="collection">Collection to which the Node object belongs.</param>
		/// <param name="node">Node object that has collided with another instance.</param>
		/// <returns>A Node object that represents the collision.</returns>
		public Node CreateCollisionNode( Collection collection, Node node )
		{
			// Create a node that will represent the collision.
			Node collisionNode = new Node( node.Name );

			// Add a property that holds the data from the collided Node object.
			collisionNode.Properties.AddNodeProperty( PropertyTags.CollisionData, node.Properties.PropertyDocument );

			// Add to the collision Node object a relationship to the original Node object.
			collisionNode.Properties.AddNodeProperty( PropertyTags.Collision, new Relationship( collection.ID, node.ID ) );
			return collisionNode;
		}

		/// <summary>
		/// Gets a list of ShallowNode objects that represent collisions in the specified collection.
		/// </summary>
		/// <param name="collection">Collection object to get collisions for.</param>
		/// <returns>An ICSList object containing ShallowNode objects representing collision Nodes.</returns>
		public ICSList GetCollisions( Collection collection )
		{
			return Search( PropertyTags.Collision, collection.ID, SearchOp.Begins );
		}

		/// <summary>
		/// Gets a list of ShallowNode objects that represent collisions for the specified Node object.
		/// </summary>
		/// <param name="collection">Collection object.</param>
		/// <param name="node">Node object that has collisions.</param>
		/// <returns>An ICSList object containing ShallowNode objects representing collision Nodes.</returns>
		public ICSList GetCollisions( Collection collection, Node node )
		{
			return Search( PropertyTags.Collision, new Relationship( collection.ID, node.ID ) );
		}

		/// <summary>
		/// Gets the Node object that the specified collision Node object represents.
		/// </summary>
		/// <param name="collisionNode">Collision Node object.</param>
		/// <returns>The Node object that is contained by the Collision Node object.</returns>
		public Node GetNodeFromCollision( Node collisionNode )
		{
			Property p = collisionNode.Properties.GetSingleProperty( PropertyTags.CollisionData );
			if ( p == null )
			{
				throw new CollectionStoreException( String.Format( "Collision Node object: {0} - ID: {1} does not contain the CollisionData property", collisionNode.Name, collisionNode.ID ) );
			}

			return new Node( p.Value as XmlDocument );
		}

		/// <summary>
		/// Returns whether the specified collection has collisions.
		/// </summary>
		/// <param name="collection">Collection to check for collisions.</param>
		/// <returns>True if the collection contains collisions, otherwise false is returned.</returns>
		public bool HasCollisions( Collection collection )
		{
			ICSEnumerator e = GetCollisions( collection ).GetEnumerator() as ICSEnumerator;
			bool hasCollisions = e.MoveNext();
			e.Dispose();
			return hasCollisions;
		}

		/// <summary>
		/// Returns whether the specified collection has collisions.
		/// </summary>
		/// <param name="collection">Collection to check for collisions.</param>
		/// <param name="node">Node object that has collisions.</param>
		/// <returns>True if the collection contains collisions, otherwise false is returned.</returns>
		public bool HasCollisions( Collection collection, Node node )
		{
			ICSEnumerator e = GetCollisions( collection, node ).GetEnumerator() as ICSEnumerator;
			bool hasCollisions = e.MoveNext();
			e.Dispose();
			return hasCollisions;
		}
		#endregion
	}
}
