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

namespace Simias.Storage
{
	/// <summary>
	/// Class that defines the type of access control rights available on a Collection Store.
	/// </summary>
	public class Access
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
		/// Well know identity role for world access.
		/// </summary>
		public const string World = "1edcfe93-45e8-11d8-a9c7-444553544200";
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
			set 
			{
				rights = value;
				aceProperty.SetPropertyValue( id + ":" + Enum.GetName( typeof( Access.Rights ), rights ) );
			}
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
			aceProperty = new Property( PropertyTags.Ace, id + ":" + Enum.GetName( typeof( Access.Rights ), rights ) );
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
				throw new CollectionStoreException( String.Format( "Invalid access control format: {0}.", acs ) );
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
		/// Sets the access control information contained by this object on the specified Member object.
		/// </summary>
		/// <param name="member">Member object to set access control information on.</param>
		internal void Set( Member member )
		{
			// If this property is not already associated with the specified collection, add it.
			if ( !aceProperty.IsAssociatedProperty )
			{
				member.Properties.AddNodeProperty( aceProperty );
			}
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
		/// Container used to keep track of the current identity for this collection.
		/// </summary>
		private Stack impersonationList = new Stack();
		#endregion

		#region Properties
		/// <summary>
		/// Gets the Member object of the currently impersonating user.
		/// </summary>
		public Member ImpersonationMember
		{
			get { return IsImpersonating ? impersonationList.Peek() as Member : null; }
		}

		/// <summary>
		/// Gets whether there is a user being impersonated.
		/// </summary>
		public bool IsImpersonating
		{
			get { return ( impersonationList.Count > 0 ) ? true : false; }
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
		#endregion

		#region Private Methods
		/// <summary>
		/// Determines if the world role has the desired access rights.
		/// </summary>
		/// <param name="desiredRights">Desired rights.</param>
		/// <returns>True if the world role has the desired access rights, otherwise false.</returns>
		private bool IsWorldAccessAllowed( Access.Rights desiredRights )
		{
			Member member = GetMember( Access.World );
			return ( ( member != null ) && ( member.ValidateAce.Rights >= desiredRights ) ) ? true : false;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Makes the specified user owner of the collection that this object protects.
		/// </summary>
		/// <param name="newOwner">Member object that is to become the new owner.</param>
		/// <param name="oldOwnerRight">The rights that the old owner should be assigned.</param>
		/// <returns>An array of Nodes which need to be committed to make this operation permanent.</returns>
		public Node[] ChangeOwner( Member newOwner, Access.Rights oldOwnerRight )
		{
			// Find the existing owner.
			Member oldOwner = collection.Owner;
			if ( oldOwner == null )
			{
				throw new DoesNotExistException( String.Format( "The collection: {0} - ID: {1} does not have an owner.", collection.Name, collection.ID ) );
			}

			// Reset the old ace.
			if ( oldOwnerRight == Access.Rights.Deny )
			{
				// Old owner will have no rights to the collection.
				collection.Delete( oldOwner );
			}
			else
			{
				// Set the new right for the old owner.
				oldOwner.Rights = oldOwnerRight;
				oldOwner.IsOwner = false;
			}

			// Just change the rights on the current ace and reset it.
			newOwner.Rights = Access.Rights.Admin;
			newOwner.IsOwner = true;

			Node[] nodeList = { oldOwner, newOwner };
			return nodeList;
		}

		/// <summary>
		/// Gets the Member object that represents the currently executing security context.
		/// </summary>
		/// <param name="store">Store object.</param>
		/// <param name="domainID">The domain used to map the current user to.</param>
		/// <param name="createMember">If true, creates Member object if it does not exist.</param>
		/// <returns>A Member object that represents the currently executing security context.</returns>
		public Member GetCurrentMember( Store store, string domainID, bool createMember )
		{
			// See if there is a currently impersonating user.
			Member member = ImpersonationMember;
			if ( member == null )
			{
				// This collection is not currently being impersonated, go look up the Member object of 
				// the current identity in the store.
				Identity identity = store.CurrentUser;
				string userID = identity.GetUserIDFromDomain( domainID );
				if ( userID == null )
				{
					// The domain mapping has to exist or it means that we never were invited to this domain.
					throw new DoesNotExistException( String.Format( "There is no identity mapping for identity {0} to domain {1}.", identity.ID, domainID ) );
				}

				// Check in the local store to see if there is an existing member.
				member = GetMember( userID );
				if ( member == null )
				{
					// The Member object does not exist, we were specified to create it with full rights.
					// If this is a collection in a proxy state, create a special member for it with read
					// only rights. This member will not be committable and should be treated as a read-only
					// object.
					string userName = identity.Name;

					// Get the member that is represented in the domain.
					Domain domain = store.GetDomain( collection.Domain );
					if ( domain != null )
					{
						// Find the member in the domain.
						Member domainMember = domain.GetMemberByID( userID );
						if ( domainMember != null )
						{
							userName = domainMember.Name;
						}
					}

					if ( createMember )
					{
						member = new Member( userName, userID, Access.Rights.Admin );
						member.IsOwner = true;
					}
					else
					{
						throw new DoesNotExistException( String.Format( "The identity {0} - ID: {1} is not a member of collection {2} - ID: {3}.", userName, userID, collection.Name, collection.ID ) );
					}
				}
			}

			return member;
		}

		/// <summary>
		/// Gets the specified Member object.
		/// </summary>
		/// <param name="userID">User ID of the member to find.</param>
		/// <returns>The Member object represented by the specified user guid.</returns>
		public Member GetMember( string userID )
		{
			ICSList list = collection.Search( PropertyTags.Ace, userID, SearchOp.Begins );
			ICSEnumerator e = list.GetEnumerator() as ICSEnumerator;
			Member member = e.MoveNext() ? new Member( collection, e.Current as ShallowNode ) : null;
			e.Dispose();
			return member;
		}

		/// <summary>
		/// Impersonates the specified Member object.
		/// </summary>
		/// <param name="member">Member object to impersonate.</param>
		public void Impersonate( Member member )
		{
			// Push the user onto the impersonation stack.
			impersonationList.Push( member );
		}

		/// <summary>
		/// Determines if the current user has the desired access rights.
		/// </summary>
		/// <param name="member">Member object to check access for.</param>
		/// <param name="desiredRights">Desired rights.</param>
		/// <returns>True if the user has the desired access rights, otherwise false.</returns>
		public bool IsAccessAllowed( Member member, Access.Rights desiredRights )
		{
			bool allowed = true;

			// Check if the member has sufficient rights.
			if ( member.ValidateAce.Rights < desiredRights )
			{
				allowed = IsWorldAccessAllowed( desiredRights );
			}

			return allowed;
		}

		/// <summary>
		/// Reverts back to the previous impersonating identity.
		/// </summary>
		public void Revert()
		{
			// Don't ever pop an empty stack.
			if ( impersonationList.Count > 0 )
			{
				impersonationList.Pop();
			}
		}
		#endregion
	}
}
