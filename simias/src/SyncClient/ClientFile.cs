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
using Simias.Sync;

namespace Simias.Sync.Client
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
	/// Used to find the deltas between
	/// the local file and the server file.
	/// </summary>
	public class ClientFile : SyncFile
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

		/// <summary>
		/// Writes the new file based on The fileMap array.
		/// </summary>
		/// <param name="fileMap">An array of FileSegments That describes how the server file relates to 
		/// the local file.</param>
		/// <param name="sFile">The server file.</param>
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

		/// <summary>
		/// Writes the new file based on The fileMap.
		/// </summary>
		/// <param name="fileMap">The fileMap.</param>
		/// <param name="server">The server file.</param>
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
		/// Called to get a string description of the Diffs.
		/// </summary>
		/// <param name="segments">An array of segment descriptions.</param>
		/// <returns>The string description.</returns>
		internal string ReportUploadDiffs(ArrayList segments)
		{
			StringWriter sw = new StringWriter();
			foreach (FileSegment segment in segments)
			{
				switch (segment.Type)
				{
					case SegmentType.Block:
						BlockSegment bs = (BlockSegment)segment;
						sw.WriteLine("Found Match Block {0} to Block {1}", bs.StartBlock, bs.EndBlock);
						break;
					case SegmentType.Offset:
						OffsetSegment seg = (OffsetSegment)segment;
						sw.WriteLine("Found change size = {0}", seg.Length);
						break;
				}
			}
			return sw.ToString();
		}

		/// <summary>
		/// Called to get a string description of the diffs.
		/// </summary>
		/// <param name="fileMap">The filmap array.</param>
		/// <returns>The string description.</returns>
		internal string ReportDownloadDiffs(long[] fileMap)
		{
			StringWriter sw = new StringWriter();
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
						sw.WriteLine("Found Missing Block {0} to Block {1}", startBlock, endBlock);
					startBlock = -1;
				}
			}
			if (startBlock != -1)
				sw.WriteLine("Found Missing Block {0} to Block {1}", startBlock, endBlock);
			return sw.ToString();
		}
	}
}
