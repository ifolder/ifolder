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
 *  Author: Dale Olds <olds@novell.com>
 *  Author: Russ Young
 * 
 ***********************************************************************/
using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;

using Simias.Storage;
using Simias;

namespace Simias.Sync
{
	/// <summary>
	/// Class used to set up the state for a sync pass.
	/// </summary>
	public class SyncStartInfo
	{
		/// <summary>
		/// The collection to sync.
		/// </summary>
		public string			CollectionID;
		/// <summary>
		/// The sync context.
		/// </summary>
		public string			Context;
		/// <summary>
		/// True if only changes since last sync are wanted.
		/// </summary>
		public bool				ChangesOnly;
		/// <summary>
		/// True if the client has changes. Used to Determine if there is work.
		/// </summary>
		public bool				ClientHasChanges;
		/// <summary>
		/// The Status of this sync.
		/// </summary>
		public SyncColStatus	Status;
		/// <summary>
		/// The access allowed to the collection.
		/// </summary>
		public Access.Rights	Access;
	}

	public enum SyncColStatus
	{
		/// <summary>
		/// We need to sync.
		/// </summary>
		Success,
		/// <summary>
		/// There is nothing to do.
		/// </summary>
		NoWork,
		/// <summary>
		/// The collection was not found.
		/// </summary>
		NotFound,
		/// <summary>
		/// Someone is sync-ing now come back latter.
		/// </summary>
		Busy,
	};
	
	/// <summary>
	/// The node represented as the XML string.
	/// </summary>
	[Serializable]
	public class SyncNode
	{
		/// <summary>
		/// The node as an XML string.
		/// </summary>
		public string node;
		/// <summary>
		/// The Master incarnation that this node is derived from.
		/// </summary>
		public ulong expectedIncarn;
		/// <summary>
		/// The operation that was that needs to be synced.
		/// </summary>
		public SyncOperation operation;
	}

	public enum SyncOperation
	{
		/// <summary>
		/// The node exists but no log record has been created.
		/// Do a brute force sync.
		/// </summary>
		Unknown,
		/// <summary>
		/// Node object was created.
		/// </summary>
		Create,
		/// <summary>
		/// Node object was deleted.
		/// </summary>
		Delete,
		/// <summary>
		/// Node object was changed.
		/// </summary>
		Change,
		/// <summary>
		/// Node object was renamed.
		/// </summary>
		Rename
	};

	/// <summary>
	/// class to represent the minimal information that the sync code needs
	/// to know about a node to determine if it needs to be synced.
	/// </summary>
	[Serializable]
	public class SyncNodeStamp: IComparable
	{
		/// <summary>
		/// The Node ID.
		/// </summary>
		public string ID;

		/// <summary>
		/// The Master incarnation for the node.
		/// </summary>
		public ulong Incarnation;

		/// <summary>
		///	The base type of this node. 
		/// </summary>
		public string BaseType;

		/// <summary>
		/// 
		/// </summary>
		public SyncOperation Operation;

		public SyncNodeStamp()
		{
		}

		internal SyncNodeStamp(string id, ulong incarnation, string baseType, SyncOperation operation)
		{
			ID = id;
			Incarnation = incarnation;
			BaseType = baseType;
			Operation = operation;
		}

		/// <summary> implement some convenient operator overloads </summary>
		public int CompareTo(object obj)
		{
			return ID.CompareTo(((SyncNodeStamp)obj).ID);
		}
	}


	/// <summary>
	/// Used to report the status of a sync.
	/// </summary>
	[Serializable]
	public class SyncNodeStatus
	{
		/// <summary>
		/// The ID of the node.
		/// </summary>
		public string		nodeID;
		/// <summary>
		/// The status of the sync.
		/// </summary>
		public SyncStatus	status;
	
		public enum SyncStatus
		{
			/// <summary>
			/// The operation was successful.
			/// </summary>
			Success,
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
		}
	}

//---------------------------------------------------------------------------
/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
public class SyncService
{
	public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncService));
	SyncCollection collection;
	Store			store;
	Member			member;
	Access.Rights	rights = Access.Rights.Deny;
	ArrayList		NodeList = new ArrayList();
	ServerFile		file;
	
	/// <summary>
	/// public ctor 
	/// </summary>
	public SyncService(string collectionID)
	{
		
	}

	
	/// <summary>
	/// start sync of this collection -- perform basic role checks and dredge server file system
	/// </summary>
	/// <param name="si">The start info to initialize the sync.</param>
	/// <param name="user">This is temporary.</param>
	public SyncNodeStamp[] Start(ref SyncStartInfo si, string user)
	{
		si.Status = SyncColStatus.Success;
		si.Access = Access.Rights.Deny;
		SyncNodeStamp[] nodes = null;
		
		store = Store.GetStore();
		Collection col = store.GetCollectionByID(si.CollectionID);
		if (col == null)
		{
			si.Status = SyncColStatus.NotFound;
			return null;
		}
		
		collection = new SyncCollection(col);
		// BUGBUG SyncAccess access = SyncAccess.Busy;

		// Check our rights.
		string userID = Thread.CurrentPrincipal.Identity.Name;
		if ((userID == null) || (userID.Length == 0))
		{
			// Kludge: for now trust the client.  this need to be removed before shipping.
			userID = user;
		}
		member = collection.GetMemberByID(userID);
		if (member != null)
		{
			collection.Impersonate(member);
			rights = member.Rights;
			si.Access = rights;
			log.Info("Starting Sync of {0} for {1} rights : {2}.", collection, member.Name, rights);
		}
		else if (userID == collection.ProxyUserID)
		{
			rights = Access.Rights.Admin;
			si.Access = rights;
			log.Info("Sync session starting for {0}", userID);
		}

		switch (rights)
		{
			case Access.Rights.Admin:
			case Access.Rights.ReadOnly:
			case Access.Rights.ReadWrite:
				// See if there is any work to do before we try to get the lock.
				if (si.ChangesOnly)
				{
					// we only need the changes.
					si.ChangesOnly = GetChangedNodeStamps(out nodes, ref si.Context);
				}

				// If we failed to get the changes get all.
				if (!si.ChangesOnly)
				{
					// We need to get all of the nodes.
					si.Context = new ChangeLogReader(collection).GetEventContext().ToString();
					nodes = GetNodeStamps();
					if (nodes.Length == 0)
					{
						rights = Access.Rights.Deny;
						si.Access = rights;
					}
				}
				else if (!si.ClientHasChanges && nodes.Length == 0)
				{
					si.Status = SyncColStatus.NoWork;
					nodes = null;
				}
				break;
			case Access.Rights.Deny:
				nodes = null;
				si.Status = SyncColStatus.NotFound;
				break;
		}
		return nodes;
	}

	/// <summary>
	/// Called when done with the sync cycle.
	/// </summary>
	public void Stop()
	{
		collection.Revert();
		log.Info("Finished Sync of {0} for {1}.", collection, member != null ? member.Name : null);
		this.collection = null;
	}

	/// <summary>
	/// Checks if the current user has rights to perform the desired operation.
	/// </summary>
	/// <param name="desiredRights">The desired operation.</param>
	/// <returns>True if allowed.</returns>
	private bool IsAccessAllowed(Access.Rights desiredRights)
	{
		return (rights >= desiredRights) ? true : false;
	}

	void Import(Node node, ulong expectedIncarn)
	{
		collection.ImportNode(node, true, expectedIncarn);
		node.IncarnationUpdate = node.LocalIncarnation;
	}

	public SyncNodeStatus[] PutNonFileNodes(SyncNode [] nodes)
	{
		SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];

		// Try to commit all the nodes at once.
		int i = 0;
		foreach (SyncNode sn in nodes)
		{
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(sn.node);
			Node node = Node.NodeFactory(store, xNode);
			Import(node, sn.expectedIncarn);
			NodeList.Add(node);
			statusList[i] = new SyncNodeStatus();
			statusList[i].nodeID = node.ID;
			statusList[i++].status = SyncNodeStatus.SyncStatus.Success;
		}
		if (!CommitNonFileNodes())
		{
			i = 0;
			// If we get here the import failed try to commit the nodes one at a time.
			foreach (Node node in NodeList)
			{
				try
				{
					collection.Commit(node);
				}
				catch (CollisionException)
				{
					// The current node failed because of a collision.
					statusList[i++].status = SyncNodeStatus.SyncStatus.UpdateConflict;
				}
				catch
				{
					// Handle any other errors.
					statusList[i++].status = SyncNodeStatus.SyncStatus.ServerFailure;
				}
				i++;
			}
		}
		NodeList.Clear();
		return (statusList);
	}

	public bool CommitNonFileNodes()
	{
		try
		{
			Node[] commitList = new Node[NodeList.Count];
			NodeList.CopyTo(commitList, 0);
			if (NodeList.Count > 0)
			{
				collection.Commit((Node[])(NodeList.ToArray(typeof(Node))));
			}
		}
		catch
		{
			return false;
		}
		
		return true;
	}

	public SyncNodeStatus[] PutDirs(SyncNode [] nodes)
	{
		SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];

		int i = 0;
		foreach (SyncNode snode in nodes)
		{
			SyncNodeStatus status = new SyncNodeStatus();
			statusList[i++] = status;
			status.status = SyncNodeStatus.SyncStatus.ServerFailure;
			try
			{
				XmlDocument xNode = new XmlDocument();
				xNode.LoadXml(snode.node);
				DirNode node = (DirNode)Node.NodeFactory(store, xNode);
				log.Debug("Updating {0} {1} from client", node.Name, node.Type);

				status.nodeID = node.ID;
				Import(node, snode.expectedIncarn);
			
				// Get the old node to see if the node was renamed.
				DirNode oldNode = collection.GetNodeByID(node.ID) as DirNode;
				string path;
				if (node.IsRoot)
				{
					path = oldNode.GetFullPath(collection);
				}
				else
				{
					path = node.GetFullPath(collection);
					if (oldNode != null)
					{
						// We already have this node look for a rename.
						string oldPath = oldNode.GetFullPath(collection);
						if (oldPath != path)
						{
							Directory.Move(oldPath, path);
						}
					}
				}

				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				collection.Commit(node);
				status.status = SyncNodeStatus.SyncStatus.Success;
			}
			catch (CollisionException)
			{
				// The current node failed because of a collision.
				status.status = SyncNodeStatus.SyncStatus.UpdateConflict;
			}
			catch {}
		}
		return statusList;
	}

	public SyncNode[] GetNonFileNodes(string[] nodeIDs)
	{
		SyncNode[] nodes = new SyncNode[nodeIDs.Length];

		try
		{
			for (int i = 0; i < nodeIDs.Length; ++i)
			{
				Node node = collection.GetNodeByID(nodeIDs[i]);
				SyncNode snode = new SyncNode();
				snode.node = node.Properties.ToString(true);
				snode.expectedIncarn = node.MasterIncarnation;
				nodes[i] = snode;
			}
		}
		catch
		{
		}
		return nodes;
	}

	public SyncNodeStamp[] GetNodeStamps()
	{
		log.Debug("GetNodeStamps start");
		if (!IsAccessAllowed(Access.Rights.ReadOnly))
			throw new UnauthorizedAccessException("Current user cannot read this collection");

		ArrayList stampList = new ArrayList();
		foreach (ShallowNode sn in collection)
		{
			Node node;
			try
			{
				node = new Node(collection, sn);
				SyncNodeStamp stamp = new SyncNodeStamp(node.ID, node.LocalIncarnation, node.Type, SyncOperation.Unknown);
				stampList.Add(stamp);
			}
			catch (Storage.DoesNotExistException)
			{
				log.Debug("Node: Name:{0} ID:{1} Type:{2} no longer exists.", sn.Name, sn.ID, sn.Type);
				continue;
			}
		}
		log.Debug("Returning {0} SyncNodeStamps", stampList.Count);
		return (SyncNodeStamp[])stampList.ToArray(typeof(SyncNodeStamp));
	}



	/// <summary>
	/// 
	/// </summary>
	/// <param name="nodes"></param>
	/// <param name="context"></param>
	/// <returns></returns>
	public bool GetChangedNodeStamps(out SyncNodeStamp[] nodes, ref string context)
	{
		log.Debug("GetChangedNodeStamps Start");

		EventContext eventContext;
		// Create a change log reader.
		ChangeLogReader logReader = new ChangeLogReader( collection );
		nodes = null;
		bool more = true;
		try
		{	
			// Read the cookie from the last sync and then get the changes since then.
			if (context != null)
			{
				ArrayList changeList = null;
				ArrayList stampList = new ArrayList();

				eventContext = new EventContext(context);
				while(more)
				{
				more = logReader.GetEvents(eventContext, out changeList);
					foreach( ChangeLogRecord rec in changeList )
					{
						// Make sure the events are not for local only changes.
						if (((NodeEventArgs.EventFlags)rec.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
						{
							SyncOperation operation = SyncOperation.Unknown;
							switch (rec.Operation)
							{
								case ChangeLogRecord.ChangeLogOp.Changed:
									operation = SyncOperation.Change;
									break;
								case ChangeLogRecord.ChangeLogOp.Created:
									operation = SyncOperation.Create;
									break;
								case ChangeLogRecord.ChangeLogOp.Deleted:
									operation = SyncOperation.Delete;
									break;
								case ChangeLogRecord.ChangeLogOp.Renamed:
									operation = SyncOperation.Rename;
									break;
							}
							SyncNodeStamp stamp = new SyncNodeStamp(
								rec.EventID, rec.SlaveRev, rec.Type.ToString(), operation);
							stampList.Add(stamp);
						}
					}
				}
			
				log.Debug("Found {0} changed nodes.", stampList.Count);
				nodes = (SyncNodeStamp[])stampList.ToArray(typeof(SyncNodeStamp));
				context = eventContext.ToString();
				return true;
			}
		}
		catch
		{
		}

		// The cookie is invalid.  Get a valid cookie and save it for the next sync.
		eventContext = logReader.GetEventContext();
		if (eventContext != null)
			context = eventContext.ToString();
		return false;
	}

	public SyncNodeStatus[] DeleteNodes(string[] nodeIDs)
	{
		SyncNodeStatus[] nodeStatus = new SyncNodeStatus[nodeIDs.Length];
		
		int i = 0;
		foreach (string id in nodeIDs)
		{
			try
			{
				nodeStatus[i] = new SyncNodeStatus();
				nodeStatus[i].nodeID = id;
				Node node = collection.GetNodeByID(id);
				if (node == null)
				{
					log.Debug("Ignoring attempt to delete non-existent node {0}", id);
					nodeStatus[i].status = SyncNodeStatus.SyncStatus.Success;
					continue;
				}

				log.Info("Deleting {0}", node.Name);
				BaseFileNode bfn = node as BaseFileNode;
				if (bfn != null)
				{
					// If this is a file delete the file.
					File.Delete(bfn.GetFullPath(collection));
				}
				else
				{
					// If this is a directory remove the directory.
					DirNode dn = node as DirNode;
					if (dn != null)
						Directory.Delete(dn.GetFullPath(collection), true);
				}
						
				// Do a deep delete.
				Node[] deleted = collection.Delete(node, PropertyTags.Parent);
				collection.Commit(deleted);

				nodeStatus[i].status = SyncNodeStatus.SyncStatus.Success;
			}
			catch
			{
				nodeStatus[i].status = SyncNodeStatus.SyncStatus.ServerFailure;
			}
			i++;
		}
		return nodeStatus;
	}

	/// <summary>
	/// Put the node that represents the file to the server. This call is made to begin
	/// an upload of a file.  Close must be called to cleanup resources.
	/// </summary>
	/// <param name="node">The node to put to ther server.</param>
	/// <returns>True if successful.</returns>
	public bool PutFileNode(SyncNode node)
	{
		file = new ServerFile(collection, node);
		file.Open();
		return true;
	}

	/// <summary>
	/// Get the node that represents the file. This call is made to begin
	/// a download of a file.  Close must be called to cleanup resources.
	/// </summary>
	/// <param name="nodeID">The node to get.</param>
	/// <returns>The SyncNode.</returns>
	public SyncNode GetFileNode(string nodeID)
	{
		BaseFileNode node = collection.GetNodeByID(nodeID) as BaseFileNode;
		if (node != null)
		{
			file = new ServerFile(collection, node);
			file.Open();
			SyncNode snode = new SyncNode();
			snode.node = node.Properties.ToString(true);
			snode.expectedIncarn = node.MasterIncarnation;
			return snode;
		}
		return null;
	}

	/// <summary>
	/// Get a HashMap of the file.
	/// </summary>
	/// <param name="blockSize">The block size to be hashed.</param>
	/// <returns>The HashMap.</returns>
	public HashData[] GetHashMap(int blockSize)
	{
		return file.GetHashMap();
	}

	/// <summary>
	/// Write the included data to the new file.
	/// </summary>
	/// <param name="buffer">The data to write.</param>
	/// <param name="offset">The offset in the new file of where to write.</param>
	/// <param name="count">The number of bytes to write.</param>
	public void Write(byte[] buffer, long offset, int count)
	{
		file.Write(buffer, offset, count);
	}

	/// <summary>
	/// Copy data from the old file to the new file.
	/// </summary>
	/// <param name="oldOffset">The offset in the old (original file).</param>
	/// <param name="offset">The offset in the new file.</param>
	/// <param name="count">The number of bytes to copy.</param>
	public void Copy(long oldOffset, long offset, int count)
	{
		file.Copy(oldOffset, offset, count);
	}

	/// <summary>
	/// Read data from the currently opened file.
	/// </summary>
	/// <param name="buffer">Byte array of bytes read.</param>
	/// <param name="offset">The offset to begin reading.</param>
	/// <param name="count">The number of bytes to read.</param>
	/// <returns>The number of bytes read.</returns>
	public int Read(out byte[] buffer, long offset, int count)
	{
		buffer = null;
		return 0;
	}

	/// <summary>
	/// Close the current file.
	/// </summary>
	/// <param name="commit">True: commit the filenode and file.
	/// False: Abort the changes.</param>
	/// <returns>The status of the sync.</returns>
	public SyncNodeStatus CloseFileNode(bool commit)
	{
		return file.Close(commit);
	}

	/// <summary>
	/// simple version string, also useful to check remoting
	/// </summary>
	public string Version
	{
		get { return "0.0.0"; }
	}
}

//===========================================================================
}
