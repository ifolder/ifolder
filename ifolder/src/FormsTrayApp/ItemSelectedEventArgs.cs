/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
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
 |
 |***************************************************************************/

using System;
using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for ItemSelectedEventArgs.
	/// </summary>
	public class ItemSelectedEventArgs : EventArgs
	{
		#region Class Members

		private TileListViewItem tlvi;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs an ItemSelectedEventArgs object.
		/// </summary>
		/// <param name="lvi">The selected ListViewItem.</param>
		public ItemSelectedEventArgs( TileListViewItem tlvi )
		{
			this.tlvi = tlvi;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the selected ListViewItem.
		/// </summary>
		public TileListViewItem Item
		{
			get { return tlvi; }
		}

		#endregion
	}
}
