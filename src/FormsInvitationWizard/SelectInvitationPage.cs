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
using System.IO;
using Simias;
using Simias.Sync;
using Simias.POBox;

namespace Novell.iFolder.InvitationWizard
{
	/// <summary>
	/// Class for the wizard page where the invitation file is selected.
	/// </summary>
	public class SelectInvitationPage : Novell.iFolder.InvitationWizard.InteriorPageTemplate
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SelectInvitationPage));
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button fileBrowser;
		private System.Windows.Forms.TextBox invitationFile;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.ComponentModel.IContainer components = null;
		#endregion

		/// <summary>
		/// Constructs a SelectInvitationPage object.
		/// </summary>
		public SelectInvitationPage()
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
			this.invitationFile = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.fileBrowser = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// invitationFile
			// 
			this.invitationFile.Location = new System.Drawing.Point(40, 168);
			this.invitationFile.Name = "invitationFile";
			this.invitationFile.Size = new System.Drawing.Size(360, 20);
			this.invitationFile.TabIndex = 3;
			this.invitationFile.Text = "";
			this.invitationFile.TextChanged += new System.EventHandler(this.invitation_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 152);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "iFolder Invitation File";
			// 
			// fileBrowser
			// 
			this.fileBrowser.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.fileBrowser.Location = new System.Drawing.Point(408, 166);
			this.fileBrowser.Name = "fileBrowser";
			this.fileBrowser.Size = new System.Drawing.Size(72, 24);
			this.fileBrowser.TabIndex = 4;
			this.fileBrowser.Text = "Browse...";
			this.fileBrowser.Click += new System.EventHandler(this.fileBrowser_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(392, 40);
			this.label2.TabIndex = 5;
			this.label2.Text = "Type the path, including the filename, where you saved the Collection Subscriptio" +
				"n Information (CSI) file on this computer, or click Browse and select the file.";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(40, 280);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 16);
			this.label3.TabIndex = 6;
			this.label3.Text = "Click Next to continue.";
			// 
			// SelectInvitationPage
			// 
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.fileBrowser);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.invitationFile);
			this.HeaderSubTitle = "HeaderSubTitle";
			this.HeaderTitle = "HeaderTitle";
			this.Name = "SelectInvitationPage";
			this.Controls.SetChildIndex(this.invitationFile, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.fileBrowser, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void fileBrowser_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

//			openFileDialog1.InitialDirectory = "c:\\" ;
			openFileDialog1.Filter = "csi files (*.csi)|*.csi" ;
			openFileDialog1.RestoreDirectory = true ;

			if(openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				invitationFile.Text = openFileDialog1.FileName;
			}
		}

		private void invitation_TextChanged(object sender, System.EventArgs e)
		{
			// Enable the buttons.
			if (invitationFile.Text != "")
			{
				((InvitationWizard)this.Parent).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
			}
			else
			{
				((InvitationWizard)this.Parent).WizardButtons = WizardButtons.Back | WizardButtons.Cancel;
			}
		}
		#endregion

		#region Overridden Methods
		internal override int ValidatePage(int currentIndex)
		{
			if (!File.Exists(invitationFile.Text))
			{
				// The file doesn't exist ... 
				// TODO - resource strings.
				MessageBox.Show("The file specified does not exist.", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
				invitationFile.Focus();

				return currentIndex;
			}
			else
			{
				try
				{
					InvitationWizard invitationWizard = (InvitationWizard)this.Parent;

					// Get the subscription.
					Subscription subscription  = Subscription.GetSubscriptionFromSubscriptionInfo(invitationWizard.Store, invitationFile.Text);

					// Save the subscription.
					invitationWizard.Subscription = subscription;
				}
				catch (SimiasException e)
				{
					e.LogError();
					MessageBox.Show("The file specified is not a valid Collection Subscription Information file.\n\n" + e.Message, "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					invitationFile.Focus();

					return currentIndex;
				}
				catch (Exception e)
				{
					// TODO - resource strings.
					logger.Debug(e, "Invalid file");
					MessageBox.Show("The file specified is not a valid Collection Subscription Information file.\n\n" + e.Message, "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
					invitationFile.Focus();

					return currentIndex;
				}
			}

			return base.ValidatePage (currentIndex);
		}

		internal override void ActivatePage(int previousIndex)
		{
			// Enable the buttons.
			invitation_TextChanged(null, null);

			base.ActivatePage (previousIndex);
		}

		internal override int DeactivatePage()
		{
			// TODO - implement this...
			return base.DeactivatePage ();
		}
		#endregion
	}
}

