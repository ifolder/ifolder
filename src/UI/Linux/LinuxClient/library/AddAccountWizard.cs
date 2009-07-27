/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
  *
  *                 $Author: Boyd Timothy <btimothy@novell.com>
  *                 $Modified by: <Modifier>
  *                 $Mod Date: <Date Modified>
  *                 $Revision: 0.0
  *-----------------------------------------------------------------------------
  * This module is used to:
  *        Wizard used for creation for Accounts. 
  *
  *
  *******************************************************************************/

using System;
using System.Collections;
using Simias.Client;
using Gtk;

using Novell.iFolder.Controller;
#if NOT_USED
		enum CertificateProblem	: uint
		{
			CertOK						  = 0,
			CertEXPIRED 				  = 0x800B0101,
			CertVALIDITYPERIODNESTING	  = 0x800B0102,
			CertROLE					  = 0x800B0103,
			CertPATHLENCONST			  = 0x800B0104,
			CertCRITICAL				  = 0x800B0105,
			CertPURPOSE 				  = 0x800B0106,
			CertISSUERCHAINING			  = 0x800B0107,
			CertMALFORMED				  = 0x800B0108,
			CertUNTRUSTEDROOT			  = 0x800B0109,
			CertCHAINING				  = 0x800B010A,
			CertREVOKED 				  = 0x800B010C,
			CertUNTRUSTEDTESTROOT		  = 0x800B010D,
			CertREVOCATION_FAILURE		  = 0x800B010E,
			CertCN_NO_MATCH 			  = 0x800B010F,
			CertWRONG_USAGE 			  = 0x800B0110,
			CertUNTRUSTEDCA 			  = 0x800B0112
		}
#endif
namespace Novell.iFolder
{

    /// <summary>
    /// class AddAccount Wizard
    /// </summary>
	public class AddAccountWizard : Window
	{
                enum SecurityState
                {
                        encrypt = 1,
                        enforceEncrypt = 2,
                        SSL = 4,
                        enforceSSL = 8
                }
#if NOT_USED
		enum CertificateProblem	: uint
		{
			CertOK						  = 0,
			CertEXPIRED 				  = 0x800B0101,
			CertVALIDITYPERIODNESTING	  = 0x800B0102,
			CertROLE					  = 0x800B0103,
			CertPATHLENCONST			  = 0x800B0104,
			CertCRITICAL				  = 0x800B0105,
			CertPURPOSE 				  = 0x800B0106,
			CertISSUERCHAINING			  = 0x800B0107,
			CertMALFORMED				  = 0x800B0108,
			CertUNTRUSTEDROOT			  = 0x800B0109,
			CertCHAINING				  = 0x800B010A,
			CertREVOKED 				  = 0x800B010C,
			CertUNTRUSTEDTESTROOT		  = 0x800B010D,
			CertREVOCATION_FAILURE		  = 0x800B010E,
			CertCN_NO_MATCH 			  = 0x800B010F,
			CertWRONG_USAGE 			  = 0x800B0110,
			CertUNTRUSTEDCA 			  = 0x800B0112
		}
#endif			
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
		
		private bool CertAcceptedCond1 = false;
		private ArrayList ServersForCertStore = new ArrayList();
		
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
		private bool AlreadyPrepared;

		///
		/// Default iFolder Page
		///
		private RadioButton encryptionCheckButton;
		private RadioButton sslCheckButton;
		private CheckButton SecureSync;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simws">Simias WebService</param>
		public AddAccountWizard(SimiasWebService simws) : base(WindowType.Toplevel)
		{
			this.Title = Util.GS("iFolder Account Creation Wizard");
			this.Resizable = false;
			this.Modal = true;
			this.WindowPosition = Gtk.WindowPosition.Center;

			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));

			this.simws = simws;
//			this.AlreadyPrepared = false;

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
		
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simws">Simias WebService</param>
        /// <param name="serv">Server</param>
		public AddAccountWizard(SimiasWebService simws, string serv) : this(simws)
		{
            ServerNameEntry.Text = serv;
            ServerNameEntry.Editable = false;
        }
        
        /// <summary>
        /// Create Widgets
        /// </summary>
        /// <returns></returns>
		private Widget CreateWidgets()
		{
			EventBox widgetEventbox = new EventBox();
		    widgetEventbox.ModifyBg(StateType.Normal, this.Style.Background(StateType.Normal));

			VBox vbox = new VBox(false, 0);

			widgetEventbox.Add(vbox);
			
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
			AccountDruid.SetButtonsSensitive(false, true, true, true);
			
			//return vbox;
			return widgetEventbox;
		}
		
		///
		/// Introductory Page
		///
		private Gnome.DruidPage CreateIntroductoryPage()
		{
			IntroductoryPage = new Gnome.DruidPageEdge(Gnome.EdgePosition.Start,
				true,	// use an antialiased canvas
				Util.GS("Configure an iFolder Account"),
				Util.GS("Welcome to the iFolder Account Creation Wizard.\n\nClick \"Forward\" to begin."),
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
			l = new Label(Util.GS("Server _address:"));
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
			MakeDefaultLabel = new Label(Util.GS("Setting this iFolder Server as your default server will enable iFolder to automatically select this server when adding new folders."));
			table.Attach(MakeDefaultLabel, 0,3, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MakeDefaultLabel.LineWrap = true;
			MakeDefaultLabel.Xalign = 0.0F;
			
			// Row 4
			DefaultServerCheckButton = new CheckButton(Util.GS("Set this as my _default server"));
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
					Util.GS("iFolder Account Information"),
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
			l = new Label(Util.GS("_User name:"));
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
			RememberPasswordCheckButton = new CheckButton(Util.GS("_Remember password on this computer"));
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
			Label l = new Label(Util.GS("Please verify the information you have entered."));
			table.Attach(l, 0,3, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			l = new Label(Util.GS("Server address:"));
			table.Attach(l, 1,2, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			ServerNameVerifyLabel = new Label("");
			table.Attach(ServerNameVerifyLabel, 2,3, 1,2,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			ServerNameVerifyLabel.Xalign = 0.0F;

			// Row 3
			l = new Label(Util.GS("User name:"));
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
					Util.GS("Click \"Connect\" to proceed.")));
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
			this.AlreadyPrepared = false;

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
			RetypePassPhraseLabel = new Label(Util.GS("R_etype the passphrase:"));
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
			RememberPassPhraseCheckButton = new CheckButton(Util.GS("_Remember the passphrase"));
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

        /// <summary>
        /// Create Default iFolder Page
        /// </summary>
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
			locationLabel = new Label(Util.GS("_Location:"));
			table.Attach(locationLabel, 1,2, 1,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			//l.Xalign = 0.0F;
			
			LocationEntry = new Entry();
			table.Attach(LocationEntry, 2,4, 1,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			BrowseButton = new Button(Util.GS("_Browse"));
			table.Attach(BrowseButton, 4,5, 1,2, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			BrowseButton.Clicked += new EventHandler(OnBrowseButtonClicked);

			// row 3
			securityLabel = new Label(Util.GS("Security:"));
			table.Attach( securityLabel, 1,2, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
			//l.Xalign = 0.0F;
			encryptionCheckButton = new RadioButton(Util.GS("Passphrase Encryption"));
			table.Attach(encryptionCheckButton, 2,3, 2,3,
					AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        //Row 5
			sslCheckButton = new RadioButton(encryptionCheckButton, Util.GS("Regular"));
			table.Attach(sslCheckButton, 3,4, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);

			SecureSync = new CheckButton(Util.GS("Secure Sync"));
                        table.Attach(SecureSync, 4,5, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
		
			if( upload == false)
			{
				encryptionCheckButton.Visible = false;
				sslCheckButton.Visible = false;
				SecureSync.Visible = false;
			}
			if( this.CreateDefault.Active == false)
			{
				encryptionCheckButton.Sensitive = false;
				sslCheckButton.Sensitive = false;
				LocationEntry.Sensitive = false;
				BrowseButton.Sensitive = false;
				SecureSync.Sensitive = false;
			}
			else
			{
				encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = true;
				LocationEntry.Sensitive = BrowseButton.Sensitive = true;
				SecureSync.Sensitive = true;
				
				if( ConnectedDomain.HostUrl.StartsWith( System.Uri.UriSchemeHttps ) )
                                {
                                	SecureSync.Active = true;
                                        SecureSync.Sensitive = false;
                                }	
				
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

        /// <summary>
        /// Get Default Path
        /// </summary>
        /// <returns>Default Path</returns>
		private string GetDefaultPath()
		{
			string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
			str += "/ifolder/"+ConnectedDomain.Name+"/"+UserNameEntry.Text;
			if( upload == true )
				return str+"/"+"Default_"+UserNameEntry.Text;
			else
				return str;
		}

        /// <summary>
        /// Event Handler for Create Default Changed
        /// </summary>
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
					
					if( ConnectedDomain.HostUrl.StartsWith( System.Uri.UriSchemeHttps ) )
					{
						SecureSync.Active = true;
						SecureSync.Sensitive = false;
					}
					else
					{
						SecureSync.Active = false;
                                                SecureSync.Sensitive = true;
					}
					
						
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

        /// <summary>
        /// Event Handler for Browse Button Clicked
        /// </summary>
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

        /// <summary>
        /// Event Handler for UpdateuserInfo Page sensitivity
        /// </summary>
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

        /// <summary>
        /// Event Handler for Update RA Page Sensitivity
        /// </summary>
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
			this.Title = Util.GS("iFolder Account Creation Wizard");
			AccountDruid.SetButtonsSensitive(false, true, true, true);
		}
		
		private void OnServerInformationPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Creation Wizard");
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
			this.Title = Util.GS("iFolder Account Creation Wizard");
			string str = "";
			this.CreateDefault.Active = true;
			try
			{
				str = this.simws.GetDefaultiFolder( ConnectedDomain.ID );
			}
			catch(Exception ex)
			{
				Debug.PrintLine(String.Format("Exception: old server: {0}", ex.Message));
				AccountDruid.Page = SummaryPage;
				return;
			}
			if(  str == null || str == "")
			{
				Debug.PrintLine("Default account does not exist");
				upload = true;
				LocationEntry.Sensitive = true;
				securityLabel.Sensitive = true;
			}
			else
			{
				Debug.PrintLine("Default account exists");
				upload = false;
				CreateDefault.Label = Util.GS("_Download default iFolder");
				securityLabel.Visible = false;
				encryptionCheckButton.Visible = sslCheckButton.Visible = SecureSync.Visible = false;
			}
			if( passPhraseEntered == true)
				AccountDruid.SetButtonsSensitive(false, true, true, true);
			else
				Debug.PrintLine("Passphrase entered is false");
			iFolderWebService ifws = DomainController.GetiFolderService();
			int securityPolicy = ifws.GetSecurityPolicy(ConnectedDomain.ID);
			ChangeStatus( securityPolicy);
			LocationEntry.Text = GetDefaultPath();
			AccountDruid.BackButton.Sensitive = false;
			AccountDruid.CancelButton.Sensitive = false;
			ForwardButton.Label = Util.GS("Next");

			// Check for ssl and display the status of encryption radfio buttons
		}
        
        /// <summary>
        /// Change Status
        /// </summary>
        /// <param name="SecurityPolicy">Security Policy</param>
		private void ChangeStatus(int SecurityPolicy)
		{
			encryptionCheckButton.Active = sslCheckButton.Active = false;
			encryptionCheckButton.Sensitive = sslCheckButton.Sensitive = false;
			Debug.PrintLine(String.Format("Security Policy is: {0}", SecurityPolicy));	
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
			
			if( ConnectedDomain.HostUrl.StartsWith( System.Uri.UriSchemeHttps ) )
                        {
                                SecureSync.Active = true;
                                SecureSync.Sensitive = false;
                        }
                        else
                                        {
                                                SecureSync.Active = false;
                                                SecureSync.Sensitive = true;
                                        }
					
		}

        /// <summary>
        /// Event Handler for RA Page Prepared
        /// </summary>
		private void OnRAPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Creation Wizard");
			ForwardButton.Label = "gtk-go-forward";
			ForwardButton.Sensitive = false;

			//TODO :
			BackButton.Label = Util.GS("_Skip");
			if( !AlreadyPrepared)
			{
				if ( domainController.IsPassPhraseSet (ConnectedDomain.ID) == false)
				{
					PassPhraseSet = false;
				       string[] list = domainController.GetRAList (ConnectedDomain.ID);
					
					 RATreeStore.AppendValues( Util.GS("Server_Default"));
				       foreach (string raagent in list )
					       RATreeStore.AppendValues (raagent);

				}
				else
				{
				       // PassPhrase already available.
				       PassPhraseSet = true;
 				       PassPhraseVerifyEntry.Hide ();
				       RATreeView.Hide();
 				       SelectRALabel.Hide();
 				       RetypePassPhraseLabel.Hide();
				}
			}

			AccountDruid.SetButtonsSensitive(true , true, true, true);
			AlreadyPrepared = true;
		}

		private void OnUserInformationPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Creation Wizard");
			UpdateUserInformationPageSensitivity(null, null);
			UserNameEntry.GrabFocus();

			// Hack to make sure the "Forward" button has the right text.
			ForwardButton.Label = "gtk-go-forward";
		}

        /// <summary>
        /// Event Handler for Connect Page Prepared
        /// </summary>
		private void OnConnectPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Creation Wizard");

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

        /// <summary>
        /// Event Handler for Summary Page Prepared
        /// </summary>
		private void OnSummaryPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Account Creation Wizard");

			if (ConnectedDomain != null && ConnectedDomain.Name != null && ConnectedDomain.Host != null)
			{
				SummaryPage.Text = 
					string.Format(
						Util.GS("You are now connected to:\n\nServer name: \t{0}\nServer address: {1}\nUser name: \t{2}\n\nClick \"Finish\" to close this wizard."),
						ConnectedDomain.Name,
						ConnectedDomain.Host,
						UserNameEntry.Text.Trim());
			}
			
			// Hack to modify the "Apply" button to be a "Finish" button
//			AccountDruid.Forall(EnableFinishButtonCallback);
			FinishButton.Label = Util.GS("_Finish");
			
			AccountDruid.SetButtonsSensitive(false, true, false, true);
		}

        /// <summary>
        /// Create Default iFolder
        /// </summary>
        /// <returns>true on success</returns>
		private bool CreateDefaultiFolder()
		{
			iFolderData ifdata = iFolderData.GetData();
			iFolderHolder ifHolder = null;

			iFolderWebService ifws = DomainController.GetiFolderService();
			if( ifws.GetLimitPolicyStatus(ConnectedDomain.ID)	!= 1 )
			{

				iFolderMsgDialog dg = new iFolderMsgDialog(
                                                                null,
                                                                iFolderMsgDialog.DialogType.Warning,
                                                                iFolderMsgDialog.ButtonSet.Ok,
                                                                "",
                                                                Util.GS("Error"),
                                                                Util.GS("iFolder could not be created as you are exceeding the limit of ifolders set by your Administrator."));
                                                        dg.Run();
                                                        dg.Hide();
                                                        dg.Destroy();
							dg = null;
				return true;
			}


			// regular 
			if( this.encryptionCheckButton.Active == false)
			{
				Debug.PrintLine("Regular");
				try
				{
					System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(LocationEntry.Text);
					dir.Create();
					if( (ifHolder = ifdata.CreateiFolder(this.LocationEntry.Text, ConnectedDomain.ID, this.SecureSync.Active, null)) == null)
						return false;
					else
					{
						this.simws.DefaultAccount(ConnectedDomain.ID, ifHolder.iFolder.ID);
						AccountDruid.Page = SummaryPage;
						Debug.PrintLine("Making default account and verifying");
						string str = this.simws.GetDefaultiFolder( ConnectedDomain.ID );
						if( str == null)
							Debug.PrintLine("Not set the default account");
						else
							Debug.PrintLine(String.Format("Set the default iFolder to: {0}", str));
						return true;
					}
				}
				catch( Exception ex)
				{
					DisplayCreateOrSetupException(ex, true);
					return false;
				}
			}
			else 
			{
				Debug.PrintLine("encrypted");
				if( this.passPhraseEntered == true )
				{
					Debug.PrintLine("passphrase entered");
					try
					{
						System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(LocationEntry.Text);
						dir.Create();
						if( (ifHolder = ifdata.CreateiFolder(this.LocationEntry.Text, ConnectedDomain.ID, this.SecureSync.Active, "BlowFish")) == null)
						{
							throw new Exception("Simias returned null");
						}
						else
						{
							Debug.PrintLine("Making default account");
							this.simws.DefaultAccount(ConnectedDomain.ID, ifHolder.iFolder.ID);
							string str = this.simws.GetDefaultiFolder( ConnectedDomain.ID );
							if( str == null)
								Debug.PrintLine("Not set the default account");
							else
								Debug.PrintLine(String.Format("Set the default iFolder to: {0}", str));

							AccountDruid.Page = SummaryPage;
							return true;
						}
					}
					catch(Exception ex)
					{
						DisplayCreateOrSetupException(ex, true);
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

        /// <summary>
        /// Event Handler for Default iFolder Validate Clicked
        /// </summary>
		private bool OnDefaultiFolderValidateClicked(object o, EventArgs args)
		{
			// if there is default iFolder present already check for download
			// else upload

			bool status = false;
			
			if( this.CreateDefault.Active == false)
			{
				status = true;
				return status;
			}
			string defaultiFolderID = simws.GetDefaultiFolder( ConnectedDomain.ID );
			if( defaultiFolderID == null || defaultiFolderID == "")
			{
				// No deafult ifolder so create a one
				upload = true;
				status = CreateDefaultiFolder();
				return status;
			}
			else
			{
				// download the deafult ifolder
				upload = false;
				iFolderData ifdata = iFolderData.GetData();
				Debug.PrintLine("Reading ifdata");
				defaultiFolder = ifdata.GetDefaultiFolder( defaultiFolderID );
				if( defaultiFolder != null)
					status = DownloadiFolder();
				else
					Debug.PrintLine("iFolder object is null");
			}			
			return status;
		}
			
        /// <summary>
        /// Download iFolder
        /// </summary>
        /// <returns>true on success</returns>
		private bool DownloadiFolder()
		{
			iFolderData ifdata = iFolderData.GetData();
			bool downLoad = false;
			if( defaultiFolder.encryptionAlgorithm == null || defaultiFolder.encryptionAlgorithm == "")
			{
				downLoad = true;				
			}
			else
			{
				if( this.passPhraseEntered == false )
				{
					// Go to passphrase page to get the passphrase
					waitForPassphrase = true;
					AccountDruid.Page = RAPage;
					return false;
				}
				else
					downLoad = true;				
			}
			
			if( downLoad == true )
			{
				try
				{
					string downloadpath = this.LocationEntry.Text;
					System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(downloadpath);
					if(dir.Name == defaultiFolder.Name)
					{
                                		downloadpath = System.IO.Directory.GetParent(this.LocationEntry.Text).ToString();
						dir = new System.IO.DirectoryInfo(downloadpath);
					}
					dir.Create();
					if( System.IO.Directory.Exists( System.IO.Path.Combine(downloadpath,defaultiFolder.Name))) 
					{
						iFolderMsgDialog DownloadMergeDialog = new iFolderMsgDialog(
                                                null,
                                                iFolderMsgDialog.DialogType.Info,
                                                iFolderMsgDialog.ButtonSet.OkCancel,
                                                Util.GS("A folder with the same name already exists."),
                                                string.Format(Util.GS("Click Ok to merge the folder or Cancel to select a different location")),null);
                                                int rc = DownloadMergeDialog.Run();
                                                DownloadMergeDialog.Hide();
                                                DownloadMergeDialog.Destroy();
                                                if ((ResponseType)rc == ResponseType.Ok)
                                                {
                                                        ifdata.AcceptiFolderInvitation( defaultiFolder.ID, defaultiFolder.DomainID, System.IO.Path.Combine(downloadpath,defaultiFolder.Name),true);
                                                }
						else
							return false;
					}
					else 
						ifdata.AcceptiFolderInvitation( defaultiFolder.ID, defaultiFolder.DomainID, downloadpath);
					Debug.PrintLine("finished accepting invitation");
					AccountDruid.Page = SummaryPage;
					return true;
				}
				catch(Exception ex)
				{
					//	Debug.PrintLine("Exception: Unable to download: {0}", ex.Message);
					DisplayCreateOrSetupException(ex, true);
					AccountDruid.Page = DefaultiFolderPage;
					return false;
				}
			}
			return false;
		}

        /// <summary>
        /// Event Handler for Validate Clicked event
        /// </summary>
		private bool OnValidateClicked(object o, EventArgs args)
		{
			bool NextPage = true;
			string publicKey = null;
			string memberUID = null;
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
															Util.GS("Passphrase mismatch"),
															Util.GS("The passphrase and retyped passphrase are not same"),
															Util.GS("Please enter the passphrase again"));
															dialog.Run();
															dialog.Hide();
															dialog.Destroy();
															dialog = null;				        
															NextPage = false;
					}
					else
					{
						string recoveryAgentName = null;
						TreeSelection tSelect = RATreeView.Selection;
						if(tSelect != null && tSelect.CountSelectedRows() == 1)
						{
							TreeModel tModel;
							TreeIter iter;
							tSelect.GetSelected(out tModel, out iter);
							recoveryAgentName = (string) tModel.GetValue(iter, 0);
							//if( recoveryAgentName == Util.GS("None"))
							//	recoveryAgentName = null;
						}
						if( recoveryAgentName != null && recoveryAgentName != Util.GS("Server_Default") )
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
									Debug.PrintLine(String.Format(" The public key is: {0}", publicKey));
								}
								else
								{
									Debug.PrintLine("Response type is not ok");
								        simws.StorePassPhrase(ConnectedDomain.ID, "", CredentialType.None, false);
							//		this.passPhraseEntered = true;
									NextPage = false;
								}
							}
						}
                     /*   else
                        {
                            iFolderMsgDialog dg = new iFolderMsgDialog(
                                this, 
                                iFolderMsgDialog.DialogType.Warning,
                                iFolderMsgDialog.ButtonSet.YesNo,
                                "No Recovery Agent",
                                Util.GS("Recovery Agent Not Selected"),
                                Util.GS("There is no Recovery Agent selected. Encrypted data cannot be recovered later, if passphrase is lost. Do you want to continue?"));
		       	                int rc = dg.Run();
                	    		dg.Hide();
            	                dg.Destroy();
                                if( (ResponseType)rc != ResponseType.Yes )
                                {
                                    NextPage = false;
                                }
                        }*/


			else
			{
				recoveryAgentName = "DEFAULT";
				DomainInformation domainInfo = (DomainInformation)this.simws.GetDomainInformation(ConnectedDomain.ID);
				memberUID = domainInfo.MemberUserID;
				iFolderWebService ifws = DomainController.GetiFolderService();
				try{
					publicKey = ifws.GetDefaultServerPublicKey(ConnectedDomain.ID,memberUID);
				}
				catch
				{
					return false;
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
																Util.GS("Error setting the passphrase"),
																Util.GS("Unable to set the passphrase"),
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
					Debug.PrintLine("Validating passphrase");
					Status validationStatus = domainController.ValidatePassPhrase (ConnectedDomain.ID, PassPhraseEntry.Text );
					if (validationStatus.statusCode == StatusCodes.PassPhraseInvalid ) 
					{
						NextPage = false;
						iFolderMsgDialog dialog = new iFolderMsgDialog(
						null,
						iFolderMsgDialog.DialogType.Error,
						iFolderMsgDialog.ButtonSet.None,
						Util.GS("PassPhrase Invalid"),
						Util.GS("The PassPhrase entered is not valid"),
						Util.GS("Please enter the passphrase again"));
						dialog.Run();
						dialog.Hide();
						dialog.Destroy();
						dialog = null;				        
					}
					else if(validationStatus.statusCode == StatusCodes.Success )
					{
						Debug.PrintLine("Success. storing passphrase");
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
				Debug.PrintLine("In the same page");
				AccountDruid.Page = RAPage;
				return false;
			}
			BackButton.Label = Util.GS("gtk-go-back");
			return true;
		}

        /// <summary>
        /// Event Handler for Skip Clicked event
        /// </summary>
		private bool OnSkipClicked(object o, EventArgs args)
		{
			AccountDruid.Page = DefaultiFolderPage;

			BackButton.Label = Util.GS("gtk-go-back");
			return false;
		}

        /// <summary>
        /// Event Handler for DefaultAccpunt Skip clicked event
        /// </summary>
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

        /// <summary>
        /// Event Handler for Add Domain Completed
        /// </summary>
		private void OnAddDomainCompleted(object o, EventArgs args)
		{
			AddDomainThread addDomainThread = (AddDomainThread)o;
			DomainInformation dom = addDomainThread.Domain;
			string serverName = addDomainThread.ServerName;
			Exception e = addDomainThread.Exception;
			Gdk.Pixbuf certPixbuf1 = new Gdk.Pixbuf(Util.ImagesPath("ifolder-application-x-x509-ca-cert_48.png"));
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
						Util.GS(String.Format("Msg {0} \n Stack {1}", e.Message, e.StackTrace)));
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

					serverName = dom.HostUrl != null?dom.HostUrl:addDomainThread.ServerName;
					CertificateProblem certprob;
					byte[] byteArray = simws.GetCertificate2(serverName, out certprob);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(byteArray);

					if(CertPolicy.CertificateProblem.CertEXPIRED.Equals(certprob))
					{
						iFolderMsgDialog dialog1 = new iFolderMsgDialog(
							this,
							iFolderMsgDialog.DialogType.Question,
							iFolderMsgDialog.ButtonSet.YesNo,
							Util.GS("Unable to Verify Identity"),
							Util.GS("Expired!"),
							string.Format(Util.GS("Certificate Expired! for \"{0}\" iFolder server.  You should examine this server's identity certificate carefully.Do you still want to continue?"), serverName),
							cert.ToString(true));
						
						if (certPixbuf1 != null && dialog1.Image != null)
							dialog1.Image.Pixbuf = certPixbuf1;
						
						int rc1 = dialog1.Run();
						dialog1.Hide();
						dialog1.Destroy();
						if(rc1 == 8) // User clicked the Yes button
						{
							iFolderMsgDialog dialog = new iFolderMsgDialog(
								this,
								iFolderMsgDialog.DialogType.Question,
								iFolderMsgDialog.ButtonSet.YesNo,
								Util.GS("Unable to Verify Identity"),
								Util.GS("Accept the certificate of this server?"),
								string.Format(Util.GS("iFolder is unable to verify \"{0}\" as a trusted server.  You should examine this server's identity certificate carefully."), serverName),
								cert.ToString(true));
		
							if (certPixbuf1 != null && dialog.Image != null)
								dialog.Image.Pixbuf = certPixbuf1;
		
							int rc = dialog.Run();
							dialog.Hide();
							dialog.Destroy();
							if(rc == -8) // User clicked the Yes button
							{
								if( !(serverName.ToLower()).StartsWith(Uri.UriSchemeHttp))
								{
									serverName = (new Uri( Uri.UriSchemeHttps + Uri.SchemeDelimiter + serverName.TrimEnd( new char[] {'/'}))).ToString();
		//							serverName = Uri.UriSchemeHttps + "://" + serverName ;
								}
								else
								{
									UriBuilder ub = new UriBuilder(serverName);
													   ub.Scheme = Uri.UriSchemeHttps;
									serverName = ub.ToString(); 
								}
								ServerNameEntry.Text = string.Copy(serverName);
								simws.StoreCertificate(byteArray, serverName);
								CertAcceptedCond1 = true;
								ServersForCertStore.Add(serverName);
								OnConnectClicked(o, args);
							}
							else
							{
								CertAcceptedCond1 = false;	
								simws.RemoveCertFromTable(serverName);
							}
						}
					}
					else
					{
							iFolderMsgDialog dialog = new iFolderMsgDialog(
								this,
								iFolderMsgDialog.DialogType.Question,
								iFolderMsgDialog.ButtonSet.YesNo,
								Util.GS("Unable to Verify Identity"),
								Util.GS("Accept the certificate of this server?"),
								string.Format(Util.GS("iFolder is unable to verify \"{0}\" as a trusted server.  You should examine this server's identity certificate carefully."), serverName),
								cert.ToString(true));
		
							if (certPixbuf1 != null && dialog.Image != null)
								dialog.Image.Pixbuf = certPixbuf1;
		
							int rc = dialog.Run();
							dialog.Hide();
							dialog.Destroy();
							if(rc == -8) // User clicked the Yes button
							{
								if( !(serverName.ToLower()).StartsWith(Uri.UriSchemeHttp))
								{
									serverName = (new Uri( Uri.UriSchemeHttps + Uri.SchemeDelimiter + serverName.TrimEnd( new char[] {'/'}))).ToString();
		//							serverName = Uri.UriSchemeHttps + "://" + serverName ;
								}
								else
								{
									UriBuilder ub = new UriBuilder(serverName);
													   ub.Scheme = Uri.UriSchemeHttps;
									serverName = ub.ToString(); 
								}
								ServerNameEntry.Text = string.Copy(serverName);
								simws.StoreCertificate(byteArray, serverName);
								CertAcceptedCond1 = true;
								ServersForCertStore.Add(serverName);
								OnConnectClicked(o, args);
							}
							else
							{
								CertAcceptedCond1 = false;	
								simws.RemoveCertFromTable(serverName);
							}

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

							// remove the certificate of master
							if(ServersForCertStore.Count > 1 )
							{
								string serverKey = ServersForCertStore[ServersForCertStore.Count - 2] as string;
								UriBuilder TempServerUri = new UriBuilder(serverKey);
								//byte[] byteArrayCert = simws.GetCertificate(TempServerUri.Host);
								//if (CertAcceptedCond1)
								{
									//simws.StoreCertificate(byteArrayCert, TempServerUri.Host);
									simws.RemoveCertFromTable(TempServerUri.Host);
								}	
							}
							ServersForCertStore.Clear();

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
							Debug.PrintLine("Error while authenticating");
							Util.ShowLoginError(this, authStatus.statusCode);
						}
					}
					else
					{
						Util.ShowLoginError(this, StatusCodes.Unknown);
					}
					if(dom.ID != null)
					{
						// Fix for the DNS name change issue. bug #346207
					//	simws.SetDomainHostAddress(dom.ID, serverName, UserNameEntry.Text, PasswordEntry.Text);
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

        /// <summary>
        /// Event Handler for On Cancel Clicked
        /// </summary>
		private void OnCancelClicked(object o, Gnome.CancelClickedArgs args)
		{
			CloseDialog();
            ShowNextWizard();
		}

        /// <summary>
        /// Event Handler for Change Sensitivity
        /// </summary>
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

        /// <summary>
        /// Event Handler for On Finish Clicked
        /// </summary>
		private void OnFinishClicked(object o, Gnome.FinishClickedArgs args)
		{
			CloseDialog();
            if(!ShowNextWizard())
            {
                Util.ShowiFolderWindow();
            }
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
		public static bool DisplayCreateOrSetupException(Exception e, bool displayDialog)
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
                if( displayDialog )
                {
	    			iFolderMsgDialog dg = new iFolderMsgDialog(
		    			null,
			    		iFolderMsgDialog.DialogType.Warning,
				    	iFolderMsgDialog.ButtonSet.Ok,
					    "",
    					primaryText,
	    				secondaryText);
		    		dg.Run();
			    	dg.Hide();
				    dg.Destroy();
                }
                else
                {
                    iFolderWindow.log.Info("{0} \n {1}", primaryText,secondaryText);
                }
				
				return true;
			}
			else
			{
                if( displayDialog )
                {
				    iFolderExceptionDialog ied =
					    new iFolderExceptionDialog(
						null,
						e);
    				ied.Run();
	    			ied.Hide();
		    		ied.Destroy();
                }
			}
			
			return false;
		}
	
        /// <summary>
        /// Show Next Wizard
        /// </summary>
        /// <returns>true on success</returns>
        public bool ShowNextWizard()
        {
            try
            {
                AddAccountWizard aaw = null;
                aaw = (AddAccountWizard)WizardStack.Pop();
                if(null != aaw)
                    aaw.Maximize();
            }
            catch(Exception e)
            {
                //iFolderWindow.log.Info("Excepion in Pop of ShowNextWizard. {0} {1}", e.GetType(), e.Message);
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// close Dialog
        /// </summary>
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

    public class WizardStack
    {
        static Stack s = new Stack(32);

        public static void Push( AddAccountWizard a )
        {
            s.Push(a);
        }

        public static AddAccountWizard Pop()
        {
            return (AddAccountWizard)s.Pop();
        }
    }
} 
