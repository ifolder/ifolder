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
	/// The types of Segment descriptors.
	/// </summary>
	[Serializable]
	public enum SegmentType
	{
		/// <summary>
		/// Block Segment descriptor.
		/// </summary>
		Block,
		/// <summary>
		/// Offset Segment descriptor
		/// </summary>
		Offset
	}

	/// <summary>
	/// The base class for an upload file segment.
	/// </summary>
	[Serializable]
	public class FileSegment
	{
		/// <summary>
		/// This is the type of Segment this instance describes.
		/// It can either be Block or Data.
		/// </summary>
		public SegmentType Type;
	}


	/// <summary>
	/// Describes a file segment using a block from the remote file. Can be a
	/// Range of block.
	/// </summary>
	[Serializable]
	class BlockSegment : FileSegment
	{
		/// <summary>
		/// This is the start block for the unchanged segment of data.
		/// </summary>
		public int				StartBlock;
		/// <summary>
		/// This is the end block for the unchanged segment of data.
		/// </summary>
		public int				EndBlock;
	}

	/// <summary>
	/// Descibes a file segment using the offset and the length from the local file.
	/// </summary>
	[Serializable]
	class OffsetSegment : FileSegment
	{
		/// <summary>
		/// The length of the segment.
		/// </summary>
		public int		Length;
		/// <summary>
		/// The offset in the local file of the segment.
		/// </summary>
		public long		Offset;
	}

	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerFile : FileDelta
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
	/// Used to find the deltas between
	/// the local file and the server file.
	/// </summary>
	public class ClientFile : FileDelta
	{
		StrongWeakHashtable		table = new StrongWeakHashtable();
		HashEntry[]				serverHashMap;
		
		/// <summary>
		/// Contructs a ClientFile object that can be used to find the deltas between
		/// the local file and the server file.
		/// </summary>
		/// <param name="clientFile">The local file.</param>
		/// <param name="serverHashMap">The hash map of ther server file.</param>
		public ClientFile(string clientFile, HashEntry[] serverHashMap) :
			base(clientFile)
		{
			this.serverHashMap = serverHashMap;
		}

		/// <summary>
		/// Gets an ArrayList of all the changes that need to be made to the server file
		/// to make the files identical.
		/// </summary>
		/// <returns></returns>
		public ArrayList GetUploadFileMap()
		{
			table.Clear();
			table.Add(serverHashMap);
			ArrayList fileMap = new ArrayList();

			int				bytesRead = BlockSize * 16;
			byte[]			buffer = new byte[BlockSize * 16];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			BlockSegment	lastBS = null;
			int				startByte = 0;
			int				endByte = 0;
			int				endOfLastMatch = 0;
			byte			dropByte = 0;
			
			stream.Position = 0;		
			while (bytesRead != 0)
			{
				bytesRead = stream.Read(buffer, readOffset, bytesRead - readOffset);
				if (bytesRead == 0)
					break;

				bytesRead = bytesRead + readOffset;
				
				if (bytesRead >= BlockSize)
				{
					endByte = startByte + BlockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, BlockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(BlockSize, dropByte, buffer[endByte]);
						ArrayList entryList = table[entry.WeakHash];
						if (entryList != null)
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, BlockSize);
							int eIndex = entryList.IndexOf(entry);
							if (eIndex != -1)
							{
								HashEntry match = (HashEntry)entryList[eIndex];
								// We found a match save the data that does not match;
								if (endOfLastMatch != startByte)
								{
									OffsetSegment seg = new OffsetSegment();
									seg.Type = SegmentType.Offset;
									seg.Length = startByte - endOfLastMatch;
									seg.Offset = stream.Position - bytesRead + endOfLastMatch;
									fileMap.Add(seg);
								}
								startByte = endByte + 1;
								endByte = startByte + BlockSize - 1;
								endOfLastMatch = startByte;
								recomputeWeakHash = true;

								if (lastBS != null && lastBS.EndBlock == match.BlockNumber -1)
								{
									lastBS.EndBlock = match.BlockNumber;
								}
								else
								{
									// Save the matched block.
									lastBS = new BlockSegment();
									lastBS.Type = SegmentType.Block;
									lastBS.StartBlock = match.BlockNumber;
									lastBS.EndBlock = match.BlockNumber;
									fileMap.Add(lastBS);
								}
								continue;
							}
						}
						dropByte = buffer[startByte];
						++startByte;
						++endByte;
					}

					// We need to copy any data that has not been saved.
					if (endOfLastMatch == 0)
					{
						// We don't want to send to large of a buffer. Create a DiffRecord
						// for the data in the buffer.
						OffsetSegment seg = new OffsetSegment();
						seg.Type = SegmentType.Offset;
						seg.Length = startByte - endOfLastMatch;
						seg.Offset = stream.Position - bytesRead + endOfLastMatch;
						fileMap.Add(seg);
						endOfLastMatch = startByte;
					}
					readOffset = bytesRead - endOfLastMatch;
					Array.Copy(buffer, endOfLastMatch, buffer, 0, readOffset);
					startByte = startByte - endOfLastMatch; //0;
					endOfLastMatch = 0;
					endByte = readOffset - 1;
				}
				else
				{
					endByte = bytesRead - 1;
					break;
				}
			}

			// Get the remaining changes.
			if (endOfLastMatch != endByte)//== 0 && endByte != 0)
			{
				OffsetSegment seg = new OffsetSegment();
				seg.Type = SegmentType.Offset;
				seg.Length = endByte - endOfLastMatch + 1;
				seg.Offset = stream.Position - seg.Length;
				fileMap.Add(seg);
			}

			return fileMap;
		}

		/// <summary>
		/// Compute the Blocks that need to be downloaded from the server. This builds
		/// an array of offsets where the blocks need to be placed in the local file.
		/// The block is represented by the index of the array.
		/// </summary>
		/// <returns>The file map.</returns>
		public long[] GetDownloadFileMap()
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			table.Clear();
			table.Add(serverHashMap);
			long[] fileMap = new long[serverHashMap.Length];

			int				bytesRead = BlockSize * 16;
			byte[]			buffer = new byte[BlockSize * 16];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			int				startByte = 0;
			int				endByte = 0;
			byte			dropByte = 0;

			// Set the file map to not match anything.
			for (int i = 0; i < fileMap.Length; ++ i)
			{
				fileMap[i] = -1;
			}

			stream.Position = 0;					
			while (bytesRead != 0)
			{
				bytesRead = stream.Read(buffer, readOffset, bytesRead - readOffset);
				if (bytesRead == 0)
					break;
				bytesRead = bytesRead == 0 ? bytesRead : bytesRead + readOffset;
				
				if (bytesRead >= BlockSize)
				{
					endByte = startByte + BlockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, BlockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(BlockSize, dropByte, buffer[endByte]);
						ArrayList entryList = table[entry.WeakHash];
						if (entryList != null)
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, BlockSize);
							int eIndex = entryList.IndexOf(entry);
							if (eIndex != -1)
							{
								HashEntry match = (HashEntry)entryList[eIndex];
								// We found a match save the match;
								fileMap[match.BlockNumber] = stream.Position - bytesRead + startByte;
							}
						}
						dropByte = buffer[startByte];
						++startByte;
						++endByte;
					}

					readOffset = bytesRead - startByte;
					Array.Copy(buffer, startByte, buffer, 0, readOffset);
					startByte = 0;
				}
				else
				{
					break;
				}
			}
			return fileMap;
		}
	}

	/// <summary>
	/// Class used to determine the common data between two files.
	/// This is done from a copy of the local file and a map of hash code for the server file.
	/// </summary>
	public class FileDelta
	{
		protected string		file;
		protected string		tmpFile;
		protected FileStream	stream;
		protected const int		BlockSize = 4096;
		protected const int		MaxXFerSize = 1024 * 64;
		protected FileStream	outStream;
				
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
			Console.WriteLine("Download Changes");
			ReportDownloadDiffs(downloadMap);
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
			Console.WriteLine("Upload Changes");
			cFile.ReportUploadDiffs(uploadMap);
			cFile.UploadFile(uploadMap, sFile);
			sFile.Close();
			cFile.Close();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="file">The file to be used.</param>
		public FileDelta(string file)
		{
			this.file = file;
		}

		/// <summary>
		/// Finalizer.
		/// </summary>
		~FileDelta()
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

		/// <summary>
		/// Writes the new file based on The fileMap.
		/// </summary>
		/// <param name="changes"></param>
		public void DownLoadFile(long[] fileMap, ServerFile server)
		{
			lock (this)
			{
				byte[] buffer = new byte[BlockSize];
				outStream.Position = 0;
				
				for (int i = 0; i < fileMap.Length; ++i)
				{
					if (fileMap[i] != -1)
					{
						stream.Position = fileMap[i];
						int bytesRead = stream.Read(buffer, 0, BlockSize);
						outStream.Write(buffer, 0, bytesRead);
					}
					else
					{
						// We need to get this block from the server.
						// Check if we have more consecutive blocks to get from the server.
						int readBufferSize = BlockSize;
						int offset = i * BlockSize;
						for (int j = i + 1; j < fileMap.Length; ++j)
						{
							if (fileMap[j] == -1)
							{
								// We need to get the next segment.
								// Add the segment size to this read and skip over the segment.
								readBufferSize += BlockSize;
								i++;
								if (readBufferSize >= MaxXFerSize)
								{
									// We don't allow bigger XFers than this get out.
									break;
								}
							}
							else
							{
								// The next segment is already local.
								break;
							}
						}

						byte[] readBuffer;
						if (readBufferSize != BlockSize)
							readBuffer = new byte[readBufferSize];
						else
							readBuffer = buffer;

						int bytesRead = server.Read(readBuffer, offset, readBufferSize);
						outStream.Write(readBuffer, 0, bytesRead);
					}
				}
			}
		}

		/// <summary>
		/// Writes the new file based on The fileMap array.
		/// </summary>
		/// <param name="fileMap">An array of FileSegments That describes how the server file relates to 
		/// the local file.</param>
		public void UploadFile(ArrayList fileMap, ServerFile sFile)
		{
			byte[] buffer = new byte[BlockSize];
			long offset = 0;
			foreach(FileSegment segment in fileMap)
			{
				switch (segment.Type)
				{
					case SegmentType.Block:
						BlockSegment bs = (BlockSegment)segment;
						stream.Position = bs.StartBlock * BlockSize;
						int bytesToWrite = (bs.EndBlock - bs.StartBlock + 1) * BlockSize;
						sFile.Copy(bs.StartBlock * BlockSize, offset, bytesToWrite);
						offset += bytesToWrite;
						break;
					case SegmentType.Offset:
						// Write the bytes to the output stream.
						OffsetSegment seg = (OffsetSegment)segment;
						byte[] dataBuffer = new byte[seg.Length];
						stream.Position = seg.Offset;
						int bytesRead = stream.Read(dataBuffer, 0, seg.Length);
						sFile.Write(dataBuffer, offset, seg.Length);
						//outStream.Write(ds.Data, 0, ds.length);
						offset += seg.Length;
						break;
				}
			}
		}

		private void ReportUploadDiffs(ArrayList segments)
		{
			Console.WriteLine("*****************************************");
			foreach (FileSegment segment in segments)
			{
				switch (segment.Type)
				{
					case SegmentType.Block:
						BlockSegment bs = (BlockSegment)segment;
						Console.WriteLine("Found Match Block {0} to Block {1}", bs.StartBlock, bs.EndBlock);
						break;
					case SegmentType.Offset:
						OffsetSegment seg = (OffsetSegment)segment;
						Console.WriteLine("Found change size = {0}", seg.Length);
						break;
				}
			}
			Console.WriteLine("*****************************************");
		}

		private static void ReportDownloadDiffs(long[] fileMap)
		{
			Console.WriteLine("*****************************************");
			int startBlock = -1;
			int endBlock = 0;
			for (int i = 0; i < fileMap.Length; ++i)
			{
				if (fileMap[i] == -1)
				{
					if (startBlock == -1)
					{
						startBlock = i;
					}
					endBlock = i;
				}
				else
				{
					if (startBlock != -1)
						Console.WriteLine("Found Missing Block {0} to Block {1}", startBlock, endBlock);
					startBlock = -1;
				}
			}
			if (startBlock != -1)
				Console.WriteLine("Found Missing Block {0} to Block {1}", startBlock, endBlock);
			Console.WriteLine("*****************************************");
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

	/// <summary>
	/// Class to compute a weak hash for a block of data.
	/// The hash can be calculated quickly, and can be rolled.
	/// That is recaculated as the window is slid one byte at a time through a buffer.
	/// </summary>
	class WeakHash
	{
		UInt16 a;
		UInt16 b;

		internal UInt32 ComputeHash(byte[] buffer, int offset, UInt16 count)
		{
			a = 0;
			b = 0;
			UInt16 l = (UInt16)(count);
			
			for (int i = offset; i < offset + count; ++i)
			{
				a += buffer[i];
				b += (UInt16)(l-- * buffer[i]);
			}

			return a + (UInt32)(b * 0x10000);
		}

		internal UInt32 RollHash(int count, byte dropValue, byte addValue)
		{
			a = (UInt16)(a - dropValue + addValue);
			b = (UInt16)(b - ((count) * dropValue) + a);
			return a + (UInt32)(b * 0x10000);
		}
	}

	/// <summary>
	/// Class to compute a strong Hash for a block of data.
	/// </summary>
	class StrongHash
	{
		MD5		md5 = new MD5CryptoServiceProvider();
			
		/// <summary>
		/// Computes an MD5 hash of the data block passed in.
		/// </summary>
		/// <param name="buffer">The data to hash.</param>
		/// <param name="offset">The offset in the byte array to start hashing.</param>
		/// <param name="count">The number of bytes to include in the hash.</param>
		/// <returns></returns>
		internal byte[] ComputeHash(byte[] buffer, int offset, int count)
		{
			return md5.ComputeHash(buffer, offset, count);
		}
	}

	/// <summary>
	/// Hashtable class that contains a strong and a weak hash code for the value.
	/// Elements are stored in an ArrayList using the weak hash as the key.
	/// </summary>
	class StrongWeakHashtable
	{
		Hashtable table = new Hashtable();

		/// <summary>
		/// Add a new HashEntry to the table.
		/// </summary>
		/// <param name="entry">The entry to add.</param>
		public void Add(HashEntry entry)
		{
			lock (this)
			{
				ArrayList entryArray;
				entryArray = (ArrayList)table[entry.WeakHash];
				if (entryArray == null)
				{
					entryArray = new ArrayList();
					table.Add(entry.WeakHash, entryArray);
				}
				entryArray.Add(entry);
			}
		}

		/// <summary>
		/// Add the List of entries to the table.
		/// </summary>
		/// <param name="entryList">The list of entries to add.</param>
		public void Add(HashEntry[] entryList)
		{
			foreach (HashEntry entry in entryList)
			{
				Add(entry);
			}
		}

		/// <summary>
		/// Indexer for the table.  The index is the weakHash.
		/// Returns the ArrayList of entries that all have this weak hash.
		/// </summary>
		public ArrayList this[UInt32 weakHash]
		{
			get
			{
				return (ArrayList)table[weakHash];
			}
		}

		/// <summary>
		/// Clears all elements from the table.
		/// </summary>
		public void Clear()
		{
			table.Clear();
		}
	}
}
