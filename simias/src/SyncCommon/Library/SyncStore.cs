/***********************************************************************
 *  $RCSfile$
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
 *  Author: Rob
 * 
 ***********************************************************************/

using System;
using System.IO;

using Simias;
using Simias.Storage;
using Simias.Agent;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Store
	/// </summary>
	public class SyncStore : IDisposable
	{
		private string path;
		private Store baseStore;

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncStore() : this(null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="path">The collection store path</param>
		public SyncStore(string path)
		{
			this.path = path;
			
			// store required uri handling
			Uri storeUri = null;

			if ((path != null) && (path.Length > 0))
			{
				this.path = Path.GetFullPath(path);

				storeUri = new Uri(this.path);
			}
			
			// base store
			baseStore = Store.Connect(storeUri, null);
		}

		/// <summary>
		/// Open a colection object with the given id.
		/// </summary>
		/// <param name="id">The id (guid) of the collection to object.</param>
		/// <returns>The collection object.</returns>
		public SyncCollection OpenCollection(string id)
		{
			return GetCollection(baseStore.GetCollectionById(id));
		}

		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="name">The name for the collection.</param>
		/// <param name="path">The root path for the collection.</param>
		/// <returns>The collection object of the created collection.</returns>
		public SyncCollection CreateCollection(string name, string path)
		{
			Collection c = baseStore.CreateCollection(name, new Uri(Path.GetFullPath(path)));

			return GetCollection(c);
		}
		
		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="id">The id (guid) for the collection.</param>
		/// <param name="name">The name for the collection.</param>
		/// <param name="type">The type for the collection.</param>
		/// <param name="path">The root path for the collection.</param>
		/// <returns>The collection object of the created collection.</returns>
		public SyncCollection CreateCollection(string id, string name, string type, string path)
		{
			Collection c = new Collection(baseStore, name, id, type, new Uri(Path.GetFullPath(path)));

			return GetCollection(c);
		}
		
		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="id">The id (guid) for the collection.</param>
		/// <param name="name">The name for the collection.</param>
		/// <param name="type">The type for the collection.</param>
		/// <param name="path">The root path for the collection.</param>
		/// <param name="role">The sync role for the collection.</param>
		/// <returns>The collection object of the created collection.</returns>
		public SyncCollection CreateCollection(string id, string name, string type, string path, SyncCollectionRoles role)
		{
			Collection c = new Collection(baseStore, name, id, type, new Uri(Path.GetFullPath(path)));

			SyncCollection sc = GetCollection(c);

			sc.Role = role;

			return sc;
		}
		
		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="id">The id (guid) for the collection.</param>
		/// <param name="name">The name for the collection.</param>
		/// <param name="type">The type for the collection.</param>
		/// <param name="path">The root path for the collection.</param>
		/// <param name="role">The sync role for the collection.</param>
		/// <param name="host">The sync host for the collection.</param>
		/// <param name="port">The sync port for the collection.</param>
		/// <returns>The collection object of the created collection.</returns>
		public SyncCollection CreateCollection(string id, string name, string type, string path, SyncCollectionRoles role,
			string host, int port)
		{
			Collection c = new Collection(baseStore, name, id, type, new Uri(Path.GetFullPath(path)));

			SyncCollection sc = GetCollection(c);

			sc.Role = role;
			sc.Host = host;
			sc.Port = port;

			return sc;
		}
		
		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="invitation">An invitation object with the required information to create a collection.</param>
		/// <returns>The collection object of the created collection.</returns>
		public SyncCollection CreateCollection(Invitation invitation)
		{
			return CreateCollection(invitation.CollectionId, invitation.CollectionName,
				invitation.CollectionType, Path.Combine(invitation.RootPath, invitation.CollectionName),
				SyncCollectionRoles.Slave, invitation.MasterHost, int.Parse(invitation.MasterPort));
		}
		
		private SyncCollection GetCollection(Collection c)
		{
			SyncCollection sc = null;

			if (c != null)
			{
				sc = new SyncCollection(c);
			}

			return sc;
		}

		/// <summary>
		/// Generate the remoting end point for the collection.
		/// </summary>
		/// <param name="port">The remoting port of the collection.</param>
		/// <returns>The remoting end point for the collection.</returns>
		public static string GetEndPoint(int port)
		{
			return "store" + port + ".rem";
		}

		/// <summary>
		/// Delete the collection from the store.
		/// </summary>
		public void Delete()
		{
			baseStore.ImpersonateUser(Access.StoreAdminRole);
			baseStore.Delete();
			baseStore.Revert();
			baseStore.Dispose();
			
			this.Dispose();
		}

		#region IDisposable Members

		/// <summary>
		/// Clean-up all the resources used by the collection.
		/// </summary>
		public void Dispose()
		{
			baseStore.Dispose();
			baseStore = null;
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// The base store object.
		/// </summary>
		public Store BaseStore
		{
			get { return baseStore; }
		}

		/// <summary>
		/// The store id.
		/// </summary>
		public string ID
		{
			get { return baseStore.GetDatabaseObject().Id; }
		}

		/// <summary>
		/// The store path.
		/// </summary>
		public string StorePath
		{
			get { return path; }
		}

		#endregion
	}
}
