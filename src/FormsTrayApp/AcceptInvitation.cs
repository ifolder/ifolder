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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for AcceptInvitation.
	/// </summary>
	public class AcceptInvitation : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.ListBox iFolderDetails;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox iFolderLocation;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button browse;
		private bool successful;
		private iFolderWebService ifWebService;
		private iFolder ifolder;
		private System.Windows.Forms.Label label3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AcceptInvitation(iFolderWebService ifolderWebService, iFolder ifolder)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.ifWebService = ifolderWebService;
			this.ifolder = ifolder;

			//iFolderLocation.Text = Subscription.DefaultRootPath;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.iFolderDetails = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.iFolderLocation = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.browse = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(4, 264);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(480, 4);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Enabled = false;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(328, 280);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "Accept";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(408, 280);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			// 
			// iFolderDetails
			// 
			this.iFolderDetails.Location = new System.Drawing.Point(16, 32);
			this.iFolderDetails.Name = "iFolderDetails";
			this.iFolderDetails.Size = new System.Drawing.Size(456, 95);
			this.iFolderDetails.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "iFolder Details:";
			// 
			// iFolderLocation
			// 
			this.iFolderLocation.Location = new System.Drawing.Point(16, 208);
			this.iFolderLocation.Name = "iFolderLocation";
			this.iFolderLocation.Size = new System.Drawing.Size(376, 20);
			this.iFolderLocation.TabIndex = 5;
			this.iFolderLocation.Text = "";
			this.iFolderLocation.TextChanged += new System.EventHandler(this.iFolderLocation_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 192);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(128, 16);
			this.label2.TabIndex = 6;
			this.label2.Text = "iFolder Location:";
			// 
			// browse
			// 
			this.browse.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.browse.Location = new System.Drawing.Point(400, 207);
			this.browse.Name = "browse";
			this.browse.Size = new System.Drawing.Size(72, 23);
			this.browse.TabIndex = 7;
			this.browse.Text = "Browse...";
			this.browse.Click += new System.EventHandler(this.browse_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(32, 168);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(352, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "Choose a location for the iFolder to be created on this computer.";
			// 
			// AcceptInvitation
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(490, 310);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.browse);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.iFolderLocation);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.iFolderDetails);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AcceptInvitation";
			this.Text = "Setup iFolder <name>";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.AcceptInvitation_Closing);
			this.Load += new System.EventHandler(this.AcceptInvitation_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void ok_Click(object sender, System.EventArgs e)
		{
			successful = true;

			if ((GlobalProperties.GetDriveType(Path.GetPathRoot(iFolderLocation.Text)) & GlobalProperties.DRIVE_REMOTE) 
				!= GlobalProperties.DRIVE_REMOTE)
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
					if (MessageBox.Show("The directory specified does not exist.  Do you want to create the directory?", "Create Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
					{					
						// Create the directory.
						try
						{
							Directory.CreateDirectory(iFolderLocation.Text);
						}
						catch (Exception ex)
						{
							MessageBox.Show("Unable to create directory:\n\n" + ex.Message, "Create Failed");
							iFolderLocation.Focus();
							successful = false;
						}
					}
					else
					{
						iFolderLocation.Focus();
						successful = false;
					}
				}

				if (successful)
				{
					// Display wait cursor.
					Cursor = Cursors.WaitCursor;

					bool isPathInvalid = true;

					// Call into iFolder to make sure the directory specified is valid...
					try
					{
						isPathInvalid = ifWebService.IsPathIniFolder(iFolderLocation.Text);
					}
					catch (WebException ex)
					{
						// TODO: Localize
						MessageBox.Show("An exception occurred while validating the path.\n\n" + ex.Message, "Path Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
					catch (Exception ex)
					{
						// TODO: Localize
						MessageBox.Show("An exception occurred while validating the path.\n\n" + ex.Message, "Path Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}

					// Restore the cursor.
					Cursor = Cursors.Default;

					if (isPathInvalid)
					{
						// The directory is under an existing iFolder ... 
						// TODO: Localize
						MessageBox.Show("The location selected for the new iFolder is below an existing iFolder and cannot be used.  Please select a new location.", "Invalid Directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
						iFolderLocation.Focus();

						successful = false;
					}
					else
					{
						// Save the path ...
						//subscription.CollectionRoot = iFolderLocation.Text;
						try
						{
							ifWebService.AcceptiFolderInvitation(ifolder.ID, iFolderLocation.Text);
						}
						catch (WebException ex)
						{
							// TODO: Localize
							MessageBox.Show("An exception occurred while accepting the invitation.\n\n" + ex.Message, "Accept Error");
							successful = false;
						}
						catch (Exception ex)
						{
							// TODO: Localize
							MessageBox.Show("An exception occurred while accepting the invitation.\n\n" + ex.Message, "Accept Error");
							successful = false;
						}
					}
				}
			}
			else
			{
				MessageBox.Show("iFolders cannot be placed in a network drive.");
				iFolderLocation.Focus();
				successful = false;
			}
		}

		private void browse_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			// TODO: Localize
			folderBrowserDialog.Description = "Select a location for the iFolder.";

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
			ok.Enabled = iFolderLocation.Text.Length > 0;
		}

		private void AcceptInvitation_Load(object sender, System.EventArgs e)
		{
			this.Text = this.Text.Replace("<name>", ifolder.Name);
			// Add the iFolder details to the list box.
			string blank = "";
			// TODO: Localize
			string name = "iFolder name: " + ifolder.Name;
			iFolderDetails.Items.Add(name);
			iFolderDetails.Items.Add(blank);

/*			string sharedBy = "Shared by: " + subscription.FromName;
			iFolderDetails.Items.Add(sharedBy);
			iFolderDetails.Items.Add(blank);
*/
//			string rights = "Rights: " + ((InvitationWizard)(this.Parent)).Subscription.SubscriptionRights;
//			iFolderDetails.Items.Add(rights);
//			iFolderDetails.Items.Add(blank);
		}

		private void AcceptInvitation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ((this.DialogResult == DialogResult.OK) && !successful)
			{
				e.Cancel = true;
			}
		}
		#endregion
	}
}
