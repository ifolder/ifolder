/***********************************************************************
 *  $RCSfile: AddAccountWizard.cs,v $
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
 *  Author:
 *		Boyd Timothy <btimothy@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using Gtk;

using Novell.iFolder.Controller;

namespace Novell.iFolder
{
	public class AddAccountWizard : Window
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
		private Gnome.DruidPageStandard	ServerInformationPage;
		private Gnome.DruidPageStandard	UserInformationPage;
		private DruidConnectPage		ConnectPage;
		private Gnome.DruidPageEdge		SummaryPage;
		private DruidRAPage	                RAPage;
		private DruidRAPage	                DefaultiFolderPage;
		private DomainController		domainController;
		private SimiasWebService		simws;
		private bool					ControlKeyPressed;
		private Button						ForwardButton;
		private Button						BackButton;	        
		private Button						FinishButton;
		private bool passPhraseEntered;
		private bool waitForPassphrase;
		private bool upload;
		private iFolderWeb defaultiFolder;
		
		private Gdk.Pixbuf				AddAccountPixbuf;
		
		///
		/// Server Information Page Widgets
		///
		private Entry		ServerNameEntry;
		private Label		MakeDefaultLabel;
		private CheckButton	DefaultServerCheckButton;
		
		///
		/// User Information Page Widgets
		///
		private Entry		UserNameEntry;
		private Entry		PasswordEntry;	// set Visibility = false;
		private CheckButton	RememberPasswordCheckButton;
		
		///
		/// Recovery Agents Page Widgets
		///

	        private iFolderTreeView RATreeView;
	        private ScrolledWindow  RAScrolledWindow;
	        private Entry           PassPhraseEntry;
	        private Entry           PassPhraseVerifyEntry;
	        private CheckButton	RememberPassPhraseCheckButton;
	        private string[]        RAList;
	        private ListStore       RATreeStore;
	        private bool            PassPhraseSet;
	        private bool            RememberPassPhrase;
		private Label		RetypePassPhraseLabel;
		private Label		SelectRALabel;

		///
		/// Default iFolder Page
		///
		private RadioButton encryptionCheckButton;
		private RadioButton sslCheckButton;
		private Entry LocationEntry;
		private CheckButton CreateDefault;
		private Button BrowseButton;
		private string DefaultPath;
		private Label locationLabel;
		private Label securityLabel;

		///
		/// Connect Page Widgets
		///
		private Label		ServerNameVerifyLabel;
		private Label		UserNameVerifyLabel;
		private Label		RememberPasswordVerifyLabel;
		private Label		MakeDefaultPromptLabel;
		private Label		MakeDefaultVerifyLabel;
		
		///
		/// Summary Page Widgets
		///
		DomainInformation	ConnectedDomain;
		
		///
		/// Wait Message
		///
		iFolderWaitDialog	WaitDialog;

		public AddAccountWizard(SimiasWebService simws) : base(WindowType.Toplevel)
		{
			this.Title = Util.GS("iFolder Account Assistant");
			this.Resizable = false;
			this.Modal = true;
			this.WindowPosition = Gtk.WindowPosition.Center;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			this.simws = simws;

			domainController = DomainController.GetDomainController();
			this.passPhraseEntered = false;
			this.waitForPassphrase = false;
			
			ConnectedDomain = null;
			
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
			AccountDruid.AppendPage(CreateServerInformationPage());
			AccountDruid.AppendPage(CreateUserInformationPage());
			AccountDruid.AppendPage(CreateConnectPage());
			AccountDruid.AppendPage(CreateRAPage());
			AccountDruid.AppendPage(CreateDefaultiFolderPage());
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
				Util.GS("Configure an iFolder Account"),
				Util.GS("Welcome to the iFolder Account Assistant.\n\nClick \"Forward\" to begin."),
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
		private Gnome.DruidPage CreateServerInformationPage()
		{
			ServerInformationPage =
				new Gnome.DruidPageStandard(
					Util.GS("iFolder Server"),
					AddAccountPixbuf,
					null);

			ServerInformationPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);

			ServerInformationPage.Prepared +=
				new Gnome.PreparedHandler(OnServerInformationPagePrepared);

			///
			/// Content
			///
			Table table = new Table(4, 3, false);
			ServerInformationPage.VBox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Enter the name of your iFolder Server (for example, \"ifolder.example.net\")."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("Server _Address:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			ServerNameEntry = new Entry();
			table.Attach(ServerNameEntry, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = ServerNameEntry;
			if ((bool)ClientConfig.Get(
				ClientConfig.KEY_IFOLDER_ACCOUNT_PREFILL))
			{
				ServerNameEntry.Text = (string)
					ClientConfig.Get(
						ClientConfig.KEY_IFOLDER_ACCOUNT_SERVER_ADDRESS);
			}
			ServerNameEntry.Changed += new EventHandler(UpdateServerInformationPageSensitivity);
			ServerNameEntry.ActivatesDefault = true;
			
			// Row 3
			MakeDefaultLabel = new Label(Util.GS("Setting this iFolder Server as your default server will allow iFolder to automatically select this server when adding new folders."));
			table.Attach(MakeDefaultLabel, 0,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MakeDefaultLabel.LineWrap = true;
			MakeDefaultLabel.Xalign = 0.0F;
			
			// Row 4
			DefaultServerCheckButton = new CheckButton(Util.GS("Make this my _default server"));
			table.Attach(DefaultServerCheckButton, 1,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			
			return ServerInformationPage;
		}
		
		///
		/// User Information Page (2 of 3)
		///
		private Gnome.DruidPage CreateUserInformationPage()
		{
			UserInformationPage =
				new Gnome.DruidPageStandard(
					Util.GS("Identity"),
					AddAccountPixbuf,
					null);

			UserInformationPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);

			UserInformationPage.Prepared +=
				new Gnome.PreparedHandler(OnUserInformationPagePrepared);
			
			///
			/// Content
			///
			Table table = new Table(4, 3, false);
			UserInformationPage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Enter your iFolder user name and password (for example, \"jsmith\")."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("_User Name:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			UserNameEntry = new Entry();
			table.Attach(UserNameEntry, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = UserNameEntry;
			if ((bool)ClientConfig.Get(
				ClientConfig.KEY_IFOLDER_ACCOUNT_PREFILL))
			{
				UserNameEntry.Text = (string)
					ClientConfig.Get(
						ClientConfig.KEY_IFOLDER_ACCOUNT_USER_NAME);
			}
			UserNameEntry.Changed += new EventHandler(UpdateUserInformationPageSensitivity);
			UserNameEntry.ActivatesDefault = true;

			// Row 3
			l = new Label(Util.GS("_Password:"));
			table.Attach(l, 1,2, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			PasswordEntry = new Entry();
			table.Attach(PasswordEntry, 2,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.MnemonicWidget = PasswordEntry;
			PasswordEntry.Visibility = false;
			if ((bool)ClientConfig.Get(
				ClientConfig.KEY_IFOLDER_ACCOUNT_PREFILL))
			{
				PasswordEntry.Text = (string)
					ClientConfig.Get(
						ClientConfig.KEY_IFOLDER_ACCOUNT_PASSWORD);
			}
			PasswordEntry.Changed += new EventHandler(UpdateUserInformationPageSensitivity);
			PasswordEntry.ActivatesDefault = true;

			// Row 4
			RememberPasswordCheckButton = new CheckButton(Util.GS("_Remember my password"));
			table.Attach(RememberPasswordCheckButton, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			if ((bool)ClientConfig.Get(
				ClientConfig.KEY_IFOLDER_ACCOUNT_PREFILL) &&
				(bool)ClientConfig.Get(
				ClientConfig.KEY_IFOLDER_ACCOUNT_REMEMBER_PASSWORD))
			{
				RememberPasswordCheckButton.Active = (bool)
					ClientConfig.Get(
						ClientConfig.KEY_IFOLDER_ACCOUNT_REMEMBER_PASSWORD);
			}
			
			return UserInformationPage;
		}
		
		///
		/// Connect Page (3 of 3)
		///
		private Gnome.DruidPage CreateConnectPage()
		{
			ConnectPage =
				new DruidConnectPage(
					Util.GS("Verify and Connect"),
					AddAccountPixbuf,
					null);

			ConnectPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);
			
			ConnectPage.ConnectClicked +=
				new ConnectClickedHandler(OnConnectClicked);

			ConnectPage.Prepared +=
				new Gnome.PreparedHandler(OnConnectPagePrepared);
			
			///
			/// Content
			///
			Table table = new Table(6, 3, false);
			ConnectPage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Please verify that the information you have entered is correct."));
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
			ServerNameVerifyLabel = new Label("");
			table.Attach(ServerNameVerifyLabel, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			ServerNameVerifyLabel.Xalign = 0.0F;

			// Row 3
			l = new Label(Util.GS("User Name:"));
			table.Attach(l, 1,2, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			UserNameVerifyLabel = new Label("");
			table.Attach(UserNameVerifyLabel, 2,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			UserNameVerifyLabel.Xalign = 0.0F;
			
			// Row 4
			l = new Label(Util.GS("Remember password:"));
			table.Attach(l, 1,2, 3,4,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			RememberPasswordVerifyLabel = new Label("");
			table.Attach(RememberPasswordVerifyLabel, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			RememberPasswordVerifyLabel.Xalign = 0.0F;
			
			// Row 5
			MakeDefaultPromptLabel = new Label(Util.GS("Make default account:"));
			table.Attach(MakeDefaultPromptLabel, 1,2, 4,5,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			MakeDefaultPromptLabel.Xalign = 0.0F;
			MakeDefaultVerifyLabel = new Label("");
			table.Attach(MakeDefaultVerifyLabel, 2,3, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MakeDefaultVerifyLabel.Xalign = 0.0F;
			
			// Row 6
			l = new Label(
				string.Format(
					"\n\n{0}",
					Util.GS("Click \"Connect\" to validate your connection with the server.")));
			table.Attach(l, 0,3, 5,6,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;
			
			return ConnectPage;
		}
		
		///
		/// Recovery Agents Page 
		///
		private Gnome.DruidPage CreateRAPage()
		{
			RAPage = new DruidRAPage(
					Util.GS("Encryption"),
					AddAccountPixbuf,
					null);

			RAPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);

			RAPage.ValidateClicked +=
				new ValidateClickedHandler(OnValidateClicked);

			RAPage.SkipClicked +=
				new SkipClickedHandler(OnSkipClicked);

			RAPage.Prepared +=
				new Gnome.PreparedHandler(OnRAPagePrepared);
			
			///
			/// Content
			///
			Table table = new Table(6, 3, false);
			RAPage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Enter the Passphrase"));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("_Passphrase:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			PassPhraseEntry = new Entry();
			PassPhraseEntry.Visibility = false;
			table.Attach(PassPhraseEntry, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			PassPhraseEntry.Changed += new EventHandler(ChangeSensitivity);
			l.MnemonicWidget = PassPhraseEntry;

			// Row 3
			RetypePassPhraseLabel = new Label(Util.GS("R_etype the Passphrase:"));
			table.Attach(RetypePassPhraseLabel, 1,2, 2,3,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			PassPhraseVerifyEntry = new Entry();
			PassPhraseVerifyEntry.Visibility = false;
			table.Attach(PassPhraseVerifyEntry, 2,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			PassPhraseVerifyEntry.Changed += new EventHandler(ChangeSensitivity);
			l.MnemonicWidget = PassPhraseVerifyEntry;

			// Row 4
			RememberPassPhraseCheckButton = new CheckButton(Util.GS("_Remember the Passphrase"));
			table.Attach(RememberPassPhraseCheckButton, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			// Row 5
			SelectRALabel = new Label(Util.GS("Select the Passphrase Recovery Agent"));
			table.Attach(SelectRALabel, 0,3, 4,5,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 6-7
			RATreeView = new iFolderTreeView ();
			ScrolledWindow RAScrolledWindow = new ScrolledWindow();
			RAScrolledWindow.ShadowType = Gtk.ShadowType.None;
			RAScrolledWindow.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			RAScrolledWindow.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			RAScrolledWindow.Add(RATreeView);

			RATreeStore = new ListStore(typeof(string));
			RATreeView.Model = RATreeStore;

			// RA Name Column
			TreeViewColumn raNameColumn = new TreeViewColumn();
			raNameColumn.Title = Util.GS("Passphrase Recovery Agents");
			CellRendererText cr = new CellRendererText();
			cr.Xpad = 5;
			raNameColumn.PackStart(cr, false);
			raNameColumn.SetCellDataFunc(cr,
						     new TreeCellDataFunc(RANameCellTextDataFunc));
			raNameColumn.Resizable = true;
			raNameColumn.MinWidth = 250;

			RATreeView.AppendColumn(raNameColumn);

			RATreeView.Selection.Mode = SelectionMode.Single;

 			table.Attach(RAScrolledWindow, 0,3, 5,7,
 				AttachOptions.Expand | AttachOptions.Fill, 0,0,0);

			AccountDruid.CancelButton.Sensitive = false;


			return RAPage;
		}

		private Gnome.DruidPage CreateDefaultiFolderPage()
		{
			DefaultiFolderPage = new DruidRAPage(
					Util.GS("Default iFolder"),
					AddAccountPixbuf,
					null);

			DefaultiFolderPage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);

			DefaultiFolderPage.ValidateClicked +=
				new ValidateClickedHandler(OnDefaultiFolderValidateClicked);

			DefaultiFolderPage.SkipClicked +=
				new SkipClickedHandler(OnDefaultAccountSkipClicked);

			DefaultiFolderPage.Prepared +=
				new Gnome.PreparedHandler(OnDefaultAccountPagePrepared);
			
			///
			/// Content
			///
			Table table = new Table(3, 5, false);
			DefaultiFolderPage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			//Label l = new Label(Util.GS("Enter the Passphrase"));
			CreateDefault = new CheckButton(Util.GS("Create default iFolder."));
			table.Attach(CreateDefault, 0,5, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			//l.LineWrap = true;
			//l.Xalign = 0.0F;
			CreateDefault.Xalign = 0.0F;
			CreateDefault.Toggled += new EventHandler(OnCreateDefaultChanged);
			
			// Row 2
			locationLabel = new Label(Util.GS("Location:"));
			table.Attach(locationLabel, 1,2, 1,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			//l.Xalign = 0.0F;
			
			LocationEntry = new Entry();
			table.Attach(LocationEntry, 2,4, 1,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			BrowseButton = new Button(Util.GS("Browse"));
			table.Attach(BrowseButton, 4,5, 1,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			BrowseButton.Clicked += new EventHandler(OnBrowseButtonClicked);

			// row 3
			securityLabel = new Label("Security:");
			table.Attach( securityLabel, 1,2, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
			//l.Xalign = 0.0F;
			encryptionCheckButton = new RadioButton(Util.GS("Encrypted"));
			table.Attach(encryptionCheckButton, 2,3, 2,3,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        //Row 5
			sslCheckButton = new RadioButton(encryptionCheckButton, Util.GS("Shared"));
			table.Attach(sslCheckButton, 3,4, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
			if( upload == false)
			{
				encryptionCheckButton.Visible = false;
				sslCheckButton.Visible = false;
			}
			if( this.CreateDefault.Active == false)
			{
				encryptionCheckButton.Sensitive = false;
				sslCheckButton.Sensitive = false;
				LocationEntry.Sensitive = false;
				BrowseButton.Sensitive = false;
			}
			else
			{
				encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = true;
				LocationEntry.Sensitive = BrowseButton.Sensitive = true;
			}
			AccountDruid.BackButton.Sensitive = false;
			return DefaultiFolderPage;
		}

		private void RANameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string value = (string) tree_model.GetValue(iter, 0);
			((CellRendererText) cell).Text = value;
		}

		private string GetDefaultPath()
		{
			string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
			str += "/ifolder/"+ConnectedDomain.Name+"/"+UserNameEntry.Text;
			if( upload == true )
				return str+"/"+"Default";
			else
				return str;
		}

		private void OnCreateDefaultChanged(object o, EventArgs e)
		{
			if( this.CreateDefault.Active == false)
			{
				encryptionCheckButton.Sensitive = false;
				sslCheckButton.Sensitive = false;
				BrowseButton.Sensitive = false;
				locationLabel.Sensitive = false;
				securityLabel.Sensitive = false;
				LocationEntry.Sensitive = false;
			}
			else
			{
				if( upload == true)
				{
					securityLabel.Sensitive = encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = true;
					iFolderWebService ifws = DomainController.GetiFolderService();
					int securityPolicy = ifws.GetSecurityPolicy(ConnectedDomain.ID);
					ChangeStatus( securityPolicy);
				}
				else
				{
					encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = false;
					encryptionCheckButton.Active = sslCheckButton.Active = false;
					securityLabel.Sensitive = false;
				}
				LocationEntry.Sensitive = true;
				BrowseButton.Sensitive = true;
				locationLabel.Sensitive = true;
			}
		}

		private void OnBrowseButtonClicked(object o, EventArgs e)
		{
			if( upload == true)
			{
				MigrateLocation NewLoc = new MigrateLocation(this, System.IO.Directory.GetCurrentDirectory(), DomainController.GetiFolderService() );
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
			else
			{
				FileChooserDialog dlg = new FileChooserDialog("", Util.GS("Select Location..."), null, FileChooserAction.SelectFolder, Stock.Ok, ResponseType.Ok, Stock.Cancel, ResponseType.Cancel);
				int rc = dlg.Run();
				string path = dlg.CurrentFolder;
				dlg.Hide();
				dlg.Destroy();
				if( rc == (int)ResponseType.Ok)
					LocationEntry.Text = path;				
			}
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
			string currentServerName = ServerNameEntry.Text;
			if (currentServerName != null)
			{
				currentServerName = currentServerName.Trim();
				if (currentServerName.Length > 0)
					AccountDruid.SetButtonsSensitive(true, true, true, true);
				else
					AccountDruid.SetButtonsSensitive(true, false, true, true);
			}
			else
				AccountDruid.SetButtonsSensitive(true, false, true, true);
		}
		
		private void UpdateUserInformationPageSensitivity(object o, EventArgs args)
		{
			string currentUserName = UserNameEntry.Text;
			string currentPassword = PasswordEntry.Text;
			if (currentUserName != null /*&& currentPassword != null*/)
			{
				currentUserName = currentUserName.Trim();
			//	currentPassword = currentPassword.Trim();
				if (currentUserName.Length > 0 /*&& currentPassword.Length > 0*/)
					AccountDruid.SetButtonsSensitive(true, true, true, true);
				else
					AccountDruid.SetButtonsSensitive(true, false, true, true);
			}
			else
				AccountDruid.SetButtonsSensitive(true, false, true, true);
		}

		private void UpdateRAPageSensitivity(object o, EventArgs args)
		{
			string currentUserName = UserNameEntry.Text;
			string currentPassword = PasswordEntry.Text;
			if (currentUserName != null && currentPassword != null)
			{
				currentUserName = currentUserName.Trim();
				currentPassword = currentPassword.Trim();
				if (currentUserName.Length > 0 && currentPassword.Length > 0)
					AccountDruid.SetButtonsSensitive(true, true, true, true);
				else
					AccountDruid.SetButtonsSensitive(true, false, true, true);
			}
			else
				AccountDruid.SetButtonsSensitive(true, false, true, true);
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
			this.Title = Util.GS("iFolder Account Assistant");
			AccountDruid.SetButtonsSensitive(false, true, true, true);
		}
		
		private void OnServerInformationPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Assistant - (1 of 5)");
			UpdateServerInformationPageSensitivity(null, null);
			
			DomainInformation[] domains = domainController.GetDomains();
			if (domains != null && domains.Length > 0)
			{
				MakeDefaultLabel.Visible = true;
				DefaultServerCheckButton.Visible = true;
			}
			else
			{
				DefaultServerCheckButton.Active = true;
				MakeDefaultLabel.Visible = false;
				DefaultServerCheckButton.Visible = false;
			}
			
			ServerNameEntry.GrabFocus();
		}

		private void OnDefaultAccountPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Assistant - (5 of 6)");
			string str = "";
			this.CreateDefault.Active = true;
			try
			{
				str = this.simws.GetDefaultiFolder( ConnectedDomain.ID );
			}
			catch(Exception ex)
			{
				Console.WriteLine("Exception: old server: {0}", ex.Message);
				AccountDruid.Page = SummaryPage;
				return;
			}
			if(  str == null || str == "")
			{
				Console.WriteLine("Default account does not exist");
				upload = true;
				LocationEntry.Sensitive = true;
				securityLabel.Sensitive = true;
			}
			else
			{
				Console.WriteLine("Default account exists");
				upload = false;
				CreateDefault.Label = Util.GS("Download Default iFolder:");
				securityLabel.Visible = false;
				encryptionCheckButton.Visible = sslCheckButton.Visible = false;
			}
			if( passPhraseEntered == true)
				AccountDruid.SetButtonsSensitive(false, true, true, true);
			else
				Console.WriteLine("Passphrase entered is false");
			iFolderWebService ifws = DomainController.GetiFolderService();
			int securityPolicy = ifws.GetSecurityPolicy(ConnectedDomain.ID);
			ChangeStatus( securityPolicy);
			LocationEntry.Text = GetDefaultPath();
			AccountDruid.BackButton.Sensitive = false;
			ForwardButton.Label = Util.GS("Next");

			// Check for ssl and display the status of encryption radfio buttons
		}
		private void ChangeStatus(int SecurityPolicy)
		{
			encryptionCheckButton.Active = sslCheckButton.Active = false;
			encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = false;
			Console.WriteLine("Security Policy is: {0}", SecurityPolicy);	
			if(SecurityPolicy !=0)
			{
				if( (SecurityPolicy & (int)SecurityState.encrypt) == (int) SecurityState.encrypt)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceEncrypt) == (int) SecurityState.enforceEncrypt)
						encryptionCheckButton.Active = true;
					else
					{
						encryptionCheckButton.Sensitive = true;
						sslCheckButton.Sensitive = true;
					}
				}
				else
				{
					sslCheckButton.Active = true;
				}
				/*
				if( (SecurityPolicy & (int)SecurityState.SSL) == (int) SecurityState.SSL)
				{
					if( (SecurityPolicy & (int)SecurityState.enforceSSL) == (int) SecurityState.enforceSSL)
						sslCheckButton.Active = true;
					else
						sslCheckButton.Sensitive = true;
				}
				*/
			}
			else
			{
				sslCheckButton.Active = true;
			}
		}

		private void OnRAPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Assistant - (4 of 5)");
			PassPhraseSet = false;
			ForwardButton.Label = "gtk-go-forward";
			ForwardButton.Sensitive = false;

			//TODO :
			BackButton.Label = Util.GS("_Skip");

			if ( domainController.IsPassPhraseSet (ConnectedDomain.ID) == false)
			{
			       string[] list = domainController.GetRAList (ConnectedDomain.ID);

			       foreach (string raagent in list )
				       RATreeStore.AppendValues (raagent);
				RATreeStore.AppendValues( Util.GS("None"));

			} else {
			       // PassPhrase already available.
			       PassPhraseSet = true;
 			       PassPhraseVerifyEntry.Hide ();
			       RATreeView.Hide();
 			       SelectRALabel.Hide();
 			       RetypePassPhraseLabel.Hide();
			}

			AccountDruid.SetButtonsSensitive(true , true, true, true);
		}

		private void OnUserInformationPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Assistant - (2 of 5)");
			UpdateUserInformationPageSensitivity(null, null);
			UserNameEntry.GrabFocus();

			// Hack to make sure the "Forward" button has the right text.
			ForwardButton.Label = "gtk-go-forward";
		}
		
		private void OnConnectPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Assistant - (3 of 5)");

			ServerNameVerifyLabel.Text = ServerNameEntry.Text;
			UserNameVerifyLabel.Text = UserNameEntry.Text;

			RememberPasswordVerifyLabel.Text =
				RememberPasswordCheckButton.Active ?
					Util.GS("Yes") :
					Util.GS("No");

			MakeDefaultVerifyLabel.Text =
				DefaultServerCheckButton.Active ?
					Util.GS("Yes") :
					Util.GS("No");

			DomainInformation[] domains = domainController.GetDomains();
			if (domains != null && domains.Length > 0)
			{
				MakeDefaultPromptLabel.Visible = true;
				MakeDefaultVerifyLabel.Visible = true;
			}
			else
			{
				MakeDefaultPromptLabel.Visible = false;
				MakeDefaultVerifyLabel.Visible = false;
			}
			
			// Hack to modify the "Forward" button to be a "Connect" button
			ForwardButton.Label = Util.GS("Co_nnect");
//			AccountDruid.Forall(EnableConnectButtonCallback);
		}
		
		private void OnSummaryPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Assistant");

			if (ConnectedDomain != null && ConnectedDomain.Name != null && ConnectedDomain.Host != null)
			{
				SummaryPage.Text = 
					string.Format(
						Util.GS("Congratulations!  You are now\nconnected to:\n\nAccount Name: {0}\nServer Address: {1}\nUser Name: {2}\n\nClick \"Finish\" to close this window."),
						ConnectedDomain.Name,
						ConnectedDomain.Host,
						UserNameEntry.Text.Trim());
			}
			
			// Hack to modify the "Apply" button to be a "Finish" button
//			AccountDruid.Forall(EnableFinishButtonCallback);
			FinishButton.Label = Util.GS("_Finish");
			
			AccountDruid.SetButtonsSensitive(false, true, false, true);
		}

		private bool CreateDefaultiFolder()
		{
			iFolderData ifdata = iFolderData.GetData();
			iFolderHolder ifHolder = null;
			// shared
			if( this.encryptionCheckButton.Active == false)
			{
				Console.WriteLine("Shared");
				try
				{
					if( (ifHolder = ifdata.CreateiFolder(this.LocationEntry.Text, ConnectedDomain.ID, this.sslCheckButton.Active, null)) == null)
						return false;
					else
					{
						this.simws.DefaultAccount(ConnectedDomain.ID, ifHolder.iFolder.ID);
						AccountDruid.Page = SummaryPage;
						Console.WriteLine("Making default account and verifying");
						string str = this.simws.GetDefaultiFolder( ConnectedDomain.ID );
						if( str == null)
							Console.WriteLine("Not set the default account");
						else
							Console.WriteLine("Set the default iFolder to: {0}", str);
						return true;
					}
				}
				catch( Exception ex)
				{
					DisplayCreateOrSetupException(ex);
					return false;
				}
			}
			else 
			{
				Console.WriteLine("encrypted");
				if( this.passPhraseEntered == true )
				{
					Console.WriteLine("passphrase entered");
					try
					{
						if( (ifHolder = ifdata.CreateiFolder(this.LocationEntry.Text, ConnectedDomain.ID, this.sslCheckButton.Active, "BlowFish")) == null)
						{
							throw new Exception("Simias returned null");
							return false;
						}
						else
						{
							Console.WriteLine("Making default account");
							this.simws.DefaultAccount(ConnectedDomain.ID, ifHolder.iFolder.ID);
							string str = this.simws.GetDefaultiFolder( ConnectedDomain.ID );
							if( str == null)
								Console.WriteLine("Not set the default account");
							else
								Console.WriteLine("Set the default iFolder to: {0}", str);

							AccountDruid.Page = SummaryPage;
							return true;
						}
					}
					catch(Exception ex)
					{
						DisplayCreateOrSetupException(ex);
						return false;
					}
				}
				else
				{
					// Go to passphrase page
					waitForPassphrase = true;
					AccountDruid.Page = RAPage;
				}
			}
			return false;
		}

		private bool OnDefaultiFolderValidateClicked(object o, EventArgs args)
		{
			// if there is default iFolder present already check for download
			// else upload
			if( this.CreateDefault.Active == false)
				return true;
			string defaultiFolderID = simws.GetDefaultiFolder( ConnectedDomain.ID );
			if( defaultiFolderID == null || defaultiFolderID == "")
			{
				// upload
				upload = true;
				//if( LocationEntry.Text == GetDefaultPath())
				//{
					if(!System.IO.Directory.Exists( LocationEntry.Text ))
					{
						System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(LocationEntry.Text);
						dir.Create();
					}
				//}
				return CreateDefaultiFolder();
			}
			else
			{
				// download
				upload = false;
				iFolderData ifdata = iFolderData.GetData();
				Console.WriteLine("Reading ifdata");
				defaultiFolder = ifdata.GetDefaultiFolder( defaultiFolderID );
				if( defaultiFolder == null)
					Console.WriteLine("iFolder object is null");
				else //if( defaultiFolder.encryptionAlgorithm == null || defaultiFolder.encryptionAlgorithm == "")
				{
					// Not encrypted... Download here
				//	Console.WriteLine("Unencrypted download: " );
					return DownloadiFolder();
				}
				/*
				else
				{
					// encrypted... Check for passphrase
					Console.WriteLine("Encrypted {0} download: ", ifolder.encryptionAlgorithm);
				}
				*/
			}
			return false;
		}
			
		private bool DownloadiFolder()
		{
			iFolderData ifdata = iFolderData.GetData();
			bool canbeSet = false;
			if( defaultiFolder.encryptionAlgorithm == null || defaultiFolder.encryptionAlgorithm == "")
			{
				canbeSet = true;
				/*
				try
				{
					ifdata.AcceptiFolderInvitation( ifolderID, domainID, this.LocationEntry.Text);
					Console.WriteLine("finished accepting invitation");
					return true;
				}
				catch(Exception ex)
				{
					Console.WriteLine("Exception: Unable to download: {0}", ex.Message);
					return false;
				}
				*/
			}
			else
			{
				if( this.passPhraseEntered == false )
				{
					// Go to passphrase page
					waitForPassphrase = true;
					AccountDruid.Page = RAPage;
					return false;
				}
				else
				{
					canbeSet = true;
				}
			}
			if( canbeSet == true )
			{
				try
				{
					ifdata.AcceptiFolderInvitation( defaultiFolder.ID, defaultiFolder.DomainID, this.LocationEntry.Text);
					Console.WriteLine("finished accepting invitation");
					AccountDruid.Page = SummaryPage;
					return true;
				}
				catch(Exception ex)
				{
				//	Console.WriteLine("Exception: Unable to download: {0}", ex.Message);
					DisplayCreateOrSetupException(ex);
					AccountDruid.Page = DefaultiFolderPage;
					return false;
				}
			}
			return false;
		}

		private bool OnValidateClicked(object o, EventArgs args)
		{
			bool NextPage = true;
			string publicKey = null;
			iFolderData ifdata = iFolderData.GetData();
			
			try
			{
			    	if ( PassPhraseSet ==false )
				{
					if (PassPhraseEntry.Text != PassPhraseVerifyEntry.Text)
					{
						iFolderMsgDialog dialog = new iFolderMsgDialog(
															null,
															iFolderMsgDialog.DialogType.Error,
															iFolderMsgDialog.ButtonSet.None,
															Util.GS("PassPhrase mismatch"),
															Util.GS("The PassPhrase and retyped PassPhrase are not same"),
															Util.GS("Please enter the passphrase again"));
															dialog.Run();
															dialog.Hide();
															dialog.Destroy();
															dialog = null;				        
															NextPage = false;
					}
					else
					{
						string recoveryAgentName = "";
						TreeSelection tSelect = RATreeView.Selection;
						if(tSelect != null && tSelect.CountSelectedRows() == 1)
						{
							TreeModel tModel;
							TreeIter iter;
							tSelect.GetSelected(out tModel, out iter);
							recoveryAgentName = (string) tModel.GetValue(iter, 0);
							if( recoveryAgentName == Util.GS("None"))
								recoveryAgentName = null;
						}
						if( recoveryAgentName != null && recoveryAgentName != Util.GS("None"))
						{
							// Show Certificate..
							byte [] RACertificateObj = domainController.GetRACertificate(ConnectedDomain.ID, recoveryAgentName);
							if( RACertificateObj != null && RACertificateObj.Length != 0)
							{
								System.Security.Cryptography.X509Certificates.X509Certificate Cert = new System.Security.Cryptography.X509Certificates.X509Certificate(RACertificateObj);
								CertificateDialog dlg = new CertificateDialog(Cert.ToString(true));

								int res = dlg.Run();
								dlg.Hide();
								dlg.Destroy();
								dlg = null;
								if( res == (int)ResponseType.Ok)
								{
									publicKey = Convert.ToBase64String(Cert.GetPublicKey());
									Console.WriteLine(" The public key is: {0}", publicKey);
								}
								else
								{
									Console.WriteLine("Response type is not ok");
								        simws.StorePassPhrase(ConnectedDomain.ID, "", CredentialType.None, false);
							//		this.passPhraseEntered = true;
									NextPage = false;
								}
							}
						}
						if( NextPage)
						{
							Status passPhraseStatus = simws.SetPassPhrase (ConnectedDomain.ID, PassPhraseEntry.Text, recoveryAgentName, publicKey);
							if(passPhraseStatus.statusCode == StatusCodes.Success)
							{
								simws.StorePassPhrase( ConnectedDomain.ID, PassPhraseEntry.Text,
									CredentialType.Basic, RememberPassPhraseCheckButton.Active);
								this.passPhraseEntered = true;
								if( this.waitForPassphrase == true)
								{
									if( upload == true)
									{
										return CreateDefaultiFolder();
									}
									else
									{
										return DownloadiFolder();
									}
									// Create the default iFolder and go to summary page...
									/*
									if( ifdata.CreateiFolder(this.LocationEntry.Text, ConnectedDomain.ID, this.sslCheckButton.Active, "BlowFish") == null)
									{
										throw new Exception("Simias returned null");
										return false;
									}
									else
									{
										AccountDruid.Page = SummaryPage;
										return true;	
									}
									*/
								}
							}
							else
							{
							   iFolderMsgDialog dialog = new iFolderMsgDialog(
																null,
																iFolderMsgDialog.DialogType.Error,
																iFolderMsgDialog.ButtonSet.None,
																Util.GS("Error setting the Passphrase"),
																Util.GS("Unable to change the Passphrase"),
																Util.GS("Please try again"));
																dialog.Run();
																dialog.Hide();
																dialog.Destroy();
																dialog = null;
																NextPage = false;
							}
						}
					}					
				}
				else 
				{
					// PassPhrase is already set.
					Console.WriteLine("Validating passphrase");
					Status validationStatus = domainController.ValidatePassPhrase (ConnectedDomain.ID, PassPhraseEntry.Text );
					if (validationStatus.statusCode == StatusCodes.PassPhraseInvalid ) 
					{
						NextPage = false;
						iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("PassPhrase Invlid"),
						Util.GS("The PassPhrase entered is not valid"),
						Util.GS("Please enter the passphrase again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;				        
					}
					else if(validationStatus.statusCode == StatusCodes.Success )
					{
						Console.WriteLine("Success. storing passphrase");
						domainController.StorePassPhrase( ConnectedDomain.ID, PassPhraseEntry.Text,
												CredentialType.Basic, RememberPassPhraseCheckButton.Active);
						this.passPhraseEntered = true;
						if( this.waitForPassphrase == true)
						{
							// Create the default iFolder and go to summary page...
							if( upload == true)
							{
								return CreateDefaultiFolder();
							}
							else
							{
								return DownloadiFolder();
							}
							/*
							if( ifdata.CreateiFolder(this.LocationEntry.Text, ConnectedDomain.ID, this.sslCheckButton.Active, "BlowFish") == null)
							{
								throw new Exception("Simias returned null");
								return false;
							}
							else
							{
								AccountDruid.Page = SummaryPage;
								return true;	
							}
							*/
						}
					}
				}
			} 
			catch (Exception ex)
			{
				iFolderMsgDialog dialog = new iFolderMsgDialog(
				null,
				iFolderMsgDialog.DialogType.Error,
				iFolderMsgDialog.ButtonSet.None,
				Util.GS("Unable to set the passphrase"),
				Util.GS(ex.Message),
				Util.GS("Please enter the passphrase again"));
				dialog.Run();
				dialog.Hide();
				dialog.Destroy();
				dialog = null;
				NextPage = false;
				//Avoid ifolder crash incase of exception.
			}
			if( NextPage == false)
			{
				Console.WriteLine("In the same page");
				AccountDruid.Page = RAPage;
				return false;
			}
			BackButton.Label = Util.GS("gtk-go-back");
			return true;
		}
		
		private bool OnSkipClicked(object o, EventArgs args)
		{
			AccountDruid.Page = DefaultiFolderPage;

			BackButton.Label = Util.GS("gtk-go-back");
			return false;
		}

		private bool OnDefaultAccountSkipClicked(object o, EventArgs args)
		{
			AccountDruid.Page = RAPage;

			BackButton.Label = Util.GS("gtk-go-back");
			return false;
		}
		/// <summary>
		/// Return true if the connect was successful, otherwise, return false.
		/// Returning true will allow the druid to advance one page.
		/// </summary>		
		private bool OnConnectClicked(object o, EventArgs args)
		{
			string serverName	= ServerNameEntry.Text.Trim();
			string userName		= UserNameEntry.Text.Trim();
			string password		= PasswordEntry.Text;
			
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
						Util.GS("Connecting..."),
						Util.GS("Connecting..."),
						Util.GS("Please wait while your iFolder account is connecting."));
				
				WaitDialog.Show();
			}
			
			AddDomainThread addDomainThread =
				new AddDomainThread(
					domainController,
					serverName,
					userName,
					password,
					RememberPasswordCheckButton.Active,
					DefaultServerCheckButton.Active);
			
			addDomainThread.Completed +=
				new EventHandler(OnAddDomainCompleted);
			
			addDomainThread.AddDomain();
			
			return false;
		}
		
		private void OnAddDomainCompleted(object o, EventArgs args)
		{
			AddDomainThread addDomainThread = (AddDomainThread)o;
			DomainInformation dom = addDomainThread.Domain;
			Exception e = addDomainThread.Exception;
			if (dom == null && e != null)
			{
				if (e is DomainAccountAlreadyExistsException)
				{
					iFolderMsgDialog dg = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("An account already exists"),
						Util.GS("An account for this server already exists on the local machine.  Only one account per server is allowed."));
					dg.Run();
					dg.Hide();
					dg.Destroy();
				}
				else
				{
					iFolderMsgDialog dg2 = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.Ok,
						"",
						Util.GS("Unable to connect to the iFolder Server"),
						Util.GS("An error was encountered while connecting to the iFolder server.  Please verify the information entered and try again.  If the problem persists, please contact your network administrator."),
						Util.GS(e.Message));
					dg2.Run();
					dg2.Hide();
					dg2.Destroy();
				}

				if (WaitDialog != null)
				{
					WaitDialog.Hide();
					WaitDialog.Destroy();
					WaitDialog = null;
				}
			}
			
			if (dom == null) // This shouldn't happen, but just in case...
			{
				if (WaitDialog != null)
				{
					WaitDialog.Hide();
					WaitDialog.Destroy();
					WaitDialog = null;
				}

				return;
			}

			switch(dom.StatusCode)
			{
				case StatusCodes.InvalidCertificate:
					if (WaitDialog != null)
					{
						WaitDialog.Hide();
						WaitDialog.Destroy();
						WaitDialog = null;
					}

					string serverName = addDomainThread.ServerName;

					byte[] byteArray = simws.GetCertificate(serverName);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

					iFolderMsgDialog dialog = new iFolderMsgDialog(
						this,
						iFolderMsgDialog.DialogType.Question,
						iFolderMsgDialog.ButtonSet.YesNo,
						"",
						Util.GS("Accept the certificate of this server?"),
						string.Format(Util.GS("iFolder is unable to verify \"{0}\" as a trusted server.  You should examine this server's identity certificate carefully."), serverName),
						cert.ToString(true));

					Gdk.Pixbuf certPixbuf = Util.LoadIcon("gnome-mime-application-x-x509-ca-cert", 48);
					if (certPixbuf != null && dialog.Image != null)
						dialog.Image.Pixbuf = certPixbuf;

					int rc = dialog.Run();
					dialog.Hide();
					dialog.Destroy();
					if(rc == -8) // User clicked the Yes button
					{
						simws.StoreCertificate(byteArray, serverName);
						OnConnectClicked(o, args);
					}
					break;
				case StatusCodes.Success:
				case StatusCodes.SuccessInGrace:
					if (WaitDialog != null)
					{
						WaitDialog.Hide();
						WaitDialog.Destroy();
						WaitDialog = null;
					}

					string	password = addDomainThread.Password;
					bool	bRememberPassword = addDomainThread.RememberPassword;

					Status authStatus =
						domainController.AuthenticateDomain(
							dom.ID, password, bRememberPassword);

					if (authStatus != null)
					{
						if (authStatus.statusCode == StatusCodes.Success ||
							authStatus.statusCode == StatusCodes.SuccessInGrace)
						{
							// We connected successfully!
							ConnectedDomain = dom;
							//TODO - Check for recoveryagent status.

							iFolderWebService ifws = DomainController.GetiFolderService();

							int policy = ifws.GetSecurityPolicy(dom.ID);
							if( policy % 2 ==0)
							{
								AccountDruid.Page = SummaryPage;
								AccountDruid.Page = DefaultiFolderPage;
							}
							else
							{
								AccountDruid.Page = RAPage;
								ForwardButton.Sensitive = false;
								AccountDruid.CancelButton.Sensitive = false;
							}
							break;
						}
						else
						{
							Console.WriteLine("Error while authenticating");
							Util.ShowLoginError(this, authStatus.statusCode);
						}
					}
					else
					{
						Util.ShowLoginError(this, StatusCodes.Unknown);
					}
					break;
				default:
					if (WaitDialog != null)
					{
						WaitDialog.Hide();
						WaitDialog.Destroy();
						WaitDialog = null;
					}

					// Failed to connect
					Util.ShowLoginError(this, dom.StatusCode);
					break;
			}
		}
		
		private void OnCancelClicked(object o, Gnome.CancelClickedArgs args)
		{
			CloseDialog();
		}

		private void ChangeSensitivity( object o, EventArgs args)
		{
			if( PassPhraseEntry.Text == PassPhraseVerifyEntry.Text)
				ForwardButton.Sensitive = true;
			else if(PassPhraseVerifyEntry.Text == "" && PassPhraseEntry.Text.Length > 0)
			{
				ForwardButton.Sensitive = true;
			}
			else
				ForwardButton.Sensitive = false;
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
						else if (button.Label == "gtk-go-back")
							BackButton = button;
					}
				}
			}
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
		
		public void CloseDialog()
		{
			this.Hide();
			this.Destroy();
		}

	}
	
	///
	/// Override Gnome.DruidPageStandard for our Connect Page so that we can
	/// override the behavior of the Next button
	///
	public class DruidConnectPage : Gnome.DruidPageStandard
	{
		public event ConnectClickedHandler ConnectClicked;

		public DruidConnectPage(string title, Gdk.Pixbuf logo, Gdk.Pixbuf top_watermark)
			: base (title, logo, top_watermark)
		{
		}
		
		protected override bool OnNextClicked(Widget druid)
		{
			if (ConnectClicked != null)
			{
				if (!ConnectClicked(this, EventArgs.Empty))
					return true;	// Prevent the default event handler (from advancing to the next druid page
			}

			return false;	// Allow the default event handler (to advance to the next druid page)
		}
	}

	///
	/// Override Gnome.DruidPageStandard for our Connect Page so that we can
	/// override the behavior of the Next button
	///
	public class DruidRAPage : Gnome.DruidPageStandard
	{
		public event ValidateClickedHandler ValidateClicked;
		public event SkipClickedHandler SkipClicked;

		public DruidRAPage(string title, Gdk.Pixbuf logo, Gdk.Pixbuf topwatermark)
			: base (title, logo, topwatermark)
		{
		}
		
		protected override bool OnNextClicked(Widget druid)
		{
			if (ValidateClicked != null)
			{
				if (!ValidateClicked(this, EventArgs.Empty))
					return true;	// Prevent the default event handler (from advancing to the next druid page
			}

		    //Validate the passphrase here.
		    
			return false;	// Allow the default event handler (to advance to the next druid page)
		}

	        //Skip.
		protected override bool OnBackClicked(Widget druid)
		{
			if (SkipClicked != null)
			{
				if (!SkipClicked(this, EventArgs.Empty))
					return true;	// Prevent the default event handler (from advancing to the next druid page
			}

			return false;	// Allow the default event handler (to advance to the next druid page)
		}

	}

	public delegate bool ValidateClickedHandler(object o, EventArgs args);
	public delegate bool SkipClickedHandler(object o, EventArgs args);
	
	/// <summary>
	/// Return true if the connect was successful, otherwise return false.
	/// </summary>
	public delegate bool ConnectClickedHandler(object o, EventArgs args);
} 
