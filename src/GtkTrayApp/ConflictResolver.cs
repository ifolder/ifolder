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

namespace Novell.iFolder
{
	using System;
	using System.IO;
	using System.Drawing;
	using Simias.Storage;
	using Simias.Sync;
	using Simias;

	using Gtk;
	using Gdk;
	using Glade;
	using GtkSharp;
	using GLib;

	public class ConflictResolver 
	{
		[Glade.Widget] private Gtk.Dialog 		ConflictDialog = null;
		[Glade.Widget] private Gtk.TreeView		ConflictTreeView = null;

		[Glade.Widget] private Gtk.Label		iFolderNameLabel = null;
		[Glade.Widget] private Gtk.Label		iFolderPathLabel = null;

		[Glade.Widget] private Gtk.RadioButton	LocalRadioButton = null;
		[Glade.Widget] private Gtk.RadioButton	ServerRadioButton = null;

		[Glade.Widget] private Gtk.Label		LocalNameLabel = null;
		[Glade.Widget] private Gtk.Label		LocalDateLabel = null;
		[Glade.Widget] private Gtk.Label		LocalLocLabel = null;
		[Glade.Widget] private Gtk.Label		LocalTypeLabel = null;
		[Glade.Widget] private Gtk.Label		LNameLabel = null;
		[Glade.Widget] private Gtk.Label		LDateLabel = null;
		[Glade.Widget] private Gtk.Label		LLocLabel = null;
		[Glade.Widget] private Gtk.Label		LTypeLabel = null;

		[Glade.Widget] private Gtk.Label		ServerNameLabel = null;
		[Glade.Widget] private Gtk.Label		ServerDateLabel = null;
		[Glade.Widget] private Gtk.Label		ServerLocLabel = null;
		[Glade.Widget] private Gtk.Label		ServerTypeLabel = null;
		[Glade.Widget] private Gtk.Label		SNameLabel = null;
		[Glade.Widget] private Gtk.Label		SDateLabel = null;
		[Glade.Widget] private Gtk.Label		SLocLabel = null;
		[Glade.Widget] private Gtk.Label		STypeLabel = null;

		private ListStore		ConflictTreeStore = null;
		private Pixbuf			conflictPixBuf = null;
		private iFolder			ifolder = null;

		public Gtk.Window TransientFor
		{
			set
			{
				if(ConflictDialog != null)
					ConflictDialog.TransientFor = value;
			}
		}

		public iFolder iFolder
		{
			set
			{
				ifolder = value;
			}
		}

		public ConflictResolver() 
		{
		}

		public void InitGlade()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath(
					"conflict-resolver.glade"), 
					"ConflictDialog", 
					null);
			gxml.Autoconnect (this);

			// Setup the Conflict TreeView
			ConflictTreeStore = new ListStore(typeof(Node));
			ConflictTreeView.Model = ConflictTreeStore;
			CellRendererPixbuf conflictRP = new CellRendererPixbuf();
			TreeViewColumn conflictColumn = new TreeViewColumn();
			conflictColumn.PackStart(conflictRP, false);
			conflictColumn.SetCellDataFunc(conflictRP, new TreeCellDataFunc(
						ConflictCellPixbufDataFunc));

			CellRendererText conflictRT = new CellRendererText();
			conflictColumn.PackStart(conflictRT, false);
			conflictColumn.SetCellDataFunc(conflictRT, new TreeCellDataFunc(
						ConflictCellTextDataFunc));
			conflictColumn.Title = "Conflict List";
			ConflictTreeView.AppendColumn(conflictColumn);
			ConflictTreeView.Selection.Changed += new EventHandler(
						on_conflict_selection_changed);

			conflictPixBuf = 
				new Pixbuf(Util.ImagesPath("ifolder-collision.png"));
		}

		private void ConflictCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Node conflictNode = (Node) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = conflictNode.Name;
		}


		private void ConflictCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = conflictPixBuf;
		}


		private void PopulateWidgets()
		{
			on_refresh(null, null);

			if(ifolder != null)
			{
				iFolderNameLabel.Text = ifolder.Name;
				iFolderPathLabel.Text = ifolder.LocalPath;
			}
			else
			{
				iFolderNameLabel.Text = "";
				iFolderPathLabel.Text = "";
			}
		}

		private void UpdateFields(Node conflictNode)
		{
			if(conflictNode == null)
			{
				LocalNameLabel.Text = "";
				LocalDateLabel.Text = "";
				LocalLocLabel.Text = "";
				LocalTypeLabel.Text = "";
				ServerNameLabel.Text = "";
				ServerDateLabel.Text = "";
				ServerLocLabel.Text = "";
				ServerTypeLabel.Text = "";

				LocalRadioButton.Sensitive = false;
				ServerRadioButton.Sensitive = false;

				LocalNameLabel.Sensitive = false;
				LocalDateLabel.Sensitive = false;
				LocalLocLabel.Sensitive = false;
				LocalTypeLabel.Sensitive = false;
				LNameLabel.Sensitive = false;
				LDateLabel.Sensitive = false;
				LLocLabel.Sensitive = false;
				LTypeLabel.Sensitive = false;
				ServerNameLabel.Sensitive = false;
				ServerDateLabel.Sensitive = false;
				ServerLocLabel.Sensitive = false;
				ServerTypeLabel.Sensitive = false;
				SNameLabel.Sensitive = false;
				SDateLabel.Sensitive = false;
				SLocLabel.Sensitive = false;
				STypeLabel.Sensitive = false;
			}
			else
			{
				LocalNameLabel.Text = "";
				LocalDateLabel.Text = "";
				LocalLocLabel.Text = "";
				LocalTypeLabel.Text = "";
				ServerNameLabel.Text = "";
				ServerDateLabel.Text = "";
				ServerLocLabel.Text = "";
				ServerTypeLabel.Text = "";

				LocalRadioButton.Sensitive = true;
				ServerRadioButton.Sensitive = true;

				LocalNameLabel.Sensitive = true;
				LocalDateLabel.Sensitive = true;
				LocalLocLabel.Sensitive = true;
				LocalTypeLabel.Sensitive = true;
				LNameLabel.Sensitive = true;
				LDateLabel.Sensitive = true;
				LLocLabel.Sensitive = true;
				LTypeLabel.Sensitive = true;
				ServerNameLabel.Sensitive = true;
				ServerDateLabel.Sensitive = true;
				ServerLocLabel.Sensitive = true;
				ServerTypeLabel.Sensitive = true;
				SNameLabel.Sensitive = true;
				SDateLabel.Sensitive = true;
				SLocLabel.Sensitive = true;
				STypeLabel.Sensitive = true;
			}
		}

		public int Run()
		{
			InitGlade();
			PopulateWidgets();

			int rc = 0;
			if(ConflictDialog != null)
			{
				while(rc == 0)
				{
					rc = ConflictDialog.Run();
					if(rc == -11) // help
					{
						rc = 0;
						Util.ShowHelp("front.html", null);
					}
				}

				ConflictDialog.Hide();
				ConflictDialog.Destroy();
				ConflictDialog = null;
			}
			return rc;
		}


		public void on_conflict_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = ConflictTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
	//			TreeModel tModel;
	//			TreeIter iter;

	//			tSelect.GetSelected(out tModel, out iter);
	//			iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

	//			This appears to hang?
	//			SyncSize.CalculateSendSize(	ifolder, 
	//										out nodeCount, 
	//										out bytesToSend);
			}
		}


		public void on_open(object o, EventArgs args)
		{
		}

		public void on_auto(object o, EventArgs args)
		{
		}

		public void on_resolve(object o, EventArgs args)
		{
		}

		public void on_refresh(object o, EventArgs args)
		{
			ConflictTreeStore.Clear();

			if(ifolder != null)
			{
				ICSList collisionList = ifolder.GetCollisions();
				foreach (ShallowNode sn in collisionList)
				{
					// Get the collision node.
					Node conflictNode = new Node(ifolder, sn);

					ConflictTreeStore.AppendValues(conflictNode);
				}
			}
		}
	}
}
