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
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

using Simias;
using Simias.Sync;
using Simias.Storage;

namespace Novell.iFolder.Mini
{
	/// <summary>
	/// Mini iFolder
	/// </summary>
	class MiniFolder
	{
		private static readonly string collectionId =
			"a10db843-6048-4343-bedd-d2d807dfa358";
		private static readonly string collectionName = "MiniCollection";
		private static readonly string collectionType = "MiniCollectionType";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			// tracing
			MyTrace.SendTraceToStandardOutput();
			MyTrace.Switch.Level = TraceLevel.Verbose;

			// title
			Console.WriteLine();
			Console.WriteLine("MiniFolder");
			Console.WriteLine();

			// check arguments
			if (args.Length < 1 || args.Length > 2)
			{
				Console.WriteLine("USAGE: MiniFolder.exe RootPath [MasterHost]");
				return -1;
			}

			// exit message
			Console.WriteLine("Press [Enter] to exit...");
			Console.WriteLine();

			// properties
			string rootPath = Path.GetFullPath(args[0]);
			SyncProperties properties = new SyncProperties();

			// role
			SyncCollectionRoles role;
			
			if (args.Length == 1)
			{
				role = SyncCollectionRoles.Master;
				properties.StorePath = Path.GetFullPath("./MiniMasterStore");
			}
			else
			{
				role = SyncCollectionRoles.Slave;
				properties.StorePath = Path.GetFullPath("./MiniSlaveStore");
			}
			
			MyTrace.WriteLine("Role: {0}", role);

			// host
			if (role == SyncCollectionRoles.Slave)
			{
				properties.DefaultHost = args[1];

				// put the slave on a different port
				properties.DefaultPort = SyncProperties._DefaultPort + 1;
			}
			
			MyTrace.WriteLine("Local Host: {0}:{1}", properties.DefaultHost, properties.DefaultPort);

			// logic factory
			properties.DefaultLogicFactory = typeof(SyncLogicFactoryLite);
			//properties.DefaultLogicFactory = typeof(SynkerA);

			SyncManager manager = new SyncManager(properties);
			
			manager.ChangedState += new ChangedSyncStateEventHandler(OnChangedSyncState);
			
			manager.Start();

			MyTrace.WriteLine("Path: {0}", manager.StorePath);
			
			// create collection, if needed
			SyncCollection collection = null;	

			collection = manager.StoreManager.Store.OpenCollection(collectionId);

			if (collection == null)
			{
				MyTrace.WriteLine("Creating Collection...");

				SyncStore store = new SyncStore(manager.StorePath);
				
				// set the role
				if (role == SyncCollectionRoles.Master)
				{
					// create the master collection
					// for testing purposes let the role, host, and port default
					collection = store.CreateCollection(collectionId,
						collectionName, collectionType,
						Path.Combine(rootPath, collectionName));

					// save the new collection
					collection.Commit();
				}
				else if (role == SyncCollectionRoles.Slave)
				{
					// create the slave collection
					collection = store.CreateCollection(collectionId,
						collectionName, collectionType,
						Path.Combine(rootPath, collectionName),
						role, properties.DefaultHost, SyncProperties._DefaultPort);

					// save the new collection
					collection.Commit();
				}
			}
			else if (role == SyncCollectionRoles.Slave)
			{
				// reset the host
				collection.Host = properties.DefaultHost;
				collection.Commit();
			}

			// end
			Console.ReadLine();

			manager.Stop();
			
			manager.Dispose();

			MyTrace.WriteLine("Done.");

			// done
			return 0;
		}

		private static void OnChangedSyncState(SyncManagerStates state)
		{
			MyTrace.WriteLine("Sync Manager Stage: {0}", state);
		}
	}
}
