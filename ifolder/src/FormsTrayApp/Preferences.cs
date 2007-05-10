/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
using Novell.Wizard;
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
		private Domain selectedDomain = null;
		private ListViewItem newAccountLvi = null;
		private bool processing = false;
		private bool successful;
		private bool updatePassword = false;
		private bool updateEnabled = false;
		private bool updateHost = false;
		private bool initialPositionSet = false;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.CheckBox autoStart;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.CheckBox notifyShared;
		private System.Windows.Forms.CheckBox notifyCollisions;
		private System.Windows.Forms.CheckBox notifyJoins;
		private System.Windows.Forms.Button addAccount;
		private System.Windows.Forms.Button removeAccount;
		private System.Windows.Forms.Button details;
		private System.Windows.Forms.ListView accounts;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabAccounts;
		private System.Windows.Forms.TabPage tabMigrate;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.ComboBox timeUnit;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.Button btnMigrate;
		private Manager simiasManager;
		public Novell.FormsTrayApp.GlobalProperties parent;
		#endregion

		/// <summary>
		/// Instantiates a Preferences object.
		/// </summary>
		public Preferences(iFolderWebService ifolderWebService, SimiasWebService simiasWebService, Manager simiasManager)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			defaultInterval.TextChanged += new EventHandler(defaultInterval_ValueChanged);

			ifWebService = ifolderWebService;
			this.simiasWebService = simiasWebService;
			this.simiasManager = simiasManager;

			// Resize/reposition controls
//			int delta = calculateSize(label5, 0);
//			delta = calculateSize(label10, delta);
//			delta = calculateSize(label9, delta);

//			if (delta > 0)
			{
//				label5.Width = label10.Width = label9.Width += delta;
//				rememberPassword.Left = enableAccount.Left = defaultServer.Left = label5.Left + label5.Width;
			}

			int delta = calculateSize(label1, 0);

			if (delta > 0)
			{
				groupBox1.Height += 8 * (int)Math.Ceiling((float)delta / (float)label1.Width);
			}

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
			this.label1 = new System.Windows.Forms.Label();
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
			this.removeAccount = new System.Windows.Forms.Button();
			this.addAccount = new System.Windows.Forms.Button();
			this.tabMigrate = new System.Windows.Forms.TabPage();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.ok = new System.Windows.Forms.Button();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			this.btnMigrate = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabAccounts.SuspendLayout();
			this.tabMigrate.SuspendLayout();
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
			//this.tabControl1.Controls.Add(this.tabMigrate);
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
			this.groupBox1.Controls.Add(this.label1);
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
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.helpProvider1.SetHelpKeyword(this.label1, resources.GetString("label1.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.label1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("label1.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.label1, resources.GetString("label1.HelpString"));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.label1, ((bool)(resources.GetObject("label1.ShowHelp"))));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
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
			this.accounts.CheckBoxes = true;
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
			this.accounts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.accounts_ItemCheck);
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
			// tabMigrate
			// 
			this.tabMigrate.AccessibleDescription = resources.GetString("tabMigrate.AccessibleDescription");
			this.tabMigrate.AccessibleName = resources.GetString("tabMigrate.AccessibleName");
			this.tabMigrate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabMigrate.Anchor")));
			this.tabMigrate.AutoScroll = ((bool)(resources.GetObject("tabMigrate.AutoScroll")));
			this.tabMigrate.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabMigrate.AutoScrollMargin")));
			this.tabMigrate.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabMigrate.AutoScrollMinSize")));
			this.tabMigrate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabMigrate.BackgroundImage")));
			this.tabMigrate.Controls.Add(this.btnMigrate);
			this.tabMigrate.Controls.Add(this.listView1);
			this.tabMigrate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabMigrate.Dock")));
			this.tabMigrate.Enabled = ((bool)(resources.GetObject("tabMigrate.Enabled")));
			this.tabMigrate.Font = ((System.Drawing.Font)(resources.GetObject("tabMigrate.Font")));
			this.helpProvider1.SetHelpKeyword(this.tabMigrate, resources.GetString("tabMigrate.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.tabMigrate, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("tabMigrate.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.tabMigrate, resources.GetString("tabMigrate.HelpString"));
			this.tabMigrate.ImageIndex = ((int)(resources.GetObject("tabMigrate.ImageIndex")));
			this.tabMigrate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabMigrate.ImeMode")));
			this.tabMigrate.Location = ((System.Drawing.Point)(resources.GetObject("tabMigrate.Location")));
			this.tabMigrate.Name = "tabMigrate";
			this.tabMigrate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabMigrate.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.tabMigrate, ((bool)(resources.GetObject("tabMigrate.ShowHelp"))));
			this.tabMigrate.Size = ((System.Drawing.Size)(resources.GetObject("tabMigrate.Size")));
			this.tabMigrate.TabIndex = ((int)(resources.GetObject("tabMigrate.TabIndex")));
			this.tabMigrate.Text = resources.GetString("tabMigrate.Text");
			this.tabMigrate.ToolTipText = resources.GetString("tabMigrate.ToolTipText");
			this.tabMigrate.Visible = ((bool)(resources.GetObject("tabMigrate.Visible")));
			// 
			// listView1
			// 
			this.listView1.AccessibleDescription = resources.GetString("listView1.AccessibleDescription");
			this.listView1.AccessibleName = resources.GetString("listView1.AccessibleName");
			this.listView1.Alignment = ((System.Windows.Forms.ListViewAlignment)(resources.GetObject("listView1.Alignment")));
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listView1.Anchor")));
			this.listView1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listView1.BackgroundImage")));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader4,
																						this.columnHeader5});
			this.listView1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listView1.Dock")));
			this.listView1.Enabled = ((bool)(resources.GetObject("listView1.Enabled")));
			this.listView1.Font = ((System.Drawing.Font)(resources.GetObject("listView1.Font")));
			this.listView1.FullRowSelect = true;
			this.helpProvider1.SetHelpKeyword(this.listView1, resources.GetString("listView1.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.listView1, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("listView1.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.listView1, resources.GetString("listView1.HelpString"));
			this.listView1.HideSelection = false;
			this.listView1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listView1.ImeMode")));
			this.listView1.LabelWrap = ((bool)(resources.GetObject("listView1.LabelWrap")));
			this.listView1.Location = ((System.Drawing.Point)(resources.GetObject("listView1.Location")));
			this.listView1.Name = "listView1";
			this.listView1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listView1.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.listView1, ((bool)(resources.GetObject("listView1.ShowHelp"))));
			this.listView1.Size = ((System.Drawing.Size)(resources.GetObject("listView1.Size")));
			this.listView1.TabIndex = ((int)(resources.GetObject("listView1.TabIndex")));
			this.listView1.Text = resources.GetString("listView1.Text");
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.Visible = ((bool)(resources.GetObject("listView1.Visible")));
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = resources.GetString("columnHeader4.Text");
			this.columnHeader4.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader4.TextAlign")));
			this.columnHeader4.Width = ((int)(resources.GetObject("columnHeader4.Width")));
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = resources.GetString("columnHeader5.Text");
			this.columnHeader5.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("columnHeader5.TextAlign")));
			this.columnHeader5.Width = ((int)(resources.GetObject("columnHeader5.Width")));
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
			// btnMigrate
			// 
			this.btnMigrate.AccessibleDescription = resources.GetString("btnMigrate.AccessibleDescription");
			this.btnMigrate.AccessibleName = resources.GetString("btnMigrate.AccessibleName");
			this.btnMigrate.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnMigrate.Anchor")));
			this.btnMigrate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnMigrate.BackgroundImage")));
			this.btnMigrate.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnMigrate.Dock")));
			this.btnMigrate.Enabled = ((bool)(resources.GetObject("btnMigrate.Enabled")));
			this.btnMigrate.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnMigrate.FlatStyle")));
			this.btnMigrate.Font = ((System.Drawing.Font)(resources.GetObject("btnMigrate.Font")));
			this.helpProvider1.SetHelpKeyword(this.btnMigrate, resources.GetString("btnMigrate.HelpKeyword"));
			this.helpProvider1.SetHelpNavigator(this.btnMigrate, ((System.Windows.Forms.HelpNavigator)(resources.GetObject("btnMigrate.HelpNavigator"))));
			this.helpProvider1.SetHelpString(this.btnMigrate, resources.GetString("btnMigrate.HelpString"));
			this.btnMigrate.Image = ((System.Drawing.Image)(resources.GetObject("btnMigrate.Image")));
			this.btnMigrate.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnMigrate.ImageAlign")));
			this.btnMigrate.ImageIndex = ((int)(resources.GetObject("btnMigrate.ImageIndex")));
			this.btnMigrate.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnMigrate.ImeMode")));
			this.btnMigrate.Location = ((System.Drawing.Point)(resources.GetObject("btnMigrate.Location")));
			this.btnMigrate.Name = "btnMigrate";
			this.btnMigrate.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnMigrate.RightToLeft")));
			this.helpProvider1.SetShowHelp(this.btnMigrate, ((bool)(resources.GetObject("btnMigrate.ShowHelp"))));
			this.btnMigrate.Size = ((System.Drawing.Size)(resources.GetObject("btnMigrate.Size")));
			this.btnMigrate.TabIndex = ((int)(resources.GetObject("btnMigrate.TabIndex")));
			this.btnMigrate.Text = resources.GetString("btnMigrate.Text");
			this.btnMigrate.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnMigrate.TextAlign")));
			this.btnMigrate.Visible = ((bool)(resources.GetObject("btnMigrate.Visible")));
			this.btnMigrate.Click +=new EventHandler(btnMigrate_Click);
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
			this.tabMigrate.ResumeLayout(false);
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
		/// <param name="createAccount">Set to <b>True</b> to activate the page to create a new account.</param>
		public void SelectAccounts(bool createAccount)
		{
			tabControl1.SelectedTab = tabAccounts;
			if (createAccount)
			{
				addAccount_Click(this, new EventArgs());
			}
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
					if ((currentDefaultDomain != null) && !currentDefaultDomain.ID.Equals( domainInfo.ID ))
					{
						currentDefaultDomain.DomainInfo.IsDefault = false;
					}

					currentDefaultDomain = domain;

					// Fire the event telling that the default domain has changed.
					if (ChangeDefaultDomain != null)
					{
						ChangeDefaultDomain(this, new DomainConnectEventArgs(currentDefaultDomain.DomainInfo));
					}
				}

				ListViewItem lvi = new ListViewItem(
					new string[] { string.Empty, domain.Name,
									 domainInfo.MemberName } );
				lvi.Checked = domainInfo.Authenticated;
				lvi.Tag = domain;
				lvi.Selected = domainInfo.IsDefault;
				accounts.Items.Add(lvi);
			}
		}

		/// <summary>
		/// Gets the name of the domain for the specified ID.
		/// </summary>
		/// <param name="poBoxID">The ID of the POBox for the domain.</param>
		/// <returns>The name of the domain.</returns>
		public string GetDomainName(string poBoxID)
		{
			string name = string.Empty;

			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;

				if (d.DomainInfo.POBoxID.Equals(poBoxID))
				{
					name = d.Name;
					break;
				}
			}

			return name;
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
		/// Removes the specified domain from the dropdown lists.
		/// </summary>
		/// <param name="domainInfo">The DomainInformation object to remove from the list.</param>
		/// <param name="defaultDomainID">The ID of the new default domain.</param>
		public void RemoveDomainFromList(DomainInformation domainInfo, string defaultDomainID)
		{
			ListViewItem lvitem = null;
			Domain defaultDomain = null;

			try
			{
				foreach (ListViewItem lvi in accounts.Items)
				{
					Domain d = (Domain)lvi.Tag;

					if (d.ID.Equals(domainInfo.ID))
					{
						// The domain is in the list.
						lvitem = lvi;
					}
					else if ((defaultDomainID != null) && d.ID.Equals(defaultDomainID))
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
				}
			}
			catch {}
		}

		public void SetProxyForDomain( string hostUrl, bool unknownScheme )
		{
			UriBuilder ubHost = new UriBuilder( hostUrl );

			// If a domain name was passed in without a scheme, a proxy will
			// need to be setup for both http and https schemes because we don't
			// know how it will ultimately be sent.
			if ( unknownScheme )
			{
				// Set the proxy for http.
				ubHost.Scheme = Uri.UriSchemeHttp;
				ubHost.Port = 80;
				SetProxyForDomain( ubHost.Uri.ToString(), false );

				// Now set it for https.
				ubHost.Scheme = Uri.UriSchemeHttps;
				ubHost.Port = 443;
				SetProxyForDomain( ubHost.Uri.ToString(), false );
			}
			else
			{
				// Set any proxy information for this domain.
				IWebProxy iwp = GlobalProxySelection.Select;
				if ( !iwp.IsBypassed( ubHost.Uri ) )
				{
					string proxyUser = null;
					string proxyPassword = null;

					Uri proxyUri = iwp.GetProxy( ubHost.Uri );
					if ( iwp.Credentials != null )
					{
						NetworkCredential netCred = iwp.Credentials.GetCredential( proxyUri, "Basic" );
						if ( netCred != null )
						{
							proxyUser = netCred.UserName;
							proxyPassword = netCred.Password;
						}
					}

					// The scheme for the proxy address needs to match the scheme for the host address.
					simiasWebService.SetProxyAddress( ubHost.Uri.ToString(), proxyUri.ToString(), proxyUser, proxyPassword );
				}
			}
		}

		/// <summary>
		/// Updates the domain status in the Accounts page.
		/// </summary>
		/// <param name="domain">The domain to update.</param>
		public void UpdateDomainStatus(Domain domain)
		{
			foreach (ListViewItem lvi in accounts.Items)
			{
				Domain d = (Domain)lvi.Tag;
				if (d.ID.Equals(domain.ID))
				{
					lvi.Tag = domain;
					lvi.Checked = domain.DomainInfo.Authenticated;
					break;
				}
			}

			if (UpdateDomain != null)
			{
				UpdateDomain(this, new DomainConnectEventArgs(domain.DomainInfo));
			}
		}
		/// <summary>
		/// Adds Migration Details to the list view item..
		/// </summary>
		public void AddMigrationDetails()
		{
			string iFolderRegistryKey = @"Software\Novell iFolder";
			RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey);
			string[] AllKeys = new string[iFolderKey.SubKeyCount];
			string User;
			AllKeys = iFolderKey.GetSubKeyNames();
			this.listView1.Items.Clear();
		
			for(int i=0; i< AllKeys.Length; i++)
			{
				ListViewItem lvi;
				User = iFolderRegistryKey + "\\" + AllKeys[i];
				RegistryKey UserKey = Registry.LocalMachine.OpenSubKey(User);
				if( UserKey.GetValue("FolderPath") != null)
				{
					lvi = new ListViewItem( new string[]{AllKeys[i], (string)UserKey.GetValue("FolderPath")});
					listView1.Items.Add(lvi);
					lvi.Selected = true;
				}
				UserKey.Close();
				/*
				else
				{
					lvi = new ListViewItem( new string[]{AllKeys[i], "Not a user"});
					this.listView1.Items.Add(lvi);
				}
				*/
				
			}
			iFolderKey.Close();
			
		}

		#endregion

		#region Private Methods
		private int calculateSize(Control control, int delta)
		{
			int size;
			Graphics g = control.CreateGraphics();
			try
			{
				SizeF textSize = g.MeasureString(control.Text, control.Font);
				size = (int)Math.Ceiling(textSize.Width) - control.Width;
			}
			finally
			{
				g.Dispose();
			}

			return (int)Math.Max(delta, size);
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

		private bool login( int itemIndex )
		{
			bool result = false;

			ListViewItem lvi = accounts.Items[ itemIndex ];
			Domain domain = (Domain)lvi.Tag;
			if (domain != null)
			{
				Connecting connecting = new Connecting( simiasWebService, simiasManager, domain.DomainInfo );
				if ( connecting.ShowDialog() == DialogResult.OK )
				{
					result = true;
				}

				if ( !result )
				{
					ServerInfo serverInfo = new ServerInfo( simiasManager, domain.DomainInfo, connecting.Password );
					serverInfo.ShowDialog();
					result = serverInfo.DomainInfo.Authenticated;
					serverInfo.Dispose();
				}

				connecting.Dispose();
			}

			if ( result )
			{
				domain.DomainInfo.Authenticated = true;
				lvi.Tag = domain;
			}

			return result;
		}

		private bool logout( int itemIndex )
		{
			bool result = false;

			ListViewItem lvi = accounts.Items[ itemIndex ];
			Domain domain = (Domain)lvi.Tag;
			if (domain != null)
			{
				DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domain.ID, null);
				Status authStatus = domainAuth.Logout(simiasManager.WebServiceUri, simiasManager.DataPath);
				if ( authStatus != null && authStatus.statusCode == StatusCodes.Success )
				{
					result = true;
					domain.DomainInfo.Authenticated = false;
					lvi.Tag = domain;
					// Domain Logged out. Remove Passphrase if store passphrase is not selected..
					if( this.simiasWebService.GetRememberOption(domain.ID) == false)
					{
						//MessageBox.Show("Remember option not selected");
						this.simiasWebService.StorePassPhrase(domain.ID, "", CredentialType.None, false);
					}
					//else
					//	MessageBox.Show("Remember option selected");
				}
			}
			return result;
		}

		private bool processChanges()
		{
			bool result = true;

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

						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("saveSyncError"), resourceManager.GetString("PreferencesErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						mmb.Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				result = false;

				Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), resourceManager.GetString("PreferencesErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
				mmb.ShowDialog();
				mmb.Dispose();
			}

			Cursor.Current = Cursors.Default;

			return result;
		}

		private void resizeButton(Button button)
		{
			Graphics g = button.CreateGraphics();
			try
			{
				Point p = button.Location;
				int width = button.Width;
				// Calculate the size of the string.
				SizeF size = g.MeasureString(button.Text, button.Font);
				button.Width = (int)Math.Ceiling(size.Width) + 20;
				if ((button.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
				{
					button.Left = p.X - (button.Width - width);
				}
			}
			finally
			{
				g.Dispose();
			}
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

		/// <summary>
		/// Delegate used to display the iFolders Dialog.
		/// </summary>
		public delegate void DisplayiFolderDialogDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs after a new account has been created.
		/// </summary>
		public event DisplayiFolderDialogDelegate DisplayiFolderDialog;
		#endregion

		#region Event Handlers
		private void accountWizard_EnterpriseConnect(object sender, DomainConnectEventArgs e)
		{
			AddDomainToList( e.DomainInfo );

			if (EnterpriseConnect != null)
			{
				// Fire the event telling that a new domain has been added.
				EnterpriseConnect( this, new DomainConnectEventArgs( e.DomainInfo ) );
			}
		}

		private void Preferences_Load(object sender, System.EventArgs e)
		{
			// Reference the help using locale-specific path.
			string helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"doc\user\data\preferences.html");
			if (!File.Exists(helpFile))
			{
				// The language help file wasn't found ... default to English.
				helpFile = Path.Combine(Application.StartupPath, @"help\en\doc\user\data\preferences.html");
			}

			if (File.Exists(helpFile))
			{
				helpProvider1.HelpNamespace = helpFile;
			}

			// Load the application icon and banner image.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
			}
			catch {} // Non-fatal ...

			if(Environment.OSVersion.Version.Major > 4 
				& Environment.OSVersion.Version.Minor > 0 
				& System.IO.File.Exists(Application.ExecutablePath + ".manifest"))
			{
//				tabGeneral.BackColor = tabAccounts.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight);
			}

			minimumSyncInterval = minimumSeconds = defaultMinimumSeconds;
			
			timeUnit.Items.Add(resourceManager.GetString("seconds"));
			timeUnit.Items.Add(resourceManager.GetString("minutes"));
			timeUnit.Items.Add(resourceManager.GetString("hours"));
			timeUnit.Items.Add(resourceManager.GetString("days"));
			/*
			timeUnit.Items.Add("seconds");
			timeUnit.Items.Add("minutes");
			timeUnit.Items.Add("hours");
			timeUnit.Items.Add("days");
			*/
			// Resize the buttons
//			resizeButton(login);
//			resizeButton(logout);
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
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readAccountsError"), resourceManager.GetString("accountErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();
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
					minimumSeconds = (!syncInterval.Equals(System.Threading.Timeout.Infinite) &&
						(syncInterval < (int)defaultMinimumSeconds)) ? (decimal)syncInterval : defaultMinimumSeconds;

					displaySyncInterval(syncInterval);
				}
				catch (Exception ex)
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("readSyncError"), resourceManager.GetString("PreferencesErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();
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
				currentDefaultDomain = null;
				newAccountLvi = null;

				// Disable/enable the controls.
				addAccount.Enabled = true;
				details.Enabled = removeAccount.Enabled = false;

				updatePassword = updateEnabled = updateHost = false;

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

			defaultInterval.Enabled = timeUnit.Enabled = label1.Enabled = autoSync.Checked;			
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
		private void addAccount_Click(object sender, System.EventArgs e)
		{
			AccountWizard accountWizard = new AccountWizard( ifWebService, simiasWebService, simiasManager, accounts.Items.Count == 0, this );
			accountWizard.EnterpriseConnect += new Novell.Wizard.AccountWizard.EnterpriseConnectDelegate(accountWizard_EnterpriseConnect);
			if ( accountWizard.ShowDialog() == DialogResult.OK )
			{
				// Display the iFolders dialog.
				if ( DisplayiFolderDialog != null )
				{
					DisplayiFolderDialog( this, new EventArgs() );
				}
			}

			accountWizard.Dispose();
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
				updatePassword = updateEnabled = updateHost = false;
				addAccount.Enabled = true;
			}
			else
			{
				// Remove the enterprise account.
				RemoveAccount removeAccount = new RemoveAccount(domain.DomainInfo);
				if (removeAccount.ShowDialog() == DialogResult.Yes)
				{
					try
					{
						simiasWebService.LeaveDomain(domain.ID, !removeAccount.RemoveAll);
						lvi.Remove();

						string defaultDomainID = null;

						if (domain.Equals(currentDefaultDomain))
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
									break;
								}
							}
						}

						updatePassword = updateEnabled = updateHost = false;
					}
					catch (Exception ex)
					{
						MyMessageBox mmb = new MyMessageBox(resourceManager.GetString("removeAccountError"), resourceManager.GetString("accountErrorTitle"), ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						mmb.Dispose();
					}
				}

				removeAccount.Dispose();
			}
			
		}

		private void timer1_Tick(object sender, System.EventArgs e)
		{
//			timer1.Stop();
//			newAccountLvi.Selected = true;
//			processing = false;
		}

		private void accounts_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if ( accounts.Focused )
			{
				if ( e.CurrentValue == CheckState.Checked )
				{
					if ( !logout( e.Index ) )
					{
						e.NewValue = CheckState.Checked;
					}
					else
					{
						// Call refresh
						(FormsTrayApp.globalProp()).refreshAll();
					}
				}
				else
				{
					if ( !login( e.Index ) )
					{
						e.NewValue = CheckState.Unchecked;
					}
					else
						(FormsTrayApp.globalProp()).refreshAll();
				}
			}
		}

		private void accounts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if ((accounts.SelectedItems.Count == 1) &&
				(accounts.Items.Count > 0))
			{
				ListViewItem lvi = accounts.SelectedItems[0];
				if (lvi != null)
				{
					removeAccount.Enabled = details.Enabled = true;
				}  
			}
			else
			{
				removeAccount.Enabled = details.Enabled = false;
			}
		}

		private void details_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = accounts.SelectedItems[0];
			Domain domain = (Domain)lvi.Tag;
			if (domain != null)
			{
				ServerDetails serverDetails = new ServerDetails(this.simiasWebService, this.ifWebService, domain);
				if ( serverDetails.ShowDialog() == DialogResult.OK )
				{
					// Check if the server was updated.
					if ( serverDetails.EnableChanged || serverDetails.AddressChanged )
					{
						// TODO: need to check the behavior of "Automatically Connect".
						// Need to login
					}

					if ( serverDetails.DefaultChanged )
					{
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
				}

				serverDetails.Dispose();
			}
		}

		#endregion

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			string helpFile;
			switch (tabControl1.SelectedIndex)
			{
				case 0:
					// Reference the help using locale-specific path.
					helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"doc\user\data\preferences.html");
					if (!File.Exists(helpFile))
					{
						// The language help file wasn't found ... default to English.
						helpFile = Path.Combine(Application.StartupPath, @"help\en\doc\user\data\preferences.html");
					}

					if (File.Exists(helpFile))
					{
						helpProvider1.HelpNamespace = helpFile;
					}
					break;
				case 1:
					// Reference the help using locale-specific path.
					helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"doc\user\data\accounts.html");
					if (!File.Exists(helpFile))
					{
						// The language help file wasn't found ... default to English.
						helpFile = Path.Combine(Application.StartupPath, @"help\en\doc\user\data\accounts.html");
					}

					if (File.Exists(helpFile))
					{
						helpProvider1.HelpNamespace = helpFile;
					}
					break;
				case 2:
					// Migration tab clicked
					string iFolderRegistryKey = @"Software\Novell iFolder";
					RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey);
					string[] AllKeys = new string[iFolderKey.SubKeyCount];
					AllKeys = iFolderKey.GetSubKeyNames();
					string total="";
					for ( int i=0;i<AllKeys.Length; i++)
						total += AllKeys[i];
					AddMigrationDetails();
					
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

					// Only set the location if the point is on the screen.
					if (SystemInformation.VirtualScreen.Contains(point))
					{
						this.Location = point;
					}
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

		private void btnMigrate_Click(object sender, EventArgs e)
		{
			ListViewItem lvi = this.listView1.SelectedItems[0];
		//	MigrationWizard migrationWizard = new MigrationWizard( lvi.SubItems[0].Text, lvi.SubItems[1].Text, ifWebService);
		//	accountWizard.EnterpriseConnect += new Novell.Wizard.AccountWizard.EnterpriseConnectDelegate(accountWizard_EnterpriseConnect);
		/*	if ( migrationWizard.ShowDialog() == DialogResult.OK )
			{
				// Display the iFolders dialog.
				if ( DisplayiFolderDialog != null )
				{
					DisplayiFolderDialog( this, new EventArgs() );
				}
			}

			migrationWizard.Dispose();
*/
			/*
			ListViewItem lvi = this.listView1.SelectedItems[0];
			string str= lvi.SubItems[1].Text;
			//CreateiFolder createiFolder = new CreateiFolder();
			//ArrayList domains = new ArrayList();
			//Domain selectedDomain = (Domain)servers.SelectedItem;
			//selectedDomain = selectedDomain.ShowAll ? defaultDomain : selectedDomain;
			//DomainItem selectedDomainItem = null;
			/*
			foreach (Domain d in servers.Items)
			{
				if (!d.ShowAll)
				{
					DomainItem domainItem = new DomainItem(d.Name, d.ID);
					if ((selectedDomain != null) && d.ID.Equals(selectedDomain.ID))
					{
						selectedDomainItem = domainItem;
					}

					domains.Add(domainItem);
				}
			}*/
			/*
			CreateiFolder createiFolder = new CreateiFolder();
			createiFolder.ShowDialog();
			//createiFolder.Servers = domains;
			//createiFolder.SelectedDomain = selectedDomainItem;
			//createiFolder.LoadPath = Application.StartupPath;
			
			//Novell.iFolderCom.MyMessageBox mmb1 = new MyMessageBox(str, resourceManager.GetString("accountErrorTitle"), "nothing", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
			//mmb1.ShowDialog();
			*/
		}
	}
}

