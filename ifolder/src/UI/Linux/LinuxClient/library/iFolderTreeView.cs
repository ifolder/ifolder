/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Calvin Gaisford <cgaisford@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        <Description of the functionality of the file >
  *
  *
  *******************************************************************************/

using Gtk;
using System;

namespace Novell.iFolder
{
    /// <summary>
    /// class iFolderButtonPressEventArgs
    /// </summary>
	public class iFolderButtonPressEventArgs : EventArgs
	{
		private Gdk.EventButton eb;

        /// <summary>
        /// Gets Button Event
        /// </summary>
		public Gdk.EventButton Event
		{
			get
			{
				return this.eb;
			}
		}

        /// <summary>
        /// Cnostructor
        /// </summary>
        /// <param name="eb">EventButton</param>
        public iFolderButtonPressEventArgs(Gdk.EventButton eb)
		{
			this.eb = eb;
		}
	}


    /// <summary>
    /// class iFOlder Tree View
    /// </summary>
	public class iFolderTreeView : Gtk.TreeView
	{
//		public event iFolderButtonPressEventHandler IFButtonPressed;

        /// <summary>
        /// Constructor
        /// </summary>
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
