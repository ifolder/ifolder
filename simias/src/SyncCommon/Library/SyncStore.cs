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

using Simias;
using Simias.Storage;

namespace Simias.Sync
{
	/// <summary>
	/// Sync Store
	/// </summary>
	public class SyncStore : Store
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SyncStore(Configuration configuration) : base(configuration)
		{

		}

		/// <summary>
		/// Open a colection object with the given id.
		/// </summary>
		/// <param name="id">The id (guid) of the collection to object.</param>
		/// <returns>The collection object.</returns>
		public SyncCollection OpenCollection(string id)
		{
			return null;
		}

		/// <summary>
		/// The remoting end point for the sync store service.
		/// </summary>
		public static string GetEndPoint(int port)
		{
			return String.Format("SyncStoreService{0}.rem", port);
		}

		#region Properties
		
		/// <summary>
		/// The store id.
		/// </summary>
		public string ID
		{
			get { return base.GetDatabaseObject().ID; }
		}

		#endregion
	}
}
