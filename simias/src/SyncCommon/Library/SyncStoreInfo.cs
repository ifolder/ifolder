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
	/// Sync Store Info
	/// </summary>
	[Serializable]
	public class SyncStoreInfo
	{
		private string machine;
		private string user;
		private string os;
		private string storeID;

		internal SyncStoreInfo(SyncStore store)
		{
			machine = Environment.MachineName;
			user = Environment.UserName;
			os = Environment.OSVersion.ToString();
			storeID = store.ID;
		}
		
		/// <summary>
		/// Generate a string representation of the store information.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("Machine: {0}, User: {1}, OS: {2}, Store: {3}", machine, user, os, storeID);
		}

		#region Properties
		
		/// <summary>
		/// The store machine name.
		/// </summary>
		public string Machine
		{
			get { return machine; }
		}
		
		/// <summary>
		/// The store user name.
		/// </summary>
		public string User
		{
			get { return user; }
		}
		
		/// <summary>
		/// The store operating system name.
		/// </summary>
		public string OS
		{
			get { return os; }
		}
		
		/// <summary>
		/// The store id.
		/// </summary>
		public string StoreID
		{
			get { return storeID; }
		}

		#endregion
	}
}
