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
  *                 $Author: Abhilash<gabhilash@novell.com>
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
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using Mono.Security.Authenticode;
using Mono.Security.X509;

using Novell.iFolder;
//using Novell.iFolder.Utility;

namespace Novell.iFolder
{

        public class KeyRecoveryWizard : Window
        {

		private Gnome.Druid                    	KeyRecoveryDruid;
        	
		private modelKeyPage 					DomainSelectionPage;
		private modelKeyPage 					EnterPassphrasePage;
		private Gnome.DruidPageStandard 		InfoPage;
		private modelKeyPage					SelectionPage; 
		private modelKeyPage 			 		SingleWizPage;
		private modelKeyPage	 				ImportKeyPage;
		private Gnome.DruidPageStandard					FinishPage;
		private modelKeyPage					ExportKeyPage; 
		private Gnome.DruidPageStandard			EmailPage;	


	  	private bool                            ControlKeyPressed;
	
		private Button             	        	ForwardButton;
	    private Button                          BackButton;
        private Button                          FinishButton;
		
		        // private Image                                    iFolderBanner;
                //private Image                            iFolderScaledBanner;
               // private Gdk.Pixbuf                       ScaledPixbuf;

		private Gdk.Pixbuf                      KeyRecoveryPixbuf;
	
		private SimiasWebService                simws;	
		private iFolderWebService               ifws;	

		//DomainSelectionPage
		
		private ComboBox  domainComboBox;
		 private DomainInformation[] domains = null;
		
		
		//EnterPassphrase Page
		
		private Entry iFolderAcc;
		private Entry newPassphrase;
		private Entry confirmPassphrase;
		private Entry userName;
		private Entry password;
		
		//Selection Page
		private RadioButton haveBoth;
		private RadioButton haveOne;
		private RadioButton haveNone;



		//Export Key Page
		private Entry exportDomain;
                private Button 	exportBrowseButton;
                private Entry exportLocation;
               // private Entry email;
                private Entry recoveryAgent;
                private string emailAddress;
              

	//Import Key Page

		private Entry importDomain;
                private Button importBrowseButton;
                private Entry importLocation;
				private Entry oneTimePassphrase;
                private Entry importPageNewPassphrase;
                private Entry importPageConfirmPassphrase;
                private string DomainID;
        	private CheckButton isEncrypted;      


	//Single wizard page
	
                private Entry singleWizDomain;
		private Entry p12FilePath;
                private Entry domainPasswd;
                private Entry singleWizNewPassphrase;
                private Entry singleWizConfirmPassphrase;
				private Button singleWizBrowseButton;	
			    private string titleTag = "CryptoKeyRecovery";
                private string CollectionIDTag = "iFolderCollection";
                private string iFolderIDTag = "iFolderID";
                private string KeyTag = "Key";
				private string singleWizExportPath;	
				private string singleWizImportPath;
              
                private RSACryptoServiceProvider rsadec;

            //Email Page
            private Entry exportedPath;
            private Entry emailID;





	public KeyRecoveryWizard(SimiasWebService simws,iFolderWebService ifws):base(WindowType.Toplevel)
	{
			  		this.Title = Util.GS("Passphrase Recovery Wizard");
	                this.Resizable = false;
                    this.Modal = true;
                    this.WindowPosition = Gtk.WindowPosition.Center;
					this.simws=simws;
					this.ifws=ifws;
                        
					this.Icon = new Gdk.Pixbuf(Util.ImagesPath("ifolder16.png"));



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

                        KeyRecoveryPixbuf = new Gdk.Pixbuf(Util.ImagesPath("ifolder48.png"));
                        KeyRecoveryPixbuf = KeyRecoveryPixbuf.ScaleSimple(48, 48, Gdk.InterpType.Bilinear);

                        KeyRecoveryDruid = new Gnome.Druid();
                        vbox.PackStart(KeyRecoveryDruid, false,false ,0);

                        KeyRecoveryDruid.ShowHelp = true;
                        //KeyRecoveryDruid.Help += new EventHandler(OnKeyRecoveryWizardHelp);

			KeyRecoveryDruid.AppendPage(CreateDomainSelectionPage());
			KeyRecoveryDruid.AppendPage(CreateEnterPassphrasePage());
			KeyRecoveryDruid.AppendPage(CreateInfoPage());
                        KeyRecoveryDruid.AppendPage(CreateSelectionPage());
                        KeyRecoveryDruid.AppendPage(CreateSingleWizPage());
			KeyRecoveryDruid.AppendPage(CreateImportKeyPage());
                      	KeyRecoveryDruid.AppendPage(CreateExportKeyPage());
			KeyRecoveryDruid.AppendPage(CreateEmailPage());
        		KeyRecoveryDruid.AppendPage(CreateFinishPage());
	                KeyRecoveryDruid.SetButtonsSensitive(false, true, true, true);

                        return vbox;
                }



		public string selectedDomainName
		{
		
			get{
				DomainInformation domainInfo = (DomainInformation)this.simws.GetDomainInformation(this.selectedDomain);
				return domainInfo.Name;

				}
		}

		public string selectedDomain
		{
			get
			{  
					if( domains != null)
		                             return domains[domainComboBox.Active].ID;
					else
					        return null;
			}	
		}
		
				private modelKeyPage CreateDomainSelectionPage()
				{
						DomainSelectionPage = new modelKeyPage(Util.GS("Select account"),KeyRecoveryPixbuf,null);
						
						DomainSelectionPage.CancelClicked += new Gnome.CancelClickedHandler(OnCancelClicked);
			
						DomainSelectionPage.Prepared += new Gnome.PreparedHandler(OnDomainSelectionPagePrepared);
			
						DomainSelectionPage.ValidateClicked += new KRValidateClickedHandler(OnDomainSelectionPageValidated);
		
						Table table = new Table(4, 3, false);
    	                DomainSelectionPage.VBox.PackStart(table,false,false, 0);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;
					
						//Row 1
						  Label l1 = new Label(Util.GS("Select the account for which the passphrase must be reset."));
                        table.Attach(l1, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l1.LineWrap = true;
                        l1.Xalign = 0.0F;
	
						//Row 2
							Label l2 = new Label(Util.GS("iFolder Account")+":");
			
							table.Attach(l2, 0,1,5,6,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l2.LineWrap = true;
                        l2.Xalign = 0.0F;
			
		
							
		//DomainComboBox
								         
			domainComboBox = ComboBox.NewText();
                        // read domains from domain controller...
                        domainComboBox = ComboBox.NewText();
			DomainController domainController = DomainController.GetDomainController();
                        domains= domainController.GetLoggedInDomains();
                        for (int x = 0; x < domains.Length; x++)
                        {
                                domainComboBox.AppendText(domains[x].Name);
                        }
                        if( domains.Length > 0)
                                domainComboBox.Active = 0;
                        // read domains from domain controller...
                        table.Attach(domainComboBox, 1,2,5,6, AttachOptions.Fill, 0,0,0);


                        domainComboBox.Changed += new EventHandler(OnDomainChangedEvent);
			            l2.MnemonicWidget = domainComboBox;

                        //Row 3
                        Label l3 = new Label(Util.GS("Click Next to proceed."));
                        table.Attach(l3, 0, 1, 7, 8,
                                AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);
                        l3.LineWrap = true;
                        l3.Xalign = 0.0F;
				return DomainSelectionPage;	
						
				}


                  private void OnDomainSelectionPagePrepared(object o, Gnome.PreparedArgs args)
                {
                    this.Title = Util.GS("Passphrase Recovery Wizard");
			UpdateSensitivity();
                    KeyRecoveryDruid.SetButtonsSensitive(false, false, true, true);
                }


            private bool OnDomainSelectionPageValidated(object obj, EventArgs args)

            {
	              	
			  
                        DomainController domController = DomainController.GetDomainController();
                        string raName = domController.GetRAName(this.selectedDomain);
			if(raName == "DEFAULT")
			{
				KeyRecoveryDruid.Page = DomainSelectionPage;
			}
			else
				KeyRecoveryDruid.Page = EnterPassphrasePage;

                return true;
            }

               private void OnDomainChangedEvent( object o, EventArgs args)
                {
                	UpdateSensitivity();
		}
		
		private void UpdateSensitivity()
		{
			 if( ifws.GetSecurityPolicy(this.selectedDomain) != 0 && simws.IsPassPhraseSet(this.selectedDomain))
				 KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);

			else
				 KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);
	}	

            private Gnome.DruidPage  CreateEnterPassphrasePage()
            {
		EnterPassphrasePage = new modelKeyPage(Util.GS("Set New Passphrase"), KeyRecoveryPixbuf, null);

                EnterPassphrasePage.CancelClicked += new Gnome.CancelClickedHandler(OnCancelClicked);

                EnterPassphrasePage.Prepared += new Gnome.PreparedHandler(OnEnterPassphrasePagePrepared);

                EnterPassphrasePage.ValidateClicked += new KRValidateClickedHandler(OnEnterPassphrasePageValidated);

                Table table = new Table(4, 3, false);
                EnterPassphrasePage.VBox.PackStart(table, false, false, 0);
                table.ColumnSpacing = 6;
                table.RowSpacing = 6;
                table.BorderWidth = 12;

                // Row 1
                Label l = new Label(Util.GS("iFolder Account") + ":");
                l.Xalign = 0.0F;
                table.Attach(l, 0, 1, 0, 1,
                        AttachOptions.Fill, 0, 0, 0);

                iFolderAcc = new Entry();
                iFolderAcc.Sensitive = false;
		 table.Attach(iFolderAcc, 1, 2, 0, 1,
                        AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
                l.MnemonicWidget = iFolderAcc;

                // Row 3
                l = new Label(Util.GS("New Passphrase") + ":");
                l.Xalign = 0.0F;
                table.Attach(l, 0, 1, 1, 2,
                        AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);
                
                newPassphrase = new Entry();
                newPassphrase.Visibility = false;
                newPassphrase.Changed += new EventHandler(OnEnterPassphraseFieldsChanged);
                table.Attach(newPassphrase, 1, 2, 1, 2,
                        AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
                l.MnemonicWidget = newPassphrase;

                // Row 4
                l = new Label(Util.GS("Confirm Passphrase") + ":");
                l.Xalign = 0.0F;
                table.Attach(l, 0, 1, 2, 3,
                        AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);
                
                confirmPassphrase = new Entry();
                confirmPassphrase.Visibility = false;
                confirmPassphrase.Changed += new EventHandler(OnEnterPassphraseFieldsChanged);
                table.Attach(confirmPassphrase, 1, 2, 2, 3,
                        AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
                l.MnemonicWidget = confirmPassphrase;
		
		 l = new Label(Util.GS("Username") + ":");
                l.Xalign = 0.0F;
                table.Attach(l, 0, 1, 3, 4,
                        AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);

		userName = new Entry();
		userName.Changed += new EventHandler(OnEnterPassphraseFieldsChanged);
		 table.Attach(userName, 1, 2, 3, 4,
                        AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
                l.MnemonicWidget = userName;

		 l = new Label(Util.GS("Password") + ":");
                l.Xalign = 0.0F;
                table.Attach(l, 0, 1, 4, 5,
                        AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);

		password = new Entry();
		password.Visibility = false;
		password.Changed += new EventHandler(OnEnterPassphraseFieldsChanged);
		 table.Attach(password, 1, 2, 4, 5,
                        AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
                l.MnemonicWidget = password;


		return EnterPassphrasePage;
			
            }


            private void OnEnterPassphrasePagePrepared(object o, Gnome.PreparedArgs args)
            {
                this.Title = Util.GS("Passphrase Recovery Wizard");
		this.iFolderAcc.Text = this.selectedDomainName;
                //KeyRecoveryDruid.SetButtonsSensitive(true, false, true, true);
            }


            private bool OnEnterPassphrasePageValidated(object obj, EventArgs args)
            {
	
		bool result = false;
		Status status = null;

		
		 DomainInformation domainInfo = (DomainInformation)this.simws.GetDomainInformation(this.selectedDomain);
		try{
		 status = this.simws.LogoutFromRemoteDomain(domainInfo.ID);
                if (status.statusCode == StatusCodes.Success)

		{
		//	Console.WriteLine("Inside login");
                    status = this.simws.LoginToRemoteDomain(domainInfo.ID, this.password.Text);
			if(status.statusCode != StatusCodes.Success)
			{
                               iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,Util.GS("Passphrase reset failed"), Util.GS("Unable to authenticate to the domain.You have been logged out of the account.Please login and try again"), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;
               		 //         EnterPassphrasePage.Hide();
		         return false;
			//		CloseDialog();

			//	Console.WriteLine("Inside not success");

			}
		result = true;
		}
                }
		catch(Exception e)
		{
			 iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,Util.GS("Passphrase reset failed"), Util.GS("Unable to authenticate to the domain.You have been logged out of the account.Please login and try again"), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;
                                EnterPassphrasePage.Hide(); 

		}
		if(result)
		{
		 string memberUID = domainInfo.MemberUserID;
	         string publicKey = null;

		try{	
			//Console.WriteLine("Inside recover");
                    this.simws.ExportRecoverImport(domainInfo.ID, memberUID, this.newPassphrase.Text);
			KeyRecoveryDruid.Page = FinishPage;
			}
			catch(Exception e)
			{
                              iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,Util.GS("Passphrase reset failed"), Util.GS("You have been logged out of the account.Please login and try again."), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;
                       CloseDialog(); 
		
			}
		//	KeyRecoveryDruid.Page = FinishPage;
		//	result = true;
		}

			
		return result;
            }


	 private void OnEnterPassphraseFieldsChanged(object obj, EventArgs args)
                {
	         
			if(this.newPassphrase.Text.Length > 0 && this.confirmPassphrase.Text.Length > 0  && this.newPassphrase.Text == this.confirmPassphrase.Text)
			if(userName.Text.Length > 0 && password.Text.Length > 0)	
			 KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);
			else
				 KeyRecoveryDruid.SetButtonsSensitive(true, false, true, true);
      
		 }
                ///
                /// Info Page
                ///
              private Gnome.DruidPage CreateInfoPage()
                {

                        InfoPage = new Gnome.DruidPageStandard(
                                Util.GS("Welcome to the Passphrase Recovery Wizard"),
                                KeyRecoveryPixbuf, null);

                        InfoPage.CancelClicked +=
                                new Gnome.CancelClickedHandler(OnCancelClicked);

                        InfoPage.Prepared +=
                                new Gnome.PreparedHandler(OnInfoPagePrepared);
			// InfoPage.SkipClicked +=
                          //      new Gnome.SkipClickedHandler(OnInfoPageSkipClicked);

                        ///
                        /// Content
                        ///
                        Table table = new Table(4,3, false);

                        InfoPage.VBox.PackStart(table, false, false, 0);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;

 

                        // Row 2
                        Label l2 = new Label(Util.GS("To recover a lost passphrase, you need the private key file (private key of the certificate) and the password to it or the decrypted key file from the administrator."));
                        table.Attach(l2,0,1, 1,2,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l2.LineWrap = true;
                        l2.Xalign = 0.0F;


                        // Row 5
                        Label l5 = new Label(Util.GS("If you don't have these parameters, you can use this wizard to export the encrypted key file, which you can mail to your administrator who will decrypt and send it back to you. You can then use this decrypted key file to reset your passphrase."));

                        table.Attach(l5, 0,1, 4,5,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l5.LineWrap = true;
                        l5.Xalign = 0.0F;

		//	KeyRecoveryDruid.BackButton.Sensitive = false;
			return InfoPage;
                }

            private void OnInfoPagePrepared(object o, Gnome.PreparedArgs args)
            {
                this.Title = Util.GS("Passphrase Recovery Wizard");

                KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);
		 KeyRecoveryDruid.BackButton.Sensitive = false;
            }
 

		   private bool OnInfoPageSkipClicked(object o, EventArgs args)
                {
                        KeyRecoveryDruid.Page = DomainSelectionPage;

                       // BackButton.Label = Util.GS("gtk-go-back");
                        return true;
                }
 

		 	private void OnCancelClicked(object o, Gnome.CancelClickedArgs args)
                {
		                        CloseDialog();
		        		ShowNextWizard();
                }

		///Selection Page
		
		private Gnome.DruidPage CreateSelectionPage()
		
		{
			SelectionPage = new modelKeyPage("Select Passphrase Recovery Step",KeyRecoveryPixbuf,null);
					
			SelectionPage.CancelClicked += new Gnome.CancelClickedHandler(OnCancelClicked);
			
			SelectionPage.Prepared += new Gnome.PreparedHandler(OnSelectionPagePrepared);
			
			SelectionPage.ValidateClicked += new KRValidateClickedHandler(OnSelectionPageValidated);

            ///
           /// Content
           ///
                        Table table = new Table(4, 3, false);
                     SelectionPage.VBox.PackStart(table, false, false, 0);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;
	
			 Label l = new Label(Util.GS("Select an option to recover your passphrase."));
                        table.Attach(l, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
                        l.Xalign = 0.0F;



			//Choice 1: have Both parameters				
			 haveBoth = new RadioButton(Util.GS("I have the secret file and its password"));
                        table.Attach(haveBoth, 0,1, 1,2,
                                        AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
			haveBoth.Sensitive = true;
                       	haveBoth.Active = true; 
			

			//Choice 2: have one parameter
                        haveOne = new RadioButton(haveBoth, Util.GS("I have the new data file sent by the administrator"));
                        table.Attach(haveOne, 0,1, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
			haveOne.Sensitive = true;


			//Choice 3:have none

			haveNone = new RadioButton(haveBoth,Util.GS("I have no secret or data file."));
			table.Attach(haveNone,0,1,3,4,AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
			haveNone.Sensitive = true;

			return SelectionPage;
		}

		private void OnSelectionPagePrepared(object o, Gnome.PreparedArgs args)
                {
                        this.Title = Util.GS("Passphrase Recovery Wizard");
                        KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);
                }
		
		private bool OnSelectionPageValidated(object o,EventArgs args)
		{
			if(this.haveBoth.Active == true)
			{
				KeyRecoveryDruid.Page = SelectionPage;
				
				
		}
	
			else if(this.haveOne.Active == true)

			{
				KeyRecoveryDruid.Page = SingleWizPage;
				
			}
				
			else
			{
				KeyRecoveryDruid.Page = ImportKeyPage;
				
			}
			return true;
		
		}	

	///
	///Export Page
	///
	
     

	private Gnome.DruidPage CreateExportKeyPage()
	{
	
			ExportKeyPage =  new modelKeyPage(
                                        Util.GS("Obtain old data file"),
                                        KeyRecoveryPixbuf,
                                        null);

         	 ExportKeyPage.CancelClicked +=
                                new Gnome.CancelClickedHandler(OnCancelClicked);

             ExportKeyPage.Prepared +=
                                new Gnome.PreparedHandler(OnExportKeyPagePrepared);
			
				ExportKeyPage.ValidateClicked +=
								new KRValidateClickedHandler(OnExportKeyPageValidated);
				ExportKeyPage.SkipClicked +=
                                new KRSkipClickedHandler(OnExportKeyPageSkipClicked);


                        ///
                        /// Content
                        ///
                        Table table = new Table(4, 3, false);
                        ExportKeyPage.VBox.PackStart(table, false, false, 0);
						table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;

                        // Row 1
                        Label l = new Label(Util.GS("iFolder Account")+":");
                        table.Attach(l, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
                        l.Xalign = 0.0F;

                        exportDomain = new Entry();
                        
			exportDomain.Sensitive = false;		
                                                                                                                                      
 			table.Attach(exportDomain, 1,2,0,1, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
                        l.MnemonicWidget = exportDomain;
                        
						// Row 2
						// Populate combo box with recovery agents
                        l = new Label(Util.GS("Recovery Agent")+":");
                        l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 1,2,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

                        recoveryAgent = new Entry();
                        recoveryAgent.Sensitive = false;
		        		table.Attach(recoveryAgent, 1,2,1,2, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
                        l.MnemonicWidget = recoveryAgent;
            
						
            
		  				// Row 3
                        l = new Label(Util.GS("Save the old data file in")+":");
                        l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 2,3,
                                AttachOptions.Fill, 0,0,0); // spacer

                        exportLocation = new Entry();
                        this.exportLocation.Changed += new EventHandler(OnExportPageFieldsChanged);

                        table.Attach(exportLocation, 1,2, 2,3,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = exportLocation;

                        exportBrowseButton = new Button(Util.GS("_Browse"));
                        table.Attach(exportBrowseButton, 2,3, 2,3, AttachOptions.Fill, 0,0,0);
                        exportBrowseButton.Sensitive = true;
                        exportBrowseButton.Clicked += new EventHandler(OnExportBrowseButtonClicked);

			return ExportKeyPage;
                }


		  private bool OnExportKeyPageSkipClicked(object o, EventArgs args)
                {
                       	KeyRecoveryDruid.Page = SelectionPage;

                       // BackButton.Label = Util.GS("gtk-go-back");
                        return true;
                }


                private void OnExportKeyPagePrepared(object o, Gnome.PreparedArgs args)
				{
					this.Title = Util.GS("Passphrase Recovery Wizard");
					//ExportUpdateUI();
					DisplayRAName();			
					exportLocation.GrabFocus();
					exportDomain.Text = this.selectedDomainName;		
			 	// Hack to make sure the "Forward" button has the right text.
                		     ForwardButton.Label = "gtk-go-forward";
			
				}

		


                private void DisplayRAName()
                {
                        string domainID = this.selectedDomain;
                        DomainController domController = DomainController.GetDomainController();
                        string raName = domController.GetRAName(domainID);
                        //email.Text = Util.GS("Information Not Available");
                        if( raName ==null || raName == "")
                        {
                                recoveryAgent.Text = Util.GS("Information Not Available");
                        }
                        else
                        {
                                //parsing the RAName by '='
                                char [] EmailParser = {'='};
                                string [] ParsedString = raName.Split(EmailParser);
                                //string emailID = "";
                                if (ParsedString.Length > 1)
                                {
                                        for(int x = 0; x < ParsedString.Length; x++)
                                        {
                                                // Iterate through the parsed string to again parse for '@' to get mail-id
                                                char [] FinalEmailParser = {'@'};
                                                string [] FinalParsedString = ParsedString[x].Split(FinalEmailParser);
                                                if(FinalParsedString.Length > 1)
                                                {
                                                        emailAddress = ParsedString[x];
                                                      //  email.Text = emailID;
                                                }
                                        }
                                }
                                recoveryAgent.Text = raName;
				          }
                }
				

		  	private void OnExportBrowseButtonClicked(object o, EventArgs e)
                	{
                        FileChooserDialog filedlg = new FileChooserDialog("", Util.GS("Select a file ...."), this, FileChooserAction.Save, Stock.Cancel, ResponseType.Cancel,Stock.Ok, ResponseType.Ok);
                        int res = filedlg.Run();
                        string str = filedlg.Filename;
                        filedlg.Hide();
                        filedlg.Destroy();
                        if( res == (int)ResponseType.Ok)
                        {
                                this.exportLocation.Text = str;
                        }
                        //Otherwise do nothing
                }
             
                private void OnExportPageFieldsChanged(object obj, EventArgs args)
                {
                        //bool enableOK = false;
                        if( this.exportLocation.Text.Length > 0 && this.exportLocation.Text.EndsWith(".xml"))
				 KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);

                	else
				 KeyRecoveryDruid.SetButtonsSensitive(true, false, true, true);
		
		}

		private bool OnExportKeyPageValidated(object obj, EventArgs args)
		{
			bool result = false;
				try
				{	
					this.simws.ExportiFoldersCryptoKeys(this.selectedDomain,this.exportLocation.Text );		
					result = true;	
				}
				catch(Exception ex)
				{
				iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,Util.GS("Export Encrypted Keys"), Util.GS(ex.Message), Util.GS(""));
				  dialog.Run();
				  dialog.Hide();	
				  dialog.Destroy();
				  dialog = null;
					result =false;
				}
			return result;

		}


		
		private Gnome.DruidPage CreateEmailPage()
		{
			
			EmailPage = new Gnome.DruidPageStandard("Obtain old data file",KeyRecoveryPixbuf,null);

			EmailPage.CancelClicked +=
                                new Gnome.CancelClickedHandler(OnCancelClicked);

           		 EmailPage.Prepared +=
            	                  new Gnome.PreparedHandler(OnEmailPagePrepared);
			
			


			//Create Email Page
				
			  Table table = new Table(4, 3, false);
                        EmailPage.VBox.PackStart(table, false, false, 0);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;

                        // Row 1
                        Label l = new Label(Util.GS("The old data file was successfully stored at the following path."));
                        table.Attach(l, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
                        l.Xalign = 0.0F;

                        //Row 2

                        exportedPath = new Entry();
                        //exportedPath.Text = this.exportLocation.Text;
                        exportedPath.Sensitive = false;

                        table.Attach(exportedPath, 0, 1, 1, 2,
                                AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);
                        //l.LineWrap = true;
                        //l.Xalign = 0.0F;


                        // Row 2
                        Label l2 = new Label(Util.GS("Send this file to the administrator to obtain the new data file. You must then use the new data file to reset your passphrase using the second option of the wizard."));
                        table.Attach(l2,0,1, 2,3,
                	                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l2.LineWrap = true;
                        l2.Xalign = 0.0F;

                       /* // Row 3
                        Label l3 = new Label("then use the decrypted key file to reset your passphrase. ");
                        table.Attach(l3, 0,1, 2,3,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l3.LineWrap = true;*/

                        l = new Label(Util.GS("Email address")+":");
                        l.Xalign = 0.0F;
                        table.Attach(l, 0, 1, 3, 4,
                                AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);

                       emailID = new Entry();
                       //emailID.Text = this.emailAddress;
                        emailID.Sensitive = false;
                        table.Attach(emailID, 1, 2, 3, 4, AttachOptions.Fill | AttachOptions.Expand, 0, 0, 0);
                        l.MnemonicWidget = emailID;
			        return EmailPage;	
	
		}

	
              private void OnEmailPagePrepared(object o, Gnome.PreparedArgs args)
              {
                        this.Title = Util.GS("Passphrase Recovery Wizard");
		//	FinishButton.Label = Util.GS("_Finish");
			this.exportedPath.Text = this.exportLocation.Text;
			KeyRecoveryDruid.CancelButton.Label = Util.GS("Finish");
                        KeyRecoveryDruid.SetButtonsSensitive(false,false , true, true);


                }
	

	
		

                public string OneTimePP
                {
                        get
                        {
                                if( this.oneTimePassphrase.Text != null)
                                        return this.oneTimePassphrase.Text;
                                else
                                        return null;
                        }
                }
                public string PassPhrase
                {
                        get
                        {
                                return this.importPageNewPassphrase.Text;
                        }
                }

		private Gnome.DruidPage CreateImportKeyPage()
		{
               		ImportKeyPage =
                                new modelKeyPage(Util.GS("Set new passphrase"),
                                        KeyRecoveryPixbuf,
                                        null);
			
    	     		ImportKeyPage.CancelClicked +=
	                                new Gnome.CancelClickedHandler(OnCancelClicked);

                    ImportKeyPage.Prepared +=
            	                   new Gnome.PreparedHandler(OnImportKeyPagePrepared);


					ImportKeyPage.ValidateClicked +=
									new KRValidateClickedHandler(OnImportKeyPageValidated);	
				
					ImportKeyPage.SkipClicked +=
                                new KRSkipClickedHandler(OnImportKeyPageSkipClicked);

		                ///
                        /// Content
                        ///
                        Table table = new Table(4, 3, false);
                        ImportKeyPage.VBox.PackStart(table, false, false, 0);
						table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;

                        //Row 0
                        Label l = new Label(Util.GS("iFolder Account")+":");
                        table.Attach(l, 0,1, 0,1, AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
                        l.Xalign = 0.0F;
                        
                        importDomain = new Entry();
                        importDomain.Sensitive = false;
			table.Attach(importDomain, 1,2,0,1, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);
                        l.MnemonicWidget = importDomain;

                      

                        // Row 1
                        l = new Label(Util.GS("Location of new data file")+":");
                        table.Attach(l, 0,1, 1,2,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l.LineWrap = true;
                        l.Xalign = 0.0F;
                        importLocation = new Entry();
                        this.importLocation.Changed += new EventHandler(OnImportFieldsChanged);
                        table.Attach(importLocation, 1,2, 1,2,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = importLocation;

                        importBrowseButton = new Button(Util.GS("_Browse"));
                        table.Attach(importBrowseButton, 2,3, 1,2, AttachOptions.Fill, 0,0,0);
                        importBrowseButton.Sensitive = true;
                        importBrowseButton.Clicked += new EventHandler(OnImportBrowseButtonClicked);

                        
			 isEncrypted = new CheckButton(Util.GS("Is the above file encrypted?"));
                        table.Attach(isEncrypted, 1,2, 2,3, AttachOptions.Fill | AttachOptions.Shrink, 0,0,0);
		//	isEncrypted.Toggled += new EventHandler(OnImportFieldsChanged);
			isEncrypted.Active = true;
			// Row 2
                        l = new Label(Util.GS("One Time Password")+":");
                        l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 3,4,
                                AttachOptions.Fill, 0,0,0);

                        oneTimePassphrase = new Entry();
                        oneTimePassphrase.Visibility = false;
                        oneTimePassphrase.Changed += new EventHandler(OnImportFieldsChanged);
                        table.Attach(oneTimePassphrase, 1,2, 3,4,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = oneTimePassphrase;

                        // Row 3
                        l = new Label(Util.GS("New Passphrase")+":");
                        l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 4,5,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        importPageNewPassphrase = new Entry();
                        importPageNewPassphrase.Visibility = false;
                        importPageNewPassphrase.Changed += new EventHandler(OnImportFieldsChanged);
                        table.Attach(importPageNewPassphrase, 1, 2, 4, 5,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = importPageNewPassphrase;

						 // Row 4
                        l = new Label("Confirm Passphrase"+":");
                        l.Xalign = 0.0F;
                        table.Attach(l, 0,1, 5,6,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        importPageConfirmPassphrase = new Entry();
                        importPageConfirmPassphrase.Visibility = false;
                        importPageConfirmPassphrase.Changed += new EventHandler(OnImportFieldsChanged);
                        table.Attach(importPageConfirmPassphrase, 1, 2, 5, 6,
                                AttachOptions.Expand | AttachOptions.Fill, 0,0,0);
                        l.MnemonicWidget = importPageConfirmPassphrase;
					return ImportKeyPage;
                }


		  private bool OnImportKeyPageSkipClicked(object o, EventArgs args)
                {
                        KeyRecoveryDruid.Page = SelectionPage;

                        BackButton.Label = Util.GS("gtk-go-back");
                        return false;
                }


                private void OnImportKeyPagePrepared(object o, Gnome.PreparedArgs args)
		        {
				this.importDomain.Text = this.selectedDomainName;

				KeyRecoveryDruid.SetButtonsSensitive(false,false , true, true);
		}
		

		  private void OnImportBrowseButtonClicked(object o, EventArgs e)
                {
                        FileChooserDialog filedlg = new FileChooserDialog("", Util.GS("Select a file ...."), this, FileChooserAction.Open, Stock.Cancel, ResponseType.Cancel,Stock.Ok, ResponseType.Ok);
                        int res = filedlg.Run();
                        string str = filedlg.Filename;
                        filedlg.Hide();
                        filedlg.Destroy();
                        if( res == (int)ResponseType.Ok)
                        {
                                this.importLocation.Text = str;
                        }
                        //Otherwise do nothing
                }



		
		   private void OnImportFieldsChanged(object obj, EventArgs args)
                {
                        //bool enableOK = false;
                    if (this.importPageNewPassphrase.Text.Length > 0 && this.importPageConfirmPassphrase.Text.Length > 0 && this.importPageNewPassphrase.Text == this.importPageConfirmPassphrase.Text)
                           if( this.importLocation.Text.Length > 0)
			{	
				 KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);
			}
                        else
                                 KeyRecoveryDruid.SetButtonsSensitive(true, false, true, true);

                }
		
		private bool OnImportKeyPageValidated (object o, EventArgs args)
		{	
			try{
			 bool rememberOption = this.simws.GetRememberOption(this.selectedDomain);
			this.simws.ImportiFoldersCryptoKeys( this.selectedDomain, importPageNewPassphrase.Text, this.OneTimePP, importLocation.Text);
			 //clear the values
                                //        this.simws.StorePassPhrase(this.selectedDomain, "", CredentialType.None, false);
							
			 //  simws.StorePassPhrase(this.selectedDomain, importPageNewPassphrase.Text, CredentialType.Basic, rememberOption);
			}
			catch(Exception ex)
			{
		                                 iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,Util.GS("Passphrase reset failed"), Util.GS("Please try again"), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;
                //                        result =false;
			
					
			return false;
			}

			KeyRecoveryDruid.Page = FinishPage;
			return true;

		}
		
		
		 
                        
		private Gnome.DruidPage CreateSingleWizPage()

		{	
			SingleWizPage = new modelKeyPage(Util.GS("Set new passphrase"),
                                        KeyRecoveryPixbuf,
                                        null);


			SingleWizPage.CancelClicked +=
                                new Gnome.CancelClickedHandler(OnCancelClicked);

			SingleWizPage.ValidateClicked +=
				new KRValidateClickedHandler(OnSingleWizPageValidated);
			
			 SingleWizPage.SkipClicked +=
                               new KRSkipClickedHandler(OnSingleWizSkipClicked);
			
			 SingleWizPage.Prepared +=
                                   new Gnome.PreparedHandler(OnSingleWizPagePrepared);


			 Table table = new Table(4, 3, false);
              SingleWizPage.VBox.PackStart(table, false, false, 0);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;

                         // Row 1
                        Label lbl = new Label(Util.GS("iFolder Account"+":"));
                        table.Attach(lbl, 0,1, 0,1,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lbl.LineWrap = true;
                        lbl.Xalign = 0.0F;


                        singleWizDomain = new Entry();
                        // read domains from domain controller...

                        lbl.MnemonicWidget = singleWizDomain;
                        singleWizDomain.Sensitive = false;
                        table.Attach(singleWizDomain, 1,2,0,1, AttachOptions.Fill|AttachOptions.Expand, 0,0,0);


                        

                        //Row 2

                        Label lb2 = new Label(Util.GS("Location of secret file")+ ":");
                        table.Attach(lb2, 0,1, 1,2,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb2.LineWrap = true;
                        lb2.Xalign = 0.0F;

                        p12FilePath  = new Entry();
                         p12FilePath.Visibility = true;
                        table.Attach(p12FilePath, 1,2, 1,2,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb2.MnemonicWidget = p12FilePath;
                        p12FilePath.Changed += new EventHandler(UpdateSensitivity);
			
			            singleWizBrowseButton = new Button(Util.GS("_Browse"));
                        table.Attach(singleWizBrowseButton, 2,3, 1,2, AttachOptions.Fill, 0,0,0);
                        singleWizBrowseButton.Sensitive = true;
                        singleWizBrowseButton.Clicked += new EventHandler(OnSingleWizBrowseButtonClicked);




                        //Row 3


                         Label lb3 = new Label(Util.GS("Secret Password")+":");
                        table.Attach(lb3, 0,1, 2,3,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb3.LineWrap = true;
                        lb3.Xalign = 0.0F;

                        domainPasswd = new Entry();
                        domainPasswd.Visibility = false;
                        table.Attach(domainPasswd, 1,2, 2,3,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);

			            lb3.MnemonicWidget = domainPasswd;
                        domainPasswd.Changed += new EventHandler(UpdateSensitivity);


                        //Row 4

                         Label lb4 = new Label(Util.GS("New Passphrase")+":");
                        table.Attach(lb4, 0,1, 3,4,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb4.LineWrap = true;
                        lb4.Xalign = 0.0F;

                       singleWizNewPassphrase = new Entry();
                        singleWizNewPassphrase.Visibility = false;
                        table.Attach(singleWizNewPassphrase, 1,2, 3,4,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb4.MnemonicWidget = singleWizNewPassphrase;
                        singleWizNewPassphrase.Changed += new EventHandler(UpdateSensitivity);


                        //Row 5
                        //encryResult = new CheckButton("Encrypt Resulting Key ? ");
                        //table.Attach(encryResult, 1,2,5,6, AttachOptions.Expand|AttachOptions.Fill, 0,0,0);

                        //Row 6

                        Label lb5 = new Label(Util.GS("Confirm Passphrase") + ":");
                        table.Attach(lb5, 0,1, 4,5,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb5.LineWrap = true;
                        lb5.Xalign = 0.0F;

                        singleWizConfirmPassphrase = new Entry();
                        singleWizConfirmPassphrase.Visibility = false;
                        table.Attach(singleWizConfirmPassphrase, 1,2, 4,5,
                                AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        lb5.MnemonicWidget = singleWizConfirmPassphrase;
                       singleWizConfirmPassphrase.Changed += new EventHandler(UpdateSensitivity);

			return SingleWizPage;
		}		
		


		 private void OnSingleWizPagePrepared(object o, Gnome.PreparedArgs args)
                        {
				this.singleWizDomain.Text = this.selectedDomainName;
			}
		  private bool OnSingleWizSkipClicked(object o, EventArgs args)
                {
                       KeyRecoveryDruid.Page = SelectionPage;

                        BackButton.Label = Util.GS("gtk-go-back");
                        return false;
                }
	

		private void UpdateSensitivity(object o,EventArgs args)
		{
			
			if (
                		this.p12FilePath.Text.Length > 0 && 
                this.domainPasswd.Text.Length > 0 &&   this.newPassphrase.Text.Length > 0 &&
               this.confirmPassphrase.Text.Length > 0
                )
			  KeyRecoveryDruid.SetButtonsSensitive(true, true, true, true);

                        else
                          KeyRecoveryDruid.SetButtonsSensitive(true, false, true, true);


		}

	
		
                private void OnSingleWizBrowseButtonClicked(object o, EventArgs e)
                {
                        FileChooserDialog filedlg = new FileChooserDialog("", Util.GS("Select a file..."), this, FileChooserAction.Open, Stock.Cancel, ResponseType.Cancel,Stock.Ok, ResponseType.Ok);
                        int res = filedlg.Run();
                        string str = filedlg.Filename;
                        filedlg.Hide();
                        filedlg.Destroy();
                        if( res == (int)ResponseType.Ok)
                        {
                                this.p12FilePath.Text = str;
                        }
                        //Otherwise do nothing
                }



		private void singleWizGetDefaultPath()
		{
		
			 string str = Mono.Unix.UnixEnvironment.EffectiveUser.HomeDirectory;
                         singleWizExportPath = str+ "/"+ this.singleWizDomain+"_Encry.xml";
			singleWizImportPath = str + "/"+ this.singleWizDomain+"_Decry.xml";

		}
	

		 private bool  OnSingleWizPageValidated(object o, EventArgs args)
                {

       		     Debug.PrintLine("Submit clicked");
			bool result =false;
			singleWizGetDefaultPath();	
			   
			//export first to default path
			try
                                {
					this.simws.ExportiFoldersCryptoKeys(this.selectedDomain,singleWizExportPath);
                                        
                                }
                                catch(Exception ex)
                                {
                                  iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,"Export Encrypted Keys in single wiz", Util.GS(ex.Message), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;
                                  result = false;
                                }

			//Convert into usable form after export

			try{
			InitializeKey();
			ProcessInputKeyFile();
			}
			catch(Exception ex1)
			{
				//	Debug.WriteLine("Error in key recovery process");
				                                        iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,(Util.GS("Resetting Passphrase failed")), Util.GS(ex1.Message), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;

				
				result = false;
			
			}
				
			
			//Now import the converted file

			try
			{
				bool rememberOption = this.simws.GetRememberOption(this.selectedDomain);
                        	this.simws.ImportiFoldersCryptoKeys( this.selectedDomain,newPassphrase.Text , null, singleWizImportPath);
                         	//clear the values
                                        this.simws.StorePassPhrase(this.selectedDomain, "", CredentialType.None, false);

                           	simws.StorePassPhrase(this.selectedDomain, singleWizNewPassphrase.Text, CredentialType.Basic, rememberOption);
			}	
			catch(Exception ex)
			 {
                                        iFolderMsgDialog dialog = new iFolderMsgDialog(null,iFolderMsgDialog.DialogType.Error,iFolderMsgDialog.ButtonSet.None,(Util.GS("Resetting Passphrase failed")), Util.GS(ex.Message), Util.GS(""));
                                  dialog.Run();
                                  dialog.Hide();
                                  dialog.Destroy();
                                  dialog = null;      
                                   result = false;
			}
			if(result)
			
			{
				 if (File.Exists(singleWizExportPath))
        				        File.Delete(singleWizExportPath);
				 if (File.Exists(singleWizImportPath))
				                File.Delete(singleWizImportPath);
			
				KeyRecoveryDruid.Page = FinishPage;	
			}
			return result;
				

		}

		
		  // Initialize an rsaKey member variable with the specified RSA key.
                private void InitializeKey()
                {
                        try{
                        
			PKCS12 pkcs12 = PKCS12.LoadFromFile(p12FilePath.Text, domainPasswd.Text);
                        if(pkcs12 != null)
                        
{
                                foreach(RSA rsa in pkcs12.Keys)
                                {
                                        rsadec = rsa as RSACryptoServiceProvider;
                                        break;
                                }
                        }
                        }
                        catch (System.Security.Cryptography.CryptographicException e){
                        }
                }

	 private void ProcessInputKeyFile()
        {
            string strKey = string.Format("//{0}/{1}", CollectionIDTag, KeyTag);
            string strID = string.Format("//{0}/{1}", CollectionIDTag, iFolderIDTag);
            string decKey;
            byte[] decKeyByteArray;
            string inKeyPath = singleWizExportPath;
            string outKeyPath = singleWizImportPath;
            XmlDocument encFile = new XmlDocument();
            encFile.Load(inKeyPath);
            XmlNodeList keyNodeList, idNodeList;

            XmlElement root = encFile.DocumentElement;

            keyNodeList = root.SelectNodes(strKey);
            idNodeList = root.SelectNodes(strID);

            XmlDocument document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
            document.InsertBefore(xmlDeclaration, document.DocumentElement);
            XmlElement title = document.CreateElement(titleTag);
            document.AppendChild(title);
            int i = 0;
            foreach (XmlNode idNode in idNodeList)
            {
                //Console.WriteLine(idNode.InnerText);
                XmlNode newNode = document.CreateNode("element", CollectionIDTag, "");
                newNode.InnerText = "";
                document.DocumentElement.AppendChild(newNode);
                XmlNode innerNode = document.CreateNode("element", iFolderIDTag, "");
                innerNode.InnerText = idNode.InnerText;
                newNode.AppendChild(innerNode);
                {
                    XmlNode keyNode = keyNodeList[i++];
                    decKey = keyNode.InnerText;
                    decKeyByteArray = Convert.FromBase64String(decKey);
                    XmlNode newElem2 = document.CreateNode("element", KeyTag, "");
                   // if (OTP.Value == true)
                     //   newElem2.InnerText = DecodeMessage(decKeyByteArray, OTPass.Value);
                   // else
                        newElem2.InnerText = DecodeMessage(decKeyByteArray);
                    newNode.AppendChild(newElem2);
                }
            }
            if (File.Exists(outKeyPath))
                File.Delete(outKeyPath);
            document.Save(outKeyPath);
        }
	
	 private string DecodeMessage(byte[] encmess)
        {
            string mess = null;
            try
            {
                mess = Convert.ToBase64String(rsadec.Decrypt(encmess, false));
            }
            catch (CryptographicException cExp)
            {
		//                Debug.WriteLine("Crpto Error {0}", cExp.Message);
            }
            return mess;
        }
	

	        ///
                /// Summary Page
                ///
                private Gnome.DruidPage CreateFinishPage()
                {
			FinishPage = new Gnome.DruidPageStandard(Util.GS("Passphrase reset successfully"),KeyRecoveryPixbuf,null);
                        //FinishPage.ValidateClicked +=
        	                  //      new KRValidateClickedHandler(OnFinishPageValidated);
			FinishPage.CancelClicked +=
                                new Gnome.CancelClickedHandler(OnCancelClicked);
	
                        //FinishPage.Can
			FinishPage.Prepared +=
                                new Gnome.PreparedHandler(OnFinishPagePrepared);

			Table table = new Table(4, 3, false);
		        FinishPage.VBox.PackStart(table, false, false, 0);
                        table.ColumnSpacing = 6;
                        table.RowSpacing = 6;
                        table.BorderWidth = 12;



                        //Row 1
                        Label l1 = new Label(Util.GS("Congratulations!! You have successfully reset the passphrase.Now,you can use the new passphrase to access your data."));
                        table.Attach(l1, 0,1, 0,1,AttachOptions.Fill | AttachOptions.Expand, 0,0,0);
                        l1.LineWrap = true;
                        l1.Xalign = 0.0F;

			 return FinishPage;
                }


		    private bool  OnFinishPageValidated(object o,EventArgs  args)

                {	
			
	               		try
			{ 
				CloseDialog();
				/*if(!ShowNextWizard())
            			{
                			Util.ShowiFolderWindow();
					
            			}*/
					return true;			
			}		

				catch(Exception e){return false;}
			
			
                }

		  private void OnFinishPagePrepared(object o, Gnome.PreparedArgs args)
                {
                        this.Title = Util.GS("Passphrase Recovery Wizard");

                        KeyRecoveryDruid.SetButtonsSensitive(false, false, true, true);
			KeyRecoveryDruid.CancelButton.Label = Util.GS("Finish");
                }

		 public bool ShowNextWizard()
        {
            try
            {
                KeyRecoveryWizard krw = null;
                krw = (KeyRecoveryWizard)KRWizardStack.Pop();
                if(null != krw)
                    krw.Maximize();
            }
            catch(Exception e)
            {
                //iFolderWindow.log.Info("Excepion in Pop of ShowNextWizard. {0} {1}", e.GetType(), e.Message);
                return false;
            }
            return true;
        }

                public void CloseDialog()
                {
                        this.Hide();
                        this.Destroy();
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
                       KeyRecoveryDruid.Forall(SetUpButtonsCallback);
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
                                                else if (button.Label == "gtk-go-cancel")
                                                        FinishButton = button;
                                                else if (button.Label == "gtk-go-back")

                                                        BackButton = button;
						
                                        }
                                }
                        }
                }

	


     }

	public class modelKeyPage : Gnome.DruidPageStandard
        {
                public event KRValidateClickedHandler ValidateClicked;
		                public event KRSkipClickedHandler SkipClicked;

                public modelKeyPage(string title, Gdk.Pixbuf logo, Gdk.Pixbuf topwatermark)
                        : base (title, logo, topwatermark)
                {
                }

                protected override bool OnNextClicked(Widget druid)
                {
                        if (ValidateClicked != null)
                        {
                                if (!ValidateClicked(this, EventArgs.Empty))
                                        return true;    // Prevent the default event handler (from advancing to the next druid page
                        }


                        return false;   // Allow the default event handler (to advance to the next druid page)
                }

                //Skip.
                protected override bool OnBackClicked(Widget druid)
                {
                        if (SkipClicked != null)
                        {
                                if (!SkipClicked(this, EventArgs.Empty))
                                        return true;    // Prevent the default event handler (from advancing to the next druid page
                        }

                        return false;   // Allow the default event handler (to advance to the next druid page)
                }

        }

        public delegate bool KRValidateClickedHandler(object o, EventArgs args);
	   public delegate bool KRSkipClickedHandler(object o, EventArgs args);
       

	
	 public class KRWizardStack
    {
        static Stack k = new Stack(32);

        public static void Push( KeyRecoveryWizard a )
        {
            k.Push(a);
        }

        public static KeyRecoveryWizard Pop()
        {
            return (KeyRecoveryWizard)k.Pop();
        }
    }



    
	
	
  }




