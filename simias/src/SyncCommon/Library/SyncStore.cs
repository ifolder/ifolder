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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Store
	/// </summary>
	public class SyncStore : Store
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SyncStore(Configuration configuration) : base(configuration)
		{

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
			Uri MasterUri)
		{
			Collection c = new Collection(baseStore, name, id, type, new Uri(Path.GetFullPath(path)));

			SyncCollection sc = GetCollection(c);

			sc.Role = role;
			sc.MasterUri = MasterUri;

			return sc;
		}
		
		/// <summary>
		/// Create a collection.
		/// </summary>
		/// <param name="invitation">An invitation object with the required information to create a collection.</param>
		/// <returns>The collection object of the created collection.</returns>
		public SyncCollection CreateCollection(Invitation invitation)
		{
			// add any secret to the current identity chain
			if ((invitation.PublicKey != null) && (invitation.PublicKey.Length > 0))
			{
				Identity identity = baseStore.CurrentIdentity;
				identity.CreateAlias(invitation.Domain, invitation.Identity, invitation.PublicKey);
				identity.Commit();
			}
	
			return CreateCollection(invitation.CollectionId, invitation.CollectionName,
				invitation.CollectionType, Path.Combine(invitation.RootPath, invitation.CollectionName),
				SyncCollectionRoles.Slave, invitation.MasterUri);
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
		/// The remoting end point for the sync store service.
		/// </summary>
		public static string GetEndPoint(int port)
		{
			return String.Format("SyncStoreService{0}.rem", port);
		}

		#region Properties
		
		/// <summary>
		/// The store id.
		/// </summary>
		public string ID
		{
			get { return base.GetDatabaseObject().ID; }
		}

		#endregion
	}
}
