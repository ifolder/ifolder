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
using System.Collections;

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Packet
	/// </summary>
	public class SyncPacket : MarshalByRefObject
	{
		private SyncPacketHeader header;
		private SyncPacketData data;

		public SyncPacket(SyncNode node)
		{
			// create a stream list
			SyncNodeStreamInfo[] streamInfos = null;
			ArrayList streamInfoList = new ArrayList();
			Hashtable nodeStreams = new Hashtable();

			// TODO: get "modified" streams?
			foreach(NodeStream ns in node.BaseNode.GetStreamList())
			{
				SyncNodeStream sns = new SyncNodeStream(ns);

				streamInfoList.Add(new SyncNodeStreamInfo(sns));
				nodeStreams.Add(sns.ID, sns);
			}

			// get the stream infos
			streamInfos = (SyncNodeStreamInfo[])streamInfoList.ToArray(typeof(SyncNodeStreamInfo));

			// create the header
			header = new SyncPacketHeader(node.ID, node.NodePath, node.ToXml(), streamInfos);
			data = new SyncPacketData(nodeStreams);
		}

		public static ulong Commit(SyncPacket packet, SyncCollection collection)
		{
			return Commit(packet, collection, 0);
		}
	
		public static ulong Commit(SyncPacket packet, SyncCollection collection, ulong masterIncarnation)
		{
			byte[] buffer = new byte[1024 * 32];

			ulong result = 0;

			try
			{
				SyncPacketHeader header = packet.Header;
				SyncPacketData data = packet.Data;

				MyTrace.WriteLine("Starting Node Download: {0}", header.NodePath);

				int start = Environment.TickCount;

				SyncNode node = collection.CreateNodeFromXml(header.NodeXml);
				
				// loop through all the streams
				foreach(SyncNodeStreamInfo info in header.Streams)
				{
					string tempFileName = Path.GetTempFileName();
					
					// validate that the temp directory exists
					if (!Directory.Exists(Path.GetDirectoryName(tempFileName)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));
					}

					FileStream remoteFS = data.LockStream(info.ID);
	
					FileStream localFS = File.OpenWrite(tempFileName);

					int length = 0;
					
					MyTrace.WriteLine("Starting Stream Download: {0}", info.Path);
					
					int streamStart = Environment.TickCount;

					while((length = remoteFS.Read(buffer, 0, buffer.Length)) > 0)
					{
						localFS.Write(buffer, 0, length);	
					}

					localFS.Close();
					remoteFS.Close();

					int streamStop = Environment.TickCount;

					MyTrace.WriteLine("Completed Stream Download: {0} ({1} ms)", info.Path, (streamStop - streamStart));
					
					// TODO: this rename needs to be hidded from the File Monitor
					string fullPath = Path.Combine(collection.StreamRootPath, info.Path);
					
					// validate that the full path directory exists
					if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
					}

					// TODO: needs to be atomic?
					if (File.Exists(fullPath)) File.Delete(fullPath);

					File.Move(tempFileName, fullPath);

					// update the stream
					NodeStream ns = node.BaseNode.GetStreamById(info.ID);
					
					if (ns == null)
					{
						// a new stream
						ns = node.BaseNode.AddStream(info.Name, info.Type, info.Path);
					}

					SyncNodeStream sns = new SyncNodeStream(ns);

					// update the locat last write stamp
					sns.LastWrite = File.GetLastWriteTime(fullPath);
					sns.LocalLastWrite = sns.LastWrite;
				}

				// create directory
				if (node.IsDirectory)
				{
					string dirPath = Path.Combine(collection.RootPath, node.NodePath);

					if (!Directory.Exists(dirPath))
					{
						Directory.CreateDirectory(dirPath);
					}
				}

				// commit node
				if (masterIncarnation != 0)
				{
					// sync role
					collection.BaseCollection.LocalStore.ImpersonateUser(Access.SyncOperatorRole);
				
					// this command does an implicit commit
					node.BaseNode.UpdateIncarnation(masterIncarnation);

					// sync role
					collection.BaseCollection.LocalStore.Revert();
				}
				else
				{
					node.Commit();
				}

				int stop = Environment.TickCount;

				MyTrace.WriteLine("Completed Node Download: {0} ({1}.{2}) [{3} ms]", header.NodePath, node.MasterIncarnation,
					node.LocalIncarnation, (stop-start));

				result = node.BaseNode.LocalIncarnation;
			}
			catch(Exception e)
			{
				// ignore ?
				MyTrace.WriteLine(e);
			}

			return result;
		}

		#region Properties
		
		public SyncPacketHeader Header
		{
			get { return header; }
		}

		public SyncPacketData Data
		{
			get { return data; }
		}

		#endregion
	}
}
