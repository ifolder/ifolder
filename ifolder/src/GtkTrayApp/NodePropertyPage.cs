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
using System.Collections;
using System.Drawing;
using Simias.Storage;

using Gtk;
using Gdk;
using Gnome;
using Glade;
using GtkSharp;
using GLib;

namespace Novell.iFolder
{
	public class NodePropertyPage
	{
		[Glade.Widget] TreeView	PropertyTreeView = null;

		Gtk.VBox PropertyVBox;
		ListStore PropertyTreeStore;
		Pixbuf	PropertyPixBuf;
		Node	node;

		public NodePropertyPage (Node node)
		{
			this.node = node;

			Glade.XML gxml = new Glade.XML ("ifolder.glade", "NodePropertiesPage", null);

			gxml.Autoconnect (this);

			PropertyVBox = (Gtk.VBox) gxml.GetWidget("NodePropertiesPage");

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

			PropertyPixBuf = new Pixbuf("property.png");
			foreach(Simias.Storage.Property prop in this.node.Properties)
			{
				PropertyTreeStore.AppendValues(prop);
			}
		}

		public Gtk.Widget GetWidget()
		{
			return PropertyVBox;
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

		private void on_unrealize(object o, EventArgs args) 
		{
			// Close out the contact picker
		}
	}
}
