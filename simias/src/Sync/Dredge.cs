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
	Collection collection = null;

	/* TODO: onServer needs to be removed. It controls how tombstones are handled:
	 *   they are deleted on the server but left on the client. What it
	 *   really needs to be is deleted if there is no upstream server. Perhaps
	 *   the best way to handle it would be for this code to always leave a
	 *   tombstone, but the sync code would just remove them if there was no
	 *   upstream server.
	 */
	bool onServer = false;

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
	DirNode DoNode(DirNode parentNode, string path, string type)
	{
		Node node = null;
		string name = Path.GetFileName(path);

		// don't let temp files from sync into the collection as regular nodes
		if (name.StartsWith(IncomingNode.TempFilePrefix) && type == typeof(FileNode).Name)
			return null;

		// delete nodes that are wrong type or dups
		// TODO: perhaps we should move dups to trash or log as error
		// TODO: what about file permissions and symlinks?
		// TODO: if it is a duplicate DirNode, its children should be moved to the winning node

		foreach (ShallowNode sn in collection.GetNodesByName(name))
		{
			Node n = new Node(collection, sn);
			Property p = n.Properties.GetSingleProperty(PropertyTags.Parent);
			Relationship parent = p == null? null: p.Value as Relationship;

			if (p != null && parent.NodeID == parentNode.ID && n.Name == name
					&& (collection.IsType(n, typeof(DirNode).Name)
							|| collection.IsType(n, typeof(FileNode).Name)))
			{
				if (!collection.IsType(n, type) || node != null)
					DeleteNode(n);
				else
					node = n;
			}
		}

		bool newNode = node == null;
		if (newNode)
		{
			if (type == typeof(FileNode).Name)
			{
				FileNode fnode = new FileNode(collection, parentNode, name);
				fnode.LastWriteTime = File.GetLastWriteTime(path);
				fnode.CreationTime = File.GetCreationTime(path);
				Log.Spew("Dredger adding file node for {0} {1}", path, fnode.ID);
				collection.Commit(fnode);
				return null;
			}
			DirNode dnode = new DirNode(collection, parentNode, name);
			dnode.LastWriteTime = Directory.GetLastWriteTime(path);
			dnode.CreationTime = Directory.GetCreationTime(path);
			Log.Spew("Dredger adding dir node for {0} {1}", path, dnode.ID);
			collection.Commit(dnode);
			return dnode;
		}
		if (type != typeof(FileNode).Name)
			return new DirNode(node);

		// from here we are just checking for modified files
		FileNode unode = new FileNode(node);
		DateTime lastWrote = File.GetLastWriteTime(path);
		DateTime created = File.GetCreationTime(path);
		if (unode.LastWriteTime != lastWrote || unode.CreationTime != created)
		{
			unode.LastWriteTime = lastWrote;
			unode.CreationTime = created;
			Log.Spew("Dredger updating file node for {0} {1}", path, node.ID);
			collection.Commit(unode);
		}
		return null;
	}

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
	void DoSubtree(DirNode dnode)
	{
		string path = dnode.GetFullPath(collection);

		// remove all nodes from store that no longer exist in the file system
		foreach (ShallowNode sn in collection.Search(PropertyTags.Parent, new Relationship(collection.ID, dnode.ID)))
		{
			Node kid = new Node(collection, sn);
			if (collection.IsType(kid, typeof(DirNode).Name) && !DirThere(path, kid.Name)
					|| collection.IsType(kid, typeof(FileNode).Name) && !FileThere(path, kid.Name))
				DeleteNode(kid);
		}

		// merge files from file system to store
		foreach (string file in Directory.GetFiles(path))
			DoNode(dnode, file, typeof(FileNode).Name);

		// merge subdirs and recurse.
		foreach (string dir in Directory.GetDirectories(path))
			DoSubtree(DoNode(dnode, dir, typeof(DirNode).Name));
	}

	//--------------------------------------------------------------------
	public Dredger(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
		DirNode root = collection.GetRootDirectory();
		if (root != null)
			DoSubtree(root);
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
		
		private void DoDredge()
		{
			while (!shuttingDown)
			{
				try
				{
					if (!paused)
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
										new Dredger(col, false);
										break;
								}
							}
						}
					}
					Thread.Sleep(1000 * 60);
				}
				catch
				{
				}
			}
		}

		#region IThreadService Members

		public void Start(Simias.Configuration conf)
		{
			store = new Store(conf);
			paused = shuttingDown = false;
			thread = new Thread(new ThreadStart(DoDredge));
			thread.IsBackground = true;
			thread.Priority = ThreadPriority.Lowest;
			thread.Start();
		}

		public void Resume()
		{
			paused = false;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Custom(int message, string data)
		{
		}

		public void Stop()
		{
			shuttingDown = true;
			thread.Interrupt();
			store.Dispose();
		}

		#endregion
	}
}
