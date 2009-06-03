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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.IO;


namespace Novell.Wizard
{
    public partial class SinglePageWizard : Novell.Wizard.InteriorPageTemplate
    {
                
        private DomainItem selectedDomain = null;
        private string recoveryAgent;
        private iFolderWebService ifWebService = null;
        private SimiasWebService simiasWebService = null;
        private KeyRecoveryWizard wizard;

        private Button browse;
        private string domainID;
        private string inputFilePath;
        private string outputFilePath;
        
        /// <summary>
        /// Required design variable
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// CReate the singlePageWizard object
        /// </summary>
        /// <param name="ifws"></param>
        /// <param name="simws"></param>
        public SinglePageWizard(iFolderWebService ifws, SimiasWebService simws)
        {
            InitializeComponent();
            this.ifWebService = ifws;
            this.simiasWebService = simws;
           
            
        }

        /// <summary>
        /// Activate Page
        /// 
        /// </summary>
        /// <param name="previousIndex">previous Page</param>
        internal override void ActivatePage(int previousIndex)
        {
            base.ActivatePage(previousIndex);
            wizard = (KeyRecoveryWizard)this.Parent;

            selectedDomain = wizard.DomainSelectionPage.SelectedDomain;
            this.accountBox.Text = selectedDomain.Name;

                 
            ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
             UpdateSensitivity();
             p12TextBox.Focus();
           
        }

        /// <summary>
        /// Validate the current page
        /// </summary>
        /// <param name="currentIndex"></param>
        /// <returns></returns>
        internal override int ValidatePage(int currentIndex)
        {
            

            if (Export_func())
                if (KeyRecovery_func())
                    if (Import_func())
                    {
                        currentIndex = wizard.MaxPages - 4;
                        return base.ValidatePage(currentIndex);
                        // if all fine, move to final page
                    }
           return currentIndex;
        }




        internal override int DeactivatePage()
        {

            try
            {   if(File.Exists(inputFilePath))
                File.Delete(inputFilePath);

                if(File.Exists(outputFilePath))
                File.Delete(outputFilePath);
            }
              
            catch(Exception e)
            {}

            return base.DeactivatePage();

        }

       
       
        /// <summary>
        /// obtain default path to perform operations on user box
        /// </summary>
        /// <param name="domainName"></param>

        private void GetDefaultPath(string domainName)
        {
            string appdata = System.Environment.GetEnvironmentVariable("TEMP");
            int i = appdata.LastIndexOf("\\");
            appdata = appdata.Substring(0, i);
            inputFilePath = appdata + "\\" + domainName + "_encry.xml";
            outputFilePath = appdata + "\\" + domainName + "_decry.xml";

        }

          
        private bool Export_func()
        {
            bool result = false;
            try
            {
                GetDefaultPath(selectedDomain.Name);
                this.simiasWebService.ExportiFoldersCryptoKeys(selectedDomain.ID, inputFilePath);
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(TrayApp.Properties.Resources.unableToExportMesg);


            }
            finally
            { result = false;
           
            }

            return result;
        }



               
        private void browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "PKCS12 Files|*.p12";

            if (fileDlg.ShowDialog() == DialogResult.OK)
            {
                p12TextBox.Text = fileDlg.FileName;
            }
        }


        private bool KeyRecovery_func()
        {
            string certname = "";
            string pass = "";
            bool result = false;


            certname = Path.GetFullPath(p12TextBox.Text);

            if ((!File.Exists(certname)) || (!certname.EndsWith(".p12")))
            {

                MessageBox.Show(TrayApp.Properties.Resources.secretPathInvalid);
                return false;
            }

            pass = passwdDomainTextBox.Text;

            X509Certificate2 xcert;
            try
            {
                if (pass.Length > 0)

                    xcert = new X509Certificate2(certname, pass);
                else

                    throw new ArgumentNullException("pass");



                Inner_keyRecovery inner = new Inner_keyRecovery();


                bool flag = false;
                inner.ProcessInputKeyFile(this.inputFilePath, this.outputFilePath, pass, xcert, flag, null);
                result = true;
            }

            catch
            {
                MessageBox.Show(TrayApp.Properties.Resources.importErrorMesg);

            }
            finally { result = false; }
            

            return result;
        }

        private bool Import_func()
        {
            bool result = false;
            try
            {
                string onetimepp = null;

                this.simiasWebService.ImportiFoldersCryptoKeys(selectedDomain.ID, this.newPassphrase.Text, onetimepp, this.outputFilePath);
                result = true;
               // bool rememberOption = this.simiasWebService.GetRememberOption(selectedDomain.ID);
                //clear the values
               // this.simiasWebService.StorePassPhrase(selectedDomain.ID, "", CredentialType.None, false);
                //set the values
               // this.simiasWebService.StorePassPhrase(selectedDomain.ID, this.newPassphrase.Text, CredentialType.Basic, rememberOption);

            

            }
            catch (Exception ex)
            {
                MessageBox.Show(TrayApp.Properties.Resources.importErrorMesg);

                
            }
            finally { 
                result = false;
               
            }
            return result;
        }

        private void UpdateSensitivity()
        {
            if (this.newPassphrase.Text.Length > 0 && this.confirmPassphrase.Text.Length > 0 && this.newPassphrase.Text == this.newPassphrase.Text)

                if (this.p12TextBox.Text.Length > 0 && this.passwdDomainTextBox.Text.Length >= 0)
                {
                    ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Next | KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
                }
                else
                {
                    ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
                }

        }

 
        private void p12TextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        private void passwdDomainTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        private void newPassphrase_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        private void confirmPassphrase_TextChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

    }
}
