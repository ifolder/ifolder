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
using System.Xml;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Class used to keep track of the file Blocks and hash
	/// codes assosiated with the block.
	/// </summary>
	public class HashData
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
	}

	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerInFile : InFile
	{
		SyncNode snode;
		#region Constructors

		/// <summary>
		/// Contructs a ServerFile object that is used to sync a file from the client.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync.</param>
		public ServerInFile(Collection collection, SyncNode snode) :
			base(collection)
		{
			this.snode = snode;
		}

		#endregion

		public void Open()
		{
			if (snode == null)
			{
				throw new SimiasException(string.Format("Node {0} not found on server.", nodeID));
			}
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(snode.node);
			node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
			collection.ImportNode(node, false, 0);
			node.IncarnationUpdate = node.LocalIncarnation;
			base.Open(node);
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <param name="commit">True if changes should be commited.</param>
		/// <returns>The status of the sync.</returns>
		public new SyncNodeStatus Close(bool commit)
		{
			SyncNodeStatus status = new SyncNodeStatus();
			status.nodeID = node.ID;
			status.status = SyncNodeStatus.SyncStatus.Success;
			if (commit)
			{
				try
				{
					collection.Commit(node);
				}
				catch (CollisionException)
				{
					commit = false;
					status.status = SyncNodeStatus.SyncStatus.UpdateConflict;
				}
				catch
				{
					commit = false;
					status.status = SyncNodeStatus.SyncStatus.ServerFailure;
				}
			}
			base.Close(commit);
			return status;
		}

		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		/// <returns></returns>
		public HashData[] GetHashMap()
		{
			if (Length <= BlockSize)
			{
				return null;
			}

			int				blockCount = (int)(Length / BlockSize) + 1;
			HashData[]		list = new HashData[blockCount];
			byte[]			buffer = new byte[BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			lock (this)
			{
				// Compute the hash codes.
				ReadPosition = 0;
				int i = 0;
				while ((bytesRead = Read(buffer, 0, BlockSize)) != 0)
				{
					HashData entry = new HashData();
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
			ReadPosition = offset;
			return base.Read(buffer, 0, count);
		}

		/// <summary>
		/// Write the binary data to the file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset in the file where the data is to be written.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(byte[] buffer, long offset, int count)
		{
			WritePosition = offset;
			base.Write(buffer, 0, count);
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
				ReadPosition = originalOffset;
				WritePosition = offset;
				while (count > 0)
				{
					int bytesRead = Read(buffer, 0, bufferSize);
					Write(buffer, 0, bytesRead);
					count -= bytesRead;
				}
			}
		}
	}

	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerOutFile : OutFile
	{
		SyncNode snode;
		#region Constructors

		/// <summary>
		/// Constructs a ServerFile object that can be used to sync a file in from a client.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync down</param>
		public ServerOutFile(Collection collection, BaseFileNode node) :
			base(collection)
		{
			this.node = node;
		}

		#endregion

		public void Open()
		{
			base.Open(node);
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <returns>The status of the sync.</returns>
		public SyncNodeStatus Close()
		{
			SyncNodeStatus status = new SyncNodeStatus();
			status.nodeID = node.ID;
			status.status = SyncNodeStatus.SyncStatus.Success;
			base.Close();
			return status;
		}

		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		/// <returns></returns>
		public HashData[] GetHashMap()
		{
			if (Length <= BlockSize)
			{
				return null;
			}

			int				blockCount = (int)(Length / BlockSize) + 1;
			HashData[]		list = new HashData[blockCount];
			byte[]			buffer = new byte[BlockSize];
			StrongHash		sh = new StrongHash();
			WeakHash		wh = new WeakHash();
			int				bytesRead;
			int				currentBlock = 0;
		
			lock (this)
			{
				// Compute the hash codes.
				ReadPosition = 0;
				int i = 0;
				while ((bytesRead = Read(buffer, 0, BlockSize)) != 0)
				{
					HashData entry = new HashData();
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
			ReadPosition = offset;
			return base.Read(buffer, 0, count);
		}

	}
}
