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
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Class that defines the type of access control rights available on a Collection Store.
	/// </summary>
	public class Access : ICSEnumerator
	{
		#region Class Members
		/// <summary>
		/// Access rights used by the CollectionStore.  These rights are mutually exclusive.
		/// However they are cumulative, meaning that if a user has the ReadWrite right, they
		/// also have the ReadOnly right.
		/// </summary>
		public enum Rights 
		{ 
			/// <summary>
			/// User has no rights to the collection.
			/// </summary>
			Deny,

			/// <summary>
			/// User can view information in a collection.
			/// </summary>
			ReadOnly, 

			/// <summary>
			/// User can view and modify information in a collection.
			/// </summary>
			ReadWrite, 

			/// <summary>
			/// User can view, modify and change rights in a collection.
			/// </summary>
			Admin 
		};

		/// <summary>
		/// Well known identity role for the administrator of the local database.
		/// </summary>
		public const string StoreAdminRole = "1edcfe90-45e8-11d8-a9c7-444553544200";

		/// <summary>
		/// Well known identity role for the synchronizing operator.
		/// </summary>
		public const string SyncOperatorRole = "1edcfe91-45e8-11d8-a9c7-444553544200";

		/// <summary>
		/// Well known identity role for the backup operator.
		/// </summary>
		public const string BackupOperatorRole = "1edcfe92-45e8-11d8-a9c7-444553544200";

		/// <summary>
		/// Well know identity role for world access.
		/// </summary>
		public const string WorldRole = "1edcfe93-45e8-11d8-a9c7-444553544200";

		/// <summary>
		/// Reference to the store object.
		/// </summary>
		private Store store;

		/// <summary>
		/// Access control list enumerator.
		/// </summary>
		private ICSEnumerator aclEnumerator;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for Access object.
		/// </summary>
		/// <param name="collection">Collection to enumerator rights on.</param>
		internal Access( Collection collection )
		{
			this.store = collection.LocalStore;
			MultiValuedList mvl = collection.Properties.FindValues( Property.Ace, true );
			aclEnumerator = ( ICSEnumerator )mvl.GetEnumerator();
		}
		#endregion

		#region IEnumerator Members
		/// <summary>
		/// Sets the enumerator to its initial position, which is before
		/// the first element in the collection.
		/// </summary>
		public void Reset()
		{
			lock ( store )
			{
				aclEnumerator.Reset();
			}
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get 
			{ 
				lock ( store )
				{
					return new AccessControlEntry( ( Property )aclEnumerator.Current ); 
				}
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; 
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			lock ( store )
			{
				return aclEnumerator.MoveNext();
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// This is declared here to satisfy the interface requirements, but the MultiValuedEnumerator
		/// does not use any unmanaged resources that it needs to dispose of.
		/// </summary>
		public void Dispose()
		{
		}
		#endregion
	}

	/// <summary>
	/// Object that represents an access control entry.
	/// </summary>
	public class AccessControlEntry
	{
		#region Class Members
		/// <summary>
		/// Identifier for the user.
		/// </summary>
		private string id;

		/// <summary>
		/// Access rights for the user.
		/// </summary>
		private Access.Rights rights;

		/// <summary>
		///  Property that represents the access control information.
		/// </summary>
		private Property aceProperty;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the id for this entry.
		/// </summary>
		public string Id
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the access rights for this entry.
		/// </summary>
		public Access.Rights Rights
		{
			get { return rights; }
		}

		/// <summary>
		/// Returns true if this object is a well known identity
		/// </summary>
		public bool WellKnown
		{
			get
			{
				if( (id == Access.StoreAdminRole)		||
					(id == Access.SyncOperatorRole)		||
					(id == Access.BackupOperatorRole)	||
					(id == Access.WorldRole) )
					return true;
				return false;
			}
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for the AccessControlEntry object.
		/// </summary>
		/// <param name="id">Identifier for user.</param>
		/// <param name="rights">Access control rights to assign to user.</param>
		internal AccessControlEntry( string id, Access.Rights rights )
		{
			this.id = id.ToLower();
			this.rights = rights;
			aceProperty = new Property( Property.Ace, this.id + ":" + Enum.GetName( typeof( Access.Rights ), rights ) );
			aceProperty.HiddenProperty = true;
		}

		/// <summary>
		/// Constructor for the AccessControlEntry object.
		/// </summary>
		/// <param name="aceProperty">A Property object that contains access control information.</param>
		internal AccessControlEntry( Property aceProperty )
		{
			this.aceProperty = aceProperty;

			// Find the delimiting character in the string.
			string acs = aceProperty.ToString();
			int delimiter = acs.IndexOf( ':' );
			if ( delimiter == 0 )
			{
				throw new ApplicationException( "Invalid access control format" );
			}

			// Get the id out of the string.
			id = acs.Substring(0, delimiter );

			// Get the rights out of the string.
			rights = ( Access.Rights )Enum.Parse( typeof( Access.Rights ), acs.Substring( delimiter + 1 ) );
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Removes this access control entry from off of the collection.
		/// </summary>
		internal void Delete()
		{
			// Only need to remove this ace if it is really associated with a collection.
			if ( aceProperty.IsAssociatedProperty )
			{
				aceProperty.DeleteProperty();
			}
		}

		/// <summary>
		/// Sets the access control information contained by this object on the specified collection.
		/// </summary>
		/// <param name="collection">Collection to set access control information on.</param>
		internal void Set( Collection collection )
		{
			// If this property is not already associated with the specified collection, add it.
			if ( !aceProperty.IsAssociatedProperty )
			{
				collection.Properties.AddNodeProperty( aceProperty );
			}
		}

		/// <summary>
		/// Sets the desired access rights on the object.
		/// </summary>
		/// <param name="desiredRights">Rights to set on the object.</param>
		internal void SetRights( Access.Rights desiredRights )
		{
			rights = desiredRights; 
			aceProperty.SetPropertyValue( id + ":" + Enum.GetName( typeof( Access.Rights ), rights ) );
		}
		#endregion
	}

	/// <summary>
	/// Object that provides access control functionality for the CollectionStore.
	/// </summary>
	internal class AccessControl
	{
		#region Class Members
		/// <summary>
		/// Collection object that this object controls access for.
		/// </summary>
		private Collection collection;

		/// <summary>
		/// Specifies the owner of the collection.
		/// </summary>
		private string owner = null;

		/// <summary>
		/// List of access control entries at the time the object was constructed.
		/// This list will be updated whenever the collection access control list is changed.
		/// </summary>
		private ArrayList committedAcl = new ArrayList();

		/// <summary>
		/// Constructor access control entry so it doesn't have to be looked up each time.
		/// </summary>
		private AccessControlEntry impersonatingAce = null;

		/// <summary>
		/// World ace used to cache the ace entry so it doesn't have to be looked up each time.
		/// </summary>
		private AccessControlEntry worldAce = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the owner of the collection.
		/// </summary>
		public string Owner
		{
			get 
			{ 
				if ( owner == null )
				{
					// The collection has never been accessed. Force the collection properties to be loaded on this
					// object and the owner will automatically be filled in.
					collection.SetNodeProperties();
				}

				return owner; 
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="collection">Collection that this object controls access for.</param>
		public AccessControl( Collection collection )
		{
			this.collection = collection;
		}

		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="collection">Collection that this object controls access for.</param>
		/// <param name="constructorId">User identifier that is constructing the specified collection object.</param>
		public AccessControl( Collection collection, string constructorId )
		{
			this.collection = collection;
			owner = constructorId.ToLower();
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Finds the access control entry for the specified user ID.
		/// </summary>
		/// <param name="userId">User ID to find access control entry for.</param>
		/// <returns>An AccessControlEntry object that contains the access rights for the specified user.</returns>
		private AccessControlEntry FindAce( string userId )
		{
			AccessControlEntry ace = null;

			// Find the specified access control entry in the committed acl.
			foreach ( AccessControlEntry committedAce in committedAcl )
			{
				if ( committedAce.Id == userId )
				{
					ace = committedAce;
					break;
				}
			}

			return ace;
		}

		/// <summary>
		/// Returns whether the user has owner rights to the collection.
		/// </summary>
		/// <param name="userId">User id to check for owner rights.</param>
		/// <returns>True if userId has owner rights on the collection, otherwise false.</returns>
		private bool IsOwner( string userId )
		{
			return ( ( userId == Access.StoreAdminRole ) || ( userId == Owner ) ) ? true : false;
		}

		/// <summary>
		/// Determines if the world role has the desired access rights.
		/// </summary>
		/// <param name="desiredRights">Desired rights.</param>
		/// <returns>True if the world role has the desired access rights, otherwise false.</returns>
		private bool IsWorldAccessAllowed( Access.Rights desiredRights )
		{
			bool allowed = false;

			if ( worldAce == null )
			{
				// See if world access is allowed.
				worldAce = FindAce( Access.WorldRole );
				if ( ( worldAce != null ) && ( worldAce.Rights >= desiredRights ) )
				{
					allowed = true;
				}
			}
			else if ( worldAce.Rights >= desiredRights )
			{
				allowed = true;
			}

			return allowed;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Gets the current list of access control entries from the collection object and maintains them
		/// in this object.
		/// </summary>
		internal void GetCommittedAcl()
		{
			// Clear out the old acl.
			committedAcl.Clear();
			impersonatingAce = null;

			// Get the list of access control entries.
			ICSList acl = collection.GetAccessControlList();
			foreach ( AccessControlEntry ace in acl )
			{
				committedAcl.Add( ace );
			}

			// Get the committed owner of the collection.
			Property ownerProperty = collection.Properties.GetSingleProperty( Property.Owner );
			if ( ownerProperty != null )
			{
				owner = ownerProperty.ToString();
			}
			else
			{
				throw new ApplicationException( "There is no owner for the collection." );
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Makes the specified user owner of the collection that this object protects.
		/// </summary>
		/// <param name="userId">User ID to make owner.</param>
		/// <param name="oldOwnerRight">The rights that the old owner should be assigned.</param>
		public void ChangeOwner( string userId, Access.Rights oldOwnerRight )
		{
			// Only the current owner can change ownership rights.
			if ( !IsOwnerAccessAllowed() )
			{
				throw new UnauthorizedAccessException( "Current user cannot modify collection owner's right." );
			}

			// Find the existing owner ace.
			AccessControlEntry oldOwnerAce = FindAce( Owner );
			if ( oldOwnerAce != null )
			{
				// Reset the old ace.
				if ( oldOwnerRight == Access.Rights.Deny )
				{
					// Old owner will have no rights to the collection.
					oldOwnerAce.Delete();
				}
				else
				{
					// Set the new right for the old owner.
					oldOwnerAce.SetRights( oldOwnerRight );
					oldOwnerAce.Set( collection );
				}
			}
			else
			{
				throw new ApplicationException( "Existing collection does not have an owner ace" );
			}

			// Add an ace for the new owner. Check if there is an existing ace first.
			AccessControlEntry newOwnerAce = FindAce( userId );
			if ( newOwnerAce != null )
			{
				// Just change the rights on the current ace and reset it.
				newOwnerAce.SetRights( Access.Rights.Admin );
			}
			else
			{
				// The ace did not exist. Set the new rights on the collection.
				newOwnerAce = new AccessControlEntry( userId, Access.Rights.Admin );
			}

			// Set the new collection owner.
			newOwnerAce.Set( collection );
			collection.Properties.ModifyNodeProperty( Property.Owner, userId );
		}

		/// <summary>
		/// Gets the access rights for the specified user on the collection protected by this object.
		/// </summary>
		/// <param name="userId">User ID to get rights for.</param>
		/// <returns>Access rights for the specified user ID.</returns>
		public Access.Rights GetUserRights( string userId )
		{
			Access.Rights rights;

			// See if the user is the owner of the store or the collection.
			if ( IsOwner( userId ) )
			{
				// User has all rights.
				rights = Access.Rights.Admin;
			}
			else
			{
				// See if there is an ace for this user.
				AccessControlEntry ace = FindAce( userId );
				if ( ace != null )
				{
					// User has rights granted to them.
					rights = ace.Rights;
				}
				else
				{
					// User has no rights.
					rights = Access.Rights.Deny;
				}
			}

			return rights;
		}

		/// <summary>
		/// Determines if the current user has the desired access rights.
		/// </summary>
		/// <param name="desiredRights">Desired rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Access.Rights desiredRights )
		{
			bool allowed = true;
			string currentId = collection.DomainIdentity;

			// Is this user the database owner?
			if ( ( currentId != Access.StoreAdminRole ) && ( currentId != Owner ) )
			{
				// Check if the current identity's ace has already been found.
				if ( impersonatingAce == null || ( currentId != impersonatingAce.Id ) )
				{
					// Check the rights on the owner ace.
					impersonatingAce = FindAce( currentId );
					if ( ( impersonatingAce == null ) || ( impersonatingAce.Rights < desiredRights ) )
					{
						allowed = IsWorldAccessAllowed( desiredRights );
					}
				}
				else if ( impersonatingAce.Rights < desiredRights )
				{
					allowed = IsWorldAccessAllowed( desiredRights );
				}
			}

			return allowed;
		}

		/// <summary>
		/// Determines if the current user has owner rights to this collection.  This means that the
		/// current user must be either the database owner or the collection owner.
		/// </summary>
		/// <returns>True if the current user is a database owner or collection owner. Otherwise false is returned.</returns>
		public bool IsOwnerAccessAllowed()
		{
			// Is this user the database owner or the collection owner?
			string currentId = collection.DomainIdentity;
			return ( ( currentId == Access.StoreAdminRole ) || ( currentId == Owner ) ) ? true : false;
		}

		/// <summary>
		/// Removes all access rights on the collection for the specified user.
		/// </summary>
		/// <param name="userId">User ID to remove rights for.</param>
		public void RemoveUserRights( string userId )
		{
			if ( !IsAccessAllowed( Access.Rights.Admin ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection access modify right." );
			}

			// Don't allow the owner's access to be removed.
			if ( IsOwner( userId ) )
			{
				throw new UnauthorizedAccessException( "Cannot remove owner access rights" );
			}

			// Find the user's ace and remove it.
			AccessControlEntry ace = FindAce( userId );
			if ( ace != null )
			{
				ace.Delete();
			}
		}

		/// <summary>
		/// Sets the specified access rights for the specified user on the collection protected by this object.
		/// </summary>
		/// <param name="userId">User ID to set rights for.</param>
		/// <param name="rights">Access rights to set for the user.</param>
		public void SetUserRights( string userId, Access.Rights rights )
		{
			// See if current user has rights to change access control list.
			if ( !IsAccessAllowed( Access.Rights.Admin ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection access modify right." );
			}

			// Don't allow the collection owner's rights to be modified.
			if ( IsOwner( userId ) )
			{
				throw new UnauthorizedAccessException( "Current user cannot modify collection owner's right." );
			}

			// Check if there is an existing ace for the specified user.
			AccessControlEntry ace = FindAce( userId );
			if ( ace != null )
			{
				if ( rights == Access.Rights.Deny )
				{
					// Remove the current ace off the collection.
					ace.Delete();
				}
				else
				{
					// Just change the rights on the current ace and reset it.
					ace.SetRights( rights );
					ace.Set( collection );
				}
			}
			else
			{
				// The ace did not exist.  If this is a deny, we don't have to do anything.
				if ( rights != Access.Rights.Deny )
				{
					// Set the new rights on the collection.
					ace = new AccessControlEntry( userId, rights );
					ace.Set( collection );
				}
			}
		}

		/// <summary>
		/// Makes the current user owner of a new collection that this object protects and adds other
		/// default access control.
		/// </summary>
		public void SetDefaultAccessControl()
		{
			// Set the owner of this collection.
			owner = collection.LocalStore.CurrentUser;
			collection.Properties.AddNodeProperty( Property.Owner, owner );
			AccessControlEntry ace = new AccessControlEntry( owner, Access.Rights.Admin );
			ace.Set( collection );

			// Give the backup operator read/write rights.
			ace = new AccessControlEntry( Access.BackupOperatorRole, Access.Rights.ReadWrite );
			ace.Set( collection );

			// Give the sync operator all access rights.
			ace = new AccessControlEntry( Access.SyncOperatorRole, Access.Rights.Admin );
			ace.Set( collection );
		}
		#endregion
	}
}
