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
	/// Represents a file entry in an external file system.
	/// </summary>
	[ Serializable ]
	public class FileNode : BaseFileNode
	{
		#region Constructor
		/// <summary>
		/// Constructor used to create a new FileNode object.
		/// </summary>
		/// <param name="collection">Collection that this FileNode object will be associated with.</param>
		/// <param name="parentNode">The DirNode object that will be the parent to this object.</param>
		/// <param name="fileName">Friendly name of the FileNode object.</param>
		public FileNode( Collection collection, DirNode parentNode, string fileName ) :
			this ( collection, parentNode, fileName, Guid.NewGuid().ToString() )
		{
		}

		/// <summary>
		/// Constructor used to create a new FileNode object with a specified ID.
		/// </summary>
		/// <param name="collection">Collection that this FileNode object will be associated with.</param>
		/// <param name="parentNode">The DirNode object that will be the parent to this object.</param>
		/// <param name="fileName">Friendly name of the FileNode object.</param>
		/// <param name="fileID">Globally unique identifier for the FileNode object.</param>
		public FileNode( Collection collection, DirNode parentNode, string fileName, string fileID ) :
			base ( collection, fileName, fileID, NodeTypes.FileNodeType )
		{
			// Set the parent attribute.
			properties.AddNodeProperty( PropertyTags.Parent, new Relationship( collection.ID, parentNode.ID ) );
		}

		/// <summary>
		/// Constructor for creating an existing FileNode object.
		/// </summary>
		/// <param name="node">Node object to create FileNode object from.</param>
		public FileNode( Node node ) :
			base ( node )
		{
			if ( type != NodeTypes.FileNodeType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.FileNodeType, type ) );
			}
		}

		/// <summary>
		/// Constructor for create an existing FileNode object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create new FileNode object from.</param>
		public FileNode( Collection collection, ShallowNode shallowNode ) :
			base ( collection, shallowNode )
		{
			if ( type != NodeTypes.FileNodeType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.FileNodeType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing FileNode object from an Xml document.
		/// </summary>
		/// <param name="document">Xml document object to create FileNode object from.</param>
		internal FileNode( XmlDocument document ) :
			base ( document )
		{
			if ( type != NodeTypes.FileNodeType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.FileNodeType, type ) );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the file entry name with its extension.
		/// </summary>
		/// <returns>The name of the file including the extension.</returns>
		public override string GetFileName()
		{
			return name;
		}

		/// <summary>
		/// Gets the full path of the file entry.
		/// </summary>
		/// <param name="collection">Collection that this file entry is associated with.</param>
		/// <returns>The absolute path to the file.</returns>
		public override string GetFullPath( Collection collection )
		{
			DirNode dirNode = GetParent( collection );
			if ( dirNode == null )
			{
				throw new DoesNotExistException( String.Format( "Parent relationship property for Node object: {0} - ID: {1} does not exist.", name, id ) );
			}

			return Path.Combine( dirNode.GetFullPath( collection ), name );
		}

		/// <summary>
		/// Gets the DirNode object that represents the parent relationship to this file entry.
		/// </summary>
		/// <param name="collection">Collection that this FileNode object is associated with.</param>
		/// <returns>A DirNode object that represents the parent relationship.</returns>
		public DirNode GetParent( Collection collection )
		{
			Property property = properties.FindSingleValue( PropertyTags.Parent );
			if ( property == null )
			{
				throw new DoesNotExistException( String.Format( "Parent relationship property for Node object: {0} - ID: {1} does not exist.", name, id ) );
			}

			Relationship parent = property.Value as Relationship;
			DirNode dirNode = collection.GetNodeByID( parent.NodeID ) as DirNode;
			if ( dirNode == null )
			{
				throw new DoesNotExistException( String.Format( "Property {0} does not exist for Node object: {1} - ID: {2}.", PropertyTags.Parent, name, id ) );
			}

			return dirNode;
		}

		/// <summary>
		/// Gets the file path relative to the collection root directory.
		/// </summary>
		/// <param name="collection">Collection object that this object belongs to.</param>
		/// <returns>The file path relative to the collection root directory.</returns>
		public string GetRelativePath( Collection collection )
		{
			DirNode dirNode = GetParent( collection );
			if ( dirNode == null )
			{
				throw new DoesNotExistException( String.Format( "Parent relationship property for Node object: {0} - ID: {1} does not exist.", name, id ) );
			}

			return Path.Combine( dirNode.GetRelativePath( collection ), name );
		}
		#endregion
	}
}
