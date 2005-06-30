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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;

using Simias;
using Simias.Client;
using Simias.Event;
using Simias.Policy;
using Simias.Storage.Provider;
using Simias.Sync;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// This is the top level object for the Collection Store.  The Store object can contain multiple 
	/// collection objects.
	/// </summary>
	public sealed class Store : IEnumerable
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		/// <summary>
		/// Used to keep track of changes to the layout of the store database.
		/// </summary>
		static private string storeVersion = "1.0.1";

		/// <summary>
		/// Directories where store-managed and unmanaged files are kept.
		/// </summary>
		static private string storeManagedDirectoryName = "CollectionFiles";
		static private string storeUnmanagedDirectoryName = "SimiasFiles";

		/// <summary>
		/// File used to store the local password used to authenticate local
		/// web services.
		/// </summary>
		static private string LocalPasswordFile = ".local.if";

		/// <summary>
		/// Name of the local domain.
		/// </summary>
		static internal string LocalDomainName = "Local";

		/// <summary>
		/// Default sync interval for the machine. Synchronizes every 5 minutes.
		/// </summary>
		static private int DefaultMachineSyncInterval = 300;

		/// <summary>
		/// Flag that indicates whether the database is shutting down. No changes
		/// will be allowed in the database if this flags is true.
		/// </summary>
		private bool shuttingDown = false;

		/// <summary>
		/// Handle to the local store provider.
		/// </summary>
		private Persist.IProvider storageProvider = null;

		/// <summary>
		/// Path to where store managed files are kept.
		/// </summary>
		private string storeManagedPath;

		/// <summary>
		/// Path to where the unmanged files are kept.
		/// </summary>
		private string storeUnmanagedPath;

		/// <summary>
		/// Configuration object passed during connect.
		/// </summary>
		private Configuration config;

		/// <summary>
		/// Cross-process database lock function.
		/// </summary>
		static private Mutex storeMutex = new Mutex();

		/// <summary>
		/// String that identifies the publisher of events for this object instance.
		/// </summary>
		private string publisher = "Unspecified";

		/// <summary>
		/// Used to publish collection store events.
		/// </summary>
		private EventPublisher eventPublisher;

		/// <summary>
		/// Used for quick lookup of the current logged on user.
		/// </summary>
		private string identity = null;

		/// <summary>
		/// Used for quick lookup of the local database.
		/// </summary>
		private string localDb = null;

		/// <summary>
		/// Singleton of the store.
		/// </summary>
		private static Store instance = null;

		/// <summary>
		/// Used to indicate whether this instance is running on an enterprise server.
		/// </summary>
		private bool enterpriseServer = false;

		/// <summary>
		/// Object used to cache node objects.
		/// </summary>
		private NodeCache cache;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the event publisher object.
		/// </summary>
		internal EventPublisher EventPublisher
		{
			get { return eventPublisher; }
		}

		/// <summary>
		/// Gets the local database object.
		/// </summary>
		internal LocalDatabase LocalDb
		{
			get { return GetCollectionByID( localDb ) as LocalDatabase; }
		}

		/// <summary>
		/// Gets or sets the publisher event source identifier.
		/// </summary>
		internal string Publisher
		{
			get { return publisher; }
			set { publisher = value; }
		}

		/// <summary>
		/// Gets the Storage provider interface.
		/// </summary>
		internal Persist.IProvider StorageProvider
		{
			get { return storageProvider; }
		}

		/// <summary>
		/// Gets the NodeCache object.
		/// </summary>
		internal NodeCache Cache
		{
			get { return cache; }
		}

		/// <summary>
		/// Gets or sets a local password on the local domain object.
		/// </summary>
		internal string LocalPassword
		{
			get
			{
				Property p = LocalDb.Properties.GetSingleProperty( PropertyTags.LocalPassword );
				return ( p != null ) ? p.ToString() : null;
			}

			set
			{
				LocalDatabase ldb = LocalDb;
				ldb.Properties.ModifyNodeProperty( PropertyTags.LocalPassword, value );
				ldb.Commit();
			}
		}

		/// <summary>
		/// Gets the configuration object passed to the store.Connect() method.
		/// </summary>
		public Configuration Config
		{
			get { return config; }
		}

		/// <summary>
		/// Gets or sets the default domain ID.
		/// </summary>
		public string DefaultDomain
		{
			get { return LocalDb.DefaultDomain; }
			set
			{
				LocalDatabase ldb = LocalDb;
				ldb.DefaultDomain = value;
				ldb.Commit();
			}
		}

		/// <summary>
		/// Gets the identifier for this Collection Store.
		/// </summary>
		public string ID
		{
			get { return localDb; }
		}

		/// <summary>
		/// Gets the Identity object that represents the currently logged on user.
		/// </summary>
		public Identity CurrentUser
		{
			get { return GetNodeByID( localDb, identity ) as Identity; }
		}

		/// <summary>
		/// Gets whether this instance is running on a enterprise server.
		/// </summary>
		public bool IsEnterpriseServer
		{
			get { return enterpriseServer; }
		}

		/// <summary>
		/// Gets the ID of the local domain.
		/// </summary>
		public string LocalDomain
		{
			get { return LocalDb.Domain; }
		}

		/// <summary>
		///  Specifies where the default store path
		/// </summary>
		public string StorePath
		{
			get { return Config.StorePath; }
		}

		/// <summary>
		/// Gets the version of the database.
		/// </summary>
		public string Version
		{
			get
			{
				Property p = LocalDb.Properties.FindSingleValue( PropertyTags.StoreVersion );
				return ( p != null ) ? p.ToString() : "Unknown version";
			}
		}

		/// <summary>
		/// Gets whether the database is being shut down. No changes to the database
		/// will be allowed if this property returns true.
		/// </summary>
		public bool ShuttingDown
		{
			get { return shuttingDown; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the Store object.
		/// </summary>
		/// <param name="config">A Configuration object that contains a path that specifies where to create
		/// or open the database.</param>
		private Store( Configuration config )
		{
			bool created;

			// Store the configuration that opened this instance.
			this.config = config;

			// Does the configuration indicate that this is an enterprise server?
			enterpriseServer = config.Exists( Domain.SectionName, null );

			// Setup the event publisher object.
			eventPublisher = new EventPublisher();

			// Initialize the node cache.
			cache = new NodeCache( this );

			// Create or open the underlying database.
			storageProvider = Persist.Provider.Connect( new Persist.ProviderConfig(), out created );

			// Set the managed and unmanaged paths to the store.
			storeManagedPath = Path.Combine( storageProvider.StoreDirectory.LocalPath, storeManagedDirectoryName );
			storeUnmanagedPath = Path.Combine( storageProvider.StoreDirectory.LocalPath, storeUnmanagedDirectoryName );

			// Either create the store or authenticate to it.
			if ( created )
			{
				try
				{
					// Default the user name to the local logged in user.
					string userName = Environment.UserName;

					// Get the name of the user to create as the identity.
					if ( IsEnterpriseServer )
					{
						// If there is a domain specified, get the specified admin user to be the
						// store owner.
						string adminDNName = config.Get( Domain.SectionName, Domain.AdminDNTag, null );
						if ( adminDNName != null )
						{
							userName = ParseUserName( adminDNName );
						}
					}

					// Create an object that represents the database collection.
					string localDomainID = Guid.NewGuid().ToString();
					LocalDatabase ldb = new LocalDatabase( this, localDomainID );
					ldb.Properties.AddNodeProperty( PropertyTags.StoreVersion, storeVersion );
					localDb = ldb.ID;

					// Create an identity that represents the current user.  This user will become the 
					// database owner. Add the domain mapping to the identity.
					Identity owner = new Identity( this, userName, Guid.NewGuid().ToString() );
					identity = owner.ID;

					// Create a credential to be used to identify the local user.
					RSACryptoServiceProvider credential = new RSACryptoServiceProvider( 1024 );

					// Create a credential to be used to identify the local user.
					owner.AddDomainIdentity( owner.ID, localDomainID, credential.ToXmlString( true ), CredentialType.PPK );
                    
					// Create a member object that will own the local database.
					Member member = new Member( owner.Name, owner.ID, Access.Rights.Admin );
					member.IsOwner = true;

					// Save the local database changes. Impersonate so that the creator of these nodes
					// can be set before the identity node is committed to the store.
					ldb.Impersonate( member );
					ldb.Commit( new Node[] { ldb, member, owner } );

					// Create the local domain.
					Domain domain = new Domain( this, LocalDomainName, localDomainID, "Local Machine Domain", SyncRoles.Local, Domain.ConfigurationType.None );
					Member domainOwner = new Member( owner.Name, owner.ID, Access.Rights.Admin, owner.PublicKey );
					domainOwner.IsOwner = true;
					domain.Commit( new Node[] { domain, domainOwner } );

					if ( !IsEnterpriseServer )
					{
						// Create a SyncInterval policy.
						SyncInterval.Create( DefaultMachineSyncInterval );
					}

					// Create a FileFilter local machine policy that disallows the Thumbs.db 
					// and .DS_Store files from synchronizing. This fix in in response to bug #73517.
					FileTypeFilter.Create( new FileTypeEntry[] { new FileTypeEntry( "Thumbs.db", false, true ),
																 new FileTypeEntry( ".DS_Store", false, false )
															   } );
				}
				catch ( Exception e )
				{
					// Log this error.
					log.Fatal( e, "Could not initialize Collection Store." );

					// The store didn't initialize delete it and rethrow the exception.
					if ( storageProvider != null )
					{
						storageProvider.DeleteStore();
						storageProvider.Dispose();
					}

					// Rethrow the exception.
					throw;
				}
			}
			else
			{
				// Get the local database object.
				LocalDatabase ldb = GetDatabaseObject();
				if ( ldb == null )
				{
					throw new DoesNotExistException( "Local database object does not exist." );
				}

				// Compare the store version to make sure that it is correct.
				if ( storeVersion != Version )
				{
					throw new SimiasException( String.Format( "Incompatible database version. Expected version {0} - Found version {1}.", storeVersion, Version ) );
				}

				// Get the identity object that represents this logged on user.
				Identity lid = ldb.GetSingleNodeByType( NodeTypes.IdentityType ) as Identity;
				if ( lid != null )
				{
					identity = lid.ID;
				}
				else
				{
					throw new DoesNotExistException( "Identity object does not exist." );
				}
			}

			// Create a one-time password for non-simias processes to use to authenticate to the local web services.
			CreateLocalCredential();
		}
		#endregion

		#region Factory  Methods
		/// <summary>
		/// Gets a handle to the Collection store.
		/// </summary>
		/// <returns>A reference to a Store object.</returns>
		static public Store GetStore()
		{
			lock ( typeof( Store ) )
			{
				if ( instance == null )
				{
					instance = new Store( Configuration.GetConfiguration() );
					new CertPolicy();
					Simias.Security.CertificateStore.LoadCertsFromStore();
				}

				return instance;
			}
		}

		/// <summary>
		/// Deletes the singleton instance of the store object.
		/// NOTE: This call is for utility programs that need to browse to the store.
		/// Don't call this unless you understand the consequences of closing
		/// the process's only store instance.
		/// </summary>
		static public void DeleteInstance()
		{
			lock ( typeof( Store ) )
			{
				instance = null;
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Creates a local credential to be used to identify the local user.
		/// </summary>
		private void CreateLocalCredential()
		{
			// Check if the file already exists.
			string path = Path.Combine( StorePath, LocalPasswordFile );
			if ( !File.Exists( path ) )
			{
				// Set a random password.
				LocalPassword = Guid.NewGuid().ToString();

				// Export the credential to a file stored in the directory where the simias store
				// is located. The local web services will use the file credential to authenticate
				// to the local box.
				using ( StreamWriter sw = new StreamWriter( path ) )
				{
					sw.Write( LocalDomain + LocalPassword );
				}
			}
		}

		/// <summary>
		/// Returns a Node object for the specified identifier.
		/// </summary>
		/// <param name="collectionID">Identifier of the collection that the node is contained by.</param>
		/// <param name="nodeID">Globally unique identifier for the object.</param>
		/// <returns>A Node object for the specified identifier.  If the object doesn't 
		/// exist a null is returned.</returns>
		private Node GetNodeByID( string collectionID, string nodeID )
		{
			// Normalize the collectionID and node ID.
			collectionID = collectionID.ToLower();
			nodeID = nodeID.ToLower();

			// See if the node exists in the cache first.
			Node node = cache.Get( collectionID, nodeID );
			if ( node == null )
			{
				// Get the specified object from the persistent store.
				XmlDocument document = storageProvider.GetRecord( nodeID, collectionID );
				if ( document != null )
				{
					node = Node.NodeFactory( this, document );
					Collection collection = ( collectionID == nodeID ) ? node as Collection : GetNodeByID( collectionID, collectionID ) as Collection;
					cache.Add( collection, node );
				}
			}		

			return node;
		}

		/// <summary>
		/// Returns the 'cn' portion of the ldap string.
		/// </summary>
		/// <param name="ldapName">LDAP name to parse.</param>
		/// <returns>The 'cn' portion of the LDAP name.</returns>
		private string ParseUserName( string ldapName )
		{
			if ( ldapName == null )
			{
				throw new CollectionStoreException( "LDAP proxy name is null" );
			}

			// Skip over the 'cn=' if it exists.
			int startIndex = ldapName.ToLower().StartsWith( "cn=" ) ? 3 : 0;
			int endIndex = ldapName.IndexOf( ',' );
			int length = ( endIndex == -1 ) ? ldapName.Length - startIndex : endIndex - startIndex;
			return ldapName.Substring( startIndex, length );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets a path to where the store managed files for the specified collection should be created.
		/// </summary>
		/// <param name="collectionID">Collection identifier that files will be associated with.</param>
		/// <returns>A path string that represents the store managed path.</returns>
		internal string GetStoreManagedPath( string collectionID )
		{
			return Path.Combine( storeManagedPath, collectionID.ToLower() );
		}

		/// <summary>
		/// Gets a path to where the store unmanaged files for the specified collection should be created.
		/// </summary>
		/// <param name="collectionID">Collection identifier that files will be associated with.</param>
		/// <returns>A path string that represents the store unmanaged path.</returns>
		internal string GetStoreUnmanagedPath( string collectionID )
		{
			return Path.Combine( storeUnmanagedPath, collectionID.ToLower() );
		}

		/// <summary>
		/// Acquires the store lock protecting the database against simultaneous commits.
		/// </summary>
		internal void LockStore()
		{
			storeMutex.WaitOne();
		}

		/// <summary>
		/// Indicates that the database is being shutdown. No more changes will be allowed
		/// when this method completes.
		/// </summary>
		internal void ShutDown()
		{
			shuttingDown = true;
		}

		/// <summary>
		/// Releases the store lock.
		/// </summary>
		internal void UnlockStore()
		{
			storeMutex.ReleaseMutex();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds a domain identity to the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		public void AddDomainIdentity( string domainID, string userID )
		{
			AddDomainIdentity( domainID, userID, null, CredentialType.None );
		}

		/// <summary>
		/// Adds a domain identity to the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		///	<param name="credentials">Credentials for the user. May be null.</param>
		///	<param name="credType">Type of credentials being stored.</param>
		public void AddDomainIdentity( string domainID, string userID, string credentials, CredentialType credType )
		{
			// Get the domain.
			Domain domain = GetDomain( domainID.ToLower() );
			if ( domain == null )
			{
				throw new SimiasException( String.Format( "The domain {0} does not exist.", domainID ) );
			}

			// Add the domain mapping for the specified user.
			LocalDb.Commit( CurrentUser.AddDomainIdentity( userID.ToLower(), domain.ID, credentials, credType ) );
		}

		/// <summary>
		/// Deletes the persistent store database and disposes this object.
		/// </summary>
		public void Delete()
		{
			// Check if the store managed path still exists. If it does, delete it.
			if ( Directory.Exists( storeManagedPath ) )
			{
				Directory.Delete( storeManagedPath, true );
			}

			// Delete the local password file.
			string path = Path.Combine( StorePath, LocalPasswordFile );
			if ( File.Exists( path ) )
			{
				File.Delete( path );
			}

			// Say bye-bye to the store.
			storageProvider.DeleteStore();
			storageProvider.Dispose();
		}

		/// <summary>
		/// Removes the specified domain identity from the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		public void DeleteDomainIdentity( string domainID )
		{
			// Delete the domain object and commit the changes.
			LocalDb.Commit( CurrentUser.DeleteDomainIdentity( domainID.ToLower() ) );
		}

		/// <summary>
		/// Removes the credentials from the specified domain.
		/// </summary>
		/// <param name="domainID">Domain identifier to remove the password from.</param>
		public void DeleteDomainCredentials( string domainID )
		{
			LocalDb.Commit( CurrentUser.SetDomainCredentials( domainID.ToLower(), null, CredentialType.None ) );
		}

		/// <summary>
		/// Returns a Collection object for the specified identifier.
		/// </summary>
		/// <param name="collectionID">Globally unique identifier for the object.</param>
		/// <returns>A Collection object for the specified identifier.  If the object doesn't 
		/// exist a null is returned.</returns>
		public Collection GetCollectionByID( string collectionID )
		{
			return GetNodeByID( collectionID, collectionID ) as Collection;
		}

		/// <summary>
		/// Gets all collections that belong to the specified domain.
		/// </summary>
		/// <param name="domainID">Domain identifier.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection
		/// objects that matched the specified domain.</returns>
		public ICSList GetCollectionsByDomain( string domainID )
		{
			// Create a container object to hold all collections that match the specified domain.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( PropertyTags.DomainID, SearchOp.Equal, domainID.ToLower(), Syntax.String );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						if ( xe.GetAttribute( XmlTags.IdAttr ) == xe.GetAttribute( XmlTags.CIdAttr ) )
						{
							collectionList.Add( new ShallowNode( xe ) );
						}
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s) to search for.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection 
		/// objects that matched the specified name.</returns>
		public ICSList GetCollectionsByName( string name )
		{
			return GetCollectionsByName( name, SearchOp.Equal );
		}

		/// <summary>
		/// Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s) to search for.</param>
		/// <param name="searchOp">The search operation used with the search.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection 
		/// objects that matched the specified name.</returns>
		public ICSList GetCollectionsByName( string name, SearchOp searchOp )
		{
			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			Property p = new Property( BaseSchema.ObjectName, name );
			Persist.Query query = new Persist.Query( p.Name, searchOp, p.SearchString, p.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// See if this element represents a collection.
						if ( xe.GetAttribute( XmlTags.IdAttr ) == xe.GetAttribute( XmlTags.CIdAttr ) )
						{
							collectionList.Add( new ShallowNode( xe ) );
						}
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Gets a list of collections that the specified user is the owner of.
		/// </summary>
		/// <param name="userID">User identifier that is the owner.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that the specified user is
		/// the owner of.</returns>
		public ICSList GetCollectionsByOwner( string userID )
		{
			return GetCollectionsByOwner( userID, null );
		}

		/// <summary>
		/// Gets a list of collections that the specified user is the owner of.
		/// </summary>
		/// <param name="userID">User identifier that is the owner.</param>
		/// <param name="domainID">Domain identifier to filter the collections by. If this parameter is
		/// null, all collections are returned regardless of which domain they are in.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that the specified user is
		/// the owner of.</returns>
		public ICSList GetCollectionsByOwner( string userID, string domainID )
		{
			userID = userID.ToLower();
			domainID = ( domainID != null ) ? domainID.ToLower() : null;
			ICSList ownerList = new ICSList();

			// Get all of the collections that the user is a member of in the specified domain.
			ICSList collectionList = GetCollectionsByUser( userID );
			foreach ( ShallowNode sn in collectionList )
			{
				Collection c = new Collection( this, sn );
				if ( ( c.Owner.UserID == userID ) && ( ( domainID == null ) || ( c.Domain == domainID ) ) )
				{
					ownerList.Add( sn );
				}
			}

			return ownerList;
		}

		/// <summary>
		/// Gets all collections that contain node objects with the specified property.
		/// </summary>
		/// <param name="property">Property to search for contained in node objects.</param>
		/// <param name="op">Type of search operation.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// found Collection objects.</returns>
		public ICSList GetCollectionsByProperty( Property property, SearchOp op )
		{
			// Create a container object to hold all collections that match the specified user.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( property.Name, op, property.SearchString, property.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// Get the collection that this Member object belongs to.
						string collectionID = xe.GetAttribute( XmlTags.CIdAttr );
						
						// Get the collection object.
						XmlDocument cDoc = storageProvider.GetShallowRecord( collectionID );
						collectionList.Add( new ShallowNode( cDoc.DocumentElement[ XmlTags.ObjectTag ] ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		///  Gets all collections that have the specified type.
		/// </summary>
		/// <param name="type">String that contains the type of the collection(s) to search for.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that match the specified 
		/// type.</returns>
		public ICSList GetCollectionsByType( string type )
		{
			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			Property p = new Property( PropertyTags.Types, type );
			Persist.Query query = new Persist.Query( p.Name, SearchOp.Equal, p.SearchString, p.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// See if this element represents a collection.
						if ( xe.GetAttribute( XmlTags.IdAttr ) == xe.GetAttribute( XmlTags.CIdAttr ) )
						{
							collectionList.Add( new ShallowNode( xe ) );
						}
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Gets all collections that belong to the specified user.
		/// </summary>
		/// <param name="userID">User identifier.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection
		/// objects that matched the specified user.</returns>
		public ICSList GetCollectionsByUser( string userID )
		{
			// Create a container object to hold all collections that match the specified user.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( PropertyTags.Ace, SearchOp.Begins, userID, Syntax.String );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						// Get the collection that this Member object belongs to.
						string collectionID = xe.GetAttribute( XmlTags.CIdAttr );
						
						// Get the collection object.
						XmlDocument cDoc = storageProvider.GetShallowRecord( collectionID );
						collectionList.Add( new ShallowNode( cDoc.DocumentElement[ XmlTags.ObjectTag ], collectionID ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return collectionList;
		}

		/// <summary>
		/// Returns the collection that represents the database object.
		/// </summary>
		/// <returns>A LocalDatabase object that represents the local store. A null is returned if
		/// the database object does not exist.</returns>
		public LocalDatabase GetDatabaseObject()
		{
			LocalDatabase ldb = null;

			// See if the local database object has already been looked up.
			if ( localDb == null )
			{
				Persist.Query query = new Persist.Query( BaseSchema.ObjectType, SearchOp.Equal, NodeTypes.LocalDatabaseType, Syntax.String );
				Persist.IResultSet chunkIterator = storageProvider.Search( query );
				if ( chunkIterator != null )
				{
					char[] results = new char[ 4096 ];

					// Get the first set of results from the query.
					int length = chunkIterator.GetNext( ref results );
					if ( length > 0 )
					{
						// Set up the XML document so the data can be easily extracted.
						XmlDocument document = new XmlDocument();
						document.LoadXml( new string( results, 0, length ) );
						ldb = new LocalDatabase( this, new ShallowNode( document.DocumentElement[ XmlTags.ObjectTag ] ) );
						localDb = ldb.ID;
					}

					chunkIterator.Dispose();
				}
			}
			else
			{
				ldb = LocalDb;
			}

			return ldb;
		}

		/// <summary>
		/// Gets the Domain object from its ID.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>Domain object that the specified ID refers to if successful. Otherwise returns a null.</returns>
		public Domain GetDomain( string domainID )
		{
			return GetCollectionByID( domainID ) as Domain;
		}

		/// <summary>
		/// Gets a list of all of the domain objects.
		/// </summary>
		/// <returns>An ICSList object containing all of the domain objects.</returns>
		public ICSList GetDomainList()
		{
			return GetCollectionsByType( NodeTypes.DomainType );
		}

		/// <summary>
		/// Gets the domain credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">Identifier of the domain to get the credentials from.</param>
		/// <param name="userID">Gets the identifier of the domain user.</param>
		/// <param name="credentials">Gets the credentials for the domain.</param>
		/// <returns>The type of credentials.</returns>
		public CredentialType GetDomainCredentials( string domainID, out string userID, out string credentials )
		{
			return CurrentUser.GetDomainCredentials( domainID, out userID, out credentials );
		}

		/// <summary>
		/// Gets the Domain object that the specified user belongs to.
		/// </summary>
		/// <param name="userID">Identifier for the user.</param>
		/// <returns>Domain object that the specified user belongs to if successful. Otherwise returns a null.</returns>
		public Domain GetDomainForUser( string userID )
		{
			string domainID = CurrentUser.GetDomainFromUserID( userID.ToLower() );
			return ( domainID != null ) ? GetDomain( domainID ) : null;
		}

		/// <summary>
		/// Gets a list of collections that have been locked.
		/// </summary>
		/// <returns>An ICSList object containing ShallowNode objects that represent locked collections.</returns>
		public ICSList GetLockedCollections()
		{
			ICSList shallowList = new ICSList();

			// Get the list of locked collection IDs.
			string[] cidList = Collection.GetLockedList();
			if ( cidList != null )
			{
				foreach( string cid in cidList )
				{
					Collection c = GetNodeByID( cid, cid ) as Collection;
					shallowList.Add( new ShallowNode( c.Properties.PropertyRoot, cid ) );
				}
			}

			return shallowList;
		}

		/// <summary>
		/// Gets all Node objects that contain the specified property.
		/// </summary>
		/// <param name="property">Property to search for contained in Node objects.</param>
		/// <param name="op">Type of search operation.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the
		/// found Node objects.</returns>
		public ICSList GetNodesByProperty( Property property, SearchOp op )
		{
			// Create a container object to hold all nodes that match the specified user.
			ICSList nodeList = new ICSList();

			Persist.Query query = new Persist.Query( property.Name, op, property.SearchString, property.Type );
			Persist.IResultSet chunkIterator = storageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				while ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument document = new XmlDocument();
					document.LoadXml( new string( results, 0, length ) );

					foreach ( XmlElement xe in document.DocumentElement )
					{
						nodeList.Add( new ShallowNode( xe ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return nodeList;
		}

		/// <summary>
		/// Gets the first Collection object that matches the specified name.
		/// </summary>
		/// <param name="name">A string containing the name for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first Collection object that matches the specified name.  A null is 
		/// returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByName( string name )
		{
			Collection collection = null;
			ICSList collectionList = GetCollectionsByName( name );
			foreach ( ShallowNode sn in collectionList )
			{
				collection = new Collection( this, sn );
				break;
			}

			return collection;
		}

		/// <summary>
		/// Gets the first Collection object that matches the specified type.
		/// </summary>
		/// <param name="type">A string containing the type for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first Collection object that matches the specified type.  A null is 
		/// returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByType( string type )
		{
			Collection collection = null;
			ICSList collectionList = GetCollectionsByType( type );
			foreach ( ShallowNode sn in collectionList )
			{
				collection = new Collection( this, sn );
				break;
			}

			return collection;
		}

		/// <summary>
		/// Gets the user ID that the logged on user is known as in the specified domain.
		/// </summary>
		/// <param name="domainID">Well known domain identifier.</param>
		/// <returns>The user ID that the logged on user is known as in the specified domain.</returns>
		public string GetUserIDFromDomainID( string domainID )
		{
			return CurrentUser.GetUserIDFromDomain( domainID.ToLower() );
		}

		/// <summary>
		/// Sets credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain ID to set the password for.</param>
		/// <param name="credentials">Credentials for the domain.</param>
		/// <param name="credType">Type of credentials being stored.</param>
		public void SetDomainCredentials( string domainID, string credentials, CredentialType credType )
		{
			LocalDb.Commit( CurrentUser.SetDomainCredentials( domainID.ToLower(), credentials, credType ) );
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Method used by applications to enumerate the Collection objects contained in the Collection Store.
		/// </summary>
		/// <returns>IEnumerator object used to enumerate the Collection objects. IEnumerator will return
		/// ShallowNode objects that represent Collection objects.</returns>
		public IEnumerator GetEnumerator()
		{
			return new StoreEnumerator( this );
		}

		/// <summary>
		/// Enumerator class for the Store object that allows enumeration of the Collection objects
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
			/// Store object from which the collections are being enumerated.
			/// </summary>
			private Store store;

			/// <summary>
			/// The internal enumerator to use to enumerate all of the child nodes belonging to this node.
			/// </summary>
			private Persist.IResultSet chunkIterator = null;

			/// <summary>
			/// Array where the query results are stored.
			/// </summary>
			private char[] results = new char[ 4096 ];
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the StoreEnumerator class.
			/// </summary>
			/// <param name="storeObject">Store object where to enumerate the collections.</param>
			public StoreEnumerator( Store storeObject )
			{
				store = storeObject;
				Reset();
			}
			#endregion

			#region Properties
			/// <summary>
			/// Gets the total number of objects contained in the search.
			/// </summary>
			public int Count
			{
				get { return chunkIterator.Count; }
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
					throw new DisposedException( this );
				}

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// Create the collection query.
				Persist.Query query = new Persist.Query( PropertyTags.Types, SearchOp.Equal, NodeTypes.CollectionType, Syntax.String );
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

					if ( collectionEnumerator == null )
					{
						throw new InvalidOperationException( "Empty enumeration" );
					}

					return new ShallowNode( ( XmlElement )collectionEnumerator.Current );
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
					throw new DisposedException( this );
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
								// Out of data.
								collectionEnumerator = null;
							}
						}
						else
						{
							// Out of data.
							collectionEnumerator = null;
						}
					}
				}

				return moreData;
			}

			/// <summary>
			/// Set the cursor for the current search to the specified index.
			/// </summary>
			/// <param name="origin">The origin to move from.</param>
			/// <param name="offset">The offset to move the index by.</param>
			/// <returns>True if successful, otherwise false is returned.</returns>
			public bool SetCursor( IndexOrigin origin, int offset )
			{
				// Set the new index for the cursor.
				bool cursorSet = chunkIterator.SetIndex( origin, offset );
				if ( cursorSet )
				{
					// Get the next page of the results set.
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
						// Out of data.
						collectionEnumerator = null;
					}
				}

				return cursorSet;
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
	}
}
