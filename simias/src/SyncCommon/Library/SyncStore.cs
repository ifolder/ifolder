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

		public SyncStore() : this(null)
		{
		}

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

		public SyncCollection OpenCollection(string id)
		{
			return GetCollection(baseStore.GetCollectionById(id));
		}

		public SyncCollection CreateCollection(string name, string path)
		{
			Collection c = baseStore.CreateCollection(name, new Uri(Path.GetFullPath(path)));

			return GetCollection(c);
		}
		
		public SyncCollection CreateCollection(string id, string name, string type, string path)
		{
			Collection c = new Collection(baseStore, name, id, type,
				new Uri(Path.GetFullPath(path)));

			return GetCollection(c);
		}
		
		public SyncCollection CreateCollection(string id, string name, string type, string path, SyncCollectionRoles role)
		{
			Collection c = new Collection(baseStore, name, id, type,
				new Uri(Path.GetFullPath(path)));

			SyncCollection sc = GetCollection(c);

			sc.Role = role;

			return sc;
		}
		
		public SyncCollection CreateCollection(Invitation invitation)
		{
			return CreateCollection(invitation.CollectionId, invitation.CollectionName,
				invitation.CollectionType, Path.Combine(invitation.RootPath, invitation.CollectionName),
				SyncCollectionRoles.Slave, invitation.MasterHost, int.Parse(invitation.MasterPort));
		}
		
		public SyncCollection CreateCollection(string id, string name, string type, string path, SyncCollectionRoles role, string host, int port)
		{
			Collection c = new Collection(baseStore, name, id, type,
				new Uri(Path.GetFullPath(path)));

			SyncCollection sc = GetCollection(c);

			sc.Role = role;
			sc.Host = host;
			sc.Port = port;

			return sc;
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

		public static string GetEndPoint(int port)
		{
			return "store" + port + ".rem";
		}

		public void Delete()
		{
			baseStore.ImpersonateUser(Access.StoreAdminRole);
			baseStore.Delete();
			baseStore.Revert();
			baseStore.Dispose();
			
			this.Dispose();
		}

		#region IDisposable Members

		public void Dispose()
		{
			baseStore.Dispose();
			baseStore = null;
		}

		#endregion

		#region Properties
		
		public Store BaseStore
		{
			get { return baseStore; }
		}

		public string ID
		{
			get { return baseStore.GetDatabaseObject().Id; }
		}

		public string StorePath
		{
			get { return path; }
		}

		#endregion
	}
}
