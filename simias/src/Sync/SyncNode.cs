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
using Simias.Storage;
using Simias.Client;

namespace Simias.Sync
{
	#region SyncNodeType

	/// <summary>
	/// Special Node types that sync deals with.
	/// </summary>
	public enum SyncNodeType : byte
	{
		/// <summary>
		/// This is a generic node.
		/// </summary>
		Generic,
		/// <summary>
		/// This node represents a file in the FS.
		/// </summary>
		File,
		/// <summary>
		/// This node represents a directory in the FS.
		/// </summary>
		Directory,
		/// <summary>
		/// This node represents a deleted node.
		/// </summary>
		Tombstone
	}

	#endregion

	#region SyncNodeInfo

	/// <summary>
	/// class to represent the minimal information that the sync code needs
	/// to know about a node to determine if it needs to be synced.
	/// </summary>
	public class SyncNodeInfo: IComparable
	{
		#region Fields

		/// <summary>
		/// The Node ID.
		/// </summary>
		public string ID;
		/// <summary>
		/// The local incarnation for the node.
		/// </summary>
		public ulong LocalIncarnation;
		/// <summary>
		/// The Master incarnation for this node.
		/// </summary>
		public ulong MasterIncarnation;
		/// <summary>
		///	The base type of this node. 
		/// </summary>
		public SyncNodeType NodeType;
		/// <summary>
		/// The SyncOperation to perform on this node.
		/// </summary>
		public SyncOperation Operation;
		/// <summary>
		/// The size of this instance serialized.
		/// If fields are added this needs to be updated.
		/// </summary>
		public static int InstanceSize = 34;

		#endregion

		#region Consturctor

		/// <summary>
		/// 
		/// </summary>
		public SyncNodeInfo()
		{
		}

		/// <summary>
		/// Constructs a SyncNodeInfo from a ShallowNode.
		/// </summary>
		/// <param name="collection"></param>
		/// <param name="sn"></param>
		public SyncNodeInfo(Collection collection, ShallowNode sn) :
			this (new Node(collection, sn))
		{
		}

		/// <summary>
		/// Construct a SyncNodeInfo from a stream.
		/// </summary>
		/// <param name="reader"></param>
		internal SyncNodeInfo(BinaryReader reader)
		{
			this.ID = new Guid(reader.ReadBytes(16)).ToString();
			this.LocalIncarnation = reader.ReadUInt64();
			this.MasterIncarnation = reader.ReadUInt64();
			this.NodeType = (SyncNodeType)reader.ReadByte();
			this.Operation = (SyncOperation)reader.ReadByte();
		}

		/// <summary>
		/// Construct a SyncNodeStamp from a Node.
		/// </summary>
		/// <param name="node">the node to use.</param>
		internal SyncNodeInfo(Node node)
		{
			this.ID = node.ID;
			this.LocalIncarnation = node.LocalIncarnation;
			this.MasterIncarnation = node.MasterIncarnation;
			this.NodeType = GetSyncNodeType(node.Type);
			this.Operation = SyncOperation.Unknown;
		}

		/// <summary>
		/// Consturct a SyncNodeStamp from a ChangeLogRecord.
		/// </summary>
		/// <param name="record">The record to use.</param>
		internal SyncNodeInfo(ChangeLogRecord record)
		{
			this.ID = record.EventID;
			this.LocalIncarnation = record.SlaveRev;
			this.MasterIncarnation = record.MasterRev;
			this.NodeType = GetSyncNodeType(record.Type.ToString());
			switch (record.Operation)
			{
				case ChangeLogRecord.ChangeLogOp.Changed:
					this.Operation = SyncOperation.Change;
					break;
				case ChangeLogRecord.ChangeLogOp.Created:
					this.Operation = SyncOperation.Create;
					break;
				case ChangeLogRecord.ChangeLogOp.Deleted:
					this.Operation = SyncOperation.Delete;
					break;
				case ChangeLogRecord.ChangeLogOp.Renamed:
					this.Operation = SyncOperation.Rename;
					break;
				default:
					this.Operation = SyncOperation.Unknown;
					break;
			}
		}

		#endregion
		
		#region publics

		/// <summary>
		/// Serializes this instance into a stream.
		/// </summary>
		/// <param name="writer">The stream to serialize to.</param>
		internal void Serialize(BinaryWriter writer)
		{
			writer.Write(new Guid(ID).ToByteArray());
			writer.Write(LocalIncarnation);
			writer.Write(MasterIncarnation);
			writer.Write((byte)NodeType);
			writer.Write((byte)Operation);
		}


		/// <summary> implement some convenient operator overloads </summary>
		public int CompareTo(object obj)
		{
			return ID.CompareTo(((SyncNodeInfo)obj).ID);
		}

		#endregion

		#region privates

		/// <summary>
		/// Converts the base type string into a SyncNodeType
		/// </summary>
		/// <param name="baseType">The base type.</param>
		/// <returns>the SyncNodeType.</returns>
		private SyncNodeType GetSyncNodeType(string baseType)
		{
			if (baseType == NodeTypes.DirNodeType)
			{
				return SyncNodeType.Directory;
			}
			else if (baseType == NodeTypes.FileNodeType || baseType == NodeTypes.StoreFileNodeType)
			{
				return SyncNodeType.File;
			}
			else
			{
				return SyncNodeType.Generic;
			}
		}

		#endregion
	}

	#endregion

	#region SyncNode

	/// <summary>
	/// This is the object that is used to sync a node.
	/// </summary>
	public class SyncNode : SyncNodeInfo
	{
		#region fields

		/// <summary>
		/// The node as an XML string.
		/// </summary>
		public string node;

		/// <summary>
		/// The size of this instance serialized.
		/// </summary>
		public new int InstanceSize
		{
			get { return SyncNodeInfo.InstanceSize + node.Length; }
		}
		
		#endregion

		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		internal SyncNode()
		{
		}

		/// <summary>
		/// Create a SyncNode from a Node.
		/// </summary>
		/// <param name="node">The node used to create the sync node.</param>
		internal SyncNode(Node node) :
			base(node)
		{
			this.node = node.Properties.ToString(true);
		}

		/// <summary>
		/// Create a SyncNode from a stream.
		/// </summary>
		/// <param name="reader">The stream containing the SyncNode.</param>
		internal SyncNode(BinaryReader reader) :
			base(reader)
		{
			this.MasterIncarnation = reader.ReadUInt64();
			node = reader.ReadString();
		}

		#endregion

		#region publics

		/// <summary>
		/// Serialize the SyncNode to the stream.
		/// </summary>
		/// <param name="writer"></param>
		internal new void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);
			writer.Write(MasterIncarnation);
			writer.Write(node);
		}

		#endregion
	}

	#endregion

	#region SyncStatus

	/// <summary>
	/// The status codes for a sync attempt.
	/// </summary>
	public enum SyncStatus : byte
	{
		/// <summary>
		/// The operation was successful.
		/// </summary>
		Success,
		/// <summary>
		/// There was an error.
		/// </summary>
		Error,
		/// <summary> 
		/// node update was aborted due to update from other client 
		/// </summary>
		UpdateConflict,
		/// <summary> 
		/// node update was completed, but temporary file could not be moved into place
		/// </summary>
		FileNameConflict,
		/// <summary> 
		/// node update was probably unsuccessful, unhandled exception on the server 
		/// </summary>
		ServerFailure,
		/// <summary> 
		/// node update is in progress 
		/// </summary>
		InProgess,
		/// <summary>
		/// The File is in use.
		/// </summary>
		InUse,
		/// <summary>
		/// The Server is busy.
		/// </summary>
		Busy,
		/// <summary>
		/// The client passed invalid data.
		/// </summary>
		ClientError,
		/// <summary>
		/// The policy doesnot allow this file.
		/// </summary>
		Policy,
		/// <summary>
		/// Insuficient rights for the operation.
		/// </summary>
		Access,
		/// <summary>
		/// The collection is Locked.
		/// </summary>
		Locked,
	}

	#endregion


	/// <summary>
	/// Used to report the status of a sync.
	/// </summary>
	public class SyncNodeStatus
	{
		#region Fields

		/// <summary>
		/// The ID of the node.
		/// </summary>
		public string		nodeID;
		/// <summary>
		/// The status of the sync.
		/// </summary>
		public SyncStatus	status;

		/// <summary>
		/// The size of this instance serialized.
		/// </summary>
		public int InstanceSize = 17;

		#endregion
	
		/// <summary>
		/// Constructs a SyncNodeStatus object.
		/// </summary>
		internal SyncNodeStatus()
		{
			status = SyncStatus.Error;
		}

		/// <summary>
		/// Create a SyncNodeStatus from a stream.
		/// </summary>
		/// <param name="reader">The stream containing the SyncNode.</param>
		internal SyncNodeStatus(BinaryReader reader)
		{
			this.nodeID = new Guid(reader.ReadBytes(16)).ToString();
			this.status = (SyncStatus)reader.ReadByte();
		}

		/// <summary>
		/// Serialize the SyncNodeStatus to the stream.
		/// </summary>
		/// <param name="writer"></param>
		internal void Serialize(BinaryWriter writer)
		{
			writer.Write(new Guid(nodeID).ToByteArray());
			writer.Write((byte)status);
		}
	}
}
