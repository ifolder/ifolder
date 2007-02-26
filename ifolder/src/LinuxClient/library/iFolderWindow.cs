/***********************************************************************
 *  $RCSfile: iFolderWindow.cs,v $
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
using System.Text;
using System.Threading;
using Gtk;

using Simias.Client;
using Simias.Client.Event;

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

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
		private iFolderWebService	ifws;
		private SimiasWebService	simws;
		private iFolderData		ifdata;
		private SimiasEventBroker	simiasEventBroker;

		private Statusbar			MainStatusBar;
		private ProgressBar		SyncBar;

		private ImageMenuItem		NewMenuItem;
		private Gtk.MenuItem		ShareMenuItem;
		private ImageMenuItem		OpenMenuItem;
		private Gtk.MenuItem		ConflictMenuItem;
		private Gtk.MenuItem		SyncNowMenuItem;
		private ImageMenuItem		RevertMenuItem;
		private ImageMenuItem		DeleteMenuItem;
		private ImageMenuItem		RemoveMenuItem;
		private ImageMenuItem		DownloadMenuItem;
		private ImageMenuItem		PropMenuItem;
		private ImageMenuItem		CloseMenuItem;
		private ImageMenuItem		QuitMenuItem;
		private ImageMenuItem		RefreshMenuItem;
		private ImageMenuItem		HelpMenuItem;
	        private MenuItem                RecoveryMenuItem;
		private MenuItem		ExportMenuSubItem;
		private MenuItem 		ImportMenuSubItem;
	        private MenuItem                ResetPassMenuItem;
		private ImageMenuItem		AboutMenuItem;
		
		private ImageMenuItem		PreferencesMenuItem;
		private Gtk.MenuItem		AccountsMenuItem;
		private Gtk.MenuItem		SyncLogMenuItem;
		private CheckMenuItem		ViewServeriFoldersMenuItem;

		private Gtk.MenuItem		MigrateMenuItem;
		private Gtk.MenuItem		MigrateMenuSubItem;

//		private Gtk.MenuItem 		Migrate2xMenuItem;		

		private DomainController	domainController;

		// Manager object that knows about simias resources.
		private Manager			simiasManager;

		///
		/// Keep track of open windows so that if we're called again for one
		/// that is already open, we'll just present it to the user instead
		/// of opening an additional one.
		///
		private Hashtable			PropDialogs;
		private Hashtable			ConflictDialogs;

		///
		/// iFolder Content Area
		///
		private EventBox			ContentEventBox;

		///
		/// Actions Pane
		///
		private Entry				SearchEntry;
		private Button				CancelSearchButton;
		private uint				searchTimeoutID;

		private Button				AddiFolderButton;
		private Button				ShowHideAllFoldersButton;
		private Label				ShowHideAllFoldersButtonText;
		
		private bool				bAvailableFoldersShowing;

		private ScrolledWindow		iFoldersScrolledWindow;
		private iFolderIconView	iFoldersIconView;
		private iFolderViewGroup	localGroup;
		private TreeModelFilter	myiFoldersFilter;
		private Timer				updateStatusTimer;

		private VBox				SynchronizedFolderTasks;

		///
		/// Buttons for local iFolders
		///
		private Button				OpenSynchronizedFolderButton;
		private Button				SynchronizeNowButton;
		private Button				ShareSynchronizedFolderButton;
		private Button				ResolveConflictsButton;
		private Button				RemoveiFolderButton;
		private Button				ViewFolderPropertiesButton;
		
		///
		/// Buttons for Available iFolders
		///
		private Button				DownloadAvailableiFolderButton;
		private Button				DeleteFromServerButton;
		private Button				RemoveMembershipButton;
		
		private Hashtable			serverGroups;
		private Hashtable			serverGroupFilters;
		
		///
		/// Variables to keep track of the last position of the main window.
		/// This is needed because somehow it's forgotton sometimes.
		/// Util.ShowiFolderWindow will use this.
		///
		private int lastXPos;
		private int lastYPos;
		
		public int LastXPos
		{
			get
			{
				return lastXPos;
			}
		}
		
		public int LastYPos
		{
			get
			{
				return lastYPos;
			}
		}

        // Drag and Drop
        public enum DragTargetType
        {
        	UriList,
        	RootWindow,
        	iFolderID
        };

		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public iFolderWindow(iFolderWebService webService, SimiasWebService SimiasWS, Manager simiasManager)
			: base (Util.GS("iFolder"))
		{
			if(webService == null)
				throw new ApplicationException("iFolderWebServices was null");

			ifws = webService;
			simws = SimiasWS;
			this.simiasManager = simiasManager;
			ifdata = iFolderData.GetData();

			serverGroups = new Hashtable();
			serverGroupFilters = new Hashtable();

			PropDialogs = new Hashtable();
			ConflictDialogs = new Hashtable();
			
			lastXPos = -1;
			lastYPos = -1;
			
			searchTimeoutID = 0;
			
			domainController = DomainController.GetDomainController();

			bAvailableFoldersShowing = false;

			CreateWidgets();

			RefreshiFolders(true);
			
			if (domainController != null)
			{
				domainController.DomainAdded +=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted +=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
				domainController.DomainLoggedIn +=
					new DomainLoggedInEventHandler(OnDomainLoggedInEvent);
				domainController.DomainLoggedOut +=
					new DomainLoggedOutEventHandler(OnDomainLoggedOutEvent);
			}
			
			simiasEventBroker = SimiasEventBroker.GetSimiasEventBroker();
			if (simiasEventBroker != null)
			{
				simiasEventBroker.CollectionSyncEventFired +=
					new CollectionSyncEventHandler(OniFolderSyncEvent);
				simiasEventBroker.FileSyncEventFired +=
					new FileSyncEventHandler(OniFolderFileSyncEvent);
			}
		}
		
		~iFolderWindow()
		{
			if (domainController != null)
			{
				domainController.DomainAdded -=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted -=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
				domainController.DomainLoggedIn -=
					new DomainLoggedInEventHandler(OnDomainLoggedInEvent);
				domainController.DomainLoggedOut -=
					new DomainLoggedOutEventHandler(OnDomainLoggedOutEvent);
			}
			
			if (simiasEventBroker != null)
			{
				simiasEventBroker.CollectionSyncEventFired -=
					new CollectionSyncEventHandler(OniFolderSyncEvent);
				simiasEventBroker.FileSyncEventFired -=
					new FileSyncEventHandler(OniFolderFileSyncEvent);
			}
		}

		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (600, 480);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			this.Add(CreateContentArea());

			// Set up an event to refresh when the window is
			// being drawn
			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
		///
		/// Normal Page
		///
		
		private Widget CreateContentArea()
		{
			VBox vbox = new VBox (false, 0);

			//-----------------------------
			// Create the menubar
			//-----------------------------
			MenuBar menubar = CreateNormalMenu ();
			vbox.PackStart (menubar, false, false, 0);

			///
			/// Create the main content area
			///
			vbox.PackStart(CreateiFolderContentArea(), true, true, 0);

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
			DownloadMenuItem.Sensitive = false;
			DeleteMenuItem.Sensitive = false;
			RemoveMenuItem.Sensitive = false;
			RemoveMenuItem.Visible = false;
			ShareMenuItem.Sensitive = false;
			OpenMenuItem.Sensitive = false;
			SyncNowMenuItem.Sensitive = false;
			ConflictMenuItem.Sensitive = false;
			RevertMenuItem.Sensitive = false;
			PropMenuItem.Sensitive = false;
			
			return vbox;
		}
		
		/// <summary>
		/// Creates the menubar for the iFolderWindow
		/// </summary>
		/// <returns>
		/// MenuBar for the iFolderWindow
		/// </returns>
		private MenuBar CreateNormalMenu ()
		{
			// FIXME: Work over the normal menu to update it with the latest terminology
			MenuBar menubar = new MenuBar ();
			AccelGroup agrp = new AccelGroup();
			this.AddAccelGroup(agrp);

			//----------------------------
			// iFolder Menu
			//----------------------------
			Menu iFolderMenu = new Menu();

			NewMenuItem = new ImageMenuItem (Util.GS("_Upload a folder..."));
			NewMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder-upload16.png")));
			iFolderMenu.Append(NewMenuItem);
			NewMenuItem.AddAccelerator("activate", agrp,
				new AccelKey(Gdk.Key.N, Gdk.ModifierType.ControlMask,
								AccelFlags.Visible));
			NewMenuItem.Activated += new EventHandler(AddiFolderHandler);

			DownloadMenuItem =
				new ImageMenuItem (Util.GS("_Download..."));
			DownloadMenuItem.Image = new Image(
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-download16.png")));
			iFolderMenu.Append(DownloadMenuItem);
			DownloadMenuItem.Activated += new EventHandler(DownloadAvailableiFolderHandler);

			DeleteMenuItem =
				new ImageMenuItem (Util.GS("Dele_te from server"));
			DeleteMenuItem.Image = new Image(Stock.Delete, Gtk.IconSize.Menu);
			iFolderMenu.Append(DeleteMenuItem);
			DeleteMenuItem.Activated += new EventHandler(DeleteFromServerHandler);

			RemoveMenuItem =
				new ImageMenuItem (Util.GS("Re_move my membership"));
			RemoveMenuItem.Image = new Image(Stock.Delete, Gtk.IconSize.Menu);
			iFolderMenu.Append(RemoveMenuItem);
			RemoveMenuItem.Activated += new EventHandler(RemoveMembershipHandler);

			iFolderMenu.Append(new SeparatorMenuItem());
			OpenMenuItem = new ImageMenuItem ( Stock.Open, agrp );
			iFolderMenu.Append(OpenMenuItem);
			OpenMenuItem.Activated += new EventHandler(OnOpenSynchronizedFolder);

			ShareMenuItem = new MenuItem (Util.GS("Share _with..."));
			iFolderMenu.Append(ShareMenuItem);
			ShareMenuItem.Activated += new EventHandler(OnShareSynchronizedFolder);

			ConflictMenuItem = new MenuItem (Util.GS("Resolve conflic_ts"));
			iFolderMenu.Append(ConflictMenuItem);
			ConflictMenuItem.Activated +=
					new EventHandler(OnResolveConflicts);

			SyncNowMenuItem = new MenuItem(Util.GS("S_ynchronize now"));
			iFolderMenu.Append(SyncNowMenuItem);
			SyncNowMenuItem.Activated += new EventHandler(OnSynchronizeNow);

			RevertMenuItem = 
				new ImageMenuItem (Util.GS("_Revert to a normal folder"));
			RevertMenuItem.Image = new Image(Stock.Undo, Gtk.IconSize.Menu);
			iFolderMenu.Append(RevertMenuItem);
			RevertMenuItem.Activated += new EventHandler(RemoveiFolderHandler);

			PropMenuItem = new ImageMenuItem (Stock.Properties, agrp);
			iFolderMenu.Append(PropMenuItem);
			PropMenuItem.Activated += new EventHandler(OnShowFolderProperties);

			iFolderMenu.Append(new SeparatorMenuItem());

			MigrateMenuItem = new MenuItem(Util.GS("_Migrate iFolder"));
			Menu MigrateMenu = new Menu();
			MigrateMenuSubItem = new MenuItem(Util.GS("Migrate from 2.x"));
			MigrateMenu.Append(MigrateMenuSubItem);
			MigrateMenuItem.Submenu = MigrateMenu;
			iFolderMenu.Append( MigrateMenuItem);
			MigrateMenuSubItem.Activated += new EventHandler(Migrate2xClickedHandler);

			iFolderMenu.Append(new SeparatorMenuItem());
			CloseMenuItem = new ImageMenuItem (Stock.Close, agrp);
			iFolderMenu.Append(CloseMenuItem);
			CloseMenuItem.Activated += new EventHandler(CloseEventHandler);
			
			QuitMenuItem = new ImageMenuItem(Stock.Quit, agrp);
			iFolderMenu.Append(QuitMenuItem);
			QuitMenuItem.Activated += new EventHandler(QuitEventHandler);

			MenuItem iFolderMenuItem = new MenuItem(Util.GS("i_Folder"));
			iFolderMenuItem.Submenu = iFolderMenu;
			menubar.Append (iFolderMenuItem);

			//----------------------------
			// Edit Menu
			//----------------------------
			Menu EditMenu = new Menu();

			AccountsMenuItem =
				new MenuItem (Util.GS("_Account Settings..."));
			EditMenu.Append(AccountsMenuItem);
			AccountsMenuItem.Activated += new EventHandler(AccountsMenuItemHandler);

			PreferencesMenuItem = new ImageMenuItem(Util.GS("_Preferences"));
			PreferencesMenuItem.Image = new Image(Stock.Preferences, Gtk.IconSize.Menu);
			EditMenu.Append(PreferencesMenuItem);
			PreferencesMenuItem.Activated += new EventHandler(ShowPreferencesHandler);
			
			MenuItem EditMenuItem = new MenuItem(Util.GS("_Edit"));
			EditMenuItem.Submenu = EditMenu;
			menubar.Append(EditMenuItem);

			//----------------------------
			// View Menu
			//----------------------------
			Menu ViewMenu = new Menu();

			RefreshMenuItem =
				new ImageMenuItem(Stock.Refresh, agrp);
			ViewMenu.Append(RefreshMenuItem);
			RefreshMenuItem.Activated +=
					new EventHandler(RefreshiFoldersHandler);
			
			ViewMenu.Append(new SeparatorMenuItem());
			
			SyncLogMenuItem =
				new MenuItem (Util.GS("Synchronization _Log"));
			ViewMenu.Append(SyncLogMenuItem);
			SyncLogMenuItem.Activated += new EventHandler(SyncLogMenuItemHandler);

			ViewMenu.Append(new SeparatorMenuItem());
			
			// FIXME: Eventually save off the View server iFolders state so it comes up how the user left it when iFolder is restarted
			ViewServeriFoldersMenuItem =
				new CheckMenuItem(Util.GS("View _available iFolders"));
			ViewMenu.Append(ViewServeriFoldersMenuItem);
			ViewServeriFoldersMenuItem.Toggled +=
				new EventHandler(OnToggleViewServeriFoldersMenuItem);
					
			if((bool)ClientConfig.Get(ClientConfig.KEY_IFOLDER_DEBUG_COLOR_PALETTE))
			{
				MenuItem showColorPaletteMenuItem =	// FIXME: Remove this before shipping
					new MenuItem("Show _Color Palette (FIXME: Remove this before shipping)...");
				ViewMenu.Append(showColorPaletteMenuItem);
				showColorPaletteMenuItem.Activated += ShowColorPalette;
			}
			
			MenuItem ViewMenuItem = new MenuItem(Util.GS("_View"));
			ViewMenuItem.Submenu = ViewMenu;
			menubar.Append(ViewMenuItem);

			//	Changes for Migration menu item
			/*
			Menu MigrationMenu = new Menu();
	
			Migrate2xMenuItem = new MenuItem(Util.GS("Migrate from 2.x"));
			MigrationMenu.Append(Migrate2xMenuItem);
			Migrate2xMenuItem.Activated += new EventHandler(Migrate2xClickedHandler);
			MenuItem MigrationMenuItem = new MenuItem(Util.GS("_Migrate"));
			MigrationMenuItem.Submenu = MigrationMenu;
			menubar.Append(MigrationMenuItem);
			*/

			//----------------------------
			// Security Menu
			//----------------------------
			Menu SecurityMenu = new Menu();

			RecoveryMenuItem = new MenuItem(Util.GS("_Key Recovery"));
//			RecoveryMenuItem.Activated += new EventHandler(OnRecoveryMenuItem);
			SecurityMenu.Append(RecoveryMenuItem);
			ImportMenuSubItem = new MenuItem(Util.GS("Import Decrypted Keys"));
			ExportMenuSubItem = new MenuItem(Util.GS("Export Encrypted Keys")); 
			ImportMenuSubItem.Activated += new EventHandler(ImportClicked);
			ExportMenuSubItem.Activated += new EventHandler(ExportClicked);
		
			Menu recoverMenu = new Menu();
			recoverMenu.Append( ExportMenuSubItem);
			recoverMenu.Append( ImportMenuSubItem);

			RecoveryMenuItem.Submenu = recoverMenu;;

			ResetPassMenuItem = new MenuItem(Util.GS("Reset _Passphrase"));
			ResetPassMenuItem.Activated += new EventHandler(OnResetPassMenuItem);
			SecurityMenu.Append(ResetPassMenuItem);

			MenuItem MainSecurityMenuItem = new MenuItem (Util.GS ("_Security"));
			MainSecurityMenuItem.Submenu = SecurityMenu;
			menubar.Append (MainSecurityMenuItem);
			
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
//					new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png")));
			HelpMenu.Append(AboutMenuItem);
			AboutMenuItem.Activated += new EventHandler(OnAbout);

			MenuItem MainHelpMenuItem = new MenuItem(Util.GS("_Help"));
			MainHelpMenuItem.Submenu = HelpMenu;
			menubar.Append(MainHelpMenuItem);

			return menubar;
		}
		
		private Widget CreateiFolderContentArea()
		{
			ContentEventBox = new EventBox();
			ContentEventBox.ModifyBg(StateType.Normal, this.Style.Background(StateType.Active));

			VBox vbox = new VBox(false, 0);
			ContentEventBox.Add(vbox);

			HBox hbox = new HBox(false, 0);

			hbox.PackStart(CreateActions(), false, false, 12);
			hbox.PackStart(CreateIconViewPane(), true, true, 0);

			vbox.PackStart(hbox, true, true, 0);

			return ContentEventBox;
		}
		
		private Widget CreateActions()
		{
			VBox actionsVBox = new VBox(false, 0);
			actionsVBox.WidthRequest = 175;

			///
			/// Spacer
			///
			Label l = new Label("<span size=\"small\"></span>");
			actionsVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;

			///
			/// Search
			///
			l = new Label(
				string.Format(
					"<span size=\"large\">{0}</span>",
					Util.GS("Filter")));
			actionsVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.ModifyFg(StateType.Normal, this.Style.Base(StateType.Selected));
			l.Xalign = 0.0F;
			
			HBox searchHBox = new HBox(false, 4);
			actionsVBox.PackStart(searchHBox, false, false, 0);
			
			SearchEntry = new Entry();
			searchHBox.PackStart(SearchEntry, true, true, 0);
			SearchEntry.SelectRegion(0, -1);
			SearchEntry.CanFocus = true;
			SearchEntry.Changed +=
				new EventHandler(OnSearchEntryChanged);

			Image stopImage = new Image(Stock.Stop, Gtk.IconSize.Menu);
			stopImage.SetAlignment(0.5F, 0F);
			
			CancelSearchButton = new Button(stopImage);
			searchHBox.PackEnd(CancelSearchButton, false, false, 0);
			CancelSearchButton.Relief = ReliefStyle.None;
			CancelSearchButton.Sensitive = false;
			
			CancelSearchButton.Clicked +=
				new EventHandler(OnCancelSearchButton);

			///
			/// Spacer
			///
			l = new Label("<span size=\"small\"></span>");
			actionsVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;

			///
			/// iFolder Actions
			///
			l = new Label(
				string.Format(
					"<span size=\"large\">{0}</span>",
					Util.GS("General Actions")));
			actionsVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.ModifyFg(StateType.Normal, this.Style.Base(StateType.Selected));
			l.Xalign = 0.0F;

			HBox spacerHBox = new HBox(false, 0);
			actionsVBox.PackStart(spacerHBox, false, false, 0);
			spacerHBox.PackStart(new Label(""), false, false, 4);
			VBox vbox = new VBox(false, 0);
			spacerHBox.PackStart(vbox, true, true, 0);

			///
			/// Add a folder Button
			///
			HBox hbox = new HBox(false, 0);
			AddiFolderButton = new Button(hbox);
			vbox.PackStart(AddiFolderButton, false, false, 0);
			AddiFolderButton.Relief = ReliefStyle.None;

			Label buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Upload a folder...")));
//							  Util.GS("Upload a folder")));
			hbox.PackStart(buttonText, false, false, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			AddiFolderButton.Clicked +=
				new EventHandler(AddiFolderHandler);
			
			///
			/// ShowHideAllFoldersButton
			///
			hbox = new HBox(false, 0);
			ShowHideAllFoldersButton = new Button(hbox);
			vbox.PackStart(ShowHideAllFoldersButton, false, false, 0);
			ShowHideAllFoldersButton.Relief = ReliefStyle.None;

			ShowHideAllFoldersButtonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("View available iFolders")));
			hbox.PackStart(ShowHideAllFoldersButtonText, false, false, 4);
			ShowHideAllFoldersButtonText.UseMarkup = true;
			ShowHideAllFoldersButtonText.UseUnderline = false;
			ShowHideAllFoldersButtonText.Xalign = 0;
			
			ShowHideAllFoldersButton.Clicked +=
				new EventHandler(ShowHideAllFoldersHandler);
			

			///
			/// Spacer
			///
			l = new Label("<span size=\"small\"></span>");
			actionsVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;

			///
			/// Folder Actions
			///
			SynchronizedFolderTasks = new VBox(false, 0);
			actionsVBox.PackStart(SynchronizedFolderTasks, false, false, 0);
			l = new Label(
				string.Format(
					"<span size=\"large\">{0}</span>",
					Util.GS("iFolder Actions")));
			SynchronizedFolderTasks.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.ModifyFg(StateType.Normal, this.Style.Base(StateType.Selected));
			l.Xalign = 0.0F;

			spacerHBox = new HBox(false, 0);
			SynchronizedFolderTasks.PackStart(spacerHBox, false, false, 0);
			spacerHBox.PackStart(new Label(""), false, false, 4);
			vbox = new VBox(false, 0);
			spacerHBox.PackStart(vbox, true, true, 0);

			///
			/// OpenSynchronizedFolderButton
			///
			hbox = new HBox(false, 0);
			OpenSynchronizedFolderButton = new Button(hbox);
			vbox.PackStart(OpenSynchronizedFolderButton, false, false, 0);
			OpenSynchronizedFolderButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Open...")));
			hbox.PackStart(buttonText, false, false, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			OpenSynchronizedFolderButton.Clicked +=
				new EventHandler(OnOpenSynchronizedFolder);
			
			
			
			///
			/// ResolveConflictsButton
			///
			hbox = new HBox(false, 0);
			ResolveConflictsButton = new Button(hbox);
			vbox.PackStart(ResolveConflictsButton, false, false, 0);
			ResolveConflictsButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Resolve conflicts...")));
			hbox.PackStart(buttonText, false, false, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			ResolveConflictsButton.Clicked +=
				new EventHandler(OnResolveConflicts);

			///
			/// SynchronizeNowButton
			///
			hbox = new HBox(false, 0);
			SynchronizeNowButton = new Button(hbox);
			vbox.PackStart(SynchronizeNowButton, false, false, 0);
			SynchronizeNowButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Synchronize now")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			SynchronizeNowButton.Clicked +=
				new EventHandler(OnSynchronizeNow);
			

			///
			/// ShareSynchronizedFolderButton
			///
			hbox = new HBox(false, 0);
			ShareSynchronizedFolderButton = new Button(hbox);
			vbox.PackStart(ShareSynchronizedFolderButton, false, false, 0);
			ShareSynchronizedFolderButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Share with...")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			ShareSynchronizedFolderButton.Clicked +=
				new EventHandler(OnShareSynchronizedFolder);


			///
			/// RemoveiFolderButton
			///
			hbox = new HBox(false, 0);
			RemoveiFolderButton = new Button(hbox);
			vbox.PackStart(RemoveiFolderButton, false, false, 0);
			RemoveiFolderButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Revert to a normal folder")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			RemoveiFolderButton.Clicked +=
				new EventHandler(RemoveiFolderHandler);

			///
			/// ViewFolderPropertiesButton
			///
			hbox = new HBox(false, 0);
			ViewFolderPropertiesButton = new Button(hbox);
			vbox.PackStart(ViewFolderPropertiesButton, false, false, 0);
			ViewFolderPropertiesButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Properties...")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			ViewFolderPropertiesButton.Clicked +=
				new EventHandler(OnShowFolderProperties);


			///
			/// DownloadAvailableiFolderButton
			///
			hbox = new HBox(false, 0);
			DownloadAvailableiFolderButton = new Button(hbox);
			vbox.PackStart(DownloadAvailableiFolderButton, false, false, 0);
			DownloadAvailableiFolderButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Download...")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			DownloadAvailableiFolderButton.Clicked +=
				new EventHandler(DownloadAvailableiFolderHandler);


			///
			/// DeleteFromServerButton
			///
			hbox = new HBox(false, 0);
			DeleteFromServerButton = new Button(hbox);
			vbox.PackStart(DeleteFromServerButton, false, false, 0);
			DeleteFromServerButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Delete from server")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			DeleteFromServerButton.Clicked +=
				new EventHandler(DeleteFromServerHandler);


			///
			/// RemoveMembershipButton
			///
			hbox = new HBox(false, 0);
			RemoveMembershipButton = new Button(hbox);
			vbox.PackStart(RemoveMembershipButton, false, false, 0);
			RemoveMembershipButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span>{0}</span>",
							  Util.GS("Remove my membership")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			RemoveMembershipButton.Clicked +=
				new EventHandler(RemoveMembershipHandler);

			return actionsVBox;
		}
		
		private Widget CreateIconViewPane()
		{
			iFoldersScrolledWindow = new ScrolledWindow();
			iFoldersIconView = new iFolderIconView(iFoldersScrolledWindow);

			///
			/// iFolders on This Computer
			///			
			myiFoldersFilter = new TreeModelFilter(ifdata.iFolders, null);
			myiFoldersFilter.VisibleFunc = SynchronizedFoldersFilterFunc;
			
			///
			/// Create a timer that calls UpdateLocalViewItems every 30 seconds
			/// beginning in 30 seconds from now.
			updateStatusTimer =
				new Timer(new TimerCallback(UpdateLocalViewItems),
						  myiFoldersFilter,
						  30000,
						  30000);
			
			localGroup = new iFolderViewGroup(Util.GS("iFolders on This Computer"), myiFoldersFilter, SearchEntry);
			iFoldersIconView.AddGroup(localGroup);
			VBox emptyVBox = new VBox(false, 0);
			emptyVBox.BorderWidth = 12;

			Table table = new Table(3, 2, false);
			emptyVBox.PackStart(table, true, true, 0);
			table.RowSpacing = 12;
			table.ColumnSpacing = 12;
			
			// Row 1: Header
			Label l = new Label(
				string.Format("<span>{0}</span>",
							   Util.GS("There are no iFolders on this computer.  To set up an iFolder, do one of the following:")));
			table.Attach(l,
						 0, 2,
						 0, 1,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0, 0, 0);
			l.UseMarkup = true;
			l.LineWrap = true;
			l.Xalign = 0;
			
			// Row 2: Upload
			Image uploadImg = new Image(Util.ImagesPath("ifolder-upload48.png"));
			table.Attach(uploadImg,
						 0, 1,
						 1, 2,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0, 0, 0);
			l = new Label(
				string.Format("<span>{0}</span>",
							   Util.GS("Select an existing folder on this computer to upload to an iFolder Server")));
			table.Attach(l,
						 1, 2,
						 1, 2,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0, 0, 0);
			l.UseMarkup = true;
			l.LineWrap = true;
			l.Xalign = 0;
			
			// Row 3: Download
			Image downloadImg = new Image(Util.ImagesPath("ifolder-download48.png"));
			table.Attach(downloadImg,
						 0, 1,
						 2, 3,
						 AttachOptions.Shrink | AttachOptions.Fill,
						 0, 0, 0);
			l = new Label(
				string.Format("<span>{0}</span>",
							   Util.GS("Select an iFolder on the server to download to this computer")));
			table.Attach(l,
						 1, 2,
						 2, 3,
						 AttachOptions.Expand | AttachOptions.Fill,
						 0, 0, 0);
			l.UseMarkup = true;
			l.LineWrap = true;
			l.Xalign = 0;
			
			localGroup.EmptyWidget = emptyVBox;
			
			VBox emptySearchVBox = new VBox(false, 0);
			emptySearchVBox.BorderWidth = 12;
			l = new Label(
				string.Format("<span size=\"large\">{0}</span>",
							   Util.GS("No matches found")));
			emptySearchVBox.PackStart(l, true, true, 0);
			l.UseMarkup = true;
			l.LineWrap = true;

			localGroup.EmptySearchWidget = emptySearchVBox;

			// FIXME: Attach the drag and drop receiver to the synchronized iFolders only
			TargetEntry[] targets =
				new TargetEntry[]
				{
	                new TargetEntry ("text/uri-list", 0, (uint) DragTargetType.UriList),
	                new TargetEntry ("application/x-root-window-drop", 0, (uint) DragTargetType.RootWindow),
	                new TargetEntry ("text/ifolder-id", 0, (uint) DragTargetType.iFolderID)
				};

			Drag.DestSet(iFoldersIconView,
						 //Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask,
						 DestDefaults.All,
						 targets,
						 Gdk.DragAction.Copy | Gdk.DragAction.Move);

			iFoldersIconView.DragMotion +=
				new DragMotionHandler(OnIconViewDragMotion);
				
			iFoldersIconView.DragDrop +=
				new DragDropHandler(OnIconViewDragDrop);
			
			iFoldersIconView.DragDataReceived +=
				new DragDataReceivedHandler(OnIconViewDragDataReceived);
			

			///
			/// My Available iFolders
			///
			DomainInformation[] domains = domainController.GetDomains();
			foreach (DomainInformation domain in domains)
			{
				AddServerGroup(domain.ID);
			}

			iFoldersIconView.SelectionChanged +=
				new EventHandler(OniFolderIconViewSelectionChanged);
			
			iFoldersIconView.BackgroundClicked +=
				new iFolderClickedHandler(OniFolderIconViewBackgroundClicked);
			iFoldersIconView.iFolderClicked +=
				new iFolderClickedHandler(OniFolderClicked);
//			iFoldersIconView.iFolderDoubleClicked +=
//				new iFolderClickedHandler(OniFolderDoubleClicked);
			iFoldersIconView.iFolderActivated +=
				new iFolderActivatedHandler(OniFolderActivated);
				
			iFoldersIconView.KeyPressEvent +=
				new KeyPressEventHandler(OniFolderIconViewKeyPress);

/*		
			// FIXME: Attach the drag and drop receiver to the synchronized iFolders only
			TargetEntry[] targets =
				new TargetEntry[]
				{
	                new TargetEntry ("text/uri-list", 0, (uint) DragTargetType.UriList),
	                new TargetEntry ("application/x-root-window-drop", 0, (uint) DragTargetType.RootWindow)
				};

			Drag.DestSet(iFoldersIconView,
						 //Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask,
						 DestDefaults.All,
						 targets,
						 Gdk.DragAction.Copy | Gdk.DragAction.Move);

			iFoldersIconView.DragMotion +=
				new DragMotionHandler(OnIconViewDragMotion);
				
			iFoldersIconView.DragDrop +=
				new DragDropHandler(OnIconViewDragDrop);
			
			iFoldersIconView.DragDataReceived +=
				new DragDataReceivedHandler(OnIconViewDragDataReceived);
			
*/
			iFoldersScrolledWindow.AddWithViewport(iFoldersIconView);
			
			return iFoldersScrolledWindow;
		}
		
		///
		/// Event Handlers
		///
		
		private void UpdateLocalViewItems(object state)
		{
			// Do the work on the main UI thread so that stuff isn't corrupted.
			GLib.Idle.Add(UpdateLocalViewItemsMainThread);
		}
		
		private bool UpdateLocalViewItemsMainThread()
		{
			iFolderViewItem[] viewItems = localGroup.Items;
			
			foreach(iFolderViewItem item in viewItems)
			{
				item.Refresh();
			}
		
			return false;	// Prevent GLib.Idle from calling this again automatically
		}
		
		private void OnOpenSynchronizedFolder(object o, EventArgs args)
		{
			OpenSelectedFolder();
		}
		
		private void OnResolveConflicts(object o, EventArgs args)
		{
			ResolveSelectedFolderConflicts();
		}

		private void OnSynchronizeNow(object o, EventArgs args)
		{
			SyncSelectedFolder();
		}
		
		private void OnShareSynchronizedFolder(object o, EventArgs args)
		{
			ShareSelectedFolder();
		}

		private void ShowFolderProperties(iFolderHolder ifHolder, int desiredPage)
		{
			if (ifHolder != null)
			{
				iFolderPropertiesDialog propsDialog =
					(iFolderPropertiesDialog) PropDialogs[ifHolder.iFolder.ID];
				if (propsDialog == null)
				{
					try
					{
						propsDialog = 
							new iFolderPropertiesDialog(this, ifHolder.iFolder, ifws, simws, simiasManager);
						propsDialog.SetPosition(WindowPosition.Center);
						propsDialog.Response += 
								new ResponseHandler(OnPropertiesDialogResponse);
						propsDialog.CurrentPage = desiredPage;
						propsDialog.ShowAll();
	
						PropDialogs[ifHolder.iFolder.ID] = propsDialog;
					}
					catch(Exception e)
					{
						if(propsDialog != null)
						{
							propsDialog.Hide();
							propsDialog.Destroy();
							propsDialog = null;
						}
	
						iFolderExceptionDialog ied = 
							new iFolderExceptionDialog(this, e);
						ied.Run();
						ied.Hide();
						ied.Destroy();
						ied = null;
					}
				}
				else
				{
					propsDialog.Present();
					propsDialog.CurrentPage = desiredPage;
				}
			}
		}

		private void AddiFolderHandler(object o,  EventArgs args)
		{
			CreateNewiFolder();
		}

		private void Migrate2xClickedHandler(object o, EventArgs args)
		{
			MigrationWindow migrationWindow = new MigrationWindow(this, ifws);
			migrationWindow.ShowAll();
			return;
		}

		private void ExportClicked( object o, EventArgs args)
		{
			Console.WriteLine(" export clicked");
			ExportKeysDialog export = new ExportKeysDialog();
			export.TransientFor = this;
			export.Run();
			export.Hide();
			export.Destroy();
		}

		private void ImportClicked( object o, EventArgs args)
		{
			ImportKeysDialog export = new ImportKeysDialog();
			export.TransientFor = this;
			export.Run();
			export.Hide();
			export.Destroy();
		}
		
		private void OnToggleViewServeriFoldersMenuItem(object o, EventArgs args)
		{
			if (ViewServeriFoldersMenuItem.Active)
				ShowAvailableiFolders();
			else
				HideAvailableiFolders();
		}
		
		private void ShowHideAllFoldersHandler(object o, EventArgs args)
		{
			if (bAvailableFoldersShowing)
				HideAvailableiFolders();
			else
				ShowAvailableiFolders();
		}
		
		private void RemoveiFolderHandler(object o,  EventArgs args)
		{
			RemoveSelectedFolderHandler();
		}

		private void DownloadAvailableiFolderHandler(object o, EventArgs args)
		{
			DownloadSelectedFolder();
		}
		
		private void DeleteFromServerHandler(object o, EventArgs args)
		{
			DeleteSelectedFolderFromServer();
		}
		
		private void RemoveMembershipHandler(object o, EventArgs args)
		{
			RemoveMembershipFromSelectedFolder();
		}
		
		private void OnCancelSearchButton(object o, EventArgs args)
		{
			// Set the text empty.  The Changed event handler will do the rest.
			SearchEntry.Text = "";
			SearchEntry.GrabFocus();
		}
		
		private void OnSearchEntryChanged(object o, EventArgs args)
		{
			if (searchTimeoutID != 0)
			{
				GLib.Source.Remove(searchTimeoutID);
				searchTimeoutID = 0;
			}

			if (SearchEntry.Text.Length > 0)
				CancelSearchButton.Sensitive = true;
			else
				CancelSearchButton.Sensitive = false;

			searchTimeoutID = GLib.Timeout.Add(
				500, new GLib.TimeoutHandler(SearchCallback));
		}
		
		private void OniFolderIconViewBackgroundClicked(object o, iFolderClickedArgs args)
		{
			iFoldersIconView.UnselectAll();

			if (args.Button == 3)
			{
				Menu menu = new Menu();
	
				MenuItem item_refresh =
					new MenuItem(Util.GS("Refresh"));
				menu.Append(item_refresh);
				item_refresh.Activated += new EventHandler(
					RefreshiFoldersHandler);
	
				menu.ShowAll();
				
				menu.Popup(null, null, null, 
					IntPtr.Zero, 3, 
					Gtk.Global.CurrentEventTime);
			}
		}
		
//		private void OniFolderDoubleClicked(object o, iFolderClickedArgs args)
//		{
//			iFolderHolder holder = args.Holder;
//			if (holder == null) return;	// Just in case
//
//			switch(args.Button)
//			{
//				case 1:	// left-click
//					if (holder.iFolder.IsSubscription)
//						DownloadSelectedFolder();
//					else
//						OpenSelectedFolder();
//					break;
//				default:
//					break;
//			}
//		}
		
		private void OniFolderActivated(object o, iFolderActivatedArgs args)
		{
			if (args.Holder == null || args.Holder.iFolder == null) return;
			
			if (args.Holder.iFolder.IsSubscription)
				DownloadSelectedFolder();
			else
				OpenSelectedFolder();
		}
		
		private void OniFolderIconViewKeyPress(object o, KeyPressEventArgs args)
		{
			switch(args.Event.Key)
			{
				case Gdk.Key.Delete:
					iFolderHolder holder = iFoldersIconView.SelectedFolder;
					if (holder != null)
					{
						if (holder.iFolder.IsSubscription)
						{
							DomainInformation domain =
								domainController.GetDomain(holder.iFolder.DomainID);
							if (domain == null || 
								domain.MemberUserID == holder.iFolder.OwnerID)
							{
								// The current user is the owner
								DeleteSelectedFolderFromServer();
							}
							else
							{
								RemoveMembershipFromSelectedFolder();
							}
						}
						else
						{
							RemoveSelectedFolderHandler();
						}
					}
					break;
				default:
					break;
			}
		}
		
		private void OniFolderClicked(object o, iFolderClickedArgs args)
		{
			iFolderHolder holder = args.Holder;
			if (holder == null) return;

			switch(args.Button)
			{
				case 3:	// right-click
					Menu menu = new Menu();

					if (holder.iFolder.IsSubscription)
					{
						MenuItem item_download =
							new MenuItem(Util.GS("Download..."));
						menu.Append(item_download);
						item_download.Activated += new EventHandler(
								DownloadAvailableiFolderHandler);

						menu.Append(new SeparatorMenuItem());

						DomainInformation domain =
							domainController.GetDomain(holder.iFolder.DomainID);
						if (domain == null || 
							domain.MemberUserID == holder.iFolder.OwnerID)
						{
							// The current user is the owner
							MenuItem item_delete = new MenuItem (
									Util.GS("Delete from server"));
							menu.Append (item_delete);
							item_delete.Activated += new EventHandler(
									DeleteFromServerHandler);
						}
						else
						{
							// The current user is not the owner
							MenuItem item_remove_membership = new MenuItem (
									Util.GS("Remove my membership"));
							menu.Append (item_remove_membership);
							item_remove_membership.Activated +=
								new EventHandler(
									RemoveMembershipHandler);
						}
					}
					else
					{
						MenuItem item_open =
							new MenuItem (Util.GS("Open..."));
						menu.Append (item_open);
						item_open.Activated += new EventHandler(
								OnOpenFolderMenu);

						menu.Append(new SeparatorMenuItem());

						if(holder.iFolder.HasConflicts)
						{
							MenuItem item_resolve = new MenuItem (
									Util.GS("Resolve conflicts..."));
							menu.Append (item_resolve);
							item_resolve.Activated += new EventHandler(
								OnResolveConflicts);
						
							menu.Append(new SeparatorMenuItem());
						}

						MenuItem item_sync =
							new MenuItem(Util.GS("Synchronize now"));
						menu.Append (item_sync);
						item_sync.Activated += new EventHandler(
								OnSynchronizeNow);

						MenuItem item_share =
							new MenuItem (Util.GS("Share with..."));
						menu.Append (item_share);
						item_share.Activated += new EventHandler(
								OnShareSynchronizedFolder);

						if (!holder.iFolder.Role.Equals("Master"))
						{
							MenuItem item_revert = new MenuItem (
									Util.GS("Revert to a normal folder"));
							menu.Append (item_revert);
							item_revert.Activated += new EventHandler(
									RemoveiFolderHandler);
						}
						else if(holder.iFolder.OwnerID !=
										holder.iFolder.CurrentUserID)
						{
							MenuItem item_delete = new MenuItem (
									Util.GS("Revert to a normal folder"));
							menu.Append (item_delete);
							item_delete.Activated += new EventHandler(
									RemoveiFolderHandler);
						}

						menu.Append(new SeparatorMenuItem());
	
						MenuItem item_properties =
							new MenuItem (Util.GS("Properties"));
						menu.Append (item_properties);
						item_properties.Activated +=
							new EventHandler(OnShowFolderProperties);
					}

					menu.ShowAll();

					menu.Popup(null, null, null, 
						IntPtr.Zero, 3, 
						Gtk.Global.CurrentEventTime);
					break;
				default:
					break;
			}
		}
		
		private void OniFolderIconViewSelectionChanged(object o, EventArgs args)
		{
			UpdateSensitivity();
		}
		
		private void OnIconViewDragMotion(object o, DragMotionArgs args)
		{
			Gdk.Drag.Status(args.Context, args.Context.SuggestedAction, args.Time);
			
			args.RetVal = true;
		}
		
		private void OnIconViewDragDrop(object o, DragDropArgs args)
		{
			args.RetVal = true;
		}
		
		private void OnIconViewDragDataReceived(object o, DragDataReceivedArgs args)
		{
			bool bFolderCreated = false;
			
			switch (args.Info)
			{
				case (uint) DragTargetType.iFolderID:
					string ifolderID =
						System.Text.Encoding.UTF8.GetString(args.SelectionData.Data);
					iFolderHolder holder = ifdata.GetAvailableiFolder(ifolderID);
					if (holder != null)
						DownloadiFolder(holder);
					break;
				case (uint) DragTargetType.UriList:
//					DomainInformation defaultDomain = domainController.GetDefaultDomain();
//					if (defaultDomain == null) return;
	
					UriList uriList = new UriList(args.SelectionData);
					if (uriList.Count == 1)
					{
						string path = null;
						try
						{
							path = uriList.ToLocalPaths()[0];
						}
						catch
						{
							return;
						}

						DomainInformation[] domains = domainController.GetDomains();
						if (domains.Length <= 0) return;	// FIXME: This should never happen.  Maybe alert the user?
						string domainID = domains[0].ID;	// Default to the first in the list
						DomainInformation defaultDomain = domainController.GetDefaultDomain();
						if (defaultDomain != null)
							domainID = defaultDomain.ID;
			
						DragCreateDialog cd = new DragCreateDialog(this, domains, domainID, path, ifws);
						cd.TransientFor = this;
				
						int rc = 0;
						do
						{
							rc = cd.Run();
							cd.Hide();
			
							if (rc == (int)ResponseType.Ok)
							{
								try
								{
									string selectedFolder = cd.iFolderPath.Trim();
									string selectedDomain = cd.DomainID;
									bool SSL = cd.ssl;
									string algorithm = cd.EncryptionAlgorithm;
				
									string parentDir = System.IO.Path.GetDirectoryName( selectedFolder );
									if ( ( parentDir == null ) || ( parentDir == String.Empty ) )
									{
										iFolderMsgDialog dg = new iFolderMsgDialog(
											this,
											iFolderMsgDialog.DialogType.Warning,
											iFolderMsgDialog.ButtonSet.Ok,
											"",
											Util.GS("Invalid folder specified"),
											Util.GS("An invalid folder was specified"));
										dg.Run();
										dg.Hide();
										dg.Destroy();
										continue;
									}
									
									iFolderHolder ifHolder = null;
									try
									{
										ifHolder = ifdata.CreateiFolder(selectedFolder,
													 selectedDomain, SSL, algorithm);
									}
									catch(Exception e)
									{
										if (DisplayCreateOrSetupException(e))
										{
											// Update the selectedFolder path
											continue;	// The function handled the exception
										}
									}
				
									if(ifHolder == null)
										throw new Exception("Simias returned null");
				
									// If we make it this far, we've succeeded and we don't
									// need to keep looping.
									rc = 0;
				
									// Save off the path so that the next time the user
									// creates an iFolder, we'll open it to the directory
									// they used last.
									Util.LastCreatedPath = ifHolder.iFolder.UnManagedPath;
				
									if ((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_CREATION))
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
												ClientConfig.KEY_SHOW_CREATION, false);
										}
					
										cd.Destroy();
										cd = null;
									}
								}
								catch (Exception e)
								{
									Console.WriteLine(e.Message);
									continue;
								}
							}
						}
						while(rc == (int)ResponseType.Ok);



					}

					break;
				default:
					break;
			}

			Gtk.Drag.Finish (args.Context, bFolderCreated, false, args.Time);
		}

		private void OnRealizeWidget(object o, EventArgs args)
		{
//			if (domainController.GetDomains().Length == 0)
//				WindowNotebook.CurrentPage = 0;
//			else
//			{
//				WindowNotebook.CurrentPage = 1;
				ShowAvailableiFolders();	// FIXME: Make this an automatic config setting that is remembered (i.e., if a user hides this and restarts iFolder, it is not shown)
//			}
			
			OniFolderIconViewSelectionChanged(null, EventArgs.Empty);
		}

		private void RefreshiFoldersHandler(object o, EventArgs args)
		{
			RefreshiFolders(true);
		}
		
		private void AccountsMenuItemHandler(object o, EventArgs args)
		{
			Util.ShowPrefsPage(1, simiasManager);
		}

		private void SyncLogMenuItemHandler(object o, EventArgs args)
		{
			Util.ShowLogWindow(simiasManager);
		}
		
		private void ShowColorPalette(object o, EventArgs args)
		{
			ColorPaletteDialog palette = new ColorPaletteDialog();
			palette.Run();
			palette.Hide();
			palette.Destroy();
		}



		private void CloseEventHandler(object o, EventArgs args)
		{
			CloseWindow();
		}

		private void QuitEventHandler(object o, EventArgs args)
		{
			Util.QuitiFolder();
		}
		
		
		private void ShowPreferencesHandler(object o, EventArgs args)
		{
			Util.ShowPrefsPage(0, simiasManager);
		}

		private void OnOpenFolderMenu(object o, EventArgs args)
		{
			OpenSelectedFolder();
		}

		private void OnShowFolderProperties(object o, EventArgs args)
		{
			ShowSelectedFolderProperties();
		}

		private void OnPropertiesDialogResponse(object o, ResponseArgs args)
		{
			iFolderPropertiesDialog propsDialog = (iFolderPropertiesDialog) o;

			switch(args.ResponseId)
			{
				case Gtk.ResponseType.Help:
					if (propsDialog != null)
					{
						if (propsDialog.CurrentPage == 0)
						{
							Util.ShowHelp("propifolders.html", this);
						}
						else if (propsDialog.CurrentPage == 1)
						{
							Util.ShowHelp("sharewith.html", this);
						}
						else
						{
							Util.ShowHelp(Util.HelpMainPage, this);
						}
					}
					break;
				default:
				{
					if(propsDialog != null)
					{
						propsDialog.Hide();
						propsDialog.Destroy();

						if (PropDialogs.ContainsKey(propsDialog.iFolder.ID))
							PropDialogs.Remove(propsDialog.iFolder.ID);

						propsDialog = null;
					}
					break;
				}
			}
		}

		private void OnConflictDialogResponse(object o, ResponseArgs args)
		{
			iFolderConflictDialog conflictDialog = (iFolderConflictDialog) o;
			if (args.ResponseId == ResponseType.Help)
				Util.ShowHelp("conflicts.html", this);
			else
			{
				if (conflictDialog != null)
				{
					conflictDialog.Hide();
					conflictDialog.Destroy();
					
					if (ConflictDialogs.ContainsKey(conflictDialog.iFolder.ID))
						ConflictDialogs.Remove(conflictDialog.iFolder.ID);

					conflictDialog = null;
				}
			}
		}

		private void OnHelpMenuItem(object o, EventArgs args)
		{
			Util.ShowHelp(Util.HelpMainPage, this);
		}

		private void OnRecoveryMenuItem(object o, EventArgs args)
		{
			Util.ShowHelp(Util.HelpMainPage, this);
		}

		private void OnResetPassMenuItem(object o, EventArgs args)
		{
			ResetPassPhraseDialog resetDialog = new ResetPassPhraseDialog();
			resetDialog.TransientFor = this;
			int result = resetDialog.Run();
			string DomainID = resetDialog.Domain;
			Console.WriteLine("DomainID is {0}", DomainID);
			string oldPassphrase = resetDialog.OldPassphrase;
			string newPassphrase = resetDialog.NewPassphrase;
			//string RAName = resetDialog.RAName;
			//string RAPublicKey = ;
			resetDialog.Hide();
			resetDialog.Destroy();
			if( result == (int) ResponseType.Ok)
			{
				Console.WriteLine("Calling Reset Passphrase");
				domainController.ReSetPassphrase(DomainID, oldPassphrase, newPassphrase, null, null);
			}
		}

		private void OnAbout(object o, EventArgs args)
		{
			Util.ShowAbout();
		}
		
		private void OnDomainAddedEvent(object sender, DomainEventArgs args)
		{
//			RefreshiFolders(true);

//			if (domainController.GetDomains().Length == 0)
//				WindowNotebook.CurrentPage = 0;
//			else if (WindowNotebook.CurrentPage != 1)
//			{
//				WindowNotebook.CurrentPage = 1;
				ShowAvailableiFolders();
//			}
			
			AddServerGroup(args.DomainID);
			
			RefilterServerGroups();
		}
		
		private void OnDomainDeletedEvent(object sender, DomainEventArgs args)
		{
//			RefreshiFolders(true);
			
//			if (domainController.GetDomains().Length == 0)
//				WindowNotebook.CurrentPage = 0;
//			else
//				WindowNotebook.CurrentPage = 1;
			
			if (serverGroups.ContainsKey(args.DomainID))
			{
				iFolderViewGroup group =
					(iFolderViewGroup)serverGroups[args.DomainID];
				iFoldersIconView.RemoveGroup(group);
				serverGroups.Remove(args.DomainID);
				group.Dispose();
				group = null;
			}
			
			if (serverGroupFilters.ContainsKey(args.DomainID))
			{
				serverGroupFilters.Remove(args.DomainID);
			}

			iFoldersIconView.UnselectAll();
			RefilterServerGroups();
		}
		
		private void OnDomainLoggedInEvent(object sender, DomainEventArgs args)
		{
			iFolderViewItem[] viewItems = localGroup.Items;
			
			foreach(iFolderViewItem item in viewItems)
			{
				iFolderHolder holder = item.Holder;
				if (args.DomainID == holder.iFolder.DomainID)
					holder.State = iFolderState.Initial;
			}

			// Update the item on the main thread
			GLib.Idle.Add(UpdateLocalViewItemsMainThread);
		}
		
		private void OnDomainLoggedOutEvent(object sender, DomainEventArgs args)
		{
			iFolderViewItem[] viewItems = localGroup.Items;
			
			foreach(iFolderViewItem item in viewItems)
			{
				iFolderHolder holder = item.Holder;
				if (args.DomainID == holder.iFolder.DomainID)
					holder.State = iFolderState.Disconnected;
			}

			// Update the item on the main thread
			GLib.Idle.Add(UpdateLocalViewItemsMainThread);
		}
		
		///
		/// Utility Methods
		///
		
		private void ShowAvailableiFolders()
		{
			foreach(iFolderViewGroup group in serverGroups.Values)
			{
				iFoldersIconView.AddGroup(group);
			}

			ShowHideAllFoldersButtonText.Markup =
				string.Format("<span>{0}</span>",
							  Util.GS("Hide available iFolders"));

			bAvailableFoldersShowing = true;
			ViewServeriFoldersMenuItem.Toggled -= new EventHandler(OnToggleViewServeriFoldersMenuItem);
			ViewServeriFoldersMenuItem.Active = true;
			ViewServeriFoldersMenuItem.Toggled += new EventHandler(OnToggleViewServeriFoldersMenuItem);
			RefreshiFolders(true);
		}
		
		private void HideAvailableiFolders()
		{
			// If the currently selected item is a subscription, unselect all
			// so the actions on the left don't still show up once the server
			// iFolders are hidden.
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null && holder.iFolder.IsSubscription)
				iFoldersIconView.UnselectAll();
			
			foreach(iFolderViewGroup group in serverGroups.Values)
			{
				iFoldersIconView.RemoveGroup(group);
			}

			ShowHideAllFoldersButtonText.Markup =
				string.Format("<span>{0}</span>",
							  Util.GS("View available iFolders"));

			bAvailableFoldersShowing = false;
			ViewServeriFoldersMenuItem.Toggled -= new EventHandler(OnToggleViewServeriFoldersMenuItem);
			ViewServeriFoldersMenuItem.Active = false;
			ViewServeriFoldersMenuItem.Toggled += new EventHandler(OnToggleViewServeriFoldersMenuItem);
		}
		
		private void DownloadSelectedFolder()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			DownloadiFolder(holder);
		}
		
		private void DownloadiFolder(iFolderHolder holder)
		{
                        if( holder.iFolder.encryptionAlgorithm != null && holder.iFolder.encryptionAlgorithm != "")
                        {
				if( IsPassPhraseAvailable(holder.iFolder.DomainID) == false)
				{
					return;
				}
                        }

			if (holder != null && holder.iFolder.IsSubscription)
			{
				string newPath = "";
                int rc = 0;

                do
                {
                    iFolderAcceptDialog iad = 
                            new iFolderAcceptDialog(holder.iFolder, Util.LastSetupPath);
                    iad.TransientFor = this;
                    rc = iad.Run();
                    newPath = ParseAndReplaceTildeInPath(iad.Path);
                    iad.Hide();
                    iad.Destroy();
                    if(rc != -5)
                        return;

                    try
                    {
                        ifdata.AcceptiFolderInvitation(
													   holder.iFolder.ID,
													   holder.iFolder.DomainID,
													   newPath);

						iFoldersIconView.UnselectAll();
                        rc = 0;

                        // Save off the path so that the next time the user
                        // opens the setup dialog, we'll open to the same
                        // directory
                        Util.LastSetupPath = newPath;

                    }
                    catch(Exception e)
                    {
                        DisplayCreateOrSetupException(e);
                    }
                }
                while(rc == -5);
			}
			RefreshiFolders(true);
		}
		
		private void DeleteSelectedFolderFromServer()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null && holder.iFolder.IsSubscription)
			{
				int rc = 0;

				rc = AskDeleteiFolder(holder);

				// User pressed OK?
				if(rc != -8)
					return;

				try
				{
					ifdata.DeleteiFolder(holder.iFolder.ID);
					iFoldersIconView.UnselectAll();
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
			RefreshiFolders(true);
		}
		
		private void RemoveMembershipFromSelectedFolder()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null && holder.iFolder.IsSubscription)
			{
				int rc = 0;

				rc = AskRemoveMembership(holder);

				// User pressed OK?
				if(rc != -8)
					return;

				try
				{
					ifdata.DeleteiFolder(holder.iFolder.ID);
					iFoldersIconView.UnselectAll();
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
		
		private bool SearchCallback()
		{
			SearchFolders();
			return false;
		}

		private void SearchFolders()
		{
			RefilterServerGroups();
		}
		
		private void RefilterServerGroups()
		{
			myiFoldersFilter.Refilter();
			foreach(iFolderViewGroup group in serverGroups.Values)
			{
				group.Model.Refilter();
			}
		}
		
		private void AddServerGroup(string domainID)
		{
			// Don't add it if it already exists
			if (serverGroups.ContainsKey(domainID)) return;
		
			DomainInformation domain = domainController.GetDomain(domainID);
			if (domain == null) return;
			
			iFolderServerFilter serverFilter =
				new iFolderServerFilter(domainID, SearchEntry);
			TreeModelFilter treeModelFilter = new TreeModelFilter(ifdata.iFolders, null);
			treeModelFilter.VisibleFunc = serverFilter.FilterFunc;

			iFolderViewGroup group =
				new iFolderViewGroup(
					string.Format(
						Util.GS("iFolders on {0}"),
						domain.Name),
					treeModelFilter,
					SearchEntry);
			serverGroups[domainID] = group;
			serverGroupFilters[domainID] = serverFilter;
			
			group.VisibleWhenEmpty = false;
			
			if (bAvailableFoldersShowing)
				iFoldersIconView.AddGroup(group);
		}

		private bool SynchronizedFoldersFilterFunc(TreeModel model, TreeIter iter)
		{
			ListStore ifolderListStore = model as ListStore;
			if (!ifolderListStore.IterIsValid(iter))
				return false;	// Prevent a bad TreeIter from causing problems

			iFolderHolder ifHolder = (iFolderHolder)ifolderListStore.GetValue(iter, 0);
			if (ifHolder != null && ifHolder.iFolder != null && !ifHolder.iFolder.IsSubscription)
			{
				string searchString = SearchEntry.Text;
				if (searchString != null)
				{
					searchString = searchString.Trim();
					if (searchString.Length > 0)
						searchString = searchString.ToLower();
				}
	
				if (searchString == null || searchString.Trim().Length == 0)
					return true;	// Include this
				else
				{
					// Search the iFolder's Name (for now)
					string name = ifHolder.iFolder.Name;
					if (name != null)
					{
						name = name.ToLower();
						if (name.IndexOf(searchString) >= 0)
							return true;
					}
				}
			}
			
			return false;
		}
		
		private void UpdateSensitivity()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;

			UpdateActionsSensitivity(holder);
			UpdateMenuSensitivity(holder);
		}
		
		private void UpdateActionsSensitivity(iFolderHolder holder)
		{
			if (holder == null)
			{
				SynchronizedFolderTasks.Visible = false;
			}
			else
			{
				if (holder.iFolder.IsSubscription)
				{
					// Hide the Local iFolder Buttons
					OpenSynchronizedFolderButton.Visible	= false;
					SynchronizeNowButton.Visible			= false;
					ShareSynchronizedFolderButton.Visible	= false;
					ViewFolderPropertiesButton.Visible		= false;
					ResolveConflictsButton.Visible			= false;
					RemoveiFolderButton.Visible	= false;

					// Show the Available iFolder Buttons
					DownloadAvailableiFolderButton.Visible	= true;

					DomainInformation domain =
						domainController.GetDomain(holder.iFolder.DomainID);
					if (domain == null || 
						domain.MemberUserID == holder.iFolder.OwnerID)
					{
						// The current user is the owner
						DeleteFromServerButton.Visible			= true;
						RemoveMembershipButton.Visible			= false;
					}
					else
					{
						// The current user is not the owner
						DeleteFromServerButton.Visible			= false;
						RemoveMembershipButton.Visible			= true;
					}
				}
				else
				{
					// Hide the Available iFolders Buttons
					DownloadAvailableiFolderButton.Visible	= false;
					DeleteFromServerButton.Visible			= false;
					RemoveMembershipButton.Visible			= false;
					
					// Show the Local iFolder Buttons
					OpenSynchronizedFolderButton.Visible	= true;
					SynchronizeNowButton.Visible			= true;
					ShareSynchronizedFolderButton.Visible	= true;
					ViewFolderPropertiesButton.Visible		= true;
					RemoveiFolderButton.Visible	= true;

					if (holder.iFolder.HasConflicts)
						ResolveConflictsButton.Visible = true;
					else
						ResolveConflictsButton.Visible = false;
				}

				SynchronizedFolderTasks.Visible = true;
				
				RemoveiFolderButton.Sensitive = true;
			}
		}
		
		private void UpdateMenuSensitivity(iFolderHolder holder)
		{
			if (holder != null)
			{
				if(	(holder.iFolder != null) &&
									(holder.iFolder.HasConflicts) )
				{
					ConflictMenuItem.Sensitive = true;
//					ConflictButton.Sensitive = true;
				}
				else
				{
					ConflictMenuItem.Sensitive = false;
//					ConflictButton.Sensitive = false;
				}

				if(!holder.iFolder.IsSubscription)
				{
					DownloadMenuItem.Sensitive = false;
					ShareMenuItem.Sensitive = true;
					OpenMenuItem.Sensitive = true;
					SyncNowMenuItem.Sensitive = true;
					if (holder.iFolder.Role.Equals("Master"))
						RevertMenuItem.Sensitive = false;
					else
						RevertMenuItem.Sensitive = true;
					PropMenuItem.Sensitive = true;

					DeleteMenuItem.Sensitive = false;
					RemoveMenuItem.Sensitive = false;
				}
				else
				{
					DownloadMenuItem.Sensitive = true;
					ShareMenuItem.Sensitive = false;
					OpenMenuItem.Sensitive = false;
					SyncNowMenuItem.Sensitive = false;
					RevertMenuItem.Sensitive = false;
					PropMenuItem.Sensitive = false;
	
					DomainInformation domain =
						domainController.GetDomain(holder.iFolder.DomainID);
					if (domain == null || 
						domain.MemberUserID == holder.iFolder.OwnerID)
					{
						// The current user is the owner
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
			}
			else
			{
				ShareMenuItem.Sensitive = false;
				OpenMenuItem.Sensitive = false;
				SyncNowMenuItem.Sensitive = false;
				ConflictMenuItem.Sensitive = false;
				RevertMenuItem.Sensitive = false;
				DeleteMenuItem.Sensitive = false;
				RemoveMenuItem.Sensitive = false;
				RemoveMenuItem.Visible = false;
				PropMenuItem.Sensitive = false;
				DownloadMenuItem.Sensitive = false;
			}
		}
		
		private void RefreshiFolders(bool bReadFromSimias)
		{
			ifdata.Refresh();
			domainController.CheckForNewiFolders();
		}

		public void CloseWindow()
		{
			int x;
			int y;

			this.GetPosition(out x, out y);
			
			lastXPos = x;
			lastYPos = y;
			
			this.Hide();
		}
		
		
		private void UpdateStatus(string message)
		{
			MainStatusBar.Pop (ctx);
			MainStatusBar.Push (ctx, message);
		}

		private void OpenSelectedFolder()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				try
				{
					Util.OpenInBrowser(holder.iFolder.UnManagedPath);
				}
				catch(Exception e)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						string.Format(Util.GS("Unable to open folder \"{0}\""), holder.iFolder.Name),
						Util.GS("iFolder could not open the Nautilus File Manager or the Konquerer File Manager."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
			}
		}

		private void ResolveSelectedFolderConflicts()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null)
				ResolveConflicts(holder);
		}
		
		private void ResolveConflicts(iFolderHolder holder)
		{
			if (holder == null) return;

			iFolderConflictDialog conflictDialog =
				(iFolderConflictDialog) ConflictDialogs[holder.iFolder.ID];
			if (conflictDialog == null)
			{
				try
				{
					conflictDialog =
						new iFolderConflictDialog(
							this, holder.iFolder, ifws, simws);
					conflictDialog.SetPosition(WindowPosition.Center);
					conflictDialog.Response +=
						new ResponseHandler(OnConflictDialogResponse);
					conflictDialog.ShowAll();
					
					ConflictDialogs[holder.iFolder.ID] = conflictDialog;
				}
				catch(Exception e)
				{
					if(conflictDialog != null)
					{
						conflictDialog.Hide();
						conflictDialog.Destroy();
						conflictDialog = null;
					}

					iFolderExceptionDialog ied = 
						new iFolderExceptionDialog(this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					ied = null;
				}
			}
			else
			{
				conflictDialog.Present();
			}
		}
		
		private void SyncSelectedFolder()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				try
				{
    				ifws.SynciFolderNow(holder.iFolder.ID);
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


		private void ShareSelectedFolder()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				ShowFolderProperties(holder, 1);
			}
		}

		private void ShowSelectedFolderProperties()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				ShowFolderProperties(holder, 0);
			}
		}

		private void RemoveSelectedFolderHandler()
		{
			iFolderHolder holder = iFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"",
					Util.GS("Revert this iFolder back to a normal folder?"),
					Util.GS("The folder will still be on your computer, but it will no longer synchronize with the iFolder Server."));

				CheckButton deleteFromServerCB;

				DomainInformation domain =
					domainController.GetDomain(holder.iFolder.DomainID);
				if (domain == null || domain.MemberUserID == holder.iFolder.OwnerID)
					deleteFromServerCB = new CheckButton(Util.GS("Also _delete this iFolder from the server"));
				else
					deleteFromServerCB = new CheckButton(Util.GS("Also _remove my membership from this iFolder"));
				
				dialog.ExtraWidget = deleteFromServerCB;

				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
						iFolderHolder subHolder =
							ifdata.RevertiFolder(holder.iFolder.ID);
						
						if (deleteFromServerCB.Active)
						{
							if (subHolder == null)
								ifdata.DeleteiFolder(holder.iFolder.ID);
							else
								ifdata.DeleteiFolder(subHolder.iFolder.ID);
						}
						
						iFoldersIconView.UnselectAll();
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
					
					UpdateSensitivity();
				}
			}
		}

		private int AskDeleteiFolder(iFolderHolder holder)
		{
			int rc = 0;

			iFolderMsgDialog dialog = new iFolderMsgDialog(
				this,
				iFolderMsgDialog.DialogType.Question,
				iFolderMsgDialog.ButtonSet.YesNo,
				"",
				string.Format(Util.GS("Delete \"{0}\" from the server?"),
							  holder.iFolder.Name),
				Util.GS("This deletes the iFolder and its files from the server."));
			rc = dialog.Run();
			dialog.Hide();
			dialog.Destroy();
			
			return rc;
		}

		private int AskRemoveMembership(iFolderHolder holder)
		{
			int rc = 0;

			iFolderMsgDialog dialog = new iFolderMsgDialog(
				this,
				iFolderMsgDialog.DialogType.Question,
				iFolderMsgDialog.ButtonSet.YesNo,
				"",
				string.Format(Util.GS("Remove your membership from \"{0}\"?"),
							  holder.iFolder.Name),
				Util.GS("This removes your membership from the iFolder and removes the iFolder from your list."));
			rc = dialog.Run();
			dialog.Hide();
			dialog.Destroy();
			
			return rc;
		}

		// update the data value in the iFolderTreeStore so the ifolder
		// will switch to one that has conflicts
//		public void iFolderHasConflicts(string iFolderID)
//		{
// FIXME: Make sure that adding conflict information into the iFolderData.iFolders is implemented
//			if(curiFolders.ContainsKey(iFolderID))
//			{
//				iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);
//
//				TreeIter iter = (TreeIter)curiFolders[iFolderID];
//
//				iFolderTreeStore.SetValue(iter, 0, ifHolder);
//			}

			// FIXME: let any property dialogs know that this iFolder has a conflict

//			UpdateSensitivity();
//		}

		private void CreateNewiFolder()
		{
			DomainInformation[] domains = domainController.GetDomains();
			if (domains.Length <= 0) return;	// FIXME: This should never happen.  Maybe alert the user?
			string domainID = domains[0].ID;	// Default to the first in the list
			DomainInformation defaultDomain = domainController.GetDefaultDomain();
			if (defaultDomain != null)
				domainID = defaultDomain.ID;
		/*
			ResetPassPhraseDialog resetDialog = new ResetPassPhraseDialog(domainID);
			resetDialog.TransientFor = this;
			resetDialog.Run();
			resetDialog.Hide();
			resetDialog.Destroy();
		*/
			CreateDialog cd = new CreateDialog(this, domains, domainID, Util.LastCreatedPath, ifws);
			cd.TransientFor = this;
	
			int rc = 0;
			do
			{
				rc = cd.Run();
				cd.Hide();

				if (rc == (int)ResponseType.Ok)
				{
					try
					{
						string selectedFolder = cd.iFolderPath.Trim();
						string selectedDomain = cd.DomainID;
					
						bool SSL = cd.ssl;
						string algorithm = cd.EncryptionAlgorithm;
	
						if (selectedFolder == String.Empty)
						{
							iFolderMsgDialog dg = new iFolderMsgDialog(
								this,
								iFolderMsgDialog.DialogType.Warning,
								iFolderMsgDialog.ButtonSet.Ok,
								"",
								Util.GS("Invalid folder specified"),
								Util.GS("An invalid folder was specified."));
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
								"",
								Util.GS("Invalid folder specified"),
								Util.GS("An invalid folder was specified"));
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
								"",
								Util.GS("Invalid folder specified"),
								Util.GS("The folder you've specified is invalid.  Please remove the trailing separator character (/) and try again."));
							dg.Run();
							dg.Hide();
							dg.Destroy();
							continue;
						}
	
						selectedFolder = ParseAndReplaceTildeInPath(selectedFolder);
	
						iFolderHolder ifHolder = null;
						try
						{
							if( algorithm != "")
							{
								// encryption is selected
								bool passPhraseStatus = false;
		                                                bool passphraseStatus = simws.IsPassPhraseSet(selectedDomain);
								if(passphraseStatus == true)
								{
									// if passphrase not given during login
									string passphrasecheck = 	simws.GetPassPhrase(selectedDomain);
									if( passphrasecheck == null || passphrasecheck =="")
									{
										Console.WriteLine(" passphrase not entered at login");
										passPhraseStatus = ShowVerifyDialog(selectedDomain, simws);
										/*
										// check for remember option
										bool rememberOption = simws.GetRememberOption(DomainID);
										if( rememberOption == false)
										{
											passPhraseStatus = ShowVerifyDialog( DomainID, simws);
										}
										else 
										{
											Console.WriteLine(" remember Option true. Checking for passphrase existence");
											string passphrasecheck,uid;
											simws.GetPassPhrase( DomainID, out uid, out passphrasecheck);
											if(passphrasecheck == null || passphrasecheck == "")
												passPhraseStatus = ShowVerifyDialog( DomainID, simws);
											else
												passPhraseStatus = true;
										}
										*/
									}
									else
									{
										passPhraseStatus = true;
									}
								}
								else
								{
									// if passphrase is not set
									passPhraseStatus = ShowEnterPassPhraseDialog(selectedDomain, simws);
								}
								if( passPhraseStatus == false)
								{
									// No Passphrase
									continue;
								}
							}
							ifHolder = ifdata.CreateiFolder(selectedFolder, selectedDomain, SSL, algorithm);
						}
						catch(Exception e)
						{
							if (DisplayCreateOrSetupException(e))
							{
								// Update the selectedFolder path
								cd.iFolderPath = selectedFolder;
								continue;	// The function handled the exception
							}
						}
	
						if(ifHolder == null)
							throw new Exception("Simias returned null");
	
						// If we make it this far, we've succeeded and we don't
						// need to keep looping.
						rc = 0;
	
						// Save off the path so that the next time the user
						// creates an iFolder, we'll open it to the directory
						// they used last.
						Util.LastCreatedPath = ifHolder.iFolder.UnManagedPath;
	
						if((bool)ClientConfig.Get(ClientConfig.KEY_SHOW_CREATION))
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
									ClientConfig.KEY_SHOW_CREATION, false);
							}
		
							cd.Destroy();
							cd = null;
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						continue;
					}
				}
			}
			while(rc == (int)ResponseType.Ok);
		}

		// Return true if we were able to determine the exception type.
		private bool DisplayCreateOrSetupException(Exception e)
		{
			string primaryText = null;
			string secondaryText = null;
			if (e.Message.IndexOf("Path did not exist") >= 0 || e.Message.IndexOf("URI scheme was not recognized") >= 0)
			{
				primaryText = Util.GS("Invalid folder specified");
				secondaryText = Util.GS("The folder you've specified does not exist.  Please select an existing folder and try again.");
			}
			else if (e.Message.IndexOf("PathExists") >= 0)
			{
				primaryText = Util.GS("A folder with the same name already exists.");
				secondaryText = Util.GS("The location you selected already contains a folder with the same name as this iFolder.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("RootOfDrivePath") >= 0)
			{
				primaryText = Util.GS("iFolders cannot exist at the drive level.");
				secondaryText = Util.GS("The location you selected is at the root of the drive.  Please select a location that is not at the root of a drive and try again.");
			}
			else if (e.Message.IndexOf("InvalidCharactersPath") >= 0)
			{
				primaryText = Util.GS("The selected location contains invalid characters.");
				secondaryText = Util.GS("The characters \\:*?\"<>| cannot be used in an iFolder. Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("AtOrInsideStorePath") >= 0)
			{
				primaryText = Util.GS("The selected location is inside the iFolder data folder.");
				secondaryText = Util.GS("The iFolder data folder is normally located in your home folder in the folder \".local/share\".  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("ContainsStorePath") >= 0)
			{
				primaryText = Util.GS("The selected location contains the iFolder data files.");
				secondaryText = Util.GS("The location you have selected contains the iFolder data files.  These are normally located in your home folder in the folder \".local/share\".  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("NotFixedDrivePath") >= 0)
			{
				primaryText = Util.GS("The selected location is on a network or non-physical drive.");
				secondaryText = Util.GS("iFolders must reside on a physical drive.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("SystemDirectoryPath") >= 0)
			{
				primaryText = Util.GS("The selected location contains a system folder.");
				secondaryText = Util.GS("System folders cannot be contained in an iFolder.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("SystemDrivePath") >= 0)
			{
				primaryText = Util.GS("The selected location is a system drive.");
				secondaryText = Util.GS("System drives cannot be contained in an iFolder.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("IncludesWinDirPath") >= 0)
			{
				primaryText = Util.GS("The selected location includes the Windows folder.");
				secondaryText = Util.GS("The Windows folder cannot be contained in an iFolder.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("IncludesProgFilesPath") >= 0)
			{
				primaryText = Util.GS("The selected location includes the Program Files folder.");
				secondaryText = Util.GS("The Program Files folder cannot be contained in an iFolder.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("DoesNotExistPath") >= 0)
			{
				primaryText = Util.GS("The selected location does not exist.");
				secondaryText = Util.GS("iFolders can only be created from folders that exist.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("NoReadRightsPath") >= 0)
			{
				primaryText = Util.GS("You do not have access to read files in the selected location.");
				secondaryText = Util.GS("iFolders can only be created from folders where you have access to read and write files.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("NoWriteRightsPath") >= 0)
			{
				primaryText = Util.GS("You do not have access to write files in the selected location.");
				secondaryText = Util.GS("iFolders can only be created from folders where you have access to read and write files.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("ContainsCollectionPath") >= 0)
			{
				primaryText = Util.GS("The selected location already contains an iFolder.");
				secondaryText = Util.GS("iFolders cannot exist inside other iFolders.  Please select a different location and try again.");
			}
			else if (e.Message.IndexOf("AtOrInsideCollectionPath") >= 0)
			{
				primaryText = Util.GS("The selected location is inside another iFolder.");
				secondaryText = Util.GS("iFolders cannot exist inside other iFolders.  Please select a different location and try again.");
			}
						
			if (primaryText != null)
			{
				iFolderMsgDialog dg = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Warning,
					iFolderMsgDialog.ButtonSet.Ok,
					"",
					primaryText,
					secondaryText);
					dg.Run();
					dg.Hide();
					dg.Destroy();
					
					return true;
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
			
			return false;
		}

		///
		/// Searches for a '~' character in the specified path and replaces it
		/// with the user's home directory
		private string ParseAndReplaceTildeInPath(string origPath)
		{
			string parsedString = origPath;
			if (origPath.IndexOf('~') >= 0)
			{
				string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				parsedString = origPath.Replace("~", homeDirectory);
			}
			
			return parsedString;
		}
		
		///
		/// Public Methods
		///

		public void DownloadiFolder(string ifolderID)
		{
			iFolderHolder holder = ifdata.GetiFolder(ifolderID);
			DownloadiFolder(holder);
		}

		public void ResolveConflicts(string ifolderID)
		{
			if (ifolderID == null) return;
			
			iFolderHolder holder = ifdata.GetiFolder(ifolderID);
			if (holder != null)
				ResolveConflicts(holder);
		}
		
		private void OniFolderSyncEvent(object o, CollectionSyncEventArgs args)
		{
			if (args == null || args.ID == null || args.Name == null)
				return;	// Prevent a null object exception

			switch(args.Action)
			{
				case Simias.Client.Event.Action.StartLocalSync:
				{
					if (args.Name != null && args.Name.StartsWith("POBox:"))
					{
						DomainInformation domain = domainController.GetPOBoxDomain(args.ID);
						if (domain != null)
							UpdateStatus(string.Format(Util.GS("Checking for new iFolders: {0}"), domain.Name));
						else
							UpdateStatus(Util.GS("Checking for new iFolders..."));
					}
					else
					{
						UpdateStatus(string.Format(Util.GS(
									"Checking for changes: {0}"), args.Name));
					}

					break;
				}
				case Simias.Client.Event.Action.StartSync:
				{
					if (args.Name != null && args.Name.StartsWith("POBox:"))
					{
						DomainInformation domain = domainController.GetPOBoxDomain(args.ID);
						if (domain != null)
							UpdateStatus(string.Format(Util.GS("Checking for new iFolders: {0}"), domain.Name));
						else
							UpdateStatus(Util.GS("Checking for new iFolders..."));
					}
					else
					{
						UpdateStatus(string.Format(Util.GS(
										"Synchronizing: {0}"), args.Name));
					}

					break;
				}
				case Simias.Client.Event.Action.StopSync:
				{
					if(SyncBar != null)
						SyncBar.Hide();

					UpdateStatus(Util.GS("Idle..."));
					break;
				}
			}

			// If the properties dialog is open, update it so it shows the
			// current status (last sync time, objects to sync, etc.)
			// FIXME: Register event handlers in iFolderPropertiesDialog to update things (Code to do this was deleted from here)
			// Maybe emit a NewConflictEvent here?
		}

		private void OniFolderFileSyncEvent(object o, FileSyncEventArgs args)
		{
			if (args == null || args.CollectionID == null || args.Name == null)
				return;	// Prevent a null object exception

			if (args.SizeRemaining == args.SizeToSync)
			{
				if (!args.Direction.Equals(Simias.Client.Event.Direction.Local))
				{
					if(SyncBar == null)
					{
						SyncBar = new ProgressBar();
						SyncBar.Orientation = Gtk.ProgressBarOrientation.LeftToRight;
						SyncBar.PulseStep = .01;
						MainStatusBar.PackEnd(SyncBar, false, true, 0);
					}

					// Init the progress bar now that we know we're synchronizing
					// to the server and not just checking local changes.
					SyncBar.Fraction = 0;
					SyncBar.Show();
				}

				switch (args.ObjectType)
				{
					case ObjectType.File:
						if (args.Delete)
							UpdateStatus(string.Format(
								Util.GS("Deleting file: {0}"),
								args.Name));
						else
						{
							switch (args.Direction)
							{
								case Simias.Client.Event.Direction.Uploading:
									UpdateStatus(string.Format(
										Util.GS("Uploading file: {0}"),
										args.Name));
									break;
								case Simias.Client.Event.Direction.Downloading:
									UpdateStatus(string.Format(
										Util.GS("Downloading file: {0}"),
										args.Name));
									break;
								case Simias.Client.Event.Direction.Local:
									UpdateStatus(string.Format(
										Util.GS("Found changes in file: {0}"),
										args.Name));
									break;
								default:
									UpdateStatus(string.Format(
										Util.GS("Synchronizing file: {0}"),
										args.Name));
									break;
							}
						}
						break;
					case ObjectType.Directory:
						if (args.Delete)
							UpdateStatus(string.Format(
								Util.GS("Deleting directory: {0}"),
								args.Name));
						else
						{
							switch (args.Direction)
							{
								case Simias.Client.Event.Direction.Uploading:
									UpdateStatus(string.Format(
										Util.GS("Uploading directory: {0}"),
										args.Name));
									break;
								case Simias.Client.Event.Direction.Downloading:
									UpdateStatus(string.Format(
										Util.GS("Downloading directory: {0}"),
										args.Name));
									break;
								case Simias.Client.Event.Direction.Local:
									UpdateStatus(string.Format(
										Util.GS("Found changes in directory: {0}"),
										args.Name));
									break;
								default:
									UpdateStatus(string.Format(
										Util.GS("Synchronizing directory: {0}"),
										args.Name));
									break;
							}
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
				if (SyncBar != null)
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
		}

		private bool ShowEnterPassPhraseDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;	
			EnterPassPhraseDialog epd = new EnterPassPhraseDialog(DomainID);
			try
			{
			do
			{
				result = epd.Run();
				epd.Hide();
                                if( result == (int)ResponseType.Cancel || result == (int) ResponseType.DeleteEvent)
                                        break;
				if( epd.PassPhrase != epd.RetypedPassPhrase )
				{
					Console.WriteLine("PassPhrases do not match");
					// show an error message
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("PassPhrase mismatch"),
						Util.GS("The PassPhrase and retyped Passphrase are not same"),
						Util.GS("Enter the passphrase again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
				}
				else
					break;
			}while( result != (int)ResponseType.Cancel);
                        if( result == (int)ResponseType.Cancel || result ==(int)ResponseType.DeleteEvent)
                        {
                                status = false;
                                simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
                        }
			
			if( epd.PassPhrase == epd.RetypedPassPhrase)
			{
				// Check the recovery agent
				string publicKey = "";
				Status passPhraseStatus = simws.SetPassPhrase( DomainID, epd.PassPhrase, epd.RecoveryAgent, publicKey);
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					status = true;
					simws.StorePassPhrase( DomainID, epd.PassPhrase, CredentialType.Basic, epd.ShouldSavePassPhrase);
				}
				else 
				{
					// error setting the passphrase
					status = false;
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("Error setting the Passphrase"),
						Util.GS("Unable to set the passphrase"),
						Util.GS("Try again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;
				}
			}
			}
			catch(Exception e)
			{
				return false;
			}
			return status;
		}

		private bool ShowVerifyDialog(string DomainID, SimiasWebService simws)
		{
			bool status = false;
			int result;
			Status passPhraseStatus= null;
			VerifyPassPhraseDialog vpd = new VerifyPassPhraseDialog();
			// vpd.TransientFor = this;
			try
			{
			do
			{
				result = vpd.Run();
				vpd.Hide();
				// Verify PassPhrase..  If correct store passphrase and set a local property..
				if( result == (int)ResponseType.Ok)
					passPhraseStatus =  simws.ValidatePassPhrase(DomainID, vpd.PassPhrase);
				if( passPhraseStatus != null)
				{
					if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
					{
						// Display an error Message
						Console.WriteLine("Invalid Passphrase");
						iFolderMsgDialog dialog = new iFolderMsgDialog(
							null,
							iFolderMsgDialog.DialogType.Error,
							iFolderMsgDialog.ButtonSet.None,
							Util.GS("Invalid Passphrase"),
							Util.GS("The Passphrase entered is invalid"),
							Util.GS("Please re-enter the passphrase"));
							dialog.Run();
							dialog.Hide();
							dialog.Destroy();
							dialog = null;
						passPhraseStatus = null;
					}
					else if(passPhraseStatus.statusCode == StatusCodes.Success)
						break;
				}
			}while( result != (int)ResponseType.Cancel && result !=(int)ResponseType.DeleteEvent);
			if( result == (int)ResponseType.Cancel || result == (int)ResponseType.DeleteEvent)
			{
				try
				{
					simws.StorePassPhrase(DomainID, "", CredentialType.Basic, false);
					status = false;
				}
				catch(Exception e)
				{
					return false;
				}
			}
			else if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success)
			{
				try
				{
					simws.StorePassPhrase( DomainID, vpd.PassPhrase, CredentialType.Basic, vpd.ShouldSavePassPhrase);
					status = true;
				}
				catch(Exception ex) 
				{
					return false;
				}
			}
			}
			catch(Exception e)
			{
				return false;
			}
//			return false;
			return status;
		}

		private bool IsPassPhraseAvailable(string selectedDomain)
		{
			bool passPhraseStatus = false;;
			bool passphraseStatus = simws.IsPassPhraseSet(selectedDomain);
			if(passphraseStatus == true)
			{
				// if passphrase not given during login
				string passphrasecheck = simws.GetPassPhrase(selectedDomain);
				if( passphrasecheck == null || passphrasecheck =="")
				{
					passPhraseStatus = ShowVerifyDialog(selectedDomain, simws);
				}
				else
				{
					passPhraseStatus = true;
				}
			}
			else
			{
				// if passphrase is not set
				passPhraseStatus = ShowEnterPassPhraseDialog(selectedDomain, simws);
			}
			return passPhraseStatus;
		}
	}



///
/// This UrlList class comes directly from F-Spot written by Miguel de Icaza
///
public class UriList : ArrayList {
	private void LoadFromString (string data) {
		//string [] items = System.Text.RegularExpressions.Regex.Split ("\n", data);
		string [] items = data.Split ('\n');
		
		foreach (String i in items) {
			if (!i.StartsWith ("#")) {
				Uri uri;
				String s = i;

				if (i.EndsWith ("\r")) {
					s = i.Substring (0, i.Length - 1);
//					Console.WriteLine ("uri = {0}", s);
				}
				
				try {
					uri = new Uri (s);
				} catch {
					continue;
				}
				Add (uri);
			}
		}
	}

	static char[] CharsToQuote = { ';', '?', ':', '@', '&', '=', '$', ',', '#' };

	public static Uri PathToFileUri (string path)
	{
		path = Path.GetFullPath (path);

		StringBuilder builder = new StringBuilder ();
		builder.Append (Uri.UriSchemeFile);
		builder.Append (Uri.SchemeDelimiter);

		int i;
		while ((i = path.IndexOfAny (CharsToQuote)) != -1) {
			if (i > 0)
				builder.Append (path.Substring (0, i));
			builder.Append (Uri.HexEscape (path [i]));
			path = path.Substring (i+1);
		}
		builder.Append (path);

		return new Uri (builder.ToString (), true);
	}

	public UriList (string [] uris)
	{	
		// FIXME this is so lame do real chacking at some point
		foreach (string str in uris) {
			Uri uri;

			if (File.Exists (str) || Directory.Exists (str))
				uri = PathToFileUri (str);
			else 
				uri = new Uri (str);
			
			Add (uri);
		}
	}

	public UriList (string data) {
		LoadFromString (data);
	}
	
	public UriList (Gtk.SelectionData selection) 
	{
		// FIXME this should check the atom etc.
		LoadFromString (System.Text.Encoding.UTF8.GetString (selection.Data));
	}

	public override string ToString () {
		StringBuilder list = new StringBuilder ();

		foreach (Uri uri in this) {
			if (uri == null)
				break;

			list.Append (uri.ToString () + "\r\n");
		}

		return list.ToString ();
	}

	public string [] ToLocalPaths () {
		int count = 0;
		foreach (Uri uri in this) {
			if (uri.IsFile)
				count++;
		}
		
		String [] paths = new String [count];
		count = 0;
		foreach (Uri uri in this) {
			if (uri.IsFile)
				paths[count++] = uri.LocalPath;
		}
		return paths;
	}
	
}

	///
	/// Debug: ColorPaletteDialog
	///
	public class ColorPaletteDialog : Dialog
	{
		public ColorPaletteDialog() : base()
		{
			this.VBox.Add(CreateWidgets());
			
			this.AddButton("_Close", ResponseType.Close);
		}
		
		private Widget CreateWidgets()
		{
			VBox vbox = new VBox(false, 0);
			
			///
			/// Base
			///
			Label l = new Label("<span size=\"large\">Base Colors</span>");
			vbox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.Xalign = 0;
			
			vbox.PackStart(CreateColorBox("Normal", this.Style.Base(StateType.Normal)));
			vbox.PackStart(CreateColorBox("Active", this.Style.Base(StateType.Active)));
			vbox.PackStart(CreateColorBox("Prelight", this.Style.Base(StateType.Prelight)));
			vbox.PackStart(CreateColorBox("Selected", this.Style.Base(StateType.Selected)));
			vbox.PackStart(CreateColorBox("Insensitive", this.Style.Base(StateType.Insensitive)));

			///
			/// Background
			///
			l = new Label("<span size=\"large\">Background Colors</span>");
			vbox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.Xalign = 0;
			
			vbox.PackStart(CreateColorBox("Normal", this.Style.Background(StateType.Normal)));
			vbox.PackStart(CreateColorBox("Active", this.Style.Background(StateType.Active)));
			vbox.PackStart(CreateColorBox("Prelight", this.Style.Background(StateType.Prelight)));
			vbox.PackStart(CreateColorBox("Selected", this.Style.Background(StateType.Selected)));
			vbox.PackStart(CreateColorBox("Insensitive", this.Style.Background(StateType.Insensitive)));

			///
			/// Foreground
			///
			l = new Label("<span size=\"large\">Foreground Colors</span>");
			vbox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.Xalign = 0;
			
			vbox.PackStart(CreateColorBox("Normal", this.Style.Foreground(StateType.Normal)));
			vbox.PackStart(CreateColorBox("Active", this.Style.Foreground(StateType.Active)));
			vbox.PackStart(CreateColorBox("Prelight", this.Style.Foreground(StateType.Prelight)));
			vbox.PackStart(CreateColorBox("Selected", this.Style.Foreground(StateType.Selected)));
			vbox.PackStart(CreateColorBox("Insensitive", this.Style.Foreground(StateType.Insensitive)));

			vbox.ShowAll();
			
			return vbox;
		}
		
		private Widget CreateColorBox(string name, Gdk.Color color)
		{
			EventBox eb = new EventBox();
			eb.ModifyBg(StateType.Normal, color);
			
			Label l = new Label(name);
			eb.Add(l);
			l.Show();
			
			return eb;
		}
	}

	public class iFolderServerFilter
	{
		private string	domainID;
		private Entry	searchEntry;

		public iFolderServerFilter(string domainID, Entry searchEntry)
		{
			this.domainID = domainID;
			this.searchEntry = searchEntry;
		}
		
		public bool FilterFunc(TreeModel model, TreeIter iter)
		{
			iFolderHolder ifHolder = (iFolderHolder)model.GetValue(iter, 0);
			if (ifHolder == null || ifHolder.iFolder == null || ifHolder.iFolder.DomainID == null) return false;

			if (ifHolder.iFolder.IsSubscription
				&& ifHolder.iFolder.DomainID == domainID)
			{
				string searchString = searchEntry.Text;
				if (searchString != null)
				{
					searchString = searchString.Trim();
					if (searchString.Length > 0)
						searchString = searchString.ToLower();
				}
	
				if (searchString == null || searchString.Trim().Length == 0)
					return true;	// Include this
				else
				{
					// Search the iFolder's Name (for now)
					string name = ifHolder.iFolder.Name;
					if (name != null)
					{
						name = name.ToLower();
						if (name.IndexOf(searchString) >= 0)
							return true;
					}
				}
			}
			
			return false;
		}
	}
}
