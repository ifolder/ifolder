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
 * Thanks to Andrew Tridgell for the rsync algorythm.
 * http://samba.anu.edu.au/rsync/tech_report/
 ***********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Threading;
using Simias.Storage;

namespace Simias.Sync.Delta
{
	#region HashData

	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	public class HashData
	{
		/// <summary>
		/// The serialized size of the instance.
		/// </summary>
		public static int InstanceSize = 4 + 4 + 16;
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
		/// Constructs a HashData Object.
		/// </summary>
		/// <param name="blockNumber"></param>
		/// <param name="weakHash"></param>
		/// <param name="strongHash"></param>
		public HashData(int blockNumber, UInt32 weakHash, byte[] strongHash)
		{
			this.BlockNumber = blockNumber;
			this.WeakHash = weakHash;
			this.StrongHash = strongHash;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public HashData(BinaryReader reader)
		{
			BlockNumber = reader.ReadInt32();
			WeakHash = reader.ReadUInt32();
			StrongHash = reader.ReadBytes(16);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(BlockNumber);
			writer.Write(WeakHash);
			writer.Write(StrongHash);
		}
	}

	#endregion

	#region HashMap

	/// <summary>
	/// Used to Write and Get HashMaps of a stream.
	/// </summary>
	public class HashMap
	{
		const string			MapFilePrefix = ".simias.map.";
		Collection				collection;
		BaseFileNode			node;
		string					file;
		static int				MaxThreadCount = 20;
		static int				ThreadCount = 0;
		static Queue			mapQ = new Queue();
		static AutoResetEvent	queueEvent = new AutoResetEvent(false);
		delegate void	HashMapDelegate();

		internal struct HashFileHeader
		{
			static byte[]	signature = {(byte)'#', (byte)'M', (byte)'a', (byte)'P', (byte)'f', (byte)'I', (byte)'l', (byte)'e'};
			static int		headerSize = 12;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="reader"></param>
			/// <param name="blockSize"></param>
			/// <param name="entryCount"></param>
			/// <returns></returns>
			internal static bool ReadHeader(BinaryReader reader, out int blockSize, out int entryCount)
			{
				byte[] sig = reader.ReadBytes(8);
				blockSize = reader.ReadInt32();
				entryCount = 0;
				if (sig.Length == signature.Length)
				{
					for (int i= 0; i < sig.Length; ++i)
					{
						if (sig[i] != signature[i])
							return false;
					}
					entryCount = (int)((reader.BaseStream.Length - headerSize)/ HashData.InstanceSize);
					return true;
				}
				return false;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="writer"></param>
			/// <param name="blockSize"></param>
			internal static void WriteHeader(BinaryWriter writer, int blockSize)
			{
				writer.Write(signature);
				writer.Write(blockSize);
			}
		}

		// We want to keep Map file size small enough that it will work over a 56Kb/Sec modem.
		// Lets try to get the file in 60 seconds.
		// In 60 seconds we can transfer 56k / 8 = 430,080 bytes of data.
		// In 60 seconds we can transfer 17920 blocks rount to 18000
		// We are assuming that larger files are less likely to change.
		static int maxBlocks = 18000;

		internal HashMap(Collection collection, BaseFileNode node)
		{
			this.collection = collection;
			this.node = node;
			file = Path.Combine(collection.ManagedPath, MapFilePrefix + node.ID);
		}

		/// <summary>
		/// 
		/// </summary>
		private static void HashMapThread()
		{
			// Now see if we have any work queued.
			while (true)
			{
				// If we have had no work for 5 min exit thread.
				bool timedOut = !queueEvent.WaitOne(5 * 60 * 1000, false);
				try
				{
					while (true)
					{
						HashMapDelegate hmd;
						lock (mapQ)
						{
							hmd = mapQ.Count > 0 ? mapQ.Dequeue() as HashMapDelegate : null;
							if (hmd == null)
							{
								if (timedOut)
								{
									// Exit the thread.
									--ThreadCount;
									return;
								}
								break;
							}
						}
						try { hmd(); }
						catch { /* Don't let the thread go away.*/ }
					}
				}
				catch{}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void CreateHashMapFile()
		{
			FileStream mapSrcStream = File.Open(node.GetFullPath(collection), FileMode.Open, FileAccess.Read, FileShare.Read);	
			int blockSize = CalculateBlockSize(mapSrcStream.Length);
			try
			{
				string mapFile = file;
				string tmpMapFile = mapFile + ".tmp";
				// Copy the current file to a tmp name.
				if (File.Exists(mapFile))
					File.Move(mapFile, tmpMapFile);

				BinaryWriter writer = new BinaryWriter( File.OpenWrite(tmpMapFile));

				// Write the header.
				HashFileHeader.WriteHeader(writer, blockSize);
				try
				{
					mapSrcStream.Position = 0;
					HashMap.SerializeHashMap(mapSrcStream, writer, blockSize);
					writer.Close();
					File.Move(tmpMapFile, mapFile);
					File.SetCreationTime(mapFile, node.CreationTime);
					File.SetLastWriteTime(mapFile, node.LastWriteTime);
				}
				catch (Exception ex)
				{
					writer.Close();
					writer = null;
					File.Delete(mapFile);
					if (File.Exists(tmpMapFile))
						File.Move(tmpMapFile, mapFile);
					throw ex;
				}
				finally
				{
					if (File.Exists(tmpMapFile))
						File.Delete(tmpMapFile);
				}
			}
			finally
			{
				// Close the file.
				mapSrcStream.Close();
			}
		}

		/// <summary>
		/// Calculate the block size to use for hash blocks
		/// </summary>
		/// <param name="streamSize">The size of the file.</param>
		/// <returns>The blockSize</returns>
		private static int CalculateBlockSize(long streamSize)
		{
			long size = streamSize / HashMap.maxBlocks;
			if (size < 0x1000)
				return 0x1000;
			if (size < 0x2000)
				return 0x2000;
			if (size < 0x4000)
				return 0x4000;
			if (size < 0x8000)
				return 0x8000;
			if (size < 0x10000)
				return 0x10000;
			if (size < 0x20000)
				return 0x20000;
			if (size < 0x40000)
				return 0x40000;
			return 0x80000;
		}


		/// <summary>
		/// 
		/// </summary>
		internal void CreateHashMap()
		{
			// Delete the file now.
			Delete();
			bool startThread = false;
			lock (mapQ)
			{
				mapQ.Enqueue(new HashMapDelegate(CreateHashMapFile));
				if (ThreadCount == 0 || (mapQ.Count > 1 && ThreadCount < MaxThreadCount))
				{
					// Startup a thread.
					startThread = true;
					++ThreadCount;
				}
			}
			if (startThread)
			{
				Thread thread = new Thread(new ThreadStart(HashMapThread));
				thread.IsBackground = true;
				thread.Start();
			}
			queueEvent.Set();
		}

		internal static void Delete(Collection collection, BaseFileNode node)
		{
			new HashMap(collection, node).Delete();
		}

		internal void Delete()
		{
			if (File.Exists(file))
				File.Delete(file);
		}

		/// <summary>
		/// Gets the array of HashMap
		/// </summary>
		/// <param name="entryCount">The number of hash entries.</param>
		/// <param name="blockSize">The size of the data blocks that were hashed.</param>
		/// <returns></returns>
		internal FileStream GetHashMapStream(out int entryCount, out int blockSize)
		{
			if (File.Exists(file))
			{
				FileStream stream = File.OpenRead(file);
				if (HashFileHeader.ReadHeader(new BinaryReader(stream), out blockSize, out entryCount))
				{
					return stream;
				}
			}
			this.CreateHashMap();
			entryCount = 0;
			blockSize = 0;
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public static HashData[] DeSerializeHashMap(BinaryReader reader, int count)
		{
			HashData[] fileMap = new HashData[count];
			for (int i = 0; i < count; ++i)
			{
				fileMap[i] = new HashData(reader);
			}
			return fileMap;
		}
		
		/// <summary>
		/// Return the number of Blocks that the HashMap will need.
		/// </summary>
		/// <param name="streamSize"></param>
		/// <returns></returns>
		public static int GetBlockCount(long streamSize, out int blockSize)
		{
			blockSize = CalculateBlockSize(streamSize);
			return (int)((streamSize + blockSize -1)/ blockSize);
		}

		/// <summary>
		/// Serialized the Hash Created from the input stream to the writer stream.
		/// </summary>
		/// <param name="inStream">The stream of raw data to create the HashMap from.</param>
		/// <param name="writer">The stream to write the HashMap to.</param>
		/// <param name="blockSize">The size of the hashed data blocks.</param>
		public static void SerializeHashMap(Stream inStream, BinaryWriter writer, int blockSize)
		{
			if (inStream.Length <= blockSize)
			{
				return;
			}
			byte[]			buffer = new byte[blockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			// Compute the hash codes.
			inStream.Position = 0;
			while ((bytesRead = inStream.Read(buffer, 0, blockSize)) != 0)
			{
				new HashData(
					currentBlock++,
					wh.ComputeHash(buffer, 0, (UInt16)bytesRead),
					sh.ComputeHash(buffer, 0, bytesRead)).Serialize(writer);
			}
		}
	}
	
	#endregion
}
