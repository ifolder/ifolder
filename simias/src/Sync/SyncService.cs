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
 *  Author: Dale Olds <olds@novell.com>
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
using Simias.Client;
using Simias.Client.Event;
using Simias.Policy;
using Simias.Sync.Delta;

namespace Simias.Sync
{
	
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

	
	/// <summary>
	/// server side top level class of SynkerA-style synchronization
	/// </summary>
	public class SyncService
	{
		static Store store = Store.GetStore();
		/// <summary>
		/// Used to log to the log file.
		/// </summary>
		public static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncService));
		Collection collection;
		CollectionLock	cLock;
		Member			member;
		Access.Rights	rights = Access.Rights.Deny;
		ArrayList		NodeList = new ArrayList();
		ServerInFile	inFile;
		ServerOutFile	outFile;
		SyncPolicy		policy;
		string			sessionID;
		IEnumerator		nodeContainer;
		bool			getAllNodes;
		const int		MaxBuffSize = 1024 * 64;
        
		/// <summary>
		/// Finalizer.
		/// </summary>
		~SyncService()
		{
			Dispose(true);		
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="inFinalize"></param>
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
		/// <param name="sessionID">The unique sessionID.</param>
		public void Start(ref StartSyncInfo si, string user, string sessionID)
		{
			this.sessionID = sessionID;
			si.Status = StartSyncStatus.Success;
			rights = si.Access = Access.Rights.Deny;
			cLock = null;
			nodeContainer = null;
			getAllNodes = false;

			collection = store.GetCollectionByID(si.CollectionID);
			if (collection == null)
			{
				si.Status = StartSyncStatus.NotFound;
				return;
			}

			// If we are locked return busy.
			if (collection.IsLocked)
			{
				si.Status = StartSyncStatus.Locked;
				return;
			}
		
			// Check our rights.
			member = null;
			string userID = Thread.CurrentPrincipal.Identity.Name;
			if (userID != null)
			{
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

			if (member == null)
			{
				si.Status = StartSyncStatus.AccessDenied;
				return;
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
						nodeContainer = this.BeginListChangedNodes(ref si.Context, out si.ChangesOnly);
						// Check if we have any work to do
						if (si.ChangesOnly && !si.ClientHasChanges && nodeContainer == null)
						{
							si.Status = StartSyncStatus.NoWork;
							break;
						}
					}

					cLock = CollectionLock.GetLock(collection.ID);
					if (cLock == null)
					{
						nodeContainer = null;
						si.Status = StartSyncStatus.Busy;
						return;
					}
					// See if we need to return all of the nodes.
					if (!si.ChangesOnly)
					{
						cLock.LockRequest();
						try
						{
							// We need to get all of the nodes.
							si.Context = new ChangeLogReader(collection).GetEventContext().ToString();
							nodeContainer = this.BeginListAllNodes();
						}
						finally
						{
							cLock.ReleaseRequest();
						}
						if (nodeContainer == null)
						{
							Dispose(false);
							rights = Access.Rights.Deny;
							si.Access = rights;
						}
					}
					break;
				case Access.Rights.Deny:
					si.Status = StartSyncStatus.NotFound;
					break;
				
			}
			return;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public SyncNodeInfo[] NextNodeInfoList(ref int count)
		{
			if (nodeContainer == null)
			{
				count = 0;
				return new SyncNodeInfo[0];
			}

			SyncNodeInfo[] infoArray = new SyncNodeInfo[count];
            int i = 0;
			if (getAllNodes)
			{
				for (i = 0; i < count;)
				{
					Node node = Node.NodeFactory(collection, (ShallowNode)nodeContainer.Current);
					if (node != null)
					{
						infoArray[i++] = new SyncNodeInfo(node);
					}
					if (!nodeContainer.MoveNext())
					{
						nodeContainer = null;
						break;
					}
				}
			}
			else
			{
				for (i = 0; i < count;)
				{
					infoArray[i++] = new SyncNodeInfo((ChangeLogRecord)nodeContainer.Current);
					if (!nodeContainer.MoveNext())
					{
						nodeContainer = null;
						break;
					}
				}
			}
			count = i;
			return infoArray;
		}

		/// <summary>
		/// Gets an enumerator that can be used to list a SyncNodeInfo for all objects in the store.
		/// </summary>
		/// <returns>The enumerator or null.</returns>
		private IEnumerator BeginListAllNodes()
		{
			log.Debug("BeginListAllNodes start");
			if (!IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			IEnumerator enumerator = collection.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				enumerator = null;
			}
			getAllNodes = true;
			log.Debug("BeginListAllNodes End{0}", enumerator == null ? " Error No Nodes" : "");
			return enumerator;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="contextValid"></param>
		/// <returns></returns>
		private IEnumerator BeginListChangedNodes(ref string context, out bool contextValid)
		{
			log.Debug("BeginListChangedNodes Start");

			IEnumerator enumerator = null;
			EventContext eventContext;
			// Create a change log reader.
			ChangeLogReader logReader = new ChangeLogReader( collection );
			try
			{	
				// Read the cookie from the last sync and then get the changes since then.
				if (context != null)
				{
					ArrayList changeList = null;
					eventContext = new EventContext(context);
					logReader.GetEvents(eventContext, out changeList);
					enumerator = changeList.GetEnumerator();
					if (!enumerator.MoveNext())
					{
						enumerator = null;
					}
					log.Debug("BeginListChangedNodes End. Found {0} changed nodes.", changeList.Count);
					context = eventContext.ToString();
					contextValid = true;
					getAllNodes = false;
					return enumerator;
				}
			}
			catch (Exception ex)
			{
				log.Debug(ex, "BeginListChangedNodes");
			}

			// The cookie is invalid.  Get a valid cookie and save it for the next sync.
			eventContext = logReader.GetEventContext();
			if (eventContext != null)
				context = eventContext.ToString();
			log.Debug("BeginListChangedNodes End");
			contextValid = false;
			return null;
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

		/// <summary>
		/// Store the supplied nodes in the store.
		/// </summary>
		/// <param name="nodes">The array of nodes to store.</param>
		/// <returns>The status of the operation.</returns>
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
					statusList[i].status = SyncStatus.ServerFailure;
				
					if (sn != null)
					{
						statusList[i].nodeID = sn.ID;
						XmlDocument xNode = new XmlDocument();
						xNode.LoadXml(sn.node);
						Node node = Node.NodeFactory(store, xNode);
						Import(node, sn.MasterIncarnation);
						NodeList.Add(node);
						statusList[i++].status = SyncStatus.Success;
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
							statusList[i++].status = SyncStatus.UpdateConflict;
						}
						catch (LockException)
						{
							statusList[i++].status = SyncStatus.Locked;
						}
						catch
						{
							// Handle any other errors.
							statusList[i++].status = SyncStatus.ServerFailure;
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

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nodes"></param>
		/// <returns></returns>
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
					status.status = SyncStatus.ServerFailure;
					try
					{
						XmlDocument xNode = new XmlDocument();
						xNode.LoadXml(snode.node);
						DirNode node = (DirNode)Node.NodeFactory(store, xNode);
						log.Debug("Updating {0} {1} from client", node.Name, node.Type);

						status.nodeID = node.ID;
						Import(node, snode.MasterIncarnation);
			
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
						status.status = SyncStatus.Success;
					}
					catch (CollisionException)
					{
						// The current node failed because of a collision.
						status.status = SyncStatus.UpdateConflict;
					}
					catch (LockException)
					{
						status.status = SyncStatus.Locked;
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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="nodeIDs"></param>
		/// <returns></returns>
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
					try
					{
						Node node = collection.GetNodeByID(nodeIDs[i]);
						nodes[i] = new SyncNode(node);
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
							nStatus.status = SyncStatus.Success;
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

						nStatus.status = SyncStatus.Success;
					}
					catch(LockException)
					{
						nStatus.status = SyncStatus.Locked;
					}
					catch
					{
						nStatus.status = SyncStatus.ServerFailure;
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
		public SyncStatus PutFileNode(SyncNode node)
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
			{
				return SyncStatus.Access;
			}
			if (cLock == null) 
			{
				return SyncStatus.ClientError;
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
					outFile.Open(sessionID);
					return new SyncNode(node);
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
		/// <returns>The HashMap.</returns>
		public HashData[] GetHashMap()
		{
			if (inFile != null)
				return inFile.GetHashMap();
			else
				return outFile.GetHashMap();
		}

		/// <summary>
		/// Write the included data to the new file.
		/// </summary>
		/// <param name="stream">The stream to write.</param>
		/// <param name="offset">The offset in the new file of where to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		public void Write(Stream stream, long offset, int count)
		{
			inFile.WritePosition = offset;
			inFile.Write(stream, count);
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
		/// <param name="stream">Stream to read into.</param>
		/// <param name="offset">The offset to begin reading.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The number of bytes read.</returns>
		public int Read(Stream stream, long offset, int count)
		{
			outFile.ReadPosition = offset;
			return outFile.Read(stream, count);
		}

		/// <summary>
		/// Gets the read stream.
		/// </summary>
		/// <returns>The file stream.</returns>
		public StreamStream GetReadStream()
		{
			return outFile.outStream;
		}

		/// <summary>
		/// Get the WriteStream.
		/// </summary>
		/// <returns>The file stream.</returns>
		public StreamStream GetWriteStream()
		{
			return inFile.inStream;
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
