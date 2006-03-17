/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
	public enum MoveDirection
	{
		Down,
		Right,
		Up
	}

	/// <summary>
	/// Summary description for NavigateItemEventArgs.
	/// </summary>
	public class NavigateItemEventArgs : EventArgs
	{
		#region Class Members
		private int row;
		private int column;
		private MoveDirection direction;
		#endregion

		#region Constructor
		public NavigateItemEventArgs( int row, int column, MoveDirection direction )
		{
			this.row = row;
			this.column = column;
			this.direction = direction;
		}
		#endregion

		#region Properties
		public int Column
		{
			get { return column; }
		}

		public MoveDirection Direction
		{
			get { return direction; }
		}

		public int Row
		{
			get { return row; }
		}
		#endregion
	}
}
