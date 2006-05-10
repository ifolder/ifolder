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
using System.IO;
using System.Drawing;
using System.Collections;
using Simias.Storage;
using Simias;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.AddressBook.UI.gtk
{
	public class CollectionPicker
	{
		[Glade.Widget] private Gtk.Dialog	CollectionDialog = null;
		[Glade.Widget] private Gtk.TreeView	ColTreeView = null;
		[Glade.Widget] private Gtk.Button	OKButton = null;

		private ListStore		ColTreeStore;
		private Pixbuf			CollectionPixBuf;
		private Collection		curCol = null;
		private Store			store;
		private string			ignoreType = "";

		public string IgnoreType
		{
			set
			{
				ignoreType = value;
			}
		}

		public Gtk.Window TransientFor
		{
			set
			{
				if(CollectionDialog != null)
					CollectionDialog.TransientFor = value;
			}
		}

		public Collection Collection
		{
			get
			{
				return curCol;
			}
		}

		public CollectionPicker() 
		{
			InitUI();
			store = Store.GetStore();
		}

		private void InitUI()
		{
			Glade.XML gxml = new 
					Glade.XML(Util.GladePath("collection-picker.glade"), 
					"CollectionDialog", 
					null);
			gxml.Autoconnect (this);

			// Setup the Collection TreeView
			ColTreeStore = new ListStore(typeof(Collection));
			ColTreeView.Model = ColTreeStore;
			CellRendererPixbuf colcrp = new CellRendererPixbuf();
			TreeViewColumn coltvc = new TreeViewColumn();
			coltvc.PackStart(colcrp, false);
			coltvc.SetCellDataFunc(colcrp, new TreeCellDataFunc(
						ColCellPixbufDataFunc));

			CellRendererText colcrt = new CellRendererText();
			coltvc.PackStart(colcrt, false);
			coltvc.SetCellDataFunc(colcrt, new TreeCellDataFunc(
						ColCellTextDataFunc));
			coltvc.Title = "Collections";
			ColTreeView.AppendColumn(coltvc);
			ColTreeView.Selection.Changed += new EventHandler(
						on_collection_selection_changed);

			CollectionPixBuf = new Pixbuf(Util.ImagesPath("teamspace.png"));
			OKButton.Sensitive = false;
		}


		private void ColCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Collection col = (Collection) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = col.Name;
		}


		private void ColCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = CollectionPixBuf;
		}

		private void ReadCollections()
		{
			foreach(ShallowNode sn in store)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				if(!col.Sealed)
				{
					// Ignore any collections that were
					// requested to be ignored
					if(!col.IsType(col, ignoreType))
						ColTreeStore.AppendValues(col);
				}
			}
		}

		public int Run()
		{
			int rc = 0;
			if(CollectionDialog != null)
			{
				ReadCollections();
				rc = CollectionDialog.Run();
				CollectionDialog.Hide();
				CollectionDialog.Destroy();
				CollectionDialog = null;
			}
			return rc;
		}


		public void on_collection_selection_changed(object o, EventArgs args)
		{
			curCol = null;

			TreeSelection tSelect = ColTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Collection col = (Collection) tModel.GetValue(iter, 0);
				curCol = col;
				OKButton.Sensitive = true;
			}
			else
				OKButton.Sensitive = false;
		}
	}
}
