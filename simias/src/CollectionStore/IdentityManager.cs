/***********************************************************************
 *  IdentityManager.cs - Class that implements identity management which
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
	/// Implements the identity manager which is used to control access to Collection Store objects.
	/// There is only one identity that is allowed to authenticate to the CollectionStore database, 
	/// since the database only ever has one owner.  All other identities can access the database 
	/// only by impersonation.
	/// </summary>
	internal class IdentityManager
	{
		#region Class Members
		/// <summary>
		/// Name of this domain that the store object belongs in.
		/// </summary>
		private string domainName;

		/// <summary>
		/// Represents the identity of the user that instantiated this object.
		/// </summary>
		private Identity identity;

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
		public Identity CurrentIdentity
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
		public IdentityManager( Identity identity )
		{
			this.identity = identity;
			this.domainName = identity.CollectionNode.DomainName;
		}

		/// <summary>
		/// Constructor of the object.
		/// </summary>
		/// <param name="domainName">Name of the domain for this identity.</param>
		private IdentityManager( string domainName )
		{
			this.identity = null;
			this.domainName = domainName;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Gets an identity which represents the store administrator.
		/// </summary>
		/// <param name="domainName">Name of the domain.</param>
		static public IdentityManager CreateStoreAdmin( string domainName )
		{
			// Create a temporary identity in the local address book for the admin role.
			IdentityManager identity = new IdentityManager( domainName );

			// Push the user onto the impersonation stack.
			identity.impersonationId.Push( new ImpersonationInfo( Access.StoreAdminRole ) );
			return identity;
		}

		/// <summary>
		/// Impersonates the specified identity, if the userId is verified.
		/// TODO: May want to look at limiting who can impersonate.
		/// </summary>
		/// <param name="userId">User ID to impersonate.</param>
		public void Impersonate( string userId )
		{
			// Look up the specified user in the local address book.
			Identity impIdentity = ( identity.CollectionNode as LocalAddressBook ).GetIdentityById( userId );
			if ( impIdentity == null )
			{
				throw new ApplicationException( "No such user." );
			}

			// Push the user onto the impersonation stack.
			impersonationId.Push( new ImpersonationInfo( impIdentity ) );
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
