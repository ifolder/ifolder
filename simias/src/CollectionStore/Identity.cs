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
using System.Xml;
using System.IO;

using Persist = Simias.Storage.Provider;
using Novell.Security.SecureSink.SecurityProvider.RsaSecurityProvider;

namespace Simias.Storage
{
	/// <summary>
	/// Class that is used to supply credentials for remote access security.
	/// </summary>
	internal class IdentityKeyStore : RsaKeyStore
	{
		#region Class Members
		/// <summary>
		/// Reference to a Store object.
		/// </summary>
		private Store store;
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="store">Reference to a Store object.</param>
		public IdentityKeyStore( Store store )
		{
			this.store = store;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the Member object from the user identitifer.
		/// </summary>
		/// <param name="userID">Identifier of user to look up Member object with.</param>
		/// <returns>The Member object that is associated with the specified user ID if successful. Otherwise
		/// null is returned.</returns>
		private Member GetWorkGroupMember( string userID )
		{
			Member member = null;

			// Need to search across collection looking for any Member object that contains the specified
			// user ID.
			Persist.Query query = new Persist.Query( PropertyTags.Ace, SearchOp.Begins, userID, Syntax.String );
			Persist.IResultSet chunkIterator = store.StorageProvider.Search( query );
			if ( chunkIterator != null )
			{
				// Get the first set of results from the query.
				char[] results = new char[ 1024 ];
				int length = chunkIterator.GetNext( ref results );
				if ( length > 0 )
				{
					// Set up the XML document to parse the results.
					XmlDocument shallowList = new XmlDocument();
					shallowList.LoadXml( new string( results, 0, length ) );
					foreach ( XmlElement xe in shallowList.DocumentElement )
					{
						// The member can only be created from an XML Document object.
						XmlDocument xmlMember = new XmlDocument();
						xmlMember.AppendChild( xmlMember.CreateElement( XmlTags.ObjectListTag ) );
						xmlMember.DocumentElement.AppendChild( xmlMember.ImportNode( xe, true ) );
						member = new Member( xmlMember );
						break;
					}
				}

				chunkIterator.Dispose();
			}

			return member;
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
			throw new CollectionStoreException( "Credentials not accepted." );
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
			throw new CollectionStoreException( "Credentials not accepted." );
		}

		/// <summary>
		/// Obtains the credentials associated with the specified "client" principal.
		/// </summary>
		/// <param name="realm">The realm to which the client principal belongs</param>
		/// <param name="principalName">The name of the client principal</param>
		/// <param name="rsaKeys">RSA algorithm with the client principal's public key, null if no credentials found</param>
		public override void GetClientPrincipalCredentials(string realm, string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			// Get the Member object for the specified realm.
			Member member = GetWorkGroupMember( principalName );
			if ( member != null )
			{
				rsaKeys = member.PublicKey;
			}
			else
			{
				rsaKeys = null;
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
			Identity identity = store.CurrentUser;
			realm = Domain.WorkGroupDomainID;
			principalName = identity.ID;
			rsaKeys = identity.Credential;
		}

		/// <summary>
		/// Gets the server credentials for the WorkGroup domain. 
		/// 
		/// Note: Exception thrown if no credential materials are found.
		/// </summary>
		/// <param name="realm">The realm to which the principal belongs</param>
		/// <param name="principalName">The name of the server principal</param>
		/// <param name="rsaKeys">RSA algorithm with the principal's public and private keys</param>
		public override void GetServerCredentials(out string realm, out string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			Identity identity = store.CurrentUser;
			realm = Domain.WorkGroupDomainID;
			principalName = identity.ID;
			rsaKeys = identity.Credential;
		}

		/// <summary>
		/// Obtains the credentials associated with the specified "server" principal.
		/// </summary>
		/// <param name="realm">The realm to which the server principal belongs</param>
		/// <param name="principalName">The name of the server principal</param>
		/// <param name="rsaKeys">RSA algorithm with the server principal's public key, null if no credentials found</param>
		public override void GetServerPrincipalCredentials(string realm, string principalName, out RSACryptoServiceProvider rsaKeys)
		{
			// Get the Member object for the specified realm.
			Member member = GetWorkGroupMember( principalName );
			if ( member != null )
			{
				rsaKeys = member.PublicKey;
			}
			else
			{
				rsaKeys = null;
			}
		}
		#endregion
	}

	/// <summary>
	/// Class that represents a user identity in the Collection Store.
	/// </summary>
	public class Identity : Node
	{
		#region Class Members
		/// <summary>
		/// This is used to keep from generating a new key set everytime a new RSACryptoSecurityProvider
		/// object is instantiated. This is passed as a parameter to the constructor and will initially
		/// use the dummy key set until the real key set is imported.
		/// </summary>
		static internal CspParameters DummyCsp;

		/// <summary>
		/// Used to separate the user ID and domain ID in the Domain property.
		/// </summary>
		static private char valueSeparator = ':';

		/// <summary>
		/// Used to get credentials for remote access security.
		/// </summary>
		private IdentityKeyStore keyStore;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the public/private key values for the specified identity.
		/// </summary>
		internal RSACryptoServiceProvider Credential
		{
			get
			{
				RSACryptoServiceProvider credential = null;

				// Lookup the credential property on the identity.
				Property p = properties.FindSingleValue( PropertyTags.Credential );
				if ( p != null )
				{
					credential = new RSACryptoServiceProvider( Identity.DummyCsp );
					credential.FromXmlString( p.Value as string );
				}

				return credential;
			}
		}

		/// <summary>
		/// Gets the RSAKeyStore object in order to validate remote access.
		/// </summary>
		internal RsaKeyStore KeyStore
		{
			get { return keyStore; }
		}

		/// <summary>
		/// Gets the public key for the Identity object.
		/// </summary>
		public RSACryptoServiceProvider PublicKey
		{
			get
			{
				// Export the public key from the credential set.
				RSACryptoServiceProvider pk = null;
				RSACryptoServiceProvider credential = Credential;
				if ( credential != null )
				{
					pk = new RSACryptoServiceProvider( Identity.DummyCsp );
					pk.ImportParameters( credential.ExportParameters( false ) );
				}

				return pk;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Static constructor for the object.
		/// </summary>
		static Identity()
		{
			// Set up the dummy key store so that it will contain a dummy key set.
			DummyCsp = new CspParameters();
			DummyCsp.KeyContainerName = "DummyKeyStore";
			new RSACryptoServiceProvider( DummyCsp );
		}

		/// <summary>
		/// Constructor for creating a new Identity object.
		/// </summary>
		/// <param name="store">Reference to a Store object.</param>
		/// <param name="userName">User name of the identity.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		internal Identity( Store store, string userName, string userGuid ) :
			base ( userName, userGuid, NodeTypes.IdentityType )
		{
			// Setup the key store.
			keyStore = new IdentityKeyStore( store );

			// Create a local, hidden property to store the credential in.
			Property p;

			/* TODO: fix this messy hack
			* The keypair generation is currently so slow on linux that is causes other problems
			* and makes testing difficult, so this code now uses a hardcoded keypair on linux -- dro
			*/
			if (Path.DirectorySeparatorChar == '/')
			{
				// must be non-Windows
				p = new Property( PropertyTags.Credential, "<RSAKeyValue><Modulus>xA3z1CSlQoI65Wd85FIbGW4yXF7Kv6e+5/zcqkUkQGAZSdyaAUh9sLEwpU3AOBKEaFn1jwe0lLTgHmSzMerM5lx+lYDV2AWZhj3TIr819sMmxmPPXHLq9cMqs7s95T0lE1mqJxdtCe2FXFhAdn1/yvG8AG2zRNEZb4kzG+Rt/9yAJ+Zhi1B/JbI8lxa00YafssNsI5V5cp3eOQkKk/ji05oAgr3o0bAzKN8Zd3IocD5oLT0pjq6w95uTpuDY8jw3d7kkBzq/IQvZ2vWtVjIM/yD3lnY39lFRPHXEbeIQW7uxFl50v6LpGGIu1kkmLNZRpaR/ylM5SbUVUjnEbU2qRw==</Modulus><Exponent>EQ==</Exponent><P>zbTrChzdUcftyzOT+sl9POKamDv4eKwS5HKUq4vSjUlXprHmw6pThJDkFxBNHZP/UoYAAppXL//FDCUogzWZDupf2RzD6QNPx5IBCpCuetLb+dcZhWOSOkOQUHMBFmtS7Qf6gZ6uLI95R2ixuH5fRgFY1zTyHvQ+owI4qSXKems=</P><Q>8/zoQ3nh98q9gEuHI1Uoq5+AQ3sxWwKp65qtaTDgBIRqCJ81bSs8HMODyIpIkAdEja9KrC8DryKYgWuoIaqZuJf4lwecUzeqSwu/QP+U1VbMK0de6E3oCQeuLTG27ZnZcpUW3Pd0DpwxtjQCMFMH56ZymG+mbbF3PtgJ2O8LvpU=</Q><DP>SJo01mSKWRli/GyOlL+VnQSvCI2i/WnojN0labjg5pJbK8ZRcjwdehUFNVEMKI6WWVx4eWOmTS0YXqOz8hLqufheEGSBYUx2gqwAXhTySXeY7sRjXEFCqybno+xavJ5ZgNWjtUcQS/ZnCiTzbkq4NtNMiDDN7M6ssgDItCt0o60=</DP><DQ>R8La5qtglCySFqzNZL6xm+OeMfcOhC31vcQU4rQFpvnE1VwAp6NN6lefOvt+wPMUKawG51kfM4KlUz3ICebh6v+FWZjToAFQNDChqbSVL7Ad7pyFU2I1L9UVHFnqgh4w5Xck15QTE1sdrg9L8BhrvJpd8JlPETQyIYrVuEZOv5U=</DQ><InverseQ>uRVPC1cDqIqy5E9kOIHly3G8e/MCVMMIRoLTYmTTEeqPAtFuSvr0PLt80mPZBuJBZ2TwkSnsuv+bn5lQploLUPRAO7+/J0euEhxKu9+t4rsctEUdkifcTrKRe8VK/0CDfLt4MT7T96UlFk7JZdC3wyJ+3KWIy/PZX0XqQA2zeKA=</InverseQ><D>LiFmjETbly2zgUWGzE+N592xfyVc4c0d3Dt/Nx9T0ulvXKxgeMXDOKIpkE6HlLkQGI2jMLaE17IWnb1XVwoSGBXDbniqyWq6tiyqJksbwZdUaupO6JOClC3r7g3wciyBMbq+n8lG1SjUFbpphUq0qDjg8QrAxOXn3gIqJK43//c734wEsgQMmxCxMtHJGweyogmLZM4I4suK3R/BX9RPTKteZgfvDiXH9cGzQwazfWgf/hrvtfWayMNMueWx5iBWlBbA6hVL+eYQwxueiOzhIyt80PBr0/Zdoisftty3m+V8lQMURgAo7GdW4F9Zuyk1pSQDzKRk20sbK0k8yYWEEQ==</D></RSAKeyValue>" );
			}
			else
			{
				// The RSA parameters will be stored as a string object on the identity node.
				RSACryptoServiceProvider credential = RsaKeyStore.CreateRsaKeys();
				p = new Property( PropertyTags.Credential, credential.ToXmlString( true ) );
			}
			
			p.HiddenProperty = true;
			p.LocalProperty = true;
			properties.AddNodeProperty( p );
		}

		/// <summary>
		/// Constructor that creates an Identity object from a Node object.
		/// </summary>
		/// <param name="store">Reference to a Store object.</param>
		/// <param name="node">Node object to create the Identity object from.</param>
		internal Identity( Store store, Node node ) :
			base( node )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}

			keyStore = new IdentityKeyStore( store );
		}

		/// <summary>
		/// Constructor that creates an Identity object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the Identity object from.</param>
		internal Identity( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}

			keyStore = new IdentityKeyStore( collection.StoreReference );
		}

		/// <summary>
		/// Constructor that creates an Identity object from an Xml document object.
		/// </summary>
		/// <param name="store">Reference to a Store object.</param>
		/// <param name="document">Xml document object to create the Identity object from.</param>
		internal Identity( Store store, XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}

			keyStore = new IdentityKeyStore( store );
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the specified Domain property.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>A Property object containing the found domain property.</returns>
		private Property GetPropertyByDomain( string domainID )
		{
			Property property = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				if ( ( p.Value as string ).EndsWith( domainID ) )
				{
					property = p;
					break;
				}
			}

			return property;
		}

		/// <summary>
		/// Gets the specified Domain property.
		/// </summary>
		/// <param name="userID">User ID to use to discover domain property.</param>
		/// <returns>A Property object containing the found domain property.</returns>
		private Property GetPropertyByUserID( string userID )
		{
			Property property = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				if ( ( p.Value as string ).StartsWith( userID ) )
				{
					property = p;
					break;
				}
			}

			return property;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds a domain identity property to the Identity object.
		/// </summary>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity AddDomainIdentity( string userID, string domainID )
		{
			properties.AddNodeProperty( PropertyTags.Domain, userID.ToLower() + valueSeparator + domainID );
			return this;
		}

		/// <summary>
		/// Removes the specified domain mapping from the identity object.
		/// </summary>
		/// <param name="localDb">Reference to the local database object.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity DeleteDomainIdentity( LocalDatabase localDb, string domainID )
		{
			// Find the property to be deleted.
			Property p = GetPropertyByDomain( domainID );
			if ( ( p == null ) && ( localDb != null ) )
			{
				// This may be caused by a stale node. It needs to be refreshed.
				localDb.Refresh( this );
				
				// Try again.
				p = GetPropertyByDomain( domainID );
			}

			if ( p != null )
			{
				p.DeleteProperty();
			}

			return this;
		}

		/// <summary>
		/// Gets the domain associated with the specified user ID.
		/// </summary>
		/// <param name="localDb">Reference to the local database object.</param>
		/// <param name="userID">User ID to find the associated domain for.</param>
		/// <returns>Domain name associated with the specified user ID if it exists. Otherwise null is returned.</returns>
		internal string GetDomainFromUserID( LocalDatabase localDb, string userID )
		{
			string normalizedID = userID.ToLower();

			// Find the property associated with the user ID.
			Property p = GetPropertyByUserID( normalizedID );
			if ( ( p == null ) && ( localDb != null ) )
			{
				// This may be caused by a stale node. It needs to be refreshed.
				localDb.Refresh( this );

				// Try again.
				p = GetPropertyByUserID( normalizedID );
			}

			return ( p != null ) ? ( p.Value as string ).Substring( normalizedID.Length + 1 ) : null;
		}

		/// <summary>
		/// Gets the user ID associated with the specified domain ID.
		/// </summary>
		/// <param name="localDb">Reference to the local database object.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>User ID associated with the specified domain ID if it exists. Otherwise null is returned.</returns>
		internal string GetUserIDFromDomain( LocalDatabase localDb, string domainID )
		{
			// Find the property associated with the user ID.
			Property p = GetPropertyByDomain( domainID );
			if ( ( p == null ) && ( localDb != null ) )
			{
				// This may be caused by a stale node. It needs to be refreshed.
				localDb.Refresh( this );

				// Try again.
				p = GetPropertyByDomain( domainID );
			}

			return ( p != null ) ? ( p.Value as string ).Substring( 0, id.Length ) : null;
		}
		#endregion
	}
}
