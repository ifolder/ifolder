/***********************************************************************
 *  LocalAddressBook.cs - Class that represents the local address book
 *  that contains identity objects which represent users of the 
 *  collection store. This object as a whole is not synchronized or 
 *  sharable, although the identities contained by this object may be.
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

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents the local address book that contains identity objects which 
	/// represent users of the collection store. 
	/// </summary>
	public class LocalAddressBook : Collection
	{
		#region Constructor
		/// <summary>
		/// Constructor for this object that creates and persists a local address book.
		/// </summary>
		/// <param name="store">Local store object.</param>
		/// <param name="bookName">Name of the address book.</param>
		internal LocalAddressBook( Store store, string bookName ) :
			base( store, bookName, Property.AddressBookType )
		{
			// Add the local address book property.
			Properties.AddNodeProperty( Property.LocalAddressBook, true );

			// Make this the default address book.
			Property p = new Property( Property.DefaultAddressBook, true );
			p.LocalProperty = true;
			Properties.AddNodeProperty( p );

			// Set an ACL that allows everyone read/write access.
			SetUserAccess( Access.WorldRole, Access.Rights.ReadWrite );

			// Set that this collection is not synchronizable.
			Synchronizeable = false;

			// Commit the changes.
			Commit();
		}

		/// <summary>
		/// Constructor for this object that creates a local address book from an existing collection.
		/// </summary>
		/// <param name="store">Local store object.</param>
		/// <param name="collection">Collection that is an address book.</param>
		internal LocalAddressBook( Store store, Collection collection ) :
			base( store, collection.cNode )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds an identity to the local address book.
		/// </summary>
		/// <param name="userName">Name of the identity to add.</param>
		/// <returns>An Identity object representing the specified user.</returns>
		public Identity AddIdentity( string userName )
		{
			return AddIdentity( userName, Guid.NewGuid().ToString().ToLower() );
		}

		/// <summary>
		/// Adds an identity to the local address book with an Id.
		/// </summary>
		/// <param name="userName">Name of the identity to add.</param>
		/// <param name="userGuid">Unique identifier for user.</param>
		/// <returns>An Identity object representing the specified user.</returns>
		public Identity AddIdentity( string userName, string userGuid )
		{
			// Check to see if this identity already exists.
			Identity identity = GetSingleIdentityByName( userName );
			if ( identity == null )
			{
				// See if the same identity exists by Id.
				identity = GetIdentityById( userGuid );
				if ( identity == null )
				{
					// The identity doesn't exist, create it.
					identity = new Identity( this, userName, userGuid );
				}
			}
			else
			{
				// Don't want to have separate identities with the same user name.
				if ( identity.Id != userGuid )
				{
					throw new ApplicationException( "Identity already exists by specified name." );
				}
			}

			return identity;
		}

		/// <summary>
		/// Deletes an identity form the local address book.
		/// </summary>
		/// <param name="userGuid">Unique identifier of identity to delete.</param>
		public void DeleteIdentity( string userGuid )
		{
			// Lookup the identity to make sure that it exists.
			Identity identity = GetIdentityById( userGuid );
			if ( identity != null )
			{
				identity.Delete( true );
			}
		}

		/// <summary>
		/// Gets the identity in the name base that matches the specified user Id.
		/// </summary>
		/// <param name="userGuid">Id of the user to find associated identity for.</param>
		/// <returns>An Identity object representing the user Id if it exists. Otherwise a null is returned.</returns>
		public Identity GetIdentityById( string userGuid )
		{
			Identity identityNode = null;

			// Get the matching node out of the address book collection.
			Node tempNode = GetNodeById( userGuid );
			if ( tempNode != null )
			{
				identityNode = new Identity( tempNode );
			}

			return identityNode;
		}

		/// <summary>
		/// Gets the first identity in the name base that matches the specified user name.
		/// </summary>
		/// <param name="userName">Name of the user to find associated identity for.</param>
		/// <returns>An Identity object representing the userName if it exists. Otherwise a null is returned.</returns>
		public Identity GetSingleIdentityByName( string userName )
		{
			Identity identityNode = null;

			// Get the matching node out of the address book collection.
			Node tempNode = GetSingleNodeByName( userName );
			if ( tempNode != null )
			{
				identityNode = new Identity( tempNode );
			}

			return identityNode;
		}
		#endregion
	}
}
