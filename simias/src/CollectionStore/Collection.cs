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
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Simias;
using Simias.Event;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// A Collection object is contained by a Store object and describes a relationship between the objects
	/// that it contains.
	/// </summary>
	public class Collection : Node, IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Collection ) );

		/// <summary>
		/// Reference to the store.
		/// </summary>
		private Store store;

		/// <summary>
		/// Access control object for this Collection object.
		/// </summary>
		private AccessControl accessControl;

		/// <summary>
		/// Used to do a quick lookup of the domain ID.
		/// </summary>
		string domainID = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the domain that this collection belongs to.
		/// </summary>
		public string Domain
		{
			get 
			{ 
				if ( domainID == null )
				{
					// Only look it up one time.
					domainID = properties.FindSingleValue( PropertyTags.DomainName ).Value as string; 
				}

				return domainID;
			}
		}

		/// <summary>
		/// Gets the directory where store managed files are kept.
		/// </summary>
		public string ManagedPath
		{
			get { return store.GetStoreManagedPath( id ); }
		}

		/// <summary>
		///  Gets the current owner of the collection.
		/// </summary>
		public Member Owner
		{
			get 
			{ 
				Member owner = null;

				// Find the Member object where the Owner tag exists.
				ICSList list = Search( PropertyTags.Owner, Syntax.Boolean );
				foreach ( ShallowNode sn in list )
				{
					owner = new Member( this, sn );
					break;
				}

				return owner;
			}
		}

		/// <summary>
		/// Gets the Store reference for this Collection object.
		/// </summary>
		public Store StoreReference
		{
			get { return store; }
		}

		/// <summary>
		/// Gets or sets whether this collection can be synchronized.  By default, a collection is always
		/// synchronizeable. The Collection Store cannot prevent an application from synchronizing a
		/// collection even though this property is set not synchronizable.  This property is only meant
		/// as a common means to indicate synchronizability and must be enforced at a higher layer.
		/// </summary>
		public bool Synchronizable
		{
			get	{ return properties.HasProperty( PropertyTags.Syncable ) ? false : true; }
			set 
			{ 
				if ( value )
				{
					properties.DeleteSingleNodeProperty( PropertyTags.Syncable );
				}
				else
				{
					properties.ModifyNodeProperty( PropertyTags.Syncable, false ); 
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe the collection.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		public Collection( Store storeObject, string collectionName, string domainID ) :
			this ( storeObject, collectionName, Guid.NewGuid().ToString(), domainID )
		{
		}

		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe
		/// this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		public Collection( Store storeObject, string collectionName, string collectionID, string domainID ) :
			this( storeObject, collectionName, collectionID, NodeTypes.CollectionType, domainID )
		{
		}

		/// <summary>
		/// Constructor for creating an existing Collection object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public Collection( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject.GetCollectionByID( shallowNode.ID ) )
		{
			store = storeObject;
			accessControl = new AccessControl( this );
		}

		/// <summary>
		/// Constructor to create a Collection object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct Collection object from.</param>
		public Collection( Store storeObject, Node node ) :
			base( node )
		{
			if ( !IsType( this, NodeTypes.CollectionType ) )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.CollectionType, type ) );
			}

			store = storeObject;
			accessControl = new AccessControl( this );
		}

		/// <summary>
		/// Copy constructor for Collection object.
		/// </summary>
		/// <param name="collection">Collection object to construct new Collection object from.</param>
		public Collection( Collection collection ) :
			base( collection )
		{
			store = collection.store;
			accessControl = new AccessControl( this );
		}

		/// <summary>
		/// Constructor to create a new Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="collectionName">This is the friendly name that is used by applications to describe this object.</param>
		/// <param name="collectionID">The globally unique identifier for this object.</param>
		/// <param name="collectionType">Base type of collection object.</param>
		/// <param name="domainID">The domain that this object is stored in.</param>
		internal protected Collection( Store storeObject, string collectionName, string collectionID, string collectionType, string domainID ) :
			base( collectionName, collectionID, collectionType )
		{
			store = storeObject;

			// Don't allow this collection to be created, if one already exist by the same id.
			if ( store.GetCollectionByID( id ) != null )
			{
				throw new AlreadyExistsException( String.Format( "The collection: {0} - ID: {1} already exists.", collectionName, collectionID ) );
			}

			// If the managed directory does not exist, create it.
			if ( !Directory.Exists( ManagedPath ) )
			{
				Directory.CreateDirectory( ManagedPath );
			}

			// Add that this is a Collection type if it is specified as a derived type.
			if ( collectionType != NodeTypes.CollectionType )
			{
				properties.AddNodeProperty( PropertyTags.Types, NodeTypes.CollectionType );
			}

			// Add the domain ID as a property.
			properties.AddNodeProperty( PropertyTags.DomainName, domainID );

			// Setup the access control for this collection.
			accessControl = new AccessControl( this );
		}

		/// <summary>
		/// Constructor for creating an existing Collection object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document that describes a Collection object.</param>
		internal protected Collection( Store storeObject, XmlDocument document ) :
			base( document )
		{
			store = storeObject;
			accessControl = new AccessControl( this );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Changes a Node object into a Tombstone object.
		/// </summary>
		/// <param name="node">Node object to change.</param>
		private void ChangeToTombstone( Node node )
		{
			string oldType = node.Type;
			node.Name = "Tombstone:" + node.Name;
			node.BaseType = "Tombstone";
			node.InternalList = new PropertyList( node.Name, node.ID, node.Type );
			node.Properties.AddNodeProperty( PropertyTags.Types, "Tombstone" );
			node.Properties.AddNodeProperty( PropertyTags.TombstoneType, oldType );
			node.IncarnationUpdate = 0;
		}

		/// <summary>
		/// Gets all of the child descendents of the specified Node for the specified relationship.
		/// </summary>
		/// <param name="name">Name of the relationship property.</param>
		/// <param name="relationship">Relationship to use to search for children.</param>
		/// <param name="childList">ArrayList to add Node children objects to.</param>
		private void GetAllDescendants( string name, Relationship relationship, ArrayList childList )
		{
			// Search for all objects that have this object as a relationship.
			ICSList results = Search( name, relationship );
			foreach ( ShallowNode shallowNode in results )
			{
				childList.Add( Node.NodeFactory( this, shallowNode ) );
				GetAllDescendants( name, new Relationship( id, shallowNode.ID ), childList );
			}
		}

		/// <summary>
		/// Increments the local incarnation property.
		///
		/// NOTE: The database must be locked before making this call and must continue to be held until
		/// this node has been committed to disk.
		/// </summary>
		/// <param name="node">Node object that contains the local incarnation value.</param>
		private void IncrementLocalIncarnation( Node node )
		{
			ulong incarnationValue;

			// The master incarnation value only needs to be set during import of a Node object.
			if ( node.Properties.State == PropertyList.PropertyListState.Import )
			{
				// Going to use the passed in value for the incarnation number.
				incarnationValue = node.IncarnationUpdate;

				// Make sure that the expected incarnation value matches the current value.
				Node checkNode = GetNodeByID( node.ID );
				
				// Check if we are importing on the master or slave.
				if ( !node.IsMaster)
				{
					// No collision if:
					//	1. Specifically told to ignore check.
					//	2. Node object does not exist locally.
					//	3. Master incarnation value is zero (first time sync).
					if ( !node.SkipCollisionCheck && ( checkNode != null ) && ( checkNode.MasterIncarnation != 0 ) )
					{
						// Need to check for a collision here. A collision is defined as an update to the client
						// Node object that the server doesn't know about.
						if ( checkNode.LocalIncarnation != checkNode.MasterIncarnation )
						{
							// There was a collision. Strip the local properties back off the Node object
							// before indication the collision. That way when the collision gets stored it
							// won't duplicate the local properties.
							node.Properties.StripLocalProperties();
							throw new CollisionException( checkNode.ID, checkNode.LocalIncarnation );
						}
					}

					// Update the master and local incarnation value to the specified value.
					node.Properties.ModifyNodeProperty( PropertyTags.MasterIncarnation, incarnationValue );

					// Reset the skip collision check value.
					node.SkipCollisionCheck = false;
				}
				else
				{
					// The server is running.
					// No collision if:
					//	1. Node object does not exist locally.
					//	2. Expected incarnation value is equal to the local incarnation value.
					if ( ( checkNode != null ) && ( node.ExpectedIncarnation != checkNode.LocalIncarnation ) )
					{
						// There was a collision. Strip the local properties back off the Node object
						// before indication the collision.
						node.Properties.StripLocalProperties();
						throw new CollisionException( checkNode.ID, checkNode.LocalIncarnation );
					}
				}
			}
			else
			{
				incarnationValue = node.LocalIncarnation + 1;
			}

			// Update the local incarnation value to the specified value.
			node.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, incarnationValue );
		}

		/// <summary>
		/// Returns whether the specified Node object is a deleted Node object type.
		/// </summary>
		/// <param name="node">Node to check to see if it is a deleted Node object.</param>
		/// <returns>True if the specified Node object is a deleted Node object. Otherwise false.</returns>
		private bool IsTombstone( Node node )
		{
			return ( node.Type == "Tombstone" ) ? true : false;
		}

		/// <summary>
		/// Merges all property changes on the current node with the current object in the database.
		///
		/// Note: The database lock must be acquired before making this call.
		/// </summary>
		/// <param name="node">Existing node that may or may not contain changed properties.</param>
		/// <param name="onlyLocalChanges">Is set to true if only local property changes have been made on the Node object.</param>
		/// <returns>A node that contains the current object from the database with all of the property
		/// changes of the current node.</returns>
		private Node MergeNodeProperties( Node node, out bool onlyLocalChanges )
		{
			// Default the value.
			onlyLocalChanges = true;

			// Get this node from the database.
			Node mergedNode = GetNodeByID( node.ID );
			if ( mergedNode != null )
			{
				// If this node is not a tombstone and the merged node is, then the node has been deleted
				// and delete wins.
				if ( !IsTombstone( node ) && IsTombstone( mergedNode ) )
				{
					mergedNode = null;
				}
				else if ( IsTombstone( node ) && !IsTombstone( mergedNode ) )
				{
					// If this node is a tombstone and the merged node is not, then delete wins again and
					// the merged node will be turned into a tombstone.
					mergedNode = node;
				}
				else
				{
					// Before the merge of properties take place, check if there is a collision property
					// and if it is supposed to be included in the merge on the resulting Node object.
					Property collision = node.Properties.FindSingleValue( PropertyTags.Collision );
					if ( ( collision != null ) && ( node.MergeCollisions == false ) )
					{
						collision.DeleteProperty();
						node.MergeCollisions = true;
					}

					// If this node is a tombstone and the merged node is a tombstone, then merge the changes or
					// if this node is not a tombstone and the merged node is not a tombstone, merge the changes.
					// Walk the merge list and perform the changes specified there to the mergedNode.
					foreach ( Property p in node.Properties.ChangeList )
					{
						// See if this is a local property change.
						if ( !p.LocalProperty )
						{
							onlyLocalChanges = false;
						}

						p.ApplyMergeInformation( mergedNode );
					}
				}
			}

			// Clear the change list.
			node.Properties.ClearChangeList();
			return mergedNode;
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a node has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Collection object in the database.
		/// </summary>
		/// <param name="nodeList">Array of Node objects to commit to the database.</param>
		private void ProcessCommit( Node[] nodeList )
		{
			bool deleteCollection = false;
			Node[] commitList = nodeList;

			// Create an XML document that will contain all of the changed nodes.
			XmlDocument commitDocument = new XmlDocument();
			commitDocument.AppendChild( commitDocument.CreateElement( XmlTags.ObjectListTag ) );

			// Create an XML document that will contain all of the deleted nodes.
			XmlDocument deleteDocument = new XmlDocument();
			deleteDocument.AppendChild( deleteDocument.CreateElement( XmlTags.ObjectListTag ) );

			foreach ( Node node in commitList )
			{
				switch ( node.Properties.State )
				{
					case PropertyList.PropertyListState.Add:
					{
						// Validate this Collection object.
						ValidateNodeForCommit( node );

						// Set the modify time for this object.
						node.Properties.ModifyNodeProperty( "ModifyTime", DateTime.UtcNow );

						// Increment the local incarnation number for the object.
						IncrementLocalIncarnation( node );

						// If this is a StoreFileNode, commit the buffered stream to disk.
						if ( IsType( node, NodeTypes.StoreFileNodeType ) )
						{
							( node as StoreFileNode ).FlushStreamData( this );
						}

						// Copy the XML node over to the modify document.
						XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
						commitDocument.DocumentElement.AppendChild( xmlNode );
						break;
					}

					case PropertyList.PropertyListState.Delete:
					{
						if ( IsType( node, NodeTypes.CollectionType ) )
						{
							deleteCollection = true;
						}
						else
						{
							// If this Node object is already a Tombstone, delete it.
							if ( IsTombstone( node ) )
							{
								// Copy the XML node over to the delete document.
								XmlNode xmlNode = deleteDocument.ImportNode( node.Properties.PropertyRoot, true );
								deleteDocument.DocumentElement.AppendChild( xmlNode );
							}
							else
							{
								// If this is a StoreFileNode object, delete the store managed file.
								if ( IsType( node, NodeTypes.StoreFileNodeType ) )
								{
									try
									{
										// Delete the file.
										File.Delete( ( node as StoreFileNode ).GetFullPath( this ) );
									}
									catch {}
								}

								// Convert this Node object to a Tombstone.
								ChangeToTombstone( node );

								// Validate this Collection object.
								ValidateNodeForCommit( node );

								// Set the modify time for this object.
								node.Properties.ModifyNodeProperty( "ModifyTime", DateTime.UtcNow );

								// Increment the local incarnation number for the object.
								IncrementLocalIncarnation( node );

								// Copy the XML node over to the modify document.
								XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
								commitDocument.DocumentElement.AppendChild( xmlNode );
							}
						}
						break;
					}

					case PropertyList.PropertyListState.Update:
					{
						// Make sure that there are changes to the Node object.
						if ( node.Properties.ChangeList.Count != 0 )
						{
							// Merge any changes made to the object on the database before this object's
							// changes are committed.
							bool onlyLocalChanges;
							Node mergeNode = MergeNodeProperties( node, out onlyLocalChanges );
							if ( mergeNode != null )
							{
								// Validate this Collection object.
								ValidateNodeForCommit( mergeNode );

								// Don't bump the incarnation value if only local property changes have
								// been made.
								if ( !onlyLocalChanges )
								{
									// Set the modify time for this object.
									node.Properties.ModifyNodeProperty( "ModifyTime", DateTime.UtcNow );

									// Increment the local incarnation number for the object.
									IncrementLocalIncarnation( mergeNode );
								}

								// Copy the XML node over to the modify document.
								XmlNode xmlNode = commitDocument.ImportNode( mergeNode.Properties.PropertyRoot, true );
								commitDocument.DocumentElement.AppendChild( xmlNode );
							}
						}
						break;
					}

					case PropertyList.PropertyListState.Import:
					{
						// Validate this Collection object.
						ValidateNodeForCommit( node );

						// Copy over the local properties to this Node object which is being imported.
						SetLocalProperties( node );

						// Increment the local incarnation number for the object.
						IncrementLocalIncarnation( node );

						// Copy the XML node over to the modify document.
						XmlNode xmlNode = commitDocument.ImportNode( node.Properties.PropertyRoot, true );
						commitDocument.DocumentElement.AppendChild( xmlNode );
						break;
					}

					case PropertyList.PropertyListState.Internal:
					{
						// Merge any changes made to the object on the database before this object's
						// changes are committed.
						bool onlyLocalChanges;
						Node mergeNode = MergeNodeProperties( node, out onlyLocalChanges );
						if ( mergeNode != null )
						{
							// Copy the XML node over to the modify document.
							XmlNode xmlNode = commitDocument.ImportNode( mergeNode.Properties.PropertyRoot, true );
							commitDocument.DocumentElement.AppendChild( xmlNode );
						}
						break;
					}
				}
			}

			// See if the whole Collection is to be deleted.
			if ( deleteCollection )
			{
				// Delete the collection from the database.
				store.StorageProvider.DeleteContainer( id );

				// If there are store managed files, delete them also.
				if ( Directory.Exists( ManagedPath ) )
				{
					Directory.Delete( ManagedPath, true );
				}
			}
			else
			{
				// Call the store provider to update the records.
				store.StorageProvider.CommitRecords( id, commitDocument, deleteDocument );
			}
			
			// Walk the commit list and change all states to updated.
			foreach( Node node in commitList )
			{
				// If this Node object is a Tombstone that is beinging added, then it came into the commit as
				// an actual node being deleted. Indicate that the object has been deleted. Otherwise do not
				// indicate an event for a Tombstone operation.
				if ( IsType( node, NodeTypes.TombstoneType ) )
				{
					if ( node.Properties.State == PropertyList.PropertyListState.Add )
					{
						string oldType = node.Properties.FindSingleValue( PropertyTags.TombstoneType ).ToString();
						store.EventPublisher.RaiseEvent( new NodeEventArgs( store.Publisher, node.ID, id, oldType, EventType.NodeDeleted, 0 ) );
						node.Properties.State = PropertyList.PropertyListState.Disposed;
					}
				}
				else
				{
					switch ( node.Properties.State )
					{
						case PropertyList.PropertyListState.Add:
							store.EventPublisher.RaiseEvent( new NodeEventArgs( store.Publisher, node.ID, id, node.Type, EventType.NodeCreated, 0 ) );
							node.Properties.State = PropertyList.PropertyListState.Update;
							break;

						case PropertyList.PropertyListState.Delete:
							store.EventPublisher.RaiseEvent( new NodeEventArgs( store.Publisher, node.ID, id, node.Type, EventType.NodeDeleted, 0 ) );
							node.Properties.State = PropertyList.PropertyListState.Disposed;
							break;

						case PropertyList.PropertyListState.Import:
							store.EventPublisher.RaiseEvent( new NodeEventArgs( store.Publisher, node.ID, id, node.Type, EventType.NodeChanged, 0 ) );
							node.Properties.State = PropertyList.PropertyListState.Update;
							break;

						case PropertyList.PropertyListState.Update:
							store.EventPublisher.RaiseEvent( new NodeEventArgs( store.Publisher, node.ID, id, node.Type, EventType.NodeChanged, 0 ) );

							// If this is a member Node, update the access control entry.
							if ( IsType( node, NodeTypes.MemberType ) )
							{
								( node as Member ).UpdateAccessControl();
							}
							break;

						case PropertyList.PropertyListState.Internal:
							node.Properties.State = PropertyList.PropertyListState.Update;

							// If this is a member Node, update the access control entry.
							if ( IsType( node, NodeTypes.MemberType ) )
							{
								( node as Member ).UpdateAccessControl();
							}
							break;
					}
				}
			}
		}

		/// <summary>
		/// Gets the local properties from a Node object in the database and adds them to the specified Node object.
		/// </summary>
		/// <param name="node">Node to copy local properties to.</param>
		private void SetLocalProperties( Node node )
		{
			if ( IsType( node, NodeTypes.CollectionType ) )
			{
				// If the managed directory does not exist, create it.
				Collection importCollection = node as Collection;
				if ( !Directory.Exists( importCollection.ManagedPath ) )
				{
					Directory.CreateDirectory( importCollection.ManagedPath );
				}
			}

			// Get the local properties from the old node, if it exists, and add them to the new node.
			Node oldNode = GetNodeByID( node.ID );
			if ( oldNode != null )
			{
				// Get the local properties.
				MultiValuedList localProps = new MultiValuedList( oldNode.Properties, Property.Local );
				foreach ( Property p in localProps )
				{
					// Don't copy over a collision property.
					if ( p.Name != PropertyTags.Collision )
					{
						node.Properties.AddNodeProperty( p );
					}
				}
			}
			else
			{
				// If there is no existing Node object, this imported Node object needs a MasterIncarnation value.
				Property mvProp = new Property( PropertyTags.MasterIncarnation, ( ulong )0 );
				mvProp.LocalProperty = true;
				node.Properties.AddNodeProperty( mvProp );
			}
		}

		/// <summary>
		/// Validates and performs access checks on a Collection before it is committed.
		/// </summary>
		/// <param name="node">Node object to validate changes for.</param>
		private void ValidateNodeForCommit( Node node )
		{
			// Check if there is a valid collection ID property.
			Property property = node.Properties.FindSingleValue( BaseSchema.CollectionId );
			if ( property != null )
			{
				// Verify that this object belongs to this collection.
				if ( property.Value as string != id )
				{
					throw new CollectionStoreException( String.Format( "Node object: {0} - ID: {1} does not belong to collection: {2} - ID: {3}.", node.Name, node.ID, name, id ) );
				}
			}
			else
			{
				// Make sure that this is not a collection object from a different collection.
				if ( IsType( node, NodeTypes.CollectionType ) && ( node.ID != id ) )
				{
					throw new CollectionStoreException( String.Format( "Node object: {0} - ID: {1} does not belong to collection: {2} - ID: {3}.", node.Name, node.ID, name, id ) );
				}

				// Assign the collection id.
				node.Properties.AddNodeProperty( BaseSchema.CollectionId, id );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Aborts non-committed changes to the specified Node object.
		/// </summary>
		/// <param name="node">Node object to abort changes.</param>
		public void Abort( Node node )
		{
			// Save the old PropertyList state.
			PropertyList.PropertyListState oldState = node.Properties.State;

			// Set the current state of the PropertyList object to abort.
			node.Properties.State = PropertyList.PropertyListState.Abort;

			// Walk the merge list and reverse the changes specified there.
			foreach ( Property p in node.Properties.ChangeList )
			{
				p.AbortMergeInformation( node );
			}

			// Get rid of all entries in the change list.
			node.Properties.ClearChangeList();

			// Restore the PropertyList state.
			node.Properties.State = oldState;
		}

		/// <summary>
		/// Changes the owner of the collection and assigns the specified right to the old owner.
		/// Only the current owner can set new ownership on the collection.
		/// </summary>
		/// <param name="newOwner">Member object that is to become the new owner.</param>
		/// <param name="oldOwnerRights">Rights to give the old owner of the collection.</param>
		/// <returns>An array of Nodes which need to be committed to make this operation permanent.</returns>
		public Node[] ChangeOwner( Member newOwner, Access.Rights oldOwnerRights )
		{
			return accessControl.ChangeOwner( newOwner, oldOwnerRights );
		}

		/// <summary>
		/// Collection factory method that constructs a derived Collection object type from the specified 
		/// ShallowNode object.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="shallowNode">ShallowNode object to construct new Collection object from.</param>
		/// <returns>Downcasts the derived Collection object back to a Collection that can then be 
		/// explicitly casted back up.</returns>
		static public Collection CollectionFactory( Store store, ShallowNode shallowNode )
		{
			Collection rCollection = null;
			switch ( shallowNode.Type )
			{
				case "Collection":
					rCollection = new Collection( store, shallowNode );
					break;

				case "LocalDatabase":
					rCollection = new LocalDatabase( store, shallowNode );
					break;

				default:
					throw new CollectionStoreException( "An unknown type: " + shallowNode.Type + " was specified." );
			}

			return rCollection;
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a Node object has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Node object in the database.
		/// </summary>
		public void Commit()
		{
			Commit( this );
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a node has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Collection object in the database.
		/// </summary>
		/// <param name="node">Node object to commit to the database.</param>
		public void Commit( Node node )
		{
			Node[] nodeList = { node };
			Commit( nodeList );
		}

		/// <summary>
		/// Commits all of the changes made to the Collection object to persistent storage.
		/// After a node has been committed, it will be updated to reflect any new changes that
		/// have occurred if it had to be merged with the current Collection object in the database.
		/// </summary>
		/// <param name="nodeList">An array of Node objects to commit to the database.</param>
		public void Commit( Node[] nodeList )
		{
			// Make sure that something is in the list.
			if ( nodeList.Length > 0 )
			{
				bool createCollection = false;
				bool deleteCollection = false;
				bool hasMembers = false;
				Member collectionOwner = null;

				// Walk the commit list to see if there are any creation and deletion of the collection states.
				foreach( Node node in nodeList )
				{
					if ( IsType( node, NodeTypes.CollectionType ) )
					{
						if ( node.Properties.State == PropertyList.PropertyListState.Delete )
						{
							deleteCollection = true;
						}
						else if ( node.Properties.State == PropertyList.PropertyListState.Add )
						{
							createCollection = true;
						}
					}
					else if ( IsType( node, NodeTypes.MemberType ) )
					{
						// Administrative access needs to be checked because collection membership has changed.
						hasMembers = true;

						// Keep track of any ownership changes.
						if ( ( node as Member ).IsOwner )
						{
							// There can only be a single collection owner. Also make sure that it just isn't
							// the same Node object being committed twice.
							if ( ( collectionOwner != null ) && ( collectionOwner.ID != node.ID ) )
							{
								throw new AlreadyExistsException( String.Format( "Owner {0} - ID: {1} already exists for collection {2} - ID: {3}.", collectionOwner.Name, collectionOwner.ID, name, id ) );
							}

							collectionOwner = node as Member;
						}
					}
				}

				// If the collection is both created and deleted, then there is nothing to do.
				if ( !deleteCollection || !createCollection )
				{
					Node[] commitList;

					// Delete of a collection supercedes all other operations.  It also is not subject to
					// a rights check.
					if ( deleteCollection )
					{
						// Only the collection needs to be processed. All other Node objects will automatically
						// be deleted when the collection is deleted.
						commitList = new Node[ 1 ];
						commitList[ 0 ] = this;
					}
					else if ( createCollection )
					{
						// If there is no collection owner specified, then one needs to be created.
						if ( collectionOwner == null )
						{
							// If a collection is being created, then a Member object containing the owner of the
							// collection needs to be created also.
							commitList = new Node[ nodeList.Length + 1 ];
							nodeList.CopyTo( commitList, 0 );
							commitList[ commitList.Length - 1 ] = accessControl.GetCurrentMember( store, Domain, true );
						}
						else
						{
							// The owner is already specified in the list. Use the list as is.
							commitList = nodeList;
						}
					}
					else
					{
						// Need to get who I am in this collection so that access control can be checked.
						Member member = accessControl.GetCurrentMember( store, Domain, false );

						// If membership is changing on the collection, make sure that the current
						// user has sufficient rights.
						if ( hasMembers )
						{
							if ( !IsAccessAllowed( member, Access.Rights.Admin ) )
							{
								throw new AccessException( this, member, Access.Rights.Admin, String.Format( "User {0} - ID: {1} does not have sufficient rights to change the member list.", member.Name, member.UserID ) );
							}

							// If ownership rights are changing, make sure the current user has sufficient rights.
							if ( collectionOwner != null )
							{
								// Get the current owner of the collection.
								Member currentOwner = Owner;

								// See if ownership is changing and if it is, then the current user has to be
								// the current owner.
								if ( ( collectionOwner.UserID != currentOwner.UserID ) && ( currentOwner.UserID != member.UserID ) )
								{
									throw new AccessException( this, member, String.Format( "User {0} - ID: {1} does not have sufficient rights to change the collection ownership.", member.Name, member.UserID ) );
								}

								// Don't allow the owner's rights to be set below admin level.
								if ( collectionOwner.Rights != Access.Rights.Admin )
								{
									throw new AccessException( this, member, String.Format( "Owner {0} - ID: {1} rights cannot be downgraded.", collectionOwner.Name, collectionOwner.UserID ) );
								}
							}
						}
						else
						{
							// Make sure that current user has write rights to this collection.
							if ( !IsAccessAllowed( member, Access.Rights.ReadWrite ) )
							{
								throw new AccessException( this, member, Access.Rights.ReadWrite, String.Format( "User {0} - ID: {1} does not have sufficient rights to change the collection.", member.Name, member.UserID ) );
							}
						}

						// Use the passed in list.
						commitList = nodeList;
					}

					try
					{
						// Acquire the store lock.
						store.LockStore();
						ProcessCommit( commitList );
					}
					finally
					{
						// Release the store lock.
						store.UnlockStore();
					}
				}

				// Check if the collection was deleted.
				if ( deleteCollection )
				{
					// Go through each entry marking it deleted.
					foreach( Node node in nodeList )
					{
						node.Properties.State = PropertyList.PropertyListState.Disposed;
					}
				}
			}
		}

		/// <summary>
		/// Creates a property on a Node object that represents the collision of the specified Node object 
		/// with another instance.
		/// </summary>
		/// <param name="collisionNode">Node object that has collided with another instance.</param>
		/// <param name="isFileCollision">True if the collision was caused by a file.</param>
		/// <returns>The Node object that the collision was stored on.</returns>
		public Node CreateCollision( Node collisionNode, bool isFileCollision )
		{
			// Look up the Node where the collision occurred.
			Node localNode = GetNodeByID( collisionNode.ID );
			if ( localNode != null )
			{
				// Set the state to update internally.
				localNode.Properties.State = PropertyList.PropertyListState.Internal;

				// See if a collision property already exists.
				Property p = localNode.Properties.GetSingleProperty( PropertyTags.Collision );
				CollisionList cList = ( p == null ) ? new CollisionList() : new CollisionList( p.Value as XmlDocument );

				// Add the new collision to the collision list.
				if ( isFileCollision )
				{
					cList.Modify( new Collision( Collision.CollisionType.File, String.Empty ) );
				}
				else
				{
					cList.Modify( new Collision( Collision.CollisionType.Node, collisionNode.Properties.PropertyDocument.InnerXml ) );
				}

				// Modify or add the collision list.
				p = new Property( PropertyTags.Collision, cList.Document );
				p.LocalProperty = true;
				localNode.Properties.ModifyNodeProperty( p );
			}

			return localNode;
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.
		/// </summary>
		/// <returns>The Node object that has been deleted.</returns>
		public Node Delete()
		{
			return Delete( this );
		}

		/// <summary>
		/// Deletes the specified Node object from the persistent store.
		/// </summary>
		/// <param name="node">Node object to delete.</param>
		/// <returns>The Node object that has been deleted.</returns>
		public Node Delete( Node node )
		{
			Node[] nodeList = Delete( node, null );
			return nodeList[ 0 ];
		}

		/// <summary>
		/// Deletes an array of Node objects from the persistent store.
		/// </summary>
		/// <param name="nodeList">Array of Node objects to delete.</param>
		/// <returns>An array of Node objects that has been deleted.</returns>
		public Node[] Delete( Node[] nodeList )
		{
			foreach ( Node node in nodeList )
			{
				Delete( node, null );
			}

			return nodeList;
		}

		/// <summary>
		/// Deletes the specified collection from the persistent store.
		/// </summary>
		/// <param name="node">Node to delete.</param>
		/// <param name="relationshipName">If not null, indicates to delete all Node objects that have a
		/// descendent relationship to the specified Node object.</param>
		/// <returns>An array of Node objects that have been deleted.</returns>
		public Node[] Delete( Node node, string relationshipName )
		{
			// Temporary holding list.
			ArrayList tempList = new ArrayList();

			// If the node has not been previously committed or is already deleted, don't add it to the list.
			if ( node.Properties.State == PropertyList.PropertyListState.Update )
			{
				tempList.Add( node );
			}

			if ( relationshipName != null )
			{
				// Get all of the decendents of this object.
				GetAllDescendants( relationshipName, new Relationship( id, node.ID ), tempList );
			}

			// Allocate the Node object array and copy over the results.
			foreach( Node n in tempList )
			{
				n.Properties.State = PropertyList.PropertyListState.Delete;
			}

			return tempList.ToArray( typeof( Node ) ) as Node[];
		}

		/// <summary>
		/// Deletes the collision from the specified Node object.
		/// </summary>
		/// <param name="node">Node object from which to delete collision.</param>
		/// <returns>The node object that the collision was deleted from.</returns>
		public Node DeleteCollision( Node node )
		{
			Property p = node.Properties.GetSingleProperty( PropertyTags.Collision );
			if ( p != null )
			{
				p.DeleteProperty();
			}

			return node;
		}

		/// <summary>
		/// Searches all Node objects in the Collection for the specified Types value.
		/// </summary>
		/// <param name="type">String object containing class type to find.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the found Node objects.</returns>
		public ICSList FindType( string type )
		{
			return Search( PropertyTags.Types, type, SearchOp.Equal );
		}

		/// <summary>
		/// Gets a list of ShallowNode objects that represent Node objects that contain collisions in the current
		/// collection.
		/// </summary>
		/// <returns>An ICSList object containing ShallowNode objects representing Node objects that 
		/// contain collisions.</returns>
		public ICSList GetCollisions()
		{
			return Search( PropertyTags.Collision, Syntax.XmlDocument );
		}

		/// <summary>
		/// Gets the Member object that represents the currently executing security context.
		/// </summary>
		/// <returns>A Member object that represents the currently executing security context.</returns>
		public Member GetCurrentMember()
		{
			return accessControl.GetCurrentMember( store, Domain, false );
		}

		/// <summary>
		/// Gets the Member object associated with the specified user ID.
		/// </summary>
		/// <param name="userID">Identifier to look up the Member object with.</param>
		/// <returns>The Member object associated with the specified user ID. May return null if the
		/// Member object does not exist in the collection.</returns>
		public Member GetMember( string userID )
		{
			return accessControl.GetMember( userID.ToLower() );
		}

		/// <summary>
		/// Gets the list of Member objects for this collection object.
		/// </summary>
		/// <returns>An ICSEnumerator object that will enumerate the member list. The ICSList object
		/// will contain ShallowNode objects that represent Member objects.</returns>
		public ICSList GetMemberList()
		{
			return Search( BaseSchema.ObjectType, NodeTypes.MemberType, SearchOp.Equal );
		}

		/// <summary>
		/// Gets a Node object for the specified identifier.
		/// </summary>
		/// <param name="nodeID">Identifier uniquely naming the node.</param>
		/// <returns>Node object for the specified identifier.</returns>
		public Node GetNodeByID( string nodeID )
		{
			Node node = null;

			// Call the provider to get an XML string that represents this node.
			XmlDocument document = store.StorageProvider.GetRecord( nodeID.ToLower(), id );
			if ( document != null )
			{
				// Construct a temporary Node object from the DOM.
				node = Node.NodeFactory( store, document );
			}

			return node;
		}

		/// <summary>
		/// Get all Node objects that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the Node object(s).</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Node object(s)
		/// that that have the specified name.</returns>
		public ICSList GetNodesByName( string name )
		{
			return Search( BaseSchema.ObjectName, name, SearchOp.Equal );
		}

		/// <summary>
		/// Get all Node objects that have the specified type.
		/// </summary>
		/// <param name="typeString">A string containing the type for the Node object(s).</param>
		/// <returns>An ICSList object containing the ShallowNode objects that represent the Node object(s)
		/// that that have the specified type.</returns>
		public ICSList GetNodesByType( string typeString )
		{
			return Search( PropertyTags.Types, typeString , SearchOp.Equal );
		}

		/// <summary>
		/// Gets the Node object that the collision property represents.
		/// </summary>
		/// <param name="node">Node object that contains a collision property.</param>
		/// <returns>The Node object that caused the collision. Otherwise a null is returned.</returns>
		public Node GetNodeFromCollision( Node node )
		{
			Node collisionNode = null;

			// Get the collision property.
			Property p = node.Properties.GetSingleProperty( PropertyTags.Collision );
			if ( p != null )
			{
				// Get a list of collisions.
				ICSEnumerator e = new CollisionList( p.Value as XmlDocument ).GetEnumerator() as ICSEnumerator;
				if ( e.MoveNext() )
				{
					Collision c = e.Current as Collision;
					if ( c.Type == Collision.CollisionType.Node )
					{
						XmlDocument document = new XmlDocument();
						document.LoadXml( c.ContextData );
						collisionNode = Node.NodeFactory( StoreReference, document );
					}
				}

				e.Dispose();
			}

			return collisionNode;
		}

		/// <summary>
		/// Gets the DirNode object that represents the root directory in the collection.
		/// </summary>
		/// <returns>A DirNode object that represents the root directory in the Collection. A null may
		/// be returned if no root directory has been specified for the Collection.</returns>
		public DirNode GetRootDirectory()
		{
			DirNode rootDir = null;

			ICSList results = Search( PropertyTags.Root, Syntax.Uri );
			foreach ( ShallowNode shallowNode in results )
			{
				rootDir = new DirNode( this, shallowNode );
				break;
			}

			return rootDir;
		}

		/// <summary>
		/// Gets the first Node object that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the Node object.</param>
		/// <returns>The first Node object that matches the specified name.  A null is returned if no
		/// matching Node object is found.</returns>
		public Node GetSingleNodeByName( string name )
		{
			Node node = null;
			ICSList nodeList = GetNodesByName( name );
			foreach( ShallowNode shallowNode in nodeList )
			{
				node = Node.NodeFactory( this, shallowNode );
				break;
			}

			return node;
		}

		/// <summary>
		///  Gets the first Node object that corresponds to the specified type.
		/// </summary>
		/// <param name="typeString">String that contains the type of the node.</param>
		/// <returns>The first Node object that corresponds to the specified node path name.  A null
		/// is returned if no matching Node object is found.</returns>
		public Node GetSingleNodeByType( string typeString )
		{
			Node node = null;
			ICSList nodeList = GetNodesByType( typeString );
			foreach ( ShallowNode shallowNode in nodeList )
			{
				node = Node.NodeFactory( this, shallowNode );
				break;
			}

			return node;
		}

		/// <summary>
		/// Returns whether the collection has collisions.
		/// </summary>
		/// <returns>True if the collection contains collisions, otherwise false is returned.</returns>
		public bool HasCollisions()
		{
			ICSEnumerator e = GetCollisions().GetEnumerator() as ICSEnumerator;
			bool hasCollisions = e.MoveNext();
			e.Dispose();
			return hasCollisions;
		}

		/// <summary>
		/// Returns whether the specified Node object has collisions.
		/// </summary>
		/// <param name="node">Node object to check for collisions.</param>
		/// <returns>True if the Node object contains collisions, otherwise false is returned.</returns>
		public bool HasCollisions( Node node )
		{
			return ( node.Properties.GetSingleProperty( PropertyTags.Collision ) != null ) ? true : false;
		}

		/// <summary>
		/// Impersonates the specified identity, if the user ID is verified.
		/// </summary>
		/// <param name="member">Member object to impersonate.</param>
		public void Impersonate( Member member )
		{
			accessControl.Impersonate( member );
		}

		/// <summary>
		/// Readies a Node object for import into this Collection.
		/// </summary>
		/// <param name="node">Node to import into this Collection.</param>
		/// <param name="isMaster">Indicates whether Node object is being imported on the master or slave store.</param>
		/// <param name="expectedIncarnation">The expected value of the Node object's incarnation number. If
		/// the Node object incarnation value is not equal to the expected value, a collision is the result.</param>
		public void ImportNode( Node node, bool isMaster, ulong expectedIncarnation )
		{
			// Set the current state of the node indicating that it is being imported.
			node.Properties.State = PropertyList.PropertyListState.Import;
			node.ExpectedIncarnation = expectedIncarnation;
			node.IsMaster = isMaster;

			// Strip any local properties that may exist on the Node object.
			node.Properties.StripLocalProperties();
		}

		/// <summary>
		/// Checks whether the specified user has sufficient access rights for an operation.
		/// </summary>
		/// <param name="member">Member object to check access for.</param>
		/// <param name="desiredRights">Desired access rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Member member, Access.Rights desiredRights )
		{
			return accessControl.IsAccessAllowed( member, desiredRights );
		}

		/// <summary>
		/// Gets whether the specified member has sufficient rights to share this collection.
		/// </summary>
		/// <param name="member">Member object contained by this collection.</param>
		public bool IsShareable( Member member )
		{
			return ( member.ValidateAce.Rights == Access.Rights.Admin ) ? true : false;
		}

		/// <summary>
		/// Returns whether specified Node object is the specified type.
		/// </summary>
		/// <param name="node">Node object to check type.</param>
		/// <param name="typeString">Type of Node object.</param>
		/// <returns>True if Node object is the specified type, otherwise false is returned.</returns>
		public bool IsType( Node node, string typeString )
		{
			bool isType = false;
			MultiValuedList mvl = node.Properties.FindValues( PropertyTags.Types );
			foreach( Property p in mvl )
			{
				if ( p.ToString() == typeString )
				{
					isType = true;
					break;
				}
			}

			return isType;
		}

		/// <summary>
		/// Gets a new copy of the Collection object data from the database. All changed Collection object data
		/// will be lost.
		/// </summary>
		/// <returns>The Collection object that was refreshed.</returns>
		public Collection Refresh()
		{
			return Refresh( this ) as Collection;
		}

		/// <summary>
		/// Gets a new copy of the Node object data from the database. All changed Node object data
		/// will be lost.
		/// </summary>
		/// <param name="node">Node object to refresh.</param>
		/// <returns>The Node object that was refreshed.</returns>
		public Node Refresh( Node node )
		{
			// Call the provider to get an XML string that represents this node.
			XmlDocument document = store.StorageProvider.GetRecord( node.ID, id );
			if ( document != null )
			{
				XmlElement element = document.DocumentElement[ XmlTags.ObjectTag ];

				node.Name = element.GetAttribute( XmlTags.NameAttr );
				node.BaseType = element.GetAttribute( XmlTags.TypeAttr );
				node.InternalList = new PropertyList( document );
				node.IncarnationUpdate = 0;
			}

			return node;
		}

		/// <summary>
		/// Removes the specified class type from the Types property.
		/// </summary>
		/// <param name="node">Node object to remove type from.</param>
		/// <param name="type">String object containing class type to remove.</param>
		public void RemoveType( Node node, string type )
		{
			if ( NodeTypes.IsNodeType( type ) )
			{
				throw new InvalidOperationException( "Cannot remove base type of Node object." );
			}

			// Get the multi-valued property and search for the specific value.
			MultiValuedList mvl = node.Properties.GetProperties( PropertyTags.Types );
			foreach ( Property p in mvl )
			{
				if ( p.ToString() == type )
				{
					p.DeleteProperty();
					break;
				}
			}
		}

		/// <summary>
		/// Resolves a collision on the specified Node object.
		/// </summary>
		/// <param name="node">Node object that contains a collision.</param>
		/// <param name="incarnationValue">Remote local incarnation value.</param>
		/// <param name="resolveLocal">If true, the local Node becomes authoritative. Otherwise the 
		/// remote Node object becomes authoritative.</param>
		/// <returns>Returns the authoritative Node object.</returns>
		public Node ResolveCollision( Node node, ulong incarnationValue, bool resolveLocal )
		{
			Node resNode;
			
			if ( resolveLocal )
			{
				resNode = node;
				resNode.Properties.State = PropertyList.PropertyListState.Internal;
				resNode.MergeCollisions = false;
				resNode.Properties.ModifyNodeProperty( PropertyTags.MasterIncarnation, incarnationValue );
				resNode.Properties.ModifyNodeProperty( PropertyTags.LocalIncarnation, incarnationValue + 1 );
				DeleteCollision( resNode );
			}
			else
			{
				resNode = GetNodeFromCollision( node );
				resNode.Properties.State = PropertyList.PropertyListState.Import;
				resNode.SkipCollisionCheck = true;
				resNode.IncarnationUpdate = incarnationValue;
				resNode.ExpectedIncarnation = 0;
			}

			return resNode;
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			accessControl.Revert();
		}

		/// <summary>
		/// Searches the collection for the specified property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Property name to search for.</param>
		/// <param name="propertySyntax">Syntax of property to search for.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Syntax propertySyntax )
		{
			if ( propertySyntax == Syntax.Uri )
			{
				// The Uri object must contain a valid path or it cannot be constructed. Put in a bogus path
				// so that it can be constructed. The value will be ignored in the search.
				return new ICSList( new NodeEnumerator( this, new Property( propertyName, new Uri( Directory.GetCurrentDirectory() ) ), SearchOp.Exists ) );
			}
			else if ( propertySyntax == Syntax.XmlDocument )
			{
				XmlDocument document = new XmlDocument();
				document.LoadXml( "<Dummy/>" );
				return new ICSList( new NodeEnumerator( this, new Property( propertyName, document ), SearchOp.Exists ) );
			}
			else
			{
				return new ICSList( new NodeEnumerator( this, new Property( propertyName, propertySyntax, String.Empty ), SearchOp.Exists ) );
			}
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="property">Property object containing the value to search for.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( Property property, SearchOp searchOperator )
		{
			return new ICSList( new NodeEnumerator( this, property, searchOperator ) );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, object propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, string propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified sbyte property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">sbyte value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, sbyte propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified byte property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">byte value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, byte propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified short property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">short value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, short propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ushort property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ushort value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ushort propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified int properties.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">int value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, int propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified uint property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">uint value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, uint propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified long property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">long value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, long propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified ulong property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">ulong value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, ulong propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified char property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">char value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, char propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified float property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">float value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, float propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified bool property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">bool value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, bool propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified DateTime property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">DateTime value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, DateTime propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified Uri property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Uri value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Uri propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified XmlDocument property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">XmlDocument value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, XmlDocument propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified TimeSpan property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">TimeSpan value to match.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, TimeSpan propertyValue, SearchOp searchOperator )
		{
			return Search( new Property( propertyName, propertyValue ), searchOperator );
		}

		/// <summary>
		/// Searches the collection for the specified Relationship property.  An enumerator is returned that
		/// returns all of the ShallowNode objects that match the query criteria.
		/// </summary>
		/// <param name="propertyName">Name of property.</param>
		/// <param name="propertyValue">Relationship value to match.</param>
		/// <returns>An ICSList object that contains the results of the search.</returns>
		public ICSList Search( string propertyName, Relationship propertyValue )
		{
			// Since GUIDs are unique, only search for the Node object GUID. Don't take the time to compare
			// the Collection object GUID.
			return Search( new Property( propertyName, propertyValue ), SearchOp.Equal );
		}

		/// <summary>
		/// Sets the Types property to the specified class type.
		/// </summary>
		/// <param name="node">Node object to set type on.</param>
		/// <param name="type">String object containing class type to set.</param>
		public void SetType( Node node, string type )
		{
			if ( NodeTypes.IsNodeType( type ) )
			{
				throw new InvalidOperationException( "Cannot set a base type of a Node object." );
			}

			// Set the new type.
			node.Properties.AddNodeProperty( PropertyTags.Types, type );
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Gets an enumerator for all of the Node objects belonging to this collection.
		/// </summary>
		/// <returns>An IEnumerator object.</returns>
		public IEnumerator GetEnumerator()
		{
			return new NodeEnumerator( this, new Property( BaseSchema.CollectionId, id ), SearchOp.Equal );
		}

		/// <summary>
		/// Enumerator class for the node object that allows enumeration of specified node objects
		/// within the collection.
		/// </summary>
		protected class NodeEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// Indicates whether this object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// Collection associated with this search.
			/// </summary>
			private Collection collection;

			/// <summary>
			/// Property containing the data to search for.
			/// </summary>
			private Property property;

			/// <summary>
			/// Type of search operation.
			/// </summary>
			private SearchOp queryOperator;

			/// <summary>
			/// Enumerator used to enumerate each returned item in the chunk enumerator list.
			/// </summary>
			private IEnumerator nodeListEnumerator;

			/// <summary>
			/// The internal enumerator to use to enumerate all of the child nodes belonging to this node.
			/// </summary>
			private Persist.IResultSet chunkIterator = null;

			/// <summary>
			/// Array where the query results are stored.
			/// </summary>
			private char[] results = new char[ 4096 ];

			/// <summary>
			/// Hashtable used to filter out duplicate nodes returned by the chunkIterator.
			/// </summary>
			private Hashtable filterNodes;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the NodeEnumerator object.
			/// </summary>
			/// <param name="collection">Collection object that this enumerator belongs to.</param>
			/// <param name="property">Property object containing the data to search for.</param>
			/// <param name="queryOperator">Query operator to use when comparing value.</param>
			public NodeEnumerator( Collection collection, Property property, SearchOp queryOperator )
			{
				this.collection = collection;
				this.property = property;
				this.queryOperator = queryOperator;
				Reset();
			}
			#endregion

			#region Private Methods
			/// <summary>
			/// Determines if the specified ID already has been returned to the user during enumeration.
			/// </summary>
			/// <param name="nodeID">Node identifier to check for duplicity.</param>
			/// <returns>True if the ID is a duplicate, otherwise false.</returns>
			private bool IsDuplicate( string nodeID )
			{
				bool keyExists = filterNodes.ContainsKey( nodeID );
				if ( !keyExists )
				{
					filterNodes.Add( nodeID, null );
				}

				return keyExists;
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
					throw new DisposedException( this );
				}

				// Create a new hashtable instance.
				filterNodes = new Hashtable();

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// Create a query object that will return a result set containing the children of this node.
				Persist.Query query = new Persist.Query( collection.id, property.Name, queryOperator, property.ToString(), property.Type );
				chunkIterator = collection.store.StorageProvider.Search( query );
				if ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						XmlDocument nodeList = new XmlDocument();
						nodeList.LoadXml( new string( results, 0, length ) );
						nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();
					}
					else
					{
						nodeListEnumerator = null;
					}
				}
				else
				{
					nodeListEnumerator = null;
				}
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
						throw new DisposedException( this );
					}

					if ( nodeListEnumerator == null )
					{
						throw new InvalidOperationException( "Empty enumeration" );
					}

					return new ShallowNode( ( XmlElement )nodeListEnumerator.Current );
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
				bool moreData;

				if ( disposed )
				{
					throw new DisposedException( this );
				}

				// Make sure that there is data in the list.
				if ( nodeListEnumerator != null )
				{
					// Prime the loop.
					bool duplicate = true;

					// There is a valid enumerator, assume there is more data.
					moreData = true;

					// Look for the next unique node in the enumeration list.
					while ( moreData && duplicate )
					{
						// See if there is anymore data left in this result set.
						moreData = nodeListEnumerator.MoveNext();
						if ( !moreData )
						{
							// Get the next page of the results set.
							int length = chunkIterator.GetNext( ref results );
							if ( length > 0 )
							{
								// Set up the XML document that we will use as the granular query to the client.
								XmlDocument nodeList = new XmlDocument();
								nodeList.LoadXml( new string( results, 0, length ) );
								nodeListEnumerator = nodeList.DocumentElement.GetEnumerator();

								// Move to the first entry in the document.
								moreData = nodeListEnumerator.MoveNext();
								if ( moreData )
								{
									// Filter out nodes that are duplicates.
									duplicate = IsDuplicate( new ShallowNode( ( XmlElement )nodeListEnumerator.Current ).ID );
								}
								else
								{
									// Out of data.
									nodeListEnumerator = null;
								}
							}
							else
							{
								// Out of data.
								nodeListEnumerator = null;
								moreData = false;
							}
						}
						else
						{
							// Filter out nodes that are duplicates.
							duplicate = IsDuplicate( new ShallowNode( ( XmlElement )nodeListEnumerator.Current ).ID );
						}
					}
				}
				else
				{
					moreData = false;
				}

				return moreData;
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
						if ( chunkIterator != null )
						{
							chunkIterator.Dispose();
						}
					}
				}
			}

			/// <summary>
			/// Use C# destructor syntax for finalization code.
			/// This destructor will run only if the Dispose method does not get called.
			/// It gives your base class the opportunity to finalize.
			/// Do not provide destructors in types derived from this class.
			/// </summary>
			~NodeEnumerator()
			{
				Dispose( false );
			}
			#endregion
		}
		#endregion
	}
}
