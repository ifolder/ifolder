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
using Simias.Sync;
using Simias.Event;

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

	#region WsClientInFile

	/// <summary>
	/// Used to find the deltas between
	/// the local file and the server file.
	/// </summary>
	public class WsClientInFile : InFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		WsServerFile				serverFile;
		/// <summary>True if the node should be marked readOnly.</summary>
		bool					readOnly = false;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Constructs a ClientFile object that can be used to sync a file down from the server.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="nodeID">The id of the node to sync down</param>
		/// <param name="service">The service to access the server side sync.</param>
		public WsClientInFile(SyncCollection collection, string nodeID, SimiasSyncService service) :
			base(collection)
		{
			serverFile = new WsServerFile(service);
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
			SyncNode snode = serverFile.GetFileNode(nodeID);
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
			serverFile.CloseFileNode();
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
		public virtual bool DownLoadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			long[] fileMap = this.GetDownloadFileMap(out sizeToSync);
			sizeRemaining = sizeToSync;
			WritePosition = 0;
				
			Log.log.Debug("Downloading {0} bytes, filesize = {1}", sizeToSync, fileSize); 
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
			try
			{
				for (int i = 0; i < fileMap.Length; ++i)
				{
					if (fileMap[i] != -1)
					{
						Copy(fileMap[i], WritePosition, BlockSize);
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
								if ((readBufferSize + BlockSize) >= MaxXFerSize)
								{
									// We don't allow bigger XFers than this get out.
									break;
								}
								else
								{
									readBufferSize += BlockSize;
									i++;
								}
							}
							else
							{
								// The next segment is already local.
								break;
							}
						}

						byte[] readBuffer;
						readBufferSize = (readBufferSize > sizeRemaining) ? (int)sizeRemaining : readBufferSize;
						int bytesRead = serverFile.Read(offset, readBufferSize, out readBuffer);
						if (bytesRead == 0)
							break;
						Write(readBuffer, 0, bytesRead);
						sizeRemaining -= bytesRead;
						eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
					}
				}
			}
			finally
			{
				eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, 0, Direction.Downloading));
				Log.log.Debug("Finished Download bytes remaining = {0}", sizeRemaining);
			}
			if (sizeRemaining != 0)
				return false;
			return true;
		}

		#endregion
		
		#region protected
		
		/// <summary>
		/// Compute the Blocks that need to be downloaded from the server. This builds
		/// an array of offsets where the blocks need to be placed in the local file.
		/// The block is represented by the index of the array.
		/// </summary>
		/// <param name="sizeToSync">The number of bytes that need to be synced.</param>
		/// <returns>The file map.</returns>
		protected long[] GetDownloadFileMap(out long sizeToSync)
		{
			// Since we are doing the diffing on the client we will download all blocks that
			// don't match.
			table.Clear();
			HashData[] serverHashMap = serverFile.GetHashMap(BlockSize);
			long[] fileMap;
			if (serverHashMap == null)
			{
				sizeToSync = node.Length;
				fileMap = new long[1];
				fileMap[0] = -1;
				return fileMap;
			}
			
			sizeToSync = BlockSize * serverHashMap.Length;
			long remainingBytes = node.Length % BlockSize;
			if (remainingBytes != 0)
				sizeToSync = sizeToSync - BlockSize + remainingBytes;
			table.Add(serverHashMap);
			fileMap = new long[serverHashMap.Length];

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
								sizeToSync -= BlockSize;
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
		protected string ReportDiffs(long[] fileMap)
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

	#region WsClientOutFile

	/// <summary>
	/// Used to find the deltas between
	/// the local file and the server file.
	/// </summary>
	public class WsClientOutFile : OutFile
	{
		#region fields

		StrongWeakHashtable		table = new StrongWeakHashtable();
		WsServerFile			serverFile;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Contructs a ClientFile object that can be used to sync a file up to the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync up.</param>
		/// <param name="service">The service to access the server side sync.</param>
		public WsClientOutFile(SyncCollection collection, BaseFileNode node, SimiasSyncService service) :
			base(collection)
		{
			serverFile = new WsServerFile(service);
			this.node = node;
		}

		#endregion

		#region publics

		/// <summary>
		/// Open the file.
		/// </summary>
		/// <returns>The status of the open.</returns>
		public virtual SyncStatus Open()
		{
			SyncNode snode = new SyncNode();
			snode.nodeID = node.ID;
			snode.node = node.Properties.ToString(true);
			snode.expectedIncarn = node.MasterIncarnation;
			SyncStatus status = serverFile.PutFileNode(snode);
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
			SyncNodeStatus status = serverFile.CloseFileNode(commit);
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
		public virtual bool UploadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			ArrayList fileMap = GetUploadFileMap(out sizeToSync);
			sizeRemaining = sizeToSync;
//			byte[]	dataBuffer = new byte[MaxXFerSize];
			
			long offset = 0;
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));
			foreach(FileSegment segment in fileMap)
			{
				if (segment is BlockSegment)
				{
					BlockSegment bs = (BlockSegment)segment;
					int bytesToWrite = (bs.EndBlock - bs.StartBlock + 1) * BlockSize;
					serverFile.Copy(bs.StartBlock * BlockSize, offset, bytesToWrite);
					offset += bytesToWrite;
				}
				else
				{
					// Write the bytes to the output stream.
					OffsetSegment seg = (OffsetSegment)segment;
					byte[] dataBuffer = new byte[seg.Length];
					ReadPosition = seg.Offset;
					int bytesRead = Read(dataBuffer, 0, seg.Length);
					serverFile.Write(dataBuffer, offset, bytesRead);
					sizeRemaining -= bytesRead;
					offset += seg.Length;
					eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Uploading));
				}
			}
			if (sizeRemaining == 0)
				return true;
			return false;
		}

		#endregion

		#region protected

		/// <summary>
		/// Gets an ArrayList of all the changes that need to be made to the server file
		/// to make the files identical.
		/// </summary>
		/// <returns></returns>
		protected ArrayList GetUploadFileMap(out long sizeToSync)
		{
			sizeToSync = 0;
			ArrayList fileMap = new ArrayList();

			// Get the hash map from the server.
			HashData[] serverHashMap = serverFile.GetHashMap(BlockSize);
			
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
									sizeToSync += seg.Length;
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
		/// Called to get a string description of the Diffs.
		/// </summary>
		/// <param name="segments">An array of segment descriptions.</param>
		/// <returns>The string description.</returns>
		protected string ReportDiffs(ArrayList segments)
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

	#region HttpClientInFile

	/// <summary>
	/// ClientInFile class that uses HTTP to download the file from the server.
	/// </summary>
	public class HttpClientInFile : WsClientInFile
	{
		HttpServerFile	serverFile;
		/// <summary>
		/// Constructs a HttpClientFile object that can be used to sync a file down from the server.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="nodeID">The id of the node to sync down</param>
		/// <param name="service">The service to access the server side sync.</param>
		public HttpClientInFile(SyncCollection collection, string nodeID, SimiasSyncService service) :
			base(collection, nodeID, service)
		{
			serverFile = new HttpServerFile(service);
		}

		/// <summary>
		/// Downloads the file from the server.
		/// </summary>
		public override bool DownLoadFile()
		{
			long	fileSize = Length;
			long	sizeToSync;
			long	sizeRemaining;
			long[] fileMap = this.GetDownloadFileMap(out sizeToSync);
			sizeRemaining = sizeToSync;
			WritePosition = 0;
				
			Log.log.Debug("Downloading {0} bytes, filesize = {1}", sizeToSync, fileSize); 
			eventPublisher.RaiseEvent(new FileSyncEventArgs(collection.ID, ObjectType.File, false, Name, fileSize, sizeToSync, sizeRemaining, Direction.Downloading));
			
			// Get the file blocks from the server.
			byte[] buffer = new byte[BlockSize];
			HttpWebResponse response = serverFile.Read(fileMap, BlockSize);
			Stream inStream = response.GetResponseStream();
			try
			{
				for (int i = 0; i < fileMap.Length; ++i)
				{
					if (fileMap[i] != -1)
					{
						Copy(fileMap[i], WritePosition, BlockSize);
					}
					else
					{
						int buffOffset = 0;
						int bytesInBuffer = 0;
						do
						{
							int bytesRead = 0;
							bytesRead = inStream.Read(buffer, buffOffset, BlockSize - bytesInBuffer);
							if (bytesRead == 0)
								break;
							bytesInBuffer += bytesRead;
							buffOffset += bytesRead;
						} while (bytesInBuffer != BlockSize);
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
	}

	#endregion

	#region HttpClientOutFile

	/// <summary>
	/// Class used to push a file to the server using HTTP.
	/// </summary>
	class HttpClientOutFile : WsClientOutFile
	{
		#region fields

		HttpServerFile			serverFile;
		
		#endregion
		
		#region Constructors

		/// <summary>
		/// Contructs a ClientFile object that can be used to sync a file up to the server.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync up.</param>
		/// <param name="service">The service to access the server side sync.</param>
		public HttpClientOutFile(SyncCollection collection, BaseFileNode node, SimiasSyncService service) :
			base(collection, node, service)
		{
			serverFile = new HttpServerFile(service);
		}

		#endregion

		#region publics

		/// <summary>
		/// Uploads the file to the server.
		/// </summary>
		public override bool UploadFile()
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
					int bytesToWrite = (bs.EndBlock - bs.StartBlock + 1) * BlockSize;
					serverFile.Copy(bs.StartBlock * BlockSize, offset, bytesToWrite);
					offset += bytesToWrite;
				}
				else
				{
					// Write the bytes to the output stream.
					OffsetSegment seg = (OffsetSegment)segment;
					byte[] dataBuffer = new byte[seg.Length];
					ReadPosition = seg.Offset;
					int bytesRead = Read(dataBuffer, 0, seg.Length);
					serverFile.Write(dataBuffer, offset, bytesRead);
					sizeRemaining -= bytesRead;
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
	}

	#endregion

	#region IServerFile

	/// <summary>
	/// Interface to access a file on the server.
	/// </summary>
	public interface IServerFile
	{
		/// <summary>
		/// Get the Node that represents this file from the server.
		/// The file must be closed if null is not returned.
		/// </summary>
		/// <param name="nodeID">The ID of the node to get.</param>
		/// <returns>The node. null if failed.</returns>
		SyncNode GetFileNode(string nodeID);

		/// <summary>
		/// Put the node that represents the file to the server.
		/// </summary>
		/// <param name="snode">The node to put.</param>
		/// <returns>The status of the put.</returns>
		SyncStatus PutFileNode(SyncNode snode);

		/// <summary>
		/// Get the hash map of the file. This can be used to do a delta sync.
		/// </summary>
		/// <param name="BlockSize">The size of chuncks to hash.</param>
		/// <returns>The hash map or null if failed.</returns>
		HashData[] GetHashMap(int BlockSize);

		/// <summary>
		/// Copy data from the current file to the new file.
		/// </summary>
		/// <param name="originalOffset">The offset in the original file.</param>
		/// <param name="offset">The offset in the new file.</param>
		/// <param name="count">The number of bytes to copy.</param>
		void Copy(long originalOffset, long offset, int count);

		/// <summary>
		/// Read data from the server file.
		/// </summary>
		/// <param name="offset">The offset in the file to begin the read.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="buffer">The data that was read.</param>
		/// <returns>The number of bytes read.</returns>
		int Read(long offset, int count, out byte[] buffer);

		/// <summary>
		/// Write data to the file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset to write at.</param>
		/// <param name="count">The number of bytes to write.</param>
		void Write(byte[] buffer, long offset, int count);

		/// <summary>
		/// Close the file and cleanup any resources.
		/// This must be called ater a successful call to PutFileNode.
		/// </summary>
		/// <param name="commit">true if the files should be commited.</param>
		/// <returns>The status of the commit.</returns>
		SyncNodeStatus CloseFileNode(bool commit);

		/// <summary>
		/// Close the file and cleanup any resources.
		/// This must be called ater a successful call to GetFileNode.
		/// </summary>
		void CloseFileNode();
	}

	#endregion

	#region WsServerFile

	/// <summary>
	/// 
	/// </summary>
	public class WsServerFile : IServerFile
	{
		protected SimiasSyncService		service;
		Exception				exception;
		ManualResetEvent		asyncEvent = new ManualResetEvent(true);
		

		/// <summary>
		/// Constructs a object that can be used to sync a file to the server.
		/// </summary>
		/// <param name="webService">The Sync web service.</param>
		internal WsServerFile(SimiasSyncService webService)
		{
			service = webService;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void WriteCallback(IAsyncResult result)
		{
			try
			{
				service.EndWrite(result);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			asyncEvent.Set();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="result"></param>
		private void CopyCallback(IAsyncResult result)
		{
			try
			{
				service.EndCopy(result);
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			asyncEvent.Set();
		}

		#region IServerFile Members

		/// <summary>
		/// Get the Node that represents this file from the server.
		/// The file must be closed if null is not returned.
		/// </summary>
		/// <param name="nodeID">The ID of the node to get.</param>
		/// <returns>The node. null if failed.</returns>
		public virtual SyncNode GetFileNode(string nodeID)
		{
			return service.GetFileNode(nodeID);
		}

		/// <summary>
		/// Put the node that represents the file to the server.
		/// </summary>
		/// <param name="snode">The node to put.</param>
		/// <returns>The status of the put.</returns>
		public virtual SyncStatus PutFileNode(SyncNode snode)
		{
			return service.PutFileNode(snode);
		}

		/// <summary>
		/// Get the hash map of the file. This can be used to do a delta sync.
		/// </summary>
		/// <param name="blockSize">The size of chuncks to hash.</param>
		/// <returns>The hash map or null if failed.</returns>
		public virtual HashData[] GetHashMap(int blockSize)
		{
			return service.GetHashMap(blockSize);
		}

		/// <summary>
		/// Copy data from the current file to the new file.
		/// </summary>
		/// <param name="originalOffset">The offset in the original file.</param>
		/// <param name="offset">The offset in the new file.</param>
		/// <param name="count">The number of bytes to copy.</param>
		public virtual void Copy(long originalOffset, long offset, int count)
		{
			/*
			asyncEvent.WaitOne();
			if (exception == null)
			{
				asyncEvent.Reset();
				service.BeginCopy(originalOffset, offset, count, new AsyncCallback(CopyCallback), null);
			}
			else
			{
				throw(exception);
			}
			*/
			service.Copy(originalOffset, offset, count);
		}

		/// <summary>
		/// Read data from the server file.
		/// </summary>
		/// <param name="offset">The offset in the file to begin the read.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="buffer">The data that was read.</param>
		/// <returns>The number of bytes read.</returns>
		public virtual int Read(long offset, int count, out byte[] buffer)
		{
			return service.Read(out buffer, offset, count);
		}

		/// <summary>
		/// Write data to the file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset to write at.</param>
		/// <param name="count">The number of bytes to write.</param>
		public virtual void Write(byte[] buffer, long offset, int count)
		{
			/*
			asyncEvent.WaitOne();
			if (exception == null)
			{
				asyncEvent.Reset();
				service.BeginWrite(buffer, offset, count, new AsyncCallback(WriteCallback), null);
			}
			else
			{
				throw(exception);
			}
			*/
			service.Write(buffer, offset, count);
		}

		/// <summary>
		/// Close the file and cleanup any resources.
		/// This must be called ater a successful call to PutFileNode.
		/// </summary>
		/// <param name="commit">true if the files should be commited.</param>
		/// <returns>The status of the commit.</returns>
		public virtual SyncNodeStatus CloseFileNode(bool commit)
		{
			return service.CloseFileNode(commit);
		}

		/// <summary>
		/// Close the file and cleanup any resources.
		/// This must be called ater a successful call to GetFileNode.
		/// </summary>
		public virtual void CloseFileNode()
		{
			service.CloseFileNode(false);
		}
		#endregion
	}

	#endregion

	#region HttpServerFile

	class HttpServerFile : WsServerFile
	{
		string url;
		/// <summary>
		/// Construct an HttpServerWriteFile to upload a file to the server.
		/// </summary>
		/// <param name="webService">The web service.</param>
		internal HttpServerFile(SimiasSyncService webService) :
			base(webService)
		{
			url = service.Url;
			url = url.Substring(0, url.LastIndexOf('/') + 1) + "SyncHandler.ashx";
		}

		private HttpWebRequest GetRequest()
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/octet-stream";
			request.Credentials = service.Credentials;
			request.CookieContainer = service.CookieContainer;
			return request;
		}

		#region IServerFile Members

		/// <summary>
		/// Copy data from the current file to the new file.
		/// </summary>
		/// <param name="originalOffset">The offset in the original file.</param>
		/// <param name="offset">The offset in the new file.</param>
		/// <param name="count">The number of bytes to copy.</param>
		public override void Copy(long originalOffset, long offset, int count)
		{
			HttpWebRequest request = GetRequest();
			WebHeaderCollection headers = request.Headers;
			request.Method = "POST";
			request.ContentLength = 0;
			headers.Add(SyncHttp.CopyOffset, originalOffset.ToString());
			headers.Add(SyncHttp.SyncRange, offset.ToString() + '-' + (offset + count).ToString());
			headers.Add(SyncHttp.SyncOperation, SyncHttp.Operation.Copy.ToString());
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			response.Close();
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new SimiasException(response.StatusDescription);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blocks"></param>
		/// <param name="blockSize"></param>
		/// <returns></returns>
		public HttpWebResponse Read(long[] blocks, int blockSize)
		{
			HttpWebRequest request = GetRequest();
			WebHeaderCollection headers = request.Headers;
			request.Method = "POST";
			headers.Add(SyncHttp.SyncBlocks, blocks.Length.ToString());
			headers.Add(SyncHttp.BlockSize, blockSize.ToString());
			headers.Add(SyncHttp.SyncOperation, SyncHttp.Operation.Read.ToString());
			request.ContentLength = blocks.Length * 8;
			Stream outStream = request.GetRequestStream();
			foreach (long block in blocks)
			{
				outStream.Write(BitConverter.GetBytes(block), 0, 8);
			}
			outStream.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			if (response.StatusCode == HttpStatusCode.OK)
			{
				return response;
			}
			response.Close();
			throw new SimiasException(response.StatusDescription);
		}

		/// <summary>
		/// Read data from the server file.
		/// </summary>
		/// <param name="offset">The offset in the file to begin the read.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="buffer">The data that was read.</param>
		/// <returns>The number of bytes read.</returns>
		public override int Read(long offset, int count, out byte[] buffer)
		{
			HttpWebRequest request = GetRequest();
			WebHeaderCollection headers = request.Headers;
			request.Method = "GET";
			headers.Add(SyncHttp.SyncRange, offset.ToString() + '-' + (offset + count).ToString());
			headers.Add(SyncHttp.SyncOperation, SyncHttp.Operation.Read.ToString());
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					buffer = new byte[count];
					int length = response.GetResponseStream().Read(buffer, 0, count);
					return length;
				}
			}
			finally
			{
				response.Close();
			}
			throw new SimiasException(response.StatusDescription);
		}

		/// <summary>
		/// Write data to the file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset to write at.</param>
		/// <param name="count">The number of bytes to write.</param>
		public override void Write(byte[] buffer, long offset, int count)
		{
			HttpWebRequest request = GetRequest();
			WebHeaderCollection headers = request.Headers;
			request.Method = "POST";
			request.ContentLength = count;
			headers.Add(SyncHttp.SyncOperation, SyncHttp.Operation.Write.ToString());
			headers.Add(SyncHttp.SyncRange, offset.ToString() + "-" + ((long)(offset + count)).ToString());
			Stream outStream = request.GetRequestStream();
			outStream.Write(buffer, 0, count);
			outStream.Close();
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw new SimiasException(response.StatusDescription);
				}
			}
			finally
			{
				response.Close();
			}
		}

		/// <summary>
		/// Write data to the file.
		/// </summary>
		/// <param name="inStream">The data to write.</param>
		/// <param name="offset">The offset to write at.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(Stream inStream, long offset, long count)
		{
			HttpWebRequest request = GetRequest();
			HttpWebResponse response = null;
			WebHeaderCollection headers = request.Headers;
			request.Method = "POST";
			request.ContentLength = count;
			headers.Add(SyncHttp.SyncOperation, SyncHttp.Operation.Write.ToString());
			headers.Add(SyncHttp.SyncRange, offset.ToString() + "-" + ((long)(offset + count)).ToString());
			Stream outStream = request.GetRequestStream();
			byte[] buffer = new byte[4096];
			long bytesSent = 0;
			while (bytesSent < count)
			{
				int bytesRead = inStream.Read(buffer, 0, buffer.Length);
				if (bytesRead == 0)
					break;
				outStream.Write(buffer, 0, bytesRead);
				bytesSent += bytesRead;
			}
			outStream.Close();
			response = (HttpWebResponse)request.GetResponse();
			try
			{
				if (response.StatusCode != HttpStatusCode.OK)
				{
					throw new SimiasException(response.StatusDescription);
				}
			}
			finally
			{
				response.Close();
			}
		}

		#endregion
	}

	#endregion
}
