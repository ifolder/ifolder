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
using System.Collections;

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Packet
	/// </summary>
	public class SyncPacket : MarshalByRefObject
	{
		private Node node;
		private string path;
		private FileStream stream;

		public SyncPacket(Node node)
		{
			this.node = node;
			this.path = null;
			this.stream = null;

			if (node.GetType().IsSubclassOf(typeof(DirNode)))
			{
				this.path = null;
			}
			else if (node.GetType().IsSubclassOf(typeof(BaseFileNode)))
			{
				this.path = null;
				this.stream = null;
			}
		}

		#region Properties
		
		public Node SyncNode
		{
			get { return node; }
		}

		public string SyncPath
		{
			get { return path; }
		}

		public FileStream SyncStream
		{
			get { return stream; }
		}

		#endregion
	}
}
