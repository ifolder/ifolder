/***********************************************************************
 *  FileEntry.cs - Class that implements manipulation of files that are
 *  stored as a property on a Node.
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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Represents an object that references a file in the file system.
	/// </summary>
	public class FileEntry : FileSystemEntry
	{
		#region Class Members
		/// <summary>
		/// Default stream buffer size.
		/// </summary>
		private const int streamBufferSize = 4096;

		/// <summary>
		/// Xml file where metadata file types are registered.
		/// </summary>
		private const string metadataExtensionFile = "csmetadata.xml";

		/// <summary>
		/// Well known xml attribute tags.
		/// </summary>
		private const string associatedtypeString = "associatedtype";
		private const string extensionString = "extension";
		private const string assemblyString = "assembly";

		/// <summary>
		/// Name of the method to invoke to gather the file's metadata.
		/// </summary>
		private const string metadataMethodName = "GetMetaData";
		#endregion

		#region Properties
		/// <summary>
		/// Gets a value indicating whether the file exists in the file system.
		/// </summary>
		public override bool Exists
		{
			get { return new FileInfo( FullName ).Exists; }
		}

		/// <summary>
		/// Gets the string representing the file extension.
		/// </summary>
		public string Extension
		{
			get { return Path.GetExtension( FullName ); }
		}

		/// <summary>
		/// Gets the file name with its extension.
		/// </summary>
		public override string FileName
		{
			get { return Path.GetFileName( FullName ); }
		}

		/// <summary>
		/// Gets the size of the file.
		/// </summary>
		public long Length
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.FileLength );
				if ( p != null )
				{
					return ( long )p.Value;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor used to create a new file entry object.
		/// </summary>
		/// <param name="node">Node that this file will belong to.</param>
		/// <param name="name">Friendly name of the file.</param>
		/// <param name="type">Type of file.</param>
		/// <param name="relativePath">Path relative to the collection root where this file exists.</param>
		internal FileEntry( Node node, string name, string type, string relativePath ) :
			base( node, name, type, relativePath )
		{
			// Set the entry type.
			properties.AddNodeProperty( Property.EntryType, Property.FileType );

			// Update the file properties. Make sure that the file exists.
			FileInfo fInfo = new FileInfo( FullName );
			if ( fInfo.Exists )
			{
				properties.AddNodeProperty( Property.FileCreationTime, fInfo.CreationTime );
				properties.AddNodeProperty( Property.FileLastAccessTime, fInfo.LastAccessTime );
				properties.AddNodeProperty( Property.FileLastWriteTime, fInfo.LastWriteTime );
				properties.AddNodeProperty( Property.FileLength, fInfo.Length );

				// If there are metadata collectors registered for this file type, add the extra metadata.
				AddFileMetadata();
			}
		}

		/// <summary>
		/// Constructor used to create a new file entry object.
		/// </summary>
		/// <param name="node">Node that this file will belong to.</param>
		/// <param name="fileProperty">Property that represents this file.</param>
		internal FileEntry( Node node, Property fileProperty ) :
			base( node, fileProperty )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// If file type is a registered metadata type, then the file metadata is collected and automatically
		/// added to this node.
		/// </summary>
		private void AddFileMetadata()
		{
			// loadDir contains the path where this assembly was loaded from.
			string loadDir = node.CollectionNode.LocalStore.AssemblyPath.LocalPath;
			string xmlMetaFile = Path.Combine( loadDir, metadataExtensionFile );
			if ( File.Exists( xmlMetaFile ) )
			{
				// Check if this file is a registered metadata type.
				XmlDocument regExtDoc = new XmlDocument();
				regExtDoc.Load( xmlMetaFile );

				// See if this file has an extension.
				if ( Extension != String.Empty )
				{
					// Find out if there is a registered entry for this extension.
					XmlNode metaNode = regExtDoc.DocumentElement.SelectSingleNode( associatedtypeString + "[@" + extensionString + "='" + Extension.ToLower() + "']" );
					if ( metaNode != null )
					{
						// Load the assembly that knows how to parse the file metadata.
						Assembly metaAssembly = Assembly.LoadFrom( Path.Combine( loadDir, metaNode.Attributes[ assemblyString ].Value ) );
						Type[] types = metaAssembly.GetTypes();
						foreach ( Type type in types )
						{
							// Find the method to invoke to gather the metadata.
							MethodInfo method = type.GetMethod( metadataMethodName, BindingFlags.Static | BindingFlags.Public );
							if ( method != null )
							{
								try
								{
									// Found the method.  Invoke it to return a name-value object that will contain the metadata.
									StringDictionary sd = ( StringDictionary )method.Invoke( null, new object[] { FullName } );

									// Walk through each name-value and add it to the parent node.
									foreach ( string s in sd.Keys )
									{
										node.Properties.ModifyNodeProperty( s, sd[ s ] );
									}
								}
								catch
								{
									// Do nothing.  Just don't let it blow up.
								}

								break;
							}
						}
					}
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Deletes the directory entry from the file system (if it exists) and this object.
		/// </summary>
		/// <param name="deleteEntry">Deletes the file system entry that this property represents 
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
				File.Delete( FullName );
			}

			// Delete this property off of the node.
			entryProperty.DeleteProperty();
		}

		/// <summary>
		/// Initializes a new instance of the FileInfo class with the specified path and creation mode.
		/// </summary>
		/// <param name="mode">A FileMode constant that determines how to open or create the file.</param>
		/// <returns>A Stream object representing the specified file.</returns>
		public Stream Open( FileMode mode )
		{
			return Open( mode, FileAccess.ReadWrite, FileShare.Read, streamBufferSize, false );
		}

		/// <summary>
		/// Initializes a new instance of the FileInfo class with the specified path, creation mode, 
		/// and read/write permission.
		/// </summary>
		/// <param name="mode">A FileMode constant that determines how to open or create the file.</param>
		/// <param name="access">A FileAccess constant that determines how the file can be accessed 
		/// by the FileStream object. This gets the CanRead and CanWrite properties of the FileStream 
		/// object. CanSeek is true if path specifies a disk file. </param>
		/// <returns>A Stream object representing the specified file.</returns>
		public Stream Open( FileMode mode, FileAccess access )
		{
			return Open( mode, access, FileShare.Read, streamBufferSize, false );
		}

		/// <summary>
		/// Initializes a new instance of the FileInfo class with the specified path, creation mode, 
		/// read/write permission, and sharing permission.
		/// </summary>
		/// <param name="mode">A FileMode constant that determines how to open or create the file.</param>
		/// <param name="access">A FileAccess constant that determines how the file can be accessed 
		/// by the FileStream object. This gets the CanRead and CanWrite properties of the FileStream 
		/// object. CanSeek is true if path specifies a disk file. </param>
		/// <param name="share">A FileShare constant that determines how the file will be shared by processes.</param>
		/// <returns>A Stream object representing the specified file.</returns>
		public Stream Open( FileMode mode, FileAccess access, FileShare share )
		{
			return Open( mode, access, share, streamBufferSize, false );
		}

		/// <summary>
		/// Initializes a new instance of the FileInfo class with the specified path, creation mode, 
		/// read/write and sharing permission, and buffer size.
		/// </summary>
		/// <param name="mode">A FileMode constant that determines how to open or create the file.</param>
		/// <param name="access">A FileAccess constant that determines how the file can be accessed 
		/// by the FileStream object. This gets the CanRead and CanWrite properties of the FileStream 
		/// object. CanSeek is true if path specifies a disk file. </param>
		/// <param name="share">A FileShare constant that determines how the file will be shared by processes.</param>
		/// <param name="bufferSize">The desired buffer size in bytes. For bufferSize values between zero 
		/// and eight, the actual buffer size is set to eight bytes. </param>
		/// <returns>A Stream object representing the specified file.</returns>
		public Stream Open( FileMode mode, FileAccess access, FileShare share, int bufferSize )
		{
			return Open( mode, access, share, bufferSize, false );
		}

		/// <summary>
		/// Initializes a new instance of the FileInfo class with the specified path, creation mode, 
		/// read/write and sharing permission, buffer size, and synchronous or asynchronous state.
		/// </summary>
		/// <param name="mode">A FileMode constant that determines how to open or create the file.</param>
		/// <param name="access">A FileAccess constant that determines how the file can be accessed 
		/// by the FileStream object. This gets the CanRead and CanWrite properties of the FileStream 
		/// object. CanSeek is true if path specifies a disk file. </param>
		/// <param name="share">A FileShare constant that determines how the file will be shared by processes.</param>
		/// <param name="bufferSize">The desired buffer size in bytes. For bufferSize values between zero 
		/// and eight, the actual buffer size is set to eight bytes. </param>
		/// <param name="syncState">Specifies whether to use asynchronous I/O or synchronous I/O. However, 
		/// note that the underlying operating system might not support asynchronous I/O, so when 
		/// specifying true, the handle might be opened synchronously depending on the platform.</param>
		/// <returns>A Stream object representing the specified file.</returns>
		public Stream Open( FileMode mode, FileAccess access, FileShare share, int bufferSize, bool syncState )
		{
			return new FileStream( FullName, mode, access, share, bufferSize, syncState );
		}
		#endregion
	}
}
