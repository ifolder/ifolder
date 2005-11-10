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

using Novell.iFolder.Events;
using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	/// <summary>
	/// iFolder states.
	/// </summary>
	public enum iFolderState
	{
		/// <summary>
		/// Initial state before anything has happened
		/// </summary>
		Initial,

		/// <summary>
		/// The Normal state.
		/// </summary>
		Normal,

		/// <summary>
		/// The Synchronizing state.
		/// </summary>
		Synchronizing,

		/// <summary>
		/// The FailedSync state.
		/// </summary>
		FailedSync,

		/// <summary>
		/// Synchronizing with the local store.
		/// </summary>
		SynchronizingLocal,

		/// <summary>
		/// Unable to connect to the server.
		/// </summary>
		Disconnected
	}
	
	public enum HomeActionItem
	{
		AddFolderToSync,
		SearchRemoteFolder,
		ConnectToNewServer
	}
	
	public enum RemoteActionItem
	{
		DownloadFolder,
		DownloadAndSyncFolder,
		DeleteFolder
	}

	/// <summary>
	/// This is a holder class for iFolders so the client can place
	/// extra data with an iFolder about it's status and such.
	/// </summary>
	public class iFolderHolder
	{
		private iFolderWeb		ifolder;
		private iFolderState	state;
		private string			stateString;
		private string			path;
		private uint			objectsToSync;

		public iFolderHolder(iFolderWeb ifolder)
		{
			this.ifolder	= ifolder;
			state			= iFolderState.Initial;
			objectsToSync	= 0;
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

		public string Path
		{
			get{ return path; }
		}

		public string StateString
		{
			get{ return stateString; }
		}

		public iFolderState State
		{
			get{ return state; }
			set
			{
				this.state = value;
				UpdateDisplayData();
			}
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
			if (iFolder.HasConflicts)
			{
				stateString = Util.GS("Has conflicts");
			}
			else
			{
				switch (state)
				{
					case iFolderState.Initial:
						switch (iFolder.State)
						{
							case "Available":
								stateString = Util.GS("Not set up");
								break;
							case "WaitConnect":
								stateString = Util.GS("Waiting to connect");
								break;
							case "WaitSync":
								stateString = Util.GS("Waiting to synchronize");
								break;
							case "Local":
								stateString = Util.GS("OK");
								break;
							default:
								stateString = Util.GS("Unknown");
								break;
						}
						break;
					case iFolderState.Normal:
						if (objectsToSync > 0)
							stateString = string.Format(Util.GS("{0} items not synchronized"), objectsToSync);
						else
							stateString = Util.GS("OK");
						break;
					case iFolderState.Synchronizing:
						if (objectsToSync > 0)
							stateString = string.Format(Util.GS("{0} items to synchronize"), objectsToSync);
						else
							stateString = Util.GS("Synchronizing");
						break;
					case iFolderState.FailedSync:
						stateString = Util.GS("Incomplete synchronization");
						break;
					case iFolderState.SynchronizingLocal:
						stateString = Util.GS("Checking for changes");
						break;
					case iFolderState.Disconnected:
						stateString = Util.GS("Server unavailable");
						break;
					default:
						stateString = Util.GS("Unknown");
						break;
				}
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

		private iFolderConflictDialog ConflictDialog;

		private Hashtable			curiFolders;

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
		
//		private iFolderNotebook		myiFolderNotebook;
		private Notebook			myiFolderNotebook;
		
		///
		/// Home Page
		///
		private Button				AddFolderToSyncHomeButton;
		private Button				SearchRemoteFoldersHomeButton;
		private Button				ConnectToServerHomeButton;

		///
		/// Remote Folders
		///
		private ComboBox			remoteDomainsComboBox;
		private Entry				remoteSearchEntry;

		private Paned				remotePaned;
		
		private Notebook			RemoteDetailsNotebook;
		
		private ListStore			remoteFoldersListStore;
		private iFolderIconView		remoteFoldersIconView;

		private Button				DownloadAndSyncButton;
		private Button				DeleteFromServerButton;
		
		private Label				RemoteNameLabel;
		private Label				RemoteSizeLabel;
		private Label				RemoteOwnerLabel;
		private Label				RemoteServerLabel;
		private TextView			RemoteDescriptionTextView;


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
			curDomain = null;
			curDomains = null;

			propDialogs = new Hashtable();
			
			CreateWidgets();

			RefreshDomains(true);
			RefreshiFolders(true);
			
			domainController = DomainController.GetDomainController(simiasManager);
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

			VBox vbox = new VBox (false, 0);
			this.Add (vbox);

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
		
		
		
//		private iFolderNotebook CreateiFolderNotebook()
		private Notebook CreateiFolderNotebook()
		{
//			iFolderNotebook notebook = new iFolderNotebook();
			Notebook notebook = new Notebook();
		
			VBox vbox = new VBox(false, 2);
			Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(Util.ImagesPath("home-temp.png"));
			pixbuf = pixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			Image image = new Image(pixbuf);
			vbox.PackStart(image, false, false, 0);
			image.SetAlignment(0.5F, 0.5F);
			Label l = new Label(string.Format("<span weight=\"bold\">{0}</span>", Util.GS("Home")));
			vbox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			vbox.ShowAll();
			
			notebook.AppendPage(CreateHomePage(), vbox);
			
//			ToggleToolButton homeButton = new ToggleToolButton(Stock.Home);
//			notebook.AppendPage(CreateHomePage(), homeButton);
			
			vbox = new VBox(false, 2);
			pixbuf = new Gdk.Pixbuf(Util.ImagesPath("synchronized-folder64.png"));
			image = new Image(pixbuf);
			vbox.PackStart(image, false, false, 0);
			image.SetAlignment(0.5F, 0.5F);
			l = new Label(string.Format("<span weight=\"bold\">{0}</span>", Util.GS("Synchronized Folders")));
			vbox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.LineWrap = true;
			vbox.ShowAll();
			
			notebook.AppendPage(CreateSynchronizedFoldersPage(), vbox);
			
//			ToggleToolButton availableButton = new ToggleToolButton(Stock.Open);
//			availableButton.Label = Util.GS("Synchronized Folders");
//			notebook.AppendPage(new Label("Not implemented"), availableButton);
//			notebook.AppendPage(CreateSynchronizedFoldersPage(), availableButton);
			
			
			vbox = new VBox(false, 2);
			pixbuf = new Gdk.Pixbuf(Util.ImagesPath("remote-folder64.png"));
			image = new Image(pixbuf);
			vbox.PackStart(image, false, false, 0);
			image.SetAlignment(0.5F, 0.5F);
			l = new Label(string.Format("<span weight=\"bold\">{0}</span>", Util.GS("Remote Folders")));
			vbox.PackStart(l, false, false, 0);
			l.UseMarkup = true;
			l.LineWrap = true;
			vbox.ShowAll();
			
			notebook.AppendPage(CreateRemoteFoldersPage(), vbox);

//			ToggleToolButton remoteButton = new ToggleToolButton(Stock.Network);
//			remoteButton.Label = Util.GS("Remote Folders");
//			notebook.AppendPage(CreateRemoteFoldersPage(), remoteButton);
			
			notebook.CurrentPage = 0;

			
			return notebook;
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
			
			// folder128.png
			Gdk.Pixbuf folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("add-folder-to-synchronize.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			Image folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			VBox buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			Label buttonText = new Label(string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", Util.GS("Add a folder to synchronize")));
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			Label buttonMessage = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Choose a folder on your computer to synchronize with an iFolder Server")));
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
			folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("search-folder.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			buttonText = new Label(string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", Util.GS("Search for remote folders")));
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			buttonMessage = new Label(string.Format("<span size=\"small\">{0}</span>", Util.GS("Search for remote folders to synchronize to your computer")));
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
			myiFolderNotebook.CurrentPage = 1;
			CreateNewiFolder();
		}

		private void OnSearchRemoteFoldersHomeButton(object o, EventArgs args)
		{
			myiFolderNotebook.CurrentPage = 2;
		}

		private void OnConnectToServerHomeButton(object o, EventArgs args)
		{
			Util.ShowPrefsPage(1, simiasManager);
		}

		


		private Widget CreateSynchronizedFoldersPage()
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
		
		
		
		private Widget CreateRemoteFoldersPage()
		{
			VBox vbox = new VBox(false, 0);
			vbox.PackStart(CreateRemoteToolbar(), false, false, 0);
			vbox.PackStart(CreateRemotePaned(), true, true, 0);

			return vbox;
		}
		
		private Widget CreateRemoteToolbar()
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

			remoteDomainsComboBox = new ComboBox();
			hbox.PackStart(remoteDomainsComboBox, false, false, 0);

			remoteDomainsComboBox.Model = DomainListStore;
			
			CellRenderer domainTR = new CellRendererText();
			remoteDomainsComboBox.PackStart(domainTR, true);

			remoteDomainsComboBox.SetCellDataFunc(domainTR,
				new CellLayoutDataFunc(RemoteDomainComboBoxCellTextDataFunc));

			remoteDomainsComboBox.ShowAll();

			hbox.PackStart(new Label(""), true, true, 0); // spacer

			remoteSearchEntry = new Entry();
			hbox.PackEnd(remoteSearchEntry, false, false, 0);
		
			l = new Label(Util.GS("Search:"));
			hbox.PackEnd(l, false, false, 0);
			
			return toolbarFrame;
		}
		
		private Widget CreateRemotePaned()
		{
			remotePaned = new HPaned();
			remotePaned.Position = 220;
			
			remotePaned.Add1(CreateRemoteActionsPane());
			remotePaned.Add2(CreateRemoteIconViewPane());
			
			return remotePaned;
		}
		
		private Widget CreateRemoteActionsPane()
		{
			RemoteDetailsNotebook = new Notebook();
			RemoteDetailsNotebook.ShowTabs = false;
			RemoteDetailsNotebook.AppendPage(new Label("Select a folder"), null);
			
			
			VBox vbox = new VBox(false, 0);
			RemoteDetailsNotebook.AppendPage(vbox, null);
			vbox.PackStart(CreateRemoteInfo(), false, false, 0);
			vbox.PackStart(CreateRemoteActions(), false, false, 0);
			vbox.PackStart(CreateRemoteDetails(), true, true, 0);
			
			return RemoteDetailsNotebook;
		}
		
		private Widget CreateRemoteInfo()
		{
			VBox vbox = new VBox(false, 0);
			
			// folder128.png
			Gdk.Pixbuf folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("folder128.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			Image folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0);
			
			vbox.PackStart(folderImage, false, false, 0);

			RemoteNameLabel = new Label("");
			RemoteNameLabel.UseMarkup = true;
			RemoteNameLabel.UseUnderline = false;
			RemoteNameLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteNameLabel, false, true, 5);

			RemoteSizeLabel = new Label("47 MB");
			RemoteSizeLabel.UseMarkup = true;
			RemoteSizeLabel.UseUnderline = false;
			RemoteSizeLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteSizeLabel, false, true, 0);
			
			RemoteOwnerLabel = new Label("");
			RemoteOwnerLabel.UseMarkup = true;
			RemoteOwnerLabel.UseUnderline = false;
			RemoteOwnerLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteOwnerLabel, false, true, 0);
			
			RemoteServerLabel = new Label("");
			RemoteServerLabel.UseMarkup = true;
			RemoteServerLabel.UseUnderline = false;
			RemoteServerLabel.Xalign = 0.5F;
			vbox.PackStart(RemoteServerLabel, false, true, 0);
			
			return vbox;
		}
		
		private Widget CreateRemoteActions()
		{
			VBox vbox = new VBox(false, 0);
			vbox.PackStart(new Label(""), false, false, 0); // spacer

			///
			/// DownloadAndSyncButton
			///
			HBox hbox = new HBox(false, 0);
			DownloadAndSyncButton = new Button(hbox);
			DownloadAndSyncButton.Relief = ReliefStyle.None;
//			DownloadAndSyncButton.Sensitive = false;
			vbox.PackStart(DownloadAndSyncButton, false, false, 0);
			
			// folder128.png
			Gdk.Pixbuf folderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("folder128.png"));
			folderPixbuf = folderPixbuf.ScaleSimple(24, 24, Gdk.InterpType.Bilinear);
			Image folderImage = new Image(folderPixbuf);
			folderImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(folderImage, false, false, 0);
			
			VBox buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			Label buttonText = new Label("<span size=\"small\" weight=\"bold\">Download and synchronize</span>");
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			Label buttonMessage = new Label("<span size=\"x-small\">Click here to download and begin synchronizing this folder to your computer</span>");
			buttonVBox.PackStart(buttonMessage, false, false, 0);
			buttonMessage.UseMarkup = true;
			buttonMessage.UseUnderline = false;
			buttonMessage.LineWrap = true;
			buttonMessage.Justify = Justification.Left;
			buttonMessage.Xalign = 0;
			buttonMessage.Yalign = 0;
			buttonMessage.WidthRequest = 170;
			buttonMessage.HeightRequest = 40;
			
			DownloadAndSyncButton.Clicked +=
				new EventHandler(OnDownloadAndSyncButton);
						
			
			///
			/// DeleteFromServerButton
			///
			hbox = new HBox(false, 0);
			DeleteFromServerButton = new Button(hbox);
			DeleteFromServerButton.Relief = ReliefStyle.None;
//			DeleteFromServerButton.Sensitive = false;
			vbox.PackStart(DeleteFromServerButton, false, false, 0);
						
			Image deleteImage = new Image(Stock.Delete, Gtk.IconSize.Menu);
			deleteImage.SetAlignment(0.5F, 0F);
			hbox.PackStart(deleteImage, false, false, 0);
			
			buttonVBox = new VBox(false, 0);
			hbox.PackStart(buttonVBox, true, true, 4);
			
			buttonText = new Label("<span size=\"small\" weight=\"bold\">Delete from server</span>");
			buttonVBox.PackStart(buttonText, false, false, 0);
			buttonText.UseMarkup = true;
			buttonText.UseUnderline = false;
			buttonText.Xalign = 0;
			
			buttonMessage = new Label("<span size=\"x-small\">Click here to delete this folder and its files from the server</span>");
			buttonVBox.PackStart(buttonMessage, false, false, 0);
			buttonMessage.UseMarkup = true;
			buttonMessage.UseUnderline = false;
			buttonMessage.LineWrap = true;
			buttonMessage.Justify = Justification.Left;
			buttonMessage.Xalign = 0;
			buttonMessage.Yalign = 0;
			buttonMessage.WidthRequest = 170;
			buttonMessage.HeightRequest = 25;
			
			
			return vbox;
		}
		
		private void OnDownloadAndSyncButton(object o, EventArgs args)
		{
			myiFolderNotebook.CurrentPage = 1;
			SetupiFolder();
		}
		
		private Widget CreateRemoteDetails()
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

			RemoteDescriptionTextView = new TextView();
			RemoteDescriptionTextView.Editable = false;
			RemoteDescriptionTextView.Sensitive = false;
			RemoteDescriptionTextView.WrapMode = WrapMode.Word;
			sw.Add(RemoteDescriptionTextView);
			
			return vbox;
		}
		
		private Widget CreateRemoteIconViewPane()
		{
			ScrolledWindow sw = new ScrolledWindow();
//			sw.ShadowType = Gtk.ShadowType.EtchedIn;

			remoteFoldersListStore = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(iFolderHolder));
			remoteFoldersIconView = new iFolderIconView(remoteFoldersListStore);
			remoteFoldersIconView.SelectionMode = SelectionMode.Single;

			remoteFoldersIconView.ButtonPressEvent +=
				new ButtonPressEventHandler(OnRemoteFoldersButtonPressed);
			
			remoteFoldersIconView.SelectionChanged +=
				new EventHandler(OnRemoteFoldersSelectionChanged);

			sw.Add(remoteFoldersIconView);
			
			Gdk.Pixbuf remoteFolderPixbuf = new Gdk.Pixbuf(Util.ImagesPath("folder128.png"));
			remoteFolderPixbuf = remoteFolderPixbuf.ScaleSimple(64, 64, Gdk.InterpType.Bilinear);
			iFolderHolder[] ifolders = ifdata.GetiFolders();
			if(ifolders != null)
			{
				foreach(iFolderHolder holder in ifolders)
				{
					if (holder.iFolder.IsSubscription)
					{
						remoteFoldersListStore.AppendValues(remoteFolderPixbuf, holder.iFolder.Name, holder);
					}
				}
			}

			return sw;
		}
		
//		private Widget GetRemoteActionPane()
//		{
//		}
		
//		private Widget GetRemoteIconView()
//		{
//		}
		
//		private Widget GetRemoteToolbar()
//		{
//		}



		private void OnRemoteFoldersButtonPressed(object o, ButtonPressEventArgs args)
		{
			TreePath tPath =
				remoteFoldersIconView.GetPathAtPos((int)args.Event.X,
												   (int)args.Event.Y);
			if (tPath != null)
			{
				TreeModel tModel = remoteFoldersIconView.Model;
				
				TreeIter iter;
				if (tModel.GetIter(out iter, tPath))
				{
					string folderName =
						(string)tModel.GetValue(iter, 1);
					if (folderName != null)
						Console.WriteLine("Folder clicked: {0}", folderName);
				}
			}
		}
		
		private void OnRemoteFoldersSelectionChanged(object o, EventArgs args)
		{
Console.WriteLine("iFolderWindow.OnRemoteFoldersSelectionChanged()");
			TreePath[] selection = remoteFoldersIconView.SelectedItems;
			if (selection.Length == 0)
			{
//				DownloadAndSyncButton.Sensitive = false;
//				DeleteFromServerButton.Sensitive = false;
				
				RemoteDetailsNotebook.CurrentPage = 0;
			}
			else
			{
				TreeModel tModel = remoteFoldersIconView.Model;
				for (int i = 0; i < selection.Length; i++)
				{
					TreeIter iter;
					if (tModel.GetIter(out iter, selection[i]))
					{
						iFolderHolder holder =
							(iFolderHolder)tModel.GetValue(iter, 2);
						if (holder != null)
						{
							RemoteNameLabel.Markup =
								string.Format("<span size=\"large\" weight=\"bold\">{0}</span>", holder.iFolder.Name);
							RemoteSizeLabel.Markup =
								string.Format("{0} MB", "47");
							RemoteOwnerLabel.Markup =
								string.Format("Owner: {0}", holder.iFolder.Owner);
							DomainInformation domain = domainController.GetDomain(holder.iFolder.DomainID);
							if (domain != null)
								RemoteServerLabel.Markup =
									string.Format("Server: {0}", domain.Name);
							else
								RemoteServerLabel.Text = "";
							
							if (holder.iFolder.Description != null)
								RemoteDescriptionTextView.Buffer.Text = holder.iFolder.Description;
							else
								RemoteDescriptionTextView.Buffer.Text = "";
						}
					}
				}

//				DownloadAndSyncButton.Sensitive = true;
//				DeleteFromServerButton.Sensitive = true;

				RemoteDetailsNotebook.CurrentPage = 1;
			}
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
			iFolderTreeView.Model = iFolderTreeStore;

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
					if (holder.iFolder.IsSubscription) continue;
					
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
    					iFolderHolder newHolder =
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
			SetupiFolder();
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
			Util.ShowiFolderWindow(simiasManager);

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
				
				// Ignore subscriptions
				if (ifHolder.iFolder.IsSubscription) return;

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
			// Guarantee that the iFolderWindow is showing
			Util.ShowiFolderWindow(simiasManager);

			// Select the specified available iFolder and call SetupiFolder().
			if(curiFolders.ContainsKey(ifolderID))
			{
				TreeIter iter = (TreeIter)curiFolders[ifolderID];
				TreeSelection tSelect = iFolderTreeView.Selection;
				tSelect.SelectIter(iter);
				SetupiFolder();
			}
		}

		private void SetupiFolder()
		{
			string newPath  = "";
			TreeModel tModel;
			TreeIter iter;

			TreePath[] selectedPaths = remoteFoldersIconView.SelectedItems;
			if (selectedPaths.Length != 1) return;

			tModel = remoteFoldersIconView.Model;
			if (tModel.GetIter(out iter, selectedPaths[0]))
			{
				iFolderHolder holder =
					(iFolderHolder)tModel.GetValue(iter, 2);
				if (holder == null) return;

				int rc = 0;

				do
				{
					string lastSetupPath = Util.LastSetupPath;
					iFolderAcceptDialog iad =
							new iFolderAcceptDialog(holder.iFolder, lastSetupPath);
					iad.TransientFor = this;
					rc = iad.Run();
					newPath = ParseAndReplaceTildeInPath(iad.Path);
					iad.Hide();
					iad.Destroy();
					if(rc != -5)
						return;

					try
					{
						// This will remove the current subscription
						// Read the updated subscription, and place it back
						// in the list to show status until the real iFolder
						// comes along
	//					curiFolders.Remove(holder.iFolder.ID);
	
						iFolderHolder newHolder = ifdata.AcceptiFolderInvitation(
														holder.iFolder.ID,
														holder.iFolder.DomainID,
														newPath);
	
						tModel.SetValue(iter, 0, newHolder);
	//					curiFolders.Add(newiFolder.ID, iter);

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
		}




		private void CreateNewiFolder()
		{
			// Re-read the data in case a new domain has been created
			ifdata.RefreshDomains();

			if(ifdata.GetDomainCount() < 1)
			{
				// Prompt the user about there not being any domains
				iFolderWindow ifwin = Util.GetiFolderWindow(simiasManager);
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
			
			DomainFilterComboBox.Active = 0;	// Show All
			remoteDomainsComboBox.Active = 0;	// Show All
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
		}
		
		private void OnDomainDeletedEvent(object sender, DomainEventArgs args)
		{
			RefreshDomains(true);
			RefreshiFolders(true);
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
