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

namespace Simias.Sync.Delta
{
	#region HashData

	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	public class HashData
	{
		/// <summary>The size of the Blocks that are hashed.</summary>
		public static int	BlockSize = 4096;
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
		/// <summary>
		/// 
		/// </summary>
		/// <param name="file">The file containing the hash data.</param>
		/// <param name="entryCount">The number of hash entries.</param>
		/// <returns></returns>
		public static byte[] GetHashMapFile(string file, out int entryCount)
		{
			Stream stream = File.OpenRead(file);
			try
			{
				entryCount = (int)(stream.Length / HashData.InstanceSize);
				byte[] hashData = new byte[stream.Length];
				stream.Read(hashData, 0, hashData.Length);
				return hashData;
			}
			finally
			{
				stream.Close();
			}
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
		public static int GetBlockCount(long streamSize)
		{
			return (int)((streamSize + HashData.BlockSize -1)/ HashData.BlockSize);
		}

		/// <summary>
		/// Serialized the Hash Created from the input stream to the writer stream.
		/// </summary>
		/// <param name="inStream">The stream of raw data to create the HashMap from.</param>
		/// <param name="entryCount">The number of hash entries.</param>
		public static byte[] GetHashMap(Stream inStream, out int entryCount)
		{
			if (inStream == null || inStream.Length <= HashData.BlockSize)
			{
				entryCount = 0;
				return null;
			}
			int				blockCount = GetBlockCount(inStream.Length);
			byte[]			hashData = new byte[blockCount * HashData.InstanceSize];
			BinaryWriter	writer = new BinaryWriter( new MemoryStream(hashData));
			byte[]			buffer = new byte[HashData.BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
			
			// Compute the hash codes.
			inStream.Position = 0;
			while ((bytesRead = inStream.Read(buffer, 0, HashData.BlockSize)) != 0)
			{
				HashData entry = new HashData(
					currentBlock,
					wh.ComputeHash(buffer, 0, (UInt16)bytesRead),
					sh.ComputeHash(buffer, 0, bytesRead));
				entry.Serialize(writer);
				currentBlock++;
			}
			writer.Close();
			entryCount = blockCount;
			return hashData;
		}


		/// <summary>
		/// Serialized the Hash Created from the input stream to the writer stream.
		/// </summary>
		/// <param name="inStream">The stream of raw data to create the HashMap from.</param>
		/// <param name="writer">The stream to write the HashMap to.</param>
		public static void SerializeHashMap(Stream inStream, BinaryWriter writer)
		{
			if (inStream.Length <= HashData.BlockSize)
			{
				return;
			}
			byte[]			buffer = new byte[HashData.BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			// Compute the hash codes.
			inStream.Position = 0;
			while ((bytesRead = inStream.Read(buffer, 0, HashData.BlockSize)) != 0)
			{
				new HashData(
					currentBlock++,
					wh.ComputeHash(buffer, 0, (UInt16)bytesRead),
					sh.ComputeHash(buffer, 0, bytesRead)).Serialize(writer);
			}
		}

		/// <summary>
		/// Serialized the HashMap to the writer stream.
		/// </summary>
		/// <param name="hashMap">The HashMap to serialize.</param>
		/// <param name="writer">The stream to write the HashMap to.</param>
		public static void SerializeHashMap(HashData[] hashMap, BinaryWriter writer)
		{
			foreach (HashData hash in hashMap)
			{
				hash.Serialize(writer);
			}
		}
	}

	#endregion
}
