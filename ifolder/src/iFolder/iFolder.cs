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
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.IO;
using System.Net;
using Simias.Storage;
using Simias.Agent;

using Novell.AddressBook;

namespace Novell.iFolder
{
	/// <summary>
	/// Provides methods for manipulating the properties and contents of an iFolder.
	/// </summary>
	//public class iFolder
	public class iFolder : IEnumerable, IEnumerator
	{
#region Class Members
		private	Collection		collection = null;
		private Store			store = null;
		private Node			currentNode;
		private	IEnumerator		nodeEnum = null;

		/// <summary>
		/// Type of collection that represents an iFolder.
		/// </summary>
		internal const string iFolderType = "iFolder";

		/// <summary>
		/// A file in an iFolder collection.
		/// </summary>
		internal const string iFolderFileType = "File";

		/// <summary>
		/// A directory in an iFolder collection.
		/// </summary>
		internal const string iFolderDirectoryType = "Directory";
#endregion

#region Constructors
		internal iFolder(Store store)
		{
			this.store = store;
		}
#endregion

#region Internal methods
		/// <summary>
		/// Method: Load
		/// Abstract: iFolders are created only through the manager class.  The
		/// FinalConstructor method is called after construction so exceptions can be
		/// generated back to the manager method "CreateiFolder".
		///
		/// </summary>
		///
		internal void Load(Store callingStore, string iFolderID)
		{
			//Property.Syntax propertyType;

			store = callingStore;
			this.collection = store.GetCollectionById(iFolderID);

			// Make sure this collection has our store propery
			if(this.collection.Type != iFolderType)
			{
				// Raise an exception here
			}

			// Set the current node to this iFolder.
			CurrentNode= collection;
		}

		/// <summary>
		/// Create an iFolder collection.  This version of create creates the iFolder
		/// with a name of the leaf node of the path.
		/// </summary>
		/// <param name="path">The path where the iFolder collection will be rooted.</param>
		internal void Create(string path)
		{
			// Make sure the path doesn't end with a separator character.
			if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path= path.Substring(0, path.Length - 1);
			}

			// Get the leaf name.
			string name = Path.GetFileName(path);

			Uri documentRoot = new Uri(path);

			// Call to create the collection.
			this.collection = store.CreateCollection(name, iFolderType, documentRoot);

			this.collection.Commit();

			// Set the current node to newly created iFolder.
			CurrentNode= collection;
		}

		/// <summary>
		/// Create an iFolder collection.  iFolders are created only through the manager class.
		/// The FinalConstructor method is called after construction so exceptions can be
		/// generated back to the manager method "CreateiFolder".
		/// </summary>
		/// <param name="name">The friendly name of the collection.</param>
		/// <param name="path">The path where the iFolder collection will be rooted.</param>
		internal void Create(string name, string path)
		{
			// Make sure the path doesn't end with a separator character.
			if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				path= path.Substring(0, path.Length - 1);
			}

			Uri documentRoot = new Uri(path);
			this.collection = store.CreateCollection(name, iFolderType, documentRoot);

			this.collection.Commit();

			// Set the current node to newly created iFolder.
			CurrentNode= collection;
		}

		internal void Delete()
		{
			collection.Delete(true);
		}

		/// <summary>
		/// Recursively add iFolderFile nodes.
		/// </summary>
		/// <param name="path">The path at which to begin the recursive add.</param>
		/// <param name="count">Holds the number of nodes added.</param>
		internal void RecursiveAddiFolderFileNodes(string path, ref int count)
		{
			// Get the files in this directory.
			string[] dirs= Directory.GetFiles(path);
			foreach (string file in dirs)
			{
				// Create the iFolderFile of type File.
				CreateiFolderFile(file, true, iFolderFileType, false);
				count++;
			}

			// Get the sub-directories in this directory.
			dirs= Directory.GetDirectories(path);
			foreach (string dir in dirs)
			{
				// Save off the current node.
				Node node= CurrentNode;

				// Create the iFolderFile of type Directory.
				iFolderFile ifile= CreateiFolderFile(dir, true, iFolderDirectoryType, false);
				count++;

				// Set the current node to this directory node.
				CurrentNode= ifile.ThisNode;

				// Recurse and add files/directories contained in this directory.
				RecursiveAddiFolderFileNodes(dir, ref count);

				// Reset the current node to the previous parent node.
				CurrentNode= node;
			}
		}

		internal Node GetNodeForPath(string path)
		{
			Node node = null;

			// Get the leaf name.
			string relativeName = Path.GetFileName(path);

			// Search the collection for the leaf name.
			ICSEnumerator e = (ICSEnumerator)collection.Search(Property.ObjectName, relativeName, Property.Operator.Equal).GetEnumerator();
			try
			{
				bool found = false;

				// Get the next node.
				while (!found && e.MoveNext())
				{
					// Initialize parentName to the directory name of the path.
					string parentName = Path.GetDirectoryName(path);

					// Initialize parentNode to the parent of the this node.
					Node parentNode = ((Node)e.Current).GetParent();

					// TODO - fix this.
					if (parentNode == null)
					{
						Uri documentRoot= (Uri) ((Collection)e.Current).Properties.GetSingleProperty(Property.DocumentRoot).Value;
						if (((Path.DirectorySeparatorChar == Convert.ToChar("\\")) &&
									(String.Compare(path, documentRoot.LocalPath, true) == 0)) ||
								path.Equals(documentRoot.LocalPath))
						{
							node = (Node)e.Current;
						}

						// There isn't a parent node, so we are done.
						break;
					}

					// See if this node is the one we want.
					while (true)
					{
						// See if the directory name matches the parent node name.
						string parentPath = Path.GetFileName(parentName);
						if (((Path.DirectorySeparatorChar == Convert.ToChar("\\")) &&
									(String.Compare(parentPath, parentNode.Name, true) != 0)) ||
								!parentPath.Equals(parentNode.Name))
						{
							// This isn't the right node, move on to the next one.
							break;
						}

						// Check if we're at the top node of the collection.
						if (parentNode.Type == iFolderType)
						{
							// This must be the right node.
							found = true;
							node = (Node)e.Current;
							break;
						}

						// Move up a directory in the name.
						parentName = Path.GetDirectoryName(parentName);

						// Move up to the parent node.
						parentNode = parentNode.GetParent();						
					}
				}

				if (node == null)
				{
					// See if this path is the root of the collection.
					if (((Path.DirectorySeparatorChar == Convert.ToChar("\\")) &&
								(String.Compare(path, LocalPath, true) == 0)) ||
							path.Equals(LocalPath))
					{
						node = collection;
					}
				}
			}
			finally
			{
				e.Dispose();
			}

			return node;
		}
#endregion

#region Properties
		/// <summary>
		/// Gets the identity of the owner of the iFolder.
		/// </summary>
		public string OwnerIdentity 
		{
			get
			{
				return(this.collection.Owner);
			}
		}

		/// <summary>
		/// Gets/sets the name of the iFolder.
		/// </summary>
		public string Name
		{
			get
			{
				return(this.collection.Name);
			}

			set
			{
				this.collection.Name = value;
			}
		}

		/// <summary>
		/// Gets the iFolder ID.
		/// </summary>
		public string ID
		{
			get
			{
				//Property.Syntax propertyType;
				return((string) this.collection.Properties.GetSingleProperty( Property.CollectionID ).Value);
			}
		}

		/// <summary>
		/// Gets the local path of the iFolder.
		/// </summary>
		public string LocalPath
		{
			get
			{
				//Property.Syntax propertyType;
				Uri documentRoot= (Uri) this.collection.Properties.GetSingleProperty(Property.DocumentRoot).Value;
				return(documentRoot.LocalPath);
			}
		}

		/// <summary>
		/// Gets/sets the current node in the iFolder.
		/// </summary>
		public Node CurrentNode
		{
			get
			{
				return currentNode;
			}

			set
			{
				currentNode= value;
			}
		}
#endregion

#region Public Methods
		/// <summary>
		/// Recursively adds directories and files to the iFolder
		/// starting at a specified path.
		/// </summary>
		/// <param name="path">
		/// A valid path within the iFolder from which to begin the recursive add.
		/// </param>
		/// <returns>
		/// The number of nodes added.
		/// </returns>
		/// <remarks>
		/// The nodes are committed at the end of the operation.
		/// </remarks>
		public int AddiFolderFileNodes(string path)
		{
			int count= 0;

			try
			{
				string type = GetFileType(path);
				if (type == iFolderDirectoryType)
				{
					// Make sure the path doesn't end with a separator character.
					if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						path= path.Substring(0, path.Length - 1);
					}
				}

				ValidatePath(path);
				SetCurrentNodeFromRoot(path);

				if (type == iFolderDirectoryType)
				{
					// recursive add.
					RecursiveAddiFolderFileNodes(path, ref count);

					// Commit the changes.
					collection.Commit(true);
				}
				else
				{
					CreateiFolderFile(path, true, type, true);
					count++;
				}
			}
			catch (Exception e)
			{
			}

			return count;
		}

		/// <summary>
		/// Creates all directories and subdirectories as specified by path and
		/// adds them to the iFolder.
		/// </summary>
		/// <param name="path">
		/// The directory path to create.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <returns>
		/// A <see cref="DirectoryInfo"/> as specified by <paramref name="path"/>.
		/// </returns>
		/// <remarks>
		/// This is equivalent to creating a directory using
		/// <see cref="Directory.CreateDirectory"/>(<paramref name="path"/>)
		/// and then adding it to the iFolder using
		/// <see cref="CreateiFolderFile"/>(<paramref name="path"/>).
		/// </remarks>
		public DirectoryInfo CreateDirectory(string path)
		{
			// Replace slash/backslash with backslash/slash as needed
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			// If the path is a full path, verify that the specified file is
			// in this iFolder.
			if (Path.IsPathRooted(path))
			{
				path = ValidatePath(path);
			}

			// Get the full path.
			string fullPath = Path.Combine(this.LocalPath, path);

			// Create the directory path.
			DirectoryInfo dirInfo = Directory.CreateDirectory(fullPath);

			// Find the top-most directory that has not yet been
			// added to the iFolder.
			string topmostPathToAdd = null;
			DirectoryInfo topmostDirInfo = new DirectoryInfo(fullPath);
			iFolderFile file = GetiFolderFileByName(topmostDirInfo.FullName);
			while (file == null)
			{
				topmostPathToAdd = topmostDirInfo.FullName;
				topmostDirInfo = topmostDirInfo.Parent;
				file = GetiFolderFileByName(topmostDirInfo.FullName);
			}

			// Add the new directory path to the iFolder.
			if (topmostPathToAdd != null)
			{
				file = CreateiFolderFile(topmostPathToAdd);
				int count = AddiFolderFileNodes(topmostPathToAdd);
			}

			return dirInfo;
		}

		/// <summary>
		/// Creates or overwrites a file in the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of the file to create.
		/// </param>
		/// <returns>
		/// A new <see cref="FileStream"/> with a default buffer size of 8192.
		/// </returns>
		/// <remarks>
		/// This is equivalent to <see cref="CreateFile"/>(<paramref name="path"/>, 8192).
		/// </remarks>
		public FileStream CreateFile(string path)
		{
			return CreateFile(path, 8192);
		}

		/// <summary>
		/// Creates or overwrites a file in the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of the file to create.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <param name="bufferSize">
		/// The number of bytes buffered for reads and writes to the file.
		/// </param>
		/// <returns>
		/// A new <see cref="FileStream"/> with the specified buffer size.
		/// </returns>
		/// <remarks>
		/// This is equivalent to creating a file using
		/// <see cref="FileStream"/>(<paramref name="path"/>, FileMode.Create,
		/// FileAccess.ReadWrite, FileShare.None, <paramref name="bufferSize"/>)
		/// and then adding it to the iFolder using
		/// <see cref="CreateiFolderFile"/>(<paramref name="path"/>).
		/// </remarks>
		public FileStream CreateFile(string path, int bufferSize)
		{
			// Replace slash/backslash with backslash/slash as needed
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			// If the path is a full path, verify that the specified file is
			// in this iFolder.
			if (Path.IsPathRooted(path))
			{
				path = ValidatePath(path);
			}

			// Get the full path to the file.
			string fullPath = Path.Combine(this.LocalPath, path);

			// Create the file.
			FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize);

			// Add the new file to the iFolder.
			// add new file to iFolder
			iFolderFile file = CreateiFolderFile(fullPath);
			if (file == null)
			{
				throw new ApplicationException("FileCreate(" + fullPath + ", " + bufferSize + ") failed: was not added to iFolder");
			}

			return fileStream;
		}

		/// <summary>
		/// Adds a file or directory to the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of a file or directory to add to the iFolder.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <returns>
		/// An <see cref="iFolderFile"/> if successful; otherwise, null.
		/// </returns>
		/// <remarks>
		/// This version of <see cref="CreateiFolderFile"/>
		/// validates <paramref name="path"/> and searches to find a
		/// corresponding iFolder node that will be the new iFolderFile
		/// node's parent.  It then calls <see cref="CreateiFolderFile"/>
		/// (<paramref name="path"/>, false, type, true).
		/// The newly added node is committed to the database at the
		/// end of the operation.
		/// </remarks>
		public iFolderFile CreateiFolderFile(string path)
		{
			try
			{
				string type= GetFileType(path);

				string relativeName= ValidatePath(path);
				SetCurrentNodeFromRoot(path);

				return CreateiFolderFile(relativeName, false, type, true);
			}
			catch(Exception e)
			{
				return null;
			}
		}

		/// <summary>
		/// Adds a file or directory to the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of a file or directory to add to the iFolder.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <param name="isFullPath">
		/// Set to true if <paramref name="path"/> specifies a full path.
		/// </param>
		/// <param name="type">
		/// The type of this node: "File" or "Directory".
		/// </param>
		/// <param name="commit">
		/// Set to true to have the node committed after creation.
		/// </param>
		/// <returns>
		/// An <see cref="iFolderFile"/> if successful; otherwise, null.
		/// </returns>
		/// <remarks>
		/// This version of <see cref="CreateiFolderFile"/> does not perform
		/// any validation on <paramref name="path"/>.
		/// </remarks>
		public iFolderFile CreateiFolderFile(string path, bool isFullPath, string type, bool commit)
		{
			try
			{
				iFolderFile ifolderfile= new iFolderFile(collection, this, CurrentNode);
				ifolderfile.Create(path, isFullPath, type);
				if (commit)
					ifolderfile.ThisNode.Commit();
				return ifolderfile;
			}
			catch (Exception e)
			{
				return null;
			}
		}

		/// <summary>
		/// Deletes a file from the iFolder and the file system.
		/// </summary>
		/// <param name="path">
		/// The path and name of the file to delete.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <remarks>
		/// The file is deleted from the iFolder and from the file system.
		/// </remarks>
		public void DeleteFile(string path)
		{
			// Replace slash/backslash with backslash/slash as needed
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			// Make sure the is a full path.
			if (!Path.IsPathRooted(path))
			{
				path = Path.Combine(this.LocalPath, path);
			}

			// Get the iFolderFile for the specified file.
			iFolderFile file = GetiFolderFileByName(path);
			if (file == null)
			{
				throw new ArgumentException("Path " + path + " is not in this iFolder!");
			}

			// Delete the iFolder file.
			DeleteiFolderFile(file.ThisNode.Id);

			// Delete the file.
			File.Delete(path);
		}

		/// <summary>
		/// Deletes a file or directory from the iFolder, but leaves it on the file system.
		/// </summary>
		/// <param name="fileID">
		/// The iFolder file ID of the file or directory to delete.
		/// </param>
		/// <remarks>
		/// The file ID can be obtained from an <see cref="iFolderFile"/> using <b>ThisNode.Id</b>.
		/// <p>TODO: We need to determine if this method should verify that a
		/// directory has no child nodes in the iFolder before removing the
		/// directory from the iFolder.</p>
		/// </remarks>
		public void DeleteiFolderFile(string fileID)
		{
			Node nodeToDelete= collection.GetNodeById(fileID);
			if (nodeToDelete != null)
			{
				// TODO - should we only delete a Directory type iFolderFile if it has no child nodes???
				if ((nodeToDelete.Type == iFolderFileType) ||
						(nodeToDelete.Type == iFolderDirectoryType))
				{
					nodeToDelete.Delete(true);
					return;
				}
			}

			throw new ApplicationException("iFolderFile " + fileID + " not found!");
		}

		/// <summary>
		/// Gets the type of a file or directory.
		/// </summary>
		/// <param name="path">
		/// The path and name of a file or directory for which to return the type.
		/// A relative path is assumed to be relative to the current working directory.
		/// </param>
		/// <returns>
		/// <b>"File"</b> if the path specifies a file; <b>"Directory"</b> if the path specifies a directory.
		/// </returns>
		/// <exception cref="ApplicationException">
		/// The specified path does not exist.
		/// </exception>
		public string GetFileType(string path)
		{
			string type;

			if (File.Exists(path))
			{
				type = iFolderFileType;
			}
			else if (Directory.Exists(path))
			{
				type = iFolderDirectoryType;
			}
			else
			{
				throw new ApplicationException(path + " does not exist!");
			}

			return type;
		}

		/// <summary>
		/// Gets an <see cref="iFolderFile"/> for a given iFolder file ID.
		/// </summary>
		/// <param name="fileID">
		/// The iFolder file ID of the file.
		/// </param>
		/// <returns>
		/// An <see cref="iFolderFile"/> for the file specified by <paramref name="fileID"/>.
		/// </returns>
		public iFolderFile GetiFolderFile(string fileID)
		{
			iFolderFile ifolderfile= new iFolderFile(collection, this, currentNode);
			if (ifolderfile.Load(fileID) == false)
			{
				return(null);
			}

			return(ifolderfile);
		}

		/// <summary>
		/// Gets an <see cref="iFolderFile"/> for a file or directory in the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of a file or directory in the iFolder for which to
		/// return an <see cref="iFolderFile"/>.
		/// </param>
		/// <returns>
		/// An <see cref="iFolderFile"/> for the file or directory specified by
		/// <paramref name="path"/>, or <b>null</b> if it does not exist in the iFolder.
		/// </returns>
		public iFolderFile GetiFolderFileByName(string path)
		{
			// Replace slash/backslash with backslash/slash as needed
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			Node node = GetNodeForPath(path);
			if (node != null)
			{
				// Make an iFolderFile object to return.
				iFolderFile ifolderfile= new iFolderFile(collection, this, currentNode);
				if (ifolderfile.Load(node) == true)
				{
					return ifolderfile;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that iterates over all users
		/// that have rights on the iFolder.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerator"/> that iterates over all users that have
		/// rights to the iFolder and returns an
		/// <see cref="AccessControlEntry"/> for each user.
		/// </returns>
		public ICSList GetShareAccess()
		{
			return collection.GetAccessControlList();
		}

		/// <summary>
		/// Returns the access rights that a specified user has on the iFolder.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user for which to return access rights.
		/// </param>
		/// <returns>
		/// The <see cref="Access.Rights"/> for the user specified by <paramref name="userID"/>. 
		/// </returns>
		public Access.Rights GetShareAccess(string userID)
		{
			return collection.GetUserAccess(userID);
		}
		/// <summary>
		/// Determines whether the current user has rights to share the iFolder.
		/// </summary>
		/// <returns>
		/// <b>true</b> if the user can share the iFolder; <b>false</b> if the
		/// user does not have rights to share the iFolder.
		/// </returns>
		public bool IsShareable()
		{
			return collection.Shareable;
		}

		/// <summary>
		/// Moves a file into, out of, or within the iFolder. 
		/// </summary>
		/// <param name="sourcePath">
		/// The path and name of the file to move.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// </param>
		/// <param name="destPath">
		/// The path and name of the target location for the file.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// </param>
		/// <remarks>
		/// This is equivalent to using <see cref="File.Move"/> to move the file,
		/// using <see cref="DeleteiFolderFile"/> to delete
		/// <paramref name="sourcePath"/> from the iFolder (if necessary), and
		/// then using <see cref="CreateiFolderFile"/> to add
		/// <paramref name="destPath"/> to the iFolder.
		/// </remarks>
		public void MoveFile(string sourcePath, string destPath)
		{
			// Replace slash/backslash with backslash/slash as needed
			sourcePath = sourcePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			destPath = destPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			// Get full source and destination paths
			string fullSourcePath = sourcePath;
			if (!Path.IsPathRooted(sourcePath))
			{
				fullSourcePath = Path.Combine(LocalPath, sourcePath);
			}
			string fullDestPath = destPath;
			if (!Path.IsPathRooted(destPath))
			{
				fullDestPath = Path.Combine(LocalPath, destPath);
			}

			// Move the file
			File.Move(fullSourcePath, fullDestPath);

			// If the source file was in the iFolder, remove it
			iFolderFile file = GetiFolderFileByName(fullSourcePath);
			if (file != null)
			{
				this.DeleteiFolderFile(file.ThisNode.Id);
			}

			// See if the destination file lands in the iFolder
			bool isDestPathIniFolder = true;
			try
			{
				ValidatePath(fullDestPath);
			}
			catch (ArgumentException e)
			{
				isDestPathIniFolder = false;
			}

			// If the destination file should be in the iFolder
			// and is not, add it
			if (isDestPathIniFolder)
			{
				// See if the file is already in the iFolder
				file = GetiFolderFileByName(fullDestPath);
				if (file == null)
				{
					// Add file to iFolder
					file = CreateiFolderFile(fullDestPath);
					if (file == null)
					{
						throw new ApplicationException("MoveFile (" + fullSourcePath + ", " + fullDestPath + "): failed to add file to iFolder");
					}
				}
			}
		}

		/// <summary>
		/// Opens or creates a non-shared file with read/write access in the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of the file to open.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <param name="mode">
		/// A <see cref="System.IO.FileMode"/> that specifies
		/// whether the file should be created if it does not exist, and
		/// whether the its contents should be overwritten if it does exist.
		/// </param>
		/// <returns>
		/// An open non-shared <see cref="System.IO.FileStream"/> for
		/// <paramref name="path"/> with the mode specified by
		/// <paramref name="mode"/> and with read/write access.
		/// </returns>
		/// <remarks>
		/// This is equivalent to <see cref="OpenFile"/>(<paramref name="path"/>,
		/// <paramref name="mode"/>,
		/// <see cref="System.IO.FileAccess"/>.<b>ReadWrite</b>,
		/// <see cref="System.IO.FileShare"/>.<b>None</b>).
		/// </remarks>
		public FileStream OpenFile(string path, FileMode mode)
		{
			return OpenFile(path, mode, FileAccess.ReadWrite, FileShare.None);
		}

		/// <summary>
		/// Opens or creates a non-shared file with the specified access in the iFolder.
		/// </summary>
		/// <param name="path">
		/// The path and name of the file to open.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <param name="mode">
		/// A <see cref="System.IO.FileMode"/> that specifies
		/// whether the file should be created if it does not exist, and
		/// whether the its contents should be overwritten if it does exist.
		/// </param>
		/// <param name="access">
		/// A <see cref="System.IO.FileAccess"/> that specifies
		/// the operations that can be performed on the file.
		/// </param>
		/// <returns>
		/// An open non-shared <see cref="System.IO.FileStream"/> for
		/// <paramref name="path"/> with the mode specified by
		/// <paramref name="mode"/> and the access specified by
		/// <paramref name="access"/>.
		/// </returns>
		/// <remarks>
		/// This is equivalent to <see cref="OpenFile"/>(<paramref name="path"/>,
		/// <paramref name="mode"/>,
		/// <paramref name="access"/>,
		/// <see cref="System.IO.FileShare"/>.<b>None</b>).
		/// </remarks>
		public FileStream OpenFile(string path, FileMode mode, FileAccess access)
		{
			return OpenFile(path, mode, access, FileShare.None);
		}

		/// <summary>
		/// Opens or creates a file in the iFolder with the specified access
		/// and sharing option.
		/// </summary>
		/// <param name="path">
		/// The path and name of the file to open.
		/// A relative path is assumed to be relative to the root of the iFolder.
		/// A rooted path is validated to make sure it is within the iFolder.
		/// </param>
		/// <param name="mode">
		/// A <see cref="System.IO.FileMode"/> that specifies
		/// whether the file should be created if it does not exist, and
		/// whether the its contents should be overwritten if it does exist.
		/// </param>
		/// <param name="access">
		/// A <see cref="System.IO.FileAccess"/> that specifies
		/// the operations that can be performed on the file.
		/// </param>
		/// <param name="share">
		/// A <see cref="System.IO.FileShare"/> that specifies
		/// the kind of access other <see cref="System.IO.FileStream"/>s
		/// can have to the same file.
		/// </param>
		/// <returns>
		/// An open <see cref="System.IO.FileStream"/> for
		/// <paramref name="path"/> with the mode specified by
		/// <paramref name="mode"/>, the access specified by
		/// <paramref name="access"/>, and the sharing option specified by
		/// <paramref name="share"/>.
		/// </returns>
		/// <remarks>
		/// This is equivalent to opening or creating the file using
		/// <see cref="FileStream"/>
		/// (<paramref name="path"/>,
		/// <paramref name="mode"/>,
		/// <paramref name="access"/>,
		/// <paramref name="share"/>)
		/// and then adding <paramref name="path"/> to the iFolder (if needed)
		/// using <see cref="CreateiFolderFile"/>.
		/// </remarks>
		public FileStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
		{
			// Replace slash/backslash with backslash/slash as needed
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			// If the path is a full path, verify that the specified file is
			// in this iFolder.
			if (Path.IsPathRooted(path))
			{
				path = ValidatePath(path);
			}

			// Get the full path to the file.
			string fullPath = Path.Combine(this.LocalPath, path);

			// Open the file (mode may say to create it if it does not exist).
			FileStream fileStream = new FileStream(fullPath, mode, access, share);

			// Verify that the (possibly new) file is in the iFolder.
			iFolderFile file = GetiFolderFileByName(fullPath);
			if (file == null)
			{
				// add new file to iFolder
				file = CreateiFolderFile(fullPath);
				if (file == null)
				{
					throw new ApplicationException("FileOpen (" + path + ") failed: was not added to iFolder");
				}
			}

			return fileStream;
		}

		/// <summary>
		/// Sets <see cref="CurrentNode"/> to the parent node of the specified path.
		/// </summary>
		/// <param name="path">
		/// A path to use to set the current node.
		/// </param>
		/// <remarks>
		/// For example, if <paramref name="path"/>= <b>"C:\subDir1\subDir2\subDir3"</b>,
		/// <see cref="CurrentNode"/> is set to the node representing subDir2.
		/// </remarks>
		public void SetCurrentNodeFromRoot(string path)
		{
			// Check that the path is subordinate to the iFolder.
			if (path.IndexOf(LocalPath) == -1)
			{
				throw new ArgumentException("Path " + path + " is not subordinate to this iFolder!");
			}

			Node node = GetNodeForPath(Path.GetDirectoryName(path));
			if (node != null)
			{
				CurrentNode = node;
			}
		}

		/// <summary>
		/// Sets <see cref="CurrentNode"/> to the root of the iFolder.
		/// </summary>
		public void SetCurrentNodeToRoot()
		{
			CurrentNode = collection;
		}

		/// <summary>
		/// Shares the iFolder with a specified user.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user with whom to share the iFolder.
		/// </param>
		/// <param name="rights">
		/// The <see cref="Access.Rights"/> to grant the user.
		/// </param>
		/// <param name="invite">
		/// <b>true</b> if an invitation to share the iFolder should be sent;
		/// otherwise, <b>false</b>.
		/// </param>
		public void Share(string userID, Access.Rights rights, bool invite)
		{
			// Set the specified rights on the collection only if the id is not
			// the current owner.
			if (collection.LocalStore.CurrentUser != userID)
			{
				collection.SetUserAccess(userID, rights);
				collection.Commit();
			}

			if (invite)
			{
				// inform the notification service that we have shared
				IInviteAgent agent = AgentFactory.GetInviteAgent();
				Invitation invitation = agent.CreateInvitation(collection, userID);

				// TODO: where should we discover the contact information?
				Novell.AddressBook.Manager abManager =
					Novell.AddressBook.Manager.Connect(collection.LocalStore.StorePath);
				Novell.AddressBook.AddressBook ab = abManager.OpenDefaultAddressBook();

				// from
				Contact from = ab.GetContact(collection.Owner);
				invitation.FromName = from.FN;
				invitation.FromEmail = from.EMail;

				// to
				Contact to = ab.GetContact(userID);
				invitation.ToName = to.FN;
				invitation.ToEmail = to.EMail;

				// send the invitation
				agent.Invite(invitation);
			}
		}

		/// <summary>
		/// Validates that the path to a given file or directory is within the iFolder.
		/// </summary>
		/// <param name="path">
		/// An absolute path to a file or directory.
		/// </param>
		/// <returns>
		/// The file or directory path relative to the root of the iFolder.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="path"/> is not subordinate to this iFolder.
		/// </exception>
		public string ValidatePath(string path)
		{
			//int test= path.IndexOf(LocalPath);

			// Get the substring from the path that represents the iFolder path.
			string pathToTest= path.Substring(0, LocalPath.Length);

			// Check if the paths are equal.
			if (!Path.Equals(LocalPath, pathToTest))
			{
				throw new ArgumentException("File " + path + " is not subordinate to this iFolder!");
			}

			string relativeName= path.Remove(0, LocalPath.Length);

			// Make sure the relative path doesn't start with separator char.
			if (relativeName.StartsWith(Path.DirectorySeparatorChar.ToString()))
			{
				relativeName= relativeName.Remove(0, 1);
			}

			return relativeName;
		}

		/// <summary>
		/// Sets the access rights on the iFolder for the specified user.
		/// </summary>
		/// <param name="userID">
		/// The ID of the user for which to set access rights.
		/// </param>
		/// <param name="rights">
		/// The <see cref="Access.Rights"/> to set.
		/// </param>
		public void SetShareAccess(string userID, Access.Rights rights)
		{
			if (collection.LocalStore.CurrentUser != userID)
			{
				collection.SetUserAccess(userID, rights);
				collection.Commit();
			}
		}

		/// <summary>
		/// Removes all access rights on the iFolder for the specified user.
		/// </summary>
		/// <param name="userId">
		/// The ID of the user for which to remove access rights.
		/// </param>
		public void RemoveUserAccess( string userId )
		{
			collection.RemoveUserAccess( userId );
			collection.Commit();
		}
#endregion

#region IEnumerable
		/// <summary>
		/// Returns an <see cref="IEnumerator"/> that iterates over all files in the iFolder.
		/// </summary>
		/// <returns>
		/// An <see cref="IEnumerator"/> that iterates over all files in the iFolder.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			nodeEnum = collection.GetEnumerator();
			return(this);
		}

		/// <summary>
		/// Advances the enumerator to the next file in the iFolder.
		/// </summary>
		/// <returns>
		/// <b>true</b> if the enumerator was successfully advanced to the next file; <b>false</b> if the enumerator has passed the end of the iFolder.
		/// </returns>
		public bool MoveNext()
		{
			//Property.Syntax propertyType;

			if (nodeEnum != null)
			{
				while(nodeEnum.MoveNext() == true)
				{
					Node tmpNode= (Node)nodeEnum.Current;

					if (tmpNode.Type == iFolderFileType)
					{
						return (true);
					}
				}
			}

			return(false);
		}

		/// <summary>
		/// Gets the current file in the iFolder.
		/// </summary>
		/// <returns>
		/// An <see cref="iFolderFile"/> for the current file.
		/// </returns>
		/// <remarks>
		/// This property returns the current element in the enumerator.
		/// </remarks>
		public object Current
		{
			get
			{
				//Property.Syntax propertyType;

				if (nodeEnum != null)
				{
					Node currentNode= (Node) nodeEnum.Current;
					return (GetiFolderFile(currentNode.Id));
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first file in the iFolder.
		/// </summary>
		public void Reset()
		{
			nodeEnum.Reset();
		}
#endregion

	}
}
