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
 *  Author: Dale Olds <olds@novell.com>
 *
 ***********************************************************************/
using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Diagnostics;

using Simias.Storage;
using Simias;
using Simias.Service;
using Simias.Event;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to sync a portion of the file system with a collection
/// applying iFolder specific behavior
/// </summary>

/* TODO: need to handle if we are on a case-insensitive file system and file name
 * changes only by case? Actually this would be a rather rare optimization and
 * probably not worth it for the dredger (except perhaps for a directory rename).
 * If the event system is up, we catch it as a rename. If not, the dredger treats
 * it as a delete and create. Dredger should always be case sensitive.
 */

public class Dredger
{
	SyncCollection collection = null;

	/* TODO: onServer needs to be removed. It controls how tombstones are handled:
	 *   they are deleted on the server but left on the client. What it
	 *   really needs to be is deleted if there is no upstream server. Perhaps
	 *   the best way to handle it would be for this code to always leave a
	 *   tombstone, but the sync code would just remove them if there was no
	 *   upstream server.
	 */
	bool onServer = false;
	const string lastDredgeProp = "LastDredgeTime";
	DateTime dredgeTimeStamp = DateTime.Now;
	DateTime lastDredgeTime = DateTime.MinValue;

	//--------------------------------------------------------------------
	// only returns true if file exists and name matches case exactly
	bool FileThere(string path, string name)
	{
		FileInfo fi = new FileInfo(Path.Combine(path, name));
		return fi.Exists && name == fi.Name;
	}

	//--------------------------------------------------------------------
	// only returns true if directory exists and name matches case exactly
	bool DirThere(string path, string name)
	{
		DirectoryInfo di = new DirectoryInfo(Path.Combine(path, name));
		return di.Exists && name == di.Name;
	}

	//--------------------------------------------------------------------
	void DeleteNode(Node node)
	{
		Log.Spew("Dredger deleting orphaned node {0}, {1}", node.Name, node.ID);
		Node[] deleted = collection.Delete(node, PropertyTags.Parent);
		collection.Commit(deleted);

		/* TODO: right now we never leave tombstones on the server. Fix this
		 * such that we only leave tombstones when this collection has an
		 * upstream master.
		 */
		if (onServer)
			collection.Commit(collection.Delete(deleted));
	}

	//--------------------------------------------------------------------
	// TODO: what about file permissions and symlinks?

	void DoNode(DirNode parentNode, string path, string type)
	{
		Node node = null;
		string name = Path.GetFileName(path);

		//Log.Spew("Dredger processing node of path {0}", path);

		// don't let temp files from sync into the collection as regular nodes
		if (name.StartsWith(".simias.") && type == typeof(FileNode).Name)
			return;

		// find if node for this file or dir already exists
		// delete nodes that are wrong type
		foreach (ShallowNode sn in collection.GetNodesByName(name))
		{
			Node n = new Node(collection, sn);
			DirNode dn = collection.IsType(n, typeof(DirNode).Name)? new DirNode(n): null;
			FileNode fn = collection.IsType(n, typeof(FileNode).Name)? new FileNode(n): null;
			string npath = null;
			if (dn != null)
				npath = dn.GetFullPath(collection);
			else if (fn != null)
				npath = fn.GetFullPath(collection);
			if (npath != null && npath == path)
			{
				if (!collection.IsType(n, type) || node != null && dn == null)
					DeleteNode(n); // remove node if wrong type or duplicate file
				else
				{
					if (dn != null)
						DoSubtree(dn);
					if (node == null)
						node = n;
				}
			}
		}

		if (node == null)
		{
			// it's a new node
			if (type == typeof(FileNode).Name)
			{
				FileNode fnode = new FileNode(collection, parentNode, name);
				fnode.LastWriteTime = File.GetLastWriteTime(path);
				fnode.CreationTime = File.GetCreationTime(path);
				Log.Spew("Dredger adding file node for {0} {1}", path, fnode.ID);
				collection.Commit(fnode);
			}
			else
			{
				DirNode dnode = new DirNode(collection, parentNode, name);
				dnode.LastWriteTime = Directory.GetLastWriteTime(path);
				dnode.CreationTime = Directory.GetCreationTime(path);
				Log.Spew("Dredger adding dir node for {0} {1}", path, dnode.ID);
				collection.Commit(dnode);
				DoSubtree(dnode);
			}
		}
		else if (type == typeof(FileNode).Name)
		{
			// here we are just checking for modified files
			FileNode unode = new FileNode(node);
			DateTime lastWrote = File.GetLastWriteTime(path);
			DateTime created = File.GetCreationTime(path);
			if (unode.LastWriteTime != lastWrote)
			{
				unode.LastWriteTime = lastWrote;
				unode.CreationTime = created;
				Log.Spew("Dredger updating file node for {0} {1}", path, node.ID);
				collection.Commit(unode);
			}
		}
	}

	//--------------------------------------------------------------------
	void DoSubtree(DirNode dnode)
	{
		if (dnode == null)
		{
			//Log.Spew("Dredger skipping empty subtree");
			return;
		}

		
		string path = dnode.GetFullPath(collection);
		//Log.Spew("Dredger processing subtree of path {0}", path);

		DirectoryInfo tmpDi = new DirectoryInfo(path);
		bool lookForDeletes = false;
		
		if (tmpDi.LastWriteTime > lastDredgeTime)
		{
			lookForDeletes = true;
		}
		
		if (lookForDeletes)
		{
			// remove all nodes from store that no longer exist in the file system
			foreach (ShallowNode sn in collection.Search(PropertyTags.Parent, new Relationship(collection.ID, dnode.ID)))
			{
				Node kid = new Node(collection, sn);
				if (collection.IsType(kid, typeof(DirNode).Name) && !DirThere(path, kid.Name)
					|| collection.IsType(kid, typeof(FileNode).Name) && !FileThere(path, kid.Name))
					DeleteNode(kid);
				// else Log.Spew("Dredger leaving node {0}", kid.Name);
			}
		}

		// merge files from file system to store
		foreach (string file in Directory.GetFiles(path))
		{
			if (File.GetLastWriteTime(file) > lastDredgeTime || lookForDeletes)
				DoNode(dnode, file, typeof(FileNode).Name);
		}
		
		

		// merge subdirs and recurse.
		foreach (string dir in Directory.GetDirectories(path))
		{
			DoNode(dnode, dir, typeof(DirNode).Name);
		}
	}

	/// <summary>
	/// Dredge the Managed path.
	/// </summary>
	/// <param name="path"></param>
	void DoManagedPath(string path)
	{
		DirectoryInfo tmpDi = new DirectoryInfo(path);
		
		// merge files from file system to store
		foreach (string file in Directory.GetFiles(path))
		{
			if (File.GetLastWriteTime(file) > lastDredgeTime)
			{
				// here we are just checking for modified files
				BaseFileNode unode = (BaseFileNode)collection.GetNodeByID(Path.GetFileName(file));
				if (unode != null)
				{
					DateTime lastWrote = File.GetLastWriteTime(file);
					DateTime created = File.GetCreationTime(file);
					if (unode.LastWriteTime != lastWrote)
					{
						unode.LastWriteTime = lastWrote;
						unode.CreationTime = created;
						Log.Spew("Dredger updating store file node for {0} {1}", path, file);
						collection.Commit(unode);
					}
				}
			}
		}
			
		//dnode.LastWriteTime = tmpDi.LastWriteTime;
		//collection.Commit(dnode);
	}

	//--------------------------------------------------------------------
	/// <summary>
	/// Creates a dredger for this collection and dredges the system.
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="onServer"></param>
	public Dredger(Collection collection, bool onServer)
	{
		this.collection = new SyncCollection(collection);
		this.onServer = onServer;
		try
		{
			lastDredgeTime = (DateTime)(collection.Properties.GetSingleProperty(lastDredgeProp).Value);
		}
		catch
		{
		}
		DoSubtree(collection.GetRootDirectory());
		DoManagedPath(collection.ManagedPath);
		Property tsp = new Property(lastDredgeProp, dredgeTimeStamp);
		tsp.LocalProperty = true;
		collection.Properties.ModifyProperty(tsp);
		collection.Commit(collection);
	}
}

//===========================================================================

	/// <summary>
	/// Service to control the dredger.
	/// </summary>
	public class DredgeService : IThreadService
	{
		Store	store = null;
		Thread  thread = null;
		bool	shuttingDown;
		bool	paused;
		bool	needToDredge = true;
		EventSubscriber es = null;
		
		private void DoDredge()
		{
			while (!shuttingDown)
			{
				try
				{
					if (!paused  & needToDredge)
					{
						foreach (ShallowNode sn in store)
						{
							if (!shuttingDown)
							{
								Collection col = new Collection(store, sn);
								SyncCollection sCol = new SyncCollection(col);
								switch (sCol.Role)
								{
									case SyncCollectionRoles.Master:
										new Dredger(col, true);
										break;
									case SyncCollectionRoles.Local:
									case SyncCollectionRoles.Slave:
									default:
										new Dredger(col, false);
										break;
								}
							}
						}
						if (!MyEnvironment.Mono)
							needToDredge = false;
					}
				}
				catch
				{
				}
				Thread.Sleep(1000 * 60);
			}
		}

		#region IThreadService Members

		/// <summary>
		/// 
		/// </summary>
		/// <param name="conf"></param>
		public void Start(Simias.Configuration conf)
		{
			store = new Store(conf);
			paused = shuttingDown = false;
			if (!MyEnvironment.Mono)
			{
				// Start listening to file change events.
				es = new EventSubscriber(conf);
				es.FileChanged += new FileEventHandler(es_FileChanged);
				es.FileCreated += new FileEventHandler(es_FileCreated);
				es.FileDeleted += new FileEventHandler(es_FileDeleted);
				es.FileRenamed += new FileRenameEventHandler(es_FileRenamed);
			}
			thread = new Thread(new ThreadStart(DoDredge));
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.BelowNormal;
			thread.Start();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Resume()
		{
			paused = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Pause()
		{
			paused = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="data"></param>
		public void Custom(int message, string data)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public void Stop()
		{
			shuttingDown = true;
			if (thread != null)
				thread.Interrupt();
			if (es != null)
				es.Dispose();
			if (store != null)
				store.Dispose();
		}

		#endregion

		private bool isSyncFile(string name)
		{
			return name.StartsWith(".simias.");
		}

		/// <summary>
		/// Gets the node represented by the file.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="fullPath"></param>
		/// <param name="isFile"></param>
		/// <returns></returns>
		private Node GetNodeFromFileName(Collection collection, string fullPath, out bool isFile)
		{
			isFile = false;

			string name = Path.GetFileName(fullPath);
			Node node = null;
				// find if node for this file or dir already exists
				// delete nodes that are wrong type
			foreach (ShallowNode sn in collection.GetNodesByName(name))
			{
				Node n = new Node(collection, sn);
				string npath = null;
				DirNode dn;
				BaseFileNode fn;

				if (collection.IsType(n, typeof(DirNode).Name))
				{
					dn = new DirNode(n);
					npath = dn.GetFullPath(collection);
					n = dn;
				}
				else if (collection.IsType(n, typeof(FileNode).Name))
				{
					isFile = true;
					fn = new FileNode(n);
					npath = fn.GetFullPath(collection);
					n = fn;
				}
				else if (collection.IsType(n, typeof(StoreFileNode).Name))
				{
					isFile = true;
					fn = new StoreFileNode(n);
					npath = fn.GetFullPath(collection);
					n = fn;
				}

				if (npath != null && npath == fullPath)
				{
					node = n;
					break;
				}
			}
			return node;
		}

		private void es_FileChanged(FileEventArgs args)
		{
			if (!isSyncFile(args.Name))
			{
				string path = args.FullPath;
				bool isFile;
				Collection collection = store.GetCollectionByID(args.Collection);
				Node n = GetNodeFromFileName(collection, path, out isFile);
				if (n != null)
				{
					if (isFile)
					{
						ModifyFileNode(collection, (BaseFileNode)n, args);
					}
					else
					{
						ModifyDirNode(collection, (DirNode)n, args);
					}
				}
				else
				{
					needToDredge = true;
				}
			}
		}

		private void es_FileCreated(FileEventArgs args)
		{
			if (!isSyncFile(args.Name))
			{
				bool isFile;
				Collection collection = store.GetCollectionByID(args.Collection);
				Node n = GetNodeFromFileName(collection, args.FullPath, out isFile);

				if (n != null)
				{
					// Delete the old node.
					DeleteNode(collection, n);
				}
				FileInfo tmpFi = new FileInfo(args.FullPath);
				if ((tmpFi.Attributes & FileAttributes.Directory) == 0)
				{
					AddFileNode(collection, args);
				}
				else
				{
					AddDirNode(collection, args);
				}
			}
		}

		private void es_FileDeleted(FileEventArgs args)
		{
			if (!isSyncFile(args.Name))
			{
				bool isFile;
				Collection collection = store.GetCollectionByID(args.Collection);
				Node n = GetNodeFromFileName(collection, args.FullPath, out isFile);

				if (n != null)
				{
					// Delete the old node.
					DeleteNode(collection, n);
				}
				else
				{
					needToDredge = true;
				}
			}
		}

		private void es_FileRenamed(FileRenameEventArgs args)
		{
			if (!isSyncFile(args.Name) && !isSyncFile(args.OldName))
			{
				bool isFile;
				Collection collection = store.GetCollectionByID(args.Collection);
				Node n = GetNodeFromFileName(collection, args.OldPath, out isFile);

				if (n != null)
				{
					if (isFile)
					{
						RenameFileNode(collection, (FileNode)n, args);
					}
					else
					{
						RenameDirNode(collection, (DirNode)n, args);
					}
				}
				else
				{
					needToDredge = true;
				}
			}
		}

		void AddFileNode(Collection collection, FileEventArgs args)
		{
			// We have a new file create a node for it.
			string path = args.FullPath;
			bool isFile;
			Node n = GetNodeFromFileName(collection, Path.GetDirectoryName(path), out isFile);
			DirNode parentNode = (DirNode)n;
			if (parentNode != null)
			{
				FileNode fnode = new FileNode(collection, parentNode, args.Name);
				fnode.LastWriteTime = File.GetLastWriteTime(path);
				fnode.CreationTime = File.GetCreationTime(path);
				Log.Spew("Adding file node for {0} {1} from event.", path, fnode.ID);
				collection.Commit(fnode);
			}
			else
			{
				needToDredge = true;
			}
		}

		void AddDirNode(Collection collection, FileEventArgs args)
		{
			// We have a new file create a node for it.
			string path = args.FullPath;
			bool isFile;
			Node n = GetNodeFromFileName(collection, Path.GetDirectoryName(path), out isFile);
			DirNode parentNode = (DirNode)n;
			if (parentNode != null)
			{
				DirNode dnode = new DirNode(collection, parentNode, args.Name);
				dnode.LastWriteTime = Directory.GetLastWriteTime(path);
				dnode.CreationTime = Directory.GetCreationTime(path);
				Log.Spew("Adding dir node for {0} {1} from event.", path, dnode.ID);
				collection.Commit(dnode);
			}
			else
			{
				needToDredge = true;
			}
		}

		void DeleteNode(Collection collection, Node node)
		{
			Log.Spew("Deleting node {0}, {1} from event.", node.Name, node.ID);
			Node[] deleted = collection.Delete(node, PropertyTags.Parent);
			collection.Commit(deleted);

			SyncCollection sCol = new SyncCollection(collection);
			if (sCol.Role == SyncCollectionRoles.Master)
			{
				// if we are the master do not leave the tombstone.
				collection.Commit(deleted);
			}
		}

		void ModifyFileNode(Collection collection, BaseFileNode node, FileEventArgs args)
		{
			// here we are just checking for modified files
			DateTime lastWrote = File.GetLastWriteTime(args.FullPath);
			if (node.LastWriteTime != lastWrote)
			{
				node.LastWriteTime = lastWrote;
				Log.Spew("Updating file node for {0} {1} from event.", args.FullPath, node.ID);
				collection.Commit(node);
			}
		}

		void ModifyDirNode(Collection collection, DirNode node, FileEventArgs args)
		{
			// Don't do anything it could cause a collision.
		}

		void RenameFileNode(Collection collection, FileNode node, FileRenameEventArgs args)
		{
			// Make sure the node is still in the collection.
			if (!(Path.GetDirectoryName(args.OldPath).Equals(Path.GetDirectoryName(args.FullPath))))
			{
				// We have a new parent find the parent node.
				bool isFile;
				Node parent = GetNodeFromFileName(collection, Path.GetDirectoryName(args.FullPath), out isFile);
				if (parent != null && !isFile)
				{
					// We have a parent reset the parent node.
					node.Properties.ModifyNodeProperty(PropertyTags.Parent, new Relationship(collection.ID, parent.ID));
				}
				else
				{
					// The node is no longer in the collection.
					// Delete the node.
					DeleteNode(collection, node);
					return;
				}
			}
			// Set the new name.
			node.Name = args.Name;
			// Set the last access time.
			DateTime lastWrote = File.GetLastWriteTime(args.FullPath);
			if (node.LastWriteTime != lastWrote)
			{
				node.LastWriteTime = lastWrote;
				Log.Spew("Updating file node for {0} {1} from event.", args.FullPath, node.ID);
			}
			collection.Commit(node);
			return;
		}

		void RenameDirNode(Collection collection, DirNode node, FileRenameEventArgs args)
		{
			// Do not change a root node.
			if (!node.IsRoot)
			{
				// Make sure the node is still in the collection.
				if (!(Path.GetDirectoryName(args.OldPath).Equals(Path.GetDirectoryName(args.FullPath))))
				{
					// We have a new parent find the parent node.
					bool isFile;
					Node parent = GetNodeFromFileName(collection, Path.GetDirectoryName(args.FullPath), out isFile);
					if (parent != null && !isFile)
					{
						// We have a parent reset the parent node.
						node.Properties.ModifyNodeProperty(PropertyTags.Parent, new Relationship(collection.ID, parent.ID));
					}
					else
					{
						// The node is no longer in the collection.
						// Delete the node.
						DeleteNode(collection, node);
						return;
					}
				}
				// Set the new name.
				node.Name = args.Name;
				// Set the last access time.
				DateTime lastWrote = File.GetLastWriteTime(args.FullPath);
				if (node.LastWriteTime != lastWrote)
				{
					node.LastWriteTime = lastWrote;
					Log.Spew("Updating file node for {0} {1} from event.", args.FullPath, node.ID);
				}
				collection.Commit(node);
				return;
			}
			else
			{
				needToDredge = true;
				// TODO is this the right thing to do on a rootDir rename.
				// This is a rename of a root node do not sync it back.
				/*
				SyncCollection sCol = new SyncCollection(collection);
				sCol.Synchronizable = false;
				sCol.Commit(sCol);

				// Make sure the root path has not changed.
				if (!Path.GetDirectoryName(node.GetFullPath(collection)).Equals(Path.GetDirectoryName(args.OldPath)))
				{

				}
				*/
			}
		}
	}
}
