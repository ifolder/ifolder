/***********************************************************************
 *  Collection.cs - Class that implements the containment and access of
 *  properties and nodes that form a collection.
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
using System.IO;
using System.Xml;
using Simias;
using Simias.Event;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// A collection is contained by a Store.  It contains properties and nodes that describe a grouping
	/// of objects (such as files).  A collection cannot be contained in another Collection or within a
	/// node.
	/// </summary>
	public class Collection : Node
	{
		#region Class Members
		/// <summary>
		/// Initial size of the list that keeps track of the dirty nodes.
		/// </summary>
		private const int initialDirtyNodeListSize = 100;

		/// <summary>
		/// Reference to the virtual store object where this collection object is contained.
		/// </summary>
		private Store localStore;

		/// <summary>
		/// Reference to the persistent database object.
		/// </summary>
		private Persist.IProvider dataBase;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the storage provider object for this node.
		/// </summary>
		internal Persist.IProvider StorageProvider
		{
			get { return dataBase; }
		}

		/// <summary>
		/// Gets the local store handle.
		/// </summary>
		public Store LocalStore
		{
			get { return localStore; }
		}

		/// <summary>
		///  Gets the current owner of the collection.
		/// </summary>
		public string Owner
		{
			// An owner property will always exist.
			get { return cNode.accessControl.Owner; }
		}

		/// <summary>
		/// Gets whether current user has owner access rights to this collection.
		/// </summary>
		internal bool HasOwnerAccess
		{
			get { return cNode.accessControl.IsOwnerAccessAllowed(); }
		}

		/// <summary>
		/// Gets or sets whether this collection can be shared.  By default, a collection is always shareable.
		/// The Collection Store cannot prevent an application from sharing a collection even though this property
		/// is set non-shareable.  This property is only meant as a common means to indicate shareability and must
		/// be enforced at a higher layer.
		/// </summary>
		public bool Shareable
		{
			get 
			{
				Property p = Properties.GetSingleProperty( Property.Shareable );
				bool shareable = ( p != null ) ? ( bool )p.Value : true;
				return ( IsAccessAllowed( Access.Rights.Admin ) && shareable && Synchronizeable ) ? true : false;
			}

			set 
			{
				// Only allow the collection owner to set this property.
				if ( !HasOwnerAccess )
				{
					throw new ApplicationException( "Current user is not the collection owner." );
				}

				Properties.ModifyNodeProperty( Property.Shareable, value );
			}
		}

		/// <summary>
		/// Gets or sets whether this collection can be synchronized.  By default, a collection is always synchronizeable.
		/// The Collection Store cannot prevent an application from synchronizing a collection even though this property
		/// is set not synchronizable.  This property is only meant as a common means to indicate synchronizability and must
		/// be enforced at a higher layer.
		/// </summary>
		public bool Synchronizeable
		{
			get 
			{
				Property p = Properties.GetSingleProperty( Property.Syncable );
				return ( p != null ) ? ( bool )p.Value : true;
			}

			set 
			{
				// Only allow the collection owner to set this property.
				if ( !HasOwnerAccess )
				{
					throw new ApplicationException( "Current user is not the collection owner." );
				}

				Properties.ModifyNodeProperty( Property.Syncable, value );
			}
		}

		/// <summary>
		/// Gets the domain name that this collection belongs to.
		/// </summary>
		public string DomainName
		{
			get 
			{
				Property p = Properties.GetSingleProperty( Property.DomainName );
				return ( p != null ) ? p.ToString() : null;
			}
		}

		/// <summary>
		/// Gets and sets the document root where all files belonging to the collection are rooted.  If the document
		/// root is changed, all files belonging to the collection are moved to the new document root in the file system.
		/// </summary>
		public Uri DocumentRoot
		{
			get 
			{
				Property p = Properties.GetSingleProperty( Property.DocumentRoot );
				return ( p != null ) ? ( Uri )p.Value : null;
			}

			set { MoveRoot( value ); }
		}

		/// <summary>
		/// Gets the identity that the current user is known as in the collection's domain.
		/// </summary>
		internal string DomainIdentity
		{
			get { return ( LocalStore.CurrentIdentity == null ) ? LocalStore.CurrentUser : LocalStore.CurrentIdentity.GetDomainUserGuid( DomainName ); }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a new collection object.
		/// </summary>
		/// <param name="localStore">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name that is used by applications to describe the collection.</param>
		/// <param name="id">Globally unique identifier for this collection.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="documentRoot">Path where the collection documents are rooted.</param>
		public Collection( Store localStore, string name, string id, string type, Uri documentRoot ) :
			base( localStore, name, ( id == String.Empty ) ? Guid.NewGuid().ToString() : id, CollectionType + type, false )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the 'this' value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Don't allow another database object to be created.
			if ( ( type == Store.DatabaseType ) && ( localStore.GetDatabaseObject() != null ) )
			{
				throw new ApplicationException( Store.DatabaseType + " already exists." );
			}

			// Don't allow this collection to be created, if one already exist by the same id.
			if ( localStore.GetCollectionById( Id ) != null )
			{
				throw new ApplicationException( "Collection already exists with specified ID." );
			}

			// Initialize my class members.
            this.localStore = localStore;
			this.dataBase = localStore.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this );

			// Set the default access control for this collection.
			cNode.accessControl.SetDefaultAccessControl();
			UpdateAccessControl();

			// If no document root was passed in, use the default one.
			if ( documentRoot == null )
			{
				documentRoot = GetStoreManagedPath();
			}

			// If the document root directory does not exist, create it.
			if ( !Directory.Exists( documentRoot.LocalPath ) )
			{
				Directory.CreateDirectory( documentRoot.LocalPath );
			}

			// Set the default properties for this node.
			Properties.AddNodeProperty( Property.CreationTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.ModifyTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.CollectionID, Id );
			Properties.AddNodeProperty( Property.IDPath, "/" + Id );
			Properties.AddNodeProperty( Property.DomainName, LocalStore.DomainName );

			// Add the document root as a local property.
			Property docRootProp = new Property( Property.DocumentRoot, documentRoot );
			docRootProp.LocalProperty = true;
			Properties.AddNodeProperty( docRootProp );

			// Set the sync versions.
			Property mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
			mvProp.LocalProperty = true;
			Properties.AddNodeProperty( mvProp );

			Property lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
			lvProp.LocalProperty = true;
			Properties.AddNodeProperty( lvProp );

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor to create a new collection object.
		/// </summary>
		/// <param name="localStore">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name that is used by applications to describe the collection.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="documentRoot">Path where the collection documents are rooted.</param>
		public Collection( Store localStore, string name, string type, Uri documentRoot ) :
			this( localStore, name, String.Empty, type, documentRoot )
		{
		}

		/// <summary>
		/// Constructor to create a new collection object that contains store-managed files.
		/// </summary>
		/// <param name="localStore">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name that is used by applications to describe the collection.</param>
		/// <param name="type">Type of collection.</param>
		public Collection( Store localStore, string name, string type ) :
			this( localStore, name, String.Empty, type, ( Uri )null )
		{
		}

		/// <summary>
		/// Constructor to create an existing collection object.
		/// </summary>
		/// <param name="localStore">Virtual store that this collection belongs to.</param>
		/// <param name="xmlProperties">List of properties that belong to this collection.</param>
		internal Collection( Store localStore, XmlElement xmlProperties ) :
			base( localStore, xmlProperties )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Initialize my class members.
			this.localStore = localStore;
			this.dataBase = localStore.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this );
			UpdateAccessControl();

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor to create an existing collection object without properties.
		/// </summary>
		/// <param name="localStore">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name used by applications to describe the collection.</param>
		/// <param name="id">Globally unique identifier for this collection.</param>
		/// <param name="type">Type of collection.</param>
		internal Collection( Store localStore, string name, string id, string type ) :
			base( localStore, name, id, type, true )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Initialize my class members.
			this.localStore = localStore;
			this.dataBase = localStore.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this );

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor to create an existing collection object without properties with a specified owner.
		/// This constructor is used at store construction time because there is no current owner of the store 
		/// established yet.
		/// </summary>
		/// <param name="localStore">Virtual store that this collection belongs to.</param>
		/// <param name="name">Name used by applications to describe the collection.</param>
		/// <param name="id">Globally unique identifier for this collection.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="constructorId">Identifier of user opening this object.</param>
		internal Collection( Store localStore, string name, string id, string type, string constructorId ) :
			base( localStore, name, id, type, true )
		{
			// Set the collection into the node.  Since node is a sub-class of collection, its 
			// constructor runs before the collection constructor, so we can't set the value.
			InternalCollectionHandle = this;

			// Setup the dirty list.
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Initialize my class members.
			this.localStore = localStore;
			this.dataBase = localStore.StorageProvider;

			// Initialize the access control object.
			cNode.accessControl = new AccessControl( this, constructorId );

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor for creating a collection from a cache node.
		/// </summary>
		/// <param name="store">Object representing the local store.</param>
		/// <param name="cNode">Cache node that contains the node data.</param>
		internal Collection( Store store, CacheNode cNode ) :
			base( cNode )
		{
			// Initialize my class members.
			this.localStore = store;
			this.dataBase = store.StorageProvider;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets a path to where the store managed files for this collection should be created.
		/// </summary>
		/// <returns>A Uri object that represents the store managed path.</returns>
		private Uri GetStoreManagedPath()
		{
			return new Uri( Path.Combine( StorageProvider.StoreDirectory.LocalPath, Path.Combine( localStore.StoreManagedPath.LocalPath, Id ) ) );
		}

		/// <summary>
		/// Moves where the files in the collection are rooted in the filesystem.  This change will automatically commit
		/// the collection node and cannot be rolled back.
		/// </summary>
		/// <param name="newRoot">New location to root collection files.</param>
		private void MoveRoot( Uri newRoot )
		{
			// Make sure the current user has write access to this collection.
			if ( !IsAccessAllowed( Access.Rights.ReadWrite ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			// Move the file system directory where all of the files are contained.
			string sourcePathString = DocumentRoot.LocalPath;

			// Try and move the files to the new directory.
			Directory.Move( sourcePathString, newRoot.LocalPath );

			try
			{
				// Now reset the new document root.
				Properties.ModifyNodeProperty( Property.DocumentRoot, newRoot );
				Commit();
			}
			catch ( Exception e )
			{
				try
				{
					// Attempt to move the files back.
					Directory.Move( newRoot.LocalPath, sourcePathString );

					// Generate event that document root was changed.
					LocalStore.Publisher.RaiseCollectionRootChangedEvent( new CollectionRootChangedEventArgs( LocalStore.ComponentId, Id, DomainName, NameSpaceType, sourcePathString, newRoot.LocalPath ) );
				}
				catch
				{
					// Don't report any errors putting the files back.
					;
				}

				throw e;
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds nodes to a list that need to be written to the persistent store.
		/// </summary>
		/// <param name="dirtyNode">Node to add to the list.</param>
		internal void AddDirtyNodeToList( Node dirtyNode )
		{
			if ( !cNode.dirtyNodeList.ContainsKey( dirtyNode.Id ) && !dirtyNode.IsTombstone )
			{
				cNode.dirtyNodeList.Add( dirtyNode.Id, dirtyNode );
			}
		}

		/// <summary>
		/// Clears out the dirty node list.
		/// </summary>
		internal void ClearDirtyList()
		{
			cNode.dirtyNodeList.Clear();
		}

		/// <summary>
		/// Returns whether the specified node is in the dirty list.
		/// </summary>
		/// <param name="node">The node to check in the list for.</param>
		/// <returns>True if the node is in the list, otherwise false.</returns>
		internal bool IsNodeInDirtyList( Node node )
		{
			return cNode.dirtyNodeList.ContainsKey( node.Id );
		}

		/// <summary>
		/// Removes the specified node from the list.
		/// </summary>
		/// <param name="dirtyNode">Node to remove from the dirtyList.</param>
		internal void RemoveDirtyNodeFromList( Node dirtyNode )
		{
			cNode.dirtyNodeList.Remove( dirtyNode.Id );
		}

		// Updates the access control list from the committed properties.
		internal void UpdateAccessControl()
		{
			cNode.accessControl.GetCommittedAcl();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Changes the owner of the collection and assigns the specified right to the old owner.
		/// Only the current owner can set new ownership on the collection.
		/// </summary>
		/// <param name="newOwnerId">User identifier of the new owner.</param>
		/// <param name="oldOwnerRights">Rights to give the old owner of the collection.</param>
		public void ChangeOwner( string newOwnerId, Access.Rights oldOwnerRights )
		{
			cNode.accessControl.ChangeOwner( newOwnerId.ToLower(), oldOwnerRights );
		}

		/// <summary>
		/// Commits all changes in the collection to persistent storage if deep is set to true.
		/// Otherwise, just commits the collection node.
		/// </summary>
		public void Commit( bool deep )
		{
			// Make sure the current user has write access to this collection.
			if ( !IsAccessAllowed( Access.Rights.ReadWrite ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			if ( deep )
			{
				// Create an XML document that will contain all of the changed nodes.
				XmlDocument commitDoc = new XmlDocument();
				commitDoc.AppendChild( commitDoc.CreateElement( Property.ObjectListTag ) );

				try
				{
					// Acquire the store mutex.
					LocalStore.LockStore();

					// Increment the collection incarnation number here so it gets added to the dirty list and
					// processed with the rest of the changed nodes.
					IncrementLocalIncarnation();

					// Parse the node into an XML document because that is the format that the provider expects.
					foreach ( Node tempNode in cNode.dirtyNodeList.Values )
					{
						// Set the modify time for this node.
						tempNode.Properties.ModifyNodeProperty( "ModifyTime", DateTime.UtcNow );

						// Don't increment the incarnation number on the collection again.
						if ( !tempNode.IsCollection )
						{
							// Increment the local incarnation number.
							tempNode.IncrementLocalIncarnation();
						}

						// Copy the XML node over to the modify document.
						XmlNode xmlNode = commitDoc.ImportNode( tempNode.Properties.PropertyRoot, true );
						commitDoc.DocumentElement.AppendChild( xmlNode );
					}

					// If this collection is new, call to create it before sending down the nodes.
					if ( !IsPersisted )
					{
						dataBase.CreateCollection( Id );
					}

					// Call the store provider to create the records.
					dataBase.CreateRecord( commitDoc.OuterXml, Id );
				}
				finally
				{
					// Release the store mutex.
					LocalStore.UnlockStore();
				}

				// Set all of the nodes in the list as committed.
				foreach ( Node committedNode in cNode.dirtyNodeList.Values )
				{
					// Fire an event for this commit action.
					if ( committedNode.IsPersisted )
					{
						LocalStore.Publisher.RaiseNodeEvent( new NodeEventArgs( LocalStore.ComponentId, committedNode.Id, Id, DomainName, committedNode.NameSpaceType, NodeEventArgs.EventType.Changed, LocalStore.Instance ) );
					}
					else
					{
						LocalStore.Publisher.RaiseNodeEvent( new NodeEventArgs( LocalStore.ComponentId, committedNode.Id, Id, DomainName, committedNode.NameSpaceType, NodeEventArgs.EventType.Created, LocalStore.Instance ) );
					}

					committedNode.IsPersisted = true;
				}

				// Clear the dirty node queue.
				ClearDirtyList();

				// Update the access control list.
				UpdateAccessControl();
			}
			else
			{
				base.Commit();
			}
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.  If there are nodes
		/// subordinate to this collection, an exception will be thrown.
		/// </summary>
		public new void Delete()
		{
			Delete( false );
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.  There is no access check on delete of a
		/// collection.
		/// </summary>
		/// <param name="deep">Indicates whether to all children nodes of this node are deleted also.</param>
		public new void Delete( bool deep )
		{
			// Delete the collection and all of its members if specified.
			base.Delete( deep );

			// If there are store managed files, delete them also.
			Uri documentRoot = GetStoreManagedPath();
			if ( Directory.Exists( documentRoot.LocalPath ) )
			{
				Directory.Delete( documentRoot.LocalPath, true );
			}
		}

		/// <summary>
		/// Gets the access control list for this collection object.
		/// </summary>
		/// <returns>An ICSEnumerator object that will enumerate the access control list.</returns>
		public ICSList GetAccessControlList()
		{
			return new ICSList( new Access( this ) );
		}

		/// <summary>
		/// Gets the access rights for the specified user on the collection.
		/// </summary>
		/// <param name="userId">User ID to get rights for.</param>
		/// <returns>Access rights for the specified user ID.</returns>
		public Access.Rights GetUserAccess( string userId )
		{
			return cNode.accessControl.GetUserRights( userId.ToLower() );
		}

		/// <summary>
		/// Checks whether the current user has sufficient access rights for an operation.
		/// </summary>
		/// <param name="desiredRights">Desired access rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Access.Rights desiredRights )
		{
			return cNode.accessControl.IsAccessAllowed( desiredRights );
		}

		/// <summary>
		/// Removes all access rights on the collection for the specified user.
		/// </summary>
		/// <param name="userId">User ID to remove rights for.</param>
		public void RemoveUserAccess( string userId )
		{
			cNode.accessControl.RemoveUserRights( userId.ToLower() );
		}

		/// <summary>
		/// Rolls back changes made to the last time the collection was committed.  If the a node has never been committed,
		/// it is just removed from the transaction list.
		/// </summary>
		new public void Rollback()
		{
			// Take each node that is currently on the dirty list and roll it back to it's post committed state.
			foreach ( Node tempNode in cNode.dirtyNodeList.Values )
			{
				tempNode.RollbackNode();
			}

			ClearDirtyList();
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="property">Property object containing the value to search for.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( Property property, Property.Operator queryOperator )
		{
			return new ICSList( new NodeEnumerator( this, property, Property.MapQueryOp( queryOperator ) ) );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, object propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, string propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified sbyte property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">sbyte value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, sbyte propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified byte property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">byte value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, byte propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified short property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">short value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, short propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ushort property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ushort value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ushort propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified int properties.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">int value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, int propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified uint property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">uint value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, uint propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified long property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">long value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, long propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ulong property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ulong value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ulong propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified char property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">char value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, char propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified float property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">float value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, float propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified bool property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">bool value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, bool propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified DateTime property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">DateTime value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, DateTime propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified Uri property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Uri value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Uri propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified XmlDocument property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">XmlDocument value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, XmlDocument propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Searches the collection for the specified TimeSpan property.  An enumerator is returned that
		/// returns all nodes that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">TimeSpan value to match.</param>
		/// <param name="queryOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, TimeSpan propertyValue, Property.Operator queryOperator )
		{
			return Search( new Property( propertyName, propertyValue ), queryOperator );
		}

		/// <summary>
		/// Sets the specified access rights for the specified user on the collection.
		/// </summary>
		/// <param name="userId">User to add to the collection's access control list.</param>
		/// <param name="desiredRights">Rights to assign to user.</param>
		public void SetUserAccess( string userId, Access.Rights desiredRights )
		{
			cNode.accessControl.SetUserRights( userId.ToLower(), desiredRights );
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Mandatory method used by clients to enumerate node objects.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>IEnumerator object used to enumerate nodes within collections.</returns>
		public new IEnumerator GetEnumerator()
		{
			return new NodeEnumerator( this, new Property( Property.CollectionID, Id ), Persist.Query.Operator.Equal );
		}
		#endregion
	}
}
