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
	}

	public enum SyncChangeType
	{
		/// <summary>
		/// The node exists but no log record has been created.
		/// Do a brute force sync.
		/// </summary>
		Unknown,
		/// <summary>
		/// Node object was created.
		/// </summary>
		Created,
		/// <summary>
		/// Node object was deleted.
		/// </summary>
		Deleted,
		/// <summary>
		/// Node object was changed.
		/// </summary>
		Changed,
		/// <summary>
		/// Node object was renamed.
		/// </summary>
		Renamed
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
		public SyncChangeType ChangeType;

		public SyncNodeStamp()
		{
		}

		internal SyncNodeStamp(string id, ulong incarnation, string baseType, SyncChangeType changeType)
		{
			ID = id;
			Incarnation = incarnation;
			BaseType = baseType;
			ChangeType = changeType;
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
	Member			member;
	Access.Rights	rights = Access.Rights.Deny;
	//SyncOps			ops;
	//IncomingNode	inNode;
	//OutgoingNode	outNode;
	ArrayList		NodeList = new ArrayList();
	
	/// <summary>
	/// public ctor 
	/// </summary>
	public SyncService(SyncCollection collection)
	{
		this.collection = collection;
	}

	
	/// <summary>
	/// start sync of this collection -- perform basic role checks and dredge server file system
	/// </summary>
	public Access.Rights Start(string user)
	{
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
			log.Info("Starting Sync of {0} for {1} rights : {2}.", collection, member.Name, rights);
		}
		else if (userID == collection.ProxyUserID)
		{
			rights = Access.Rights.Admin;
			log.Info("Sync session starting for {0}", userID);
		}
		return rights;
	}

	/// <summary>
	/// Called when done with the sync cycle.
	/// </summary>
	public void Stop()
	{
		collection.Revert();
		log.Info("Finished Sync of {0} for {1}.", collection, member.Name);
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

	public SyncNodeStatus[] PutNonFileNodes(SyncNode [] nodes)
	{
		SyncNodeStatus[]	statusList = new SyncNodeStatus[nodes.Length];

		// Try to commit all the nodes at once.
		int i = 0;
		foreach (SyncNode sn in nodes)
		{
			XmlDocument xNode = new XmlDocument();
			xNode.LoadXml(sn.node);
			Node node = new Node(xNode);
			collection.ImportNode(node, true, sn.expectedIncarn);
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
				SyncNodeStamp stamp = new SyncNodeStamp(node.ID, node.LocalIncarnation, node.Type, SyncChangeType.Unknown);
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
	public bool GetChangedNodeStamps(out SyncNodeStamp[] nodes, ref string context, out bool more)
	{
		log.Debug("GetChangedNodeStamps Start");

		EventContext eventContext;
		// Create a change log reader.
		ChangeLogReader logReader = new ChangeLogReader( collection );
		nodes = null;
		more = false;
		try
		{	
			// Read the cookie from the last sync and then get the changes since then.
			if (context != null)
			{
				ArrayList changeList = null;
				ArrayList stampList = new ArrayList();

				eventContext = new EventContext(context);
				more = logReader.GetEvents(eventContext, out changeList);
				foreach( ChangeLogRecord rec in changeList )
				{
					// Make sure the events are not for local only changes.
					if (((NodeEventArgs.EventFlags)rec.Flags & NodeEventArgs.EventFlags.LocalOnly) == 0)
					{
						SyncNodeStamp stamp = new SyncNodeStamp(
							rec.EventID, rec.SlaveRev, rec.Type.ToString(), 
							(SyncChangeType)Enum.Parse(typeof(SyncChangeType),rec.Operation.ToString()));
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
	/// simple version string, also useful to check remoting
	/// </summary>
	public string Version
	{
		get { return "0.0.0"; }
	}

	/*

	/// <summary>
	/// takes an array of small nodes. returns rejected nodes
	/// </summary>
	public RejectedNode[] PutSmallNodes(NodeChunk[] nodes)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			log.Debug("SyncSession.PutSmallNodes() Count {0}", nodes.Length);
			return ops.PutSmallNodes(nodes);
		}
		catch (Exception e)
		{
			Log.Uncaught(e);
			//TODO: handle this. Can't return null here because it is valid return (since empty arrays don't currently work on mono)
			throw e; 
		}
		//return null;
	}

	/// <summary>
	/// gets an array of small nodes
	/// </summary>
	public NodeChunk[] GetSmallNodes(string[] nids)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			log.Debug("SyncSession.GetSmallNodes() Count {0}", nids.Length);
			return ops.GetSmallNodes(nids);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}

	/// <summary>
	/// takes metadata and first chunk of data for a large node
	/// </summary>
	public bool WriteLargeNode(Node node, byte[] data)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.Start(node, null);
			inNode.BlowChunk(data);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	/// <summary>
	/// takes next chunk of data for a large node, completes node if done
	/// </summary>
	public NodeStatus WriteLargeNode(byte[] data, ulong expectedIncarn, bool done)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.BlowChunk(data);
			return done? inNode.Complete(expectedIncarn): NodeStatus.InProgess;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return NodeStatus.ServerFailure;
	}

	/// <summary>
	/// gets metadata and first chunk of data for a large node
	/// </summary>
	public NodeChunk ReadLargeNode(string nid, int maxSize)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			NodeChunk nc = new NodeChunk();
			nc.data = (nc.node = outNode.Start(nid)) != null? outNode.ReadChunk(maxSize, out nc.totalSize): null;
			return nc;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return new NodeChunk();
	}

	/// <summary>
	/// gets next chunks of data for a large node
	/// </summary>
	public byte[] ReadLargeNode(int maxSize)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			int unused;
			return outNode.ReadChunk(maxSize, out unused);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}
	*/
}

//===========================================================================
}
