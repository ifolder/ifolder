/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
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
		private System.Windows.Forms.Label label2;
		private System.ComponentModel.IContainer components = null;
		#endregion

		public AcceptDeclinePage()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

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
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// iFolderDetails
			// 
			this.iFolderDetails.Location = new System.Drawing.Point(40, 80);
			this.iFolderDetails.Name = "iFolderDetails";
			this.iFolderDetails.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.iFolderDetails.Size = new System.Drawing.Size(408, 95);
			this.iFolderDetails.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Shared iFolder:";
			// 
			// acceptInvitation
			// 
			this.acceptInvitation.Checked = true;
			this.acceptInvitation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.acceptInvitation.Location = new System.Drawing.Point(24, 16);
			this.acceptInvitation.Name = "acceptInvitation";
			this.acceptInvitation.Size = new System.Drawing.Size(368, 24);
			this.acceptInvitation.TabIndex = 3;
			this.acceptInvitation.TabStop = true;
			this.acceptInvitation.Text = "Accept       Participate on one or more computers";
			// 
			// declineInvitation
			// 
			this.declineInvitation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.declineInvitation.Location = new System.Drawing.Point(24, 40);
			this.declineInvitation.Name = "declineInvitation";
			this.declineInvitation.Size = new System.Drawing.Size(368, 24);
			this.declineInvitation.TabIndex = 2;
			this.declineInvitation.Text = "Decline      Do not participate";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.acceptInvitation);
			this.groupBox2.Controls.Add(this.declineInvitation);
			this.groupBox2.Location = new System.Drawing.Point(40, 184);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(408, 72);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 272);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(408, 32);
			this.label2.TabIndex = 6;
			this.label2.Text = "Click Cancel to process the invitation later on this or another computer. The inv" +
				"itation automatically expires after 7 days if you do not respond.";
			// 
			// AcceptDeclinePage
			// 
			this.Controls.Add(this.label2);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.iFolderDetails);
			this.HeaderSubTitle = "HeaderSubTitle";
			this.HeaderTitle = "HeaderTitle";
			this.Name = "AcceptDeclinePage";
			this.Controls.SetChildIndex(this.iFolderDetails, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.groupBox2, 0);
			this.Controls.SetChildIndex(this.label2, 0);
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
			string name = "iFolder name: " + ((InvitationWizard)(this.Parent)).Subscription.SubscriptionCollectionName;
			iFolderDetails.Items.Add(name);
			iFolderDetails.Items.Add(blank);

			string sharedBy = "Shared by: " + ((InvitationWizard)(this.Parent)).Subscription.FromName;
			iFolderDetails.Items.Add(sharedBy);
			iFolderDetails.Items.Add(blank);

			string rights = "Rights: " + ((InvitationWizard)(this.Parent)).Subscription.SubscriptionRights;
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

