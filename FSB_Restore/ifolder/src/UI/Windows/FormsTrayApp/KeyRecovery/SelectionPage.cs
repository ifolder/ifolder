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
using TrayApp.Properties;

namespace Novell.Wizard
{
    public partial class SelectionPage: Novell.Wizard.InteriorPageTemplate
    {
                      
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private KeyRecoveryWizard wizard;

      //  private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp.FormsTrayApp));

        /// <summary>
        /// Create Selection PAge object
        /// </summary>
        public SelectionPage()
        {
            InitializeComponent();
                 
        }

        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous Page</param>        
        internal override void ActivatePage(int previousIndex)
        {
            base.ActivatePage(previousIndex);
            wizard = (KeyRecoveryWizard)this.Parent;
            ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Cancel | KeyRecoveryWizardButtons.Back;
             UpdateSensitivity();
        }

        /// <summary>
        /// Enable/disable buttons
        /// </summary>
        private void UpdateSensitivity()
        {
            if ((singleWizRadio.Checked == true )|| (exportRadio.Checked == true) || (importRadio.Checked == true))
            {
                ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Next | KeyRecoveryWizardButtons.Back | KeyRecoveryWizardButtons.Cancel;
            }
        }

        /// <summary>
        /// Validate the current page
        /// </summary>
        /// <param name="currentIndex">Current page</param>
        /// <returns></returns>
      
        internal override int ValidatePage(int currentIndex)
        {

            

           if (singleWizRadio.Checked == true)
            {
                currentIndex = wizard.MaxPages - 6;
            }

            else if (exportRadio.Checked == true)
            {
                currentIndex = wizard.MaxPages - 3;
            }

            else if (importRadio.Checked == true)
            {
                currentIndex = wizard.MaxPages - 5;
            }
            return base.ValidatePage(currentIndex);
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
      /// Event handler for the export radio checkbox changed event
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
        private void exportRadio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

        /// <summary>
        ///  Event handler for the recovery radio checkbox changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recoverButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();

        }

        /// <summary>
        ///  Event handler for the import radio checkbox changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importradio_CheckedChanged(object sender, EventArgs e)
        {
            UpdateSensitivity();
        }

    }
}
