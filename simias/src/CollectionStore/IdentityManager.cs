/***********************************************************************
 *  IdentityManager.cs - Class that implements identity management which
 *  provides access control to collection objects in the store.
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
using System.Security.Cryptography;
using System.Xml;
using Persist = Simias.Storage.Provider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

namespace Simias.Storage
{
	/// <summary>
	/// Implements the identity manager which is used to control access to Collection Store objects.
	/// There is only one identity that is allowed to authenticate to the CollectionStore database, 
	/// since the database only ever has one owner.  All other identities can access the database 
	/// only by impersonation.
	/// </summary>
	internal class IdentityManager : RsaKeyStore
	{
		#region Class Members
		/// <summary>
		/// Store object.
		/// </summary>
		private Store store;

		/// <summary>
		/// Name of this domain that the store object belongs in.
		/// </summary>
		private string domainName;

		/// <summary>
		/// Represents the identity of the user that instantiated this object.
		/// </summary>
		private string identityGuid;

		/// <summary>
		/// Holds the public key for the server.
		/// </summary>
		private RSACryptoServiceProvider publicKey;

		/// <summary>
		/// Container used to keep track of the current identity for this store handle.
		/// </summary>
		private Stack impersonationId = new Stack();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the currently impersonating user guid.
		/// </summary>
		public string CurrentUserGuid
		{
			get { return ( impersonationId.Count == 0 ) ? identityGuid : impersonationId.Peek() as string; }
		}

		/// <summary>
		/// Gets the identity object for the non-impersonating user.
		/// </summary>
		private Identity BaseIdentity
		{
			get { return AddressBook.GetIdentityById( identityGuid ); }
		}

		/// <summary>
		/// Gets the local address book.
		/// </summary>
		private LocalAddressBook AddressBook
		{
			get { return GetLocalAddressBook(); }
		}

		/// <summary>
		/// Gets the current impersonating identity.
		/// </summary>
		public Identity CurrentIdentity
		{
			get 
			{ 
				// Figure out if the current identity is being impersonated.
				string currentGuid = ( impersonationId.Count == 0 ) ? identityGuid : impersonationId.Peek() as string; 

				// Get this identity from the local address book.
				return ( AddressBook != null ) ? AddressBook.GetIdentityById( currentGuid ) : null;
			}
		}

		/// <summary>
		/// Gets the domain name where this identity exists.
		/// </summary>
		public string DomainName
		{
			get { return domainName; }
		}

		/// <summary>
		/// Gets the public key for the owner identity.
		/// </summary>
		public RSACryptoServiceProvider PublicKey
		{
			get { return publicKey; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor of the object.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="identity">Object that represents the current identity.</param>
		public IdentityManager( Store store, Identity identity )
		{
			this.store = store;
			this.identityGuid = identity.Id;
			this.domainName = identity.CollectionNode.DomainName;
			this.publicKey = new RSACryptoServiceProvider();
			this.publicKey.ImportParameters( identity.ServerCredential.ExportParameters( false ) );
		}

		/// <summary>
		/// Constructor of the object.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="domainName">Name of the domain for this identity.</param>
		/// <param name="identityGuid">Guid to use as the identity.</param>
		private IdentityManager( Store store, string domainName, string identityGuid )
		{
			this.store = store;
			this.domainName = domainName;
			this.identityGuid = identityGuid;
			this.publicKey = null;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the local address book with no special access checks.
		/// </summary>
		private LocalAddressBook GetLocalAddressBook()
		{
			LocalAddressBook localAb = null;

			// Look for the address book by its name, which is the domain name.
			Persist.Query query = new Persist.Query( Property.ObjectName, Persist.Query.Operator.Equal, domainName, Syntax.String );

			// Do the search.
			char[] results = new char[ 4096 ];
			Persist.IResultSet chunkIterator = store.StorageProvider.Search( query );
			if ( chunkIterator != null )
			{
				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				if ( length > 0 )
				{
					// Set up the XML document so the data can be easily extracted.
					XmlDocument abDocument = new XmlDocument();
					abDocument.LoadXml( new string( results, 0, length ) );
					localAb = new LocalAddressBook( store, store.GetShallowCollection( abDocument.DocumentElement.FirstChild ) );
				}

				chunkIterator.Dispose();
			}

			return localAb;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Requests the acceptance of the presented "server" principal credentials.
		/// 
		/// IMPORTANT: The method must raise an exception if there is a security policy in effect that
		/// prevents it from accepting credentials received during peer authentication. Failing to
		/// raise an exception with such a policy in effect will result in a policy violation because
		/// the caller will consider that it is OK to use the credentials to authenticate the peer.
		/// /// </summary>
		/// <param name="realm">The realm to which the server principal belongs</param>
		/// <param name="principalName">The name of the server principal</param>
		/// <param name="rsaKeys">RSA algorithm with the server principal's public key</param>
		public override void AcceptServerPrincipalCredentials(string realm, string principalName, RSACryptoServiceProvider rsaKeys)
		{
			throw new ApplicationException( "Credentials not accepted." );
		}

		/// <summary>
		/// Requests the acceptance of the presented "client" principal credentials.
		/// 
		/// IMPORTANT: The method must raise an exception if there is a security policy in effect that
		/// prevents it from accepting credentials received during peer authentication. Failing to
		/// raise an exception with such a policy in effect will result in a policy violation because
		/// the caller will consider that it is OK to use the credentials to authenticate the peer.
		/// /// </summary>
		/// <param name="realm">The realm to which the client principal belongs</param>
		/// <param name="principalName">The name of the client principal</param>
		/// <param name="rsaKeys">RSA algorithm with the client principal's public key</param>
		public override void AcceptClientPrincipalCredentials(string realm, string principalName, RSACryptoServiceProvider rsaKeys)
		{
			// Find this contact.
			Identity tempIdentity = AddressBook.GetIdentityById( principalName );
			if ( tempIdentity == null )
			{
				throw new ApplicationException( "No such identity." );
			}

			// Add the public key to this identity.
			tempIdentity.AddPublicKey( realm, rsaKeys );
			tempIdentity.Commit();
		}

		/// <summary>
		/// Gets an identity which represents the store administrator.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="domainName">Name of the domain.</param>
		static public IdentityManager CreateStoreAdmin( Store store, string domainName )
		{
			// Create a temporary identity in the local address book for the admin role.
			return new IdentityManager( store, domainName, Access.StoreAdminRole );
		}

		/// <summary>
		/// Obtains the credentials associated with the specified "client" principal.
		/// </summary>
		/// <param name="realm">The realm to which the client principal belongs</param>
		/// <param name="principalName">The name of the client principal</param>
		/// <param name="rsaKeys">RSA algorithm with the client principal's public key, null if no credentials found</param>
		public override void GetClientPrincipalCredentials(string realm, string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			// Get the public key associated with the domain.
			// If the specified realm is the same as the current realm, use the public key from the 
			// principal name instead of a client key.
			if ( realm == domainName )
			{
				rsaKeys = publicKey;
			}
			else
			{
				// Find the specified contact and return the public key information.
				Identity tempIdentity = AddressBook.GetIdentityById( principalName );
				if ( tempIdentity != null )
				{
					rsaKeys = tempIdentity.GetDomainPublicKey( realm );
				}
				else
				{
					rsaKeys = null;
				}
			}
		}

		/// <summary>
		/// Obtains the credentials necessary for authentication against the specified server
		/// 
		/// Note: Exception thrown if no credential materials are found.
		/// </summary>
		/// <param name="serverRealm">The realm to which the server principal belongs</param>
		/// <param name="server">The name of the server principal</param>
		/// <param name="realm">The realm to which the client principal belongs</param>
		/// <param name="principalName">The name of the client principal</param>
		/// <param name="rsaKeys">RSA algorithm with the client principal's public and private keys</param>
		public override void GetLocalCredentialsForServer(string serverRealm, string server, out string realm, out string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			// Get the base identity for this store.
			Identity identity = BaseIdentity;

			// If the server realm is the same as the current realm then use the primary credentials.
			if ( serverRealm != domainName )
			{
				// Find the alias that belongs to the specified domain.
				Alias alias = identity.GetAliasFromDomain( serverRealm );
				if ( alias == null )
				{
					throw new ApplicationException( "No identity exists for specified domain." );
				}

				principalName = alias.Id;
			}
			else
			{
				principalName = identity.Id;
			}

			realm = domainName;
			rsaKeys = identity.ServerCredential;
		}

		/// <summary>
		/// Gets the server credentials.
		/// 
		/// Note: Exception thrown if no credential materials are found.
		/// </summary>
		/// <param name="realm">The realm to which the principal belongs</param>
		/// <param name="principalName">The name of the server principal</param>
		/// <param name="rsaKeys">RSA algorithm with the principal's public and private keys</param>
		public override void GetServerCredentials(out string realm, out string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			Identity identity = BaseIdentity;
			realm = domainName;
			principalName = identity.Id;
			rsaKeys = identity.ServerCredential;
		}

		/// <summary>
		/// Obtains the credentials associated with the specified "server" principal.
		/// </summary>
		/// <param name="realm">The realm to which the server principal belongs</param>
		/// <param name="principalName">The name of the server principal</param>
		/// <param name="rsaKeys">RSA algorithm with the server principal's public key, null if no credentials found</param>
		public override void GetServerPrincipalCredentials(string realm, string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			// If the realm is the same as the current realm, just return the public key for the current realm.
			if ( realm == domainName )
			{
				rsaKeys = publicKey;
			}
			else
			{
				// Need to look up the alias for the specified domain and return the public key.
				Alias alias = BaseIdentity.GetAliasFromDomain( realm );
				if ( alias != null )
				{
					rsaKeys = alias.PublicKey;
				}
				else
				{
					rsaKeys = null;
				}
			}
		}

		/// <summary>
		/// Impersonates the specified identity, if the userId is verified.
		/// TODO: May want to look at limiting who can impersonate.
		/// </summary>
		/// <param name="userId">User ID to impersonate.</param>
		public void Impersonate( string userId )
		{
			// Look up the specified user in the local address book.
			Identity identity = AddressBook.GetIdentityById( userId );
			if ( identity == null )
			{
				throw new ApplicationException( "No such user." );
			}

			// Push the user onto the impersonation stack.
			impersonationId.Push( userId );
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			// Don't ever pop an empty stack.
			if ( impersonationId.Count > 0 )
			{
				impersonationId.Pop();
			}
		}
		#endregion
	}
}
