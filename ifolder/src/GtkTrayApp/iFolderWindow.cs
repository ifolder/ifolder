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
using Gtk;

namespace Novell.iFolder
{

	/// <summary>
	/// This is the main iFolder Window.  This window implements all of the
	/// client code for iFolder.
	/// </summary>
	public class iFolderWindow : Window
	{
		// for the statusbar
		const int ctx = 1;

		private iFolderWebService	iFolderWS;
		private Gdk.Pixbuf			iFolderPixBuf;
		private Gdk.Pixbuf			CollisionPixBuf;

		private Statusbar			MainStatusBar;
		private Gtk.Notebook		MainNoteBook;
		private Gtk.TreeView		iFolderTreeView;
		private Gtk.ListStore		iFolderTreeStore;

		private ImageMenuItem		CreateMenuItem;
		private Gtk.MenuItem		ShareMenuItem;
		private ImageMenuItem		OpenMenuItem;
		private Gtk.MenuItem		ConflictMenuItem;
		private ImageMenuItem		RevertMenuItem;
		private ImageMenuItem		PropMenuItem;
		private ImageMenuItem		CloseMenuItem;
		private ImageMenuItem		RefreshMenuItem;
		private ImageMenuItem		HelpMenuItem;
		private ImageMenuItem		AboutMenuItem;


		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public iFolderWindow(iFolderWebService ifws) : base ("iFolder")
		{
			if(ifws == null)
				throw new ApplicationException("iFolderWebServices was null");
			iFolderWS = ifws;
			InitializeWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void InitializeWidgets()
		{
			this.SetDefaultSize (200, 400);
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

			// Create the menubar

			AccelGroup accelGroup = new AccelGroup ();
			this.AddAccelGroup (accelGroup);
			
			MenuBar menubar = CreateMenu ();
			vbox.PackStart (menubar, false, false, 0);


			Image iFolderImage = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
			vbox.PackStart (iFolderImage, false, false, 0);


			MainNoteBook = new Notebook();
			MainNoteBook.AppendPage(	CreateiFoldersPage(), 
										new Label("iFolders"));
			vbox.PackStart(MainNoteBook, true, true, 0);


			MainStatusBar = new Statusbar ();
			UpdateStatus("Idle...");

			vbox.PackStart (MainStatusBar, false, false, 0);

			RefreshiFolderTreeView(null, null);

			CreateMenuItem.Sensitive = true;
			ShareMenuItem.Sensitive = false;
			OpenMenuItem.Sensitive = false;
			ConflictMenuItem.Sensitive = false;
			RevertMenuItem.Sensitive = false;
			PropMenuItem.Sensitive = false;;
		}



		/// <summary>
		/// Creates the menubar for the iFolderWindow
		/// </summary>
		/// <returns>
		/// MenuBar for the iFolderWindow
		/// </returns>
		private MenuBar CreateMenu ()
		{
			MenuBar menubar = new MenuBar ();
			AccelGroup agrp = new AccelGroup();
			this.AddAccelGroup(agrp);

			//----------------------------
			// iFolder Menu
			//----------------------------
			Menu iFolderMenu = new Menu();

			CreateMenuItem = new ImageMenuItem ("_Create");
			CreateMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder.png")));
			iFolderMenu.Append(CreateMenuItem);
			CreateMenuItem.AddAccelerator("activate", agrp,
				new AccelKey(Gdk.Key.C, Gdk.ModifierType.ControlMask,
								AccelFlags.Visible));
			CreateMenuItem.Activated += new EventHandler(On_CreateiFolder);

			iFolderMenu.Append(new SeparatorMenuItem());
			OpenMenuItem = new ImageMenuItem ( Stock.Open, agrp );
			iFolderMenu.Append(OpenMenuItem);
//			OpenMenuItem.Activated += new EventHandler(On_CreateiFolder);

			ShareMenuItem = new MenuItem ("Share _with...");
			iFolderMenu.Append(ShareMenuItem);
//			ShareMenuItem.Activated += new EventHandler(On_CreateiFolder);

			ConflictMenuItem = new MenuItem ("Re_solve Conflicts");
			iFolderMenu.Append(ConflictMenuItem);
//			ConflictMenuItem.Activated += new EventHandler(On_CreateiFolder);

			RevertMenuItem = new ImageMenuItem ("Re_vert");
			RevertMenuItem.Image = new Image(Stock.Undo, Gtk.IconSize.Menu);
			iFolderMenu.Append(RevertMenuItem);
//			RevertMenuItem.Activated += new EventHandler(On_CreateiFolder);

			PropMenuItem = new ImageMenuItem (Stock.Properties, agrp);
			iFolderMenu.Append(PropMenuItem);
//			PropMenuItem.Activated += new EventHandler(On_CreateiFolder);

			iFolderMenu.Append(new SeparatorMenuItem());
			CloseMenuItem = new ImageMenuItem (Stock.Close, agrp);
			iFolderMenu.Append(CloseMenuItem);
//			CloseMenuItem.Activated += new EventHandler(On_CreateiFolder);

			MenuItem iFolderMenuItem = new MenuItem("i_Folder");
			iFolderMenuItem.Submenu = iFolderMenu;
			menubar.Append (iFolderMenuItem);


			//----------------------------
			// View Menu
			//----------------------------
			Menu ViewMenu = new Menu();

			RefreshMenuItem = 
				new ImageMenuItem(Stock.Refresh, agrp);
			ViewMenu.Append(RefreshMenuItem);
//			RefreshMenuItem.Activated += new EventHandler(On_CreateiFolder);

			MenuItem ViewMenuItem = new MenuItem("_View");
			ViewMenuItem.Submenu = ViewMenu;
			menubar.Append(ViewMenuItem);


			//----------------------------
			// View Menu
			//----------------------------
			Menu HelpMenu = new Menu();

			HelpMenuItem = 
				new ImageMenuItem(Stock.Help, agrp);
			HelpMenu.Append(HelpMenuItem);
//			HelpMenuItem.Activated += new EventHandler(On_CreateiFolder);

			AboutMenuItem = new ImageMenuItem("A_bout");
			AboutMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder.png")));
			HelpMenu.Append(AboutMenuItem);
//			AboutMenuItem.Activated += new EventHandler(On_CreateiFolder);

			MenuItem MainHelpMenuItem = new MenuItem("_Help");
			MainHelpMenuItem.Submenu = HelpMenu;
			menubar.Append(MainHelpMenuItem);

			return menubar;
		}
	

		/// <summary>
		/// Creates the Main Widget for the iFolderPage
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreateiFoldersPage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = 6;
			vbox.BorderWidth = 6;
			
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			iFolderTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(iFolderTreeView);
			vbox.PackStart(sw, true, true, 0);


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

			iFolderTreeView.ButtonPressEvent += new ButtonPressEventHandler(
						iFolderTreeView_ButtonPressed);

			iFolderTreeView.RowActivated += new RowActivatedHandler(
						OniFolderRowActivated);


			iFolderPixBuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			CollisionPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-collision.png"));



			// Create an HBox that is not homogeneous and spacing of 6
			HBox hbox = new HBox(false, 6);
			// Create another HBox (in case we add more buttons)
			// so they will line up to the right and be the same
			// widgth
			HBox leftHBox = new HBox(true, 6);
			Button add_button = new Button(Gtk.Stock.Add);

			add_button.Clicked += new EventHandler(On_CreateiFolder);
			
			leftHBox.PackEnd(add_button);
			hbox.PackEnd(leftHBox, false, false, 0);
			vbox.PackStart(hbox, false, false, 0);
		
			return vbox;
		}




		private void iFolderLocationCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifolder.UnManagedPath;
		}




		private void iFolderStatusCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
//			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
//			if(ifolder.HasCollisions())
//				((CellRendererText) cell).Text = "Has File Conflicts";
//			else
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
//			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
//			if(ifolder.HasCollisions())
//				((CellRendererPixbuf) cell).Pixbuf = CollisionPixBuf;
//			else
				((CellRendererPixbuf) cell).Pixbuf = iFolderPixBuf;
		}




		public void RefreshiFolderTreeView(object o, EventArgs args)
		{
			iFolderTreeStore.Clear();

			iFolder[] iFolderArray = iFolderWS.GetAlliFolders();

			foreach(iFolder ifolder in iFolderArray)
			{
				iFolderTreeStore.AppendValues(ifolder);
			}
		}



		private void On_CreateiFolder(object o, EventArgs args)
		{
			Console.WriteLine("Create an iFolder here");
			// TODO
		}




		private void WindowDelete (object o, DeleteEventArgs args)
		{
			this.Hide ();
			this.Destroy ();
			args.RetVal = true;
		}




		void UpdateStatus(string message)
		{
			MainStatusBar.Pop (ctx);
			MainStatusBar.Push (ctx, message);
		}




		public void on_iFolder_selection_changed(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				uint nodeCount = 47;
				ulong bytesToSend = 121823;
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

	//			This appears to hang?
	//			SyncSize.CalculateSendSize(	ifolder, 
	//										out nodeCount, 
	//										out bytesToSend);

	//			UploadLabel.Text = bytesToSend.ToString();
	//			SyncFilesLabel.Text = nodeCount.ToString();

/*
				if(	(ifolder != null) && (ifolder.HasCollisions()) )
				{
					ConflictMenuItem.Sensitive = true;
				}
				else
				{
					ConflictMenuItem.Sensitive = false;
				}
*/
					ConflictMenuItem.Sensitive = false;
				ShareMenuItem.Sensitive = true;
				OpenMenuItem.Sensitive = true;
				RevertMenuItem.Sensitive = true;
				PropMenuItem.Sensitive = true;
			}
			else
			{
				ShareMenuItem.Sensitive = false;
				OpenMenuItem.Sensitive = false;
				ConflictMenuItem.Sensitive = false;
				RevertMenuItem.Sensitive = false;
				PropMenuItem.Sensitive = false;
			}
		}




		[GLib.ConnectBefore]
		public void iFolderTreeView_ButtonPressed(	object obj, 
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
								OnOpeniFolderContactMenu);

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

/*
						if(	(ifolder != null) && (ifolder.HasCollisions()) )
						{
							MenuItem item_resolve = 
								new MenuItem ("Resolve Conflicts");
							ifMenu.Append (item_resolve);
							item_resolve.Activated += new EventHandler(
								on_show_conflict_resolver);
						
							ifMenu.Append(new SeparatorMenuItem());
						}
*/

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
							new EventHandler(RefreshiFolderTreeView);
					}
		
					ifMenu.ShowAll();

					ifMenu.Popup(null, null, null, IntPtr.Zero, 3, 
						Gtk.Global.CurrentEventTime);
					break;
				}
			}
		}

		private void OnOpeniFolderContactMenu(object o, EventArgs args)
		{
			OpenSelectediFolder();
		}


		private void OniFolderRowActivated(object o, RowActivatedArgs args)
		{
			OpenSelectediFolder();
		}


		private void OpenSelectediFolder()
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
					process.StartInfo.Arguments = ifolder.UnManagedPath;
					process.Start();
				}
				catch(Exception e)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"iFolder Error",
						"Unable to launch Nautilus",
						"iFolder attempted to open the Nautilus File Manager and was unable to do so");
					dg.Run();
					dg.Hide();
					dg.Destroy();
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
/*
				CollectionProperties colProp = new CollectionProperties();
				colProp.TransientFor = this;
				colProp.Collection = ifolder;
				colProp.ActiveTag = 1;
				colProp.Run();
*/

/*				iFolderProperties ifProp = new iFolderProperties();
				ifProp.TransientFor = this;
				ifProp.CurrentiFolder = ifolder;
				ifProp.ActiveTag = 1;
				ifProp.Run();
*/
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
/*
				try
				{
					CollectionProperties colProp = new CollectionProperties();
					colProp.TransientFor = this;
					colProp.Collection = ifolder;
					colProp.Run();
				}
				catch(Exception e)
				{
					Console.WriteLine(e);
					Console.WriteLine("Unable to Show Properties");
				}
*/
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

				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"iFolder Confirmation",
					"Revert this iFolder?",
					"This will revert this iFolder back to a normal iFolder and leave the files intact.  The iFolder will also be reverted for anyone you have shared it with.");
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
    					iFolderWS.DeleteiFolder(ifolder.ID);
						//ifolder.Delete();
						//ifolder.Commit();
						iFolderTreeStore.Remove(ref iter);
					}
					catch(Exception e)
					{
						iFolderExceptionDialog ied = 
							new iFolderExceptionDialog(
								this,
								e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
					}
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
			fs.TransientFor = this;

			int rc = fs.Run ();
			fs.Hide();
			if(rc == -5)
			{
				try
				{
    				iFolder newiFolder = 
						iFolderWS.CreateLocaliFolder(fs.Filename);
					iFolderTreeStore.AppendValues(newiFolder);

// TODO: determine how to do this!
//					if(IntroDialog.UseDialog())
//					{
						iFolderCreationDialog dlg = 
							new iFolderCreationDialog(newiFolder);
						dlg.TransientFor = this;
						dlg.Run();
						dlg.Hide();
						dlg.Destroy();
						dlg = null;
//					}
				}
				catch(Exception e)
				{
					iFolderExceptionDialog ied = 
						new iFolderExceptionDialog(
							this,
							e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
				}
			}
		}



	}
}
