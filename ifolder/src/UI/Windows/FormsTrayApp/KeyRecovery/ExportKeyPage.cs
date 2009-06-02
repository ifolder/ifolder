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
using System.Xml;
using Novell.iFolderCom;
using Novell.iFolder.Web;
using System.IO;

namespace Novell.Wizard
{
    public partial class ExportKeyPage : Novell.Wizard.InteriorPageTemplate
    {
        private SimiasWebService simiasWebService = null;
        private iFolderWebService ifWebService = null;
        private DomainItem selectedDomain = null;
        private KeyRecoveryWizard wizard;
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox filePath;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Label recoveryAgentLabel;
        private Label label1;
        private System.Windows.Forms.TextBox recoveryAgent;
        private string domainID;
        private string emailAddress;

        
        /// <summary>
        /// Create Export page object
        /// </summary>
        /// <param name="ifws">ifolderWebservice</param>
        /// <param name="simws">simiasWebservice</param>
     
        public ExportKeyPage(iFolderWebService ifws, SimiasWebService simws)
        {
            InitializeComponent();
            this.simiasWebService = simws;
            this.ifWebService = ifws;
           
        }
        /// <summary>
        /// Obtain default to store the exported key file
        /// </summary>
        /// <param name="domainName">Domain selected by the user</param>
        /// <returns></returns>
         private string GetDefaultPath(string domainName)
        {
            string appdata = System.Environment.GetEnvironmentVariable("APPDATA");
            int i = appdata.LastIndexOf("\\");
            appdata = appdata.Substring(0, i);
            appdata = appdata + "\\" + domainName + ".xml";
            return appdata;
        }

        #region Properties

        public string EmailAddress
        {
            get { return emailAddress; }
            set { emailAddress = value; }
        }

        public string ExportPath
        {
            get { return filePath.Text; }
            set { filePath.Text = value; }
        }

        #endregion

        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous Page</param>
        internal override void ActivatePage(int previousIndex)
        {
            base.ActivatePage(previousIndex);

            // Enable/disable the buttons.
            wizard = (KeyRecoveryWizard)this.Parent;

            selectedDomain = wizard.DomainSelectionPage.SelectedDomain;
            this.accountBox.Text = selectedDomain.Name;
            DisplayRAName(selectedDomain);
           
            UpdateSensitivity();
           //domainComboBox.Focus();
          
        }

        /// <summary>
        /// Deactivate the current page
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
            bool result = false;
            result = Export_func();

            if (result == false)
            {
                return currentIndex;
            }
            
            return base.ValidatePage(currentIndex);
        }

        /// <summary>
        /// The core function calling simiaswebservice for export
        /// </summary>
        /// <returns></returns>

        private bool Export_func()
        {
            bool result = true;
            try
            {
                this.simiasWebService.ExportiFoldersCryptoKeys(selectedDomain.ID, this.filePath.Text);
                
            }
            catch (Exception ex)
            {
                MyMessageBox mmb = 
                    new MyMessageBox(TrayApp.Properties.Resources.unableToExportMesg, 
                    TrayApp.Properties.Resources.wizardText,
                    null,MyMessageBoxButtons.OK, 
                    MyMessageBoxIcon.Error);//"Unable to Export keys");
                    mmb.ShowDialog();
                    result = false;           
                
            }

            return result;
        }
        
        
        /// <summary>
        /// Enable/Disable buttons
        /// </summary>
        private void UpdateSensitivity()
        {
            if (
               this.filePath.Text.Length > 0 && this.filePath.Text.EndsWith(".xml") 
               )
            {
                ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Next | KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
            }

            else
            {
                ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
            }
           
        }


        /// <summary>
        /// Display Recovery Agent
        /// </summary>
        /// <param name="selectedDomain">Domain selected in the combo box</param>
        private void DisplayRAName(DomainItem selectedDomain)
        {
            try
            {
                string RAName = this.ifWebService.GetRAName(selectedDomain.ID);
                if (RAName == null || RAName == "")
                {
                    this.recoveryAgent.Text = ""; 
                    return;
                }
                else
                {

                    this.recoveryAgent.Text = RAName;
                    char[] EmailParser = { '=' };
                    string[] ParsedString = RAName.Split(EmailParser);
                    string emailID = "";
                    if (ParsedString.Length > 1)
                    {
                        for (int x = 0; x < ParsedString.Length; x++)
                        {
                            char[] FinalEmailParser = { '@' };
                            string[] FinalParsedString = ParsedString[x].Split(FinalEmailParser);
                            if (FinalParsedString.Length > 1)
                            {
                                emailID = ParsedString[x];
                                emailAddress = emailID;
                                //this.emailID.Text = emailID;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show("DisplayRAName : {0}", ex.Message);
            }
        }

        /// <summary>
        /// Event handler for browse button clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDlg = new SaveFileDialog();
            fileDlg.Filter = "XML Files|*.xml";
            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
               this.filePath.Text = fileDlg.FileName;
            }
        }

        /// <summary>
        /// Event handler for filePath text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filePath_TextChanged(object sender, EventArgs e)
        {
           UpdateSensitivity();
        }
       
    }
}
