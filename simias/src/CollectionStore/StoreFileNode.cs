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

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a file entry that whose location is managed by the Collection Store.
	/// </summary>
	[ Serializable ]
	public class StoreFileNode : BaseFileNode
	{
		#region Class Members
		/// <summary>
		/// Memory stream object used to buffer the StoreFileNode data until it is committed.
		/// </summary>
		[ NonSerialized() ]
		private Stream nodeStream = null;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to create a new StoreFileNode object.
		/// 
		/// Note: The Stream object parameter that is passed to this constructor becomes owned by the
		/// StoreFileNode object and should not be manipulated or closed by the caller.
		/// </summary>
		/// <param name="name">Name of this StoreFileNode object.</param>
		/// <param name="stream">A Stream object where the data can be read.</param>
		public StoreFileNode( string name, Stream stream ) :
			this ( name, Guid.NewGuid().ToString(), stream )
		{
		}

		/// <summary>
		/// Constructor used to create a new StoreFileNode object with a specified ID.
		/// 
		/// Note: The Stream object parameter that is passed to this constructor becomes owned by the
		/// StoreFileNode object and should not be manipulated or closed by the caller.
		/// </summary>
		/// <param name="name">Name of this StoreFileNode object.</param>
		/// <param name="fileID">Globally unique identifier for the StoreFileNode object.</param>
		/// <param name="stream">A Stream object where the data can be read.</param>
		public StoreFileNode( string name, string fileID, Stream stream ) :
			base( stream, name, fileID, NodeTypes.StoreFileNodeType )
		{
			// Hold the stream object until the object is committed.
			nodeStream = stream;
		}

		/// <summary>
		/// Constructor for creating an existing StoreFileNode object.
		/// </summary>
		/// <param name="node">Node object to create StoreFileNode object from.</param>
		public StoreFileNode( Node node ) :
			base ( node )
		{
			if ( type != NodeTypes.StoreFileNodeType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.StoreFileNodeType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing StoreFileNode object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create StoreFileNode object from.</param>
		public StoreFileNode( Collection collection, ShallowNode shallowNode ) :
			base ( collection, shallowNode )
		{
			if ( type != NodeTypes.StoreFileNodeType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.StoreFileNodeType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing StoreFileNode object from an Xml document.
		/// </summary>
		/// <param name="document">Xml document object to create StoreFileNode object from.</param>
		internal StoreFileNode( XmlDocument document ) :
			base ( document )
		{
			if ( type != NodeTypes.StoreFileNodeType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.StoreFileNodeType, type ) );
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// If any stream data is buffer on the object, it is flushed to disk.
		/// </summary>
		/// <param name="collection">Collection that this Node object belongs to.</param>
		internal void FlushStreamData( Collection collection )
		{
			if ( nodeStream != null )
			{
				// Create the managed file path.
				string managedFile = Path.Combine( collection.ManagedPath, id );

				// Create the store managed file.
				using ( FileStream fs = File.Open( managedFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None ) )
				{
					// Copy the data.
					byte[] buffer = new byte[ 64 * 1024 ];
					int bytesRead = nodeStream.Read( buffer, 0, buffer.Length );
					while ( bytesRead > 0 )
					{
						fs.Write( buffer, 0, bytesRead );
						bytesRead = nodeStream.Read( buffer, 0, buffer.Length );
					}
				}

				// Set the creation time, last write time, and length on the node.
				CreationTime = File.GetCreationTime( managedFile );
				LastAccessTime = File.GetLastAccessTime( managedFile );
				LastWriteTime = File.GetLastWriteTime( managedFile );
				Length = nodeStream.Length;

				// Close the source stream.
				nodeStream.Close();
				nodeStream = null;
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
			return id;
		}

		/// <summary>
		/// Gets the full path of the file entry.
		/// </summary>
		/// <param name="collection">Collection that this file entry is associated with.</param>
		/// <returns>The absolute path to the file.</returns>
		public override string GetFullPath( Collection collection )
		{
			return Path.Combine( collection.ManagedPath, id );
		}
		#endregion
	}
}
