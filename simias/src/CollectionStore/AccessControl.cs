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

		/// Well know identity role for world access.
		/// </summary>
		public const string World = "1edcfe93-45e8-11d8-a9c7-444553544200";

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
			aclEnumerator.Reset();
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get { return new AccessControlEntry( ( Property )aclEnumerator.Current ); }
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
			return aclEnumerator.MoveNext();
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
		public string ID
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
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for the AccessControlEntry object.
		/// </summary>
		/// <param name="ID">Identifier for user.</param>
		/// <param name="accessRights">Access control rights to assign to user.</param>
		internal AccessControlEntry( string ID, Access.Rights accessRights )
		{
			id = ID.ToLower();
			rights = accessRights;
			aceProperty = new Property( Property.Ace, id + ":" + Enum.GetName( typeof( Access.Rights ), rights ) );
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
	/// Object that provides access control functionality for the Collection Store.
	/// </summary>
	internal class AccessControl
	{
		#region Class Members
		/// <summary>
		/// Collection object that this object protects.
		/// </summary>
		private Collection collection;

		/// <summary>
		/// Specifies the identifier of the owner of the collection.
		/// </summary>
		private string ownerID;

		/// <summary>
		/// List of access control entries at the time the object was constructed.
		/// This list will be updated whenever the collection access control list is changed.
		/// </summary>
		private ArrayList aclList = new ArrayList();

		/// <summary>
		/// Constructor access control entry so it doesn't have to be looked up each time.
		/// </summary>
		private AccessControlEntry impersonatingAce;

		/// <summary>
		/// World ace used to cache the ace entry so it doesn't have to be looked up each time.
		/// </summary>
		private AccessControlEntry worldAce;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the object.
		/// </summary>
		/// <param name="collection">Collection that this object controls access for.</param>
		public AccessControl( Collection collection )
		{
			this.collection = collection;
			GetAccessInfo();
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Finds the access control entry for the specified user ID.
		/// </summary>
		/// <param name="userID">User ID to find access control entry for.</param>
		/// <returns>An AccessControlEntry object that contains the access rights for the specified user.</returns>
		private AccessControlEntry FindAce( string userID )
		{
			AccessControlEntry ace = null;

			// Find the specified access control entry in the acl list.
			foreach ( AccessControlEntry committedAce in aclList )
			{
				if ( committedAce.ID == userID )
				{
					ace = committedAce;
					break;
				}
			}

			return ace;
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
				worldAce = FindAce( Access.World );
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
		/// Makes the specified user owner of the collection that this object protects.
		/// </summary>
		/// <param name="userID">User ID to make owner.</param>
		/// <param name="oldOwnerRight">The rights that the old owner should be assigned.</param>
		internal void ChangeCollectionOwner( string userID, Access.Rights oldOwnerRight )
		{
			// Normalize the user ID.
			string normUserID = userID.ToLower();

			// Find the existing owner ace.
			AccessControlEntry oldOwnerAce = FindAce( collection.Owner );
			if ( oldOwnerAce == null )
			{
				throw new ApplicationException( "Existing collection does not have an owner ace" );
			}

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

			// Add an ace for the new owner. Check if there is an existing ace first.
			AccessControlEntry newOwnerAce = FindAce( normUserID );
			if ( newOwnerAce != null )
			{
				// Just change the rights on the current ace and reset it.
				newOwnerAce.SetRights( Access.Rights.Admin );
			}
			else
			{
				// The ace did not exist. Set the new rights on the collection.
				newOwnerAce = new AccessControlEntry( normUserID, Access.Rights.Admin );
			}

			// Set the new collection owner.
			newOwnerAce.Set( collection );
			collection.Properties.ModifyNodeProperty( Property.Owner, normUserID );
		}

		/// <summary>
		/// Gets the current list of access control entries from the collection object and maintains them
		/// in this object.
		/// </summary>
		internal void GetAccessInfo()
		{
			// Clear out the old access information
			aclList.Clear();
			impersonatingAce = null;
			worldAce = null;

			// Get the list of access control entries.
			ICSList acl = collection.GetAccessControlList();
			foreach ( AccessControlEntry ace in acl )
			{
				aclList.Add( ace );
			}

			// Get the committed owner of the collection.
			ownerID = collection.Owner;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Makes the specified user owner of the collection that this object protects.
		/// </summary>
		/// <param name="userID">User ID to make owner.</param>
		/// <param name="oldOwnerRight">The rights that the old owner should be assigned.</param>
		public void ChangeOwner( string userID, Access.Rights oldOwnerRight )
		{
			// Only the current owner can change ownership rights.
			if ( !IsOwnerAccessAllowed() )
			{
				throw new UnauthorizedAccessException( "Current user cannot modify collection owner's right." );
			}

			ChangeCollectionOwner( userID, oldOwnerRight );
		}

		/// <summary>
		/// Gets the access rights for the specified user on the collection protected by this object.
		/// </summary>
		/// <param name="userID">User ID to get rights for.</param>
		/// <returns>Access rights for the specified user ID.</returns>
		public Access.Rights GetUserRights( string userID )
		{
			// See if there is an ace for this user.
			AccessControlEntry ace = FindAce( userID.ToLower() );
			return ( ace != null ) ? ace.Rights : Access.Rights.Deny;
		}

		/// <summary>
		/// Determines if the current user has the desired access rights.
		/// </summary>
		/// <param name="desiredRights">Desired rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Access.Rights desiredRights )
		{
			bool allowed = true;
			string currentID = collection.DomainIdentity;

			// Is this user the database owner?
			if ( collection.StoreReference.IsImpersonating && ( currentID != ownerID ) )
			{
				// Check if the current identity's ace has already been found.
				if ( impersonatingAce == null || ( currentID != impersonatingAce.ID ) )
				{
					// Check the rights on the owner ace.
					impersonatingAce = FindAce( currentID );
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
		/// Returns whether the user has owner rights to the collection.
		/// </summary>
		/// <param name="userID">User id to check for owner rights.</param>
		/// <returns>True if userId has owner rights on the collection, otherwise false.</returns>
		public bool IsOwner( string userID )
		{
			return ( !collection.StoreReference.IsImpersonating || ( userID == ownerID ) ) ? true : false;
		}

		/// <summary>
		/// Determines if the current user has owner rights to this collection.  This means that the
		/// current user must be either the database owner or the collection owner.
		/// </summary>
		/// <returns>True if the current user is a database owner or collection owner. Otherwise false is returned.</returns>
		public bool IsOwnerAccessAllowed()
		{
			// Is this user the collection owner?
			return IsOwner( collection.DomainIdentity );
		}

		/// <summary>
		/// Removes all access rights on the collection for the specified user.
		/// </summary>
		/// <param name="userId">User ID to remove rights for.</param>
		public void RemoveUserRights( string userID )
		{
			if ( !IsAccessAllowed( Access.Rights.Admin ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection access modify right." );
			}

			// Don't allow the owner's access to be removed.
			if ( IsOwner( userID ) )
			{
				throw new UnauthorizedAccessException( "Cannot remove owner access rights" );
			}

			// Find the user's ace and remove it.
			AccessControlEntry ace = FindAce( userID.ToLower() );
			if ( ace != null )
			{
				ace.Delete();
			}
		}

		/// <summary>
		/// Sets the specified access rights for the specified user on the collection protected by this object.
		/// </summary>
		/// <param name="userID">User ID to set rights for.</param>
		/// <param name="rights">Access rights to set for the user.</param>
		public void SetUserRights( string userID, Access.Rights rights )
		{
			// See if current user has rights to change access control list.
			if ( !IsAccessAllowed( Access.Rights.Admin ) )
			{
				throw new UnauthorizedAccessException( "Current user does not have collection access modify right." );
			}

			// Don't allow the collection owner's rights to be modified.
			if ( IsOwner( userID ) )
			{
				throw new UnauthorizedAccessException( "Current user cannot modify collection owner's right." );
			}

			// Normalize the user ID
			string normUserID = userID.ToLower();

			// Check if there is an existing ace for the specified user.
			AccessControlEntry ace = FindAce( normUserID );
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
					ace = new AccessControlEntry( normUserID, rights );
					ace.Set( collection );
				}
			}
		}
		#endregion
	}
}
