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
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using Simias.Client;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
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
		/// Xml tags used to store the domain mapping information.
		/// </summary>
		static private readonly string MappingTag = "Mapping";
		static private readonly string DomainTag = "Domain";
		static private readonly string UserTag = "User";
		static private readonly string CredentialTag = "Credential";
		static private readonly string TypeTag = "Type";
		#endregion

		#region Properties
		/// <summary>
		/// Gets the public/private key values for the local identity.
		/// </summary>
		internal RSACryptoServiceProvider Credential
		{
			get
			{
				RSACryptoServiceProvider credential = null;

				// Lookup the credential property on the identity.
				XmlDocument mapDoc = GetDocumentByDomain( Store.GetStore().LocalDomain );
				if ( mapDoc != null )
				{
					credential = new RSACryptoServiceProvider( Identity.DummyCsp );
					credential.FromXmlString( mapDoc.DocumentElement.GetAttribute( CredentialTag ) );
				}

				return credential;
			}
		}

		/// <summary>
		/// Returns the number of subscribed domains.
		/// </summary>
		internal int DomainCount
		{
			get
			{
				MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
				return mvl.Count;
			}
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
		/// <param name="userName">User name of the identity.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		internal Identity( string userName, string userGuid ) :
			base ( userName, userGuid, NodeTypes.IdentityType )
		{
		}

		/// <summary>
		/// Constructor that creates an Identity object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the Identity object from.</param>
		internal Identity( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
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
		}

		/// <summary>
		/// Constructor that creates an Identity object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the Identity object from.</param>
		internal Identity( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.IdentityType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.IdentityType, type ) );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Decrypts the credential.
		/// </summary>
		/// <param name="encryptedCredential">A string object that contain the encrypted credential.</param>
		/// <returns>A string object containing the clear credential.</returns>
		private string DecryptCredential( string encryptedCredential )
		{
			// Decrypt the byte array and convert it back into a string.
			byte[] buffer = Credential.Decrypt( Convert.FromBase64String( encryptedCredential ), false );
			return new UTF8Encoding().GetString( buffer );
		}

		/// <summary>
		/// Encrypts the credential.
		/// </summary>
		/// <param name="credential">Credential to encrypt.</param>
		/// <returns>A string object containing the encrypted credential.</returns>
		private string EncryptCredential( string credential )
		{
			// Convert the string to a byte array.
			UTF8Encoding encoding = new UTF8Encoding();
			int byteCount = encoding.GetByteCount( credential );
			byte[] buffer = new byte[ byteCount ];
			encoding.GetBytes( credential, 0, credential.Length, buffer, 0 );

			// Encrypt the byte array and turn it into a string.
			return Convert.ToBase64String( Credential.Encrypt( buffer, false ) );
		}

		/// <summary>
		/// Gets the XML document that contains the specified Domain property.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>An XmlDocument object containing the found domain property.</returns>
		private XmlDocument GetDocumentByDomain( string domainID )
		{
			XmlDocument document = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( DomainTag ) == domainID )
				{
					document = mapDoc;
					break;
				}
			}

			return document;
		}

		/// <summary>
		/// Gets the XML document that contains the specified Domain property.
		/// </summary>
		/// <param name="userID">User ID to use to discover domain property.</param>
		/// <returns>An XmlDocument object containing the found domain property.</returns>
		private XmlDocument GetDocumentByUserID( string userID )
		{
			XmlDocument document = null;

			MultiValuedList mvl = properties.GetProperties( PropertyTags.Domain );
			foreach ( Property p in mvl )
			{
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( UserTag ) == userID )
				{
					document = mapDoc;
					break;
				}
			}

			return document;
		}

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
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( DomainTag ) == domainID )
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
				XmlDocument mapDoc = p.Value as XmlDocument;
				if ( mapDoc.DocumentElement.GetAttribute( UserTag ) == userID )
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
			return AddDomainIdentity( userID, domainID, null, CredentialType.None );
		}

		/// <summary>
		/// Adds a domain identity property to the Identity object.
		/// </summary>
		/// <param name="userID">Identity that this user is known as in the specified domain.</param>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <param name="credentials">Credentials for this domain. This may be null.</param>
		/// <param name="type">The type of credentials stored.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity AddDomainIdentity( string userID, string domainID, string credentials, CredentialType type )
		{
			XmlDocument mapDoc = null;
			
			// Check to see if the domain already exists.
			Property p = GetPropertyByDomain( domainID );
			if ( p != null )
			{
				mapDoc = p.Value as XmlDocument;
			}
			else
			{
				mapDoc = new XmlDocument();
				XmlElement root = mapDoc.CreateElement( MappingTag );
				mapDoc.AppendChild( root );
				mapDoc.DocumentElement.SetAttribute( DomainTag, domainID );

				p = new Property( PropertyTags.Domain, mapDoc );
				properties.AddNodeProperty( p );
			}

			mapDoc.DocumentElement.SetAttribute( UserTag, userID );
			mapDoc.DocumentElement.SetAttribute( TypeTag, type.ToString() );

			if ( ( credentials != null ) && ( type != CredentialType.None ) )
			{
				if ( type == CredentialType.Basic )
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, EncryptCredential( credentials ) );
				}
				else
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, credentials );
				}
			}

			p.SetPropertyValue( mapDoc );
			return this;
		}

		/// <summary>
		/// Removes the specified domain mapping from the identity object.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity DeleteDomainIdentity( string domainID )
		{
			// Normalize the domain ID.
			domainID = domainID.ToLower();

			// Do not allow the local domain to be deleted.
			if ( domainID == Store.GetStore().LocalDomain )
			{
				throw new CollectionStoreException( "Cannot remove the local domain." );
			}

			// Find the property to be deleted.
			Property p = GetPropertyByDomain( domainID );
			if ( p != null )
			{
				p.DeleteProperty();
			}

			return this;
		}

		/// <summary>
		/// Gets the domain associated with the specified user ID.
		/// </summary>
		/// <param name="userID">User ID to find the associated domain for.</param>
		/// <returns>Domain name associated with the specified user ID if it exists. Otherwise null is returned.</returns>
		internal string GetDomainFromUserID( string userID )
		{
			string domainID = null;

			// Find the property associated with the user ID.
			XmlDocument document = GetDocumentByUserID( userID.ToLower() );
			if ( document != null )
			{
				domainID = document.DocumentElement.GetAttribute( DomainTag );
			}

			return ( ( domainID != null ) && ( domainID != String.Empty ) ) ? domainID : null;
		}

		/// <summary>
		/// Gets the user ID associated with the specified domain ID.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>User ID associated with the specified domain ID if it exists. Otherwise null is returned.</returns>
		internal string GetUserIDFromDomain( string domainID )
		{
			string userID = null;

			// Find the property associated with the user ID.
			XmlDocument document = GetDocumentByDomain( domainID.ToLower() );
			if ( document != null )
			{
				userID = document.DocumentElement.GetAttribute( UserTag );
			}

			return ( ( userID != null ) && ( userID != String.Empty ) ) ? userID : null;
		}

		/// <summary>
		/// Gets the user identifier and credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">The identifier for the domain.</param>
		/// <param name="userID">Gets the userID of the user associated with the specified domain.</param>
		/// <param name="credentials">Gets the credentials for the user.</param>
		/// <returns>CredentialType enumerated object.</returns>
		internal CredentialType GetDomainCredentials( string domainID, out string userID, out string credentials )
		{
			// Find the property associated with the domain.
			XmlDocument document = GetDocumentByDomain( domainID );
			if ( document == null )
			{
				throw new CollectionStoreException( "The specified domain does not exist." );
			}

			// Return the User ID.
			userID = document.DocumentElement.GetAttribute( UserTag );

			// Get the credential type.
			string credTypeString = document.DocumentElement.GetAttribute( TypeTag );
			CredentialType credType = ( CredentialType )Enum.Parse( typeof( CredentialType ), credTypeString, true );

			// Return the credentials.
			credentials = document.DocumentElement.GetAttribute( CredentialTag );
			if ( credentials != String.Empty )
			{
				if ( credType == CredentialType.Basic )
				{
					credentials = DecryptCredential( credentials );
				}
			}
			else
			{
				credentials = null;
			}

			return credType;
		}

		/// <summary>
		/// Sets the credentials for the specified domain.
		/// </summary>
		/// <param name="domainID">The domain to set the password for.</param>
		/// <param name="credentials">The domain credentials.</param>
		/// <param name="type">Type of credentials.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity SetDomainCredentials( string domainID, string credentials, CredentialType type )
		{
			Property p = GetPropertyByDomain( domainID.ToLower() );
			if ( p == null )
			{
				throw new CollectionStoreException( "There is no mapping for this domain." );
			}

			// Set the password on the mapping.
			XmlDocument mapDoc = p.Value as XmlDocument;
			if ( type == CredentialType.None )
			{
				if ( domainID == Store.GetStore().LocalDomain )
				{
					throw new CollectionStoreException( "Cannot remove the local domain credentials." );
				}

				mapDoc.DocumentElement.RemoveAttribute( CredentialTag );
			}
			else
			{
				if ( type == CredentialType.Basic )
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, EncryptCredential( credentials ) );
				}
				else
				{
					mapDoc.DocumentElement.SetAttribute( CredentialTag, credentials );
				}
			}

			mapDoc.DocumentElement.SetAttribute( TypeTag, type.ToString() );
			p.SetPropertyValue( mapDoc );
			return this;
		}
		#endregion
	}
}
