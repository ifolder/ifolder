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
	/// <summary>
	/// Class used to determine the common data between two files.
	/// This is done from a copy of the local file and a map of hash code for the server file.
	/// </summary>
	public class SyncFile
	{
		protected Collection	collection;
		protected BaseFileNode	node;
		protected string		nodeID;
		protected string		file;
		protected string		workFile;
		private	FileStream		workStream;
		private FileStream		stream;
		protected SyncDirection	direction;
		protected const int		BlockSize = 4096;
		protected const int		MaxXFerSize = 1024 * 64;
		protected const string ConflictUpdatePrefix = ".simias.cu.";
		protected const string ConflictFilePrefix = ".simias.cf.";
		protected const string WorkFilePrefix = ".simias.wf.";

		protected enum SyncDirection
		{
			IN,
			OUT
		}
	
		/*
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Usage : {0} (file1) (file2)", "FileDelta");
				return;
			}
		
			string file1 = Path.GetFullPath(args[0]);
			string file2 = Path.GetFullPath(args[1]);
			
			// Get the file from the server.
			ServerFile sFile = new ServerFile(file1);
			sFile.Open(null);
			ClientFile cFile = new ClientFile(file2, sFile.GetHashMap());
			string DownFile = Path.Combine(Path.GetDirectoryName(file1), "Download");
			File.Delete(DownFile);
			cFile.Open(DownFile);
			long[] downloadMap = cFile.GetDownloadFileMap();
			Console.WriteLine("*****************************************");
			Console.WriteLine("Download Changes");
			Console.WriteLine(cFile.ReportDownloadDiffs(downloadMap));
			Console.WriteLine("*****************************************");
			cFile.DownLoadFile(downloadMap, sFile);
			sFile.Close();
			cFile.Close();


			// Push the changes to the server.
			sFile = new ServerFile(file1);
			string UpFile = Path.Combine(Path.GetDirectoryName(file1), "Upload");
			File.Delete(UpFile);
			sFile.Open(UpFile);
			cFile = new ClientFile(file2, sFile.GetHashMap());
			cFile.Open(null);
			ArrayList uploadMap = cFile.GetUploadFileMap();
			Console.WriteLine("*****************************************");
			Console.WriteLine("Upload Changes");
			Console.WriteLine(cFile.ReportUploadDiffs(uploadMap));
			Console.WriteLine("*****************************************");
			cFile.UploadFile(uploadMap, sFile);
			sFile.Close();
			cFile.Close();
		}
		*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="collection">The collection that the node belongs to.</param>
		/// <param name="direction">The direction of the sync.</param>
		protected SyncFile(Collection collection, SyncDirection direction)
		{
			this.collection = collection;
			this.direction = direction;
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~SyncFile()
		{
			Close (true, false);
		}

		/// <summary>
		/// Reads data into the buffer.
		/// </summary>
		/// <param name="buffer">The buffer to read into.</param>
		/// <param name="offset">The offset in the buffer to read into.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(byte[] buffer, int offset, int count)
		{
			if (direction == SyncDirection.OUT)
			{
				return workStream.Read(buffer, offset, count);
			}
			else
			{
				return stream.Read(buffer, offset, count);
			}
		}

		/// <summary>
		/// Writes data from buffer into file.
		/// </summary>
		/// <param name="buffer">The buffer containing the data.</param>
		/// <param name="offset">The offset in the buffer to write from.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(byte[] buffer, int offset, int count)
		{
			if (direction == SyncDirection.OUT)
			{
				throw new SimiasException("Invalid operation");
			}
			else
			{
				workStream.Write(buffer, offset, count);
			}
		}

		/// <summary>
		/// Copy data from the original file to the workfile.
		/// </summary>
		/// <param name="offset">The offset in the original file.</param>
		/// <param name="count">The nuber of bytes to copy.</param>
		/// <returns></returns>
		public void Copy(long offset, int count)
		{
			if (direction == SyncDirection.OUT)
			{
				throw new SimiasException("Invalid operation");
			}
			else
			{
				byte[] buffer = new byte[count];
				stream.Position = offset;
				count = stream.Read(buffer, 0, count);
				workStream.Write(buffer, 0, count);
			}
		}
		

		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long ReadPosition
		{
			get 
			{ 
				if (direction == SyncDirection.OUT)
					return workStream.Position; 
				else
					return stream.Position;
			}
			set 
			{ 
				if (direction == SyncDirection.OUT)
					workStream.Position = value; 
				else
					stream.Position = value;
			}
		}

		/// <summary>
		/// Gets or Sets the file position.
		/// </summary>
		public long WritePosition
		{
			get 
			{ 
				if (direction == SyncDirection.OUT)
					throw new SimiasException("Invalid operation");
				else
                    return workStream.Position; 
			}
			set 
			{ 
				if (direction == SyncDirection.OUT)
					throw new SimiasException("Invalid operation");
				else
					workStream.Position = value; 
			}
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		public long Length
		{
			get
			{
				if (direction == SyncDirection.OUT)
					return workStream.Length;
				else
					return stream.Length;
			}
		}

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="node">The node that represents the file.</param>
		protected void Open(BaseFileNode node)
		{
			file = node.GetFullPath(collection);
			workFile = Path.Combine(Path.GetDirectoryName(file), WorkFilePrefix + Path.GetFileName(file));
			if (direction == SyncDirection.OUT)
			{
				// This file is being pushed make a copy to work from.
				File.Copy(file, workFile);
				workStream = File.Open(workFile, FileMode.Open, FileAccess.Read);
			}
			else
			{
				// Open the file so that it cannot be modified.
				stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
				workStream = File.Open(workFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
			}
			File.SetAttributes(workFile, FileAttributes.Hidden);
		}

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
				if (direction == SyncDirection.IN)
				{
					if (commit)
					{
						File.Copy(workFile, file, true);
					}
				}
				// We need to delete the temp file.
				File.Delete(workFile);
			}
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		protected void Close(bool commit)
		{
			Close (false, commit);
		}
	}
}
