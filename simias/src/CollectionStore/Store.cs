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
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;
using System.Xml;

using Simias;
using Simias.Event;
using Persist = Simias.Storage.Provider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

namespace Simias.Storage
{
	/// <summary>
	/// This is the top level object for the Collection Store.  The Store object can contain multiple 
	/// collection objects.
	/// </summary>
	public sealed class Store : IEnumerable, IDisposable
	{
		#region Class Members
		/// <summary>
		/// Used to log messages.
		/// </summary>
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		/// <summary>
		/// Cross process member used to control access to the constructor.
		/// </summary>
		private static string ctorLock = "";

		/// <summary>
		/// Directory where store-managed files are kept.
		/// </summary>
		private const string storeManagedDirectoryName = "CollectionFiles";

		/// <summary>
		/// Specifies whether object is viable.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Handle to the local store provider.
		/// </summary>
		private Persist.IProvider storageProvider;

		/// <summary>
		/// Path to where store managed files are kept.
		/// </summary>
		private string storeManagedPath;

		/// <summary>
		/// Uniquely defines this database.
		/// </summary>
		private string databaseID;

		/// <summary>
		/// Object that identifies the current owner or impersonator.
		/// </summary>
		private IdentityManager identityManager;

		/// <summary>
		/// Configuration object passed during connect.
		/// </summary>
		private Configuration config;

		/// <summary>
		/// Cross-process database lock function.
		/// </summary>
		private Mutex storeMutex;

		/// <summary>
		/// String that identifies the publisher of events for this object instance.
		/// </summary>
		private string publisher = "Unspecified";

		/// <summary>
		/// Used to publish collection store events.
		/// </summary>
		private EventPublisher eventPublisher;
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
		/// Gets whether the current executing user is being impersonated.
		/// </summary>
		internal bool IsImpersonating
		{
			get { return ( identityManager != null ) ? identityManager.IsImpersonating : false; }
		}

		/// <summary>
		/// Gets the Storage provider interface.
		/// </summary>
		internal Persist.IProvider StorageProvider
		{
			get { return storageProvider; }
		}

		/// <summary>
		/// Gets the configuration object passed to the store.Connect() method.
		/// </summary>
		public Configuration Config
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return config; 
			}
		}

		/// <summary>
		/// Gets the identity object associated with this store that represents the currently impersonating user.
		/// </summary>
		public BaseContact CurrentIdentity
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return identityManager.CurrentIdentity; 
			}
		}

		/// <summary>
		/// Gets the current impersonation identity for the store.
		/// </summary>
		public string CurrentUserGuid
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return identityManager.CurrentUserGuid; 
			}
		}

		/// <summary>
		/// Gets the domain name for the current user.
		/// </summary>
		public string LocalDomain
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return identityManager.DomainName;
			}
		}

		/// <summary>
		/// Gets the identifier for this Collection Store.
		/// </summary>
		public string ID
		{
			get { return databaseID; }
		}

		/// <summary>
		/// Gets the RsaKeyStore object that provides an interface for the secure remote authentication.
		/// </summary>
		public RsaKeyStore KeyStore
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return identityManager; 
			}
		}

		/// <summary>
		/// Gets or sets the publisher event source identifier.
		/// </summary>
		public string Publisher
		{
			get { return publisher; }
			set { publisher = value; }
		}

		/// <summary>
		/// Gets the public key for the server.
		/// </summary>
		public RSACryptoServiceProvider ServerPublicKey
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return identityManager.PublicKey; 
			} 
		}

		/// <summary>
		///  Specifies where the default store path
		/// </summary>
		public string StorePath
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return storageProvider.StoreDirectory.LocalPath; 
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the Store object.
		/// </summary>
		/// <param name="config">A Configuration object that contains a path that specifies where to create
		/// or open the database.</param>
		public Store( Configuration config )
		{
			lock( ctorLock )
			{
				bool created;

				// Store the configuration that opened this instance.
				this.config = config;

				// Setup the event publisher object.
				eventPublisher = new EventPublisher( config );

				// Create or open the underlying database.
				storageProvider = Persist.Provider.Connect( new Persist.ProviderConfig( config ), out created );

				// Set the path to the store.
				storeManagedPath = Path.Combine( storageProvider.StoreDirectory.LocalPath, storeManagedDirectoryName );

				// Either create the store or authenticate to it.
				if ( created )
				{
					try
					{
						// Create a domain name for this domain.
						string domainName = Environment.UserDomainName + ":" + Guid.NewGuid().ToString().ToLower();

						// Create an identifier for the owner of this Collection Store.
						string ownerGuid = Guid.NewGuid().ToString();

						// Create the database lock.
						storeMutex = new Mutex( false, domainName );

						// Create the local address book.
						LocalAddressBook localAb = new LocalAddressBook( this, domainName, Guid.NewGuid().ToString(), ownerGuid, domainName );

						// Create an identity that represents the current user.  This user will become the 
						// database owner.
						BaseContact ownerIdentity = new BaseContact( Environment.UserName, ownerGuid );

						// Add a key pair to this identity to be used as credentials.
						ownerIdentity.CreateKeyPair();

						// Set the identity into the manager object.
						identityManager = new IdentityManager( domainName, localAb, ownerIdentity );

						// Add the well-known world identity and save the address book changes.
						Node[] identities = { localAb, ownerIdentity, new BaseContact( "World", Access.World ) };
						localAb.Commit( identities );

						// Create the default local workspace.
						WorkGroup workGroup = new WorkGroup( this, Environment.UserDomainName, Guid.NewGuid().ToString(), ownerGuid );
						workGroup.Commit();

						// Create an object that represents the database collection.
						LocalDatabase localDb = new LocalDatabase( this, workGroup, ownerGuid, domainName );
						localDb.Commit();

						// Set the database ID.
						databaseID = localDb.ID;

						// Create the collision container.
						Collision collision = new Collision( this );
						collision.Commit();
					}
					catch ( Exception e )
					{
						// Log this error.
						log.Fatal( e, "Could not initialize Collection Store." );

						// The store didn't initialize delete it and rethrow the exception.
						storageProvider.DeleteStore();
						storageProvider.Dispose();
						throw;
					}
				}
				else
				{
					// Get the local address book.
					LocalAddressBook localAb = GetLocalAddressBook();
					if ( localAb == null )
					{
						throw new DoesNotExistException( "Local address book does not exist." );
					}

					// Look up to see if the current user has an identity.
					BaseContact identity = localAb.GetContactByName( Environment.UserName );
					if ( identity == null )
					{
						throw new DoesNotExistException( String.Format( "User: {0} does not exist in local address book.", Environment.UserName ) );
					}

					// Create a identity manager object that will be used by the store object from here on out.
					identityManager = new IdentityManager( localAb.Name, localAb, identity );

					// Set the database ID.
					databaseID = GetDatabaseObject().ID;

					// Create the database lock.
					storeMutex = new Mutex( false, identityManager.DomainName );
				}

				// Impersonate the current user so that access control will work.
				identityManager.Impersonate( identityManager.CurrentUserGuid );
			}
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
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			return Path.Combine( storeManagedPath, collectionID.ToLower() );
		}

		/// <summary>
		/// Acquires the store lock protecting the database against simultaneous commits.
		/// </summary>
		internal void LockStore()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			storeMutex.WaitOne();
		}

		/// <summary>
		/// Releases the store lock.
		/// </summary>
		internal void UnlockStore()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			storeMutex.ReleaseMutex();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Deletes the persistent store database and disposes this object.
		/// </summary>
		public void Delete()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			// Check if the store managed path still exists. If it does, delete it.
			if ( Directory.Exists( storeManagedPath ) )
			{
				Directory.Delete( storeManagedPath, true );
			}

			// Say bye-bye to the store.
			storageProvider.DeleteStore();
			Dispose();
		}

		/// <summary>
		/// Returns a Collection object for the specified identifier.
		/// </summary>
		/// <param name="collectionID">Globally unique identifier for the object.</param>
		/// <returns>A Collection object for the specified identifier.  If the object doesn't 
		/// exist a null is returned.</returns>
		public Collection GetCollectionByID( string collectionID )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			Collection collection = null;

			// Get the specified object from the persistent store.
			string normalizedID = collectionID.ToLower();
			XmlDocument document = storageProvider.GetRecord( normalizedID, normalizedID );
			if ( document != null )
			{
				collection = Node.NodeFactory( this, document ) as Collection;
				
				// Make sure that access is allowed to the Collection object.
				if ( !collection.IsAccessAllowed( Access.Rights.ReadOnly ) )
				{
					// Don't return Collections that the user does not have any rights to.
					collection = null;
				}
			}

			return collection;
		}

		/// <summary>
		///  Gets all collections that have the specified name.
		/// </summary>
		/// <param name="name">A string containing the name of the collection(s) to search for.
		/// This parameter may be specified as a regular expression.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection 
		/// objects that matched the specified name.</returns>
		public ICSList GetCollectionsByName( string name )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			// Build a regular expression class to use as the comparision.
			Regex searchName = new Regex( "^" + name + "$", RegexOptions.IgnoreCase );

			// Look at each collection that this user has rights to and match up on the name.
			foreach ( ShallowNode shallowNode in this )
			{
				if ( searchName.IsMatch( shallowNode.Name ) )
				{
					collectionList.Add( shallowNode );
				}
			}

			return collectionList;
		}

		/// <summary>
		///  Gets all collections that have the specified type.
		/// </summary>
		/// <param name="type">String that contains the type of the collection(s) to search for.
		/// This parameter may be specified as a regular expression.</param>
		/// <returns>An ICSList object containing the ShallowNode objects that match the specified 
		/// type.</returns>
		public ICSList GetCollectionsByType( string type )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			// Build a regular expression class to use as the comparision.
			Regex searchType = new Regex( "^" + type + "$", RegexOptions.IgnoreCase );

			// Look at each collection that this user has rights to and match up on the name.
			foreach ( ShallowNode shallowNode in this )
			{
				Collection collection = Collection.CollectionFactory( this, shallowNode );
				MultiValuedList mvl = collection.Properties.FindValues( PropertyTags.Types );
				foreach ( Property property in mvl )
				{
					if ( searchType.IsMatch( property.ToString() ) )
					{
						collectionList.Add( shallowNode );
						break;
					}
				}
			}

			return collectionList;
		}

		/// <summary>
		/// Returns the Collision object for this store.
		/// </summary>
		/// <returns>The Collision object for this store. A null is returned if the Collision object 
		/// does not exist.</returns>
		public Collision GetCollisionObject()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			return GetCollectionByID( Collision.CollisionID ) as Collision;
		}

		/// <summary>
		/// Returns the collection that represents the database object.
		/// </summary>
		/// <returns>A LocalDatabase object that represents the local store. A null is returned if
		/// the database object does not exist.</returns>
		public LocalDatabase GetDatabaseObject()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			LocalDatabase localDb = null;

			Persist.Query query = new Persist.Query( PropertyTags.LocalDatabase, SearchOp.Equal, "true", Syntax.Boolean );
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
					localDb = new LocalDatabase( this, new ShallowNode( document.DocumentElement[ XmlTags.ObjectTag ] ) );
				}

				chunkIterator.Dispose();
			}

			return localDb;
		}

		/// <summary>
		/// Gets the local address book for this Collection Store.
		/// </summary>
		/// <returns>A LocalAddressBook object.</returns>
		public LocalAddressBook GetLocalAddressBook()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			LocalAddressBook localAb = null;

			Persist.Query query = new Persist.Query( PropertyTags.LocalAddressBook, SearchOp.Equal, "true", Syntax.Boolean );
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
					localAb = new LocalAddressBook( this, new ShallowNode( document.DocumentElement[ XmlTags.ObjectTag ] ) );
				}

				chunkIterator.Dispose();
			}

			return localAb;
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
			if ( disposed )
			{
				throw new DisposedException( this );
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
		/// Gets the first Collection object that matches the specified type.
		/// </summary>
		/// <param name="type">A string containing the type for the collection. This parameter may be
		/// specified as a regular expression.</param>
		/// <returns>The first Collection object that matches the specified type.  A null is 
		/// returned if no matching collections are found.</returns>
		public Collection GetSingleCollectionByType( string type )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
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
		public void ImpersonateUser( string userGuid )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			identityManager.Impersonate( userGuid.ToLower() );
		}

		/// <summary>
		/// Reverts the user security context back to the previous owner.
		/// </summary>
		public void Revert()
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			identityManager.Revert();
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
			if ( disposed )
			{
				throw new DisposedException( this );
			}

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
			private char[] results = new char[ 4096 ];

			/// <summary>
			/// Enumerator used to enumerate all IDs that the user is known as.
			/// </summary>
			private IEnumerator IDEnumerator;
			#endregion

			#region Constructor
			/// <summary>
			/// Constructor for the StoreEnumerator class.
			/// </summary>
			/// <param name="storeObject">Store object where to enumerate the collections.</param>
			public StoreEnumerator( Store storeObject )
			{
				store = storeObject;

				// Get all the identities that he is known as.
				IDEnumerator = store.CurrentIdentity.GetIdentityAndAliases().GetEnumerator();
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
				// Create the collection query.
				Persist.Query query = new Persist.Query( PropertyTags.Ace, SearchOp.Begins, currentUserGuid, Syntax.String );

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
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				// Release previously allocated chunkIterator.
				if ( chunkIterator != null )
				{
					chunkIterator.Dispose();
				}

				// Start at the beginning of the id list and make sure there are entries in the list.
				IDEnumerator.Reset();
				if ( IDEnumerator.MoveNext() )
				{
					CollectionQuery( ( string )IDEnumerator.Current );
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
								// See if there are any more IDs to enumerate.
								if ( ( IDEnumerator != null ) && IDEnumerator.MoveNext() )
								{
									CollectionQuery( ( string )IDEnumerator.Current );
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
							// See if there are more IDs to enumerate.
							if ( ( IDEnumerator != null ) && IDEnumerator.MoveNext() )
							{
								CollectionQuery( ( string )IDEnumerator.Current );
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
