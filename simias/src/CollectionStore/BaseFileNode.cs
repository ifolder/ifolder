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
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;

using Simias.Client;

namespace Simias.Storage
{
	/// <summary>
	/// Represents a generic file object.
	/// </summary>
	[ Serializable ]
	public abstract class BaseFileNode : Node
	{
		#region Class Members
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
		/// Gets or sets the file creation time in the metadata.
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
		/// Gets or sets the file last access time in the metadata.
		/// </summary>
		public DateTime LastAccessTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.LastAccessTime );
				return ( p != null ) ? ( DateTime )p.Value : DateTime.MinValue;
			}

			set { properties.ModifyNodeProperty( PropertyTags.LastAccessTime, value ); }
		}

		/// <summary>
		/// Gets or sets the file last write time in the metadata.
		/// </summary>
		public DateTime LastWriteTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.LastWriteTime );
				return ( p != null ) ? ( DateTime )p.Value : DateTime.MinValue;
			}

			set { properties.ModifyNodeProperty( PropertyTags.LastWriteTime, value ); }
		}

		/// <summary>
		/// Gets or sets the file length in the metadata.
		/// </summary>
		public long Length
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.FileLength );
				return ( p != null ) ? ( long )p.Value : 0;
			}

			set { properties.ModifyNodeProperty( PropertyTags.FileLength, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor used to create a new BaseFileNode object.
		/// </summary>
		/// <param name="collection">Collection that this file entry will be associated with.</param>
		/// <param name="parentPath">Fully qualified path to the parent directory.</param>
		/// <param name="fileName">Friendly name of the file entry.</param>
		/// <param name="fileID">Globally unique identifier for the file entry.</param>
		/// <param name="fileType">Class type to deserialize file entry as.</param>
		internal protected BaseFileNode( Collection collection, string parentPath, string fileName, string fileID, string fileType ) :
			this ( fileName, fileID, fileType )
		{
			// Add the disk file attributes if the file exists.
			if ( UpdateFileInfo( collection, Path.Combine( parentPath, fileName ) ) )
			{
				// If there are metadata collectors registered for this file type, add the extra metadata.
				AddFileMetadata( collection, GetFullPath( collection ) );
			}
		}

		/// <summary>
		/// Constructor used to create a new BaseFileNode object.
		/// </summary>
		/// <param name="stream">Stream object that contains the file data.</param>
		/// <param name="fileName">Friendly name of the file entry.</param>
		/// <param name="fileID">Globally unique identifier for the file entry.</param>
		/// <param name="fileType">Class type to deserialize file entry as.</param>
		internal protected BaseFileNode( Stream stream, string fileName, string fileID, string fileType ) :
			this ( fileName, fileID, fileType )
		{
			// Add the length of the file.
			Length = stream.Length;
		}

		/// <summary>
		/// Constructor used to create a new BaseFileNode object.
		/// </summary>
		/// <param name="fileName">Friendly name of the file entry.</param>
		/// <param name="fileID">Globally unique identifier for the file entry.</param>
		/// <param name="fileType">Class type to deserialize file entry as.</param>
		internal protected BaseFileNode( string fileName, string fileID, string fileType ) :
			base ( fileName, fileID, fileType )
		{
			// Add to the Types list.
			properties.AddNodeProperty( PropertyTags.Types, NodeTypes.BaseFileNodeType );
		}

		/// <summary>
		/// Constructor for creating an existing BaseFileNode object.
		/// </summary>
		/// <param name="node">Node object to create BaseFileNode object from.</param>
		internal protected BaseFileNode( Node node ) :
			base ( node )
		{
		}

		/// <summary>
		/// Constructor for create an existing BaseFileNode object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the ShallowNode belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create new BaseFileNode object from.</param>
		internal protected BaseFileNode( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
		}

		/// <summary>
		/// Constructor for creating an existing BaseFileNode object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create BaseFileNode object from.</param>
		internal protected BaseFileNode( XmlDocument document ) :
			base ( document )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// If file type is a registered metadata type, then the file metadata is collected and automatically
		/// added to this node.
		/// </summary>
		/// <param name="collection">Collection that this file entry will be associated with.</param>
		/// <param name="filePath">Fully qualified path to the file.</param>
		private void AddFileMetadata( Collection collection, string filePath )
		{
			// Get the directory path that this assembly was loaded from.
			string loadDir = Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase );

			// Build a path to the extension mapping file.
			string xmlMetaFile = Path.Combine( loadDir, metadataExtensionFile );
			if ( File.Exists( xmlMetaFile ) )
			{
				// Check if this file is a registered metadata type.
				XmlDocument regExtDoc = new XmlDocument();
				regExtDoc.Load( xmlMetaFile );

				// See if this file has an extension.
				string extension = Path.GetExtension( filePath );
				if (extension != String.Empty )
				{
					// Find out if there is a registered entry for this extension.
					XmlNode metaNode = regExtDoc.DocumentElement.SelectSingleNode( associatedtypeString + "[@" + extensionString + "='" + extension.ToLower() + "']" );
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
									StringDictionary sd = ( StringDictionary )method.Invoke( null, new object[] { filePath } );

									// Walk through each name-value and add it to the parent node.
									foreach ( string s in sd.Keys )
									{
										properties.ModifyProperty( s, sd[ s ] );
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
		/// Gets the file entry name with its extension.
		/// </summary>
		/// <returns>The name of the file including the extension.</returns>
		public abstract string GetFileName();

		/// <summary>
		/// Gets the full path of the file entry.
		/// </summary>
		/// <param name="collection">Collection that this file entry is associated with.</param>
		/// <returns>The absolute path to the file.</returns>
		public abstract string GetFullPath( Collection collection );

		/// <summary>
		/// Updates the file node with file information from the disk file.
		/// </summary>
		/// <param name="collection">Collection that this file entry is associated with.</param>
		/// <returns>True if the file information has changed, otherwise false is returned.</returns>
		public bool UpdateFileInfo( Collection collection )
		{
			return UpdateFileInfo( collection, GetFullPath( collection ) );
		}

		/// <summary>
		/// Updates the file node with file information from the disk file.
		/// </summary>
		/// <param name="collection">Collection that this file entry is associated with.</param>
		/// <param name="fullPath">Absolute path to the disk file.</param>
		/// <returns>True if the file information has changed, otherwise false is returned.</returns>
		public bool UpdateFileInfo( Collection collection, string fullPath )
		{
			bool infoChanged = false;
			if ( File.Exists( fullPath ) )
			{
				// Get the file information for this file and set it as properties in the object.
				FileInfo fi = new FileInfo( fullPath );
				Length = fi.Length;

				// Don't update the creation time if it already exists.
				if ( CreationTime == DateTime.MinValue )
				{
					CreationTime = fi.CreationTime;
				}

				LastAccessTime = fi.LastAccessTime;
				LastWriteTime = fi.LastWriteTime;
				infoChanged = true;
			}

			return infoChanged;
		}
		#endregion
	}
}
