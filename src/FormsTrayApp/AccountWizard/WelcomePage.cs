/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

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
			// TODO: Localize
			this.DescriptionText = Resource.GetString("CompletionPageDT");//"Description...";
			this.Name = "WelcomePage";
			this.WelcomeTitle = Resource.GetString("WelcomePageTitle");//"Welcome to the iFolder Invitation Wizard";

		}
		#endregion

		#region Overridden Methods

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

		internal override int ValidatePage(int currentIndex)
		{
			return base.ValidatePage (currentIndex);
		}

		#endregion
	}
}

