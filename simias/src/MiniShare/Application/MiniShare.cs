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

namespace Simias.Mini
{
	/// <summary>
	/// Mini Share
	/// </summary>
	class MiniShare
	{
		private static readonly string collectionId =
			"a10db843-6048-4343-bedd-d2d807dfa358";

		private static readonly string collectionName = "minishare";
		private static readonly string collectionType = "minishare";

		private static readonly string masterPath = Path.GetFullPath("./minimaster");
		private static readonly string slavePath = Path.GetFullPath("./minislave");

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			// tracing
			MyTrace.SendToConsole();
			MyTrace.Switch.Level = TraceLevel.Verbose;

			// title
			Console.WriteLine();
			Console.WriteLine("MiniShare");
			Console.WriteLine();

			// check arguments
			if (args.Length > 1)
			{
				Console.WriteLine("USAGE: MiniShare.exe [MasterHost]");
				return -1;
			}

			// exit message
			Console.WriteLine("Press [Enter] to exit...");
			Console.WriteLine();

			// properties
			SyncProperties properties = new SyncProperties();

			// sinks
			properties.DefaultChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor;

			// role
			SyncCollectionRoles role;
			
			if (args.Length == 1)
			{
				role = SyncCollectionRoles.Slave;
				properties.StorePath = slavePath;

				// master host
				properties.DefaultHost = args[0];

				// put the slave on a different port
				properties.DefaultPort = SyncProperties.SuggestedPort + 1;
			}
			else
			{
				role = SyncCollectionRoles.Master;
				properties.StorePath = masterPath;
			}
			
			MyTrace.WriteLine("Sync Role: {0}", role);

			// clean up directory
			if (Directory.Exists(properties.StorePath)) Directory.Delete(properties.StorePath, true);

			// host
			if (role == SyncCollectionRoles.Slave)
			{
			}
			
			MyTrace.WriteLine("Master Host: {0}:{1}", properties.DefaultHost, SyncProperties.SuggestedPort);

			// logic factory
			//properties.DefaultLogicFactory = typeof(SyncLogicFactoryLite);
			properties.DefaultLogicFactory = typeof(SynkerA);

			SyncManager manager = new SyncManager(properties);
			
			manager.ChangedState += new ChangedSyncStateEventHandler(OnChangedSyncState);
			
			manager.Start();

			MyTrace.WriteLine("Store Path: {0}", manager.StorePath);
			
			MyTrace.WriteLine("Creating Shared Collection...");

			SyncStore store = new SyncStore(manager.StorePath);
			
			SyncCollection collection = null;

			// set the role
			if (role == SyncCollectionRoles.Master)
			{
				// create the master collection
				// for testing purposes let the role, host, and port default
				collection = store.CreateCollection(collectionId,
					collectionName, collectionType,
					Path.Combine(properties.StorePath, collectionName));
			}
			else if (role == SyncCollectionRoles.Slave)
			{
				// create the slave collection
				collection = store.CreateCollection(collectionId,
					collectionName, collectionType,
					Path.Combine(properties.StorePath, collectionName),
					role, properties.DefaultHost, SyncProperties.SuggestedPort);
			}

			MyTrace.WriteLine("Committing Collection...");

			// save the new collection
			collection.Commit();

			MyTrace.WriteLine("Working...");

			// end
			Console.ReadLine();

			manager.Dispose();

			MyTrace.WriteLine("Done.");

			// kludge to kill all threads
			Environment.Exit(0);

			return 0;
		}

		private static void OnChangedSyncState(SyncManagerStates state)
		{
			MyTrace.WriteLine("Sync Manager State: {0}", state);
		}
	}
}
