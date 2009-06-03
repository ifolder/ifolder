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

namespace Novell.Wizard
{
    public partial class InfoPage : Novell.Wizard.InteriorPageTemplate
    {

        private System.ComponentModel.IContainer components = null;

       // private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));

        /// <summary>
        /// Create the info page object
        /// </summary>
        public InfoPage()
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

            // Enable/disable the buttons.
            ((KeyRecoveryWizard)this.Parent).WizardButtons = KeyRecoveryWizardButtons.Next | KeyRecoveryWizardButtons.Cancel;

        }

        /// <summary>
        /// Validate the current page inputs
        /// </summary>
        /// <param name="currentIndex">current page</param>
        /// <returns></returns>
        internal override int ValidatePage(int currentIndex)
        {

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
       
    }
}
