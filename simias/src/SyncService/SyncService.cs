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
using Simias.Client.Event;
using Simias.Policy;

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
		/// <summary>
		/// The user is not authenticated.
		/// </summary>
		AccessDenied,
	};
	
	/// <summary>
	/// The node represented as the XML string.
	/// </summary>
	[Serializable]
	public class SyncNode
	{
		/// <summary>
		/// The ID of the node.
		/// </summary>
		public string		nodeID;
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
		public ulong MasterIncarnation;

		/// <summary>
		/// The local incarnation for the node.
		/// </summary>
		public ulong LocalIncarnation;

		/// <summary>
		///	The base type of this node. 
		/// </summary>
		public string BaseType;

		/// <summary>
		/// 
		/// </summary>
		public SyncOperation Operation;

		/// <summary>
		/// 
		/// </summary>
		public SyncNodeStamp()
		{
		}

		/// <summary>
		/// Construct a SyncNodeStamp from a Node.
		/// </summary>
		/// <param name="node">the node to use.</param>
		internal SyncNodeStamp(Node node)
		{
			this.ID = node.ID;
			this.LocalIncarnation = node.LocalIncarnation;
			this.MasterIncarnation = node.MasterIncarnation;
			this.BaseType = node.Type;
			this.Operation = SyncOperation.Unknown;
		}

		/// <summary>
		/// Consturct a SyncNodeStamp from a ChangeLogRecord.
		/// </summary>
		/// <param name="record">The record to use.</param>
		internal SyncNodeStamp(ChangeLogRecord record)
		{
			this.ID = record.EventID;
			this.LocalIncarnation = record.SlaveRev;
			this.MasterIncarnation = record.MasterRev;
			this.BaseType = record.Type.ToString();
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
		}
	}

	/// <summary>
	/// Class to synchronize access to a collection.
	/// </summary>
	internal class CollectionLock
	{
		static Hashtable	CollectionLocks = new Hashtable();
		const int			queueDepth = 10;	
		int					count = 0;
		
		/// <summary>
		/// Gets a lock on the collection.
		/// </summary>
		/// <param name="collectionID">The collection to block on.</param>
		internal static CollectionLock GetLock(string collectionID)
		{
			CollectionLock cLock;
			lock (CollectionLocks)
			{
				cLock = (CollectionLock)CollectionLocks[collectionID];
				if (cLock == null)
				{
					cLock = new CollectionLock();
					CollectionLocks.Add(collectionID, cLock);
				}
			}
			lock (cLock)
			{
				if (cLock.count > queueDepth)
				{
					return null;
				}
				else
				{
					cLock.count++;
				}
			}

			return cLock;
		}

		/// <summary>
		/// Release the Lock on the collection.
		/// </summary>
		internal void ReleaseLock()
		{
			lock (this)
			{
				count--;
			}
		}

		/// <summary>
		/// Called to Synchronize access to this collection.
		/// </summary>
		internal void LockRequest()
		{
			Monitor.Enter(this);
		}

		/// <summary>
		/// Called to Release the request lock.
		/// </summary>
		internal void ReleaseRequest()
		{
			Monitor.Exit(this);
		}
	}

	
//---------------------------------------------------------------------------
/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
	public class SyncService
	{
		static Store store = Store.GetStore();
		public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncService));
		SyncCollection collection;
		CollectionLock	cLock;
		Member			member;
		Access.Rights	rights = Access.Rights.Deny;
		ArrayList		NodeList = new ArrayList();
		ServerInFile	inFile;
		ServerOutFile	outFile;
		SyncPolicy		policy;

		~SyncService()
		{
			Dispose(true);		
		}

		private void Dispose(bool inFinalize)
		{
			lock (this)
			{
				if (inFinalize)
				{
					GC.SuppressFinalize(this);
				}
				if (cLock != null)
				{
					cLock.ReleaseLock();
					cLock = null;
				}
			}
		}

		/// <summary>
		/// Get they sync policy for this collection.
		/// </summary>
		private SyncPolicy Policy
		{
			get
			{
				if (policy == null)
					policy = new SyncPolicy(collection);
				return policy;
			}
		}
	
		/// <summary>
		/// start sync of this collection -- perform basic role checks and dredge server file system
		/// </summary>
		/// <param name="si">The start info to initialize the sync.</param>
		/// <param name="user">This is temporary.</param>
		public SyncNodeStamp[] Start(ref SyncStartInfo si, string user)
		{
			si.Status = SyncColStatus.Success;
			rights = si.Access = Access.Rights.Deny;
			SyncNodeStamp[] nodes = null;
			cLock = null;
		
			Collection col = store.GetCollectionByID(si.CollectionID);
			if (col == null)
			{
				si.Status = SyncColStatus.NotFound;
				return null;
			}
		
			collection = new SyncCollection(col);

			// Check our rights.
			string userID = Thread.CurrentPrincipal.Identity.Name;
			if (userID != null)
			{
				// BUGBUG
				if (userID.Length == 0)
					userID = user;
				// End BUGBUG
				if (userID.Length != 0)
					member = collection.GetMemberByID(userID);
				if (member != null)
				{
					collection.Impersonate(member);
					rights = member.Rights;
					si.Access = rights;
					log.Info("Starting Sync of {0} for {1} rights : {2}.", collection.Name, member.Name, rights);
				}
			}
			else
			{
				si.Status = SyncColStatus.AccessDenied;
				return null;
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
						// Check if we have any work to do
						if (si.ChangesOnly && !si.ClientHasChanges && nodes.Length == 0)
						{
							si.Status = SyncColStatus.NoWork;
							nodes = null;
							break;
						}
					}

					cLock = CollectionLock.GetLock(collection.ID);
					if (cLock == null)
					{
						si.Status = SyncColStatus.Busy;
						return null;
					}
					// See if we need to return all of the nodes.
					if (!si.ChangesOnly)
					{
						cLock.LockRequest();
						try
						{
							// We need to get all of the nodes.
							si.Context = new ChangeLogReader(collection).GetEventContext().ToString();
							nodes = GetNodeStamps();
						}
						finally
						{
							cLock.ReleaseRequest();
						}
						if (nodes.Length == 0)
						{
							Dispose(false);
							rights = Access.Rights.Deny;
							si.Access = rights;
						}
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
			Dispose(false);
			collection.Revert();
			log.Info("Finished Sync of {0} for {1}.", collection.Name, member != null ? member.Name : null);
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

		private void Import(Node node, ulong expectedIncarn)
		{
			collection.ImportNode(node, true, expectedIncarn);
			node.IncarnationUpdate = node.LocalIncarnation;
		}

		public SyncNodeStatus[] PutNonFileNodes(SyncNode [] nodes)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadWrite))
			{
				return null;
			}

			SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];
			
			cLock.LockRequest();
			try
			{
				// Try to commit all the nodes at once.
				int i = 0;
				foreach (SyncNode sn in nodes)
				{
					statusList[i] = new SyncNodeStatus();
					statusList[i].status = SyncNodeStatus.SyncStatus.ServerFailure;
				
					if (sn != null)
					{
						statusList[i].nodeID = sn.nodeID;
						XmlDocument xNode = new XmlDocument();
						xNode.LoadXml(sn.node);
						Node node = Node.NodeFactory(store, xNode);
						Import(node, sn.expectedIncarn);
						NodeList.Add(node);
						statusList[i++].status = SyncNodeStatus.SyncStatus.Success;
					}
					else
					{
						NodeList.Add(null);
					}
				}
				if (!CommitNonFileNodes())
				{
					i = 0;
					// If we get here the import failed try to commit the nodes one at a time.
					foreach (Node node in NodeList)
					{
						if (node == null)
							continue;
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
			}
			finally
			{
				cLock.ReleaseRequest();
			}
			NodeList.Clear();
			return (statusList);
		}

		private bool CommitNonFileNodes()
		{
			try
			{
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
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadWrite))
				return null;

			SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];

			cLock.LockRequest();
			try
			{
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
			}
			finally
			{
				cLock.ReleaseRequest();
			}
			return statusList;
		}

		public SyncNode[] GetNonFileNodes(string[] nodeIDs)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadOnly))
				return null;

			SyncNode[] nodes = new SyncNode[nodeIDs.Length];

			cLock.LockRequest();
			try
			{
				for (int i = 0; i < nodeIDs.Length; ++i)
				{
					SyncNode snode = new SyncNode();
					try
					{
						nodes[i] = snode;
						snode.nodeID = nodeIDs[i];
						Node node = collection.GetNodeByID(nodeIDs[i]);
						snode.node = node.Properties.ToString(true);
						snode.expectedIncarn = node.MasterIncarnation;
					}
					catch
					{
					}
				}
			}
			finally
			{
				cLock.ReleaseRequest();
			}
			return nodes;
		}

		private SyncNodeStamp[] GetNodeStamps()
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
					SyncNodeStamp stamp = new SyncNodeStamp(node);
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
		private bool GetChangedNodeStamps(out SyncNodeStamp[] nodes, ref string context)
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
							SyncNodeStamp stamp = new SyncNodeStamp(rec);
							stampList.Add(stamp);
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nodeIDs"></param>
		/// <returns></returns>
		public SyncNodeStatus[] DeleteNodes(string[] nodeIDs)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadWrite))
				return null;

			SyncNodeStatus[] statusArray = new SyncNodeStatus[nodeIDs.Length];
		
			cLock.LockRequest();
			try
			{
				int i = 0;
				foreach (string id in nodeIDs)
				{
					SyncNodeStatus nStatus = new SyncNodeStatus();
					try
					{
						statusArray[i++] = nStatus;
						nStatus.nodeID = id;
						Node node = collection.GetNodeByID(id);
						if (node == null)
						{
							log.Debug("Ignoring attempt to delete non-existent node {0}", id);
							nStatus.status = SyncNodeStatus.SyncStatus.Success;
							continue;
						}

						log.Info("Deleting {0}", node.Name);
						// If this is a directory remove the directory.
						DirNode dn = node as DirNode;
						if (dn != null)
						{
							Directory.Delete(dn.GetFullPath(collection), true);
							// Do a deep delete.
							Node[] deleted = collection.Delete(node, PropertyTags.Parent);
							collection.Commit(deleted);
						}
						else
						{
							// If this is a file delete the file.
							BaseFileNode bfn = node as BaseFileNode;
							if (bfn != null)
								File.Delete(bfn.GetFullPath(collection));

							collection.Delete(node);
							collection.Commit(node);
						}

						nStatus.status = SyncNodeStatus.SyncStatus.Success;
					}
					catch
					{
						nStatus.status = SyncNodeStatus.SyncStatus.ServerFailure;
					}
				}
			}
			finally
			{
				cLock.ReleaseRequest();
			}
			return statusArray;
		}

		/// <summary>
		/// Put the node that represents the file to the server. This call is made to begin
		/// an upload of a file.  Close must be called to cleanup resources.
		/// </summary>
		/// <param name="node">The node to put to ther server.</param>
		/// <returns>True if successful.</returns>
		public SyncNodeStatus.SyncStatus PutFileNode(SyncNode node)
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
			{
				return SyncNodeStatus.SyncStatus.Access;
			}
			if (cLock == null) 
			{
				return SyncNodeStatus.SyncStatus.ClientError;
			}

			cLock.LockRequest();
			try
			{
				inFile = new ServerInFile(collection, node, Policy);
				outFile = null;
				return inFile.Open();
			}
			finally
			{
				cLock.ReleaseRequest();
			}
		}

		/// <summary>
		/// Get the node that represents the file. This call is made to begin
		/// a download of a file.  Close must be called to cleanup resources.
		/// </summary>
		/// <param name="nodeID">The node to get.</param>
		/// <returns>The SyncNode.</returns>
		public SyncNode GetFileNode(string nodeID)
		{
			if (cLock == null || !IsAccessAllowed(Access.Rights.ReadOnly))
				return null;

			cLock.LockRequest();
			try
			{
				BaseFileNode node = collection.GetNodeByID(nodeID) as BaseFileNode;
				inFile = null;
				outFile = null;
				if (node != null)
				{
					outFile = new ServerOutFile(collection, node);
					outFile.Open();
					SyncNode snode = new SyncNode();
					snode.nodeID = node.ID;
					snode.node = node.Properties.ToString(true);
					snode.expectedIncarn = node.MasterIncarnation;
					return snode;
				}
				return null;
			}
			finally
			{
				cLock.ReleaseRequest();
			}
		}

		/// <summary>
		/// Get a HashMap of the file.
		/// </summary>
		/// <param name="blockSize">The block size to be hashed.</param>
		/// <returns>The HashMap.</returns>
		public HashData[] GetHashMap(int blockSize)
		{
			if (inFile != null)
				return inFile.GetHashMap();
			else
				return outFile.GetHashMap();
		}

		/// <summary>
		/// Write the included data to the new file.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <param name="offset">The offset in the new file of where to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(byte[] buffer, long offset, int count)
		{
			inFile.WritePosition = offset;
			inFile.Write(buffer, 0, count);
		}

		/// <summary>
		/// Copy data from the old file to the new file.
		/// </summary>
		/// <param name="oldOffset">The offset in the old (original file).</param>
		/// <param name="offset">The offset in the new file.</param>
		/// <param name="count">The number of bytes to copy.</param>
		public void Copy(long oldOffset, long offset, int count)
		{
			inFile.Copy(oldOffset, offset, count);
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
			outFile.ReadPosition = offset;
			buffer = new byte[count];
			return outFile.Read(buffer, 0, count);
		}

		/// <summary>
		/// Close the current file.
		/// </summary>
		/// <param name="commit">True: commit the filenode and file.
		/// False: Abort the changes.</param>
		/// <returns>The status of the sync.</returns>
		public SyncNodeStatus CloseFileNode(bool commit)
		{
			if (cLock == null)
				return null;

			cLock.LockRequest();
			try
			{
				SyncNodeStatus status = null;
				if (inFile != null)
					status = inFile.Close(commit);
				else if (outFile != null)
					status = outFile.Close();
				inFile = null;
				outFile = null;
				return status;
			}
			finally
			{
				cLock.ReleaseRequest();
			}
		}

		/// <summary>
		/// simple version string, also useful to check remoting
		/// </summary>
		public string Version
		{
			get { return "0.9.0"; }
		}
	}
}
