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
using System.Collections;
using Gtk;
using Simias.Client.Event;

namespace Novell.iFolder
{
	/// <summary>
	/// This is a holder class for iFolders so the client can place
	/// extra data with an iFolder about it's status and such.
	/// </summary>
	public class iFolderHolder
	{
		private iFolderWeb	ifolder;
		private bool		isSyncing;
		private bool		syncSuccessful;
		private string		path;
		private string		state;

		public iFolderHolder(iFolderWeb ifolder)
		{
			this.ifolder = ifolder;
			isSyncing = false;
			syncSuccessful = true;
			UpdateDisplayData();
		}

		public iFolderWeb iFolder
		{
			get{ return ifolder; }

			set
			{ 
				this.ifolder = value; 
				UpdateDisplayData();
			}
		}

		public bool IsSyncing
		{
			get{ return isSyncing; }
			set
			{ 
				this.isSyncing = value; 
				UpdateDisplayData();
			}
		}

		public bool SyncSuccessful
		{
			get{ return syncSuccessful; }
			set
			{
				this.syncSuccessful = value;
				UpdateDisplayData();
			}
		}

		public string Path
		{
			get{ return path; }
		}

		public string State
		{
			get{ return state; }
		}

		private void UpdateDisplayData()
		{
			if(iFolder.IsSubscription)
			{
				if(iFolder.State == "Available")
					state = Util.GS("Available");
				else if(iFolder.State == "WaitConnect")
					state = Util.GS("Waiting to Connect");
				else if(iFolder.State == "WaitSync")
					state = Util.GS("Waiting to Sync");
				else
					state = Util.GS("Unknown");
			}
			else
			{
				if(IsSyncing)
					state = Util.GS("Synchronizing");
				else if(iFolder.State == "WaitSync")
					state = Util.GS("Waiting to Sync");
				else if(iFolder.State == "Local")
				{
					if(iFolder.HasConflicts)
						state = Util.GS("Has File Conflicts");
					else if(!SyncSuccessful)
						state = Util.GS("Sync Failed");
					else
						state = Util.GS("OK");
				}
				else
					state = Util.GS("Unknown");
			}


			if(iFolder.IsSubscription)
			{
				if(iFolder.State == "Available")
					path = iFolder.Owner;
			}
			else
			{
				path = iFolder.UnManagedPath;
			}

		}
	}



	/// <summary>
	/// This is the main iFolder Window.  This window implements all of the
	/// client code for iFolder.
	/// </summary>
	public class iFolderWindow : Window
	{
		// for the statusbar
		const int ctx = 1;
		private iFolderWebService	ifws;
		private iFolderData			ifdata;
		private Gdk.Pixbuf			iFolderPixBuf;
		private Gdk.Pixbuf			ServeriFolderPixBuf;
		private Gdk.Pixbuf			ConflictPixBuf;

		private Statusbar			MainStatusBar;
		private ProgressBar			SyncBar;
		private Toolbar				toolbar;
		private Gtk.TreeView		iFolderTreeView;
		private Gtk.ListStore		iFolderTreeStore;

		private Gtk.Widget			NewButton;
		private Gtk.Widget			SetupButton;
		private Gtk.Widget			SyncButton;
		private Gtk.Widget			ShareButton;
		private Gtk.Widget			ConflictButton;


		private ImageMenuItem		NewMenuItem;
		private Gtk.MenuItem		ShareMenuItem;
		private ImageMenuItem		OpenMenuItem;
		private Gtk.MenuItem		ConflictMenuItem;
		private Gtk.MenuItem		SyncNowMenuItem;
		private ImageMenuItem		RevertMenuItem;
		private ImageMenuItem		DeleteMenuItem;
		private ImageMenuItem		RemoveMenuItem;
		private Gtk.MenuItem		SetupMenuItem;
		private ImageMenuItem		PropMenuItem;
		private ImageMenuItem		CloseMenuItem;
		private ImageMenuItem		RefreshMenuItem;
		private ImageMenuItem		HelpMenuItem;
		private ImageMenuItem		AboutMenuItem;

		private iFolderConflictDialog ConflictDialog;
		private iFolderPropertiesDialog PropertiesDialog;

		private Hashtable			curiFolders;
		private Hashtable			acceptediFolders;


		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public iFolderWindow(iFolderWebService webService)
			: base (Util.GS("iFolders"))
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;
			ifdata = iFolderData.GetData();
			curiFolders = new Hashtable();
			acceptediFolders = new Hashtable();
			CreateWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (600, 480);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

			//-----------------------------
			// Create the menubar
			//-----------------------------
			MenuBar menubar = CreateMenu ();
			vbox.PackStart (menubar, false, false, 0);

			//-----------------------------
			// Create the Toolbar
			//-----------------------------
			toolbar = CreateToolbar();
			vbox.PackStart (toolbar, false, false, 0);


			//-----------------------------
			// Create the Tree View
			//-----------------------------
			vbox.PackStart(SetupTreeView(), true, true, 0);


			//-----------------------------
			// Create Status Bar
			//-----------------------------
			MainStatusBar = new Statusbar ();
			UpdateStatus(Util.GS("Idle..."));

			vbox.PackStart (MainStatusBar, false, false, 0);

			//-----------------------------
			// Set Menu Status
			//-----------------------------
			NewMenuItem.Sensitive = true;
			SetupMenuItem.Sensitive = false;
			DeleteMenuItem.Sensitive = false;
			RemoveMenuItem.Sensitive = false;
			RemoveMenuItem.Visible = false;
			ShareMenuItem.Sensitive = false;
			OpenMenuItem.Sensitive = false;
			SyncNowMenuItem.Sensitive = false;
			ConflictMenuItem.Sensitive = false;
			RevertMenuItem.Sensitive = false;
			PropMenuItem.Sensitive = false;;

			NewButton.Sensitive = true;
			SetupButton.Sensitive = false;
			SyncButton.Sensitive = false;
			ShareButton.Sensitive = false;
			ConflictButton.Sensitive = false;

			// Setup an event to refresh when the window is
			// being drawn
			this.Realized += new EventHandler(OnRealizeWidget);
		}




		/// <summary>
		/// Creates the Toolbar for the iFolder Window
		/// </summary>
		/// <returns>
		/// Toolbar for the window
		/// </returns>
		private Toolbar CreateToolbar()
		{
			Toolbar tb = new Toolbar();

			NewButton = tb.AppendItem(Util.GS("New"), 
				Util.GS("Create a New iFolder"), "Toolbar/New iFolder",
				new Image(new Gdk.Pixbuf(Util.ImagesPath("newifolder24.png"))),
				new SignalFunc(CreateNewiFolder));

			SetupButton = tb.AppendItem(Util.GS("Setup"),
				Util.GS("Setup an Existing iFolder"), "Toolbar/Setup iFolder",
				new Image(new Gdk.Pixbuf(Util.ImagesPath("setup24.png"))),
				new SignalFunc(SetupiFolder));

			tb.AppendSpace ();

			SyncButton = tb.AppendItem(Util.GS("Sync"),
				Util.GS("Synchronize an iFolder"), "Toolbar/Sync iFolder",
				new Image(new Gdk.Pixbuf(Util.ImagesPath("sync24.png"))),
				new SignalFunc(SynciFolder));

			ShareButton = tb.AppendItem(Util.GS("Share"),
				Util.GS("Share an iFolder"), "Toolbar/Share iFolder",
				new Image(new Gdk.Pixbuf(Util.ImagesPath("share24.png"))),
				new SignalFunc(ShareiFolder));

			ConflictButton = tb.AppendItem(Util.GS("Resolve"),
				Util.GS("Resolve File Conflicts"), "Toolbar/Resolve iFolder",
				new Image(new Gdk.Pixbuf(Util.ImagesPath("conflict24.png"))),
				new SignalFunc(ResolveConflicts));

//			tb.AppendSpace ();

			return tb;
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

			NewMenuItem = new ImageMenuItem (Util.GS("C_reate"));
			NewMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder.png")));
			iFolderMenu.Append(NewMenuItem);
			NewMenuItem.AddAccelerator("activate", agrp,
				new AccelKey(Gdk.Key.N, Gdk.ModifierType.ControlMask,
								AccelFlags.Visible));
			NewMenuItem.Activated += new EventHandler(NewiFolderHandler);

			SetupMenuItem =
				new MenuItem (Util.GS("_Setup iFolder"));
			iFolderMenu.Append(SetupMenuItem);
			SetupMenuItem.Activated += new EventHandler(SetupiFolderHandler);

			DeleteMenuItem =
				new ImageMenuItem (Util.GS("_Delete iFolder"));
			DeleteMenuItem.Image = new Image(Stock.Delete, Gtk.IconSize.Menu);
			iFolderMenu.Append(DeleteMenuItem);
			DeleteMenuItem.Activated += new EventHandler(OnRemoveiFolder);

			RemoveMenuItem =
				new ImageMenuItem (Util.GS("Re_move iFolder"));
			RemoveMenuItem.Image = new Image(Stock.Delete, Gtk.IconSize.Menu);
			iFolderMenu.Append(RemoveMenuItem);
			RemoveMenuItem.Activated += new EventHandler(OnRemoveiFolder);

			iFolderMenu.Append(new SeparatorMenuItem());
			OpenMenuItem = new ImageMenuItem ( Stock.Open, agrp );
			iFolderMenu.Append(OpenMenuItem);
			OpenMenuItem.Activated += new EventHandler(OnOpeniFolderMenu);

			ShareMenuItem = new MenuItem (Util.GS("Share _with..."));
			iFolderMenu.Append(ShareMenuItem);
			ShareMenuItem.Activated += new EventHandler(ShareiFolderHandler);

			ConflictMenuItem = new MenuItem (Util.GS("Re_solve conflicts"));
			iFolderMenu.Append(ConflictMenuItem);
			ConflictMenuItem.Activated += 
					new EventHandler(ResolveConflictHandler);

			SyncNowMenuItem = new MenuItem(Util.GS("Sync _now"));
			iFolderMenu.Append(SyncNowMenuItem);
			SyncNowMenuItem.Activated += new EventHandler(SynciFolderHandler);

			RevertMenuItem = 
				new ImageMenuItem (Util.GS("Re_vert to a normal folder"));
			RevertMenuItem.Image = new Image(Stock.Undo, Gtk.IconSize.Menu);
			iFolderMenu.Append(RevertMenuItem);
			RevertMenuItem.Activated += new EventHandler(OnRevertiFolder);

			PropMenuItem = new ImageMenuItem (Stock.Properties, agrp);
			iFolderMenu.Append(PropMenuItem);
			PropMenuItem.Activated += new EventHandler( OnShowProperties );

			iFolderMenu.Append(new SeparatorMenuItem());
			CloseMenuItem = new ImageMenuItem (Stock.Close, agrp);
			iFolderMenu.Append(CloseMenuItem);
			CloseMenuItem.Activated += new EventHandler(CloseEventHandler);

			MenuItem iFolderMenuItem = new MenuItem(Util.GS("i_Folder"));
			iFolderMenuItem.Submenu = iFolderMenu;
			menubar.Append (iFolderMenuItem);


			//----------------------------
			// View Menu
			//----------------------------
			Menu ViewMenu = new Menu();

			RefreshMenuItem = 
				new ImageMenuItem(Stock.Refresh, agrp);
			ViewMenu.Append(RefreshMenuItem);
			RefreshMenuItem.Activated += 
					new EventHandler(RefreshiFoldersHandler);

			MenuItem ViewMenuItem = new MenuItem(Util.GS("_View"));
			ViewMenuItem.Submenu = ViewMenu;
			menubar.Append(ViewMenuItem);


			//----------------------------
			// Help Menu
			//----------------------------
			Menu HelpMenu = new Menu();

			HelpMenuItem = 
				new ImageMenuItem(Stock.Help, agrp);
			HelpMenu.Append(HelpMenuItem);
			HelpMenuItem.Activated += new EventHandler(OnHelpMenuItem);

			AboutMenuItem = new ImageMenuItem(Util.GS("A_bout"));
			AboutMenuItem.Image = new Image(Gnome.Stock.About, 
							Gtk.IconSize.Menu);
//			AboutMenuItem.Image = new Image(
//					new Gdk.Pixbuf(Util.ImagesPath("ifolder.png")));
			HelpMenu.Append(AboutMenuItem);
			AboutMenuItem.Activated += new EventHandler(OnAbout);

			MenuItem MainHelpMenuItem = new MenuItem(Util.GS("_Help"));
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
		private Widget SetupTreeView()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
//			vbox.Spacing = 10;
//			vbox.BorderWidth = Util.DefaultBorderWidth;
			
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			iFolderTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(iFolderTreeView);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			vbox.PackStart(sw, true, true, 0);


			// Setup the iFolder TreeView
			iFolderTreeStore = new ListStore(typeof(iFolderHolder));
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
			ifolderColumn.Title = Util.GS("Name");
			ifolderColumn.Resizable = true;
			iFolderTreeView.AppendColumn(ifolderColumn);


			// Setup Text Rendering for "Location" column
			CellRendererText locTR = new CellRendererText();
			locTR.Xpad = 10;
			TreeViewColumn locColumn = new TreeViewColumn();
			locColumn.PackStart(locTR, false);
			locColumn.SetCellDataFunc(locTR, new TreeCellDataFunc(
						iFolderLocationCellTextDataFunc));
			locColumn.Title = Util.GS("Location");
			locColumn.Resizable = true;
			locColumn.MinWidth = 250;
			iFolderTreeView.AppendColumn(locColumn);


			// Setup Text Rendering for "Status" column
			CellRendererText statusTR = new CellRendererText();
			statusTR.Xpad = 10;
			TreeViewColumn statusColumn = new TreeViewColumn();
			statusColumn.PackStart(statusTR, false);
			statusColumn.SetCellDataFunc(statusTR, new TreeCellDataFunc(
						iFolderStatusCellTextDataFunc));
			statusColumn.Title = Util.GS("Status");
			statusColumn.Resizable = false;
			iFolderTreeView.AppendColumn(statusColumn);




			iFolderTreeView.Selection.Changed += new EventHandler(
						OniFolderSelectionChanged);

			iFolderTreeView.ButtonPressEvent += new ButtonPressEventHandler(
						iFolderTreeViewButtonPressed);

			iFolderTreeView.RowActivated += new RowActivatedHandler(
						OniFolderRowActivated);


			ServeriFolderPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("serverifolder.png"));
			iFolderPixBuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			ConflictPixBuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-collision.png"));
		
			return vbox;
		}
	



		private void OnRealizeWidget(object o, EventArgs args)
		{
			iFolderTreeView.HasFocus = true;
			RefreshiFolders(false);
		}




		private void iFolderLocationCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder = 
						(iFolderHolder) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifHolder.Path;
		}




		private void iFolderStatusCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder =
					(iFolderHolder) tree_model.GetValue(iter,0);

			((CellRendererText) cell).Text = ifHolder.State;
		}




		private void iFolderCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder = (iFolderHolder)tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifHolder.iFolder.Name;
		}




		private void iFolderCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder = 
					(iFolderHolder) tree_model.GetValue(iter,0);

			if(ifHolder.iFolder.IsSubscription)
				((CellRendererPixbuf) cell).Pixbuf = ServeriFolderPixBuf;
			else
			{
				if(ifHolder.iFolder.HasConflicts)
					((CellRendererPixbuf) cell).Pixbuf = ConflictPixBuf;
				else
					((CellRendererPixbuf) cell).Pixbuf = iFolderPixBuf;
			}
		}


		private void RefreshiFoldersHandler(object o, EventArgs args)
		{
			RefreshiFolders(true);
		}


		public void RefreshiFolders(bool readFromSimias)
		{
			curiFolders.Clear();
			iFolderTreeStore.Clear();
			acceptediFolders.Clear();

			if(readFromSimias)
				ifdata.RefreshData();

			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					TreeIter iter = iFolderTreeStore.AppendValues(holder);
					curiFolders.Add(holder.iFolder.ID, iter);
				}
			}
		}




		private void CloseEventHandler(object o, EventArgs args)
		{
			CloseWindow();
		}



		private void CloseWindow()
		{
			this.Hide();
			this.Destroy();
		}



		void UpdateStatus(string message)
		{
			MainStatusBar.Pop (ctx);
			MainStatusBar.Push (ctx, message);
		}




		public void OniFolderSelectionChanged(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);

				if(	(ifHolder.iFolder != null) && 
									(ifHolder.iFolder.HasConflicts) )
				{
					ConflictMenuItem.Sensitive = true;
					ConflictButton.Sensitive = true;
				}
				else
				{
					ConflictMenuItem.Sensitive = false;
					ConflictButton.Sensitive = false;
				}

				if(!ifHolder.iFolder.IsSubscription)
				{
					SetupMenuItem.Sensitive = false;
					ShareMenuItem.Sensitive = true;
					OpenMenuItem.Sensitive = true;
					SyncNowMenuItem.Sensitive = true;
					RevertMenuItem.Sensitive = true;
					PropMenuItem.Sensitive = true;

					SetupButton.Sensitive = false;
					SyncButton.Sensitive = true;
					ShareButton.Sensitive = true;
				}
				else
				{
					SetupMenuItem.Sensitive = true;
					ShareMenuItem.Sensitive = false;
					OpenMenuItem.Sensitive = false;
					SyncNowMenuItem.Sensitive = false;
					RevertMenuItem.Sensitive = false;
					PropMenuItem.Sensitive = false;

					SetupButton.Sensitive = true;
					SyncButton.Sensitive = false;
					ShareButton.Sensitive = false;
				}

				if(ifHolder.iFolder.OwnerID == 
						ifHolder.iFolder.CurrentUserID)
				{
					DeleteMenuItem.Sensitive = true;
					DeleteMenuItem.Visible = true;
					RemoveMenuItem.Sensitive = false;
					RemoveMenuItem.Visible = false;
				}
				else
				{
					DeleteMenuItem.Sensitive = false;
					DeleteMenuItem.Visible = false;
					RemoveMenuItem.Sensitive = true;
					RemoveMenuItem.Visible = true;
				}
			}
			else
			{
				ShareMenuItem.Sensitive = false;
				OpenMenuItem.Sensitive = false;
				SyncNowMenuItem.Sensitive = false;
				ConflictMenuItem.Sensitive = false;
				RevertMenuItem.Sensitive = false;
				DeleteMenuItem.Sensitive = false;
//				DeleteMenuItem.Visible = false;
				RemoveMenuItem.Sensitive = false;
				RemoveMenuItem.Visible = false;
				PropMenuItem.Sensitive = false;
				SetupMenuItem.Sensitive = false;

				SetupButton.Sensitive = false;
				SyncButton.Sensitive = false;
				ShareButton.Sensitive = false;
				ConflictButton.Sensitive = false;
			}
		}




		[GLib.ConnectBefore]
		public void iFolderTreeViewButtonPressed(	object obj, 
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
						iFolderHolder ifHolder = null;

						TreeSelection tSelect = iFolderTreeView.Selection;
						tSelect.SelectPath(tPath);
						if(tSelect.CountSelectedRows() == 1)
						{
							TreeModel tModel;
							TreeIter iter;

							tSelect.GetSelected(out tModel, out iter);
							ifHolder = (iFolderHolder) tModel.GetValue(iter, 0);

							if(ifHolder.iFolder.IsSubscription == false)
							{
								MenuItem item_open = 
									new MenuItem (Util.GS("Open"));
								ifMenu.Append (item_open);
								item_open.Activated += new EventHandler(
										OnOpeniFolderMenu);

								ifMenu.Append(new SeparatorMenuItem());

								MenuItem item_share = 
									new MenuItem (Util.GS("Share with..."));
								ifMenu.Append (item_share);
								item_share.Activated += new EventHandler(
										ShareiFolderHandler);

								if(ifHolder.iFolder.HasConflicts)
								{
									MenuItem item_resolve = new MenuItem (
											Util.GS("Resolve conflicts"));
									ifMenu.Append (item_resolve);
									item_resolve.Activated += new EventHandler(
										ResolveConflictHandler);
							
									ifMenu.Append(new SeparatorMenuItem());
								}

								MenuItem item_sync =
									new MenuItem(Util.GS("Sync now"));
								ifMenu.Append (item_sync);
								item_sync.Activated += new EventHandler(
										SynciFolderHandler);

								MenuItem item_revert = new MenuItem (
										Util.GS("Revert to a normal folder"));
								ifMenu.Append (item_revert);
								item_revert.Activated += new EventHandler(
										OnRevertiFolder);

								if(ifHolder.iFolder.OwnerID == 
												ifHolder.iFolder.CurrentUserID)
								{
									MenuItem item_delete = new MenuItem (
											Util.GS("Delete iFolder"));
									ifMenu.Append (item_delete);
									item_delete.Activated += new EventHandler(
											OnRemoveiFolder);
								}
								else
								{
									MenuItem item_delete = new MenuItem (
											Util.GS("Remove iFolder"));
									ifMenu.Append (item_delete);
									item_delete.Activated += new EventHandler(
											OnRemoveiFolder);
								}


								ifMenu.Append(new SeparatorMenuItem());
	
								MenuItem item_properties = 
									new MenuItem (Util.GS("Properties"));
								ifMenu.Append (item_properties);
								item_properties.Activated += 
									new EventHandler( OnShowProperties );
							}
							else if(ifHolder.iFolder.State == "Available")
							{
								MenuItem item_accept = 
									new MenuItem (Util.GS("Setup iFolder"));
								ifMenu.Append (item_accept);
								item_accept.Activated += new EventHandler(
										SetupiFolderHandler);

								if(ifHolder.iFolder.OwnerID == 
												ifHolder.iFolder.CurrentUserID)
								{
									MenuItem item_decline = 
										new MenuItem(Util.GS("Delete iFolder"));
									ifMenu.Append (item_decline);
									item_decline.Activated += new EventHandler(
											OnRemoveiFolder);
								}
								else
								{
									MenuItem item_decline = 
									new MenuItem (Util.GS("Remove iFolder"));
									ifMenu.Append (item_decline);
									item_decline.Activated += new EventHandler(
											OnRemoveiFolder);
								}
							}
							else
							{
								MenuItem item_decline = 
									new MenuItem (Util.GS("Remove iFolder"));
								ifMenu.Append (item_decline);
								item_decline.Activated += new EventHandler(
										OnRemoveiFolder);
							}
						}
					}
					else
					{
						MenuItem item_create = 
							new MenuItem (Util.GS("Create iFolder"));
						ifMenu.Append (item_create);
						item_create.Activated += 
							new EventHandler(NewiFolderHandler);

						MenuItem item_refresh = 
							new MenuItem (Util.GS("Refresh list"));
						ifMenu.Append (item_refresh);
						item_refresh.Activated += 
							new EventHandler(RefreshiFoldersHandler);
					}
		
					ifMenu.ShowAll();

					ifMenu.Popup(null, null, null, IntPtr.Zero, 3, 
						Gtk.Global.CurrentEventTime);
					break;
				}
			}
		}

		private void OnOpeniFolderMenu(object o, EventArgs args)
		{
			OpenSelectediFolder();
		}


		private void OniFolderRowActivated(object o, RowActivatedArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);
				if(ifHolder.iFolder.IsSubscription)
				{
					if(ifHolder.iFolder.State == "Available")
						SetupiFolderHandler(o, args);
				}
				else
				{
					OpenSelectediFolder();
				}
			}
		}


		private void OpenSelectediFolder()
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);

				try
				{
					Util.OpenInBrowser(ifHolder.iFolder.UnManagedPath);
				}
				catch(Exception e)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						Util.GS("iFolder Error"),
						Util.GS("Unable to launch File Browser"),
						Util.GS("iFolder attempted to open the Nautilus File Manager and the Konqueror File Manager and was unable to launch either of them."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
			}
		}




		public void ShareiFolderHandler(object o, EventArgs args)
		{
			ShareiFolder();
		}



		private void ShareiFolder()
		{
			ShowProperties(1);
		}




		public void OnShowProperties(object o, EventArgs args)
		{
			ShowProperties(0);
		}




		private void ShowProperties(int currentPage)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
							(iFolderHolder) tModel.GetValue(iter, 0);

				// build an array of all current iFolders and
				// hand it to the Properties Dialog
				iFolderWeb[] ifList;
				ArrayList list = new ArrayList();

				if(iFolderTreeStore.GetIterFirst(out iter))
				{
					do
					{
						iFolderHolder tmpHldr = (iFolderHolder) 
									iFolderTreeStore.GetValue(iter,0);
						if(!tmpHldr.iFolder.IsSubscription)
							list.Add(tmpHldr.iFolder);
					}
					while(iFolderTreeStore.IterNext(ref iter));
				}

				ifList = (iFolderWeb[])(list.ToArray(typeof(iFolderWeb)));

				try
				{
					PropertiesDialog = 
						new iFolderPropertiesDialog(this, 
									ifHolder.iFolder, 
									ifList,
									ifws);
					PropertiesDialog.Response += 
							new ResponseHandler(OnPropertiesDialogResponse);
					PropertiesDialog.CurrentPage = currentPage;
					PropertiesDialog.ShowAll();
				}
				catch(Exception e)
				{
					if(PropertiesDialog != null)
					{
						PropertiesDialog.Hide();
						PropertiesDialog.Destroy();
						PropertiesDialog = null;
					}

					iFolderExceptionDialog ied = 
						new iFolderExceptionDialog(this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					ied = null;
				}
			}
		}


		private void OnPropertiesDialogResponse(object o, ResponseArgs args)
		{
			switch(args.ResponseId)
			{
				case Gtk.ResponseType.Help:
					Util.ShowHelp("front.html", this);
					break;
				default:
				{
					if(PropertiesDialog != null)
					{
						PropertiesDialog.Hide();
						PropertiesDialog.Destroy();
						PropertiesDialog = null;
					}
					break;
				}
			}
		}



		public void	SynciFolderHandler(object o, EventArgs args)
		{
			SynciFolder();
		}




		public void OnRevertiFolder(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);

				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					Util.GS("iFolder Confirmation"),
					Util.GS("Revert this iFolder?"),
					Util.GS("This will revert this iFolder back to a normal folder and leave the files intact.  The iFolder will then be available from the server and will need to be setup in a different location in order to sync."));
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
    					iFolderWeb newiFolder = 
								ifws.RevertiFolder(ifHolder.iFolder.ID);
						curiFolders.Remove(ifHolder.iFolder.ID);

						// Set the value of the returned value for the one
						// that was there
						iFolderTreeStore.SetValue(iter, 0, 
								new iFolderHolder(newiFolder));
						curiFolders.Add(newiFolder.ID, iter);
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




		public void NewiFolderHandler(object o, EventArgs args)
		{
			CreateNewiFolder();
		}




		private void SetupiFolderHandler(object o, EventArgs args)
		{
			SetupiFolder();
		}


		private bool ShowBadiFolderPath(string path, string name)
		{
			try
			{
				bool isGood = true;
				if(name != null)
				{
					isGood = !ifws.IsPathIniFolder(path);
					if(isGood)
					{
						// now we need to check if there is already an
						// ifolder at that path
						isGood = !ifws.IsPathIniFolder(
								System.IO.Path.Combine(path, name));
					}
				}
				else
				{
					isGood = ifws.CanBeiFolder(path);
				}

				if(!isGood)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Info,
						iFolderMsgDialog.ButtonSet.Ok,
						Util.GS("Invalid iFolder Path"),
						Util.GS("Invalid iFolder path selected"),
						Util.GS("The path you have selected is invalid for an iFolder.  iFolders cannot contain other iFolders.  The folder you selected is either inside an iFolder, is an iFolder, or already contains an iFolder by the same name.  Please select an alternate folder."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
					return true;
				}
			}
			catch(Exception e)
			{
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
														this, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
				return true;
			}
			return false;
		}




		private void OnRemoveiFolder(object o, EventArgs args)
		{
			iFolderHolder ifHolder = null;
			TreeModel tModel;
			TreeIter iter;

			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				tSelect.GetSelected(out tModel, out iter);
				ifHolder = (iFolderHolder) tModel.GetValue(iter, 0);
				if(ifHolder.iFolder == null)
					return;
				int rc = 0;

				rc = AskRemoveiFolder(ifHolder);

				// User pressed OK?
				if(rc != -8)
					return;

				iFolderWeb remiFolder = ifHolder.iFolder;

				// Check if this is a subscription
				// if it is not, the revert the ifolder first
				if(!ifHolder.iFolder.IsSubscription)
				{
					try
					{
    					remiFolder = 
								ifws.RevertiFolder(ifHolder.iFolder.ID);
						curiFolders.Remove(ifHolder.iFolder.ID);

						// Set the value of the returned value for the one
						// that was there
						iFolderTreeStore.SetValue(iter, 0, 
								new iFolderHolder(remiFolder));

						curiFolders.Add(remiFolder.ID, iter);
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
						return;
					}
				}


				try
				{
					// remove the current iFolder so events don't replace it
					curiFolders.Remove(remiFolder.ID);
   		 			ifws.DeclineiFolderInvitation(remiFolder.DomainID, remiFolder.ID);
					// if no exception, remove it from the list
					iFolderTreeStore.Remove(ref iter);
				}
				catch(Exception e)
				{
					// if we threw an exceptoin, add the old ifolder back
					curiFolders.Add(remiFolder.ID, iter);

					iFolderExceptionDialog ied = new iFolderExceptionDialog(
														this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					return;
				}
			}
		}




		private int AskRemoveiFolder(iFolderHolder ifHolder)
		{
			int rc = 0;

			if(ifHolder.iFolder.OwnerID == ifHolder.iFolder.CurrentUserID)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					Util.GS("Remove iFolder Confirmation"),
					string.Format(Util.GS("Remove iFolder {0}?"),
											ifHolder.iFolder.Name),
					Util.GS("This will remove this iFolder from your local machine.  Because you are the owner of this iFolder, the iFolder will also be removed from the iFolder server and all users you have shared with.  The iFolder cannot be recovered or re-shared on another machine.  The files will not be deleted from your local hard drive."));
				rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
			}
			else
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					Util.GS("Remove iFolder Confirmation"),
					string.Format(Util.GS("Remove iFolder {0}?"),
											ifHolder.iFolder.Name),
					Util.GS("This will remove you as a member of this iFolder.  You will not be able to access this iFolder unless the owner re-invites you to this iFolder.  The files will not be deleted from your local hard drive."));
				rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
			}
			return rc;
		}



		private void ResolveConflicts()
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);
			
				
				ConflictDialog = new iFolderConflictDialog(
										this,
										ifHolder.iFolder,
										ifws);
				ConflictDialog.Response += 
							new ResponseHandler(OnConflictDialogResponse);
				ConflictDialog.ShowAll();
			}
		}




		private void ResolveConflictHandler(object o, EventArgs args)
		{
			ResolveConflicts();
		}



		private void OnConflictDialogResponse(object o, ResponseArgs args)
		{
			if(ConflictDialog != null)
			{
				ConflictDialog.Hide();
				ConflictDialog.Destroy();
				ConflictDialog = null;
			}
			// CRG: TODO
			// At this point, refresh the selected iFolder to see if it
			// has any more conflicts
		}




		// update the data value in the iFolderTreeStore so the ifolder
		// will switch to one that has conflicts
		public void iFolderHasConflicts(string iFolderID)
		{
			if(curiFolders.ContainsKey(iFolderID))
			{
				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID, false);

				TreeIter iter = (TreeIter)curiFolders[iFolderID];

				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}

			// TODO: let any property dialogs know that this iFolder
			// has a conflict
		}



		public void iFolderChanged(string iFolderID)
		{
			if(curiFolders.ContainsKey(iFolderID))
			{
				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID, false);

				TreeIter iter = (TreeIter)curiFolders[iFolderID];

				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}
		}



		public void iFolderDeleted(string iFolderID)
		{
			if(curiFolders.ContainsKey(iFolderID))
			{
				TreeIter iter = (TreeIter)curiFolders[iFolderID];
				iFolderTreeStore.Remove(ref iter);
				curiFolders.Remove(iFolderID);
			}
		}



		public void iFolderCreated(string iFolderID)
		{
			if(!curiFolders.ContainsKey(iFolderID))
			{
				TreeIter iter;
				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID, false);

				if(acceptediFolders.ContainsKey(iFolderID))
				{
					iter = (TreeIter) acceptediFolders[iFolderID];

					iFolderTreeStore.SetValue(iter, 0, ifHolder);

					acceptediFolders.Remove(iFolderID);
				}
				else
				{
					iter = iFolderTreeStore.AppendValues(ifHolder);
				}

				curiFolders[iFolderID] = iter;
			}
		}



		public void HandleSyncEvent(CollectionSyncEventArgs args)
		{
			switch(args.Action)
			{
				case Action.StartSync:
				{
					UpdateStatus(string.Format(Util.GS(
									"Syncing: {0}"), args.Name));

					if(curiFolders.ContainsKey(args.ID))
					{
						TreeIter iter = (TreeIter)curiFolders[args.ID];
						iFolderHolder ifHolder = 
							(iFolderHolder) iFolderTreeStore.GetValue(iter,0);
						ifHolder.IsSyncing = true;

						// This is kind of a hack
						// Sometimes, iFolders will be in the list but
						// they don't have the path.  Check for the path
						// here and if it is missing, re-read the ifolder
						// 'cause we'll have the path at this point
						if( (ifHolder.iFolder.UnManagedPath == null) ||
								(ifHolder.iFolder.UnManagedPath.Length == 0) )
						{
							iFolderWeb updatediFolder;

							try
							{
								updatediFolder = ifws.GetiFolder(
									args.ID);
							}
							catch(Exception e)
							{
								updatediFolder = null;
							}
							if(updatediFolder != null)
								ifHolder.iFolder = updatediFolder;
						}
						iFolderTreeStore.SetValue(iter, 0, ifHolder);
					}
					break;
				}
				case Action.StopSync:
				{
					if(SyncBar != null)
						SyncBar.Hide();

					if(curiFolders.ContainsKey(args.ID))
					{
						TreeIter iter = (TreeIter)curiFolders[args.ID];
						iFolderHolder ifHolder = (iFolderHolder) 
								iFolderTreeStore.GetValue(iter,0);
						ifHolder.IsSyncing = false;
						ifHolder.SyncSuccessful = args.Successful;

						// This is kind of a hack
						// Sometimes, iFolders will come through that
						// don't have members so we need to update them
						// to have the members
						if(	(ifHolder.iFolder.State == "WaitSync") ||
							(ifHolder.iFolder.CurrentUserID == null) ||
							(ifHolder.iFolder.CurrentUserID.Length == 0) )
						{
							iFolderWeb updatediFolder;
							try
							{
								updatediFolder = ifws.GetiFolder(
									args.ID);
							}
							catch(Exception e)
							{
								updatediFolder = null;
							}
							if(updatediFolder != null)
								ifHolder.iFolder = updatediFolder;
						}
						iFolderTreeStore.SetValue(iter, 0, ifHolder);
					}

					if(args.Successful)
					{
						UpdateStatus(Util.GS("Idle..."));
					}
					else
					{
						UpdateStatus(Util.GS("Failed synchronization"));
					}
					break;
				}
			}
		}


		public void HandleFileSyncEvent(FileSyncEventArgs args)
		{
			// if it isn't a file or folder, return
			if(args.ObjectType == ObjectType.Unknown)
				return;

			switch(args.Direction)
			{
				case Simias.Client.Event.Direction.Uploading:
				{
					if(args.Delete)
					{
						UpdateStatus(string.Format(Util.GS(
								"Deleting file from server: {0}"), args.Name));
					}
					else
					{
						UpdateStatus(string.Format(Util.GS(
									"Uploading file: {0}"), args.Name));
					}
					break;
				}
				case Simias.Client.Event.Direction.Downloading:
				{
					if(args.Delete)
					{
						UpdateStatus(string.Format(Util.GS(
									"Deleting file: {0}"), args.Name));
					}
					else
					{
						UpdateStatus(string.Format(Util.GS(
									"Downloading file: {0}"), args.Name));
					}
					break;
				}
			}

			if(SyncBar == null)
			{
				SyncBar = new ProgressBar();
				SyncBar.Orientation = Gtk.ProgressBarOrientation.LeftToRight;
				SyncBar.PulseStep = .01;
				MainStatusBar.PackEnd(SyncBar, false, true, 0);
			}

			if(args.SizeRemaining == args.SizeToSync)
			{
				if(args.SizeToSync > 0)
				{
					SyncBar.Show();
					SyncBar.Fraction = 0;
				}
				else
					SyncBar.Hide();

			}
			else
			{
				SyncBar.Show();
				if(args.SizeToSync == 0)
					SyncBar.Fraction = 1;
				else
				{
					double frac = (((double)args.SizeToSync) - 
									((double)args.SizeRemaining)) / 
												((double)args.SizeToSync);
					SyncBar.Fraction = frac;
				}
			}
		}


		public void OnHelpMenuItem(object o, EventArgs args)
		{
			Util.ShowHelp("front.html", this);
		}

		private void OnAbout(object o, EventArgs args)
		{
			Util.ShowAbout();
		}




		public void	SynciFolder()
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);

				try
				{
    				ifws.SynciFolderNow(ifHolder.iFolder.ID);
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




		private void SetupiFolder()
		{
			string newPath  = "";
			iFolderHolder ifHolder = null;
			TreeModel tModel;
			TreeIter iter;

			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				tSelect.GetSelected(out tModel, out iter);
				ifHolder = (iFolderHolder) tModel.GetValue(iter, 0);
				if(ifHolder.iFolder == null)
					return;
				int rc = 0;

				do
				{
					iFolderAcceptDialog iad = 
							new iFolderAcceptDialog(ifHolder.iFolder);
					iad.TransientFor = this;
					rc = iad.Run();
					newPath = iad.Path;
					iad.Hide();
					iad.Destroy();
					if(rc != -5)
						return;

					// if the user selected OK, check the path they
					// selectected, if we didn't show there was a bad
					// path, set rc to 0 to accept the ifolder
					if(!ShowBadiFolderPath(newPath, ifHolder.iFolder.Name))
						rc = 0;
				}
				while(rc == -5);
				
				try
				{
					// This will remove the current subscription
					// Read the updated subscription, and place it back
					// in the list to show status until the real iFolder
					// comes along
					curiFolders.Remove(ifHolder.iFolder.ID);

					acceptediFolders[ifHolder.iFolder.CollectionID]
							= iter;

   		 			iFolderWeb newiFolder = ifws.AcceptiFolderInvitation(
											ifHolder.iFolder.DomainID,
											ifHolder.iFolder.ID,
											newPath);
	
					tModel.SetValue(iter, 0, 
							new iFolderHolder(newiFolder));
					curiFolders.Add(newiFolder.ID, iter);
				}
				catch(Exception e)
				{
					// if we threw an exceptoin, add the old ifolder back
					curiFolders.Add(ifHolder.iFolder.ID, iter);

					iFolderExceptionDialog ied = new iFolderExceptionDialog(
														this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					return;
				}
			}
		}




		private void CreateNewiFolder()
		{
			if(ifdata.GetDomainCount() < 1)
			{
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Warning,
					iFolderMsgDialog.ButtonSet.Ok,
					Util.GS("Create iFolder"),
					Util.GS("No iFolder Domains"),
					Util.GS("A new iFolder cannot be create because you have not attached to any iFolder servers."));
				dg.Run();
				dg.Hide();
				dg.Destroy();
			}
			else
			{
				// Read all current domains before letting them create
				// a new ifolder
				DomainInformation[] domains = ifdata.GetDomains();
	
				CreateDialog cd = new CreateDialog(domains);
				cd.TransientFor = this;
	
				int rc = 0;
				do
				{
					rc = cd.Run();
					cd.Hide();

					if(rc == -5)
					{
						string selectedFolder = cd.iFolderPath;
						string selectedDomain = cd.DomainID;
	
						if(ShowBadiFolderPath(selectedFolder, null))
						continue;

						// break loop
						rc = 0;
						try
						{
   			 				iFolderWeb newiFolder = 
								ifws.CreateiFolderInDomain(selectedFolder,
															selectedDomain);
	
							if(newiFolder == null)
								throw new Exception("Simias returned null");
	
							TreeIter iter = 
								iFolderTreeStore.AppendValues(
									new iFolderHolder(newiFolder));
							curiFolders.Add(newiFolder.ID, iter);
	
							if(ClientConfig.Get(ClientConfig.KEY_SHOW_CREATION, 
											"true") == "true")
							{
								iFolderCreationDialog dlg = 
									new iFolderCreationDialog(newiFolder);
								dlg.TransientFor = this;
								int createRC;
								do
								{
									createRC = dlg.Run();
									if(createRC == (int)Gtk.ResponseType.Help)
									{
										Util.ShowHelp("front.html", this);
									}
								}while(createRC != (int)Gtk.ResponseType.Ok);
	
								dlg.Hide();
	
								if(dlg.HideDialog)
								{
									ClientConfig.Set(
										ClientConfig.KEY_SHOW_CREATION, "false");
								}
	
								cd.Destroy();
								cd = null;
							}
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
				while(rc == -5);
			}
		}
	
	

		private void ShowPreferences()
		{
			iFolderMsgDialog dg = new iFolderMsgDialog(
				this,
				iFolderMsgDialog.DialogType.Info,
				iFolderMsgDialog.ButtonSet.Ok,
				Util.GS("Show Preferences"),
				Util.GS("This will show preferences"),
				Util.GS("Eventually Calvin will get around to adding code here that will show the preferences dialog."));
			dg.Run();
			dg.Hide();
			dg.Destroy();
		
		}

/*
		// These methods are to manipulate the toolbar

		private void set_large_icon ()
		{
			toolbar.IconSize = IconSize.LargeToolbar;
		}

		private void set_icon_only ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.Icons;
		}

		private void set_text_only ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.Text;
		}

		private void set_horizontal ()
		{
			toolbar.Orientation = Orientation.Horizontal;
		}

		private void set_vertical ()
		{
			toolbar.Orientation = Orientation.Vertical;
		}
		
		private void set_both ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.Both;
		}

		private void set_both_horiz ()
		{
			toolbar.ToolbarStyle = ToolbarStyle.BothHoriz;
		}

		private void toggle_tooltips ()
		{
			if (showTooltips == true)
				showTooltips = false;
			else
				showTooltips = true;

			toolbar.Tooltips = showTooltips;
			Console.WriteLine ("Show tooltips: " + showTooltips);
		}
*/


	}
}
