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

namespace Simias.Sync
{
	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerFile : SyncFile
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="file">The file that is to be synced either up or down.</param>
		public ServerFile(string file) :
			base(file)
		{
		}

		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		/// <returns></returns>
		public HashEntry[] GetHashMap()
		{
			int				blockCount = (int)(stream.Length / BlockSize) + 1;
			HashEntry[]		list = new HashEntry[blockCount];
			byte[]			buffer = new byte[BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			lock (this)
			{
				// Compute the hash codes.
				stream.Position = 0;
				int i = 0;
				while ((bytesRead = stream.Read(buffer, 0, BlockSize)) != 0)
				{
					HashEntry entry = new HashEntry();
					entry.WeakHash = wh.ComputeHash(buffer, 0, (UInt16)bytesRead);
					entry.StrongHash =  sh.ComputeHash(buffer, 0, bytesRead);
					entry.BlockNumber = currentBlock++;
					list[i++] = entry;
				}
			}
			return list;
		}

		/// <summary>
		/// Read binary data from the file.
		/// </summary>
		/// <param name="buffer">The buffer to place the data into.</param>
		/// <param name="offset">The offset in the file where reading should begin.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns></returns>
		public int Read(byte[] buffer, long offset, int count)
		{
			lock (this)
			{
				stream.Position = offset;
				return stream.Read(buffer, 0, count);
			}
		}

		/// <summary>
		/// Write the binary data to the file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset in the file where the data is to be written.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(byte[] buffer, long offset, int count)
		{
			lock (this)
			{
				outStream.Position = offset;
				outStream.Write(buffer, 0, count);
			}
		}

		/// <summary>
		/// Copyt the data from the original file into the new file.
		/// </summary>
		/// <param name="originalOffset">The offset in the original file to copy from.</param>
		/// <param name="offset">The offset in the file where the data is to be written.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Copy(long originalOffset, long offset, int count)
		{
			int bufferSize = count > BlockSize ? BlockSize : count;
			byte[] buffer = new byte[bufferSize];

			lock (this)
			{
				stream.Position = originalOffset;
				outStream.Position = offset;
				while (count > 0)
				{
					int bytesRead = stream.Read(buffer, 0, bufferSize);
					outStream.Write(buffer, 0, bytesRead);
					count -= bytesRead;
				}
			}
		}
	}

	
	/// <summary>
	/// Class used to determine the common data between two files.
	/// This is done from a copy of the local file and a map of hash code for the server file.
	/// </summary>
	public class SyncFile
	{
		protected string		file;
		protected string		tmpFile;
		protected FileStream	stream;
		protected const int		BlockSize = 4096;
		protected const int		MaxXFerSize = 1024 * 64;
		protected FileStream	outStream;
			
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
		/// Constructor
		/// </summary>
		/// <param name="file">The file to be used.</param>
		public SyncFile(string file)
		{
			this.file = file;
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~SyncFile()
		{
			Close (true);
		}

		/// <summary>
		/// Called to open the file.
		/// </summary>
		/// <param name="tmpFile">The temporary file that is used while the update (upload/download) occures.
		/// Can be null.</param>
		public void Open(string tmpFile)
		{
			this.tmpFile = tmpFile;
			stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);

			if (tmpFile != null)
				outStream = File.Open(tmpFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		}

		private void Close(bool InFinalizer)
		{
			if (!InFinalizer)
				GC.SuppressFinalize(this);

			if (stream != null)
			{
				stream.Close();
				stream = null;
			}
			if (outStream != null)
			{
				outStream.Close();
				outStream = null;
			}
		}

		/// <summary>
		/// Called to close the file and cleanup resources.
		/// </summary>
		public void Close()
		{
			Close (false);
		}
	}

	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	public class HashEntry
	{
		/// <summary>
		/// The Block number that this hash represents. 0 based.
		/// </summary>
		public int		BlockNumber;
		/// <summary>
		/// The Weak or quick hash of this block.
		/// </summary>
		public UInt32	WeakHash;
		/// <summary>
		/// The strong hash of this block.
		/// </summary>
		public byte[]	StrongHash;

		/// <summary>
		/// Override to test for equality of a HashEntry.
		/// </summary>
		/// <param name="obj">The object to compare against.</param>
		/// <returns>True if equal.</returns>
		public override bool Equals(object obj)
		{
			if (this.WeakHash == ((HashEntry)obj).WeakHash)
			{
				for (int i = 0; i < StrongHash.Length; ++i)
				{
					if (StrongHash[i] != ((HashEntry)obj).StrongHash[i])
						return false;
				}
				return true;
			}
			return false;
		}

		///<summary>
		/// Overide needed because Equals is overriden. 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}
	}
}
