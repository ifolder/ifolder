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
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for iFolderInfo.
	/// </summary>
	[ComVisible(false)]
	public class iFolderInfo
	{
		#region Class Members
		private string localPath;
		private string id;
		#endregion

		/// <summary>
		/// Constructs a ShallowiFolder object.
		/// </summary>
		public iFolderInfo()
		{
		}

		#region Properties
		/// <summary>
		/// Gets/sets the local path for this iFolder.
		/// </summary>
		public string LocalPath
		{
			get { return localPath; }
			set { localPath = value; }
		}

		/// <summary>
		/// Gets/sets the ID for this iFolder.
		/// </summary>
		public string ID
		{
			get { return id; }
			set { id = value; }
		}
		#endregion

		/// <summary>
		/// This method overrides Object.ToString().
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return localPath;
		}

	}
}
