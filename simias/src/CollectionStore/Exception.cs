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
using System.Runtime.Serialization;

using Simias;

namespace Simias.Storage
{
	/// <summary>
	/// Collection Store exceptions.
	/// </summary>
	public class CollectionStoreException : SimiasException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class with a specified error message.
		/// </summary>
		public CollectionStoreException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the object class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public CollectionStoreException( string message ) :
			base ( message )
		{
		}

		/// <summary>
		/// Initializes a new instance of the Exception class with a reference to the inner 
		/// exception that is the cause of this exception.
		/// </summary>
		/// <param name="innerException">The exception that is the cause of the current exception. If 
		/// the innerException parameter is not a null reference, the current exception is raised in 
		/// a catch block that handles the inner exception.</param>
		public CollectionStoreException( Exception innerException ) :
			this ( innerException.Message, innerException )
		{
		}

		/// <summary>
		/// Initializes a new instance of the Exception class with a specified error message and 
		/// a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If 
		/// the innerException parameter is not a null reference, the current exception is raised in 
		/// a catch block that handles the inner exception.</param>
		public CollectionStoreException( string message, Exception innerException ) :
			base ( message, innerException )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that a Collection access violation has occurred.
	/// </summary>
	public class AccessException : CollectionStoreException
	{
		#region Class Members
		private Collection collection;
		private Access.Rights desiredRights;
		private string currentUserGuid;
		private bool ownerAccess;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the collection where access was denied.
		/// </summary>
		public Collection Collection
		{
			get { return collection; }
		}

		/// <summary>
		/// Gets the requested access rights.
		/// </summary>
		public Access.Rights DesiredRights
		{
			get { return desiredRights; }
		}

		/// <summary>
		/// Gets the user that is requesting the access to the collection.
		/// </summary>
		public string CurrentUserGuid
		{
			get { return currentUserGuid; }
		}

		/// <summary>
		/// Gets whether the user is requesting owner access to the collection.
		/// </summary>
		public bool DesiredOwnerAccess
		{
			get { return ownerAccess; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class where access is being requested.
		/// </summary>
		/// <param name="collection">Collection where access violation occurred.</param>
		/// <param name="member">Member that is requesting the access.</param>
		/// <param name="desiredRights">Requested access rights.</param>
		public AccessException( Collection collection, Member member, Access.Rights desiredRights ) :
			this ( collection, member, desiredRights, "Current user does not have collection modify rights." )
		{
		}

		/// <summary>
		/// Initializes a new instance of the object class where access is being requested.
		/// </summary>
		/// <param name="collection">Collection where access violation occurred.</param>
		/// <param name="member">Member that is requesting the access.</param>
		/// <param name="desiredRights">Requested access rights.</param>
		/// <param name="message">Message to pass to exception.</param>
		public AccessException( Collection collection, Member member, Access.Rights desiredRights, string message ) :
			base ( message )
		{
			this.collection = collection;
			this.desiredRights = desiredRights;
			this.currentUserGuid = member.UserID;
			this.ownerAccess = false;
		}

		/// <summary>
		/// Initializes a new instance of the object class where owner access is being requested.
		/// </summary>
		/// <param name="collection">Collection where access violation occurred.</param>
		/// <param name="member">Member that is requesting the access.</param>
		/// <param name="message">Message to pass to exception.</param>
		public AccessException( Collection collection, Member member, string message ) :
			base( message )
		{
			this.collection = collection;
			this.desiredRights = Access.Rights.Admin;
			this.currentUserGuid = member.UserID;
			this.ownerAccess = true;
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that an object already exists.
	/// </summary>
	public class AlreadyExistsException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="message">Message to pass to the exception.</param>
		public AlreadyExistsException( string message ) :
			base ( message )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates a Node object collision when writing to the local database.
	/// </summary>
	public class CollisionException : CollectionStoreException
	{
		#region Class Members
		/// <summary>
		/// The globally unique identifier of the Node objects that have a conflict.
		/// </summary>
		private string id;

		/// <summary>
		/// The incarnation value that was expected during the commit.
		/// </summary>
		private ulong expectedIncarnation;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class with a specified Node object ID and 
		/// the expected incarnation value of the Node object.
		/// </summary>
		/// <param name="ID">The globally unique identifier of the Node objects that have a conflict.</param>
		/// <param name="expectedIncarnation">The incarnation value that was expected during the commit.</param>
		public CollisionException( string ID, ulong expectedIncarnation ) :
			base ( "There was a collision during the update of a Node object." )
		{
			this.id = ID;
			this.expectedIncarnation = expectedIncarnation;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets the identifier for the Node object that the collision occurred on.
		/// </summary>
		public string ID
		{
			get { return id; }
		}

		/// <summary>
		/// Gets the expected incarnation value.
		/// </summary>
		public ulong ExpectedIncarnation
		{
			get { return expectedIncarnation; }
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates an object has been disposed.
	/// </summary>
	public class DisposedException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="csObject">The Collection Store object that has been disposed.</param>
		public DisposedException( object csObject) :
			base ( String.Format( "The object {0} has been disposed.", csObject.ToString() ) )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that the specified object does not exist.
	/// </summary>
	public class DoesNotExistException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="message">Message to pass to exception.</param>
		public DoesNotExistException( string message ) :
			base ( message )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that an invalid operation was attempted.
	/// </summary>
	public class InvalidOperationException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="message">A string containing the type of invalid access operation.</param>
		public InvalidOperationException( string message ) :
			base ( message )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates an invalid syntax type was specified.
	/// </summary>
	public class SyntaxException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		/// <param name="syntax">Invalid syntax type.</param>
		public SyntaxException( Syntax syntax ) :
			base ( String.Format( "Invalid syntax type was specified: {0}.", syntax.ToString() ) )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that a policy violation has occurred.
	/// </summary>
	public class PolicyException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class where access is being requested.
		/// </summary>
		public PolicyException() :
			base ( "A policy exception has occurred." )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that a collection is locked and changes are not permitted.
	/// </summary>
	public class LockException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public LockException() :
			base ( "Collection is locked." )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that the store is being shutdown.
	/// </summary>
	public class SimiasShutdownException : CollectionStoreException
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public SimiasShutdownException() :
			base ( "The simias store process is being shutdown." )
		{
		}
		#endregion
	}

	/// <summary>
	/// Exception that indicates that collection has already been locked.
	/// </summary>
	public class AlreadyLockedException : CollectionStoreException
	{
		#region Class Members
		private string collectionName;
		private string collectionID;
		#endregion

		#region Properties
		/// <summary>
		/// Gets the name of the collection.
		/// </summary>
		public string CollectionName
		{
			get { return collectionName; }
		}

		/// <summary>
		/// Gets the identifier of the collection.
		/// </summary>
		public string CollectionID
		{
			get { return collectionID; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the object class.
		/// </summary>
		public AlreadyLockedException( Collection c ) :
			base ( String.Format( "The collection {0} has already been locked.", c.Name ) )
		{
			this.collectionName = c.Name;
			this.collectionID = c.ID;
		}
		#endregion
	}
}
