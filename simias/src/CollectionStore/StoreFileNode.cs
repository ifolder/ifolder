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

namespace Simias.Storage
{
	/// <summary>
	/// Represents a file entry that whose location is managed by the Collection Store.
	/// </summary>
	public class StoreFileNode : BaseFileNode
	{
		#region Constructor
		/// <summary>
		/// Constructor used to create a new StoreFileNode object.
		/// </summary>
		/// <param name="collection">Collection that this StoreFileNode object will be associated with.</param>
		/// <param name="name">Name of this StoreFileNode object.</param>
		/// <param name="stream">A Stream object where the data can be read.</param>
		public StoreFileNode( Collection collection, string name, Stream stream ) :
			this ( collection, name, Guid.NewGuid().ToString(), stream )
		{
		}

		/// <summary>
		/// Constructor used to create a new StoreFileNode object with a specified ID.
		/// </summary>
		/// <param name="collection">Collection that this StoreFileNode object will be associated with.</param>
		/// <param name="name">Name of this StoreFileNode object.</param>
		/// <param name="fileID">Globally unique identifier for the StoreFileNode object.</param>
		/// <param name="stream">A Stream object where the data can be read.</param>
		public StoreFileNode( Collection collection, string name, string fileID, Stream stream ) :
			base( collection, name, fileID, NodeTypes.StoreFileNodeType )
		{
			// Create the managed file path.
			string managedFile = Path.Combine( collection.ManagedPath, fileID.ToLower() );

			// Create a file in the managed directory and copy the stream into it.
			FileStream fs = File.Open( managedFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None );

			// Copy the stream data.
			byte[] buffer = new byte[ 4096 ];
			int bytesRead = stream.Read( buffer, 0, buffer.Length );

			while ( bytesRead > 0 )
			{
				fs.Write( buffer, 0, bytesRead );
				bytesRead = stream.Read( buffer, 0, buffer.Length );
			}

			// Close the file.
			fs.Close();

			// Update the file properties. Make sure that the file exists.
			FileInfo fInfo = new FileInfo( managedFile );
			if ( fInfo.Exists )
			{
				properties.AddNodeProperty( Property.FileCreationTime, fInfo.CreationTime );
				properties.AddNodeProperty( Property.FileLastAccessTime, fInfo.LastAccessTime );
				properties.AddNodeProperty( Property.FileLastWriteTime, fInfo.LastWriteTime );
				properties.AddNodeProperty( Property.FileLength, fInfo.Length );
			}
		}

		/// <summary>
		/// Constructor for creating an existing StoreFileNode object.
		/// </summary>
		/// <param name="collection">Collection that the Node object belongs to.</param>
		/// <param name="node">Node object to create StoreFileNode object from.</param>
		protected StoreFileNode( Collection collection, Node node ) :
			base ( node )
		{
			if ( !collection.IsType( node, NodeTypes.StoreFileNodeType ) )
			{
				throw new ApplicationException( "Cannot construct object from specified type." );
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
