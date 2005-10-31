/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2005 Novell, Inc.
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
 *  Author: Brady Anderson <banderso@novell.com>
 *
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;

using Simias;
using Simias.Client;
using Simias.Storage;

namespace Simias.Tags
{
	/// <summary>
	/// Class for creating a tag library in a specified collection.
	/// Nodes in the collection can then be related to tags which at
	/// a later point in time can be queried
	/// such as: give me all nodes in the collection that contain
	/// the tag "Friends".
	/// If an existing tag in the library is deleted, all node relationships
	/// to the tag are deleted as well.
	/// </summary>
	public class Tag : Node
	{
		#region Constructors
		public Tag( string Name ) :
			base ( Name, Guid.NewGuid().ToString(), NodeTypes.TagType )
		{
		}

		public Tag( ShallowNode ShallowTagNode ) :
			base( Store.GetStore().GetCollectionByID( ShallowTagNode.CollectionID ), ShallowTagNode )
		{
		}
		#endregion

		#region Private Methods
		// Private method to remove all relationships that exist in
		// nodes to this tag instance
		private void RemoveTagRelationships( Collection collection )
		{
			ArrayList changeList = null;
			Relationship tagRelationship = new Relationship( collection.ID, this.ID );
			Property p = new Property( "Tag", tagRelationship );
			ICSList results = collection.Search( p, SearchOp.Equal );
			if ( results.Count > 0 )
			{
				changeList = new ArrayList();
				foreach( ShallowNode sn in results )
				{
					Node node = new Node( collection, sn );
					MultiValuedList mvl = node.Properties.GetProperties( "Tag" );
					if ( mvl.Count > 0 )
					{
						foreach( Property property in mvl )
						{
							Relationship relationship = property.Value as Relationship;
							if ( relationship.NodeID == tagRelationship.NodeID )
							{
								property.Delete();
								changeList.Add( node );
								break;
							}
						}
					}
				}

				if ( changeList.Count > 0 )
				{
					collection.Commit( changeList.ToArray( typeof( Node ) ) as Node[] );
				}
			}
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Add a tag to the collection
		/// An exception is thrown if the system cannot add the tag
		/// If the collectionID does not exist a "NotExistException" is thrown
		/// If the tag already exists an "ExistsException" is thrown
		/// </summary>
		public void Add( string collectionID )
		{
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				lock( typeof( Simias.Tags.Tag ) )
				{
					// Does this tag already exist in the store
					if ( collection.GetSingleNodeByName( this.Name ) == null )
					{
						try
						{
							collection.Commit( this );
						}
						catch( Exception e )
						{
							throw e;
						}
					}
					else
					{
						throw new ExistsException( this.Name );
					}
				}
			}
			else
			{
				throw new NotExistException( collectionID );
			}

			return;
		}

		/// <summary>
		/// Bulk method to add tags to a collection.
		/// An exception is raised if the method fails to
		/// add all the tags in the array.
		/// Note: the tag library is locked while the tags are added
		/// </summary>
		/// <returns>none</returns>
		static public void Add( string collectionID, Tag[] tags )
		{
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				Simias.Tags.Tag.Add( collection, tags );
			}
			else
			{
				throw new NotExistException( collectionID );
			}
		}

		/// <summary>
		/// Bulk method to add tags to a collection.
		/// An exception is raised if the method fails to
		/// add all the tags in the array.
		/// Note: the tag library is locked while the tags are added
		/// </summary>
		/// <returns>none</returns>
		static public void Add( Collection collection, Tag[] tags )
		{
			ArrayList addList = new ArrayList();
			lock( typeof( Simias.Tags.Tag ) )
			{
				foreach( Tag tag in tags )
				{
					// Make sure we don't create duplicate tags
					if ( collection.GetSingleNodeByName( tag.Name ) == null )
					{
						addList.Add( tag );
					}
				}

				if ( addList.Count > 0 )
				{
					collection.Commit( addList.ToArray( typeof( Node ) ) as Node[] );
				}
			}
		}

		/// <summary>
		/// Imports an Icon which will graphically represent the tag
		/// </summary>
		/// <param name="FileName">Source Filename</param>
		/// <remarks>
		/// </remarks>
		/// <returns>true if the icon was successfully imported.</returns>
		public bool ImportIcon( string FileName )
		{
			bool	result = false;
			/*
			Stream	srcStream = null;

			try
			{
				srcStream = new FileStream( FileName, FileMode.Open );
				result = this.ImportIcon( srcStream );
				// BUGBUG store the source file name in the store - where it came from
			}
			catch{}
			finally
			{
				if ( srcStream != null )
				{
					srcStream.Close();
				}
			}
			*/

			return( result );
		}

		/// <summary>
		/// Imports an icon from a stream object.
		/// </summary>
		/// <param name="SrcStream">Source Stream</param>
		/// <remarks>
		/// </remarks>
		/// <returns>true if the icon was successfully imported.</returns>
		public bool	ImportIcon( Stream SrcStream )
		{
			bool			finished = false;
			//StoreFileNode	storeFileNode = null;

			/*
			if (this.addressBook != null)
			{
				try
				{
					// See if a photo stream already exists for this contact node.
					// If one is found - delete it
					Property p =
						this.Properties.GetSingleProperty( "Tag.Icon" );
					if ( p != null )
					{
						Simias.Storage.Relationship relationship =
							(Simias.Storage.Relationship) p.Value;

						Node cPhotoNode = this.addressBook.GetNodeByID(relationship.NodeID);
						if (cPhotoNode != null)
						{
							this.addressBook.Delete(cPhotoNode);
							this.addressBook.Commit(cPhotoNode);
						}
					}
				}
				catch{}

				// Create the new node
				try
				{
					sfn =
						new StoreFileNode(Common.photoProperty, srcStream);

					Relationship parentChild = new
						Relationship(
						this.addressBook.ID,
						sfn.ID);

					this.Properties.ModifyProperty(Common.contactToPhoto, parentChild);
					this.addressBook.Commit(sfn);
					this.addressBook.Commit(this);
					finished = true;
				}
				catch{}
			}
			else
			{
				BinaryReader	bReader = null;
				BinaryWriter	bWriter = null;

				// Copy the photo into the cached stream
				try
				{
					// Create the new stream in the file system
					this.photoStream = new MemoryStream();
					bWriter = new BinaryWriter(this.photoStream);

					// Copy the source stream
					bReader = new BinaryReader(srcStream);
					bReader.BaseStream.Position = 0;
					bWriter.BaseStream.Position = 0;

					//bWriter.Write(bReader.BaseStream, 0, bReader.BaseStream.Length);
					
					// BUGBUG better algo for copying
					int i = 0;
					while(true)
					{
						i = bReader.BaseStream.ReadByte();
						if(i == -1)
						{
							break;
						}

						bWriter.BaseStream.WriteByte((byte) i);
					}

					bWriter.BaseStream.Position = 0;
					finished = true;
				}
				catch{}
				finally
				{
					if (bReader != null)
					{
						bReader.Close();
					}

					//if (bWriter != null)
					//{
					//	bWriter.Close();
					//}
				}
			}
			*/

			return finished;
		}

		/// <summary>
		/// Bulk method to remove tags from a collection
		/// where the caller passes in a Collection ID.
		/// An exception is raised if the method fails to
		/// remove all tags in the array.
		/// </summary>
		/// <returns>none</returns>
		static public void Remove( string collectionID, Tag[] tags )
		{
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				Simias.Tags.Tag.Remove( collection, tags );
			}
			else
			{
				throw new NotExistException( collectionID );
			}
		}

		/// <summary>
		/// Bulk method to remove tags from a collection
		/// An exception is raised if the method fails to
		/// remove all tags in the array.
		/// </summary>
		/// <returns>none</returns>
		static public void Remove( Collection collection, Tag[] tags )
		{
			if ( tags.Length > 0 )
			{
				lock( typeof( Simias.Tags.Tag ) )
				{
					collection.Commit( collection.Delete( tags ) );
				}
			}
			else
			{
				throw new NotExistException( "Empty array" );
			}
		}

		/// <summary>
		/// Remove an existing tag from the collection
		/// All nodes that have this tag related will have the
		/// relationship broken before the tag is deleted.
		/// A "NotExistException" is thrown if the collection or tag does not exist
		/// </summary>
		public void Remove( string collectionID )
		{
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				// Remove the "Tag" property from all nodes that are
				// related to this tag
				this.RemoveTagRelationships( collection );

				lock( typeof( Simias.Tags.Tag ) )
				{
					// Does this tag already exist in the store
					Node node = collection.GetSingleNodeByName( this.Name );
					if ( node != null )
					{
						try
						{
							collection.Commit( collection.Delete( node ) );
						}
						catch( Exception e )
						{
							throw e;
						}
					}
					else
					{
						throw new NotExistException( this.Name );
					}
				}
			}
			else
			{
				throw new NotExistException( collectionID );
			}

			return;
		}

		/// <summary>
		/// Tag a node with this tag instance
		/// A "NotExistException" will be thrown if the collectionID is invalid
		/// An "ExistException" will be thrown if the node is already tagged
		/// with this instance.
		/// </summary>
		public void TagNode( string collectionID, Node node )
		{
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				// Add a relationship that will reference the tag.
				Relationship tagRelationship = new Relationship( collection.ID, this.ID );
				MultiValuedList mvl = node.Properties.GetProperties( "Tag" );
				if ( mvl.Count > 0 )
				{
					foreach( Property property in mvl )
					{
						Relationship relationship = property.Value as Relationship;
						if ( relationship.NodeID == tagRelationship.NodeID )
						{
							throw new ExistsException( this.Name );
						}
					}
				}

				node.Properties.AddProperty( "Tag", tagRelationship );
				collection.Commit( node );
			}
			else
			{
				throw new NotExistException( collectionID );
			}
		}

		/// <summary>
		/// Untag a previously tagged node
		/// A "NotExistException" will be thrown if any of the following occurs:
		///   collectionID does not exist
		///   this (tag) has not been previously tagged to the node
		/// </summary>
		public void UntagNode( string collectionID, Node node )
		{
			Collection collection = Store.GetStore().GetCollectionByID( collectionID );
			if ( collection != null )
			{
				// build a relationship for this tag instance
				Relationship tagRelationship = new Relationship( collection.ID, this.ID );

				// Get all "Tag" properties on the node
				MultiValuedList mvl = node.Properties.GetProperties( "Tag" );
				if ( mvl.Count > 0 )
				{
					foreach( Property property in mvl )
					{
						Relationship relationship = property.Value as Relationship;
						if ( relationship.NodeID == tagRelationship.NodeID )
						{
							property.DeleteProperty();
							collection.Commit( node );
							return;
						}
					}

					throw new NotExistException( this.Name );
				}
				else
				{
					throw new NotExistException( this.Name );
				}
			}
			else
			{
				throw new NotExistException( collectionID );
			}
		}
		#endregion
	}
}
