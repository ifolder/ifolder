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

namespace FileDelta
{
	/// <summary>
	/// The types of FileSegments that are defined.
	/// </summary>
	[Serializable]
	public enum FileSegmentType
	{
		Block,
		Data
	}

	[Serializable]
	public class FileSegment
	{
		/// <summary>
		/// This is the type of Segment this instance describes.
		/// It can either be Block or Data.
		/// </summary>
		public FileSegmentType Type;
	}


	/// <summary>
	/// Class to describe Either a Range of Blocks that match the file on the server.
	/// or a block of data to be added to the file.  An ArrayList
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

	[Serializable]
	class DataSegment : FileSegment
	{
		public int		length;
		public byte[]	Data;
	}

	class ServerFile : FileDelta
	{
		FileStream		stream;
		
		public ServerFile(string file, bool writeAccess) :
			base(file)
		{
			if (writeAccess)
				stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			else
				stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			
		}

		public ArrayList GetHashMap()
		{
			FileInfo		fi = new FileInfo(file);
			int				blockCount = (int)(fi.Length / BlockSize) + 1;
			ArrayList		list = new ArrayList(blockCount);
			byte[]			buffer = new byte[BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			lock (this)
			{
				// Compute the hash codes.
				stream.Position = 0;
				while ((bytesRead = stream.Read(buffer, 0, BlockSize)) != 0)
				{
					HashEntry entry = new HashEntry();
					entry.WeakHash = wh.ComputeHash(buffer, 0, (UInt16)bytesRead);
					entry.StrongHash =  sh.ComputeHash(buffer, 0, bytesRead);
					entry.BlockNumber = currentBlock++;
					list.Add(entry);
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
				stream.Position = offset;
				stream.Write(buffer, 0, count);
			}
		}

		/// <summary>
		/// Writes the new file based on The DiffRecord array.
		/// </summary>
		/// <param name="changes"></param>
		public void WriteChanges(ArrayList fileMap, string outFile)
		{
			byte[] buffer = new byte[BlockSize];
			outStream = new BinaryWriter(File.Open(outFile, FileMode.CreateNew, FileAccess.Write, FileShare.None));
			foreach(FileSegment segment in fileMap)
			{
				switch (segment.Type)
				{
					case FileSegmentType.Block:
						BlockSegment bs = (BlockSegment)segment;
						stream.Position = bs.StartBlock * BlockSize;
						for (int i = bs.StartBlock; i <= bs.EndBlock; ++i)
						{
							int bytesRead = stream.Read(buffer, 0, BlockSize);
							outStream.Write(buffer, 0, BlockSize);
						}
						break;
					case FileSegmentType.Data:
						// Write the bytes to the output stream.
						DataSegment ds = (DataSegment)segment;
						outStream.Write(ds.Data, 0, ds.length);
						break;
				}
			}
			outStream.Close();
		}
	}

	class ClientFile : FileDelta
	{
		StrongWeakHashtable table = new StrongWeakHashtable();
		ArrayList	serverHashMap;

		public ClientFile(string clientFile, ArrayList serverHashMap) :
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

			BinaryReader	reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
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
								
			while (bytesRead != 0)
			{
				bytesRead = reader.Read(buffer, readOffset, bytesRead - readOffset);
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
								// We found a match save the data that does not match;
								if (endOfLastMatch != startByte)
								{
									DataSegment ds = new DataSegment();
									ds.Type = FileSegmentType.Data;
									ds.length = startByte - endOfLastMatch;
									ds.Data = new byte[ds.length];
									Array.Copy(buffer, endOfLastMatch, ds.Data, 0, ds.length);
									fileMap.Add(ds);
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
									lastBS.Type = FileSegmentType.Block;
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
						// We don't want to send to large of a buffer create a DiffRecord
						// for the data in the buffer.
						DataSegment ds = new DataSegment();
						ds.Type = FileSegmentType.Data;
						ds.length = startByte - endOfLastMatch;
						ds.Data = new byte[ds.length];
						Array.Copy(buffer, endOfLastMatch, ds.Data, 0, ds.length);
						fileMap.Add(ds);
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
				DataSegment ds = new DataSegment();
				ds.Type = FileSegmentType.Data;
				ds.length = endByte - endOfLastMatch + 1;
				ds.Data = new byte[ds.length];
				Array.Copy(buffer, endOfLastMatch, ds.Data, 0, ds.length);
				fileMap.Add(ds);
			}

			reader.Close();
			return fileMap;
		}

		public long[] GetDownloadFileMap()
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			//table.Clear();
			//table.Add(serverHashMap);
			long[] fileMap = new long[serverHashMap.Count];

			BinaryReader	reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
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
								
			while (bytesRead != 0)
			{
				bytesRead = reader.Read(buffer, readOffset, bytesRead - readOffset);
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
								fileMap[match.BlockNumber] = reader.BaseStream.Position - bytesRead + startByte;

								startByte = endByte + 1;
								endByte = startByte + BlockSize - 1;
								recomputeWeakHash = true;
								continue;
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

			
			reader.Close();
			return fileMap;
		}

		/// <summary>
		/// Writes the new file based on The fileMap.
		/// </summary>
		/// <param name="changes"></param>
		public void WriteChanges(long[] fileMap, string outFile, ServerFile server)
		{
			byte[] buffer = new byte[BlockSize];
			inStream = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read));
			outStream = new BinaryWriter(File.Open(outFile, FileMode.CreateNew, FileAccess.Write, FileShare.None));

			for (int i = 0; i < fileMap.Length; ++i)
			{
				if (fileMap[i] != -1)
				{
					outStream.BaseStream.Position = i * BlockSize;
					inStream.BaseStream.Position = fileMap[i];
					int bytesRead = inStream.Read(buffer, 0, BlockSize);
					outStream.Write(buffer, 0, bytesRead);
				}
				else
				{
					// We need to get this block from the server.
					outStream.BaseStream.Position = i * BlockSize;
					int bytesRead = server.Read(buffer, i * BlockSize, BlockSize);
					outStream.Write(buffer, 0, bytesRead);
				}
			}
			inStream.Close();
			outStream.Close();
		}

		//Close();
	}

	/// <summary>
	/// Class used to determine the common data between two files.
	/// This is done from a copy of the local file and a map of hash code for the server file.
	/// </summary>
	class FileDelta
	{
		protected string		file;
		protected const int		BlockSize = 4096;
		protected BinaryReader	inStream;
		protected BinaryWriter	outStream;

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
			
			ServerFile sFile = new ServerFile(file1, true);
			ArrayList fileMap = sFile.GetHashMap();
			ClientFile cFile = new ClientFile(file2, fileMap);
			ArrayList uploadMap = cFile.GetUploadFileMap();
			long[] downloadMap = cFile.GetDownloadFileMap();

			string UpFile = Path.Combine(Path.GetDirectoryName(file1), "Upload");
			string DownFile = Path.Combine(Path.GetDirectoryName(file1), "Download");

			File.Delete(UpFile);
			File.Delete(DownFile);
			
			Console.WriteLine("Upload Changes");
			ReportUploadDiffs(uploadMap);
			sFile.WriteChanges(uploadMap, UpFile);
			Console.WriteLine("Download Changes");
			ReportDownloadDiffs(downloadMap);
			cFile.WriteChanges(downloadMap, DownFile, sFile);
		}

		public FileDelta(string file)
		{
			this.file = file;
		}

		private static void ReportUploadDiffs(ArrayList segments)
		{
			Console.WriteLine("*****************************************");
			foreach (FileSegment segment in segments)
			{
				switch (segment.Type)
				{
					case FileSegmentType.Block:
						BlockSegment bs = (BlockSegment)segment;
						Console.WriteLine("Found Match Block {0} to Block {1}", bs.StartBlock, bs.EndBlock);
						break;
					case FileSegmentType.Data:
						DataSegment ds = (DataSegment)segment;
						Console.WriteLine("Found change size = {0}", ds.length);
						if (ds.length < 80)
						{
							foreach(byte b in ds.Data)
							{
								char c = (char)b;
								Console.Write(c);
							}
							Console.WriteLine();
						}
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
			Console.WriteLine("*****************************************");
		}
	}

	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	class HashEntry
	{
		public int		BlockNumber;
		public UInt32	WeakHash;
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
			//return base.GetHashCode ();
			return (StrongHash.GetHashCode());
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
		public void Add(ArrayList entryList)
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
