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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Event;
using Persist = Simias.Storage.Provider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

namespace Simias.Storage
{
	/// <summary>
	/// This is the top level object for the Collection Store.  The Store 
	/// object can contain multiple collection objects.  The Store object 
	/// is not necessarily a one-to-one mapping with a persistent store.
	/// Collections within a Store may resided in different persistent 
	/// stores either locally or more likely remotely.
	/// </summary>
	public sealed class Store : IEnumerable, IDisposable
	{
		#region Class Members
		/// <summary>
		/// Object used to control race condition when creating the database.
		/// </summary>
		static private string ctorLock = "";

		/// <summary>
		/// Initial size of the cacheNode hash table.
		/// </summary>
		private const int cacheNodeTableSize = 20;

		/// <summary>
		/// Directory where store-managed files are kept.
		/// </summary>
		private const string storeManagedDirectoryName = "CollectionFiles";

		/// <summary>
		/// Tag used to sign serialized collection nodes.
		/// </summary>
		private const string collectionSignature = "Novell Serialized Collection";

		/// <summary>
		/// Specifies whether object is viable.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Object that identifies the current owner or impersonator.
		/// </summary>
		private IdentityManager identityManager = null;

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
		private EventPublisher publisher;

		/// <summary>
		/// TODO: Remove this once we have a cross-process database lock function.
		/// </summary>
		private Mutex storeMutex;

		/// <summary>
		/// Table used to lookup nodes as singleton objects.
		/// </summary>
		private Hashtable cacheNodeTable = new Hashtable( cacheNodeTableSize );

		/// <summary>
		/// Identifier of the component that opened this instance of the object.  This is helpful information
		/// to the event system for preventing circular events.
		/// </summary>
		private string componentId;

		/// <summary>
		/// Handle to the local address book.
		/// </summary>
		private LocalAddressBook localAb = null;

		/// <summary>
		/// Handle to the local database object.
		/// </summary>
		private Collection localDb = null;

		/// <summary>
		/// Configuration object passed during connect.
		/// </summary>
		private Configuration config;

		/// <summary>
		/// The allocation size of the results array.
		/// </summary>
		internal const int resultsArraySize = 4096;

		/// <summary>
		/// Type of collection that represents the store database.
		/// </summary>
		internal const string DatabaseType = "CsDatabase";
		#endregion

		#region Properties
		/// <summary>
		/// Gets the owner of the store.
		/// </summary>
		private string StoreAdmin
		{
			get { return Access.StoreAdminRole; }
		}

		/// <summary>
		/// Gets whether the current user is the store owner.
		/// </summary>
		private bool IsStoreAdmin
		{
			get { return ( StoreAdmin == CurrentUser ) ? true : false; }
		}

		/// <summary>
		/// Gets the handle to the persistent store.
		/// </summary>
		internal Persist.IProvider StorageProvider
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return storageProvider; 
			}
		}

		/// <summary>
		/// Gets the store managed path.
		/// </summary>
		internal Uri StoreManagedPath
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return storeManagedPath; 
			}
		}

		/// <summary>
		/// Gets the directory where this assembly was loaded from.
		/// </summary>
		internal Uri AssemblyPath
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return assemblyPath; 
			}
		}

		/// <summary>
		/// Gets the event publisher for the store.
		/// </summary>
		internal EventPublisher Publisher
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return publisher; 
			}
		}

		/// <summary>
		/// Gets the identifier for the component that opened this instance of the store.
		/// </summary>
		internal string ComponentId
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return componentId; 
			}
		}

		/// <summary>
		/// Gets a unique value for this instance.
		/// </summary>
		internal int Instance
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return this.GetHashCode(); 
			}
		}

		/// <summary>
		/// Gets the current impersonation identity for the store.
		/// </summary>
		public string CurrentUser
		{
			get 
			{ 
				lock ( this )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return identityManager.CurrentUserGuid; 
				}
			}
		}

		/// <summary>
		/// Gets the domain name for the current user.
		/// </summary>
		public string DomainName
		{
			get 
			{ 
				lock ( this )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return identityManager.DomainName; 
				}
			}
		}

		/// <summary>
		/// Gets the identity object associated with this store that represents the currently impersonating user.
		/// </summary>
		public Identity CurrentIdentity
		{
			get 
			{ 
				lock ( this )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return identityManager.CurrentIdentity; 
				}
			}
		}

		/// <summary>
		///  Specifies where the default store path
		/// </summary>
		public Uri StorePath
		{
			get 
			{ 
				lock ( this )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return storageProvider.StoreDirectory; 
				}
			}
		}

		/// <summary>
		/// Gets the RsaKeyStore object that provides an interface for the secure remote authentication.
		/// </summary>
		public RsaKeyStore KeyStore
		{
			get 
			{ 
				lock ( this )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return identityManager; 
				}
			}
		}

		/// <summary>
		/// Gets the public key for the server.
		/// </summary>
		public RSACryptoServiceProvider ServerPublicKey
		{
			get 
			{ 
				lock ( this )
				{
					if ( disposed )
					{
						throw new ObjectDisposedException( this.ToString() );
					}

					return identityManager.PublicKey; 
				}
			} 
		}

		/// <summary>
		/// Gets the configuration object passed to the store.Connect() method.
		/// </summary>
		public Configuration Config
		{
			get { return config; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the Store object.
		/// </summary>
		/// <param name="config">A Configuration object that contains a path that specifies where to create
		/// or open the database.</param>
		/// <param name="componentId">An identifier for the component that is opening this instance of the 
		/// object.</param>
		private Store( Configuration config, string componentId )
		{
			// Don't let another process authenticate while the database is still being initialized.
			lock ( Store.ctorLock )
			{
				// Store the configuration and component that opened this instance.
				this.config = config;
				this.componentId = componentId;

				// Get the path where the assembly was loaded from.
				assemblyPath = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) );

				// Create or open the underlying database.
				bool created;
				storageProvider = Persist.Provider.Connect( config.BasePath, out created );

				// Set the path to the store.
				storeManagedPath = new Uri( Path.Combine( storageProvider.StoreDirectory.LocalPath, storeManagedDirectoryName ) );

				// Allocate a publisher.
				publisher = new EventPublisher( config );

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
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Authenticates the current user to make sure that it is the same as the store owner.
		/// Throws an exception if the authentication fails.
		/// </summary>
		private void AuthenticateStore()
		{
			LocalAddressBook localAb = GetLocalAddressBook();
			if ( localAb == null )
			{
				throw new ApplicationException( "Local address book does not exist." );
			}

			// TODO: Remove this when Russ gets the database lock in.
			storeMutex = new Mutex( false, localAb.Name );

			// Look up to see if the current user has an identity.
			Identity identity = localAb.GetSingleIdentityByName( Environment.UserName );
			if ( identity == null )
			{
				throw new ApplicationException( "No such user." );
			}

			// Get the database object to check if this ID is the same as the owner.
			Collection dbCollection = GetDatabaseObject();
			if ( dbCollection == null )
			{
				throw new ApplicationException( "Store database object does not exist" );
			}

			// Get the owner property and make sure that it is the same as the current user.
			if ( dbCollection.Owner != identity.Id )
			{
				throw new UnauthorizedAccessException( "Current user is not store owner." );
			}

			// Create a identity manager object that will be used by the store object from here on out.
			identityManager = new IdentityManager( identity );
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
				localdb.Synchronizable = false;
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
				collectionDoc.LoadXml( signedXml.GetIdElement( signedDoc, collectionSignature ).InnerXml );
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
			// Create a domain name for this domain.
			string domainName = Environment.UserDomainName + ":" + Guid.NewGuid().ToString().ToLower();

			// Create a new guid for this local identity.
			string ownerGuid = Guid.NewGuid().ToString().ToLower();

			// Create the local address book.
			LocalAddressBook localAb = new LocalAddressBook( this, domainName, ownerGuid );

			// TODO: Remove this when Russ gets the database lock in.
			storeMutex = new Mutex( false, localAb.Name );

			// Create an identity that represents the current user.  This user will become the database owner.
			Identity identity = new Identity( localAb, Environment.UserName, ownerGuid );

			// Add a key pair to this identity to be used as credentials.
			identity.CreateKeyPair();

			// Create the role identities in the local address book.  These are used when impersonating.  They are not
			// normal contact entries.
			new Identity( localAb, "StoreAdmin", Access.StoreAdminRole, Property.IdentityRole );
			new Identity( localAb, "SyncIdentityRole", Access.SyncOperatorRole, Property.IdentityRole );
			new Identity( localAb, "BackupIdentityRole", Access.BackupOperatorRole, Property.IdentityRole );
			new Identity( localAb, "WorldIdentityRole", Access.WorldRole, Property.IdentityRole );

			// Set this new identity in the identity manager.
			identityManager = new IdentityManager( identity );

			// Save the address book changes.
			localAb.Commit( true );

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
				MultiValuedList localProps = new MultiValuedList( oldNode.Properties, Property.Local );
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
						localProps = new MultiValuedList( fse.Properties, Property.Local );
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
				return new Collection( this, xmlNode, true );
			}
			else
			{
				return new Node( collection, xmlNode, ( oldNode != null ) ? true : false, true, true );
			}
		}

		/// <summary>
		/// This method is informed of a change to the local addressbook object and will automatically refresh it.
		/// </summary>
		/// <param name="args">Event context arguments.</param>
		private void OnChangedAb( NodeEventArgs args )
		{
			MyTrace.WriteLine( "Refreshing local address book object." );
			localAb.Refresh();
		}

		/// <summary>
		/// This method is informed of a change to the local database object and will automatically refresh it.
		/// </summary>
		/// <param name="args">Event context arguments.</param>
		private void OnChangedDb( NodeEventArgs args )
		{
			MyTrace.WriteLine( "Refreshing local database object." );
			localDb.Refresh();
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
				dataObject.Id = collectionSignature;

				// Add the data object to the signature.
				signedXml.AddObject(dataObject);
 
				// Create a reference to be able to package everything into the message.
				Reference reference = new Reference();
				reference.Uri = "#" + collectionSignature;
 
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
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			// Create a new collection object from the DOM.
			return new Collection( this, xmlNode.Attributes[ Property.NameAttr ].Value, xmlNode.Attributes[ Property.IDAttr ].Value, xmlNode.Attributes[ Property.TypeAttr ].Value );
		}

		/// <summary>
		/// Acquires the store lock protecting the database against simultaneous commits.
		/// </summary>
		internal void LockStore()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			storeMutex.WaitOne();
		}

		/// <summary>
		/// Gets the cache node object in the hashtable if one exists.
		/// </summary>
		/// <param name="key">Identifier for the cache node object.</param>
		/// <returns>A cache node object if one exists, otherwise a null.</returns>
		internal CacheNode GetCacheNode( string key )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			lock ( this )
			{
				CacheNode cNode = ( CacheNode )cacheNodeTable[ key ];
				if ( cNode != null )
				{
					++cNode.referenceCount;
				}

				return cNode;
			}
		}

		/// <summary>
		/// Removes the specified entry from the hashtable.
		/// </summary>
		/// <param name="cNode">Node to remove from the cache table.</param>
		/// <param name="force">If true, object is removed from table regardless of the reference count.</param>
		internal void RemoveCacheNode( CacheNode cNode, bool force )
		{
			if ( !disposed )
			{
				lock ( this )
				{
					// Make sure that the node is in the table and that it is the same object.
					CacheNode cTempNode = ( CacheNode )cacheNodeTable[ cNode.id ];
					if ( ( cTempNode != null ) && cTempNode.Equals( cNode ) )
					{
						if ( force || ( --cTempNode.referenceCount == 0 ) )
						{
							cacheNodeTable.Remove( cTempNode.id );
						}
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
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

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

				++cNode.referenceCount;
				return cNode;
			}
		}

		/// <summary>
		/// Releases the store lock.
		/// </summary>
		internal void UnlockStore()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			storeMutex.ReleaseMutex();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Connects to the persistent store server.  Lets the storage provider decide the path to
		/// where to create or open the database.
		/// </summary>
		///	<returns>An object that represents a connection to the store.</returns>
		[ Obsolete( "This method has been marked for removal. Use other overloads that pass a Configuration object instead.", false ) ]
		public static Store Connect()
		{
			return Connect( null as Uri );
		}

		/// <summary>
		/// Connects to the persistent store server.
		/// </summary>
		/// <param name="databasePath">Path that specifies where to create or open the database. If this parameter
		/// is null, the default database path is used.</param>
		///	<returns>An object that represents a connection to the store.</returns>
		[ Obsolete( "This method has been marked for removal. Use other overloads that pass a Configuration object instead.", false ) ]
		public static Store Connect( Uri databasePath )
		{
			return Connect( databasePath, null );
		}

		/// <summary>
		/// Connects to the persistent store server.
		/// </summary>
		/// <param name="componentId">An identifier for the component that is opening this instance of the object.
		/// A suggested way to generate this parameter is using: 'this.GetType().FullName'.  This will produce the 
		/// fully qualified name of the Type, including the namespace of the Type.</param>
		///	<returns>An object that represents a connection to the store.</returns>
		[ Obsolete( "This method has been marked for removal. Use other overloads that pass a Configuration object instead.", false ) ]
		public static Store Connect( string componentId )
		{
			return Connect( null, componentId );
		}

		/// <summary>
		/// Connects to the persistent store server.
		/// </summary>
		/// <param name="databasePath">Path that specifies where to create or open the database. If this parameter
		/// is null, the default database path is used.</param>
		/// <param name="componentId">An identifier for the component that is opening this instance of the object.
		/// A suggested way to generate this parameter is using: 'this.GetType().FullName'.  This will produce the 
		/// fully qualified name of the Type, including the namespace of the Type.</param>
		///	<returns>An object that represents a connection to the store.</returns>
		[ Obsolete( "This method has been marked for removal. Use other overloads that pass a Configuration object instead.", false ) ]
		public static Store Connect( Uri databasePath, string componentId )
		{
			if ( componentId == null )
			{
				// Get the calling class type.
				StackFrame sf = new StackFrame( 2 );
				componentId = sf.GetMethod().DeclaringType.FullName;
			}

			Configuration config = new Configuration( ( databasePath != null ) ? databasePath.LocalPath : null );
			return new Store( config, componentId );
		}

		/// <summary>
		/// Connects to the collection store.
		/// </summary>
		/// <param name="config">A Configuration object which contains store startup information.</param>
		///	<returns>A store object that represents a connection to the store.</returns>
		public static Store Connect( Configuration config )
		{
			// Get an identifier unique to the caller so that circular events can be prevented.
			StackFrame sf = new StackFrame( 2 );
			return new Store( config, sf.GetMethod().DeclaringType.FullName );
		}

		/// <summary>
		/// Method to create a collection in the local store.
		/// </summary>
		/// <param name="collectionName">Name the newly created collection will be called.</param>
		/// <param name="type">Type of collection.</param>
		/// <returns>An object to a new collection.</returns>
		public Collection CreateCollection( string collectionName, string type )
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return new Collection( this, collectionName, type );
			}
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
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return new Collection( this, collectionName, type, documentRoot );
			}
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
		/// Deletes the persistent store database and disposes this object.
		/// </summary>
		public void Delete()
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Check if the current user is the store owner.
				if ( !IsStoreAdmin )
				{
					throw new UnauthorizedAccessException( "Current user is not the store owner." );
				}

				// Check if the store managed path still exists. If it does, delete it.
				if ( Directory.Exists( storeManagedPath.LocalPath ) )
				{
					Directory.Delete( storeManagedPath.LocalPath, true );
				}

				// Let go of the address book and collection database references.
				if ( localAb != null )
				{
					localAb.Dispose();
					localAb = null;
				}

				if ( localDb != null )
				{
					localDb.Dispose();
					localDb = null;
				}

				// Clean out the cache node table.
				cacheNodeTable.Clear();

				// Let go of the event publisher.
				publisher = null;

				// Let go of the identity manager.
				if ( identityManager != null )
				{
					identityManager.Dispose();
					identityManager = null;
				}

				// Say bye-bye to the store.
				storageProvider.DeleteStore();
				Dispose();
			}
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
			lock ( this )
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
		/// Gets all nodes that are associated with a file system entry.
		/// </summary>
		/// <param name="documentRoot">Uri object that contains the root path for the file system entry.</param>
		/// <param name="relativePath">String containing a path relative to documentRoot that specifies a file or directory entry.</param>
		/// <returns>An ICSList object containg nodes that reference the specified file system entry.</returns>
		[ Obsolete( "This method is marked for removal. Use other overloaded method instead.", false ) ]
		public ICSList GetNodesAssociatedWithPath( Uri documentRoot, string relativePath )
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Create an empty list.
				ICSList nodeList = new ICSList();

				// Build a query for the document root.
				Persist.Query query = new Persist.Query( Property.DocumentRoot, Persist.Query.Operator.Equal, documentRoot.ToString(), Syntax.Uri );
				char[] results = new char[ Store.resultsArraySize ];

				// Do the search.
				Persist.IResultSet chunkIterator = storageProvider.Search( query );
				while ( chunkIterator != null )
				{
					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document that we will use as the granular query to the client.
						XmlDocument xmlNodeList = new XmlDocument();
						xmlNodeList.LoadXml( new string( results, 0, length ) );

						// Enumerate through the results.
						foreach ( XmlElement element in xmlNodeList.DocumentElement )
						{
							ICSList pathList = GetNodesAssociatedWithPath( element.GetAttribute( Property.IDAttr ), relativePath );
							foreach ( Node node in pathList )
							{
								// Insert this list into the outer list.
								nodeList.Add( node );
							}
						}
					}
					else
					{
						chunkIterator.Dispose();
						chunkIterator = null;
					}
				}

				return nodeList;
			}
		}

		/// <summary>
		/// Gets all nodes that are associated with a file system entry.
		/// </summary>
		/// <param name="collectionId">Identifier for the collection that this relative path is associated with.</param>
		/// <param name="relativePath">String containing a path relative to collection documentRoot that specifies a file or directory entry.</param>
		/// <returns>An ICSList object containg nodes that reference the specified file system entry.</returns>
		public ICSList GetNodesAssociatedWithPath( string collectionId, string relativePath )
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				// Create an empty list.
				ICSList nodeList = new ICSList();

				// Instantiate the specified collection object.
				Collection collection = GetCollectionById( collectionId );
				if ( collection == null )
				{
					throw new ApplicationException( "Collection does not exist." );
				}

				// Search in this collection for the relative path.
				ICSList pathList = collection.Search( Property.NodeFileSystemEntry, relativePath, Property.Operator.Contains );
				foreach ( Node node in pathList )
				{
					// See if this node contains the proper path.
					foreach ( FileSystemEntry fse in node.GetFileSystemEntryList() )
					{
						if ( fse.RelativePath == relativePath )
						{
							// This is a node that we are looking for.
							nodeList.Add( node );
							break;
						}
					}
				}

				return nodeList;
			}
		}

		/// <summary>
		/// Returns a collection object from the data store by its identifier.
		/// </summary>
		/// <param name="collectionId">Unique identifier for the collection.</param>
		/// <returns>Collection object that corresponds to the specified collection ID.  If the collection doesn't exist a null is returned.</returns>
		public Collection GetCollectionById( string collectionId )
		{
			lock ( this )
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
						collection = new Collection( this, collectionDoc.DocumentElement[ Property.ObjectTag ], false );
					}
				}
				else
				{
					// Just build the collection from the existing data.
					collection = new Collection( this, cNode, false );
				}

				return collection;
			}
		}

		/// <summary>
		///  Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s).  This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>An ICSList object containing the collections(s) that have the specified name.</returns>
		public ICSList GetCollectionsByName( string name )
		{
			lock ( this )
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
		}

		/// <summary>
		///  Gets all collections that have the specified type.
		/// </summary>
		/// <param name="type">String that contains the type of the collection.  This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>An ICSList object containing the collection(s) that have the specified type.</returns>
		public ICSList GetCollectionsByType( string type )
		{
			lock ( this )
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
		}

		/// <summary>
		/// Returns the collection that represents the database object.
		/// </summary>
		/// <returns>A collection object that represents a database object.</returns>
		public Collection GetDatabaseObject()
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				if ( localDb == null )
				{
					// Look for the address book by its name, which is the domain name.
					Persist.Query query = new Persist.Query( Property.ObjectType, Persist.Query.Operator.Equal, Node.CollectionType + DatabaseType, Syntax.String );

					// Do the search.
					char[] results = new char[ 4096 ];
					Persist.IResultSet chunkIterator = storageProvider.Search( query );
					if ( chunkIterator != null )
					{
						// Get the first set of results from the query.
						int length = chunkIterator.GetNext( ref results );
						if ( length > 0 )
						{
							// Set up the XML document so the data can be easily extracted.
							XmlDocument dbDocument = new XmlDocument();
							dbDocument.LoadXml( new string( results, 0, length ) );
							localDb = GetShallowCollection( dbDocument.DocumentElement.FirstChild );

							//							// Set up a delegate to update the local database object if it changes.
							//							string[] nodeFilter = new string[ 1 ];
							//							nodeFilter[ 0 ] = localDb.Id;
							//							localDb.NodeEventsSubscribe( new Collection.NodeChangeHandler( OnChangedDb ), nodeFilter );
						}

						chunkIterator.Dispose();
					}
				}
				else
				{
					localDb.Refresh();
				}

				return localDb;
			}
		}

		/// <summary>
		/// Gets the local address book that contains the identities for the store's domain.
		/// </summary>
		/// <returns>A Collection object that represents the local address book if it exists. 
		/// Otherwise null is returned.</returns>
		public LocalAddressBook GetLocalAddressBook()
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				if ( localAb == null )
				{
					// Look for the address book by its name, which is the domain name.
					Persist.Query query = new Persist.Query( Property.LocalAddressBook, Persist.Query.Operator.Equal, "true", Syntax.Boolean );

					// Do the search.
					char[] results = new char[ 4096 ];
					Persist.IResultSet chunkIterator = storageProvider.Search( query );
					if ( chunkIterator != null )
					{
						// Get the first set of results from the query.
						int length = chunkIterator.GetNext( ref results );
						if ( length > 0 )
						{
							// Set up the XML document so the data can be easily extracted.
							XmlDocument abDocument = new XmlDocument();
							abDocument.LoadXml( new string( results, 0, length ) );
							localAb = new LocalAddressBook( this, GetShallowCollection( abDocument.DocumentElement.FirstChild ) );

							//							// Set up a delegate to update the local address book object if it changes.
							//							string[] nodeFilter = new string[ 1 ];
							//							nodeFilter[ 0 ] = localAb.Id;
							//							localAb.NodeEventsSubscribe( new LocalAddressBook.NodeChangeHandler( OnChangedAb ), nodeFilter );
						}

						chunkIterator.Dispose();
					}
				}
				else
				{
					localAb.Refresh();
				}

				return localAb;
			}
		}

		/// <summary>
		/// Gets the first collection that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first collection object that matches the specified name.  A null is returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByName( string name )
		{
			lock ( this )
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
		}

		/// <summary>
		/// Gets the first collection that matches the specified type.
		/// </summary>
		/// <param name="type">A string containing the type for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first collection object that matches the specified type.  A null is returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByType( string type )
		{
			lock ( this )
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
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				identityManager.Impersonate( userGuid.ToLower() );
			}
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
			lock ( this )
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
		}

		/// <summary>
		/// Imports an xml document representing a collection store node into the specified collection.
		/// </summary>
		/// <param name="collection">Collection to import the node into.</param>
		/// <param name="signedDocument">Digitally signed xml document that contains the node.</param>
		/// <returns>Imported node object instantiated from the xml document.</returns>
		public Node ImportSingleNodeFromXml( Collection collection, XmlDocument signedDocument )
		{
			lock ( this )
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
		}

		/// <summary>
		/// Reverts the user security context back to the previous owner.
		/// </summary>
		public void Revert()
		{
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				identityManager.Revert();
			}
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
			lock ( this )
			{
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return new StoreEnumerator( this );
			}
		}

		/// <summary>
		/// Enumerator class for the Store object that allows enumeration of collection objects
		/// within the Store.
		/// </summary>
		private class StoreEnumerator : ICSEnumerator
		{
			#region Class Members
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
			private Store store;

			/// <summary>
			/// The internal enumerator to use to enumerate all of the child nodes belonging to this node.
			/// </summary>
			private Persist.IResultSet chunkIterator = null;

			/// <summary>
			/// Array where the query results are stored.
			/// </summary>
			private char[] results = new char[ Store.resultsArraySize ];

			/// <summary>
			/// Enumerator used to enumerate all IDs that the user is known as.
			/// </summary>
			private IEnumerator idsEnumerator = null;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the StoreEnumerator class.
			/// </summary>
			/// <param name="store">Virtual store object where to enumerate the collections.</param>
			public StoreEnumerator( Store store )
			{
				this.store = store;

				// If this user does not have owner rights, then get all the identities that he is known as.
				if ( store.CurrentUser != store.StoreAdmin )
				{
					idsEnumerator = store.CurrentIdentity.GetIdentityAndAliases().GetEnumerator();
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
				if ( currentUserGuid == store.StoreAdmin )
				{
					// If the store owner is doing the search, return all collections in his store.
					query = new Persist.Query( Property.ObjectType, Persist.Query.Operator.Begins, Node.CollectionType, Syntax.String );
				}
				else
				{
					// This is not the store owner.  Only search for collections that the specified user has rights to.
					query = new Persist.Query( Property.Ace, Persist.Query.Operator.Begins, currentUserGuid, Syntax.String );
				}

				// Do the search.
				chunkIterator = store.storageProvider.Search( query );
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
				lock ( store )
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
						CollectionQuery( store.StoreAdmin );
					}
				}
			}

			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public object Current
			{
				get
				{
					lock ( store )
					{
						if ( disposed )
						{
							throw new ObjectDisposedException( this.ToString() );
						}

						if ( collectionEnumerator == null )
						{
							throw new InvalidOperationException( "Empty enumeration" );
						}

						return store.GetShallowCollection( ( XmlNode )collectionEnumerator.Current );
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
				lock ( store )
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
			}
			#endregion

			#region IDisposable Members
			/// <summary>
			/// Allows for quick release of managed and unmanaged resources.
			/// Called by applications.
			/// </summary>
			public void Dispose()
			{
				lock ( store )
				{
					Dispose( true );
					GC.SuppressFinalize( this );
				}
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
				lock ( store )
				{
					Dispose( false );
				}
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
			lock ( this )
			{
				Dispose( true );
				GC.SuppressFinalize( this );
			}
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
					// Let go of the address book and collection database references.
					if ( localAb != null )
					{
						localAb.Dispose();
					}

					if ( localDb != null )
					{
						localDb.Dispose();
					}

					// Clean out the cache node table.
					cacheNodeTable.Clear();

					// Let go of the event publisher.
					publisher = null;

					// Let go of the identity manager.
					if ( identityManager != null )
					{
						identityManager.Dispose();
					}

					// Dispose managed resources.
					storageProvider.Dispose();
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
			lock ( this )
			{
				Dispose( false );
			}
		}
		#endregion
	}
}
