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
		private Gnome.DruidPageStandard 	RAPage;
		private DruidConnectPage		MigratePage;
		private Gnome.DruidPageEdge		SummaryPage;
		private DomainController		domainController;
		private DomainInformation[]		domains;
		private iFolderWebService 		ifws;
		private SimiasWebService		simws;
		private bool					ControlKeyPressed;
		private Button						ForwardButton;
		private Button						FinishButton;
		private string				location;
		private string				prevLocation;
		private string 				Uname;
		private bool 				Prepared;
		private iFolderData			ifdata;
		private bool				migrationStatus;
		private bool				alreadyEncrypted;

		public event MigrationValidateClickedHandler ValidateClicked;
		
		private Gdk.Pixbuf				AddAccountPixbuf;
		private MigrationWindow page;

		///
		/// Migration Options page
		///
		private RadioButton 			deleteFromServer;
		private RadioButton			copyToServer;
		
		private RadioButton 			group;

		private CheckButton 			copyDir;
		private Button				BrowseButton;
		private Entry 				LocationEntry;

		///
		/// Server Info page
		///
		private ComboBox 			domainList;
		private RadioButton			encryptionCheckButton;
		private RadioButton			sslCheckButton;
		
		///
		/// Migrate Page Widgets
		///
		private Label		ServerAddressLabel;
		private Label		Location;
		private Label		MigrationOptionLabel;
		private Label		ShowSecurityLabel;
		private Label		SecurityLabel;

		///
		/// RAPage widgets
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
	
		/// Wait Message
		///
		iFolderWaitDialog	WaitDialog;

		public MigrationWizard(string User, string path, iFolderWebService ifws, SimiasWebService simws, MigrationWindow page, bool encrypted) : base(WindowType.Toplevel)
		{
			this.Title = Util.GS("iFolder Migration Assistant");
			this.Resizable = false;
			this.Modal = true;
			this.WindowPosition = Gtk.WindowPosition.Center;
			this.location = path;
			prevLocation = "";
			this.ifdata = iFolderData.GetData();
			this.ifws = ifws;
			this.simws = simws;
			this.Uname = User;
			this.page = page;
			this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));
			this.alreadyEncrypted = encrypted;

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
			AccountDruid.AppendPage(CreateRAPage());
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
				Util.GS("Migrate the iFolder "),
				Util.GS("Welcome to iFolder Migration Assistant.\nThe next few screens will let you,\nmigrate your data to iFolder 3.x.\n\nClick \"Forward\" to continue."),
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
			Table table = new Table(5, 6, false);
			MigrationOptionsPage.VBox.PackStart(table, true, true, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Select one of the following options")+":");
			table.Attach(l, 0,6, 0,1,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			l.LineWrap = true;
			l.Xalign = 0.0F;

			// Row 2
			table.Attach(new Label(""), 0,1, 1,2,
				AttachOptions.Fill, 0,12,0); // spacer
			deleteFromServer = new RadioButton(Util.GS("Migrate the iFolder and disconnect it from 2.x domain"));
			deleteFromServer.Active = true;
			deleteFromServer.Sensitive = true;
			deleteFromServer.Toggled += new EventHandler(OndeleteCheckButtonChanged);
			table.Attach(deleteFromServer, 1,6, 1,2,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			
			// Row 3

			table.Attach(new Label(""), 0,1, 2,3,
				AttachOptions.Fill, 0,12,0); // spacer
			copyToServer = new RadioButton(deleteFromServer, Util.GS("Create a copy of the iFolder and migrate"));
			copyToServer.Active = false;
			copyToServer.Toggled += new EventHandler(OncopyCheckButtonChanged);

			table.Attach(copyToServer, 1,6, 2,3,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			// Row 4

//			table.Attach(new Label("'), 0,1, 3,4, AttachOptions.Shrink, 0,15, 0);  // spacer
			table.Attach(new Label(""), 0,1, 3,4,
				AttachOptions.Shrink, 0,12,0); // spacer
			table.Attach(new Label(""), 1,2, 3,4, AttachOptions.Fill, 0,3,0);

			copyDir = new CheckButton(Util.GS("Copy the parent folder"));	
			copyDir.Sensitive = false;
			table.Attach(copyDir, 2,6, 3,4, AttachOptions.Fill| AttachOptions.Expand, 0,0,0);

			// Row 5
			
			table.Attach(new Label("Location:"), 0,1, 4,5, AttachOptions.Fill, 0,0,0);
			LocationEntry = new Entry();
			LocationEntry.Text = location;
			LocationEntry.Sensitive = false;
			table.Attach(LocationEntry, 1,5, 4,5, AttachOptions.Fill, 0,0,0);	
			BrowseButton = new Button("_Browse");
			table.Attach(BrowseButton, 5,6, 4,5, AttachOptions.Fill, 0,0,0); 
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

			UserInformationPage.NextClicked += new Gnome.NextClickedHandler(OnUserInfoForwardClicked);
			
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
			encryptionCheckButton = new RadioButton(Util.GS("Encrypted"));
			table.Attach(encryptionCheckButton, 1,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			//Row 5
			sslCheckButton = new RadioButton(encryptionCheckButton, Util.GS("Shared"));
			table.Attach(sslCheckButton, 1,3, 4,5, AttachOptions.Fill | AttachOptions.Expand, 0,0,0); 
			return UserInformationPage;
		}

		///
		/// RAPage
		///
		private Gnome.DruidPage CreateRAPage()
		{
			RAPage = new Gnome.DruidPageStandard(
				Util.GS("Passphrase"),
				AddAccountPixbuf,
				null);
			RAPage.CancelClicked += new Gnome.CancelClickedHandler(OnCancelClicked);
			RAPage.Prepared +=
				new Gnome.PreparedHandler(OnRAPagePrepared);
			RAPage.NextClicked +=
				new Gnome.NextClickedHandler(OnValidateClicked);
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

//			AccountDruid.CancelButton.Sensitive = false;

			return RAPage;		
		}

		private void RANameCellTextDataFunc (Gtk.TreeViewColumn tree_column,
				Gtk.CellRenderer cell, Gtk.TreeModel tree_model,
				Gtk.TreeIter iter)
		{
			string value = (string) tree_model.GetValue(iter, 0);
			((CellRendererText) cell).Text = value;
		}

		private void OnValidateClicked(object o, EventArgs args)
		{
			bool NextPage = true;
			string publicKey = "";
		    try {
		        //Validate the PassPhrase Locally.
		        if ( !PassPhraseSet )
			{
			        if (PassPhraseEntry.Text == PassPhraseVerifyEntry.Text)
				{
					string recoveryAgentName = "";
					TreeSelection tSelect = RATreeView.Selection;
					if(tSelect != null && tSelect.CountSelectedRows() == 1)
					{
						TreeModel tModel;
						TreeIter iter;
						tSelect.GetSelected(out tModel, out iter);
						recoveryAgentName = (string) tModel.GetValue(iter, 0);
					}
					if( recoveryAgentName != null && recoveryAgentName != Util.GS("None"))
					{
						// Show Certificate..
						byte [] RACertificateObj = domainController.GetRACertificate((domains[domainList.Active]).ID, recoveryAgentName);
						if( RACertificateObj != null && RACertificateObj.Length != 0)
						{
							System.Security.Cryptography.X509Certificates.X509Certificate Cert = new System.Security.Cryptography.X509Certificates.X509Certificate(RACertificateObj);
							CertificateDialog dlg = new CertificateDialog(Cert.ToString(true));
						/*
							if (!Util.RegisterModalWindow(dlg))
							{
								dlg.Destroy();
								dlg = null;
								return false;
							}
						*/
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
				                                //status = false;
	                        			        simws.StorePassPhrase((domains[domainList.Active]).ID, "", CredentialType.None, false);
								NextPage = false;

							}
						//	string publickey = (string)Cert.GetPublicKey();
							
						}
					}
					if( NextPage)
					{
					        Status passPhraseStatus = domainController.SetPassPhrase ((domains[domainList.Active]).ID, PassPhraseEntry.Text, "");
						if(passPhraseStatus.statusCode == StatusCodes.Success)
						{
							domainController.StorePassPhrase( (domains[domainList.Active]).ID, PassPhraseEntry.Text,
								CredentialType.Basic, RememberPassPhraseCheckButton.Active);
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

				} else {
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
				       
				      // return false;
				}
			} else {
			    // PassPhrase is already set.
			        Status validationStatus = domainController.ValidatePassPhrase ((domains[domainList.Active]).ID, PassPhraseEntry.Text );
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
	                                domainController.StorePassPhrase( (domains[domainList.Active]).ID, PassPhraseEntry.Text,
								  CredentialType.Basic, RememberPassPhraseCheckButton.Active);
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
				AccountDruid.Page = UserInformationPage;
			//	return false;
			}
		//	return true;
		}
		
		///
		/// Connect Page (3 of 3)
		///
		private Gnome.DruidPage CreateMigratePage()
		{
			
			MigratePage = new DruidConnectPage(Util.GS("Verify and Migrate"),AddAccountPixbuf,null);

			MigratePage.CancelClicked +=
				new Gnome.CancelClickedHandler(OnCancelClicked);
			
			MigratePage.ConnectClicked +=
				new ConnectClickedHandler(OnMigrateClicked);

			MigratePage.Prepared +=
				new Gnome.PreparedHandler(OnMigratePagePrepared);

			MigratePage.BackClicked += new Gnome.BackClickedHandler(OnBackButtonClicked);
			
			///
			/// Content
			///
			Table table = new Table(6, 3, false);
			MigratePage.VBox.PackStart(table, false, false, 0);
			table.ColumnSpacing = 6;
			table.RowSpacing = 6;
			table.BorderWidth = 12;

			// Row 1
			Label l = new Label(Util.GS("Please verify that the information you've entered is correct")+".");
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
			l = new Label(Util.GS("Migration Option")+":");
			table.Attach(l, 1,2, 3,4,
				AttachOptions.Shrink | AttachOptions.Fill, 0,0,0);
			l.Xalign = 0.0F;
			MigrationOptionLabel = new Label("");
			table.Attach(MigrationOptionLabel, 2,3, 3,4,
				AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			MigrationOptionLabel.Xalign = 0.0F;
			
			// Row 5
			ShowSecurityLabel = new Label(Util.GS("Security")+":");
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
					Util.GS("Click \"Migrate\" to migrate your folder to the server specified")+"."));
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
			Util.ShowHelp("migration.html", this);
		}

		private void OnIntroductoryPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant");
			AccountDruid.SetButtonsSensitive(false, true, true, true);
		}
		
		private void OnMigrationOptionsPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (1 of 5)");
			
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

		
		private void OnUserInformationPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (2 of 5)");
			ForwardButton.Label = "gtk-go-forward";
			if(Prepared)
				return;
			Prepared = true;
			Console.WriteLine("Preparing UserInformation Page");
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

		private void OnRAPagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (4 of 5)");
			PassPhraseSet = false;
			if( encryptionCheckButton.Active == false)
			{
				Console.WriteLine("On rapage prepared. Encryption");
				return;
			}
			Console.WriteLine("OnRAPagePrepared");
			try
			{
				if ( domainController.IsPassPhraseSet ((domains[domainList.Active]).ID) == false)
				{
				       string[] list = domainController.GetRAList ((domains[domainList.Active]).ID);
					PassPhraseVerifyEntry.Show();
				       RATreeView.Show();
 				       SelectRALabel.Show();
 				       RetypePassPhraseLabel.Show();
				       RATreeStore.Clear();
				       foreach (string raagent in list )
					       RATreeStore.AppendValues (raagent);

				} else {
				       // PassPhrase already available.
				       PassPhraseSet = true;
 				       PassPhraseVerifyEntry.Hide ();
				       RATreeView.Hide();
 				       SelectRALabel.Hide();
 				       RetypePassPhraseLabel.Hide();
				}

				AccountDruid.SetButtonsSensitive(true , true, true, true);
				ForwardButton.Sensitive = false;
			}
			catch(Exception ex)
			{
				iFolderMsgDialog errorMsg = new iFolderMsgDialog(null, 
												iFolderMsgDialog.DialogType.Info,
												iFolderMsgDialog.ButtonSet.Ok,
												Util.GS("Migration"),
												Util.GS("User not Logged-in to the domain"), 
												string.Format(Util.GS("For creating an encrypted iFolder you should be connected to the domain.")));
				errorMsg.Run();
				errorMsg.Destroy();
				AccountDruid.Page = MigrationOptionsPage;
			}
		}
		
		private void OnMigratePagePrepared(object o, Gnome.PreparedArgs args)
		{
			this.Title = Util.GS("iFolder Migration Assistant - (3 of 5)");
			ServerAddressLabel.Text = (domains[domainList.Active]).Name;	
			Location.Text = location;
			
			if(deleteFromServer.Active)
				MigrationOptionLabel.Text = Util.GS("Delete from iFolder2.X server");
			else
				MigrationOptionLabel.Text = Util.GS("Create a copy and migrate to iFolder3.x");
			if(encryptionCheckButton.Active)
			{
				SecurityLabel.Text = Util.GS("Encrypt the iFolder ");
				if(sslCheckButton.Active)
				{
					SecurityLabel.Text += Util.GS("and Use secure channel for data transfer");
				}
			}
			else if(sslCheckButton.Active)
			{
				SecurityLabel.Text = Util.GS("Shared iFolder");
			}
			else
				SecurityLabel.Text = Util.GS("None");
			
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
						Util.GS("Congratulations!  Your folder has been\nsuccessfully migrated to \nthe latest version of iFolder.\nIt is recommended to \ndisconnect from 2.x server to \n avoid multiple synchronizations.\n\nClick \"Finish\" to close this window."));

			// Hack to modify the "Apply" button to be a "Finish" button
//			AccountDruid.Forall(EnableFinishButtonCallback);
				FinishButton.Label = Util.GS("_Finish");
			
				AccountDruid.SetButtonsSensitive(false, true, false, true);
			}
			else
			{
				SummaryPage.Title = Util.GS("Error in Migration");
				SummaryPage.Text = string.Format(Util.GS("Sorry! The iFolder cannot be migrated to the specified account.\n\nPlease try again."));
				AccountDruid.SetButtonsSensitive(true, false, false, false);
			}
		}
	void OnBackButtonClicked(object o, EventArgs args)
	{
			if( encryptionCheckButton.Active == false)
				AccountDruid.Page = RAPage;
			// Check if the passphrase is stored and validated properly..
			
			else 
			{
				Console.WriteLine("Checking for passphrase entered at login");
				string passphrasecheck = simws.GetPassPhrase((domains[domainList.Active]).ID);
				if( passphrasecheck != null && passphrasecheck != "")
				{
					Console.WriteLine("some passphrase exists: {0}", passphrasecheck);
					Status passPhraseStatus =  simws.ValidatePassPhrase((domains[domainList.Active]).ID, passphrasecheck);
					if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success )
					{
						Console.WriteLine("Passphrase is validated");
						AccountDruid.Page = RAPage;
					}
					else if( passPhraseStatus == null)
						Console.WriteLine("Status is null:");
					else if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)
						Console.WriteLine("Passphrase is invalid");
					else
						Console.WriteLine("Some other error");
						
				}
				else 
					Console.WriteLine("No passphrase exists");
			}
	}

	public static bool CopyDirectory(DirectoryInfo source, DirectoryInfo destination) 
	{
		bool success = false;

        	if (!destination.Exists) 
		{
			try
			{
            			destination.Create();
			}
			catch(Exception e)
			{
				return success;
			}
	        }
		
		if(!source.Exists)
		{
			return success;
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
			return success;
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
	        	    return CopyDirectory(dir, new DirectoryInfo(destinationDir));
        		}
		}
		catch(Exception e)
		{
			return success;
		}
		return true;
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
			System.IO.DirectoryInfo source = new System.IO.DirectoryInfo(location);
			System.IO.DirectoryInfo destination = new System.IO.DirectoryInfo(LocationEntry.Text);	
			string destDir = LocationEntry.Text;
			if( copyDir.Active )
			{
				string fileName;
				fileName = source.Name;
				destDir += "/" + fileName;
				System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(destDir);
				if( !di.Exists)
				{
					try
					{
						di.Create();
					}
					catch(Exception ex)
					{
						migrationStatus = false;
					}
				}
				destination = new System.IO.DirectoryInfo(destDir);
			}
			if( ifws.CanBeiFolder(destDir))
			{
				if(copyToServer.Active == true)  // copy the ifolder in location to LocationEntry.Text
				{
					if( !CopyDirectory(source, destination) )
					{
						migrationStatus = false;
						return true;;
					}
				}
				Console.WriteLine("Migrating:");
				DirectoryInfo d = new DirectoryInfo(location);
				
				// Create iFolder
				string Algorithm = null;
				Algorithm = encryptionCheckButton.Active ? "BlowFish" : null;
				if((d.Exists) && (ifdata.CreateiFolder(destDir, (domains[domainList.Active]).ID, sslCheckButton.Active, Algorithm ) == null))
				{
					// error creating the ifolder..
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
			//	copyToServer.Active = false;
			//	copyToServer.Sensitive = true;
			//	deleteFromServer.Sensitive = false;
				prevLocation = LocationEntry.Text;
				LocationEntry.Text = location;
				LocationEntry.Sensitive = false;
				BrowseButton.Sensitive = false;
				copyDir.Sensitive = false;
			}
		}	
		private void OncopyCheckButtonChanged(object o, EventArgs e)
		{
			if(copyToServer.Active == true)
			{
			//	deleteFromServer.Active = false;
			//	deleteFromServer.Sensitive = true;
			//	copyToServer.Sensitive = false;
				LocationEntry.Sensitive = true;
				BrowseButton.Sensitive = true;
				LocationEntry.Text = prevLocation;
				copyDir.Sensitive = true;
				
			}
		}
		private void OnUserInfoForwardClicked(object o, EventArgs args)
		{
			Console.WriteLine("Forward clicked");
			// Check for encryption status. If not yes then, skip the RAPage
			if( encryptionCheckButton.Active == false && alreadyEncrypted == true)
			{
				// 2.x is encrypted and the new folder is not
				iFolderMsgDialog dlg = new iFolderMsgDialog( null, iFolderMsgDialog.DialogType.Info, iFolderMsgDialog.ButtonSet.OkCancel, "Caution", "The original 2.x iFolder is encrypted and the migrated folder is chosen not to encrypt", "Do you want to continue?" );
				int result = dlg.Run();
				dlg.Hide();
				dlg.Destroy();
				if( result != (int) ResponseType.Ok)
				{
					AccountDruid.Page = MigrationOptionsPage;
					return;
				}
			}
			if( encryptionCheckButton.Active == false)
				AccountDruid.Page = RAPage;
			else 
			{
				Console.WriteLine("Checking for passphrase entered at login");
				string passphrasecheck = simws.GetPassPhrase((domains[domainList.Active]).ID);
				if( passphrasecheck != null && passphrasecheck != "")
				{
					Console.WriteLine("some passphrase exists: {0}", passphrasecheck);
					Status passPhraseStatus =  simws.ValidatePassPhrase((domains[domainList.Active]).ID, passphrasecheck);
					if( passPhraseStatus != null && passPhraseStatus.statusCode == StatusCodes.Success )
					{
						Console.WriteLine("Passphrase is validated");
						AccountDruid.Page = RAPage;
					}
					else if( passPhraseStatus == null)
						Console.WriteLine("Status is null:");
					else if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)
						Console.WriteLine("Passphrase is invalid");
					else
						Console.WriteLine("Some other error");
						
				}
				else 
					Console.WriteLine("No passphrase exists");
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
	public delegate bool MigrationValidateClickedHandler(object o, EventArgs args);
} 
