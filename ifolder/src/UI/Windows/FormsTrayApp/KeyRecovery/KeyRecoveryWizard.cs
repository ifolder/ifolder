
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
*                 $Modified by: 
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Net;
using TrayApp.Properties;

using Novell.iFolder.Web;
using Novell.FormsTrayApp;
using Novell.iFolderCom;
using Simias.Client;
using Simias.Client.Authentication;
using Simias.Client.Event;


namespace Novell.Wizard
{
    /// <summary>
    /// Enumeration used to tell which wizard buttons are active.
    /// </summary>
    [Flags]
    public enum KeyRecoveryWizardButtons
    {
        /// <summary>
        /// None of the buttons are enabled.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Back button is enabled.
        /// </summary>
        Back = 1,

        /// <summary>
        /// The Next button is enabled.
        /// </summary>
        Next = 2,

        /// <summary>
        /// The Cancel button is enabled.
        /// </summary>
        Cancel = 4
    }

    /// <summary>
    /// Summary description for KeyRecoveryWizard.
    /// </summary>
    public partial class KeyRecoveryWizard : System.Windows.Forms.Form
    {
        #region Class Members

        public System.Windows.Forms.Button cancel;
        public System.Windows.Forms.Button next;
        public System.Windows.Forms.Button back;
        private System.Windows.Forms.Button btnHelp;
        internal System.Windows.Forms.GroupBox groupBox1;

        private DomainSelectionPage domainSelectionPage;
        private EnterPassphrasePage enterPassphrasePage;
        private InfoPage infoPage;
        private SelectionPage selectionPage;
        private SinglePageWizard singleWizPage;
        private ImportKeyPage importKeyPage;
        private FinalPage finalPage;
        private ExportKeyPage exportKeyPage;
        private EmailPage emailPage;
        
        private BaseWizardPage[] pages;

        internal const int maxPages = 9;
        protected int currentIndex = 0;

        private SimiasWebService simiasWebService;
        private iFolderWebService ifWebService;
        private Manager simiasManager;
       

       
        /// <summary>
        /// Required designer variable.
        /// 
        /// </summary>
        private System.ComponentModel.Container components = null;

        #endregion

       /// <summary>
       /// Creates a KeyRecoveryWizard object with the member pages
       /// </summary>
       /// <param name="ifWebService">iFolderwebservice</param>
       /// <param name="simiasWebService">simiasWebService</param>
        public KeyRecoveryWizard(iFolderWebService ifWebService, SimiasWebService simiasWebService,Manager simiasManager)
        {
           
            this.simiasWebService = simiasWebService;
            this.ifWebService = ifWebService;
            this.simiasManager = simiasManager;
          
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Initialize the wizard pages 

            this.domainSelectionPage = new DomainSelectionPage(this.ifWebService,this.simiasWebService);
            
            this.enterPassphrasePage = new EnterPassphrasePage(this.ifWebService,this.simiasWebService,this.simiasManager);
          
            
            this.infoPage = new InfoPage();
            this.selectionPage = new SelectionPage();
         

            //selection 1 = have all parameters to do things 
            this.singleWizPage = new SinglePageWizard(this.ifWebService, this.simiasWebService);

            //selection 2 = have only decrypted file sent by admin
            this.importKeyPage = new ImportKeyPage(this.ifWebService, this.simiasWebService);

            this.finalPage = new FinalPage();//successfully reset passphrase

            //selection 3= have nothing
            this.exportKeyPage = new ExportKeyPage(this.ifWebService, this.simiasWebService);

            this.emailPage = new EmailPage();

            //domainSelectionPage

            this.domainSelectionPage.HeaderTitle = TrayApp.Properties.Resources.domainSelectionPageTitle;
            this.domainSelectionPage.Location = new System.Drawing.Point(0, 0);
            this.domainSelectionPage.Name = TrayApp.Properties.Resources.domainSelectionPageName;
            this.domainSelectionPage.Size = new System.Drawing.Size(496, 304);
            this.domainSelectionPage.TabIndex = 1;

            //enterpassphrasepage

            this.enterPassphrasePage.HeaderTitle = TrayApp.Properties.Resources.setNewPassphraseTitle;
            this.enterPassphrasePage.Location = new System.Drawing.Point(0, 0);
            this.enterPassphrasePage.Name = TrayApp.Properties.Resources.enterPassphrasePageName;
            this.enterPassphrasePage.Size = new System.Drawing.Size(496, 304);
            this.enterPassphrasePage.TabIndex = 1;
            

      

            //infoPage
            this.infoPage.HeaderTitle = TrayApp.Properties.Resources.infoPageTitle; // "Welcome to the Passphrase Recovery Wizard";
            this.infoPage.Location = new System.Drawing.Point(0, 0);
            this.infoPage.Name = TrayApp.Properties.Resources.infoPageName;
            this.infoPage.Size = new System.Drawing.Size(496, 304);
            this.infoPage.TabIndex = 1;

            //selectionPage
            this.selectionPage.HeaderTitle = TrayApp.Properties.Resources.selectionPageTitle; // "Select Passphrase Recovery step";
            this.selectionPage.Location = new System.Drawing.Point(0, 0);
            this.selectionPage.Name = TrayApp.Properties.Resources.selectionPageName;// "keyRecoverySelectionPage";
            this.selectionPage.Size = new System.Drawing.Size(496, 304);
            this.selectionPage.TabIndex = 1;
                        
            //singlewizpage
            this.singleWizPage.HeaderTitle = TrayApp.Properties.Resources.setNewPassphraseTitle;// "Set New Passphrase";
            this.singleWizPage.Location = new System.Drawing.Point(0, 0);
            this.singleWizPage.Name = TrayApp.Properties.Resources.singleWizPageName; // "keyRecoverySingleWizPage";
            this.singleWizPage.Size = new System.Drawing.Size(496, 304);
            this.singleWizPage.TabIndex = 1;


            //importKeyPage
            this.importKeyPage.HeaderTitle = TrayApp.Properties.Resources.setNewPassphraseTitle;//"Resetting Passphrase Using Decrypted Key File";
            this.importKeyPage.Location = new System.Drawing.Point(0, 0);
            this.importKeyPage.Name = TrayApp.Properties.Resources.importPageName;
            this.importKeyPage.Size = new System.Drawing.Size(496, 304);
            this.importKeyPage.TabIndex = 1;

            //finalPage
            this.finalPage.HeaderTitle = TrayApp.Properties.Resources.finalPageTitle;// "Passphrase is reset successfully.";
            this.finalPage.Location = new System.Drawing.Point(0, 0);
            this.finalPage.Name = TrayApp.Properties.Resources.finalPageName;// "finalPage";
            this.finalPage.Size = new System.Drawing.Size(496, 304);
            this.finalPage.TabIndex = 1;

            // exportKeyPage
            this.exportKeyPage.HeaderTitle = TrayApp.Properties.Resources.exportPageTitle;// "Export Key File from the Server";
            this.exportKeyPage.Location = new System.Drawing.Point(0, 0);
            this.exportKeyPage.Name = TrayApp.Properties.Resources.exportPageName; //"keyRecoveryExportKeyPage";
            this.exportKeyPage.Size = new System.Drawing.Size(496, 304);
            this.exportKeyPage.TabIndex = 1;
            // 
            // emailPage
            this.emailPage.HeaderTitle = TrayApp.Properties.Resources.emailPageTitle; //"Export of Key file Successful";
            this.emailPage.Location = new System.Drawing.Point(0, 0);
            this.emailPage.Name = TrayApp.Properties.Resources.emailPageName;//"keyrecoverySuccessPage";
            this.emailPage.Size = new System.Drawing.Size(496, 304);
            this.emailPage.TabIndex = 1;

            //Add member pages to wizard
            this.Controls.Add(this.domainSelectionPage);
            this.Controls.Add(this.enterPassphrasePage);
            this.Controls.Add(this.infoPage);
            this.Controls.Add(this.selectionPage);
            this.Controls.Add(this.singleWizPage);
            this.Controls.Add(this.importKeyPage);
            this.Controls.Add(this.finalPage);
            this.Controls.Add(this.exportKeyPage);
            this.Controls.Add(this.emailPage);
            this.BackColor = System.Drawing.Color.Gainsboro;


            // Load the application icon.
            try
            {
                this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
            }
            catch { } // Ignore

            // Put the wizard pages in order.
            pages = new BaseWizardPage[maxPages];
            pages[0] = this.domainSelectionPage;
            pages[1] = this.enterPassphrasePage;
            pages[2] = this.infoPage;
            pages[3] = this.selectionPage;
            pages[4] = this.singleWizPage;
            pages[5] = this.importKeyPage;
            pages[6] = this.finalPage;
            pages[7] = this.exportKeyPage;
            pages[8] = this.emailPage;

            try
            {
               
                Image image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
                this.domainSelectionPage.Thumbnail = image;
                this.enterPassphrasePage.Thumbnail = image;
                this.infoPage.Thumbnail = image;
                this.selectionPage.Thumbnail = image;
                this.singleWizPage.Thumbnail = image;
                this.importKeyPage.Thumbnail = image;
                this.finalPage.Thumbnail = image;
                this.exportKeyPage.Thumbnail = image;
                this.emailPage.Thumbnail = image;
                
  
            }
            catch { } // Ignore. Unable to load watermark. No functionality issues

            foreach (BaseWizardPage page in pages)
            {
                page.Hide();
            }

            // Activate the first wizard page.
            //pages[0].ActivatePage(0);
            bool result = GetLoggedInDomains();
            if(result == true)
                domainSelectionPage.ActivatePage(0);
        }

        /// <summary>
        /// Gets list of currently logged in domains
        /// </summary>
        /// <returns></returns>
    

        public bool GetLoggedInDomains()
        {
            bool result = false;
            try
            {
                DomainInformation[] domains;
                domains = this.simiasWebService.GetDomains(true);
                int domainCount = 0;
                foreach (DomainInformation di in domains)
                {
                    if (di.Authenticated)
                    {
                        domainCount++;
                       
                    }
                }

                if (domainCount >= 1)
                    result = true;

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        
        /// <summary>
        /// Event handler for back button clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void back_Click(object sender, System.EventArgs e)
        {
            this.WizardButtons = KeyRecoveryWizardButtons.Next | KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
            int previousIndex = this.pages[currentIndex].DeactivatePage();
            this.pages[previousIndex].ActivatePage(0);

            currentIndex = previousIndex;

        }

        /// <summary>
        /// Event handler for next button clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void next_Click(object sender, System.EventArgs e)
        {
            int nextIndex = this.pages[currentIndex].ValidatePage(currentIndex);
            
            if (nextIndex == -999)
            {
                this.Close();
                this.Dispose();
                return;
            }
            if (nextIndex != currentIndex)
            {
                this.pages[currentIndex].DeactivatePage();
                this.pages[nextIndex].ActivatePage(currentIndex);

                currentIndex = nextIndex;

              }

            //if email page or final page ; change cancel to finish
            if (this.currentIndex == this.MaxPages - 1 || this.currentIndex == this.MaxPages - 3)
            {
                this.cancel.Text = TrayApp.Properties.Resources.finishText;
                next.DialogResult = DialogResult.OK;
                WizardButtons = KeyRecoveryWizardButtons.Cancel;
            }

            
        }


        #region Properties

        public int MaxPages
        {
            get { return maxPages; }
        }

        /// <summary>
        /// Return a export key page object
        /// </summary>
        public ExportKeyPage ExportKeyPage
        {
            get { return exportKeyPage; }
        }

        public DomainSelectionPage DomainSelectionPage
        {
            get { return domainSelectionPage; }
        }

        
        /// <summary>
        /// Gets or sets flags determining which wizard buttons are enabled or disabled.
        /// </summary>
        public KeyRecoveryWizardButtons WizardButtons
        {
            get
            {
                KeyRecoveryWizardButtons wizardButtons = KeyRecoveryWizardButtons.None;
                wizardButtons |= cancel.Enabled ? KeyRecoveryWizardButtons.Cancel : KeyRecoveryWizardButtons.None;
                wizardButtons |= next.Enabled ? KeyRecoveryWizardButtons.Next : KeyRecoveryWizardButtons.None;
                wizardButtons |= back.Enabled ? KeyRecoveryWizardButtons.Back : KeyRecoveryWizardButtons.None;
                return wizardButtons;
            }
            set
            {
                KeyRecoveryWizardButtons wizardButtons = value;
                cancel.Enabled = ((wizardButtons & KeyRecoveryWizardButtons.Cancel) == KeyRecoveryWizardButtons.Cancel);
                next.Enabled = ((wizardButtons & KeyRecoveryWizardButtons.Next) == KeyRecoveryWizardButtons.Next);
                back.Enabled = ((wizardButtons & KeyRecoveryWizardButtons.Back) == KeyRecoveryWizardButtons.Back);
            }
        }

        #endregion

       /// <summary>
        /// Event handler for cancel clicked event
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
      
        private void cancel_Click(object sender, EventArgs e)
        {
            
            if (this.currentIndex == this.MaxPages - 1)
            {
                this.cancel.Text = TrayApp.Properties.Resources.finishText;
                WizardButtons = KeyRecoveryWizardButtons.Cancel;
            }
            this.Close();
            this.Dispose();
           
        }

        /// <summary>
        /// Event handler for location help button clicked event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
            private void help_Click(object o, EventArgs args)
         {
             string helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"managingpassphrse.html");
             new iFolderComponent().ShowHelp(Application.StartupPath, helpFile);
         }
     
    }
}
