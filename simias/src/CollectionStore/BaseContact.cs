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
	/// Class that is used to manage domain public keys.
	/// </summary>
	internal class DomainPublicKey
	{
		#region Class Members
		/// <summary>
		/// Name of the domain that the public key belongs to.
		/// </summary>
		private string domain;

		/// <summary>
		/// Public key credential to authenticate to the domain.
		/// </summary>
		private RSACryptoServiceProvider credential;
		#endregion

		#region	 Properties
		/// <summary>
		/// Gets the domain that the public key belongs to.
		/// </summary>
		public string Domain
		{
			get { return domain; }
		}

		/// <summary>
		/// Gets the public key for the domain.
		/// </summary>
		public RSACryptoServiceProvider PublicKey
		{
			get { return credential; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="domain">Domain that the public key belongs to.</param>
		/// <param name="credential">Public key credential to authenticate to the domain.</param>
		public DomainPublicKey( string domain, RSACryptoServiceProvider credential )
		{
			this.domain = domain;
			this.credential = credential;
		}

		/// <summary>
		/// Constructor to create an empty object.
		/// </summary>
		public DomainPublicKey()
		{
			this.domain = null;
			this.credential = null;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Reconstructs a DomainPublicKey object from an xml string.
		/// </summary>
		/// <param name="xmlString">String used to reconstruct object.</param>
		public void FromXmlString( string xmlString )
		{
			XmlDocument dpkDocument = new XmlDocument();
			dpkDocument.LoadXml( xmlString );

			domain = dpkDocument.DocumentElement.GetAttribute( PropertyTags.DomainName );
			credential = new RSACryptoServiceProvider( IdentityManager.dummyCsp );
			credential.FromXmlString( dpkDocument.DocumentElement.InnerText );
		}

		/// <summary>
		/// Gets a xml string representation of the DomainPublicKey object.  This is the format used to
		/// store the object as a property on the identity object.
		/// </summary>
		/// <returns>A string that represents the serialized DomainPublicKey object.</returns>
		public string ToXmlString()
		{
			// Create an xml document that will hold the serialized object.
			XmlDocument dpkDocument = new XmlDocument();
			XmlElement dpkRoot = dpkDocument.CreateElement( PropertyTags.ClientPublicKey );
			dpkDocument.AppendChild( dpkRoot );

			// Set the attributes on the object.
			dpkRoot.SetAttribute( PropertyTags.DomainName, domain );
			dpkRoot.InnerText = credential.ToXmlString( false );
			return dpkRoot.OuterXml;
		}
		#endregion
	}


	/// <summary>
	/// Class that represents a user in the Collection Store.
	/// </summary>
	[ Serializable ]
	public class BaseContact : Node
	{
		#region Properties
		/// <summary>
		/// Gets the public/private key values for the specified identity.
		/// </summary>
		internal RSACryptoServiceProvider ServerCredential
		{
			get
			{
				// Lookup the credential property on the identity.
				Property p = properties.FindSingleValue( PropertyTags.ServerCredential );
				if ( p != null )
				{
					RSACryptoServiceProvider credential = new RSACryptoServiceProvider( IdentityManager.dummyCsp );
					credential.FromXmlString( p.Value as string );
					return credential;
				}
				else
				{
					throw new DoesNotExistException( String.Format( "The server credential does not exist for identity: {0} - ID: {1}.", name, id ) );
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating a new BaseContact object.
		/// </summary>
		/// <param name="userName">User name of the identity.</param>
		public BaseContact( string userName ) :
			this( userName, Guid.NewGuid().ToString() )
		{
		}

		/// <summary>
		/// Constructor for creating a new BaseContact object.
		/// </summary>
		/// <param name="userName">User name of the identity.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		public BaseContact( string userName, string userGuid ) :
			base ( userName, userGuid, NodeTypes.BaseContactType )
		{
			properties.AddNodeProperty( PropertyTags.Types, PropertyTags.ContactType );
		}

		/// <summary>
		/// Constructor that creates a BaseContact object from a Node object.
		/// </summary>
		/// <param name="node">Node object to create the BaseContact object from.</param>
		public BaseContact( Node node ) :
			base( node )
		{
			if ( type != NodeTypes.BaseContactType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.BaseContactType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a BaseContact object from a ShallowNode object.
		/// </summary>
		/// <param name="collection">Collection that the specified Node object belongs to.</param>
		/// <param name="shallowNode">ShallowNode object to create the BaseContact object from.</param>
		public BaseContact( Collection collection, ShallowNode shallowNode ) :
			base( collection, shallowNode )
		{
			if ( type != NodeTypes.BaseContactType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.BaseContactType, type ) );
			}
		}

		/// <summary>
		/// Constructor that creates a BaseContact object from an Xml document object.
		/// </summary>
		/// <param name="document">Xml document object to create the BaseContact object from.</param>
		internal BaseContact( XmlDocument document ) :
			base( document )
		{
			if ( type != NodeTypes.BaseContactType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.BaseContactType, type ) );
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Finds the specified alias property on the BaseContact object.
		/// </summary>
		/// <param name="domain">Domain that alias is contained in.</param>
		/// <param name="userGuid">Unique identifier that alias is known as in the specified domain. </param>
		/// <returns>A property object that contains the alias object.</returns>
		private Property FindAliasProperty( string domain, string userGuid )
		{
			Property aliasProperty = null;

			// Find if there is an existing alias.  If there is don't add the new one.
			MultiValuedList mvl = properties.FindValues( PropertyTags.Alias );
			foreach( Property p in mvl )
			{
				Alias tempAlias = new Alias( p );
				if ( ( tempAlias.Domain == domain ) && ( tempAlias.ID == userGuid ) )
				{
					aliasProperty = p;
					break;
				}
			}

			return aliasProperty;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Adds the specified public key to this BaseContact object's client credential list.
		/// </summary>
		/// <param name="domain">Domain that the public key belongs to.</param>
		/// <param name="publicKey">Public key for the domain.</param>
		internal void AddPublicKey( string domain, RSACryptoServiceProvider publicKey )
		{
			// Create an xml document that will hold the serialized object.
			DomainPublicKey dpk = new DomainPublicKey( domain, publicKey );

			// Set the property on the identity object.  Don't show this property through normal
			// enumeration, but let the property replicate.
			Property clientpkp = new Property( PropertyTags.ClientCredential, dpk.ToXmlString() );
			clientpkp.HiddenProperty = true;
			properties.AddNodeProperty( clientpkp );
		}

		/// <summary>
		/// Removes the alias from the specified domain.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="domainName">Domain of the collection that has been deleted.</param>
		internal void CleanupAliases( Store storeObject, string domainName )
		{
			// Search for any collections that have the specified domain name.
			Persist.Query query = new Persist.Query( PropertyTags.DomainName, SearchOp.Equal, domainName, Syntax.String );
			Persist.IResultSet chunkIterator = storeObject.StorageProvider.Search( query );
			if ( chunkIterator != null )
			{
				char[] results = new char[ 4096 ];

				// Get the first set of results from the query.
				int length = chunkIterator.GetNext( ref results );
				if ( length == 0 )
				{
					// No results, okay to delete the alias for this domain.
					Alias alias = GetAliasFromDomain( domainName );
					if ( alias != null )
					{
						DeleteAlias( alias.Domain, alias.ID );
						storeObject.GetLocalAddressBook().Commit( this );
					}
				}

				chunkIterator.Dispose();
			}
		}

		/// <summary>
		/// Generates an RSA public/private key pair that will be used to authenticate this identity
		/// to a remote connection.
		/// </summary>
		internal void CreateKeyPair()
		{
			// Create a local, hidden property to store the credential in.
			Property p;

			/* TODO: fix this messy hack
			 * The keypair generation is currently so slow on linux that is causes other problems
			 * and makes testing difficult, so this code now uses a hardcoded keypair on linux -- dro
			 */
			if (Path.DirectorySeparatorChar == '/')
			{
				// must be non-Windows
				p = new Property(PropertyTags.ServerCredential, "<RSAKeyValue><Modulus>xA3z1CSlQoI65Wd85FIbGW4yXF7Kv6e+5/zcqkUkQGAZSdyaAUh9sLEwpU3AOBKEaFn1jwe0lLTgHmSzMerM5lx+lYDV2AWZhj3TIr819sMmxmPPXHLq9cMqs7s95T0lE1mqJxdtCe2FXFhAdn1/yvG8AG2zRNEZb4kzG+Rt/9yAJ+Zhi1B/JbI8lxa00YafssNsI5V5cp3eOQkKk/ji05oAgr3o0bAzKN8Zd3IocD5oLT0pjq6w95uTpuDY8jw3d7kkBzq/IQvZ2vWtVjIM/yD3lnY39lFRPHXEbeIQW7uxFl50v6LpGGIu1kkmLNZRpaR/ylM5SbUVUjnEbU2qRw==</Modulus><Exponent>EQ==</Exponent><P>zbTrChzdUcftyzOT+sl9POKamDv4eKwS5HKUq4vSjUlXprHmw6pThJDkFxBNHZP/UoYAAppXL//FDCUogzWZDupf2RzD6QNPx5IBCpCuetLb+dcZhWOSOkOQUHMBFmtS7Qf6gZ6uLI95R2ixuH5fRgFY1zTyHvQ+owI4qSXKems=</P><Q>8/zoQ3nh98q9gEuHI1Uoq5+AQ3sxWwKp65qtaTDgBIRqCJ81bSs8HMODyIpIkAdEja9KrC8DryKYgWuoIaqZuJf4lwecUzeqSwu/QP+U1VbMK0de6E3oCQeuLTG27ZnZcpUW3Pd0DpwxtjQCMFMH56ZymG+mbbF3PtgJ2O8LvpU=</Q><DP>SJo01mSKWRli/GyOlL+VnQSvCI2i/WnojN0labjg5pJbK8ZRcjwdehUFNVEMKI6WWVx4eWOmTS0YXqOz8hLqufheEGSBYUx2gqwAXhTySXeY7sRjXEFCqybno+xavJ5ZgNWjtUcQS/ZnCiTzbkq4NtNMiDDN7M6ssgDItCt0o60=</DP><DQ>R8La5qtglCySFqzNZL6xm+OeMfcOhC31vcQU4rQFpvnE1VwAp6NN6lefOvt+wPMUKawG51kfM4KlUz3ICebh6v+FWZjToAFQNDChqbSVL7Ad7pyFU2I1L9UVHFnqgh4w5Xck15QTE1sdrg9L8BhrvJpd8JlPETQyIYrVuEZOv5U=</DQ><InverseQ>uRVPC1cDqIqy5E9kOIHly3G8e/MCVMMIRoLTYmTTEeqPAtFuSvr0PLt80mPZBuJBZ2TwkSnsuv+bn5lQploLUPRAO7+/J0euEhxKu9+t4rsctEUdkifcTrKRe8VK/0CDfLt4MT7T96UlFk7JZdC3wyJ+3KWIy/PZX0XqQA2zeKA=</InverseQ><D>LiFmjETbly2zgUWGzE+N592xfyVc4c0d3Dt/Nx9T0ulvXKxgeMXDOKIpkE6HlLkQGI2jMLaE17IWnb1XVwoSGBXDbniqyWq6tiyqJksbwZdUaupO6JOClC3r7g3wciyBMbq+n8lG1SjUFbpphUq0qDjg8QrAxOXn3gIqJK43//c734wEsgQMmxCxMtHJGweyogmLZM4I4suK3R/BX9RPTKteZgfvDiXH9cGzQwazfWgf/hrvtfWayMNMueWx5iBWlBbA6hVL+eYQwxueiOzhIyt80PBr0/Zdoisftty3m+V8lQMURgAo7GdW4F9Zuyk1pSQDzKRk20sbK0k8yYWEEQ==</D></RSAKeyValue>");
			}
			else
			{
				// The RSA parameters will be stored as a string object on the identity node.
				RSACryptoServiceProvider credential = RsaKeyStore.CreateRsaKeys();
				p = new Property( PropertyTags.ServerCredential, credential.ToXmlString( true ) );
			}
			
			p.HiddenProperty = true;
			p.LocalProperty = true;
			Properties.ModifyNodeProperty( p );
		}

		/// <summary>
		/// Removes the public key associated with the specified domain.
		/// </summary>
		/// <param name="domain">Domain that public key belongs to.</param>
		internal void DeletePublicKey( string domain )
		{
			// Look up any client credential properties for this identity.
			MultiValuedList mvl = properties.FindValues( PropertyTags.ClientCredential, true );
			foreach ( Property keyProp in mvl )
			{
				// Export the public key from the client credentials.
				DomainPublicKey dpk = new DomainPublicKey();
				dpk.FromXmlString( keyProp.Value as String );
				if ( dpk.Domain == domain )
				{
					keyProp.DeleteProperty();
					break;
				}
			}
		}

		/// <summary>
		/// Gets a public key for the specified domain.
		/// </summary>
		/// <param name="domain">Domain that public key belongs to.</param>
		/// <returns>The public key for the specified domain.</returns>
		internal RSACryptoServiceProvider GetDomainPublicKey( string domain )
		{
			RSACryptoServiceProvider credential = null;

			// Look up any client credential properties for this identity.
			MultiValuedList mvl = properties.FindValues( PropertyTags.ClientCredential, true );
			foreach ( Property keyProp in mvl )
			{
				// Export the public key from the client credentials.
				DomainPublicKey dpk = new DomainPublicKey();
				dpk.FromXmlString( keyProp.Value as String );
				if ( dpk.Domain == domain )
				{
					credential = dpk.PublicKey;
					break;
				}
			}

			return credential;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Sets a property on the identity object that represents this identity in another domain.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		/// <param name="publicKeyString">Domain's public key represented as a string.</param>
		public Alias CreateAlias( string domain, string userGuid, string publicKeyString )
		{
			RSACryptoServiceProvider publicKey = new RSACryptoServiceProvider( IdentityManager.dummyCsp );
			publicKey.FromXmlString( publicKeyString );

			// Set up an alias object to store on this identity.
			Alias alias = new Alias( domain, userGuid, publicKey );

			// Look for an existing alias property.
			Property aliasProperty = FindAliasProperty( domain, userGuid );
			if (aliasProperty == null )
			{
				// Create a property to store the object on.
				properties.AddNodeProperty( new Property( PropertyTags.Alias, alias.ToXmlString() ) );
			}
			else
			{
				// Set a new credential on an existing alias property.
				aliasProperty.SetPropertyValue( alias.ToXmlString() );
			}

			return alias;
		}

		/// <summary>
		/// Deletes the specified alias off this identity node.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		public void DeleteAlias( string domain, string userGuid )
		{
			Property p = FindAliasProperty( domain, userGuid );
			if ( p != null )
			{
				p.DeleteProperty();
			}
		}

		/// <summary>
		/// Finds the specified alias on the identity object. that represents this identity in another domain.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		/// <returns>The specified alias if found, otherwise a null.</returns>
		public Alias FindAlias( string domain, string userGuid )
		{
			Property p = FindAliasProperty( domain, userGuid );
			return ( p != null ) ? new Alias( p ) : null;
		}

		/// <summary>
		/// Returns the alias object that the current user is known as in the specified domain.
		/// </summary>
		/// <param name="domain">The domain that the user is in.</param>
		/// <returns>An alias object representing the user in the specified domain.</returns>
		public Alias GetAliasFromDomain( string domain )
		{
			Alias alias = null;

			// Look through the list of aliases that this identity is known by in other domains.
			foreach ( Alias tempAlias in GetAliasList() )
			{
				if ( tempAlias.Domain == domain )
				{
					alias = tempAlias;
					break;
				}
			}

			return alias;
		}

		/// <summary>
		/// Gets the list of aliases that this user is known by in other domains.
		/// </summary>
		/// <returns>An ICSList object containing all of the aliases for this identity.</returns>
		public ICSList GetAliasList()
		{
			ICSList aliasList = new ICSList();

			MultiValuedList mvl = properties.FindValues( PropertyTags.Alias );
			foreach( Property p in mvl )
			{
				aliasList.Add( new Alias( p ) );
			}

			return aliasList;
		}

		/// <summary>
		/// Gets the identity of the current user and all of its aliases.
		/// </summary>
		/// <returns>An array list of guids that represents the current user and all of its aliases.</returns>
		public ArrayList GetIdentityAndAliases()
		{
			ArrayList ids = new ArrayList();
			ids.Add( id );

			// Add any aliases to the list also.
			foreach ( Alias alias in GetAliasList() )
			{
				ids.Add( alias.ID );
			}

			return ids;
		}

		/// <summary>
		/// Returns the user guid that the current user is known as in the specified domain.
		/// </summary>
		/// <param name="storeObject">The store object for the local domain.</param>
		/// <param name="userDomain">The domain that the user is in.</param>
		/// <returns>A string representing the user's guid in the specified domain.  If the user does not exist
		/// in the specified domain, the current user guid is returned.</returns>
		public string GetDomainUserGuid( Store storeObject, string userDomain )
		{
			string userGuid = id;

			// If no domain is specified or it is the current domain, use the current identity.
			if ( ( userDomain != null ) && ( userDomain != storeObject.LocalDomain ) )
			{
				// This is not the store's domain.  Look through the list of aliases that this
				// identity is known by in other domains.
				foreach ( Alias alias in GetAliasList() )
				{
					if ( alias.Domain == userDomain )
					{
						userGuid = alias.ID;
						break;
					}
				}
			}

			return userGuid;
		}
		#endregion
	}
}
