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
	Node conflictNode;

	//---------------------------------------------------------------------------
	/// <summary>
	/// constructor -- Conflict looks a lot like a Node.
	/// Perhaps it should be derived from Node.
	/// </summary>
	public Conflict(Collection collection, Node node)
	{
		this.collection = collection;
		this.node = node;
		conflictNode = collection.GetNodeFromCollision(node);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// determines if this Node has an Update conflict
	/// </summary>
	public bool IsUpdateConflict
	{
		get
		{
			return conflictNode != null;
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
			if (!collection.IsType(node, typeof(BaseFileNode).Name) || conflictNode == null)
				return null;
			string path = IncomingNode.ParentPath(collection, node);
			return path == null? null: Path.Combine(path,
					IncomingNode.ConflictUpdatePrefix + node.ID + Path.GetExtension(conflictNode.Name));
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
			return conflictNode;
		}
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// resolve update conflict and commit 
	/// </summary>
	public void Resolve(bool localChangesWin)
	{
		if (conflictNode == null)
			return;

		if (localChangesWin)
		{
			string ucp = UpdateConflictPath;
			if (ucp != null)
				File.Delete(ucp);
			node = collection.ResolveCollision(node, conflictNode.LocalIncarnation, true);
			collection.Commit(node);
			Log.Spew("Local changes win in conflict for {0} node {1}", node.Type, node.Name);
			return;
		}

		// conflict node wins
		// we may be resolving an update conflict on a node that has a naming conflict
		string path = NonconflictedPath, fncpath = null;
		if (path != null)
		{
			try
			{
				File.Delete(path);
				File.Move(UpdateConflictPath, path);
			}
			catch (Exception ne)
			{
				Log.Spew("Could not move update conflict file to {0}: {1}", path, ne.Message);
				fncpath = FileNameConflictPath;
				File.Delete(fncpath);
				File.Move(UpdateConflictPath, fncpath);
			}
		}
		node = collection.ResolveCollision(node, conflictNode.LocalIncarnation, false);
		if (fncpath != null)
			node = collection.CreateCollision(conflictNode, true);
		conflictNode = null;
		collection.Commit(node);
		Log.Spew("Master update wins in conflict for {0} node {1}", node.Type, node.Name);
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
		collection.Commit(collection.DeleteCollision(node));
	}

	
	//---------------------------------------------------------------------------
	/// <summary>
	/// Automatically resolve all update conflicts in a collection. This method
	/// does not resolve file name conflicts.
	/// </summary>
	public static void Resolve(Collection collection, bool localChangesWin)
	{
		foreach (ShallowNode sn in collection.GetCollisions())
		{
			Conflict cf = new Conflict(collection, new Node(collection, sn));
			cf.Resolve(localChangesWin);
		}
	}
}

//===========================================================================
}
