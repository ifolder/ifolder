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
		private string id;
		private string name;
		private ulong masterIncarnation;
		private ulong localIncarnation;

		public SyncNodeInfo(Node node)
		{
			id = node.ID;
			name = node.Name;
			localIncarnation = node.LocalIncarnation;
			masterIncarnation = node.MasterIncarnation;
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

		public ulong LocalIncarnation
		{
			get { return localIncarnation; }
		}

		public ulong MasterIncarnation
		{
			get { return masterIncarnation; }
		}
		
		#endregion
	}
}
