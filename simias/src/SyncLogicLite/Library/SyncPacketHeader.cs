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

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Packet Header
	/// </summary>
	[Serializable]
	public class SyncPacketHeader
	{
		string nodeID;
		string nodePath;
		string nodeXml;
		SyncNodeStreamInfo[] streams;

		public SyncPacketHeader(string nodeID, string nodePath, string nodeXml, SyncNodeStreamInfo[] streams)
		{
			this.nodeID = nodeID;
			this.nodePath = nodePath;
			this.nodeXml = nodeXml;
			this.streams = streams;
		}

		public string NodeID
		{
			get { return nodeID; }
		}
		
		public string NodePath
		{
			get { return nodePath; }
		}
		
		public string NodeXml
		{
			get { return nodeXml; }
		}
		
		public SyncNodeStreamInfo[] Streams
		{
			get { return streams; }
		}
	}
}
