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
		iFolderManager				manager;
		Pixbuf						iFolderPixBuf;
		Pixbuf						CollisionPixBuf;
		internal Configuration		config;

		public Gtk.Window TransientFor
		{
			set
			{
				if(ApplicationPropDialog != null)
					ApplicationPropDialog.TransientFor = value;
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

			// Setup the iFolder TreeView
			iFolderTreeStore = new ListStore(typeof(iFolder));
			iFolderTreeView.Model = iFolderTreeStore;

			// Setup Pixbuf and Text Rendering for "iFolders" column
			CellRendererPixbuf ifcrp = new CellRendererPixbuf();
			TreeViewColumn ifolderColumn = new TreeViewColumn();
			ifolderColumn.PackStart(ifcrp, false);
			ifolderColumn.SetCellDataFunc(ifcrp, new TreeCellDataFunc(
						iFolderCellPixbufDataFunc));
			CellRendererText ifcrt = new CellRendererText();
			ifolderColumn.PackStart(ifcrt, false);
			ifolderColumn.SetCellDataFunc(ifcrt, new TreeCellDataFunc(
						iFolderCellTextDataFunc));
			ifolderColumn.Title = "iFolders";
			ifolderColumn.Resizable = true;
			iFolderTreeView.AppendColumn(ifolderColumn);


			// Setup Text Rendering for "Location" column
			CellRendererText locTR = new CellRendererText();
			TreeViewColumn locColumn = new TreeViewColumn();
			locColumn.PackStart(locTR, false);
			locColumn.SetCellDataFunc(locTR, new TreeCellDataFunc(
						iFolderLocationCellTextDataFunc));
			locColumn.Title = "Location";
			locColumn.Resizable = true;
			iFolderTreeView.AppendColumn(locColumn);


			// Setup Text Rendering for "Status" column
			CellRendererText statusTR = new CellRendererText();
			TreeViewColumn statusColumn = new TreeViewColumn();
			statusColumn.PackStart(statusTR, false);
			statusColumn.SetCellDataFunc(statusTR, new TreeCellDataFunc(
						iFolderStatusCellTextDataFunc));
			statusColumn.Title = "Status";
			statusColumn.Resizable = true;
			iFolderTreeView.AppendColumn(statusColumn);


			iFolderTreeView.Selection.Changed += new EventHandler(
						on_iFolder_selection_changed);

			iFolderPixBuf = new Pixbuf(Util.ImagesPath("ifolder.png"));
			CollisionPixBuf = 
				new Pixbuf(Util.ImagesPath("ifolder-collision.png"));
		}

		public int Run()
		{
			InitGlade();
			PopulateWidgets();

			int rc = 0;
			if(ApplicationPropDialog != null)
			{
				while(rc == 0)
				{
					rc = ApplicationPropDialog.Run();
					if(rc == -11) // help
					{
						rc = 0;
						Util.ShowHelp("front.html", null);
					}
				}

				ApplicationPropDialog.Hide();
				ApplicationPropDialog.Destroy();
				ApplicationPropDialog = null;
			}
			return rc;
		}

		private void iFolderLocationCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifolder.LocalPath;
		}

		private void iFolderStatusCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			if(ifolder.HasCollisions())
				((CellRendererText) cell).Text = "Has File Conflicts";
			else
				((CellRendererText) cell).Text = "OK";
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
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			if(ifolder.HasCollisions())
				((CellRendererPixbuf) cell).Pixbuf = CollisionPixBuf;
			else
				((CellRendererPixbuf) cell).Pixbuf = iFolderPixBuf;
		}

		private void PopulateWidgets()
		{
			manager = iFolderManager.Connect();
			on_refreshiFolders(null, null);

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

		public void on_refreshiFolders(object o, EventArgs args)
		{
			iFolderTreeStore.Clear();

			foreach(iFolder ifolder in manager)
			{
				iFolderTreeStore.AppendValues(ifolder);
			}
		}

		public void on_iFolder_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				uint nodeCount = 47;
				ulong bytesToSend = 121823;
	//			TreeModel tModel;
	//			TreeIter iter;

	//			tSelect.GetSelected(out tModel, out iter);
	//			iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

	//			This appears to hang?
	//			SyncSize.CalculateSendSize(	ifolder, 
	//										out nodeCount, 
	//										out bytesToSend);

	//			UploadLabel.Text = bytesToSend.ToString();
	//			SyncFilesLabel.Text = nodeCount.ToString();
				UploadLabel.Text = "N/A";
				SyncFilesLabel.Text = "N/A";
			}
		}

		private void on_StartupCheckButton_toggled(object o, EventArgs args)
		{
//			if(StartupCheckButton.Active == true)
//				Console.WriteLine("Startup == true");
//			else
//				Console.WriteLine("Startup == false");
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
//			if(AutoSyncCheckButton.Active == true)
//				Console.WriteLine("Auto Sync == true");
//			else
//				Console.WriteLine("Auto Sync == false");
		}

		private void on_RefreshSpinButton_changed(object o, EventArgs args)
		{
			manager.DefaultRefreshInterval = (int)RefreshSpinButton.Value;
		}

		[GLib.ConnectBefore]
		public void on_iFolderTreeView_button_press_event(	object obj, 
								ButtonPressEventArgs args)
		{
			switch(args.Event.Button)
			{
				case 1: // first mouse button
					break;
				case 2: // second mouse button
					break;
				case 3: // third mouse button
				{
					Menu ifMenu = new Menu();

					TreePath tPath = null;

					iFolderTreeView.GetPathAtPos((Int32)args.Event.X, 
								(Int32)args.Event.Y, out tPath);

					if(tPath != null)
					{
						iFolder ifolder = null;

						TreeSelection tSelect = iFolderTreeView.Selection;
						tSelect.SelectPath(tPath);
						if(tSelect.CountSelectedRows() == 1)
						{
							TreeModel tModel;
							TreeIter iter;

							tSelect.GetSelected(out tModel, out iter);
							ifolder = (iFolder) tModel.GetValue(iter, 0);
						}

						MenuItem item_open = 
							new MenuItem ("Open");
						ifMenu.Append (item_open);
						item_open.Activated += new EventHandler(
								on_openifolder_context_menu);

						MenuItem item_share = 
							new MenuItem ("Share with...");
						ifMenu.Append (item_share);
						item_share.Activated += new EventHandler(
								on_shareifolder_context_menu);

						MenuItem item_revert = 
								new MenuItem ("Revert to a Normal Folder");
						ifMenu.Append (item_revert);
						item_revert.Activated += new EventHandler(
								on_deleteiFolder);

						ifMenu.Append(new SeparatorMenuItem());

						if(	(ifolder != null) && (ifolder.HasCollisions()) )
						{
							MenuItem item_resolve = 
								new MenuItem ("Resolve Conflicts");
							ifMenu.Append (item_resolve);
							item_resolve.Activated += new EventHandler(
								on_show_conflict_resolver);
						
							ifMenu.Append(new SeparatorMenuItem());
						}

						MenuItem item_properties = 
							new MenuItem ("Properties");
						ifMenu.Append (item_properties);
						item_properties.Activated += 
							new EventHandler( on_properties_event );
					}
					else
					{
						MenuItem item_create = 
							new MenuItem ("Create iFolder");
						ifMenu.Append (item_create);
						item_create.Activated += 
							new EventHandler(on_newiFolder);

						MenuItem item_refresh = 
							new MenuItem ("Refresh List");
						ifMenu.Append (item_refresh);
						item_refresh.Activated += 
							new EventHandler(on_refreshiFolders);
					}
		
					ifMenu.ShowAll();

					ifMenu.Popup(null, null, null, IntPtr.Zero, 3, 
						Gtk.Global.CurrentEventTime);
					break;
				}
			}
		}

		public void on_show_conflict_resolver(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

				ConflictResolver conres = new ConflictResolver();
				conres.iFolder = ifolder;
				conres.TransientFor = ApplicationPropDialog;
				conres.Run();
			}
		}


		public void on_openifolder_context_menu(object o, EventArgs args)
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
					System.Diagnostics.Process process;

					process = new System.Diagnostics.Process();
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.UseShellExecute = false;
					process.StartInfo.FileName = "nautilus";
					process.StartInfo.Arguments = ifolder.LocalPath;
					process.Start();
				}
				catch(Exception e)
				{
					MessageDialog dialog = 
						new MessageDialog(ApplicationPropDialog,
						DialogFlags.Modal | DialogFlags.DestroyWithParent,
						MessageType.Info,
						ButtonsType.Ok,
						"Unable to open a Nautilus window.");
					dialog.Title = "iFolder Message";
					dialog.Run();
					dialog.Hide();
					dialog.Destroy();
				}
			}
		}

		public void on_shareifolder_context_menu(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

				iFolderProperties ifProp = new iFolderProperties();
				ifProp.TransientFor = ApplicationPropDialog;
				ifProp.CurrentiFolder = ifolder;
				ifProp.ActiveTag = 1;
				ifProp.Run();
			}
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
					iFolderProperties ifProps = new iFolderProperties();
					ifProps.CurrentiFolder = ifolder;
					ifProps.TransientFor = ApplicationPropDialog;
					ifProps.Run();
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

					if(IntroDialog.UseDialog())
					{
						IntroDialog iDialog = new IntroDialog();
						iDialog.iFolderPath = fs.Filename;
						iDialog.TransientFor = ApplicationPropDialog;
						iDialog.Run();
					}
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

				MessageDialog dialog = new MessageDialog(ApplicationPropDialog,
					DialogFlags.Modal | DialogFlags.DestroyWithParent,
					MessageType.Question,
					ButtonsType.YesNo,
					"Are you sure you want to revert the selected iFolder to a normal folder?");
				dialog.Title = "Revert iFolder";
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
						ifolder.Delete();
						ifolder.Commit();
						iFolderTreeStore.Remove(ref iter);
					}
					catch(Exception e)
					{
						Console.WriteLine("Unable to delete iFolder");
					}
				}
			}
		}

	}
}
