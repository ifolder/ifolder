/***********************************************************************
 *  FileSystemEntry.cs - Class that implements the manipulation of common
 *  attributes for files and directories that are stored as properties 
 *  on a Node.
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
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Represents an object that references a file or directory entry in the file system.
	/// </summary>
	public abstract class FileSystemEntry
	{
		#region Class Members
		/// <summary>
		/// Node that this file system entry belongs to.
		/// </summary>
		internal protected Node node;

		/// <summary>
		/// Property on node that represents the entry.
		/// </summary>
		internal protected Property entryProperty = null;

		/// <summary>
		/// Object that contains a list of properties on the node stream object.
		/// </summary>
		internal protected PropertyList properties = null;
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
		public abstract bool Exists
		{
			get;
		}

		/// <summary>
		/// Gets the file name with its extension.
		/// </summary>
		public abstract string FileName
		{
			get;
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
		/// Gets whether this entry is a directory.
		/// </summary>
		public bool IsDirectory
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.EntryType );
				if ( p != null )
				{
					return ( p.ToString() == Property.DirectoryType ) ? true : false;
				}
				else
				{
					throw new FileNotFoundException();
				}
			}
		}

		/// <summary>
		/// Gets whether this entry is a file.
		/// </summary>
		public bool IsFile
		{
			get 
			{ 
				Property p = properties.FindSingleValue( Property.EntryType );
				if ( p != null )
				{
					return ( p.ToString() == Property.FileType ) ? true : false;
				}
				else
				{
					throw new FileNotFoundException();
				}
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
		/// Gets or sets the name of this stream.
		/// </summary>
		public string Name
		{
			get { return entryProperty.XmlProperty[ Property.FileSystemEntryTag ].GetAttribute( Property.NameAttr ); }
			set { entryProperty.XmlProperty[ Property.FileSystemEntryTag ].SetAttribute( Property.NameAttr, value ); node.Properties.SetListChanged(); }
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
			get { return entryProperty.XmlProperty[ Property.FileSystemEntryTag ].GetAttribute( Property.TypeAttr ); }
		}

		/// <summary>
		/// Gets the Id of this stream.
		/// </summary>
		public string Id
		{
			get { return entryProperty.XmlProperty[ Property.FileSystemEntryTag ].GetAttribute( Property.IDAttr ); }
		}

		/// <summary>
		/// Gets the list of properties for this node stream object.
		/// </summary>
		public PropertyList Properties
		{
			get { return properties; }
		}

		/// <summary>
		/// Gets the entry property represented by this file system entry.
		/// </summary>
		internal Property EntryProperty
		{
			get { return entryProperty; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor used to create a new file system entry object.
		/// </summary>
		/// <param name="node">Node that this stream node will belong to.</param>
		/// <param name="name">Friendly name of the stream node.</param>
		/// <param name="type">Type of stream node.</param>
		/// <param name="relativePath">Path relative to the collection root where this stream exists.</param>
		internal FileSystemEntry( Node node, string name, string type, string relativePath )
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
			entryProperty = new Property( Property.NodeFileSystemEntry, streamData );
			this.node.Properties.AddNodeProperty( entryProperty );

			// Instantiate the property list.
			properties = new PropertyList( node, entryProperty.XmlProperty[ Property.FileSystemEntryTag ] );

			// Add the children properties describing the stream. Make sure the path doesn't begin with a '\'.
			properties.AddNodeProperty( Property.DocumentPath, ( relativePath[ 0 ] == Path.DirectorySeparatorChar ) ? relativePath.Substring( 1 ) : relativePath );
		}

		/// <summary>
		/// Constructor used to create an existing file system entry object.
		/// </summary>
		/// <param name="node">Node that this stream node will belong to.</param>
		/// <param name="entryProperty">Property that represents this node stream.</param>
		internal FileSystemEntry( Node node, Property entryProperty )
		{
			this.node = node;
			this.entryProperty = entryProperty;
			this.properties = new PropertyList( node, entryProperty.XmlProperty[ Property.FileSystemEntryTag ] );
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
		/// <param name="deleteEntry">Deletes the file system entry that this property represents 
		/// from the file system if true.</param>
		public abstract void Delete( bool deleteEntry );
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Class used to enumerate file system entries on a node.
		/// </summary>
		internal class FileSystemEntryEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Node that these entries belong to.
			/// </summary>
			private Node node;

			/// <summary>
			/// Indicates whether this object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// Enumerator used to enumerate each file system entry contained by this node.
			/// </summary>
			private ICSEnumerator entryListEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the FileSystemEntryEnumerator object to enumerate the file system entry properties.
			/// </summary>
			/// <param name="node">Node that this enumerator belongs to.</param>
			internal FileSystemEntryEnumerator( Node node )
			{
				this.node = node;
				entryListEnumerator = ( ICSEnumerator )node.Properties.FindValues( Property.NodeFileSystemEntry, true ).GetEnumerator();
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

				entryListEnumerator.Reset();
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

					// Figure out if the type is a file or directory.
					Property p = ( Property )entryListEnumerator.Current;
					XmlNode entryType = p.XmlProperty.FirstChild.SelectSingleNode( Property.PropertyTag + "[@" + Property.NameAttr + "='" + Property.EntryType + "']" );
					if ( entryType.InnerText == Property.DirectoryType )
					{
						return new DirectoryEntry( node, p );
					}
					else
					{
						return new FileEntry( node, p );
					}
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
				return entryListEnumerator.MoveNext();
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
						entryListEnumerator.Dispose();
					}
				}
			}

			/// <summary>
			/// Use C# destructor syntax for finalization code.
			/// This destructor will run only if the Dispose method does not get called.
			/// It gives your base class the opportunity to finalize.
			/// Do not provide destructors in types derived from this class.
			/// </summary>
			~FileSystemEntryEnumerator()      
			{
				Dispose( false );
			}
			#endregion
		}
		#endregion
	}
}
