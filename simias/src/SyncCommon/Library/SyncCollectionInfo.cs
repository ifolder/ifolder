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

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Info
	/// </summary>
	[Serializable]
	public class SyncCollectionInfo : SyncNodeInfo
	{
		protected string url;
		protected string logicType;
		protected SyncCollectionRoles role;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public SyncCollectionInfo(SyncCollection collection) : base(collection)
		{
			this.url = collection.Url;
			this.logicType = collection.LogicType;
			this.role = collection.Role;
		}
		
		public override string ToString()
		{
			return String.Format("{0} {1}.{2} [{3}]", name, masterIncarnation, localIncarnation, id);
		}

		#region Properties

		public string Url
		{
			get { return url; }
		}
		
		public string LogicType
		{
			get { return logicType; }
		}

		public SyncCollectionRoles SyncRole
		{
			get { return role; }
		}

		#endregion
	}
}
