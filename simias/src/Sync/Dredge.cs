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
using System.Collections;
using System.IO;
using System.Diagnostics;

using Simias.Storage;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to sync a portion of the file system with a collection
/// applying iFolder specific behavior
/// </summary>
internal class Dredger
{
	Collection collection = null;
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

		// delete nodes that are wrong type or dups
		// TODO: perhaps we should move dups to trash or log as error
		//TODO: handle issues of file name chars and case here?
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
			return new DirNode(collection, node);

		// from here we are just checking for modified files
		FileNode unode = new FileNode(collection, node);
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
	void DoSubtree(DirNode dnode)
	{
		string path = dnode.GetFullPath(collection);

		// remove all nodes from store that no longer exist in the file system
		foreach (ShallowNode sn in collection.Search(PropertyTags.Parent, new Relationship(collection.ID, dnode.ID)))
		{
			Node kid = new Node(collection, sn);
			if (collection.IsType(kid, typeof(DirNode).Name)
					&& !Directory.Exists(Path.Combine(path, kid.Name))
					|| collection.IsType(kid, typeof(FileNode).Name)
					&& !File.Exists(Path.Combine(path, kid.Name)))
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
}
