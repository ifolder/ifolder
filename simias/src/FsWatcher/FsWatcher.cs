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
using Simias.Service;

namespace Simias.Event
{
	internal class CollectionFilesWatcher : IDisposable
	{
		ISimiasLog					logger = SimiasLogManager.GetLogger(typeof(FsWatcher));
		bool						disposed;
		string						collectionId;
		internal FileSystemWatcher	watcher;
		EventPublisher				publish;
		

		internal CollectionFilesWatcher(Collection col)
		{
            this.collectionId = col.ID;
			publish = new EventPublisher(col.StoreReference.Config);
			DirNode rootDir = col.GetRootDirectory();
			if (rootDir != null)
			{
				string rootPath = col.GetRootDirectory().GetFullPath(col);
				watcher = new FileSystemWatcher(rootPath);
				logger.Debug("New File Watcher at {0}", rootPath);
				watcher.Changed += new FileSystemEventHandler(OnChanged);
				watcher.Created += new FileSystemEventHandler(OnCreated);
				watcher.Deleted += new FileSystemEventHandler(OnDeleted);
				watcher.Renamed += new RenamedEventHandler(OnRenamed);
				watcher.IncludeSubdirectories = true;
				watcher.EnableRaisingEvents = true;
			}
			disposed = false;
		}

		~CollectionFilesWatcher()
		{
			Dispose(true);
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			publish.RaiseEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, EventType.FileChanged));
			logger.Debug("Changed File: {0} {1}", e.FullPath, e.ChangeType);
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.RaiseEvent(new FileRenameEventArgs(source.ToString(), e.FullPath, collectionId, e.OldFullPath));
			logger.Debug("Renamed File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
		}

		private void OnDeleted(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is changed, created, or deleted.
			publish.RaiseEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, EventType.FileDeleted));
			logger.Debug("Deleted File: {0} {1}",  e.FullPath, e.ChangeType);
		}

		private void OnCreated(object source, FileSystemEventArgs e)
		{
			// Specify what is done when a file is renamed.
			publish.RaiseEvent(new FileEventArgs(source.ToString(), e.FullPath, collectionId, EventType.FileCreated));
			logger.Debug("Created File: {0}", e.FullPath);
		}

		private void Dispose(bool inFinalize)
		{
			lock (this)
			{
				if (!disposed)
				{
					if (!inFinalize)
					{
						System.GC.SuppressFinalize(this);
					}
					if (watcher != null)
					{
						watcher.Dispose();
					}
					disposed = true;
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(false);
		}

		#endregion
	}

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class FsWatcher : IThreadService
	{
		static Store				store;
		static Hashtable			watcherTable;
		EventSubscriber				collectionWatcher;
		Configuration				conf;
		
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

		#region IThreadService Members

		public void Start(Configuration conf)
		{
			lock (this)
			{
				// Create a hash table to store the watchers in.
				this.conf = conf;
				watcherTable = new Hashtable();
				store = new Store(conf);
				collectionWatcher = new EventSubscriber(conf);
				collectionWatcher.NodeCreated += new NodeEventHandler(OnNewCollection);
				collectionWatcher.NodeDeleted += new NodeEventHandler(OnDeleteNode);
				foreach (ShallowNode sn in store)
				{
					Collection col = new Collection(store, sn);
					WatchCollection(col);
				}
			}
		}

		public void Stop()
		{
			lock (this)
			{
				try
				{
					foreach (CollectionFilesWatcher cw in watcherTable)
					{
						cw.Dispose();
					}
				}
				catch {}
				watcherTable.Clear();
				watcherTable = null;
				collectionWatcher.Dispose();
				collectionWatcher = null;
				store.Dispose();
				store = null;
			}
		}

		public void Resume()
		{
			Start(conf);
		}

		public void Pause()
		{
			Stop();
		}

		public void Custom(int message, string data)
		{
		}

		#endregion
	}
}
