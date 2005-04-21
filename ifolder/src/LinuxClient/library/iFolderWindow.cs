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
 *  Authors:
 *		Calvin Gaisford <cgaisford@novell.com>
 *		Boyd Timothy <btimothy@novell.com>
 * 
 ***********************************************************************/


using System;
using System.IO;
using System.Collections;
using Gtk;

using Simias.Client;
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
		private uint		objectsToSync;

		public iFolderHolder(iFolderWeb ifolder)
		{
			this.ifolder = ifolder;
			isSyncing = false;
			syncSuccessful = true;
			objectsToSync = 0;
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
		
		public uint ObjectsToSync
		{
			get
			{
				return objectsToSync;
			}
			set
			{
				objectsToSync = value;
				UpdateDisplayData();
			}
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
				{
					if (objectsToSync > 0)
					{
						state = string.Format(Util.GS("{0} objects to sync"), objectsToSync);
					}
					else
					{
						state = Util.GS("Synchronizing");
					}
				}
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
		private SimiasWebService	simws;
		private iFolderData			ifdata;
		private Gdk.Pixbuf			iFolderPixBuf;
		private Gdk.Pixbuf			ServeriFolderPixBuf;

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
		private Gtk.OptionMenu		DomainFilterOptionMenu;

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

		// curDomain should be set to the ID of the domain selected in the
		// Domain Filter or if "all" domains are selected, this should be
		// set to null.		
		private string				curDomain;
		private DomainInformation[] curDomains;

		// These variables are used to keep track of how many
		// outstanding objects there are during a sync so that we don't
		// have to call CalculateSyncSize() over and over needlessly.
		private uint objectsToSync = 0;
		private bool startingSync  = false;

		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public iFolderWindow(iFolderWebService webService, SimiasWebService SimiasWS)
			: base (Util.GS("iFolders"))
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;
			simws = SimiasWS;
			ifdata = iFolderData.GetData();
			curiFolders = new Hashtable();
			curDomain = null;
			curDomains = null;

			CreateWidgets();
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (600, 480);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
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

			tb.AppendSpace();

			HBox domainFilterBox = new HBox();
			domainFilterBox.Spacing = 5;
			tb.AppendWidget(domainFilterBox,
							Util.GS("Filter the list of iFolders by server"),
							null);
							
			Label l = new Label(Util.GS("Server:"));
			domainFilterBox.PackStart(l, false, false, 0);

			VBox domainFilterSpacerBox = new VBox();
			domainFilterBox.PackStart(domainFilterSpacerBox, false, false, 0);

			// We have to add a spacer before and after the option menu to get the
			// OptionMenu to size properly in the Toolbar.
			Label spacer = new Label("");
			domainFilterSpacerBox.PackStart(spacer, false, false, 0);
			
			DomainFilterOptionMenu = new OptionMenu();
			DomainFilterOptionMenu.Changed += new EventHandler(DomainFilterChangedHandler);
			domainFilterSpacerBox.PackStart(DomainFilterOptionMenu, false, false, 0);

			spacer = new Label("");
			domainFilterSpacerBox.PackEnd(spacer, false, false, 0);

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
					new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png")));
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
//					new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png")));
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
				new Gdk.Pixbuf(Util.ImagesPath("serverifolder24.png"));
			iFolderPixBuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
		
			return vbox;
		}
	



		private void OnRealizeWidget(object o, EventArgs args)
		{
			iFolderTreeView.HasFocus = true;
			RefreshDomains(false);
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

			if(readFromSimias)
				ifdata.Refresh();

			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					if (curDomain == null)
					{
						TreeIter iter = iFolderTreeStore.AppendValues(holder);
						curiFolders[holder.iFolder.CollectionID] = iter;
					}
					else if (curDomain == holder.iFolder.DomainID)
					{
						// Only add in iFolders that match the current domain filter
						TreeIter iter = iFolderTreeStore.AppendValues(holder);
						curiFolders[holder.iFolder.CollectionID] = iter;
					}
				}
			}
			
			// Update the POBox for every domain so that the user can get
			// notification of new iFolder subscriptions.
			DomainInformation[] domains = ifdata.GetDomains();
			if (domains != null)
			{
				foreach(DomainInformation domain in domains)
				{
					try
					{
						ifws.SynciFolderNow(domain.POBoxID);
					}
					catch
					{
					}
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
			UpdateButtonSensitivity();
		}

		private void UpdateButtonSensitivity()
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

				try
				{
					PropertiesDialog = 
						new iFolderPropertiesDialog(this, 
									ifHolder.iFolder, 
									ifws, simws);
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
					if (PropertiesDialog != null)
					{
						if (PropertiesDialog.CurrentPage == 0)
						{
							Util.ShowHelp("properties.html.html", this);
						}
						else if (PropertiesDialog.CurrentPage == 1)
						{
							Util.ShowHelp("sharewith.html", this);
						}
						else
						{
							Util.ShowHelp("front.html", this);
						}
					}
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
    					iFolderHolder newHolder =
								ifdata.RevertiFolder(ifHolder.iFolder.ID);

//						curiFolders.Remove(ifHolder.iFolder.ID);

						// Set the value of the returned value for the one
						// that was there
//						iFolderHolder holder = new iFolderHolder(newiFolder);
						iFolderTreeStore.SetValue(iter, 0, newHolder);
//						curiFolders.Add(newiFolder.ID, iter);
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

					UpdateButtonSensitivity();
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
			if (path == null || name == null)
				throw new Exception("Cannot pass in null parameters to ShowBadiFolderPath");

			string fullPath = System.IO.Path.Combine(path, name);
			if (!ifws.CanBeiFolder(fullPath))
			{
				// Check to see if the name has any invalid characters in it
				char[] invalidChars = simws.GetInvalidSyncFilenameChars();
				if (name.IndexOfAny(invalidChars) >= 0)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Info,
						iFolderMsgDialog.ButtonSet.Ok,
						Util.GS("Invalid iFolder Path"),
						Util.GS("Invalid characters in selected path"),
						string.Format(Util.GS("The path you have entered contains invalid characters for an iFolder.  iFolders cannot contain the following characters: {0}"),
									  new string(invalidChars)));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
				else
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
				}

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

//				iFolderWeb remiFolder = ifHolder.iFolder;

				try
				{
					iFolderTreeStore.Remove(ref iter);

					curiFolders.Remove(ifHolder.iFolder.CollectionID);

					// use the ID here because it could be a subscription
					ifdata.DeleteiFolder(ifHolder.iFolder.ID);

					UpdateButtonSensitivity();
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
										ifws,
										simws);
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
				if (args.ResponseId == ResponseType.Help)
					Util.ShowHelp("conflicts.html", this);
				else
				{
					ConflictDialog.Hide();
					ConflictDialog.Destroy();
					ConflictDialog = null;
				}
			}
			// CRG: TODO
			// At this point, refresh the selected iFolder to see if it
			// has any more conflicts

			UpdateButtonSensitivity();
		}




		// update the data value in the iFolderTreeStore so the ifolder
		// will switch to one that has conflicts
		public void iFolderHasConflicts(string iFolderID)
		{
			if(curiFolders.ContainsKey(iFolderID))
			{
				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);

				TreeIter iter = (TreeIter)curiFolders[iFolderID];

				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}

			// TODO: let any property dialogs know that this iFolder
			// has a conflict

			UpdateButtonSensitivity();
		}



		public void iFolderChanged(string iFolderID)
		{
			if(curiFolders.ContainsKey(iFolderID))
			{
				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);

				TreeIter iter = (TreeIter)curiFolders[iFolderID];

				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}

			UpdateButtonSensitivity();
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
				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);

				if( (curDomain != null) && 
						(curDomain != ifHolder.iFolder.DomainID) )
				{
					// don't do anything because we are not showing this
					// domain right now
				}
				else
				{
					iter = iFolderTreeStore.AppendValues(ifHolder);
					curiFolders[iFolderID] = iter;
				}
			}
			else
			{
				// just update with the current from ifdata
				TreeIter iter = (TreeIter)curiFolders[iFolderID];
				iFolderHolder ifHolder = 
							ifdata.GetiFolder(iFolderID);
				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}

			UpdateButtonSensitivity();
		}



		public void HandleSyncEvent(CollectionSyncEventArgs args)
		{
			switch(args.Action)
			{
				case Action.StartSync:
				{
					UpdateStatus(string.Format(Util.GS(
									"Syncing: {0}"), args.Name));

					// Keep track of when a sync starts regardless of
					// whether the iFolder is currently shown because
					// if the user switches the iFolder Window domain
					// filter, we'll still need this.
					startingSync = true;

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
							iFolderHolder updatedHolder = null;
							updatedHolder = ifdata.ReadiFolder(args.ID);
							if(updatedHolder != null)
								ifHolder = updatedHolder;
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
						ifHolder.SyncSuccessful = args.Connected;

						// This is kind of a hack
						// Sometimes, iFolders will come through that
						// don't have members so we need to update them
						// to have the members
						if(	(ifHolder.iFolder.State == "WaitSync") ||
							(ifHolder.iFolder.CurrentUserID == null) ||
							(ifHolder.iFolder.CurrentUserID.Length == 0) )
						{
							iFolderHolder updatedHolder = null;
							updatedHolder = ifdata.ReadiFolder(args.ID);
							if(updatedHolder != null)
								ifHolder = updatedHolder;
						}

						SyncSize syncSize = null;
				
						try
						{
							syncSize = ifws.CalculateSyncSize(args.ID);
							ifHolder.ObjectsToSync = syncSize.SyncNodeCount;
						}
						catch
						{}
				
						iFolderTreeStore.SetValue(iter, 0, ifHolder);

						UpdateButtonSensitivity();
					}

					objectsToSync = 0;

					UpdateStatus(Util.GS("Idle..."));
					break;
				}
			}

			// If the properties dialog is open, update it so it shows the
			// current status (last sync time, objects to sync, etc.)						
			if (PropertiesDialog != null && 
				PropertiesDialog.iFolder.ID == args.ID)
			{
				if (curiFolders.ContainsKey(args.ID))
				{
					iFolderHolder ifHolder = ifdata.GetiFolder(args.ID);
					PropertiesDialog.UpdateiFolder(ifHolder.iFolder);
				}
			}
		}


		public void HandleFileSyncEvent(FileSyncEventArgs args)
		{
			if(SyncBar == null)
			{
				SyncBar = new ProgressBar();
				SyncBar.Orientation = Gtk.ProgressBarOrientation.LeftToRight;
				SyncBar.PulseStep = .01;
				MainStatusBar.PackEnd(SyncBar, false, true, 0);
			}

			if (args.SizeRemaining == args.SizeToSync)
			{
				// Init the progress bar
				SyncBar.Show();
				SyncBar.Fraction = 0;

				if (startingSync || (objectsToSync <= 0))
				{
					startingSync = false;
					try
					{
						SyncSize syncSize = ifws.CalculateSyncSize(args.CollectionID);
						objectsToSync = syncSize.SyncNodeCount;
					}
					catch(Exception e)
					{
						objectsToSync = 1;
					}
				}

				// Decrement the count whether we're showing the iFolder
				// in the current list or not.  We'll need this if the
				// user switches back to the list that contains the iFolder
				// that is actually synchronizing.
				objectsToSync--;

				// Get the iFolderHolder and set the objectsToSync (only if the
				// domain filter isn't set or is for this iFolder's domain.
				iFolderHolder ifHolder = ifdata.GetiFolder(args.CollectionID);
				if (ifHolder != null && (curDomain == null || curDomain == ifHolder.iFolder.DomainID))
				{
					ifHolder.ObjectsToSync = objectsToSync;
					TreeIter iter = (TreeIter)curiFolders[args.CollectionID];
					iFolderTreeStore.SetValue(iter, 0, ifHolder);
				}

				switch (args.ObjectType)
				{
					case ObjectType.File:
						if (args.Delete)
							UpdateStatus(string.Format(
								Util.GS("Deleting file on client: {0}"),
								args.Name));
						else
						{
							if (args.Direction == Simias.Client.Event.Direction.Uploading)
								UpdateStatus(string.Format(
									Util.GS("Uploading file: {0}"),
									args.Name));
							else
								UpdateStatus(string.Format(
									Util.GS("Downloading file: {0}"),
									args.Name));
						}
						break;
					case ObjectType.Directory:
						if (args.Delete)
							UpdateStatus(string.Format(
								Util.GS("Deleting directory on client: {0}"),
								args.Name));
						else
						{
							if (args.Direction == Simias.Client.Event.Direction.Uploading)
								UpdateStatus(string.Format(
									Util.GS("Uploading directory: {0}"),
									args.Name));
							else
								UpdateStatus(string.Format(
									Util.GS("Downloading directory: {0}"),
									args.Name));
						}
						break;
					case ObjectType.Unknown:
						UpdateStatus(string.Format(
							Util.GS("Deleting on server: {0}"),
							args.Name));
						break;
				}
			}
			else
			{
				// Update the sync progress bar
				SyncBar.Show();
				if (args.SizeToSync > 0)
				{
					SyncBar.Fraction =
						(((double)args.SizeToSync) - ((double)args.SizeRemaining)) /
						((double)args.SizeToSync);
				}
				else
					SyncBar.Fraction = 1;
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
							new iFolderAcceptDialog(ifHolder.iFolder, Util.LastSetupPath);
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
					{
						rc = 0;

						// Save off the path so that the next time the user
						// opens the setup dialog, we'll open to the same
						// directory
						Util.LastSetupPath = newPath;
					}
				}
				while(rc == -5);
				
				try
				{
					// This will remove the current subscription
					// Read the updated subscription, and place it back
					// in the list to show status until the real iFolder
					// comes along
//					curiFolders.Remove(ifHolder.iFolder.ID);

					iFolderHolder newHolder = ifdata.AcceptiFolderInvitation(
													ifHolder.iFolder.ID,
													ifHolder.iFolder.DomainID,
													newPath);

					tModel.SetValue(iter, 0, newHolder);
//					curiFolders.Add(newiFolder.ID, iter);
				}
				catch(Exception e)
				{
					// if we threw an exceptoin, add the old ifolder back
//					curiFolders.Add(ifHolder.iFolder.ID, iter);

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
			// Re-read the data in case a new domain has been created
			ifdata.RefreshDomains();

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
				return;
			}

			DomainInformation[] domains = ifdata.GetDomains();
	
			CreateDialog cd = new CreateDialog(domains, Util.LastCreatedPath);
			cd.TransientFor = this;
	
			int rc = 0;
			do
			{
				rc = cd.Run();
				cd.Hide();

				if(rc == -5)
				{
					string selectedFolder = cd.iFolderPath.Trim();
					string selectedDomain = cd.DomainID;

					if (selectedFolder == String.Empty)
					{
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Warning,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("Invalid iFolder Path"),
							Util.GS("Invalid iFolder path selected"),
							Util.GS("The path you've specified is empty.  Please enter a properly formatted path for the new iFolder."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						continue;
					}
						
					string parentDir = System.IO.Path.GetDirectoryName( selectedFolder );
					if ( ( parentDir == null ) || ( parentDir == String.Empty ) )
					{
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Warning,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("Invalid iFolder Path"),
							Util.GS("Invalid iFolder path selected"),
							Util.GS("The path you've specified is invalid.  Please enter a properly formatted path for the new iFolder."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						continue;
					}
					
					string name = selectedFolder.Substring(parentDir.Length + 1);
					if (name == null || name == String.Empty)
					{
						iFolderMsgDialog dg = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Warning,
							iFolderMsgDialog.ButtonSet.Ok,
							Util.GS("Invalid iFolder Path"),
							Util.GS("Invalid iFolder path selected"),
							Util.GS("The path you've specified is invalid.  Please remove the trailing path separator character (/) and try again."));
						dg.Run();
						dg.Hide();
						dg.Destroy();
						continue;
					}

					if(ShowBadiFolderPath(parentDir, name))
					continue;

					iFolderHolder ifHolder = null;
					try
					{
						ifHolder = 
							ifdata.CreateiFolder(	selectedFolder,
													selectedDomain);
					}
					catch(Exception e)
					{
						if (e.Message.IndexOf("Path did not exist") >= 0)
						{
							iFolderMsgDialog dg = new iFolderMsgDialog(
								this,
								iFolderMsgDialog.DialogType.Warning,
								iFolderMsgDialog.ButtonSet.Ok,
								Util.GS("Invalid iFolder Path"),
								Util.GS("Invalid iFolder path selected"),
								Util.GS("The path you've specified does not exist.  Please select an existing folder and try again."));
							dg.Run();
							dg.Hide();
							dg.Destroy();
							continue;
						}
						else
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

					if(ifHolder == null)
						throw new Exception("Simias returned null");

					// If we make it this far, we've succeeded and we don't
					// need to keep looping.
					rc = 0;

					// Reset the domain filter so the new iFolder will show
					// up in the list regardless of what was selected previously.
					// DomainFilterOptionMenu.SetHistory(0);

					TreeIter iter = 
						iFolderTreeStore.AppendValues(ifHolder);

					curiFolders[ifHolder.iFolder.ID] = iter;
	
					UpdateButtonSensitivity();

					// Save off the path so that the next time the user
					// creates an iFolder, we'll open it to the directory
					// they used last.
					Util.LastCreatedPath = ifHolder.iFolder.UnManagedPath;

					if(ClientConfig.Get(ClientConfig.KEY_SHOW_CREATION, 
									"true") == "true")
					{
						iFolderCreationDialog dlg = 
							new iFolderCreationDialog(ifHolder.iFolder);
						dlg.TransientFor = this;
						int createRC;
						do
						{
							createRC = dlg.Run();
							if(createRC == (int)Gtk.ResponseType.Help)
							{
								Util.ShowHelp("myifolders.html", this);
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
			}
			while(rc == -5);
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

		public void DomainFilterChangedHandler(object o, EventArgs args)
		{
			// Change the global "domainSelected" (null if "All" is chosen by
			// the user) and then make the call to refresh the window.
			if (curDomains != null)
			{
				int selectedItem = DomainFilterOptionMenu.History;
				if (selectedItem == 0)
				{
					curDomain = null;
				}
				else
				{
					// The OptionMenu has 1 extra item in it than the list
					// of domains in curDomain, so offset the index by 1.
					selectedItem--;
					curDomain = curDomains[selectedItem].ID;
				}
			
				RefreshiFolders(false);
			}
		}

		public void RefreshDomains(bool readFromSimias)
		{
			if(readFromSimias)
				ifdata.RefreshDomains();

			// Add on "Show All Servers"
			Menu m = new Menu();
			m.Title = Util.GS("Server:");
			m.Append(new MenuItem(Util.GS("Show All")));

			curDomains = ifdata.GetDomains();
			if (curDomains != null)
			{
				foreach(DomainInformation domain in curDomains)
				{
					m.Append(new MenuItem(domain.Name));
				}
			}
			
			DomainFilterOptionMenu.Menu = m;
			DomainFilterOptionMenu.ShowAll();
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
