/***********************************************************************
 *  DirectoryEntry.cs - Class that implements manipulation of directories 
 *  that are stored as properties on a Node.
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
	/// Represents an object that references a directory entry in the file system.
	/// </summary>
	public class DirectoryEntry : FileSystemEntry
	{
		#region Properties
		/// <summary>
		/// Gets a value indicating whether the directory exists in the file system.
		/// </summary>
		public override bool Exists
		{
			get { return new DirectoryInfo( FullName ).Exists; }
		}

		/// <summary>
		/// Gets the directory name.
		/// </summary>
		public override string FileName
		{
			get { return new DirectoryInfo( FullName ).Name; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to create a new directory entry object.
		/// </summary>
		/// <param name="node">Node that this directory property will belong to.</param>
		/// <param name="name">Friendly name of the directory property.</param>
		/// <param name="type">Type of directory property.</param>
		/// <param name="relativePath">Path relative to the collection root where this directory exists.</param>
		internal DirectoryEntry( Node node, string name, string type, string relativePath ) :
			base( node, name, type, relativePath )
		{
			// Set the entry type.
			properties.AddNodeProperty( Property.EntryType, Property.DirectoryType );

			// Update the directory properties. Make sure that the directory exists.
			DirectoryInfo dInfo = new DirectoryInfo( FullName );
			if ( dInfo.Exists )
			{
				properties.AddNodeProperty( Property.FileCreationTime, dInfo.CreationTime );
				properties.AddNodeProperty( Property.FileLastAccessTime, dInfo.LastAccessTime );
				properties.AddNodeProperty( Property.FileLastWriteTime, dInfo.LastWriteTime );
			}
		}

		/// <summary>
		/// Constructor used to create a new directory entry object.
		/// </summary>
		/// <param name="node">Node that this directory property will belong to.</param>
		/// <param name="directoryProperty">Property that represents this directory.</param>
		internal DirectoryEntry( Node node, Property directoryProperty ) :
			base( node, directoryProperty )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Deletes the directory entry from the file system (if it exists) and this object.
		/// </summary>
		/// <param name="deleteEntry">Deletes the directory that this property represents 
		/// from the file system if true.</param>
		public override void Delete( bool deleteEntry )
		{
			// Make sure that current user has write rights to this collection.
			if ( !node.CollectionNode.IsAccessAllowed( Access.Rights.ReadWrite ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			if ( deleteEntry && Exists )
			{
				Directory.Delete( FullName, true );
			}

			// Delete this property off of the node.
			entryProperty.DeleteProperty();
		}
		#endregion
	}
}
