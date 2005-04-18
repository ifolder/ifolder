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
using System.Threading;
using System.Net;
using Simias.Client.Event;
using Simias.Storage;
using Simias.Sync.Http;
using Simias.Sync.Delta;
using Simias.Event;

namespace Simias.Sync
{
	#region FileSegment

	/// <summary>
	/// The base class for an upload file segment.
	/// </summary>
	[Serializable]
	public abstract class FileSegment
	{
		/// <summary>
		/// The offset of where the data goes.
		/// </summary>
		public long Offset;
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
		/// The ending block in the range of contiguous blocks.
		/// </summary>
		public int				EndBlock;
		
		public static int		InstanceSize = (8 + 4 + 4);
		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="offset">The offset of where to copy.</param>
		/// <param name="block">The block to copy.</param>
		public BlockSegment(long offset, int block)
		{
			this.Offset = offset;
			this.StartBlock = block;
			this.EndBlock = block;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public BlockSegment(BinaryReader reader)
		{
			Offset = reader.ReadInt64();
			StartBlock = reader.ReadInt32();
			EndBlock = reader.ReadInt32();
		}

		/// <summary>
		/// Adds the segment to the array.  If this segment is contiguous with the last segment
		/// combine them.
		/// </summary>
		/// <param name="segArray">The array to add the segment to.</param>
		/// <param name="seg">The new segment to add.</param>
		public static void AddToArray(ArrayList segArray, BlockSegment seg)
		{
			BlockSegment lastSeg;
			if (segArray.Count > 0)
			{
				lastSeg = segArray[segArray.Count -1] as BlockSegment;
				// Make sure the source and destination are contiguous.
				if ((lastSeg.EndBlock + 1 == seg.StartBlock) 
					&& ((lastSeg.Offset + (HashData.BlockSize * (lastSeg.EndBlock - lastSeg.StartBlock + 1))) == seg.Offset))
				{
					lastSeg.EndBlock = seg.StartBlock;
					return;
				}
			}
			segArray.Add(seg);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(Offset);
			writer.Write(StartBlock);
			writer.Write(EndBlock);
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
		public long		Length;
		
		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="length">The length of the segment.</param>
		/// <param name="offset">The offset of the segment.</param>
		public OffsetSegment(long length, long offset)
		{
			this.Length = length;
			this.Offset = offset;
		}

		/// <summary>
		/// Adds the segment to the array.  If this segment is contiguous with the last segment
		/// combine them.
		/// </summary>
		/// <param name="segArray">The array to add the segment to.</param>
		/// <param name="seg">The new segment to add.</param>
		public static void AddToArray(ArrayList segArray, OffsetSegment seg)
		{
			OffsetSegment lastSeg;
			if (segArray.Count > 0)
			{
				lastSeg = segArray[segArray.Count -1] as OffsetSegment;
				if (seg.Offset - lastSeg.Length == lastSeg.Offset)
				{
					lastSeg.Length += seg.Length;
					return;
				}
			}
			segArray.Add(seg);
		}
	}

	#endregion

	#region DownloadSegment

	/// <summary>
	/// Describes a file segment using a block from the remote file. Can be a
	/// Range of blocks.
	/// </summary>
	public class DownloadSegment
	{
		/// <summary>
		/// This is the start block for the unchanged segment of data.
		/// </summary>
		public int				StartBlock;
		/// <summary>
		/// The ending block in the range of contiguous blocks.
		/// </summary>
		public int				EndBlock;
		
		public static int		InstanceSize = (4 + 4);
		/// <summary>
		/// Initialize a new Offset Segment.
		/// </summary>
		/// <param name="block">The block to copy.</param>
		public DownloadSegment(int block)
		{
			this.StartBlock = block;
			this.EndBlock = block;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		public DownloadSegment(BinaryReader reader)
		{
			StartBlock = reader.ReadInt32();
			EndBlock = reader.ReadInt32();
		}

		/// <summary>
		/// Adds the segment to the array.  If this segment is contiguous with the last segment
		/// combine them.
		/// </summary>
		/// <param name="segArray">The array to add the segment to.</param>
		/// <param name="seg">The new segment to add.</param>
		public static void AddToArray(ArrayList segArray, DownloadSegment seg)
		{
			DownloadSegment lastSeg;
			if (segArray.Count > 0)
			{
				lastSeg = segArray[segArray.Count -1] as DownloadSegment;
				// Make sure the source and destination are contiguous.
				if (lastSeg.EndBlock + 1 == seg.StartBlock && lastSeg.EndBlock - lastSeg.StartBlock < 100) 
				{
					lastSeg.EndBlock = seg.StartBlock;
					return;
				}
			}
			segArray.Add(seg);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize(BinaryWriter writer)
		{
			writer.Write(StartBlock);
			writer.Write(EndBlock);
		}
	}

	#endregion

	#region HttpClientInFile

	/// <summary>
	/// ClientInFile class that uses HTTP to download the file from the server.
	/// </summary>
	public class HttpClientInFile : InFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		HttpSyncProxy				syncService;
		/// <summary>True if the node should be marked readOnly.</summary>
		bool					readOnly = false;
		
		#endregion
		
		#region Constructor

		/// <summary>
		/// Constructs a HttpClientFile object that can be used to sync a file down from the server.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="nodeID">The id of the node to sync down</param>
		/// <param name="syncService">The client used to access the server.</param>
		public HttpClientInFile(Collection collection, string nodeID, HttpSyncProxy syncService) :
			base(collection)
		{
			this.syncService = syncService;
			this.nodeID = nodeID;
		}

		#endregion

		#region publics

		/// <summary>
		/// Open the file.
		/// </summary>
		/// <param name="readOnly">True if the file should be marked readonly.</param>
		/// <returns>True if the file was opened.</returns>
		public bool Open(bool readOnly)
		{
			this.readOnly = readOnly;
			SyncNode snode = syncService.OpenFileGet(nodeID);
			if (snode == null)
			{
				return false;
			}
			try
			{
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(snode.node);
				node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
				collection.ImportNode(node, false, 0);
				node.IncarnationUpdate = node.LocalIncarnation;
				base.Open(node);
				return true;
			}
			catch (Exception ex)
			{
				syncService.CloseFile();
				base.Close(false);
				Log.log.Debug(ex, "Failed opening file {0}", file);
				throw ex;
			}
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		/// <returns>true if successful.</returns>
		public new bool Close(bool commit)
		{
			Log.log.Debug("Closing File success = {0}", commit);
			bool bStatus = commit;
			// Close the file on the server.
			try
			{
				syncService.CloseFile();
			}
			catch {}
			if (commit)
			{
				try
				{
					collection.Commit(node);
					if (conflictingNode != null)
						collection.Commit(conflictingNode);
				}
				catch (CollisionException)
				{
					// Create an update conflict.
					file = Conflict.GetUpdateConflictPath(collection, node);
					collection.Commit(collection.CreateCollision(node, false));
					oldNode = null;
				}
				catch
				{
					bStatus = false;
					commit = false;
				}
			}
		
			// Make sure the file is not read only.
			FileInfo fi = new FileInfo(file);
			FileAttributes fa;
			if (fi.Exists)
			{
				fa = fi.Attributes;
				fi.Attributes = fa & ~FileAttributes.ReadOnly;
			}
			else
			{
				fa = FileAttributes.Normal;
			}
			base.Close(commit);
			if (readOnly)
			{
				// BUGBUG this is commented out until we decide what to do with readonly collections.
				//fa |= FileAttributes.ReadOnly;
			}
			if (fi.Exists)
				fi.Attributes = fa;
			return bStatus;
		}


		/// <summary>
		/// Downloads the file from the server.
		/// </summary>
		public bool DownLoadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			long[] fileMap = GetDownloadFileMap(out sizeToSync);
			sizeRemaining = sizeToSync;
			WritePosition = 0;
				
			Log.log.Debug("Downloading {0} bytes, filesize = {1}", sizeToSync, fileSize); 
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
			// If We don't have any entries in the file map the file is of length 0.
			if (fileMap.Length == 0)
				return true;

			ArrayList downloadMap = new ArrayList();
			// Copy the matches from the local file.
			for (int i = 0; i < fileMap.Length; ++i)
			{
				if (fileMap[i] != -1)
				{
					Copy(fileMap[i], WritePosition, HashData.BlockSize);
				}
				else
				{
					DownloadSegment.AddToArray(downloadMap, new DownloadSegment(i));
				}
			}
				
			
			// Get the file blocks from the server.
			foreach (DownloadSegment seg in downloadMap)
			{
				if (stopping)
					break;
				HttpWebResponse response = syncService.ReadFile(seg, HashData.BlockSize);
				Stream inStream = response.GetResponseStream();
				try
				{
					int bytesToWrite = (int)Math.Min(sizeRemaining, (seg.EndBlock - seg.StartBlock + 1) * HashData.BlockSize);
					Write(inStream, bytesToWrite);
					sizeRemaining -= bytesToWrite;
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
				}
				finally
				{
					inStream.Close();
					response.Close();
				}
			}
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, 0, Direction.Downloading));
			Log.log.Debug("Finished Download bytes remaining = {0}", sizeRemaining);
			if (sizeRemaining != 0)
				return false;
			return true;
		}

		#endregion

		#region private
		
		/// <summary>
		/// Compute the Blocks that need to be downloaded from the server. This builds
		/// an array of offsets where the blocks need to be placed in the local file.
		/// The block number is represented by the index of the array. -1 means no match.
		/// </summary>
		/// <param name="sizeToSync">The number of bytes that need to be synced.</param>
		/// <returns>The file map.</returns>
		private long[] GetDownloadFileMap(out long sizeToSync)
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			table.Clear();
			HashData[] serverHashMap;
			long[] fileMap;
			
			if (ReadStream != null)
				serverHashMap = syncService.GetHashMap();
			else
				serverHashMap = new HashData[0];

			if (serverHashMap.Length == 0)
			{
				sizeToSync = node.Length;
				fileMap = new long[HashMap.GetBlockCount(node.Length)];
				for (int i = 0; i < fileMap.Length; ++i)
					fileMap[i] = -1;
				return fileMap;
			}
			
			sizeToSync = HashData.BlockSize * serverHashMap.Length;
			long remainingBytes = node.Length % HashData.BlockSize;
			if (remainingBytes != 0)
				sizeToSync = sizeToSync - HashData.BlockSize + remainingBytes;
			table.Add(serverHashMap);
			fileMap = new long[serverHashMap.Length];

			int				bytesRead = HashData.BlockSize * 16;
			byte[]			buffer = new byte[HashData.BlockSize * 16];
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
				
				if (bytesRead >= HashData.BlockSize)
				{
					endByte = startByte + HashData.BlockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, (ushort)HashData.BlockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(HashData.BlockSize, dropByte, buffer[endByte]);
						if (table.Contains(entry.WeakHash))
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, HashData.BlockSize);
							HashEntry match = table.GetEntry(entry);
							if (match != null)
							{
								// We found a match save the match;
								if (fileMap[match.BlockNumber] == -1)
								{
									fileMap[match.BlockNumber] = ReadPosition - bytesRead + startByte;
									sizeToSync -= HashData.BlockSize;
								}
								startByte += HashData.BlockSize;
								endByte += HashData.BlockSize;
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
			return fileMap;
		}

		/// <summary>
		/// Called to get a string description of the diffs.
		/// </summary>
		/// <param name="fileMap">The filmap array.</param>
		/// <returns>The string description.</returns>
		private string ReportDiffs(long[] fileMap)
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

	#region HttpClientOutFile

	/// <summary>
	/// Class used to push a file to the server using HTTP.
	/// </summary>
	class HttpClientOutFile : OutFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		HttpSyncProxy				syncService;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Contructs a ClientFile object that can be used to sync a file up to the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync up.</param>
		/// <param name="syncService">The service to access the server side sync.</param>
		public HttpClientOutFile(Collection collection, BaseFileNode node, HttpSyncProxy syncService) :
			base(collection)
		{
			this.node = node;
			this.syncService = syncService;
		}

		#endregion

		#region publics

		/// <summary>
		/// Open the file.
		/// </summary>
		/// <returns>The status of the open.</returns>
		public virtual SyncStatus Open()
		{
			SyncNode snode = new SyncNode(node);
			SyncStatus status = syncService.OpenFilePut(snode);
			if (status == SyncStatus.Success)
			{
				try
				{
					base.Open(node, "");
				}
				catch (Exception ex)
				{
					Log.log.Debug(ex, "Failed opening file {0}", file);
					syncService.CloseFile(false);
					base.Close();
					throw ex;
				}
			}
			
			return status;
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		/// <returns>true if successful.</returns>
		public virtual SyncNodeStatus Close(bool commit)
		{
			// Close the file on the server.
			try
			{
				SyncNodeStatus status = syncService.CloseFile(commit);
				if (commit && status.status == SyncStatus.Success)
				{
					node.SetMasterIncarnation(node.LocalIncarnation);
					collection.Commit(node);
				}
				return status;
			}
			finally
			{
				base.Close();
			}
		}
		
		/// <summary>
		/// Uploads the file to the server.
		/// </summary>
		public bool UploadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			ArrayList copyArray;
			ArrayList writeArray;
			GetUploadFileMap(out sizeToSync, out copyArray, out writeArray);
			sizeRemaining = sizeToSync;
			
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));

			if (copyArray.Count > 0)
			{
				syncService.CopyFile(copyArray);
			}
			
			foreach(OffsetSegment seg in writeArray)
			{
				if (stopping)
					break;
				// Write the bytes to the output stream.
				if (seg.Length > 0)
				{
					long leftToSend = seg.Length;
					ReadPosition = seg.Offset;
					while (leftToSend > 0)
					{	
						int bytesToSend = (int)Math.Min(MaxXFerSize, leftToSend);
						syncService.WriteFile(OutStream, ReadPosition, bytesToSend);
						leftToSend -= bytesToSend;
						sizeRemaining -= bytesToSend;
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));
					}
				}
			}
			if (sizeRemaining == 0)
				return true;
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, 0, Direction.Uploading));
			return false;
		}

		#endregion

		#region private

		/// <summary>
		/// Gets the copy and write arrays that are used to create the file on the server.
		/// </summary>
		/// <param name="sizeToSync"></param>
		/// <param name="copyArray">The array of BlockSegments that need to be copied from the old file.</param>
		/// <param name="writeArray">The array of OffsetSegments that need to be sent from the client.</param>
		private void GetUploadFileMap(out long sizeToSync, out ArrayList copyArray, out ArrayList writeArray)
		{
			sizeToSync = 0;
			copyArray = new ArrayList();
			writeArray = new ArrayList();

			// Get the hash map from the server.
			HashData[] serverHashMap = syncService.GetHashMap();
			
			if (serverHashMap.Length == 0)
			{
				// Send the whole file.
				sizeToSync = Length;
				writeArray.Add(new OffsetSegment(sizeToSync, 0));
				return;
			}

			table.Clear();
			table.Add(serverHashMap);
			
			int				bytesRead = HashData.BlockSize * 16;
			byte[]			buffer = new byte[HashData.BlockSize * 16];
			int				readOffset = 0;
			WeakHash		wh = new WeakHash();
			StrongHash		sh = new StrongHash();
			bool			recomputeWeakHash = true;
			int				startByte = 0;
			int				endByte = 0;
			int				endOfLastMatch = 0;
			byte			dropByte = 0;
			
			ReadPosition = 0;		
			while (bytesRead != 0)
			{
				bytesRead = outStream.Read(buffer, readOffset, bytesRead - readOffset);
				if (bytesRead == 0)
					break;

				bytesRead = bytesRead + readOffset;
				
				if (bytesRead >= HashData.BlockSize)
				{
					endByte = startByte + HashData.BlockSize - 1;
					HashEntry entry = new HashEntry();
					while (endByte < bytesRead)
					{
						if (recomputeWeakHash)
						{
							entry.WeakHash = wh.ComputeHash(buffer, startByte, (ushort)HashData.BlockSize);
							recomputeWeakHash = false;
						}
						else
							entry.WeakHash = wh.RollHash(HashData.BlockSize, dropByte, buffer[endByte]);
						if (table.Contains(entry.WeakHash))
						{
							entry.StrongHash = sh.ComputeHash(buffer, startByte, HashData.BlockSize);
							HashEntry match = table.GetEntry(entry);
							if (match != null)
							{
								// We found a match save the data that does not match;
								if (endOfLastMatch != startByte)
								{
									long segLen = startByte - endOfLastMatch;
									long segOffset = ReadPosition - bytesRead + endOfLastMatch;
									OffsetSegment seg = new OffsetSegment(segLen, segOffset);
									OffsetSegment.AddToArray(writeArray, seg);
									sizeToSync += segLen;
								}
								// Save the matched block.
								long blockOffset = ReadPosition - bytesRead + startByte;
								BlockSegment.AddToArray(copyArray, new BlockSegment(blockOffset, match.BlockNumber));
								
								startByte = endByte + 1;
								endByte = startByte + HashData.BlockSize - 1;
								endOfLastMatch = startByte;
								recomputeWeakHash = true;
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
						// Add this segment to the array.
						long segOffset = ReadPosition - bytesRead + endOfLastMatch;
						long segLen = startByte - endOfLastMatch;
						OffsetSegment seg = new OffsetSegment(segLen, segOffset);
						OffsetSegment.AddToArray(writeArray, seg);
						sizeToSync += segLen;
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
			if ((endOfLastMatch + 1) != endByte)//== 0 && endByte != 0)
			{
				long segLen = endByte - endOfLastMatch + 1;
				long segOffset = ReadPosition - segLen;
				OffsetSegment seg = new OffsetSegment(segLen, segOffset);
				OffsetSegment.AddToArray(writeArray, seg);
				sizeToSync += segLen;
			}
		}

		/// <summary>
		/// Send the hash map to the server.
		/// </summary>
		/// <returns></returns>
        private SyncStatus UploadHashMap()
		{
			return syncService.PutHashMap(outStream);
		}

		/// <summary>
		/// Gets the hash map for the file on the server.
		/// </summary>
		/// <returns>The hash map.</returns>
		private HashData[] DownloadHashMap()
		{
			return syncService.GetHashMap();
		}

		/// <summary>
		/// Called to get a string description of the Diffs.
		/// </summary>
		/// <param name="segments">An array of segment descriptions.</param>
		/// <returns>The string description.</returns>
		private string ReportDiffs(ArrayList segments)
		{
			StringWriter sw = new StringWriter();
			foreach (FileSegment segment in segments)
			{
				if (segment is BlockSegment)
				{
					BlockSegment bs = (BlockSegment)segment;
					sw.WriteLine("Found Match Offset = {0} Block {1} - {2}", bs.Offset, bs.StartBlock, bs.EndBlock);
				}
				else
				{
					OffsetSegment seg = (OffsetSegment)segment;
					sw.WriteLine("Found change size = {0}", seg.Length);
				}
			}
			return sw.ToString();
		}

		#endregion
	}

	#endregion
}
