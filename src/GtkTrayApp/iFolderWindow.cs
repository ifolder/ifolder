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

		public iFolderHolder(iFolderWeb ifolder)
		{
			this.ifolder = ifolder;
			isSyncing = false;
			syncSuccessful = true;
		}

		public iFolderWeb iFolder
		{
			get{ return ifolder; }
			set{ this.ifolder = value; }
		}

		public bool IsSyncing
		{
			get{ return isSyncing; }
			set{ this.isSyncing = value; }
		}

		public bool SyncSuccessful
		{
			get{ return syncSuccessful; }
			set{ this.syncSuccessful = value; }
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
		private string				SyncFileName = "";

		private iFolderWebService	iFolderWS;
		private Gdk.Pixbuf			iFolderPixBuf;
		private Gdk.Pixbuf			ServeriFolderPixBuf;
		private Gdk.Pixbuf			ConflictPixBuf;

		private Statusbar			MainStatusBar;
		private ProgressBar			SyncBar;
		private Gtk.Notebook		MainNoteBook;
		private Gtk.TreeView		iFolderTreeView;
		private Gtk.ListStore		iFolderTreeStore;

		// Preferences widgets
		private Gtk.CheckButton			AutoSyncCheckButton;
		private Gtk.SpinButton			SyncSpinButton;
		private Gtk.Label				SyncUnitsLabel;

		private Gtk.CheckButton			StartAtLoginButton;
		private Gtk.CheckButton			ShowConfirmationButton; 
		private Gtk.CheckButton			UseProxyButton; 
		private Gtk.Entry				ProxyHostEntry;
		private Gtk.SpinButton			ProxyPortSpinButton;
		private Gtk.Label				ProxyHostLabel;
		private Gtk.Label				ProxyPortLabel;

		private ImageMenuItem		CreateMenuItem;
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
		private Image				 iFolderBanner;
		private Image				 iFolderScaledBanner;
		private Gdk.Pixbuf			 ScaledPixbuf;

		private iFolderSettings		ifSettings;
		private Hashtable			curiFolders;

		private TreeView			LogTreeView;
		private ListStore			LogTreeStore;
		private Button				SaveButton;
		private Button				ClearButton;

		public iFolderSettings GlobalSettings
		{
			set
			{
				if(value.HaveEnterprise != ifSettings.HaveEnterprise)
				{
					ifSettings = value;
					if(value.HaveEnterprise = true)
					{
						MainNoteBook.AppendPage( CreateEnterprisePage(),
											new Label(Util.GS("Server")));
						MainNoteBook.ShowAll();
					}
				}
				ifSettings = value;
				RefreshWidgets();
			}
		}




		/// <summary>
		/// Default constructor for iFolderWindow
		/// </summary>
		public iFolderWindow(iFolderWebService ifws, iFolderSettings settings)
			: base ("iFolder")
		{
			if(ifws == null)
				throw new ApplicationException("iFolderWebServices was null");
			iFolderWS = ifws;
			ifSettings = settings;
			curiFolders = new Hashtable();
			CreateWidgets();
			RefreshWidgets();
		}




		/// <summary>
		/// Refresh the values that are in the widgets and add or remove the
		/// Enterprise tab depending on our state
		/// </summary>
		private void RefreshWidgets()
		{
			//------------------------------
			// Setup all of the default values
			//------------------------------
			if(ifSettings.DisplayConfirmation)
				ShowConfirmationButton.Active = true;
			else
				ShowConfirmationButton.Active = false;

			StartAtLoginButton.Active = false;
			StartAtLoginButton.Sensitive = false;


			SyncSpinButton.Value = ifSettings.DefaultSyncInterval;
			if(SyncSpinButton.Value == 0)
			{
				AutoSyncCheckButton.Active = false;
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false; 
			}
			else
			{
				AutoSyncCheckButton.Active = true;
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
			}

			if(AutoSyncCheckButton.Active == true)
			{
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
			}
			else
			{
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false;
			}


			if(ifSettings.UseProxy)
			{
				ProxyHostEntry.Sensitive = true;
				ProxyPortSpinButton.Sensitive = true;
				ProxyHostLabel.Sensitive = true;
				ProxyPortLabel.Sensitive = true;
				UseProxyButton.Active = true; 
				ProxyHostEntry.Text = ifSettings.ProxyHost;
				ProxyPortSpinButton.Value = ifSettings.ProxyPort;
			}
			else
			{
				ProxyHostEntry.Sensitive = false;
				ProxyPortSpinButton.Sensitive = false;
				ProxyHostLabel.Sensitive = false;
				ProxyPortLabel.Sensitive = false;
				UseProxyButton.Active = false; 
			}
		}




		/// <summary>
		/// Setup the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (540, 480);
			this.DeleteEvent += new DeleteEventHandler (WindowDelete);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

			//-----------------------------
			// Create the menubar
			//-----------------------------
			AccelGroup accelGroup = new AccelGroup ();
			this.AddAccelGroup (accelGroup);
			
			MenuBar menubar = CreateMenu ();
			vbox.PackStart (menubar, false, false, 0);


			//-----------------------------
			// Add iFolderGraphic
			//-----------------------------
			HBox imagebox = new HBox();
			imagebox.Spacing = 0;
			iFolderBanner = new Image(
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
			imagebox.PackStart(iFolderBanner, false, false, 0);

			ScaledPixbuf = 
				new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner-scaled.png"));
			iFolderScaledBanner = new Image(ScaledPixbuf);
			iFolderScaledBanner.ExposeEvent += 
					new ExposeEventHandler(OnBannerExposed);
			imagebox.PackStart(iFolderScaledBanner, true, true, 0);
			vbox.PackStart (imagebox, false, true, 0);

			//-----------------------------
			// Create Tabs
			//-----------------------------
			MainNoteBook = new Notebook();
//			MainNoteBook.BorderWidth = 10;
			MainNoteBook.AppendPage(	CreateiFoldersPage(), 
										new Label(Util.GS("iFolders")));
			MainNoteBook.AppendPage( CreatePreferencesPage(),
										new Label(Util.GS("Preferences")));
			MainNoteBook.AppendPage( CreateLogPage(),
										new Label(Util.GS("Sync Log")));
			if(ifSettings.HaveEnterprise)
			{
				MainNoteBook.AppendPage( CreateEnterprisePage(),
											new Label(Util.GS("Server")));
			}
			vbox.PackStart(MainNoteBook, true, true, 0);
			MainNoteBook.SwitchPage += 
					new SwitchPageHandler(OnSwitchPage);


			//-----------------------------
			// Create Status Bar
			//-----------------------------
			MainStatusBar = new Statusbar ();
			UpdateStatus(Util.GS("Idle..."));

			vbox.PackStart (MainStatusBar, false, false, 0);

			//-----------------------------
			// Set Menu Status
			//-----------------------------
			CreateMenuItem.Sensitive = true;
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

			// Setup an event to refresh when the window is
			// being drawn
			this.Realized += new EventHandler(OnRealizeWidget);
		}




		private void OnBannerExposed(object o, ExposeEventArgs args)
		{
			if(args.Event.Count > 0)
				return;

			Gdk.Pixbuf spb = 
				ScaledPixbuf.ScaleSimple(iFolderScaledBanner.Allocation.Width,
										iFolderScaledBanner.Allocation.Height,
										Gdk.InterpType.Nearest);

			Gdk.GC gc = new Gdk.GC(iFolderScaledBanner.GdkWindow);

			spb.RenderToDrawable(iFolderScaledBanner.GdkWindow,
											gc,
											0, 0,
											args.Event.Area.X,
											args.Event.Area.Y,
											args.Event.Area.Width,
											args.Event.Area.Height,
											Gdk.RgbDither.Normal,
											0, 0);
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

			CreateMenuItem = new ImageMenuItem (Util.GS("C_reate"));
			CreateMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder.png")));
			iFolderMenu.Append(CreateMenuItem);
			CreateMenuItem.AddAccelerator("activate", agrp,
				new AccelKey(Gdk.Key.N, Gdk.ModifierType.ControlMask,
								AccelFlags.Visible));
			CreateMenuItem.Activated += new EventHandler(OnCreateiFolder);

			SetupMenuItem =
				new MenuItem (Util.GS("_Setup iFolder"));
			iFolderMenu.Append(SetupMenuItem);
			SetupMenuItem.Activated += new EventHandler(OnSetupiFolder);

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
			ShareMenuItem.Activated += new EventHandler(OnShareProperties);

			ConflictMenuItem = new MenuItem (Util.GS("Re_solve conflicts"));
			iFolderMenu.Append(ConflictMenuItem);
			ConflictMenuItem.Activated += new EventHandler(OnResolveConflicts);

			SyncNowMenuItem = new MenuItem(Util.GS("Sync _now"));
			iFolderMenu.Append(SyncNowMenuItem);
			SyncNowMenuItem.Activated += new EventHandler(OnSyncNow);

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
			CloseMenuItem.Activated += new EventHandler(OnCloseWindow);

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
					new EventHandler(RefreshiFolderTreeView);

			MenuItem ViewMenuItem = new MenuItem(Util.GS("_View"));
			ViewMenuItem.Submenu = ViewMenu;
			menubar.Append(ViewMenuItem);


			//----------------------------
			// View Menu
			//----------------------------
			Menu HelpMenu = new Menu();

			HelpMenuItem = 
				new ImageMenuItem(Stock.Help, agrp);
			HelpMenu.Append(HelpMenuItem);
			HelpMenuItem.Activated += new EventHandler(OnHelpMenuItem);

			AboutMenuItem = new ImageMenuItem(Util.GS("A_bout"));
			AboutMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder.png")));
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
		private Widget CreateiFoldersPage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = 10;
			vbox.BorderWidth = Util.DefaultBorderWidth;
			
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


			// Create an HBox that is not homogeneous and spacing of 10 
			HBox hbox = new HBox(false, 10);
			// Create another HBox (in case we add more buttons)
			// so they will line up to the right and be the same
			// widgth
			HBox leftHBox = new HBox(true, 10);
			Button create_button = new Button(Util.GS("_Create"));

			create_button.Clicked += new EventHandler(OnCreateiFolder);
			
			leftHBox.PackEnd(create_button);
			hbox.PackEnd(leftHBox, false, false, 0);
			vbox.PackStart(hbox, false, false, 0);
		
			return vbox;
		}
	



		/// <summary>
		/// Creates the Preferences Page
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreatePreferencesPage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = Util.SectionSpacing;
			vbox.BorderWidth = Util.DefaultBorderWidth;

			//------------------------------
			// Application Settings
			//------------------------------
			// create a section box
			VBox appSectionBox = new VBox();
			appSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(appSectionBox, false, true, 0);
			Label appSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Application") +
												"</span>");
			appSectionLabel.UseMarkup = true;
			appSectionLabel.Xalign = 0;
			appSectionBox.PackStart(appSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox appSpacerBox = new HBox();
			appSectionBox.PackStart(appSpacerBox, false, true, 0);
			Label appSpaceLabel = new Label("    "); // four spaces
			appSpacerBox.PackStart(appSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox appWidgetBox = new VBox();
			appSpacerBox.PackStart(appWidgetBox, false, true, 0);

			StartAtLoginButton = 
				new CheckButton(Util.GS("Start iFolder when logging in to the desktop"));
			appWidgetBox.PackStart(StartAtLoginButton, false, true, 0);

			ShowConfirmationButton = 
				new CheckButton(Util.GS("Show Confirmation dialog when creating iFolders"));
			appWidgetBox.PackStart(ShowConfirmationButton, false, true, 0);
			ShowConfirmationButton.Toggled += 
						new EventHandler(OnShowConfButton);



			//------------------------------
			// Sync Settings
			//------------------------------
			// create a section box
			VBox syncSectionBox = new VBox();
			syncSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(syncSectionBox, false, true, 0);
			Label syncSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Synchronization") +
												"</span>");
			syncSectionLabel.UseMarkup = true;
			syncSectionLabel.Xalign = 0;
			syncSectionBox.PackStart(syncSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox syncSpacerBox = new HBox();
			syncSectionBox.PackStart(syncSpacerBox, false, true, 0);
			Label syncSpaceLabel = new Label("    "); // four spaces
			syncSpacerBox.PackStart(syncSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox syncWidgetBox = new VBox();
			syncSpacerBox.PackStart(syncWidgetBox, false, true, 0);
			syncWidgetBox.Spacing = 10;


			Label syncHelpLabel = new Label(Util.GS("Specify the default Sync interval for synchronizing your iFolders with the host.  To specify a different Sync interval for an individual iFolder, use the iFolder's Properties dialog."));
			syncHelpLabel.LineWrap = true;
			syncHelpLabel.Xalign = 0;
			syncWidgetBox.PackStart(syncHelpLabel, false, true, 0);

			HBox syncHBox = new HBox();
			syncWidgetBox.PackStart(syncHBox, false, true, 0);
			syncHBox.Spacing = 10;
			AutoSyncCheckButton = 
					new CheckButton(Util.GS("Sync to host every:"));
			AutoSyncCheckButton.Toggled += new EventHandler(OnAutoSyncButton);
			syncHBox.PackStart(AutoSyncCheckButton, false, false, 0);
			SyncSpinButton = new SpinButton(0, 99999, 1);
			SyncSpinButton.ValueChanged += 
					new EventHandler(OnSyncIntervalChanged);
			syncHBox.PackStart(SyncSpinButton, false, false, 0);
			SyncUnitsLabel = new Label(Util.GS("seconds"));
			SyncUnitsLabel.Xalign = 0;
			syncHBox.PackEnd(SyncUnitsLabel, true, true, 0);



			//------------------------------
			// Proxy Frame
			//------------------------------
			// create a section box
			VBox proxySectionBox = new VBox();
			proxySectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(proxySectionBox, true, true, 0);
			Label proxySectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Proxy") +
												"</span>");
			proxySectionLabel.UseMarkup = true;
			proxySectionLabel.Xalign = 0;
			proxySectionBox.PackStart(proxySectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox proxySpacerBox = new HBox();
			proxySectionBox.PackStart(proxySpacerBox, false, true, 0);
			Label proxySpaceLabel = new Label("    "); // four spaces
			proxySpacerBox.PackStart(proxySpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox proxyWidgetBox = new VBox();
			proxySpacerBox.PackStart(proxyWidgetBox, true, true, 0);
			proxyWidgetBox.Spacing = 5;


			UseProxyButton = 
				new CheckButton(Util.GS("Use this proxy server to sync iFolders with the host"));
			proxyWidgetBox.PackStart(UseProxyButton, false, true, 0);
			UseProxyButton.Toggled += new EventHandler(OnUseProxyButton);


			HBox pSettingBox = new HBox();
			pSettingBox.Spacing = 10;
			proxyWidgetBox.PackStart(pSettingBox, true, true, 0);

			ProxyHostLabel = new Label(Util.GS("Proxy host:"));
			pSettingBox.PackStart(ProxyHostLabel, false, true, 0);
			ProxyHostEntry = new Entry();
			ProxyHostEntry.Changed += new EventHandler(OnProxySettingsChanged);

			pSettingBox.PackStart(ProxyHostEntry, true, true, 0);
			ProxyPortLabel = new Label(Util.GS("Port:"));
			pSettingBox.PackStart(ProxyPortLabel, false, true, 0);
			ProxyPortSpinButton = new SpinButton(0, 99999, 1);

			ProxyPortSpinButton.ValueChanged += 
					new EventHandler(OnProxySettingsChanged);
			pSettingBox.PackStart(ProxyPortSpinButton, false, true, 0);


			// Disable all proxy stuff right now
			proxySectionLabel.Sensitive = false;
			UseProxyButton.Sensitive = false;
			ProxyHostLabel.Sensitive = false;
			ProxyHostEntry.Sensitive = false;
			ProxyPortSpinButton.Sensitive = false;
			ProxyPortLabel.Sensitive = false;

			return vbox;
		}




		/// <summary>
		/// Creates the Enterprise Page
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreateEnterprisePage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = Util.SectionSpacing;
			vbox.BorderWidth = Util.DefaultBorderWidth;


			//------------------------------
			// Server Information
			//------------------------------
			// create a section box
			VBox srvSectionBox = new VBox();
			srvSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(srvSectionBox, false, true, 0);
			Label srvSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Server Information") +
												"</span>");
			srvSectionLabel.UseMarkup = true;
			srvSectionLabel.Xalign = 0;
			srvSectionBox.PackStart(srvSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox srvSpacerBox = new HBox();
			srvSpacerBox.Spacing = 10;
			srvSectionBox.PackStart(srvSpacerBox, false, true, 0);
			Label srvSpaceLabel = new Label("");
			srvSpacerBox.PackStart(srvSpaceLabel, false, true, 0);

			// create a vbox to actually place the widgets in for section
			VBox srvWidgetBox = new VBox();
			srvSpacerBox.PackStart(srvWidgetBox, true, true, 0);

			// create a table to hold the values
			Table srvTable = new Table(3,2,false);
			srvWidgetBox.PackStart(srvTable, true, true, 0);
			srvTable.ColumnSpacing = 20;
			srvTable.RowSpacing = 5;

			Label usrNameLabel = new Label(Util.GS("User name:"));
			usrNameLabel.Xalign = 0;
			srvTable.Attach(usrNameLabel, 0,1,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usrNameValue = new Label(ifSettings.CurrentUserName);
			usrNameValue.Xalign = 0;
			srvTable.Attach(usrNameValue, 1,2,0,1);


			Label srvNameLabel = new Label(Util.GS("iFolder server:"));
			srvNameLabel.Xalign = 0;
			srvTable.Attach(srvNameLabel, 0,1,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label srvNameValue = new Label(ifSettings.EnterpriseName);
			srvNameValue.Xalign = 0;
			srvTable.Attach(srvNameValue, 1,2,1,2);

			Label srvDescLabel = new Label(Util.GS("Server description:"));
			srvDescLabel.Xalign = 0;
			srvDescLabel.Yalign = 0;
			srvTable.Attach(srvDescLabel, 0,1,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 
					AttachOptions.Fill,0,0);

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			TextView srvDescValue = new TextView();
			if(ifSettings.EnterpriseDescription != null)
				srvDescValue.Buffer.Text = ifSettings.EnterpriseDescription;
			srvDescValue.WrapMode = Gtk.WrapMode.Word;
			srvDescValue.Editable = false;
			srvDescValue.CursorVisible = false;
			srvDescValue.RightMargin = 5;
			srvDescValue.LeftMargin = 5;
			sw.Add(srvDescValue);
			srvTable.Attach(sw, 1,2,2,3,
					AttachOptions.Expand | AttachOptions.Fill , 0,0,0);



			//------------------------------
			// Disk Space
			//------------------------------
			// create a section box
			VBox diskSectionBox = new VBox();
			diskSectionBox.Spacing = Util.SectionTitleSpacing;
			vbox.PackStart(diskSectionBox, false, true, 0);
			Label diskSectionLabel = new Label("<span weight=\"bold\">" +
												Util.GS("Disk Space") +
												"</span>");
			diskSectionLabel.UseMarkup = true;
			diskSectionLabel.Xalign = 0;
			diskSectionBox.PackStart(diskSectionLabel, false, true, 0);

			// create a hbox to provide spacing
			HBox diskSpacerBox = new HBox();
			diskSpacerBox.Spacing = 10;
			diskSectionBox.PackStart(diskSpacerBox, true, true, 0);
			Label diskSpaceLabel = new Label("");
			diskSpacerBox.PackStart(diskSpaceLabel, false, true, 0);


			// create a table to hold the values
			Table diskTable = new Table(3,3,false);
			diskSpacerBox.PackStart(diskTable, true, true, 0);
			diskTable.ColumnSpacing = 20;
			diskTable.RowSpacing = 5;

			Label totalLabel = new Label(Util.GS("Free space on server:"));
			totalLabel.Xalign = 0;
			diskTable.Attach(totalLabel, 0,1,0,1,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label totalValue = new Label("0");
			totalValue.Xalign = 1;
			diskTable.Attach(totalValue, 1,2,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label totalUnit = new Label(Util.GS("MB"));
			diskTable.Attach(totalUnit, 2,3,0,1,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label usedLabel = new Label(Util.GS("Used space on server:"));
			usedLabel.Xalign = 0;
			diskTable.Attach(usedLabel, 0,1,1,2,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label usedValue = new Label("0");
			usedValue.Xalign = 1;
			diskTable.Attach(usedValue, 1,2,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label usedUnit = new Label(Util.GS("MB"));
			diskTable.Attach(usedUnit, 2,3,1,2,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);

			Label availLabel = new Label(Util.GS("Total space on server:"));
			availLabel.Xalign = 0;
			diskTable.Attach(availLabel, 0,1,2,3,
					AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
			Label availValue = new Label("0");
			availValue.Xalign = 1;
			diskTable.Attach(availValue, 1,2,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			Label availUnit = new Label(Util.GS("MB"));
			diskTable.Attach(availUnit, 2,3,2,3,
					AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);


			Frame graphFrame = new Frame();
			graphFrame.Shadow = Gtk.ShadowType.EtchedOut;
			graphFrame.ShadowType = Gtk.ShadowType.EtchedOut;
			diskSpacerBox.PackStart(graphFrame, false, true, 0);
			HBox graphBox = new HBox();
			graphBox.Spacing = 5;
			graphBox.BorderWidth = 5;
			graphFrame.Add(graphBox);

			ProgressBar diskGraph = new ProgressBar();
			graphBox.PackStart(diskGraph, false, true, 0);

			diskGraph.Orientation = Gtk.ProgressBarOrientation.BottomToTop;
//			diskGraph.Text = "%3";
			diskGraph.PulseStep = .10;
			diskGraph.Fraction = 0;

			VBox graphLabelBox = new VBox();
			graphBox.PackStart(graphLabelBox, false, true, 0);

			Label fullLabel = new Label(Util.GS("full"));
			fullLabel.Xalign = 0;
			fullLabel.Yalign = 0;
			graphLabelBox.PackStart(fullLabel, true, true, 0);

			Label emptyLabel = new Label(Util.GS("empty"));
			emptyLabel.Xalign = 0;
			emptyLabel.Yalign = 1;
			graphLabelBox.PackStart(emptyLabel, true, true, 0);


			DiskSpace ds = null;
			try
			{
				ds = iFolderWS.GetUserDiskSpace(ifSettings.CurrentUserID);
			}
			catch(Exception e)
			{
//				iFolderExceptionDialog ied = new iFolderExceptionDialog(
//													null, e);
//				ied.Run();
//				ied.Hide();
//				ied.Destroy();
//				ds = null;
			}

			if(ds == null)
			{
				totalValue.Text = Util.GS("N/A");
				totalUnit.Text = "";
				availValue.Text = Util.GS("N/A");
				availUnit.Text = "";
				usedValue.Text = Util.GS("N/A");
				usedUnit.Text = "";
				diskGraph.Fraction = 0;
			}
			else
			{
				int tmpValue;

				if(ds.Limit == 0)
				{
					totalValue.Text = Util.GS("N/A");
					totalUnit.Text = "";
				}
				else
				{
					tmpValue = (int)(ds.Limit / (1024 * 1024));
					totalValue.Text = string.Format("{0}", tmpValue);
					totalUnit.Text = Util.GS("MB");
				}

				if(ds.AvailableSpace == 0)
				{
					availValue.Text = Util.GS("N/A");
					availUnit.Text = "";
				}
				else
				{
					tmpValue = (int)(ds.AvailableSpace / (1024 * 1024));
					availValue.Text = string.Format("{0}",tmpValue);
					availUnit.Text = Util.GS("MB");
				}

				if(ds.UsedSpace == 0)
				{
					usedValue.Text = Util.GS("N/A");
					usedUnit.Text = "";
				}
				else
				{
					tmpValue = (int)(ds.UsedSpace / (1024 * 1024)) + 1;
					usedValue.Text = string.Format("{0}", tmpValue);
					usedUnit.Text = Util.GS("MB");
				}

				if(ds.Limit == 0)
				{
					diskGraph.Fraction = 0;
				}
				else
				{
					if(ds.Limit < ds.UsedSpace)
						diskGraph.Fraction = 1;
					else
						diskGraph.Fraction = ((double)ds.UsedSpace) / 
												((double)ds.Limit);
				}
			}

			return vbox;
		}




		/// <summary>
		/// Creates the Log tab
		/// </summary>
		/// <returns>
		/// Widget to display
		/// </returns>
		private Widget CreateLogPage()
		{
			// Create a new VBox and place 10 pixels between
			// each item in the vBox
			VBox vbox = new VBox();
			vbox.Spacing = 10;
			vbox.BorderWidth = Util.DefaultBorderWidth;
		
			Label lbl = new Label(Util.GS("This log shows current iFolder activity"));
			vbox.PackStart(lbl, false, true, 0);
			lbl.Xalign = 0;

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			vbox.PackStart(sw, true, true, 0);
			LogTreeView = new TreeView();
			sw.Add(LogTreeView);
			LogTreeView.HeadersVisible = false;

			// Setup the iFolder TreeView
			LogTreeStore = new ListStore(typeof(string));
			LogTreeView.Model = LogTreeStore;

			CellRendererText logcr = new CellRendererText();
			logcr.Xpad = 10;
			LogTreeView.AppendColumn(Util.GS("Log"), logcr, "text", 0);


			// Setup buttons for add/remove/accept/decline
			HBox buttonBox = new HBox();
			buttonBox.Spacing = 10;
			vbox.PackStart(buttonBox, false, false, 0);

			HBox leftBox = new HBox();
			leftBox.Spacing = 10;
			buttonBox.PackStart(leftBox, false, false, 0);
			HBox midBox = new HBox();
			midBox.Spacing = 10;
			buttonBox.PackStart(midBox, true, true, 0);
			HBox rightBox = new HBox();
			rightBox.Spacing = 10;
			buttonBox.PackStart(rightBox, false, false, 0);

			SaveButton = new Button(Gtk.Stock.Save);
			rightBox.PackStart(SaveButton);
			SaveButton.Clicked += new EventHandler(OnSaveLog);
			SaveButton.Sensitive = false;

			ClearButton = new Button(Gtk.Stock.Clear);
			rightBox.PackStart(ClearButton);
			ClearButton.Clicked += new EventHandler(OnClearLog);
			ClearButton.Sensitive = false;

			return vbox;
		}



		private void LogMessage(string logEntry)
		{
			TreeIter iter;

			while(LogTreeStore.IterNChildren() > 500)
			{
				if(LogTreeStore.GetIterFirst(out iter))
				{
					LogTreeStore.Remove(ref iter);
				}
			}

			iter = LogTreeStore.AppendValues(string.Format(
							"{0} {1}", DateTime.Now.ToString(), logEntry));

			TreePath path = LogTreeStore.GetPath(iter);

			LogTreeView.ScrollToCell(path, null, true, 1, 1);	

			SaveButton.Sensitive = true;
			ClearButton.Sensitive = true;
		}




		private void OnRealizeWidget(object o, EventArgs args)
		{
			RefreshiFolderTreeView(o, args);
		}




		private void iFolderLocationCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder = 
						(iFolderHolder) tree_model.GetValue(iter,0);
			if(ifHolder.iFolder.State == "Local")
				((CellRendererText) cell).Text = ifHolder.iFolder.UnManagedPath;
			else if(ifHolder.iFolder.State == "Available")
				((CellRendererText) cell).Text = ifHolder.iFolder.Owner;
		}




		private void iFolderStatusCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder =
					(iFolderHolder) tree_model.GetValue(iter,0);
			if(ifHolder.iFolder.State == "Local")
			{
				if(ifHolder.IsSyncing)
					((CellRendererText) cell).Text = 
									Util.GS("Synchronizing");
				else if(ifHolder.iFolder.HasConflicts)
					((CellRendererText) cell).Text = 
									Util.GS("Has File Conflicts");
				else if(!ifHolder.SyncSuccessful)
					((CellRendererText) cell).Text = 
									Util.GS("Sync Failed");
				else
					((CellRendererText) cell).Text = Util.GS("OK");
			}
			else if(ifHolder.iFolder.State == "Available")
				((CellRendererText) cell).Text = Util.GS("Available");
			else if(ifHolder.iFolder.State == "WaitConnect")
				((CellRendererText) cell).Text = Util.GS("Waiting to Connect");
			else if(ifHolder.iFolder.State == "WaitSync")
				((CellRendererText) cell).Text = Util.GS("Waiting to Sync");
			else
				((CellRendererText) cell).Text = Util.GS("Unknown");
		}




		private void iFolderCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder = (iFolderHolder) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifHolder.iFolder.Name;
		}




		private void iFolderCellPixbufDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolderHolder ifHolder = 
					(iFolderHolder) tree_model.GetValue(iter,0);

			if(ifHolder.iFolder.State == "Local")
			{
				if(ifHolder.iFolder.HasConflicts)
					((CellRendererPixbuf) cell).Pixbuf = ConflictPixBuf;
				else
					((CellRendererPixbuf) cell).Pixbuf = iFolderPixBuf;
			}
			else
				((CellRendererPixbuf) cell).Pixbuf = ServeriFolderPixBuf;
		}




		public void RefreshiFolderTreeView(object o, EventArgs args)
		{
			iFolderWeb[]	iFolderArray;
			try
			{
				iFolderArray = iFolderWS.GetAlliFolders();
			}
			catch(Exception e)
			{
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
													this, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
				return;
			}

			curiFolders.Clear();
			iFolderTreeStore.Clear();

			foreach(iFolderWeb ifolder in iFolderArray)
			{
				iFolderHolder holder = 
					new iFolderHolder(ifolder);

				TreeIter iter = iFolderTreeStore.AppendValues(holder);
				curiFolders.Add(ifolder.ID, iter);
			}
		}


		// This message is sent when the window is deleted 
		// or the X is clicked.  We just want to hide it so
		// we set the args.RetVal to true saying we handled the
		// delete even when we didn't
		private void WindowDelete (object o, DeleteEventArgs args)
		{
			OnCloseWindow(o, args);
			args.RetVal = true;
		}



		private void OnCloseWindow(object o, EventArgs args)
		{
			this.Hide ();
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
//				uint nodeCount = 47;
//				ulong bytesToSend = 121823;
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder = 
						(iFolderHolder) tModel.GetValue(iter, 0);

	//			This appears to hang?
	//			SyncSize.CalculateSendSize(	ifolder, 
	//										out nodeCount, 
	//										out bytesToSend);

	//			UploadLabel.Text = bytesToSend.ToString();
	//			SyncFilesLabel.Text = nodeCount.ToString();

				if(	(ifHolder.iFolder != null) && 
									(ifHolder.iFolder.HasConflicts) )
				{
					ConflictMenuItem.Sensitive = true;
				}
				else
				{
					ConflictMenuItem.Sensitive = false;
				}

				if(!ifHolder.iFolder.IsSubscription)
				{
					SetupMenuItem.Sensitive = false;
					ShareMenuItem.Sensitive = true;
					OpenMenuItem.Sensitive = true;
					SyncNowMenuItem.Sensitive = true;
					RevertMenuItem.Sensitive = true;
					PropMenuItem.Sensitive = true;
				}
				else
				{
					SetupMenuItem.Sensitive = true;
					ShareMenuItem.Sensitive = false;
					OpenMenuItem.Sensitive = false;
					SyncNowMenuItem.Sensitive = false;
					RevertMenuItem.Sensitive = false;
					PropMenuItem.Sensitive = false;
				}


				if(ifHolder.iFolder.OwnerID == 
						ifSettings.CurrentUserID)
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

							if(ifHolder.iFolder.State == "Local")
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
										OnShareProperties);

								if(ifHolder.iFolder.HasConflicts)
								{
									MenuItem item_resolve = new MenuItem (
											Util.GS("Resolve conflicts"));
									ifMenu.Append (item_resolve);
									item_resolve.Activated += new EventHandler(
										OnResolveConflicts);
							
									ifMenu.Append(new SeparatorMenuItem());
								}

								MenuItem item_sync =
									new MenuItem(Util.GS("Sync now"));
								ifMenu.Append (item_sync);
								item_sync.Activated += new EventHandler(
										OnSyncNow);

								MenuItem item_revert = new MenuItem (
										Util.GS("Revert to a normal folder"));
								ifMenu.Append (item_revert);
								item_revert.Activated += new EventHandler(
										OnRevertiFolder);

								if(ifHolder.iFolder.OwnerID == 
												ifSettings.CurrentUserID)
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
										OnSetupiFolder);

								if(ifHolder.iFolder.OwnerID == 
												ifSettings.CurrentUserID)
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
							new EventHandler(OnCreateiFolder);

						MenuItem item_refresh = 
							new MenuItem (Util.GS("Refresh list"));
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
					OnSetupiFolder(o, args);
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




		public void OnShareProperties(object o, EventArgs args)
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
									iFolderWS);
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



		public void	OnSyncNow(object o, EventArgs args)
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
    				iFolderWS.SynciFolderNow(ifHolder.iFolder.ID);
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
								iFolderWS.RevertiFolder(ifHolder.iFolder.ID);
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




		public void OnCreateiFolder(object o, EventArgs args)
		{
			int rc = 0;

			do
			{
				// Switched out to use the compatible file selector
				CompatFileChooserDialog cfcd = new CompatFileChooserDialog(
					Util.GS("Choose a folder..."), this, 
					CompatFileChooserDialog.Action.SelectFolder);

				rc = cfcd.Run();
				cfcd.Hide();

				if(rc == -5)
				{
					string selectedFolder = cfcd.Selections[0];

					if(ShowBadiFolderPath(selectedFolder, null))
						continue;

					// break loop
					rc = 0;
					try
					{
   		 				iFolderWeb newiFolder = 
							iFolderWS.CreateLocaliFolder(selectedFolder);
						if(newiFolder == null)
							throw new Exception("Simias returned null");

						TreeIter iter = 
							iFolderTreeStore.AppendValues(
								new iFolderHolder(newiFolder));
						curiFolders.Add(newiFolder.ID, iter);


						if(ifSettings.DisplayConfirmation)
						{
							iFolderCreationDialog dlg = 
								new iFolderCreationDialog(newiFolder);
							dlg.TransientFor = this;
							dlg.Run();
							dlg.Hide();

							if(dlg.HideDialog)
							{
								ShowConfirmationButton.Active = false;
								OnShowConfButton(null, null);
							}

							dlg.Destroy();
							dlg = null;
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




		private void OnSetupiFolder(object o, EventArgs args)
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
					curiFolders.Remove(ifHolder.iFolder.ID);
   		 			iFolderWeb newiFolder = iFolderWS.AcceptiFolderInvitation(
											ifHolder.iFolder.ID,
											newPath);
	
					// replace the old iFolder with this one
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


		private bool ShowBadiFolderPath(string path, string name)
		{
			try
			{
				bool isGood = true;
				if(name != null)
				{
					isGood = !iFolderWS.IsPathIniFolder(path);
					if(isGood)
					{
						// now we need to check if there is already an
						// ifolder at that path
						isGood = !iFolderWS.IsPathIniFolder(
								System.IO.Path.Combine(path, name));
					}
				}
				else
				{
					isGood = iFolderWS.CanBeiFolder(path);
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
								iFolderWS.RevertiFolder(ifHolder.iFolder.ID);
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
   		 			iFolderWS.DeclineiFolderInvitation(remiFolder.ID);
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

			if(ifHolder.iFolder.OwnerID == ifSettings.CurrentUserID)
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



		private void OnResolveConflicts(object o, EventArgs args)
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
										iFolderWS);
				ConflictDialog.Response += 
							new ResponseHandler(OnConflictDialogResponse);
				ConflictDialog.ShowAll();
			}
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


		private void OnUseProxyButton(object o, EventArgs args)
		{
			if(UseProxyButton.Active == true)
			{
				ProxyHostEntry.Sensitive = true;
				ProxyPortSpinButton.Sensitive = true;
				ProxyHostLabel.Sensitive = true;
				ProxyPortLabel.Sensitive = true;
			}
			else
			{
				ProxyHostEntry.Sensitive = false;
				ProxyPortSpinButton.Sensitive = false;
				ProxyHostLabel.Sensitive = false;
				ProxyPortLabel.Sensitive = false;
			}
		}




		private void OnShowConfButton(object o, EventArgs args)
		{
			ifSettings.DisplayConfirmation =
				ShowConfirmationButton.Active;
			try
			{
				iFolderWS.SetDisplayConfirmation(
									ifSettings.DisplayConfirmation);
			}
			catch(Exception e)
			{
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
													this, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
				return;
			}
		}




		private void OnProxySettingsChanged(object o, EventArgs args)
		{
			Console.WriteLine("Save ProxySettings here");
			// Save the settings here?
		}




		private void OnAutoSyncButton(object o, EventArgs args)
		{
			if(AutoSyncCheckButton.Active == true)
			{
				SyncSpinButton.Sensitive = true;
				SyncUnitsLabel.Sensitive = true;
			}
			else
			{
				SyncSpinButton.Sensitive = false;
				SyncUnitsLabel.Sensitive = false;
				SyncSpinButton.Value = 0;

/*				try
				{
					ifSettings.DefaultSyncInterval = (int)SyncSpinButton.Value;
					iFolderWS.SetDefaultSyncInterval(
									ifSettings.DefaultSyncInterval);
				}
				catch(Exception e)
				{
					iFolderExceptionDialog ied = new iFolderExceptionDialog(
													this, e);
					ied.Run();
					ied.Hide();
					ied.Destroy();
					return;
				}
*/
			}
		}




		private void OnSyncIntervalChanged(object o, EventArgs args)
		{
			try
			{
				ifSettings.DefaultSyncInterval = (int)SyncSpinButton.Value;
				iFolderWS.SetDefaultSyncInterval(
									ifSettings.DefaultSyncInterval);
			}
			catch(Exception e)
			{
				iFolderExceptionDialog ied = new iFolderExceptionDialog(
													this, e);
				ied.Run();
				ied.Hide();
				ied.Destroy();
				return;
			}
		}




		private void OnSwitchPage(object o, SwitchPageArgs args)
		{
			if(MainNoteBook.CurrentPage != 0)
			{
				CreateMenuItem.Sensitive = false;
				ShareMenuItem.Sensitive = false;
				OpenMenuItem.Sensitive = false;
				SyncNowMenuItem.Sensitive = false;
				ConflictMenuItem.Sensitive = false;
				RevertMenuItem.Sensitive = false;
				DeleteMenuItem.Sensitive = false;
//				DeleteMenuItem.Visible = false;
				RemoveMenuItem.Sensitive = false;
//				RemoveMenuItem.Visible = false;
				PropMenuItem.Sensitive = false;;
				RefreshMenuItem.Sensitive = false;
				SetupMenuItem.Sensitive = false;
			}
			else
			{
				CreateMenuItem.Sensitive = true;
				RefreshMenuItem.Sensitive = true;
				OniFolderSelectionChanged(o, args);
			}
		}




		// update the data value in the iFolderTreeStore so the ifolder
		// will switch to one that has conflicts
		public void iFolderHasConflicts(iFolderWeb ifolder)
		{
			if(curiFolders.ContainsKey(ifolder.ID))
			{
				TreeIter iter = (TreeIter)curiFolders[ifolder.ID];
				iFolderTreeStore.SetValue(iter, 0, 
						new iFolderHolder(ifolder));
			}

			// TODO: let any property dialogs know that this iFolder
			// has a conflict
		}



		public void iFolderChanged(iFolderWeb ifolder)
		{
			if(curiFolders.ContainsKey(ifolder.ID))
			{
				TreeIter iter = (TreeIter)curiFolders[ifolder.ID];

				if( ifolder.State == "Available" )
				{
					// this is a subscription, check to see if we
					// have a matching ifolder to yank this one
					// out if we need to
					iFolderWeb realiFolder = iFolderWS.GetiFolder(
							ifolder.CollectionID);
					if(realiFolder != null)
					{
						iFolderTreeStore.Remove(ref iter);
						curiFolders.Remove(ifolder.ID);
						return;
					}
				}

				iFolderTreeStore.SetValue(iter, 0, new iFolderHolder(ifolder));
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



		public void iFolderCreated(iFolderWeb ifolder)
		{
			if(!curiFolders.ContainsKey(ifolder.ID))
			{
				TreeIter iter = iFolderTreeStore.AppendValues(
						new iFolderHolder(ifolder));
				curiFolders.Add(ifolder.ID, iter);
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
						// 'cause we'll have the path at this poing
						if( (ifHolder.iFolder.UnManagedPath == null) ||
								(ifHolder.iFolder.UnManagedPath.Length == 0) )
						{
							iFolderWeb updatediFolder;

							try
							{
								updatediFolder = iFolderWS.GetiFolder(
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

						LogMessage(string.Format(Util.GS(
							"Started sync of iFolder {0}"), args.Name));
					}
					else
						LogMessage(string.Format(Util.GS(
							"Started sync of Collection {0}"), args.Name));

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
						if( (ifHolder.iFolder.CurrentUserID == null) ||
								(ifHolder.iFolder.CurrentUserID.Length == 0) )
						{
							iFolderWeb updatediFolder;
							try
							{
								updatediFolder = iFolderWS.GetiFolder(
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

						if(curiFolders.ContainsKey(args.ID))
						{
							LogMessage(string.Format(Util.GS(
								"Finshed sync of iFolder {0}"), args.Name));
						}
						else
							LogMessage(string.Format(Util.GS(
								"Finshed sync of Collection {0}"), args.Name));
					}
					else
					{
						UpdateStatus(Util.GS("Failed synchronization"));

						if(curiFolders.ContainsKey(args.ID))
						{
							LogMessage(string.Format(Util.GS(
								"Failed sync of iFolder {0}"), args.Name));
						}
						else
							LogMessage(string.Format(Util.GS(
								"Failed Syncing Collection {0}"), args.Name));
					}
					break;
				}
			}
		}


		public void HandleFileSyncEvent(FileSyncEventArgs args)
		{
			switch(args.Direction)
			{
				case Simias.Client.Event.Direction.Uploading:
				{
					if(args.Delete)
					{
						UpdateStatus(string.Format(Util.GS(
								"Deleting file from server {0}"), args.Name));
						if(SyncFileName != args.Name)
						{
							LogMessage(string.Format(Util.GS(
								"Deleting file from server {0}"), args.Name));
							SyncFileName = args.Name;
						}
					}
					else
					{
						UpdateStatus(string.Format(Util.GS(
									"Uploading file {0}"), args.Name));
						if(SyncFileName != args.Name)
						{
							LogMessage(string.Format(Util.GS(
									"Uploading file {0}"), args.Name));
							SyncFileName = args.Name;
						}
					}
					break;
				}
				case Simias.Client.Event.Direction.Downloading:
				{
					if(args.Delete)
					{
						UpdateStatus(string.Format(Util.GS(
									"Deleting local file {0}"), args.Name));
						if(SyncFileName != args.Name)
						{
							LogMessage(string.Format(Util.GS(
									"Deleing local file {0}"), args.Name));
							SyncFileName = args.Name;
						}
					}
					else
					{
						UpdateStatus(string.Format(Util.GS(
									"Downloading file {0}"), args.Name));
						if(SyncFileName != args.Name)
						{
							LogMessage(string.Format(Util.GS(
									"Downloading file {0}"), args.Name));
							SyncFileName = args.Name;
						}
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


		public void OnSaveLog(object o, EventArgs args)
		{
			int rc = 0;
			bool saveFile = false;
			string filename = null;

			// Switched out to use the compatible file selector
			CompatFileChooserDialog cfcd = new CompatFileChooserDialog(
				Util.GS("Save iFolder Log..."), this, 
				CompatFileChooserDialog.Action.Save);

			rc = cfcd.Run();
			cfcd.Hide();

			if(rc == -5)
			{
				filename = cfcd.Selections[0];

				if(File.Exists(filename))
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						Util.GS("iFolder Save Log"),
						Util.GS("Overwrite existing file?"),
						Util.GS("The file you have selected exists.  Selecting yes will overwrite the contents of this file.  Do you want to overwrite this file?"));
					rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8)
					{
						saveFile = true;
					}
				}
				else
					saveFile = true;
			}

			if(saveFile)
			{
				FileStream fs = File.Create(filename);
				if(fs != null)
				{
					TreeIter iter;
					StreamWriter w = new StreamWriter(fs);

					if(LogTreeStore.GetIterFirst(out iter))
					{
						string logEntry = 
							(string)LogTreeStore.GetValue(iter, 0);

						w.WriteLine(logEntry);

						while(LogTreeStore.IterNext(ref iter))
						{
							logEntry = 
								(string)LogTreeStore.GetValue(iter, 0);

							w.WriteLine(logEntry);
						}
					}
					
					w.Close();
				}
			}
		}



		public void OnClearLog(object o, EventArgs args)
		{
			iFolderMsgDialog dialog = new iFolderMsgDialog(
				this,
				iFolderMsgDialog.DialogType.Question,
				iFolderMsgDialog.ButtonSet.YesNo,
				Util.GS("Clear iFolder Log"),
				Util.GS("Clear iFolder Log?"),
				Util.GS("This will clear all log entries.  Are you sure you want to clear the iFolder Log?"));
			int rc = dialog.Run();
			dialog.Hide();
			dialog.Destroy();
			if(rc == -8)
			{
				LogTreeStore.Clear();
				SaveButton.Sensitive = false;
				ClearButton.Sensitive = false;
			}
		}

	}
}
