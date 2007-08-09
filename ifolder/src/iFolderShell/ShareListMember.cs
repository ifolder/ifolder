/****************************************************************************
 |
 | Copyright (c) 2007 Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com
 |
 | Author: Bruce Getter <bgetter@novell.com>
 |**************************************************************************/

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

		/// <summary>
		/// Gets the name of the iFolder user.
		/// </summary>
		public string Name
		{
			get
			{
				return (ifolderUser.FN != null) && !ifolderUser.FN.Equals(string.Empty) ? ifolderUser.FN : ifolderUser.Name;
			}
		}
		#endregion
	}
}
