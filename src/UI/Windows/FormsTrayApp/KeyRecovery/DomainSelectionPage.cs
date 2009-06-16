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
    public partial class DomainSelectionPage : Novell.Wizard.InteriorPageTemplate
    {
        private iFolderWebService ifWebService = null;
        private SimiasWebService simiasWebService = null;
        private KeyRecoveryWizard wizard;
        

        public DomainSelectionPage(iFolderWebService ifws, SimiasWebService simws)
        {
            InitializeComponent();
            this.ifWebService = ifws;
            this.simiasWebService = simws;
        }

        public DomainItem SelectedDomain
        {

            get
            {
                DomainItem domainItem = (DomainItem)this.domainComboBox.SelectedItem;
              
               
                return domainItem;
            }
        }
        /// <summary>
        /// Fill the domain combo box with all logged in domains
        /// </summary>
        private void GetLoggedInDomains()
        {
            try
            {
                DomainInformation[] domains;
                domains = this.simiasWebService.GetDomains(true);
                foreach (DomainInformation di in domains)
                {
                    
                    if (di.Authenticated)
                    {
                       
                        DomainItem domainItem = new DomainItem(di.Name, di.ID,di.Host);
                        this.domainComboBox.Items.Add(domainItem);
                        this.domainComboBox.SelectedIndex = 0;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("No currently logged-in domain");
            }
        }

        /// <summary>
        /// Deactivate current page
        /// </summary>
        /// <returns></returns>
        internal override int DeactivatePage()
        {

            return base.DeactivatePage();
        }

        internal override void ActivatePage(int previousIndex)
        {
           
               base.ActivatePage(previousIndex);
               wizard = (KeyRecoveryWizard)this.Parent;
               GetLoggedInDomains();
               ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Cancel;
               domainComboBox.Focus();
                UpdateSensitivity();
 
        }

        private void domainComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            UpdateSensitivity();
        }

        private void UpdateSensitivity()
        {

            if ((ifWebService.GetSecurityPolicy(this.SelectedDomain.ID) != 0) && simiasWebService.IsPassPhraseSet(this.SelectedDomain.ID))
            {
                ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Cancel | KeyRecoveryWizardButtons.Next;
            }

            else

                ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Cancel;
        }


        internal override int ValidatePage(int currentIndex)
        {
            string RAName = this.ifWebService.GetRAName(SelectedDomain.ID);

            if (RAName != "DEFAULT" && RAName != null)
                currentIndex = wizard.MaxPages - 8;

            else
                currentIndex = wizard.MaxPages - 9;
            
            return base.ValidatePage(currentIndex);
        }
    }
}
