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
using Simias.Sync.Delta;

namespace Simias.Sync
{
	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerInFile : InFile
	{
		SyncNode	snode;
		SyncPolicy	policy;

		#region Constructors

		/// <summary>
		/// Contructs a ServerFile object that is used to sync a file from the client.
		/// </summary>
		/// <param name="collection">The collection the node belongs to.</param>
		/// <param name="snode">The node to sync.</param>
		/// <param name="policy">The policy to check the file against.</param>
		public ServerInFile(SyncCollection collection, SyncNode snode, SyncPolicy policy) :
			base(collection)
		{
			this.snode = snode;
			this.policy = policy;
		}

		#endregion

		/// <summary>
		/// Open the server file and validate access.
		/// </summary>
		/// <returns>Status of the open.</returns>
		public SyncStatus Open()
		{
			if (snode == null)
			{
				return SyncStatus.ClientError;
			}
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(snode.node);
			node = (BaseFileNode)Node.NodeFactory(collection.StoreReference, xNode);
			if (!policy.Allowed(node))
			{
				return SyncStatus.Policy;
			}
			collection.ImportNode(node, true, snode.MasterIncarnation);
			node.IncarnationUpdate = node.LocalIncarnation;
			base.Open(node);
			return SyncStatus.Success;
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
			status.status = SyncStatus.ClientError;
			if (commit)
			{
				status.status = SyncStatus.Success;
				try
				{
					collection.Commit(node);
				}
				catch (CollisionException)
				{
					commit = false;
					status.status = SyncStatus.UpdateConflict;
				}
				catch
				{
					commit = false;
					status.status = SyncStatus.ServerFailure;
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
			return HashMap.GetHashMap(inStream);
		}

		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		/// <returns></returns>
		public string GetHashMapFile()
		{
			string mapFile = GetMapFileName();
			FileInfo mapFi = new FileInfo(mapFile);
			FileInfo fi = new FileInfo(file);
			if (mapFi.Exists)
			{
				if (mapFi.LastWriteTime == fi.LastWriteTime)
					return mapFile;
			}
			else
			{
				try { mapFi.Delete(); }
				catch {}
			}
			return null;
		}
	}

	/// <summary>
	/// Class used on ther server to determine the changes from the client file.
	/// </summary>
	public class ServerOutFile : OutFile
	{
		#region Constructors

		/// <summary>
		/// Constructs a ServerFile object that can be used to sync a file in from a client.
		/// </summary>
		/// /// <param name="collection">The collection the node belongs to.</param>
		/// <param name="node">The node to sync down</param>
		public ServerOutFile(SyncCollection collection, BaseFileNode node) :
			base(collection)
		{
			this.node = node;
		}

		#endregion

		/// <summary>
		/// Open the file for download access.
		/// </summary>
		/// <param name="sessionID">The unique session ID.</param>
		public void Open(string sessionID)
		{
			base.Open(node, sessionID);
		}

		/// <summary>
		/// Called to close the file.
		/// </summary>
		/// <returns>The status of the sync.</returns>
		public new SyncNodeStatus Close()
		{
			SyncNodeStatus status = new SyncNodeStatus();
			status.nodeID = node.ID;
			status.status = SyncStatus.Success;
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
			return HashMap.GetHashMap(this.outStream);
		}


		/// <summary>
		/// Get a hashed map of the file.  This can then be
		/// used to create an upload or download filemap.
		/// </summary>
		public string GetHashMapFile()
		{
			string mapFile = GetMapFileName();
			FileInfo mapFi = new FileInfo(mapFile);
			FileInfo fi = new FileInfo(file);
			if (mapFi.Exists)
			{
				if (mapFi.LastWriteTime == fi.LastWriteTime)
					return mapFile;
			}
			else
			{
				try { mapFi.Delete(); }
				catch {}
			}
			return null;
		}
	}
}
