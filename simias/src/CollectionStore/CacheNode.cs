/***********************************************************************
 *  CacheNode.cs - Class that implements singleton data access for 
 *  multiple nodes.
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
using System.Threading;
using Simias;

namespace Simias.Storage
{
	/// <summary>
	/// Represents the data of a node in the store.  Multiple node objects allocated from
	/// the same store object can reference this as a singleton object.
	/// </summary>
	public class CacheNode : IDisposable
	{
		#region Class Members
		/// <summary>
		/// Used to know when to let go of this object.
		/// </summary>
		internal long referenceCount = 0;

		/// <summary>
		/// A reference to the local store object.
		/// </summary>
		internal Store store;

		/// <summary>
		/// Object that contains a list of properties on the node.
		/// </summary>
		public PropertyList properties = null;

		/// <summary>
		/// The display name for the node.  This is the "friendly" name that
		/// applications will use for the node.
		/// </summary>
		public string name;

		/// <summary>
		/// The globally unique identifier for this node.
		/// </summary>
		public readonly string id;

		/// <summary>
		/// The type of node.
		/// </summary>
		public string type;

		/// <summary>
		/// All nodes must belong to a collection.  This is here so we don't have to look
		/// it up each time we need to know which collection we belong to.
		/// </summary>
		public Collection collection;

		/// <summary>
		/// A flag that is used to tell whether this node has been committed to the database or if it
		/// is newly created.
		/// </summary>
		public bool isPersisted;

		/// <summary>
		/// Contains nodes that have changed and need to be written to the persistent store.
		/// </summary>
		public Hashtable dirtyNodeList = null;

		/// <summary>
		/// Provides an interface for access control.
		/// </summary>
		public AccessControl accessControl = null;

		/// <summary>
		/// List used to hold Property objects so that they may be merged at object commit time.
		/// </summary>
		public ArrayList mergeList = new ArrayList();
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor for the object.  All other members will be initialized by the referencing object.
		/// </summary>
		/// <param name="store">An object that represents the local store.</param>
		/// <param name="id">The Id that uniquely identifies this object.</param>
		public CacheNode( Store store, string id )
		{
			this.id = id;
			this.store = store;
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Adds this cache node to the store's cache table.
		/// </summary>
		/// <returns>If there is already an existing node in the cache table, it will be returned in place of the
		/// current object.  Otherwise the current object is added to the table and returned.</returns>
		public CacheNode AddToCacheTable()
		{
			// See if this node already exists in the store cache table.
			return store.SetCacheNode( id, this );
		}

		/// <summary>
		/// Copies the specified cache node object to this object.
		/// </summary>
		/// <param name="cNode">Cache node object to copy.</param>
		public void Copy( CacheNode cNode )
		{
			this.properties.Copy( cNode.properties );
			this.name = cNode.name;
			this.type = cNode.type;
			this.isPersisted = cNode.isPersisted;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Decrements the reference count on the object and removes it from the hashtable when it
		/// goes to zero.
		/// </summary>
		public void Dispose()
		{
			// Remove this object from the store cache table.
			store.RemoveCacheNode( id, false );
		}
		#endregion
	}
}
