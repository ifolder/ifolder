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
using System.Xml.Serialization;
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

			this.domain = dpkDocument.DocumentElement.GetAttribute( Property.DomainName );
			this.credential = new RSACryptoServiceProvider();
			this.credential.FromXmlString( dpkDocument.DocumentElement.InnerText );
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
			XmlElement dpkRoot = dpkDocument.CreateElement( Property.ClientPublicKey );
			dpkDocument.AppendChild( dpkRoot );

			// Set the attributes on the object.
			dpkRoot.SetAttribute( Property.DomainName, domain );
			dpkRoot.InnerText = credential.ToXmlString( false );
			return dpkRoot.OuterXml;
		}
		#endregion
	}


	/// <summary>
	/// Class that represents a user in the Collection Store.
	/// </summary>
	public class Identity : Node
	{
		#region Class Members
		/// <summary>
		/// The local address book that this identity belongs to.
		/// </summary>
		private LocalAddressBook localAb;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the local address book that this identity belongs to.
		/// </summary>
		internal LocalAddressBook AddressBook
		{
			get { return localAb; }
		}

		/// <summary>
		/// Gets the public/private key values for the specified identity.
		/// </summary>
		internal RSACryptoServiceProvider ServerCredential
		{
			get
			{
				// Lookup the credential property on the identity.
				Property p = Properties.FindSingleValue( Property.ServerCredential );
				if ( p != null )
				{
					RSACryptoServiceProvider credential = new RSACryptoServiceProvider();
					credential.FromXmlString( p.Value as string );
					return credential;
				}
				else
				{
					throw new ApplicationException( "Server credential not set on identity." );
				}
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for creating an identity with a known GUID.
		/// </summary>
		/// <param name="localAb">Address book that the identity will belong to.</param>
		/// <param name="userName">User name of the identity.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		/// <param name="type">Type of identity to create.</param>
		internal Identity( LocalAddressBook localAb, string userName, string userGuid, string type ) :
			base ( localAb, userName, userGuid, type )
		{
			this.localAb = localAb;
			SetParent( localAb );
		}

		/// <summary>
		/// Constructor for creating an identity with a known GUID.
		/// </summary>
		/// <param name="localAb">Address book that the identity will belong to.</param>
		/// <param name="userName">User name of the identity.</param>
		/// <param name="userGuid">Unique identifier for the user.</param>
		public Identity( LocalAddressBook localAb, string userName, string userGuid ) :
			this ( localAb, userName, userGuid, Property.IdentityType )
		{
			this.localAb = localAb;
		}

		/// <summary>
		/// Constructor for creating a new identity.
		/// </summary>
		/// <param name="localAb">Address book that the identity will belong to.</param>
		/// <param name="userName">User name of the identity.</param>
		public Identity( LocalAddressBook localAb, string userName ) :
			this( localAb, userName, Guid.NewGuid().ToString().ToLower(), Property.IdentityType )
		{
			this.localAb = localAb;
		}

		/// <summary>
		/// Constructor that creates a identity from a Node.
		/// </summary>
		/// <param name="node">Node that is an identity type.</param>
		internal Identity( Node node ) :
			base( node.cNode, true )
		{
			// Need to convert the collection type to a local address book type.
			this.localAb = new LocalAddressBook( store, CollectionNode );
			InternalCollectionHandle = this.localAb;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Finds the specified alias property on the identity node.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		/// <returns>A property object that contains the alias object.</returns>
		public Property FindAliasProperty( string domain, string userGuid )
		{
			Property aliasProperty = null;

			// Find if there is an existing alias.  If there is don't add the new one.
			MultiValuedList mvl = Properties.FindValues( Property.Alias );
			foreach( Property p in mvl )
			{
				Alias tempAlias = new Alias( p );
				if ( ( tempAlias.Domain == domain ) && ( tempAlias.Id == userGuid ) )
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
		/// Adds the specified public key to this identity's client credential list.
		/// </summary>
		/// <param name="domain">Domain that the public key belongs to.</param>
		/// <param name="publicKey">Public key for the domain.</param>
		internal void AddPublicKey( string domain, RSACryptoServiceProvider publicKey )
		{
			// Create an xml document that will hold the serialized object.
			DomainPublicKey dpk = new DomainPublicKey( domain, publicKey );

			// Set the property on the identity object.  Don't show this property through normal
			// enumeration.  But, let the property replicate.
			Property clientpkp = new Property( Property.ClientCredential, dpk.ToXmlString() );
			clientpkp.HiddenProperty = true;
			Properties.AddNodeProperty( clientpkp );
		}

		/// <summary>
		/// Generates an RSA public/private key pair that will be used to authenticate this identity
		/// to a remote connection.
		/// </summary>
		internal void CreateKeyPair()
		{
			// The RSA parameters will be stored as a string object on the identity node.
			RSACryptoServiceProvider credential = RsaKeyStore.CreateRsaKeys();
			
			// Create a local, hidden property to store the credential in.
			Property p = new Property( Property.ServerCredential, credential.ToXmlString( true ) );
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
			MultiValuedList mvl = Properties.FindValues( Property.ClientCredential, true );
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
			MultiValuedList mvl = Properties.FindValues( Property.ClientCredential, true );
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
			RSACryptoServiceProvider publicKey = new RSACryptoServiceProvider();
			publicKey.FromXmlString( publicKeyString );
			return CreateAlias( domain, userGuid, publicKey );
		}

		/// <summary>
		/// Sets a property on the identity object that represents this identity in another domain.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		/// <param name="publicKey">Domain's public key.</param>
		public Alias CreateAlias( string domain, string userGuid, RSACryptoServiceProvider publicKey )
		{
			lock ( store )
			{
				// Set up an alias object to store on this identity.
				Alias alias = new Alias( domain, userGuid, publicKey );

				// Look for an existing alias property.
				Property aliasProperty = FindAliasProperty( domain, userGuid );
				if (aliasProperty == null )
				{
					// Create a property to store the object on.
					Properties.AddNodeProperty( new Property( Property.Alias, alias.ToXmlString() ) );
				}
				else
				{
					// Set a new credential on an existing alias property.
					aliasProperty.SetPropertyValue( alias.ToXmlString() );
				}

				return alias;
			}
		}

		/// <summary>
		/// Deletes the specified alias off this identity node.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		public void DeleteAlias( string domain, string userGuid )
		{
			lock ( store )
			{
				Property p = FindAliasProperty( domain, userGuid );
				if ( p != null )
				{
					p.Delete();
				}
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
			lock ( store )
			{
				Property p = FindAliasProperty( domain, userGuid );
				return ( p != null ) ? new Alias( p ) : null;
			}
		}

		/// <summary>
		/// Returns the alias object that the current user is known as in the specified domain.
		/// </summary>
		/// <param name="domain">The domain that the user is in.</param>
		/// <returns>An alias object representing the user in the specified domain.</returns>
		public Alias GetAliasFromDomain( string domain )
		{
			lock ( store )
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
		}

		/// <summary>
		/// Gets the list of aliases that this user is known by in other domains.
		/// </summary>
		/// <returns>An ICSList object containing all of the aliases for this identity.</returns>
		public ICSList GetAliasList()
		{
			lock ( store )
			{
				ICSList aliasList = new ICSList();
				MultiValuedList mvl = Properties.FindValues( Property.Alias );
				foreach( Property p in mvl )
				{
					aliasList.Add( new Alias( p ) );
				}

				return aliasList;
			}
		}

		/// <summary>
		/// Gets the identity of the current user and all of its aliases.
		/// </summary>
		/// <returns>An array list of guids that represents the current user and all of its aliases.</returns>
		public ArrayList GetIdentityAndAliases()
		{
			lock ( store )
			{
				ArrayList ids = new ArrayList();
				ids.Add( Id );

				// Add any aliases to the list also.
				foreach ( Alias alias in GetAliasList() )
				{
					ids.Add( alias.Id );
				}

				return ids;
			}
		}

		/// <summary>
		/// Returns the user guid that the current user is known as in the specified domain.
		/// </summary>
		/// <param name="domain">The domain that the user is in.</param>
		/// <returns>A string representing the user's guid in the specified domain.  If the user does not exist
		/// in the specified domain, the current user guid is returned.</returns>
		public string GetDomainUserGuid( string domain )
		{
			lock ( store )
			{
				string userGuid = Id;

				// If no domain is specified or it is the current domain, use the current identity.
				if ( ( domain != null ) && ( domain != CollectionNode.DomainName ) )
				{
					// This is not the store's domain.  Look through the list of aliases that this
					// identity is known by in other domains.
					foreach ( Alias alias in GetAliasList() )
					{
						if ( alias.Domain == domain )
						{
							userGuid = alias.Id;
							break;
						}
					}
				}

				return userGuid;
			}
		}
		#endregion
	}
}
