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
using System.IO;
using Simias.Storage;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// class to assist in conflict resolution
/// </summary>
public class Conflict
{
	Collection collection;
	Node node;

	//---------------------------------------------------------------------------
	/// <summary>
	/// constructor, looks a lot like a Node
	/// </summary>
	public Conflict(Collection collection, Node node)
	{
		this.collection = collection;
		this.node = node;
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// determines if this Node has an Update conflict
	/// </summary>
	public bool IsUpdateConflict
	{
		get
		{
			return collection.HasCollisions(node);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// constructor, looks a lot like a Node
	/// </summary>
	public bool IsFileNameConflict
	{
		get
		{
			string n = FileNameConflictPath;
			return n != null && File.Exists(n);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// gets the file name of the non-conflicted file for the node
	/// </summary>
	public string NonconflictedPath
	{
		get
		{
			BaseFileNode bfn = SyncOps.CastToBaseFileNode(collection, node);
			return bfn == null? null: bfn.GetFullPath(collection);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// gets the file name of the temporary file for a node whose name conflicts
	/// with something in the local file system.
	/// </summary>
	public string FileNameConflictPath
	{
		get
		{
			if (!collection.IsType(node, typeof(BaseFileNode).Name))
				return null;
			string path = IncomingNode.ParentPath(collection, node);
			return path == null? null: Path.Combine(path, IncomingNode.ConflictFilePrefix + node.ID);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// gets the full path of the file contents of the update that conflict with
	/// the local file for this node
	/// </summary>
	public string UpdateConflictPath
	{
		get
		{
			if (!collection.IsType(node, typeof(BaseFileNode).Name))
				return null;
			string path = IncomingNode.ParentPath(collection, node);
			return path == null? null: Path.Combine(path, IncomingNode.ConflictUpdatePrefix + node.ID);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// gets the contents of the node that conflicts with this node
	/// </summary>
	public Node UpdateConflictNode
	{
		get
		{
			return collection.GetNodeFromCollision(node);
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// resolve update conflict and commit 
	/// </summary>
	public void Resolve(bool localChangesWin)
	{
		Node cn = UpdateConflictNode;
		if (localChangesWin)
		{
			File.Delete(UpdateConflictPath);
			node = collection.DeleteCollision(node);

			//TODO: need a method to set node.LocalIncarnation to cn.LocalIncarnation + 1
			collection.Commit(node);
			while (node.LocalIncarnation < cn.LocalIncarnation + 1)
				collection.Commit(node);
			return;
		}

		// collision node wins
		// we may be resolving an update conflict on a node that has a naming conflict
		string path = FileNameConflictPath;
		if (path == null)
			path = NonconflictedPath;
		File.Delete(path);
		File.Move(UpdateConflictPath, path);
		collection.ImportNode(cn, node.LocalIncarnation);
		cn = collection.DeleteCollision(cn);
		collection.Commit(cn);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// resolve file name conflict and commit 
	/// </summary>
	public void Resolve(string newNodeName)
	{
		//TODO: what if move succeeds but node rename or commit fails?
		File.Move(FileNameConflictPath, Path.Combine(IncomingNode.ParentPath(collection, node), newNodeName));
		node.Name = newNodeName;
		collection.Commit(node);
	}
}

//===========================================================================
}
