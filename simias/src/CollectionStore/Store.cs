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
		static private readonly ISimiasLog log = SimiasLogManager.GetLogger( typeof( Store ) );

		/// <summary>
		/// Cross process member used to control access to the constructor.
		/// </summary>
		static private string ctorLock = "";

		/// <summary>
		/// Directory where store-managed files are kept.
		/// </summary>
		static private string storeManagedDirectoryName = "CollectionFiles";

		/// <summary>
		/// Specifies whether object is viable.
		/// </summary>
		private bool disposed = false;

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
		private SimiasMutex storeMutex;

		/// <summary>
		/// String that identifies the publisher of events for this object instance.
		/// </summary>
		private string publisher = "Unspecified";

		/// <summary>
		/// Used to publish collection store events.
		/// </summary>
		private EventPublisher eventPublisher;

		/// <summary>
		/// Represents the current logged on user.
		/// </summary>
		private Identity identity;

		/// <summary>
		/// Used so the object does not have to be looked up everytime. Take caution
		/// in using this object because it may be stale. 
		/// </summary>
		private LocalDatabase localDb = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the Identity object that represents the currently logged on user.
		/// </summary>
		internal Identity CurrentUser
		{
			get { return identity; }
		}

		/// <summary>
		/// Gets the event publisher object.
		/// </summary>
		internal EventPublisher EventPublisher
		{
			get { return eventPublisher; }
		}

		/// <summary>
		/// Gets the cached (and therefore possibly stale) local database reference.
		/// If you don't care about the properties on the object itself, it is safe
		/// to use this object.
		/// </summary>
		internal LocalDatabase LocalDb
		{
			get { return localDb; }
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
		/// Gets or sets the default domain ID.
		/// </summary>
		public string DefaultDomain
		{
			get { return ( localDb.Refresh() as LocalDatabase ).DefaultDomain; }
			set
			{
				localDb.DefaultDomain = value;
				localDb.Commit();
			}
		}

		/// <summary>
		/// Gets the identifier for this Collection Store.
		/// </summary>
		public string ID
		{
			get 
			{ 
				if ( disposed )
				{
					throw new DisposedException( this );
				}

				return localDb.ID; 
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
						ArrayList nodeList = new ArrayList();

						// Create an object that represents the database collection.
						localDb = new LocalDatabase( this );
						nodeList.Add( localDb );

						// Create the database lock.
						storeMutex = new SimiasMutex( ID );

						// Create an identity that represents the current user.  This user will become the 
						// database owner. Add the domain mapping to the identity.
						identity = new Identity( this, Environment.UserName, Guid.NewGuid().ToString() );
						nodeList.Add( identity );

						// Create the default workgroup domain.
						Domain wgDomain = new Domain( Domain.WorkGroupDomainName, Domain.WorkGroupDomainID );
						nodeList.Add( wgDomain );

						// Create an identity mapping for the workgroup.
						identity.AddDomainIdentity( identity.ID, wgDomain.ID );

						// Create an empty roster for the workgroup domain.
						Roster wgRoster = new Roster( this, wgDomain );
						wgRoster.Commit();

						// See if there is a configuration parameter for an enterprise domain.
						if ( config.Exists( Domain.SectionName, Domain.EnterpriseName ) )
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

							// Create the new domain object.
							Domain eDomain = new Domain( enterpriseName, enterpriseID, description );
							nodeList.Add( eDomain );

							// Add the domain identity mapping.
							identity.AddDomainIdentity( identity.ID, eDomain.ID );

							// Add the enterprise domain as the default domain.
							localDb.DefaultDomain = eDomain.ID;

							// Create a domain roster that will contain the member of the domain.
							Roster roster = new Roster( this, eDomain );
							roster.Commit();
						}

						// Save the local database changes.
						localDb.Commit( nodeList.ToArray( typeof( Node ) ) as Node[] );
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
					if ( GetDatabaseObject() == null )
					{
						throw new DoesNotExistException( "Local database object does not exist." );
					}

					// Get the identity object that represents this logged on user.
					ICSList list = localDb.Search( BaseSchema.ObjectType, NodeTypes.IdentityType, SearchOp.Equal );
					foreach ( ShallowNode sn in list )
					{
						if ( sn.Name == Environment.UserName )
						{
							identity = new Identity( localDb, sn );
							break;
						}
					}

					// Make sure that the identity was found.
					if ( identity == null )
					{
						throw new DoesNotExistException( String.Format( "User {0} does not exist in the database.", Environment.UserName ) );
					}

					// Create the database lock.
					storeMutex = new SimiasMutex( ID );
				}
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
		/// Adds a domain identity to the Collection Store.
		/// </summary>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		/// <param name="domainName">Name of the domain.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="domainDescription">String that describes the specified domain.</param>
		/// <returns>The created Domain object.</returns>
		public Domain AddDomainIdentity( string userID, string domainName, string domainID, string domainDescription )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			Node[] nodeList = new Node[ 2 ];

			// Create the domain object.
			Domain domain = new Domain( domainName, domainID.ToLower(), domainDescription );
			nodeList[ 0 ] = domain;
			nodeList[ 1 ] = identity.AddDomainIdentity( userID.ToLower(), domainID.ToLower() );
			
			// Commit the changes.
			localDb.Commit( nodeList );
			return domain;
		}

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
		/// Removes the specified domain identity from the Collection Store.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		public void DeleteDomainIdentity( string domainID )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			Node[] nodeList = new Node[ 2 ];

			// Delete the domain object.
			nodeList[ 0 ] = localDb.Delete( GetDomain( domainID ) );
			nodeList[ 1 ] = identity.DeleteDomainIdentity( localDb, domainID.ToLower() );

			// Commit the changes.
			localDb.Commit( nodeList );
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

			// Get the specified object from the persistent store.
			string normalizedID = collectionID.ToLower();
			XmlDocument document = storageProvider.GetRecord( normalizedID, normalizedID );
			return ( document != null ) ? Node.NodeFactory( this, document ) as Collection : null;
		}

		/// <summary>
		/// Gets all collections that belong to the specified domain.
		/// </summary>
		/// <param name="domainID">Domain identifier.</param>
		/// <returns>An ICSList object containing ShallowNode objects that represent the Collection
		/// objects that matched the specified domain.</returns>
		public ICSList GetCollectionsByDomain( string domainID )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			// Create a container object to hold all collections that match the specified domain.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( PropertyTags.DomainID, SearchOp.Equal, domainID, Syntax.String );
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
						collectionList.Add( new ShallowNode( xe ) );
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
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			// Create a container object to hold all collections that match the specified name.
			ICSList collectionList = new ICSList();

			Persist.Query query = new Persist.Query( BaseSchema.ObjectName, SearchOp.Equal, name, Syntax.String );
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
		///  Gets all collections that have the specified type.
		/// </summary>
		/// <param name="type">String that contains the type of the collection(s) to search for.</param>
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
			if ( disposed )
			{
				throw new DisposedException( this );
			}

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
						localDb = new LocalDatabase( this, new ShallowNode( document.DocumentElement[ XmlTags.ObjectTag ] ) );
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

		/// <summary>
		/// Gets the Domain object from its ID.
		/// </summary>
		/// <param name="domainID">Identifier for the domain.</param>
		/// <returns>Domain object that the specified ID refers to if successful. Otherwise returns a null.</returns>
		public Domain GetDomain( string domainID )
		{
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			return localDb.GetNodeByID( domainID.ToLower() ) as Domain;
		}

		/// <summary>
		/// Gets the Domain object that the specified user belongs to.
		/// </summary>
		/// <param name="userID">Identifier for the user.</param>
		/// <returns>Domain object that the specified user belongs to if successful. Otherwise returns a null.</returns>
		public Domain GetDomainForUser( string userID )
		{
			Domain domain = null;

			string domainID = identity.GetDomainFromUserID( localDb, userID.ToLower() );
			if ( domainID != null )
			{
				domain = localDb.GetNodeByID( domainID ) as Domain;
			}

			return domain;
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
			if ( disposed )
			{
				throw new DisposedException( this );
			}

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
			if ( disposed )
			{
				throw new DisposedException( this );
			}

			return identity.GetUserIDFromDomain( localDb, domainID.ToLower() );
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
