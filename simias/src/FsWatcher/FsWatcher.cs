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

namespace Simias
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class FsWatcher : UserService
	{
		Store				store;
		Hashtable			watcherTable;
		EventPublisher		publish;
		EventSubscriber		collectionWatcher;
		Thread				t1;
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			FsWatcher fsw = new FsWatcher();
			fsw.Run();
		}

		public FsWatcher()
		{
			// Create a hash table to store the watchers in.
			watcherTable = new Hashtable();
			store = Store.Connect();
			publish = new EventPublisher();
			collectionWatcher = new EventSubscriber();
			collectionWatcher.Created += new EventHandler(OnNewCollection);
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
			Uri pathUri = (Uri)col.Properties.GetSingleProperty(Property.DocumentRoot).Value;
			string rootPath = pathUri.LocalPath;
			FileSystemWatcher w = new FileSystemWatcher(rootPath);
			watcherTable.Add(rootPath, w);
			w.Changed += new FileSystemEventHandler(OnChanged);
			w.Created += new FileSystemEventHandler(OnCreated);
			w.Deleted += new FileSystemEventHandler(OnDeleted);
			w.Renamed += new RenamedEventHandler(OnRenamed);
			//w.NotifyFilter = NotifyFilters.LastWrite;
			w.IncludeSubdirectories = true;
			w.EnableRaisingEvents = true;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			System.Diagnostics.Debug.WriteLine("Changed File: " +  e.FullPath + " " + e.ChangeType);
			publish.FireChanged(new EventArgs("file", e.Name, Path.GetDirectoryName(e.FullPath), Path.GetExtension(e.Name)));
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.FireRenamed(new EventArgs("file", e.Name, Path.GetDirectoryName(e.FullPath), e.OldName, Path.GetDirectoryName(e.OldFullPath), Path.GetExtension(e.OldName)));
			System.Diagnostics.Debug.WriteLine(string.Format("Renamed File: {0} renamed to {1}", e.OldFullPath, e.FullPath));
		}

		private void OnDeleted(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			publish.FireDeleted(new EventArgs("file", e.Name, Path.GetDirectoryName(e.FullPath), Path.GetExtension(e.Name)));
			System.Diagnostics.Debug.WriteLine("Deleted File: " +  e.FullPath + " " + e.ChangeType);
		}

		private void OnCreated(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.FireCreated(new EventArgs("file", e.Name, Path.GetDirectoryName(e.FullPath), Path.GetExtension(e.Name)));
			System.Diagnostics.Debug.WriteLine("Created File: {0} Created.", e.FullPath);
		}

		private void OnNewCollection(EventArgs args)
		{
			try
			{
				Collection col = store.GetCollectionById(args.Path);
				if (col != null)
				{
					WatchCollection(col);
				}
			}
			catch
			{
			}
		}
	}
}
