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
using System.Xml;
using System.Collections;
using System.Security.Cryptography;
using Simias.Storage;
using Simias.Sync;

namespace Simias.Sync.Client
{
	
	#region FileSegment

	/// <summary>
	/// The base class for an upload file segment.
	/// </summary>
	[Serializable]
	public abstract class FileSegment
	{
	}

	#endregion

	#region BlockSegment

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

		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="startBlock">The start block.</param>
		/// <param name="endBlock">The end block.</param>
		public BlockSegment(int startBlock, int endBlock)
		{
			this.StartBlock = startBlock;
			this.EndBlock = endBlock;
		}

	}

	#endregion

	#region OffsetSegment

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

		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="length">The length of the segment.</param>
		/// <param name="offset">The offset of the segment.</param>
		public OffsetSegment(int length, long offset)
		{
			this.Length = length;
			this.Offset = offset;
		}
	}

	#endregion

	#region ClientFile

	/// <summary>
	/// Used to find the deltas between
	/// the local file and the server file.
	/// </summary>
	public class ClientFile : SyncFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		SimiasSyncService		service;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Contructs a ClientFile object that can be used to sync a file up to the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync up.</param>
		/// <param name="service">The service to access the server file.</param>
		public ClientFile(Collection collection, BaseFileNode node, SimiasSyncService service) :
			base(collection, SyncDirection.OUT)
		{
			this.service = service;
			this.node = node;
		}

		/// <summary>
		/// Constructs a ClientFile object that can be used to sync a file down from the server.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="nodeID">The id of the node to sync down</param>
		/// <param name="service">The service to access the server file.</param>
		public ClientFile(Collection collection, string nodeID, SimiasSyncService service) :
			base(collection, SyncDirection.IN)
		{
			this.service = service;
			this.nodeID = nodeID;
		}

		#endregion

		#region publics

		public void Open()
		{
			if (direction == SyncDirection.IN)
			{
				SyncNode snode = service.GetFileNode(nodeID);
				if (snode == null)
				{
					throw new SimiasException(string.Format("Node {0} not found on server.", nodeID));
				}
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(snode.node);
				node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
				collection.ImportNode(node, false, 0);
				node.IncarnationUpdate = node.LocalIncarnation;
			}
			else
			{
				SyncNode snode = new SyncNode();
				snode.node = node.Properties.ToString(true);
				snode.expectedIncarn = node.MasterIncarnation;
						
				if (!service.PutFileNode(snode))
				{
					throw new SimiasException(string.Format("Node {0} not found on server.", nodeID));
				}
			}
			base.Open(node);
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		public new void Close(bool commit)
		{
			if (direction == SyncDirection.IN)
			{
				collection.Commit(node);
			}
			service.CloseFileNode(commit);
			base.Close(commit);
		}

		
		/// <summary>
		/// Uploads the file to the server.
		/// </summary>
		public bool UploadFile()
		{
			ArrayList fileMap = GetUploadFileMap();

			byte[] buffer = new byte[BlockSize];
			long offset = 0;
			foreach(FileSegment segment in fileMap)
			{
				if (segment is BlockSegment)
				{
					BlockSegment bs = (BlockSegment)segment;
					int bytesToWrite = (bs.EndBlock - bs.StartBlock + 1) * BlockSize;
					service.Copy(bs.StartBlock * BlockSize, offset, bytesToWrite);
					offset += bytesToWrite;
				}
				else
				{
					// Write the bytes to the output stream.
					OffsetSegment seg = (OffsetSegment)segment;
					byte[] dataBuffer = new byte[seg.Length];
					ReadPosition = seg.Offset;
					int bytesRead = Read(dataBuffer, 0, seg.Length);
					service.Write(dataBuffer, offset, bytesRead);
					offset += seg.Length;
				}
			}
			return true;
		}

		/// <summary>
		/// Downloads the file from the server.
		/// </summary>
		public bool DownLoadFile()
		{
			long[] fileMap = this.GetDownloadFileMap();
			//byte[] buffer = new byte[BlockSize];
			WritePosition = 0;
				
			for (int i = 0; i < fileMap.Length; ++i)
			{
				if (fileMap[i] != -1)
				{
					Copy(fileMap[i], BlockSize);
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
					//if (readBufferSize != BlockSize)
					//	readBuffer = new byte[readBufferSize];
					//else
					//	readBuffer = buffer;

					int bytesRead = service.Read(offset, readBufferSize, out readBuffer);
					Write(readBuffer, 0, bytesRead);
				}
			}
			return true;
		}

		#endregion

		#region privates

		/// <summary>
		/// Gets an ArrayList of all the changes that need to be made to the server file
		/// to make the files identical.
		/// </summary>
		/// <returns></returns>
		private ArrayList GetUploadFileMap()
		{
			ArrayList fileMap = new ArrayList();

			// Get the hash map from the server.
			HashData[] serverHashMap = service.GetHashMap(BlockSize);
			
			if (serverHashMap == null)
			{
				// Send the whole file.
				long offset = 0;
				long fileSize = Length;
				while (offset < fileSize)
				{
					long bytesLeft = fileSize - offset;
					int size = (int)((bytesLeft > MaxXFerSize) ? MaxXFerSize : bytesLeft);
					OffsetSegment seg = new OffsetSegment(size, offset);
					fileMap.Add(seg);
					offset += size;
				}
				return fileMap;
			}

			table.Clear();
			table.Add(serverHashMap);
			
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
			
			ReadPosition = 0;		
			while (bytesRead != 0)
			{
				bytesRead = Read(buffer, readOffset, bytesRead - readOffset);
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
						if (table.Contains(entry.WeakHash))
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, BlockSize);
							HashEntry match = table.GetEntry(entry);
							if (match != null)
							{
								// We found a match save the data that does not match;
								if (endOfLastMatch != startByte)
								{
									OffsetSegment seg = new OffsetSegment(startByte - endOfLastMatch, ReadPosition - bytesRead + endOfLastMatch);
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
									lastBS = new BlockSegment(match.BlockNumber, match.BlockNumber);
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
						OffsetSegment seg = new OffsetSegment(startByte - endOfLastMatch, ReadPosition - bytesRead + endOfLastMatch);
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
				int len = endByte - endOfLastMatch + 1;
				OffsetSegment seg = new OffsetSegment(len, ReadPosition - len);
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
		private long[] GetDownloadFileMap()
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			table.Clear();
			HashData[] serverHashMap = service.GetHashMap(BlockSize);
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

			ReadPosition = 0;					
			while (bytesRead != 0)
			{
				bytesRead = Read(buffer, readOffset, bytesRead - readOffset);
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
						if (table.Contains(entry.WeakHash))
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, BlockSize);
							HashEntry match = table.GetEntry(entry);
							if (match != null)
							{
								// We found a match save the match;
								fileMap[match.BlockNumber] = ReadPosition - bytesRead + startByte;
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
		/// Called to get a string description of the Diffs.
		/// </summary>
		/// <param name="segments">An array of segment descriptions.</param>
		/// <returns>The string description.</returns>
		private string ReportUploadDiffs(ArrayList segments)
		{
			StringWriter sw = new StringWriter();
			foreach (FileSegment segment in segments)
			{
				if (segment is BlockSegment)
				{
					BlockSegment bs = (BlockSegment)segment;
					sw.WriteLine("Found Match Block {0} to Block {1}", bs.StartBlock, bs.EndBlock);
				}
				else
				{
					OffsetSegment seg = (OffsetSegment)segment;
					sw.WriteLine("Found change size = {0}", seg.Length);
				}
			}
			return sw.ToString();
		}

		/// <summary>
		/// Called to get a string description of the diffs.
		/// </summary>
		/// <param name="fileMap">The filmap array.</param>
		/// <returns>The string description.</returns>
		private string ReportDownloadDiffs(long[] fileMap)
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

		#endregion

	}

	#endregion
}
