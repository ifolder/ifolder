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

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents the local address book that contains identity objects which 
	/// represent users of the collection store. 
	/// </summary>
	public class LocalAddressBook : Collection
	{
		#region Class Members
		/// <summary>
		/// Initial size of the list that keeps track of the dirty nodes.
		/// </summary>
		private const int initialDirtyNodeListSize = 10;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for this object that creates and persists the local address book.
		/// </summary>
		/// <param name="store">Local store object.</param>
		/// <param name="bookName">Name of the address book.</param>
		/// <param name="ownerGuid">Owner guid of this collection.</param>
		internal LocalAddressBook( Store store, string bookName, string ownerGuid ) :
			base( store, new CacheNode( store, Guid.NewGuid().ToString().ToLower() ), false )
		{
			// Fill out the Cache node object.
			cNode.collection = this;
			cNode.name = bookName;
			cNode.type = Node.CollectionType + Property.AddressBookType;
			cNode.isPersisted = false;
			cNode.properties = new PropertyList( this );
			cNode.dirtyNodeList = new Hashtable( initialDirtyNodeListSize );

			// Set the default access control for this collection.
			cNode.accessControl = new AccessControl( this );

			// Add the owner of this collection.
			Properties.AddNodeProperty( Property.Owner, ownerGuid );
			AccessControlEntry ace = new AccessControlEntry( ownerGuid, Access.Rights.Admin );
			ace.Set( this );

			// Give the backup operator read/write rights.
			ace = new AccessControlEntry( Access.BackupOperatorRole, Access.Rights.ReadWrite );
			ace.Set( this );

			// Give the sync operator all access rights.
			ace = new AccessControlEntry( Access.SyncOperatorRole, Access.Rights.Admin );
			ace.Set( this );

			// Set an ACL that allows everyone read/write access.
			ace = new AccessControlEntry( Access.WorldRole, Access.Rights.ReadWrite );
			ace.Set( this );

			// Update the access control cache on this object.
			UpdateAccessControl();

			// Use default document root. If the document root directory does not exist, create it.
			Uri documentRoot = GetStoreManagedPath();
			if ( !Directory.Exists( documentRoot.LocalPath ) )
			{
				Directory.CreateDirectory( documentRoot.LocalPath );
			}

			// Set the default properties for this node.
			Properties.AddNodeProperty( Property.CreationTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.ModifyTime, DateTime.UtcNow );
			Properties.AddNodeProperty( Property.CollectionID, Id );
			Properties.AddNodeProperty( Property.IDPath, "/" + Id );
			Properties.AddNodeProperty( Property.DomainName, bookName );

			// Add the document root as a local property.
			Property docRootProp = new Property( Property.DocumentRoot, documentRoot );
			docRootProp.LocalProperty = true;
			Properties.AddNodeProperty( docRootProp );

			// Set the sync versions.
			Property mvProp = new Property( Property.MasterIncarnation, ( ulong )0 );
			mvProp.LocalProperty = true;
			Properties.AddNodeProperty( mvProp );

			Property lvProp = new Property( Property.LocalIncarnation, ( ulong )0 );
			lvProp.LocalProperty = true;
			Properties.AddNodeProperty( lvProp );

			// Make this the default address book.
			Property p = new Property( Property.DefaultAddressBook, true );
			p.LocalProperty = true;
			Properties.AddNodeProperty( p );

			// Add the local address book property.
			Properties.AddNodeProperty( Property.LocalAddressBook, true );

			// Set that this collection is not synchronizable.
			Properties.AddNodeProperty( Property.Syncable, false );

			// Add this node to the cache table.
			cNode = cNode.AddToCacheTable();
		}

		/// <summary>
		/// Constructor for this object that creates a local address book from an existing collection.
		/// </summary>
		/// <param name="store">Local store object.</param>
		/// <param name="collection">Collection that is an address book.</param>
		internal LocalAddressBook( Store store, Collection collection ) :
			base( store, collection.cNode, true )
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
			lock ( store )
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
		}

		/// <summary>
		/// Deletes an identity form the local address book.
		/// </summary>
		/// <param name="userGuid">Unique identifier of identity to delete.</param>
		public void DeleteIdentity( string userGuid )
		{
			lock ( store )
			{
				// Lookup the identity to make sure that it exists.
				Identity identity = GetIdentityById( userGuid );
				if ( identity != null )
				{
					identity.Delete( true );
				}
			}
		}

		/// <summary>
		/// Gets the identity in the name base that matches the specified user Id.
		/// </summary>
		/// <param name="userGuid">Id of the user to find associated identity for.</param>
		/// <returns>An Identity object representing the user Id if it exists. Otherwise a null is returned.</returns>
		public Identity GetIdentityById( string userGuid )
		{
			lock ( store )
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
		}

		/// <summary>
		/// Gets the first identity in the name base that matches the specified user name.
		/// </summary>
		/// <param name="userName">Name of the user to find associated identity for.</param>
		/// <returns>An Identity object representing the userName if it exists. Otherwise a null is returned.</returns>
		public Identity GetSingleIdentityByName( string userName )
		{
			lock ( store )
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
		}
		#endregion
	}
}
