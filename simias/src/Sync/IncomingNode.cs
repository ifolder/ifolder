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

using Simias.Storage;
using Simias;

namespace Simias.Sync
{

//---------------------------------------------------------------------------
/// <summary>
/// valid states of a node update attempt
/// </summary>
[Serializable]
public enum NodeStatus
{
	/// <summary> node update was successful </summary>
	Complete,

	/// <summary> node update was aborted due to update from other client </summary>
	UpdateConflict,

	/// <summary> node update was completed, but temporary file could not be moved into place </summary>
	FileNameConflict,

	/// <summary> node update was probably unsuccessful, unhandled exception on the server </summary>
	ServerFailure,

	/// <summary> node update is in progress </summary>
	InProgess,

	/// <summary>
	/// The File is in use.
	/// </summary>
	InUse,
};

//---------------------------------------------------------------------------
/// <summary>
/// class to receive Node information in pieces and commit it when done.
/// Complete() must be called to complete the file or the partial
/// file will be deleted by the destructor.
/// </summary>
internal class IncomingNode
{
	// TODO: add code to hide (set HIDDEN file attribute on) .simias.*. files
	public const string ConflictUpdatePrefix = ".simias.cu.";
	public const string ConflictFilePrefix = ".simias.cf.";
	public const string TempFilePrefix = ".simias.tf.";
	public const string TempFileDone = "TemporaryFileDone";

	Collection collection;
	bool onServer;
	Node node, oldNode;
	FileInfo fileInfo, oldFi;
	class Fork { public string name; public Stream stream; };
	ArrayList forkList = null;
	string path = null;  // for DirNodes this is the full path, all others it is the parent path
	FileStream fileStream = null;
	bool       fileNameConflict = false;
		

	public IncomingNode(Collection collection, bool onServer)
	{
		this.collection = collection;
		this.onServer = onServer;
	}

	void CleanUp()
	{
		if (forkList != null)
			foreach (Fork fork in forkList)
				if (fork.stream != null)
					fork.stream.Close();
		forkList = null;
		if (fileInfo != null)
			fileInfo.Delete();
		fileInfo = null;
		path = null;
	}

	~IncomingNode() { CleanUp(); }

	static string ParentID(Node node)
	{
		Property p = node.Properties.GetSingleProperty(PropertyTags.Parent);
		Relationship rship = p == null? null: (p.Value as Relationship);
		return rship == null? null: rship.NodeID;
	}

	public static string ParentPath(Collection collection, Node node)
	{
		string parentID = ParentID(node);
		if (parentID == null)
			return null;
		Node n = collection.GetNodeByID(parentID);
		if (n == null)
			return null;

		DirNode pn = SyncOps.CastToDirNode(collection, n);
		return pn == null? null: pn.GetFullPath(collection);
	}

	public void Start(Node node, string relativePath)
	{
		CleanUp();
		this.node = node;
		oldNode = collection.GetNodeByID(node.ID);

		if (collection.IsType(node, typeof(DirNode).Name))
		{
			if (relativePath == null || relativePath == "")
				throw new SimiasException("incoming DirNode must supply relative path");

			DirNode rn = collection.GetRootDirectory();
			Property p = rn == null? null: rn.Properties.GetSingleProperty(PropertyTags.Root);
			if (p == null)
				throw new SimiasException("incoming DirNode to rootless Collection");

			path = Path.Combine((p.Value as Uri).LocalPath, relativePath);
			this.node = SyncOps.CastToDirNode(collection, node);
		}
		else if (collection.IsType(node, typeof(BaseFileNode).Name))
		{
			// make sure parent directory exists for temporary file
			if ((path = ParentPath(collection, node)) == null)
				throw new SimiasException("could not get parent path of incoming FileNode");
			Directory.CreateDirectory(path);
			this.node = SyncOps.CastToBaseFileNode(collection, node);
		}
		else
			path = collection.ManagedPath;

		//Log.Spew("Starting incoming node {0}", node.Name);
	}

	// accept some chunks of data for this file
	public void BlowChunks(ForkChunk[] chunks)
	{
		if (chunks == null)
			return;

		foreach (ForkChunk chunk in chunks)
		{
			bool done = false;

			if (forkList == null)
				forkList = new ArrayList();
			else
				foreach (Fork fork in forkList)
					if (chunk.name == fork.name)
					{
						fork.stream.Write(chunk.data, 0, chunk.data.Length);
						done = true;
						break;
					}

			if (!done)
			{
				/* TODO: handle multiple forks (streams), EAs, etc. For right now
				 * this is just a guess at how to do it, stubbed to support
				 * OutgoingNode.
				 * Multiple data streams are really a pain, It is unknown and
				 * unlikely that they are needed or supported by mono or .net
				 */
				Log.Assert(chunk.name == ForkChunk.DataForkName);

				if (fileInfo == null)
					fileInfo = new FileInfo(Path.Combine(path, TempFilePrefix + node.ID));
				Fork fork = new Fork();
				fork.name = chunk.name;
				fork.stream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.None);
				fork.stream.Write(chunk.data, 0, chunk.data.Length);
				forkList.Add(fork);
			}
		}
	}

	// everything else is done, just make sure we clear the TempFileDone flag
	void ClearTemp()
	{
		// TODO is the get by id needed.
		Node n = collection.GetNodeByID(node.ID);
		n.Properties.State = PropertyList.PropertyListState.Internal;
		n.Properties.DeleteSingleProperty(TempFileDone);
		collection.Commit(n);
		return;
	}

	// try to move the temp file into position
	NodeStatus CommitFile(NodeStatus status)
	{
		if (collection.IsType(node, typeof(DirNode).Name))
		{
			string p;
			try
			{
				p = new DirNode(node).GetFullPath(collection);
				Log.Spew("Creating directory {0}", p);
			}
			catch (DoesNotExistException)
			{
				p = path;
				Log.Spew("Could not create directory for full path, falling back to specific path {0}", p);
			}
			Directory.CreateDirectory(p);
			ClearTemp();
			return status;
		}

		BaseFileNode bfn = SyncOps.CastToBaseFileNode(collection, node);
		if (bfn == null)
		{
			Log.Spew("commiting nonFile, nonDir {0}", node.Name);
			Log.Assert(forkList == null);
			ClearTemp();
			return status;
		}
		
		Log.Assert(forkList != null);
		foreach (Fork fork in forkList)
		{
			fork.stream.Close();
			fork.stream = null;
		}

		path = status == NodeStatus.UpdateConflict?
				Path.Combine(path, ConflictUpdatePrefix + node.ID + Path.GetExtension(node.Name)):
				Path.Combine(path, node.Name);

		Log.Spew("placing file {0}", path);

		/* TODO: there is still a window when moving the temp file into position. It
		 * could have been updated again since the Node was committed. Should move
		 * old file out of the way, move new file in, check old file mod time and size
		 * to see if it is what we expected to have moved. If it is not, flag it as
		 * a new kind of conflict.
		 */
		try
		{
			if (fileStream != null)
			{
				fileStream.Close();
				// Delete the file if not a collision.
				if (!fileNameConflict)
					oldFi.Delete();
				fileStream = null;
			}
			fileInfo.MoveTo(path);
		}
		catch (Exception e)
		{
			Log.Spew("Could not move tmp file to {0}: {1}", path, e.Message);
			Log.Assert(status != NodeStatus.UpdateConflict); // should not happen if renaming to an update conflict file

			path = Path.Combine(ParentPath(collection, node), ConflictFilePrefix + node.ID);
			try
			{
				File.Delete(path);
				fileInfo.MoveTo(path);
			}
			catch (Exception ne)
			{
				Log.Spew("Could not move tmp file to {0}: {1}", path, ne.Message);
				return NodeStatus.ServerFailure;
			}
			status = NodeStatus.FileNameConflict;
			node = collection.CreateCollision(node, true);
			collection.Commit(node);
		}
		try
		{
			fileInfo.LastWriteTime = bfn.LastWriteTime;
			fileInfo.CreationTime = bfn.CreationTime;
		}
		catch (Exception e)
		{
			Log.Spew("Could not set time attributes on {0}", path);
		}
		fileInfo = null;
		ClearTemp();
		return status;
	}

	/* TODO: This code handles update collisions and attempts to rename the
	 * completed file to the desired name. This may involve a number of
	 * steps with partial completion possible due to power failures, disk
	 * crashes and the like.  The Dredger should be run at each startup to
	 * collect changes that may have been missed by the event system when
	 * it was not operational. Therefore, the Dredger should also be able
	 * to handle any incomplete incoming nodes.
	 */
	public NodeStatus Complete(ulong expectedIncarn)
	{
		try
		{
			Log.Spew("importing {0} {1} to collection {2}", node.Name, node.ID, collection.Name);

			if (collection.IsType(node, typeof(BaseFileNode).Name))
			{
				BaseFileNode bfn = (BaseFileNode)node;
				// TODO make sure that the parent dir is always created first.
				string fPath = bfn.GetFullPath(collection);
				try
				{
					oldFi = new FileInfo(fPath);
					// Open the file to protect against modifies while it is updated.
					if (oldFi.Exists)
					{
						fileStream = oldFi.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
						// Compare the name to see if this node represents the file.
						FileInfo[] fiArray = oldFi.Directory.GetFiles(oldFi.Name);
						if (fiArray[0].Name.Equals(bfn.GetFileName()))
						{
							BaseFileNode oldBfn = (BaseFileNode)oldNode;
							if (oldFi.LastWriteTime != oldBfn.LastWriteTime)
							{
								// The file has changed locally update the lastWrite and commit.
								oldBfn.LastWriteTime = oldFi.LastWriteTime;
								collection.Commit(oldNode);
							}
						}
						else
						{
							// We have a name conflict.
							fileNameConflict = true;
						}
					}
				}	
				catch (IOException ex)
				{
					// The file is in use will get synced next pass.
					Log.Spew("The file {0} is in use", fPath);
					return NodeStatus.Complete;
				}
				catch (ArgumentException ex)
				{
					fileNameConflict = true;
				}
			}
			
			if (!onServer)
			{
				// If we are on a client we don't know the version
				// to expect.  We only need to know if the node has been
				// changed locally.  This is done by comparing the local
				// to the master.
				expectedIncarn = 0;
			}
			collection.ImportNode(node, onServer, expectedIncarn);
			node.Properties.ModifyProperty(TempFileDone, true);
			node.IncarnationUpdate = node.LocalIncarnation;

			try
			{
				collection.Commit(node);
			}
			catch (CollisionException c)
			{
				if (onServer)
				{
					Log.Spew("Rejecting update for node {0} due to update conflict on server", node.Name);
					CleanUp();
					return NodeStatus.UpdateConflict;
				}
				else
				{
					Log.Spew("Node {0} {1} has lost an update collision", node.Type, node.Name);
					node.Properties.DeleteSingleProperty(TempFileDone);
					node = collection.CreateCollision(node, false);
					node.Properties.ModifyProperty(TempFileDone, true);
					try
					{
						collection.Commit(node);
						return CommitFile(NodeStatus.UpdateConflict);
					}
					catch (CollisionException ce)
					{
						Log.Spew("Node {0} has again lost an update collision", oldNode.Name);
					}
				}
			}
			return CommitFile(NodeStatus.Complete);
		}
		finally
		{
			// Close the file.
			if (fileStream != null)
			{
				fileStream.Close();
				fileStream = null;
			}
		}
	}
}

//===========================================================================
}
