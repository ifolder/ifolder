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
using System.Threading;
using System.Xml;

using Simias;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a node in the Collection Store. A node contain properties.
	/// </summary>
	[ Serializable ]
	public class Node
	{
		#region Class Members
		/// <summary>
		/// Object that contains a list of properties on the node.
		/// </summary>
		private PropertyList properties;

		/// <summary>
		/// The display name for the node.  This is the "friendly" name that
		/// applications will use for the node.
		/// </summary>
		private string name;

		/// <summary>
		/// The globally unique identifier for this node.
		/// </summary>
		private readonly string id;

		/// <summary>
		/// The type of node.
		/// </summary>
		private readonly string type;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the base type of node (Node, FileNode, DirNode, etc.) for this node.
		/// </summary>
		internal string Type
		{
			get { return type; }
		}

		/// <summary>
		/// Gets the globally unique identifier for this node.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets or sets the name of this node.
		/// </summary>
		public string Name
		{
			get { return name; }

			set 
			{ 
				lock ( properties )
				{
					name = value;
					properties.PropertyRoot.SetAttribute( Property.NameAttr, name );
				}
			}
		}

		/// <summary>
		/// Gets the list of properties for this node.
		/// </summary>
		public PropertyList Properties
		{
			get { return properties; }
		}

		/// <summary>
		/// Gets the local incarnation value from the node.
		/// </summary>
		public ulong LocalIncarnation
		{
			get 
			{
				lock ( properties )
				{
					Property p = properties.GetSingleProperty( Property.LocalIncarnation );
					if ( p == null )
					{
						throw new ApplicationException( "Node does not have a local incarnation property." );
					}

					return ( ulong )p.Value;
				}
			}
		}

		/// <summary>
		/// Gets the master incarnation value from the node.
		/// </summary>
		public ulong MasterIncarnation
		{
			get 
			{
				lock ( properties )
				{
					Property p = properties.GetSingleProperty( Property.MasterIncarnation );
					if ( p == null )
					{
						throw new ApplicationException( "Node does not have a master incarnation property." );
					}

					return ( ulong )p.Value;
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new node object.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the node.</param>
		public Node( string nodeName ) :
			this( nodeName, Guid.NewGuid().ToString(), GetType().ToString() )
		{
		}

		/// <summary>
		/// Constructor for creating a new node object.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the node.</param>
		/// <param name="nodeID">The globally unique identifier for this node.</param>
		public Node( string nodeName, string nodeID ) :
			this ( nodeName, nodeID, GetType().ToString() )
		{
		}

		/// <summary>
		/// Constructor for creating a new node object.
		/// </summary>
		/// <param name="nodeName">This is the friendly name that is used by applications to describe the node.</param>
		/// <param name="nodeID">The globally unique identifier for this node.</param>
		/// <param name="nodeType">The type of node to construct when deserializing node.</param>
		protected Node( string nodeName, string nodeID, string nodeType )
		{
			name = nodeName;
			id = nodeID.ToLower();
			type = nodeType;

			// Create a new PropertyList object.
			properties = new PropertyList( this );
		
			// Set the default properties for this node.
			properties.AddNodeProperty( Property.CreationTime, DateTime.UtcNow );

			Property mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
			mvProp.LocalProperty = true;
			properties.AddNodeProperty( mvProp );

			Property lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
			lvProp.LocalProperty = true;
			properties.AddNodeProperty( lvProp );
		}

		/// <summary>
		/// Constructor for creating a node that exists in the database.
		/// </summary>
		/// <param name="dbProvider">Interface to the database provider.</param>
		/// <param name="collectionID">Identifier for the Collection object.</param>
		/// <param name="nodeID">Identifier of the Node object to construct.</param>
		protected Node ( Persist.IProvider dbProvider, string collectionID, string nodeID )
		{
			// Call the provider to get an XML string that represents this node.
			string nodeStr = dbProvider.GetRecord( nodeID, collectionID );
			if ( nodeStr != null )
			{
				// Covert the string into an XML DOM.
				XmlDocument nodeDoc = new XmlDocument();
				nodeDoc.LoadXml( nodeStr );
				XmlElement nodeObject = nodeDoc.DocumentElement[ Property.ObjectTag ];

				name = nodeObject.GetAttribute( Property.NameAttr );
				id = nodeID.ToLower();
				type = nodeObject.GetAttribute( Property.TypeAttr );

				properties = new PropertyList( nodeObject, PropertyList.PropertyListState.Update );
			}
			else
			{
				throw new ApplicationException( "Node does not exist." );
			}
		}
		#endregion
	}
}
