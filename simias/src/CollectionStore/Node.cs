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
		protected ulong masterIncarnation = 0;
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
		/// Gets or sets the base type (Collection or Node) for this object.
		/// </summary>
		internal string BaseType
		{
			get { return type; }
			set { type = value; }
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
				name = value;
				properties.PropertyRoot.SetAttribute( XmlTags.NameAttr, name );
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
			get { return ( ulong )properties.FindSingleValue( Property.LocalIncarnation ).Value; }
		}

		/// <summary>
		/// Gets the master incarnation value from the object.
		/// </summary>
		public ulong MasterIncarnation
		{
			get { return ( ulong )properties.FindSingleValue( Property.MasterIncarnation ).Value; }
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
			this ( nodeName, nodeID, "Node" )
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
		/// Constructor for create an existing Node object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create new Node object from.</param>
		public Node( Collection collection, ShallowNode shallowNode ) :
			this( collection.GetNodeByID( shallowNode.ID ) )
		{
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

			// Set the object type in the properties.
			properties.AddNodeProperty( Property.Types, nodeType );
		}

		/// <summary>
		/// Constructor for creating an existing Node object.
		/// </summary>
		/// <param name="document">Xml document that describes a Node object.</param>
		internal protected Node( XmlDocument document )
		{
			XmlElement nodeObject = document.DocumentElement[ XmlTags.ObjectTag ];

			name = nodeObject.GetAttribute( XmlTags.NameAttr );
			id = nodeObject.GetAttribute( XmlTags.IdAttr );
			type = nodeObject.GetAttribute( XmlTags.TypeAttr );

			properties = new PropertyList( document );
		}
		#endregion
	}
}
