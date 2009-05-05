/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Ashok Singh <siashok@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*****************************************************************************/

using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
    partial class EnterPasswordPopup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox waterMark;
        private System.Windows.Forms.PictureBox pictureBox1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterPasswordPopup));
            this.rememberPassword = new System.Windows.Forms.CheckBox();
            this.lblRetypePassphrase = new System.Windows.Forms.Label();
            this.lblPassphrase = new System.Windows.Forms.Label();
            this.password = new System.Windows.Forms.TextBox();
            this.userName = new System.Windows.Forms.Label();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.server = new System.Windows.Forms.Label();
            this.serverValue = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.waterMark = new System.Windows.Forms.PictureBox();
            this.SuspendLayout();
            // 
            // server label
            // 
            resources.ApplyResources(this.server, "server");
            this.server.Name = "server";
            // 
            // server label value
            // 
            resources.ApplyResources(this.serverValue, "serverValue");
            this.serverValue.Name = "serverValue";
            // 
            // rememberPassword
            // 
            resources.ApplyResources(this.rememberPassword, "rememberPassword");
            this.rememberPassword.Name = "rememberPassword";
            // 
            // lblRetypePassphrase
            // 
            resources.ApplyResources(this.lblRetypePassphrase, "lblRetypePassphrase");
            this.lblRetypePassphrase.Name = "lblRetypePassphrase";
            // 
            // lblPassphrase
            // 
            resources.ApplyResources(this.lblPassphrase, "lblPassphrase");
            this.lblPassphrase.Name = "lblPassphrase";
            // 
            // password
            // 
            resources.ApplyResources(this.password, "password");
            this.password.Name = "password";
            this.password.TextChanged += new System.EventHandler(this.password_TextChanged);
            // 
            // userName
            // 
            resources.ApplyResources(this.userName, "userName");
            this.userName.Name = "userName";
            // 
            // btnOk
            // 
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnOk, "btnOk");
            this.btnOk.Name = "btnOk";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";

            // 
            // panel1
            // 
            System.Resources.ResourceManager resources1 = new System.Resources.ResourceManager(typeof(VerifyPassphraseDialog));
            this.panel1.AccessibleDescription = resources1.GetString("panel1.AccessibleDescription");
            this.panel1.AccessibleName = resources1.GetString("panel1.AccessibleName");
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources1.GetObject("panel1.Anchor")));
            this.panel1.AutoScroll = ((bool)(resources1.GetObject("panel1.AutoScroll")));
            this.panel1.AutoScrollMargin = ((System.Drawing.Size)(resources1.GetObject("panel1.AutoScrollMargin")));
            this.panel1.AutoScrollMinSize = ((System.Drawing.Size)(resources1.GetObject("panel1.AutoScrollMinSize")));
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.BackgroundImage = ((System.Drawing.Image)(resources1.GetObject("panel1.BackgroundImage")));
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.waterMark);
            this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources1.GetObject("panel1.Dock")));
            this.panel1.Enabled = ((bool)(resources1.GetObject("panel1.Enabled")));
            this.panel1.Font = ((System.Drawing.Font)(resources1.GetObject("panel1.Font")));
            this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources1.GetObject("panel1.ImeMode")));
            this.panel1.Location = ((System.Drawing.Point)(resources1.GetObject("panel1.Location")));
            this.panel1.Name = "panel1";
            this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources1.GetObject("panel1.RightToLeft")));
            this.panel1.Size = ((System.Drawing.Size)(resources1.GetObject("panel1.Size")));
            this.panel1.TabIndex = ((int)(resources1.GetObject("panel1.TabIndex")));
            this.panel1.Text = resources1.GetString("panel1.Text");
            this.panel1.Visible = ((bool)(resources1.GetObject("panel1.Visible")));
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleDescription = resources1.GetString("pictureBox1.AccessibleDescription");
            this.pictureBox1.AccessibleName = resources1.GetString("pictureBox1.AccessibleName");
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources1.GetObject("pictureBox1.Anchor")));
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources1.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources1.GetObject("pictureBox1.Dock")));
            this.pictureBox1.Enabled = ((bool)(resources1.GetObject("pictureBox1.Enabled")));
            this.pictureBox1.Font = ((System.Drawing.Font)(resources1.GetObject("pictureBox1.Font")));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources1.GetObject("pictureBox1.Image")));
            this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources1.GetObject("pictureBox1.ImeMode")));
            this.pictureBox1.Location = ((System.Drawing.Point)(resources1.GetObject("pictureBox1.Location")));
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources1.GetObject("pictureBox1.RightToLeft")));
            this.pictureBox1.Size = ((System.Drawing.Size)(resources1.GetObject("pictureBox1.Size")));
            this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources1.GetObject("pictureBox1.SizeMode")));
            this.pictureBox1.TabIndex = ((int)(resources1.GetObject("pictureBox1.TabIndex")));
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Text = resources1.GetString("pictureBox1.Text");
            this.pictureBox1.Visible = ((bool)(resources1.GetObject("pictureBox1.Visible")));
            // 
            // waterMark
            // 
            this.waterMark.AccessibleDescription = resources1.GetString("waterMark.AccessibleDescription");
            this.waterMark.AccessibleName = resources1.GetString("waterMark.AccessibleName");
            this.waterMark.Anchor = ((System.Windows.Forms.AnchorStyles)(resources1.GetObject("waterMark.Anchor")));
            this.waterMark.BackColor = System.Drawing.Color.Transparent;
            this.waterMark.BackgroundImage = ((System.Drawing.Image)(resources1.GetObject("waterMark.BackgroundImage")));
            this.waterMark.Dock = ((System.Windows.Forms.DockStyle)(resources1.GetObject("waterMark.Dock")));
            this.waterMark.Enabled = ((bool)(resources1.GetObject("waterMark.Enabled")));
            this.waterMark.Font = ((System.Drawing.Font)(resources1.GetObject("waterMark.Font")));
            this.waterMark.Image = ((System.Drawing.Image)(resources1.GetObject("waterMark.Image")));
            this.waterMark.ImeMode = ((System.Windows.Forms.ImeMode)(resources1.GetObject("waterMark.ImeMode")));
            this.waterMark.Location = ((System.Drawing.Point)(resources1.GetObject("waterMark.Location")));
            this.waterMark.Name = "waterMark";
            this.waterMark.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources1.GetObject("waterMark.RightToLeft")));
            this.waterMark.Size = ((System.Drawing.Size)(resources1.GetObject("waterMark.Size")));
            this.waterMark.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources1.GetObject("waterMark.SizeMode")));
            this.waterMark.TabIndex = ((int)(resources1.GetObject("waterMark.TabIndex")));
            this.waterMark.TabStop = false;
            this.waterMark.Text = resources1.GetString("waterMark.Text");
            this.waterMark.Visible = ((bool)(resources1.GetObject("waterMark.Visible")));
            // 
            // EnterPasswordPopup
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.server);
            this.Controls.Add(this.serverValue);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.rememberPassword);
            this.Controls.Add(this.lblRetypePassphrase);
            this.Controls.Add(this.lblPassphrase);
            this.Controls.Add(this.password);
            this.Controls.Add(this.userName);
            this.Controls.Add(this.panel1);
            this.Name = "EnterPasswordPopup";
            this.Load += new System.EventHandler(this.EnterPasswordPopup_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void EnterPasswordPopup_Load(object sender, System.EventArgs e)
        {
            /*this.btnOk.Enabled = false;
            this.Passphrase.Select();*/
            this.Icon = new System.Drawing.Icon(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
            this.waterMark.Image = System.Drawing.Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Image = System.Drawing.Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
            
        }

        #endregion

        private System.Windows.Forms.CheckBox rememberPassword;
        private System.Windows.Forms.Label lblRetypePassphrase;
        private System.Windows.Forms.Label lblPassphrase;
        private System.Windows.Forms.TextBox password;
        private System.Windows.Forms.Label userName;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label server;
        private System.Windows.Forms.Label serverValue; 
    }
}
