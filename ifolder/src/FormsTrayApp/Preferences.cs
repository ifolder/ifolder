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
using System.Diagnostics;
using System.Xml;
using System.Net;
using Microsoft.Win32;
using Novell.iFolderCom;
using Novell.Win32Util;
using Simias.Client;
using Simias.Client.Event;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for Preferences.
	/// </summary>
	public class Preferences : System.Windows.Forms.Form
	{
		#region Class Members
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Preferences));
		private const string iFolderRun = "DisableAutoStart";
		private const string notifyShareDisabled = "NotifyShareDisable";
		private const string notifyCollisionDisabled = "NotifyCollisionDisabled";
		private const string notifyJoinDisabled = "NotifyJoinDisabled";
		private const string iFolderKey = @"SOFTWARE\Novell\iFolder";
		private iFolderWebService ifWebService;
		private bool initialConnect = false;
		private bool shutdown = false;
		private Domain defaultDomain = null;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.CheckBox autoStart;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.CheckBox notifyShared;
		private System.Windows.Forms.CheckBox notifyCollisions;
		private System.Windows.Forms.CheckBox notifyJoins;
		private System.Windows.Forms.CheckBox defaultServer;
		private System.Windows.Forms.Button addAccount;
		private System.Windows.Forms.Button removeAccount;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox rememberPassword;
		private System.Windows.Forms.CheckBox autoLogin;
		private System.Windows.Forms.Button advanced;
		private System.Windows.Forms.Button details;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox userName;
		private System.Windows.Forms.TextBox server;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.ListView accounts;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button connect;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// Instantiates a Preferences object.
		/// </summary>
		public Preferences(iFolderWebService ifolderWebService)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			defaultInterval.TextChanged += new EventHandler(defaultInterval_ValueChanged);

			// Show the first tab page by default.
//			tabControl1.SelectedTab = tabPage1;

			ifWebService = ifolderWebService;

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Preferences));
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.notifyJoins = new System.Windows.Forms.CheckBox();
			this.notifyCollisions = new System.Windows.Forms.CheckBox();
			this.notifyShared = new System.Windows.Forms.CheckBox();
			this.autoStart = new System.Windows.Forms.CheckBox();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.accounts = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.details = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.connect = new System.Windows.Forms.Button();
			this.password = new System.Windows.Forms.TextBox();
			this.server = new System.Windows.Forms.TextBox();
			this.userName = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.advanced = new System.Windows.Forms.Button();
			this.autoLogin = new System.Windows.Forms.CheckBox();
			this.rememberPassword = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.defaultServer = new System.Windows.Forms.CheckBox();
			this.removeAccount = new System.Windows.Forms.Button();
			this.addAccount = new System.Windows.Forms.Button();
			this.ok = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// defaultInterval
			// 
			this.defaultInterval.AccessibleDescription = resources.GetString("defaultInterval.AccessibleDescription");
			this.defaultInterval.AccessibleName = resources.GetString("defaultInterval.AccessibleName");
			this.defaultInterval.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("defaultInterval.Anchor")));
			this.defaultInterval.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("defaultInterval.Dock")));
			this.defaultInterval.Enabled = ((bool)(resources.GetObject("defaultInterval.Enabled")));
			this.defaultInterval.Font = ((System.Drawing.Font)(resources.GetObject("defaultInterval.Font")));
			this.defaultInterval.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("defaultInterval.ImeMode")));
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = ((System.Drawing.Point)(resources.GetObject("defaultInterval.Location")));
			this.defaultInterval.Maximum = new System.Decimal(new int[] {
																			86400,
																			0,
																			0,
																			0});
			this.defaultInterval.Minimum = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			-2147483648});
			this.defaultInterval.Name = "defaultInterval";
			this.defaultInterval.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("defaultInterval.RightToLeft")));
			this.defaultInterval.Size = ((System.Drawing.Size)(resources.GetObject("defaultInterval.Size")));
			this.defaultInterval.TabIndex = ((int)(resources.GetObject("defaultInterval.TabIndex")));
			this.defaultInterval.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("defaultInterval.TextAlign")));
			this.defaultInterval.ThousandsSeparator = ((bool)(resources.GetObject("defaultInterval.ThousandsSeparator")));
			this.defaultInterval.UpDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("defaultInterval.UpDownAlign")));
			this.defaultInterval.Visible = ((bool)(resources.GetObject("defaultInterval.Visible")));
			// 
			// displayConfirmation
			// 
			this.displayConfirmation.AccessibleDescription = resources.GetString("displayConfirmation.AccessibleDescription");
			this.displayConfirmation.AccessibleName = resources.GetString("displayConfirmation.AccessibleName");
			this.displayConfirmation.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("displayConfirmation.Anchor")));
			this.displayConfirmation.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("displayConfirmation.Appearance")));
			this.displayConfirmation.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("displayConfirmation.BackgroundImage")));
			this.displayConfirmation.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.CheckAlign")));
			this.displayConfirmation.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("displayConfirmation.Dock")));
			this.displayConfirmation.Enabled = ((bool)(resources.GetObject("displayConfirmation.Enabled")));
			this.displayConfirmation.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("displayConfirmation.FlatStyle")));
			this.displayConfirmation.Font = ((System.Drawing.Font)(resources.GetObject("displayConfirmation.Font")));
			this.displayConfirmation.Image = ((System.Drawing.Image)(resources.GetObject("displayConfirmation.Image")));
			this.displayConfirmation.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.ImageAlign")));
			this.displayConfirmation.ImageIndex = ((int)(resources.GetObject("displayConfirmation.ImageIndex")));
			this.displayConfirmation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("displayConfirmation.ImeMode")));
			this.displayConfirmation.Location = ((System.Drawing.Point)(resources.GetObject("displayConfirmation.Location")));
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("displayConfirmation.RightToLeft")));
			this.displayConfirmation.Size = ((System.Drawing.Size)(resources.GetObject("displayConfirmation.Size")));
			this.displayConfirmation.TabIndex = ((int)(resources.GetObject("displayConfirmation.TabIndex")));
			this.displayConfirmation.Text = resources.GetString("displayConfirmation.Text");
			this.displayConfirmation.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.TextAlign")));
			this.displayConfirmation.Visible = ((bool)(resources.GetObject("displayConfirmation.Visible")));
			this.displayConfirmation.CheckedChanged += new System.EventHandler(this.displayConfirmation_CheckedChanged);
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
			// tabControl1
			// 
			this.tabControl1.AccessibleDescription = resources.GetString("tabControl1.AccessibleDescription");
			this.tabControl1.AccessibleName = resources.GetString("tabControl1.AccessibleName");
			this.tabControl1.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabControl1.Alignment")));
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabControl1.Anchor")));
			this.tabControl1.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabControl1.Appearance")));
			this.tabControl1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabControl1.BackgroundImage")));
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabControl1.Dock")));
			this.tabControl1.Enabled = ((bool)(resources.GetObject("tabControl1.Enabled")));
			this.tabControl1.Font = ((System.Drawing.Font)(resources.GetObject("tabControl1.Font")));
			this.tabControl1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabControl1.ImeMode")));
			this.tabControl1.ItemSize = ((System.Drawing.Size)(resources.GetObject("tabControl1.ItemSize")));
			this.tabControl1.Location = ((System.Drawing.Point)(resources.GetObject("tabControl1.Location")));
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Padding = ((System.Drawing.Point)(resources.GetObject("tabControl1.Padding")));
			this.tabControl1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabControl1.RightToLeft")));
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.ShowToolTips = ((bool)(resources.GetObject("tabControl1.ShowToolTips")));
			this.tabControl1.Size = ((System.Drawing.Size)(resources.GetObject("tabControl1.Size")));
			this.tabControl1.TabIndex = ((int)(resources.GetObject("tabControl1.TabIndex")));
			this.tabControl1.Text = resources.GetString("tabControl1.Text");
			this.tabControl1.Visible = ((bool)(resources.GetObject("tabControl1.Visible")));
			// 
			// tabPage2
			// 
			this.tabPage2.AccessibleDescription = resources.GetString("tabPage2.AccessibleDescription");
			this.tabPage2.AccessibleName = resources.GetString("tabPage2.AccessibleName");
			this.tabPage2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage2.Anchor")));
			this.tabPage2.AutoScroll = ((bool)(resources.GetObject("tabPage2.AutoScroll")));
			this.tabPage2.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage2.AutoScrollMargin")));
			this.tabPage2.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage2.AutoScrollMinSize")));
			this.tabPage2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage2.BackgroundImage")));
			this.tabPage2.Controls.Add(this.groupBox4);
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage2.Dock")));
			this.tabPage2.Enabled = ((bool)(resources.GetObject("tabPage2.Enabled")));
			this.tabPage2.Font = ((System.Drawing.Font)(resources.GetObject("tabPage2.Font")));
			this.tabPage2.ImageIndex = ((int)(resources.GetObject("tabPage2.ImageIndex")));
			this.tabPage2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage2.ImeMode")));
			this.tabPage2.Location = ((System.Drawing.Point)(resources.GetObject("tabPage2.Location")));
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage2.RightToLeft")));
			this.tabPage2.Size = ((System.Drawing.Size)(resources.GetObject("tabPage2.Size")));
			this.tabPage2.TabIndex = ((int)(resources.GetObject("tabPage2.TabIndex")));
			this.tabPage2.Text = resources.GetString("tabPage2.Text");
			this.tabPage2.ToolTipText = resources.GetString("tabPage2.ToolTipText");
			this.tabPage2.Visible = ((bool)(resources.GetObject("tabPage2.Visible")));
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
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.AccessibleDescription = resources.GetString("apply.AccessibleDescription");
			this.apply.AccessibleName = resources.GetString("apply.AccessibleName");
			this.apply.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("apply.Anchor")));
			this.apply.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("apply.BackgroundImage")));
			this.apply.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("apply.Dock")));
			this.apply.Enabled = ((bool)(resources.GetObject("apply.Enabled")));
			this.apply.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("apply.FlatStyle")));
			this.apply.Font = ((System.Drawing.Font)(resources.GetObject("apply.Font")));
			this.apply.Image = ((System.Drawing.Image)(resources.GetObject("apply.Image")));
			this.apply.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("apply.ImageAlign")));
			this.apply.ImageIndex = ((int)(resources.GetObject("apply.ImageIndex")));
			this.apply.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("apply.ImeMode")));
			this.apply.Location = ((System.Drawing.Point)(resources.GetObject("apply.Location")));
			this.apply.Name = "apply";
			this.apply.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("apply.RightToLeft")));
			this.apply.Size = ((System.Drawing.Size)(resources.GetObject("apply.Size")));
			this.apply.TabIndex = ((int)(resources.GetObject("apply.TabIndex")));
			this.apply.Text = resources.GetString("apply.Text");
			this.apply.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("apply.TextAlign")));
			this.apply.Visible = ((bool)(resources.GetObject("apply.Visible")));
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.defaultInterval);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
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
			// autoSync
			// 
			this.autoSync.AccessibleDescription = resources.GetString("autoSync.AccessibleDescription");
			this.autoSync.AccessibleName = resources.GetString("autoSync.AccessibleName");
			this.autoSync.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoSync.Anchor")));
			this.autoSync.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoSync.Appearance")));
			this.autoSync.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoSync.BackgroundImage")));
			this.autoSync.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.CheckAlign")));
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoSync.Dock")));
			this.autoSync.Enabled = ((bool)(resources.GetObject("autoSync.Enabled")));
			this.autoSync.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoSync.FlatStyle")));
			this.autoSync.Font = ((System.Drawing.Font)(resources.GetObject("autoSync.Font")));
			this.autoSync.Image = ((System.Drawing.Image)(resources.GetObject("autoSync.Image")));
			this.autoSync.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.ImageAlign")));
			this.autoSync.ImageIndex = ((int)(resources.GetObject("autoSync.ImageIndex")));
			this.autoSync.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoSync.ImeMode")));
			this.autoSync.Location = ((System.Drawing.Point)(resources.GetObject("autoSync.Location")));
			this.autoSync.Name = "autoSync";
			this.autoSync.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoSync.RightToLeft")));
			this.autoSync.Size = ((System.Drawing.Size)(resources.GetObject("autoSync.Size")));
			this.autoSync.TabIndex = ((int)(resources.GetObject("autoSync.TabIndex")));
			this.autoSync.Text = resources.GetString("autoSync.Text");
			this.autoSync.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.TextAlign")));
			this.autoSync.Visible = ((bool)(resources.GetObject("autoSync.Visible")));
			this.autoSync.CheckedChanged += new System.EventHandler(this.autoSync_CheckedChanged);
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
			// groupBox3
			// 
			this.groupBox3.AccessibleDescription = resources.GetString("groupBox3.AccessibleDescription");
			this.groupBox3.AccessibleName = resources.GetString("groupBox3.AccessibleName");
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox3.Anchor")));
			this.groupBox3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox3.BackgroundImage")));
			this.groupBox3.Controls.Add(this.autoStart);
			this.groupBox3.Controls.Add(this.displayConfirmation);
			this.groupBox3.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox3.Dock")));
			this.groupBox3.Enabled = ((bool)(resources.GetObject("groupBox3.Enabled")));
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Font = ((System.Drawing.Font)(resources.GetObject("groupBox3.Font")));
			this.groupBox3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox3.ImeMode")));
			this.groupBox3.Location = ((System.Drawing.Point)(resources.GetObject("groupBox3.Location")));
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox3.RightToLeft")));
			this.groupBox3.Size = ((System.Drawing.Size)(resources.GetObject("groupBox3.Size")));
			this.groupBox3.TabIndex = ((int)(resources.GetObject("groupBox3.TabIndex")));
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = resources.GetString("groupBox3.Text");
			this.groupBox3.Visible = ((bool)(resources.GetObject("groupBox3.Visible")));
			// 
			// notifyJoins
			// 
			this.notifyJoins.AccessibleDescription = resources.GetString("notifyJoins.AccessibleDescription");
			this.notifyJoins.AccessibleName = resources.GetString("notifyJoins.AccessibleName");
			this.notifyJoins.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("notifyJoins.Anchor")));
			this.notifyJoins.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("notifyJoins.Appearance")));
			this.notifyJoins.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("notifyJoins.BackgroundImage")));
			this.notifyJoins.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.CheckAlign")));
			this.notifyJoins.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("notifyJoins.Dock")));
			this.notifyJoins.Enabled = ((bool)(resources.GetObject("notifyJoins.Enabled")));
			this.notifyJoins.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("notifyJoins.FlatStyle")));
			this.notifyJoins.Font = ((System.Drawing.Font)(resources.GetObject("notifyJoins.Font")));
			this.notifyJoins.Image = ((System.Drawing.Image)(resources.GetObject("notifyJoins.Image")));
			this.notifyJoins.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.ImageAlign")));
			this.notifyJoins.ImageIndex = ((int)(resources.GetObject("notifyJoins.ImageIndex")));
			this.notifyJoins.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyJoins.ImeMode")));
			this.notifyJoins.Location = ((System.Drawing.Point)(resources.GetObject("notifyJoins.Location")));
			this.notifyJoins.Name = "notifyJoins";
			this.notifyJoins.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyJoins.RightToLeft")));
			this.notifyJoins.Size = ((System.Drawing.Size)(resources.GetObject("notifyJoins.Size")));
			this.notifyJoins.TabIndex = ((int)(resources.GetObject("notifyJoins.TabIndex")));
			this.notifyJoins.Text = resources.GetString("notifyJoins.Text");
			this.notifyJoins.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.TextAlign")));
			this.notifyJoins.Visible = ((bool)(resources.GetObject("notifyJoins.Visible")));
			this.notifyJoins.CheckedChanged += new System.EventHandler(this.notifyJoins_CheckedChanged);
			// 
			// notifyCollisions
			// 
			this.notifyCollisions.AccessibleDescription = resources.GetString("notifyCollisions.AccessibleDescription");
			this.notifyCollisions.AccessibleName = resources.GetString("notifyCollisions.AccessibleName");
			this.notifyCollisions.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("notifyCollisions.Anchor")));
			this.notifyCollisions.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("notifyCollisions.Appearance")));
			this.notifyCollisions.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("notifyCollisions.BackgroundImage")));
			this.notifyCollisions.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.CheckAlign")));
			this.notifyCollisions.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("notifyCollisions.Dock")));
			this.notifyCollisions.Enabled = ((bool)(resources.GetObject("notifyCollisions.Enabled")));
			this.notifyCollisions.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("notifyCollisions.FlatStyle")));
			this.notifyCollisions.Font = ((System.Drawing.Font)(resources.GetObject("notifyCollisions.Font")));
			this.notifyCollisions.Image = ((System.Drawing.Image)(resources.GetObject("notifyCollisions.Image")));
			this.notifyCollisions.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.ImageAlign")));
			this.notifyCollisions.ImageIndex = ((int)(resources.GetObject("notifyCollisions.ImageIndex")));
			this.notifyCollisions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyCollisions.ImeMode")));
			this.notifyCollisions.Location = ((System.Drawing.Point)(resources.GetObject("notifyCollisions.Location")));
			this.notifyCollisions.Name = "notifyCollisions";
			this.notifyCollisions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyCollisions.RightToLeft")));
			this.notifyCollisions.Size = ((System.Drawing.Size)(resources.GetObject("notifyCollisions.Size")));
			this.notifyCollisions.TabIndex = ((int)(resources.GetObject("notifyCollisions.TabIndex")));
			this.notifyCollisions.Text = resources.GetString("notifyCollisions.Text");
			this.notifyCollisions.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.TextAlign")));
			this.notifyCollisions.Visible = ((bool)(resources.GetObject("notifyCollisions.Visible")));
			this.notifyCollisions.CheckedChanged += new System.EventHandler(this.notifyCollisions_CheckedChanged);
			// 
			// notifyShared
			// 
			this.notifyShared.AccessibleDescription = resources.GetString("notifyShared.AccessibleDescription");
			this.notifyShared.AccessibleName = resources.GetString("notifyShared.AccessibleName");
			this.notifyShared.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("notifyShared.Anchor")));
			this.notifyShared.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("notifyShared.Appearance")));
			this.notifyShared.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("notifyShared.BackgroundImage")));
			this.notifyShared.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.CheckAlign")));
			this.notifyShared.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("notifyShared.Dock")));
			this.notifyShared.Enabled = ((bool)(resources.GetObject("notifyShared.Enabled")));
			this.notifyShared.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("notifyShared.FlatStyle")));
			this.notifyShared.Font = ((System.Drawing.Font)(resources.GetObject("notifyShared.Font")));
			this.notifyShared.Image = ((System.Drawing.Image)(resources.GetObject("notifyShared.Image")));
			this.notifyShared.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.ImageAlign")));
			this.notifyShared.ImageIndex = ((int)(resources.GetObject("notifyShared.ImageIndex")));
			this.notifyShared.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyShared.ImeMode")));
			this.notifyShared.Location = ((System.Drawing.Point)(resources.GetObject("notifyShared.Location")));
			this.notifyShared.Name = "notifyShared";
			this.notifyShared.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyShared.RightToLeft")));
			this.notifyShared.Size = ((System.Drawing.Size)(resources.GetObject("notifyShared.Size")));
			this.notifyShared.TabIndex = ((int)(resources.GetObject("notifyShared.TabIndex")));
			this.notifyShared.Text = resources.GetString("notifyShared.Text");
			this.notifyShared.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.TextAlign")));
			this.notifyShared.Visible = ((bool)(resources.GetObject("notifyShared.Visible")));
			this.notifyShared.CheckedChanged += new System.EventHandler(this.notifyShared_CheckedChanged);
			// 
			// autoStart
			// 
			this.autoStart.AccessibleDescription = resources.GetString("autoStart.AccessibleDescription");
			this.autoStart.AccessibleName = resources.GetString("autoStart.AccessibleName");
			this.autoStart.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoStart.Anchor")));
			this.autoStart.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoStart.Appearance")));
			this.autoStart.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoStart.BackgroundImage")));
			this.autoStart.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.CheckAlign")));
			this.autoStart.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoStart.Dock")));
			this.autoStart.Enabled = ((bool)(resources.GetObject("autoStart.Enabled")));
			this.autoStart.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoStart.FlatStyle")));
			this.autoStart.Font = ((System.Drawing.Font)(resources.GetObject("autoStart.Font")));
			this.autoStart.Image = ((System.Drawing.Image)(resources.GetObject("autoStart.Image")));
			this.autoStart.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.ImageAlign")));
			this.autoStart.ImageIndex = ((int)(resources.GetObject("autoStart.ImageIndex")));
			this.autoStart.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoStart.ImeMode")));
			this.autoStart.Location = ((System.Drawing.Point)(resources.GetObject("autoStart.Location")));
			this.autoStart.Name = "autoStart";
			this.autoStart.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoStart.RightToLeft")));
			this.autoStart.Size = ((System.Drawing.Size)(resources.GetObject("autoStart.Size")));
			this.autoStart.TabIndex = ((int)(resources.GetObject("autoStart.TabIndex")));
			this.autoStart.Text = resources.GetString("autoStart.Text");
			this.autoStart.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.TextAlign")));
			this.autoStart.Visible = ((bool)(resources.GetObject("autoStart.Visible")));
			this.autoStart.CheckedChanged += new System.EventHandler(this.autoStart_CheckedChanged);
			// 
			// tabPage5
			// 
			this.tabPage5.AccessibleDescription = resources.GetString("tabPage5.AccessibleDescription");
			this.tabPage5.AccessibleName = resources.GetString("tabPage5.AccessibleName");
			this.tabPage5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage5.Anchor")));
			this.tabPage5.AutoScroll = ((bool)(resources.GetObject("tabPage5.AutoScroll")));
			this.tabPage5.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage5.AutoScrollMargin")));
			this.tabPage5.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage5.AutoScrollMinSize")));
			this.tabPage5.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage5.BackgroundImage")));
			this.tabPage5.Controls.Add(this.accounts);
			this.tabPage5.Controls.Add(this.details);
			this.tabPage5.Controls.Add(this.groupBox2);
			this.tabPage5.Controls.Add(this.removeAccount);
			this.tabPage5.Controls.Add(this.addAccount);
			this.tabPage5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage5.Dock")));
			this.tabPage5.Enabled = ((bool)(resources.GetObject("tabPage5.Enabled")));
			this.tabPage5.Font = ((System.Drawing.Font)(resources.GetObject("tabPage5.Font")));
			this.tabPage5.ImageIndex = ((int)(resources.GetObject("tabPage5.ImageIndex")));
			this.tabPage5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage5.ImeMode")));
			this.tabPage5.Location = ((System.Drawing.Point)(resources.GetObject("tabPage5.Location")));
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage5.RightToLeft")));
			this.tabPage5.Size = ((System.Drawing.Size)(resources.GetObject("tabPage5.Size")));
			this.tabPage5.TabIndex = ((int)(resources.GetObject("tabPage5.TabIndex")));
			this.tabPage5.Text = resources.GetString("tabPage5.Text");
			this.tabPage5.ToolTipText = resources.GetString("tabPage5.ToolTipText");
			this.tabPage5.Visible = ((bool)(resources.GetObject("tabPage5.Visible")));
			// 
			// accounts
			// 
			this.accounts.AccessibleDescription = resources.GetString("accounts.AccessibleDescription");
			this.accounts.AccessibleName = resources.GetString("accounts.AccessibleName");
			this.accounts.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("accounts.Alignment")));
			this.accounts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accounts.Anchor")));
			this.accounts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accounts.BackgroundImage")));
			this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader2,
																					   this.columnHeader3});
			this.accounts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accounts.Dock")));
			this.accounts.Enabled = ((bool)(resources.GetObject("accounts.Enabled")));
			this.accounts.Font = ((System.Drawing.Font)(resources.GetObject("accounts.Font")));
			this.accounts.FullRowSelect = true;
			this.accounts.HideSelection = false;
			this.accounts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accounts.ImeMode")));
			this.accounts.LabelWrap = ((bool)(resources.GetObject("accounts.LabelWrap")));
			this.accounts.Location = ((System.Drawing.Point)(resources.GetObject("accounts.Location")));
			this.accounts.MultiSelect = false;
			this.accounts.Name = "accounts";
			this.accounts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accounts.RightToLeft")));
			this.accounts.Size = ((System.Drawing.Size)(resources.GetObject("accounts.Size")));
			this.accounts.TabIndex = ((int)(resources.GetObject("accounts.TabIndex")));
			this.accounts.Text = resources.GetString("accounts.Text");
			this.accounts.View = System.Windows.Forms.View.Details;
			this.accounts.Visible = ((bool)(resources.GetObject("accounts.Visible")));
			this.accounts.DoubleClick += new System.EventHandler(this.details_Click);
			this.accounts.SelectedIndexChanged += new System.EventHandler(this.accounts_SelectedIndexChanged);
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = resources.GetString("columnHeader2.Text");
			this.columnHeader2.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader2.TextAlign")));
			this.columnHeader2.Width = ((int)(resources.GetObject("columnHeader2.Width")));
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = resources.GetString("columnHeader3.Text");
			this.columnHeader3.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader3.TextAlign")));
			this.columnHeader3.Width = ((int)(resources.GetObject("columnHeader3.Width")));
			// 
			// details
			// 
			this.details.AccessibleDescription = resources.GetString("details.AccessibleDescription");
			this.details.AccessibleName = resources.GetString("details.AccessibleName");
			this.details.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("details.Anchor")));
			this.details.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("details.BackgroundImage")));
			this.details.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("details.Dock")));
			this.details.Enabled = ((bool)(resources.GetObject("details.Enabled")));
			this.details.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("details.FlatStyle")));
			this.details.Font = ((System.Drawing.Font)(resources.GetObject("details.Font")));
			this.details.Image = ((System.Drawing.Image)(resources.GetObject("details.Image")));
			this.details.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("details.ImageAlign")));
			this.details.ImageIndex = ((int)(resources.GetObject("details.ImageIndex")));
			this.details.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("details.ImeMode")));
			this.details.Location = ((System.Drawing.Point)(resources.GetObject("details.Location")));
			this.details.Name = "details";
			this.details.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("details.RightToLeft")));
			this.details.Size = ((System.Drawing.Size)(resources.GetObject("details.Size")));
			this.details.TabIndex = ((int)(resources.GetObject("details.TabIndex")));
			this.details.Text = resources.GetString("details.Text");
			this.details.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("details.TextAlign")));
			this.details.Visible = ((bool)(resources.GetObject("details.Visible")));
			this.details.Click += new System.EventHandler(this.details_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.AccessibleDescription = resources.GetString("groupBox2.AccessibleDescription");
			this.groupBox2.AccessibleName = resources.GetString("groupBox2.AccessibleName");
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox2.Anchor")));
			this.groupBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox2.BackgroundImage")));
			this.groupBox2.Controls.Add(this.connect);
			this.groupBox2.Controls.Add(this.password);
			this.groupBox2.Controls.Add(this.server);
			this.groupBox2.Controls.Add(this.userName);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.advanced);
			this.groupBox2.Controls.Add(this.autoLogin);
			this.groupBox2.Controls.Add(this.rememberPassword);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.defaultServer);
			this.groupBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox2.Dock")));
			this.groupBox2.Enabled = ((bool)(resources.GetObject("groupBox2.Enabled")));
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Font = ((System.Drawing.Font)(resources.GetObject("groupBox2.Font")));
			this.groupBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox2.ImeMode")));
			this.groupBox2.Location = ((System.Drawing.Point)(resources.GetObject("groupBox2.Location")));
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox2.RightToLeft")));
			this.groupBox2.Size = ((System.Drawing.Size)(resources.GetObject("groupBox2.Size")));
			this.groupBox2.TabIndex = ((int)(resources.GetObject("groupBox2.TabIndex")));
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = resources.GetString("groupBox2.Text");
			this.groupBox2.Visible = ((bool)(resources.GetObject("groupBox2.Visible")));
			// 
			// connect
			// 
			this.connect.AccessibleDescription = resources.GetString("connect.AccessibleDescription");
			this.connect.AccessibleName = resources.GetString("connect.AccessibleName");
			this.connect.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("connect.Anchor")));
			this.connect.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("connect.BackgroundImage")));
			this.connect.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("connect.Dock")));
			this.connect.Enabled = ((bool)(resources.GetObject("connect.Enabled")));
			this.connect.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("connect.FlatStyle")));
			this.connect.Font = ((System.Drawing.Font)(resources.GetObject("connect.Font")));
			this.connect.Image = ((System.Drawing.Image)(resources.GetObject("connect.Image")));
			this.connect.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("connect.ImageAlign")));
			this.connect.ImageIndex = ((int)(resources.GetObject("connect.ImageIndex")));
			this.connect.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("connect.ImeMode")));
			this.connect.Location = ((System.Drawing.Point)(resources.GetObject("connect.Location")));
			this.connect.Name = "connect";
			this.connect.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("connect.RightToLeft")));
			this.connect.Size = ((System.Drawing.Size)(resources.GetObject("connect.Size")));
			this.connect.TabIndex = ((int)(resources.GetObject("connect.TabIndex")));
			this.connect.Text = resources.GetString("connect.Text");
			this.connect.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("connect.TextAlign")));
			this.connect.Visible = ((bool)(resources.GetObject("connect.Visible")));
			this.connect.Click += new System.EventHandler(this.connect_Click);
			// 
			// password
			// 
			this.password.AccessibleDescription = resources.GetString("password.AccessibleDescription");
			this.password.AccessibleName = resources.GetString("password.AccessibleName");
			this.password.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("password.Anchor")));
			this.password.AutoSize = ((bool)(resources.GetObject("password.AutoSize")));
			this.password.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("password.BackgroundImage")));
			this.password.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("password.Dock")));
			this.password.Enabled = ((bool)(resources.GetObject("password.Enabled")));
			this.password.Font = ((System.Drawing.Font)(resources.GetObject("password.Font")));
			this.password.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("password.ImeMode")));
			this.password.Location = ((System.Drawing.Point)(resources.GetObject("password.Location")));
			this.password.MaxLength = ((int)(resources.GetObject("password.MaxLength")));
			this.password.Multiline = ((bool)(resources.GetObject("password.Multiline")));
			this.password.Name = "password";
			this.password.PasswordChar = ((char)(resources.GetObject("password.PasswordChar")));
			this.password.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("password.RightToLeft")));
			this.password.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("password.ScrollBars")));
			this.password.Size = ((System.Drawing.Size)(resources.GetObject("password.Size")));
			this.password.TabIndex = ((int)(resources.GetObject("password.TabIndex")));
			this.password.Text = resources.GetString("password.Text");
			this.password.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("password.TextAlign")));
			this.password.Visible = ((bool)(resources.GetObject("password.Visible")));
			this.password.WordWrap = ((bool)(resources.GetObject("password.WordWrap")));
			// 
			// server
			// 
			this.server.AccessibleDescription = resources.GetString("server.AccessibleDescription");
			this.server.AccessibleName = resources.GetString("server.AccessibleName");
			this.server.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("server.Anchor")));
			this.server.AutoSize = ((bool)(resources.GetObject("server.AutoSize")));
			this.server.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("server.BackgroundImage")));
			this.server.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("server.Dock")));
			this.server.Enabled = ((bool)(resources.GetObject("server.Enabled")));
			this.server.Font = ((System.Drawing.Font)(resources.GetObject("server.Font")));
			this.server.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("server.ImeMode")));
			this.server.Location = ((System.Drawing.Point)(resources.GetObject("server.Location")));
			this.server.MaxLength = ((int)(resources.GetObject("server.MaxLength")));
			this.server.Multiline = ((bool)(resources.GetObject("server.Multiline")));
			this.server.Name = "server";
			this.server.PasswordChar = ((char)(resources.GetObject("server.PasswordChar")));
			this.server.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("server.RightToLeft")));
			this.server.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("server.ScrollBars")));
			this.server.Size = ((System.Drawing.Size)(resources.GetObject("server.Size")));
			this.server.TabIndex = ((int)(resources.GetObject("server.TabIndex")));
			this.server.Text = resources.GetString("server.Text");
			this.server.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("server.TextAlign")));
			this.server.Visible = ((bool)(resources.GetObject("server.Visible")));
			this.server.WordWrap = ((bool)(resources.GetObject("server.WordWrap")));
			this.server.TextChanged += new System.EventHandler(this.server_TextChanged);
			// 
			// userName
			// 
			this.userName.AccessibleDescription = resources.GetString("userName.AccessibleDescription");
			this.userName.AccessibleName = resources.GetString("userName.AccessibleName");
			this.userName.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("userName.Anchor")));
			this.userName.AutoSize = ((bool)(resources.GetObject("userName.AutoSize")));
			this.userName.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("userName.BackgroundImage")));
			this.userName.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("userName.Dock")));
			this.userName.Enabled = ((bool)(resources.GetObject("userName.Enabled")));
			this.userName.Font = ((System.Drawing.Font)(resources.GetObject("userName.Font")));
			this.userName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userName.ImeMode")));
			this.userName.Location = ((System.Drawing.Point)(resources.GetObject("userName.Location")));
			this.userName.MaxLength = ((int)(resources.GetObject("userName.MaxLength")));
			this.userName.Multiline = ((bool)(resources.GetObject("userName.Multiline")));
			this.userName.Name = "userName";
			this.userName.PasswordChar = ((char)(resources.GetObject("userName.PasswordChar")));
			this.userName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userName.RightToLeft")));
			this.userName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("userName.ScrollBars")));
			this.userName.Size = ((System.Drawing.Size)(resources.GetObject("userName.Size")));
			this.userName.TabIndex = ((int)(resources.GetObject("userName.TabIndex")));
			this.userName.Text = resources.GetString("userName.Text");
			this.userName.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("userName.TextAlign")));
			this.userName.Visible = ((bool)(resources.GetObject("userName.Visible")));
			this.userName.WordWrap = ((bool)(resources.GetObject("userName.WordWrap")));
			this.userName.TextChanged += new System.EventHandler(this.userName_TextChanged);
			// 
			// label9
			// 
			this.label9.AccessibleDescription = resources.GetString("label9.AccessibleDescription");
			this.label9.AccessibleName = resources.GetString("label9.AccessibleName");
			this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label9.Anchor")));
			this.label9.AutoSize = ((bool)(resources.GetObject("label9.AutoSize")));
			this.label9.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label9.Dock")));
			this.label9.Enabled = ((bool)(resources.GetObject("label9.Enabled")));
			this.label9.Font = ((System.Drawing.Font)(resources.GetObject("label9.Font")));
			this.label9.Image = ((System.Drawing.Image)(resources.GetObject("label9.Image")));
			this.label9.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.ImageAlign")));
			this.label9.ImageIndex = ((int)(resources.GetObject("label9.ImageIndex")));
			this.label9.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label9.ImeMode")));
			this.label9.Location = ((System.Drawing.Point)(resources.GetObject("label9.Location")));
			this.label9.Name = "label9";
			this.label9.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label9.RightToLeft")));
			this.label9.Size = ((System.Drawing.Size)(resources.GetObject("label9.Size")));
			this.label9.TabIndex = ((int)(resources.GetObject("label9.TabIndex")));
			this.label9.Text = resources.GetString("label9.Text");
			this.label9.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.TextAlign")));
			this.label9.Visible = ((bool)(resources.GetObject("label9.Visible")));
			// 
			// label5
			// 
			this.label5.AccessibleDescription = resources.GetString("label5.AccessibleDescription");
			this.label5.AccessibleName = resources.GetString("label5.AccessibleName");
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label5.Anchor")));
			this.label5.AutoSize = ((bool)(resources.GetObject("label5.AutoSize")));
			this.label5.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label5.Dock")));
			this.label5.Enabled = ((bool)(resources.GetObject("label5.Enabled")));
			this.label5.Font = ((System.Drawing.Font)(resources.GetObject("label5.Font")));
			this.label5.Image = ((System.Drawing.Image)(resources.GetObject("label5.Image")));
			this.label5.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.ImageAlign")));
			this.label5.ImageIndex = ((int)(resources.GetObject("label5.ImageIndex")));
			this.label5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label5.ImeMode")));
			this.label5.Location = ((System.Drawing.Point)(resources.GetObject("label5.Location")));
			this.label5.Name = "label5";
			this.label5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label5.RightToLeft")));
			this.label5.Size = ((System.Drawing.Size)(resources.GetObject("label5.Size")));
			this.label5.TabIndex = ((int)(resources.GetObject("label5.TabIndex")));
			this.label5.Text = resources.GetString("label5.Text");
			this.label5.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.TextAlign")));
			this.label5.Visible = ((bool)(resources.GetObject("label5.Visible")));
			// 
			// advanced
			// 
			this.advanced.AccessibleDescription = resources.GetString("advanced.AccessibleDescription");
			this.advanced.AccessibleName = resources.GetString("advanced.AccessibleName");
			this.advanced.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("advanced.Anchor")));
			this.advanced.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("advanced.BackgroundImage")));
			this.advanced.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("advanced.Dock")));
			this.advanced.Enabled = ((bool)(resources.GetObject("advanced.Enabled")));
			this.advanced.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("advanced.FlatStyle")));
			this.advanced.Font = ((System.Drawing.Font)(resources.GetObject("advanced.Font")));
			this.advanced.Image = ((System.Drawing.Image)(resources.GetObject("advanced.Image")));
			this.advanced.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("advanced.ImageAlign")));
			this.advanced.ImageIndex = ((int)(resources.GetObject("advanced.ImageIndex")));
			this.advanced.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("advanced.ImeMode")));
			this.advanced.Location = ((System.Drawing.Point)(resources.GetObject("advanced.Location")));
			this.advanced.Name = "advanced";
			this.advanced.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("advanced.RightToLeft")));
			this.advanced.Size = ((System.Drawing.Size)(resources.GetObject("advanced.Size")));
			this.advanced.TabIndex = ((int)(resources.GetObject("advanced.TabIndex")));
			this.advanced.Text = resources.GetString("advanced.Text");
			this.advanced.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("advanced.TextAlign")));
			this.advanced.Visible = ((bool)(resources.GetObject("advanced.Visible")));
			// 
			// autoLogin
			// 
			this.autoLogin.AccessibleDescription = resources.GetString("autoLogin.AccessibleDescription");
			this.autoLogin.AccessibleName = resources.GetString("autoLogin.AccessibleName");
			this.autoLogin.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("autoLogin.Anchor")));
			this.autoLogin.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("autoLogin.Appearance")));
			this.autoLogin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("autoLogin.BackgroundImage")));
			this.autoLogin.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.CheckAlign")));
			this.autoLogin.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("autoLogin.Dock")));
			this.autoLogin.Enabled = ((bool)(resources.GetObject("autoLogin.Enabled")));
			this.autoLogin.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("autoLogin.FlatStyle")));
			this.autoLogin.Font = ((System.Drawing.Font)(resources.GetObject("autoLogin.Font")));
			this.autoLogin.Image = ((System.Drawing.Image)(resources.GetObject("autoLogin.Image")));
			this.autoLogin.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.ImageAlign")));
			this.autoLogin.ImageIndex = ((int)(resources.GetObject("autoLogin.ImageIndex")));
			this.autoLogin.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoLogin.ImeMode")));
			this.autoLogin.Location = ((System.Drawing.Point)(resources.GetObject("autoLogin.Location")));
			this.autoLogin.Name = "autoLogin";
			this.autoLogin.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoLogin.RightToLeft")));
			this.autoLogin.Size = ((System.Drawing.Size)(resources.GetObject("autoLogin.Size")));
			this.autoLogin.TabIndex = ((int)(resources.GetObject("autoLogin.TabIndex")));
			this.autoLogin.Text = resources.GetString("autoLogin.Text");
			this.autoLogin.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoLogin.TextAlign")));
			this.autoLogin.Visible = ((bool)(resources.GetObject("autoLogin.Visible")));
			this.autoLogin.CheckedChanged += new System.EventHandler(this.autoLogin_CheckedChanged);
			// 
			// rememberPassword
			// 
			this.rememberPassword.AccessibleDescription = resources.GetString("rememberPassword.AccessibleDescription");
			this.rememberPassword.AccessibleName = resources.GetString("rememberPassword.AccessibleName");
			this.rememberPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rememberPassword.Anchor")));
			this.rememberPassword.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rememberPassword.Appearance")));
			this.rememberPassword.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rememberPassword.BackgroundImage")));
			this.rememberPassword.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.CheckAlign")));
			this.rememberPassword.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rememberPassword.Dock")));
			this.rememberPassword.Enabled = ((bool)(resources.GetObject("rememberPassword.Enabled")));
			this.rememberPassword.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rememberPassword.FlatStyle")));
			this.rememberPassword.Font = ((System.Drawing.Font)(resources.GetObject("rememberPassword.Font")));
			this.rememberPassword.Image = ((System.Drawing.Image)(resources.GetObject("rememberPassword.Image")));
			this.rememberPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.ImageAlign")));
			this.rememberPassword.ImageIndex = ((int)(resources.GetObject("rememberPassword.ImageIndex")));
			this.rememberPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rememberPassword.ImeMode")));
			this.rememberPassword.Location = ((System.Drawing.Point)(resources.GetObject("rememberPassword.Location")));
			this.rememberPassword.Name = "rememberPassword";
			this.rememberPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rememberPassword.RightToLeft")));
			this.rememberPassword.Size = ((System.Drawing.Size)(resources.GetObject("rememberPassword.Size")));
			this.rememberPassword.TabIndex = ((int)(resources.GetObject("rememberPassword.TabIndex")));
			this.rememberPassword.Text = resources.GetString("rememberPassword.Text");
			this.rememberPassword.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.TextAlign")));
			this.rememberPassword.Visible = ((bool)(resources.GetObject("rememberPassword.Visible")));
			this.rememberPassword.CheckedChanged += new System.EventHandler(this.rememberPassword_CheckedChanged);
			// 
			// label10
			// 
			this.label10.AccessibleDescription = resources.GetString("label10.AccessibleDescription");
			this.label10.AccessibleName = resources.GetString("label10.AccessibleName");
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label10.Anchor")));
			this.label10.AutoSize = ((bool)(resources.GetObject("label10.AutoSize")));
			this.label10.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label10.Dock")));
			this.label10.Enabled = ((bool)(resources.GetObject("label10.Enabled")));
			this.label10.Font = ((System.Drawing.Font)(resources.GetObject("label10.Font")));
			this.label10.Image = ((System.Drawing.Image)(resources.GetObject("label10.Image")));
			this.label10.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.ImageAlign")));
			this.label10.ImageIndex = ((int)(resources.GetObject("label10.ImageIndex")));
			this.label10.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label10.ImeMode")));
			this.label10.Location = ((System.Drawing.Point)(resources.GetObject("label10.Location")));
			this.label10.Name = "label10";
			this.label10.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label10.RightToLeft")));
			this.label10.Size = ((System.Drawing.Size)(resources.GetObject("label10.Size")));
			this.label10.TabIndex = ((int)(resources.GetObject("label10.TabIndex")));
			this.label10.Text = resources.GetString("label10.Text");
			this.label10.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.TextAlign")));
			this.label10.Visible = ((bool)(resources.GetObject("label10.Visible")));
			// 
			// defaultServer
			// 
			this.defaultServer.AccessibleDescription = resources.GetString("defaultServer.AccessibleDescription");
			this.defaultServer.AccessibleName = resources.GetString("defaultServer.AccessibleName");
			this.defaultServer.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("defaultServer.Anchor")));
			this.defaultServer.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("defaultServer.Appearance")));
			this.defaultServer.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("defaultServer.BackgroundImage")));
			this.defaultServer.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.CheckAlign")));
			this.defaultServer.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("defaultServer.Dock")));
			this.defaultServer.Enabled = ((bool)(resources.GetObject("defaultServer.Enabled")));
			this.defaultServer.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("defaultServer.FlatStyle")));
			this.defaultServer.Font = ((System.Drawing.Font)(resources.GetObject("defaultServer.Font")));
			this.defaultServer.Image = ((System.Drawing.Image)(resources.GetObject("defaultServer.Image")));
			this.defaultServer.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.ImageAlign")));
			this.defaultServer.ImageIndex = ((int)(resources.GetObject("defaultServer.ImageIndex")));
			this.defaultServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("defaultServer.ImeMode")));
			this.defaultServer.Location = ((System.Drawing.Point)(resources.GetObject("defaultServer.Location")));
			this.defaultServer.Name = "defaultServer";
			this.defaultServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("defaultServer.RightToLeft")));
			this.defaultServer.Size = ((System.Drawing.Size)(resources.GetObject("defaultServer.Size")));
			this.defaultServer.TabIndex = ((int)(resources.GetObject("defaultServer.TabIndex")));
			this.defaultServer.Text = resources.GetString("defaultServer.Text");
			this.defaultServer.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.TextAlign")));
			this.defaultServer.Visible = ((bool)(resources.GetObject("defaultServer.Visible")));
			this.defaultServer.CheckedChanged += new System.EventHandler(this.defaultServer_CheckedChanged);
			// 
			// removeAccount
			// 
			this.removeAccount.AccessibleDescription = resources.GetString("removeAccount.AccessibleDescription");
			this.removeAccount.AccessibleName = resources.GetString("removeAccount.AccessibleName");
			this.removeAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("removeAccount.Anchor")));
			this.removeAccount.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("removeAccount.BackgroundImage")));
			this.removeAccount.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("removeAccount.Dock")));
			this.removeAccount.Enabled = ((bool)(resources.GetObject("removeAccount.Enabled")));
			this.removeAccount.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("removeAccount.FlatStyle")));
			this.removeAccount.Font = ((System.Drawing.Font)(resources.GetObject("removeAccount.Font")));
			this.removeAccount.Image = ((System.Drawing.Image)(resources.GetObject("removeAccount.Image")));
			this.removeAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("removeAccount.ImageAlign")));
			this.removeAccount.ImageIndex = ((int)(resources.GetObject("removeAccount.ImageIndex")));
			this.removeAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("removeAccount.ImeMode")));
			this.removeAccount.Location = ((System.Drawing.Point)(resources.GetObject("removeAccount.Location")));
			this.removeAccount.Name = "removeAccount";
			this.removeAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("removeAccount.RightToLeft")));
			this.removeAccount.Size = ((System.Drawing.Size)(resources.GetObject("removeAccount.Size")));
			this.removeAccount.TabIndex = ((int)(resources.GetObject("removeAccount.TabIndex")));
			this.removeAccount.Text = resources.GetString("removeAccount.Text");
			this.removeAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("removeAccount.TextAlign")));
			this.removeAccount.Visible = ((bool)(resources.GetObject("removeAccount.Visible")));
			this.removeAccount.Click += new System.EventHandler(this.removeAccount_Click);
			// 
			// addAccount
			// 
			this.addAccount.AccessibleDescription = resources.GetString("addAccount.AccessibleDescription");
			this.addAccount.AccessibleName = resources.GetString("addAccount.AccessibleName");
			this.addAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("addAccount.Anchor")));
			this.addAccount.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("addAccount.BackgroundImage")));
			this.addAccount.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("addAccount.Dock")));
			this.addAccount.Enabled = ((bool)(resources.GetObject("addAccount.Enabled")));
			this.addAccount.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("addAccount.FlatStyle")));
			this.addAccount.Font = ((System.Drawing.Font)(resources.GetObject("addAccount.Font")));
			this.addAccount.Image = ((System.Drawing.Image)(resources.GetObject("addAccount.Image")));
			this.addAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("addAccount.ImageAlign")));
			this.addAccount.ImageIndex = ((int)(resources.GetObject("addAccount.ImageIndex")));
			this.addAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addAccount.ImeMode")));
			this.addAccount.Location = ((System.Drawing.Point)(resources.GetObject("addAccount.Location")));
			this.addAccount.Name = "addAccount";
			this.addAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addAccount.RightToLeft")));
			this.addAccount.Size = ((System.Drawing.Size)(resources.GetObject("addAccount.Size")));
			this.addAccount.TabIndex = ((int)(resources.GetObject("addAccount.TabIndex")));
			this.addAccount.Text = resources.GetString("addAccount.Text");
			this.addAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("addAccount.TextAlign")));
			this.addAccount.Visible = ((bool)(resources.GetObject("addAccount.Visible")));
			this.addAccount.Click += new System.EventHandler(this.addAccount_Click);
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
			// 
			// groupBox4
			// 
			this.groupBox4.AccessibleDescription = resources.GetString("groupBox4.AccessibleDescription");
			this.groupBox4.AccessibleName = resources.GetString("groupBox4.AccessibleName");
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox4.Anchor")));
			this.groupBox4.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox4.BackgroundImage")));
			this.groupBox4.Controls.Add(this.notifyCollisions);
			this.groupBox4.Controls.Add(this.notifyShared);
			this.groupBox4.Controls.Add(this.notifyJoins);
			this.groupBox4.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox4.Dock")));
			this.groupBox4.Enabled = ((bool)(resources.GetObject("groupBox4.Enabled")));
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Font = ((System.Drawing.Font)(resources.GetObject("groupBox4.Font")));
			this.groupBox4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox4.ImeMode")));
			this.groupBox4.Location = ((System.Drawing.Point)(resources.GetObject("groupBox4.Location")));
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox4.RightToLeft")));
			this.groupBox4.Size = ((System.Drawing.Size)(resources.GetObject("groupBox4.Size")));
			this.groupBox4.TabIndex = ((int)(resources.GetObject("groupBox4.TabIndex")));
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = resources.GetString("groupBox4.Text");
			this.groupBox4.Visible = ((bool)(resources.GetObject("groupBox4.Visible")));
			// 
			// Preferences
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.cancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.KeyPreview = true;
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Preferences";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Preferences_Closing);
			this.Load += new System.EventHandler(this.Preferences_Load);
			this.VisibleChanged += new System.EventHandler(this.Preferences_VisibleChanged);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabPage5.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Set when initially connecting to enterprise so that the Servers tab can be updated after the first sync cycle.
		/// </summary>
		public bool InitialConnect
		{
			set { initialConnect = value; }
		}

		/// <summary>
		/// Gets/sets a value indicating if shared iFolder notifications should be displayed.
		/// </summary>
		public bool NotifyShareEnabled
		{
			get
			{
				int notify;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					notify = (int)regKey.GetValue(notifyShareDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (notify == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(notifyShareDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(notifyShareDisabled, 1);
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if iFolder collision notifications should be displayed.
		/// </summary>
		public bool NotifyCollisionEnabled
		{
			get
			{
				int notify;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					notify = (int)regKey.GetValue(notifyCollisionDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (notify == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(notifyCollisionDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(notifyCollisionDisabled, 1);
				}
			}
		}

		/// <summary>
		/// Gets/sets a value indicating if a notification should be displayed when a user joins an iFolder.
		/// </summary>
		public bool NotifyJoinEnabled
		{
			get
			{
				int notify;
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the notify share value ... default the value to 0 (enabled).
					notify = (int)regKey.GetValue(notifyJoinDisabled, 0);
				}
				catch
				{
					return true;
				}

				return (notify == 0);
			}
			set
			{
				// Create/open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

				if (value)
				{
					// Delete the value.
					regKey.DeleteValue(notifyJoinDisabled, false);
				}
				else
				{
					// Set the disable value.
					regKey.SetValue(notifyJoinDisabled, 1);
				}
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Checks to see if auto-run is enabled for the application.
		/// </summary>
		/// <returns><b>True</b> if auto-run is enabled; otherwise, <b>false</b>.</returns>
		static public bool IsRunEnabled()
		{
			int run;

			try
			{
				// Open the iFolder key.
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(iFolderKey);

				// Get the autorun value ... default the value to 0 (enabled).
				run = (int)regKey.GetValue(iFolderRun, 0);
			}
			catch
			{
				return true;
			}

			return (run == 0);
		}

		/// <summary>
		/// Adds the specified domain to the dropdown lists.
		/// </summary>
		/// <param name="domainWeb">The DomainWeb object to add to the list.</param>
		public void AddDomainToList(DomainWeb domainWeb)
		{
			bool domainInList = false;

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.ID.Equals(domainWeb.ID))
				{
					// The domain is already in the list.
					domainInList = true;
				}
			}

			if (!domainInList)
			{
				// Reset the current default domain if the added domain is set to be the default.
				if ((defaultDomain != null) && domainWeb.IsDefault)
				{
					defaultDomain.DomainWeb.IsDefault = false;
				}

				Domain domain = new Domain(domainWeb);

				if (domainWeb.IsDefault)
				{
					defaultDomain = domain;
				}

				try
				{
					iFolderUser ifolderUser = ifWebService.GetiFolderUser(domainWeb.UserID);
					ListViewItem lvi = new ListViewItem(new string[] {ifolderUser.Name, domain.Name});
					lvi.Tag = domain;
					accounts.Items.Add(lvi);
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readUserError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}
		}

		/// <summary>
		/// Check the specified ID to see if it is the current user.
		/// </summary>
		/// <param name="userID">The ID of the user to check.</param>
		/// <returns><b>True</b> if the specified user ID is the current user; otherwise, <b>False</b>.</returns>
		public bool IsCurrentUser(string userID)
		{
			bool result = false;

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.DomainWeb.UserID.Equals(userID))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Check the specified ID to see if it is a POBox that is registered with the client.
		/// </summary>
		/// <param name="poBoxID">The ID of the POBox to check.</param>
		/// <returns><b>True</b> if the specified POBox ID is registered with the client; otherwise, <b>False</b>.</returns>
		public bool IsPOBox(string poBoxID)
		{
			bool result = false;

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.DomainWeb.POBoxID.Equals(poBoxID))
				{
					result = true;
					break;
				}
			}

			return result;
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Set the auto-run value in the Windows registery.
		/// </summary>
		/// <param name="disable"><b>True</b> will disable auto-run.</param>
		private void setAutoRunValue(bool disable)
		{
			// Open/create the iFolder key.
			RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

			if (disable)
			{
				// Set the disable value.
				regKey.SetValue(iFolderRun, 1);
			}
			else
			{
				// Delete the value.
				regKey.DeleteValue(iFolderRun, false);
			}
		}
		#endregion

		#region Event Handlers
		private void Preferences_Load(object sender, System.EventArgs e)
		{
			// Load the application icon and banner image.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch {} // Non-fatal ...
		}

		private void Preferences_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				accounts.Items.Clear();

				DomainWeb[] domains;
				try
				{
					domains = ifWebService.GetDomains();
					foreach (DomainWeb dw in domains)
					{
						AddDomainToList(dw);
					}
				}
				catch{}

				apply.Enabled = cancel.Enabled = false;

				// Update the auto start setting.
				autoStart.Checked = IsRunEnabled();

				notifyShared.Checked = NotifyShareEnabled;
				notifyCollisions.Checked = NotifyCollisionEnabled;
				notifyJoins.Checked = NotifyJoinEnabled;

				// Update the display confirmation setting.
				displayConfirmation.Checked = iFolderComponent.DisplayConfirmationEnabled;

				try
				{
					// Update the default sync interval setting.
					defaultInterval.Value = (decimal)ifWebService.GetDefaultSyncInterval();
					autoSync.Checked = defaultInterval.Value != System.Threading.Timeout.Infinite;
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}

				Activate();
			}
		}

		private void Preferences_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// If we haven't received a shutdown event, hide this dialog and cancel the event.
			if (!shutdown)
			{
				e.Cancel = true;
				Hide();
			}
		}

		#region General Tab
		private void apply_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Check and update auto start setting.
			if (autoStart.Checked != IsRunEnabled())
			{
				setAutoRunValue(!autoStart.Checked);
			}

			NotifyShareEnabled = notifyShared.Checked;
			NotifyCollisionEnabled = notifyCollisions.Checked;
			NotifyJoinEnabled = notifyJoins.Checked;

			// Check and update display confirmation setting.
			iFolderComponent.DisplayConfirmationEnabled = displayConfirmation.Checked;

			try
			{
				// Check and update default sync interval.
				int currentInterval = ifWebService.GetDefaultSyncInterval();
				if ((defaultInterval.Value != (decimal)currentInterval) ||
					(autoSync.Checked != (currentInterval != System.Threading.Timeout.Infinite)))
				{
					try
					{
						// Save the default sync interval.
						ifWebService.SetDefaultSyncInterval(autoSync.Checked ? (int)defaultInterval.Value : System.Threading.Timeout.Infinite);
					}
					catch (Exception ex)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			Cursor.Current = Cursors.Default;
			apply.Enabled = cancel.Enabled = false;
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			// Reset the auto start setting.
			autoStart.Checked = IsRunEnabled();

			notifyShared.Checked = NotifyShareEnabled;
			notifyCollisions.Checked = NotifyCollisionEnabled;
			notifyJoins.Checked = NotifyJoinEnabled;

			// Reset the display confirmation setting.
			displayConfirmation.Checked = iFolderComponent.DisplayConfirmationEnabled;

			// Reset the default sync interval.
			defaultInterval.Value = (decimal)ifWebService.GetDefaultSyncInterval();
			autoSync.Checked = defaultInterval.Value != System.Threading.Timeout.Infinite;

			Cursor.Current = Cursors.Default;
			apply.Enabled = cancel.Enabled = false;
		}

		private void autoStart_CheckedChanged(object sender, System.EventArgs e)
		{
			if (autoStart.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void displayConfirmation_CheckedChanged(object sender, System.EventArgs e)
		{
			if (displayConfirmation.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void autoSync_CheckedChanged(object sender, System.EventArgs e)
		{
			if (autoSync.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}

			defaultInterval.Enabled = autoSync.Checked;
		}

		private void defaultInterval_ValueChanged(object sender, System.EventArgs e)
		{
			if (defaultInterval.Focused)
			{
				if (!defaultInterval.Text.Equals(string.Empty))
				{
					defaultInterval.Value = decimal.Parse(defaultInterval.Text);
				}

				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void notifyShared_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyShared.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void notifyCollisions_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyCollisions.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}

		private void notifyJoins_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyJoins.Focused)
			{
				apply.Enabled = cancel.Enabled = true;
			}
		}
		#endregion

		#region Accounts Tab
		private void defaultServer_CheckedChanged(object sender, System.EventArgs e)
		{
			if (defaultServer.Focused && defaultServer.Checked)
			{
				// Set this domain as the default.
				try
				{
					Domain domain = (Domain)accounts.SelectedItems[0].Tag;
					ifWebService.SetDefaultDomain(domain.DomainWeb.ID);

					// Reset the flag on the current default domain.
					defaultDomain.DomainWeb.IsDefault = false;

					// Set the flag on the new default domain.
					domain.DomainWeb.IsDefault = true;
					defaultDomain = domain;

					// Disable the checkbox so that it cannot be unchecked.
					defaultServer.Enabled = false;
				}
				catch
				{}
			}
		}

		private void addAccount_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = new ListViewItem(new string[] {string.Empty, string.Empty});
			accounts.Items.Add(lvi);
			accounts.SelectedItems.Clear();
			lvi.Selected = true;
		}

		private void userName_TextChanged(object sender, System.EventArgs e)
		{
			ListViewItem lvi = accounts.SelectedItems[0];
			lvi.SubItems[0].Text = userName.Text;
		}

		private void server_TextChanged(object sender, System.EventArgs e)
		{
			ListViewItem lvi = accounts.SelectedItems[0];
			lvi.SubItems[1].Text = server.Text;
		}

		private void removeAccount_Click(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void rememberPassword_CheckedChanged(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void autoLogin_CheckedChanged(object sender, System.EventArgs e)
		{
			// TODO:
		}

		private void accounts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (accounts.SelectedItems.Count == 1)
			{
				ListViewItem lvi = accounts.SelectedItems[0];
				if (lvi != null)
				{
					userName.Text = lvi.SubItems[0].Text;
					server.Text = lvi.SubItems[1].Text;
					password.Text = string.Empty;

					if (lvi.Tag == null)
					{
						// This is a new account.
						userName.ReadOnly = server.ReadOnly = false;
						userName.Enabled = server.Enabled = password.Enabled = 
							rememberPassword.Enabled = autoLogin.Enabled = 
							removeAccount.Enabled = true;
						details.Enabled = defaultServer.Checked = defaultServer.Enabled = false;
						userName.Focus();
					}
					else
					{
						details.Enabled = true;
						userName.ReadOnly = server.ReadOnly = true;

						defaultServer.Checked = ((Domain)lvi.Tag).DomainWeb.IsDefault;
						defaultServer.Enabled = !defaultServer.Checked;

						// TODO: Don't allow the workgroup account to be removed.
						// Don't allow the default account to be removed.
						removeAccount.Enabled = !defaultServer.Checked;

						// TODO: Fill in password with fixed long length of characters if it is currently remembered.
						password.Enabled = true;

						// TODO: set remember password and auto login settings.
					}
				}
			}
			else
			{
				userName.Enabled = server.Enabled = password.Enabled = rememberPassword.Enabled =
					autoLogin.Enabled = defaultServer.Enabled = details.Enabled = 
					removeAccount.Enabled = false;
			}
		}

		private void details_Click(object sender, System.EventArgs e)
		{
			ServerDetails serverDetails = new ServerDetails(this.ifWebService, accounts.Items, (Domain)accounts.SelectedItems[0].Tag);
			serverDetails.ShowDialog();
		}

		private void connect_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				DomainWeb domainWeb = ifWebService.ConnectToDomain(userName.Text, password.Text, server.Text);

				Domain domain = new Domain(domainWeb);
				ListViewItem lvi = accounts.SelectedItems[0];
				lvi.SubItems[1].Text = domainWeb.Name;
				lvi.Tag = domain;

				// Update default.
				if (domainWeb.IsDefault)
				{
					defaultDomain.DomainWeb.IsDefault = false;
					defaultDomain = domain;
					defaultServer.Checked = true;
					defaultServer.Enabled = false;
				}

				// TODO: save state of checkboxes.

				try
				{
					// Check for an update.
					bool updateStarted = FormsTrayApp.CheckForClientUpdate(domainWeb.ID, userName.Text, password.Text);
					if (updateStarted)
					{
						// TODO: Shut down the tray app.
					}

					if (!rememberPassword.Checked)
					{
						password.Text = string.Empty;
					}
				}
				catch (Exception ex)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("checkUpdateError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				if (ex.Message.IndexOf("HTTP status 401") != -1)
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
				else
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			Cursor.Current = Cursors.Default;
		}
		#endregion

		#endregion

		private const int WM_QUERYENDSESSION = 0x0011;

		protected override void WndProc(ref Message m)
		{
			// Keep track if we receive a shutdown message.
			switch (m.Msg)
			{
				case WM_QUERYENDSESSION:
					this.shutdown = true;
					break;
			}

			base.WndProc (ref m);
		}
	}
}
