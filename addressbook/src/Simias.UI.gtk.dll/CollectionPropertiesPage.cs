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
using Simias.POBox;
using Simias;

using Gtk;
using Gdk;
using Glade;
using GtkSharp;
using GLib;


namespace Simias.UI.gtk
{
	public class CollectionPropertiesPage 
	{
		[Glade.Widget] private Gtk.ScrolledWindow	PropScrolledWindow = null;
		[Glade.Widget] private TreeView				PropertyTreeView = null;
		private ListStore 							PropertyTreeStore;
		private Pixbuf								PropertyPixBuf;

		private Collection collection;

		public Collection Collection
		{
			get
			{
				return collection;
			}

			set
			{
				collection = value;
			}
		}

		public Widget MainWidget
		{
			get
			{
				if(collection == null)
					return null;

				if(PropScrolledWindow == null)
					InitGlade();

				return PropScrolledWindow;
			}
		}

		public CollectionPropertiesPage()
		{
		}

		public void InitGlade()
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("collection-properties.glade"), 
					"PropScrolledWindow", 
					null);
			gxml.Autoconnect (this);

			Init_Page();
		}


		private void Init_Page()
		{
			PropertyTreeStore = new ListStore(typeof(Simias.Storage.Property));
			PropertyTreeView.Model = PropertyTreeStore;
			CellRendererPixbuf pcrp = new CellRendererPixbuf();
			TreeViewColumn ptvc = new TreeViewColumn();
			ptvc.PackStart(pcrp, false);
			ptvc.SetCellDataFunc(pcrp, new TreeCellDataFunc(PropertyCellPixbufDataFunc));

			CellRendererText pcrt = new CellRendererText();
			ptvc.PackStart(pcrt, false);
			ptvc.SetCellDataFunc(pcrt, new TreeCellDataFunc(PropertyCellTextDataFunc));
			ptvc.Title = "Properties";
			PropertyTreeView.AppendColumn(ptvc);

			PropertyTreeView.AppendColumn("Values", 
					new CellRendererText(), 
					new TreeCellDataFunc(ValueCellTextDataFunc));

			PropertyPixBuf = new Pixbuf(Util.ImagesPath("property.png"));
			foreach(Simias.Storage.Property prop in collection.Properties)
			{
				PropertyTreeStore.AppendValues(prop);
			}
		}

		private void PropertyCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Simias.Storage.Property prop = 
				(Simias.Storage.Property)
				PropertyTreeStore.GetValue(iter,0);
			if(prop != null)
			{
				((CellRendererText) cell).Text = prop.Name;
			}
			else
				((CellRendererText) cell).Text = "** unknown **";
		}

		private void PropertyCellPixbufDataFunc(Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = PropertyPixBuf;
		}

		private void ValueCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Simias.Storage.Property prop = 
				(Simias.Storage.Property)
				PropertyTreeStore.GetValue(iter,0);
			if(prop != null)
			{
				((CellRendererText) cell).Text = prop.Value.ToString();
			}
			else
				((CellRendererText) cell).Text = "** unknown **";
		}
	}
}
