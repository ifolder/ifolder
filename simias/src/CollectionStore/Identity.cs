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
		/// Used to separate the user ID and domain ID in the Domain property.
		/// </summary>
		static private char valueSeparator = ':';
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
			// Create a local, hidden property to store the credential in.
			// The RSA parameters will be stored as a string object on the identity node.
			RSACryptoServiceProvider credential = new RSACryptoServiceProvider( 1024 );
			Property p = new Property( PropertyTags.Credential, credential.ToXmlString( true ) );
			p.HiddenProperty = true;
			p.LocalProperty = true;
			properties.AddNodeProperty( p );
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
			properties.AddNodeProperty( PropertyTags.Domain, userID.ToLower() + valueSeparator + domainID.ToLower() );
			return this;
		}

		/// <summary>
		/// Removes the specified domain mapping from the identity object.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>The modified identity object.</returns>
		internal Identity DeleteDomainIdentity( string domainID )
		{
			// Find the property to be deleted.
			Property p = GetPropertyByDomain( domainID.ToLower() );
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
			// Find the property associated with the user ID.
			Property p = GetPropertyByUserID( userID.ToLower() );
			return ( p != null ) ? ( p.Value as string ).Substring( userID.Length + 1 ) : null;
		}

		/// <summary>
		/// Gets the user ID associated with the specified domain ID.
		/// </summary>
		/// <param name="domainID">Well known identity for the specified domain.</param>
		/// <returns>User ID associated with the specified domain ID if it exists. Otherwise null is returned.</returns>
		internal string GetUserIDFromDomain( string domainID )
		{
			// Find the property associated with the user ID.
			Property p = GetPropertyByDomain( domainID.ToLower() );
			return ( p != null ) ? ( p.Value as string ).Substring( 0, domainID.Length ) : null;
		}
		#endregion
	}
}
