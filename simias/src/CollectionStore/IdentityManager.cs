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

using Simias;
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
		/// This is used to keep from generating a new key set everytime a new RSACryptoSecurityProvider
		/// object is instantiated. This is passed as a parameter to the constructor and will initially
		/// use the dummy key set until the real key set is imported.
		/// </summary>
		static internal CspParameters dummyCsp;

		/// <summary>
		/// Name of the local Collection Store domain.
		/// </summary>
		private string domainName;

		/// <summary>
		/// Reference to the LocalAddressBook object.
		/// </summary>
		private LocalAddressBook localAb;

		/// <summary>
		/// Represents the identity of the user that instantiated this object.
		/// </summary>
		private BaseContact identity;

		/// <summary>
		/// Holds the public key for the server.
		/// </summary>
		private RSACryptoServiceProvider publicKey;

		/// <summary>
		/// Container used to keep track of the current identity for this store handle.
		/// </summary>
		private Stack impersonationID = new Stack();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the currently impersonating user guid.
		/// </summary>
		public string CurrentUserGuid
		{
			get { return ( impersonationID.Count == 0 ) ? identity.ID : ( impersonationID.Peek() as BaseContact ).ID; }
		}

		/// <summary>
		/// Gets the current impersonating identity.
		/// </summary>
		public BaseContact CurrentIdentity
		{
			get 
			{ 
				// Refresh the identity object before returning it.
				if ( impersonationID.Count == 0 )
				{
					localAb.Refresh( identity );
					return identity;
				}
				else
				{
					BaseContact impersonatingContact = impersonationID.Peek() as BaseContact;
					localAb.Refresh( impersonatingContact );
					return impersonatingContact;
				}
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
		/// Gets whether the current executing user is being impersonated.
		/// </summary>
		public bool IsImpersonating
		{
			get { return ( impersonationID.Count > 0 ) ? true : false; }
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
		/// Static constructor for the object.
		/// </summary>
		static IdentityManager()
		{
			// Set up the dummy key store so that it will contain a dummy key set.
			dummyCsp = new CspParameters();
			dummyCsp.KeyContainerName = "DummyKeyStore";
			new RSACryptoServiceProvider( dummyCsp );
		}

		/// <summary>
		/// Constructor for the IdentityManager object.
		/// </summary>
		/// <param name="domainName">Name of the local Collection Store domain.</param>
		/// <param name="localAb">Object reference to the LocalAddressBook object.</param>
		/// <param name="identity">Object that represents the current identity.</param>
		public IdentityManager( string domainName, LocalAddressBook localAb, BaseContact identity )
		{
			this.domainName = domainName;
			this.identity = identity;
			this.localAb = localAb;
			publicKey = new RSACryptoServiceProvider( dummyCsp );
			publicKey.ImportParameters( identity.ServerCredential.ExportParameters( false ) );
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
		/// </summary>
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
		/// </summary>
		/// <param name="realm">The realm to which the client principal belongs</param>
		/// <param name="principalName">The name of the client principal</param>
		/// <param name="rsaKeys">RSA algorithm with the client principal's public key</param>
		public override void AcceptClientPrincipalCredentials(string realm, string principalName, RSACryptoServiceProvider rsaKeys)
		{
			// Find this contact.
			BaseContact tempIdentity = localAb.GetNodeByID( principalName ) as BaseContact;
			if ( tempIdentity == null )
			{
				throw new ApplicationException( "No such identity." );
			}

			// Add the public key to this identity.
			tempIdentity.AddPublicKey( realm, rsaKeys );
			localAb.Commit( tempIdentity );
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
				BaseContact tempIdentity = localAb.GetNodeByID( principalName ) as BaseContact;
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
			// If the server realm is the same as the current realm then use the primary credentials.
			if ( serverRealm != domainName )
			{
				// Refresh the identity object.
				localAb.Refresh( identity );

				// Find the alias that belongs to the specified domain.
				Alias alias = identity.GetAliasFromDomain( serverRealm );
				if ( alias == null )
				{
					throw new ApplicationException( "No identity exists for specified domain." );
				}

				principalName = alias.ID;
			}
			else
			{
				principalName = identity.ID;
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
			realm = domainName;
			principalName = identity.ID;
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
				// Refresh the identity object first.
				localAb.Refresh( identity );

				// Need to look up the alias for the specified domain and return the public key.
				Alias alias = identity.GetAliasFromDomain( realm );
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
		/// </summary>
		/// <param name="userGuid">User ID to impersonate.</param>
		public void Impersonate( string userGuid )
		{
			// Look up the specified user in the local address book.
			BaseContact impersonator = localAb.GetNodeByID( userGuid ) as BaseContact;
			if ( impersonator == null )
			{
				throw new ApplicationException( "No such user." );
			}

			// Push the user onto the impersonation stack.
			impersonationID.Push( impersonator );
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			// Don't ever pop an empty stack.
			if ( impersonationID.Count > 0 )
			{
				impersonationID.Pop();
			}
		}
		#endregion
	}
}
