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

namespace Simias.Sync
{
	internal class CollectionFilesWatcher : IDisposable
	{
		ISimiasLog					logger = SimiasLogManager.GetLogger(typeof(FsWatcher));
		bool						disposed;
		string						collectionId;
		internal FileSystemWatcher	watcher;
		EventPublisher				publish;
		//Dredger						dredger;
		Hashtable					changes = new Hashtable();

		internal class fileChangeEntry
		{
			internal FileSystemEventArgs	eArgs;
			internal DateTime				time;

			internal fileChangeEntry(FileSystemEventArgs e)
			{
				eArgs = e;
				time = DateTime.Now;
			}
		
			internal void update(FileSystemEventArgs e)
			{
				eArgs = e;
				time = DateTime.Now;
			}

			internal void update()
			{
				time = DateTime.Now;
			}
		}
		

			internal CollectionFilesWatcher(SyncCollection col)
			{
				this.collectionId = col.ID;
			
				if (!MyEnvironment.Mono)
				{
					// We are on .Net use events to watch for changes.
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
				}
				disposed = false;
			}

			~CollectionFilesWatcher()
			{
				Dispose(true);
			}

			private string GetName(string fullPath)
			{
				if (MyEnvironment.Windows)
				{
					try
					{
						string[] caseSensitivePath = Directory.GetFiles(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath));
						if (caseSensitivePath.Length == 1)
						{
							// We should only have one match.
							fullPath = caseSensitivePath[0];
						}
					}
					catch {}
				}
				return fullPath;
			}

			private void OnChanged(object source, FileSystemEventArgs e)
			{
				string fullPath = GetName(e.FullPath);
			
				lock (changes)
				{
					fileChangeEntry entry = (fileChangeEntry)changes[fullPath];
					if (entry != null)
					{
						// This file has already been modified.
						// Combine the state.
						switch (entry.eArgs.ChangeType)
						{
							case WatcherChangeTypes.Created:
							case WatcherChangeTypes.Deleted:
							case WatcherChangeTypes.Changed:
								entry.update(e);
								break;
							case WatcherChangeTypes.Renamed:
								entry.update();
								break;
						}
					}
					else
					{
						changes[fullPath] = new fileChangeEntry(e);
					}
				}
			}

			private void OnRenamed(object source, RenamedEventArgs e)
			{
				string fullPath = GetName(e.FullPath);
				
				lock (changes)
				{
					// Any changes made to the old file need to be removed.
					changes.Remove(e.OldFullPath);
					changes[fullPath] = new fileChangeEntry(e);
				}
			}

			private void OnDeleted(object source, FileSystemEventArgs e)
			{
				string fullPath = GetName(e.FullPath);
			
				lock (changes)
				{
					changes[fullPath] = new fileChangeEntry(e);
				}
			}

			private void OnCreated(object source, FileSystemEventArgs e)
			{
				string fullPath = GetName(e.FullPath);
			
				lock (changes)
				{
					changes[fullPath] = new fileChangeEntry(e);
				}
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
		
		private void WatchCollection(SyncCollection col)
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
						WatchCollection(new SyncCollection(col));
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

		/// <summary>
		/// Called when the service should start.
		/// </summary>
		/// <param name="conf">The configuration object to use.</param>
		public void Start(Configuration conf)
		{
			lock (this)
			{
				// Create a hash table to store the watchers in.
				this.conf = conf;
				watcherTable = new Hashtable();
				store = Store.GetStore();
				collectionWatcher = new EventSubscriber();
				collectionWatcher.NodeCreated += new NodeEventHandler(OnNewCollection);
				collectionWatcher.NodeDeleted += new NodeEventHandler(OnDeleteNode);
				foreach (ShallowNode sn in store)
				{
					Collection col = new Collection(store, sn);
					WatchCollection(new SyncCollection(col));
				}
			}
		}

		/// <summary>
		/// Called when the service should stop.
		/// </summary>
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
				store = null;
			}
		}

		/// <summary>
		/// Called to Resume the service.
		/// </summary>
		public void Resume()
		{
			Start(conf);
		}

		/// <summary>
		/// Called to pause the service.
		/// </summary>
		public void Pause()
		{
			Stop();
		}

		/// <summary>
		/// Called to send a custom message to the service.
		/// </summary>
		/// <param name="message">The custom message.</param>
		/// <param name="data">Data for the message.</param>
		public void Custom(int message, string data)
		{
		}

		#endregion
	}
}
