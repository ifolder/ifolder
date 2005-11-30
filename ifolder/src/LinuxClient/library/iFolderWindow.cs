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
using System.Text;
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
		private iFolderData			ifdata;
		private Gdk.Pixbuf			iFolderPixBuf;
		private Gdk.Pixbuf			ServeriFolderPixBuf;

		private Statusbar			MainStatusBar;
		private ProgressBar			SyncBar;
		private Toolbar				toolbar;
		private Gtk.TreeView		iFolderTreeView;
		private Gtk.ListStore		iFolderTreeStore;

		private Gtk.Tooltips		ToolbarTooltips;
		private ToolButton			NewButton;
		private ToolButton			SetupButton;
		private ToolButton			SyncButton;
		private ToolButton			ShareButton;
		private ToolButton			ConflictButton;
		private Gtk.ComboBox		DomainFilterComboBox;
		private Gtk.ListStore		DomainListStore;

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
		private ImageMenuItem		QuitMenuItem;
		private ImageMenuItem		RefreshMenuItem;
		private ImageMenuItem		HelpMenuItem;
		private ImageMenuItem		AboutMenuItem;
		
		private ImageMenuItem		PreferencesMenuItem;
		private Gtk.MenuItem		AccountsMenuItem;
		private Gtk.MenuItem		SyncLogMenuItem;
		private CheckMenuItem		RemoteFoldersCheckMenuItem;

		private iFolderConflictDialog ConflictDialog;

		private Hashtable			curiFolders;
//		private Hashtable			curSynchronizedFolders;

		// curDomain should be set to the domain selected in the
		// Domain Filter or if "all" domains are selected, this should be
		// set to null.
		private DomainInformation	curDomain;
		private DomainInformation[] curDomains;

		// These variables are used to keep track of how many
		// outstanding objects there are during a sync so that we don't
		// have to call CalculateSyncSize() over and over needlessly.
		private uint objectsToSync = 0;
		private bool startingSync  = false;

		private DomainController	domainController;

		// Manager object that knows about simias resources.
		private Manager				simiasManager;

		// Keep track of the properties dialogs so that if a user attempts
		// to open the properties of an iFolder that is already opened, it
		// won't open additional properties dialogs for the same iFolder.
		private Hashtable			propDialogs;
		
		private Notebook			WindowNotebook;
		
//		private iFolderNotebook		myiFolderNotebook;
		private Notebook			myiFolderNotebook;
		
		private int					HomePageIndex;
		private int					OldiFolderPageIndex;
		private int					SynchronizedFoldersPageIndex;
//		private int					RemoteFoldersPageIndex;
		
		///
		/// Home Page
		///
		private Button				AddFolderToSyncHomeButton;
		private Button				SearchRemoteFoldersHomeButton;
		private Button				ConnectToServerHomeButton0;
		private Button				ConnectToServerHomeButton;
		
		///
		/// Synchronized Folders Page
		///
		private ComboBox			synchronizedDomainsComboBox;

		private Gtk.Tooltips		SynchronizedFoldersTooltips;

		private EventBox			ContentEventBox;
//		private EventBox			SynchronizedActionsPane;

		///
		/// General Actions
		///
		private Button				AddSynchronizedFolderButton;
		private Button				ShowHideAllFoldersButton;
		private Label				ShowHideAllFoldersButtonText;
		
		private bool				bAvailableFoldersShowing;
//		private Button				RefreshSynchronizedFoldersButton;

		private Entry				synchronizedSearchEntry;
		private Button				SynchronizedCancelSearchButton;
		private uint				synchronizedSearchTimeoutID;

//		private Paned				synchronizedPaned;
		
//		private Notebook			SynchronizedDetailsNotebook;
		
		private Gdk.Pixbuf			synchronizedFolderPixbuf;

		private iFolderIconView		synchronizedFoldersIconView;
		private iFolderViewGroup	localGroup;
//		private iFolderViewGroup	myAvailableGroup;
//		private iFolderViewGroup	otherAvailableGroup;
		private TreeModelFilter		myiFoldersFilter;
//		private TreeModelFilter		myAvailableFoldersFilter;
//		private TreeModelFilter		otherAvailableFoldersFilter;
		
//		private Button				ShowAvailableiFoldersButton;
//		private Label				ShowAvailableiFoldersButtonText;

		private Expander			generalTasksExpander;
		private VBox				SynchronizedFolderTasks;
//		private Expander			detailsExpander;

//		private Button				AddFolderToSyncButton;
//		private Button				RemoveSynchronizedFolderButton;

		///
		/// Buttons for local iFolders
		///
		private Button				OpenSynchronizedFolderButton;
		private Button				SynchronizeNowButton;
		private Button				ShareSynchronizedFolderButton;
		private Button				ViewFolderPropertiesButton;
		private Button				ResolveConflictsButton;
		private Button				RemoveSynchronizedFolderButton;
		
		///
		/// Buttons for Available iFolders
		///
		private Button				DownloadAvailableiFolderButton;
		private Button				DeleteFromServerButton;
		private Button				RemoveMembershipButton;
		
//		private Label				SynchronizedNameLabel;
//		private Label				OwnerLabel;
//		private Label				ServerLabel;

		private Hashtable			serverGroups;
		private Hashtable			serverGroupFilters;

        // Drag and Drop
        enum TargetType
        {
        	UriList,
        	RootWindow
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
			ifdata = iFolderData.GetData(simiasManager);

			curiFolders = new Hashtable();
//			curSynchronizedFolders = new Hashtable();

			serverGroups = new Hashtable();
			serverGroupFilters = new Hashtable();

			curDomain = null;
			curDomains = null;

			propDialogs = new Hashtable();
			
			synchronizedSearchTimeoutID = 0;
			
			domainController = DomainController.GetDomainController();

			bAvailableFoldersShowing = false;

			CreateWidgets();

			RefreshDomains(true);
			RefreshiFolders(true);
			
			if (domainController != null)
			{
				domainController.DomainAdded +=
					new DomainAddedEventHandler(OnDomainAddedEvent);
				domainController.DomainDeleted +=
					new DomainDeletedEventHandler(OnDomainDeletedEvent);
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
			}
		}




		/// <summary>
		/// Set up the UI inside the Window
		/// </summary>
		private void CreateWidgets()
		{
			this.SetDefaultSize (600, 480);
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png"));
			this.WindowPosition = Gtk.WindowPosition.Center;

			WindowNotebook = new Notebook();
			this.Add(WindowNotebook);
			WindowNotebook.ShowTabs = false;
			
			WindowNotebook.AppendPage(CreateWelcomePage(), null);

			VBox vbox = new VBox (false, 0);
			WindowNotebook.AppendPage(vbox, null);
//			this.Add (vbox);

			//-----------------------------
			// Create the menubar
			//-----------------------------
			MenuBar menubar = CreateMenu ();
			vbox.PackStart (menubar, false, false, 0);
			
			myiFolderNotebook = CreateiFolderNotebook();
			vbox.PackStart (myiFolderNotebook, true, true, 0);

			//-----------------------------
			// Create the Toolbar
			//-----------------------------
//			toolbar = CreateToolbar();
//			vbox.PackStart (toolbar, false, false, 0);


			//-----------------------------
			// Create the Tree View
			//-----------------------------
//			vbox.PackStart(SetupTreeView(), true, true, 0);


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

			// Set up an event to refresh when the window is
			// being drawn
			this.Realized += new EventHandler(OnRealizeWidget);
		}
		
		private Widget CreateWelcomePage()
		{
			VBox vbox = new VBox(false, 0);

			//-----------------------------
			// Create the menubar
			//-----------------------------
			MenuBar menubar = CreateWelcomeMenuBar();
			vbox.PackStart (menubar, false, false, 0);
			
			Frame frame = new Frame();
			vbox.PackStart(frame, true, true, 0);
			vbox.ModifyBase(StateType.Normal, new Gdk.Color(255, 255, 255));
			
			VBox welcomeVBox = new VBox(false, 0);
			frame.Add(welcomeVBox);
			
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder128.png"));
			Image image = new Image(pixbuf);
			image.SetAlignment(0.5F, 0.5F);
			welcomeVBox.PackStart(image, false, false, 0);
			
			Label l = new Label(
				string.Format("<span size=\"x-large\" weight=\"bold\">{0}</span>",
				Util.GS("Welcome to iFolder")));
			welcomeVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			
			l = new Label(
				string.Format("<span>{0}</span>",
				Util.GS("iFolder is a file sharing solution for workgroup and enterprise environments.")));
			welcomeVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;

			///
			/// ConnectToServerHomeButton0
			///
			HBox hbox = new HBox(false, 0);
			ConnectToServerHomeButton0 = new Button(hbox);
			ConnectToServerHomeButton0.Relief = ReliefStyle.None;
//			ConnectToServerHomeButton0.Sensitive = false;
			vbox.PackStart(ConnectToServerHomeButton0, false, false, 0);
			
			// folder128.png
			Gdk.Pixbuf folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("add-account.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			Image folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			VBox buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			Label buttonText = new Label(string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", Util.GS("Connect to an iFolder Server")));
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			Label buttonMessage = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Start synchronizing files by connecting to an iFolder server")));
			buttonVBox.PackStart(buttonMessage, false, false, 0);
			buttonMessage.UseMarkup = true;
			buttonMessage.UseUnderline = false;
			buttonMessage.LineWrap = true;
			buttonMessage.Justify = Justification.Left;
			buttonMessage.Xalign = 0;
			buttonMessage.Yalign = 0;
			
			ConnectToServerHomeButton0.Clicked +=
				new EventHandler(OnConnectToServerHomeButton);
			
			return vbox;
		}
		
		
		private MenuBar CreateWelcomeMenuBar()
		{
			MenuBar menubar = new MenuBar ();
			AccelGroup agrp = new AccelGroup();
			this.AddAccelGroup(agrp);

			//----------------------------
			// iFolder Menu
			//----------------------------
			Menu menu = new Menu();

			ImageMenuItem imageMenuItem = new ImageMenuItem (Util.GS("Connect to a _server"));
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(Util.ImagesPath("add-account.png"));
			pixbuf = pixbuf.ScaleSimple(24, 24, Gdk.InterpType.Bilinear);
			imageMenuItem.Image = new Image(pixbuf);
			menu.Append(imageMenuItem);
			imageMenuItem.AddAccelerator("activate", agrp,
				new AccelKey(Gdk.Key.S, Gdk.ModifierType.ControlMask,
								AccelFlags.Visible));
			imageMenuItem.Activated += new EventHandler(OnAddNewAccount);

			menu.Append(new SeparatorMenuItem());

			imageMenuItem = new ImageMenuItem (Stock.Close, agrp);
			menu.Append(imageMenuItem);
			imageMenuItem.Activated += new EventHandler(CloseEventHandler);
			
			imageMenuItem = new ImageMenuItem(Stock.Quit, agrp);
			menu.Append(imageMenuItem);
			imageMenuItem.Activated += new EventHandler(QuitEventHandler);

			MenuItem menuItem = new MenuItem(Util.GS("i_Folder"));
			menuItem.Submenu = menu;
			menubar.Append (menuItem);

			//----------------------------
			// Edit Menu
			//----------------------------
			menu = new Menu();
			imageMenuItem = new ImageMenuItem(Util.GS("_Preferences"));
			imageMenuItem.Image = new Image(Stock.Preferences, Gtk.IconSize.Menu);
			menu.Append(imageMenuItem);
			imageMenuItem.Activated += new EventHandler(ShowPreferencesHandler);
			
			menuItem = new MenuItem(Util.GS("_Edit"));
			menuItem.Submenu = menu;
			menubar.Append(menuItem);

			//----------------------------
			// Help Menu
			//----------------------------
			menu = new Menu();

			imageMenuItem = new ImageMenuItem(Stock.Help, agrp);
			menu.Append(imageMenuItem);
			imageMenuItem.Activated += new EventHandler(OnHelpMenuItem);

			imageMenuItem = new ImageMenuItem(Util.GS("A_bout"));
			imageMenuItem.Image = new Image(Gnome.Stock.About,
							Gtk.IconSize.Menu);
			menu.Append(imageMenuItem);
			imageMenuItem.Activated += new EventHandler(OnAbout);

			menuItem = new MenuItem(Util.GS("_Help"));
			menuItem.Submenu = menu;
			menubar.Append(menuItem);

			return menubar;
		}
		

		private void OnAddNewAccount(object o, EventArgs args)
		{
			Util.ShowPrefsPage(1, simiasManager);
		}
		
		
		
		/// <summary>
		/// </summary>
		private Gdk.Pixbuf CreateImageWithEmblem(string mainImagePath, string emblemImagePath,
												 int height, int width, double emblemOverlap)
		{
			Gdk.Pixbuf mainPixbuf = new Gdk.Pixbuf(mainImagePath);
			Gdk.Pixbuf emblemPixbuf = new Gdk.Pixbuf(emblemImagePath);
			
			int bigHeight = mainPixbuf.Height + (int)(emblemPixbuf.Height * (1 - emblemOverlap));
			int bigWidth  = mainPixbuf.Width  + (int)(emblemPixbuf.Width  * (1 - emblemOverlap));
			
			Gdk.Pixbuf bigPixbuf0 = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, bigHeight, bigWidth);
			bigPixbuf0.Fill(0xffffffff); // opaque white background
			
			mainPixbuf.Composite(bigPixbuf0,
								 0, 0,	// dest x,y
//								 mainPixbuf.Width, mainPixbuf.Height, // dest width,height
								 bigWidth, bigHeight,
								 0, 0,	// offset x,y
								 1, 1,	// scale x,y
								 Gdk.InterpType.Bilinear,
								 255);	// overall alpha
			
//			Gdk.Pixbuf bigPixbuf1 = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, bigHeight, bigWidgth);
//			bigPixbuf1.Fill(0xffffffff); // opaque white background
			
			emblemPixbuf.Composite(bigPixbuf0,
								   bigWidth - emblemPixbuf.Width,		// dest_x
								   bigHeight - emblemPixbuf.Height,		// dest_y
//								   emblemPixbuf.Width,					// dest_width
//								   emblemPixbuf.Height,					// dest_height
								   bigWidth, bigHeight,
								   0, 0,								// offset x,y
								   1, 1,								// scale x,y
								   Gdk.InterpType.Bilinear,
								   255);								// overall alpha

			return bigPixbuf0;

/*			
			double scaleFactor;
			if (height > width)
			{
				if (bigHeight > bigWidth)
				{
					scaleFactor = ((double) height / (double) bigHeight);
				}
				else
				{
					scaleFactor = ((double) height / (double) bigWidth);
				}
			}
			else
			{
				if (bigHeight > bigWidth)
				{
					scaleFactor = ((double) width / (double) bigHeight);
				}
				else
				{
					scaleFactor = ((double) width / (double) bigWidth);
				}
			}
Console.WriteLine("scaleFactor: {0}", scaleFactor);

			Gdk.Pixbuf resultPixbuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, height, width);
			resultPixbuf.Fill(0xffffffff); // opaque white background
			
			bigPixbuf0.Composite(resultPixbuf,
								 0,0,
								 width, height,
								 0,0,
								 scaleFactor,
								 scaleFactor,
								 Gdk.InterpType.Bilinear,
								 255);
			
			return resultPixbuf;
*/
		}
		
		
		
		private Gdk.Pixbuf ScalePixbufToSize(Gdk.Pixbuf pixbuf, int maxWidth, int maxHeight)
		{
			double scaleFactor;
			if (maxHeight > maxWidth)
			{
				if (pixbuf.Height > pixbuf.Width)
				{
					scaleFactor = ((double) maxHeight / (double) pixbuf.Height);
				}
				else
				{
					scaleFactor = ((double) maxHeight / (double) pixbuf.Width);
				}
			}
			else
			{
				if (pixbuf.Height > pixbuf.Width)
				{
					scaleFactor = ((double) maxWidth / (double) pixbuf.Height);
				}
				else
				{
					scaleFactor = ((double) maxWidth / (double) pixbuf.Width);
				}
			}
			
			int width = (int) ((double)pixbuf.Width / scaleFactor);
			int height = (int) ((double)pixbuf.Height / scaleFactor);
			
			// Make sure we're not out of our bounds
			if (width > maxWidth)
				width = maxWidth;
			if (height > maxHeight)
				height = maxHeight;
				
			return pixbuf.ScaleSimple(width, height, Gdk.InterpType.Bilinear);
		}
		
		
		
		private Notebook CreateiFolderNotebook()
		{
			Notebook notebook = new Notebook();
			notebook.ShowTabs = false;

//			HBox hbox = new HBox(false, 2);
//			Image image = new Image(Stock.Home, IconSize.Menu);
//			hbox.PackStart(image, false, false, 0);
//			image.SetAlignment(0.5F, 0.5F);
//			Label l = new Label(string.Format("<span weight=\"bold\">{0}</span>", Util.GS("Home")));
//			hbox.PackStart(l, true, true, 0);
//			l.UseMarkup = true;
//			hbox.ShowAll();
			
//			HomePageIndex = notebook.AppendPage(CreateHomePage(), hbox);
			

//			hbox = new HBox(false, 2);
//			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(Util.ImagesPath("synchronized-folder64.png"));
//			pixbuf = pixbuf.ScaleSimple(24, 24, Gdk.InterpType.Bilinear);
//			image = new Image(pixbuf);
//			hbox.PackStart(image, false, false, 0);
//			image.SetAlignment(0.5F, 0.5F);
//			l = new Label(string.Format("<span weight=\"bold\">{0}</span>", Util.GS("Synchronized Folders")));
//			hbox.PackStart(l, true, true, 0);
//			l.UseMarkup = true;
//			l.LineWrap = true;
//			hbox.ShowAll();
			
//			SynchronizedFoldersPageIndex = notebook.AppendPage(CreateSynchronizedFoldersPage(), hbox);
			SynchronizedFoldersPageIndex =
				notebook.AppendPage(CreateSynchronizedFoldersPage(),
									new Label("New Interface"));

			OldiFolderPageIndex =
				notebook.AppendPage(CreateOldiFolderPage(),
									new Label("Old Interface (for debugging only)"));


//			hbox = new HBox(false, 2);
//			pixbuf = new Gdk.Pixbuf(Util.ImagesPath("remote-folder64.png"));
//			pixbuf = pixbuf.ScaleSimple(24, 24, Gdk.InterpType.Bilinear);
//			image = new Image(pixbuf);
//			hbox.PackStart(image, false, false, 0);
//			image.SetAlignment(0.5F, 0.5F);
//			l = new Label(string.Format("<span weight=\"bold\">{0}</span>", Util.GS("Remote Folders")));
//			hbox.PackStart(l, true, true, 0);
//			l.UseMarkup = true;
//			l.LineWrap = true;
//			hbox.ShowAll();
			
//			RemoteFoldersPageIndex = notebook.AppendPage(CreateRemoteFoldersPage(), hbox);

//			notebook.CurrentPage = HomePageIndex;
			notebook.CurrentPage = SynchronizedFoldersPageIndex;
			
			notebook.SwitchPage +=
				new SwitchPageHandler(OnSwitchPage);

			
			return notebook;
		}
		
		private void OnSwitchPage(object o, SwitchPageArgs args)
		{
			if (args.PageNum == SynchronizedFoldersPageIndex)
			{
				GLib.Timeout.Add(100, new GLib.TimeoutHandler(
							GrabSynchronizedSearchEntryFocus));
			}
//			else if (args.PageNum == RemoteFoldersPageIndex)
//			{
//				GLib.Timeout.Add(100, new GLib.TimeoutHandler(
//							GrabRemoteSearchEntryFocus));
//			}
		}
		
		private bool GrabSynchronizedSearchEntryFocus()
		{
			synchronizedSearchEntry.GrabFocus();
			return false;
		}
		
		private Widget CreateHomePage()
		{
			VBox vbox = new VBox(false, 0);
			vbox.PackStart(new Label(""), false, false, 0); // spacer

			///
			/// AddFolderToSyncHomeButton
			///
			HBox hbox = new HBox(false, 0);
			AddFolderToSyncHomeButton = new Button(hbox);
			AddFolderToSyncHomeButton.Relief = ReliefStyle.None;
//			AddFolderToSyncHomeButton.Sensitive = false;
			vbox.PackStart(AddFolderToSyncHomeButton, false, false, 0);
			
			Gdk.Pixbuf folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("add-folder-to-synchronize.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			Image folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			VBox buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			Label buttonText = new Label(string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", Util.GS("Add a folder")));
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			Label buttonMessage = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Choose a folder on your computer to upload and synchronize with an iFolder Server")));
			buttonVBox.PackStart(buttonMessage, false, false, 0);
			buttonMessage.UseMarkup = true;
			buttonMessage.UseUnderline = false;
			buttonMessage.LineWrap = true;
			buttonMessage.Justify = Justification.Left;
			buttonMessage.Xalign = 0;
			buttonMessage.Yalign = 0;
			
			AddFolderToSyncHomeButton.Clicked +=
				new EventHandler(OnAddFolderToSyncHomeButton);

			///
			/// SearchRemoteFoldersHomeButton
			///
			hbox = new HBox(false, 0);
			SearchRemoteFoldersHomeButton = new Button(hbox);
			SearchRemoteFoldersHomeButton.Relief = ReliefStyle.None;
//			SearchRemoteFoldersHomeButton.Sensitive = false;
			vbox.PackStart(SearchRemoteFoldersHomeButton, false, false, 0);
			
			// folder128.png
			folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("download-folder64.png"));
//			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			buttonText = new Label(string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", Util.GS("Download a remote folder")));
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			buttonMessage = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Search for remote folders to download and synchronize to your computer")));
			buttonVBox.PackStart(buttonMessage, false, false, 0);
			buttonMessage.UseMarkup = true;
			buttonMessage.UseUnderline = false;
			buttonMessage.LineWrap = true;
			buttonMessage.Justify = Justification.Left;
			buttonMessage.Xalign = 0;
			buttonMessage.Yalign = 0;
			
			SearchRemoteFoldersHomeButton.Clicked +=
				new EventHandler(OnSearchRemoteFoldersHomeButton);
				
			///
			/// Spacer (so that the ConnectToServerHomeButton is at the bottom)
			///
			vbox.PackStart(new Label(""), true, true, 0);

			///
			/// ConnectToServerHomeButton
			///
			hbox = new HBox(false, 0);
			ConnectToServerHomeButton = new Button(hbox);
			ConnectToServerHomeButton.Relief = ReliefStyle.None;
//			ConnectToServerHomeButton.Sensitive = false;
			vbox.PackStart(ConnectToServerHomeButton, false, false, 0);
			
			// folder128.png
			folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("add-account.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			buttonText = new Label(string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", Util.GS("Connect to an additional iFolder Server")));
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			buttonMessage = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Connect to another iFolder server to access additional folders")));
			buttonVBox.PackStart(buttonMessage, false, false, 0);
			buttonMessage.UseMarkup = true;
			buttonMessage.UseUnderline = false;
			buttonMessage.LineWrap = true;
			buttonMessage.Justify = Justification.Left;
			buttonMessage.Xalign = 0;
			buttonMessage.Yalign = 0;
			
			ConnectToServerHomeButton.Clicked +=
				new EventHandler(OnConnectToServerHomeButton);

			return vbox;
		}
		
		
		
		private void OnAddFolderToSyncHomeButton(object o, EventArgs args)
		{
			myiFolderNotebook.CurrentPage = SynchronizedFoldersPageIndex;
			CreateNewiFolder();
		}

		private void OnAddFolderToSyncButton(object o, EventArgs args)
		{
			CreateNewiFolder();
		}
		
		private void OnOpenSynchronizedFolder(object o, EventArgs args)
		{
			OpenSelectedFolder();
		}
		
		private void OnResolveConflicts(object o, EventArgs args)
		{
			ResolveSelectedFolderConflicts();
		}

		private void OnResolveConflictsOld(object o, EventArgs args)
		{
			TreeSelection tSelect = synchronizedFoldersIconView.Selection;
			if (tSelect != null && tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
				
				tSelect.GetSelected(out tModel, out iter);

				iFolderHolder holder =
					(iFolderHolder)tModel.GetValue(iter, 0);
				if (holder != null)
				{
					ConflictDialog = new iFolderConflictDialog(
											this,
											holder.iFolder,
											ifws,
											simws);
					ConflictDialog.Response +=
								new ResponseHandler(OnConflictDialogResponse);
					ConflictDialog.ShowAll();
				}
			}
		}
		
		private void OnSynchronizeNow(object o, EventArgs args)
		{
			SyncSelectedFolder();
		}
		
		private void OnSynchronizeNowOld(object o, EventArgs args)
		{
			TreeSelection tSelect = synchronizedFoldersIconView.Selection;
			if (tSelect != null && tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
				
				tSelect.GetSelected(out tModel, out iter);

				iFolderHolder holder =
					(iFolderHolder)tModel.GetValue(iter, 0);
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
		}

		private void OnShareSynchronizedFolder(object o, EventArgs args)
		{
			ShareSelectedFolder();
		}

		private void OnShareSynchronizedFolderOld(object o, EventArgs args)
		{
			TreeSelection tSelect = synchronizedFoldersIconView.Selection;
			if (tSelect != null && tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
				
				tSelect.GetSelected(out tModel, out iter);

				iFolderHolder holder =
					(iFolderHolder)tModel.GetValue(iter, 0);
				if (holder != null)
				{
					ShowSynchronizedFolderProperties(holder, 1);
				}
			}
		}

		private void ShowSynchronizedFolderProperties(iFolderHolder ifHolder, int desiredPage)
		{
			if (ifHolder != null)
			{
				iFolderPropertiesDialog propsDialog =
					(iFolderPropertiesDialog) propDialogs[ifHolder.iFolder.ID];
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
	
						propDialogs[ifHolder.iFolder.ID] = propsDialog;
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



		private void OnSearchRemoteFoldersHomeButton(object o, EventArgs args)
		{
//			myiFolderNotebook.CurrentPage = RemoteFoldersPageIndex;
		}

		private void OnConnectToServerHomeButton(object o, EventArgs args)
		{
			// Launch the Add Account Wizard
			AddAccountWizard aaw = new AddAccountWizard(simws);
			aaw.TransientFor = this;
			if (!Util.RegisterModalWindow(aaw))
			{
				try
				{
					Util.CurrentModalWindow.Present();
				}
				catch{}
				aaw.Destroy();
				return;
			}
			aaw.ShowAll();
		}

		


		private Widget CreateOldiFolderPage()
		{
			VBox vbox = new VBox (false, 0);
			
			//-----------------------------
			// Create the Toolbar
			//-----------------------------
			toolbar = CreateToolbar();
			vbox.PackStart (toolbar, false, false, 0);


			//-----------------------------
			// Create the Tree View
			//-----------------------------
			vbox.PackStart(SetupTreeView(), true, true, 0);
/*
			SetupTreeView();	// FIXME: Remove this eventually
			
			HBox hbox = new HBox(false, 4);
			vbox.PackStart(hbox, false, false, 0);
			
			hbox.PackStart(new Label(""), false, false, 0); //spacer
			
			Label l = new Label(Util.GS("Filter by Server:"));
			hbox.PackStart(l, false, false, 0);

			ComboBox domainFilterComboBox = new ComboBox();
			hbox.PackStart(domainFilterComboBox, false, false, 0);

			domainFilterComboBox.Model = DomainListStore;
			
			CellRenderer domainTR = new CellRendererText();
			domainFilterComboBox.PackStart(domainTR, true);

			domainFilterComboBox.SetCellDataFunc(domainTR,
				new CellLayoutDataFunc(DomainFilterComboBoxCellTextDataFunc));

			domainFilterComboBox.ShowAll();


			hbox.PackStart(new Label(""), true, true, 0); // spacer

			Entry remoteFoldersSearchEntry = new Entry();
			hbox.PackEnd(remoteFoldersSearchEntry, false, false, 0);
		
			l = new Label(Util.GS("Search:"));
			hbox.PackEnd(l, false, false, 0);

			ScrolledWindow sw = new ScrolledWindow();
			vbox.PackStart(sw, true, true, 0);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;

			remoteFoldersListStore = new ListStore(typeof(Gdk.Pixbuf), typeof(string));
			remoteFoldersIconView = new iFolderIconView(remoteFoldersListStore);
			remoteFoldersIconView.SelectionMode = SelectionMode.Single;

			remoteFoldersIconView.ButtonPressEvent +=
				new ButtonPressEventHandler(OnRemoteFoldersButtonPressed);
			
			remoteFoldersIconView.SelectionChanged +=
				new EventHandler(OnRemoteFoldersSelectionChanged);

			sw.Add(remoteFoldersIconView);
			
			Gdk.Pixbuf synchronizedFolderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("synchronized-folder64.png"));
			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					if (!holder.iFolder.IsSubscription)
					{
						remoteFoldersListStore.AppendValues(synchronizedFolderPixbuf, holder.iFolder.Name);
					}
				}
			}
*/
			return vbox;
		}
		

///
/// Synchronized Folders Page
///
		private Widget CreateSynchronizedFoldersPage()
		{
			ContentEventBox = new EventBox();
			ContentEventBox.ModifyBg(StateType.Normal, this.Style.Background(StateType.Active));

			VBox vbox = new VBox(false, 0);
			ContentEventBox.Add(vbox);
			vbox.PackStart(CreateSynchronizedPaned(), true, true, 0);

			return ContentEventBox;
		}
		

		private Widget CreateSynchronizedToolbar()
		{
			HBox searchHBox = new HBox(false, 4);
			
			Label l = new Label("");	// spacer
			searchHBox.PackStart(l, true, true, 0);
			
			l = new Label(Util.GS("Search:"));
			searchHBox.PackStart(l, true, true, 0);
			l.Xalign = 1F;
			
			synchronizedSearchEntry = new Entry();
			searchHBox.PackStart(synchronizedSearchEntry, true, true, 0);
			synchronizedSearchEntry.SelectRegion(0, -1);
			synchronizedSearchEntry.CanFocus = true;
			synchronizedSearchEntry.Changed +=
				new EventHandler(OnSynchronizedSearchEntryChanged);

			Image stopImage = new Image(Stock.Stop, Gtk.IconSize.Menu);
			stopImage.SetAlignment(0.5F, 0F);
			
			SynchronizedCancelSearchButton = new Button(stopImage);
			searchHBox.PackEnd(SynchronizedCancelSearchButton, false, false, 0);
			SynchronizedCancelSearchButton.Relief = ReliefStyle.None;
			SynchronizedCancelSearchButton.Sensitive = false;
			
			SynchronizedCancelSearchButton.Clicked +=
				new EventHandler(OnSynchronizedCancelSearchButton);

			return searchHBox;
		}
		
		private void AddSynchronizedFolderHandler(object o,  EventArgs args)
		{
Console.WriteLine("AddSynchronizedFolderHandler");
			CreateNewiFolder();
		}
		
		private void ShowHideAllFoldersHandler(object o, EventArgs args)
		{
			if (bAvailableFoldersShowing)
				HideAvailableiFolders();
			else
				ShowAvailableiFolders();
		}
		
		private void ShowAvailableiFolders()
		{
			foreach(iFolderViewGroup group in serverGroups.Values)
			{
				synchronizedFoldersIconView.AddGroup(group);
			}

//			synchronizedFoldersIconView.AddGroup(myAvailableGroup);
//			synchronizedFoldersIconView.AddGroup(otherAvailableGroup);

			ShowHideAllFoldersButtonText.Markup =
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Hide unsynchronized iFolders"));

			bAvailableFoldersShowing = true;
		}
		
		private void HideAvailableiFolders()
		{
			foreach(iFolderViewGroup group in serverGroups.Values)
			{
				synchronizedFoldersIconView.RemoveGroup(group);
			}

//			synchronizedFoldersIconView.RemoveGroup(myAvailableGroup);
//			synchronizedFoldersIconView.RemoveGroup(otherAvailableGroup);

			ShowHideAllFoldersButtonText.Markup =
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Show unsynchronized iFolders"));

//			ShowAvailableiFoldersButton.Show();

			bAvailableFoldersShowing = false;
		}
		
		private void RemoveSynchronizedFolderHandler(object o,  EventArgs args)
		{
			RemoveSelectedFolderHandler();
		}

		private void RemoveSynchronizedFolderHandlerOld(object o,  EventArgs args)
		{
Console.WriteLine("RemoveSynchronizedFolderHandler");
			TreeSelection tSelect = synchronizedFoldersIconView.Selection;
			if (tSelect != null && tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
				
				tSelect.GetSelected(out tModel, out iter);

				iFolderHolder holder =
					(iFolderHolder)tModel.GetValue(iter, 0);
				if (holder != null)
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						Util.GS("Revert this iFolder to a normal folder?"),
						"");
					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8)
					{
						try
						{
							ifdata.RevertiFolder(holder.iFolder.ID);
//								if (myiFoldersFilter.Remove(ref iter))
//								{
//									if (curSynchronizedFolders.ContainsKey(holder.iFolder.ID))
//										curSynchronizedFolders.Remove(holder.iFolder.ID);
//								}
								
//								synchronizedFoldersIconView.RefreshIcons();
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
		
		
		private void DownloadSelectedFolder()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
			if (holder != null && holder.iFolder.IsSubscription)
			{
				try
				{
					string lastSetupPath = Util.LastSetupPath;
					iFolderHolder newHolder = ifdata.AcceptiFolderInvitation(
														holder.iFolder.ID,
														holder.iFolder.DomainID,
														lastSetupPath);
	
					if (newHolder != null)
					{
	//					TreeIter iter;
	//					iter = myiFoldersFilter.AppendValues(synchronizedFolderPixbuf, newHolder.iFolder.Name, newHolder);
	//					curSynchronizedFolders[holder.iFolder.ID] = iter;
		
	//					synchronizedFoldersIconView.RefreshIcons();
					}
				}
				catch(Exception e)
				{
					DisplayCreateOrSetupException(e);
				}
			}
		}
		
		private void DeleteSelectedFolderFromServer()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
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
		
		private void RemoveMembershipFromSelectedFolder()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
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
		
		private Widget CreateSynchronizedToolbarOld()
		{
			Frame toolbarFrame = new Frame();
//			vbox.PackStart(toolbarFrame, false, false, 0);
			toolbarFrame.ShadowType = ShadowType.EtchedOut;
			
			HBox hbox = new HBox(false, 4);
			toolbarFrame.Add(hbox);
//			vbox.PackStart(hbox, false, false, 0);

			hbox.BorderWidth = 4;
			
			hbox.PackStart(new Label(""), false, false, 0); // spacer
			
			Label l = new Label(Util.GS("Filter by Server:"));
			hbox.PackStart(l, false, false, 0);

			synchronizedDomainsComboBox = new ComboBox();
			hbox.PackStart(synchronizedDomainsComboBox, false, false, 0);

			synchronizedDomainsComboBox.Model = DomainListStore;
			
			CellRenderer domainTR = new CellRendererText();
			synchronizedDomainsComboBox.PackStart(domainTR, true);

			synchronizedDomainsComboBox.SetCellDataFunc(domainTR,
				new CellLayoutDataFunc(SynchronizedDomainComboBoxCellTextDataFunc));

			synchronizedDomainsComboBox.ShowAll();

			hbox.PackStart(new Label(""), true, true, 0); // spacer
			
//			Image stopImage = new Image(Stock.Stop, Gtk.IconSize.Menu);
//			stopImage.SetAlignment(0.5F, 0F);
			
//			SynchronizedCancelSearchButton = new Button(stopImage);
//			hbox.PackEnd(SynchronizedCancelSearchButton, false, false, 0);
//			SynchronizedCancelSearchButton.Relief = ReliefStyle.None;
//			SynchronizedCancelSearchButton.Sensitive = false;
			
//			SynchronizedCancelSearchButton.Clicked +=
//				new EventHandler(OnSynchronizedCancelSearchButton);

//			synchronizedSearchEntry = new Entry();
//			hbox.PackEnd(synchronizedSearchEntry, false, false, 0);
//			synchronizedSearchEntry.SelectRegion(0, -1);
//			synchronizedSearchEntry.CanFocus = true;
//			synchronizedSearchEntry.Changed +=
//				new EventHandler(OnSynchronizedSearchEntryChanged);
		
//			l = new Label(Util.GS("Search:"));
//			hbox.PackEnd(l, false, false, 0);
			
			return toolbarFrame;
		}
		
		private void OnSynchronizedCancelSearchButton(object o, EventArgs args)
		{
			// FIXME: Implement OnSynchronizedCancelSearchButton
			synchronizedSearchEntry.Text = "";
			synchronizedSearchEntry.GrabFocus();
		}
		
		private void OnSynchronizedSearchEntryChanged(object o, EventArgs args)
		{
			if (synchronizedSearchTimeoutID != 0)
			{
				GLib.Source.Remove(synchronizedSearchTimeoutID);
				synchronizedSearchTimeoutID = 0;
			}

			if (synchronizedSearchEntry.Text.Length > 0)
				SynchronizedCancelSearchButton.Sensitive = true;
			else
				SynchronizedCancelSearchButton.Sensitive = false;

			synchronizedSearchTimeoutID = GLib.Timeout.Add(
				500, new GLib.TimeoutHandler(SynchronizedSearchCallback));
		}
		
		private bool SynchronizedSearchCallback()
		{
			SearchFolders();
			return false;
		}

		private void SearchFolders()
		{
			myiFoldersFilter.Refilter();
//			myAvailableFoldersFilter.Refilter();
//			otherAvailableFoldersFilter.Refilter();
//			synchronizedFoldersIconView.RefreshIcons();
			foreach(iFolderViewGroup group in serverGroups.Values)
			{
				group.Model.Refilter();
			}
		}
		
		private void SearchFoldersOld()
		{
//			myiFoldersFilter.Clear();
//			curSynchronizedFolders.Clear();

			string searchString = synchronizedSearchEntry.Text;
			if (searchString != null)
			{
				searchString = searchString.Trim();
				if (searchString.Length > 0)
					searchString = searchString.ToLower();
			}

			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					if (!holder.iFolder.IsSubscription)
					{
						TreeIter iter;
						if (searchString == null || searchString.Trim().Length == 0)
						{
							// Add everything in
//							iter = myiFoldersFilter.AppendValues(synchronizedFolderPixbuf, holder.iFolder.Name, holder);
//							curSynchronizedFolders[holder.iFolder.ID] = iter;
						}
						else
						{
							// Search the iFolder's Name (for now)
							string name = holder.iFolder.Name;
							if (name != null)
							{
								name = name.ToLower();
								if (name.IndexOf(searchString) >= 0)
								{
//									iter = myiFoldersFilter.AppendValues(synchronizedFolderPixbuf, holder.iFolder.Name, holder);
//									curSynchronizedFolders[holder.iFolder.ID] = iter;
								}
							}
						}
					}
				}
			}
			
//			synchronizedFoldersIconView.RefreshIcons();
		}
		
		private Widget CreateSynchronizedPaned()
		{
			HBox hbox = new HBox(false, 0);

			hbox.PackStart(CreateSynchronizedActions(), false, false, 12);
			hbox.PackStart(CreateSynchronizedIconViewPane(), true, true, 0);
			
			
//			hbox.PackStart(CreateSynchronizedIconViewPane(), false, false, 0);
//			hbox.PackStart(CreateItemCanvas(), true, true, 0);
			
			return hbox;
		}
		
		private Widget CreateItemCanvas()
		{
			ScrolledWindow sw = new ScrolledWindow();
			sw.VscrollbarPolicy = PolicyType.Automatic;
			sw.HscrollbarPolicy = PolicyType.Automatic;
			
			Gnome.Canvas canvas = Gnome.Canvas.NewAa();
			sw.Add(canvas);
			
//			Gnome.CanvasGroup group = new Gnome.CanvasGroup(canvas.Root());
			
//			Gnome.CanvasText hello = new Gnome.CanvasText(canvas.Root());
//			hello.X = 0;
//			hello.Y = 0;
//			hello.FillColor = "#000000";
//			hello.Text = "Hello World!";
//			hello.Show();

			Gnome.CanvasGroup synchronizedGroup = new Gnome.CanvasGroup(canvas.Root());
			double[] translate = new double[6];
			Art.Affine.Translate(translate, 0, 7);
			synchronizedGroup.AffineAbsolute(translate);
//			synchronizedGroup.X = 0;
//			synchronizedGroup.Y = 0;
//			synchronizedGroup.Move(0, 7);

			Gnome.CanvasText categoryTitle = new Gnome.CanvasText(synchronizedGroup);
			categoryTitle.X = 0;
			categoryTitle.Y = 0;
			categoryTitle.FillColor = "#000000";
			categoryTitle.Text = Util.GS("Synchronized Folders");
			categoryTitle.Markup =
				string.Format(
					"<span size=\"x-large\" weight=\"bold\">{0}</span>",
					categoryTitle.Text);
			categoryTitle.Anchor = AnchorType.NorthWest;
			categoryTitle.Show();
			synchronizedGroup.Show();

			Gnome.CanvasWidget cWidget = new Gnome.CanvasWidget(synchronizedGroup);
			HBox hbox = new HBox(false, 0);
			cWidget.Width = 1600;
			cWidget.Height = 40;
//			cWidget.Anchor = AnchorType.SouthEast;
			cWidget.X = 0;
			cWidget.Y = 40;
			cWidget.Widget = hbox;
			cWidget.Show();
			for (int i = 0; i < 50; i++)
			{
				Button button = new Button(string.Format("{0}: Click me", i));
				hbox.PackStart(button, false, false, 0);
			}

			canvas.Show();
			
			return sw;
		}
		
		private Widget CreateSynchronizedActions()
		{
			VBox actionsVBox = new VBox(false, 0);

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
					"<span size=\"x-large\">{0}</span>",
					Util.GS("Filter")));
			actionsVBox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.ModifyFg(StateType.Normal, this.Style.Base(StateType.Selected));
			l.Xalign = 0.0F;
			
			HBox searchHBox = new HBox(false, 4);
			actionsVBox.PackStart(searchHBox, false, false, 0);
			
			synchronizedSearchEntry = new Entry();
			searchHBox.PackStart(synchronizedSearchEntry, true, true, 0);
			synchronizedSearchEntry.SelectRegion(0, -1);
			synchronizedSearchEntry.CanFocus = true;
			synchronizedSearchEntry.Changed +=
				new EventHandler(OnSynchronizedSearchEntryChanged);

			Image stopImage = new Image(Stock.Stop, Gtk.IconSize.Menu);
			stopImage.SetAlignment(0.5F, 0F);
			
			SynchronizedCancelSearchButton = new Button(stopImage);
			searchHBox.PackEnd(SynchronizedCancelSearchButton, false, false, 0);
			SynchronizedCancelSearchButton.Relief = ReliefStyle.None;
			SynchronizedCancelSearchButton.Sensitive = false;
			
			SynchronizedCancelSearchButton.Clicked +=
				new EventHandler(OnSynchronizedCancelSearchButton);

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
					"<span size=\"x-large\">{0}</span>",
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
			AddSynchronizedFolderButton = new Button(hbox);
			vbox.PackStart(AddSynchronizedFolderButton, false, false, 0);
			AddSynchronizedFolderButton.Relief = ReliefStyle.None;

			Label buttonText = new Label(
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Upload a folder")));
			hbox.PackStart(buttonText, false, false, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			AddSynchronizedFolderButton.Clicked +=
				new EventHandler(AddSynchronizedFolderHandler);
			
			///
			/// ShowHideAllFoldersButton
			///
			hbox = new HBox(false, 0);
			ShowHideAllFoldersButton = new Button(hbox);
			vbox.PackStart(ShowHideAllFoldersButton, false, false, 0);
			ShowHideAllFoldersButton.Relief = ReliefStyle.None;

			ShowHideAllFoldersButtonText = new Label(
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Show unsynchronized iFolders")));
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
					"<span size=\"x-large\">{0}</span>",
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
				string.Format("<span size=\"large\">{0}</span>",
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

//			Gdk.Pixbuf buttonPixbuf = new Gdk.Pixbuf(Util.ImagesPath("conflict24.png"));
//			buttonImage = new Image(buttonPixbuf);
//			hbox.PackStart(buttonImage, false, false, 0);
//			buttonImage.SetAlignment(0.5F, 0F);

			buttonText = new Label(
				string.Format("<span size=\"large\">{0}</span>",
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
				string.Format("<span size=\"large\">{0}</span>",
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
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Share with...")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			ShareSynchronizedFolderButton.Clicked +=
				new EventHandler(OnShareSynchronizedFolder);


			///
			/// ViewFolderPropertiesButton
			///
			hbox = new HBox(false, 0);
			ViewFolderPropertiesButton = new Button(hbox);
			vbox.PackStart(ViewFolderPropertiesButton, false, false, 0);
			ViewFolderPropertiesButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("View properties...")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			ViewFolderPropertiesButton.Clicked +=
				new EventHandler(OnShowFolderProperties);


			///
			/// RemoveSynchronizedFolderButton
			///
			hbox = new HBox(false, 0);
			RemoveSynchronizedFolderButton = new Button(hbox);
			vbox.PackStart(RemoveSynchronizedFolderButton, false, false, 0);
			RemoveSynchronizedFolderButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Revert to a normal folder")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			RemoveSynchronizedFolderButton.Clicked +=
				new EventHandler(RemoveSynchronizedFolderHandler);

			///
			/// DownloadAvailableiFolderButton
			///
			hbox = new HBox(false, 0);
			DownloadAvailableiFolderButton = new Button(hbox);
			vbox.PackStart(DownloadAvailableiFolderButton, false, false, 0);
			DownloadAvailableiFolderButton.Relief = ReliefStyle.None;

			buttonText = new Label(
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Download")));
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
				string.Format("<span size=\"large\">{0}</span>",
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
				string.Format("<span size=\"large\">{0}</span>",
							  Util.GS("Remove membership")));
			hbox.PackStart(buttonText, true, true, 4);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;

			RemoveMembershipButton.Clicked +=
				new EventHandler(RemoveMembershipHandler);


			///
			/// Details Expander
			///
/*
			detailsExpander =
				new Expander(
					string.Format("<span size=\"small\" weight=\"bold\">{0}</span>",
					Util.GS("Details:")));
			actionsVBox.PackStart(detailsExpander, false, false, 0);
			detailsExpander.Expanded = false;
			detailsExpander.UseMarkup = true;
			spacerHBox = new HBox(false, 0);
			detailsExpander.Add(spacerHBox);
			spacerHBox.PackStart(new Label(""), false, false, 4);
			vbox = new VBox(false, 0);
			spacerHBox.PackStart(vbox, true, true, 0);

			SynchronizedNameLabel = new Label("");
			SynchronizedNameLabel.UseMarkup = true;
			SynchronizedNameLabel.UseUnderline = false;
			SynchronizedNameLabel.Xalign = 0.5F;
			vbox.PackStart(SynchronizedNameLabel, false, true, 5);


			Table detailsTable = new Table(2, 2, false);
			vbox.PackStart(detailsTable, true, true, 0);
			detailsTable.ColumnSpacing = 5;
			detailsTable.RowSpacing = 5;
			
			Label label = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Owner:")));
			label.UseMarkup = true;
			label.Xalign = 0;
			detailsTable.Attach(label, 0, 1, 0, 1,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			OwnerLabel = new Label("");
			OwnerLabel.UseMarkup = true;
			OwnerLabel.UseUnderline = false;
			OwnerLabel.Xalign = 0;
			detailsTable.Attach(OwnerLabel, 1, 2, 0, 1,
					AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			
			label = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Server:")));
			label.UseMarkup = true;
			label.Xalign = 0;
			detailsTable.Attach(label, 0, 1, 1, 2,
					AttachOptions.Shrink | AttachOptions.Fill, 0, 0, 0);
			
			ServerLabel = new Label("");
			ServerLabel.UseMarkup = true;
			ServerLabel.UseUnderline = false;
			ServerLabel.Xalign = 0;
			detailsTable.Attach(ServerLabel, 1, 2, 1, 2,
					AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
*/
			return actionsVBox;
		}
		
		private Widget CreateSynchronizedDetails()
		{
			VBox vbox = new VBox(false, 0);
			
			Label l = new Label(Util.GS("Description:"));
			l.Xalign = 0;
			vbox.PackStart(l, false, false, 0);

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			vbox.PackStart(sw, true, true, 0);
			sw.VscrollbarPolicy = PolicyType.Automatic;
			sw.HscrollbarPolicy = PolicyType.Automatic;

			TextView textView = new TextView();
			sw.Add(textView);
			textView.Buffer.Text = "Not implemented yet";
			textView.Editable = false;
			textView.Sensitive = false;
			textView.WrapMode = WrapMode.Word;
			
			return vbox;
		}
		
		private Widget CreateSynchronizedIconViewPane()
		{
			ScrolledWindow sw = new ScrolledWindow();
//			sw.ShadowType = Gtk.ShadowType.EtchedIn;

//			myiFoldersFilter = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(iFolderHolder));
//			synchronizedFoldersIconView = new iFolderIconView(myiFoldersFilter);
			synchronizedFoldersIconView = new iFolderIconView();

			///
			/// Synchronized iFolders
			///			
			myiFoldersFilter = new TreeModelFilter(ifdata.iFolders, null);
			myiFoldersFilter.VisibleFunc = SynchronizedFoldersFilterFunc;
			
			localGroup = new iFolderViewGroup(Util.GS("My Synchronized iFolders"), myiFoldersFilter);
			synchronizedFoldersIconView.AddGroup(localGroup);
//			synchronizedFoldersIconView.SelectionMode = SelectionMode.Single;


			DomainInformation[] domains = domainController.GetDomains();
			foreach (DomainInformation domain in domains)
			{
				AddServerGroup(domain.ID);
			}

			///
			/// ShowAvailableiFoldersButton
			///
/*
			HBox hbox = new HBox(false, 0);
			hbox.PackStart(new Label(""), true, true, 0);	// spacer
			HBox hbox2 = new HBox(false, 0);
			ShowAvailableiFoldersButton = new Button(hbox2);
			hbox.PackStart(ShowAvailableiFoldersButton, false, false, 0);
			ShowAvailableiFoldersButton.Xalign = 1.0F;
			ShowAvailableiFoldersButton.Relief = ReliefStyle.None;

			ShowAvailableiFoldersButtonText = new Label(
				string.Format("<span size=\"large\" underline=\"single\">{0}</span>",
							  Util.GS("Show downloadable iFolders...")));
			hbox2.PackStart(ShowAvailableiFoldersButtonText, true, true, 4);
			ShowAvailableiFoldersButtonText.UseMarkup = true;
			ShowAvailableiFoldersButtonText.UseUnderline = false;
			ShowAvailableiFoldersButtonText.Xalign = 0;

			ShowAvailableiFoldersButton.Clicked += ShowHideAllFoldersHandler;
			
			synchronizedFoldersIconView.AddWidget(hbox);
*/

			
			///
			/// My Available iFolders
			///
//			myAvailableFoldersFilter = new TreeModelFilter(ifdata.iFolders, null);
//			myAvailableFoldersFilter.VisibleFunc = MyAvailableFoldersFilterFunc;
//			myAvailableGroup =
//				new iFolderViewGroup(Util.GS("My iFolders on the Server"),
//									 myAvailableFoldersFilter);
			
			///
			/// Other Available iFolders
			///
//			otherAvailableFoldersFilter = new TreeModelFilter(ifdata.iFolders, null);
//			otherAvailableFoldersFilter.VisibleFunc = OtherAvailableFoldersFilterFunc;
//			otherAvailableGroup =
//				new iFolderViewGroup(Util.GS("iFolders Shared With Me"),
//									 otherAvailableFoldersFilter);

			synchronizedFoldersIconView.ButtonPressEvent +=
				new ButtonPressEventHandler(OnSynchronizedFoldersButtonPressed);
			
			synchronizedFoldersIconView.SelectionChanged +=
				new EventHandler(OnSynchronizedFoldersSelectionChanged);

			TargetEntry[] targets =
				new TargetEntry[]
				{
	                new TargetEntry ("text/uri-list", 0, (uint) TargetType.UriList),
	                new TargetEntry ("application/x-root-window-drop", 0, (uint) TargetType.RootWindow)
				};

			Drag.DestSet(synchronizedFoldersIconView,
						 //Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask,
						 DestDefaults.All,
						 targets,
						 Gdk.DragAction.Copy | Gdk.DragAction.Move);

			synchronizedFoldersIconView.DragMotion +=
				new DragMotionHandler(OnIconViewDragMotion);
				
			synchronizedFoldersIconView.DragDrop +=
				new DragDropHandler(OnIconViewDragDrop);
			
			synchronizedFoldersIconView.DragDataReceived +=
				new DragDataReceivedHandler(OnIconViewDragDataReceived);
			
//			TargetEntry[] targetEntry =
//				new TargetEntry[]{
//					new TargetEntry("STRING", 0, 0),
//					new TargetEntry("text/plain", 0, 0),
//					new TargetEntry("text/uri-list", 0, 0),
//					new TargetEntry("application/x-rootwindow-drop", 0, 0)
//				};

			
			sw.Add(synchronizedFoldersIconView);
			
//			synchronizedFolderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("synchronized-folder64.png"));
//			iFolderHolder[] ifolders = ifdata.GetiFolders();
//			if(ifolders != null)
//			{
//				foreach(iFolderHolder holder in ifolders)
//				{
//					if (!holder.iFolder.IsSubscription)
//					{
//						TreeIter iter;
//						iter = myiFoldersFilter.AppendValues(synchronizedFolderPixbuf, holder.iFolder.Name, holder);
//						curSynchronizedFolders[holder.iFolder.ID] = iter;
//					}
//				}
//			}

			return sw;
		}
		
		private void AddServerGroup(string domainID)
		{
			// Don't add it if it already exists
			if (serverGroups.ContainsKey(domainID)) return;
		
			DomainInformation domain = domainController.GetDomain(domainID);
			if (domain == null) return;
			
			iFolderServerFilter serverFilter =
				new iFolderServerFilter(domainID, synchronizedSearchEntry);
			TreeModelFilter treeModelFilter = new TreeModelFilter(ifdata.iFolders, null);
			treeModelFilter.VisibleFunc = serverFilter.FilterFunc;

			iFolderViewGroup group =
				new iFolderViewGroup(
					string.Format(
						Util.GS("iFolders on {0}"),
						domain.Name),
					treeModelFilter);
			serverGroups[domainID] = group;
			serverGroupFilters[domainID] = serverFilter;
			
			if (bAvailableFoldersShowing)
				synchronizedFoldersIconView.AddGroup(group);
		}

		private bool SynchronizedFoldersFilterFunc(TreeModel model, TreeIter iter)
		{
			iFolderHolder ifHolder = (iFolderHolder)model.GetValue(iter, 0);
			if (ifHolder != null && !ifHolder.iFolder.IsSubscription)
			{
				string searchString = synchronizedSearchEntry.Text;
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
		
//		private bool MyAvailableFoldersFilterFunc(TreeModel model, TreeIter iter)
//		{
//			iFolderHolder ifHolder = (iFolderHolder)model.GetValue(iter, 0);
//			return IsAvailableFolder(ifHolder, true);
//		}
		
//		private bool OtherAvailableFoldersFilterFunc(TreeModel model, TreeIter iter)
//		{
//			iFolderHolder ifHolder = (iFolderHolder)model.GetValue(iter, 0);
//			return IsAvailableFolder(ifHolder, false);
//		}
		
		private bool IsAvailableFolder(iFolderHolder ifHolder, bool owner)
		{
			if (ifHolder == null || ifHolder.iFolder == null || ifHolder.iFolder.DomainID == null) return false;

			DomainInformation domain = domainController.GetDomain(ifHolder.iFolder.DomainID);
			if (domain == null) return false;
			
			string myDomainUserID = domain.MemberUserID;

			if (ifHolder != null && ifHolder.iFolder.IsSubscription)
			{
				if ((owner && ifHolder.iFolder.OwnerID == myDomainUserID)
					|| (!owner && ifHolder.iFolder.OwnerID != myDomainUserID))
				{
					string searchString = synchronizedSearchEntry.Text;
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
			}
			
			return false;
		}
		
		private void OnSynchronizedFoldersButtonPressed(object o, ButtonPressEventArgs args)
		{
			switch(args.Event.Button)
			{
				case 1:	// First mouse button
					break;
				case 2:
					break;
				case 3: // right-click
/*
					TreePath tPath =
						synchronizedFoldersIconView.GetPathAtPos((int)args.Event.X,
														   (int)args.Event.Y);
					if (tPath != null)
					{
						TreeModel tModel = synchronizedFoldersIconView.Model;
						
						TreeIter iter;
						if (tModel.GetIter(out iter, tPath))
						{
							iFolderHolder holder =
								(iFolderHolder)tModel.GetValue(iter, 0);
							if (holder == null) return;

							Menu menu = new Menu();
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
										RemoveSynchronizedFolderHandler);
							}
							else if(holder.iFolder.OwnerID !=
											holder.iFolder.CurrentUserID)
							{
								MenuItem item_delete = new MenuItem (
										Util.GS("Revert to a normal folder"));
								menu.Append (item_delete);
								item_delete.Activated += new EventHandler(
										RemoveSynchronizedFolderHandler);
							}


							menu.Append(new SeparatorMenuItem());
	
							MenuItem item_properties =
								new MenuItem (Util.GS("Properties"));
							menu.Append (item_properties);
							item_properties.Activated +=
								new EventHandler(OnShowFolderProperties);

							menu.ShowAll();

							menu.Popup(null, null, null, 
								IntPtr.Zero, 3, 
								Gtk.Global.CurrentEventTime);
						}
					}
*/
					break;
			}
		}
		
		private void OnSynchronizedFoldersSelectionChanged(object o, EventArgs args)
		{
			iFolderHolder holder = (iFolderHolder)o;
Console.WriteLine("iFolderWindow.OnSynchronizedFoldersSelectionChanged()");
			if (holder == null)
			{
				SynchronizedFolderTasks.Visible = false;
//				detailsExpander.Visible = false;
			}
			else
			{
/*
				SynchronizedNameLabel.Markup =
					string.Format("<span size=\"small\" weight=\"bold\">{0}</span>", holder.iFolder.Name);
				OwnerLabel.Markup =
					string.Format("<span size=\"small\">{0}</span>", holder.iFolder.Owner);
				DomainInformation domain = domainController.GetDomain(holder.iFolder.DomainID);
				if (domain != null)
					ServerLabel.Markup =
						string.Format("<span size=\"small\">{0}</span>", domain.Name);
*/
				if (holder.iFolder.IsSubscription)
				{
					// Hide the Local iFolder Buttons
					OpenSynchronizedFolderButton.Visible	= false;
					SynchronizeNowButton.Visible			= false;
					ShareSynchronizedFolderButton.Visible	= false;
					ViewFolderPropertiesButton.Visible		= false;
					ResolveConflictsButton.Visible			= false;
					RemoveSynchronizedFolderButton.Visible	= false;

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
					RemoveSynchronizedFolderButton.Visible	= true;

					if (holder.iFolder.HasConflicts)
						ResolveConflictsButton.Visible = true;
					else
						ResolveConflictsButton.Visible = false;
				}

				SynchronizedFolderTasks.Visible = true;
				
//				detailsExpander.Visible = true;

				RemoveSynchronizedFolderButton.Sensitive = true;
			}
		}
		
		private void OnIconViewDragMotion(object o, DragMotionArgs args)
		{
//			Widget source = Gtk.Drag.GetSourceWidget(args.Context);
//			Console.WriteLine("OnIconViewDragMotion: {0}", source == null ? "null" : source.Name);
			
			Gdk.Drag.Status(args.Context, args.Context.SuggestedAction, args.Time);
			
			args.RetVal = true;
		}
		
		private void OnIconViewDragDrop(object o, DragDropArgs args)
		{
//			Widget source = Gtk.Drag.GetSourceWidget(args.Context);
//			Console.WriteLine("OnIconViewDragDrop: {0}", source == null ? "null" : source.Name);
			
			args.RetVal = true;
		}
		
		private void OnIconViewDragDataReceived(object o, DragDataReceivedArgs args)
		{
			Console.WriteLine("OnIconViewDragDataReceived: {0}", (TargetType)args.Info);
			bool bFolderCreated = false;
			
			switch (args.Info)
			{
				case (uint) TargetType.UriList:
					DomainInformation defaultDomain = domainController.GetDefaultDomain();
					if (defaultDomain == null) return;
	
					UriList uriList = new UriList(args.SelectionData);
					foreach (string path in uriList.ToLocalPaths())
					{
//						Console.WriteLine("\t{0}", path);
						if (ifws.CanBeiFolder(path))
						{
							try
							{
								iFolderHolder holder = ifdata.CreateiFolder(path, defaultDomain.ID);

//								TreeIter iter =
//									myiFoldersFilter.AppendValues(synchronizedFolderPixbuf,
//																			  holder.iFolder.Name,
//																			  holder);
//								curSynchronizedFolders[holder.iFolder.ID] = iter;
								bFolderCreated = true;
							}
							catch
							{
								Console.WriteLine("Error creating folder on drag-and-drop");
							}
						}
					}

//					if (bFolderCreated)
//						synchronizedFoldersIconView.RefreshIcons();
					break;
				default:
					break;
			}

			Gtk.Drag.Finish (args.Context, bFolderCreated, false, args.Time);
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
			tb.IconSize = IconSize.SmallToolbar;

			ToolbarTooltips = new Tooltips();

			NewButton = new ToolButton(
				new Image(new Gdk.Pixbuf(Util.ImagesPath("newifolder24.png"))),
				Util.GS("New"));
			NewButton.SetTooltip(ToolbarTooltips, Util.GS("Create a new iFolder"), "Toolbar/New iFolder");
			NewButton.Clicked += new EventHandler(NewiFolderHandler);
			tb.Insert(NewButton, -1);

			SetupButton = new ToolButton(
				new Image(new Gdk.Pixbuf(Util.ImagesPath("setup24.png"))),
				Util.GS("Set Up"));
			SetupButton.SetTooltip(ToolbarTooltips, Util.GS("Set up the selected iFolder"), "Toolbar/Set Up iFolder");
			SetupButton.Clicked += new EventHandler(SetupiFolderHandler);
			tb.Insert(SetupButton, -1);

			tb.Insert(new SeparatorToolItem(), -1);

			SyncButton = new ToolButton(
				new Image(new Gdk.Pixbuf(Util.ImagesPath("sync24.png"))),
				Util.GS("Synchronize"));
			SyncButton.SetTooltip(ToolbarTooltips, Util.GS("Synchronize the selected iFolder"), "Toolbar/Synchronize iFolder");
			SyncButton.Clicked += new EventHandler(SynciFolderHandler);
			tb.Insert(SyncButton, -1);

			ShareButton = new ToolButton(
				new Image(new Gdk.Pixbuf(Util.ImagesPath("share24.png"))),
				Util.GS("Share"));
			ShareButton.SetTooltip(ToolbarTooltips, Util.GS("Share the selected iFolder"), "Toolbar/Share iFolder");
			ShareButton.Clicked += new EventHandler(ShareiFolderHandler);
			tb.Insert(ShareButton, -1);

			ConflictButton = new ToolButton(
				new Image(new Gdk.Pixbuf(Util.ImagesPath("conflict24.png"))),
				Util.GS("Resolve"));
			ConflictButton.SetTooltip(ToolbarTooltips, Util.GS("Resolve conflicts in the selected iFolder"), "Toolbar/Resolve iFolder");
			ConflictButton.Clicked += new EventHandler(ResolveConflictHandler);
			tb.Insert(ConflictButton, -1);

			// FIXME: Figure out why if this next separator is added, the server filter combobox disappears
//			tb.Insert(new SeparatorToolItem(), -1);

			HBox domainFilterBox = new HBox();
			domainFilterBox.Spacing = 5;
			ToolItem domainFilterToolItem = new ToolItem();
			domainFilterToolItem.SetTooltip(ToolbarTooltips, Util.GS("Filter the list of iFolders by server"), "Toolbar/Server Filter");
			domainFilterToolItem.Add(domainFilterBox);

			Label l = new Label(Util.GS("Server:"));
			domainFilterBox.PackStart(l, false, false, 0);

			VBox domainFilterSpacerBox = new VBox();
			domainFilterBox.PackStart(domainFilterSpacerBox, false, false, 0);

			// We have to add a spacer before and after the option menu to get the
			// OptionMenu to size properly in the Toolbar.
			Label spacer = new Label("");
			domainFilterSpacerBox.PackStart(spacer, false, false, 0);
			
			DomainFilterComboBox = new ComboBox();
			domainFilterSpacerBox.PackStart(DomainFilterComboBox, false, false, 0);

			DomainListStore = new ListStore(typeof(DomainInformation));
			DomainFilterComboBox.Model = DomainListStore;
			
			CellRenderer domainTR = new CellRendererText();
			DomainFilterComboBox.PackStart(domainTR, true);

			DomainFilterComboBox.SetCellDataFunc(domainTR,
				new CellLayoutDataFunc(DomainFilterComboBoxCellTextDataFunc));

			DomainFilterComboBox.Changed += new EventHandler(DomainFilterChangedHandler);
			
			DomainFilterComboBox.ShowAll();

			spacer = new Label("");
			domainFilterSpacerBox.PackEnd(spacer, false, false, 0);

			tb.Insert(domainFilterToolItem, -1);

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

			NewMenuItem = new ImageMenuItem (Util.GS("_New"));
			NewMenuItem.Image = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder24.png")));
			iFolderMenu.Append(NewMenuItem);
			NewMenuItem.AddAccelerator("activate", agrp,
				new AccelKey(Gdk.Key.N, Gdk.ModifierType.ControlMask,
								AccelFlags.Visible));
			NewMenuItem.Activated += new EventHandler(NewiFolderHandler);

			SetupMenuItem =
				new MenuItem (Util.GS("_Set Up..."));
			iFolderMenu.Append(SetupMenuItem);
			SetupMenuItem.Activated += new EventHandler(SetupiFolderHandler);

			DeleteMenuItem =
				new ImageMenuItem (Util.GS("_Delete"));
			DeleteMenuItem.Image = new Image(Stock.Delete, Gtk.IconSize.Menu);
			iFolderMenu.Append(DeleteMenuItem);
			DeleteMenuItem.Activated += new EventHandler(OnRemoveiFolder);

			RemoveMenuItem =
				new ImageMenuItem (Util.GS("Re_move"));
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

			ConflictMenuItem = new MenuItem (Util.GS("Resolve conflic_ts"));
			iFolderMenu.Append(ConflictMenuItem);
			ConflictMenuItem.Activated +=
					new EventHandler(ResolveConflictHandler);

			SyncNowMenuItem = new MenuItem(Util.GS("S_ynchronize now"));
			iFolderMenu.Append(SyncNowMenuItem);
			SyncNowMenuItem.Activated += new EventHandler(SynciFolderHandler);

			RevertMenuItem = 
				new ImageMenuItem (Util.GS("_Revert to a Normal Folder"));
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
			
			AccountsMenuItem =
				new MenuItem (Util.GS("_Accounts"));
			ViewMenu.Append(AccountsMenuItem);
			AccountsMenuItem.Activated += new EventHandler(AccountsMenuItemHandler);

			SyncLogMenuItem =
				new MenuItem (Util.GS("Synchronization _Log"));
			ViewMenu.Append(SyncLogMenuItem);
			SyncLogMenuItem.Activated += new EventHandler(SyncLogMenuItemHandler);

			ViewMenu.Append(new SeparatorMenuItem());
			
			RemoteFoldersCheckMenuItem =
				new CheckMenuItem(Util.GS("Show _Remote Folders"));
			ViewMenu.Append(RemoteFoldersCheckMenuItem);

			// Load in the user preference for whether this is Active or not
			if (ClientConfig.Get(ClientConfig.KEY_SHOW_SERVER_IFOLDERS, "false")
					== "true")
				RemoteFoldersCheckMenuItem.Active = true;
			else
				RemoteFoldersCheckMenuItem.Active = false;

			RemoteFoldersCheckMenuItem.Toggled +=
				new EventHandler(RemoteFoldersCheckMenuItemToggled);

			ViewMenu.Append(new SeparatorMenuItem());
			
			MenuItem toggleDebugMenuItem =		// FIXME: Remove this debug menu
				new MenuItem(Util.GS("_Toggle Debug Interface"));
			ViewMenu.Append(toggleDebugMenuItem);
			toggleDebugMenuItem.Activated += new EventHandler(ToggleDebugInterface);
			
			MenuItem showColorPaletteMenuItem =
				new MenuItem(Util.GS("Show _Color Palette..."));
			ViewMenu.Append(showColorPaletteMenuItem);
			showColorPaletteMenuItem.Activated += ShowColorPalette;

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


			// Set up the iFolder TreeView
			iFolderTreeStore = new ListStore(typeof(iFolderHolder));
//			iFolderTreeView.Model = iFolderTreeStore;
			iFolderTreeView.Model = myiFoldersFilter;

			// Set up Pixbuf and Text Rendering for "iFolders" column
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


			// Set up Text Rendering for "Location" column
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


			// Set up Text Rendering for "Status" column
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
//			RefreshDomains(false);
//			RefreshiFolders(false);

			if (curDomains == null || curDomains.Length == 0)
				WindowNotebook.CurrentPage = 0;
			else
				WindowNotebook.CurrentPage = 1;
			
			OnSynchronizedFoldersSelectionChanged(null, EventArgs.Empty);
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

			((CellRendererText) cell).Text = ifHolder.StateString;
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

		private void DomainFilterComboBoxCellTextDataFunc(
				CellLayout cell_layout,
				CellRenderer cell,
				TreeModel tree_model,
				TreeIter iter)
		{
			DomainInformation domain =
				(DomainInformation)tree_model.GetValue(iter, 0);
			if (domain != null)
				((CellRendererText) cell).Text = domain.Name;
		}
		
		private void RemoteDomainComboBoxCellTextDataFunc(
				CellLayout cell_layout,
				CellRenderer cell,
				TreeModel tree_model,
				TreeIter iter)
		{
			// FIXME: Figure out how much space is available and truncate the server text if needed
			DomainInformation domain =
				(DomainInformation)tree_model.GetValue(iter, 0);
			if (domain != null)
				((CellRendererText) cell).Text = domain.Name;
		}

		private void SynchronizedDomainComboBoxCellTextDataFunc(
				CellLayout cell_layout,
				CellRenderer cell,
				TreeModel tree_model,
				TreeIter iter)
		{
			// FIXME: Figure out how much space is available and truncate the server text if needed
			DomainInformation domain =
				(DomainInformation)tree_model.GetValue(iter, 0);
			if (domain != null)
				((CellRendererText) cell).Text = domain.Name;
		}

//		private void OnHomePageLinkClicked(object o, LinkClickedArgs args)
//		{
//			// FIXME: Implement OnHomePageLinkClicked
//			Console.WriteLine("OnHomePageLinkClicked: {0}", args.Url);
//		}

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
					// Don't show subscriptions anymore (in this view)
//					if (holder.iFolder.IsSubscription) continue;
					
					if (curDomain == null || curDomain.ID == "0")
					{
						TreeIter iter = iFolderTreeStore.AppendValues(holder);
						curiFolders[holder.iFolder.CollectionID] = iter;
					}
					else if (curDomain.ID == holder.iFolder.DomainID)
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



		private void AccountsMenuItemHandler(object o, EventArgs args)
		{
			Util.ShowPrefsPage(1, simiasManager);
		}



		private void SyncLogMenuItemHandler(object o, EventArgs args)
		{
			Util.ShowLogWindow(simiasManager);
		}
		
		private void RemoteFoldersCheckMenuItemToggled(object o, EventArgs args)
		{
			Console.WriteLine("RemoteFoldersCheckMenuItemToggled: {0}",
							  RemoteFoldersCheckMenuItem.Active);
			
			// Save off this user preference
			if (RemoteFoldersCheckMenuItem.Active)
				ClientConfig.Set(ClientConfig.KEY_SHOW_SERVER_IFOLDERS, "true");
			else
				ClientConfig.Set(ClientConfig.KEY_SHOW_SERVER_IFOLDERS, "false");
		}
		
		
		private void ToggleDebugInterface(object o, EventArgs args)
		{
			myiFolderNotebook.ShowTabs = !myiFolderNotebook.ShowTabs;
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



		private void CloseWindow()
		{
			this.Hide();
		}
		
		
		private void QuitEventHandler(object o, EventArgs args)
		{
			Util.QuitiFolder();
		}
		
		
		private void ShowPreferencesHandler(object o, EventArgs args)
		{
			Util.ShowPrefsPage(0, simiasManager);
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
					if (ifHolder.iFolder.Role.Equals("Master"))
						RevertMenuItem.Sensitive = false;
					else
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
									new MenuItem(Util.GS("Synchronize Now"));
								ifMenu.Append (item_sync);
								item_sync.Activated += new EventHandler(
										SynciFolderHandler);

								if (!ifHolder.iFolder.Role.Equals("Master"))
								{
									MenuItem item_revert = new MenuItem (
											Util.GS("Revert to a Normal Folder"));
									ifMenu.Append (item_revert);
									item_revert.Activated += new EventHandler(
											OnRevertiFolder);
								}

								if(ifHolder.iFolder.OwnerID ==
												ifHolder.iFolder.CurrentUserID)
								{
									MenuItem item_delete = new MenuItem (
											Util.GS("Delete"));
									ifMenu.Append (item_delete);
									item_delete.Activated += new EventHandler(
											OnRemoveiFolder);
								}
								else
								{
									MenuItem item_delete = new MenuItem (
											Util.GS("Remove"));
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
									new MenuItem (Util.GS("Set Up..."));
								ifMenu.Append (item_accept);
								item_accept.Activated += new EventHandler(
										SetupiFolderHandler);

								if(ifHolder.iFolder.OwnerID ==
												ifHolder.iFolder.CurrentUserID)
								{
									MenuItem item_decline =
										new MenuItem(Util.GS("Delete"));
									ifMenu.Append (item_decline);
									item_decline.Activated += new EventHandler(
											OnRemoveiFolder);
								}
								else
								{
									MenuItem item_decline =
									new MenuItem (Util.GS("Remove"));
									ifMenu.Append (item_decline);
									item_decline.Activated += new EventHandler(
											OnRemoveiFolder);
								}
							}
							else
							{
								MenuItem item_decline =
									new MenuItem (Util.GS("Remove"));
								ifMenu.Append (item_decline);
								item_decline.Activated += new EventHandler(
										OnRemoveiFolder);
							}
						}
					}
					else
					{
						MenuItem item_create =
							new MenuItem (Util.GS("New..."));
						ifMenu.Append (item_create);
						item_create.Activated +=
							new EventHandler(NewiFolderHandler);

						MenuItem item_refresh =
							new MenuItem (Util.GS("Refresh"));
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
		
		private void OnOpenFolderMenu(object o, EventArgs args)
		{
			OpenSelectedFolder();
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
		
		
		private void OpenSelectedFolder()
		{
//			TreeSelection tSelect = synchronizedFoldersIconView.Selection;
//			if (tSelect != null && tSelect.CountSelectedRows() == 1)
//			{
//				TreeModel tModel;
//				TreeIter iter;
				
//				tSelect.GetSelected(out tModel, out iter);

//				iFolderHolder holder =
//					(iFolderHolder)tModel.GetValue(iter, 0);
				iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
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
//			}
		}

		private void ResolveSelectedFolderConflicts()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				ConflictDialog = new iFolderConflictDialog(
										this,
										holder.iFolder,
										ifws,
										simws);
				ConflictDialog.Response +=
							new ResponseHandler(OnConflictDialogResponse);
				ConflictDialog.ShowAll();
			}
		}

		private void SyncSelectedFolder()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
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
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				ShowSynchronizedFolderProperties(holder, 1);
			}
		}

		private void ShowSelectedFolderProperties()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				ShowSynchronizedFolderProperties(holder, 0);
			}
		}

		private void RemoveSelectedFolderHandler()
		{
			iFolderHolder holder = synchronizedFoldersIconView.SelectedFolder;
			if (holder != null)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"",
					Util.GS("Revert this iFolder to a normal folder?"),
					Util.GS("The folder will still be on your computer, but it will no longer synchronize with the iFolder Server."));
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
						ifdata.RevertiFolder(holder.iFolder.ID);
//								if (myiFoldersFilter.Remove(ref iter))
//								{
//									if (curSynchronizedFolders.ContainsKey(holder.iFolder.ID))
//										curSynchronizedFolders.Remove(holder.iFolder.ID);
//								}
								
//								synchronizedFoldersIconView.RefreshIcons();
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
						"",
						string.Format(Util.GS("Unable to open iFolder \"{0}\""), ifHolder.iFolder.Name),
						Util.GS("iFolder could not open the Nautilus File Manager or the Konquerer File Manager."));
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



		public void OnShowFolderProperties(object o, EventArgs args)
		{
			ShowSelectedFolderProperties();
		}

		public void OnShowFolderPropertiesOld(object o, EventArgs args)
		{
			TreeSelection tSelect = synchronizedFoldersIconView.Selection;
			if (tSelect != null && tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;
				
				tSelect.GetSelected(out tModel, out iter);

				iFolderHolder holder =
					(iFolderHolder)tModel.GetValue(iter, 0);
				if (holder != null)
				{
					ShowSynchronizedFolderProperties(holder, 0);
				}
			}
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

				if (ifHolder != null)
				{
					iFolderPropertiesDialog propsDialog =
						(iFolderPropertiesDialog) propDialogs[ifHolder.iFolder.ID];
					if (propsDialog == null)
					{
						try
						{
							propsDialog = 
								new iFolderPropertiesDialog(this, ifHolder.iFolder, ifws, simws, simiasManager);
							propsDialog.SetPosition(WindowPosition.Center);
							propsDialog.Response += 
									new ResponseHandler(OnPropertiesDialogResponse);
							propsDialog.CurrentPage = currentPage;
							propsDialog.ShowAll();
		
							propDialogs[ifHolder.iFolder.ID] = propsDialog;
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
						propsDialog.CurrentPage = currentPage;
					}
				}
			}
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
							Util.ShowHelp("front.html", this);
						}
					}
					break;
				default:
				{
					if(propsDialog != null)
					{
						propsDialog.Hide();
						propsDialog.Destroy();

						if (propDialogs.ContainsKey(propsDialog.iFolder.ID))
							propDialogs.Remove(propsDialog.iFolder.ID);

						propsDialog = null;
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
					"",
					Util.GS("Revert this iFolder to a normal folder?"),
					Util.GS("This reverts the iFolder back to a normal folder and leaves the files intact.  The iFolder is then available from the server and must be set up in a different location to synchronize."));
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
//    					iFolderHolder newHolder =
								ifdata.RevertiFolder(ifHolder.iFolder.ID);

//						newHolder.State = iFolderState.Initial;
//						iFolderTreeStore.SetValue(iter, 0, newHolder);
						if (iFolderTreeStore.Remove(ref iter))
						{
							if (curiFolders.ContainsKey(ifHolder.iFolder.ID))
								curiFolders.Remove(ifHolder.iFolder.ID);
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
Console.WriteLine("SetupiFolderHandler()");
			SetupiFolder();
		}
		
		private void SetupiFolder()
		{
Console.WriteLine("SetupiFolder() not implemented");
			// FIXME: Implement SetupiFolder()
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
		


		private int AskRemoveiFolder(iFolderHolder ifHolder)
		{
			int rc = 0;

			if (ifHolder.iFolder.Role != null && ifHolder.iFolder.Role.Equals("Master"))
			{
				// This machine is the Workgroup Server for this iFolder
				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"",
					string.Format(Util.GS("Remove iFolder {0}?"),
											ifHolder.iFolder.Name),
					Util.GS("This removes the iFolder from your local computer.  Because you are the owner, the iFolder is removed from all member computers.  The iFolder cannot be recovered or re-shared on another computer.  The files are not deleted from your local hard drive."));
				rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
			}
			else
			{
				if(ifHolder.iFolder.OwnerID == ifHolder.iFolder.CurrentUserID)
				{
					iFolderMsgDialog dialog = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						string.Format(Util.GS("Remove iFolder {0}?"),
												ifHolder.iFolder.Name),
						Util.GS("This removes the iFolder from your local computer.  Because you are the owner, the iFolder is removed from the server and all member computers.  The iFolder cannot be recovered or re-shared on another computer.  The files are not deleted from your local hard drive."));
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
						"",
						string.Format(Util.GS("Remove iFolder {0}?"),
												ifHolder.iFolder.Name),
						Util.GS("This removes you as a member of the iFolder.  You cannot access the iFolder unless the owner re-invites you.  The files are not deleted from your local hard drive."));
					rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
				}
			}
			return rc;
		}


		public void ResolveConflicts(string ifolderID)
		{
			// Guarantee that the iFolderWindow is showing
			Util.ShowiFolderWindow();

			// Select the specified available iFolder and call SetupiFolder().
			if(curiFolders.ContainsKey(ifolderID))
			{
				TreeIter iter = (TreeIter)curiFolders[ifolderID];
				TreeSelection tSelect = iFolderTreeView.Selection;
				tSelect.SelectIter(iter);
				ResolveConflicts();
			}
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

			// Refresh the selected iFolder to see if it has any more conflicts
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolderHolder ifHolder =
						(iFolderHolder) tModel.GetValue(iter, 0);

				iFolderHolder updatedHolder = null;
				updatedHolder = ifdata.ReadiFolder(ifHolder.iFolder.ID);
				if(updatedHolder != null)
					iFolderTreeStore.SetValue(iter, 0, updatedHolder);
			}

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
			iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);
			TreeIter iter;

			if(curiFolders.ContainsKey(iFolderID))
			{
				iter = (TreeIter)curiFolders[iFolderID];

				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}
			
//			if (curSynchronizedFolders.ContainsKey(iFolderID))
//			{
				// FIXME: Determine if the state has changed to know if we should update the pixbuf
//				iter = (TreeIter)curSynchronizedFolders[iFolderID];
//				myiFoldersFilter.SetValue(iter, 2, ifHolder);
//			}
			

			UpdateButtonSensitivity();
		}



		public void iFolderDeleted(string iFolderID)
		{
			TreeIter iter;
		
			if(curiFolders.ContainsKey(iFolderID))
			{
				iter = (TreeIter)curiFolders[iFolderID];
				iFolderTreeStore.Remove(ref iter);
				curiFolders.Remove(iFolderID);
			}

//			if (curSynchronizedFolders.ContainsKey(iFolderID))
//			{
				// FIXME: Determine if the state has changed to know if we should update the pixbuf
//				iter = (TreeIter)curSynchronizedFolders[iFolderID];
//				myiFoldersFilter.Remove(ref iter);
//			}
		}



		public void iFolderCreated(string iFolderID)
		{
Console.WriteLine("iFolderWindow.iFolderCreated()");
			iFolderHolder ifHolder = ifdata.GetiFolder(iFolderID);
			TreeIter iter;

Console.WriteLine("\t1");
			if(!curiFolders.ContainsKey(iFolderID))
			{
//				if (!ifHolder.iFolder.IsSubscription)
//				{
					if( (curDomain != null) &&
							(curDomain.ID != ifHolder.iFolder.DomainID) )
					{
						// don't do anything because we are not showing this
						// domain right now
					}
					else
					{
						iter = iFolderTreeStore.AppendValues(ifHolder);
						curiFolders[iFolderID] = iter;
					}
//				}
			}
			else
			{
				// just update with the current from ifdata
				iter = (TreeIter)curiFolders[iFolderID];
				ifHolder = ifdata.GetiFolder(iFolderID);
				iFolderTreeStore.SetValue(iter, 0, ifHolder);
			}

Console.WriteLine("\t2");
//			if (!curSynchronizedFolders.ContainsKey(iFolderID))
//			{
//Console.WriteLine("\t3");
//				if (!ifHolder.iFolder.IsSubscription)
//				{
//Console.WriteLine("\t4");
//					iter = myiFoldersFilter.AppendValues(synchronizedFolderPixbuf, ifHolder.iFolder.Name, ifHolder);
//					curSynchronizedFolders[iFolderID] = iter;
//				}
//			}
			

			UpdateButtonSensitivity();
		}


		private void iFolderDisplayPathHack(iFolderHolder ifHolder)
		{
			// This is kind of a hack
			// Sometimes, iFolders will be in the list but
			// they don't have the path.  Check for the path
			// here and if it is missing, re-read the ifolder
			// 'cause we'll have the path at this point
			if( (ifHolder.iFolder.UnManagedPath == null) ||
					(ifHolder.iFolder.UnManagedPath.Length == 0) )
			{
				iFolderHolder updatedHolder = null;
				updatedHolder = ifdata.ReadiFolder(ifHolder.iFolder.ID);
				if(updatedHolder != null)
					ifHolder = updatedHolder;
			}
		}


		public void HandleSyncEvent(CollectionSyncEventArgs args)
		{
			iFolderHolder ifHolder = null;
			TreeIter iter;
			iFolderTreeStore.GetIterFirst(out iter);	// Initialize iter
			if (curiFolders.ContainsKey(args.ID))
			{
				iter = (TreeIter)curiFolders[args.ID];
				ifHolder = (iFolderHolder) iFolderTreeStore.GetValue(iter,0);
			}

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

					if (ifHolder != null)
					{
						ifHolder.State = iFolderState.SynchronizingLocal;

						iFolderDisplayPathHack(ifHolder);
						iFolderTreeStore.SetValue(iter, 0, ifHolder);
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

					// Keep track of when a sync starts regardless of
					// whether the iFolder is currently shown because
					// if the user switches the iFolder Window domain
					// filter, we'll still need this.
					startingSync = true;

					if (ifHolder != null)
					{
						ifHolder.State = iFolderState.Synchronizing;

						iFolderDisplayPathHack(ifHolder);
						iFolderTreeStore.SetValue(iter, 0, ifHolder);
					}
					break;
				}
				case Simias.Client.Event.Action.StopSync:
				{
					if(SyncBar != null)
						SyncBar.Hide();

					if (ifHolder != null)
					{
						try
						{
							SyncSize syncSize = ifws.CalculateSyncSize(args.ID);
							objectsToSync = syncSize.SyncNodeCount;
							ifHolder.ObjectsToSync = objectsToSync;
						}
						catch
						{}

						if (ifHolder.ObjectsToSync > 0)
							ifHolder.State = iFolderState.Normal;
						else
						{
							if (args.Connected)
								ifHolder.State = iFolderState.Normal;
							else
								ifHolder.State = iFolderState.Disconnected;
						}

						iFolderDisplayPathHack(ifHolder);
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
//			if (ifHolder != null &&
//				PropertiesDialog != null && 
//				PropertiesDialog.iFolder.ID == args.ID)
			if (ifHolder != null)
			{
				iFolderPropertiesDialog propsDialog =
					(iFolderPropertiesDialog) propDialogs[ifHolder.iFolder.ID];
				if (propsDialog != null)
				{
					propsDialog.UpdateiFolder(ifHolder.iFolder);
				}
			}
		}


		public void HandleFileSyncEvent(FileSyncEventArgs args)
		{
			if (args.SizeRemaining == args.SizeToSync)
			{
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

					// Decrement the count whether we're showing the iFolder
					// in the current list or not.  We'll need this if the
					// user switches back to the list that contains the iFolder
					// that is actually synchronizing.
					if (objectsToSync <= 0)
						objectsToSync = 0;
					else
						objectsToSync--;
	
					// Get the iFolderHolder and set the objectsToSync (only if the
					// domain filter isn't set or is for this iFolder's domain.
					iFolderHolder ifHolder = ifdata.GetiFolder(args.CollectionID);
					if (ifHolder != null && (curDomain == null || curDomain.ID == "0" || curDomain.ID == ifHolder.iFolder.DomainID))
					{
						ifHolder.ObjectsToSync = objectsToSync;
						TreeIter iter = (TreeIter)curiFolders[args.CollectionID];
						iFolderTreeStore.SetValue(iter, 0, ifHolder);

//						myiFoldersFilter.Refilter();
					}
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


		public void SetUpiFolder(string ifolderID)
		{
Console.WriteLine("SetUpiFolder({0})", ifolderID);
			// Guarantee that the iFolderWindow is showing
			Util.ShowiFolderWindow();

			// Select the specified available iFolder and call SetupiFolder().
			if(curiFolders.ContainsKey(ifolderID))
			{
				TreeIter iter = (TreeIter)curiFolders[ifolderID];
				TreeSelection tSelect = iFolderTreeView.Selection;
				tSelect.SelectIter(iter);
				SetupiFolder();
			}
		}
		
		private void DownloadFolder(iFolderHolder holder)
		{
			if (holder == null) return;
			
			try
			{
				string lastSetupPath = Util.LastSetupPath;
				iFolderHolder newHolder = ifdata.AcceptiFolderInvitation(
													holder.iFolder.ID,
													holder.iFolder.DomainID,
													lastSetupPath);

				if (newHolder != null)
				{
//					TreeIter iter;
//					iter = myiFoldersFilter.AppendValues(synchronizedFolderPixbuf, newHolder.iFolder.Name, newHolder);
//					curSynchronizedFolders[holder.iFolder.ID] = iter;
	
//					synchronizedFoldersIconView.RefreshIcons();
				}
			}
			catch(Exception e)
			{
				DisplayCreateOrSetupException(e);
			}
		}

		private void CreateNewiFolder()
		{
			// Re-read the data in case a new domain has been created
			ifdata.RefreshDomains();

			if(ifdata.GetDomainCount() < 1)
			{
				// Prompt the user about there not being any domains
				iFolderWindow ifwin = Util.GetiFolderWindow();
				iFolderMsgDialog dg = new iFolderMsgDialog(
					ifwin,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"",
					Util.GS("Set up an iFolder account?"),
					Util.GS("To begin using iFolder, you must first set up an iFolder account."));
				int response = dg.Run();
				dg.Hide();
				dg.Destroy();
				if (response == -8)
				{
					Util.ShowPrefsPage(1, simiasManager);
				}
				
				return;
			}

			DomainInformation[] domains = ifdata.GetDomains();
			string domainID = null;
			if (curDomain == null)
			{
				DomainInformation defaultDomain = ifdata.GetDefaultDomain();
				if (defaultDomain != null)
					domainID = defaultDomain.ID;
			}
			else
				domainID = curDomain.ID;

			CreateDialog cd = new CreateDialog(this, domains, domainID, Util.LastCreatedPath, ifws);
			cd.TransientFor = this;
	
			int rc = 0;
			do
			{
				rc = cd.Run();
				cd.Hide();

				if (rc == (int)ResponseType.Ok)
//				if(rc == -5)
				{
					try
					{
						string selectedFolder = cd.iFolderPath.Trim();
						string selectedDomain = cd.DomainID;
	
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
							ifHolder = 
								ifdata.CreateiFolder(	selectedFolder,
														selectedDomain);
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
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						continue;
					}
				}
			}
			while(rc == (int)ResponseType.Ok);
//			while(rc == -5);
		}

		public void DomainFilterChangedHandler(object o, EventArgs args)
		{
			// Change the global "domainSelected" (null if "All" is chosen by
			// the user) and then make the call to refresh the window.
			if (curDomains != null)
			{
				TreeIter iter;
				
				if (DomainFilterComboBox.GetActiveIter(out iter))
				{
					DomainInformation tmpDomain = (DomainInformation)
						DomainFilterComboBox.Model.GetValue(iter, 0);
					if (tmpDomain.ID == "0")
						curDomain = null;
					else
						curDomain = tmpDomain;
				}
				else
				{
					curDomain = null;
				}
				
				RefreshiFolders(false);
			}
		}

		public void RefreshDomains(bool readFromSimias)
		{
			if(readFromSimias)
				ifdata.RefreshDomains();
			
			DomainListStore.Clear();

			curDomains = ifdata.GetDomains();
			if (curDomains != null)
			{
				DomainInformation selectAllDomain = new DomainInformation();
				selectAllDomain.ID = "0";
				selectAllDomain.Name = Util.GS("Show All");
				DomainListStore.AppendValues(selectAllDomain);

				foreach(DomainInformation domain in curDomains)
				{
					DomainListStore.AppendValues(domain);
				}
			}
			
			DomainFilterComboBox.Active = 0;		// Show All
//			synchronizedDomainsComboBox.Active = 0;	// Show All
//			remoteDomainsComboBox.Active = 0;		// Show All
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
		
		private void OnDomainAddedEvent(object sender, DomainEventArgs args)
		{
			RefreshDomains(true);
			RefreshiFolders(true);
			
			if (curDomains == null || curDomains.Length == 0)
				WindowNotebook.CurrentPage = 0;
			else
				WindowNotebook.CurrentPage = 1;
			
			AddServerGroup(args.DomainID);
		}
		
		private void OnDomainDeletedEvent(object sender, DomainEventArgs args)
		{
			RefreshDomains(true);
			RefreshiFolders(true);
			
			if (curDomains == null || curDomains.Length == 0)
				WindowNotebook.CurrentPage = 0;
			else
				WindowNotebook.CurrentPage = 1;
			
			if (serverGroups.ContainsKey(args.DomainID))
				serverGroups.Remove(args.DomainID);
			
			if (serverGroupFilters.ContainsKey(args.DomainID))
				serverGroupFilters.Remove(args.DomainID);
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
					Console.WriteLine ("uri = {0}", s);
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
