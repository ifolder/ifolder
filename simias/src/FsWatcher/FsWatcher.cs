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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Threading;
using System.Collections;
using Simias.Storage;
using Simias.Event;

namespace Simias.Event
{
	internal class CollectionFilesWatcher
	{
		string						collectionId;
		FileSystemWatcher			watcher;
		static EventPublisher		publish;
		

		internal CollectionFilesWatcher(Collection col)
		{
			this.collectionId = col.ID;
			publish = new EventPublisher(col.StoreReference.Config);
			string rootPath = col.GetRootDirectory().GetFullPath(col);
			watcher = new FileSystemWatcher(rootPath);
			watcher.Changed += new FileSystemEventHandler(OnChanged);
			watcher.Created += new FileSystemEventHandler(OnCreated);
			watcher.Deleted += new FileSystemEventHandler(OnDeleted);
			watcher.Renamed += new RenamedEventHandler(OnRenamed);
			watcher.IncludeSubdirectories = true;
			watcher.EnableRaisingEvents = true;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			System.Diagnostics.Debug.WriteLine("Changed File: " +  e.FullPath + " " + e.ChangeType);
			publish.RaiseEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, EventType.FileChanged));
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.RaiseEvent(new FileRenameEventArgs(source.ToString(), e.FullPath, collectionId, e.OldFullPath));
			System.Diagnostics.Debug.WriteLine(string.Format("Renamed File: {0} renamed to {1}", e.OldFullPath, e.FullPath));
		}

		private void OnDeleted(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			publish.RaiseEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, EventType.FileDeleted));
			System.Diagnostics.Debug.WriteLine("Deleted File: " +  e.FullPath + " " + e.ChangeType);
		}

		private void OnCreated(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.RaiseEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, EventType.FileCreated));
			System.Diagnostics.Debug.WriteLine("Created File: {0} Created.", e.FullPath);
		}
	}

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class FsWatcher : UserService
	{
		static Store				store;
		static Hashtable			watcherTable;
		EventSubscriber				collectionWatcher;
		static Configuration		conf;
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			FsWatcher fsw = new FsWatcher(args);
			fsw.Run(conf);
		}

		public FsWatcher(string[] args)
		{
			// Create a hash table to store the watchers in.
			watcherTable = new Hashtable();
			if (args.Length == 1)
			{
				conf = new Configuration(args[0]);
			}
			else
			{
				conf = new Configuration();
			}
			store = new Store(conf);
			collectionWatcher = new EventSubscriber(conf);
			collectionWatcher.NodeCreated += new NodeEventHandler(OnNewCollection);
			collectionWatcher.NodeDeleted += new NodeEventHandler(OnDeleteNode);
			foreach (Collection col in store)
			{
				WatchCollection(col);
			}
		}

		protected override void OnShutdown()
		{
			collectionWatcher.Dispose();
		}

		private void WatchCollection(Collection col)
		{
			watcherTable.Add(col.ID, new CollectionFilesWatcher(col));
		}


		private void OnNewCollection(NodeEventArgs args)
		{
			try
			{
				if (args.ID == args.Collection)
				{
					Collection col = store.GetCollectionByID(args.ID);
					if (col != null)
					{
						WatchCollection(col);
					}
				}
			}
			catch
			{
			}
		}

		private void OnDeleteNode(NodeEventArgs args)
		{
			if (args.ID == args.Collection)
			{
				watcherTable.Remove(args.Collection);
			}
		}
	}
}
