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
using Gnome;
using Glade;
using GtkSharp;
using GLib;


namespace Novell.iFolder
{
	public class iFolderBrowser
	{
		[Glade.Widget] internal Gtk.TreeView	iFolderTreeView;
		[Glade.Widget] internal Gtk.TreeView	FileTreeView;
		[Glade.Widget] internal Gtk.Window		iFolderWindow;
		[Glade.Widget] internal Gtk.Entry		CurrentPathEntry;

		private ListStore			iFolderTreeStore;
		private ListStore			NodeTreeStore;
		Pixbuf						iFolderPixBuf;
		Pixbuf						filePixBuf;
		Pixbuf						folderPixBuf;
		iFolderManager				manager;
		DirectoryEntry				curDirEntry;
		DirectoryEntry				topDirEntry;

		public event EventHandler BrowserClosed;

		public iFolderBrowser() 
		{
			InitUI();

			manager = iFolderManager.Connect();

			RefreshViews();
		}

		private void InitUI()
		{
			Glade.XML gxml = 
					new Glade.XML (Util.GladePath("ifolder-browser.glade"), 
					"iFolderWindow", 
					null);
			gxml.Autoconnect (this);

			// Setup the Collection TreeView
			iFolderTreeStore = new ListStore(typeof(iFolder));
			iFolderTreeView.Model = iFolderTreeStore;
			CellRendererPixbuf ifcrp = new CellRendererPixbuf();
			TreeViewColumn iftvc = new TreeViewColumn();
			iftvc.PackStart(ifcrp, false);
			iftvc.SetCellDataFunc(ifcrp, new TreeCellDataFunc(
						iFolderCellPixbufDataFunc));

			CellRendererText ifcrt = new CellRendererText();
			iftvc.PackStart(ifcrt, false);
			iftvc.SetCellDataFunc(ifcrt, new TreeCellDataFunc(
						iFolderCellTextDataFunc));
			iftvc.Title = "iFolders";
			iFolderTreeView.AppendColumn(iftvc);
			iFolderTreeView.Selection.Changed += new EventHandler(
						on_iFolder_selection_changed);


			// Setup the Node TreeView
			NodeTreeStore = new ListStore(typeof(DirectoryEntry));
			FileTreeView.Model = NodeTreeStore;
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
			FileTreeView.AppendColumn(nodeNameColumn);
			FileTreeView.Selection.Mode = SelectionMode.Multiple;

			TreeViewColumn nodeTypeColumn = new TreeViewColumn();
			CellRendererText nodeTypecrt = new CellRendererText();
			nodeTypeColumn.PackStart(nodeTypecrt, false);
			nodeTypeColumn.SetCellDataFunc(nodeTypecrt, new TreeCellDataFunc(
						NodeTypeCellTextDataFunc));
			nodeTypeColumn.Title = "Type";
			FileTreeView.AppendColumn(nodeTypeColumn);

			TreeViewColumn nodeIDColumn = new TreeViewColumn();
			CellRendererText nodeIDcrt = new CellRendererText();
			nodeIDColumn.PackStart(nodeIDcrt, false);
			nodeIDColumn.SetCellDataFunc(nodeIDcrt, new TreeCellDataFunc(
						NodeIDCellTextDataFunc));
			nodeIDColumn.Title = "ID";
			FileTreeView.AppendColumn(nodeIDColumn);

			iFolderPixBuf = new Pixbuf(Util.ImagesPath("ifolder.png"));
			filePixBuf = new Pixbuf(Util.ImagesPath("file.png"));
			folderPixBuf = new Pixbuf(Util.ImagesPath("folder.png"));
		}


		private void iFolderCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifolder.Name;
		}


		private void iFolderCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			((CellRendererPixbuf) cell).Pixbuf = iFolderPixBuf;
		}

		private void NodeNameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DirectoryEntry de = (DirectoryEntry) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = de.Name;
		}

		private void NodeTypeCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DirectoryEntry de = (DirectoryEntry) tree_model.GetValue(iter,0);
			if(de.IsDirectory)
				((CellRendererText) cell).Text = "Folder";
			else
				((CellRendererText) cell).Text = "File";
		}

		private void NodeIDCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DirectoryEntry de = (DirectoryEntry) tree_model.GetValue(iter,0);
			if(de.IsDirectory)
				((CellRendererText) cell).Text = "Folder";
			else
				((CellRendererText) cell).Text = "File";
		}

		private void NodeCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			DirectoryEntry de = (DirectoryEntry) tree_model.GetValue(iter,0);
			if(de.IsDirectory)
				((CellRendererPixbuf) cell).Pixbuf = folderPixBuf;
			else
				((CellRendererPixbuf) cell).Pixbuf = filePixBuf;
		}

		private void RefreshViews()
		{
			iFolderTreeStore.Clear();
			NodeTreeStore.Clear();

			foreach(iFolder ifolder in manager)
			{
				iFolderTreeStore.AppendValues(ifolder);
			}
		}

		private void RefreshNodes()
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

				DirectoryInfo di = new DirectoryInfo(ifolder.LocalPath);
				DirectoryEntry de = new DirectoryEntry(di);

				SetCurrentDir(de);
				topDirEntry = de;
			}
		}

		private void SetCurrentDir(DirectoryEntry de)
		{
			NodeTreeStore.Clear();
			curDirEntry = de;

			CurrentPathEntry.Text = de.FullName;

			try 
			{
				DirectoryInfo[] dirs = de.DirectoryInfo.GetDirectories();
				foreach (DirectoryInfo diNext in dirs) 
				{
					if(diNext.Name[0] != '.')
					{
						DirectoryEntry dEntry = new DirectoryEntry(diNext);
						NodeTreeStore.AppendValues(dEntry);
					}
				}

				FileInfo[] files = de.DirectoryInfo.GetFiles();
				foreach (FileInfo fiNext in files) 
				{
					if(fiNext.Name[0] != '.')
					{
						DirectoryEntry dEntry = new DirectoryEntry(fiNext);
						NodeTreeStore.AppendValues(dEntry);
					}
				}
			} 
			catch (Exception e) 
			{
				Console.WriteLine("The process failed: {0}", 
						e.ToString());
			}
		}

		public void ShowAll()
		{
			if(iFolderWindow != null)
				iFolderWindow.ShowAll();
		}

		private void on_ColWindow_delete_event (object o, DeleteEventArgs args) 
		{
			if(BrowserClosed != null)
			{
				EventArgs e = new EventArgs();
				BrowserClosed(this, e);
			}
		}

		public void on_refreshCollections(object o, EventArgs args)
		{
			RefreshViews();
		}

		public void on_refreshNodes(object o, EventArgs args)
		{
			RefreshViews();
		}

		public void on_iFolder_selection_changed(object o, EventArgs args)
		{
			RefreshNodes();
		}

		public void on_refresh_event(object o, EventArgs args)
		{
			RefreshViews();
		}

		public void on_move_up_event(object o, EventArgs args)
		{
			if(curDirEntry.FullName != topDirEntry.FullName)
			{
				SetCurrentDir(new 
						DirectoryEntry(curDirEntry.DirectoryInfo.Parent) );
			}
		}

		public void on_home_clicked(object o, EventArgs args)
		{
			RefreshNodes();
		}

		public void on_properties_event(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);
				try
				{
					PropertiesDialog pd = new PropertiesDialog();
					pd.iFolder = ifolder;
					pd.TransientFor = iFolderWindow; 
					pd.Run();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine("Unable to Show Properties");
				}
			}
		}

		public void on_newiFolder(object o, EventArgs args)
		{
			// create a file selection dialog and turn off all of the
			// file operations and controlls
			FileSelection fs = new FileSelection ("Choose a folder...");
			fs.FileList.Parent.Hide();
			fs.SelectionEntry.Hide();
			fs.FileopDelFile.Hide();
			fs.FileopRenFile.Hide();

			int rc = fs.Run ();
			fs.Hide();
			if(rc == -5)
			{
				try
				{
					iFolder newiFolder = manager.CreateiFolder(fs.Filename);
					iFolderTreeStore.AppendValues(newiFolder);
				}
				catch(Exception e)
				{
					Console.WriteLine("Failed to create iFolder " + e);
				}
			}
		}

		public void on_deleteiFolder(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);
				try
				{
					ifolder.Delete();
					ifolder.Commit();
					iFolderTreeStore.Remove(ref iter);
					NodeTreeStore.Clear();
				}
				catch(Exception e)
				{
					Console.WriteLine("Unable to delete iFolder");
				}
			}
		}


		private void on_FileTreeView_row_activated(object obj,
				RowActivatedArgs args)
		{
			Console.WriteLine("Hey, we clicked");

			TreeIter iter;

			if(NodeTreeStore.GetIter(out iter, args.Path))
			{
				DirectoryEntry de = 
						(DirectoryEntry) NodeTreeStore.GetValue(iter,0);

				if(de.IsDirectory)
				{
					SetCurrentDir(de);
					Console.WriteLine("Opening directory {0}", de.Name);
				}
				else
					Console.WriteLine("Opening file {0}", de.Name);
			}
		}


	}
}
