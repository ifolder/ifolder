/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
using Simias.Identity;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
public class Dredger
{
	Collection collection = null;
	bool onServer = false;
	string docRoot = null;

	public const string NodeTypeFile = "File";
	public const string NodeTypeDir = "Directory";
	
	//TODO need to get StringBuilder to work for this
	public static string FullPath(string docRoot, Node node)
	{
		Log.Assert(node != null);
		if (node.IsCollection)
			return docRoot;

		Node p = node;
		string fp = "";
		if (!p.IsCollection)
			do
			{
				fp = Path.Combine(p.Name, fp);
				p = p.GetParent();
			} while (p != null && !p.IsCollection);
		if (p == null)
			Log.Spew("could not find collection root of {0}", node.Name);
		return Path.Combine(docRoot, fp);
	}

	string FullPath(Node node)
	{
		return FullPath(docRoot, node);
	}

	void DeleteNode(Node node)
	{
		Log.Spew("Dredger deleting orphaned node {0}, {1}", FullPath(node), node.Id);
		node.Delete(true);
		if (onServer)
			node.Delete(); // don't leave a tombstone on the server
	}

	Node DoNode(Node parentNode, string path, string type)
	{
		Log.Assert(docRoot.Length > 0 && path.StartsWith(docRoot));

		string nodeName = Path.GetFileName(path);
		string nodePath = path.Substring(docRoot.Length + 1);
		Node node = null;

		// delete nodes that are wrong type or dups
		// TODO: perhaps we should move dups to trash or log as error
		foreach (Node n in collection.GetNodesByName(nodeName))
		{
			Property p = n.Properties.GetSingleProperty(Property.ParentID);
			string parentId = p == null? null: (string)p.Value;
			if (p != null && parentId == parentNode.Id
					&& (n.Type == NodeTypeDir || n.Type == NodeTypeFile))
			{
				if (n.Type != type || node != null)
					DeleteNode(n);
				else
					node = n;
			}
		}

		bool newNode = node == null;
		if (newNode)
		{
			node = parentNode.CreateChild(nodeName, type);
			Log.Spew("Dredger adding node for {0} {1}", FullPath(node), node.Id);
			if (type == NodeTypeFile)
			{	//TODO: handle multiple streams 
				FileEntry fe = node.AddFileEntry(null, nodePath);
				fe.LastWriteTime = File.GetLastWriteTime(path);
			}
			node.Commit();
			return node;
		}
		if (type != NodeTypeFile)
			return node;

		// from here we are just checking for modified files
		DateTime fsLastWrite = File.GetLastWriteTime(path);
		foreach (FileSystemEntry fse in node.GetFileSystemEntryList())
		{
			Log.Assert(fse.IsFile && fse.RelativePath == nodePath);
			if (fse.LastWriteTime != fsLastWrite)
			{
				fse.LastWriteTime = fsLastWrite;
				node.Commit();
			}
			break; //TODO: handle multiple streams 
		}
		return node;
	}

	void DoSubtree(Node node, string path)
	{
		Log.Assert(node != null && path != null);

		// remove all nodes from store that no longer exist in the file system
		foreach (Node kid in collection.Search(Property.ParentID, node.Id,
				Property.Operator.Equal))
		{
			if (kid.Type == NodeTypeFile && !File.Exists(FullPath(kid))
					|| kid.Type == NodeTypeDir && !Directory.Exists(FullPath(kid)))
				DeleteNode(kid);
		}

		// merge files from file system to store
		foreach (string file in Directory.GetFiles(path))
			DoNode(node, file, NodeTypeFile);

		// merge subdirs and recurse.
		foreach (string dir in Directory.GetDirectories(path))
			DoSubtree(DoNode(node, dir, NodeTypeDir), dir);
	}

	public Dredger(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
		this.docRoot = collection.DocumentRoot.LocalPath;
		DoSubtree(collection, this.docRoot);
		//collection.Commit(true);
	}
}

//===========================================================================
}
