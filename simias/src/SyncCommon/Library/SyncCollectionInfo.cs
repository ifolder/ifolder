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
		/// <summary>
		/// The sync url of the collection.
		/// </summary>
		protected string url;

		/// <summary>
		/// The sync logic type of the collection.
		/// </summary>
		protected string logicType;

		/// <summary>
		/// The sync role of the collection.
		/// </summary>
		protected SyncCollectionRoles role;

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncCollectionInfo(SyncCollection collection) : base(collection)
		{
			this.url = collection.ServiceUrl;
			this.logicType = collection.LogicType;
			this.role = collection.Role;
		}
		
		/// <summary>
		/// Generate a string representation of the collection information.
		/// </summary>
		/// <returns>The string representation of the collection information.</returns>
		public override string ToString()
		{
			return String.Format("{0} {1}.{2} [{3}]", name, masterIncarnation, localIncarnation, id);
		}

		#region Properties

		/// <summary>
		/// The sync URL of the collection.
		/// </summary>
		public string Url
		{
			get { return url; }
		}
		
		/// <summary>
		/// The sync logic type of the collection.
		/// </summary>
		public string LogicType
		{
			get { return logicType; }
		}

		/// <summary>
		/// The sync role of the collection.
		/// </summary>
		public SyncCollectionRoles SyncRole
		{
			get { return role; }
		}

		#endregion
	}
}
