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

	#region HttpClientInFile

	/// <summary>
	/// ClientInFile class that uses HTTP to download the file from the server.
	/// </summary>
	public class HttpClientInFile : InFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		HttpClient				httpClient;
		/// <summary>True if the node should be marked readOnly.</summary>
		bool					readOnly = false;
		
		#endregion
		
		#region Constructor

		/// <summary>
		/// Constructs a HttpClientFile object that can be used to sync a file down from the server.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="nodeID">The id of the node to sync down</param>
		/// <param name="httpClient">The client used to access the server.</param>
		public HttpClientInFile(SyncCollection collection, string nodeID, HttpClient httpClient) :
			base(collection)
		{
			this.httpClient = httpClient;
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
			SyncNode snode = httpClient.OpenFileGet(nodeID);
			if (snode == null)
			{
				return false;
			}
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(snode.node);
			node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
			collection.ImportNode(node, false, 0);
			node.IncarnationUpdate = node.LocalIncarnation;
			base.Open(node);
			return true;
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
			httpClient.CloseFile();
			if (commit)
			{
				try
				{
					collection.Commit(node);
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
			try
			{
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
				fi.Attributes = fa;
			}
			catch (Exception ex)
			{
				Log.log.Debug(ex, "Failed Close");
				string collisionName = Conflict.GetFileConflictPath(collection, node);
				File.Delete(collisionName);
				File.Move(workFile, collisionName);
				collection.Commit(collection.CreateCollision(node, true));
			}
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
			
			// Get the file blocks from the server.
			byte[] buffer = new byte[HashData.BlockSize];
			HttpWebResponse response = httpClient.ReadFile(fileMap, HashData.BlockSize);
			Stream inStream = response.GetResponseStream();
			try
			{
				for (int i = 0; i < fileMap.Length; ++i)
				{
					if (fileMap[i] != -1)
					{
						Copy(fileMap[i], WritePosition, HashData.BlockSize);
					}
					else
					{
						int buffOffset = 0;
						int bytesInBuffer = 0;
						while (bytesInBuffer < HashData.BlockSize)
						{
							int bytesRead = 0;
							bytesRead = inStream.Read(buffer, buffOffset, HashData.BlockSize - bytesInBuffer);
							if (bytesRead == 0)
								break;
							bytesInBuffer += bytesRead;
							buffOffset += bytesRead;
						}
						Write(buffer, 0, bytesInBuffer);
						sizeRemaining -= bytesInBuffer;
						if ((i % 16) == 15)
							eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
					}
				}
			}
			finally
			{
				inStream.Close();
				response.Close();
				eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, 0, Direction.Downloading));
				Log.log.Debug("Finished Download bytes remaining = {0}", sizeRemaining);
			}
			if (sizeRemaining != 0)
				return false;
			return true;
		}

		#endregion

		#region private
		
		/// <summary>
		/// Compute the Blocks that need to be downloaded from the server. This builds
		/// an array of offsets where the blocks need to be placed in the local file.
		/// The block is represented by the index of the array.
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
			
			if (this.Exists)
				serverHashMap = httpClient.GetHashMap();
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
								fileMap[match.BlockNumber] = ReadPosition - bytesRead + startByte;
								sizeToSync -= HashData.BlockSize;
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
		HttpClient				httpClient;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Contructs a ClientFile object that can be used to sync a file up to the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync up.</param>
		/// <param name="httpClient">The service to access the server side sync.</param>
		public HttpClientOutFile(SyncCollection collection, BaseFileNode node, HttpClient httpClient) :
			base(collection)
		{
			this.node = node;
			this.httpClient = httpClient;
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
			SyncStatus status = httpClient.OpenFilePut(snode);
			if (status == SyncStatus.Success)
			{
				base.Open(node, "");
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
			SyncNodeStatus status = httpClient.CloseFile(commit);
			if (commit && status.status == SyncStatus.Success)
			{
				node.SetMasterIncarnation(node.LocalIncarnation);
				collection.Commit(node);
			}
			base.Close();
			return status;
		}
		
		/// <summary>
		/// Uploads the file to the server.
		/// </summary>
		public bool UploadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			ArrayList fileMap = GetUploadFileMap(out sizeToSync);
			sizeRemaining = sizeToSync;
			
			long offset = 0;
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));
			foreach(FileSegment segment in fileMap)
			{
				if (segment is BlockSegment)
				{
					BlockSegment bs = (BlockSegment)segment;
					int bytesToWrite = (bs.EndBlock - bs.StartBlock + 1) * HashData.BlockSize;
					httpClient.CopyFile(bs.StartBlock * HashData.BlockSize, offset, bytesToWrite);
					offset += bytesToWrite;
				}
				else
				{
					// Write the bytes to the output stream.
					OffsetSegment seg = (OffsetSegment)segment;
					byte[] dataBuffer = new byte[seg.Length];
					ReadPosition = seg.Offset;
					//int bytesRead = Read(dataBuffer, 0, seg.Length);
					httpClient.WriteFile(OutStream, offset, seg.Length);
					sizeRemaining -= seg.Length;
					//serverFile.Write(dataBuffer, offset, bytesRead);
					//sizeRemaining -= bytesRead;
					offset += seg.Length;
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));
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
		/// Gets an ArrayList of all the changes that need to be made to the server file
		/// to make the files identical.
		/// </summary>
		/// <returns></returns>
		private ArrayList GetUploadFileMap(out long sizeToSync)
		{
			sizeToSync = 0;
			ArrayList fileMap = new ArrayList();

			// Get the hash map from the server.
			HashData[] serverHashMap = httpClient.GetHashMap();
			
			if (serverHashMap == null)
			{
				// Send the whole file.
				long offset = 0;
				long fileSize = sizeToSync = Length;
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
			
			int				bytesRead = HashData.BlockSize * 16;
			byte[]			buffer = new byte[HashData.BlockSize * 16];
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
									OffsetSegment seg = new OffsetSegment(startByte - endOfLastMatch, ReadPosition - bytesRead + endOfLastMatch);
									fileMap.Add(seg);
									sizeToSync += seg.Length;
								}
								startByte = endByte + 1;
								endByte = startByte + HashData.BlockSize - 1;
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
						sizeToSync += seg.Length;
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
				sizeToSync += seg.Length;
			}

			return fileMap;
		}

		/// <summary>
		/// Send the hash map to the server.
		/// </summary>
		/// <returns></returns>
        private SyncStatus UploadHashMap()
		{
			return httpClient.PutHashMap(outStream);
		}

		/// <summary>
		/// Gets the hash map for the file on the server.
		/// </summary>
		/// <returns>The hash map.</returns>
		private HashData[] DownloadHashMap()
		{
			return httpClient.GetHashMap();
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

		#endregion
	}

	#endregion
}
