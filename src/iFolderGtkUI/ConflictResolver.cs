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
		[Glade.Widget] private Gtk.Label		LocalSizeLabel = null;
		[Glade.Widget] private Gtk.Label		LNameLabel = null;
		[Glade.Widget] private Gtk.Label		LDateLabel = null;
		[Glade.Widget] private Gtk.Label		LSizeLabel = null;
		[Glade.Widget] private Gtk.Button		LocalOpenButton = null;

		[Glade.Widget] private Gtk.Label		ServerNameLabel = null;
		[Glade.Widget] private Gtk.Label		ServerDateLabel = null;
		[Glade.Widget] private Gtk.Label		ServerSizeLabel = null;
		[Glade.Widget] private Gtk.Label		SNameLabel = null;
		[Glade.Widget] private Gtk.Label		SDateLabel = null;
		[Glade.Widget] private Gtk.Label		SSizeLabel = null;
		[Glade.Widget] private Gtk.Button		ServerOpenButton = null;

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
			conflictColumn.Title = "Name";
			ConflictTreeView.AppendColumn(conflictColumn);

			TreeViewColumn pathColumn = new TreeViewColumn();
			CellRendererText pathRT = new CellRendererText();
			pathColumn.PackStart(pathRT, false);
			pathColumn.SetCellDataFunc(pathRT, new TreeCellDataFunc(
						PathCellTextDataFunc));
			pathColumn.Title = "Path";
			ConflictTreeView.AppendColumn(pathColumn);

			ConflictTreeView.Selection.Changed += new EventHandler(
						on_conflict_selection_changed);

			conflictPixBuf = 
				new Pixbuf(Util.ImagesPath("file.png"));
		}

		private void PathCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			Node conflictNode = (Node) tree_model.GetValue(iter,0);
			FileNode fileNode = new FileNode(conflictNode);
			((CellRendererText) cell).Text = fileNode.GetFullPath(ifolder);
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
				LocalSizeLabel.Text = "";
				ServerNameLabel.Text = "";
				ServerDateLabel.Text = "";
				ServerSizeLabel.Text = "";

				LocalRadioButton.Sensitive = false;
				ServerRadioButton.Sensitive = false;

				LocalNameLabel.Sensitive = false;
				LocalDateLabel.Sensitive = false;
				LocalSizeLabel.Sensitive = false;
				LNameLabel.Sensitive = false;
				LDateLabel.Sensitive = false;
				LSizeLabel.Sensitive = false;
				LocalOpenButton.Sensitive = false;
				ServerNameLabel.Sensitive = false;
				ServerDateLabel.Sensitive = false;
				ServerSizeLabel.Sensitive = false;
				SNameLabel.Sensitive = false;
				SDateLabel.Sensitive = false;
				SSizeLabel.Sensitive = false;
				ServerOpenButton.Sensitive = false;
			}
			else
			{
				FileNode fileNode = new FileNode(conflictNode);
				Node localNode = ifolder.GetNodeFromCollision(conflictNode);
				FileNode localfileNode = new FileNode(localNode);

				LocalNameLabel.Text = localfileNode.GetFileName();
				LocalDateLabel.Text = localfileNode.LastWriteTime.ToString();
				LocalSizeLabel.Text = "N/A";
				ServerNameLabel.Text = fileNode.GetFileName();
				ServerDateLabel.Text = fileNode.LastWriteTime.ToString();
				ServerSizeLabel.Text = "N/A";
				LocalOpenButton.Sensitive = true;
				ServerOpenButton.Sensitive = true;

				LocalRadioButton.Sensitive = true;
				LocalRadioButton.Active = true;
				ServerRadioButton.Sensitive = true;

				LocalNameLabel.Sensitive = true;
				LocalDateLabel.Sensitive = true;
				LocalSizeLabel.Sensitive = true;
				LNameLabel.Sensitive = true;
				LDateLabel.Sensitive = true;
				LSizeLabel.Sensitive = true;
				ServerNameLabel.Sensitive = true;
				ServerDateLabel.Sensitive = true;
				ServerSizeLabel.Sensitive = true;
				SNameLabel.Sensitive = true;
				SDateLabel.Sensitive = true;
				SSizeLabel.Sensitive = true;
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
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Node conflictNode = (Node) tModel.GetValue(iter, 0);

				UpdateFields(conflictNode);
			}
		}


		public void on_local_open(object o, EventArgs args)
		{
		}

		public void on_server_open(object o, EventArgs args)
		{
		}

		public void on_auto(object o, EventArgs args)
		{
			// test to see if there is anyting in the store
			if(ConflictTreeStore.IterNChildren() > 0)
			{
				MessageDialog dialog = new MessageDialog(ConflictDialog,
					DialogFlags.Modal | DialogFlags.DestroyWithParent,
					MessageType.Question,
					ButtonsType.None,
					"Auto Resolution will resolve all conflicts.  Which copy should all conflicts be resolved to?");
				dialog.Title = "Auto Resolve Conflicts";
				dialog.AddButton("Local Copy", -1);
				dialog.AddButton("Server Copy", -2);
				dialog.AddButton("Cancel", -9);
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				switch(rc)
				{
					case -1:	// resolve all local
						resolveAll(true);
						break;
					case -2:	// resolve all server
						resolveAll(false);
						break; 
					case -9:	// cancel
						break;
				}
			}
			else
			{
				MessageDialog dialog = new MessageDialog(ConflictDialog,
					DialogFlags.Modal | DialogFlags.DestroyWithParent,
					MessageType.Info,
					ButtonsType.Ok,
					"There are no conflicts to resolve");
				dialog.Title = "Conflict Resolver Message";
				dialog.Run();
				dialog.Hide();
				dialog.Destroy();
			}
		}


		private void resolveAll(bool local)
		{
			TreeIter iter;

			while(ConflictTreeStore.GetIterFirst(out iter))
			{
				Node conflictNode = (Node) ConflictTreeStore.GetValue(iter, 0);
				Simias.Sync.Conflict conflict = 
					new Simias.Sync.Conflict(ifolder, conflictNode);

				if(local)
					conflict.Resolve(true);
				else
					conflict.Resolve(false);

				ConflictTreeStore.Remove(ref iter);
				UpdateFields(null);
			}
		}

		public void on_resolve(object o, EventArgs args)
		{
			TreeSelection tSelect = ConflictTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				Node conflictNode = (Node) tModel.GetValue(iter, 0);
				Simias.Sync.Conflict conflict = 
					new Simias.Sync.Conflict(ifolder, conflictNode);

				if(LocalRadioButton.Active == true)
					conflict.Resolve(true);
				else
					conflict.Resolve(false);

				ConflictTreeStore.Remove(ref iter);
				UpdateFields(null);
			}
		}

		public void on_refresh(object o, EventArgs args)
		{
			ConflictTreeStore.Clear();
			UpdateFields(null);

			if(ifolder != null)
			{
				ICSList collisionList = ifolder.GetCollisions();
				foreach (ShallowNode sn in collisionList)
				{
					// Get the collision node.
					Node conflictNode = new Node(ifolder, sn);

					if(ifolder.IsType(conflictNode, typeof(BaseFileNode).Name))
					{
						ConflictTreeStore.AppendValues(conflictNode);
					}
				}
			}
		}
	}
}
