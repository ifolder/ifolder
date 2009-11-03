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
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Novell.FormsTrayApp;

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the welcome wizard page.
	/// </summary>
	public class WelcomePage : Novell.Wizard.WelcomePageTemplate
	{
		#region Class Members
		private System.ComponentModel.IContainer components = null;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
		#endregion

		/// <summary>
		/// Constructs a WelcomePage object.
		/// </summary>
		public WelcomePage()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// WelcomePage
			// 
			this.DescriptionText = Resource.GetString("CompletionPageDT");//"Description...";
			this.Name = "WelcomePage";
			this.WelcomeTitle = Resource.GetString("WelcomePageTitle");//"Welcome to the iFolder Invitation Wizard";

		}
		#endregion

		#region Overridden Methods

        /// <summary>
        /// ActivatePage
        /// </summary>
        /// <param name="previousIndex">previous index</param>
		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// Disable the Back button and finish button.
			((AccountWizard)(this.Parent)).WizardButtons = WizardButtons.Next | WizardButtons.Cancel;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

        /// <summary>
        /// Validate Page
        /// </summary>
        /// <param name="currentIndex">current index</param>
        internal override int ValidatePage(int currentIndex)
		{
			return base.ValidatePage (currentIndex);
		}

		#endregion
	}
}

