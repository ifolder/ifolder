/***********************************************************************
 *  Identity.cs - Class that represents a user in the Collection Store.
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
 *			Brady Anderson <banderso@novell.com>
 * 
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a user in the Collection Store.
	/// </summary>
	public class Identity : Node
	{
		#region Properties
		/// <summary>
		/// Gets the public/private key values for the specified identity.
		/// NOTE: Usually this type of method is not provided as allowing anyone access to the private key is a
		/// bad idea.  However, in the case of Collection Store the private key is stored in the local database
		/// and is assumed to be protected by the file system access control. If a process has access to the file
		/// system and is the database owner, then it would have access to this key information.
		/// </summary>
		public RSAParameters KeyValues
		{
			get
			{
				// Lookup the credential property on the identity.
				Property p = Properties.GetSingleProperty( Property.Credential );
				if ( p != null )
				{
					RSA credential = RSA.Create();
					credential.FromXmlString( p.Value as string );
					return credential.ExportParameters( true );
				}
				else
				{
					return new RSAParameters();
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
			// Add the address book collection as the parent for this node.
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
		}

		/// <summary>
		/// Constructor for creating a new identity.
		/// </summary>
		/// <param name="localAb">Address book that the identity will belong to.</param>
		/// <param name="userName">User name of the identity.</param>
		public Identity( LocalAddressBook localAb, string userName ) :
			this( localAb, userName, Guid.NewGuid().ToString().ToLower(), Property.IdentityType )
		{
		}

		/// <summary>
		/// Constructor that creates a identity from a Node.
		/// </summary>
		/// <param name="node">Node that is an identity type.</param>
		internal Identity( Node node ) :
			base( node.cNode )
		{
			// Need to convert the collection type to a local address book type.
			InternalCollectionHandle = new LocalAddressBook( CollectionNode.LocalStore, CollectionNode );
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
		/// Generates an RSA public/private key pair that will be used to authenticate this identity
		/// to a remote connection.
		/// </summary>
		internal void CreateKeyPair()
		{
			// The RSA parameters will be stored as a string object on the identity node.
			RSA credential = RSA.Create();
			
			// Create a local, hidden property to store the credential in.
			Property p = new Property( Property.Credential, credential.ToXmlString( true ) );
			p.HiddenProperty = true;
			p.LocalProperty = true;
			Properties.ModifyNodeProperty( p );
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Sets a property on the identity object that represents this identity in another domain.
		/// </summary>
		/// <param name="domain">Domain that id is contained in.</param>
		/// <param name="userGuid">Unique identifier that user is known as in specified domain. </param>
		/// <param name="keyValues">RSAParameters object used to prove identity to a remote connection.</param>
		public Alias CreateAlias( string domain, string userGuid, RSAParameters keyValues )
		{
			// Set up an alias object to store on this identity.
			Alias alias = new Alias( domain, userGuid, keyValues );

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
				p.Delete();
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
		/// Gets the list of aliases that this user is known by in other domains.
		/// </summary>
		/// <returns>An ICSList object containing all of the aliases for this identity.</returns>
		public ICSList GetAliasList()
		{
			ICSList aliasList = new ICSList();
			MultiValuedList mvl = Properties.FindValues( Property.Alias );
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
			ids.Add( Id );

			// Add any aliases to the list also.
			foreach ( Alias alias in GetAliasList() )
			{
				ids.Add( alias.Id );
			}

			return ids;
		}

		/// <summary>
		/// Returns the user guid that the current user is known as in the specified domain.
		/// </summary>
		/// <param name="domain">The domain that the user is in.</param>
		/// <returns>A string representing the user's guid in the specified domain.  If the user does not exist
		/// in the specified domain, the current user guid is returned.</returns>
		public string GetDomainUserGuid( string domain )
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
		#endregion
	}
}
