
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
*                 $Author: Bruce Getter <bgetter@novell.com>
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
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Net;
using Microsoft.Win32;

using Novell.iFolder.Web;
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
        private const decimal defaultMinimumSeconds = 300;
        private const decimal maximumSeconds = int.MaxValue;
        private const decimal maximumMinutes = (decimal)(maximumSeconds / 60);
        private const decimal maximumHours = (decimal)(maximumMinutes / 60);
        private const decimal maximumDays = (decimal)(maximumHours / 24);
        private const decimal minmumSecondsAllowed = 5;
        private const string startiFolderinTray = "StartiFolderinTray";
        private const string iFolderRun = "DisableAutoStart";

        private const string hideSyncWindowPopup = "hideSyncWindowPopup";
        private const string hidePolicynotification = "hidePolicynotification";

        public static readonly string iFolderKey = @"SOFTWARE\Novell\iFolder";
        private const string preferencesX = "PreferencesX";
        private const string preferencesY = "PreferencesY";

	//Registry keys for notification preferences
        private const string notifyPolicyQuotaVoilation = "PolicyQuotaVoilation";
        private const string notifyFilePermissionVoilation = "FilePermissionVoilation";
        private const string notifyDiskFullFailure = "DiskFullFailure";
        private const string notifyPolicyTypeVoilation = "PolicyTypeVoilation";
        private const string notifyPolicySizeVoilation = "PolicySizeVoilation";
        private const string notifyIOPermissionFailure = "IOPermissionFailure";
        private const string notifyPathLongFailure = "PathLongFailure";
        private const string notifyShareDisabled = "NotifyShareDisable";
        private const string notifyCollisionDisabled = "NotifyCollisionDisabled";
        private const string notifyJoinDisabled = "NotifyJoinDisabled";

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
        private System.Windows.Forms.Button apply;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.TabPage tabMigrate;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button btnMigrate;
        private Manager simiasManager;
        private System.Windows.Forms.Button btnHelp;
        public Novell.FormsTrayApp.GlobalProperties parent;
        private TabPage tabAccounts;
        private ListView accounts;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader1;
        private Button details;
        private Button removeAccount;
        private Button addAccount;
        private TabPage tabGeneral;
        private GroupBox groupBox4;
        private CheckedListBox notificationsPreferencesList;
        private GroupBox groupBox1;
        private Label label1;
        private ComboBox timeUnit;
        private NumericUpDown defaultInterval;
        private CheckBox autoSync;
        private GroupBox groupBox3;
        private CheckBox autoStart;
        private CheckBox displayConfirmation;
        private CheckBox displayTrayIcon;
        private CheckBox startInTrayIcon;
        private CheckBox hideSyncLog;
        private TabControl tabControl1;
        public string str;


        enum policyVoilation
        {
            QuotaVoliation,
            FileSizeVoilation,
            FileTypeVoilation,
            DiskFullVoilation,
            PremissionUnavailable,
	    LongPath,
	    iFolderShared,
	    Collisions
        };

        enum preferenceTab
        {
            General = 0,
            Accounts = 1,
            Settings = 2
        };

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
           // defaultInterval.LostFocus += new EventHandler(defaultInterval_LostFocus);
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Preferences));
            this.tabMigrate = new System.Windows.Forms.TabPage();
            this.btnMigrate = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.cancel = new System.Windows.Forms.Button();
            this.apply = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.btnHelp = new System.Windows.Forms.Button();
            this.tabAccounts = new System.Windows.Forms.TabPage();
            this.accounts = new System.Windows.Forms.ListView();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.details = new System.Windows.Forms.Button();
            this.removeAccount = new System.Windows.Forms.Button();
            this.addAccount = new System.Windows.Forms.Button();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.notificationsPreferencesList = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timeUnit = new System.Windows.Forms.ComboBox();
            this.defaultInterval = new System.Windows.Forms.NumericUpDown();
            this.autoSync = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.autoStart = new System.Windows.Forms.CheckBox();
            this.displayConfirmation = new System.Windows.Forms.CheckBox();
            this.displayTrayIcon = new System.Windows.Forms.CheckBox();
            this.startInTrayIcon = new System.Windows.Forms.CheckBox();
            this.hideSyncLog = new System.Windows.Forms.CheckBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabMigrate.SuspendLayout();
            this.tabAccounts.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabMigrate
            // 
            this.tabMigrate.Controls.Add(this.btnMigrate);
            this.tabMigrate.Controls.Add(this.listView1);
            resources.ApplyResources(this.tabMigrate, "tabMigrate");
            this.tabMigrate.Name = "tabMigrate";
            this.helpProvider1.SetShowHelp(this.tabMigrate, ((bool)(resources.GetObject("tabMigrate.ShowHelp"))));
            // 
            // btnMigrate
            // 
            resources.ApplyResources(this.btnMigrate, "btnMigrate");
            this.btnMigrate.Name = "btnMigrate";
            this.helpProvider1.SetShowHelp(this.btnMigrate, ((bool)(resources.GetObject("btnMigrate.ShowHelp"))));
            this.btnMigrate.Click += new System.EventHandler(this.btnMigrate_Click);
            // 
            // listView1
            // 
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Name = "listView1";
            this.helpProvider1.SetShowHelp(this.listView1, ((bool)(resources.GetObject("listView1.ShowHelp"))));
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Name = "cancel";
            this.helpProvider1.SetShowHelp(this.cancel, ((bool)(resources.GetObject("cancel.ShowHelp"))));
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // apply
            // 
            resources.ApplyResources(this.apply, "apply");
            this.apply.Name = "apply";
            this.helpProvider1.SetShowHelp(this.apply, ((bool)(resources.GetObject("apply.ShowHelp"))));
            this.apply.Click += new System.EventHandler(this.apply_Click);
            // 
            // ok
            // 
            resources.ApplyResources(this.ok, "ok");
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Name = "ok";
            this.helpProvider1.SetShowHelp(this.ok, ((bool)(resources.GetObject("ok.ShowHelp"))));
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnHelp
            // 
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.Name = "btnHelp";
            this.helpProvider1.SetShowHelp(this.btnHelp, ((bool)(resources.GetObject("btnHelp.ShowHelp"))));
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // tabAccounts
            // 
            this.tabAccounts.Controls.Add(this.accounts);
            this.tabAccounts.Controls.Add(this.details);
            this.tabAccounts.Controls.Add(this.removeAccount);
            this.tabAccounts.Controls.Add(this.addAccount);
            resources.ApplyResources(this.tabAccounts, "tabAccounts");
            this.tabAccounts.Name = "tabAccounts";
            this.helpProvider1.SetShowHelp(this.tabAccounts, ((bool)(resources.GetObject("tabAccounts.ShowHelp"))));
            this.tabAccounts.UseVisualStyleBackColor = true;
            // 
            // accounts
            // 
            resources.ApplyResources(this.accounts, "accounts");
            this.accounts.CheckBoxes = true;
            this.accounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader2,
            this.columnHeader1});
            this.accounts.FullRowSelect = true;
            this.helpProvider1.SetHelpString(this.accounts, resources.GetString("accounts.HelpString"));
            this.accounts.HideSelection = false;
            this.accounts.MultiSelect = false;
            this.accounts.Name = "accounts";
            this.helpProvider1.SetShowHelp(this.accounts, ((bool)(resources.GetObject("accounts.ShowHelp"))));
            this.accounts.UseCompatibleStateImageBehavior = false;
            this.accounts.View = System.Windows.Forms.View.Details;
            this.accounts.SelectedIndexChanged += new System.EventHandler(this.accounts_SelectedIndexChanged);
            this.accounts.DoubleClick += new System.EventHandler(this.details_Click);
            this.accounts.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.accounts_ItemCheck);
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // details
            // 
            resources.ApplyResources(this.details, "details");
            this.helpProvider1.SetHelpString(this.details, resources.GetString("details.HelpString"));
            this.details.Name = "details";
            this.helpProvider1.SetShowHelp(this.details, ((bool)(resources.GetObject("details.ShowHelp"))));
            this.details.Click += new System.EventHandler(this.details_Click);
            // 
            // removeAccount
            // 
            resources.ApplyResources(this.removeAccount, "removeAccount");
            this.helpProvider1.SetHelpString(this.removeAccount, resources.GetString("removeAccount.HelpString"));
            this.removeAccount.Name = "removeAccount";
            this.helpProvider1.SetShowHelp(this.removeAccount, ((bool)(resources.GetObject("removeAccount.ShowHelp"))));
            this.removeAccount.Click += new System.EventHandler(this.removeAccount_Click);
            // 
            // addAccount
            // 
            resources.ApplyResources(this.addAccount, "addAccount");
            this.helpProvider1.SetHelpString(this.addAccount, resources.GetString("addAccount.HelpString"));
            this.addAccount.Name = "addAccount";
            this.helpProvider1.SetShowHelp(this.addAccount, ((bool)(resources.GetObject("addAccount.ShowHelp"))));
            this.addAccount.Click += new System.EventHandler(this.addAccount_Click);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBox4);
            this.tabGeneral.Controls.Add(this.groupBox1);
            this.tabGeneral.Controls.Add(this.groupBox3);
            resources.ApplyResources(this.tabGeneral, "tabGeneral");
            this.tabGeneral.Name = "tabGeneral";
            this.helpProvider1.SetShowHelp(this.tabGeneral, ((bool)(resources.GetObject("tabGeneral.ShowHelp"))));
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Controls.Add(this.notificationsPreferencesList);
            this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox4.Name = "groupBox4";
            this.helpProvider1.SetShowHelp(this.groupBox4, ((bool)(resources.GetObject("groupBox4.ShowHelp"))));
            this.groupBox4.TabStop = false;
            // 
            // notificationsPreferencesList
            // 
            this.notificationsPreferencesList.FormattingEnabled = true;
            this.notificationsPreferencesList.Items.AddRange(new object[] {
            resources.GetString("notificationsPreferencesList.Items"),
            resources.GetString("notificationsPreferencesList.Items1"),
            resources.GetString("notificationsPreferencesList.Items2"),
            resources.GetString("notificationsPreferencesList.Items3"),
            resources.GetString("notificationsPreferencesList.Items4"),
            resources.GetString("notificationsPreferencesList.Items5"),
            resources.GetString("notificationsPreferencesList.Items6"),
            resources.GetString("notificationsPreferencesList.Items7")});
            resources.ApplyResources(this.notificationsPreferencesList, "notificationsPreferencesList");
            this.notificationsPreferencesList.Name = "notificationsPreferencesList";
            this.helpProvider1.SetShowHelp(this.notificationsPreferencesList, ((bool)(resources.GetObject("notificationsPreferencesList.ShowHelp"))));
            this.notificationsPreferencesList.SelectedIndexChanged += new System.EventHandler(this.notificationsPreferencesList_SelectedIndexChanged);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.timeUnit);
            this.groupBox1.Controls.Add(this.defaultInterval);
            this.groupBox1.Controls.Add(this.autoSync);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Name = "groupBox1";
            this.helpProvider1.SetShowHelp(this.groupBox1, ((bool)(resources.GetObject("groupBox1.ShowHelp"))));
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.helpProvider1.SetShowHelp(this.label1, ((bool)(resources.GetObject("label1.ShowHelp"))));
            // 
            // timeUnit
            // 
            resources.ApplyResources(this.timeUnit, "timeUnit");
            this.timeUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.timeUnit.Name = "timeUnit";
            this.helpProvider1.SetShowHelp(this.timeUnit, ((bool)(resources.GetObject("timeUnit.ShowHelp"))));
            this.timeUnit.SelectedIndexChanged += new System.EventHandler(this.timeUnit_SelectedIndexChanged);
            // 
            // defaultInterval
            // 
            resources.ApplyResources(this.defaultInterval, "defaultInterval");
            this.helpProvider1.SetHelpString(this.defaultInterval, resources.GetString("defaultInterval.HelpString"));
            this.defaultInterval.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.defaultInterval.Maximum = new decimal(new int[] {
            2147483647,
            0,
            0,
            0});
            this.defaultInterval.Name = "defaultInterval";
            this.helpProvider1.SetShowHelp(this.defaultInterval, ((bool)(resources.GetObject("defaultInterval.ShowHelp"))));
            this.defaultInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.defaultInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.defaultInterval_KeyDown);
            // 
            // autoSync
            // 
            this.autoSync.Checked = true;
            this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.autoSync, "autoSync");
            this.helpProvider1.SetHelpString(this.autoSync, resources.GetString("autoSync.HelpString"));
            this.autoSync.Name = "autoSync";
            this.helpProvider1.SetShowHelp(this.autoSync, ((bool)(resources.GetObject("autoSync.ShowHelp"))));
            this.autoSync.CheckedChanged += new System.EventHandler(this.autoSync_CheckedChanged);
            // 
            // groupBox3
            // 
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Controls.Add(this.autoStart);
            this.groupBox3.Controls.Add(this.displayConfirmation);
            this.groupBox3.Controls.Add(this.displayTrayIcon);
            this.groupBox3.Controls.Add(this.startInTrayIcon);
            this.groupBox3.Controls.Add(this.hideSyncLog);
            this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox3.Name = "groupBox3";
            this.helpProvider1.SetShowHelp(this.groupBox3, ((bool)(resources.GetObject("groupBox3.ShowHelp"))));
            this.groupBox3.TabStop = false;
            // 
            // autoStart
            // 
            resources.ApplyResources(this.autoStart, "autoStart");
            this.helpProvider1.SetHelpString(this.autoStart, resources.GetString("autoStart.HelpString"));
            this.autoStart.Name = "autoStart";
            this.helpProvider1.SetShowHelp(this.autoStart, ((bool)(resources.GetObject("autoStart.ShowHelp"))));
            this.autoStart.CheckedChanged += new System.EventHandler(this.autoStart_CheckedChanged);
            // 
            // displayConfirmation
            // 
            resources.ApplyResources(this.displayConfirmation, "displayConfirmation");
            this.helpProvider1.SetHelpString(this.displayConfirmation, resources.GetString("displayConfirmation.HelpString"));
            this.displayConfirmation.Name = "displayConfirmation";
            this.helpProvider1.SetShowHelp(this.displayConfirmation, ((bool)(resources.GetObject("displayConfirmation.ShowHelp"))));
            this.displayConfirmation.CheckedChanged += new System.EventHandler(this.displayConfirmation_CheckedChanged);
            // 
            // displayTrayIcon
            // 
            resources.ApplyResources(this.displayTrayIcon, "displayTrayIcon");
            this.helpProvider1.SetHelpString(this.displayTrayIcon, resources.GetString("displayTrayIcon.HelpString"));
            this.displayTrayIcon.Name = "displayTrayIcon";
            this.helpProvider1.SetShowHelp(this.displayTrayIcon, ((bool)(resources.GetObject("displayTrayIcon.ShowHelp"))));
            this.displayTrayIcon.CheckedChanged += new System.EventHandler(this.displayTrayIcon_CheckedChanged);
            // 
            // startInTrayIcon
            // 
            resources.ApplyResources(this.startInTrayIcon, "startInTrayIcon");
            this.helpProvider1.SetHelpString(this.startInTrayIcon, resources.GetString("startInTrayIcon.HelpString"));
            this.startInTrayIcon.Name = "startInTrayIcon";
            this.helpProvider1.SetShowHelp(this.startInTrayIcon, ((bool)(resources.GetObject("startInTrayIcon.ShowHelp"))));
            this.startInTrayIcon.CheckedChanged += new System.EventHandler(this.startInTrayIcon_CheckedChanged);
            // 
            // hideSyncLog
            // 
            resources.ApplyResources(this.hideSyncLog, "hideSyncLog");
            this.helpProvider1.SetHelpString(this.hideSyncLog, resources.GetString("hideSyncLog.HelpString"));
            this.hideSyncLog.Name = "hideSyncLog";
            this.helpProvider1.SetShowHelp(this.hideSyncLog, ((bool)(resources.GetObject("hideSyncLog.ShowHelp"))));
            this.hideSyncLog.CheckedChanged += new System.EventHandler(this.hideSyncLog_CheckedChanged);
            // 
            // tabControl1
            // 
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabAccounts);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.helpProvider1.SetShowHelp(this.tabControl1, ((bool)(resources.GetObject("tabControl1.ShowHelp"))));
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // Preferences
            // 
            this.AcceptButton = this.ok;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.CancelButton = this.cancel;
            this.Controls.Add(this.ok);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.apply);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.btnHelp);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Preferences";
            this.helpProvider1.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            this.Load += new System.EventHandler(this.Preferences_Load);
            this.VisibleChanged += new System.EventHandler(this.Preferences_VisibleChanged);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Preferences_Closing);
            this.Move += new System.EventHandler(this.Preferences_Move);
            this.tabMigrate.ResumeLayout(false);
            this.tabAccounts.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value indicating if shared iFolder notifications should be displayed.
        /// </summary>
        public static bool NotifyShareEnabled
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
        public static bool NotifyCollisionEnabled
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
        public static bool NotifyJoinEnabled
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




        public static bool HideiFolderInTray
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(startiFolderinTray, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(startiFolderinTray, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(startiFolderinTray, 1);
                }
            }
        }
 
        
        public static bool HideSyncLogWindow
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(hideSyncWindowPopup, 1);
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
                    
                    regKey.SetValue(hideSyncWindowPopup, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(hideSyncWindowPopup, 1);
                }
            }
        }



        
        public static bool HidePolicyVoilationNotification
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(hidePolicynotification, 1);
                    
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
                    
                    regKey.SetValue(hidePolicynotification, 0);
                }
                else
                {
                    
                    // Set the disable value.
                    regKey.SetValue(hidePolicynotification, 1);
                }
            }
        }


        public static bool NotifyPolicyQouta
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyPolicyQuotaVoilation, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyPolicyQuotaVoilation, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyPolicyQuotaVoilation, 1);
                }
            }
        }


        public static bool NotifyFilePermission
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyFilePermissionVoilation, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyFilePermissionVoilation, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyFilePermissionVoilation, 1);
                }
            }
        }


        public static bool NotifyDiskFull
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyDiskFullFailure, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyDiskFullFailure, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyDiskFullFailure, 1);
                }
            }
        }


        public static bool NotifyPolicyType
        {
            //TODO: get function can be removed, as not being used now OR can be kept for future use
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyPolicyTypeVoilation, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyPolicyTypeVoilation, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyPolicyTypeVoilation, 1);
                }
            }
        }


        public static bool NotifyPolicySize
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyPolicySizeVoilation, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyPolicySizeVoilation, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyPolicySizeVoilation, 1);
                }
            }
        }


        public static bool NotifyIOPermission
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyIOPermissionFailure, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyIOPermissionFailure, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyIOPermissionFailure, 1);
                }
            }
        }


        public static bool NotifyPathLong
        {
            get
            {
                int notify;
                try
                {
                    // Create/open the iFolder key.
                    RegistryKey regKey = Registry.CurrentUser.CreateSubKey(iFolderKey);

                    // Get the notify share value ... default the value to 0 (enabled).
                    notify = (int)regKey.GetValue(notifyPathLongFailure, 1);
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
                    //regKey.DeleteValue(startiFolderinTray, false);
                    //set enable value
                    regKey.SetValue(notifyPathLongFailure, 0);
                }
                else
                {
                    // Set the disable value.
                    regKey.SetValue(notifyPathLongFailure, 1);
                }
            }
        }


        public iFolderWebService ifolderWebService
        {
            set
            {
                this.ifWebService = value;
            }
        }

        public SimiasWebService Simws
        {
            set
            {
                this.simiasWebService = value;
            }
        }

        public Manager simManager
        {
            set
            {
                this.simiasManager = value;
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
                    if ((currentDefaultDomain != null) && !currentDefaultDomain.ID.Equals(domainInfo.ID))
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
									 domainInfo.MemberName });
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

                if (d.DomainInfo.POBoxID != null && d.DomainInfo.POBoxID.Equals(poBoxID))
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
            /*
            foreach (ListViewItem lvi in accounts.Items)
            {
                Domain d = (Domain)lvi.Tag;

                if (d.DomainInfo.POBoxID.Equals(poBoxID))
                {
                    result = true;
                    break;
                }
            }
            */
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
            catch { }
        }

        public void SetProxyForDomain(string hostUrl, bool unknownScheme)
        {
            UriBuilder ubHost = new UriBuilder(hostUrl);

            // If a domain name was passed in without a scheme, a proxy will
            // need to be setup for both http and https schemes because we don't
            // know how it will ultimately be sent.
            if (unknownScheme)
            {
                // Set the proxy for http.
                ubHost.Scheme = Uri.UriSchemeHttp;
                ubHost.Port = 80;
                SetProxyForDomain(ubHost.Uri.ToString(), false);

                // Now set it for https.
                ubHost.Scheme = Uri.UriSchemeHttps;
                ubHost.Port = 443;
                SetProxyForDomain(ubHost.Uri.ToString(), false);
            }
            else
            {
                // Set any proxy information for this domain.
                IWebProxy iwp = WebRequest.GetSystemWebProxy();
                if (!iwp.IsBypassed(ubHost.Uri))
                {
                    string proxyUser = null;
                    string proxyPassword = null;

                    Uri proxyUri = iwp.GetProxy(ubHost.Uri);
                    if (iwp.Credentials != null)
                    {
                        NetworkCredential netCred = iwp.Credentials.GetCredential(proxyUri, "Basic");
                        if (netCred != null)
                        {
                            proxyUser = netCred.UserName;
                            proxyPassword = netCred.Password;
                        }
                    }

                    // The scheme for the proxy address needs to match the scheme for the host address.
                    simiasWebService.SetProxyAddress(ubHost.Uri.ToString(), proxyUri.ToString(), proxyUser, proxyPassword);
                }
            }
        }

        /// <summary>
        /// Gets if Shutdown or logoff message is received.
        /// </summary>
        public bool MachineShutdown()
        {
            return this.shutdown;
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

            for (int i = 0; i < AllKeys.Length; i++)
            {
                ListViewItem lvi;
                User = iFolderRegistryKey + "\\" + AllKeys[i];
                RegistryKey UserKey = Registry.LocalMachine.OpenSubKey(User);
                if (UserKey.GetValue("FolderPath") != null)
                {
                    lvi = new ListViewItem(new string[] { AllKeys[i], (string)UserKey.GetValue("FolderPath") });
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
            //timeUnit.SelectedItem = units;
            switch (units)
            {
                case "seconds":
                    timeUnit.SelectedIndex = 0;
                    break;
                case "minutes":
                    timeUnit.SelectedIndex = 1;
                    break;
                case "hours":
                    timeUnit.SelectedIndex = 2;
                    break;
                case "days":
                    timeUnit.SelectedIndex = 3;
                    break;
            }

            // Display the interval.
            try
            {
                defaultInterval.Value = displayValue;
            }
            catch (ArgumentOutOfRangeException ae)
            {
                defaultInterval.Value = syncInterval;
                timeUnit.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private bool login(int itemIndex)
        {
            bool result = false;

            ListViewItem lvi = accounts.Items[itemIndex];
            Domain domain = (Domain)lvi.Tag;
            result = loginToDomain(domain);
            if (result)
                lvi.Tag = domain;

            return result;
        }

        public bool loginToDomain(Domain domain)
        {
            bool result = false;
            if (domain != null)
            {
                Connecting connecting = new Connecting(this.ifWebService, simiasWebService, simiasManager, domain.DomainInfo);
                if (connecting.ShowDialog() == DialogResult.OK)
                {
                    result = true;
                }

                if (!result)
                {
                    ServerInfo serverInfo = new ServerInfo(this.ifWebService, simiasManager, domain.DomainInfo, connecting.Password);
                    serverInfo.ShowDialog();
                    result = serverInfo.DomainInfo.Authenticated;
                    serverInfo.Dispose();
                }

                connecting.Dispose();
            }

            if (result)
            {
                domain.DomainInfo.Authenticated = true;
                FormsTrayApp.globalProp().updateifListViewDomainStatus(domain.DomainInfo.ID, true);
                FormsTrayApp.globalProp().AddDomainToUIList(domain.DomainInfo);
                FormsTrayApp.globalProp().UpdateiFolderStatus(domain.DomainInfo.Authenticated, domain.DomainInfo.ID);
            }
            return result;
        }


      


        private bool logout(int itemIndex)
        {
            bool result = false;

            ListViewItem lvi = accounts.Items[itemIndex];
            Domain domain = (Domain)lvi.Tag;
            result = logoutFromDomain(domain);
            if(result)
                lvi.Tag = domain;

            return result;
        }

        public bool logoutFromDomain(Domain domain)
        {
            bool result = false;
            if (domain != null)
            {
                DomainAuthentication domainAuth = new DomainAuthentication("iFolder", domain.ID, null);
                Status authStatus = domainAuth.Logout(simiasManager.WebServiceUri, simiasManager.DataPath);
                if (authStatus != null && authStatus.statusCode == StatusCodes.Success)
                {
                    result = true;
                    domain.DomainInfo.Authenticated = false;

                    // Domain Logged out. Remove Passphrase if store passphrase is not selected..
                    if (this.simiasWebService.GetRememberOption(domain.ID) == false)
                    {
                        this.simiasWebService.StorePassPhrase(domain.ID, "", CredentialType.None, false);
                    }
                    FormsTrayApp.globalProp().updateifListViewDomainStatus(domain.DomainInfo.ID, false);
                    (FormsTrayApp.globalProp()).RemoveDomainFromUIList(domain.DomainInfo.ID, null);
                    FormsTrayApp.globalProp().UpdateiFolderStatus(domain.DomainInfo.Authenticated, domain.DomainInfo.ID); 
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

                NotifyPolicyQouta = notificationsPreferencesList.GetItemChecked((int)policyVoilation.QuotaVoliation);
                NotifyPolicySize = notificationsPreferencesList.GetItemChecked((int)policyVoilation.FileSizeVoilation);
                NotifyPolicyType = notificationsPreferencesList.GetItemChecked((int)policyVoilation.FileTypeVoilation);
                NotifyPathLong = notificationsPreferencesList.GetItemChecked((int)policyVoilation.LongPath);
                NotifyIOPermission = notificationsPreferencesList.GetItemChecked((int)policyVoilation.PremissionUnavailable);
                NotifyDiskFull = notificationsPreferencesList.GetItemChecked((int)policyVoilation.DiskFullVoilation);
                NotifyShareEnabled = notificationsPreferencesList.GetItemChecked((int)policyVoilation.iFolderShared);
                NotifyCollisionEnabled = notificationsPreferencesList.GetItemChecked((int)policyVoilation.Collisions);

            NotifyShareEnabled = notificationsPreferencesList.GetItemChecked((int)policyVoilation.iFolderShared);

            HideiFolderInTray = startInTrayIcon.Checked;
            HideSyncLogWindow = hideSyncLog.Checked;


            // Check and update display confirmation setting.
            iFolderComponent.DisplayConfirmationEnabled = displayConfirmation.Checked;

            iFolderComponent.DisplayTrayIconEnabled = !(displayTrayIcon.Checked);
            if (displayTrayIcon.Checked)
                FormsTrayApp.SetTrayIconStatus(false);
            else
                FormsTrayApp.SetTrayIconStatus(true);

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
            AddDomainToList(e.DomainInfo);

            if (EnterpriseConnect != null)
            {
                // Fire the event telling that a new domain has been added.
                EnterpriseConnect(this, new DomainConnectEventArgs(e.DomainInfo));
            }
        }

        private void Preferences_Load(object sender, System.EventArgs e)
        {
            // Reference the help using locale-specific path.
            string helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"preferences.html");
            if (!File.Exists(helpFile))
            {
                // The language help file wasn't found ... default to English.
                helpFile = Path.Combine(Application.StartupPath, @"help\en\preferences.html");
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
            catch { } // Non-fatal ...

            if (Environment.OSVersion.Version.Major > 4
                & Environment.OSVersion.Version.Minor > 0
                & System.IO.File.Exists(Application.ExecutablePath + ".manifest"))
            {
                //				tabGeneral.BackColor = tabAccounts.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight);
            }

            minimumSyncInterval = 1;


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
                startInTrayIcon.Checked = HideiFolderInTray;

                
                hideSyncLog.Checked = HideSyncLogWindow;

                // Update the display confirmation setting.
                displayConfirmation.Checked = iFolderComponent.DisplayConfirmationEnabled;
                displayTrayIcon.Checked = !(iFolderComponent.DisplayTrayIconEnabled);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.QuotaVoliation, NotifyPolicyQouta);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.FileSizeVoilation, NotifyPolicySize);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.FileTypeVoilation, NotifyPolicyType);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.LongPath, NotifyPathLong);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.PremissionUnavailable, NotifyIOPermission);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.DiskFullVoilation, NotifyDiskFull);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.iFolderShared, NotifyShareEnabled);
            notificationsPreferencesList.SetItemChecked((int)policyVoilation.Collisions, NotifyCollisionEnabled);

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
            verifyInterval();
            // If this fails don't dismiss the dialog.
            successful = processChanges();
            Close();
        }

        private void displayTrayIcon_CheckedChanged(object sender, System.EventArgs e)
        {
            if (displayTrayIcon.Focused)
            {
                apply.Enabled = true;
            }
        }

        private void startInTrayIcon_CheckedChanged(object sender, System.EventArgs e)
        {
            if (startInTrayIcon.Focused)
            {
                apply.Enabled = true;
            }
        }

        private void hideSyncLog_CheckedChanged(object sender, System.EventArgs e)
        {
            if (hideSyncLog.Focused)
            {
                apply.Enabled = true;
            }
        }

        private void apply_Click(object sender, System.EventArgs e)
        {
            verifyInterval();
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

                defaultInterval.Maximum = maximumSeconds;
                defaultInterval.Increment = 1;

                
                // minimumSyncInterval = defaultInterval.Value;
            }
            else if (((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("minutes")))
            {
                minimumSyncInterval = 1;
                defaultInterval.Increment = 1;
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
            AccountWizard accountWizard = new AccountWizard(ifWebService, simiasWebService, simiasManager, accounts.Items.Count == 0, this, (GlobalProperties)FormsTrayApp.globalProp());
            accountWizard.EnterpriseConnect += new Novell.Wizard.AccountWizard.EnterpriseConnectDelegate(accountWizard_EnterpriseConnect);
            if (accountWizard.ShowDialog() == DialogResult.OK)
            {
                // Display the iFolders dialog.
                if (DisplayiFolderDialog != null)
                {
                    DisplayiFolderDialog(this, new EventArgs());
                }
            }

            accountWizard.Dispose();
            this.Focus();
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
                    Cursor = Cursors.WaitCursor;
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
                try
                {
                    DomainInformation[] domains;
                    System.Threading.Thread.Sleep(2000);
                    domains = this.simiasWebService.GetDomains(false);
                    if (domains.Length.Equals(0))
                    {
                        if (((GlobalProperties)FormsTrayApp.globalProp()).Visible)
                            ((GlobalProperties)FormsTrayApp.globalProp()).Hide();
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
                if (removeAccount != null)
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
            if (accounts.Focused)
            {
                if (e.CurrentValue == CheckState.Checked)
                {
                    if (!logout(e.Index))
                    {
                        e.NewValue = CheckState.Checked;
                    }
                    else
                    {
                        // Call refresh
                        // in connecting.cs , we already call refreshall
                        //(FormsTrayApp.globalProp()).refreshAll();
                    }
                }
                else
                {
                    if (!login(e.Index))
                    {
                        e.NewValue = CheckState.Unchecked;
                    }
                    else
                    {
                        //(FormsTrayApp.globalProp()).refreshAll();
                    }
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
                if (serverDetails.ShowDialog() == DialogResult.OK)
                {
                    // Check if the server was updated.
                    if (serverDetails.EnableChanged || serverDetails.AddressChanged)
                    {
                        // TODO: need to check the behavior of "Automatically Connect".
                        // Need to login
                    }

                    if (serverDetails.DefaultChanged)
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
                    helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"preferences.html");
                    if (!File.Exists(helpFile))
                    {
                        // The language help file wasn't found ... default to English.
                        helpFile = Path.Combine(Application.StartupPath, @"help\en\preferences.html");
                    }

                    if (File.Exists(helpFile))
                    {
                        helpProvider1.HelpNamespace = helpFile;
                    }
                    break;
                case 1:
                    // Reference the help using locale-specific path.
                    helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"accounts.html");
                    if (!File.Exists(helpFile))
                    {
                        // The language help file wasn't found ... default to English.
                        helpFile = Path.Combine(Application.StartupPath, @"help\en\accounts.html");
                    }

                    if (File.Exists(helpFile))
                    {
                        helpProvider1.HelpNamespace = helpFile;
                    }
                    break;
                case 2:
                    // Migration tab clicked
                 /* string iFolderRegistryKey = @"Software\Novell iFolder";
                    RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey);
                    string[] AllKeys = new string[iFolderKey.SubKeyCount];
                    AllKeys = iFolderKey.GetSubKeyNames();
                    string total = "";
                    for (int i = 0; i < AllKeys.Length; i++)
                        total += AllKeys[i];
                    AddMigrationDetails();

                  */ 
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
                catch { }
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
                catch { }

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
                    {
#if DEBUG
                        MessageBox.Show("Shutdown msg got - Preferences");
#endif
                        this.shutdown = true;
                        break;
                    }
            }

            base.WndProc(ref m);
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

        private void btnHelp_Click(object sender, System.EventArgs e)
        {
            string helpFile = null;
            switch(this.tabControl1.SelectedIndex)
            {
                case (int)preferenceTab.General:
                    helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"preferences.html");
                    break;
                case (int)preferenceTab.Accounts:
                    helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"accounts.html");
                    break;
                case (int)preferenceTab.Settings:
                    helpFile = Path.Combine(Path.Combine(Path.Combine(Application.StartupPath, "help"), iFolderAdvanced.GetLanguageDirectory()), @"settings.html");
                    break;
                default:
                    break;

            }
            new iFolderComponent().ShowHelp(Application.StartupPath, helpFile);
        }

        private void verifyInterval()
        {
            //this function perform verification w.r.t to the minmum allowed value
            if ((defaultInterval.Value < minmumSecondsAllowed) && ((string)timeUnit.SelectedItem).Equals(resourceManager.GetString("seconds")))
            {
                defaultInterval.Value = minmumSecondsAllowed;
                //Alert Message
                MessageBox.Show(str,"Synchronization Interval Limit!", 0, MessageBoxIcon.Information);
            }

        }

        private void notificationsPreferencesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!displayTrayIcon.Focused)
            {
                apply.Enabled = true;
            }
        }
    }
}
