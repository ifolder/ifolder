/***********************************************************************
 *  User.cs
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

namespace Simias.Storage
{
	/// <summary>
	/// A user object is an object that exists in a collection and can map to a user somewhere out on 
	/// the network.  This user object holds information regarding the collections that a user belongs 
	/// to and other needed persistent data.
	/// </summary>
	public class User : Node
	{
		#region Class Members
		/// <summary>
		/// Well known identifier for the local collection that stores the user Ids.
		/// </summary>
		public const string LocalUserCollectionId = "4f3bce78-30af-4147-a5b0-6d343f90e003";

		/// <summary>
		/// Well known name for the collection that stores user objects.
		/// </summary>
		public const string LocalUserCollectionName = "LocalUserCollection";

		/// <summary>
		/// User roles assigned to users of the local Collection Store database.
		/// </summary>
		public enum Roles
		{
			/// <summary>
			/// Administrative user.
			/// </summary>
			Administrator,

			/// <summary>
			/// Restricted rights user.
			/// </summary>
			User
		};
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for a new user object.
		/// </summary>
		/// <param name="collection">Collection that the user will belong to.</param>
		/// <param name="name">Name of the user.</param>
		/// <param name="userRole">Role of the user.</param>
		internal User( Collection collection, string name, Roles userRole ) :
			base( collection, name, Node.NodeType.User )
		{
			// Set the Parent ID to establish hierarchy.
			Properties.AddNodeProperty( Property.ParentID, collection.Id );

			// Add the user's role type.
			Properties.AddNodeProperty( Property.UserRole, userRole.ToString() );

			// Users need to be unique in the collection. Commit the user so that it will exist in
			// the persistent store and put a hold on the user name.  If a duplicate user is found, 
			// then delete the user that was just created.
			Commit();

			// Make sure that this user is unique.
			if ( isDuplicateUser( collection, name, Id ) )
			{
				// Delete the user that was just created.
				Delete( false );
				throw new ApplicationException( "User already exists in the collection" );
			}
		}

		/// <summary>
		/// Constructor for an existing user object.
		/// </summary>
		/// <param name="collection">Collection that the user belongs to.</param>
		/// <param name="name">Name of the user.</param>
		/// <param name="id">Unique identifier for the user.</param>
		internal User( Collection collection, string name, string id ) :
			base( collection, name, id, Node.NodeType.User )
		{
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Checks for existence of a duplicate user object.
		/// </summary>
		/// <param name="collection">Collection to search in.</param>
		/// <param name="userName">Name of the user to search for.</param>
		/// <param name="id">Id of the user that should exist.</param>
		/// <returns>True if there is a duplicate user in the specified collection, otherwise false.</returns>
		private bool isDuplicateUser( Collection collection, string userName, string id )
		{
			bool isDuplicate = false;

			// Check to see if the user already exists.
			ICSEnumerator userList = collection.Search( Property.ObjectName, userName, Property.Operator.Equal );
			while ( userList.MoveNext() )
			{
				if ( ( ( ( Node )userList.Current ).Type == Node.NodeType.User ) &&
					( ( ( Node )userList.Current ).Id != id ) )
				{
					isDuplicate = true;
					break;
				}
			}

			userList.Dispose();
			return isDuplicate;
		}
		#endregion

		#region Internal Methods
		/// <summary>
		/// Creates a user with the specified name and role in the Collection Store database.
		/// </summary>
		/// <param name="store">Local store object.</param>
		/// <param name="userName">Name of the user.</param>
		/// <param name="userRole">Role of the user.</param>
		static internal User CreateUser( Store store, string userName, Roles userRole )
		{
			// Get the local user collection.
			Collection userCollection = store.GetCollection( LocalUserCollectionId );
			if ( userCollection == null )
			{
				throw new ApplicationException( "Local User Collection does not exist" );
			}

			return new User( userCollection, userName, userRole );
		}

		/// <summary>
		/// Gets an existing user with the specified name.
		/// </summary>
		/// <param name="store">Local store object.</param>
		/// <param name="userName">Name of the user.</param>
		static internal User GetUser( Store store, string userName )
		{
			// Get the local user collection.
			Collection userCollection = store.GetCollection( LocalUserCollectionId );
			if ( userCollection == null )
			{
				throw new ApplicationException( "Local User Collection does not exist" );
			}

			User user = null;

			// Search the local user collection for objects with this name.
			ICSEnumerator userList = userCollection.Search( Property.ObjectName, userName, Property.Operator.Equal );
			while ( userList.MoveNext() )
			{
				// Only look at user objects.
				if ( ( ( Node )userList.Current ).Type == Node.NodeType.User )
				{
					user = ( User )userList.Current;
					break;
				}
			}

			userList.Dispose();
			return user;
		}
		#endregion
	}
}
