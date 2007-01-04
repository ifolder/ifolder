/***********************************************************************
 *  $RCSfile: MigrationWizard.cs,v $
 *
 *  Copyright (C) 2006 Novell, Inc.
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
 *  Author:
 *		Ramesh Sunder <sramesh@novell.com>
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Collections;
using Gtk;

using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public class MigrationWizard : Window
	{

                enum SecurityState
                {
                        encrypt = 1,
                        enforceEncrypt = 2,
                        SSL = 4,
                        enforceSSL = 8
                }

		private Gnome.Druid				AccountDruid;
		private Gnome.DruidPageEdge		IntroductoryPage;
		private Gnome.DruidPageStandard	MigrationOptionsPage;
		private Gnome.DruidPageStandard	UserInformationPage;
		private DruidConnectPage		MigratePage;
		private Gnome.DruidPageEdge		SummaryPage;
		private DomainController		domainController;
		private DomainInformation[]		domains;
		private iFolderWebService 		ifws;
		private bool					ControlKeyPressed;
		private Button						ForwardButton;
		private Button						FinishButton;
		private string				location;
		private string				prevLocation;
		private string 				Uname;
		private bool 				Prepared;
		private iFolderData			ifdata;
		private bool				migrationStatus;
		
		private Gdk.Pixbuf				AddAccountPixbuf;
		private MigrationPage page;

		///
		/// Migration Options page
		///
		private CheckButton 			deleteFromServer;
		private CheckButton			copyToServer;
		private Button				BrowseButton;
		private Entry 				LocationEntry;

		///
		/// Server Info page
		///
		private ComboBox 			domainList;
		private CheckButton			encryptionCheckButton;
		private CheckButton			sslCheckButton;
		
		///
		/// Migrate Page Widgets
		///
		private Label		ServerAddressLabel;
		private Label		Location;
		private Label		MigrationOptionLabel;
		private Label		ShowSecurityLabel;
		private Label		SecurityLabel;
		
		/// Wait Message
		///
		iFolderWaitDialog	WaitDialog;

		public MigrationWizard(string User, string path, iFolderWebService ifws, MigrationPage page) : base(WindowType.Toplevel)
		{
			this.Title = Util.GS("iFolder Migration Assistant");
			this.Resizable = false;
			this.Modal = true;
			this.WindowPosition = Gtk.WindowPosition.Center;
			this.location = path;
			prevLocation = "";
			this.ifdata = iFolderData.GetData();
			this.ifws = ifws;
			this.Uname = User;
			this.page = page;
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			domainController = DomainController.GetDomainController();
			
			WaitDialog = null;
			
			this.Add(CreateWidgets());

			// Bind ESC and C-w to close the window
			ControlKeyPressed = false;
			KeyPressEvent += new KeyPressEventHandler(KeyPressHandler);
			KeyReleaseEvent += new KeyReleaseEventHandler(KeyReleaseHandler);
			
			SetUpButtons();
		}
		
		private Widget CreateWidgets()
		{
			VBox vbox = new VBox(false, 0);
			
			AddAccountPixbuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder-add-account48.png"));
			AddAccountPixbuf = AddAccountPixbuf.ScaleSimple(48, 48, Gdk.InterpType.Bilinear);
			
			AccountDruid = new Gnome.Druid();
			vbox.PackStart(AccountDruid, true, true, 0);
			
			AccountDruid.ShowHelp = true;
			AccountDruid.Help += new EventHandler(OnAccountWizardHelp);
			
			AccountDruid.AppendPage(CreateIntroductoryPage());
			AccountDruid.AppendPage(CreateMigrationOptionsPage());
			AccountDruid.AppendPage(CreateUserInformationPage());
			AccountDruid.AppendPage(CreateMigratePage());
			AccountDruid.AppendPage(CreateSummaryPage());
			
			return vbox;
		}
		
		///
		/// Introductory Page
		///
		private Gnome.DruidPage CreateIntroductoryPage()
		{
			IntroductoryPage = new Gnome.DruidPageEdge(Gnome.EdgePosition.Start,
				true,	// use an antialiased canvas
				Util.GS("Migrate the iFolder Account"),
				Util.GS("Welcome to the iFolder Migration Wizard.\n\nClick \"Forward\" to begin."),
				AddAccountPixbuf, null, null);
			
			IntroductoryPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);
			
			IntroductoryPage.Prepared +=
				new Gnome.PreparedHandler(OnIntroductoryPagePrepared);

			return IntroductoryPage;
		}

		///
		/// Server Information Page (1 of 3)
		///
		private Gnome.DruidPage CreateMigrationOptionsPage()
		{
			MigrationOptionsPage =
				new Gnome.DruidPageStandard(
					Util.GS("Migration Options"),
					AddAccountPixbuf,
					null);

			MigrationOptionsPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);

			MigrationOptionsPage.Prepared +=
				new Gnome.PreparedHandler(OnMigrationOptionsPagePrepared);

			///
			/// Content
			///
			Table table = new Table(4, 3, false);
			MigrationOptionsPage.VBox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Select one among the following options"));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			deleteFromServer = new CheckButton("Migrate the folder and remove from 2.x domain");
			deleteFromServer.Active = true;
			deleteFromServer.Sensitive = false;
			deleteFromServer.Toggled += new EventHandler(OndeleteCheckButtonChanged);
			table.Attach(deleteFromServer, 1,3, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			
			// Row 3

			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			copyToServer = new CheckButton("Create a new copy of the folder and connect to server");
			copyToServer.Toggled += new EventHandler(OncopyCheckButtonChanged);

			table.Attach(copyToServer, 1,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			// Row 4
			
			table.Attach(new Label("Location:"), 0,1, 3,4, AttachOptions.Fill, 0,0,0);
			LocationEntry = new Entry();
			LocationEntry.Text = location;
			LocationEntry.Sensitive = false;
			table.Attach(LocationEntry, 1,2, 3,4, AttachOptions.Fill, 0,0,0);	
			BrowseButton = new Button("_Browse");
			table.Attach(BrowseButton, 2,3, 3,4, AttachOptions.Fill, 0,0,0); 
			BrowseButton.Sensitive = false;
			BrowseButton.Clicked += new EventHandler(OnBrowseButtonClicked);	
			return MigrationOptionsPage;
		}

		private void OnBrowseButtonClicked(object o, EventArgs e)
		{
			MigrateLocation NewLoc = new MigrateLocation(this, System.IO.Directory.GetCurrentDirectory(), ifws );
			NewLoc.TransientFor = this;
			int rc = 0;
			do
			{
				rc = NewLoc.Run();
				NewLoc.Hide();
				if(rc ==(int)ResponseType.Ok)
				{
					string selectedFolder = NewLoc.iFolderPath.Trim();
					LocationEntry.Text = selectedFolder;
					NewLoc.Destroy();
					NewLoc = null;
					break;
				}
			}while( rc == (int)ResponseType.Ok);
		}

		private void OnForwardClicked(object o, EventArgs e)
		{
		}
		
		///
		/// User Information Page (2 of 3)
		///
		private Gnome.DruidPage CreateUserInformationPage()
		{
			UserInformationPage =
				new Gnome.DruidPageStandard(
					Util.GS("Server Information"),
					AddAccountPixbuf,
					null);

			UserInformationPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);

			UserInformationPage.Prepared +=
				new Gnome.PreparedHandler(OnUserInformationPagePrepared);
			
			///
			/// Content
			///
			Table table = new Table(5, 3, false);
			UserInformationPage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Select the server "));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("Server Address:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			domainList= ComboBox.NewText();
			table.Attach(domainList, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			// Row 3
			
			l = new Label(Util.GS("Select Security options"));
			table.Attach(l, 0,3, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			
			// Row 4
			encryptionCheckButton = new CheckButton(Util.GS("Encrypt iFolder"));
			table.Attach(encryptionCheckButton, 1,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			//Row 5
			sslCheckButton = new CheckButton(Util.GS("Use Secure channel for data transfer"));
			table.Attach(sslCheckButton, 1,3, 4,5, AttachOptions.Fill | AttachOptions.Expand, 0,0,0); 
			return UserInformationPage;
		}
		
		///
		/// Connect Page (3 of 3)
		///
		private Gnome.DruidPage CreateMigratePage()
		{
			
			MigratePage =
				new DruidConnectPage(
					Util.GS("Verify and Migrate"),
					AddAccountPixbuf,
					null);

			MigratePage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);
			
			MigratePage.ConnectClicked +=
				new ConnectClickedHandler(OnMigrateClicked);

			MigratePage.Prepared +=
				new Gnome.PreparedHandler(OnMigratePagePrepared);
			
			///
			/// Content
			///
			Table table = new Table(6, 3, false);
			MigratePage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Please verify that the information you've entered is correct."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("Server Address:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			ServerAddressLabel = new Label("");
			table.Attach(ServerAddressLabel, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			ServerAddressLabel.Xalign = 0.0F;

			// Row 3
			l = new Label(Util.GS("Location:"));
			table.Attach(l, 1,2, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			Location = new Label("");
			table.Attach(Location, 2,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			Location.Xalign = 0.0F;
			
			// Row 4
			l = new Label(Util.GS("Migration Option:"));
			table.Attach(l, 1,2, 3,4,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			MigrationOptionLabel = new Label("");
			table.Attach(MigrationOptionLabel, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MigrationOptionLabel.Xalign = 0.0F;
			
			// Row 5
			ShowSecurityLabel = new Label(Util.GS("Security:"));
			table.Attach(ShowSecurityLabel, 1,2, 4,5,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			ShowSecurityLabel.Xalign = 0.0F;
			SecurityLabel = new Label("");
			table.Attach(SecurityLabel, 2,3, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			SecurityLabel.Xalign = 0.0F;
			
			// Row 6
			l = new Label(
				string.Format(
					"\n\n{0}",
					Util.GS("Click \"Migrate\" to migrate your folder to the server specified.")));
			table.Attach(l, 0,3, 5,6,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;
			
			return MigratePage;
			
		}
		
		///
		/// Summary Page
		///
		private Gnome.DruidPage CreateSummaryPage()
		{
			SummaryPage = new Gnome.DruidPageEdge(Gnome.EdgePosition.Finish,
				true,	// use an antialiased canvas
				Util.GS("Congratulations!"),
				"",
				AddAccountPixbuf,
				null, null);
			
			SummaryPage.FinishClicked +=
				new Gnome.FinishClickedHandler(OnFinishClicked);

			SummaryPage.Prepared +=
				new Gnome.PreparedHandler(OnSummaryPagePrepared);

			return SummaryPage;
		}

		///
		/// Sensitivity Methods
		///
		private void UpdateServerInformationPageSensitivity(object o, EventArgs args)
		{
		}
		
		///
		/// Event Handlers
		///
		private void OnAccountWizardHelp(object o, EventArgs args)
		{
			Util.ShowHelp("accounts.html", this);
		}

		private void OnIntroductoryPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant");
			AccountDruid.SetButtonsSensitive(false, true, true, true);
		}
		
		private void OnMigrationOptionsPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (1 of 3)");
			
//			ServerNameEntry.GrabFocus();
		}

                private void OnDomainChangedEvent(System.Object o, EventArgs args)
                {
                        int status = ifws.GetSecurityPolicy((domains[domainList.Active]).ID);
                        ChangeStatus(status);
                }

                private void ChangeStatus(int SecurityPolicy)
                {
                        encryptionCheckButton.Active = sslCheckButton.Active = false;
                        encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = false;
                        if(SecurityPolicy !=0)
                        {
                                if( (SecurityPolicy & (int)SecurityState.encrypt) == (int) SecurityState.encrypt)
                                {
                                        if( (SecurityPolicy & (int)SecurityState.enforceEncrypt) == (int) SecurityState.enforceEncrypt)
                                                encryptionCheckButton.Active = true;
                                        else
                                                encryptionCheckButton.Sensitive = true;
                                }
                                if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
                                {
                                        if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
                                                sslCheckButton.Active = true;
                                        else
                                                sslCheckButton.Sensitive = true;
                                }
                        }

                }

		
		private void OnUserInformationPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (2 of 3)");
			if(Prepared)
				return;
			Prepared = true;
			domains = domainController.GetDomains();
			DomainInformation defaultDomain = domainController.GetDefaultDomain();
			string domainID = "";
			if( defaultDomain != null)
				domainID = defaultDomain.ID;
			int defaultDomainID = 0;
			for( int i=0;i<domains.Length;i++)
			{
				domainList.AppendText(domains[i].Name);
				if(domainID == domains[i].ID)
					defaultDomainID = i;
			}
			domainList.Active = defaultDomainID;
			domainList.Changed += new EventHandler(OnDomainChangedEvent);
			ChangeStatus(ifws.GetSecurityPolicy(domains[domainList.Active].ID));
			// Hack to make sure the "Forward" button has the right text.
			ForwardButton.Label = "gtk-go-forward";
		}
		
		private void OnMigratePagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (3 of 3)");
			ServerAddressLabel.Text = (domains[domainList.Active]).Name;	
			Location.Text = location;
			
			if(deleteFromServer.Active)
				MigrationOptionLabel.Text = "Delete from iFolder2.X server";
			else
				MigrationOptionLabel.Text = "Create a copy and migrate to iFolder3.x";
			if(encryptionCheckButton.Active)
			{
				SecurityLabel.Text = "Encrypt the iFolder ";
				if(sslCheckButton.Active)
				{
					SecurityLabel.Text += "and Use secure channel for data transfer";
				}
			}
			else if(sslCheckButton.Active)
			{
				SecurityLabel.Text = "Use Secure channel for data transfer";
			}
			
			// Hack to modify the "Forward" button to be a "Connect" button
			ForwardButton.Label = Util.GS("Migrate");
//			AccountDruid.Forall(EnableConnectButtonCallback);
		}
		
		private void OnSummaryPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant");
			// TODO: Check here to see whether or not migration is succeeded...
			if(migrationStatus == true)
			{

				SummaryPage.Text = 
					string.Format(
						Util.GS("Congratulations!  Your folder has been\nsuccessfully migrated to \nthe latest version of iFolder.\n\nClick \"Finish\" to close this window."));

			// Hack to modify the "Apply" button to be a "Finish" button
//			AccountDruid.Forall(EnableFinishButtonCallback);
				FinishButton.Label = Util.GS("_Finish");
			
				AccountDruid.SetButtonsSensitive(false, true, false, true);
			}
			else
			{
				SummaryPage.Text = string.Format(Util.GS("Sorry! The iFolder cannot be migrated to the specified account.\n\nPlease try again."));
				AccountDruid.SetButtonsSensitive(true, false, false, false);
			}
		}

	public static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination) 
	{

        	if (!destination.Exists) 
		{
			try
			{
            			destination.Create();
			}
			catch(Exception e)
			{
			}
	        }
		
		if(!source.Exists)
		{
			return;
		}

        	// Copy all files.
		try
		{
	        	FileInfo[] files = source.GetFiles();
        		foreach (FileInfo file in files) 
			{

		            file.CopyTo(System.IO.Path.Combine(destination.FullName, file.Name));
        		}
		}
		catch(Exception e)
		{
		}

		try
		{
        		// Process sub-directories.
	        	DirectoryInfo[] dirs = source.GetDirectories();
        		foreach (DirectoryInfo dir in dirs) 
			{

		            // Get destination directory.
        		    string destinationDir = System.IO.Path.Combine(destination.FullName, dir.Name);

	        	    // Call CopyDirectory() recursively.
	        	    CopyDirectory(dir, new DirectoryInfo(destinationDir));
        		}
		}
		catch(Exception e)
		{
		}
    	}		
		/// <summary>
		/// Return true if the connect was successful, otherwise, return false.
		/// Returning true will allow the druid to advance one page.
		/// </summary>		
		private bool OnMigrateClicked(object o, EventArgs args)
		{
			/*
			if (WaitDialog == null)
			{
				VBox vbox = new VBox(false, 0);
				Image connectingImage = new Image(Util.ImagesPath("ifolder-add-account48.png"));
				vbox.PackStart(connectingImage, false, false, 0);
	
				WaitDialog = 
					new iFolderWaitDialog(
						this,
						vbox,
						iFolderWaitDialog.ButtonSet.None,
						Util.GS("Migrating..."),
						Util.GS("Migrating..."),
						Util.GS("Please wait while your iFolder is being migrated"));
				
				WaitDialog.Show();
				Console.WriteLine(" Showing wait dialog:");
			}
			*/

			if( ifws.CanBeiFolder(LocationEntry.Text) )
			{
				if(copyToServer.Active == true)  // copy the ifolder in location to LocationEntry.Text
				{
					System.IO.DirectoryInfo source = new System.IO.DirectoryInfo(location);
					System.IO.DirectoryInfo destination = new System.IO.DirectoryInfo(LocationEntry.Text);	
					CopyDirectory(source, destination);
				}
				DirectoryInfo d = new DirectoryInfo(location);
				
				// Create iFolder
				string Algorithm = "";
				Algorithm = encryptionCheckButton.Active ? "BlowFish" : "";
				if((d.Exists) && (ifdata.CreateiFolder(LocationEntry.Text, (domains[domainList.Active]).ID, sslCheckButton.Active, Algorithm ) == null))
				{
					// error creating the ifolder..
					Console.WriteLine("error creating the ifolder");
					migrationStatus = false;
				}
				else
				{
					migrationStatus = true;
					Console.WriteLine("Created successfully");
				}
			}
			else
			{
				migrationStatus = false;
			}		
			if(migrationStatus)
			{
				if(deleteFromServer.Active == true)
				{
					string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
						
					if(System.IO.Directory.Exists(str+"/.novell/ifolder/"+Uname))
						System.IO.Directory.Delete(str+"/.novell/ifolder/"+Uname, true);  
					if(System.IO.Directory.Exists(str+"/.novell/ifolder/reg/"+Uname))
						System.IO.Directory.Delete(str+"/.novell/ifolder/reg/"+Uname, true);
					page.RemoveItem();
				}
			}
			if(WaitDialog != null)
			{
				WaitDialog.Hide();
				WaitDialog.Destroy();
				WaitDialog =null;
			}
			return true;
		}
		
		private void OndeleteCheckButtonChanged(object o, EventArgs e)
		{
			if(deleteFromServer.Active == true)
			{
				copyToServer.Active = false;
				copyToServer.Sensitive = true;
				deleteFromServer.Sensitive = false;
				prevLocation = LocationEntry.Text;
				LocationEntry.Text = location;
				LocationEntry.Sensitive = false;
				BrowseButton.Sensitive = false;
			}
		}	
		private void OncopyCheckButtonChanged(object o, EventArgs e)
		{
			if(copyToServer.Active == true)
			{
				deleteFromServer.Active = false;
				deleteFromServer.Sensitive = true;
				copyToServer.Sensitive = false;
				LocationEntry.Sensitive = true;
				BrowseButton.Sensitive = true;
				LocationEntry.Text = prevLocation;
				
			}
		}
		private void OnCancelClicked(object o, Gnome.CancelClickedArgs args)
		{
			CloseDialog();
		}
		
		private void OnFinishClicked(object o, Gnome.FinishClickedArgs args)
		{
			CloseDialog();
			Util.ShowiFolderWindow();
		}
		
		void KeyPressHandler(object o, KeyPressEventArgs args)
		{
			args.RetVal = true;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Escape:
					CloseDialog();
					break;
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = true;
					args.RetVal = false;
					break;
				case Gdk.Key.W:
				case Gdk.Key.w:
					if (ControlKeyPressed)
						CloseDialog();
					else
						args.RetVal = false;
					break;
				default:
					args.RetVal = false;
					break;
			}
		}
		
		void KeyReleaseHandler(object o, KeyReleaseEventArgs args)
		{
			args.RetVal = false;
			
			switch(args.Event.Key)
			{
				case Gdk.Key.Control_L:
				case Gdk.Key.Control_R:
					ControlKeyPressed = false;
					break;
				default:
					break;
			}
		}

		///
		/// Utility/Helper Methods
		///
		private void SetUpButtons()
		{
			AccountDruid.Forall(SetUpButtonsCallback);
		}
		
		private void SetUpButtonsCallback(Widget w)
		{
			if (w is HButtonBox)
			{
				HButtonBox hButtonBox = w as HButtonBox;
				foreach(Widget buttonWidget in hButtonBox)
				{
					if (buttonWidget is Button)
					{
						Button button = buttonWidget as Button;
						if (button.Label == "gtk-go-forward")
							ForwardButton = button;
						else if (button.Label == "gtk-apply")
							FinishButton = button;
					}
				}
			}
		}
		
		public void CloseDialog()
		{
			this.Hide();
			this.Destroy();
		}
	}
	
	/// <summary>
	/// Return true if the connect was successful, otherwise return false.
	/// </summary>
//	public delegate bool ConnectClickedHandler(object o, EventArgs args);
} 
