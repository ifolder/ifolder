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
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Author: Calvin Gaisford <cgaisford@novell.com>
 *
 ***********************************************************************/

using System;
using System.Drawing;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class BookEditEventArgs : EventArgs
	{
		private readonly string newBookName;
		private readonly bool changed;

		//Constructor.
		//
		public BookEditEventArgs(bool changed, string newName)
		{
			this.newBookName = newName;
			this.changed = changed;
		}

		public string NewName
		{     
			get { return newBookName;}      
		}

		public bool Changed
		{
			get {return changed;}
		}
	}

	// Delegate declaration
	//
	public delegate void BookEditEventHandler(object sender,
			BookEditEventArgs e);

	public class BookEditor
	{
		[Glade.Widget] internal Gtk.Entry beName;

		Gtk.Dialog 		beDlg = null;

		public event BookEditEventHandler BookEdited;

		public BookEditor () 
		{
			Glade.XML gxml = new Glade.XML ("addressbook.glade",
					"BookEditor", null);
			gxml.Autoconnect (this);

			beDlg = (Gtk.Dialog) gxml.GetWidget("BookEditor");
		}


		public void ShowAll()
		{
			if(beDlg != null)
			{
				beDlg.ShowAll();
			}
		}


		public void on_okButton_clicked(object o, EventArgs args)
		{
			if(BookEdited != null)
			{
				BookEditEventArgs e = new BookEditEventArgs(true, beName.Text);
				BookEdited(this, e);
			}

			CloseDialog();
		}


		public void on_cancelButton_clicked(object o, EventArgs args)
		{
			CloseDialog();
		}


		private void CloseDialog()
		{
			beDlg.Hide();
			beDlg.Destroy();
			beDlg = null;
		}


		private void on_BookEditor_delete_event(object o,
				DeleteEventArgs args) 
		{
			CloseDialog();
		}
	}
}
