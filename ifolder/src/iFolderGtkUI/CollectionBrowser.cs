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


namespace Novell.iFolder
{
	public class CollectionBrowser
	{
		[Glade.Widget] internal Gtk.TreeView	ColTreeView;
		[Glade.Widget] internal Gtk.TreeView	NodeTreeView;
		[Glade.Widget] internal Gtk.Window		ColWindow;

		private ListStore			ColTreeStore;
		private ListStore			NodeTreeStore;
		Pixbuf						CollectionPixBuf;
		Collection					curCol = null;
		Store						store;
		Hashtable					pbTable;

		public event EventHandler BrowserClosed;

		public CollectionBrowser() 
		{
			InitUI();

			Configuration config = Configuration.GetConfiguration();
			store = Store.GetStore();

			RefreshCollections();
			//			SetCurrentNode(null);
		}

		private void InitUI()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath("collection-browser.glade"), 
					"ColWindow", 
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


			// Setup the Node TreeView
			NodeTreeStore = new ListStore(typeof(ShallowNode));
			NodeTreeView.Model = NodeTreeStore;
			CellRendererPixbuf nodecrp = new CellRendererPixbuf();
			TreeViewColumn nodeNameColumn = new TreeViewColumn();
			nodeNameColumn.PackStart(nodecrp, false);
			nodeNameColumn.SetCellDataFunc(nodecrp, new TreeCellDataFunc(
						NodeCellPixbufDataFunc));

			CellRendererText nodecrt = new CellRendererText();
			nodeNameColumn.PackStart(nodecrt, false);
			nodeNameColumn.SetCellDataFunc(nodecrt, new TreeCellDataFunc(
						NodeNameCellTextDataFunc));
			nodeNameColumn.Title = "Name";
			NodeTreeView.AppendColumn(nodeNameColumn);
			NodeTreeView.Selection.Mode = SelectionMode.Multiple;

			TreeViewColumn nodeTypeColumn = new TreeViewColumn();
			CellRendererText nodeTypecrt = new CellRendererText();
			nodeTypeColumn.PackStart(nodeTypecrt, false);
			nodeTypeColumn.SetCellDataFunc(nodeTypecrt, new TreeCellDataFunc(
						NodeTypeCellTextDataFunc));
			nodeTypeColumn.Title = "Type";
			NodeTreeView.AppendColumn(nodeTypeColumn);

			TreeViewColumn nodeIDColumn = new TreeViewColumn();
			CellRendererText nodeIDcrt = new CellRendererText();
			nodeIDColumn.PackStart(nodeIDcrt, false);
			nodeIDColumn.SetCellDataFunc(nodeIDcrt, new TreeCellDataFunc(
						NodeIDCellTextDataFunc));
			nodeIDColumn.Title = "ID";
			NodeTreeView.AppendColumn(nodeIDColumn);


			pbTable = new Hashtable();

			string[] pixbufs = System.IO.Directory.GetFiles(Util.ImagesPath(""), "*.png");
			foreach(string filename in pixbufs)
			{
				FileInfo fi = new FileInfo(filename);
				String typeName = 
						fi.Name.Substring(0, fi.Name.LastIndexOf('.'));
				Pixbuf pb = new Pixbuf(filename);
				pbTable.Add(typeName, pb);
			}
		
			CollectionPixBuf = new Pixbuf(Util.ImagesPath("collection.png"));


			//			nifWindow.Icon = NodePixBuf;
			//			nifWindow.Title = "Simias Collection Browser";

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

		private void NodeNameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			ShallowNode sn = (ShallowNode) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = sn.Name;
		}

		private void NodeTypeCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			ShallowNode sn = (ShallowNode) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = sn.Type;
		}

		private void NodeIDCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			ShallowNode sn = (ShallowNode) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = sn.ID;
		}

		private void NodeCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			ShallowNode sn = (ShallowNode) tree_model.GetValue(iter,0);
			if(pbTable.Contains(sn.Type) == true)
			{
				((CellRendererPixbuf) cell).Pixbuf = (Pixbuf) pbTable[sn.Type];
			}
			else
				((CellRendererPixbuf) cell).Pixbuf = (Pixbuf) pbTable["Node"];
		}

		private void RefreshCollections()
		{
			ColTreeStore.Clear();

			foreach(ShallowNode sn in store)
			{
				Collection col = store.GetCollectionByID(sn.ID);
				ColTreeStore.AppendValues(col);
			}
		}


		private void RefreshNodes()
		{
			NodeTreeStore.Clear();
			curCol = null;

			TreeSelection tSelect = ColTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Collection col = (Collection) tModel.GetValue(iter, 0);
				curCol = col;
				foreach(ShallowNode sn in col)
				{
					if(col.ID != sn.ID)
					{
						NodeTreeStore.AppendValues(sn);
					}
				}
			}
		}

		public void ShowAll()
		{
			if(ColWindow != null)
				ColWindow.ShowAll();
		}

		private void on_ColWindow_delete_event (object o, DeleteEventArgs args) 
		{
			if(BrowserClosed != null)
			{
				EventArgs e = new EventArgs();
				BrowserClosed(this, e);
			}
		}

		public void on_refresh(object o, EventArgs args)
		{
			RefreshCollections();
		}

		public void on_create(object o, EventArgs args)
		{
		}

		public void on_delete(object o, EventArgs args)
		{
		}

		public void on_properties(object o, EventArgs args)
		{
			if(ColTreeView.HasFocus)
			{
				on_ColTreeView_row_activated(o, args);
			}
			else if(NodeTreeView.HasFocus)
			{
				on_NodeTreeView_row_activated(o, args);
			}
		}
		
		public void on_quit(object o, EventArgs args)
		{
			if(ColWindow != null)
			{
				ColWindow.Hide();
				ColWindow.Destroy();
			}

			if(BrowserClosed != null)
			{
				EventArgs e = new EventArgs();
				BrowserClosed(this, e);
			}
		}

		public void on_collection_selection_changed(object o, EventArgs args)
		{
			RefreshNodes();
		}

		public void on_NodeTreeView_row_activated(object o, EventArgs args)
		{
			TreeModel tModel;

			TreeSelection tSelect = NodeTreeView.Selection;
			Array treePaths = tSelect.GetSelectedRows(out tModel);
			// remove compiler warning
			if(tModel != null)
				tModel = null;

			foreach(TreePath tPath in treePaths)
			{
				TreeIter iter;

				if(NodeTreeStore.GetIter(out iter, tPath))
				{
					ShallowNode sn = 
							(ShallowNode) NodeTreeStore.GetValue(iter,0);
					if(curCol != null)
					{
						Node node = curCol.GetNodeByID(sn.ID);
						NodePropertiesDialog npd = 
							new NodePropertiesDialog();
						npd.Node = node;

						if(pbTable.Contains(sn.Type) == true)
						{
							npd.Pixbuf = (Pixbuf) pbTable[sn.Type];
						}
						else
						{
							npd.Pixbuf = (Pixbuf) pbTable["Node"];
						}
						npd.Title = sn.Name + " Properties";
						npd.ShowAll();
					}
				}
			}
		}


		public void on_ColTreeView_row_activated(object o, EventArgs args)
		{
			TreeSelection tSelect = ColTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Collection col = (Collection) tModel.GetValue(iter, 0);
				NodePropertiesDialog npd = new NodePropertiesDialog();
				npd.Node = col;
				npd.Pixbuf = CollectionPixBuf;
				npd.Title = col.Name + " Properties";
				npd.ShowAll();
			}
		}
	}




	public class NodePropertiesDialog
	{
		public string Title
		{
			set
			{
				this.title = value;
			}
		}

		public Node Node
		{
			set
			{
				this.node = value;
			}
		}

		public Pixbuf Pixbuf
		{
			set
			{
				this.pb = value;
			}
		}

		[Glade.Widget] internal Gtk.TreeView	PropTreeView;
		[Glade.Widget] internal Gtk.Dialog		ColPropertiesDialog;

		private ListStore			PropTreeStore;
		Node						node;
		Pixbuf						pb;
		string						title;

		public NodePropertiesDialog() 
		{
		}

		private void InitUI()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath("collection-browser.glade"), 
					"ColPropertiesDialog", 
					null);
			gxml.Autoconnect (this);

			if(pb != null)
				ColPropertiesDialog.Icon = pb;
			if(title.Length > 0)
			{
				ColPropertiesDialog.Title = title;	
			}

			// Setup the Node TreeView
			PropTreeStore = new ListStore(typeof(Simias.Storage.Property));
			PropTreeView.Model = PropTreeStore;
			TreeViewColumn propNameColumn = new TreeViewColumn();

			CellRendererText propcrt = new CellRendererText();
			propNameColumn.PackStart(propcrt, false);
			propNameColumn.SetCellDataFunc(propcrt, new TreeCellDataFunc(
						PropNameCellTextDataFunc));
			propNameColumn.Title = "Name";
			PropTreeView.AppendColumn(propNameColumn);
			PropTreeView.Selection.Mode = SelectionMode.Multiple;

			TreeViewColumn propValueColumn = new TreeViewColumn();
			CellRendererText propValuecrt = new CellRendererText();
			propValueColumn.PackStart(propValuecrt, false);
			propValueColumn.SetCellDataFunc(propValuecrt, new TreeCellDataFunc(
						PropValueCellTextDataFunc));
			propValueColumn.Title = "Value";
			PropTreeView.AppendColumn(propValueColumn);

			if(node != null)
			{
				foreach(Simias.Storage.Property prop in node.Properties)
				{
					PropTreeStore.AppendValues(prop);
				}
			}
		}



		private void PropNameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Simias.Storage.Property prop = 
					(Simias.Storage.Property) PropTreeStore.GetValue(iter,0);
			if(prop != null)
			{
				((CellRendererText) cell).Text = prop.Name;
			}
			else
				((CellRendererText) cell).Text = "** unknown **";
		}

		private void PropValueCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Simias.Storage.Property prop = 
					(Simias.Storage.Property) PropTreeStore.GetValue(iter,0);
			if(prop != null)
			{
				((CellRendererText) cell).Text = prop.Value.ToString();
			}
			else
				((CellRendererText) cell).Text = "** unknown **";
		}

		public void ShowAll()
		{
			InitUI();

			if(ColPropertiesDialog != null)
				ColPropertiesDialog.ShowAll();
		}

		public void on_CancelButton_clicked(object o, EventArgs args)
		{
			ColPropertiesDialog.Hide();
			ColPropertiesDialog.Destroy();
		}
	}

}
