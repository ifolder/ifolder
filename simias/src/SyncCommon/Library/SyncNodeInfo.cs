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

using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Node Info
	/// </summary>
	[Serializable]
	public class SyncNodeInfo
	{
		protected string id;
		protected string name;
		protected string path;
		protected ulong masterIncarnation;
		protected ulong localIncarnation;
	
		public SyncNodeInfo(SyncNode node)
		{
			this.id = node.ID;
			this.name = node.Name;
			this.path = node.NodePath;
			this.masterIncarnation = node.MasterIncarnation;
			this.localIncarnation = node.LocalIncarnation;
		}

		public SyncNodeInfo(Node node) : this(new SyncNode(node))
		{
		}

		public override string ToString()
		{
			return String.Format("{0} {1}.{2} [{3}]", name, masterIncarnation, localIncarnation, id);
		}


		#region Properties
		
		public string ID
		{
			get { return id; }
		}
		
		public string Name
		{
			get { return name; }
		}
		
		public string NodePath
		{
			get { return path; }
		}
		
		public ulong MasterIncarnation
		{
			get { return masterIncarnation; }
		}
		
		public ulong LocalIncarnation
		{
			get { return localIncarnation; }
		}

		public bool Tombstone
		{
			get { return ((masterIncarnation == 0) && (localIncarnation == 0)); }
		}

		#endregion
	}
}
