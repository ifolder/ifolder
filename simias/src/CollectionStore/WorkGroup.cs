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
using System.IO;
using System.Xml;

namespace Simias.Storage
{
	/// <summary>
	/// Class that represents a work space that can be shared with other people. This object will
	/// contain user contact information.
	/// </summary>
	public class WorkGroup: Collection
	{
		#region Constructors
		/// <summary>
		/// Constructor for this object that creates the local address book.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="name">Name of the object.</param>
		public WorkGroup( Store storeObject, string name ) :
			this( storeObject, name, Guid.NewGuid().ToString() )
		{
		}

		/// <summary>
		/// Constructor for this object that creates the local address book.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="name">Name of the object.</param>
		/// <param name="ID">The globally unique identifier for this object.</param>
		public WorkGroup( Store storeObject, string name, string ID ) :
			this( storeObject, name, ID, storeObject.CurrentUserGuid )
		{
		}

		/// <summary>
		/// Constructor to create an existing WorkGroup object from a Node object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="node">Node object to construct this object from.</param>
		public WorkGroup( Store storeObject, Node node ) :
			base( storeObject, node )
		{
			if ( type != NodeTypes.WorkGroupType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.WorkGroupType, type ) );
			}
		}

		/// <summary>
		/// Constructor for creating an existing WorkGroup object from a ShallowNode.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="shallowNode">A ShallowNode object.</param>
		public WorkGroup( Store storeObject, ShallowNode shallowNode ) :
			base( storeObject, shallowNode )
		{
			if ( type != NodeTypes.WorkGroupType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.WorkGroupType, type ) );
			}
		}

		/// <summary>
		/// Constructor for this object that creates a WorkGroup object.
		/// </summary>
		/// <param name="storeObject">Store object.</param>
		/// <param name="name">Name of the object.</param>
		/// <param name="ID">The globally unique identifier for this object.</param>
		/// <param name="ownerGuid">Owner identifier of this object.</param>
		internal WorkGroup( Store storeObject, string name, string ID, string ownerGuid ) :
			base ( storeObject, name, ID, NodeTypes.WorkGroupType, ownerGuid, name + ":" + ID.ToLower() )
		{
		}

		/// <summary>
		/// Constructor to create an existing WorkGroup object from an Xml document object.
		/// </summary>
		/// <param name="storeObject">Store object that this collection belongs to.</param>
		/// <param name="document">Xml document object to construct this object from.</param>
		internal WorkGroup( Store storeObject, XmlDocument document ) :
			base( storeObject, document )
		{
			if ( type != NodeTypes.WorkGroupType )
			{
				throw new CollectionStoreException( String.Format( "Cannot construct an object type of {0} from an object of type {1}.", NodeTypes.WorkGroupType, type ) );
			}
		}
		#endregion
	}
}
