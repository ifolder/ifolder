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
	/// Sync Node Info
	/// </summary>
	[Serializable]
	public class SyncNodeInfo
	{
		/// <summary>
		/// The id of the node.
		/// </summary>
		protected string id;

		/// <summary>
		/// The name of the node.
		/// </summary>
		protected string name;

		/// <summary>
		/// The path of the node.
		/// </summary>
		protected string path;

		/// <summary>
		/// The master incarnation number of the node.
		/// </summary>
		protected ulong masterIncarnation;

		/// <summary>
		/// The local incarnation number of the node.
		/// </summary>
		protected ulong localIncarnation;
	
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="node">The node object.</param>
		public SyncNodeInfo(SyncNode node)
		{
			this.id = node.ID;
			this.name = node.Name;
			this.path = node.NodePath;
			this.masterIncarnation = node.MasterIncarnation;
			this.localIncarnation = node.LocalIncarnation;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="node">The node object.</param>
		public SyncNodeInfo(Node node) : this(new SyncNode(node))
		{
		}

		/// <summary>
		/// Create a string representation of the node information.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0} {1}.{2} [{3}]", name, masterIncarnation, localIncarnation, id);
		}


		#region Properties
		
		/// <summary>
		/// The id of the node.
		/// </summary>
		public string ID
		{
			get { return id; }
		}
		
		/// <summary>
		/// The name of the node.
		/// </summary>
		public string Name
		{
			get { return name; }
		}
		
		/// <summary>
		/// The path of the node.
		/// </summary>
		public string NodePath
		{
			get { return path; }
		}
		
		/// <summary>
		/// The master incarnation number of the node.
		/// </summary>
		public ulong MasterIncarnation
		{
			get { return masterIncarnation; }
		}
		
		/// <summary>
		/// The local incarnation number of the node.
		/// </summary>
		public ulong LocalIncarnation
		{
			get { return localIncarnation; }
		}

		/// <summary>
		/// Is the node a tombstone?
		/// </summary>
		public bool Tombstone
		{
			get { return ((masterIncarnation == 0) && (localIncarnation == 0)); }
		}

		#endregion
	}
}
