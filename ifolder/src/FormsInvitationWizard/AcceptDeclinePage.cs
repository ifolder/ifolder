/***********************************************************************
 *  AcceptDeclinePage.cs - Implements the accept/decline page of the
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
	public class AcceptDeclinePage : Novell.iFolder.InvitationWizard.InteriorPageTemplate
	{
		#region Class Members
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton acceptInvitation;
		private System.Windows.Forms.RadioButton declineInvitation;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ListBox iFolderDetails;
		private System.ComponentModel.IContainer components = null;
		#endregion

		public AcceptDeclinePage()
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
			this.iFolderDetails = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.acceptInvitation = new System.Windows.Forms.RadioButton();
			this.declineInvitation = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// iFolderDetails
			// 
			this.iFolderDetails.Location = new System.Drawing.Point(40, 88);
			this.iFolderDetails.Name = "iFolderDetails";
			this.iFolderDetails.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.iFolderDetails.Size = new System.Drawing.Size(408, 108);
			this.iFolderDetails.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 72);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "iFolder Details:";
			// 
			// acceptInvitation
			// 
			this.acceptInvitation.Checked = true;
			this.acceptInvitation.Location = new System.Drawing.Point(24, 16);
			this.acceptInvitation.Name = "acceptInvitation";
			this.acceptInvitation.Size = new System.Drawing.Size(368, 24);
			this.acceptInvitation.TabIndex = 3;
			this.acceptInvitation.TabStop = true;
			this.acceptInvitation.Text = "Accept iFolder invitation";
			// 
			// declineInvitation
			// 
			this.declineInvitation.Location = new System.Drawing.Point(24, 40);
			this.declineInvitation.Name = "declineInvitation";
			this.declineInvitation.Size = new System.Drawing.Size(368, 24);
			this.declineInvitation.TabIndex = 2;
			this.declineInvitation.Text = "Decline iFolder invitation";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.acceptInvitation);
			this.groupBox2.Controls.Add(this.declineInvitation);
			this.groupBox2.Location = new System.Drawing.Point(40, 216);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(408, 72);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			// 
			// AcceptDeclinePage
			// 
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.iFolderDetails);
			this.HeaderSubTitle = "Choose to accept or decline this iFolder invitation.";
			this.HeaderTitle = "Accept iFolder Invitation";
			this.Name = "AcceptDeclinePage";
			this.Controls.SetChildIndex(this.iFolderDetails, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.groupBox2, 0);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Overridden Methods
		internal override int ValidatePage(int currentIndex)
		{
			// TODO - implement this...
			if (declineInvitation.Checked)
			{
				return ((InvitationWizard)(this.Parent)).MaxPages - 1;
			}
			
			return base.ValidatePage (currentIndex);
		}

		internal override void ActivatePage(int previousIndex)
		{
			// Clear the list box.
			iFolderDetails.Items.Clear();

			// Add the iFolder details to the list box.
			string blank = "";
			string name = "iFolder name: " + ((InvitationWizard)(this.Parent)).Invitation.CollectionName;
			iFolderDetails.Items.Add(name);
			iFolderDetails.Items.Add(blank);

			string sharedBy = "Shared by: " + ((InvitationWizard)(this.Parent)).Invitation.FromName;
			iFolderDetails.Items.Add(sharedBy);
			iFolderDetails.Items.Add(blank);

			string rights = "Rights: " + ((InvitationWizard)(this.Parent)).Invitation.CollectionRights;
			iFolderDetails.Items.Add(rights);
			iFolderDetails.Items.Add(blank);

			// Enable the buttons
			((InvitationWizard)(this.Parent)).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;

			base.ActivatePage (previousIndex);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets a value indicating if the invitation was accepted.
		/// </summary>
		public bool Accept
		{
			get
			{
				return acceptInvitation.Checked;
			}
		}
		#endregion
	}
}

