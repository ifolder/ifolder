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

using Simias;
using Simias.Client;
using Simias.POBox;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a node in the Collection Store. A Node object contain properties.
	/// </summary>
	[ Serializable ]
	public class Node
	{
		#region Class Members
		/// <summary>
		/// Object that contains a list of properties on the object.
		/// </summary>
		protected PropertyList properties;

		/// <summary>
		/// The display name for the object.  This is the "friendly" name that applications will use.
		/// </summary>
		protected string name;

		/// <summary>
		/// The globally unique identifier for the object.
		/// </summary>
		protected readonly string id;

		/// <summary>
		/// The object type.
		/// </summary>
		protected string type;

		/// <summary>
		/// Master incarnation number that this object is to be updated to if it is not zero.
		/// </summary>
		[ NonSerialized() ]
		protected ulong masterIncarnation = 0;

		/// <summary>
		/// This is the expected incarnation value of the Node object when it is being imported.
		/// If this value differs from the actual object incarnation value, a collision results.
		/// </summary>
		[ NonSerialized() ]
		protected ulong expectedIncarnation = 0;

		/// <summary>
		/// Allows the Node object to skip collision detection when resolving a collision.
		/// </summary>
		[ NonSerialized() ]
		protected bool skipCollisionCheck = false;

		/// <summary>
		/// Indicates whether the Node object is being imported on the master or the slave store.
		/// </summary>
		[ NonSerialized() ]
		protected bool isMaster;

		/// <summary>
		/// Indicates whether to merge the collision property on a Node object or whether to strip it
		/// off during a commit of the Node object.
		/// </summary>
		[ NonSerialized() ]
		protected bool mergeCollisions = true;

		/// <summary>
		/// This field is used by commit so that when the database is locked and this object is read
		/// off disk for a merge or set local property operation, it doesn't have to be read twice. This
		/// field is meant for internal use only. It is only valid at certain times.
		/// </summary>
		[ NonSerialized() ]
		protected Node diskNode = null;

		/// <summary>
		/// Indicates if object property changes are local changes only.
		/// </summary>
		[ NonSerialized() ]
		protected bool localChanges = false;

		/// <summary>
		/// Indicates to the commit code whether to generate an event for this node.
		/// </summary>
		[ NonSerialized() ]
		protected bool indicateEvent = true;
		#endregion

		#region Properties
		/// <summary>
		/// Needed so that a new property list can be instantiated on an existing object.
		/// </summary>
		internal protected PropertyList InternalList
		{
			set { properties = value; }
		}

		/// <summary>
		/// Gets or sets the expected incarnation value.
		/// </summary>
		internal protected ulong ExpectedIncarnation
		{
			get { return expectedIncarnation; }
			set { expectedIncarnation = value; }
		}

		/// <summary>
		/// Sets the Node object name without triggering a property update.
		/// </summary>
		internal string BaseName
		{
			set 
			{ 
				name = value;
				properties.PropertyRoot.SetAttribute( XmlTags.NameAttr, name );
			}
		}

		/// <summary>
		/// Gets or sets the base type (Collection or Node) for this object.
		/// </summary>
		internal string BaseType
		{
			get { return type; }
			set { type = value; }
		}

		/// <summary>
		/// Gets or sets this node object that was read from the disk while the database was locked.
		/// This is intended for use by the commit code only.
		/// </summary>
		internal Node DiskNode
		{
			get { return diskNode; }
			set { diskNode = value; }
		}

		/// <summary>
		/// Gets or sets whether object property changes are local changes only.
		/// </summary>
		internal bool LocalChanges
		{
			get { return localChanges; }
			set { localChanges = value; }
		}

		/// <summary>
		/// Gets or sets whether to allow the Node object to skip collision checking at commit time.
		/// </summary>
		internal bool SkipCollisionCheck
		{
			get { return skipCollisionCheck; }
			set { skipCollisionCheck = value; }
		}
		
		/// <summary>
		/// Gets or sets whether the Node object is being imported on the master or the slave store.
		/// </summary>
		internal bool IsMaster
		{
			get { return isMaster; }
			set { isMaster = value; }
		}

		/// <summary>
		/// Gets or sets whether to merge the collision property on the Node object during commit.
		/// </summary>
		internal bool MergeCollisions
		{
			get { return mergeCollisions; }
			set { mergeCollisions = value; }
		}

		/// <summary>
		/// Gets or sets whether the commit code should indicate and event for this node.
		/// </summary>
		internal bool IndicateEvent
		{
			get { return indicateEvent; }
			set { indicateEvent = value; }
		}

		/// <summary>
		/// Gets or sets the update time of a node object.
		/// </summary>
		internal DateTime UpdateTime
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.NodeUpdateTime );
				return ( p != null ) ? ( DateTime )p.Value : DateTime.MinValue;
			}

			set 
			{ 
				Property p = new Property( PropertyTags.NodeUpdateTime, value );
				p.LocalProperty = true;
				properties.ModifyNodeProperty( p );
			}
		}

		/// <summary>
		/// Gets the creation time for this object.
		/// </summary>
		public DateTime CreationTime
		{
			get { return ( DateTime )properties.GetSingleProperty( PropertyTags.NodeCreationTime ).Value; }
		}

		/// <summary>
		/// Gets the globally unique identifier for this object.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets or sets the name of this object.
		/// </summary>
		public string Name
		{
			get { return name; }

			set 
			{ 
				// Create a property that will be used at merge time. The Property member XmlPropertyList
				// must be set to the current property list or the property object will not be considered
				Property p = new Property( BaseSchema.ObjectName, value );
				p.XmlPropertyList = properties;
				p.SaveMergeInformation( properties, Property.Operation.NameChange, name, false, 0 );
				BaseName = value;
			}
		}

		/// <summary>
		/// Gets the base type (Collection or Node) for this object.
		/// </summary>
		public string Type
		{
			get { return type; }
		}

		/// <summary>
		/// Gets the list of properties for this object.
		/// </summary>
		public PropertyList Properties
		{
			get { return properties; }
		}

		/// <summary>
		/// Gets the local incarnation value from the object.
		/// </summary>
		public ulong LocalIncarnation
		{
			get { return ( ulong )properties.FindSingleValue( PropertyTags.LocalIncarnation ).Value; }
		}

		/// <summary>
		/// Gets or sets the Node object to be a proxy that is to be overwritten by the next sync cycle.
		/// </summary>
		public bool Proxy
		{
			get { return ( properties.State == PropertyList.PropertyListState.Proxy ) ? true : false; }
			set { properties.State = value ? PropertyList.PropertyListState.Proxy : PropertyList.PropertyListState.Add; }
		}

		/// <summary>
		/// Gets the master incarnation value from the object.
		/// </summary>
		public ulong MasterIncarnation
		{
			get { return ( ulong )properties.FindSingleValue( PropertyTags.MasterIncarnation ).Value; }
		}

		/// <summary>
		/// Allows the master incarnation number to be updated on this object during commit time.
		/// 
		/// Note: This property should only be used by the synchronization process.
		/// </summary>
		public ulong IncarnationUpdate
		{
			get { return masterIncarnation; }
			set { masterIncarnation = value; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new Node object without a name.
		/// </summary>
		public Node() :
			this( String.Empty )
		{
		}

		/// <summary>
		/// Constructor for creating a new Node object.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the object.</param>
		public Node( string nodeName ) :
			this( nodeName, Guid.NewGuid().ToString() )
		{
		}

		/// <summary>
		/// Constructor for creating a new Node object with a specified ID.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the object.</param>
		/// <param name="nodeID">The globally unique identifier for this object.</param>
		public Node( string nodeName, string nodeID ) :
			this ( nodeName, nodeID, NodeTypes.NodeType )
		{
		}

		/// <summary>
		/// Copy constructor for a Node object.
		/// </summary>
		/// <param name="node">Node object to create new Node object from.</param>
		public Node( Node node )
		{
			name = node.name;
			id = node.id;
			type = node.type;
			properties = new PropertyList( node.properties );
		}

		/// <summary>
		/// Copy constructor for a Node object.
		/// </summary>
		/// <param name="ID">New identifier for Node object.</param>
		/// <param name="node">Node object to create new Node object from.</param>
		public Node( string ID, Node node )
		{
			name = node.name;
			id = ID;
			type = node.type;
			properties = new PropertyList( node.properties );
			properties.State = PropertyList.PropertyListState.Add;
			properties.PropertyRoot.SetAttribute( XmlTags.IdAttr, ID );

			// If the collection ID exists on the Node, remove it.
			Property p = properties.GetSingleProperty( BaseSchema.CollectionId );
			if ( p != null )
			{
				p.DeleteProperty();
			}

			// Reset the new incarnation values.
			Property mvProp = new Property( PropertyTags.MasterIncarnation, ( ulong )0 );
			mvProp.LocalProperty = true;
			properties.ModifyNodeProperty( mvProp );

			Property lvProp = new Property( PropertyTags.LocalIncarnation, ( ulong )0 );
			properties.ModifyNodeProperty( lvProp );
		}

		/// <summary>
		/// Constructor for create an existing Node object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create new Node object from.</param>
		public Node( Collection collection, ShallowNode shallowNode )
		{
			Node node = collection.GetNodeByID( shallowNode.ID );
			if ( node == null )
			{
				throw new DoesNotExistException( "The specified Node object does not exist." );
			}

			name = node.name;
			id = node.id;
			type = node.type;
			properties = new PropertyList( node.properties );
		}

		/// <summary>
		/// Constructor for creating a new Node object.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the object.</param>
		/// <param name="nodeID">The globally unique identifier for this object.</param>
		/// <param name="nodeType">The type of derived object to construct when deserializing the object.</param>
		internal protected Node( string nodeName, string nodeID, string nodeType )
		{
			name = nodeName;
			id = nodeID.ToLower();
			type = nodeType;

			// Create a new PropertyList object.
			properties = new PropertyList( name, id, type );

			// Set the object type(s) in the properties.
			properties.AddNodeProperty( PropertyTags.Types, nodeType );
			if ( nodeType != NodeTypes.NodeType )
			{
				properties.AddNodeProperty( PropertyTags.Types, NodeTypes.NodeType );
			}
		}

		/// <summary>
		/// Constructor for creating an existing Node object.
		/// </summary>
		/// <param name="document">Xml document that describes a Node object.</param>
		public Node( XmlDocument document )
		{
			XmlElement nodeObject = document.DocumentElement[ XmlTags.ObjectTag ];

			name = nodeObject.GetAttribute( XmlTags.NameAttr );
			id = nodeObject.GetAttribute( XmlTags.IdAttr );
			type = nodeObject.GetAttribute( XmlTags.TypeAttr );

			properties = new PropertyList( document );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Node factory method that constructs a derived Node object type from the specified Node object.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="document">Xml document to construct new Node from.</param>
		/// <returns>Downcasts the derived Node object back to a Node that can then be explicitly casted back up.</returns>
		static public Node NodeFactory( Store store, XmlDocument document )
		{
			XmlElement nodeObject = document.DocumentElement[ XmlTags.ObjectTag ];
			Node rNode = null;

			switch ( nodeObject.GetAttribute( XmlTags.TypeAttr ) )
			{
				case "Node":
					rNode = new Node( document );
					break;

				case "DirNode":
					rNode = new DirNode( document );
					break;

				case "FileNode":
					rNode = new FileNode( document );
					break;

				case "LinkNode":
					rNode = new LinkNode( document );
					break;

				case "StoreFileNode":
					rNode = new StoreFileNode( document );
					break;

				case "Collection":
					rNode = new Collection( store, document );
					break;

				case "Tombstone":
					rNode = new Node( document );
					break;

				case "LocalDatabase":
					rNode = new LocalDatabase( store, document );
					break;

				case "Identity":
					rNode = new Identity( document );
					break;

				case "Member":
					rNode = new Member( document );
					break;

				case "Domain":
					rNode = new Domain( store, document );
					break;

				case "Policy":
					rNode = new Policy.Policy( document );
					break;

				case "POBox":
					rNode = new POBox.POBox( store, document );
					break;

				case "Subscription":
					rNode = new Subscription( document );
					break;

				default:
					rNode = new Node( document );
					break;
			}

			return rNode;
		}

		/// <summary>
		/// Sets the master incarnation values on a Node object indicating that it has been
		/// sent to the server and thus has been updated.
		/// </summary>
		/// <param name="incarnationValue">The master incarnation value to set.</param>
		public void SetMasterIncarnation( ulong incarnationValue )
		{
			// Set the master incarnation value.
			properties.State = PropertyList.PropertyListState.Internal;
			properties.ModifyNodeProperty( PropertyTags.MasterIncarnation, incarnationValue );

			// This node is being updated because it was pushed to the server. Remove the
			// updated property.
			Property p = properties.GetSingleProperty( PropertyTags.NodeUpdateTime );
			if ( p != null )
			{
                p.DeleteProperty();
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Node factory method that constructs a derived Node object type from the specified ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection object associated with the specified Node object.</param>
		/// <param name="shallowNode">ShallowNode object to construct new Node from.</param>
		/// <returns>Downcasts the derived Node object back to a Node that can then be explicitly casted back up.</returns>
		static public Node NodeFactory( Collection collection, ShallowNode shallowNode )
		{
			Node rNode = null;

			switch ( shallowNode.Type )
			{
				case "Node":
					rNode = new Node( collection, shallowNode );
					break;

				case "DirNode":
					rNode = new DirNode( collection, shallowNode );
					break;

				case "FileNode":
					rNode = new FileNode( collection, shallowNode );
					break;

				case "LinkNode":
					rNode = new LinkNode( collection, shallowNode );
					break;

				case "StoreFileNode":
					rNode = new StoreFileNode( collection, shallowNode );
					break;

				case "Collection":
					rNode = new Collection( collection.StoreReference, shallowNode );
					break;

				case "Tombstone":
					rNode = new Node( collection, shallowNode );
					break;

				case "LocalDatabase":
					rNode = new LocalDatabase( collection.StoreReference, shallowNode );
					break;

				case "Identity":
					rNode = new Identity( collection, shallowNode );
					break;

				case "Member":
					rNode = new Member( collection, shallowNode );
					break;

				case "Domain":
					rNode = new Domain( collection.StoreReference, shallowNode );
					break;

				case "Policy":
					rNode = new Policy.Policy( collection, shallowNode );
					break;

				case "POBox":
					rNode = new POBox.POBox( collection.StoreReference, shallowNode );
					break;

				case "Subscription":
					rNode = new Subscription( collection, shallowNode );
					break;

				default:
					rNode = new Node( collection, shallowNode );
					break;
			}

			return rNode;
		}

		/// <summary>
		/// Node factory method that constructs a derived Node object type from the specified Node object.
		/// </summary>
		/// <param name="collection">Collection object associated with the specified Node object.</param>
		/// <param name="node">Node object to construct new Node from.</param>
		/// <returns>Downcasts the derived Node object back to a Node that can then be explicitly casted back up.</returns>
		static public Node NodeFactory( Collection collection, Node node )
		{
			return NodeFactory( collection.StoreReference, node.Properties.PropertyDocument );
		}
		#endregion
	}
}
