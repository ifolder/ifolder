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
		/// Directory where store-managed files are kept.
		/// </summary>
		static private string storeManagedDirectoryName = "CollectionFiles";

		/// <summary>
		/// XML configuration tags used to get the enterprise user name.
		/// </summary>
		static private string LdapAuthenticationTag = "LdapAuthentication";
		static private string ProxyDNTag = "ProxyDN";

		/// <summary>
		/// Default sync interval for the enterprise domain. Only synchronizes once a day.
		/// </summary>
		static private int DefaultDomainSyncInterval = 86400;

		/// <summary>
		/// Default sync interval for the machine. Synchronizes every 5 minutes.
		/// </summary>
		static private int DefaultMachineSyncInterval = 300;

		/// <summary>
		/// Handle to the local store provider.
		/// </summary>
		private Persist.IProvider storageProvider = null;

		/// <summary>
		/// Path to where store managed files are kept.
		/// </summary>
		private string storeManagedPath;

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
		/// Gets the Identity object that represents the currently logged on user.
		/// </summary>
		internal Identity CurrentUser
		{
			get { return GetNodeByID( localDb, identity ) as Identity; }
		}

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
			if ( config.Exists( Domain.SectionName, Domain.EnterpriseName ) )
			{
				// This instance is running on an enterprise server.
				enterpriseServer = true;
			}

			// Setup the event publisher object.
			eventPublisher = new EventPublisher();

			// Initialize the node cache.
			cache = new NodeCache( this );

			// Create or open the underlying database.
			storageProvider = Persist.Provider.Connect( new Persist.ProviderConfig(), out created );

			// Set the path to the store.
			storeManagedPath = Path.Combine( storageProvider.StoreDirectory.LocalPath, storeManagedDirectoryName );

			// Either create the store or authenticate to it.
			if ( created )
			{
				try
				{
					// Default the user name to the machine user name.
					string userName = Environment.UserName;
					Uri localUri = null;

					// Does the configuration indicate that this is an enterprise server?
					if ( enterpriseServer )
					{
						// Set the local uri address for the domains. This will have to change
						// when we support upstream servers.
						localUri = new UriBuilder( Uri.UriSchemeHttp, MyDns.GetHostName(), 80, "simias10" ).Uri;

						// Get the name of the user to create as the identity.
						string proxyName = config.Get( LdapAuthenticationTag, ProxyDNTag, null );
						if ( proxyName != null )
						{
							userName = ParseUserName( proxyName );
						}
					}
					else
					{
						// Set the local uri address from the config file since this is a workgroup machine.
						Uri tempUri = Manager.LocalServiceUrl;
						if ( tempUri != null )
						{
							// If the host address is loopback, convert it to the endpoint address.
							string hostAddress = IPAddress.IsLoopback( IPAddress.Parse( tempUri.Host ) ) ? 
								MyDns.GetHostName() : tempUri.Host;

							localUri = new UriBuilder( tempUri.Scheme, hostAddress, tempUri.Port, tempUri.PathAndQuery ).Uri;
						}
						else
						{
							// This is used for when running Simias without calling Manager.Start(). This is intended
							// for debug mode only.
							localUri = new UriBuilder( Uri.UriSchemeHttp, MyDns.GetHostName(), 8086, "simias10/" + Environment.UserName ).Uri;
						}
					}

					// Create an object that represents the database collection.
					string localDomainID = Guid.NewGuid().ToString();
					LocalDatabase ldb = new LocalDatabase( this, localDomainID );
					localDb = ldb.ID;

					// Create an identity that represents the current user.  This user will become the 
					// database owner. Add the domain mapping to the identity.
					Identity owner = new Identity( userName, Guid.NewGuid().ToString() );
					identity = owner.ID;

					// Create a credential to be used to identify the local user.
					RSACryptoServiceProvider credential = new RSACryptoServiceProvider( 1024 );
					owner.AddDomainIdentity( identity, localDomainID, credential.ToXmlString( true ), CredentialType.PPK );

					// Create a member object that will own the local database.
					Member member = new Member( owner.Name, owner.ID, Access.Rights.Admin );
					member.IsOwner = true;

					// Save the local database changes.
					ldb.Commit( new Node[] { ldb, member, owner } );

					// Create the local domain.
					Domain localDomain = new Domain( this, "Local", localDomainID, "Local Machine Domain", SyncRoles.Local, localUri );
					Member localDomainOwner = new Member( owner.Name, owner.ID, Access.Rights.Admin );
					localDomainOwner.IsOwner = true;
					localDomain.Commit( new Node[] { localDomain, localDomainOwner } );

					// Create a SyncInterval policy.
					SyncInterval.Create( DefaultMachineSyncInterval );

					// See if there is a configuration parameter for an enterprise domain.
					if ( IsEnterpriseServer )
					{
						// Get the name of the enterprise domain.
						string enterpriseName = config.Get( Domain.SectionName, Domain.EnterpriseName, String.Empty );
						if ( enterpriseName == String.Empty )
						{
							throw new CollectionStoreException( "Enterprise name is empty." );
						}

						// Check if an enterprise ID was specified or if it needs to be generated.
						string enterpriseID = config.Get( Domain.SectionName, Domain.EnterpriseID, Guid.NewGuid().ToString().ToLower() );

						// Check if there is a description for this enterprise domain.
						string description = config.Get( Domain.SectionName, Domain.EnterpriseDescription, String.Empty );

						// Create the enterprise domain.
						Domain enterpriseDomain = new Domain( this, enterpriseName, enterpriseID, description, SyncRoles.Master, localUri );
						enterpriseDomain.SetType( enterpriseDomain, "Enterprise" );
						Member enterpriseDomainOwner = new Member( owner.Name, owner.ID, Access.Rights.Admin );
						enterpriseDomainOwner.IsOwner = true;
						enterpriseDomain.Commit( new Node[] { enterpriseDomain, enterpriseDomainOwner } );

						// Create the identity mapping.
						owner.AddDomainIdentity( owner.ID, enterpriseID );

						// Add the enterprise domain as the default domain.
						ldb.DefaultDomain = enterpriseID;
						ldb.Commit( new Node[] { ldb, owner } );

						// Create a default sync interval policy for this domain.
						SyncInterval.Create( enterpriseDomain, DefaultDomainSyncInterval );
					}
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
		/// Gets a list of collections that have been locked.
		/// </summary>
		/// <param name="lockType">The reason that the collection was locked.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent locked collections.</returns>
		private ICSList GetLockedList( string lockType )
		{
			ICSList lockList = new ICSList();	

			// Query for the locked attribute - either for existence or a specific type.
			SearchOp op = ( lockType == String.Empty ) ? SearchOp.Exists : SearchOp.Equal;
			Persist.Query query = new Persist.Query( PropertyTags.CollectionLock, op, lockType, Syntax.Int32 );
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
						lockList.Add( new ShallowNode( xe ) );
					}

					// Get the next set of results from the query.
					length = chunkIterator.GetNext( ref results );
				}

				chunkIterator.Dispose();
			}

			return lockList;
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
					cache.Add( collectionID, node );
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
		/// Acquires the store lock protecting the database against simultaneous commits.
		/// </summary>
		internal void LockStore()
		{
			storeMutex.WaitOne();
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

			Persist.Query query = new Persist.Query( BaseSchema.ObjectName, searchOp, name, Syntax.String );
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

			Persist.Query query = new Persist.Query( property.Name, op, property.ToString(), property.Type );
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

			Persist.Query query = new Persist.Query( PropertyTags.Types, SearchOp.Equal, type, Syntax.String );
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
		/// <returns>An ICSList object containing locked collections.</returns>
		public ICSList GetLockedCollections()
		{
			return GetLockedList( String.Empty );
		}

		/// <summary>
		/// Gets a list of collections that have been locked.
		/// </summary>
		/// <param name="lockType">The reason that the collection was locked.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent locked collections.</returns>
		public ICSList GetLockedCollections( Collection.LockType lockType )
		{
			return GetLockedList( Enum.Format( typeof( Collection.LockType ), lockType, "d" ) );
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

			Persist.Query query = new Persist.Query( property.Name, op, property.ToString(), property.Type );
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
