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
using System.Collections;
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
	CollisionType cType;
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
	static string			ConflictLinkProperty = "ConflictLink";

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
		cType = collection.GetCollisionType(node);
	}

	//---------------------------------------------------------------------------
	/// <summary>
	/// determines if this Node has an Update conflict
	/// </summary>
	public bool IsUpdateConflict
	{
		get
		{
			return cType == CollisionType.Node;
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
			return cType == CollisionType.File;
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
	/// Create a name conflict on the node.
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="node"></param>
	/// <returns></returns>
	public static Node CreateNameConflict(Collection collection, Node node)
	{
		node = collection.CreateCollision(node, true);
		string path = GetFileConflictPath(collection, node);
		SetFileConflictPath(node, path);
		return node;
	}

	/// <summary>
	/// Create a name conflict on the node.
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="node"></param>
	/// <param name="path">The path where the file resides.</param>
	/// <returns></returns>
	public static Node CreateNameConflict(Collection collection, Node node, string path)
	{
		node = collection.CreateCollision(node, true);
		SetFileConflictPath(node, path);
		return node;
	}

	/// <summary>
	/// Remove the name conflict.
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="node"></param>
	/// <returns></returns>
	public static Node RemoveNameConflict(Collection collection, Node node)
	{
		node = collection.DeleteCollision(node);
		node.Properties.DeleteSingleProperty(ConflictNameProperty);
		node.Properties.DeleteSingleProperty(ConflictLinkProperty);
		return node;
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

	/// <summary>
	/// Link the two conflicting nodes together.
	/// </summary>
	/// <param name="node"></param>
	/// <param name="cNode"></param>
	public static void LinkConflictingNodes(FileNode node, FileNode cNode)
	{
		Property pLink = new Property(ConflictLinkProperty, cNode.ID);
		pLink.LocalProperty = true;
		cNode.Properties.ModifyProperty(pLink);
			
		pLink = new Property(ConflictLinkProperty, node.ID);
		pLink.LocalProperty = true;
		node.Properties.ModifyProperty(pLink);
	}

	/// <summary>
	/// Get the conflicting node.
	/// </summary>
	/// <param name="collection"></param>
	/// <param name="node"></param>
	/// <returns></returns>
	public static FileNode GetConflictingNode(Collection collection, FileNode node)
	{
		Property pLink = node.Properties.GetSingleProperty(ConflictLinkProperty);
		if (pLink != null)
		{
			return collection.GetNodeByID(pLink.Value.ToString()) as FileNode;
		}
		return null;
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
			node = CreateNameConflict(collection, node, path);
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
		if (!SyncFile.IsNameValid(newNodeName))
			throw new MalformedException(newNodeName);
		FileNode fn = node as FileNode;
		if (fn != null)
		{
			DirNode parent = fn.GetParent(collection);
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
				if (SyncFile.DoesNodeExist(collection, parent, newNodeName))
					throw new ExistsException(newNodeName);
				//TODO: what if move succeeds but node rename or commit fails?
				File.Move(FileNameConflictPath, Path.Combine(Path.GetDirectoryName(NonconflictedPath), newNodeName));
				string relativePath = fn.GetRelativePath();
				relativePath = relativePath.Remove(relativePath.Length - node.Name.Length, node.Name.Length) + newNodeName;
				node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
				node.Name = newNodeName;
			}
			node = RemoveNameConflict(collection, node);
			collection.Commit(node);
		}
		else
		{
			DirNode dn = node as DirNode;
			if (dn != null)
			{
				DirNode parent = dn.GetParent(collection);
				if (SyncFile.DoesNodeExist(collection, parent, newNodeName))
					throw new ExistsException(newNodeName);
				string oldname, newname;
				oldname = FileNameConflictPath;
				newname = Path.Combine(Path.GetDirectoryName(FileNameConflictPath), newNodeName);
				Directory.Move(oldname, newname);
				string oldRelativePath = dn.GetRelativePath();
				string relativePath = oldRelativePath.Remove(oldRelativePath.Length - node.Name.Length, node.Name.Length) + newNodeName;
				node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
				node.Name = newNodeName;
				node = RemoveNameConflict(collection, node);
				collection.Commit(node);
				FileWatcher.RenameDirsChildren(collection, dn, oldRelativePath);
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
		if (!SyncFile.IsNameValid(newName))
			throw new MalformedException(newName);
		FileNode cfn = node as FileNode;
		string relPath = cfn.GetRelativePath();

		// Now get the conflicting node.
		FileNode fn = collection.GetNodeByID(node.Properties.GetSingleProperty(ConflictLinkProperty).Value.ToString()) as FileNode;
		
		if (fn != null)
		{
			// Now rename the file and the node.
			string parentPath = Path.GetDirectoryName(fn.GetFullPath(collection));
			string tmpName = Path.Combine(parentPath, TempFilePrefix + fn.ID);
			string newFName = Path.Combine(parentPath, newName);
			File.Move(fn.GetFullPath(collection), tmpName);
			File.Move(tmpName, newFName);
			string relativePath = fn.GetRelativePath();
			relativePath = relativePath.Remove(relativePath.Length - fn.Name.Length, fn.Name.Length) + newName;
			fn.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
			fn.Name = newName;
		}
		fn.Properties.DeleteSingleProperty(ConflictLinkProperty);
		collection.Commit(fn);
	}
}

//===========================================================================
}
