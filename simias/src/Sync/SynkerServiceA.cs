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
	public override SyncCollectionWorker GetCollectionWorker(SyncCollectionService master, SyncCollection slave)
	{
		return new SynkerWorkerA((SynkerServiceA)master, slave);
	}
}

//---------------------------------------------------------------------------
/// <summary>
/// server side top level class of SynkerA-style synchronization
/// </summary>
public class SynkerServiceA: SyncCollectionService
{
	SyncCollection collection;
	Member		   member;
	SyncOps ops;
	IncomingNode inNode;
	OutgoingNode outNode;
	bool ignoreRights = true;

	/// <summary>
	/// public ctor 
	/// </summary>
	public SynkerServiceA(SyncCollection collection): this(collection, false)
	{
	}

	/// <summary>
	/// debug ctor 
	/// </summary>
	public SynkerServiceA(SyncCollection collection, bool ignoreRights): base(collection)
	{
		this.collection = collection;
		this.ignoreRights = ignoreRights;
	}

	/// <summary>
	/// start sync of this collection -- perform basic role checks and dredge server file system
	/// </summary>
	public Access.Rights Start()
	{
		Access.Rights rights = Access.Rights.Deny;
	
		string userID = Thread.CurrentPrincipal.Identity.Name;
			
		if (userID != null)
		{
			member = collection.GetMember(userID);
			collection.Impersonate(member);
			Log.Info("Sync session starting for {0}", userID);
		}
		else
		{
			// kludge
			Log.Spew("could not get identity in sync start");
			ignoreRights = true;
			member = new Member("Root", collection.ID, Access.Rights.Admin);
			collection.Impersonate(member);
		}

		// further kludge
		if (ignoreRights || userID == String.Empty)
			ignoreRights = true;

		if (ignoreRights || collection.IsAccessAllowed(member, Access.Rights.ReadOnly))
		{
			//collection.StoreReference.Revert(); //TODO: what if this is second time for this collection?
			//Log.Spew("dredging server for collection '{0}'", collection.Name);
			//new Dredger(collection, true);
			//Log.Spew("done dredging server for collection '{0}'", collection.Name);
			inNode = new IncomingNode(collection, true);
			outNode = new OutgoingNode(collection);
			ops = new SyncOps(collection, true);
		}
		
		Log.Spew("server start of collection {0} returning {1}", collection.Name, rights);
		return member.Rights;
	}

	/// <summary>
	/// returns array of NodeStamps for all Nodes in this collection
	/// </summary>
	public NodeStamp[] GetNodeStamps()
	{
		Log.Spew("server start of GetNodeStamps");
		try
		{
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadOnly))
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
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadWrite))
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
	public NodeChunk[] GetSmallNodes(Nid[] nids)
	{
		try
		{
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadOnly))
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
	public bool WriteLargeNode(Node node, ForkChunk[] forkChunks)
	{
		try
		{
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.Start(node, null);
			inNode.BlowChunks(forkChunks);
			return true;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return false;
	}

	/// <summary>
	/// takes next chunk of data for a large node, completes node if done
	/// </summary>
	public NodeStatus WriteLargeNode(ForkChunk[] forkChunks, ulong expectedIncarn, bool done)
	{
		try
		{
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadWrite))
				throw new UnauthorizedAccessException("Current user cannot modify this collection");

			inNode.BlowChunks(forkChunks);
			return done? inNode.Complete(expectedIncarn): NodeStatus.InProgess;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return NodeStatus.ServerFailure;
	}

	/// <summary>
	/// gets metadata and first chunk of data for a large node
	/// </summary>
	public NodeChunk ReadLargeNode(Nid nid, int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			NodeChunk nc = new NodeChunk();
			nc.forkChunks = (nc.node = outNode.Start(nid)) != null? outNode.ReadChunks(maxSize, out nc.totalSize): null;
			return nc;
		}
		catch (Exception e) { Log.Uncaught(e); }
		return new NodeChunk();
	}

	/// <summary>
	/// gets next chunks of data for a large node
	/// </summary>
	public ForkChunk[] ReadLargeNode(int maxSize)
	{
		try
		{
			if (!collection.IsAccessAllowed(member, Access.Rights.ReadOnly))
				throw new UnauthorizedAccessException("Current user cannot read this collection");

			int unused;
			return outNode.ReadChunks(maxSize, out unused);
		}
		catch (Exception e) { Log.Uncaught(e); }
		return null;
	}
}

//===========================================================================
}
