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

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a directory entry in a external file system.
	/// </summary>
	[ Serializable ]
	public class DirNode : Node
	{
		#region Class Members
		/// <summary>
		/// This field is only valid when the object is in the PropertyListState.Add. Once it has been committed,
		/// it may no longer be valid.
		/// </summary>
		[ NonSerialized() ]
		private string path = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the directory creation time.
		/// </summary>
		new public DateTime CreationTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.CreationTime );
				return ( p != null ) ? ( DateTime )p.Value : DateTime.MinValue;
			}

			set	{ properties.ModifyNodeProperty( PropertyTags.CreationTime, value ); }
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
			// Set the in-memory path that is only valid until this object has been committed.
			path = Path.Combine( parentNode.GetFullPath( collection ), dirName );

			// Set the relative path for this directory.
			properties.AddNodeProperty( PropertyTags.FileSystemPath, parentNode.GetRelativePath() + "/" + dirName );

			// Set the parent attribute.
			properties.AddNodeProperty( PropertyTags.Parent, new Relationship( collection.ID, parentNode.ID ) );

			// Set the create time for the directory.
			if ( Directory.Exists( path ) )
			{
				properties.AddNodeProperty( PropertyTags.CreationTime, Directory.GetCreationTime( path ) );
			}
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
			// Set the in-memory path that is only valid until this object has been committed.
			path = dirPath;

			// Set the parent attribute.
			properties.AddNodeProperty( PropertyTags.Parent, new Relationship( collection ) );

			// Set the root path property.
			string parentDir = Path.GetDirectoryName( dirPath );
			if ( ( parentDir == null ) || ( parentDir == String.Empty ) )
			{
				parentDir = Convert.ToString( Path.DirectorySeparatorChar );
			}
			else if ( parentDir != Convert.ToString( Path.DirectorySeparatorChar ) )
			{
				// Normalize the path.
				parentDir = new Uri( parentDir ).LocalPath;
			}

			// Set the file system path so that lookup of path to node is easier.
			properties.AddNodeProperty( PropertyTags.FileSystemPath, name );

			// Add the root path.
			Property p = new Property( PropertyTags.Root, parentDir );
			p.LocalProperty = true;
			properties.AddNodeProperty( p );

			// Set the create time for the directory.
			if ( Directory.Exists( path ) )
			{
				properties.AddNodeProperty( PropertyTags.CreationTime, Directory.GetCreationTime( path ) );
			}
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
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.DirNodeType, type ) );
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
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.DirNodeType, type ) );
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
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.DirNodeType, type ) );
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

			// If this dirNode has not been committed yet, we can use the cached path.
			if ( properties.State == PropertyList.PropertyListState.Add )
			{
				fullPath = path;
			}
			else
			{
				// Find the root DirNode for this collection.
				DirNode rootNode = IsRoot ? this : collection.GetRootDirectory();
				if ( rootNode == null )
				{
					throw new DoesNotExistException( "The root DirNode does not exist." );
				}

				// Get the local path from the root dirNode.
				Property localPath = rootNode.Properties.GetSingleProperty( PropertyTags.Root );
				if ( localPath == null )
				{
					throw new DoesNotExistException( String.Format( "The {0} property does not exist.", PropertyTags.Root ) );
				}

				// Normalize the returned path for the proper platform.
				fullPath = Path.Combine( localPath.Value as string, GetRelativePath() ).Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
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
				throw new DoesNotExistException( String.Format( "Parent relationship property for Node object: {0} - ID: {1} does not exist.", name, id ) );
			}

			return parent;
		}

		/// <summary>
		/// Gets the path relative to the collection root directory. This method will always return relative
		/// paths that are normalized using forward slashes as separators.
		/// </summary>
		/// <param name="collection">Collection object that this object belongs to.</param>
		/// <returns>The path relative to the collection root directory.</returns>
		[ Obsolete( "This method should no longer be used. Use GetRelativePath() instead." ) ]
		public string GetRelativePath( Collection collection )
		{
			return GetRelativePath();
		}

		/// <summary>
		/// Gets the path relative to the collection root directory. This method will always return relative
		/// paths that are normalized using forward slashes as separators.
		/// </summary>
		/// <returns>The path relative to the collection root directory.</returns>
		public string GetRelativePath()
		{
			Property p = properties.GetSingleProperty( PropertyTags.FileSystemPath );
			return ( p != null ) ? p.Value as string : null;
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

		/// <summary>
		/// Move the Root node in the file system.
		/// </summary>
		/// <param name="collection">The collection that contains the node.</param>
		/// <param name="newRoot">The new path to the parent container.</param>
		public void MoveRoot( Collection collection, string newRoot)
		{
			// Make sure that this is the root node.
			if (this.IsRoot)
			{
				// Move the directory.
				Property rootP = properties.GetSingleProperty(PropertyTags.Root);
				string oldRoot = rootP.Value.ToString();
				string name = GetRelativePath();
				string oldPath = Path.Combine(oldRoot, name);
				string newPath = Path.Combine(newRoot, name);

				Directory.Move(oldPath, newPath);

				try
				{
					// Now Reset the Paths.
					this.properties.ModifyNodeProperty(PropertyTags.Root, newRoot);
					collection.Commit(this);
				}
				catch (Exception ex)
				{
					// We failed to commit the node.
					// Move the directory back.
					Directory.Move(newPath, oldPath);
					this.properties.ModifyNodeProperty(PropertyTags.Root, oldRoot);
					throw ex;
				}
			}
			else
			{
				// This is not the root node this opertation is invalid.
				throw new InvalidOperationException("Node is not the root node");
			}
		}
		#endregion
	}
}
