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
using System.Collections;
using System.IO;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a directory entry in a external file system.
	/// </summary>
	[ Serializable ]
	public class DirNode : Node
	{
		#region Properties
		/// <summary>
		/// Gets the directory creation time.
		/// </summary>
		public DateTime CreationTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.DirCreationTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new ApplicationException( "Property not found" );
				}
			}

			set	{ properties.ModifyNodeProperty( PropertyTags.DirCreationTime, value ); }
		}

		/// <summary>
		/// Gets whether this object is the root relationship.
		/// </summary>
		public bool IsRoot
		{
			get
			{
				Property p = properties.GetSingleProperty( PropertyTags.Parent );
				return ( p != null ) ? ( p.Value as Relationship ).IsRoot : false;
			}
		}

		/// <summary>
		/// Gets the directory last access time.
		/// </summary>
		public DateTime LastAccessTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.DirLastAccessTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new ApplicationException( "Property not found." );
				}
			}

			set { properties.ModifyNodeProperty( PropertyTags.DirLastAccessTime, value ); }
		}

		/// <summary>
		/// Gets the directory last write time.
		/// </summary>
		public DateTime LastWriteTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.DirLastWriteTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new ApplicationException( "Property not found." );
				}
			}

			set { properties.ModifyNodeProperty( PropertyTags.DirLastWriteTime, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor used to create a new DirNode object.
		/// </summary>
		/// <param name="collection">Collection that this DirNode object will be associated with.</param>
		/// <param name="parentNode">The DirNode object that will be the parent to this object or null
		/// if this directory exists at the collection root.</param>
		/// <param name="dirName">Name of the directory entry.</param>
		public DirNode( Collection collection, DirNode parentNode, string dirName ) :
			this ( collection, parentNode, dirName, Guid.NewGuid().ToString() )
		{
		}

		/// <summary>
		/// Constructor used to create a new DirNode object with a specified ID.
		/// </summary>
		/// <param name="collection">Collection that this DirNode object will be associated with.</param>
		/// <param name="parentNode">The DirNode object that will be the parent to this object or null 
		/// if this directory exists at the collection root.</param>
		/// <param name="dirName">Name of the directory entry.</param>
		/// <param name="dirID">Globally unique identifier for the directory entry.</param>
		public DirNode( Collection collection, DirNode parentNode, string dirName, string dirID ) :
			base ( dirName, dirID, NodeTypes.DirNodeType )
		{
			// Set the parent attribute.
			properties.AddNodeProperty( PropertyTags.Parent, new Relationship( collection.ID, parentNode.ID ) );
		}

		/// <summary>
		/// Constructor used to create a new DirNode object with a specified ID that represents a root 
		/// directory in the Collection.
		/// </summary>
		/// <param name="collection">Collection that this DirNode object will be associated with.</param>
		/// <param name="dirPath">An absolute path to a directory entry in the external file system.</param>
		public DirNode( Collection collection, string dirPath ) :
			this ( collection, dirPath, Guid.NewGuid().ToString() )
		{
		}

		/// <summary>
		/// Constructor used to create a new DirNode object with a specified ID that represents a root 
		/// directory in the Collection.
		/// </summary>
		/// <param name="collection">Collection that this DirNode object will be associated with.</param>
		/// <param name="dirPath">An absolute path to a directory entry in the external file system.</param>
		/// <param name="dirID">Globally unique identifier for the directory entry.</param>
		public DirNode( Collection collection, string dirPath, string dirID ) :
			base ( Path.GetFileName( dirPath ), dirID, NodeTypes.DirNodeType )
		{
			// Set the parent attribute.
			properties.AddNodeProperty( PropertyTags.Parent, new Relationship( collection ) );

			// Set the root path property.
			string parentDir = Path.GetDirectoryName( dirPath );
			if ( parentDir == String.Empty )
			{
				parentDir = Convert.ToString( Path.DirectorySeparatorChar );
			}

			Property p = new Property( PropertyTags.Root, new Uri( parentDir ) );
			p.LocalProperty = true;
			properties.AddNodeProperty( p );
		}

		/// <summary>
		/// Constructor for create an existing DirNode object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create new DirNode object from.</param>
		public DirNode( Collection collection, ShallowNode shallowNode ) :
			base ( collection, shallowNode )
		{
			if ( type != NodeTypes.DirNodeType )
			{
				throw new ApplicationException( "Cannot construct object from specified type." );
			}
		}

		/// <summary>
		/// Constructor for creating an existing DirNode object.
		/// </summary>
		/// <param name="node">Node object to create DirNode object from.</param>
		public DirNode( Node node ) :
			base ( node )
		{
			if ( type != NodeTypes.DirNodeType )
			{
				throw new ApplicationException( "Cannot construct object from specified type." );
			}
		}

		/// <summary>
		/// Constructor for creating an existing DirNode object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create DirNode object from.</param>
		internal DirNode( XmlDocument document ) :
			base ( document )
		{
			if ( type != NodeTypes.DirNodeType )
			{
				throw new ApplicationException( "Cannot construct object from specified type." );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the full path of the directory entry.
		/// </summary>
		/// <param name="collection">Collection object that this object belongs to.</param>
		/// <returns>The absolute path to the directory.</returns>
		public string GetFullPath( Collection collection )
		{
			string fullPath = null;

			DirNode dirNode = GetParent( collection );
			if ( dirNode != null )
			{
				fullPath = Path.Combine( dirNode.GetFullPath( collection ), name );
			}
			else
			{
				Property p = properties.GetSingleProperty( PropertyTags.Root );
				if ( p != null )
				{
					fullPath = Path.Combine( ( p.Value as Uri ).LocalPath, name );
				}
				else
				{
					throw new ApplicationException( "Missing root path." );
				}
			}

			return fullPath;
		}

		/// <summary>
		/// Gets the DirNode object that represents the parent relationship to this directory entry.
		/// </summary>
		/// <param name="collection">Collection object that this object belongs to.</param>
		/// <returns>A DirNode object that represents the parent relationship or null if the DirNode
		/// object exists at the collection root.</returns>
		public DirNode GetParent( Collection collection )
		{
			DirNode parent = null;

			Property property = properties.FindSingleValue( PropertyTags.Parent );
			if ( property != null )
			{
				Relationship relationship = property.Value as Relationship;
				if ( !relationship.IsRoot )
				{
					parent = collection.GetNodeByID( relationship.NodeID ) as DirNode;
				}
			}
			else
			{
				throw new ApplicationException( "DirNode does not contain mandatory parent relationship." );
			}

			return parent;
		}

		/// <summary>
		/// Gets the path relative to the collection root directory.
		/// </summary>
		/// <param name="collection">Collection object that this object belongs to.</param>
		/// <returns>The path relative to the collection root directory.</returns>
		public string GetRelativePath( Collection collection )
		{
			string relativePath = "";

			DirNode dirNode = GetParent( collection );
			if ( dirNode != null )
			{
				relativePath = Path.Combine( dirNode.GetRelativePath( collection ), name );
			}

			return relativePath;
		}

		/// <summary>
		/// Returns whether this DirNode object contains children.
		/// </summary>
		/// <param name="collection">Collection object that this object belongs to.</param>
		/// <returns>True if DirNode object contains children, otherwise false is returned.</returns>
		public bool HasChildren( Collection collection )
		{
			ICSList results = collection.Search( PropertyTags.Parent, new Relationship( collection.ID, id ) );
			ICSEnumerator e = results.GetEnumerator() as ICSEnumerator;
			bool hasChildren = e.MoveNext();
			e.Dispose();
			return hasChildren;
		}
		#endregion
	}
}
