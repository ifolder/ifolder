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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Novell.iFolder.Web;
using Novell.FormsTrayApp;
using Novell.iFolderCom;
using Simias.Client;
using Simias.Client.Authentication;
using Simias.Client.Event;
using TrayApp.Properties;

namespace Novell.Wizard
{
    public partial class ImportKeyPage : Novell.Wizard.InteriorPageTemplate
    {
            
        private iFolderWebService ifWebService = null;
        private SimiasWebService simiasWebService = null;
        private KeyRecoveryWizard wizard;
        private DomainItem selectedDomain = null;
        
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
       

        /// <summary>
        /// Create the import key page object
        /// </summary>
        /// <param name="ifws">iFolderWebService</param>
        /// <param name="simws">simiasWebService</param>
        public ImportKeyPage(iFolderWebService ifws,SimiasWebService simws)
        {
            InitializeComponent();
            this.ifWebService = ifws;
            this.simiasWebService = simws;
            
         }

         /// <summary>
         /// Activate Page and perform initialisation operations.
         /// </summary>
         /// <param name="previousIndex">previous Page</param>

        internal override void ActivatePage(int previousIndex)
        {
            base.ActivatePage(previousIndex);
            wizard = (KeyRecoveryWizard)this.Parent;

            selectedDomain = wizard.DomainSelectionPage.SelectedDomain;
            this.accountBox.Text = selectedDomain.Name;
            
            this.oneTimePassphrase.Enabled = false;
           ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
           this.LocationEntry.Focus();

         
        }
        
        /// <summary>
        /// Deactivate current page
        /// </summary>
        /// <returns></returns>
        internal override int DeactivatePage()
        {
          
            return base.DeactivatePage();
        }

        /// <summary>
        /// Validate page
        /// </summary>
        /// <param name="currentIndex">current index</param>
        internal override int ValidatePage(int currentIndex)
        {
            bool result = true;
            result = Import_func();

            if (result == false)
            {
                return currentIndex;
            }
            return base.ValidatePage(currentIndex);
        }

        /// <summary>
        /// Event handler for browse button clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "XML Files|*.xml";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                this.LocationEntry.Text = fileDlg.FileName;
            }
        }

        /// <summary>
        /// Enable/Disable buttons
        /// </summary>
        private void UpdateSensitivity()
        {
            if( this.passphrase.Text.Length >0 && this.reTypePassphrase.Text.Length > 0 && this.reTypePassphrase.Text == this.passphrase.Text && this.LocationEntry.Text.Length > 0 && this.LocationEntry.Text.EndsWith(".xml") )
            {
                if ( this.isEncrypted.Checked == true)
                {
                    if( this.oneTimePassphrase.Text.Length >0)

                    ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Next | KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;

                    else

                    ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
                }
                else
                {
                    ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel|KeyRecoveryWizardButtons.Next;
                }
             }
            else
                 ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;

        }

        /// <summary>
        /// Event handler for location entry text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationEntry_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        /// <summary>
        /// Event handler for one time passphrase text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void oneTimePassphrase_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        /// <summary>
        /// Event handler for passphrase text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void passphrase_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        /// <summary>
        /// Event handler for retype passphrase text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reTypePassphrase_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

      
       
        /// <summary>
        /// Core functionality of calling the simias web service import function
        /// </summary>
        /// <returns></returns>
        private bool Import_func()
        {
            try
            {
                string onetimepp;
                if (this.oneTimePassphrase != null)
                    onetimepp = this.oneTimePassphrase.Text;
                else
                    onetimepp = null;
                this.simiasWebService.ImportiFoldersCryptoKeys(selectedDomain.ID, this.passphrase.Text, onetimepp, this.LocationEntry.Text);

                bool rememberOption = this.simiasWebService.GetRememberOption(selectedDomain.ID);
                //clear the values
                this.simiasWebService.StorePassPhrase(selectedDomain.ID, "", CredentialType.None, false);
                //set the values
                this.simiasWebService.StorePassPhrase(selectedDomain.ID, this.passphrase.Text, CredentialType.Basic, rememberOption);

              
                
            }
            catch (Exception ex)
            {
                MyMessageBox mmb = new MyMessageBox(TrayApp.Properties.Resources.importErrorMesg, TrayApp.Properties.Resources.wizardText, null,MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);//"Error importing the keys. Try again")
                mmb.ShowDialog();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Event handler for check box changed changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isEncrypted_CheckedChanged(object sender, EventArgs e)
        {
            if (isEncrypted.Checked == true)
                this.oneTimePassphrase.Enabled = true;
            else
                this.oneTimePassphrase.Enabled = false;
            UpdateSensitivity();
        }

      
    }
}
