/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Bruce Getter <bgetter@novell.com>
 |
 |***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Novell.Wizard
{
	/// <summary>
	/// The base class for a wizard page.
	/// </summary>
	public class BaseWizardPage : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// The index of the previous wizard page.
		/// </summary>
		protected int previousIndex = 0;

		/// <summary>
		/// Constructs a BaseWizardPage object.
		/// </summary>
		public BaseWizardPage()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// BaseWizardPage
			// 
			this.Name = "BaseWizardPage";
			this.Size = new System.Drawing.Size(496, 304);

		}
		#endregion

		#region Virtual Methods
		/// <summary>
		/// This method is called when a user clicks the next button.
		/// </summary>
		/// <param name="currentIndex">The index of the current page.</param>
		/// <returns>The index of the next page.</returns>
		internal virtual int ValidatePage(int currentIndex)
		{
			return ++currentIndex;
		}

		/// <summary>
		/// This method is called when a wizard page is activated.
		/// </summary>
		/// <param name="previousIndex">The index of the previous wizard page.</param>
		internal virtual void ActivatePage(int previousIndex)
		{
			if (previousIndex > 0)
				this.previousIndex = previousIndex;
			this.Show();
		}

		/// <summary>
		/// This method is called when a wizard page is deactivated.
		/// </summary>
		/// <returns>The index of the previous wizard page.</returns>
		internal virtual int DeactivatePage()
		{
			this.Hide();
			return this.previousIndex;
		}
		#endregion
	}
}
