/***********************************************************************
 *  Store.cs - Class that represents a database that contains collection
 *  objects, node objects and properties and provides methods for
 *  managing these objects.
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
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Simias;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// This is the top level object for the Collection Store.  The Store object can contain multiple
	/// collection objects.  The Store object is not necessarily a one-to-one mapping with a persistent
	/// store.  Collections within a Store may resided in different persistent stores either locally or
	/// more likely remotely.
	/// </summary>
	public sealed class Store : IEnumerable, IDisposable
	{
		#region Class Members
		/// <summary>
		/// Initial size of the cacheNode hash table.
		/// </summary>
		private const int cacheNodeTableSize = 100;

		/// <summary>
		/// Type of collection that represents the store database.
		/// </summary>
		internal const string DatabaseType = "CsDatabase";

		/// <summary>
		/// Directory where store-managed files are kept.
		/// </summary>
		private const string StoreManagedDirectoryName = "CollectionFiles";

		/// <summary>
		/// Tag used to sign serialized collection nodes.
		/// </summary>
		private const string CollectionSignature = "Novell Serialized Collection";

		/// <summary>
		/// Name of the cross process mutex that serializes access at commit time.
		/// </summary>
		private const string StoreMutexName = "4bf1ab83-1e06-4c64-8831-5f1b9b53f14f";

		/// <summary>
		/// Specifies whether object is viable.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Object that identifies the current owner or impersonator.
		/// </summary>
		private StoreIdentity identity = null;

		/// <summary>
		/// Handle to the local store provider.
		/// </summary>
		private Persist.IProvider storageProvider;

		/// <summary>
		/// Key used to digitally sign serialized collections.
		/// </summary>
		private RSA signatureKey = RSA.Create();

		/// <summary>
		/// Path to where store managed files are kept.
		/// </summary>
		private Uri storeManagedPath;

		/// <summary>
		/// Path where assembly was loaded from.
		/// </summary>
		private Uri assemblyPath;

		/// <summary>
		/// Used to publish store events.
		/// </summary>
		private EventPublisher publisher = new EventPublisher();

		/// <summary>
		/// Mutex used to serialize access to the database during a commit.
		/// </summary>
		private Mutex storeMutex = new Mutex( false, StoreMutexName );

		/// <summary>
		/// Table used to lookup nodes as singleton objects.
		/// </summary>
		private Hashtable cacheNodeTable = new Hashtable( cacheNodeTableSize );
		#endregion

		#region Properties
		/// <summary>
		/// Gets the owner of the store.
		/// </summary>
		public string StoreOwner
		{
			get { return Access.StoreAdminRole; }
		}

		/// <summary>
		/// Gets the current impersonation identity for the store.
		/// </summary>
		public string CurrentUser
		{
			get { return identity.CurrentUserGuid; }
		}

		/// <summary>
		/// Gets whether the current user is the store owner.
		/// </summary>
		public bool IsStoreOwner
		{
			get { return ( StoreOwner == CurrentUser ) ? true : false; }
		}

		/// <summary>
		/// Gets the handle to the persistent store.
		/// </summary>
		internal Persist.IProvider StorageProvider
		{
			get { return storageProvider; }
		}

		/// <summary>
		/// Gets the store managed path.
		/// </summary>
		internal Uri StoreManagedPath
		{
			get { return storeManagedPath; }
		}

		/// <summary>
		/// Gets the directory where this assembly was loaded from.
		/// </summary>
		internal Uri AssemblyPath
		{
			get { return assemblyPath; }
		}

		/// <summary>
		/// Gets the domain name for the current user.
		/// </summary>
		internal string DomainName
		{
			get { return identity.DomainName; }
		}

		/// <summary>
		/// Gets or sets the identity object associated with this store.
		/// </summary>
		internal StoreIdentity LocalIdentity
		{
			get { return identity; }
			set { identity = value; }
		}

		/// <summary>
		///  Specifies where the default store path
		/// </summary>
		public Uri StorePath
		{
			get { return storageProvider.StoreDirectory; }
		}

		/// <summary>
		/// Gets the event publisher for the store.
		/// </summary>
		internal EventPublisher Publisher
		{
			get { return publisher; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the Store object.
		/// </summary>
		/// <param name="databasePath">Path that specifies where to create or open the database. If this parameter
		/// is null, the default database path is used.</param>
		private Store( Uri databasePath )
		{
			bool created;

			// Get the path where the assembly was loaded from.
			assemblyPath = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) );

			if ( databasePath == null )
			{
				storageProvider = Persist.Provider.Connect( out created );
			}
			else
			{
				storageProvider = Persist.Provider.Connect( databasePath.LocalPath, out created );
			}

			// Set the path to the store.
			storeManagedPath = new Uri( Path.Combine( storageProvider.StoreDirectory.LocalPath, StoreManagedDirectoryName ) );

			// Either create the store or authenticate to it.
			if ( created )
			{
				if ( !InitializeStore() )
				{
					// The store didn't initialize delete it.
					storageProvider.DeleteStore();
					storageProvider.Dispose();
					throw new ApplicationException( "Store could not be initialized." );
				}
			}
			else
			{
				AuthenticateStore();
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Authenticates the current user to make sure that it is the same as the store owner.
		/// Throws an exception if the authentication fails.
		/// </summary>
		private void AuthenticateStore()
		{
			StoreIdentity.Authenticate( this );
		}

		/// <summary>
		/// Creates an object in the store that represents the database and sets an owner of the database.
		/// </summary>
		/// <returns>True if database object was successfully created, otherwise false is returned.</returns>
		private bool CreateDatabaseObject()
		{
			bool isCreated;

			try
			{
				// Create an object that represents the database.
				Collection localdb = new Collection( this, "LocalDatabase", DatabaseType );
				localdb.Synchronizeable = false;
				localdb.Commit();
				isCreated = true;
			}
			catch
			{
				isCreated = false;
			}

			return isCreated;
		}

		/// <summary>
		/// Validates the specified xml collection by validating its digital signature.
		/// </summary>
		/// <param name="signedDoc">Digitally signed xml document containing the description for nodes in a collection store.</param>
		/// <returns>An xml document containing the descriptions for nodes in a collection store.</returns>
		private XmlDocument GetValidatedXmlDocument( XmlDocument signedDoc )
		{
			try
			{
				// Create a SignedXml.
				SignedXml signedXml = new SignedXml();

				// Get the signature for this xml document. Loads the "Signature" element.
				signedXml.LoadXml( signedDoc.DocumentElement );

				// Return whether the document is valid.
				if ( !signedXml.CheckSignature() )
				{
					throw new ApplicationException( "Xml document has an invalid signature" );
				}

				// Create a new document so that the returned document can be modified if necessary.
				XmlDocument collectionDoc = new XmlDocument();
				collectionDoc.LoadXml( signedXml.GetIdElement( signedDoc, CollectionSignature ).InnerXml );
				return collectionDoc;
			}
			catch ( CryptographicException e )
			{
				throw new ApplicationException( "Xml document is not valid", e );
			}
		}

		/// <summary>
		/// Initializes the Collection Store the first time the persistent database is created.
		/// </summary>
		/// <returns>True if store was successfully initialized, otherwise false is returned.</returns>
		private bool InitializeStore()
		{
			// Get an identity that represents the current user.  This user will become the database owner.
			StoreIdentity.CreateIdentity( this );

			// Create an object that represents the database.
			bool created = CreateDatabaseObject();
			if ( created )
			{
				if ( !Directory.Exists( storeManagedPath.LocalPath ) )
				{
					Directory.CreateDirectory( storeManagedPath.LocalPath );
				}
			}

			return created;
		}

		/// <summary>
		/// Merges local properties and access control on a node being imported.
		/// </summary>
		/// <param name="collection">Collection where node is being imported into.</param>
		/// <param name="xmlNode">Xml element node that contains collection store node representation.</param>
		/// <returns>Imported node object instantiated from the xml document and merged with local properties.</returns>
		private Node MergeXmlNode( Collection collection, XmlElement xmlNode )
		{
			bool isCollection = false;

			// See if this node is a collection node.
			string nodeId = xmlNode.GetAttribute( Property.IDAttr );
			if ( nodeId == collection.Id )
			{
				// The node should be created as a collection.
				isCollection = true;

				// See if the user has the right to update the access control list.  If he doesn't, use the ACL in the
				// current collection.
				if ( !collection.IsAccessAllowed( Access.Rights.Admin ) )
				{
					// Get the list of access control entries and remove them from the collection object.
					XmlNodeList acl = xmlNode.SelectNodes( Property.PropertyTag + "[@" + Property.NameAttr + "='" + Property.Ace + "']" );
					foreach ( XmlNode ace in acl )
					{
						xmlNode.RemoveChild( ace );
					}

					// Now add in the existing collection aces
					MultiValuedList mvl = collection.Properties.FindValues( Property.Ace, true );
					foreach ( Property p in mvl )
					{
						XmlElement existingAce = xmlNode.OwnerDocument.CreateElement( Property.PropertyTag );

						// These attributes must remain in this order.
						XmlAttribute xmlAttr = xmlNode.OwnerDocument.CreateAttribute( Property.NameAttr );
						xmlAttr.Value = p.Name;
						existingAce.Attributes.Prepend( xmlAttr );

						xmlAttr = xmlNode.OwnerDocument.CreateAttribute( Property.TypeAttr );
						xmlAttr.Value = p.TypeString;
						existingAce.Attributes.InsertAfter( xmlAttr, existingAce.Attributes[ 0 ] );

						xmlAttr = xmlNode.OwnerDocument.CreateAttribute( Property.FlagsAttr );
						xmlAttr.Value = p.PropertyFlags.ToString();
						existingAce.Attributes.Append( xmlAttr );

						existingAce.InnerText = p.ToString();
						xmlNode.AppendChild( existingAce );
					}
				}

				// The user must have owner access in order to set the new owner.
				if ( !collection.HasOwnerAccess )
				{
					// Find the new owner property.
					XmlNode owner = xmlNode.SelectSingleNode( Property.PropertyTag + "[@" + Property.NameAttr + "='" + Property.Owner + "']" );
					if ( owner != null )
					{
						// Replace the new owner property with the existing owner.
						Property p = collection.Properties.GetSingleProperty( Property.Owner );
						owner.InnerText = p.ToString();
					}
					else
					{
						throw new ApplicationException( "No owner specified on collection" );
					}
				}
			}

			// Get the local properties from the old node, if it exists, and add them to the new node.
			Node oldNode = collection.GetNodeById( nodeId ); 
			if ( oldNode != null )
			{
				// Get the local properties.
				MultiValuedList localProps = new MultiValuedList( oldNode.Properties.PropertyRoot, Property.Local );
				foreach ( Property p in localProps )
				{
					xmlNode.AppendChild( xmlNode.OwnerDocument.ImportNode( p.XmlProperty, true ) );
				}

				// Get the local properties off the file system entry nodes and merge them to the new node.
				ICSList entryList = oldNode.GetFileSystemEntryList();
				foreach ( FileSystemEntry fse in entryList )
				{
					// See if there is a corresponding entry in the new list.
					XmlNode newEntryNode = xmlNode.SelectSingleNode( "//" + Property.FileSystemEntryTag + "[@" + Property.IDAttr + "='" + fse.Id + "']" );
					if ( newEntryNode != null )
					{
						// See if there are any local properties.
						localProps = new MultiValuedList( fse.Properties.PropertyRoot, Property.Local );
						foreach ( Property p in localProps )
						{
							newEntryNode.AppendChild( newEntryNode.OwnerDocument.ImportNode( p.XmlProperty, true ) );
						}
					}
				}
			}

			// Create a new node or collection from the updated document.
			if ( isCollection )
			{
				return new Collection( this, xmlNode );
			}
			else
			{
				return new Node( collection, xmlNode, ( oldNode != null ) ? true : false );
			}
		}

		/// <summary>
		/// Digitally signs an xml document so that it can be validated before it is deserialized.
		/// </summary>
		/// <param name="collectionStoreDoc"></param>
		/// <returns>The signed xml document.</returns>
		private XmlDocument SignXmlDocument( XmlDocument collectionStoreDoc )
		{
			try
			{
				// Create the SignedXml object and pass it the XML document.
				SignedXml signedXml = new SignedXml( collectionStoreDoc );
				signedXml.SigningKey = signatureKey;

				// Create a data object to hold the data to sign.
				DataObject dataObject = new DataObject();
				dataObject.Data = collectionStoreDoc.GetElementsByTagName( Property.ObjectListTag );
				dataObject.Id = CollectionSignature;

				// Add the data object to the signature.
				signedXml.AddObject(dataObject);
 
				// Create a reference to be able to package everything into the message.
				Reference reference = new Reference();
				reference.Uri = "#" + CollectionSignature;
 
				// Add the reference to the message.
				signedXml.AddReference( reference );

				// Add a KeyInfo object.
				KeyInfo keyInfo = new KeyInfo();
				keyInfo.AddClause( new RSAKeyValue( signatureKey ) );
				signedXml.KeyInfo = keyInfo;

				// Compute the signature.
				signedXml.ComputeSignature();

				// Save the signature in a new xml document.
				XmlDocument signedDoc = new XmlDocument();
				signedDoc.AppendChild( signedDoc.ImportNode( signedXml.GetXml(), true ) );
				return signedDoc;
			}
			catch( CryptographicException e )
			{
				throw new ApplicationException( "Failed to sign xml document", e );
			}
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Creates a collection from an XML document that doesn't have any instantiated property information.
		/// </summary>
		/// <param name="xmlNode">The XML node that describes this node.</param>
		/// <returns>Collection object</returns>
		internal Collection GetShallowCollection( XmlNode xmlNode )
		{
			// Create a new collection object from the DOM.
			return new Collection( this, xmlNode.Attributes[ Property.NameAttr ].Value, xmlNode.Attributes[ Property.IDAttr ].Value, xmlNode.Attributes[ Property.TypeAttr ].Value );
		}

		/// <summary>
		/// Acquires the store mutex protecting the database against simultaneous commits.
		/// </summary>
		internal void LockStore()
		{
			storeMutex.WaitOne();
		}

		/// <summary>
		/// Gets the cache node object in the hashtable if one exists.
		/// </summary>
		/// <param name="key">Identifier for the cache node object.</param>
		/// <returns>A cache node object if one exists, otherwise a null.</returns>
		internal CacheNode GetCacheNode( string key )
		{
			lock ( this )
			{
				CacheNode cNode = ( CacheNode )cacheNodeTable[ key ];
				if ( cNode != null )
				{
					Interlocked.Increment( ref cNode.referenceCount );
				}

				return cNode;
			}
		}

		/// <summary>
		/// Removes the specified entry from the hashtable.
		/// </summary>
		/// <param name="key">Unique identifier for the cache node object.</param>
		/// <param name="cNode">Object to remove from table.</param>
		/// <param name="force">If true, object is removed from table regardless of the reference count.</param>
		internal void RemoveCacheNode( string key, CacheNode cNode, bool force )
		{
			if ( !disposed )
			{
				lock ( this )
				{
					CacheNode cTempNode = ( CacheNode )cacheNodeTable[ key ];
					if ( ( cTempNode != null ) && cTempNode.Equals( cNode ) && ( force || ( Interlocked.Decrement( ref cTempNode.referenceCount ) == 0 ) ) )
					{
						cacheNodeTable.Remove( key );
					}
				}
			}
		}

		/// <summary>
		/// Adds a cache node object to the hash table.
		/// </summary>
		/// <param name="key">Unique identifier for this cache node object.</param>
		/// <param name="cNode">CacheNode object to add to the table if it does not already exist.</param>
		/// <returns>If a matching entry is found in the table, it is returned, otherwise the specified cache node
		/// object is added to the table and returned.</returns>
		internal CacheNode SetCacheNode( string key, CacheNode cNode )
		{
			lock ( this )
			{
				CacheNode cTempNode = ( CacheNode )cacheNodeTable[ key ];
				if ( cTempNode == null )
				{
					cacheNodeTable.Add( key, cNode );
				}
				else
				{
					// There is already an entry in the table.
					cNode = cTempNode;
				}

				Interlocked.Increment( ref cNode.referenceCount );
				return cNode;
			}
		}

		/// <summary>
		/// Releases the store mutex.
		/// </summary>
		internal void UnlockStore()
		{
			storeMutex.ReleaseMutex();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connects to the persistent store server.  Lets the storage provider decide the path to
		/// where to create or open the database.
		/// </summary>
		///	<returns>An object that represents a connection to the store.</returns>
		public static Store Connect()
		{
			return Connect( null );
		}

		/// <summary>
		/// Connects to the persistent store server.
		/// </summary>
		/// <param name="databasePath">Path that specifies where to create or open the database. If this parameter
		/// is null, the default database path is used.</param>
		///	<returns>An object that represents a connection to the store.</returns>
		public static Store Connect( Uri databasePath )
		{
			return new Store( databasePath );
		}

		/// <summary>
		/// Method to create a collection in the local store.
		/// </summary>
		/// <param name="collectionName">Name the newly created collection will be called.</param>
		/// <param name="type">Type of collection.</param>
		/// <returns>An object to a new collection.</returns>
		public Collection CreateCollection( string collectionName, string type )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			return new Collection( this, collectionName, type );
		}

		/// <summary>
		/// Method to create a collection in the local store.
		/// </summary>
		/// <param name="collectionName">Name the newly created collection will be called.</param>
		/// <returns>An object to a new collection.</returns>
		public Collection CreateCollection( string collectionName )
		{
			return CreateCollection( collectionName, Node.Generic );
		}

		/// <summary>
		/// Method to create a collection in the local store.
		/// </summary>
		/// <param name="collectionName">Name the newly created collection will be called.</param>
		/// <param name="type">Type of collection.</param>
		/// <param name="documentRoot">Absolute path of where files belonging to this collection will be rooted.</param>
		/// <returns>An object to a new collection.</returns>
		public Collection CreateCollection( string collectionName, string type, Uri documentRoot )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			return new Collection( this, collectionName, type, documentRoot );
		}

		/// <summary>
		/// Method to create a collection in the local store.
		/// </summary>
		/// <param name="collectionName">Name the newly created collection will be called.</param>
		/// <param name="documentRoot">Absolute path of where files belonging to this collection will be rooted.</param>
		/// <returns>An object to a new collection.</returns>
		public Collection CreateCollection( string collectionName, Uri documentRoot )
		{
			return CreateCollection( collectionName, Node.Generic, documentRoot );
		}

		/// <summary>
		/// Deletes the persistent store database.
		/// </summary>
		public void Delete()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Check if the current user is the store owner.
			if ( !IsStoreOwner )
			{
				throw new UnauthorizedAccessException( "Current user is not the store owner." );
			}

			// Check if the store managed path still exists. If it does, delete it.
			if ( Directory.Exists( storeManagedPath.LocalPath ) )
			{
				Directory.Delete( storeManagedPath.LocalPath, true );
			}

			// Say bye-bye to the store.
			storageProvider.DeleteStore();
		}

		/// <summary>
		/// Exports the specified collection store node and optionally its children into an xml representation.
		/// </summary>
		/// <param name="collection">Collection that contains the node(s) to export.</param>
		/// <param name="id">Identifier of node or collection to export into an xml document.</param>
		/// <param name="deep">If true, then all children of the specified node will be exported.</param>
		/// <returns>An XmlDocument object that represents the exported node(s).</returns>
		public XmlDocument ExportNodesToXml( Collection collection, string id, bool deep )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Create a new document and add this node to it.
			XmlDocument doc = new XmlDocument();
			XmlElement element = doc.CreateElement( Property.ObjectListTag );
			element.SetAttribute( Property.CollectionID, collection.Id );
			doc.AppendChild( element );

			// Add the node to the document.
			collection.NodeToXml( doc, collection.GetNodeById( id.ToLower() ), deep );

			// Sign the xml document before returning.
			// TEMP: This is commented out until I get a fix on Mono for the XML digital signature stuff.
			// return SignXmlDocument( doc );
			return doc;
			// TEMP
		}

		/// <summary>
		/// Exports the specified collection store node into an xml representation.
		/// </summary>
		/// <param name="collection">Collection that contains the node to export.</param>
		/// <param name="id">Identifier of node or collection to export into an xml document.</param>
		/// <returns>An XmlDocument object that represents the exported node.</returns>
		public XmlDocument ExportSingleNodeToXml( Collection collection, string id )
		{
			return ExportNodesToXml( collection, id, false );
		}

		/// <summary>
		/// Returns a collection object from the data store by its identifier.
		/// </summary>
		/// <param name="collectionId">Unique identifier for the collection.</param>
		/// <returns>Collection object that corresponds to the specified collection ID.  If the collection doesn't exist a null is returned.</returns>
		public Collection GetCollectionById( string collectionId )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			Collection collection = null;

			// Get the specified object from the persistent store.
			string normalizedId = collectionId.ToLower();

			// First check to see if an instance of the data is already in cache.
			CacheNode cNode = GetCacheNode( normalizedId );
			if ( cNode == null )
			{
				string xmlCollection = storageProvider.GetRecord( normalizedId, normalizedId );
				if ( xmlCollection != null )
				{
					// Covert the XML string into a DOM that we can then parse.
					XmlDocument collectionDoc = new XmlDocument();
					collectionDoc.LoadXml( xmlCollection );

					// Create a new collection from the DOM.
					collection = new Collection( this, collectionDoc.DocumentElement[ Property.ObjectTag ] );
				}
			}
			else
			{
				// Just build the collection from the existing data.
				collection = new Collection( this, cNode );
			}

			// Check if this user has sufficient rights to view this collection.
			if ( ( collection != null ) && !collection.IsAccessAllowed( Access.Rights.ReadOnly ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have rights to the collection." );
			}

			return collection;
		}

		/// <summary>
		///  Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s).  This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>An ICSList object containing the collections(s) that have the specified name.</returns>
		public ICSList GetCollectionsByName( string name )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			// Build a regular expression class to use as the comparision.
			Regex searchName = new Regex( "^" + name + "$", RegexOptions.IgnoreCase );

			// Look at each collection that this user has rights to and match up on the name.
			foreach ( Collection c in this )
			{
				if ( searchName.IsMatch( c.Name ) )
				{
					collectionList.Add( c );
				}
			}

			return collectionList;
		}

		/// <summary>
		///  Gets all collections that have the specified type.
		/// </summary>
		/// <param name="type">String that contains the type of the collection.  This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>An ICSList object containing the collection(s) that have the specified type.</returns>
		public ICSList GetCollectionsByType( string type )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			// Build a regular expression class to use as the comparision.
			Regex searchType = new Regex( "^" + type + "$", RegexOptions.IgnoreCase );

			// Look at each collection that this user has rights to and match up on the name.
			foreach ( Collection c in this )
			{
				if ( searchType.IsMatch( c.Type ) )
				{
					collectionList.Add( c );
				}
			}

			return collectionList;
		}

		/// <summary>
		/// Returns the collection that represents the database object.
		/// </summary>
		/// <returns>A collection object that represents a database object.</returns>
		public Collection GetDatabaseObject()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			return GetSingleCollectionByType( DatabaseType );
		}

		/// <summary>
		/// Gets the local address book that contains the identities for the store's domain.
		/// </summary>
		/// <returns>A Collection object that represents the local address book if it exists. 
		/// Otherwise null is returned.</returns>
		public LocalAddressBook GetLocalAddressBook()
		{
			LocalAddressBook localAb = null;

			// See if the local address book already exists.
			ICSList abList = GetCollectionsByType( Property.AddressBookType );
			foreach ( Collection abCollection in abList )
			{
				// The address book must have the local property set on it.
				Property p = abCollection.Properties.GetSingleProperty( Property.LocalAddressBook );
				if ( ( p != null ) && ( ( bool )p.Value == true ) )
				{
					localAb = new LocalAddressBook( this, abCollection );
					break;
				}
			}

			return localAb;
		}

		/// <summary>
		/// Gets the first collection that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first collection object that matches the specified name.  A null is returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByName( string name )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			Collection collection = null;
			ICSList collectionList = GetCollectionsByName( name );
			foreach ( Collection tempCollection in collectionList )
			{
				collection = tempCollection;
				break;
			}

			return collection;
		}

		/// <summary>
		/// Gets the first collection that matches the specified type.
		/// </summary>
		/// <param name="type">A string containing the type for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first collection object that matches the specified type.  A null is returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByType( string type )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			Collection collection = null;
			ICSList collectionList = GetCollectionsByType( type );
			foreach ( Collection tempCollection in collectionList )
			{
				collection = tempCollection;
				break;
			}

			return collection;
		}

		/// <summary>
		/// Allows the current thread to run in the specified user's security context.
		/// </summary>
		/// <param name="userGuid">Identifier for the user.</param>
		/// <param name="credential">Credential used to verify the user.</param>
		[ Obsolete( "This method is marked for removal. Use other overload instead.", false ) ]
		public void ImpersonateUser( string userGuid, object credential )
		{
			ImpersonateUser( userGuid );
		}

		/// <summary>
		/// Allows the current thread to run in the specified user's security context.
		/// </summary>
		/// <param name="userGuid">Identifier for the user.</param>
		public void ImpersonateUser( string userGuid )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			identity.Impersonate( userGuid.ToLower() );
		}

		/// <summary>
		/// Imports an xml document representing collection store nodes into the specified collection.
		/// </summary>
		/// <param name="signedDocument">Digitally signed xml document that represents the collection store nodes.</param>
		/// <param name="documentRoot">Uri that specifies where to root the files that belong to this collection. If the
		/// collection already exists, this parameter is ignored.  If the collection does not exist and null is passed in
		/// the collection's file are stored in a directory managed by the store.</param>
		/// <returns>Collection object where nodes where imported into.</returns>
		public Collection ImportNodesFromXml( XmlDocument signedDocument, Uri documentRoot )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Verify digital signature on the xml document and get the xml object that represents the object list.
			// TEMP: This is commented out until I get a fix on Mono for the XML digital signature stuff.			
			// XmlDocument collectionDocument = GetValidatedXmlDocument( signedDocument );
			XmlDocument collectionDocument = signedDocument;
			// TEMP

			// Get the identifier for the collection where the nodes are to be created.
			string collectionId = collectionDocument.DocumentElement.GetAttribute( Property.CollectionID );

			// Check if the collection already exists and if the user has rights to modify this collection.
			Collection collection = GetCollectionById( collectionId );
			if ( ( collection != null ) && ( !collection.IsAccessAllowed( Access.Rights.ReadWrite ) ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			// Find the collection node in the xml document if it exists.
			XmlElement xmlCollection = ( XmlElement )collectionDocument.DocumentElement.SelectSingleNode( Property.ObjectTag + "[@" + Property.IDAttr + "='" + collectionId + "']" );
			if ( xmlCollection == null )
			{
				// There is no collection element in the document.  There had better be an existing one.
				if ( collection == null )
				{
					throw new ApplicationException( "Cannot deserialize document when no collection node exists" );
				}
			}
			else
			{
				// If there is no collection object in the database, create an empty one.
				if ( collection == null )
				{
					// Just pass in the user type of the collection.
					string baseType = xmlCollection.GetAttribute( Property.TypeAttr );
					string userType = baseType.Substring( baseType.IndexOf( "." ) + 1 );
					collection = new Collection( this, xmlCollection.GetAttribute( Property.NameAttr), collectionId, userType, documentRoot );
					collection.Commit();

					// Create the document root directory.
					if ( !Directory.Exists( documentRoot.LocalPath ) )
					{
						Directory.CreateDirectory( documentRoot.LocalPath );
					}
				}
			}

			// Import each node in the document.
			foreach ( XmlElement xmlNode in collectionDocument.DocumentElement )
			{
				MergeXmlNode( collection, xmlNode );
			}

			// Commit all changes in the collection.
			collection.Commit( true );
			return collection;
		}

		/// <summary>
		/// Imports an xml document representing a collection store node into the specified collection.
		/// </summary>
		/// <param name="collection">Collection to import the node into.</param>
		/// <param name="signedDocument">Digitally signed xml document that contains the node.</param>
		/// <returns>Imported node object instantiated from the xml document.</returns>
		public Node ImportSingleNodeFromXml( Collection collection, XmlDocument signedDocument )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Verify digital signature on the xml document and get the xml object that represents the object list.
			// TEMP: This is commented out until I get a fix on Mono for the XML digital signature stuff.			
			// XmlDocument nodeDocument = GetValidatedXmlDocument( signedDocument );
			XmlDocument nodeDocument = signedDocument;
			// TEMP

			// Check if the user has rights to modify this collection.
			if ( !collection.IsAccessAllowed( Access.Rights.ReadWrite ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection modify right." );
			}

			// Merge the xml node document with any existing node.
			return MergeXmlNode( collection, (XmlElement)nodeDocument.DocumentElement.FirstChild );
		}

		/// <summary>
		/// Reverts the user security context back to the previous owner.
		/// </summary>
		public void Revert()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			identity.Revert();
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Mandatory method used by clients to retrieve collection objects.
		/// </summary>
		/// <remarks>
		/// The client must call Dispose() to free up system resources before releasing
		/// the reference to the ICSEnumerator.
		/// </remarks>
		/// <returns>IEnumerator object used to enumerate collections.</returns>
		public IEnumerator GetEnumerator()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			return new StoreEnumerator( this );
		}

		/// <summary>
		/// Enumerator class for the Store object that allows enumeration of collection objects
		/// within the Store.
		/// </summary>
		private class StoreEnumerator : ICSEnumerator
		{
			#region Class Members
			/// <summary>
			/// The allocation size of the results array.
			/// </summary>
			private const int resultsArraySize = 4096;

			/// <summary>
			/// Indicates whether the object has been disposed.
			/// </summary>
			private bool disposed = false;

			/// <summary>
			/// List of collections that exist under this Store.
			/// </summary>
			private XmlDocument collectionList;

			/// <summary>
			/// Enumerator used to enumerate each returned item in the chunk enumerator list.
			/// </summary>
			private IEnumerator collectionEnumerator;

			/// <summary>
			/// The virtual store of where to enumerate the collection objects.
			/// </summary>
			private Store localStore;

			/// <summary>
			/// The internal enumerator to use to enumerate all of the child nodes belonging to this node.
			/// </summary>
			private Persist.IResultSet chunkIterator = null;

			/// <summary>
			/// Array where the query results are stored.
			/// </summary>
			private char[] results = new char[ resultsArraySize ];

			/// <summary>
			/// Enumerator used to enumerate all IDs that the user is known as.
			/// </summary>
			private IEnumerator idsEnumerator = null;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the StoreEnumerator class.
			/// </summary>
			/// <param name="localStore">Virtual store object where to enumerate the collections.</param>
			public StoreEnumerator( Store localStore )
			{
				this.localStore = localStore;

				// If this user does not have owner rights, then get all the identities that he is known as.
				if ( localStore.CurrentUser != localStore.StoreOwner )
				{
					idsEnumerator = localStore.LocalIdentity.GetIdentityAndAliases().GetEnumerator();
				}

				Reset();
			}
			#endregion

			#region Private Methods
			/// <summary>
			/// Starts the query of collection objects based on who the current user is.
			/// </summary>
			/// <param name="currentUserGuid">The guid that the current user is known as.</param>
			private void CollectionQuery( string currentUserGuid )
			{
				Persist.Query query;

				// Create a query object that will return a result set containing the children of this node.
				if ( currentUserGuid == localStore.StoreOwner )
				{
					// If the store owner is doing the search, return all collections in his store.
					query = new Persist.Query( Property.ObjectType, Persist.Query.Operator.Begins, Node.CollectionType, Property.Syntax.String.ToString() );
				}
				else
				{
					// This is not the store owner.  Only search for collections that the specified user has rights to.
					query = new Persist.Query( Property.Ace, Persist.Query.Operator.Begins, currentUserGuid, Property.Syntax.String.ToString() );
				}

				// Do the search.
				chunkIterator = localStore.storageProvider.Search( query );
				if ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						collectionList = new XmlDocument();
						collectionList.LoadXml( new string( results, 0, length ) );
						collectionEnumerator = collectionList.DocumentElement.GetEnumerator();
					}
					else
					{
						collectionEnumerator = null;
					}
				}
				else
				{
					collectionEnumerator = null;
				}
			}
			#endregion

			#region IEnumerator Members
			/// <summary>
			/// Sets the enumerator to its initial position, which is before the first element in the
			/// collection.
			/// </summary>
			public void Reset()
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// If the store owner is not the current user, then use the set of ids that have been enumerated
				// by the constructor.
				if ( idsEnumerator != null )
				{
					// Start at the beginning of the id list and make sure there are entries in the list.
					idsEnumerator.Reset();
					if ( idsEnumerator.MoveNext() )
					{
						CollectionQuery( ( string )idsEnumerator.Current );
					}
					else
					{
						collectionEnumerator = null;
					}
				}
				else
				{
					// Perform a new query.
					CollectionQuery( localStore.StoreOwner );
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
						throw new ObjectDisposedException( this.ToString() );
					}

					if ( collectionEnumerator == null )
					{
						throw new InvalidOperationException( "Empty enumeration" );
					}

					return localStore.GetShallowCollection( ( XmlNode )collectionEnumerator.Current );
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
				bool moreData = false;

				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Make sure that there is data in the list.
				while ( ( collectionEnumerator != null ) && !moreData )
				{
					// See if there is anymore data left in this result set.
					moreData = collectionEnumerator.MoveNext();
					if ( !moreData )
					{
						// Get the next page of the results set.
						int length = chunkIterator.GetNext( ref results );
						if ( length > 0 )
						{
							// Set up the XML document that we will use as the granular query to the client.
							collectionList = new XmlDocument();
							collectionList.LoadXml( new string( results, 0, length ) );
							collectionEnumerator = collectionList.DocumentElement.GetEnumerator();

							// Move to the first entry in the document.
							moreData = collectionEnumerator.MoveNext();
							if ( !moreData )
							{
								// See if there are any more ids to enumerate.
								if ( ( idsEnumerator != null ) && idsEnumerator.MoveNext() )
								{
									CollectionQuery( ( string )idsEnumerator.Current );
								}
								else
								{
									// Out of data.
									collectionEnumerator = null;
								}
							}
						}
						else
						{
							// See if there are more ids to enumerate.
							if ( ( idsEnumerator != null ) && idsEnumerator.MoveNext() )
							{
								CollectionQuery( ( string )idsEnumerator.Current );
							}
							else
							{
								// Out of data.
								collectionEnumerator = null;
								moreData = false;
							}
						}
					}
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
			private void Dispose( bool disposing )
			{
				// Check to see if Dispose has already been called.
				if ( !disposed )
				{
					// Protect callers from accessing the freed members.
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
			~StoreEnumerator()      
			{
				Dispose( false );
			}
			#endregion
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
		private void Dispose( bool disposing )
		{
			// Check to see if Dispose has already been called.
			if ( !disposed )
			{
				// Protect callers from accessing the freed members.
				disposed = true;

				// If disposing equals true, dispose all managed and unmanaged resources.
				if ( disposing )
				{
					// Dispose managed resources.
					storageProvider.Dispose();
					storeMutex.Close();
				}
			}
		}
		
		/// <summary>
		/// Use C# destructor syntax for finalization code.
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize.
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~Store()      
		{
			Dispose( false );
		}
		#endregion
	}
}
