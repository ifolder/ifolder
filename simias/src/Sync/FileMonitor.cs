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
 *			Russ Young
 *
 ***********************************************************************/
using System;
using System.Threading;
using System.Collections;
using System.IO;
using System.Diagnostics;

using Simias.Storage;
using Simias;
using Simias.Client;
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

	public class FileWatcher
	{
		internal static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(FileWatcher));

		/// <summary>
		/// The collection to monitor.
		/// </summary>
		public Collection collection = null;

		/* TODO: onServer needs to be removed. It controls how tombstones are handled:
		 *   they are deleted on the server but left on the client. What it
		 *   really needs to be is deleted if there is no upstream server. Perhaps
		 *   the best way to handle it would be for this code to always leave a
		 *   tombstone, but the sync code would just remove them if there was no
		 *   upstream server.
		 */
		bool onServer = false;
		const string lastDredgeProp = "LastDredgeTime";
		DateTime dredgeTimeStamp;
		bool needToDredge = true;
		DateTime lastDredgeTime = DateTime.MinValue;
		bool foundChange;
		string rootPath;

		bool						disposed;
//		string						collectionId;
		internal FileSystemWatcher	watcher;
		Hashtable					changes = new Hashtable();
		Timer						timer;
		int							updateTime = Timeout.Infinite;

		internal class fileChangeEntry
		{
			internal FileSystemEventArgs	eArgs;
			internal DateTime				time;

			internal fileChangeEntry(FileSystemEventArgs e)
			{
				eArgs = e;
				time = DateTime.Now;
			}
		
			internal void update(FileSystemEventArgs e)
			{
				eArgs = e;
				time = DateTime.Now;
			}

			internal void update()
			{
				time = DateTime.Now;
			}
		}

		/// <summary>
		/// Creates a dredger for this collection and dredges the system.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="onServer"></param>
		public FileWatcher(Collection collection, bool onServer)
		{
			// TODO: Syncronize the dredger with the sync engine.
			this.collection = collection;
			this.onServer = onServer;
//			this.collectionId = collection.ID;
			
			if (!MyEnvironment.Mono)
			{
				// We are on .Net use events to watch for changes.
				DirNode rootDir = collection.GetRootDirectory();
				if (rootDir != null)
				{
					string rootPath = collection.GetRootDirectory().GetFullPath(collection);
					watcher = new FileSystemWatcher(rootPath);
					log.Debug("New File Watcher at {0}", rootPath);
					watcher.Changed += new FileSystemEventHandler(OnChanged);
					watcher.Created += new FileSystemEventHandler(OnCreated);
					watcher.Deleted += new FileSystemEventHandler(OnDeleted);
					watcher.Renamed += new RenamedEventHandler(OnRenamed);
					watcher.Error += new ErrorEventHandler(watcher_Error);
					watcher.IncludeSubdirectories = true;
					watcher.EnableRaisingEvents = true;
					// Now dredge to find any files that were changed while we were down.
				}
			}
			timer = new Timer(new TimerCallback(SyncChanges), null, Timeout.Infinite, Timeout.Infinite);
			disposed = false;
		}

		/// <summary>
		/// Called to sync up changes.
		/// </summary>
		/// <param name="state"></param>
		private void SyncChanges(object state)
		{
			if (changes.Count != 0)
				SyncClient.ScheduleSync(collection.ID);
		}
			
		/// <summary>
		/// // Delete the specified node.
		/// </summary>
		/// <param name="node">The node to delete.</param>
		void DeleteNode(Node node)
		{
			Log.log.Debug("File Monitor deleting orphaned node {0}, {1}", node.Name, node.ID);
			// Check to see if we have a collision.
			if (collection.HasCollisions(node))
			{
				new Conflict(collection, node).DeleteConflictFile();
			}
			Node[] deleted = collection.Delete(node, PropertyTags.Parent);
			collection.Commit(deleted);
			foundChange = true;
		}

		/// <summary>
		/// Create a FileNode for the specified file.
		/// </summary>
		/// <param name="path">The path to the node to create.</param>
		/// <param name="parentNode">The parent of the node to create.</param>
		/// <returns>The new FileNode.</returns>
		FileNode CreateFileNode(string path, DirNode parentNode)
		{
			if (isSyncFile(path))
				return null;
			FileNode fnode = new FileNode(collection, parentNode, Path.GetFileName(path));
			log.Debug("Adding file node for {0} {1}", path, fnode.ID);
			collection.Commit(fnode);
			foundChange = true;
			return fnode;
		}

		/// <summary>
		/// Modify the FileNode for the changed file.
		/// </summary>
		/// <param name="path">The path of the file that has changed.</param>
		/// <param name="fn">The node to modify.</param>
		/// <param name="hasChanges">If the node has changes set to true.</param>
		void ModifyFileNode(string path, BaseFileNode fn, bool hasChanges)
		{
			// here we are just checking for modified files
			FileInfo fi = new FileInfo(path);
			TimeSpan ts = fi.LastWriteTime - fn.LastWriteTime;
			
			if (((uint)ts.TotalSeconds != 0) && (fn.UpdateFileInfo(collection, path)))
			{
				hasChanges = true;
				log.Debug("Updating file node for {0} {1}", path, fn.ID);
			}
			if (hasChanges)
			{
				collection.Commit(fn);
				foundChange = true;
			}
		}

		/// <summary>
		/// Create a DirNode for the specified directory.
		/// </summary>
		/// <param name="path">The path to the directory.</param>
		/// <param name="parentNode">The parent DirNode.</param>
		/// <returns>The new DirNode.</returns>
		DirNode CreateDirNode(string path, DirNode parentNode)
		{
			if (isSyncFile(path))
				return null;

			DirNode dnode = new DirNode(collection, parentNode, Path.GetFileName(path));
			log.Debug("Adding dir node for {0} {1}", path, dnode.ID);
			collection.Commit(dnode);
			if (Directory.GetFileSystemEntries(path).Length != 0)
			{
				this.DoSubtree(path, dnode, dnode.ID, true);
			}
			foundChange = true;
			return dnode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldPath"></param>
		/// <param name="newPath"></param>
		/// <returns></returns>
		bool HasParentChanged(string oldPath, string newPath)
		{
			return (!(Path.GetDirectoryName(oldPath).Equals(Path.GetDirectoryName(newPath))));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		string GetNormalizedRelativePath(string path)
		{
			string relPath = path.Replace(rootPath, "");
			relPath = relPath.TrimStart(Path.DirectorySeparatorChar);
			if (Path.DirectorySeparatorChar != '/')
				relPath = relPath.Replace('\\', '/');
			return relPath;
		}

		/// <summary>
		/// Get a ShallowNode for the named file or directory.
		/// </summary>
		/// <param name="path">Path to the file.</param>
		/// <returns>The ShallowNode for this file.</returns>
		ShallowNode GetShallowNodeForFile(string path)
		{
			string relPath = GetNormalizedRelativePath(path);
			
			ICSList nodeList;
			if (MyEnvironment.Windows)
			{
				nodeList = collection.Search(PropertyTags.FileSystemPath, relPath, SearchOp.Equal);
			}
			else
			{
				nodeList = collection.Search(PropertyTags.FileSystemPath, relPath, SearchOp.CaseEqual);
			}
				
			foreach (ShallowNode sn in nodeList)
			{
				return sn;
			}
			return null;
		}

		/// <summary>
		/// Return the parent for this path.
		/// </summary>
		/// <param name="path">Path to the file whose parent is wanted.</param>
		/// <returns></returns>
		DirNode GetParentNode(string path)
		{
			ShallowNode sn = GetShallowNodeForFile(Path.GetDirectoryName(path));
			if (sn != null)
			{
				return (DirNode)collection.GetNodeByID(sn.ID);
			}
			return null;
		}

		/// <summary>
		/// Check if the file is an internal sync file.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private bool isSyncFile(string name)
		{
			string fname = Path.GetFileName(name);
			return fname.StartsWith(".simias.");
		}


		//--------------------------------------------------------------------
		// TODO: what about file permissions and symlinks?
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="sn"></param>
		/// <param name="path"></param>
		/// <param name="isDir"></param>
		void DoShallowNode(DirNode parent, ShallowNode sn, string path, bool isDir)
		{
			Node node = null;
			DirNode dn = null;
			FileNode fn = null;
			string name = Path.GetFileName(path);
		
			// don't let temp files from sync into the collection as regular nodes
			if (name.StartsWith(".simias.") && !isDir)
				return;

			// If the lastwritetime has not changed the node is up to date.
			if (sn != null && File.GetLastWriteTime(path) <= lastDredgeTime)
			{
				if (isDir)
					DoSubtree(path, null, sn.ID, false);
				return;
			}

			node = Node.NodeFactory(collection, sn);
			if (isDir)
			{
				// This is a directory.
				dn = node as DirNode;
				if (dn == null)
				{
					// This node is the wrong type.
					DeleteNode(node);
					dn = CreateDirNode(path, parent);
				}
				DoSubtree(path, dn, dn.ID, true);
			}
			else
			{
				fn = node as FileNode;
				if (fn != null)
				{
					ModifyFileNode(path, fn, false);
				}
				else
				{
					DeleteNode(node);
					fn = CreateFileNode(path, parent);
				}
			}
		}

	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentNode"></param>
		/// <param name="path"></param>
		/// <param name="isDir"></param>
		void DoNode(DirNode parentNode, string path, bool isDir)
		{
			string name = Path.GetFileName(path);

			if (isSyncFile(name))
				return;
		
			// find if node for this file or dir already exists
			// delete nodes that are wrong type
			foreach (ShallowNode sn in collection.Search(PropertyTags.FileSystemPath, parentNode.GetRelativePath() + "/" + name, SearchOp.Equal))
			{
				DoShallowNode(parentNode, sn, path, isDir);
				// There can only be one node with the matching relative path.
				break;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="dnode"></param>
		/// <param name="nodeID"></param>
		/// <param name="subTreeHasChanged"></param>
		void DoSubtree(string path, DirNode dnode, string nodeID, bool subTreeHasChanged)
		{
			//string path = dnode.GetFullPath(collection);
			//Log.Spew("Dredger processing subtree of path {0}", path);

			if (subTreeHasChanged)
			{
				// A file or directory has been added or deleted from this directory. We need to find it.
				Hashtable existingNodes = new Hashtable();
				// Put all the existing nodes in a hashtable to match against the file system.
				foreach (ShallowNode sn in collection.Search(PropertyTags.Parent, new Relationship(collection.ID, dnode.ID)))
				{
					existingNodes[sn.Name] = sn;
				}

				// Look for new and modified files.
				foreach (string file in Directory.GetFiles(path))
				{
					ShallowNode sn = (ShallowNode)existingNodes[Path.GetFileName(file)];
					if (sn != null)
					{
						DoShallowNode(dnode, sn, file, false);
						existingNodes.Remove(sn.Name);
					}
					else
					{
						// The file is new create a new file node.
						CreateFileNode(file, dnode);
					}
				}

				// look for new directories
				foreach (string dir in Directory.GetDirectories(path))
				{
					ShallowNode sn = (ShallowNode)existingNodes[Path.GetFileName(dir)];
					if (sn != null)
					{
						DoShallowNode(dnode, sn, dir, true);
						existingNodes.Remove(sn.Name);
					}
					else
					{
						// The directory is new create a new directory node.
						DirNode newDir = CreateDirNode(dir, dnode);
						DoSubtree(dir, newDir, newDir.ID, true);
					}
				}
			
				// look for deleted files.
				// All remaining nodes need to be deleted.
				foreach (ShallowNode sn in existingNodes.Values)
				{
					DeleteNode(new Node(collection, sn));
				}
			}
			else
			{
				// Just look for modified files.
				foreach (string file in Directory.GetFiles(path))
				{
					if (File.GetLastWriteTime(file) > lastDredgeTime)
					{
						if (dnode == null)
							dnode = collection.GetNodeByID(nodeID) as DirNode;
						DoNode(dnode, file, false);
					}
				}
			
				foreach (string dir in Directory.GetDirectories(path))
				{
					if (Directory.GetLastWriteTime(dir) > lastDredgeTime)
					{
						if (dnode == null)
							dnode = collection.GetNodeByID(nodeID) as DirNode;
						DoNode(dnode, dir, true);
					}
					else 
					{
						ShallowNode sn = GetShallowNodeForFile(dir);
						if (sn != null)
							DoSubtree(dir, null, sn.ID, false);
						else
						{
							// This should never happen but if it does recall with the modified true.
							DoSubtree(path, dnode, nodeID, true);
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Dredge the Managed path.
		/// </summary>
		/// <param name="path"></param>
		void DoManagedPath(string path)
		{
//			DirectoryInfo tmpDi = new DirectoryInfo(path);
		
			// merge files from file system to store
			foreach (string file in Directory.GetFiles(path))
			{
				if (File.GetLastWriteTime(file) > lastDredgeTime && !isSyncFile(file))
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
							log.Debug("Updating store file node for {0} {1}", path, file);
							collection.Commit(unode);
							foundChange = true;
						}
					}
				}
			}
		}

		/// <summary>
		/// Dredge the file sytem to find changes.
		/// </summary>
		public void CheckForFileChanges()
		{
			collection.Refresh();
			if (watcher == null || needToDredge)
			{
				Dredge();
				needToDredge = false;
			}
			else
			{
				try
				{
					// Make sure the root directory still exists.
					if (!Directory.Exists(collection.GetRootDirectory().GetFullPath(collection)))
					{
						collection.Commit(collection.Delete());
						return;
					}
					
					dredgeTimeStamp = DateTime.Now;
					fileChangeEntry[] fChanges;

					lock (changes)
					{
						fChanges = new fileChangeEntry[changes.Count];
						changes.Values.CopyTo(fChanges, 0);
					}
			
					foreach (fileChangeEntry fc in fChanges)
					{
						string fullName = GetName(fc.eArgs.FullPath);
						bool isDir = false;

						if (fullName == null)
						{
							lock(changes)
							{
								changes.Remove(fc.eArgs.FullPath);
							}
							continue;
						}
						FileInfo fi = new FileInfo(fullName);
						isDir = (fi.Attributes & FileAttributes.Directory) > 0;
						
						ShallowNode sn = GetShallowNodeForFile(fullName);
						Node node = null;
						DirNode dn = null;
						BaseFileNode fn = null;

						if (sn != null)
						{
							node = collection.GetNodeByID(sn.ID);
							fn = node as BaseFileNode;
							dn = node as DirNode;
							// Make sure the type is still valid.
							if (fi.Exists && ((isDir && fn != null) || (!isDir && dn != null)))
							{
								needToDredge = true;
								break;
							}
							
							// We have a node update it.
							switch (fc.eArgs.ChangeType)
							{
								case WatcherChangeTypes.Created:
								case WatcherChangeTypes.Changed:
									if (!isDir)
										ModifyFileNode(fullName, fn, false);
									break;
								case WatcherChangeTypes.Deleted:
									DeleteNode(node);
									break;
								case WatcherChangeTypes.Renamed:
								{
									RenamedEventArgs args = (RenamedEventArgs)fc.eArgs;
									
									// Since we are here we have a node already.
									// This is a rename back to the original name update it.
									if (!isDir)
										ModifyFileNode(fullName, fn, false);
									
									// Make sure that there is not a node for the old name.
									sn = GetShallowNodeForFile(args.OldFullPath);
									if (sn != null && sn.ID != node.ID)
									{
										node = collection.GetNodeByID(sn.ID);
										DeleteNode(node);
									}
									break;
								}
							}
						}
						else
						{
							// The node does not exist.
							switch (fc.eArgs.ChangeType)
							{
								case WatcherChangeTypes.Deleted:
									// The node does not exist just continue.
									break;
								case WatcherChangeTypes.Created:
								case WatcherChangeTypes.Changed:
									// The node does not exist create it.
									if (isDir)
									{
										CreateDirNode(fullName, GetParentNode(fullName));
									}
									else
									{
										CreateFileNode(fullName, GetParentNode(fullName));
									}
									break;

								case WatcherChangeTypes.Renamed:
									// Check if there is a node for the old name.
									// Get the node from the old name.
									RenamedEventArgs args = (RenamedEventArgs)fc.eArgs;
									DirNode parent = null;
									sn = GetShallowNodeForFile(args.OldFullPath);
									if (sn != null)
									{
										node = collection.GetNodeByID(sn.ID);
									
										// Make sure the parent has not changed.
										if (HasParentChanged(args.OldFullPath, fullName))
										{
											// We have a new parent find the parent node.
											parent = GetParentNode(fullName);
											if (parent != null)
											{
												// We have a parent reset the parent node.
												node.Properties.ModifyNodeProperty(PropertyTags.Parent, new Relationship(collection.ID, parent.ID));
											}
											else
											{
												// We do not have a node for the parent.
												// Do a dredge.
												needToDredge = true;
												break;
											}
										}
										node.Name = Path.GetFileName(fullName);
										string relativePath = GetNormalizedRelativePath(fullName);
										string oldRelativePath = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath).ValueString;
										node.Properties.ModifyNodeProperty(new Property(PropertyTags.FileSystemPath, Syntax.String, relativePath));
										if (!isDir)
										{
											ModifyFileNode(fullName, node as BaseFileNode, true);
										}
										else
										{
											// Commit the directory.
											collection.Commit(node);
											// We need to rename all of the children nodes.
											ArrayList nodeList = new ArrayList();
											ICSList csnList = collection.Search(PropertyTags.FileSystemPath, oldRelativePath, SearchOp.Begins);
											foreach (ShallowNode csn in csnList)
											{
												// Skip the collection.
												if (csn.ID == node.ID)
													continue;

												Node childNode = collection.GetNodeByID(csn.ID);
												if (childNode != null)
												{
													Property childRP = childNode.Properties.GetSingleProperty(PropertyTags.FileSystemPath);
													if (childRP != null)
													{
														string newRP = childRP.ValueString;
														childRP.SetPropertyValue(newRP.Replace(oldRelativePath, relativePath));
														childNode.Properties.ModifyNodeProperty(childRP);
														nodeList.Add(childNode);
													}
												}
											}
											collection.Commit((Node[])nodeList.ToArray(typeof(Node)));
										}
									}
									else
									{
										// The node does not exist create it.
										if (isDir)
										{
											CreateDirNode(fullName, GetParentNode(fullName));
										}
										else
										{
											CreateFileNode(fullName, GetParentNode(fullName));
										}
									}
									break;
							}
						}
						lock(changes)
						{
							changes.Remove(fc.eArgs.FullPath);
						}
					}

					if (needToDredge)
					{
						Dredge();
						needToDredge = false;
					}
					else
					{
						DoManagedPath(collection.ManagedPath);
					}
				}
				catch
				{
					Dredge();
					needToDredge = false;
				}
			}
		}

		/// <summary>
		/// Find File changes by dredging the file system.
		/// </summary>
		public void Dredge()
		{
			// Clear the event changes since we are going to dredge.
			lock (changes)
			{
				changes.Clear();
			}
				
			collection.Refresh();
			foundChange = false;
		
			try
			{
				lastDredgeTime = (DateTime)(collection.Properties.GetSingleProperty(lastDredgeProp).Value);
			}
			catch
			{
				// Set found change so the lastDredgeTime will get updated.
				foundChange = true;
				log.Debug("Failed to get the last dredge time");
			}
			// Make sure that the RootDir still exists. IF it has been deleted on a slave remove the collection
			// And exit.
			DirNode dn = collection.GetRootDirectory();
			if (dn != null)
			{
				rootPath = dn.Properties.GetSingleProperty(PropertyTags.Root).Value as string;
				string path = dn.GetFullPath(collection);
				if (onServer || Directory.Exists(path))
				{
					DoSubtree(path, dn, dn.ID, Directory.GetLastWriteTime(path) > lastDredgeTime ? true : false);
				}
				else
				{
					// The directory no loger exits. Delete the collection.
					collection.Delete();
					collection.Commit();
					foundChange = false;
				}
			}
	
			DoManagedPath(collection.ManagedPath);
		
			if (foundChange)
			{
				Property tsp = new Property(lastDredgeProp, dredgeTimeStamp);
				tsp.LocalProperty = true;
				collection.Properties.ModifyProperty(tsp);
				collection.Properties.State = PropertyList.PropertyListState.Internal;
				collection.Commit(collection);
			}
		}

		

		/// <summary>
		/// Finalizer.
		/// </summary>
		~FileWatcher()
		{
			Dispose(true);
		}

		private string GetName(string fullPath)
		{
			if (MyEnvironment.Windows)
			{
				try
				{
					string[] caseSensitivePath = Directory.GetFileSystemEntries(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath));
					if (caseSensitivePath.Length == 1)
					{
						// We should only have one match.
						fullPath = caseSensitivePath[0];
					}
				}
				catch {}
			}

			// If this is a sync generated file return null.
			if (isSyncFile(fullPath))
				return null;

			return fullPath;
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;

			if (isSyncFile(e.Name))
				return;
			
			lock (changes)
			{
				fileChangeEntry entry = (fileChangeEntry)changes[fullPath];
				if (entry != null)
				{
					// This file has already been modified.
					// Combine the state.
					switch (entry.eArgs.ChangeType)
					{
						case WatcherChangeTypes.Created:
						case WatcherChangeTypes.Deleted:
						case WatcherChangeTypes.Changed:
							entry.update(e);
							break;
						case WatcherChangeTypes.Renamed:
							entry.update();
							break;
					}
				}
				else
				{
					changes[fullPath] = new fileChangeEntry(e);
				}
				timer.Change(updateTime, Timeout.Infinite);
			}
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			string fullPath = e.FullPath;

			if (isSyncFile(e.Name) || isSyncFile(e.OldName))
				return;
			
			lock (changes)
			{
				// Any changes made to the old file need to be removed.
				changes.Remove(e.OldFullPath);
				changes[fullPath] = new fileChangeEntry(e);
			}
			timer.Change(updateTime, Timeout.Infinite);
		}

		private void OnDeleted(object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;

			if (isSyncFile(e.Name))
				return;
						
			lock (changes)
			{
				changes[fullPath] = new fileChangeEntry(e);
			}
			timer.Change(updateTime, Timeout.Infinite);
		}

		private void OnCreated(object source, FileSystemEventArgs e)
		{
			string fullPath = e.FullPath;

			if (isSyncFile(e.Name))
				return;
						
			lock (changes)
			{
				changes[fullPath] = new fileChangeEntry(e);
			}
			timer.Change(updateTime, Timeout.Infinite);
		}

		private void watcher_Error(object sender, ErrorEventArgs e)
		{
			// We have lost events. we need to dredge.
			needToDredge = true;
		}
	
		private void Dispose(bool inFinalize)
		{
			lock (this)
			{
				if (!disposed)
				{
					if (!inFinalize)
					{
						System.GC.SuppressFinalize(this);
					}
					if (watcher != null)
					{
						watcher.Dispose();
					}
					disposed = true;
				}
			}
		}

		#region IDisposable Members

		/// <summary>
		/// Called to cleanup unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
		}

		#endregion

	}
}
