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

//---------------------------------------------------------------------------
/// <summary>
/// class to register guts of SynkerA-style synchronization
/// </summary>
public class SynkerA: SyncLogicFactory
{
	/// <summary>
	/// creates a SynkerServiceA on the server
	/// </summary>
	public override SyncCollectionService GetCollectionService(SyncCollection collection)
	{
		return new SynkerServiceA(collection);
	}

	/// <summary>
	/// creates a SynkerWorkerA on the client
	/// </summary>
	public override SyncCollectionWorker GetCollectionWorker(SyncCollection collection)
	{
		return new SynkerWorkerA(collection);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
public class SynkerServiceA: SyncCollectionService
{
	Access.Rights	rights = Access.Rights.Deny;
	SyncOps ops;
	IncomingNode inNode;
	OutgoingNode outNode;
	
	/// <summary>
	/// public ctor 
	/// </summary>
	public SynkerServiceA(SyncCollection collection) : base(collection)
	{
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
		
		Member member = collection.GetMemberByID(userID);
		if (member != null)
		{
			collection.Impersonate(member);
			rights = member.Rights;
			Log.Info("Sync session starting for {0}", member.Name);
		}
		else if (userID == collection.ProxyUserID)
		{
			rights = Access.Rights.Admin;
			Log.Info("Sync session starting for {0}", userID);
		}
		
		if (rights != Access.Rights.Deny)
		{
			//Log.Spew("dredging server for collection '{0}'", collection.Name);
			//new Dredger(collection, true).Dredge();
			//Log.Spew("done dredging server for collection '{0}'", collection.Name);
			inNode = new IncomingNode(collection, true);
			outNode = new OutgoingNode(collection);
			ops = new SyncOps(collection, true);
		}
		
		Log.Spew("server start of collection {0} returning {1}", collection.Name, rights);
		return rights;
	}

	/// <summary>
	/// Called when done with the sync cycle.
	/// </summary>
	public void Stop()
	{
		collection.Revert();
		this.ops = null;
		this.inNode = null;
		this.outNode = null;
		this.collection = null;
		System.Runtime.Remoting.RemotingServices.Disconnect(this);
	}

	private bool IsAccessAllowed(Access.Rights desiredRights)
	{
		return (rights >= desiredRights) ? true : false;
	}
	/// <summary>
	/// returns array of NodeStamps for all Nodes in this collection
	/// </summary>
	public NodeStamp[] GetNodeStamps()
	{
		Log.Spew("server start of GetNodeStamps");
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");
			NodeStamp[] nss = ops.GetNodeStamps();
			Log.Spew("server returning {0} NodeStamps", nss.Length);
			return nss;
		}
		catch (Exception e) { Log.Uncaught(e); }
		Log.Spew("server no NodeStamps, returning null");
		return null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="nodes"></param>
	/// <param name="cookie"></param>
	/// <returns></returns>
	public bool GetChangedNodeStamps(out NodeStamp[] nodes, ref string cookie)
	{
		nodes = null;
		Log.Spew("server start of GetChangedNodes");
		try
		{
			return ops.GetChangedNodeStamps(out nodes, ref cookie);
		}
		catch (Exception e) {Log.Uncaught(e); }
		Log.Spew("Failed to get changes");
		return false;
	}
	

	/// <summary>
	/// simple version string, also useful to check remoting
	/// </summary>
	public string Version
	{
		get { return "0.0.0"; }
	}

	/// <summary>
	/// takes an array of small nodes. returns rejected nodes
	/// </summary>
	public RejectedNode[] PutSmallNodes(NodeChunk[] nodes)
	{
		try
		{
			if (!IsAccessAllowed(Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			Log.Spew("SyncSession.PutSmallNodes() Count {0}", nodes.Length);
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

			Log.Spew("SyncSession.GetSmallNodes() Count {0}", nids.Length);
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
}

//===========================================================================
}
