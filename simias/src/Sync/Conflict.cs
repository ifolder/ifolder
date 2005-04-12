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
using System.Text;
using Simias.Storage;
using Simias;
using Simias.Client;

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
	/// <summary>
	/// The Prefix for an update conflict.
	/// </summary>
	static string			ConflictUpdatePrefix = ".simias.cu.";
	/// <summary>
	/// The prefix for a file conflict.
	/// </summary>
	static string			ConflictFilePrefix = ".simias.cf.";
	static string			TempFilePrefix = ".simias.tmp.";

	static string			ConflictBinDir = "Conflicts";
	static string			conflictBin;
	static string			ConflictNameProperty = "ConflictName";

	//---------------------------------------------------------------------------
	/// <summary>
	/// constructor -- Conflict looks a lot like a Node.
	/// Perhaps it should be derived from Node.
	/// </summary>
	public Conflict(Collection collection, Node node)
	{
		this.collection = collection;
		this.node = Collection.NodeFactory(collection, node);
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
			return conflictNode == null;
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
			BaseFileNode bfn = node as BaseFileNode;
			return bfn == null? null: bfn.GetFullPath(collection);
		}
	}

	/// <summary>
	/// Delete the temporary conflict file.
	/// </summary>
	public void DeleteConflictFile()
	{
		try
		{
			if (this.IsFileNameConflict)
				File.Delete(FileNameConflictPath);
			else if (this.IsUpdateConflict)
				File.Delete(UpdateConflictPath);
		}
		catch {}
	}

	/// <summary>
	/// gets the file name of the temporary file for a node whose name conflicts
	/// with something in the local file system.
	/// </summary>
	public string FileNameConflictPath
	{
		get
		{
			return GetFileConflictPath(collection, node);
		}
	}

	/// <summary>
	/// gets the full path of the file contents of the update that conflict with
	/// the local file for this node
	/// </summary>
	public string UpdateConflictPath
	{
		get
		{
			BaseFileNode bfn = node as BaseFileNode;
			if (bfn == null || conflictNode == null)
				return null;
			return GetUpdateConflictPath(collection, bfn);
		}
	}

	/// <summary>
	/// Gets the ConflictBin path.
	/// </summary>
	private static string ConflictBin
	{
		get
		{
			if (conflictBin == null)
			{
				conflictBin = Path.Combine(Configuration.GetConfiguration().StorePath, ConflictBinDir);
				if (!Directory.Exists(conflictBin))
					Directory.CreateDirectory(conflictBin);
			}
			return conflictBin;
		}
	}

	/// <summary>
	/// Gets the file name for an update conflict.
	/// </summary>
	/// <param name="collection">The collection the node belongs to.</param>
	/// <param name="bfn">The BaseFile Node.</param>
	/// <returns>The path for the conflict file.</returns>
	public static string GetUpdateConflictPath(Collection collection, BaseFileNode bfn)
	{
		return (Path.Combine(ConflictBin, ConflictUpdatePrefix + bfn.ID + Path.GetExtension(bfn.Name)));
	}

	/// <summary>
	/// Gets the file name for a File name conflict.
	/// </summary>
	/// <param name="collection">The collection the node belongs to.</param>
	/// <param name="node">The Node.</param>
	/// <returns>The path for the conflict file.</returns>
	public static string GetFileConflictPath(Collection collection, Node node)
	{
		Property pPath = node.Properties.GetSingleProperty(ConflictNameProperty);
		if (pPath != null)
		{
			return pPath.Value.ToString();
		}
		return (Path.Combine(ConflictBin, ConflictFilePrefix + node.ID));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="node"></param>
	/// <param name="path"></param>
	public static void SetFileConflictPath(Node node, string path)
	{
		Property pPath = new Property(ConflictNameProperty, path);
		pPath.LocalProperty = true;
		node.Properties.ModifyProperty(pPath);
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
			Log.log.Debug("Local changes win in conflict for {0} node {1}", node.Type, node.Name);
			return;
		}

		// conflict node wins
		// we may be resolving an update conflict on a node that has a naming conflict
		string path = NonconflictedPath, fncpath = null;
		FileInfo fInfo = null;
		if (path != null)
		{
			try
			{
				File.Delete(path);
				File.Move(UpdateConflictPath, path);
				fInfo = new FileInfo(path);
			}
			catch (Exception ne)
			{
				Log.log.Debug("Could not move update conflict file to {0}: {1}", path, ne.Message);
				fncpath = FileNameConflictPath;
				File.Delete(fncpath);
				File.Move(UpdateConflictPath, fncpath);
			}
		}
		node = collection.ResolveCollision(node, conflictNode.LocalIncarnation, false);
		if (fncpath != null)
		{
			node = collection.CreateCollision(conflictNode, true);
			Conflict.SetFileConflictPath(node, path);
		}
		if (fInfo != null)
		{
			BaseFileNode bfn = node as BaseFileNode;
			if (bfn != null)
			{
				fInfo.CreationTime = bfn.CreationTime;
				fInfo.LastWriteTime = bfn.LastWriteTime;
			}
		}
		conflictNode = null;
		collection.Commit(node);
		Log.log.Debug("Master update wins in conflict for {0} node {1}", node.Type, node.Name);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// resolve file name conflict and commit 
	/// </summary>
	public void Resolve(string newNodeName)
	{
		FileNode fn = node as FileNode;
		if (fn != null)
		{
			if (newNodeName == node.Name)
			{
				// We are resolving to the same name.
				if (Path.GetDirectoryName(FileNameConflictPath) != Path.GetDirectoryName(NonconflictedPath))
				{
					// This file is in the conflict bin and has been sync-ed from the server.  We do not need
					// To push it back up.  Set internal.
					node.Properties.State = PropertyList.PropertyListState.Internal;
					File.Move(FileNameConflictPath, Path.Combine(Path.GetDirectoryName(NonconflictedPath), newNodeName));
				}
			}
			else
			{
				//TODO: what if move succeeds but node rename or commit fails?
				File.Move(FileNameConflictPath, Path.Combine(Path.GetDirectoryName(NonconflictedPath), newNodeName));
				string relativePath = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath).Value.ToString();
				relativePath = relativePath.Remove(relativePath.Length - node.Name.Length, node.Name.Length) + newNodeName;
				node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
				node.Name = newNodeName;
			}
			Node newNode = collection.DeleteCollision(node);
			newNode.Properties.DeleteSingleProperty(ConflictNameProperty);
			collection.Commit(newNode);
		}
		else
		{
			DirNode dn = node as DirNode;
			if (dn != null)
			{
				Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(FileNameConflictPath), newNodeName));
				string relativePath = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath).Value.ToString();
				relativePath = relativePath.Remove(relativePath.Length - node.Name.Length, node.Name.Length) + newNodeName;
				node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
				node.Name = newNodeName;
				Node newNode = collection.DeleteCollision(node);
				newNode.Properties.DeleteSingleProperty(ConflictNameProperty);
				collection.Commit(newNode);
			}
		}
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

	/// <summary>
	/// Rename the file that is in the way of the node with the name conflict.
	/// </summary>
	/// <param name="newName">The new name of the file.</param>
	public void RenameConflictingFile(string newName)
	{
		DirNode rn = collection.GetRootDirectory();
		string rootPath = rn.Properties.GetSingleProperty(PropertyTags.Root).Value.ToString();
		BaseFileNode cfn = node as BaseFileNode;
		string relPath = Simias.Sync.FileWatcher.GetNormalizedRelativePath(rootPath, cfn.GetFullPath(collection));

		// Now find all nodes with the old name.	
		ICSList nodeList;
		if (MyEnvironment.Windows)
		{
			nodeList = collection.Search(PropertyTags.FileSystemPath, relPath, SearchOp.Equal);
		}
		else
		{
			nodeList = collection.Search(PropertyTags.FileSystemPath, relPath, SearchOp.CaseEqual);
		}
		
		BaseFileNode fn = null;
		foreach (ShallowNode sn in nodeList)
		{
			// We don't want the node with the collision.
			if (sn.ID == node.ID)
				continue;
			fn = collection.GetNodeByID(sn.ID) as BaseFileNode;
			break;
		}

		if (fn != null)
		{
			// Now rename the file and the node.
			string parentPath = Path.GetDirectoryName(fn.GetFullPath(collection));
			string tmpName = Path.Combine(parentPath, TempFilePrefix + fn.ID);
			string newFName = Path.Combine(parentPath, newName);
			File.Move(fn.GetFullPath(collection), tmpName);
			File.Move(tmpName, newFName);
			string relativePath = fn.Properties.GetSingleProperty(PropertyTags.FileSystemPath).Value.ToString();
			relativePath = relativePath.Remove(relativePath.Length - fn.Name.Length, fn.Name.Length) + newName;
			fn.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
			fn.Name = newName;
		}
		collection.Commit(fn);
	}
}

//===========================================================================
}
