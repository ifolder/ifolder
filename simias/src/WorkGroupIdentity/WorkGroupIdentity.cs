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
using System.Reflection;
using System.Xml;

namespace Simias.Identity
{
	/// <summary>
	/// Implements the work group identity where the identity objects are stored in the Collection
	/// Store database.
	/// </summary>
	internal class WorkGroupIdentity : IIdentity
	{
		#region Class Members
		/// <summary>
		/// Xml tag that represents an alternate identity.
		/// </summary>
		private const string Alternate = "alternate";
		private const string AlternateDomain = "domain";
		private const string AlternateId = "id";
		private const string AlternateCredential = "credential";

		/// <summary>
		/// Xml element that represents an identity.
		/// </summary>
		private XmlElement identity;

		/// <summary>
		/// Keeps a copy of the factory object so that the identity file can be updated.
		/// </summary>
		private WorkGroupIdentityFactory factory;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor of the object.
		/// </summary>
		public WorkGroupIdentity( WorkGroupIdentityFactory factory, XmlElement identity )
		{
			this.identity = identity;
			this.factory = factory;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets a constant globally unique identifier for this identity.
		/// </summary>
		public string UserGuid
		{
			get { return identity.GetAttribute( WorkGroupIdentityFactory.Id ); }
		}

		/// <summary>
		/// Returns the credential associated with this identity.
		/// </summary>
		public string Credential
		{
			get { return identity.GetAttribute( WorkGroupIdentityFactory.Credential ); }
		}

		/// <summary>
		/// Returns the user identifier ( e.g. user name ) for this identity.
		/// </summary>
		public string UserId
		{
			get { return identity.GetAttribute( WorkGroupIdentityFactory.Name ); }
		}

		/// <summary>
		/// Returns the name of the domain that the identity exists in.
		/// </summary>
		public string DomainName
		{
			get { return identity.GetAttribute( WorkGroupIdentityFactory.Domain ); }
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets the credential for the specified user guid.
		/// </summary>
		/// <param name="userGuid">User guid to retrieve credential for.</param>
		/// <returns>A string that represents the credential for the specified user guid.</returns>
		public string GetCredentialFromUserGuid( string userGuid )
		{
			// Check if this guid is the main identity guid.
			if ( userGuid.ToLower() == UserGuid )
			{
				return Credential;
			}
			else
			{
				// See if the guid exists as an item in the key chain.
				KeyChainItem kcItem = GetKeyChainItem( userGuid );
				if ( kcItem != null )
				{
					return kcItem.Credential;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the identity key chain for this identity.  The identity key chain contains alternate
		/// identities that the user can be known by.
		/// </summary>
		/// <returns>An array of objects containing the alternate identities.</returns>
		public KeyChainItem[] GetKeyChain()
		{
			// Need to update the identity element from the file first.
			identity = factory.GetIdentityElementFromUserGuid( UserGuid );

			// Get all of the alternate identity xml nodes.
			XmlNodeList kcList = identity.GetElementsByTagName( Alternate );

			// Do this for each identity.
			KeyChainItem[] keyChain = new KeyChainItem[ kcList.Count ];
			for ( int i = 0; i < kcList.Count; ++i )
			{
				// Point to the xml element that represents the alternate identity.
				XmlElement item = ( XmlElement )kcList[ i ];

				// Allocate the alternate identity object and put it in the array.
				keyChain[ i ] = new KeyChainItem( item.GetAttribute( AlternateId ), item.GetAttribute( AlternateDomain), item.GetAttribute( AlternateCredential ) );
			}

			return keyChain;
		}

		/// <summary>
		/// Gets the key chain item for the specified user guid.
		/// </summary>
		/// <param name="userGuid">User guid to get key chain item for.</param>
		/// <returns>A KeyChainItem object.</returns>
		public KeyChainItem GetKeyChainItem( string userGuid )
		{
			// Need to update the identity element from the file first.
			identity = factory.GetIdentityElementFromUserGuid( UserGuid );

			// Look for an existing key chain item.
			XmlElement item = ( XmlElement )identity.SelectSingleNode( Alternate + "[@" + AlternateId + " = '" + userGuid.ToLower() + "']" );
			if ( item != null )
			{
				return new KeyChainItem( userGuid, item.GetAttribute( AlternateDomain ), item.GetAttribute( AlternateCredential ) );
			}

			return null;
		}

		/// <summary>
		/// Gets the secret associated with the specified key.
		/// </summary>
		/// <param name="key">Key associated with the secret.</param>
		/// <returns>Secret associated with the specified key.</returns>
		public string GetSecret( string key )
		{
			// Need to update the identity element from the file first.
			identity = factory.GetIdentityElementFromUserGuid( UserGuid );

			// Find the secret tag.
			XmlNode secret = identity.SelectSingleNode( key );
			if ( secret == null )
			{
				throw new ApplicationException( "No such secret" );
			}

			return secret.InnerXml;
		}

		/// <summary>
		/// Gets a user guid that is associated with this identity and belongs to the specified domain.
		/// </summary>
		/// <param name="domain">Domain that alternate guid belongs to.</param>
		/// <returns>A string that represents an alternate guid.  Null will be returned if there is no alternate
		/// guid associated with the specified domain.</returns>
		public string GetUserGuidFromDomain( string domain )
		{
			if ( String.Compare( DomainName, domain, true ) == 0 )
			{
				return UserGuid;
			}
			else
			{
				KeyChainItem[] keyChain = GetKeyChain();
				foreach ( KeyChainItem kcItem in keyChain )
				{
					if ( String.Compare( kcItem.DomainName, domain, true ) == 0 )
					{
						return kcItem.UserGuid;
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Sets an alternate identity in the user's key chain.
		/// </summary>
		/// <param name="domain">Domain that the alternate identity belongs to.</param>
		/// <param name="userGuid">A unique identifier for an alternate user.</param>
		/// <param name="credential">Credential to prove the alternate user.</param>
		public void SetKeyChainItem( string domain, string userGuid, string credential )
		{
			// Need to update the identity element from the file first.
			identity = factory.GetIdentityElementFromUserGuid( UserGuid );

			// Look for an existing key chain item.
			XmlElement item = ( XmlElement )identity.SelectSingleNode( Alternate + "[@" + AlternateId + " = '" + userGuid + "']" );
			if ( item == null )
			{
				// Add the key chain item to the xml element.
				item = identity.OwnerDocument.CreateElement( Alternate );
				item.SetAttribute( AlternateDomain, domain );
				item.SetAttribute( AlternateId, userGuid.ToLower() );
				identity.AppendChild( item );
			}

			// Set the credential.
			item.SetAttribute( AlternateCredential, credential );

			// Persist the changes.
			factory.SetIdentities( identity.OwnerDocument );
		}

		/// <summary>
		/// Sets a secret in the secret store belonging to the current user.
		/// </summary>
		/// <param name="key">Key used to reference the secret.</param>
		/// <param name="secret">Secret to be stored.</param>
		public void SetSecret( string key, string secret )
		{
			// Need to update the identity element from the file first.
			identity = factory.GetIdentityElementFromUserGuid( UserGuid );

			// Look for an existing secret.
			XmlNode secretNode = identity.SelectSingleNode( key );
			if ( secretNode == null )
			{
				secretNode = identity.OwnerDocument.CreateElement( key );
				identity.AppendChild( secretNode );
			}

			secretNode.InnerText = secret;
			factory.SetIdentities( identity.OwnerDocument );
		}
		#endregion
	}

	/// <summary>
	/// Class factory implementation for an identity object.
	/// </summary>
	public class WorkGroupIdentityFactory : IIdentityFactory
	{
		#region Class Members
		/// <summary>
		/// Name of the file where identities are stored.
		/// </summary>
		private const string IdentityFile = ".wgid.xml";

		/// <summary>
		/// Xml tags.
		/// </summary>
		internal const string IdentityList = "identitylist";
		internal const string Identity = "identity";
		internal const string Alternate = "alternate";
		internal const string Domain = "domain";
		internal const string Name = "name";
		internal const string Id = "id";
		internal const string Credential = "credential";
		#endregion

		#region Properties
		/// <summary>
		/// Returns an IIdentity interface for the current user.
		/// </summary>
		public IIdentity CurrentId
		{
			get { return new WorkGroupIdentity( this, GetUserIdElement( Environment.UserName, null ) ); }
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the file path to the identity file.
		/// </summary>
		/// <returns>An absolute path to the identity file.</returns>
		private string GetIdentityFilePath()
		{
			return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), IdentityFile );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets the user id for the specified user.
		/// </summary>
		/// <param name="userName">Name of the user to find the credential for.</param>
		/// <param name="credential">Credential to use to validate the user.  If null, no validation takes place</param>
		/// <returns>An XmlElement that represents the specified user.</returns>
		internal XmlElement GetUserIdElement( string userName, object credential )
		{
			// Get the identities.
			XmlDocument xmlId = GetIdentities();

			// Search for the user node.
			XmlElement user = ( XmlElement )xmlId.SelectSingleNode( "/" + IdentityList + "/" + Identity + "[@" + Name + "='" + userName + "']" );
			if ( user != null )
			{
				// See if we need to verify the credentials.
				if ( credential != null )
				{
					// Check if the user's credentials match.
					if ( user.GetAttribute( Credential ) != ( string )credential )
					{
						throw new ApplicationException( "Invalid credential" );
					}
				}

				return user;
			}
			else
			{
				throw new ApplicationException( "No such user" );
			}
		}

		/// <summary>
		/// Gets the identity for the specified user guid.
		/// </summary>
		/// <param name="userGuid">User guid to map to main identity.</param>
		/// <returns>An XmlElement that represents the main identity.</returns>
		internal XmlElement GetIdentityElementFromUserGuid( string userGuid )
		{
			// Get the identities.
			XmlDocument xmlId = GetIdentities();

			// Search for the user node as the main identity first.
			XmlElement user = ( XmlElement )xmlId.SelectSingleNode( "/" + IdentityList + "/" + Identity + "[@" + Id + "='" + userGuid + "']" );
			if ( user == null )
			{
				// Search for the user node as a sub identity.
				user = ( XmlElement )xmlId.SelectSingleNode( "/" + IdentityList + "/" + Identity + "/" + Alternate + "[@" + Id + "='" + userGuid + "']" );
				if ( user != null )
				{
					user = ( XmlElement )user.ParentNode;
				}
				else
				{
					throw new ApplicationException( "No such identity" );
				}
			}

			return user;
		}

		/// <summary>
		/// Gets the identity secret collection.
		/// </summary>
		/// <returns>An XML document that contains the identities.</returns>
		internal XmlDocument GetIdentities()
		{
			XmlDocument xmlId = new XmlDocument();
			string fileName = GetIdentityFilePath();
			if ( File.Exists( fileName ) )
			{
				xmlId.Load( fileName );
			}
			else
			{
				// Initialize the identity file.
				XmlElement element = xmlId.CreateElement( IdentityList );
				xmlId.AppendChild( element );
			}

			return xmlId;
		}

		/// <summary>
		/// Persists the identities.
		/// </summary>
		/// <param name="xmlId">Xml document containing the identity information.</param>
		internal void SetIdentities( XmlDocument xmlId )
		{
			XmlTextWriter writer = new XmlTextWriter( GetIdentityFilePath(), null );
			writer.Formatting = Formatting.Indented;
			xmlId.WriteTo( writer );
			writer.Close();
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Creates an indentity in the Collection Store database.
		/// </summary>
		/// <param name="id">Name of identity.</param>
		/// <param name="credential">Credentials that verify the identity.</param>
		/// <returns>An interface that represents the created identity.</returns>
		public IIdentity Create( string id, object credential )
		{
			XmlElement user = null;

			try
			{
				// Check to see if the user already exists.
				user = GetUserIdElement( id, credential );
			}
			catch ( ApplicationException e )
			{
				if ( e.Message == "No such user" )
				{
					// Get the xml document that contains the identities.
					XmlDocument xmlId = GetIdentities();

					// Create a new node representing this identity.
					user = xmlId.CreateElement( Identity );
					user.SetAttribute( Name, id );
					user.SetAttribute( Domain, Environment.UserDomainName );
					user.SetAttribute( Id, Guid.NewGuid().ToString().ToLower() );
					user.SetAttribute( Credential, ( string )credential );
					xmlId.DocumentElement.AppendChild( user );

					// Save the identity information.
					SetIdentities( xmlId );
				}
				else
				{
					throw;
				}
			}

			return new WorkGroupIdentity( this, user );
		}

		/// <summary>
		/// Authenticates an identity object.
		/// </summary>
		/// <param name="id">Name of identity.</param>
		/// <param name="credential">Credentials that verify identity.</param>
		/// <returns>An interface that represent the authenticated identity.</returns>
		public IIdentity Authenticate( string id, object credential )
		{
			return new WorkGroupIdentity( this, GetUserIdElement( id, credential ) );
		}

		/// <summary>
		/// Method that returns the identity that the user Id is associated with.
		/// </summary>
		/// <param name="userGuid">Unique user identity to look up main identity with.</param>
		/// <returns>An interface to an Identity if successful, otherwise a null.</returns>
		public IIdentity GetIdentityFromUserGuid( string userGuid )
		{
			try
			{
				// See if this identity is the main identity.
				return new WorkGroupIdentity( this, GetIdentityElementFromUserGuid( userGuid ) );
			}
			catch ( ApplicationException )
			{
				return null;
			}
		}
		#endregion
	}
}
