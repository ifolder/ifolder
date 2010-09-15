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
*                 $Author: Ramesh Sunder <sramesh@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Novell.iFolderCom;

using Simias.Client;
using Simias.Client.Authentication;
using Novell.iFolder.Web;
using Novell.Wizard;

namespace Novell.FormsTrayApp 
{
	/// <summary>
	/// Summary description for EnterPassphraseDialog.
	/// </summary>
	public class EnterPassphraseDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.ComboBox RecoveryAgentCombo;
		private System.Windows.Forms.Label lblRecoveryAgent;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.TextBox RetypePassphrase;
		private System.Windows.Forms.Label lblPassphrase;
		private System.Windows.Forms.Label lblRetypePassphrase;
		private System.Windows.Forms.CheckBox savePassphrase;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.ComponentModel.Container components = null;
		private SimiasWebService simws = null;
        private iFolderWebService ifws = null; 
		private string DomainID;
		private bool	status;
		private static System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(EnterPassphraseDialog));
		private System.Windows.Forms.PictureBox pictureBox;
        private Label lblDomainName;
        private Label lblDomainValue;
        private SizeF strSize;
        private int maxTextWidth = 240;
        private int defaultNameXPos = 16;
        private int defaultNameYPos = 75;
        private int defaultValueXPos = 159;
        private int defaultValueYPos = 75;
        private int defaultSpacing = 10;

		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));

        /// <summary>
        /// Gets the Passphrase status
        /// </summary>
		public bool PassphraseStatus
		{
			get
			{
				return status;
			}
		}

        public EnterPassphraseDialog(string domainID, SimiasWebService simws, iFolderWebService ifws)
            : this(domainID, simws)
        {
            this.ifws = ifws;    
        }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="domainID">ID of the iFolder Domain</param>
        /// <param name="simws">Simias WebService</param>
		public EnterPassphraseDialog(string domainID, SimiasWebService simws)
		{
			this.DomainID = domainID;
			this.simws = simws;
            
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EnterPassphraseDialog));
            System.ComponentModel.ComponentResourceManager componentResources = new System.ComponentModel.ComponentResourceManager(typeof(EnterPassphraseDialog));
			this.panel = new System.Windows.Forms.Panel();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.RecoveryAgentCombo = new System.Windows.Forms.ComboBox();
			this.lblRecoveryAgent = new System.Windows.Forms.Label();
			this.Passphrase = new System.Windows.Forms.TextBox();
			this.RetypePassphrase = new System.Windows.Forms.TextBox();
			this.lblPassphrase = new System.Windows.Forms.Label();
			this.lblRetypePassphrase = new System.Windows.Forms.Label();
			this.savePassphrase = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
            this.lblDomainName = new System.Windows.Forms.Label();
            this.lblDomainValue = new System.Windows.Forms.Label();
			this.panel.SuspendLayout();
            this.strSize = new SizeF();
            this.SuspendLayout();
            Graphics graphics = CreateGraphics();
			// 
			// panel
			// 
			this.panel.AccessibleDescription = resources.GetString("panel1.AccessibleDescription");
			this.panel.AccessibleName = resources.GetString("panel1.AccessibleName");
			this.panel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel1.Anchor")));
			this.panel.AutoScroll = ((bool)(resources.GetObject("panel1.AutoScroll")));
			this.panel.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMargin")));
			this.panel.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMinSize")));
			this.panel.BackColor = System.Drawing.Color.Transparent;
			this.panel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel.Controls.Add(this.pictureBox);
			this.panel.Controls.Add(this.waterMark);
			this.panel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
			this.panel.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
			this.panel.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
			this.panel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
			this.panel.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
			this.panel.Name = "panel1";
			this.panel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
			this.panel.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
			this.panel.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
			this.panel.Text = resources.GetString("panel1.Text");
			this.panel.Visible = ((bool)(resources.GetObject("panel1.Visible")));
			// 
			// pictureBox
			// 
			this.pictureBox.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
			this.pictureBox.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
			this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
			this.pictureBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
			this.pictureBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
			this.pictureBox.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
			this.pictureBox.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
			this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
			this.pictureBox.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
			this.pictureBox.Name = "pictureBox1";
			this.pictureBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
			this.pictureBox.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
			this.pictureBox.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
			this.pictureBox.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
			this.pictureBox.TabStop = false;
			this.pictureBox.Text = resources.GetString("pictureBox1.Text");
			this.pictureBox.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
			// 
			// waterMark
			// 
			this.waterMark.AccessibleDescription = resources.GetString("waterMark.AccessibleDescription");
			this.waterMark.AccessibleName = resources.GetString("waterMark.AccessibleName");
			this.waterMark.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("waterMark.Anchor")));
			this.waterMark.BackColor = System.Drawing.Color.Transparent;
			this.waterMark.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("waterMark.BackgroundImage")));
			this.waterMark.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("waterMark.Dock")));
			this.waterMark.Enabled = ((bool)(resources.GetObject("waterMark.Enabled")));
			this.waterMark.Font = ((System.Drawing.Font)(resources.GetObject("waterMark.Font")));
			this.waterMark.Image = ((System.Drawing.Image)(resources.GetObject("waterMark.Image")));
			this.waterMark.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("waterMark.ImeMode")));
			this.waterMark.Location = ((System.Drawing.Point)(resources.GetObject("waterMark.Location")));
			this.waterMark.Name = "waterMark";
			this.waterMark.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("waterMark.RightToLeft")));
			this.waterMark.Size = ((System.Drawing.Size)(resources.GetObject("waterMark.Size")));
			this.waterMark.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("waterMark.SizeMode")));
			this.waterMark.TabIndex = ((int)(resources.GetObject("waterMark.TabIndex")));
			this.waterMark.TabStop = false;
			this.waterMark.Text = resources.GetString("waterMark.Text");
			this.waterMark.Visible = ((bool)(resources.GetObject("waterMark.Visible")));
            // 
            // lblDomainName
            // 
            componentResources.ApplyResources(this.lblDomainName, "lblDomainName");
            this.lblDomainName.Name = "lblDomainName";
            // 
            // lblDomainValue
            // 
            componentResources.ApplyResources(this.lblDomainValue, "lblDomainValue");
            this.lblDomainValue.Name = "lblDomainValue";
            DomainInformation info = simws.GetDomainInformation(DomainID);
            this.lblDomainValue.Text = info.Name + " (" + info.Host + ")" ;
            this.strSize = graphics.MeasureString(this.lblDomainValue.Text, this.lblDomainValue.Font);
            this.lblDomainValue.Size = new System.Drawing.Size( this.maxTextWidth, ((int)this.strSize.Width / this.maxTextWidth + 1 ) * 16 );
            // 
            // lblRecoveryAgent
            // 
            this.lblRecoveryAgent.AccessibleDescription = resources.GetString("lblRecoveryAgent.AccessibleDescription");
            this.lblRecoveryAgent.AccessibleName = resources.GetString("lblRecoveryAgent.AccessibleName");
            this.lblRecoveryAgent.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblRecoveryAgent.Anchor")));
            this.lblRecoveryAgent.AutoSize = ((bool)(resources.GetObject("lblRecoveryAgent.AutoSize")));
            this.lblRecoveryAgent.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblRecoveryAgent.Dock")));
            this.lblRecoveryAgent.Enabled = ((bool)(resources.GetObject("lblRecoveryAgent.Enabled")));
            this.lblRecoveryAgent.Font = ((System.Drawing.Font)(resources.GetObject("lblRecoveryAgent.Font")));
            this.lblRecoveryAgent.Image = ((System.Drawing.Image)(resources.GetObject("lblRecoveryAgent.Image")));
            this.lblRecoveryAgent.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRecoveryAgent.ImageAlign")));
            this.lblRecoveryAgent.ImageIndex = ((int)(resources.GetObject("lblRecoveryAgent.ImageIndex")));
            this.lblRecoveryAgent.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblRecoveryAgent.ImeMode")));
            this.lblRecoveryAgent.Name = "lblRecoveryAgent";
            this.lblRecoveryAgent.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblRecoveryAgent.RightToLeft")));
            this.lblRecoveryAgent.Size = ((System.Drawing.Size)(resources.GetObject("lblRecoveryAgent.Size")));
            this.lblRecoveryAgent.TabIndex = ((int)(resources.GetObject("lblRecoveryAgent.TabIndex")));
            this.lblRecoveryAgent.Text = resources.GetString("lblRecoveryAgent.Text");
            this.lblRecoveryAgent.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRecoveryAgent.TextAlign")));
            this.lblRecoveryAgent.Visible = ((bool)(resources.GetObject("lblRecoveryAgent.Visible")));
            this.lblRecoveryAgent.Location = new System.Drawing.Point(this.defaultNameXPos, this.lblDomainName.Location.Y + this.lblDomainValue.Size.Height + this.defaultSpacing);
            // 
			// RecoveryAgentCombo
			// 
			this.RecoveryAgentCombo.AccessibleDescription = resources.GetString("RecoveryAgentCombo.AccessibleDescription");
			this.RecoveryAgentCombo.AccessibleName = resources.GetString("RecoveryAgentCombo.AccessibleName");
			this.RecoveryAgentCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RecoveryAgentCombo.Anchor")));
			this.RecoveryAgentCombo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RecoveryAgentCombo.BackgroundImage")));
			this.RecoveryAgentCombo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RecoveryAgentCombo.Dock")));
			this.RecoveryAgentCombo.Enabled = ((bool)(resources.GetObject("RecoveryAgentCombo.Enabled")));
			this.RecoveryAgentCombo.Font = ((System.Drawing.Font)(resources.GetObject("RecoveryAgentCombo.Font")));
			this.RecoveryAgentCombo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RecoveryAgentCombo.ImeMode")));
			this.RecoveryAgentCombo.IntegralHeight = ((bool)(resources.GetObject("RecoveryAgentCombo.IntegralHeight")));
			this.RecoveryAgentCombo.ItemHeight = ((int)(resources.GetObject("RecoveryAgentCombo.ItemHeight")));
			this.RecoveryAgentCombo.MaxDropDownItems = ((int)(resources.GetObject("RecoveryAgentCombo.MaxDropDownItems")));
			this.RecoveryAgentCombo.MaxLength = ((int)(resources.GetObject("RecoveryAgentCombo.MaxLength")));
			this.RecoveryAgentCombo.Name = "RecoveryAgentCombo";
			this.RecoveryAgentCombo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RecoveryAgentCombo.RightToLeft")));
			this.RecoveryAgentCombo.Size = ((System.Drawing.Size)(resources.GetObject("RecoveryAgentCombo.Size")));
			this.RecoveryAgentCombo.TabIndex = ((int)(resources.GetObject("RecoveryAgentCombo.TabIndex")));
			this.RecoveryAgentCombo.Text = resources.GetString("RecoveryAgentCombo.Text");
			this.RecoveryAgentCombo.Visible = ((bool)(resources.GetObject("RecoveryAgentCombo.Visible")));
            this.RecoveryAgentCombo.Location = new System.Drawing.Point(this.defaultValueXPos, this.lblRecoveryAgent.Location.Y);
            // 
            // lblPassphrase
            // 
            this.lblPassphrase.AccessibleDescription = resources.GetString("lblPassphrase.AccessibleDescription");
            this.lblPassphrase.AccessibleName = resources.GetString("lblPassphrase.AccessibleName");
            this.lblPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblPassphrase.Anchor")));
            this.lblPassphrase.AutoSize = ((bool)(resources.GetObject("lblPassphrase.AutoSize")));
            this.lblPassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblPassphrase.Dock")));
            this.lblPassphrase.Enabled = ((bool)(resources.GetObject("lblPassphrase.Enabled")));
            this.lblPassphrase.Font = ((System.Drawing.Font)(resources.GetObject("lblPassphrase.Font")));
            this.lblPassphrase.Image = ((System.Drawing.Image)(resources.GetObject("lblPassphrase.Image")));
            this.lblPassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPassphrase.ImageAlign")));
            this.lblPassphrase.ImageIndex = ((int)(resources.GetObject("lblPassphrase.ImageIndex")));
            this.lblPassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblPassphrase.ImeMode")));
            this.lblPassphrase.Name = "lblPassphrase";
            this.lblPassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblPassphrase.RightToLeft")));
            this.lblPassphrase.Size = ((System.Drawing.Size)(resources.GetObject("lblPassphrase.Size")));
            this.lblPassphrase.TabIndex = ((int)(resources.GetObject("lblPassphrase.TabIndex")));
            this.lblPassphrase.Text = resources.GetString("lblPassphrase.Text");
            this.lblPassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPassphrase.TextAlign")));
            this.lblPassphrase.Visible = ((bool)(resources.GetObject("lblPassphrase.Visible")));
            this.lblPassphrase.Location = new System.Drawing.Point(this.defaultNameXPos, this.lblRecoveryAgent.Location.Y + this.lblRecoveryAgent.Size.Height + this.defaultSpacing);
            // 
			// Passphrase
			// 
			this.Passphrase.AccessibleDescription = resources.GetString("Passphrase.AccessibleDescription");
			this.Passphrase.AccessibleName = resources.GetString("Passphrase.AccessibleName");
			this.Passphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Passphrase.Anchor")));
			this.Passphrase.AutoSize = ((bool)(resources.GetObject("Passphrase.AutoSize")));
			this.Passphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Passphrase.BackgroundImage")));
			this.Passphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Passphrase.Dock")));
			this.Passphrase.Enabled = ((bool)(resources.GetObject("Passphrase.Enabled")));
			this.Passphrase.Font = ((System.Drawing.Font)(resources.GetObject("Passphrase.Font")));
			this.Passphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Passphrase.ImeMode")));
			this.Passphrase.MaxLength = ((int)(resources.GetObject("Passphrase.MaxLength")));
			this.Passphrase.Multiline = ((bool)(resources.GetObject("Passphrase.Multiline")));
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.PasswordChar = ((char)(resources.GetObject("Passphrase.PasswordChar")));
			this.Passphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Passphrase.RightToLeft")));
			this.Passphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("Passphrase.ScrollBars")));
			this.Passphrase.Size = ((System.Drawing.Size)(resources.GetObject("Passphrase.Size")));
			this.Passphrase.TabIndex = ((int)(resources.GetObject("Passphrase.TabIndex")));
			this.Passphrase.Text = resources.GetString("Passphrase.Text");
			this.Passphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("Passphrase.TextAlign")));
			this.Passphrase.Visible = ((bool)(resources.GetObject("Passphrase.Visible")));
			this.Passphrase.WordWrap = ((bool)(resources.GetObject("Passphrase.WordWrap")));
			this.Passphrase.TextChanged += new System.EventHandler(this.Passphrase_TextChanged);
            this.Passphrase.Location = new System.Drawing.Point(this.defaultValueXPos, this.lblPassphrase.Location.Y);
			// 
			// lblRetypePassphrase
			// 
			this.lblRetypePassphrase.AccessibleDescription = resources.GetString("lblRetypePassphrase.AccessibleDescription");
			this.lblRetypePassphrase.AccessibleName = resources.GetString("lblRetypePassphrase.AccessibleName");
			this.lblRetypePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblRetypePassphrase.Anchor")));
			this.lblRetypePassphrase.AutoSize = ((bool)(resources.GetObject("lblRetypePassphrase.AutoSize")));
			this.lblRetypePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblRetypePassphrase.Dock")));
			this.lblRetypePassphrase.Enabled = ((bool)(resources.GetObject("lblRetypePassphrase.Enabled")));
			this.lblRetypePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("lblRetypePassphrase.Font")));
			this.lblRetypePassphrase.Image = ((System.Drawing.Image)(resources.GetObject("lblRetypePassphrase.Image")));
			this.lblRetypePassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRetypePassphrase.ImageAlign")));
			this.lblRetypePassphrase.ImageIndex = ((int)(resources.GetObject("lblRetypePassphrase.ImageIndex")));
			this.lblRetypePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblRetypePassphrase.ImeMode")));
			this.lblRetypePassphrase.Name = "lblRetypePassphrase";
			this.lblRetypePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblRetypePassphrase.RightToLeft")));
			this.lblRetypePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("lblRetypePassphrase.Size")));
			this.lblRetypePassphrase.TabIndex = ((int)(resources.GetObject("lblRetypePassphrase.TabIndex")));
			this.lblRetypePassphrase.Text = resources.GetString("lblRetypePassphrase.Text");
			this.lblRetypePassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRetypePassphrase.TextAlign")));
			this.lblRetypePassphrase.Visible = ((bool)(resources.GetObject("lblRetypePassphrase.Visible")));
            this.lblRetypePassphrase.Location = new System.Drawing.Point(this.defaultNameXPos, this.lblPassphrase.Location.Y + this.lblPassphrase.Size.Height + this.defaultSpacing);
            // 
            // RetypePassphrase
            // 
            this.RetypePassphrase.AccessibleDescription = resources.GetString("RetypePassphrase.AccessibleDescription");
            this.RetypePassphrase.AccessibleName = resources.GetString("RetypePassphrase.AccessibleName");
            this.RetypePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RetypePassphrase.Anchor")));
            this.RetypePassphrase.AutoSize = ((bool)(resources.GetObject("RetypePassphrase.AutoSize")));
            this.RetypePassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RetypePassphrase.BackgroundImage")));
            this.RetypePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RetypePassphrase.Dock")));
            this.RetypePassphrase.Enabled = ((bool)(resources.GetObject("RetypePassphrase.Enabled")));
            this.RetypePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("RetypePassphrase.Font")));
            this.RetypePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RetypePassphrase.ImeMode")));
            this.RetypePassphrase.MaxLength = ((int)(resources.GetObject("RetypePassphrase.MaxLength")));
            this.RetypePassphrase.Multiline = ((bool)(resources.GetObject("RetypePassphrase.Multiline")));
            this.RetypePassphrase.Name = "RetypePassphrase";
            this.RetypePassphrase.PasswordChar = ((char)(resources.GetObject("RetypePassphrase.PasswordChar")));
            this.RetypePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RetypePassphrase.RightToLeft")));
            this.RetypePassphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("RetypePassphrase.ScrollBars")));
            this.RetypePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("RetypePassphrase.Size")));
            this.RetypePassphrase.TabIndex = ((int)(resources.GetObject("RetypePassphrase.TabIndex")));
            this.RetypePassphrase.Text = resources.GetString("RetypePassphrase.Text");
            this.RetypePassphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("RetypePassphrase.TextAlign")));
            this.RetypePassphrase.Visible = ((bool)(resources.GetObject("RetypePassphrase.Visible")));
            this.RetypePassphrase.WordWrap = ((bool)(resources.GetObject("RetypePassphrase.WordWrap")));
            this.RetypePassphrase.TextChanged += new System.EventHandler(this.RetypePassphrase_TextChanged);
            this.RetypePassphrase.Location = new System.Drawing.Point(this.defaultValueXPos, this.lblRetypePassphrase.Location.Y);
			// 
			// savePassphrase
			// 
			this.savePassphrase.AccessibleDescription = resources.GetString("savePassphrase.AccessibleDescription");
			this.savePassphrase.AccessibleName = resources.GetString("savePassphrase.AccessibleName");
			this.savePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("savePassphrase.Anchor")));
			this.savePassphrase.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("savePassphrase.Appearance")));
			this.savePassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("savePassphrase.BackgroundImage")));
			this.savePassphrase.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("savePassphrase.CheckAlign")));
			this.savePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("savePassphrase.Dock")));
			this.savePassphrase.Enabled = ((bool)(resources.GetObject("savePassphrase.Enabled")));
			this.savePassphrase.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("savePassphrase.FlatStyle")));
			this.savePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("savePassphrase.Font")));
			this.savePassphrase.Image = ((System.Drawing.Image)(resources.GetObject("savePassphrase.Image")));
			this.savePassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("savePassphrase.ImageAlign")));
			this.savePassphrase.ImageIndex = ((int)(resources.GetObject("savePassphrase.ImageIndex")));
			this.savePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("savePassphrase.ImeMode")));
            this.savePassphrase.Location = new System.Drawing.Point(this.defaultValueXPos, this.RetypePassphrase.Location.Y + this.RetypePassphrase.Size.Height + this.defaultSpacing);
			this.savePassphrase.Name = "savePassphrase";
			this.savePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("savePassphrase.RightToLeft")));
			this.savePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("savePassphrase.Size")));
			this.savePassphrase.TabIndex = ((int)(resources.GetObject("savePassphrase.TabIndex")));
			this.savePassphrase.Text = resources.GetString("savePassphrase.Text");
			this.savePassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("savePassphrase.TextAlign")));
			this.savePassphrase.Visible = ((bool)(resources.GetObject("savePassphrase.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
            this.btnCancel.Location = new System.Drawing.Point(this.btnCancel.Location.X , this.savePassphrase.Location.Y + this.savePassphrase.Size.Height + this.defaultSpacing);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
			this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
			this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
			this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
			this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
            this.btnOk.Location = new System.Drawing.Point(this.btnOk.Location.X, this.savePassphrase.Location.Y + this.savePassphrase.Size.Height + this.defaultSpacing);
			this.btnOk.Name = "btnOk";
			this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
			this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
			this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
			this.btnOk.Text = resources.GetString("btnOk.Text");
			this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
			this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
			// EnterPassphraseDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblDomainValue);
            this.Controls.Add(this.lblDomainName);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.savePassphrase);
			this.Controls.Add(this.lblRetypePassphrase);
			this.Controls.Add(this.lblPassphrase);
			this.Controls.Add(this.RetypePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.lblRecoveryAgent);
			this.Controls.Add(this.RecoveryAgentCombo);
			this.Controls.Add(this.panel);
			this.AcceptButton = this.btnOk;
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
            this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
            this.ClientSize = new System.Drawing.Size(this.Size.Width, this.btnCancel.Location.Y + this.btnCancel.Size.Height + this.defaultSpacing);
            this.Name = "EnterPassphraseDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.EnterPassphraseDialog_Load);
			this.panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

        /// <summary>
        /// Event handler for Cancel button click event
        /// </summary>
        private void btnCancel_Click(object sender, System.EventArgs e)
		{
			simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
			status = false;
			this.Dispose();
			this.Close();
		}

        /// <summary>
        /// Event handler for OK button click event
        /// </summary>
        private void btnOk_Click(object sender, System.EventArgs e)
		{
			// Check for passphrase
			/*	If passphrase is valid try setting the passphrase.
			 *	if successful try storing on local place
			 *	if success close dialog.
			 *	else show dialog again..
			 * 
			 */
			System.Resources.ResourceManager resManager = new System.Resources.ResourceManager(typeof(Connecting));
			if( this.Passphrase.Text == this.RetypePassphrase.Text)
			{
				string publicKey = null;
				string ragent = null;
				if( this.RecoveryAgentCombo.SelectedItem != null && (string)this.RecoveryAgentCombo.SelectedItem != TrayApp.Properties.Resources.serverDefaultRA)
				{
					// Show the certificate.....
					byte[] CertificateObj = this.simws.GetRACertificateOnClient(this.DomainID, (string)this.RecoveryAgentCombo.SelectedItem);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(CertificateObj);
					//	MyMessageBox mmb = new MyMessageBox( "Verify Certificate", "Verify Certificate", cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 );
					MyMessageBox mmb = new MyMessageBox( string.Format(resManager.GetString("verifyCert"), (string)this.RecoveryAgentCombo.SelectedItem), resManager.GetString("verifyCertTitle"), cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2);
					DialogResult messageDialogResult = mmb.ShowDialog();
					mmb.Dispose();
					mmb.Close();
					if( messageDialogResult != DialogResult.Yes )
						return;
					else
					{
						ragent = (string)this.RecoveryAgentCombo.SelectedItem;
						publicKey = Convert.ToBase64String(cert.GetPublicKey());
					}
					//return;
				}
				/*else	// If recovery agent is not selected...
				{
					MyMessageBox mmb = new MyMessageBox( resManager.GetString("NoCertWarning"), resManager.GetString("NoCertTitle"), "", MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2);
					DialogResult messageDialogResult = mmb.ShowDialog();
					mmb.Dispose();
					mmb.Close();
					if( messageDialogResult != DialogResult.Yes )
						return;
				}*/
				
                 else
                {
                    ragent = "DEFAULT";
                    
                    DomainInformation domainInfo = (DomainInformation)this.simws.GetDomainInformation(this.DomainID);
                    string memberUID = domainInfo.MemberUserID;
                    try
                    {
                        publicKey = this.ifws.GetDefaultServerPublicKey(this.DomainID, memberUID);
                    }
                    catch(Exception ex)
                    {
                        Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("PassStoreErr")/*"Unable to set the passphrase"*/, resourceManager.GetString("$this.Text")/*"Error setting the passphrase"*/, ""/*Resource.GetString("TryAgain")*//*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                        mmb.ShowDialog();
                        mmb.Dispose();
                        return;
                    }
                    
                }
				Status passPhraseStatus = null;
				try
                   
				{
					passPhraseStatus = simws.SetPassPhrase( DomainID, this.Passphrase.Text, ragent, publicKey);
				}
				catch(Exception ex)
				{
					//MessageBox.Show("Unable to set Passphrase. "+ ex.Message);
					MessageBox.Show( Resource.GetString("IsPassphraseSetException")+ex.Message);
				}
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					// Validating Passphrase
					//passPhraseStatus = simws.ValidatePassPhrase( DomainID, PadString(this.Passphrase.Text, 16));
					//if(passPhraseStatus.statusCode != StatusCodes.Success)
					//	MessageBox.Show("Passphrase not validated");
					simws.StorePassPhrase( DomainID, this.Passphrase.Text, CredentialType.Basic, this.savePassphrase.Checked);
					//string passphr = simws.GetPassPhrase(DomainID);
					//MessageBox.Show("Passphrase is set & stored", passphr, MessageBoxButtons.OK);
					//this.status= simws.IsPassPhraseSet(DomainID);
					//if( status == true)
                    status = true;
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("SetPassphraseSuccess")/*"Successfully set the passphrase"*/, resourceManager.GetString("$this.Text"), "", MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
					mmb.Dispose();
					this.Dispose();
					this.Close();
					
				}
				else 
				{
					// Unable to set the passphrase
					status = false;
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("PassStoreErr")/*"Unable to set the passphrase"*/, resourceManager.GetString("$this.Text")/*"Error setting the passphrase"*/, ""/*Resource.GetString("TryAgain")*//*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();
				}
			}
			else
			{
				status = false;
			}
		}

        /// <summary>
        /// Event Handler for EnterPassphrasedialog load event
        /// </summary>
        private void EnterPassphraseDialog_Load(object sender, System.EventArgs e)
		{
			this.btnOk.Enabled = false;
			this.btnCancel.Select();
			this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
			this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			//this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
			this.pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
			this.pictureBox.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
            string[] rAgents =null;
            try
            {
                if (DomainID != null)
                    rAgents = this.simws.GetRAListOnClient(DomainID);
                else
                    FormsTrayApp.log.Info("DomainID is null");
            }           
            catch (Exception ex)
            {                
             	FormsTrayApp.log.Info("EnterPassphrasedialog_load, Exception at GetRAListOnClient and stack is :{0}",ex.StackTrace);
            }                        
			//TODO: Indroduce API to identify server version.
            //Passing old password and new password as NULL, as this function is used to identitfy 
            //wheather server which client is connected is latest or older then OES2SP2            
			//In case of latest server, call will go through and fail and authentication will fail, as current password is passed as NULL             
			//and in case of OLD server, server return as un-supported functioanlity.            
			try
            {
				if(DomainID != null)
				{
                	this.ifws.ChangePassword(DomainID, null, null);
                	this.RecoveryAgentCombo.Items.Add(TrayApp.Properties.Resources.serverDefaultRA);
				}	
            }
            catch (System.Web.Services.Protocols.SoapHeaderException soapEx)
            {                
					FormsTrayApp.log.Info("Server is older, not adding ServerDefaultRA and exception is:{0}",soapEx.StackTrace);
            }
            if (rAgents != null)
            {
                foreach (string rAgent in rAgents)
                {                    
					this.RecoveryAgentCombo.Items.Add(rAgent);                
				}
            }            
				if(this.RecoveryAgentCombo.Items.Count > 0)                
					this.RecoveryAgentCombo.SelectedIndex = 0;			
				// Needs to be changed
				//string[] ralist = this.simws.GetRAList(this.DomainID);
		}

        /// <summary>
        /// Event handler for Passphrase Text changed event
        /// </summary>
        private void Passphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

        /// <summary>
        /// Event Handler for Retype Passphrase text changed event
        /// </summary>
        private void RetypePassphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

        /// <summary>
        /// Update UI sensitivity
        /// </summary>
		private void UpdateSensitivity()
		{
			if( this.Passphrase.Text.Length > 0 && 
				this.RetypePassphrase.Text.Length > 0 && 
				this.Passphrase.Text == this.RetypePassphrase.Text)
			{
				this.btnOk.Enabled = true;
			}
			else
			{
				this.btnOk.Enabled = false;
			}
		}
	}
}
