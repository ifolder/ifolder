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

using Simias.Agent;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Tests
{
	/// <summary>
	/// A Sync Test Store
	/// </summary>
	public class SyncTestStore : IDisposable
	{
		private static readonly string host = "127.0.0.1";

		string path;
		int port;
		Uri uri;

		Store store;
		Synker synker;

		int activations = 0;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="path">The local path for the store.</param>
		public SyncTestStore(string path, int port)
		{
			// save arguments
			this.path = path;
			this.port = port;

			// remove the directory
			if (Directory.Exists(path)) Directory.Delete(path, true);

			// generate store uri
			this.uri = new Uri(Path.GetFullPath(path));

			// create store
			store = Store.Connect(uri);

			// create synker
			synker = new Synker();
		}

		public void SynkerUpdate(bool active)
		{
			// count activations
			if (active) ++activations;
			
			// show an update
			Console.WriteLine("Synker Status [{0}] : {1}", path,
				(active ? "Active" : "Inactive"));
		}

		public void StartSynker()
		{
			// start
			synker.Start(new StatusUpdate(this.SynkerUpdate),
				uri, host, port, 2);
		}

		public void StopSynker()
		{
			// stop
			synker.Stop();
		}

		public SyncTestCollection CreateCollection(string name)
		{
			SyncTestCollection collection =
				new SyncTestCollection(this, name, true);

			// NOTE: this should be revisited
			Invitation invitation = new Invitation();

			invitation.Identity = collection.BaseCollection.Owner;
			invitation.Domain = "novell";

			invitation.RootPath = Path.GetFullPath(this.path);
			invitation.MasterHost = "master"; // collection.Store.Host;
			invitation.MasterPort = collection.Store.Port.ToString();
			invitation.CollectionId = collection.BaseCollection.Id;

			AgentFactory agentFactory = new AgentFactory(Path.GetFullPath(this.path));
			agentFactory.GetInviteAgent().Accept(invitation);
			// NOTE: end

			return collection;
		}

		public SyncTestCollection OpenCollection(string name)
		{
			return new SyncTestCollection(this, name, false);
		}

		public void SubscribeToCollection(SyncTestCollection collection)
		{
			Invitation invitation = new Invitation();

			invitation.RootPath = Path.GetFullPath(this.path);
			
			invitation.Identity = collection.BaseCollection.Owner;
			invitation.Domain = "novell";
			
			invitation.MasterHost = collection.Store.Host;
			invitation.MasterPort = collection.Store.Port.ToString();
			invitation.CollectionId = collection.BaseCollection.Id;

			AgentFactory agentFactory = new AgentFactory(Path.GetFullPath(this.path));
			agentFactory.GetInviteAgent().Accept(invitation);
		}

		#region IDisposable Members

		/// <summary>
		/// Dispose of the sync test store
		/// </summary>
		public void Dispose()
		{
			// kludge for the storeprovider
			GC.Collect();

			// get rid of the store
			store.ImpersonateUser(Access.StoreAdminRole, null);
			store.Delete();
		}

		#endregion

		#region Properties

		public int SynkerActivations
		{
			get { return activations; }
		}

		internal string Host
		{
			get { return host; }
		}

		internal int Port
		{
			get { return port; }
		}

		internal Store BaseStore
		{
			get { return store; }
		}

		internal string StorePath
		{
			get { return path; }
		}

		#endregion
	}
}
