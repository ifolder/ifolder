/***********************************************************************
 *  NodeStream.cs - Class that implements the manipulation of common
 *  attributes for files and directories that are stored on a Node.
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
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a stream object that references a file or directory entry in the file system.
	/// </summary>
	[ Obsolete( "This object is marked for removal. Use FileEntry and DirectoryEntry instead.", false ) ]
	public class NodeStream
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

		/// <summary>
		/// Node that this file system entry belongs to.
		/// </summary>
		private Node node;

		/// <summary>
		/// Property on node that represents the stream.
		/// </summary>
		private Property streamProperty = null;

		/// <summary>
		/// Object that contains a list of properties on the node stream object.
		/// </summary>
		private PropertyList properties = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the stream creation time.
		/// </summary>
		public DateTime CreationTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.FileCreationTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}

			set	{ properties.ModifyNodeProperty( Property.FileCreationTime, value ); }
		}

		/// <summary>
		/// Returns the parent directory for the stream.
		/// </summary>
		public string DirectoryName
		{
			get { return Path.GetDirectoryName( FullName ); }
		}

		/// <summary>
		/// Gets a value indicating whether this entry exists in the file system.
		/// </summary>
		public bool Exists
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
		public string FileName
		{
			get { return Path.GetFileName( FullName ); }
		}

		/// <summary>
		/// Gets the full path of the stream.
		/// </summary>
		public string FullName
		{
			get 
			{
				// Get the paths required to create a full path to this object.
				Property rootPath = node.CollectionNode.Properties.GetSingleProperty( Property.DocumentRoot );
				return Path.Combine( ( ( Uri )rootPath.Value ).LocalPath, RelativePath );
			}
		}

		/// <summary>
		/// Gets the stream last access time.
		/// </summary>
		public DateTime LastAccessTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.FileLastAccessTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}

			set { properties.ModifyNodeProperty( Property.FileLastAccessTime, value ); }
		}

		/// <summary>
		/// Gets the stream last write time.
		/// </summary>
		public DateTime LastWriteTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.FileLastWriteTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}

			set { properties.ModifyNodeProperty( Property.FileLastWriteTime, value ); }
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

		/// <summary>
		/// Gets or sets the name of this stream.
		/// </summary>
		public string Name
		{
			get { return streamProperty.XmlProperty[ Property.FileSystemEntryTag ].GetAttribute( Property.NameAttr ); }
			set { streamProperty.XmlProperty[ Property.FileSystemEntryTag ].SetAttribute( Property.NameAttr, value ); node.Properties.SetListChanged(); }
		}

		/// <summary>
		/// Gets or sets the relative path for the file reference by this object.
		/// </summary>
		public string RelativePath
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.DocumentPath ); 
				if ( p != null )
				{
					return ( string )p.Value;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}

			set { properties.ModifyNodeProperty( Property.DocumentPath, value ); }
		}

		/// <summary>
		/// Gets the type of this stream.
		/// </summary>
		public string Type
		{
			get { return streamProperty.XmlProperty[ Property.FileSystemEntryTag ].GetAttribute( Property.TypeAttr ); }
		}

		/// <summary>
		/// Gets the Id of this stream.
		/// </summary>
		public string Id
		{
			get { return streamProperty.XmlProperty[ Property.FileSystemEntryTag ].GetAttribute( Property.IDAttr ); }
		}

		/// <summary>
		/// Gets the list of properties for this node stream object.
		/// </summary>
		public PropertyList Properties
		{
			get { return properties; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor used to create a new node stream object.
		/// </summary>
		/// <param name="node">Node that this stream node will belong to.</param>
		/// <param name="name">Friendly name of the stream node.</param>
		/// <param name="type">Type of stream node.</param>
		/// <param name="relativePath">Path relative to the collection root where this stream exists.</param>
		internal NodeStream( Node node, string name, string type, string relativePath )
		{
			// Make sure that the user has the rights.
			if ( !node.CollectionNode.IsAccessAllowed( Access.Rights.ReadWrite ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			// Save the node that this stream will belong to.
			this.node = node;

			// Create an xml document that will represent this stream.
			XmlDocument streamData = new XmlDocument();
			XmlElement streamRoot = streamData.CreateElement( Property.FileSystemEntryTag );
			streamRoot.SetAttribute( Property.NameAttr, name );
			streamRoot.SetAttribute( Property.IDAttr, Guid.NewGuid().ToString().ToLower() );
			streamRoot.SetAttribute( Property.TypeAttr, type );
			streamData.AppendChild( streamRoot );

			// Set this property on the node.
			streamProperty = new Property( Property.NodeFileSystemEntry, streamData );
			this.node.Properties.AddNodeProperty( streamProperty );

			// Instantiate the property list.
			properties = new PropertyList( node, streamProperty.XmlProperty[ Property.FileSystemEntryTag ] );

			// Add the children properties describing the stream. Make sure the path doesn't begin with a '\'.
			properties.AddNodeProperty( Property.DocumentPath, ( relativePath[ 0 ] == Path.DirectorySeparatorChar ) ? relativePath.Substring( 1 ) : relativePath );

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
		/// Constructor used to create a new node stream object.
		/// </summary>
		/// <param name="node">Node that this stream node will belong to.</param>
		/// <param name="streamProperty">Property that represents this node stream.</param>
		internal NodeStream( Node node, Property streamProperty )
		{
			this.node = node;
			this.streamProperty = streamProperty;
			this.properties = new PropertyList( node, streamProperty.XmlProperty[ Property.FileSystemEntryTag ] );
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
		/// Deletes this node stream object.
		/// </summary>
		public void Delete()
		{
			Delete( false );
		}

		/// <summary>
		/// Deletes the entry from the file system (if it exists) and this object.
		/// </summary>
		/// <param name="deleteDirectory">Deletes the file system entry that this property represents 
		/// from the file system if true.</param>
		public void Delete( bool deleteEntry )
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
			streamProperty.DeleteProperty();
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

		#region IEnumerable Members
		/// <summary>
		/// Class used to enumerate file stream on a node.
		/// </summary>
		internal class NodeStreamEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Node that these streams belongs to.
			/// </summary>
			private Node node;

			/// <summary>
			/// Indicates whether this object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// Enumerator used to enumerate each stream contained by this node.
			/// </summary>
			private ICSEnumerator streamListEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the NodeStreamEnumerator object to enumerate the stream properties.
			/// </summary>
			/// <param name="node">Node that this enumerator belongs to.</param>
			internal NodeStreamEnumerator( Node node )
			{
				this.node = node;
				streamListEnumerator = ( ICSEnumerator )node.Properties.FindValues( Property.NodeFileSystemEntry ).GetEnumerator();
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before
			/// the first element in the collection.
			/// </summary>
			public void Reset()
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				streamListEnumerator.Reset();
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return new NodeStream( node, ( Property )streamListEnumerator.Current );
				}
			}

			/// <summary>
			/// Advances the enumerator to the next element of the collection.
			/// </summary>
			/// <returns>
			/// true if the enumerator was successfully advanced to the next element; 
			/// false if the enumerator has passed the end of the collection.
			/// </returns>
			public bool MoveNext()
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Get the next object in the list.
				return streamListEnumerator.MoveNext();
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Allows for quick release of managed and unmanaged resources.
			/// Called by applications.
			/// </summary>
			public void Dispose()
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}

			/// <summary>
			/// Dispose( bool disposing ) executes in two distinct scenarios.
			/// If disposing equals true, the method has been called directly
			/// or indirectly by a user's code. Managed and unmanaged resources
			/// can be disposed.
			/// If disposing equals false, the method has been called by the 
			/// runtime from inside the finalizer and you should not reference 
			/// other objects. Only unmanaged resources can be disposed.
			/// </summary>
			/// <param name="disposing">Specifies whether called from the finalizer or from the application.</param>
			protected virtual void Dispose( bool disposing )
			{
				// Check to see if Dispose has already been called.
				if ( !disposed )
				{
					// Set disposed here to protect callers from accessing freed members.
					disposed = true;

					// If disposing equals true, dispose all managed and unmanaged resources.
					if ( disposing )
					{
						// Dispose managed resources.
						streamListEnumerator.Dispose();
					}
				}
			}

			/// <summary>
			/// Use C# destructor syntax for finalization code.
			/// This destructor will run only if the Dispose method does not get called.
			/// It gives your base class the opportunity to finalize.
			/// Do not provide destructors in types derived from this class.
			/// </summary>
			~NodeStreamEnumerator()      
			{
				Dispose( false );
			}
			#endregion
		}
		#endregion
	}
}
