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
		public DateTime CreationTime
		{
			get 
			{ 
				Property p = properties.FindSingleValue( PropertyTags.CreationTime );
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new DoesNotExistException( String.Format( "The property: {0} does not exist on Node object: {1} - ID: {2}.", PropertyTags.CreationTime, name, id ) );
				}
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
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new DoesNotExistException( String.Format( "The property: {0} does not exist on Node object: {1} - ID: {2}.", PropertyTags.LastAccessTime, name, id ) );
				}
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
				if ( p != null )
				{
					return ( DateTime )p.Value;
				}
				else
				{
					throw new DoesNotExistException( String.Format( "The property: {0} does not exist on Node object: {1} - ID: {2}.", PropertyTags.LastWriteTime, name, id ) );
				}
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
				if ( p != null )
				{
					return ( long )p.Value;
				}
				else
				{
					throw new DoesNotExistException( String.Format( "The property: {0} does not exist on Node object: {1} - ID: {2}.", PropertyTags.FileLength, name, id ) );
				}
			}

			set { properties.ModifyNodeProperty( PropertyTags.FileLength, value ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor used to create a new BaseFileNode object.
		/// </summary>
		/// <param name="collection">Collection that this file entry will be associated with.</param>
		/// <param name="fileName">Friendly name of the file entry.</param>
		/// <param name="fileID">Globally unique identifier for the file entry.</param>
		/// <param name="fileType">Class type to deserialize file entry as.</param>
		internal protected BaseFileNode( Collection collection, string fileName, string fileID, string fileType ) :
			base ( fileName, fileID, fileType )
		{
			// Add to the Types list.
			properties.AddNodeProperty( PropertyTags.Types, NodeTypes.BaseFileNodeType );

			// If there are metadata collectors registered for this file type, add the extra metadata.
			AddFileMetadata( collection );
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
		private void AddFileMetadata( Collection collection )
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
				string extension = Path.GetExtension( GetFileName() );
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
									StringDictionary sd = ( StringDictionary )method.Invoke( null, new object[] { GetFullPath( collection ) } );

									// Walk through each name-value and add it to the parent node.
									foreach ( string s in sd.Keys )
									{
										properties.ModifyNodeProperty( s, sd[ s ] );
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
		#endregion
	}
}
