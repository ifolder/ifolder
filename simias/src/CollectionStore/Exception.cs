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
	public class CollectionStoreException : Exception
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
		/// Initializes a new instance of the object class with serialized data.
		/// </summary>
		/// <param name="info">The SerializationInfo that holds the serialized object 
		/// data about the exception being thrown.</param>
		/// <param name="context">The StreamingContext that contains contextual information 
		/// about the source or destination.</param>
		public CollectionStoreException( SerializationInfo info, StreamingContext context ) :
			base ( info, context )
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
	/// Exception that indicates a Node object collision when writing to the
	/// local database.
	/// </summary>
	public class Collision : CollectionStoreException
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
		public Collision( string ID, ulong expectedIncarnation ) :
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
			get { return ID; }
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
}
