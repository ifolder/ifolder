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
	using Simias;

	using Gtk;
	using Gdk;
	using Glade;
	using GtkSharp;
	using GLib;

	public class ApplicationProperties 
	{
		[Glade.Widget] internal Gtk.Dialog ApplicationPropDialog = null;
		[Glade.Widget] internal Gtk.TreeView	iFolderTreeView;

		[Glade.Widget] internal Gtk.Label		UploadLabel;
		[Glade.Widget] internal Gtk.Label		SyncFilesLabel;

		[Glade.Widget] internal Gtk.CheckButton	StartupCheckButton;
		[Glade.Widget] internal Gtk.CheckButton DisplayCreationCheckButton;
		[Glade.Widget] internal Gtk.CheckButton AutoSyncCheckButton;
		[Glade.Widget] internal Gtk.SpinButton  RefreshSpinButton;

		private ListStore			iFolderTreeStore;
		int activeTag = 0;
		iFolderManager				manager;
		Pixbuf						iFolderPixBuf;
		internal Configuration		config;

		public Gtk.Window TransientFor
		{
			set
			{
				if(ApplicationPropDialog != null)
					ApplicationPropDialog.TransientFor = value;
			}
		}

		public int ActiveTag
		{
			set
			{
				activeTag = value;
			}
		}

		public Configuration Configuration
		{
			get
			{
				return config;
			}

			set
			{
				config = value;
			}
		}

		public ApplicationProperties() 
		{
		}

		public void InitGlade()
		{
			Glade.XML gxml = new Glade.XML (Util.GladePath(
					"application-properties.glade"), 
					"ApplicationPropDialog", 
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

			iFolderPixBuf = new Pixbuf(Util.ImagesPath("ifolder.png"));
		}

		public int Run()
		{
			InitGlade();
			PopulateWidgets();

			int rc = 0;
			if(ApplicationPropDialog != null)
			{
				rc = ApplicationPropDialog.Run();

				ApplicationPropDialog.Hide();
				ApplicationPropDialog.Destroy();
				ApplicationPropDialog = null;
			}
			return rc;
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

		private void PopulateWidgets()
		{
			iFolderTreeStore.Clear();

			manager = iFolderManager.Connect();

			foreach(iFolder ifolder in manager)
			{
				iFolderTreeStore.AppendValues(ifolder);
			}

			// ----------------------------
			// Display Creation Dialog
			// ----------------------------
			if(config == null)
				config = new Configuration();
			string showWizard = config.Get("iFolderTrayApp", 
					"Show wizard", "true");
			if (showWizard == "true")
				DisplayCreationCheckButton.Active = true;
			else
				DisplayCreationCheckButton.Active = false;


			StartupCheckButton.Active = true;;
			AutoSyncCheckButton.Active = true;
			RefreshSpinButton.Value = manager.DefaultRefreshInterval;
		}

		public void on_iFolder_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);
				Console.WriteLine("Set Values for {0}", ifolder.Name);
			}
		}

		private void on_StartupCheckButton_toggled(object o, EventArgs args)
		{
			if(StartupCheckButton.Active == true)
				Console.WriteLine("Startup == true");
			else
				Console.WriteLine("Startup == false");
		}

		private void on_DisplayCreationCheckButton_toggled(object o, 
				EventArgs args)
		{
			if(config == null)
				config = new Configuration();

			if(DisplayCreationCheckButton.Active == true)
				config.Set("iFolderTrayApp", "Show wizard", "true");
			else
				config.Set("iFolderTrayApp", "Show wizard", "false");
		}

		private void on_AutoSyncCheckButton_toggled(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
				Console.WriteLine("Auto Sync == true");
			else
				Console.WriteLine("Auto Sync == false");
		}

		private void on_RefreshSpinButton_changed(object o, EventArgs args)
		{
			manager.DefaultRefreshInterval = (int)RefreshSpinButton.Value;
		}

	}
}
