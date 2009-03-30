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
