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

			// path
			string path;
			if (args.Length == 0)
			{
				// master path
				path = Path.GetFullPath(Path.Combine(".", masterDirectory));
			}
			else
			{
				// slave path
				path = Path.GetFullPath(Path.Combine(".", slaveDirectory));
			}

			// properties
			Configuration config = new Configuration(path);
			SyncProperties properties = new SyncProperties(config);

			// logic factory
			properties.DefaultLogicFactory = typeof(SyncLogicFactoryLite);
			//properties.DefaultLogicFactory = typeof(SynkerA);

			// sinks
			properties.DefaultChannelSinks = SyncChannelSinks.Binary | SyncChannelSinks.Monitor;

			if (args.Length == 0)
			{
				// header
				Console.WriteLine();
				Console.WriteLine("MiniShare Master");
				Console.WriteLine();

				// clean up directory
				if (Directory.Exists(path)) Directory.Delete(path, true);

				// create store
				Store store = new Store(config);

				// create the master collection
				SyncCollection collection = new SyncCollection(new Collection(store, collectionName));
				UriBuilder builder = new UriBuilder("http", properties.DefaultHost, 7464);
				collection.MasterUri = builder.Uri;
				collection.Commit();

				// create invitation
				Invitation invitation = 
					collection.CreateInvitation(store.CurrentUserGuid);

				// save invitation
				invitation.Save(Path.Combine(path, "invitation.ifi"));

				// sync properties
				properties.DefaultPort = collection.MasterUri.Port;
			}
			else
            {
				// header
				Console.WriteLine();
				Console.WriteLine("MiniShare Slave");
				Console.WriteLine();

				// clean up directory
				if (Directory.Exists(path)) Directory.Delete(path, true);

				// create store
				Store store = new Store(config);

				// import invitation
				Invitation invitation = new Invitation(args[0]);
				invitation.RootPath = path;

				// accept invitation
				SyncCollection collection = new SyncCollection(store, invitation);
				collection.Commit();

				// sync properties
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
