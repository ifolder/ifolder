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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Xml;
using System.Collections;

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Worker Lite
	/// </summary>
	/// <remarks>
	/// This logic is meant to be simplistic for debugging and testing.
	/// </remarks>
	public class SyncCollectionWorkerLite : SyncCollectionWorker
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncCollectionServiceLite));

		private SyncCollectionServiceLite masterService;
		private SyncCollection local;
		private SyncCollectionServiceLite localService;

		public SyncCollectionWorkerLite(SyncCollectionServiceLite master,
			SyncCollection slave)
				: base(master, slave)
		{
			this.masterService = master;
			this.local = slave;
			this.localService = new SyncCollectionServiceLite(slave);
		}

		public override void DoSyncWork()
		{
			SyncNodeInfo[] masterNodes = masterService.GetNodes();
			SyncNodeInfo[] localNodes = localService.GetNodes();

			log.Debug("Master Nodes: {0}", masterNodes.Length);
			
			log.Debug("Local Nodes: {0}", localNodes.Length);
			
			// create a working local node list
			Hashtable localNodeList = new Hashtable();
			foreach(SyncNodeInfo info in localNodes)
			{
				localNodeList.Add(info.ID, info);
			}
			
			// scan the master nodes
			foreach(SyncNodeInfo masterNodeInfo in masterNodes)
			{
				// download any nodes that we do not have
				if (!localNodeList.Contains(masterNodeInfo.ID))
				{
					log.Debug("Download New Node: {0}", masterNodeInfo.Name);
					
					// download and update master incarnation
					Download(masterNodeInfo);
				}
				
				// compare the nodes
				else
				{
					// get the local node with the same id
					SyncNodeInfo localNodeInfo = (SyncNodeInfo)localNodeList[masterNodeInfo.ID];

					// pop the slave node from the list
					localNodeList.Remove(masterNodeInfo.ID);

					// check for changes on the master first (ignore tombstones)
					if (masterNodeInfo.LocalIncarnation > localNodeInfo.MasterIncarnation)
					{
						log.Debug("Download Changed Node: {0}", masterNodeInfo.Name);
						
						// download and update master incarnation
						Download(masterNodeInfo);
					}

					// check for local changes
					else if (localNodeInfo.LocalIncarnation > localNodeInfo.MasterIncarnation)
					{
						log.Debug("Upload Changed Node: {0}", masterNodeInfo.Name);
						
						// upload
						Upload(localNodeInfo);
					}

					// no changes
					else
					{
					}
				}
			}
			
			// scan and upload the remaing local nodes
			foreach(SyncNodeInfo localNodeInfo in localNodeList.Values)
			{
				// is this node unmodified and deleted off the master?
				if (localNodeInfo.MasterIncarnation == localNodeInfo.LocalIncarnation)
				{
					log.Debug("Download Deleted Node: {0}", localNodeInfo.Name);
					
					// pop
					localService.DeleteNode(localNodeInfo.ID);
				}
				
				// a new local node
				else
				{
					// upload
					log.Debug("Upload New Node: {0}", localNodeInfo.Name);
					
					Upload(localNodeInfo);
				}
			}
		}

		public void Download(SyncNodeInfo node)
		{
			// download and update master incarnation
			localService.Commit(masterService.GetSyncPacket(node.ID), node.MasterIncarnation);
		}

		public void Upload(SyncNodeInfo node)
		{
			// TODO: do we need a node lock to protect the node between the following calls?
			
			// upload
			ulong incarnation = masterService.Commit(localService.GetSyncPacket(node.ID));

			// update the master incarnation on the local node
			if (incarnation != 0)
			{
				Node n = local.GetNodeByID(node.ID);
				n.IncarnationUpdate = incarnation;
				local.Commit(n);
			}
		}
	}
}
