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
	/// Class to hold objects displayed in the shareWith listview.
	/// </summary>
	[ComVisible(false)]
	public class ShareListMember
	{
		private iFolderUser ifolderUser;
		private bool added = false;
		private bool changed = false;

		#region Constructors
		/// <summary>
		/// Constructs a ShareListMember object.
		/// </summary>
		public ShareListMember()
		{
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the added flag.
		/// </summary>
		public bool Added
		{
			get { return added; }
			set { added = value; }
		}

		/// <summary>
		/// Gets/sets the changed flag.
		/// </summary>
		public bool Changed
		{
			get { return changed; }
			set { changed = value; }
		}

		/// <summary>
		/// Gets/sets the iFolder user.
		/// </summary>
		public iFolderUser iFolderUser
		{
			get { return ifolderUser; }
			set { ifolderUser = value; }
		}
		#endregion
	}
}
