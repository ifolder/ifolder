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
		private Gdk.Pixbuf			ServeriFolderPixBuf;
		private Gdk.Pixbuf			ConflictPixBuf;

		private Statusbar			MainStatusBar;
		private Gtk.Notebook		MainNoteBook;
		private Gtk.TreeView		iFolderTreeView;
		private Gtk.ListStore		iFolderTreeStore;

		// Preferences widgets
		private Gtk.Button			AutoSyncCheckButton;
		private Gtk.Button			StartAtLoginButton;
		private Gtk.Button			ShowConfirmationButton; 
		private Gtk.Button			UseProxyButton; 

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

		private iFolderConflictDialog ConflictDialog;
		private iFolderPropertiesDialog PropertiesDialog;

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
			Image iFolderImage = new Image(
					new Gdk.Pixbuf(Util.ImagesPath("ifolder-banner.png")));
			vbox.PackStart (iFolderImage, false, false, 0);


			//-----------------------------
			// Create Tabs
			//-----------------------------
			MainNoteBook = new Notebook();
			MainNoteBook.AppendPage(	CreateiFoldersPage(), 
										new Label("iFolders"));
			MainNoteBook.AppendPage( CreatePreferencesPage(),
										new Label("Preferences"));
			MainNoteBook.AppendPage( CreateLogPage(),
										new Label("Activity Log"));

			vbox.PackStart(MainNoteBook, true, true, 0);


			//-----------------------------
			// Create Status Bar
			//-----------------------------
			MainStatusBar = new Statusbar ();
			UpdateStatus("Idle...");

			vbox.PackStart (MainStatusBar, false, false, 0);

			//-----------------------------
			// Set Menu Status
			//-----------------------------
			CreateMenuItem.Sensitive = true;
			ShareMenuItem.Sensitive = false;
			OpenMenuItem.Sensitive = false;
			ConflictMenuItem.Sensitive = false;
			RevertMenuItem.Sensitive = false;
			PropMenuItem.Sensitive = false;;

			// Setup an event to refresh when the window is
			// being drawn
			this.Realized += new EventHandler(OnRealizeWidget);
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
			CreateMenuItem.Activated += new EventHandler(OnCreateiFolder);

			iFolderMenu.Append(new SeparatorMenuItem());
			OpenMenuItem = new ImageMenuItem ( Stock.Open, agrp );
			iFolderMenu.Append(OpenMenuItem);
			OpenMenuItem.Activated += new EventHandler(OnOpeniFolderMenu);

			ShareMenuItem = new MenuItem ("Share _with...");
			iFolderMenu.Append(ShareMenuItem);
			ShareMenuItem.Activated += new EventHandler(OnShareProperties);

			ConflictMenuItem = new MenuItem ("Re_solve Conflicts");
			iFolderMenu.Append(ConflictMenuItem);
			ConflictMenuItem.Activated += new EventHandler(OnResolveConflicts);

			RevertMenuItem = new ImageMenuItem ("Re_vert");
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
			RefreshMenuItem.Activated += 
					new EventHandler(RefreshiFolderTreeView);

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
			vbox.Spacing = 10;
			vbox.BorderWidth = 10;
			
			// Create the main TreeView and add it to a scrolled
			// window, then add it to the main vbox widget
			iFolderTreeView = new TreeView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(iFolderTreeView);
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
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


			// Setup Text Rendering for "Status" column
			CellRendererText statusTR = new CellRendererText();
			statusTR.Xpad = 10;
			TreeViewColumn statusColumn = new TreeViewColumn();
			statusColumn.PackStart(statusTR, false);
			statusColumn.SetCellDataFunc(statusTR, new TreeCellDataFunc(
						iFolderStatusCellTextDataFunc));
			statusColumn.Title = "Status";
			statusColumn.Resizable = true;
			iFolderTreeView.AppendColumn(statusColumn);


			// Setup Text Rendering for "Location" column
			CellRendererText locTR = new CellRendererText();
			locTR.Xpad = 10;
			TreeViewColumn locColumn = new TreeViewColumn();
			locColumn.PackStart(locTR, false);
			locColumn.SetCellDataFunc(locTR, new TreeCellDataFunc(
						iFolderLocationCellTextDataFunc));
			locColumn.Title = "Location";
			locColumn.Resizable = true;
			iFolderTreeView.AppendColumn(locColumn);


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
			Button add_button = new Button(Gtk.Stock.Add);

			add_button.Clicked += new EventHandler(OnCreateiFolder);
			
			leftHBox.PackEnd(add_button);
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
			vbox.Spacing = 10;
			vbox.BorderWidth = 10;


			//------------------------------
			// Application Frame
			//------------------------------
			Frame ApplicationFrame = new Frame("Application");

			VBox appBox = new VBox();
			appBox.Spacing = 10;
			appBox.BorderWidth = 10;

			StartAtLoginButton = 
				new CheckButton("Startup iFolder at Login");
			appBox.PackStart(StartAtLoginButton, false, true, 0);

			ShowConfirmationButton = 
				new CheckButton("Show creation dialog when creating iFolders");
			appBox.PackStart(ShowConfirmationButton, false, true, 0);

			ApplicationFrame.Add(appBox);

			vbox.PackStart(ApplicationFrame, false, true, 0);


			//------------------------------
			// Sync Frame
			//------------------------------
			Frame SyncFrame = new Frame("Synchronization");
			VBox syncBox = new VBox();
			syncBox.Spacing = 10;
			syncBox.BorderWidth = 10;
			SyncFrame.Add(syncBox);
			vbox.PackStart(SyncFrame, false, true, 0);

			AutoSyncCheckButton = 
					new CheckButton("Automatically Sync iFolders");
			syncBox.PackStart(AutoSyncCheckButton, false, true, 0);

			Frame SyncToHostFrame = new Frame("Synchronize to host");
			syncBox.PackStart(SyncToHostFrame, false, true, 0);

			VBox syncVBox = new VBox();
			syncVBox.Spacing = 10;
			syncVBox.BorderWidth = 10;

			Label syncHelp = new Label("This sets the default value for how often the hosts for all iFolders will be contacted to perform a sync.  This value can can be overriden by setting a value on an individual iFolder.");
			syncHelp.LineWrap = true;
			syncHelp.Xalign = 0;
			syncVBox.PackStart(syncHelp, false, true, 0);

			HBox syncHBox = new HBox();
			syncHBox.Spacing = 10;

			Label syncLabel = new Label("Sync to host every:");
			syncHBox.PackStart(syncLabel, false, false, 0);

			SpinButton syncSpinButton = new SpinButton(0, 99999, 1);
			syncHBox.PackStart(syncSpinButton, false, false, 0);

			Label syncValue = new Label("seconds");
			syncValue.Xalign = 0;
			syncHBox.PackEnd(syncValue, true, true, 0);
			syncVBox.PackEnd(syncHBox, false, false, 0);
			SyncToHostFrame.Add(syncVBox);


			//------------------------------
			// Proxy Frame
			//------------------------------
			Frame ProxyFrame = new Frame("Proxy");
			VBox proxyBox = new VBox();
			proxyBox.Spacing = 10;
			proxyBox.BorderWidth = 10;
			ProxyFrame.Add(proxyBox);
			vbox.PackStart(ProxyFrame, false, true, 0);

			UseProxyButton = new CheckButton("Use a proxy to sync iFolders");
			proxyBox.PackStart(UseProxyButton, false, true, 0);

			HBox pSettingBox = new HBox();
			pSettingBox.Spacing = 10;
//			pSettingBox.BorderWidth = 10;
			proxyBox.PackStart(pSettingBox, false, true, 0);

			Label hostLabel = new Label("Proxy Host:");
			pSettingBox.PackStart(hostLabel, false, true, 0);
			Entry hostEntry = new Entry();
			pSettingBox.PackStart(hostEntry, true, true, 0);
			Label portLabel = new Label("Port:");
			pSettingBox.PackStart(portLabel, false, true, 0);
			SpinButton portSpinButton = new SpinButton(0, 99999, 1);
			pSettingBox.PackStart(portSpinButton, false, true, 0);

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
			vbox.BorderWidth = 10;
		
			Label lbl = new Label("This log shows current ifolder activity");
			vbox.PackStart(lbl, false, true, 0);
			lbl.Xalign = 0;

			ScrolledWindow sw = new ScrolledWindow();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			vbox.PackStart(sw, true, true, 0);
			TreeView LogTreeView = new TreeView();
			sw.Add(LogTreeView);
			LogTreeView.HeadersVisible = false;

			// Setup the iFolder TreeView
			ListStore LogTreeStore = new ListStore(typeof(string));
			LogTreeView.Model = LogTreeStore;

			CellRendererText logcr = new CellRendererText();
			logcr.Xpad = 10;
			LogTreeView.AppendColumn("Log", logcr, "text", 0);


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

			Button SaveButton = new Button(Gtk.Stock.Save);
			rightBox.PackStart(SaveButton);
//			AddButton.Clicked += new EventHandler(OnAddUser);

			Button ClearButton = new Button(Gtk.Stock.Clear);
			rightBox.PackStart(ClearButton);

			return vbox;
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
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			((CellRendererText) cell).Text = ifolder.UnManagedPath;
		}




		private void iFolderStatusCellTextDataFunc(
				Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			iFolder ifolder = (iFolder) tree_model.GetValue(iter,0);
			if(ifolder.State == "Local")
			{
				if(ifolder.HasConflicts)
					((CellRendererText) cell).Text = "Has File Conflicts";
				else
					((CellRendererText) cell).Text = "OK";
			}
			else if(ifolder.State == "Available")
				((CellRendererText) cell).Text = "Available";
			else if(ifolder.State == "WaitConnect")
				((CellRendererText) cell).Text = "Waiting to Connect";
			else if(ifolder.State == "WaitSync")
				((CellRendererText) cell).Text = "Waiting to Sync";
			else
				((CellRendererText) cell).Text = "Unknown";
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
			if(ifolder.State == "Local")
			{
				if(ifolder.HasConflicts)
					((CellRendererPixbuf) cell).Pixbuf = ConflictPixBuf;
				else
					((CellRendererPixbuf) cell).Pixbuf = iFolderPixBuf;
			}
			else
				((CellRendererPixbuf) cell).Pixbuf = ServeriFolderPixBuf;
		}




		public void RefreshiFolderTreeView(object o, EventArgs args)
		{
			iFolder[] iFolderArray;

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

			iFolderTreeStore.Clear();

			foreach(iFolder ifolder in iFolderArray)
			{
				iFolderTreeStore.AppendValues(ifolder);
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
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

	//			This appears to hang?
	//			SyncSize.CalculateSendSize(	ifolder, 
	//										out nodeCount, 
	//										out bytesToSend);

	//			UploadLabel.Text = bytesToSend.ToString();
	//			SyncFilesLabel.Text = nodeCount.ToString();

				if(	(ifolder != null) && (ifolder.HasConflicts) )
				{
					ConflictMenuItem.Sensitive = true;
				}
				else
				{
					ConflictMenuItem.Sensitive = false;
				}

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
						iFolder ifolder = null;

						TreeSelection tSelect = iFolderTreeView.Selection;
						tSelect.SelectPath(tPath);
						if(tSelect.CountSelectedRows() == 1)
						{
							TreeModel tModel;
							TreeIter iter;

							tSelect.GetSelected(out tModel, out iter);
							ifolder = (iFolder) tModel.GetValue(iter, 0);

							if(ifolder.State == "Local")
							{
								MenuItem item_open = 
									new MenuItem ("Open");
								ifMenu.Append (item_open);
								item_open.Activated += new EventHandler(
										OnOpeniFolderMenu);

								MenuItem item_share = 
									new MenuItem ("Share with...");
								ifMenu.Append (item_share);
								item_share.Activated += new EventHandler(
										OnShareProperties);

								MenuItem item_revert = 
									new MenuItem ("Revert to a Normal Folder");
								ifMenu.Append (item_revert);
								item_revert.Activated += new EventHandler(
										OnRevertiFolder);

								ifMenu.Append(new SeparatorMenuItem());

								if(	(ifolder != null) && 
										(ifolder.HasConflicts) )
								{
									MenuItem item_resolve = 
										new MenuItem ("Resolve Conflicts");
									ifMenu.Append (item_resolve);
									item_resolve.Activated += new EventHandler(
										OnResolveConflicts);
							
									ifMenu.Append(new SeparatorMenuItem());
								}
	
								MenuItem item_properties = 
									new MenuItem ("Properties");
								ifMenu.Append (item_properties);
								item_properties.Activated += 
									new EventHandler( OnShowProperties );
							}
							else if(ifolder.State == "Available")
							{
								MenuItem item_accept = 
									new MenuItem ("Setup iFolder");
								ifMenu.Append (item_accept);
								item_accept.Activated += new EventHandler(
										OnSetupiFolder);

								MenuItem item_decline = 
									new MenuItem ("Remove iFolder");
								ifMenu.Append (item_decline);
								item_decline.Activated += new EventHandler(
										OnDeclineiFolder);
							}
							else
							{
								MenuItem item_accept = 
									new MenuItem ("Connect now...");
								ifMenu.Append (item_accept);

								MenuItem item_decline = 
									new MenuItem ("Remove iFolder");
								ifMenu.Append (item_decline);
							}
						}
					}
					else
					{
						MenuItem item_create = 
							new MenuItem ("Create iFolder");
						ifMenu.Append (item_create);
						item_create.Activated += 
							new EventHandler(OnCreateiFolder);

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
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);
				if(ifolder.IsSubscription)
				{
				
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
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

				try
				{
					PropertiesDialog = 
						new iFolderPropertiesDialog(this, ifolder, iFolderWS);
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
	//		if(args.ResponseId
			if(PropertiesDialog != null)
			{
				PropertiesDialog.Hide();
				PropertiesDialog.Destroy();
				PropertiesDialog = null;
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
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);

				iFolderMsgDialog dialog = new iFolderMsgDialog(
					this,
					iFolderMsgDialog.DialogType.Question,
					iFolderMsgDialog.ButtonSet.YesNo,
					"iFolder Confirmation",
					"Revert this iFolder?",
					"This will revert this iFolder back to a normal folder and leave the files intact.  The iFolder will then be available from the server and will need to be setup in a different location in order to sync.");
				int rc = dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				if(rc == -8)
				{
					try
					{
    					iFolderWS.DeleteiFolder(ifolder.ID);
						// iFolderTreeStore.Remove(ref iter);
						// Refresh the view so the Subscription shows up again
						RefreshiFolderTreeView(o, args);
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
				// create a file selection dialog and turn off all of the
				// file operations and controlls
				FileSelection fs = new FileSelection ("Choose a folder...");
				fs.FileList.Parent.Hide();
				fs.SelectionEntry.Hide();
				fs.FileopDelFile.Hide();
				fs.FileopRenFile.Hide();
				fs.TransientFor = this;

				rc = fs.Run ();
				fs.Hide();
				if(rc == -5)
				{
					if(ShowBadiFolderPath(fs.Filename))
						continue;

					// break loop
					rc = 0;
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
			while(rc == -5);
		}




		private void OnSetupiFolder(object o, EventArgs args)
		{
			string newPath  = "";
			iFolder ifolder = null;
			TreeModel tModel;
			TreeIter iter;

			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				tSelect.GetSelected(out tModel, out iter);
				ifolder = (iFolder) tModel.GetValue(iter, 0);
				if(ifolder == null)
					return;
				int rc = 0;

				do
				{
					iFolderAcceptDialog iad = new iFolderAcceptDialog(ifolder);
					iad.TransientFor = this;
					rc = iad.Run();
					newPath = iad.Path;
					iad.Hide();
					iad.Destroy();
					if(rc != -5)
						return;

					// Crappy login here
					// if the user selected OK, check the path they
					// selectected, if we didn't show there was a bad
					// path, set rc to 0 to accept the ifolder
					if(!ShowBadiFolderPath(newPath))
						rc = 0;
				}
				while(rc == -5);
				
				try
				{
   		 			iFolder newiFolder = iFolderWS.AcceptiFolderInvitation(
											ifolder.ID,
											newPath);
	
					// replace the old iFolder with this one
					tModel.SetValue(iter, 0, newiFolder);
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
		}


		private bool ShowBadiFolderPath(string path)
		{
			try
			{
				if(iFolderWS.CanBeiFolder(path) == false)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Info,
						iFolderMsgDialog.ButtonSet.Ok,
						"Invalid iFolder Path",
						"Invalid iFolder path selected",
						"iFolders cannot contain other iFolders.  The folder you selected is either inside an iFolder or has an iFolder in it.  Please select an alternate folder.");
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




		private void OnDeclineiFolder(object o, EventArgs args)
		{
			iFolderMsgDialog dialog = new iFolderMsgDialog(
				this,
				iFolderMsgDialog.DialogType.Question,
				iFolderMsgDialog.ButtonSet.YesNo,
				"iFolder Confirmation",
				"Decline shared iFolder?",
				"This will remove your invitation and you will not be able to get it back unless the owner of this iFolder re-shares the iFolder with you.");
			int rc = dialog.Run();
			dialog.Hide();
			dialog.Destroy();
			if(rc == -8)
			{
				Console.WriteLine("Reverting Share iFolder");
			}
		}




		private void OnResolveConflicts(object o, EventArgs args)
		{
			TreeSelection tSelect = iFolderTreeView.Selection;
			if(tSelect.CountSelectedRows() == 1)
			{
				TreeModel tModel;
				TreeIter iter;

				tSelect.GetSelected(out tModel, out iter);
				iFolder ifolder = (iFolder) tModel.GetValue(iter, 0);
			
				
				ConflictDialog = new iFolderConflictDialog(
										this,
										ifolder,
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



	}
}
