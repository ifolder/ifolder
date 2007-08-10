/***********************************************************************
 |  $RCSfile: iFolderTreeView.cs,v $
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
 |   Author: Calvin Gaisford <cgaisford@novell.com>
 | 
 ***********************************************************************/


using Gtk;
using System;

namespace Novell.iFolder
{
	public class iFolderButtonPressEventArgs : EventArgs
	{
		private Gdk.EventButton eb;

		public Gdk.EventButton Event
		{
			get
			{
				return this.eb;
			}
		}
		public iFolderButtonPressEventArgs(Gdk.EventButton eb)
		{
			this.eb = eb;
		}
	}



	public class iFolderTreeView : Gtk.TreeView
	{
//		public event iFolderButtonPressEventHandler IFButtonPressed;

		public iFolderTreeView()
			: base()
		{
		}

		protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
		{
			base.OnButtonPressEvent(evnt);
			return false;

//			iFolderButtonPressEventArgs args = new ButtonPressEventArgs(evnt);

//			IFButtonPressed(this, args);
		}

	}
}
