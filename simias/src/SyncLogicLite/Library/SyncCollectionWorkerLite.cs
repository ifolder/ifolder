/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
		SyncCollectionServiceLite masterService;
		SyncCollection localCollection;
		SyncCollectionServiceLite localService;

		// TODO: should we be using two services?
		public SyncCollectionWorkerLite(SyncCollectionServiceLite master,
			SyncCollection slave)
				: base(master, slave)
		{
			this.masterService = master;
			this.localCollection = slave;
			this.localService = new SyncCollectionServiceLite(slave);
		}

		public override void DoSyncWork()
		{
			SyncNodeInfo[] masterNodes = masterService.GetNodeInfoArray();
			SyncNodeInfo[] localNodes = localService.GetNodeInfoArray();

#if TRACE
			MyTrace.WriteLine("Master Nodes: {0}", masterNodes.Length);
			
			foreach(SyncNodeInfo info in masterNodes)
			{
				MyTrace.WriteLine(" {0}", info);
			}
			
			MyTrace.WriteLine("Local Nodes: {0}", localNodes.Length);
			
			foreach(SyncNodeInfo info in localNodes)
			{
				MyTrace.WriteLine(" {0}", info);
			}
#endif

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
				if (!localNodeList.Contains(masterNodeInfo.ID) && !masterNodeInfo.Tombstone)
				{
					MyTrace.WriteLine("Download New Node: {0}", masterNodeInfo.NodePath);
					
					// download and update master incarnation
					Download(masterNodeInfo.ID, masterNodeInfo.LocalIncarnation);
				}
				
				// compare the nodes
				else
				{
					// get the local node with the same id
					SyncNodeInfo localNodeInfo = (SyncNodeInfo)localNodeList[masterNodeInfo.ID];

					// remove the slave node from the list
					localNodeList.Remove(masterNodeInfo.ID);

					// check for changes on the master first (ignore tombstones)
					if ((masterNodeInfo.LocalIncarnation > localNodeInfo.MasterIncarnation) && !localNodeInfo.Tombstone)
					{
						MyTrace.WriteLine("Download Changed Node: {0}", masterNodeInfo.NodePath);
						
						// download and update master incarnation
						Download(masterNodeInfo.ID, masterNodeInfo.LocalIncarnation);
					}

					// check for local changes
					else if (localNodeInfo.LocalIncarnation > localNodeInfo.MasterIncarnation)
					{
						MyTrace.WriteLine("Upload Changed Node: {0}", masterNodeInfo.NodePath);
						
						// upload
						Upload(localNodeInfo.ID);
					}

					// tombstones that have not been modified
					else if (localNodeInfo.Tombstone)
					{
						MyTrace.WriteLine("Upload Deleted Node: {0}", masterNodeInfo.NodePath);
						
						// delete off the master
						masterService.DeleteNode(masterNodeInfo.ID);
						localService.DeleteNode(localNodeInfo.ID);
					}
					else
					{
						// no changes
					}
				}
			}
			
			// scan and upload the remaing local nodes
			foreach(SyncNodeInfo localNodeInfo in localNodeList.Values)
			{
				// is this node unmodified and deleted off the master?
				if ((localNodeInfo.MasterIncarnation == localNodeInfo.LocalIncarnation) && !localNodeInfo.Tombstone)
				{
					MyTrace.WriteLine("Download Deleted Node: {0}", localNodeInfo.NodePath);
					
					localService.DeleteNode(localNodeInfo.ID);
				}
				
				// remove any stale tombstones
				else if (localNodeInfo.Tombstone)
				{
					MyTrace.WriteLine("Delete Local Stale Node: {0}", localNodeInfo.NodePath);
					localService.DeleteNode(localNodeInfo.ID);
				}

				// a new ifle
				else
				{
					// upload
					MyTrace.WriteLine("Upload New Node: {0}", localNodeInfo.NodePath);
					
					Upload(localNodeInfo.ID);
				}
			}
		}

		public void Download(string id, ulong incarnation)
		{
			// download and update master incarnation
			localService.CommitSyncPacket(masterService.GetSyncPacket(id), incarnation);
		}

		public void Upload(string id)
		{
			// TODO: do we need a node lock to protect the node between the following calls?
			
			// upload
			ulong incarnation = masterService.CommitSyncPacket(localService.GetSyncPacket(id));

			// update the master incarnation (except on collection)
			if (incarnation != 0)
			{
				SyncNode node = localCollection.GetNode(id);
				node.UpdateIncarnation(incarnation);
			}
		}
	}
}
