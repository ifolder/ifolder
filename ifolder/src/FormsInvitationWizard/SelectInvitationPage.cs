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
using Simias.Agent;

namespace Novell.iFolder.InvitationWizard
{
	public class SelectInvitationPage : Novell.iFolder.InvitationWizard.InteriorPageTemplate
	{
		#region Class Members
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button fileBrowser;
		private System.Windows.Forms.TextBox invitationFile;
		private System.Windows.Forms.Label label2;
		private System.ComponentModel.IContainer components = null;
		#endregion

		public SelectInvitationPage()
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
			this.invitationFile = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.fileBrowser = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// invitationFile
			// 
			this.invitationFile.Location = new System.Drawing.Point(72, 144);
			this.invitationFile.Name = "invitationFile";
			this.invitationFile.Size = new System.Drawing.Size(360, 20);
			this.invitationFile.TabIndex = 3;
			this.invitationFile.Text = "";
			this.invitationFile.TextChanged += new System.EventHandler(this.invitation_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 145);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(32, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Path:";
			// 
			// fileBrowser
			// 
			this.fileBrowser.FlatStyle = FlatStyle.System;
			this.fileBrowser.Location = new System.Drawing.Point(440, 142);
			this.fileBrowser.Name = "fileBrowser";
			this.fileBrowser.Size = new System.Drawing.Size(24, 24);
			this.fileBrowser.TabIndex = 4;
			this.fileBrowser.Text = "...";
			this.fileBrowser.Click += new System.EventHandler(this.fileBrowser_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 104);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(392, 32);
			this.label2.TabIndex = 5;
			this.label2.Text = "Enter the location of the iFolder invitation or click the button to browse for th" +
				"e invitation file.";
			// 
			// SelectInvitationPage
			// 
			this.Controls.Add(this.label2);
			this.Controls.Add(this.fileBrowser);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.invitationFile);
			this.HeaderSubTitle = "Select an iFolder invitation to accept on this computer.";
			this.HeaderTitle = "Choose iFolder Invitation";
			this.Name = "SelectInvitationPage";
			this.Controls.SetChildIndex(this.invitationFile, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.fileBrowser, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void fileBrowser_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

//			openFileDialog1.InitialDirectory = "c:\\" ;
			openFileDialog1.Filter = "ifi files (*.ifi)|*.ifi" ;
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
				Invitation invitation = new Invitation();

				try
				{
					invitation.Load(invitationFile.Text);
					((InvitationWizard)(this.Parent)).Invitation = invitation;
				}
				catch (Exception e)
				{
					// TODO - resource strings.
					MessageBox.Show("The file specified is not a valid iFolder invitation file.\n\n" + e.Message, "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

