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
using System.IO;
using System.Security.Cryptography;
using System.Xml;

using Simias;
using Persist = Simias.Storage.Provider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

namespace Simias.Storage
{
	/// <summary>
	/// </summary>
	public sealed class StoreService : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Directory where store-managed files are kept.
		/// </summary>
		private const string storeManagedDirectoryName = "CollectionFiles";

		/// <summary>
		/// Specifies whether object is viable.
		/// </summary>
		private bool disposed = false;

		/// <summary>
		/// Path to where store managed files are kept.
		/// </summary>
		private string storeManagedPath;

		/// <summary>
		/// Object that identifies the current owner or impersonator.
		/// </summary>
		private IdentityManager identityManager;

		/// <summary>
		/// Handle to the local store provider.
		/// </summary>
		private Persist.IProvider storageProvider;

		/// <summary>
		/// Configuration object passed during connect.
		/// </summary>
		private Configuration config;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the current impersonation identity for the store.
		/// </summary>
		public string CurrentUserGuid
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return identityManager.CurrentUserGuid; 
			}
		}

		/// <summary>
		/// Gets the domain name for the current user.
		/// </summary>
		public string DomainName
		{
			get 
			{ 
				if ( disposed )
				{
					throw new ObjectDisposedException( this.ToString() );
				}

				return identityManager.DomainName; 
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
					throw new ObjectDisposedException( this.ToString() );
				}

				return identityManager.CurrentIdentity; 
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
					throw new ObjectDisposedException( this.ToString() );
				}

				return storageProvider.StoreDirectory.LocalPath; 
			}
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
					throw new ObjectDisposedException( this.ToString() );
				}

				return identityManager; 
			}
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
					throw new ObjectDisposedException( this.ToString() );
				}

				return identityManager.PublicKey; 
			} 
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
					throw new ObjectDisposedException( this.ToString() );
				}

				return config; 
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the Store object.
		/// </summary>
		/// <param name="config">A Configuration object that contains a path that specifies where to create
		/// or open the database.</param>
		private StoreService( Configuration config )
		{
			bool created;

			// Store the configuration that opened this instance.
			this.config = config;

			// Create or open the underlying database.
			storageProvider = Persist.Provider.Connect( config.BasePath, out created );

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

					// Create the local address book.
					LocalAddressBook localAb = new LocalAddressBook( this, domainName, ownerGuid, domainName );

					// Create an identity that represents the current user.  This user will become the 
					// database owner.
					BaseContact ownerIdentity = new BaseContact( Environment.UserName, ownerGuid );

					// Add a key pair to this identity to be used as credentials.
					ownerIdentity.CreateKeyPair();

					// Set the identity into the manager object.
					identityManager = new IdentityManager( domainName, localAb, ownerIdentity );

					// Add the well-known world identity and save the address book changes.
					BaseNode[] identities = new BaseNode[ 2 ];
					identities[ 0 ] = ownerIdentity;
					identities[ 1 ] = new BaseContact( "World", Access.World );
					Commit( localAb );
					Commit( identities );

					// Create an object that represents the database collection.
					BaseCollection localDb = new BaseCollection( "LocalDatabase", ownerGuid, domainName );
					localDb.Properties.AddNodeProperty( Property.LocalDatabase, true );
					localDb.Properties.AddNodeProperty( Property.Syncable, false );
					Commit( localDb );
				}
				catch ( Exception e )
				{
					// TODO: Log this error.
					Console.WriteLine( "Could not initialize Collection Store. Caught exception: " + e.Message );

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
					throw new ApplicationException( "Local address book does not exist." );
				}

				// Look up to see if the current user has an identity.
				BaseContact identity = new BaseContact( GetSingleNodeByName( localAb, Environment.UserName ) );
				if ( identity == null )
				{
					throw new ApplicationException( "User does not exist in local address book." );
				}

				// Create a identity manager object that will be used by the store object from here on out.
				identityManager = new IdentityManager( localAb.Name, localAb, identity );
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
				throw new ObjectDisposedException( this.ToString() );
			}

			return Path.Combine( storeManagedPath, collectionID.ToLower() );
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
				throw new ObjectDisposedException( this.ToString() );
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
		/// Returns a BaseCollection object for the specified identifier.
		/// </summary>
		/// <param name="collectionID">Globally unique identifier for the object.</param>
		/// <returns>A BaseCollection object for the specified identifier.  If the object doesn't 
		/// exist a null is returned.</returns>
		public BaseCollection GetCollectionByID( string collectionID )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			BaseCollection collection = null;

			// Get the specified object from the persistent store.
			string normalizedID = collectionID.ToLower();
			string xmlString = storageProvider.GetRecord( normalizedID, normalizedID );
			if ( xmlString != null )
			{
				collection = new BaseCollection( new XmlDocument().LoadXml( xmlString ) );
			}

			return collection;
		}

		/// <summary>
		/// Searches the store for collections that match the specified property.
		/// </summary>
		/// <param name="propertyName">Name of the property to search for.</param>
		/// <param name="propertyValue">Value of the property to search for.</param>
		/// <param name="propertySyntax">Syntax type of property.</param>
		/// <param name="searchOperator">Query operator.</param>
		/// <returns>  An enumerator is returned that returns all collections that match the query criteria.</returns>
		public Persist.IResultSet CollectionSearch( string propertyName, SearchOp searchOperator, string propertyValue, Syntax propertySyntax )
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			return storageProvider.Search( new Persist.Query( propertyName, searchOperator, propertyValue, propertySyntax ) );
		}

		/// <summary>
		/// Returns the collection that represents the database object.
		/// </summary>
		/// <returns>A BaseCollection object that represents the local database. A null is returned if
		/// the database object does not exist.</returns>
		public BaseCollection GetDatabaseObject()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			BaseCollection localDb = null;

			Persist.IResultSet chunkIterator = CollectionSearch( Property.LocalDatabase, SearchOp.Equal, "true", Syntax.Boolean );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				if ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					localDb = new BaseCollection( new XmlDocument().LoadXml( new string( results, 0, length ) ) );
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
				throw new ObjectDisposedException( this.ToString() );
			}

			LocalAddressBook localAb = null;

			Persist.IResultSet chunkIterator = CollectionSearch( Property.LocalAddressBook, SearchOp.Equal, "true", Syntax.Boolean );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				if ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					localAb = new LocalAddressBook( new XmlDocument().LoadXml( new string( results, 0, length ) ) );
				}

				chunkIterator.Dispose();
			}

			return localAb;
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

			identityManager.Impersonate( userGuid.ToLower() );
		}

		/// <summary>
		/// Imports an xml document representing a collection store node into the specified collection.
		/// </summary>
		/// <param name="collection">Collection to import the node into.</param>
		/// <param name="signedDocument">Digitally signed xml document that contains the node.</param>
		/// <returns>Imported node object instantiated from the xml document.</returns>
/*		public Node ImportSingleNodeFromXml( Collection collection, XmlDocument signedDocument )
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
*/
		/// <summary>
		/// Reverts the user security context back to the previous owner.
		/// </summary>
		public void Revert()
		{
			if ( disposed )
			{
				throw new ObjectDisposedException( this.ToString() );
			}

			identityManager.Revert();
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
		~StoreService()      
		{
			Dispose( false );
		}
		#endregion
	}
}
