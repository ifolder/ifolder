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
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

using Simias;
using Simias.Sync;
using Simias.Storage;
using Simias.Agent;

namespace Simias.Mini
{
	/// <summary>
	/// Simias Mini Share
	/// </summary>
	class MiniShare
	{
		private static readonly string collectionName = "Share";
		private static readonly string masterDirectory = "MiniMaster";
		private static readonly string slaveDirectory = "MiniSlave";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			// tracing
			MyTrace.SendToConsole();
			MyTrace.Switch.Level = TraceLevel.Verbose;

			// check arguments
			if (args.Length > 1)
			{
				Console.WriteLine("USAGE: MiniShare.exe [Master Invitation File]");
				return -1;
			}

			// properties
			SyncProperties properties = new SyncProperties();

			// logic factory
			//properties.DefaultLogicFactory = typeof(SyncLogicFactoryLite);
			properties.DefaultLogicFactory = typeof(SynkerA);

			// sinks
			properties.DefaultChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor;

			if (args.Length == 0)
			{
				// header
				Console.WriteLine();
				Console.WriteLine("MiniShare Master");
				Console.WriteLine();

				// master path
				string path = Path.GetFullPath(Path.Combine(".", masterDirectory));

				// clean up directory
				if (Directory.Exists(path)) Directory.Delete(path, true);

				// create store
				SyncStore store = new SyncStore(path);

				// create the master collection
				SyncCollection collection = store.CreateCollection(
					collectionName, Path.Combine(path, collectionName));
				collection.Host = properties.DefaultHost;
				collection.Port = 7464;
				collection.Commit();

				// create invitation
				Invitation invitation = 
					collection.CreateInvitation(store.BaseStore.CurrentUser);

				// save invitation
				invitation.Save(Path.Combine(path, "invitation.ifi"));

				// sync properties
				properties.StorePath = path;
				properties.DefaultPort = collection.Port;
			}
			else
            {
				// header
				Console.WriteLine();
				Console.WriteLine("MiniShare Slave");
				Console.WriteLine();

				// master path
				string path = Path.GetFullPath(Path.Combine(".", slaveDirectory));

				// clean up directory
				if (Directory.Exists(path)) Directory.Delete(path, true);

				// create store
				SyncStore store = new SyncStore(path);

				// import invitation
				Invitation invitation = new Invitation(args[0]);
				invitation.RootPath = path;

				// accept invitation
				SyncCollection collection = store.CreateCollection(invitation);
				collection.Commit();

				// sync properties
				properties.StorePath = path;
				properties.DefaultPort = 7465;
			}
			
			// start sync manager
			SyncManager manager = new SyncManager(properties);
			manager.ChangedState += new ChangedSyncStateEventHandler(OnChangedSyncState);
			manager.Start();

			MyTrace.WriteLine("Running...");

			// exit message
			Console.WriteLine("Press [Enter] to exit...");
			Console.WriteLine();

			// wait on input
			Console.ReadLine();

			// clean-up
			manager.Dispose();

			MyTrace.WriteLine("Done.");

			// kludge to kill all threads
			Environment.Exit(0);

			return 0;
		}

		private static void OnChangedSyncState(SyncManagerStates state)
		{
			MyTrace.WriteLine("Sync State Changed: {0}", state);
		}
	}
}
