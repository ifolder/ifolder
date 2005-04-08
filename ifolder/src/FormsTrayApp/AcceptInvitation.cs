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
using System.Runtime.InteropServices;

using Novell.iFolderCom;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for AcceptInvitation.
	/// </summary>
	public class AcceptInvitation : System.Windows.Forms.Form
	{
		private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(AcceptInvitation));
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
		private iFolderWeb ifolder;
		private System.Windows.Forms.Label label3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs an AcceptInvitation object.
		/// </summary>
		/// <param name="ifolderWebService">The web service to use when processing the invitation.</param>
		/// <param name="ifolder">The iFolder to accept.</param>
		public AcceptInvitation(iFolderWebService ifolderWebService, iFolderWeb ifolder)
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AcceptInvitation));
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
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
			this.groupBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox1.ImeMode")));
			this.groupBox1.Location = ((System.Drawing.Point)(resources.GetObject("groupBox1.Location")));
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox1.RightToLeft")));
			this.groupBox1.Size = ((System.Drawing.Size)(resources.GetObject("groupBox1.Size")));
			this.groupBox1.TabIndex = ((int)(resources.GetObject("groupBox1.TabIndex")));
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = resources.GetString("groupBox1.Text");
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// ok
			// 
			this.ok.AccessibleDescription = resources.GetString("ok.AccessibleDescription");
			this.ok.AccessibleName = resources.GetString("ok.AccessibleName");
			this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("ok.Anchor")));
			this.ok.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ok.BackgroundImage")));
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("ok.Dock")));
			this.ok.Enabled = ((bool)(resources.GetObject("ok.Enabled")));
			this.ok.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("ok.FlatStyle")));
			this.ok.Font = ((System.Drawing.Font)(resources.GetObject("ok.Font")));
			this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
			this.ok.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ok.ImageAlign")));
			this.ok.ImageIndex = ((int)(resources.GetObject("ok.ImageIndex")));
			this.ok.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ok.ImeMode")));
			this.ok.Location = ((System.Drawing.Point)(resources.GetObject("ok.Location")));
			this.ok.Name = "ok";
			this.ok.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ok.RightToLeft")));
			this.ok.Size = ((System.Drawing.Size)(resources.GetObject("ok.Size")));
			this.ok.TabIndex = ((int)(resources.GetObject("ok.TabIndex")));
			this.ok.Text = resources.GetString("ok.Text");
			this.ok.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ok.TextAlign")));
			this.ok.Visible = ((bool)(resources.GetObject("ok.Visible")));
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.AccessibleDescription = resources.GetString("cancel.AccessibleDescription");
			this.cancel.AccessibleName = resources.GetString("cancel.AccessibleName");
			this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cancel.Anchor")));
			this.cancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancel.BackgroundImage")));
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cancel.Dock")));
			this.cancel.Enabled = ((bool)(resources.GetObject("cancel.Enabled")));
			this.cancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cancel.FlatStyle")));
			this.cancel.Font = ((System.Drawing.Font)(resources.GetObject("cancel.Font")));
			this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
			this.cancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancel.ImageAlign")));
			this.cancel.ImageIndex = ((int)(resources.GetObject("cancel.ImageIndex")));
			this.cancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cancel.ImeMode")));
			this.cancel.Location = ((System.Drawing.Point)(resources.GetObject("cancel.Location")));
			this.cancel.Name = "cancel";
			this.cancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cancel.RightToLeft")));
			this.cancel.Size = ((System.Drawing.Size)(resources.GetObject("cancel.Size")));
			this.cancel.TabIndex = ((int)(resources.GetObject("cancel.TabIndex")));
			this.cancel.Text = resources.GetString("cancel.Text");
			this.cancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancel.TextAlign")));
			this.cancel.Visible = ((bool)(resources.GetObject("cancel.Visible")));
			// 
			// iFolderDetails
			// 
			this.iFolderDetails.AccessibleDescription = resources.GetString("iFolderDetails.AccessibleDescription");
			this.iFolderDetails.AccessibleName = resources.GetString("iFolderDetails.AccessibleName");
			this.iFolderDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderDetails.Anchor")));
			this.iFolderDetails.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderDetails.BackgroundImage")));
			this.iFolderDetails.ColumnWidth = ((int)(resources.GetObject("iFolderDetails.ColumnWidth")));
			this.iFolderDetails.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderDetails.Dock")));
			this.iFolderDetails.Enabled = ((bool)(resources.GetObject("iFolderDetails.Enabled")));
			this.iFolderDetails.Font = ((System.Drawing.Font)(resources.GetObject("iFolderDetails.Font")));
			this.iFolderDetails.HorizontalExtent = ((int)(resources.GetObject("iFolderDetails.HorizontalExtent")));
			this.iFolderDetails.HorizontalScrollbar = ((bool)(resources.GetObject("iFolderDetails.HorizontalScrollbar")));
			this.iFolderDetails.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderDetails.ImeMode")));
			this.iFolderDetails.IntegralHeight = ((bool)(resources.GetObject("iFolderDetails.IntegralHeight")));
			this.iFolderDetails.ItemHeight = ((int)(resources.GetObject("iFolderDetails.ItemHeight")));
			this.iFolderDetails.Location = ((System.Drawing.Point)(resources.GetObject("iFolderDetails.Location")));
			this.iFolderDetails.Name = "iFolderDetails";
			this.iFolderDetails.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderDetails.RightToLeft")));
			this.iFolderDetails.ScrollAlwaysVisible = ((bool)(resources.GetObject("iFolderDetails.ScrollAlwaysVisible")));
			this.iFolderDetails.Size = ((System.Drawing.Size)(resources.GetObject("iFolderDetails.Size")));
			this.iFolderDetails.TabIndex = ((int)(resources.GetObject("iFolderDetails.TabIndex")));
			this.iFolderDetails.Visible = ((bool)(resources.GetObject("iFolderDetails.Visible")));
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
			// 
			// iFolderLocation
			// 
			this.iFolderLocation.AccessibleDescription = resources.GetString("iFolderLocation.AccessibleDescription");
			this.iFolderLocation.AccessibleName = resources.GetString("iFolderLocation.AccessibleName");
			this.iFolderLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderLocation.Anchor")));
			this.iFolderLocation.AutoSize = ((bool)(resources.GetObject("iFolderLocation.AutoSize")));
			this.iFolderLocation.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderLocation.BackgroundImage")));
			this.iFolderLocation.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderLocation.Dock")));
			this.iFolderLocation.Enabled = ((bool)(resources.GetObject("iFolderLocation.Enabled")));
			this.iFolderLocation.Font = ((System.Drawing.Font)(resources.GetObject("iFolderLocation.Font")));
			this.iFolderLocation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderLocation.ImeMode")));
			this.iFolderLocation.Location = ((System.Drawing.Point)(resources.GetObject("iFolderLocation.Location")));
			this.iFolderLocation.MaxLength = ((int)(resources.GetObject("iFolderLocation.MaxLength")));
			this.iFolderLocation.Multiline = ((bool)(resources.GetObject("iFolderLocation.Multiline")));
			this.iFolderLocation.Name = "iFolderLocation";
			this.iFolderLocation.PasswordChar = ((char)(resources.GetObject("iFolderLocation.PasswordChar")));
			this.iFolderLocation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderLocation.RightToLeft")));
			this.iFolderLocation.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("iFolderLocation.ScrollBars")));
			this.iFolderLocation.Size = ((System.Drawing.Size)(resources.GetObject("iFolderLocation.Size")));
			this.iFolderLocation.TabIndex = ((int)(resources.GetObject("iFolderLocation.TabIndex")));
			this.iFolderLocation.Text = resources.GetString("iFolderLocation.Text");
			this.iFolderLocation.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("iFolderLocation.TextAlign")));
			this.iFolderLocation.Visible = ((bool)(resources.GetObject("iFolderLocation.Visible")));
			this.iFolderLocation.WordWrap = ((bool)(resources.GetObject("iFolderLocation.WordWrap")));
			this.iFolderLocation.TextChanged += new System.EventHandler(this.iFolderLocation_TextChanged);
			// 
			// label2
			// 
			this.label2.AccessibleDescription = resources.GetString("label2.AccessibleDescription");
			this.label2.AccessibleName = resources.GetString("label2.AccessibleName");
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label2.Anchor")));
			this.label2.AutoSize = ((bool)(resources.GetObject("label2.AutoSize")));
			this.label2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label2.Dock")));
			this.label2.Enabled = ((bool)(resources.GetObject("label2.Enabled")));
			this.label2.Font = ((System.Drawing.Font)(resources.GetObject("label2.Font")));
			this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
			this.label2.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.ImageAlign")));
			this.label2.ImageIndex = ((int)(resources.GetObject("label2.ImageIndex")));
			this.label2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label2.ImeMode")));
			this.label2.Location = ((System.Drawing.Point)(resources.GetObject("label2.Location")));
			this.label2.Name = "label2";
			this.label2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label2.RightToLeft")));
			this.label2.Size = ((System.Drawing.Size)(resources.GetObject("label2.Size")));
			this.label2.TabIndex = ((int)(resources.GetObject("label2.TabIndex")));
			this.label2.Text = resources.GetString("label2.Text");
			this.label2.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label2.TextAlign")));
			this.label2.Visible = ((bool)(resources.GetObject("label2.Visible")));
			// 
			// browse
			// 
			this.browse.AccessibleDescription = resources.GetString("browse.AccessibleDescription");
			this.browse.AccessibleName = resources.GetString("browse.AccessibleName");
			this.browse.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("browse.Anchor")));
			this.browse.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("browse.BackgroundImage")));
			this.browse.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("browse.Dock")));
			this.browse.Enabled = ((bool)(resources.GetObject("browse.Enabled")));
			this.browse.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("browse.FlatStyle")));
			this.browse.Font = ((System.Drawing.Font)(resources.GetObject("browse.Font")));
			this.browse.Image = ((System.Drawing.Image)(resources.GetObject("browse.Image")));
			this.browse.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("browse.ImageAlign")));
			this.browse.ImageIndex = ((int)(resources.GetObject("browse.ImageIndex")));
			this.browse.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("browse.ImeMode")));
			this.browse.Location = ((System.Drawing.Point)(resources.GetObject("browse.Location")));
			this.browse.Name = "browse";
			this.browse.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("browse.RightToLeft")));
			this.browse.Size = ((System.Drawing.Size)(resources.GetObject("browse.Size")));
			this.browse.TabIndex = ((int)(resources.GetObject("browse.TabIndex")));
			this.browse.Text = resources.GetString("browse.Text");
			this.browse.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("browse.TextAlign")));
			this.browse.Visible = ((bool)(resources.GetObject("browse.Visible")));
			this.browse.Click += new System.EventHandler(this.browse_Click);
			// 
			// label3
			// 
			this.label3.AccessibleDescription = resources.GetString("label3.AccessibleDescription");
			this.label3.AccessibleName = resources.GetString("label3.AccessibleName");
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label3.Anchor")));
			this.label3.AutoSize = ((bool)(resources.GetObject("label3.AutoSize")));
			this.label3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label3.Dock")));
			this.label3.Enabled = ((bool)(resources.GetObject("label3.Enabled")));
			this.label3.Font = ((System.Drawing.Font)(resources.GetObject("label3.Font")));
			this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
			this.label3.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.ImageAlign")));
			this.label3.ImageIndex = ((int)(resources.GetObject("label3.ImageIndex")));
			this.label3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label3.ImeMode")));
			this.label3.Location = ((System.Drawing.Point)(resources.GetObject("label3.Location")));
			this.label3.Name = "label3";
			this.label3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label3.RightToLeft")));
			this.label3.Size = ((System.Drawing.Size)(resources.GetObject("label3.Size")));
			this.label3.TabIndex = ((int)(resources.GetObject("label3.TabIndex")));
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label3.TextAlign")));
			this.label3.Visible = ((bool)(resources.GetObject("label3.Visible")));
			// 
			// AcceptInvitation
			// 
			this.AcceptButton = this.ok;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.cancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.label3);
			this.Controls.Add(this.browse);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.iFolderLocation);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.iFolderDetails);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "AcceptInvitation";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.AcceptInvitation_Closing);
			this.Load += new System.EventHandler(this.AcceptInvitation_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private string rightsToString(string rights)
		{
			string rightsString;

			switch (rights)
			{
				case "Admin":
				case "ReadWrite":
				case "ReadOnly":
				case "Deny":
				{
					rightsString = resourceManager.GetString(rights);
					break;
				}
				default:
				{
					rightsString = resourceManager.GetString("unknownRights");
					break;
				}
			}

			return rightsString;
		}
		#endregion

		#region Event Handlers
		private void ok_Click(object sender, System.EventArgs e)
		{
			successful = true;

			if (GetDriveType(Path.GetPathRoot(iFolderLocation.Text)) == DRIVE_FIXED)
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
					if (MessageBox.Show(resourceManager.GetString("createDirectoryMessage"), resourceManager.GetString("createDirectoryTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
					{					
						// Create the directory.
						try
						{
							Directory.CreateDirectory(iFolderLocation.Text);
						}
						catch (Exception ex)
						{
							Novell.iFolderCom.MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(resourceManager.GetString("createDirectoryError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
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
					catch (Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(resourceManager.GetString("pathValidationError"), resourceManager.GetString("pathValidationErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}

					// Restore the cursor.
					Cursor = Cursors.Default;

					if (isPathInvalid)
					{
						// The directory is under an existing iFolder ... 
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("pathInvalidError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						iFolderLocation.Focus();

						successful = false;
					}
					else
					{
						try
						{
							// Accept the invitation.
							ifWebService.AcceptiFolderInvitation(ifolder.DomainID, ifolder.ID, iFolderLocation.Text);
						}
						catch (Exception ex)
						{
							if (ex.Message.IndexOf("Path specified cannot be an iFolder", 0) != -1)
							{
								Novell.iFolderCom.MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(resourceManager.GetString("pathInvalidError"), resourceManager.GetString("pathInvalidErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
								mmb.ShowDialog();
								iFolderLocation.Focus();
							}
							else
							{
								Novell.iFolderCom.MyMessageBox mmb = new Novell.iFolderCom.MyMessageBox(resourceManager.GetString("acceptError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
								mmb.ShowDialog();
							}

							successful = false;
						}
					}
				}
			}
			else
			{
				MessageBox.Show(resourceManager.GetString("networkPath"));
				iFolderLocation.Focus();
				successful = false;
			}
		}

		private void browse_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

			folderBrowserDialog.Description = resourceManager.GetString("selectLocation");

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
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"ifolder_app.ico"));
			}
			catch {}

			this.Text = string.Format(this.Text, ifolder.Name);

			// Add the iFolder details to the list box.
			string name = string.Format(resourceManager.GetString("iFolderName"), ifolder.Name);
			iFolderDetails.Items.Add(name);

			string sharedBy = string.Format(resourceManager.GetString("sharedBy"), ifolder.Owner);
			iFolderDetails.Items.Add(sharedBy);

			string rights = string.Format(resourceManager.GetString("rights"), rightsToString(ifolder.CurrentUserRights));
			iFolderDetails.Items.Add(rights);
		}

		private void AcceptInvitation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ((this.DialogResult == DialogResult.OK) && !successful)
			{
				e.Cancel = true;
			}
		}
		#endregion

		private const uint DRIVE_REMOVABLE = 2;
		private const uint DRIVE_FIXED = 3;

		[DllImport("kernel32.dll")]
		private static extern uint GetDriveType(string rootPathName);
	}
}
