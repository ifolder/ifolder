/***********************************************************************
 *  FsWatcher.cs - An event publisher for file system events in the store.
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
 *  Author: Russ Young <ryoung@novell.com>
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
		string						domainName;
		FileSystemWatcher			watcher;
		static EventPublisher		publish = new EventPublisher();
		

		internal CollectionFilesWatcher(Collection col)
		{
			this.collectionId = col.Id;
			domainName = col.LocalStore.DomainName;
			Uri pathUri = (Uri)col.Properties.GetSingleProperty(Property.DocumentRoot).Value;
			string rootPath = pathUri.LocalPath;
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
			publish.RaiseFileEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, domainName, FileEventArgs.EventType.Changed));
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.RaiseFileEvent(new FileRenameEventArgs(source.ToString(), e.FullPath, collectionId, domainName, e.OldFullPath));
			System.Diagnostics.Debug.WriteLine(string.Format("Renamed File: {0} renamed to {1}", e.OldFullPath, e.FullPath));
		}

		private void OnDeleted(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			publish.RaiseFileEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, domainName, FileEventArgs.EventType.Deleted));
			System.Diagnostics.Debug.WriteLine("Deleted File: " +  e.FullPath + " " + e.ChangeType);
		}

		private void OnCreated(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.RaiseFileEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, domainName, FileEventArgs.EventType.Created));
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
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			FsWatcher fsw = new FsWatcher(args);
			fsw.Run();
		}

		public FsWatcher(string[] args)
		{
			// Create a hash table to store the watchers in.
			watcherTable = new Hashtable();
			if (args.Length == 1)
			{
				store = Store.Connect(new Uri(args[0]), this.GetType().FullName);
			}
			else
			{
				store = Store.Connect(this.GetType().FullName);
			}
			collectionWatcher = new EventSubscriber(store.DomainName);
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
			watcherTable.Add(col.Id, new CollectionFilesWatcher(col));
		}


		private void OnNewCollection(NodeEventArgs args)
		{
			try
			{
				if (args.ID == args.Collection)
				{
					Collection col = store.GetCollectionById(args.ID);
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
