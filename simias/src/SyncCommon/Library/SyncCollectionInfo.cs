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

namespace Simias.Sync
{
	/// <summary>
	/// Sync Collection Info
	/// </summary>
	[Serializable]
	public class SyncCollectionInfo
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
		/// The master incarnation number of the node.
		/// </summary>
		protected ulong masterIncarnation;

		/// <summary>
		/// The local incarnation number of the node.
		/// </summary>
		protected ulong localIncarnation;
	
		/// <summary>
		/// The sync url of the collection.
		/// </summary>
		protected string url;

		/// <summary>
		/// The sync role of the collection.
		/// </summary>
		protected SyncCollectionRoles role;

		/// <summary>
		/// Constructor
		/// </summary>
		public SyncCollectionInfo(SyncCollection collection)
		{
			this.id = collection.ID;
			this.name = collection.Name;
			this.masterIncarnation = collection.MasterIncarnation;
			this.localIncarnation = collection.LocalIncarnation;
			this.url = collection.ServiceUrl;
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
		/// The sync URL of the collection.
		/// </summary>
		public string Url
		{
			get { return url; }
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
