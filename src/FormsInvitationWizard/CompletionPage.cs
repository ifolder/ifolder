/***********************************************************************
 *  CompletionPage.cs - Implements the completion page of the
 *  invitation wizard using Windows.Forms
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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

namespace Novell.iFolder.InvitationWizard
{
	public class CompletionPage : Novell.iFolder.InvitationWizard.WelcomePageTemplate
	{
		#region Class Members
		private System.ComponentModel.IContainer components = null;
		#endregion

		public CompletionPage()
		{
			// This call is required by the Windows Form Designer.
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
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// CompletionPage
			// 
			this.Name = "CompletionPage";
			this.WelcomeTitle = "Completing the iFolder Invitation Wizard";

		}
		#endregion

		#region Overridden Methods
		internal override void ActivatePage(int previousIndex)
		{
			this.DescriptionText = ((InvitationWizard)(this.Parent)).SummaryText;
			base.ActivatePage (previousIndex);
		}
		#endregion
	}
}

