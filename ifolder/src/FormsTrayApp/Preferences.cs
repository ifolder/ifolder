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
using Microsoft.Win32;
using Novell.iFolderCom;
using Novell.Win32Util;
using Simias.Client;
using Simias.Client.Authentication;
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
		private const decimal defaultMinimumSeconds = 60;
		private const decimal maximumSeconds = int.MaxValue;
		private const decimal maximumMinutes = (decimal)(maximumSeconds / 60);
		private const decimal maximumHours = (decimal)(maximumMinutes / 60);
		private const decimal maximumDays = (decimal)(maximumHours / 24);
		private const string iFolderRun = "DisableAutoStart";
		private const string notifyShareDisabled = "NotifyShareDisable";
		private const string notifyCollisionDisabled = "NotifyCollisionDisabled";
		private const string notifyJoinDisabled = "NotifyJoinDisabled";
		public static readonly string iFolderKey = @"SOFTWARE\Novell\iFolder";
		private const string preferencesX = "PreferencesX";
		private const string preferencesY = "PreferencesY";
		private decimal minimumSyncInterval;
		private decimal minimumSeconds;
		private iFolderWebService ifWebService;
		private SimiasWebService simiasWebService;
		private bool shutdown = false;
		private Domain currentDefaultDomain = null;
		private Domain newDefaultDomain = null;
		private Domain selectedDomain = null;
		private ListViewItem newAccountLvi = null;
		private bool processing = false;
		private bool successful;
		private bool updatePassword = false;
		private bool updateEnabled = false;
		private bool initialPositionSet = false;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.CheckBox autoStart;
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
		private System.Windows.Forms.Button details;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox userName;
		private System.Windows.Forms.TextBox server;
		private System.Windows.Forms.TextBox password;
		private System.Windows.Forms.ListView accounts;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabAccounts;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Button proxy;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.CheckBox enableAccount;
		private System.Windows.Forms.Button activate;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.ComboBox timeUnit;
		private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// Instantiates a Preferences object.
		/// </summary>
		public Preferences(iFolderWebService ifolderWebService, SimiasWebService simiasWebService)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			defaultInterval.TextChanged += new EventHandler(defaultInterval_ValueChanged);

			ifWebService = ifolderWebService;
			this.simiasWebService = simiasWebService;

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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Preferences));
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.notifyCollisions = new System.Windows.Forms.CheckBox();
			this.notifyShared = new System.Windows.Forms.CheckBox();
			this.notifyJoins = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.timeUnit = new System.Windows.Forms.ComboBox();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.autoStart = new System.Windows.Forms.CheckBox();
			this.tabAccounts = new System.Windows.Forms.TabPage();
			this.accounts = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.details = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.activate = new System.Windows.Forms.Button();
			this.password = new System.Windows.Forms.TextBox();
			this.server = new System.Windows.Forms.TextBox();
			this.userName = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.proxy = new System.Windows.Forms.Button();
			this.enableAccount = new System.Windows.Forms.CheckBox();
			this.rememberPassword = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.defaultServer = new System.Windows.Forms.CheckBox();
			this.removeAccount = new System.Windows.Forms.Button();
			this.addAccount = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.ok = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabAccounts.SuspendLayout();
			this.groupBox2.SuspendLayout();
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
			this.helpProvider1.SetHelpKeyword(this.defaultInterval, resources.GetString("defaultInterval.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.defaultInterval, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("defaultInterval.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.defaultInterval, resources.GetString("defaultInterval.HelpString"));
			this.defaultInterval.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("defaultInterval.ImeMode")));
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = ((System.Drawing.Point)(resources.GetObject("defaultInterval.Location")));
			this.defaultInterval.Maximum = new System.Decimal(new int[] {
																			2147483647,
																			0,
																			0,
																			0});
			this.defaultInterval.Name = "defaultInterval";
			this.defaultInterval.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("defaultInterval.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.defaultInterval, ((bool)(resources.GetObject("defaultInterval.ShowHelp"))));
			this.defaultInterval.Size = ((System.Drawing.Size)(resources.GetObject("defaultInterval.Size")));
			this.defaultInterval.TabIndex = ((int)(resources.GetObject("defaultInterval.TabIndex")));
			this.defaultInterval.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("defaultInterval.TextAlign")));
			this.defaultInterval.ThousandsSeparator = ((bool)(resources.GetObject("defaultInterval.ThousandsSeparator")));
			this.defaultInterval.UpDownAlign = ((System.Windows.Forms.LeftRightAlignment)(resources.GetObject("defaultInterval.UpDownAlign")));
			this.defaultInterval.Value = new System.Decimal(new int[] {
																		  60,
																		  0,
																		  0,
																		  0});
			this.defaultInterval.Visible = ((bool)(resources.GetObject("defaultInterval.Visible")));
			this.defaultInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.defaultInterval_KeyDown);
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
			this.helpProvider1.SetHelpKeyword(this.displayConfirmation, resources.GetString("displayConfirmation.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.displayConfirmation, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("displayConfirmation.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.displayConfirmation, resources.GetString("displayConfirmation.HelpString"));
			this.displayConfirmation.Image = ((System.Drawing.Image)(resources.GetObject("displayConfirmation.Image")));
			this.displayConfirmation.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.ImageAlign")));
			this.displayConfirmation.ImageIndex = ((int)(resources.GetObject("displayConfirmation.ImageIndex")));
			this.displayConfirmation.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("displayConfirmation.ImeMode")));
			this.displayConfirmation.Location = ((System.Drawing.Point)(resources.GetObject("displayConfirmation.Location")));
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("displayConfirmation.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.displayConfirmation, ((bool)(resources.GetObject("displayConfirmation.ShowHelp"))));
			this.displayConfirmation.Size = ((System.Drawing.Size)(resources.GetObject("displayConfirmation.Size")));
			this.displayConfirmation.TabIndex = ((int)(resources.GetObject("displayConfirmation.TabIndex")));
			this.displayConfirmation.Text = resources.GetString("displayConfirmation.Text");
			this.displayConfirmation.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("displayConfirmation.TextAlign")));
			this.displayConfirmation.Visible = ((bool)(resources.GetObject("displayConfirmation.Visible")));
			this.displayConfirmation.CheckedChanged += new System.EventHandler(this.displayConfirmation_CheckedChanged);
			// 
			// tabControl1
			// 
			this.tabControl1.AccessibleDescription = resources.GetString("tabControl1.AccessibleDescription");
			this.tabControl1.AccessibleName = resources.GetString("tabControl1.AccessibleName");
			this.tabControl1.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabControl1.Alignment")));
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabControl1.Anchor")));
			this.tabControl1.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabControl1.Appearance")));
			this.tabControl1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabControl1.BackgroundImage")));
			this.tabControl1.Controls.Add(this.tabGeneral);
			this.tabControl1.Controls.Add(this.tabAccounts);
			this.tabControl1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabControl1.Dock")));
			this.tabControl1.Enabled = ((bool)(resources.GetObject("tabControl1.Enabled")));
			this.tabControl1.Font = ((System.Drawing.Font)(resources.GetObject("tabControl1.Font")));
			this.helpProvider1.SetHelpKeyword(this.tabControl1, resources.GetString("tabControl1.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.tabControl1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabControl1.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.tabControl1, resources.GetString("tabControl1.HelpString"));
			this.tabControl1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabControl1.ImeMode")));
			this.tabControl1.ItemSize = ((System.Drawing.Size)(resources.GetObject("tabControl1.ItemSize")));
			this.tabControl1.Location = ((System.Drawing.Point)(resources.GetObject("tabControl1.Location")));
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Padding = ((System.Drawing.Point)(resources.GetObject("tabControl1.Padding")));
			this.tabControl1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabControl1.RightToLeft")));
			this.tabControl1.SelectedIndex = 0;
			this.helpProvider1.SetShowHelp(this.tabControl1, ((bool)(resources.GetObject("tabControl1.ShowHelp"))));
			this.tabControl1.ShowToolTips = ((bool)(resources.GetObject("tabControl1.ShowToolTips")));
			this.tabControl1.Size = ((System.Drawing.Size)(resources.GetObject("tabControl1.Size")));
			this.tabControl1.TabIndex = ((int)(resources.GetObject("tabControl1.TabIndex")));
			this.tabControl1.Text = resources.GetString("tabControl1.Text");
			this.tabControl1.Visible = ((bool)(resources.GetObject("tabControl1.Visible")));
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabGeneral
			// 
			this.tabGeneral.AccessibleDescription = resources.GetString("tabGeneral.AccessibleDescription");
			this.tabGeneral.AccessibleName = resources.GetString("tabGeneral.AccessibleName");
			this.tabGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabGeneral.Anchor")));
			this.tabGeneral.AutoScroll = ((bool)(resources.GetObject("tabGeneral.AutoScroll")));
			this.tabGeneral.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabGeneral.AutoScrollMargin")));
			this.tabGeneral.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabGeneral.AutoScrollMinSize")));
			this.tabGeneral.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabGeneral.BackgroundImage")));
			this.tabGeneral.Controls.Add(this.groupBox4);
			this.tabGeneral.Controls.Add(this.groupBox1);
			this.tabGeneral.Controls.Add(this.groupBox3);
			this.tabGeneral.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabGeneral.Dock")));
			this.tabGeneral.Enabled = ((bool)(resources.GetObject("tabGeneral.Enabled")));
			this.tabGeneral.Font = ((System.Drawing.Font)(resources.GetObject("tabGeneral.Font")));
			this.helpProvider1.SetHelpKeyword(this.tabGeneral, resources.GetString("tabGeneral.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.tabGeneral, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabGeneral.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.tabGeneral, resources.GetString("tabGeneral.HelpString"));
			this.tabGeneral.ImageIndex = ((int)(resources.GetObject("tabGeneral.ImageIndex")));
			this.tabGeneral.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabGeneral.ImeMode")));
			this.tabGeneral.Location = ((System.Drawing.Point)(resources.GetObject("tabGeneral.Location")));
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabGeneral.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.tabGeneral, ((bool)(resources.GetObject("tabGeneral.ShowHelp"))));
			this.tabGeneral.Size = ((System.Drawing.Size)(resources.GetObject("tabGeneral.Size")));
			this.tabGeneral.TabIndex = ((int)(resources.GetObject("tabGeneral.TabIndex")));
			this.tabGeneral.Text = resources.GetString("tabGeneral.Text");
			this.tabGeneral.ToolTipText = resources.GetString("tabGeneral.ToolTipText");
			this.tabGeneral.Visible = ((bool)(resources.GetObject("tabGeneral.Visible")));
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
			this.helpProvider1.SetHelpKeyword(this.groupBox4, resources.GetString("groupBox4.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.groupBox4, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("groupBox4.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.groupBox4, resources.GetString("groupBox4.HelpString"));
			this.groupBox4.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox4.ImeMode")));
			this.groupBox4.Location = ((System.Drawing.Point)(resources.GetObject("groupBox4.Location")));
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox4.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.groupBox4, ((bool)(resources.GetObject("groupBox4.ShowHelp"))));
			this.groupBox4.Size = ((System.Drawing.Size)(resources.GetObject("groupBox4.Size")));
			this.groupBox4.TabIndex = ((int)(resources.GetObject("groupBox4.TabIndex")));
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = resources.GetString("groupBox4.Text");
			this.groupBox4.Visible = ((bool)(resources.GetObject("groupBox4.Visible")));
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
			this.helpProvider1.SetHelpKeyword(this.notifyCollisions, resources.GetString("notifyCollisions.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.notifyCollisions, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("notifyCollisions.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.notifyCollisions, resources.GetString("notifyCollisions.HelpString"));
			this.notifyCollisions.Image = ((System.Drawing.Image)(resources.GetObject("notifyCollisions.Image")));
			this.notifyCollisions.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyCollisions.ImageAlign")));
			this.notifyCollisions.ImageIndex = ((int)(resources.GetObject("notifyCollisions.ImageIndex")));
			this.notifyCollisions.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyCollisions.ImeMode")));
			this.notifyCollisions.Location = ((System.Drawing.Point)(resources.GetObject("notifyCollisions.Location")));
			this.notifyCollisions.Name = "notifyCollisions";
			this.notifyCollisions.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyCollisions.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.notifyCollisions, ((bool)(resources.GetObject("notifyCollisions.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.notifyShared, resources.GetString("notifyShared.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.notifyShared, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("notifyShared.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.notifyShared, resources.GetString("notifyShared.HelpString"));
			this.notifyShared.Image = ((System.Drawing.Image)(resources.GetObject("notifyShared.Image")));
			this.notifyShared.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.ImageAlign")));
			this.notifyShared.ImageIndex = ((int)(resources.GetObject("notifyShared.ImageIndex")));
			this.notifyShared.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyShared.ImeMode")));
			this.notifyShared.Location = ((System.Drawing.Point)(resources.GetObject("notifyShared.Location")));
			this.notifyShared.Name = "notifyShared";
			this.notifyShared.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyShared.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.notifyShared, ((bool)(resources.GetObject("notifyShared.ShowHelp"))));
			this.notifyShared.Size = ((System.Drawing.Size)(resources.GetObject("notifyShared.Size")));
			this.notifyShared.TabIndex = ((int)(resources.GetObject("notifyShared.TabIndex")));
			this.notifyShared.Text = resources.GetString("notifyShared.Text");
			this.notifyShared.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyShared.TextAlign")));
			this.notifyShared.Visible = ((bool)(resources.GetObject("notifyShared.Visible")));
			this.notifyShared.CheckedChanged += new System.EventHandler(this.notifyShared_CheckedChanged);
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
			this.helpProvider1.SetHelpKeyword(this.notifyJoins, resources.GetString("notifyJoins.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.notifyJoins, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("notifyJoins.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.notifyJoins, resources.GetString("notifyJoins.HelpString"));
			this.notifyJoins.Image = ((System.Drawing.Image)(resources.GetObject("notifyJoins.Image")));
			this.notifyJoins.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.ImageAlign")));
			this.notifyJoins.ImageIndex = ((int)(resources.GetObject("notifyJoins.ImageIndex")));
			this.notifyJoins.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("notifyJoins.ImeMode")));
			this.notifyJoins.Location = ((System.Drawing.Point)(resources.GetObject("notifyJoins.Location")));
			this.notifyJoins.Name = "notifyJoins";
			this.notifyJoins.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("notifyJoins.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.notifyJoins, ((bool)(resources.GetObject("notifyJoins.ShowHelp"))));
			this.notifyJoins.Size = ((System.Drawing.Size)(resources.GetObject("notifyJoins.Size")));
			this.notifyJoins.TabIndex = ((int)(resources.GetObject("notifyJoins.TabIndex")));
			this.notifyJoins.Text = resources.GetString("notifyJoins.Text");
			this.notifyJoins.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("notifyJoins.TextAlign")));
			this.notifyJoins.Visible = ((bool)(resources.GetObject("notifyJoins.Visible")));
			this.notifyJoins.CheckedChanged += new System.EventHandler(this.notifyJoins_CheckedChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = resources.GetString("groupBox1.AccessibleDescription");
			this.groupBox1.AccessibleName = resources.GetString("groupBox1.AccessibleName");
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("groupBox1.Anchor")));
			this.groupBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox1.BackgroundImage")));
			this.groupBox1.Controls.Add(this.timeUnit);
			this.groupBox1.Controls.Add(this.defaultInterval);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox1.Dock")));
			this.groupBox1.Enabled = ((bool)(resources.GetObject("groupBox1.Enabled")));
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Font = ((System.Drawing.Font)(resources.GetObject("groupBox1.Font")));
			this.helpProvider1.SetHelpKeyword(this.groupBox1, resources.GetString("groupBox1.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.groupBox1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("groupBox1.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.groupBox1, resources.GetString("groupBox1.HelpString"));
			this.groupBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox1.ImeMode")));
			this.groupBox1.Location = ((System.Drawing.Point)(resources.GetObject("groupBox1.Location")));
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox1.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.groupBox1, ((bool)(resources.GetObject("groupBox1.ShowHelp"))));
			this.groupBox1.Size = ((System.Drawing.Size)(resources.GetObject("groupBox1.Size")));
			this.groupBox1.TabIndex = ((int)(resources.GetObject("groupBox1.TabIndex")));
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = resources.GetString("groupBox1.Text");
			this.groupBox1.Visible = ((bool)(resources.GetObject("groupBox1.Visible")));
			// 
			// timeUnit
			// 
			this.timeUnit.AccessibleDescription = resources.GetString("timeUnit.AccessibleDescription");
			this.timeUnit.AccessibleName = resources.GetString("timeUnit.AccessibleName");
			this.timeUnit.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("timeUnit.Anchor")));
			this.timeUnit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("timeUnit.BackgroundImage")));
			this.timeUnit.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("timeUnit.Dock")));
			this.timeUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.timeUnit.Enabled = ((bool)(resources.GetObject("timeUnit.Enabled")));
			this.timeUnit.Font = ((System.Drawing.Font)(resources.GetObject("timeUnit.Font")));
			this.helpProvider1.SetHelpKeyword(this.timeUnit, resources.GetString("timeUnit.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.timeUnit, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("timeUnit.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.timeUnit, resources.GetString("timeUnit.HelpString"));
			this.timeUnit.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("timeUnit.ImeMode")));
			this.timeUnit.IntegralHeight = ((bool)(resources.GetObject("timeUnit.IntegralHeight")));
			this.timeUnit.ItemHeight = ((int)(resources.GetObject("timeUnit.ItemHeight")));
			this.timeUnit.Location = ((System.Drawing.Point)(resources.GetObject("timeUnit.Location")));
			this.timeUnit.MaxDropDownItems = ((int)(resources.GetObject("timeUnit.MaxDropDownItems")));
			this.timeUnit.MaxLength = ((int)(resources.GetObject("timeUnit.MaxLength")));
			this.timeUnit.Name = "timeUnit";
			this.timeUnit.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("timeUnit.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.timeUnit, ((bool)(resources.GetObject("timeUnit.ShowHelp"))));
			this.timeUnit.Size = ((System.Drawing.Size)(resources.GetObject("timeUnit.Size")));
			this.timeUnit.TabIndex = ((int)(resources.GetObject("timeUnit.TabIndex")));
			this.timeUnit.Text = resources.GetString("timeUnit.Text");
			this.timeUnit.Visible = ((bool)(resources.GetObject("timeUnit.Visible")));
			this.timeUnit.SelectedIndexChanged += new System.EventHandler(this.timeUnit_SelectedIndexChanged);
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
			this.helpProvider1.SetHelpKeyword(this.autoSync, resources.GetString("autoSync.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.autoSync, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("autoSync.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.autoSync, resources.GetString("autoSync.HelpString"));
			this.autoSync.Image = ((System.Drawing.Image)(resources.GetObject("autoSync.Image")));
			this.autoSync.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.ImageAlign")));
			this.autoSync.ImageIndex = ((int)(resources.GetObject("autoSync.ImageIndex")));
			this.autoSync.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoSync.ImeMode")));
			this.autoSync.Location = ((System.Drawing.Point)(resources.GetObject("autoSync.Location")));
			this.autoSync.Name = "autoSync";
			this.autoSync.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoSync.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.autoSync, ((bool)(resources.GetObject("autoSync.ShowHelp"))));
			this.autoSync.Size = ((System.Drawing.Size)(resources.GetObject("autoSync.Size")));
			this.autoSync.TabIndex = ((int)(resources.GetObject("autoSync.TabIndex")));
			this.autoSync.Text = resources.GetString("autoSync.Text");
			this.autoSync.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoSync.TextAlign")));
			this.autoSync.Visible = ((bool)(resources.GetObject("autoSync.Visible")));
			this.autoSync.CheckedChanged += new System.EventHandler(this.autoSync_CheckedChanged);
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
			this.helpProvider1.SetHelpKeyword(this.groupBox3, resources.GetString("groupBox3.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.groupBox3, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("groupBox3.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.groupBox3, resources.GetString("groupBox3.HelpString"));
			this.groupBox3.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox3.ImeMode")));
			this.groupBox3.Location = ((System.Drawing.Point)(resources.GetObject("groupBox3.Location")));
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox3.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.groupBox3, ((bool)(resources.GetObject("groupBox3.ShowHelp"))));
			this.groupBox3.Size = ((System.Drawing.Size)(resources.GetObject("groupBox3.Size")));
			this.groupBox3.TabIndex = ((int)(resources.GetObject("groupBox3.TabIndex")));
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = resources.GetString("groupBox3.Text");
			this.groupBox3.Visible = ((bool)(resources.GetObject("groupBox3.Visible")));
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
			this.helpProvider1.SetHelpKeyword(this.autoStart, resources.GetString("autoStart.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.autoStart, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("autoStart.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.autoStart, resources.GetString("autoStart.HelpString"));
			this.autoStart.Image = ((System.Drawing.Image)(resources.GetObject("autoStart.Image")));
			this.autoStart.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.ImageAlign")));
			this.autoStart.ImageIndex = ((int)(resources.GetObject("autoStart.ImageIndex")));
			this.autoStart.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("autoStart.ImeMode")));
			this.autoStart.Location = ((System.Drawing.Point)(resources.GetObject("autoStart.Location")));
			this.autoStart.Name = "autoStart";
			this.autoStart.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("autoStart.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.autoStart, ((bool)(resources.GetObject("autoStart.ShowHelp"))));
			this.autoStart.Size = ((System.Drawing.Size)(resources.GetObject("autoStart.Size")));
			this.autoStart.TabIndex = ((int)(resources.GetObject("autoStart.TabIndex")));
			this.autoStart.Text = resources.GetString("autoStart.Text");
			this.autoStart.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("autoStart.TextAlign")));
			this.autoStart.Visible = ((bool)(resources.GetObject("autoStart.Visible")));
			this.autoStart.CheckedChanged += new System.EventHandler(this.autoStart_CheckedChanged);
			// 
			// tabAccounts
			// 
			this.tabAccounts.AccessibleDescription = resources.GetString("tabAccounts.AccessibleDescription");
			this.tabAccounts.AccessibleName = resources.GetString("tabAccounts.AccessibleName");
			this.tabAccounts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabAccounts.Anchor")));
			this.tabAccounts.AutoScroll = ((bool)(resources.GetObject("tabAccounts.AutoScroll")));
			this.tabAccounts.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabAccounts.AutoScrollMargin")));
			this.tabAccounts.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabAccounts.AutoScrollMinSize")));
			this.tabAccounts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabAccounts.BackgroundImage")));
			this.tabAccounts.Controls.Add(this.accounts);
			this.tabAccounts.Controls.Add(this.details);
			this.tabAccounts.Controls.Add(this.groupBox2);
			this.tabAccounts.Controls.Add(this.removeAccount);
			this.tabAccounts.Controls.Add(this.addAccount);
			this.tabAccounts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabAccounts.Dock")));
			this.tabAccounts.Enabled = ((bool)(resources.GetObject("tabAccounts.Enabled")));
			this.tabAccounts.Font = ((System.Drawing.Font)(resources.GetObject("tabAccounts.Font")));
			this.helpProvider1.SetHelpKeyword(this.tabAccounts, resources.GetString("tabAccounts.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.tabAccounts, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabAccounts.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.tabAccounts, resources.GetString("tabAccounts.HelpString"));
			this.tabAccounts.ImageIndex = ((int)(resources.GetObject("tabAccounts.ImageIndex")));
			this.tabAccounts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabAccounts.ImeMode")));
			this.tabAccounts.Location = ((System.Drawing.Point)(resources.GetObject("tabAccounts.Location")));
			this.tabAccounts.Name = "tabAccounts";
			this.tabAccounts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabAccounts.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.tabAccounts, ((bool)(resources.GetObject("tabAccounts.ShowHelp"))));
			this.tabAccounts.Size = ((System.Drawing.Size)(resources.GetObject("tabAccounts.Size")));
			this.tabAccounts.TabIndex = ((int)(resources.GetObject("tabAccounts.TabIndex")));
			this.tabAccounts.Text = resources.GetString("tabAccounts.Text");
			this.tabAccounts.ToolTipText = resources.GetString("tabAccounts.ToolTipText");
			this.tabAccounts.Visible = ((bool)(resources.GetObject("tabAccounts.Visible")));
			// 
			// accounts
			// 
			this.accounts.AccessibleDescription = resources.GetString("accounts.AccessibleDescription");
			this.accounts.AccessibleName = resources.GetString("accounts.AccessibleName");
			this.accounts.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("accounts.Alignment")));
			this.accounts.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accounts.Anchor")));
			this.accounts.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("accounts.BackgroundImage")));
			this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader3,
																					   this.columnHeader2,
																					   this.columnHeader1});
			this.accounts.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accounts.Dock")));
			this.accounts.Enabled = ((bool)(resources.GetObject("accounts.Enabled")));
			this.accounts.Font = ((System.Drawing.Font)(resources.GetObject("accounts.Font")));
			this.accounts.FullRowSelect = true;
			this.helpProvider1.SetHelpKeyword(this.accounts, resources.GetString("accounts.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.accounts, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("accounts.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.accounts, resources.GetString("accounts.HelpString"));
			this.accounts.HideSelection = false;
			this.accounts.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accounts.ImeMode")));
			this.accounts.LabelWrap = ((bool)(resources.GetObject("accounts.LabelWrap")));
			this.accounts.Location = ((System.Drawing.Point)(resources.GetObject("accounts.Location")));
			this.accounts.MultiSelect = false;
			this.accounts.Name = "accounts";
			this.accounts.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accounts.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.accounts, ((bool)(resources.GetObject("accounts.ShowHelp"))));
			this.accounts.Size = ((System.Drawing.Size)(resources.GetObject("accounts.Size")));
			this.accounts.TabIndex = ((int)(resources.GetObject("accounts.TabIndex")));
			this.accounts.Text = resources.GetString("accounts.Text");
			this.accounts.View = System.Windows.Forms.View.Details;
			this.accounts.Visible = ((bool)(resources.GetObject("accounts.Visible")));
			this.accounts.DoubleClick += new System.EventHandler(this.details_Click);
			this.accounts.SelectedIndexChanged += new System.EventHandler(this.accounts_SelectedIndexChanged);
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = resources.GetString("columnHeader3.Text");
			this.columnHeader3.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader3.TextAlign")));
			this.columnHeader3.Width = ((int)(resources.GetObject("columnHeader3.Width")));
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = resources.GetString("columnHeader2.Text");
			this.columnHeader2.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader2.TextAlign")));
			this.columnHeader2.Width = ((int)(resources.GetObject("columnHeader2.Width")));
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = resources.GetString("columnHeader1.Text");
			this.columnHeader1.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader1.TextAlign")));
			this.columnHeader1.Width = ((int)(resources.GetObject("columnHeader1.Width")));
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
			this.helpProvider1.SetHelpKeyword(this.details, resources.GetString("details.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.details, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("details.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.details, resources.GetString("details.HelpString"));
			this.details.Image = ((System.Drawing.Image)(resources.GetObject("details.Image")));
			this.details.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("details.ImageAlign")));
			this.details.ImageIndex = ((int)(resources.GetObject("details.ImageIndex")));
			this.details.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("details.ImeMode")));
			this.details.Location = ((System.Drawing.Point)(resources.GetObject("details.Location")));
			this.details.Name = "details";
			this.details.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("details.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.details, ((bool)(resources.GetObject("details.ShowHelp"))));
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
			this.groupBox2.Controls.Add(this.activate);
			this.groupBox2.Controls.Add(this.password);
			this.groupBox2.Controls.Add(this.server);
			this.groupBox2.Controls.Add(this.userName);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.proxy);
			this.groupBox2.Controls.Add(this.enableAccount);
			this.groupBox2.Controls.Add(this.rememberPassword);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.defaultServer);
			this.groupBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("groupBox2.Dock")));
			this.groupBox2.Enabled = ((bool)(resources.GetObject("groupBox2.Enabled")));
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Font = ((System.Drawing.Font)(resources.GetObject("groupBox2.Font")));
			this.helpProvider1.SetHelpKeyword(this.groupBox2, resources.GetString("groupBox2.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.groupBox2, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("groupBox2.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.groupBox2, resources.GetString("groupBox2.HelpString"));
			this.groupBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("groupBox2.ImeMode")));
			this.groupBox2.Location = ((System.Drawing.Point)(resources.GetObject("groupBox2.Location")));
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("groupBox2.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.groupBox2, ((bool)(resources.GetObject("groupBox2.ShowHelp"))));
			this.groupBox2.Size = ((System.Drawing.Size)(resources.GetObject("groupBox2.Size")));
			this.groupBox2.TabIndex = ((int)(resources.GetObject("groupBox2.TabIndex")));
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = resources.GetString("groupBox2.Text");
			this.groupBox2.Visible = ((bool)(resources.GetObject("groupBox2.Visible")));
			// 
			// activate
			// 
			this.activate.AccessibleDescription = resources.GetString("activate.AccessibleDescription");
			this.activate.AccessibleName = resources.GetString("activate.AccessibleName");
			this.activate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("activate.Anchor")));
			this.activate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("activate.BackgroundImage")));
			this.activate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("activate.Dock")));
			this.activate.Enabled = ((bool)(resources.GetObject("activate.Enabled")));
			this.activate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("activate.FlatStyle")));
			this.activate.Font = ((System.Drawing.Font)(resources.GetObject("activate.Font")));
			this.helpProvider1.SetHelpKeyword(this.activate, resources.GetString("activate.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.activate, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("activate.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.activate, resources.GetString("activate.HelpString"));
			this.activate.Image = ((System.Drawing.Image)(resources.GetObject("activate.Image")));
			this.activate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("activate.ImageAlign")));
			this.activate.ImageIndex = ((int)(resources.GetObject("activate.ImageIndex")));
			this.activate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("activate.ImeMode")));
			this.activate.Location = ((System.Drawing.Point)(resources.GetObject("activate.Location")));
			this.activate.Name = "activate";
			this.activate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("activate.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.activate, ((bool)(resources.GetObject("activate.ShowHelp"))));
			this.activate.Size = ((System.Drawing.Size)(resources.GetObject("activate.Size")));
			this.activate.TabIndex = ((int)(resources.GetObject("activate.TabIndex")));
			this.activate.Text = resources.GetString("activate.Text");
			this.activate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("activate.TextAlign")));
			this.activate.Visible = ((bool)(resources.GetObject("activate.Visible")));
			this.activate.Click += new System.EventHandler(this.activate_Click);
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
			this.helpProvider1.SetHelpKeyword(this.password, resources.GetString("password.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.password, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("password.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.password, resources.GetString("password.HelpString"));
			this.password.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("password.ImeMode")));
			this.password.Location = ((System.Drawing.Point)(resources.GetObject("password.Location")));
			this.password.MaxLength = ((int)(resources.GetObject("password.MaxLength")));
			this.password.Multiline = ((bool)(resources.GetObject("password.Multiline")));
			this.password.Name = "password";
			this.password.PasswordChar = ((char)(resources.GetObject("password.PasswordChar")));
			this.password.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("password.RightToLeft")));
			this.password.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("password.ScrollBars")));
			this.helpProvider1.SetShowHelp(this.password, ((bool)(resources.GetObject("password.ShowHelp"))));
			this.password.Size = ((System.Drawing.Size)(resources.GetObject("password.Size")));
			this.password.TabIndex = ((int)(resources.GetObject("password.TabIndex")));
			this.password.Text = resources.GetString("password.Text");
			this.password.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("password.TextAlign")));
			this.password.Visible = ((bool)(resources.GetObject("password.Visible")));
			this.password.WordWrap = ((bool)(resources.GetObject("password.WordWrap")));
			this.password.TextChanged += new System.EventHandler(this.password_TextChanged);
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
			this.helpProvider1.SetHelpKeyword(this.server, resources.GetString("server.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.server, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("server.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.server, resources.GetString("server.HelpString"));
			this.server.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("server.ImeMode")));
			this.server.Location = ((System.Drawing.Point)(resources.GetObject("server.Location")));
			this.server.MaxLength = ((int)(resources.GetObject("server.MaxLength")));
			this.server.Multiline = ((bool)(resources.GetObject("server.Multiline")));
			this.server.Name = "server";
			this.server.PasswordChar = ((char)(resources.GetObject("server.PasswordChar")));
			this.server.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("server.RightToLeft")));
			this.server.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("server.ScrollBars")));
			this.helpProvider1.SetShowHelp(this.server, ((bool)(resources.GetObject("server.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.userName, resources.GetString("userName.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.userName, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("userName.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.userName, resources.GetString("userName.HelpString"));
			this.userName.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("userName.ImeMode")));
			this.userName.Location = ((System.Drawing.Point)(resources.GetObject("userName.Location")));
			this.userName.MaxLength = ((int)(resources.GetObject("userName.MaxLength")));
			this.userName.Multiline = ((bool)(resources.GetObject("userName.Multiline")));
			this.userName.Name = "userName";
			this.userName.PasswordChar = ((char)(resources.GetObject("userName.PasswordChar")));
			this.userName.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("userName.RightToLeft")));
			this.userName.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("userName.ScrollBars")));
			this.helpProvider1.SetShowHelp(this.userName, ((bool)(resources.GetObject("userName.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.label9, resources.GetString("label9.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label9, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label9.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label9, resources.GetString("label9.HelpString"));
			this.label9.Image = ((System.Drawing.Image)(resources.GetObject("label9.Image")));
			this.label9.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label9.ImageAlign")));
			this.label9.ImageIndex = ((int)(resources.GetObject("label9.ImageIndex")));
			this.label9.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label9.ImeMode")));
			this.label9.Location = ((System.Drawing.Point)(resources.GetObject("label9.Location")));
			this.label9.Name = "label9";
			this.label9.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label9.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label9, ((bool)(resources.GetObject("label9.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.label5, resources.GetString("label5.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label5, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label5.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label5, resources.GetString("label5.HelpString"));
			this.label5.Image = ((System.Drawing.Image)(resources.GetObject("label5.Image")));
			this.label5.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.ImageAlign")));
			this.label5.ImageIndex = ((int)(resources.GetObject("label5.ImageIndex")));
			this.label5.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label5.ImeMode")));
			this.label5.Location = ((System.Drawing.Point)(resources.GetObject("label5.Location")));
			this.label5.Name = "label5";
			this.label5.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label5.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label5, ((bool)(resources.GetObject("label5.ShowHelp"))));
			this.label5.Size = ((System.Drawing.Size)(resources.GetObject("label5.Size")));
			this.label5.TabIndex = ((int)(resources.GetObject("label5.TabIndex")));
			this.label5.Text = resources.GetString("label5.Text");
			this.label5.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label5.TextAlign")));
			this.label5.Visible = ((bool)(resources.GetObject("label5.Visible")));
			// 
			// proxy
			// 
			this.proxy.AccessibleDescription = resources.GetString("proxy.AccessibleDescription");
			this.proxy.AccessibleName = resources.GetString("proxy.AccessibleName");
			this.proxy.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("proxy.Anchor")));
			this.proxy.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("proxy.BackgroundImage")));
			this.proxy.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("proxy.Dock")));
			this.proxy.Enabled = ((bool)(resources.GetObject("proxy.Enabled")));
			this.proxy.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("proxy.FlatStyle")));
			this.proxy.Font = ((System.Drawing.Font)(resources.GetObject("proxy.Font")));
			this.helpProvider1.SetHelpKeyword(this.proxy, resources.GetString("proxy.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.proxy, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("proxy.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.proxy, resources.GetString("proxy.HelpString"));
			this.proxy.Image = ((System.Drawing.Image)(resources.GetObject("proxy.Image")));
			this.proxy.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("proxy.ImageAlign")));
			this.proxy.ImageIndex = ((int)(resources.GetObject("proxy.ImageIndex")));
			this.proxy.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("proxy.ImeMode")));
			this.proxy.Location = ((System.Drawing.Point)(resources.GetObject("proxy.Location")));
			this.proxy.Name = "proxy";
			this.proxy.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("proxy.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.proxy, ((bool)(resources.GetObject("proxy.ShowHelp"))));
			this.proxy.Size = ((System.Drawing.Size)(resources.GetObject("proxy.Size")));
			this.proxy.TabIndex = ((int)(resources.GetObject("proxy.TabIndex")));
			this.proxy.Text = resources.GetString("proxy.Text");
			this.proxy.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("proxy.TextAlign")));
			this.proxy.Visible = ((bool)(resources.GetObject("proxy.Visible")));
			this.proxy.Click += new System.EventHandler(this.proxy_Click);
			// 
			// enableAccount
			// 
			this.enableAccount.AccessibleDescription = resources.GetString("enableAccount.AccessibleDescription");
			this.enableAccount.AccessibleName = resources.GetString("enableAccount.AccessibleName");
			this.enableAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("enableAccount.Anchor")));
			this.enableAccount.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("enableAccount.Appearance")));
			this.enableAccount.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("enableAccount.BackgroundImage")));
			this.enableAccount.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("enableAccount.CheckAlign")));
			this.enableAccount.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("enableAccount.Dock")));
			this.enableAccount.Enabled = ((bool)(resources.GetObject("enableAccount.Enabled")));
			this.enableAccount.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("enableAccount.FlatStyle")));
			this.enableAccount.Font = ((System.Drawing.Font)(resources.GetObject("enableAccount.Font")));
			this.helpProvider1.SetHelpKeyword(this.enableAccount, resources.GetString("enableAccount.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.enableAccount, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("enableAccount.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.enableAccount, resources.GetString("enableAccount.HelpString"));
			this.enableAccount.Image = ((System.Drawing.Image)(resources.GetObject("enableAccount.Image")));
			this.enableAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("enableAccount.ImageAlign")));
			this.enableAccount.ImageIndex = ((int)(resources.GetObject("enableAccount.ImageIndex")));
			this.enableAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("enableAccount.ImeMode")));
			this.enableAccount.Location = ((System.Drawing.Point)(resources.GetObject("enableAccount.Location")));
			this.enableAccount.Name = "enableAccount";
			this.enableAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("enableAccount.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.enableAccount, ((bool)(resources.GetObject("enableAccount.ShowHelp"))));
			this.enableAccount.Size = ((System.Drawing.Size)(resources.GetObject("enableAccount.Size")));
			this.enableAccount.TabIndex = ((int)(resources.GetObject("enableAccount.TabIndex")));
			this.enableAccount.Text = resources.GetString("enableAccount.Text");
			this.enableAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("enableAccount.TextAlign")));
			this.enableAccount.Visible = ((bool)(resources.GetObject("enableAccount.Visible")));
			this.enableAccount.CheckedChanged += new System.EventHandler(this.enableAccount_CheckedChanged);
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
			this.helpProvider1.SetHelpKeyword(this.rememberPassword, resources.GetString("rememberPassword.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.rememberPassword, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("rememberPassword.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.rememberPassword, resources.GetString("rememberPassword.HelpString"));
			this.rememberPassword.Image = ((System.Drawing.Image)(resources.GetObject("rememberPassword.Image")));
			this.rememberPassword.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassword.ImageAlign")));
			this.rememberPassword.ImageIndex = ((int)(resources.GetObject("rememberPassword.ImageIndex")));
			this.rememberPassword.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rememberPassword.ImeMode")));
			this.rememberPassword.Location = ((System.Drawing.Point)(resources.GetObject("rememberPassword.Location")));
			this.rememberPassword.Name = "rememberPassword";
			this.rememberPassword.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rememberPassword.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.rememberPassword, ((bool)(resources.GetObject("rememberPassword.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.label10, resources.GetString("label10.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label10, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label10.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label10, resources.GetString("label10.HelpString"));
			this.label10.Image = ((System.Drawing.Image)(resources.GetObject("label10.Image")));
			this.label10.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label10.ImageAlign")));
			this.label10.ImageIndex = ((int)(resources.GetObject("label10.ImageIndex")));
			this.label10.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label10.ImeMode")));
			this.label10.Location = ((System.Drawing.Point)(resources.GetObject("label10.Location")));
			this.label10.Name = "label10";
			this.label10.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label10.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label10, ((bool)(resources.GetObject("label10.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.defaultServer, resources.GetString("defaultServer.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.defaultServer, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("defaultServer.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.defaultServer, resources.GetString("defaultServer.HelpString"));
			this.defaultServer.Image = ((System.Drawing.Image)(resources.GetObject("defaultServer.Image")));
			this.defaultServer.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("defaultServer.ImageAlign")));
			this.defaultServer.ImageIndex = ((int)(resources.GetObject("defaultServer.ImageIndex")));
			this.defaultServer.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("defaultServer.ImeMode")));
			this.defaultServer.Location = ((System.Drawing.Point)(resources.GetObject("defaultServer.Location")));
			this.defaultServer.Name = "defaultServer";
			this.defaultServer.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("defaultServer.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.defaultServer, ((bool)(resources.GetObject("defaultServer.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.removeAccount, resources.GetString("removeAccount.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.removeAccount, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("removeAccount.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.removeAccount, resources.GetString("removeAccount.HelpString"));
			this.removeAccount.Image = ((System.Drawing.Image)(resources.GetObject("removeAccount.Image")));
			this.removeAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("removeAccount.ImageAlign")));
			this.removeAccount.ImageIndex = ((int)(resources.GetObject("removeAccount.ImageIndex")));
			this.removeAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("removeAccount.ImeMode")));
			this.removeAccount.Location = ((System.Drawing.Point)(resources.GetObject("removeAccount.Location")));
			this.removeAccount.Name = "removeAccount";
			this.removeAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("removeAccount.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.removeAccount, ((bool)(resources.GetObject("removeAccount.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.addAccount, resources.GetString("addAccount.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.addAccount, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("addAccount.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.addAccount, resources.GetString("addAccount.HelpString"));
			this.addAccount.Image = ((System.Drawing.Image)(resources.GetObject("addAccount.Image")));
			this.addAccount.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("addAccount.ImageAlign")));
			this.addAccount.ImageIndex = ((int)(resources.GetObject("addAccount.ImageIndex")));
			this.addAccount.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("addAccount.ImeMode")));
			this.addAccount.Location = ((System.Drawing.Point)(resources.GetObject("addAccount.Location")));
			this.addAccount.Name = "addAccount";
			this.addAccount.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("addAccount.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.addAccount, ((bool)(resources.GetObject("addAccount.ShowHelp"))));
			this.addAccount.Size = ((System.Drawing.Size)(resources.GetObject("addAccount.Size")));
			this.addAccount.TabIndex = ((int)(resources.GetObject("addAccount.TabIndex")));
			this.addAccount.Text = resources.GetString("addAccount.Text");
			this.addAccount.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("addAccount.TextAlign")));
			this.addAccount.Visible = ((bool)(resources.GetObject("addAccount.Visible")));
			this.addAccount.Click += new System.EventHandler(this.addAccount_Click);
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
			this.helpProvider1.SetHelpKeyword(this.cancel, resources.GetString("cancel.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.cancel, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("cancel.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.cancel, resources.GetString("cancel.HelpString"));
			this.cancel.Image = ((System.Drawing.Image)(resources.GetObject("cancel.Image")));
			this.cancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancel.ImageAlign")));
			this.cancel.ImageIndex = ((int)(resources.GetObject("cancel.ImageIndex")));
			this.cancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cancel.ImeMode")));
			this.cancel.Location = ((System.Drawing.Point)(resources.GetObject("cancel.Location")));
			this.cancel.Name = "cancel";
			this.cancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cancel.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.cancel, ((bool)(resources.GetObject("cancel.ShowHelp"))));
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
			this.helpProvider1.SetHelpKeyword(this.apply, resources.GetString("apply.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.apply, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("apply.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.apply, resources.GetString("apply.HelpString"));
			this.apply.Image = ((System.Drawing.Image)(resources.GetObject("apply.Image")));
			this.apply.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("apply.ImageAlign")));
			this.apply.ImageIndex = ((int)(resources.GetObject("apply.ImageIndex")));
			this.apply.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("apply.ImeMode")));
			this.apply.Location = ((System.Drawing.Point)(resources.GetObject("apply.Location")));
			this.apply.Name = "apply";
			this.apply.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("apply.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.apply, ((bool)(resources.GetObject("apply.ShowHelp"))));
			this.apply.Size = ((System.Drawing.Size)(resources.GetObject("apply.Size")));
			this.apply.TabIndex = ((int)(resources.GetObject("apply.TabIndex")));
			this.apply.Text = resources.GetString("apply.Text");
			this.apply.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("apply.TextAlign")));
			this.apply.Visible = ((bool)(resources.GetObject("apply.Visible")));
			this.apply.Click += new System.EventHandler(this.apply_Click);
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
			this.helpProvider1.SetHelpKeyword(this.ok, resources.GetString("ok.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.ok, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("ok.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.ok, resources.GetString("ok.HelpString"));
			this.ok.Image = ((System.Drawing.Image)(resources.GetObject("ok.Image")));
			this.ok.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ok.ImageAlign")));
			this.ok.ImageIndex = ((int)(resources.GetObject("ok.ImageIndex")));
			this.ok.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("ok.ImeMode")));
			this.ok.Location = ((System.Drawing.Point)(resources.GetObject("ok.Location")));
			this.ok.Name = "ok";
			this.ok.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("ok.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.ok, ((bool)(resources.GetObject("ok.ShowHelp"))));
			this.ok.Size = ((System.Drawing.Size)(resources.GetObject("ok.Size")));
			this.ok.TabIndex = ((int)(resources.GetObject("ok.TabIndex")));
			this.ok.Text = resources.GetString("ok.Text");
			this.ok.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("ok.TextAlign")));
			this.ok.Visible = ((bool)(resources.GetObject("ok.Visible")));
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// timer1
			// 
			this.timer1.Interval = 10;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// helpProvider1
			// 
			this.helpProvider1.HelpNamespace = resources.GetString("helpProvider1.HelpNamespace");
			// 
			// Preferences
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
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.cancel);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.HelpButton = true;
			this.helpProvider1.SetHelpKeyword(this, resources.GetString("$this.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("$this.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this, resources.GetString("$this.HelpString"));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.KeyPreview = true;
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Preferences";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Preferences_Closing);
			this.Load += new System.EventHandler(this.Preferences_Load);
			this.Move += new System.EventHandler(this.Preferences_Move);
			this.VisibleChanged += new System.EventHandler(this.Preferences_VisibleChanged);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabAccounts.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
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
		/// Selects the Accounts tab.
		/// </summary>
		public void SelectAccounts()
		{
			tabControl1.SelectedTab = tabAccounts;
		}

		/// <summary>
		/// Selects the General tab.
		/// </summary>
		public void SelectGeneral()
		{
			tabControl1.SelectedTab = tabGeneral;
		}

		/// <summary>
		/// Adds the specified domain to the dropdown lists.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object to add to the list.</param>
		public void AddDomainToList(DomainInformation domainInfo)
		{
			Domain domain = null;
			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.ID.Equals(domainInfo.ID))
				{
					// The domain is already in the list.
					domain = d;
					break;
				}
			}

			if (domain == null)
			{
				domain = new Domain(domainInfo);

				// Reset the current default domain if the added domain is set to be the default.
				if (domainInfo.IsDefault)
				{
					if (currentDefaultDomain != null)
					{
						currentDefaultDomain.DomainInfo.IsDefault = false;
					}

					currentDefaultDomain = domain;
				}

				ListViewItem lvi = new ListViewItem(
					new string[] {domain.Name,
									 domainInfo.MemberName,
									 domainInfo.Active ? 
									 resourceManager.GetString("statusEnabled") : resourceManager.GetString("statusDisabled")});
				lvi.Tag = domain;
				lvi.Selected = domainInfo.IsDefault;
				accounts.Items.Add(lvi);
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

				if (d.DomainInfo.MemberUserID.Equals(userID))
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

				if (d.DomainInfo.POBoxID.Equals(poBoxID))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// Removes the specified domain to the dropdown lists.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object to remove from the list.</param>
		/// <param name="defaultDomainID">The ID of the new default domain.</param>
		public void RemoveDomainFromList(DomainInformation domainInfo, string defaultDomainID)
		{
			ListViewItem lvitem = null;
			Domain defaultDomain = null;
			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.ID.Equals(domainInfo.ID))
				{
					// The domain is in the list.
					lvitem = lvi;
				}
				else if (d.ID.Equals(defaultDomainID))
				{
					defaultDomain = d;
				}
			}

			if (lvitem != null)
			{
				lvitem.Remove();
			}

			if (defaultDomain != null)
			{
				// Reset the current default domain.
				if ((currentDefaultDomain != null) && !currentDefaultDomain.ID.Equals(defaultDomainID))
				{
					currentDefaultDomain.DomainInfo.IsDefault = false;
				}

				currentDefaultDomain = defaultDomain;

				if (newDefaultDomain != null)
				{
					newDefaultDomain.DomainInfo.IsDefault = false;
					newDefaultDomain = null;
				}
			}
		}
		#endregion

		#region Private Methods
		private bool connectToEnterprise()
		{
			bool result = false;

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				DomainInformation domainInfo = simiasWebService.ConnectToDomain(userName.Text, password.Text, server.Text);

				if (domainInfo != null)
				{
					// Set the credentials in the current process.
					DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domainInfo.ID, password.Text);
					Status authStatus = domainAuth.Authenticate();

					MyMessageBox mmb;
					switch (authStatus.statusCode)
					{
						case StatusCodes.Success:
						case StatusCodes.SuccessInGrace:
							Domain domain = new Domain(domainInfo);

							updateAccount(domain);

							// Associate the new domain with the listview item.
							newAccountLvi.SubItems[0].Text = domainInfo.Name;

							newAccountLvi.SubItems[2].Text = resourceManager.GetString("statusEnabled");
							newAccountLvi.Tag = domain;
							server.Text = domainInfo.Name;
							newAccountLvi = null;

							// Successfully joined ... don't allow the fields to be changed.
							userName.ReadOnly = server.ReadOnly = true;
							processing = false;

							if (EnterpriseConnect != null)
							{
								// Fire the event telling that a new domain has been added.
								EnterpriseConnect(this, new DomainConnectEventArgs(domainInfo));
							}

							addAccount.Enabled = details.Enabled = enableAccount.Enabled = true;

							activate.Enabled = false;

							try
							{
								// Check for an update.
								bool updateStarted = FormsTrayApp.CheckForClientUpdate(domainInfo.ID, userName.Text, password.Text);
								if (updateStarted)
								{
									if (ShutdownTrayApp != null)
									{
										// Shut down the tray app.
										ShutdownTrayApp(this, new EventArgs());
									}
								}
							}
							catch // Ignore
							{
							}

							if (!rememberPassword.Checked)
							{
								password.Text = string.Empty;
							}

							if (defaultServer.Checked)
							{
								try
								{
									simiasWebService.SetDefaultDomain(domain.DomainInfo.ID);

									domain.DomainInfo.IsDefault = true;
									newDefaultDomain = null;
									defaultServer.Enabled = false;

									// Reset the current default.
									if (currentDefaultDomain != null)
									{
										currentDefaultDomain.DomainInfo.IsDefault = false;
									}

									// Save the new default.
									currentDefaultDomain = domain;

									// Fire the event telling that the default domain has changed.
									if (ChangeDefaultDomain != null)
									{
										ChangeDefaultDomain(this, new DomainConnectEventArgs(currentDefaultDomain.DomainInfo));
									}
								}
								catch (Exception ex)
								{
									mmb = new MyMessageBox(resourceManager.GetString("setDefaultError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
									mmb.ShowDialog();
								}
							}

							if (authStatus.RemainingGraceLogins < authStatus.TotalGraceLogins)
							{
								mmb = new MyMessageBox(
									string.Format(resourceManager.GetString("graceLogin"), authStatus.RemainingGraceLogins),
									resourceManager.GetString("graceLoginTitle"),
									string.Empty,
									MyMessageBoxButtons.OK,
									MyMessageBoxIcon.Information);
								mmb.ShowDialog();
							}
						
							result = true;
							break;
						case StatusCodes.InvalidCredentials:
						case StatusCodes.InvalidPassword:
							mmb = new MyMessageBox(resourceManager.GetString("failedAuth"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
							break;
						case StatusCodes.AccountDisabled:
							mmb = new MyMessageBox(resourceManager.GetString("accountDisabled"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
							mmb.ShowDialog();
							break;
						case StatusCodes.AccountLockout:
							mmb = new MyMessageBox(resourceManager.GetString("accountLockout"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
							mmb.ShowDialog();
							break;
						default:
							mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
							mmb.ShowDialog();
							break;
					}
				}
				else
				{
					Cursor.Current = Cursors.Default;
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				Cursor.Current = Cursors.Default;
				MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("serverConnectError"), resourceManager.GetString("serverConnectErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			Cursor.Current = Cursors.Default;

			return result;
		}

		private void displaySyncInterval(int syncInterval)
		{
			// Set the state of the checkbox.
			autoSync.Checked = syncInterval != System.Threading.Timeout.Infinite;

			// Get the value and time units.
			string units = resourceManager.GetString("seconds");
			decimal displayValue = autoSync.Checked ? 
				iFolderAdvanced.ConvertSecondsToTimeUnit(syncInterval, out units) : minimumSeconds;

			// Select the time unit in the dropdown list.
			timeUnit.SelectedItem = resourceManager.GetString(units);

			// Display the interval.
			defaultInterval.Value = displayValue;
		}

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

		private bool processChanges()
		{
			bool result = true;

			Cursor.Current = Cursors.WaitCursor;

			// Add new account.
			if (newAccountLvi != null)
			{
				if (userName.Text.Equals(string.Empty) || server.Text.Equals(string.Empty))
				{
					MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("requiredFieldsMissing"), string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
					mmb.ShowDialog();
					SelectAccounts();
					if (server.Text.Equals(string.Empty))
					{
						server.Focus();
					}
					else if (userName.Text.Equals(string.Empty))
					{
						userName.Focus();
					}

					return false;
				}

				result = connectToEnterprise();
			}
			else if (accounts.SelectedItems.Count == 1)
			{
				if (!updateAccount((Domain)accounts.SelectedItems[0].Tag))
				{
					result = false;
				}
			}

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
				decimal syncValueInSeconds;

				if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("days")))
				{
					syncValueInSeconds = defaultInterval.Value * 86400;
				}
				else if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("hours")))
				{
					syncValueInSeconds = defaultInterval.Value * 3600;
				}
				else if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("minutes")))
				{
					syncValueInSeconds = defaultInterval.Value * 60;
				}
				else
				{
					syncValueInSeconds = defaultInterval.Value;
				}
				
				int currentInterval = ifWebService.GetDefaultSyncInterval();
				if ((!syncValueInSeconds.Equals((decimal)currentInterval)) ||
					(autoSync.Checked != (currentInterval != System.Threading.Timeout.Infinite)))
				{
					try
					{
						// Save the default sync interval.
						ifWebService.SetDefaultSyncInterval(autoSync.Checked ? (int)syncValueInSeconds : System.Threading.Timeout.Infinite);

						if (autoSync.Checked)
						{
							// Update the displayed value.
							displaySyncInterval((int)syncValueInSeconds);
						}
					}
					catch (Exception ex)
					{
						result = false;

						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}
			}
			catch (Exception ex)
			{
				result = false;

				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
			}

			if ((newDefaultDomain != null) && !newDefaultDomain.ID.Equals(currentDefaultDomain.ID))
			{
				try
				{
					simiasWebService.SetDefaultDomain(newDefaultDomain.DomainInfo.ID);

					currentDefaultDomain = newDefaultDomain;
					newDefaultDomain = null;

					if (ChangeDefaultDomain != null)
					{
						ChangeDefaultDomain(this, new DomainConnectEventArgs(currentDefaultDomain.DomainInfo));
					}
				}
				catch (Exception ex)
				{
					result = false;

					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("setDefaultError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}
			}

			Cursor.Current = Cursors.Default;

			return result;
		}

		private bool updateAccount(Domain domain)
		{
			bool result = true;

			if (domain != null)
			{
				// Update the password, if the password settings have changed.
				if (updatePassword || (newAccountLvi != null))
				{
					try
					{
						if (rememberPassword.Checked)
						{
							simiasWebService.SetDomainCredentials(domain.ID, password.Text, CredentialType.Basic);
						}
						else if (newAccountLvi == null)
						{
							simiasWebService.SetDomainCredentials(domain.ID, null, CredentialType.None);
						}

						updatePassword = false;
					}
					catch (Exception ex)
					{
						result = false;
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("savePasswordError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
					}
				}

				if (updateEnabled)
				{
					try
					{
						// Update the enabled setting.
						if (enableAccount.Checked)
						{
							simiasWebService.SetDomainActive(domain.ID);
							domain.DomainInfo.Active = true;
						}
						else
						{
							simiasWebService.SetDomainInactive(domain.ID);
							domain.DomainInfo.Active = false;
						}

						updateDomainStatus(domain);
						updateEnabled = false;
					}
					catch {}
				}
			}

			return result;
		}

		private void updateDomainStatus(Domain domain)
		{
			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;
				if (d.ID.Equals(domain.ID))
				{
					lvi.SubItems[2].Text = domain.DomainInfo.Active ? 
						resourceManager.GetString("statusEnabled") : resourceManager.GetString("statusDisabled");
					break;
				}
			}

			if (UpdateDomain != null)
			{
				UpdateDomain(this, new DomainConnectEventArgs(domain.DomainInfo));
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used when successfully connected to Enterprise Server.
		/// </summary>
		public delegate void EnterpriseConnectDelegate(object sender, DomainConnectEventArgs e);
		/// <summary>
		/// Occurs when successfully connected to enterprise.
		/// </summary>
		public event EnterpriseConnectDelegate EnterpriseConnect;

		/// <summary>
		/// Delegate used when the default domain is changed.
		/// </summary>
		public delegate void ChangeDefaultDomainDelegate(object sender, DomainConnectEventArgs e);
		/// <summary>
		/// Occurs when the default domain is changed.
		/// </summary>
		public event ChangeDefaultDomainDelegate ChangeDefaultDomain;

		/// <summary>
		/// Delegate used when a domain account is removed.
		/// </summary>
		public delegate void RemoveDomainDelegate(object sender, DomainRemoveEventArgs e);
		/// <summary>
		/// Occurs when a domain account is removed.
		/// </summary>
		public event RemoveDomainDelegate RemoveDomain;

		/// <summary>
		/// Delegate used to shutdown the tray app when an upgrade is in progress.
		/// </summary>
		public delegate void ShutdownTrayAppDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when an upgrade has been started.
		/// </summary>
		public event ShutdownTrayAppDelegate ShutdownTrayApp;

		/// <summary>
		/// Delegate used to update a domain.
		/// </summary>
		public delegate void UpdateDomainDelegate(object sender, DomainConnectEventArgs e);
		/// <summary>
		/// Occurs when a domain has changed.
		/// </summary>
		public event UpdateDomainDelegate UpdateDomain;
		#endregion

		#region Event Handlers
		private void Preferences_Load(object sender, System.EventArgs e)
		{
			// Reference the help using locale-specific path.
			helpProvider1.HelpNamespace = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"doc\user\data\preferences.html");

			// Load the application icon and banner image.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch {} // Non-fatal ...

			if(Environment.OSVersion.Version.Major > 4 
				& Environment.OSVersion.Version.Minor > 0 
				& System.IO.File.Exists(Application.ExecutablePath + ".manifest"))
			{
				tabGeneral.BackColor = tabAccounts.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight);
			}

			minimumSyncInterval = minimumSeconds = defaultMinimumSeconds;
			timeUnit.Items.Add(resourceManager.GetString("seconds"));
			timeUnit.Items.Add(resourceManager.GetString("minutes"));
			timeUnit.Items.Add(resourceManager.GetString("hours"));
			timeUnit.Items.Add(resourceManager.GetString("days"));
		}

		private void Preferences_VisibleChanged(object sender, System.EventArgs e)
		{
			if (this.Visible)
			{
				accounts.Items.Clear();
				successful = true;

				DomainInformation[] domains;
				try
				{
					domains = simiasWebService.GetDomains(true);
					foreach (DomainInformation di in domains)
					{
						AddDomainToList(di);
					}
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readAccountsError"), string.Empty, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
				}

				apply.Enabled = false;

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
					int syncInterval = ifWebService.GetDefaultSyncInterval();
					minimumSeconds = syncInterval < (int)defaultMinimumSeconds ? (decimal)syncInterval : defaultMinimumSeconds;

					displaySyncInterval(syncInterval);
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
			if (!successful && DialogResult.Equals(DialogResult.OK))
			{
				// There was a failure, cancel the close event.
				e.Cancel = true;
			}
			else if (!shutdown)
			{
				// If we haven't received a shutdown event, cancel the close event and hide the dialog.
				e.Cancel = true;
				currentDefaultDomain = newDefaultDomain = null;
				newAccountLvi = null;

				// Reset the controls.
				userName.Text = server.Text = password.Text = string.Empty;
				rememberPassword.Checked = enableAccount.Checked = defaultServer.Checked = false;

				// Disable/enable the controls.
				addAccount.Enabled = true;
				userName.Enabled = server.Enabled = password.Enabled = rememberPassword.Enabled =
					enableAccount.Enabled = defaultServer.Enabled = details.Enabled = 
					removeAccount.Enabled = proxy.Enabled = activate.Enabled = false;

				updatePassword = updateEnabled = false;

				Hide();
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			// If this fails don't dismiss the dialog.
			successful = processChanges();
			Close();
		}

		private void apply_Click(object sender, System.EventArgs e)
		{
			if (processChanges())
			{
				apply.Enabled = false;
			}
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		#region General Tab
		private void autoStart_CheckedChanged(object sender, System.EventArgs e)
		{
			if (autoStart.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void displayConfirmation_CheckedChanged(object sender, System.EventArgs e)
		{
			if (displayConfirmation.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void autoSync_CheckedChanged(object sender, System.EventArgs e)
		{
			if (autoSync.Focused)
			{
				apply.Enabled = true;
			}

			defaultInterval.Enabled = timeUnit.Enabled = autoSync.Checked;			
		}

		private void notifyShared_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyShared.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void notifyCollisions_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyCollisions.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void notifyJoins_CheckedChanged(object sender, System.EventArgs e)
		{
			if (notifyJoins.Focused)
			{
				apply.Enabled = true;
			}
		}

		private void defaultInterval_ValueChanged(object sender, System.EventArgs e)
		{
			if (defaultInterval.Focused)
			{
				try
				{
					if (!defaultInterval.Text.Equals(string.Empty))
					{
						defaultInterval.Value = decimal.Parse(defaultInterval.Text);
					}
				}
				catch
				{
					defaultInterval.Value = minimumSyncInterval;
				}

				if (defaultInterval.Value < minimumSyncInterval)
				{
					defaultInterval.Value = minimumSyncInterval;
				}

				apply.Enabled = true;
			}
		}

		private void defaultInterval_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (((e.KeyCode & Keys.F) == Keys.F) &&
				((e.Modifiers & Keys.Shift) == Keys.Shift) &&
				((e.Modifiers & Keys.Control) == Keys.Control))
			{
				defaultInterval.Minimum = minimumSyncInterval = 0;
			}
			else
			{
				timeUnit_SelectedIndexChanged(this, new EventArgs());
			}
		}

		private void timeUnit_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (timeUnit.Focused)
			{
				apply.Enabled = true;
			}

			if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("seconds")))
			{
				minimumSyncInterval = minimumSeconds;
				defaultInterval.Maximum = maximumSeconds;
				defaultInterval.Increment = 5;

				if (defaultInterval.Value < minimumSyncInterval)
				{
					defaultInterval.Value = minimumSyncInterval;
				}
			}
			else if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("minutes")))
			{
				minimumSyncInterval = defaultInterval.Increment = 1;
				defaultInterval.Maximum = maximumMinutes;				
			}
			else if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("hours")))
			{
				minimumSyncInterval = defaultInterval.Increment = 1;
				defaultInterval.Maximum = maximumHours;
			}
			else if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("days")))
			{
				minimumSyncInterval = defaultInterval.Increment = 1;
				defaultInterval.Maximum = maximumDays;
			}

			defaultInterval.Minimum = minimumSyncInterval;
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

					if (domain != null)
					{
						// Reset the flag on the current default domain.
						currentDefaultDomain.DomainInfo.IsDefault = false;

						// Reset the flag on the "old" new default domain.
						if (newDefaultDomain != null)
						{
							newDefaultDomain.DomainInfo.IsDefault = false;
						}

						// Set the flag on the new default domain.
						domain.DomainInfo.IsDefault = true;
						newDefaultDomain = domain;

						// Disable the checkbox so that it cannot be unchecked and don't allow 
						// the new default to be removed.
						defaultServer.Enabled = /*removeAccount.Enabled =*/ false;

						apply.Enabled = true;
					}
				}
				catch
				{}
			}
		}

		private void addAccount_Click(object sender, System.EventArgs e)
		{
			// Only allow one-at-a-time account creation.
			addAccount.Enabled = false;

			ListViewItem lvi = new ListViewItem(new string[] {string.Empty, string.Empty, string.Empty});
			accounts.Items.Add(lvi);
			accounts.SelectedItems.Clear();
			lvi.Selected = true;
		}

		private void userName_TextChanged(object sender, System.EventArgs e)
		{
			if (userName.Focused)
			{
				try
				{
					ListViewItem lvi = accounts.SelectedItems[0];
					lvi.SubItems[1].Text = userName.Text;
					activate.Enabled = !userName.Text.Equals(string.Empty) && !server.Text.Equals(string.Empty);

//					apply.Enabled = true;
				}
				catch {}
			}
		}

		private void server_TextChanged(object sender, System.EventArgs e)
		{
			if (server.Focused)
			{
				try
				{
					ListViewItem lvi = accounts.SelectedItems[0];
					lvi.SubItems[0].Text = server.Text;
					activate.Enabled = !userName.Text.Equals(string.Empty) && !server.Text.Equals(string.Empty);

//					apply.Enabled = true;
				}
				catch {}
			}
		}

		private void password_TextChanged(object sender, System.EventArgs e)
		{
			if (password.Focused && (newAccountLvi != null))
			{
				apply.Enabled = apply.Enabled ? true : rememberPassword.Checked;
				updatePassword = rememberPassword.Checked;
			}
		}

		private void removeAccount_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = accounts.SelectedItems[0];
			Domain domain = (Domain)lvi.Tag;

			if (domain == null)
			{
				// Remove the new account
				newAccountLvi = null;
				lvi.Remove();
				updatePassword = false;
				updateEnabled = false;
				addAccount.Enabled = true;
			}
			else
			{
				// Remove the enterprise account.
				string message = resourceManager.GetString("deleteAccountPrompt") + "\n\n" +
					resourceManager.GetString("deleteAccountInfo");
				DialogResult dialogResult = MessageBox.Show(message, resourceManager.GetString("deleteAccountTitle"),
					MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if (dialogResult != DialogResult.Cancel)
				{
					try
					{
						simiasWebService.LeaveDomain(domain.ID, dialogResult == DialogResult.No);
						lvi.Remove();

						string defaultDomainID = null;

						if (domain.Equals(newDefaultDomain))
						{
							// Set the current default domain back to the default.
							currentDefaultDomain.DomainInfo.IsDefault = true;
							newDefaultDomain = null;
						}
						else if (domain.Equals(currentDefaultDomain))
						{
							// The default domain was removed, get the new default.
							defaultDomainID = simiasWebService.GetDefaultDomainID();
						}

						if (RemoveDomain != null)
						{
							// Call delegate to remove the domain from the server dropdown list.
							RemoveDomain(this, new DomainRemoveEventArgs(domain.DomainInfo, defaultDomainID));
						}

						if (defaultDomainID != null)
						{
							// Set the new default domain.
							foreach (ListViewItem item in accounts.Items)
							{
								Domain d = (Domain)item.Tag;
								if (d.ID.Equals(defaultDomainID))
								{
									currentDefaultDomain = d;
									if (newDefaultDomain == null)
									{
										d.DomainInfo.IsDefault = true;
									}
									break;
								}
							}
						}

						updatePassword = false;
						updateEnabled = false;
					}
					catch {}
				}
			}
		}

		private void rememberPassword_CheckedChanged(object sender, System.EventArgs e)
		{
			if (rememberPassword.Focused && (newAccountLvi == null) && (accounts.SelectedItems.Count == 1))
			{
				apply.Enabled = true;
				updatePassword = true;
			}
		}

		private void enableAccount_CheckedChanged(object sender, System.EventArgs e)
		{
			if (enableAccount.Focused)
			{
				apply.Enabled = true;
				updateEnabled = true;
			}
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
			timer1.Stop();
			newAccountLvi.Selected = true;
			processing = false;
		}

		private void accounts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (processing)
				return;

			if (!processing && (newAccountLvi != null))
			{
				if (MessageBox.Show(resourceManager.GetString("saveAccountPrompt"), resourceManager.GetString("saveAccountTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					if (userName.Text.Equals(string.Empty) || server.Text.Equals(string.Empty))
					{
						processing = true;
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("requiredFieldsMissing"), string.Empty, string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Information);
						mmb.ShowDialog();
						timer1.Start();

						if (server.Text.Equals(string.Empty))
						{
							server.Focus();
						}
						else if (userName.Text.Equals(string.Empty))
						{
							userName.Focus();
						}
					}
					else
					{
						processing = true;
						if (!connectToEnterprise())
						{
							timer1.Start();
							return;
						}
					}
				}
				else
				{
					addAccount.Enabled = true;
					newAccountLvi.Remove();
					newAccountLvi = null;
				}
			}

			if (updatePassword || updateEnabled)
			{
				if (MessageBox.Show(resourceManager.GetString("updatePrompt"), resourceManager.GetString("updateTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					updateAccount(selectedDomain);
				}
				else
				{
					updatePassword = false;
					updateEnabled = false;
				}
			}

			if (accounts.SelectedItems.Count == 1)
			{
				ListViewItem lvi = accounts.SelectedItems[0];
				if (lvi != null)
				{
					proxy.Enabled = true;

					if ((newAccountLvi == null) || (lvi == newAccountLvi))
					{
						userName.Enabled = server.Enabled = 
							rememberPassword.Enabled = password.Enabled = removeAccount.Enabled = true;

						userName.Text = lvi.SubItems[1].Text;
						password.Text = string.Empty;

						selectedDomain = (Domain)lvi.Tag;

						if (selectedDomain == null)
						{
							// This is a new account.
							server.Text = lvi.SubItems[0].Text;
							newAccountLvi = lvi;
							userName.ReadOnly = server.ReadOnly = false;
							details.Enabled = activate.Enabled = enableAccount.Enabled = false;
							enableAccount.Checked = true;

							defaultServer.Enabled = (accounts.Items.Count > 1);
							defaultServer.Checked = (accounts.Items.Count == 1);
							server.Focus();
						}
						else
						{
							server.Text = selectedDomain.DomainInfo.Name;
							details.Enabled = true;
							userName.ReadOnly = server.ReadOnly = true;

							defaultServer.Checked = selectedDomain.DomainInfo.IsDefault;
							defaultServer.Enabled = !defaultServer.Checked;

							// Check for a saved password.
							try
							{
								string userID;
								string credentials;
								CredentialType credType = simiasWebService.GetDomainCredentials(selectedDomain.ID, out userID, out credentials);
								if ((credType == CredentialType.Basic) && (credentials != null))
								{
									// There are credentials that were saved on the domain.
									rememberPassword.Checked = true;
									password.Text = credentials;
								}
							}
							catch (Exception ex)
							{
								// TODO: message
								MessageBox.Show(ex.Message);
							}

							enableAccount.Enabled = true;
							enableAccount.Checked = selectedDomain.DomainInfo.Active;
						}
					}
				}
			}
			else if (!processing)
			{
				selectedDomain = null;

				// Reset the controls.
				userName.Text = server.Text = password.Text = string.Empty;
				rememberPassword.Checked = enableAccount.Checked = defaultServer.Checked = false;

				// Disable the controls.
				userName.Enabled = server.Enabled = password.Enabled = rememberPassword.Enabled =
					enableAccount.Enabled = defaultServer.Enabled = details.Enabled = 
					removeAccount.Enabled = proxy.Enabled = activate.Enabled = false;
			}
		}

		private void details_Click(object sender, System.EventArgs e)
		{
			Domain domain = (Domain)accounts.SelectedItems[0].Tag;
			if (domain != null)
			{
				ServerDetails serverDetails = new ServerDetails(this.ifWebService, domain);
				serverDetails.ShowDialog();
			}
		}

		private void proxy_Click(object sender, System.EventArgs e)
		{
			AdvancedSettings advancedSettings = new AdvancedSettings();
			advancedSettings.ShowDialog();
		}

		private void activate_Click(object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			connectToEnterprise();
			Cursor.Current = Cursors.Default;
		}
		#endregion

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			switch (tabControl1.SelectedIndex)
			{
				case 0:
					// Reference the help using locale-specific path.
					helpProvider1.HelpNamespace = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"doc\user\data\preferences.html");
					break;
				case 1:
					// Reference the help using locale-specific path.
					helpProvider1.HelpNamespace = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"doc\user\data\accounts.html");
					break;
			}
		}

		private void Preferences_Move(object sender, System.EventArgs e)
		{
			if (initialPositionSet)
			{
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Set the location values.
					regKey.SetValue(preferencesX, Location.X);
					regKey.SetValue(preferencesY, Location.Y);
				}
				catch {}
			}
			else
			{
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

					// Get the location values.
					int x = (int)regKey.GetValue(preferencesX);
					int y = (int)regKey.GetValue(preferencesY);

					Point point = new Point(x, y);
					this.Location = point;
				}
				catch {}

				initialPositionSet = true;
			}
		}
		#endregion

		private const int WM_QUERYENDSESSION = 0x0011;

		/// <summary>
		/// Override of WndProc method.
		/// </summary>
		/// <param name="m">The message to process.</param>
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
