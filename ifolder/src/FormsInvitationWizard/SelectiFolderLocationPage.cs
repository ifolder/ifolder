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
using Simias.Invite;

namespace Novell.iFolder.InvitationWizard
{
	public class SelectiFolderLocationPage : Novell.iFolder.InvitationWizard.InteriorPageTemplate
	{
		#region Class Members
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button folderBrowser;
		private System.Windows.Forms.TextBox iFolderLocation;
		private System.ComponentModel.IContainer components = null;
		private bool init = false;
		#endregion

		public SelectiFolderLocationPage()
		{
			init = true;

			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			iFolderLocation.Text = Invitation.DefaultRootPath;
			init = false;
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
			this.label1 = new System.Windows.Forms.Label();
			this.iFolderLocation = new System.Windows.Forms.TextBox();
			this.folderBrowser = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 144);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "iFolder location:";
			// 
			// iFolderLocation
			// 
			this.iFolderLocation.Location = new System.Drawing.Point(120, 144);
			this.iFolderLocation.Name = "iFolderLocation";
			this.iFolderLocation.Size = new System.Drawing.Size(312, 20);
			this.iFolderLocation.TabIndex = 3;
			this.iFolderLocation.Text = "";
			this.iFolderLocation.TextChanged += new System.EventHandler(this.iFolderLocation_TextChanged);
			// 
			// folderBrowser
			// 
			this.folderBrowser.FlatStyle = FlatStyle.System;
			this.folderBrowser.Location = new System.Drawing.Point(440, 142);
			this.folderBrowser.Name = "folderBrowser";
			this.folderBrowser.Size = new System.Drawing.Size(24, 24);
			this.folderBrowser.TabIndex = 4;
			this.folderBrowser.Text = "...";
			this.folderBrowser.Click += new System.EventHandler(this.folderBrowser_Click);
			// 
			// SelectiFolderLocationPage
			// 
			this.Controls.Add(this.folderBrowser);
			this.Controls.Add(this.iFolderLocation);
			this.Controls.Add(this.label1);
			this.HeaderSubTitle = "Select a location for the shared iFolder to be placed on this computer.";
			this.HeaderTitle = "Choose iFolder Location";
			this.Name = "SelectiFolderLocationPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.iFolderLocation, 0);
			this.Controls.SetChildIndex(this.folderBrowser, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void folderBrowser_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			// If a valid directory is specified, set it in the browser dialog.
			if ((iFolderLocation.Text != "") && Directory.Exists(iFolderLocation.Text))
			{
				folderBrowserDialog.SelectedPath = iFolderLocation.Text;
			}

			if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
			{
				iFolderLocation.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void iFolderLocation_TextChanged(object sender, System.EventArgs e)
		{
			if (!init)
			{
				// Enable the buttons.
				if (iFolderLocation.Text != "")
				{
					((InvitationWizard)this.Parent).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
				}
				else
				{
					((InvitationWizard)this.Parent).WizardButtons = WizardButtons.Back | WizardButtons.Cancel;
				}
			}
		}
		#endregion

		#region Overridden Methods
		internal override int ValidatePage(int currentIndex)
		{
			if (!Directory.Exists(iFolderLocation.Text))
			{
				// If a leaf node is specified, the directory will be created
				// under the current working directory ... display this path.
				bool parentExists = false;
				string parent = Path.GetDirectoryName(iFolderLocation.Text);
				while (parent != "")
				{
					if (Directory.Exists(parent))
					{
						parentExists = true;
						break;
					}

					parent = Path.GetDirectoryName(parent);
				}

				if (!parentExists)
				{
					iFolderLocation.Text = Path.Combine(Environment.CurrentDirectory, iFolderLocation.Text);
				}

				// The directory doesn't exist ... 
				if (MessageBox.Show("The directory specified does not exist.  Do you want the wizard to create the directory?", "Create Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
				{					
					// Create the directory.
					try
					{
						Directory.CreateDirectory(iFolderLocation.Text);
					}
					catch (Exception e)
					{
						MessageBox.Show("Unable to create directory:\n\n" + e.Message, "Create Failed");
						iFolderLocation.Focus();
						return currentIndex;
					}
				}
				else
				{
					iFolderLocation.Focus();
					return currentIndex;
				}
			}
			else
			{
				// Display wait cursor.
				Cursor = Cursors.WaitCursor;

				// Call into iFolder to make sure the directory specified is valid...
				iFolderManager manager = iFolderManager.Connect();
				bool isPathInvalid = manager.IsPathIniFolder(iFolderLocation.Text);

				// Restore the cursor.
				Cursor = Cursors.Default;

				if (isPathInvalid)
				{
					// The directory is under an existing iFolder ... 
					MessageBox.Show("The location selected for the new iFolder is below an existing iFolder and cannot be used.  Please select a new location.", "Invalid Directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
					iFolderLocation.Focus();

					return currentIndex;
				}
			}

			// Save the path ...
			((InvitationWizard)(this.Parent)).Invitation.RootPath = iFolderLocation.Text;

			return base.ValidatePage (currentIndex);
		}

		internal override void ActivatePage(int previousIndex)
		{
			// Enable the buttons.
			iFolderLocation_TextChanged(null, null);

			base.ActivatePage (previousIndex);
		}
		#endregion
	}
}

