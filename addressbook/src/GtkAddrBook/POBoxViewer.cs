/************************************************************************
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
using Novell.AddressBook;
using Simias.Storage;
using System.Collections;
using System.IO;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.AddressBook.UI.gtk
{
	public class POBoxViewer
	{
		[Glade.Widget] private Gnome.App	POViewerApp = null;
		[Glade.Widget] private TreeView		SubTreeView = null;

		private ListStore	SubTreeStore;

		public event EventHandler ViewerClosed;

		public POBoxViewer() 
		{
			Init();
		}
		public void Init () 
		{
			Glade.XML gxml = 
				new Glade.XML (Util.GladePath("pobox-viewer.glade"),
				"POViewerApp", null);

			gxml.Autoconnect (this);

			// ****************************************
			// Book Tree View Setup
			// ****************************************
			SubTreeStore = new ListStore(typeof(string));
			SubTreeView.Model = SubTreeStore;

			CellRendererPixbuf bcrp = new CellRendererPixbuf();
			TreeViewColumn btvc = new TreeViewColumn();
			btvc.PackStart(bcrp, false);
			btvc.SetCellDataFunc(bcrp, new TreeCellDataFunc(
						SubCellPixbufDataFunc));

			CellRendererText bcrt = new CellRendererText();
			btvc.PackStart(bcrt, false);
			btvc.SetCellDataFunc(bcrt, new TreeCellDataFunc(
						SubCellTextDataFunc));
			btvc.Title = "Subscription";
			SubTreeView.AppendColumn(btvc);
//			BookTreeView.Selection.Changed +=
//				new EventHandler(on_book_selection_changed);
		}


		private void SubCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			AddressBook book = (AddressBook) BookTreeStore.GetValue(iter,0);
			((CellRendererText) cell).Text = "new";
		}

		private void SubCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			((CellRendererPixbuf) cell).Pixbuf = BookPixBuf;
		}

		public void ShowAll()
		{
			POViewerApp.ShowAll();
		}

		public void on_POViewerApp_delete_event(object o, DeleteEventArgs args) 
		{
			args.RetVal = true;
			//on_quit(o, args);
		}


		public void on_refresh(object o, EventArgs eventArgs)
		{
		}

		public void on_quit(object o, EventArgs eventArgs)
		{
		}
	}
}
