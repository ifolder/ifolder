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
using System.Collections;
using System.Threading;
using System.Diagnostics;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	#region Delegates

	/// <summary>
	/// A directory has been added event handler.
	/// </summary>
	public delegate void AddedDirectoryEventHandler(string id);
	
	/// <summary>
	/// A directory has been deleted event handler.
	/// </summary>
	public delegate void DeletedDirectoryEventHandler(string id);
	
	/// <summary>
	/// A file has been added event handler.
	/// </summary>
	public delegate void AddedFileEventHandler(string id);
	
	/// <summary>
	/// A file has been changed event handler.
	/// </summary>
	public delegate void ChangedFileEventHandler(string id);
	
	/// <summary>
	/// A file has been deleted event handler.
	/// </summary>
	public delegate void DeletedFileEventHandler(string id);

	#endregion

	/// <summary>
	/// Collection Watcher
	/// </summary>
	/// <remarks>
	/// This class only supports a one stream per node.
	/// </remarks>
	public class CollectionWatcher : IDisposable
	{
		#region Events

		/// <summary>
		/// Occurs when a directory is added.
		/// </summary>
		public event AddedDirectoryEventHandler AddedDirectory;

		/// <summary>
		/// Occurs when a directory is deleted.
		/// </summary>
		public event DeletedDirectoryEventHandler DeletedDirectory;

		/// <summary>
		/// Occurs when a file is added.
		/// </summary>
		public event AddedFileEventHandler AddedFile;

		/// <summary>
		/// Occurs when a file is changed.
		/// </summary>
		public event ChangedFileEventHandler ChangedFile;

		/// <summary>
		/// Occurs when a file is deleted.
		/// </summary>
		public event DeletedFileEventHandler DeletedFile;

		#endregion

		#region Fields

		private Thread worker;
		private bool working;
		
		private string path;
		private Store store;
		private string collectionId;

		private int interval = 5;

		#endregion

		#region Contructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="path">The store path.</param>
		/// <param name="id">The collection id.</param>
		public CollectionWatcher(string path, string id)
		{
			// a full path is required
			if (path != null)
			{
				this.path = Path.GetFullPath(path);
			}
			
			// save collection id
			this.collectionId = id;

			// open store
			Uri uri = null;
			if (path != null) uri = new Uri(path);
			store = Store.Connect(uri);
			Trace.Assert(store != null);

			// create the thread
			worker = new Thread(new ThreadStart(this.DoWork));
		}

		#endregion

		#region Thread Members

		/// <summary>
		/// Start watching the Store.
		/// </summary>
		public void Start()
		{
			lock(this)
			{
				// start the thread
				working = true;
				worker.Start();
			}
		}

		/// <summary>
		/// Stop watching the Store.
		/// </summary>
		public void Stop()
		{
			lock(this)
			{
				// stop the thread
				working = false;
			
				try
				{
					worker.Join();
				}
				catch
				{
					// ignore
				}
			}
		}

		/// <summary>
		/// Look for Collection changes.
		/// </summary>
		private void DoWork()
		{
			while(working)
			{
				try
				{
					ScanFileSystem();
				}
				catch(Exception e)
				{
					MyTrace.WriteLine(e);
				}

				// sleep
				Thread.Sleep(TimeSpan.FromSeconds(interval));
			}
		}

		#endregion

		#region File System Members

		private void ScanFileSystem()
		{
			Hashtable nodes = new Hashtable();

			// open collection here to get a fresh copy
			Collection collection = store.GetCollectionById(collectionId);
			Trace.Assert(collection != null);

			// search through all the nodes
			foreach(Node node in collection)
			{
				// only keep track of file and directory nodes
				if (!node.IsTombstone && 
					(node.Type.Equals(SyncNode.FileNodeType) ||
						node.Type.Equals(SyncNode.DirectoryNodeType)))
				{
					// save the node by the node relative path
					nodes.Add(node.PathName, node);
				}
			}
	
			// create a node root path that is usable with the file system
			string docRootPath = collection.DocumentRoot.LocalPath;

			MyTrace.WriteLine("Scanning the File System: {0}", docRootPath);

			// recurse through the file system check for node addition and modifications
			string nodeRoot = Path.GetDirectoryName(docRootPath).TrimEnd(new char[] { Path.DirectorySeparatorChar });
			ParseFileSystem(collection, docRootPath, nodes, nodeRoot, collection);

			// look through the remaining nodes for deletions
			foreach(Node node in nodes.Values)
			{
				// get the node id, path, and type
				// note: we need a copy of these values before we delete the node
				string nodeId = node.Id;
				string nodePath = node.PathName;
				string nodeType = node.Type;
				ulong masterIncarnation = node.MasterIncarnation;
				ulong localIncarnation = node.LocalIncarnation;

				// delete file or directory node
				SyncNode sn = new SyncNode(node);
				sn.Delete();

				if (nodeType.Equals(SyncNode.FileNodeType))
				{
					// fire the deleted file event
					MyTrace.WriteLine("Deleted File: {0} ({1}.{2})", nodePath, masterIncarnation, localIncarnation);
					if (DeletedFile != null) DeletedFile(node.Id);
				}
				else
				{
					// fire the deleted directory event
					MyTrace.WriteLine("Deleted Directory: {0}", nodePath);
					if (DeletedDirectory != null) DeletedDirectory(node.Id);
				}
			}
		}

		#endregion

		#region Parse Members

		private void ParseFileSystem(Node parentNode, string parentPath,
			Hashtable nodes, string nodeRoot, Collection collection)
		{
			// search the directories in the parent path
			foreach(string dirPath in Directory.GetDirectories(parentPath))
			{
				// calculate the node relative path
				string nodePath = dirPath.Substring(nodeRoot.Length).Replace("\\", "/");

				// does the directory have an associated node
				Node dirNode = null;

				if (nodes.Contains(nodePath))
				{
					// get the node handle
					dirNode = (Node)nodes[nodePath];
					
					// remove the node from the list
					nodes.Remove(nodePath);
				}
				else
				{
					// get the directory name
					string name = Path.GetFileName(dirPath);

					// create an associated node
					dirNode = parentNode.CreateChild(name, SyncNode.DirectoryNodeType);
					
					// save
					dirNode.Commit();

					// fire the added file event
					MyTrace.WriteLine("Added Directory: {0}", nodePath);
					if (AddedDirectory != null) AddedDirectory(dirNode.Id);
				}
	
				// recurse
				ParseFileSystem(dirNode, dirPath, nodes, nodeRoot, collection);
			}
			
			// search the files in the parent path
			foreach(string filePath in Directory.GetFiles(parentPath))
			{
				// calculate the node relative path
				string nodePath = filePath.Substring(nodeRoot.Length).Replace("\\", "/");

				// save off the last write time
				DateTime fileTime = File.GetLastWriteTime(filePath);

				// does the file have an associated node
				Node node = null;

				if (nodes.Contains(nodePath))
				{
					// get the node handle
					node = (Node)nodes[nodePath];
					
					// remove the node from the list
					nodes.Remove(nodePath);

					// scan for changes
					bool changed = false;

					foreach(NodeStream ns in node.GetStreamList())
					{
						SyncNodeStream sns = new SyncNodeStream(ns);
						
						if (fileTime > sns.LocalLastWrite)
						{
							sns.LastWrite = sns.LocalLastWrite = fileTime;
							
							changed = true;
						}
					}

					if (changed)
					{
						// save and increment version
						node.Commit();

						// fire the change file event
						MyTrace.WriteLine("Changed File: {0} ({1}.{2})", nodePath, node.MasterIncarnation, node.LocalIncarnation);
						if (ChangedFile != null) ChangedFile(node.Id);
					}
				}
				else
				{
					// get the file name
					string name = Path.GetFileName(filePath);

					// create an associated node
					node = parentNode.CreateChild(name, SyncNode.FileNodeType);

					// add the stream
					string relPath = filePath.Substring(collection.DocumentRoot.LocalPath.Length);
					SyncNodeStream sns = new SyncNodeStream(node.AddStream(name, relPath));
					sns.LastWrite = sns.LocalLastWrite = fileTime;

					// save
					node.Commit();

					// fire the added file event
					MyTrace.WriteLine("Added File: {0} ({1}.{2})", nodePath, node.MasterIncarnation, node.LocalIncarnation);
					if (AddedFile != null) AddedFile(node.Id);
				}
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Dispose this object.
		/// </summary>
		public void Dispose()
		{
			lock(this)
			{
				// validate
				Stop();

				if (store != null)
				{
					try
					{
						// cleanup
						store.Dispose();
						store = null;
					}
					catch
					{
						// ignore
					}
				}
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// The interval, in seconds, between scans of the file system.
		/// </summary>
		public int Interval
		{
			get { return interval; }
			set { interval = value; }
		}

		#endregion
	}
}
