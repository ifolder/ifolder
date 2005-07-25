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
 *  Author: Russ Young
 *
 ***********************************************************************/
using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;
using Simias.Storage;
using Simias.Event;
using Simias.Sync.Delta;
using Simias.Client;

namespace Simias.Sync
{
	#region OutFile

	/// <summary>
	/// Class to handle file operations for a file to be synced out.
	/// </summary>
	public abstract class OutFile : SyncFile
	{
		#region fields

		StreamStream	workStream;

		/// <summary>
		/// Gets the output stream.
		/// </summary>
		protected StreamStream OutStream
		{
			get { return workStream; }
		}

		#endregion
		
		#region Constructor / Finalizer

		/// <summary>
		/// Constructs an OutFile object.
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		protected OutFile(Collection collection) :
			base(collection)
		{
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~OutFile()
		{
			Close (true);
		}

		#endregion

		#region public methods.

		/// <summary>
		/// Reads data into the buffer.
		/// </summary>
		/// <param name="stream">The stream to read into.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(Stream stream, int count)
		{
			try
			{
				//Log.log.Debug("Reading File {0} : offset = {1}", file, ReadPosition);
				return workStream.Read(stream, count);
			}
			catch (Exception ex)
			{
				Log.log.Debug(ex, "Failed Reading {0}", file);
				throw ex;
			}
		}

		/// <summary>
		/// Get the platform file handle.
		/// </summary>
		public StreamStream outStream
		{
			get {return workStream;}
		}


		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long ReadPosition
		{
			get { return workStream.Position; }
			set { workStream.Position = value; }
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		public long Length
		{
			get { return workStream.Length; }
		}

		#endregion

		#region protected methods.

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		/// <param name="sessionID">The unique session ID.</param>
		protected void Open(BaseFileNode node, string sessionID)
		{
			SetupFileNames(node, sessionID);
			Log.log.Debug("Opening File {0}", file);
			FileInfo fi = new FileInfo(file);
			if (Store.GetStore().IsEnterpriseServer || fi.Length > (1024 * 100000))
			{
				workStream = new StreamStream(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
				workFile = null;
			}
			else
			{
				// This file is being pushed make a copy to work from.
				File.Copy(file, workFile, true);
				File.SetAttributes(workFile, FileAttributes.Normal);
				workStream = new StreamStream(File.Open(workFile, FileMode.Open, FileAccess.Read));
			}
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected void Close()
		{
			Log.log.Debug("Closing File {0}", file);
			Close (false);
		}
		
		#endregion

		#region private methods.

		/// <summary>
		/// Called to close the file and cleanup.
		/// </summary>
		/// <param name="InFinalizer">true if called from the finalizer.</param>
		private void Close(bool InFinalizer)
		{
			if (!InFinalizer)
				GC.SuppressFinalize(this);

			if (workStream != null)
			{
				workStream.Close();
				workStream = null;
			}
			// We need to delete the temp file.
			if (workFile != null)
				File.Delete(workFile);
		}

		#endregion
	}

	#endregion

	#region InFile

	/// <summary>
	/// Class to handle files that are being imported.
	/// </summary>
	public abstract class InFile : SyncFile
	{
		#region fields

		/// <summary>Stream to the Incoming file.</summary>
		StreamStream	workStream;
		/// <summary>Stream to the Original file.</summary>
		FileStream		stream;
		/// <summary>The partially downloaded file.</summary>
		string			partialFile;
		/// <summary>The Old Node if it exists.</summary>
		protected BaseFileNode	oldNode;
				
		#endregion
		
		#region Constructor / Finalizer.

		/// <summary>
		/// Constructs an InFile object.
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		protected InFile(Collection collection) :
			base(collection)
		{
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~InFile()
		{
			Close (true, false);
		}

		#endregion

		#region public methods.

		/// <summary>
		/// Reads data into the buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The offset in the buffer to read into.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(byte[] buffer, int offset, int count)
		{
			if (stream != null)	return stream.Read(buffer, offset, count);
			else return 0;
		}

		/// <summary>
		/// Writes data from buffer into file.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(Stream stream, int count)
		{
			//Log.log.Debug("Writing File {0} : offset = {1}", file, WritePosition);
			workStream.Write(stream, count);
		}

		/// <summary>
		/// Copyt the data from the original file into the new file.
		/// </summary>
		/// <param name="originalOffset">The offset in the original file to copy from.</param>
		/// <param name="offset">The offset in the file where the data is to be written.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Copy(long originalOffset, long offset, int count)
		{
			lock (this)
			{
				ReadPosition = originalOffset;
				WritePosition = offset;
				workStream.Write(stream, count);
			}
		}

		/// <summary>
		/// Set the Length of the file.
		/// </summary>
		/// <param name="length">The size to set.</param>
		public void SetLength(long length)
		{
			workStream.SetLength(length);
		}
		
		/// <summary>
		/// Get the stream.
		/// </summary>
		public StreamStream inStream
		{
			get {return workStream;}
		}

		/// <summary>
		/// Gets the original stream.
		/// </summary>
		public FileStream ReadStream
		{
			get {return stream;}
		}


		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long ReadPosition
		{
			get { return (stream == null ? 0 : stream.Position); }
			set { if (stream != null) stream.Position = value; }
		}

		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long WritePosition
		{
			get { return workStream.Position; }
			set { workStream.Position = value; }
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		public long Length
		{
			get { return node.Length; }
		}
		
		#endregion

		#region protected methods.

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		protected void Open(BaseFileNode node)
		{
			SetupFileNames(node, "");
			CheckForNameConflict();
			Log.log.Debug("Opening File {0}", file);
			// Open the file so that it cannot be modified.
			oldNode = collection.GetNodeByID(node.ID) as BaseFileNode;
			try
			{
				if (!NameConflict)
				{
					stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
				}
			}
			catch (FileNotFoundException)
			{
				// Check to see if we have a partially downloaded file to delta sync with.
				if (collection.Role == SyncRoles.Slave && File.Exists(workFile))
				{
					if (File.Exists(partialFile))
						File.Delete(partialFile);
					partialFile = workFile + ".part";
					File.Move(workFile, partialFile);
					stream = File.Open(partialFile, FileMode.Open, FileAccess.Read, FileShare.None);
				}
				else if (oldNode != null)
				{
					// The file may have been renamed.
					string oldPath = oldNode.GetFullPath(collection);
					if (oldPath != file)
						stream = File.Open(oldPath, FileMode.Open, FileAccess.Read, FileShare.None);
				}
			}
			// Create the file in the parent directory and then move to the work area.
			// This will insure that the proper attributes are set.
			// This was added to support EFS (Encrypted File System).
			string createName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileName(workFile));
			FileStream tmpStream = File.Open(createName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			if (File.Exists(workFile))
			{
				File.Delete(workFile);
			}
			// Make sure we have enough space for the file.
			try
			{
				tmpStream.SetLength(node.Length);
				File.Move(createName, workFile);
			}
			catch (IOException)
			{
				throw new InsufficientStorageException();
			}
			finally
			{
				tmpStream.Close();
			}
			workStream = new StreamStream(File.Open(workFile, FileMode.Truncate, FileAccess.ReadWrite, FileShare.None));
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected void Close(bool commit)
		{
			Log.log.Debug("Closing File {0}", file);
			Close (false, commit);
		}

		#endregion

		#region private methods.

		/// <summary>
		/// Called to cleanup any resources and close the file.
		/// </summary>
		/// <param name="InFinalizer"></param>
		/// <param name="commit"></param>
		private void Close(bool InFinalizer, bool commit)
		{
			if (!InFinalizer)
			{
				GC.SuppressFinalize(this);
			}
			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (workStream != null)
			{
				workStream.Close();
				workStream = null;
			}
			if (commit)
			{
				if (File.Exists(file))
				{
					string tmpFile = file + ".~stmp";
					File.Move(file, tmpFile);
					try
					{
						File.Move(workFile, file);
						File.Delete(tmpFile);
						workFile = null;	
					}
					catch
					{
						File.Move(tmpFile, file);
						throw;
					}
				}
				else
				{
					File.Move(workFile, file);
					workFile = null;
				}
				FileInfo fi = new FileInfo(file);
				fi.LastWriteTime = node.LastWriteTime;
				fi.CreationTime = node.CreationTime;
				if (oldNode != null)
				{
					// Check if this was a rename.
					string oldPath = oldNode.GetFullPath(collection);
					try
					{
						if (oldPath != file)
							File.Delete(oldPath);
					}
					catch {};
				}
			}

			// We need to delete the temp file if we are the master.
			// On the client leave for a delta sync.
			if (workFile != null)
			{
				if (collection.Role == SyncRoles.Master || (collection.Role == SyncRoles.Slave && File.Exists(file)))
				{
					File.Delete(workFile);
				}
			}

			if (partialFile != null)
				File.Delete(partialFile);
		}

		#endregion
	}

	#endregion

	#region SyncFile

	/// <summary>
	/// Class used to determine the common data between two files.
	/// This is done from a copy of the local file and a map of hash code for the server file.
	/// </summary>
	public abstract class SyncFile
	{
		#region fields

		bool					nameConflict = false;
		protected Node			conflictingNode = null;
		/// <summary>Used to signal to stop upload or downloading the file.</summary>
		protected bool			stopping = false;
		/// <summary>The Collection the file belongs to.</summary>
		protected Collection	collection;
		/// <summary> The node that represents the file.</summary>
		protected BaseFileNode	node;
		/// <summary>The ID of the node.</summary>
		protected string		nodeID;
		/// <summary>The maximun size of a transfer.</summary>
		protected const int		MaxXFerSize = 1024 * 300;
		/// <summary>The name of the actual file.</summary>
		protected string		file;
		/// <summary>The name of the working file.</summary>
		protected string		workFile;
		/// <summary>
		/// The HashMap for this file.
		/// </summary>
		protected HashMap		map;
		/// <summary>The Prefix of the working file.</summary>
		const string			WorkFilePrefix = ".simias.wf.";
		static string			workBinDir = "WorkArea";
		static string			workBin;
		// '/' is left out on purpose because all systems disallow this char.
		public static char[] InvalidChars = {'\\', ':', '*', '?', '\"', '<', '>', '|'};

		/// <summary>Used to publish Sync events.</summary>
		static public			EventPublisher	eventPublisher = new EventPublisher();
		
		#endregion

		#region protected methods.

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		protected SyncFile(Collection collection)
		{
			this.collection = collection;
		}

		/// <summary>
		/// Called to get the name of the file and workFile;
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		/// <param name="sessionID">The unique session ID.</param>
		protected void SetupFileNames(BaseFileNode node, string sessionID)
		{
			this.node = node;
			this.nodeID = node.ID;
			try
			{
				this.file = node.GetFullPath(collection);
			}
			catch
			{
				// If this failed the file name has illegal characters.
				nameConflict = true;
			}
			if (workBin == null)
			{
				workBin = Path.Combine(Configuration.GetConfiguration().StorePath, workBinDir);
				if (!Directory.Exists(workBin))
					Directory.CreateDirectory(workBin);
			}
			this.workFile = Path.Combine(workBin, WorkFilePrefix + node.ID + sessionID);
		}

		/// <summary>
		/// Checks for a name conflict.
		/// </summary>
		/// <returns>True if conflict.</returns>
		protected bool CheckForNameConflict()
		{
			if (!NameConflict)
			{
				string path = node.Properties.GetSingleProperty(PropertyTags.FileSystemPath).Value.ToString();
				ICSList nodeList;
				nodeList = collection.Search(PropertyTags.FileSystemPath, path, SearchOp.Equal);
				foreach (ShallowNode sn in nodeList)
				{
					if (sn.ID != node.ID)
					{
						conflictingNode = collection.GetNodeByID(sn.ID);
						nameConflict = true;
						break;
					}
				}
				// Now make sure we don't have any illegal characters.
				if (!IsNameValid(path))
					nameConflict = true;

				if (nameConflict)
				{
					node = Conflict.CreateNameConflict(collection, node) as BaseFileNode;
					file = Conflict.GetFileConflictPath(collection, node);
					if (conflictingNode != null)
					{
						string cnPath;
						FileNode tmpFn = conflictingNode as FileNode;
						DirNode tmpDn = conflictingNode as DirNode;
						if (tmpFn != null)
						{
							cnPath = tmpFn.GetFullPath(collection);
						}
						else
						{
							cnPath = tmpDn.GetFullPath(collection);
						}
						conflictingNode = Conflict.CreateNameConflict(collection, conflictingNode, cnPath);
						Conflict.LinkConflictingNodes(conflictingNode, node);
					}
				}
			}
			return nameConflict;
		}

		/// <summary>
		/// Called to see if a node with the same name exists.
		/// </summary>
		/// <param name="collection">The collection that contains the node.</param>
		/// <param name="parent">The parent node</param>
		/// <param name="name">The leaf name of the file.</param>
		/// <returns>true if allowed.</returns>
		public static bool DoesNodeExist(Collection collection, DirNode parent, string name)
		{
			string path = parent.Properties.GetSingleProperty(PropertyTags.FileSystemPath).Value.ToString() + "/" + name;
			ICSList nodeList;
			nodeList = collection.Search(PropertyTags.FileSystemPath, path, SearchOp.Equal);
			if (nodeList.Count > 0)
			{
				return true;
			}
			return false;
		}


		/// <summary>
		/// Gets or Sets a NameConflict.
		/// </summary>
		protected bool NameConflict
		{
			get {return nameConflict;}
			set {nameConflict = value;}
		}

		#endregion

		#region public methods.

		/// <summary>
		/// Delete ther file and map file.
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		/// <param name="node">The node that represents the file.</param>
		/// <param name="path">The full path to the file.</param>
		public static void DeleteFile(Collection collection, BaseFileNode node, string path)
		{
			if (File.Exists(path))
				File.Delete(path);
			try
			{
				// Now delete the map file.
				HashMap.Delete(collection, node);
			}
			catch {}
		}

		/// <summary>
		/// Get the file name.
		/// </summary>
		public string Name
		{
			get { return Path.GetFileName(file); }
		}

		/// <summary>
		/// Tells the file to stop and return.
		/// </summary>
		public bool Stop
		{
			set { stopping = value; }
		}

		/// <summary>
		/// Tests if the file name is valid.
		/// </summary>
		/// <param name="name">The file name.</param>
		/// <returns>true if valid.</returns>
		public static bool IsNameValid(string name)
		{
			return name.IndexOfAny(InvalidChars) == -1 ? true : false;
		}

		/// <summary>
		/// Tests if the relative path is valid.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>true if valid</returns>
		public static bool IsRelativePathValid(string path)
		{
			return path.IndexOfAny(InvalidChars) == -1 ? true : false;
		}

		#endregion
	}

	#endregion

	#region SyncSize

	/// <summary>
	/// class to approximate amount of data that is out of sync with master
	/// Note that this is worst-case of data that may need to be sent from
	/// this collection to the master. It does not include data that may need
	/// to be retrieved from the master. It also does not account for
	/// delta-sync algorithms that may reduce what needs to be sent
	/// </summary>
	public class SyncSize
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="col"></param>
		/// <param name="nodeCount"></param>
		/// <param name="maxBytesToSend"></param>
		public static void CalculateSendSize(Collection col, out uint nodeCount, out ulong maxBytesToSend)
		{
			Log.log.Debug("starting to calculate size to send to master for collection {0}", col.Name);

			maxBytesToSend = 0;
			nodeCount = 0;
			SyncClient.GetCountToSync(col.ID, out nodeCount);
		}
	}

	#endregion
}
