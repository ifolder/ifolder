/***********************************************************************
 *  StoreIdentity.cs - Class that implements identity management which
 *  provides access control to collection objects in the store.
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
 * 
 ***********************************************************************/

using System;
using System.Collections;
using Persist = Simias.Storage.Provider;

namespace Simias.Storage
{
	/// <summary>
	/// Used to hold impersonation information on the impersonation stack.
	/// </summary>
	internal struct ImpersonationInfo
	{
		#region Class Members
		/// <summary>
		/// Guid the current impersonating identity is known as.
		/// </summary>
		public string userGuid;
		
		/// <summary>
		/// The identity that describes the user.
		/// </summary>
		public Identity identity;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor using a GUID. This constructor is used when impersonating a well-known Id.
		/// </summary>
		/// <param name="userGuid">User guid of the impersonating user.</param>
		public ImpersonationInfo( string userGuid )
		{
			this.userGuid = userGuid;
			this.identity = null;
		}

		/// <summary>
		/// Constructor using an identity.  This constructor is used when impersonating a regular user Id.
		/// </summary>
		/// <param name="identity">Identity that describes the user.</param>
		public ImpersonationInfo( Identity identity )
		{
			this.userGuid = identity.Id;
			this.identity = identity;
		}
		#endregion
	}

	/// <summary>
	/// Implements the store identity which is used to control access to Collection Store objects.
	/// There is only one identity that is allowed to authenticate to the CollectionStore database, 
	/// since the database only ever has one owner.  All other identities can access the database 
	/// only by impersonation.
	/// </summary>
	internal class StoreIdentity
	{
		#region Class Members
		/// <summary>
		/// Name of this domain that the store object belongs in.
		/// </summary>
		private string domainName = null;

		/// <summary>
		/// Represents the identity of the user that instantiated this object.
		/// </summary>
		private Identity identity = null;

		/// <summary>
		/// Container used to keep track of the current identity for this store handle.
		/// </summary>
		private Stack impersonationId = new Stack();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the currently impersonating user guid.
		/// </summary>
		public string CurrentUserGuid
		{
			get { return ( impersonationId.Count == 0 ) ? identity.Id : ( ( ImpersonationInfo )impersonationId.Peek() ).userGuid; }
		}

		/// <summary>
		/// Gets the current impersonating identity.
		/// </summary>
		private Identity CurrentIdentity
		{
			get { return ( impersonationId.Count == 0 ) ? identity : ( ( ImpersonationInfo )impersonationId.Peek() ).identity; }
		}

		/// <summary>
		/// Gets the domain name where this identity exists.
		/// </summary>
		public string DomainName
		{
			get { return domainName; }
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor of the object.
		/// </summary>
		/// <param name="identity">Object that represents the current identity.</param>
		private StoreIdentity( Identity identity )
		{
			this.identity = identity;
		}

		/// <summary>
		/// Constructor that allows for impersonation of well known Ids.
		/// </summary>
		private StoreIdentity() :
			this( null )
		{
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Authenticates the identity that owns the database.
		/// </summary>
		/// <param name="store">Handle to the collection store.</param>
		/// <returns>A store identity object that represents the database owner.</returns>
		static public void Authenticate( Store store )
		{
			// Temporarily create an object that we can use to impersonate well known identities with.
			store.LocalIdentity = new StoreIdentity();

			// Impersonate the local store admin so that we can bootstrap ourselves.
			store.ImpersonateUser( Access.StoreAdminRole );

			// Open the local address book.
			LocalAddressBook localAb = store.GetLocalAddressBook();
			if ( localAb == null )
			{
				throw new ApplicationException( "Local address book does not exist." );
			}

			// Get the user that we are currently running as and get his identity information.
			Identity identity = localAb.GetSingleIdentityByName( Environment.UserName );
			if ( identity == null )
			{
				throw new ApplicationException( "No such user." );
			}

			// Create a store identity object that will be used by the store object from here on out.
			store.LocalIdentity = new StoreIdentity( identity );

			// Get the database object to check if this ID is the same as the owner.
			Collection dbCollection = store.GetDatabaseObject();
			if ( dbCollection == null )
			{
				throw new ApplicationException( "Store database object does not exist" );
			}

			// Get the owner property and make sure that it is the same as the current user.
			if ( dbCollection.Owner != store.CurrentUser )
			{
				throw new UnauthorizedAccessException( "Current user is not store owner." );
			}

			// Set the domain name for this identity.
			store.LocalIdentity.domainName = dbCollection.DomainName;
		}

		/// <summary>
		/// Creates an identity representing the database owner.
		/// </summary>
		/// <returns>A store identity object that represents the database owner.</returns>
		static public void CreateIdentity( Store store )
		{
			// Temporarily create an object that we can use to impersonate well known identities with.
			store.LocalIdentity = new StoreIdentity();

			// Impersonate the local store admin so that we can bootstrap ourselves.
			store.ImpersonateUser( Access.StoreAdminRole );

			// Create the name for this domain.
			store.LocalIdentity.domainName = Environment.UserDomainName + ":" + Guid.NewGuid().ToString().ToLower();

			// Create the local address book.
			LocalAddressBook localAb = new LocalAddressBook( store, store.LocalIdentity.domainName );

			// Add the currently executing user as an identity in the address book.
			Identity currentUser = new Identity( localAb, Environment.UserName );

			// Add a key pair to this identity to be used as credentials.
			currentUser.CreateKeyPair();

			// Change the local address book to be owned by the current user.
			localAb.ChangeOwner( currentUser.Id, Access.Rights.Deny );
			localAb.Commit( true );

			// Create a new identity object that represents the current user.
			store.LocalIdentity = new StoreIdentity( currentUser );

			// Set the domain name in the new identity object.
			store.LocalIdentity.domainName = localAb.Name;
		}

		/// <summary>
		/// Gets the identity of the current user and all of its aliases.
		/// </summary>
		/// <returns>An array list of guids that represents the current user and all of its aliases.</returns>
		public ArrayList GetIdentityAndAliases()
		{
			ArrayList ids = new ArrayList();
			ids.Add( CurrentUserGuid );

			Identity currentId = CurrentIdentity;
			if ( currentId != null )
			{
				// Get a list of aliases this user is known by in other domains.
				ICSList aliasList = currentId.GetAliases();
				foreach ( Alias alias in aliasList )
				{
					ids.Add( alias.Id );
				}
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
			string userGuid = CurrentUserGuid;

			// If no domain is speicified or it is the current domain, use the current identity.
			if ( ( domain != null ) && ( domain != domainName ) )
			{
				// This is not the store's domain.  Look through the list of aliases that this
				// identity is known by in other domains.
				Identity currentId = CurrentIdentity;
				if ( currentId != null )
				{
					// Get a list of aliases this user is known by in other domains.
					ICSList aliasList = currentId.GetAliases();
					foreach ( Alias alias in aliasList )
					{
						if ( alias.Domain == domain )
						{
							userGuid = alias.Id;
							break;
						}
					}
				}
			}

			return userGuid;
		}

		/// <summary>
		/// Impersonates the specified identity, if the userId is verified.
		/// TODO: May want to look at limiting who can impersonate.
		/// </summary>
		/// <param name="userId">User ID to impersonate.</param>
		public void Impersonate( string userId )
		{
			switch ( userId )
			{
				case Access.StoreAdminRole:
					impersonationId.Push( new ImpersonationInfo( userId ) );
					break;

				case Access.BackupOperatorRole:
					impersonationId.Push( new ImpersonationInfo( userId ) );
					break;

				case Access.SyncOperatorRole:
					impersonationId.Push( new ImpersonationInfo( userId ) );
					break;

				default:
					// Look up the specified user in the local address book.
					Identity impIdentity = ( identity.CollectionNode as LocalAddressBook ).GetIdentityById( userId );
					if ( impIdentity == null )
					{
						throw new ApplicationException( "No such user." );
					}

					// Push the user onto the impersonation stack.
					impersonationId.Push( new ImpersonationInfo( impIdentity ) );
					break;
			}
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			// Don't ever pop an empty stack.
			if ( impersonationId.Count > 0 )
			{
				impersonationId.Pop();
			}
		}
		#endregion
	}
}
