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
using System.Collections;
using System.Security.Cryptography;
using Simias.Storage;

namespace Simias.Sync
{
	#region OutFile

	/// <summary>
	/// Class to handle file operations for a file to be synced out.
	/// </summary>
	public abstract class OutFile : SyncFile
	{
		#region fields

		FileStream	workStream;

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
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The offset in the buffer to read into.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(byte[] buffer, int offset, int count)
		{
			return workStream.Read(buffer, offset, count);
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
		protected void Open(BaseFileNode node)
		{
			SetupFileNames(node);
			// This file is being pushed make a copy to work from.
			File.Copy(file, workFile, true);
			workStream = File.Open(workFile, FileMode.Open, FileAccess.Read);
			File.SetAttributes(workFile, File.GetAttributes(workFile) | FileAttributes.Hidden);
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected void Close()
		{
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
		FileStream	workStream;
		/// <summary>Stream to the Original file.</summary>
		FileStream	stream;
		/// <summary>The Old Node if it exists.</summary>
		BaseFileNode	oldNode;

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
			return stream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Writes data from buffer into file.
		/// </summary>
		/// <param name="buffer">The buffer containing the data.</param>
		/// <param name="offset">The offset in the buffer to write from.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(byte[] buffer, int offset, int count)
		{
			workStream.Write(buffer, offset, count);
		}

		/// <summary>
		/// Copy data from the original file to the workfile.
		/// </summary>
		/// <param name="offset">The offset in the original file.</param>
		/// <param name="count">The nuber of bytes to copy.</param>
		/// <returns></returns>
		public void Copy(long offset, int count)
		{
			byte[] buffer = new byte[count];
			stream.Position = offset;
			count = stream.Read(buffer, 0, count);
			workStream.Write(buffer, 0, count);
		}
		

		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long ReadPosition
		{
			get { return stream.Position; }
			set { stream.Position = value; }
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
			get { return stream.Length; }
		}

		#endregion

		#region protected methods.

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		protected void Open(BaseFileNode node)
		{
			this.SetupFileNames(node);
			// Open the file so that it cannot be modified.
			oldNode = collection.GetNodeByID(node.ID) as BaseFileNode;
			stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
			workStream = File.Open(workFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
			File.SetAttributes(workFile, File.GetAttributes(workFile) | FileAttributes.Hidden);
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected void Close(bool commit)
		{
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
				GC.SuppressFinalize(this);

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
				File.Copy(workFile, file, true);
				FileInfo fi = new FileInfo(file);
				fi.Attributes = fi.Attributes & ~FileAttributes.Hidden;
				fi.LastWriteTime = node.LastWriteTime;
				fi.CreationTime = node.CreationTime;
			}
			// We need to delete the temp file.
			File.Delete(workFile);
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

		/// <summary>The Collection the file belongs to.</summary>
		protected Collection	collection;
		/// <summary> The node that represents the file.</summary>
		protected BaseFileNode	node;
		/// <summary>The ID of the node.</summary>
		protected string		nodeID;
		/// <summary>The size of the Blocks that are hashed.</summary>
		protected const int		BlockSize = 4096;
		/// <summary>The maximun size of a transfer.</summary>
		protected const int		MaxXFerSize = 1024 * 64;
		/// <summary>The name of the actual file.</summary>
		protected string		file;
		/// <summary>The name of the working file.</summary>
		protected string		workFile;
		/// <summary>The Prefix of the working file.</summary>
		const string			WorkFilePrefix = ".simias.wf.";

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
		protected void SetupFileNames(BaseFileNode node)
		{
			this.node = node;
			this.nodeID = node.ID;
			this.file = node.GetFullPath(collection);
			this.workFile = Path.Combine(Path.GetDirectoryName(file), WorkFilePrefix + node.ID);
		}

		#endregion
	}

	#endregion
}
