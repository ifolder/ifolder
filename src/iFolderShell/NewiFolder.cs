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
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using Novell.Win32Util;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for NewiFolder.
	/// </summary>
	[ComVisible(false)]
	public class NewiFolder : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel iFolderProperties;
		private System.Windows.Forms.PictureBox iFolderEmblem;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox dontAsk;
		private string folderName;
		private string loadPath;
		private iFolderWebService ifWebService;
		private const int SHOP_FILEPATH = 0x2;
		private System.Windows.Forms.LinkLabel iFolderHelp;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs a NewiFolder object.
		/// </summary>
		public NewiFolder()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Center the window.
			this.StartPosition = FormStartPosition.CenterScreen;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(NewiFolder));
			this.close = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.iFolderProperties = new System.Windows.Forms.LinkLabel();
			this.iFolderEmblem = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.dontAsk = new System.Windows.Forms.CheckBox();
			this.iFolderHelp = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// close
			// 
			this.close.AccessibleDescription = resources.GetString("close.AccessibleDescription");
			this.close.AccessibleName = resources.GetString("close.AccessibleName");
			this.close.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("close.Anchor")));
			this.close.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("close.BackgroundImage")));
			this.close.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.close.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("close.Dock")));
			this.close.Enabled = ((bool)(resources.GetObject("close.Enabled")));
			this.close.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("close.FlatStyle")));
			this.close.Font = ((System.Drawing.Font)(resources.GetObject("close.Font")));
			this.close.Image = ((System.Drawing.Image)(resources.GetObject("close.Image")));
			this.close.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("close.ImageAlign")));
			this.close.ImageIndex = ((int)(resources.GetObject("close.ImageIndex")));
			this.close.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("close.ImeMode")));
			this.close.Location = ((System.Drawing.Point)(resources.GetObject("close.Location")));
			this.close.Name = "close";
			this.close.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("close.RightToLeft")));
			this.close.Size = ((System.Drawing.Size)(resources.GetObject("close.Size")));
			this.close.TabIndex = ((int)(resources.GetObject("close.TabIndex")));
			this.close.Text = resources.GetString("close.Text");
			this.close.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("close.TextAlign")));
			this.close.Visible = ((bool)(resources.GetObject("close.Visible")));
			this.close.Click += new System.EventHandler(this.close_Click);
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
			// iFolderProperties
			// 
			this.iFolderProperties.AccessibleDescription = resources.GetString("iFolderProperties.AccessibleDescription");
			this.iFolderProperties.AccessibleName = resources.GetString("iFolderProperties.AccessibleName");
			this.iFolderProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderProperties.Anchor")));
			this.iFolderProperties.AutoSize = ((bool)(resources.GetObject("iFolderProperties.AutoSize")));
			this.iFolderProperties.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderProperties.Dock")));
			this.iFolderProperties.Enabled = ((bool)(resources.GetObject("iFolderProperties.Enabled")));
			this.iFolderProperties.Font = ((System.Drawing.Font)(resources.GetObject("iFolderProperties.Font")));
			this.iFolderProperties.Image = ((System.Drawing.Image)(resources.GetObject("iFolderProperties.Image")));
			this.iFolderProperties.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("iFolderProperties.ImageAlign")));
			this.iFolderProperties.ImageIndex = ((int)(resources.GetObject("iFolderProperties.ImageIndex")));
			this.iFolderProperties.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderProperties.ImeMode")));
			this.iFolderProperties.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("iFolderProperties.LinkArea")));
			this.iFolderProperties.Location = ((System.Drawing.Point)(resources.GetObject("iFolderProperties.Location")));
			this.iFolderProperties.Name = "iFolderProperties";
			this.iFolderProperties.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderProperties.RightToLeft")));
			this.iFolderProperties.Size = ((System.Drawing.Size)(resources.GetObject("iFolderProperties.Size")));
			this.iFolderProperties.TabIndex = ((int)(resources.GetObject("iFolderProperties.TabIndex")));
			this.iFolderProperties.TabStop = true;
			this.iFolderProperties.Text = resources.GetString("iFolderProperties.Text");
			this.iFolderProperties.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("iFolderProperties.TextAlign")));
			this.iFolderProperties.Visible = ((bool)(resources.GetObject("iFolderProperties.Visible")));
			this.iFolderProperties.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.iFolderProperties_LinkClicked);
			// 
			// iFolderEmblem
			// 
			this.iFolderEmblem.AccessibleDescription = resources.GetString("iFolderEmblem.AccessibleDescription");
			this.iFolderEmblem.AccessibleName = resources.GetString("iFolderEmblem.AccessibleName");
			this.iFolderEmblem.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderEmblem.Anchor")));
			this.iFolderEmblem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("iFolderEmblem.BackgroundImage")));
			this.iFolderEmblem.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderEmblem.Dock")));
			this.iFolderEmblem.Enabled = ((bool)(resources.GetObject("iFolderEmblem.Enabled")));
			this.iFolderEmblem.Font = ((System.Drawing.Font)(resources.GetObject("iFolderEmblem.Font")));
			this.iFolderEmblem.Image = ((System.Drawing.Image)(resources.GetObject("iFolderEmblem.Image")));
			this.iFolderEmblem.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderEmblem.ImeMode")));
			this.iFolderEmblem.Location = ((System.Drawing.Point)(resources.GetObject("iFolderEmblem.Location")));
			this.iFolderEmblem.Name = "iFolderEmblem";
			this.iFolderEmblem.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderEmblem.RightToLeft")));
			this.iFolderEmblem.Size = ((System.Drawing.Size)(resources.GetObject("iFolderEmblem.Size")));
			this.iFolderEmblem.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("iFolderEmblem.SizeMode")));
			this.iFolderEmblem.TabIndex = ((int)(resources.GetObject("iFolderEmblem.TabIndex")));
			this.iFolderEmblem.TabStop = false;
			this.iFolderEmblem.Text = resources.GetString("iFolderEmblem.Text");
			this.iFolderEmblem.Visible = ((bool)(resources.GetObject("iFolderEmblem.Visible")));
			this.iFolderEmblem.Paint += new System.Windows.Forms.PaintEventHandler(this.iFolderEmblem_Paint);
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
			// dontAsk
			// 
			this.dontAsk.AccessibleDescription = resources.GetString("dontAsk.AccessibleDescription");
			this.dontAsk.AccessibleName = resources.GetString("dontAsk.AccessibleName");
			this.dontAsk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dontAsk.Anchor")));
			this.dontAsk.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("dontAsk.Appearance")));
			this.dontAsk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dontAsk.BackgroundImage")));
			this.dontAsk.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("dontAsk.CheckAlign")));
			this.dontAsk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dontAsk.Dock")));
			this.dontAsk.Enabled = ((bool)(resources.GetObject("dontAsk.Enabled")));
			this.dontAsk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("dontAsk.FlatStyle")));
			this.dontAsk.Font = ((System.Drawing.Font)(resources.GetObject("dontAsk.Font")));
			this.dontAsk.Image = ((System.Drawing.Image)(resources.GetObject("dontAsk.Image")));
			this.dontAsk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("dontAsk.ImageAlign")));
			this.dontAsk.ImageIndex = ((int)(resources.GetObject("dontAsk.ImageIndex")));
			this.dontAsk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dontAsk.ImeMode")));
			this.dontAsk.Location = ((System.Drawing.Point)(resources.GetObject("dontAsk.Location")));
			this.dontAsk.Name = "dontAsk";
			this.dontAsk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dontAsk.RightToLeft")));
			this.dontAsk.Size = ((System.Drawing.Size)(resources.GetObject("dontAsk.Size")));
			this.dontAsk.TabIndex = ((int)(resources.GetObject("dontAsk.TabIndex")));
			this.dontAsk.Text = resources.GetString("dontAsk.Text");
			this.dontAsk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("dontAsk.TextAlign")));
			this.dontAsk.Visible = ((bool)(resources.GetObject("dontAsk.Visible")));
			// 
			// iFolderHelp
			// 
			this.iFolderHelp.AccessibleDescription = resources.GetString("iFolderHelp.AccessibleDescription");
			this.iFolderHelp.AccessibleName = resources.GetString("iFolderHelp.AccessibleName");
			this.iFolderHelp.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("iFolderHelp.Anchor")));
			this.iFolderHelp.AutoSize = ((bool)(resources.GetObject("iFolderHelp.AutoSize")));
			this.iFolderHelp.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("iFolderHelp.Dock")));
			this.iFolderHelp.Enabled = ((bool)(resources.GetObject("iFolderHelp.Enabled")));
			this.iFolderHelp.Font = ((System.Drawing.Font)(resources.GetObject("iFolderHelp.Font")));
			this.iFolderHelp.Image = ((System.Drawing.Image)(resources.GetObject("iFolderHelp.Image")));
			this.iFolderHelp.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("iFolderHelp.ImageAlign")));
			this.iFolderHelp.ImageIndex = ((int)(resources.GetObject("iFolderHelp.ImageIndex")));
			this.iFolderHelp.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("iFolderHelp.ImeMode")));
			this.iFolderHelp.LinkArea = ((System.Windows.Forms.LinkArea)(resources.GetObject("iFolderHelp.LinkArea")));
			this.iFolderHelp.Location = ((System.Drawing.Point)(resources.GetObject("iFolderHelp.Location")));
			this.iFolderHelp.Name = "iFolderHelp";
			this.iFolderHelp.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("iFolderHelp.RightToLeft")));
			this.iFolderHelp.Size = ((System.Drawing.Size)(resources.GetObject("iFolderHelp.Size")));
			this.iFolderHelp.TabIndex = ((int)(resources.GetObject("iFolderHelp.TabIndex")));
			this.iFolderHelp.TabStop = true;
			this.iFolderHelp.Text = resources.GetString("iFolderHelp.Text");
			this.iFolderHelp.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("iFolderHelp.TextAlign")));
			this.iFolderHelp.Visible = ((bool)(resources.GetObject("iFolderHelp.Visible")));
			this.iFolderHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.iFolderHelp_LinkClicked);
			// 
			// NewiFolder
			// 
			this.AcceptButton = this.close;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.iFolderHelp);
			this.Controls.Add(this.dontAsk);
			this.Controls.Add(this.iFolderProperties);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.iFolderEmblem);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.close);
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
			this.Name = "NewiFolder";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the name of the folder.
		/// </summary>
		public string FolderName
		{
			get
			{
				return folderName;
			}
			set
			{
				folderName = value;
			}
		}

		/// <summary>
		/// Gets/sets the path where the assembly was loaded from.
		/// </summary>
		public string LoadPath
		{
			get
			{
				return loadPath;
			}

			set
			{
				loadPath = value;
			}
		}

		/// <summary>
		/// Sets the iFolderService object to use.
		/// </summary>
		public iFolderWebService iFolderWebService
		{
			set { ifWebService = value; }
		}
		#endregion

		#region Event Handlers
		private void iFolderProperties_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// Invoke the iFolder properties dialog.
//			Win32Window.ShObjectProperties(IntPtr.Zero, SHOP_FILEPATH, FolderName, "iFolder");
			new iFolderComponent().InvokeAdvancedDlg(LoadPath, FolderName, 1, false);
		}

		private void iFolderHelp_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			new iFolderComponent().ShowHelp(LoadPath);
		}

		private void close_Click(object sender, System.EventArgs e)
		{
			if (dontAsk.Checked)
			{
				try
				{
					ifWebService.SetDisplayConfirmation(false);
				}
				catch (Exception ex)
				{
					System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(NewiFolder));
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveConfigError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			this.Close();
		}

		private void iFolderEmblem_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			try
			{
				IFSHFILEINFO fi;
				IntPtr ret = Win32Window.ShGetFileInfo(
					FolderName, 
					Win32Window.FILE_ATTRIBUTE_DIRECTORY,
					out fi,
					342,
					Win32Window.SHGFI_ICON | Win32Window.SHGFI_USEFILEATTRIBUTES);

				if (ret != IntPtr.Zero)
				{
					Bitmap bmap = Bitmap.FromHicon(fi.hIcon);
					e.Graphics.DrawImage(bmap, 0, 0);

					IntPtr hIcon = Win32Window.LoadImageFromFile(
						0,
						Path.Combine(loadPath, "ifolder_emblem.ico"),
						Win32Window.IMAGE_ICON,
						32,
						32,
						Win32Window.LR_LOADFROMFILE);

					bmap = Bitmap.FromHicon(hIcon);
					e.Graphics.DrawImage(bmap, 0, 0);
				}
			}
			catch{}
		}
		#endregion
	}
}
