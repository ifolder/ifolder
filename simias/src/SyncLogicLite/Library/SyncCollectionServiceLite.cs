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

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Service Lite
	/// </summary>
	public class SyncCollectionServiceLite : SyncCollectionService
	{
		private static readonly ISimiasLog log = SimiasLogManager.GetLogger(typeof(SyncCollectionServiceLite));

		private SyncCollection collection;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="collection">The collection object.</param>
		public SyncCollectionServiceLite(SyncCollection collection)
			: base(collection)
		{
			this.collection = collection;
		}

		/// <summary>
		/// Get a list of all the nodes in the collection.
		/// </summary>
		/// <returns>An array of all the nodes in the collection.</returns>
		public SyncNodeInfo[] GetNodes()
		{
			ArrayList list = new ArrayList();

			foreach(ShallowNode sn in collection)
			{
				Node n = new Node(collection, sn);

				list.Add(new SyncNodeInfo(n));
			}

			return (SyncNodeInfo[])list.ToArray(typeof(SyncNodeInfo));
		}

		/// <summary>
		/// Get a sync packet for the given id.
		/// </summary>
		/// <param name="id">The node id.</param>
		/// <returns>A sync packet object.</returns>
		public SyncPacket GetSyncPacket(string id)
		{
			SyncPacket packet = null;

			try
			{
				Node node = collection.GetNodeByID(id);

				packet = new SyncPacket(node);
			}
			catch(Exception e)
			{
				log.Debug(e, "Ignored");
			}

			return packet;
		}

		/// <summary>
		/// Commit the sync packet.
		/// </summary>
		/// <param name="packet">The sync packet object</param>
		/// <returns>An incarnation number.</returns>
		public ulong Commit(SyncPacket packet)
		{
			return Commit(packet, 0);
		}

		/// <summary>
		/// Commit the sync packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="incarnation"></param>
		/// <returns></returns>
		public ulong Commit(SyncPacket packet, ulong incarnation)
		{
			ulong result = 0;

			try
			{
				result = Commit(packet, collection, incarnation);
			}
			catch(Exception e)
			{
				log.Debug(e, "Ignored");
			}

			return result;
		}

		/// <summary>
		/// Commit the sync packet.
		/// </summary>
		/// <param name="packet"></param>
		/// <param name="collection"></param>
		/// <param name="incarnation"></param>
		/// <returns></returns>
		private ulong Commit(SyncPacket packet, SyncCollection collection, ulong incarnation)
		{
			Node node = packet.SyncNode;
			string path = packet.SyncPath;
			FileStream stream = packet.SyncStream;
			
			byte[] buffer = new byte[1024 * 32];
			ulong result = 0;

			try
			{
				log.Debug("Node Download: {0}", node.Name);

				int start = Environment.TickCount;

				collection.ImportNode(node, node.LocalIncarnation);

				// directory node
				if (node.GetType().IsSubclassOf(typeof(DirNode)) && path != null)
				{
					string fullPath = (node as DirNode).GetFullPath(collection);

					if (!Directory.Exists(fullPath))
					{
						Directory.CreateDirectory(fullPath);
					}
				}
				
				// file node
				else if ((node.GetType().IsSubclassOf(typeof(BaseFileNode)))
					&& path != null && stream != null)
				{
					string fullPath = (node as BaseFileNode).GetFullPath(collection);

					string tempFileName = Path.GetTempFileName();
					
					// validate that the temp directory exists
					if (!Directory.Exists(Path.GetDirectoryName(tempFileName)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));
					}

					FileStream localStream = File.OpenWrite(tempFileName);

					int length = 0;
					
					log.Debug("Starting File Download: {0}", path);
					
					int streamStart = Environment.TickCount;

					while((length = stream.Read(buffer, 0, buffer.Length)) > 0)
					{
						localStream.Write(buffer, 0, length);	
					}

					stream.Close();
					localStream.Close();

					int streamStop = Environment.TickCount;

					log.Debug("Completed File Download: {0} ({1} ms)", path, (streamStop - streamStart));
					
					// validate that the full path directory exists
					if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
					}

					// TODO: needs to be atomic?
					if (File.Exists(fullPath)) File.Delete(fullPath);

					File.Move(tempFileName, fullPath);
				}

				// update incarnation
				if (incarnation != 0)
				{
					node.IncarnationUpdate = incarnation;
				}

				// commit node
				collection.Commit();

				int stop = Environment.TickCount;

				log.Debug("Completed Node Download: {0} ({1}.{2}) [{3} ms]", path,
					node.MasterIncarnation, node.LocalIncarnation, (stop-start));

				result = node.LocalIncarnation;
			}
			catch(Exception e)
			{
				log.Debug(e, "Ignored");
			}

			return result;
		}

		/// <summary>
		/// Delete a specific node.
		/// </summary>
		/// <param name="id"></param>
		public void DeleteNode(string id)
		{
			Node node = collection.GetNodeByID(id);

			log.Debug("Deleting Node: {0}", node.Name);

			collection.Delete(node);
		}
	}
}
