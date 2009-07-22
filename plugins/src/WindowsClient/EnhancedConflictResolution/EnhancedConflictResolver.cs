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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using Novell.iFolder.Web;
using Novell.iFolderCom;
using Novell.Win32Util;

namespace Novell.EnhancedConflictResolution
{
    /// <summary>
    /// Summary description for ConflictResolver.
    /// </summary>
    [ComVisible(false)]
    public class EnhancedConflictResolver : ConflictResolver
    {
        private GroupBox actionGroupBox;
        private Panel iFolderSrvPanel;
        private Label iFolderSrvLabel;
        private Panel conflictBinPanel;
        private Label conflictBinLabel;
        private RadioButton iFolderLocalVersionRdBtn;
        private Button actionSave;
        private Button browseBtn;
        private RadioButton iFolderServerVersionRdBtn;
        private RadioButton conflictBinSvrRdBtn;
        private RadioButton conflictBinLocalRdBtn;
        private TextBox pathTxtBox;
        private Label conflictBinPathLbl;
        private CheckBox conflictBinEnable;
        System.ComponentModel.ComponentResourceManager resources;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="startupPath"></param>
        /// <param name="iFolder"></param>
        public EnhancedConflictResolver(iFolderWebService service, string startupPath, iFolderWeb iFolder)
            : base(4)
        {
            InitializeComponent();
            this.MinimumSize = this.Size;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ifWebService = service;
            this.loadPath = startupPath;
            this.ifolder = iFolder;
        }

        /// <summary>
        /// Initialize this object
        /// </summary>
        private void AllocateMemForNewComponents()
        {
            this.iFolderSrvPanel = new System.Windows.Forms.Panel();
            this.iFolderServerVersionRdBtn = new System.Windows.Forms.RadioButton();
            this.iFolderLocalVersionRdBtn = new System.Windows.Forms.RadioButton();
            this.iFolderSrvLabel = new System.Windows.Forms.Label();
            this.conflictBinPanel = new System.Windows.Forms.Panel();
            this.conflictBinSvrRdBtn = new System.Windows.Forms.RadioButton();
            this.conflictBinLocalRdBtn = new System.Windows.Forms.RadioButton();
            this.conflictBinLabel = new System.Windows.Forms.Label();
            this.actionSave = new System.Windows.Forms.Button();
            this.browseBtn = new System.Windows.Forms.Button();
            this.pathTxtBox = new System.Windows.Forms.TextBox();
            this.conflictBinPathLbl = new System.Windows.Forms.Label();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.conflictBinEnable = new System.Windows.Forms.CheckBox();
        }

        /// <summary>
        /// Suspend the layout for server, conflictbin and actiongroup box
        /// </summary>
        private void SuspendLayoutForNewComponents()
        {
            this.iFolderSrvPanel.SuspendLayout();
            this.conflictBinPanel.SuspendLayout();
            this.actionGroupBox.SuspendLayout();
        }

        /// <summary>
        /// esume the layout for server, conflictbin and actiongroup box
        /// </summary>
        private void ResumeLayoutForNewComponents()
        {
            this.iFolderSrvPanel.ResumeLayout(false);
            this.iFolderSrvPanel.PerformLayout();
            this.conflictBinPanel.ResumeLayout(false);
            this.conflictBinPanel.PerformLayout();
            this.actionGroupBox.ResumeLayout(false);
            this.actionGroupBox.PerformLayout();
        }

        /// <summary>
        /// Property initialization for resources
        /// </summary>
        /// <param name="resources"></param>
        private void PropInitForNewComponents(System.ComponentModel.ComponentResourceManager resources)
        {
            // 
            // iFolderSrvPanel
            // 
            this.iFolderSrvPanel.Controls.Add(this.iFolderServerVersionRdBtn);
            this.iFolderSrvPanel.Controls.Add(this.iFolderLocalVersionRdBtn);
            this.iFolderSrvPanel.Controls.Add(this.iFolderSrvLabel);
            resources.ApplyResources(this.iFolderSrvPanel, "iFolderSrvPanel");
            this.iFolderSrvPanel.Name = "iFolderSrvPanel";
            this.helpProvider1.SetShowHelp(this.iFolderSrvPanel, ((bool)(resources.GetObject("iFolderSrvPanel.ShowHelp"))));
            // 
            // iFolderServerVersionRdBtn
            // 
            resources.ApplyResources(this.iFolderServerVersionRdBtn, "iFolderServerVersionRdBtn");
            this.iFolderServerVersionRdBtn.Name = "iFolderServerVersionRdBtn";
            this.helpProvider1.SetShowHelp(this.iFolderServerVersionRdBtn, ((bool)(resources.GetObject("iFolderServerVersionRdBtn.ShowHelp"))));
            this.iFolderServerVersionRdBtn.TabStop = true;
            this.iFolderServerVersionRdBtn.UseVisualStyleBackColor = true;

            // 
            // iFolderLocalVersionRdBtn
            // 
            resources.ApplyResources(this.iFolderLocalVersionRdBtn, "iFolderLocalVersionRdBtn");
            this.iFolderLocalVersionRdBtn.Name = "iFolderLocalVersionRdBtn";
            this.iFolderLocalVersionRdBtn.Checked = true;
            this.iFolderLocalVersionRdBtn.TabStop = true;
            this.iFolderLocalVersionRdBtn.UseVisualStyleBackColor = true;

            // 
            // iFolderSrvLabel
            // 
            resources.ApplyResources(this.iFolderSrvLabel, "iFolderSrvLabel");
            this.iFolderSrvLabel.Name = "iFolderSrvLabel";
            this.helpProvider1.SetShowHelp(this.iFolderSrvLabel, ((bool)(resources.GetObject("iFolderSrvLabel.ShowHelp"))));
            // 
            // conflictBinPanel
            //
            this.conflictBinPanel.Controls.Add(this.conflictBinEnable);
            this.conflictBinPanel.Controls.Add(this.conflictBinSvrRdBtn);
            this.conflictBinPanel.Controls.Add(this.conflictBinLocalRdBtn);
            this.conflictBinPanel.Controls.Add(this.conflictBinLabel);
            resources.ApplyResources(this.conflictBinPanel, "conflictBinPanel");
            this.conflictBinPanel.Name = "conflictBinPanel";
            //
            // conflictBinSvrRdBtn
            // 
            resources.ApplyResources(this.conflictBinSvrRdBtn, "conflictBinSvrRdBtn");
            this.conflictBinSvrRdBtn.Name = "conflictBinSvrRdBtn";
            this.helpProvider1.SetShowHelp(this.conflictBinSvrRdBtn, ((bool)(resources.GetObject("conflictBinSvrRdBtn.ShowHelp"))));
            this.conflictBinSvrRdBtn.TabStop = true;
            this.conflictBinSvrRdBtn.UseVisualStyleBackColor = true;

            // 
            // conflictBinLocalRdBtn
            // 
            resources.ApplyResources(this.conflictBinLocalRdBtn, "conflictBinLocalRdBtn");
            this.conflictBinLocalRdBtn.Name = "conflictBinLocalRdBtn";
            this.conflictBinLocalRdBtn.TabStop = true;
            this.conflictBinLocalRdBtn.UseVisualStyleBackColor = true;

            // 
            // conflictBinLabel
            // 
            resources.ApplyResources(this.conflictBinLabel, "conflictBinLabel");
            this.conflictBinLabel.Name = "conflictBinLabel";
            this.helpProvider1.SetShowHelp(this.conflictBinLabel, ((bool)(resources.GetObject("conflictBinLabel.ShowHelp"))));
            // 
            // actionSave
            // 
            resources.ApplyResources(this.actionSave, "actionSave");
            this.helpProvider1.SetHelpString(this.actionSave, resources.GetString("actionSave.HelpString"));
            this.actionSave.Name = "actionSave";
            this.helpProvider1.SetShowHelp(this.actionSave, ((bool)(resources.GetObject("actionSave.ShowHelp"))));
            this.actionSave.Enabled = false;
            // 
            // browseBtn
            // 
            resources.ApplyResources(this.browseBtn, "browseBtn");
            this.helpProvider1.SetHelpString(this.browseBtn, resources.GetString("browseBtn.HelpString"));
            this.browseBtn.Name = "browseBtn";
            this.browseBtn.Enabled = false;
            this.helpProvider1.SetShowHelp(this.browseBtn, ((bool)(resources.GetObject("browseBtn.ShowHelp"))));

            // 
            // pathTxtBox
            // 
            resources.ApplyResources(this.pathTxtBox, "pathTxtBox");
            this.pathTxtBox.Name = "pathTxtBox";
            this.pathTxtBox.Enabled = false;
            this.helpProvider1.SetShowHelp(this.pathTxtBox, ((bool)(resources.GetObject("pathTxtBox.ShowHelp"))));

            // 
            // conflictBinPathLbl
            // 
            resources.ApplyResources(this.conflictBinPathLbl, "conflictBinPathLbl");
            this.conflictBinPathLbl.Name = "conflictBinPathLbl";
            this.helpProvider1.SetShowHelp(this.conflictBinPathLbl, ((bool)(resources.GetObject("conflictBinPathLbl.ShowHelp"))));
            // 
            // conflictBinEnable
            // 
            resources.ApplyResources(this.conflictBinEnable, "conflictBinEnable");
            this.conflictBinEnable.Name = "conflictBinEnable";
            this.conflictBinEnable.UseVisualStyleBackColor = true;
            conflictBinEnable.Enabled = false;

            // 
            // actionGroupBox
            // 
            this.actionGroupBox.Controls.Add(this.pathTxtBox);
            this.actionGroupBox.Controls.Add(this.conflictBinPathLbl);
            this.actionGroupBox.Controls.Add(this.actionSave);
            this.actionGroupBox.Controls.Add(this.browseBtn);
            this.actionGroupBox.Controls.Add(this.iFolderSrvPanel);
            this.actionGroupBox.Controls.Add(this.conflictBinPanel);
            resources.ApplyResources(this.actionGroupBox, "actionGroupBox");
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("actionGroupBox.Anchor")));
        }

        /// <summary>
        /// Registration for event handlers
        /// </summary>
        private void RegisterEventHandlers()
        {
            this.conflictsView.SelectedIndexChanged += new System.EventHandler(this.enhancedConflictsView_SelectedIndexChanged);
            this.saveLocal.Click += new System.EventHandler(this.openLocal_Click);
            this.localName.DoubleClick += new System.EventHandler(this.localName_DoubleClick);
            this.saveServer.Click += new System.EventHandler(this.openServer_Click);
            this.serverName.DoubleClick += new System.EventHandler(this.serverName_DoubleClick);
            this.close.Click += new System.EventHandler(this.close_Click);
            this.resolveName.Click += new System.EventHandler(this.resolveName_Click);
            this.newName.TextChanged += new System.EventHandler(this.newName_TextChanged);
            this.localName.Paint += new System.Windows.Forms.PaintEventHandler(this.localName_Paint);
            this.serverName.Paint += new System.Windows.Forms.PaintEventHandler(this.localName_Paint);
            this.ifolderPath.Paint += new System.Windows.Forms.PaintEventHandler(this.ifolderPath_Paint);

            this.iFolderServerVersionRdBtn.CheckedChanged += new EventHandler(OnSaveToiFolder_checkedChanged);
            this.iFolderLocalVersionRdBtn.CheckedChanged += new EventHandler(OnSaveToiFolder_checkedChanged);
            this.conflictBinSvrRdBtn.CheckedChanged += new EventHandler(OnSaveToConflictBin_checkedChanged);
            this.conflictBinSvrRdBtn.CheckedChanged += new EventHandler(OnSaveToConflictBin_checkedChanged);
            this.browseBtn.Click += new EventHandler(browseBtn_Click);
            this.pathTxtBox.TextChanged += new EventHandler(pathTxtBox_TextChanged);
            this.conflictBinEnable.CheckStateChanged += new EventHandler(conflictBinEnable_CheckStateChanged);

            this.SizeChanged += new System.EventHandler(this.ConflictResolver_SizeChanged);
            this.Load += new System.EventHandler(this.ConflictResolver_Load);

            this.actionSave.Click += new System.EventHandler(this.actionSave_Click);
        }

        /// <summary>
        /// Initalize component
        /// </summary>
        private void InitializeComponent()
        {
            AllocateMemForComponents();
            AllocateMemForNewComponents();
            resources = new System.ComponentModel.ComponentResourceManager(typeof(EnhancedConflictResolver));
            SuspendLayoutForComponents();
            SuspendLayoutForNewComponents();
            this.SuspendLayout();

            PropInitForComponents(resources);
            PropInitForNewComponents(resources);

            // 
            // ConflictResolver
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.actionGroupBox);
            this.Controls.Add(this.ifolderPath);
            this.Controls.Add(this.ifolderName);
            this.Controls.Add(this.versionsPanel);
            this.Controls.Add(this.close);
            this.Controls.Add(this.conflictsView);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConflictResolver";

            RegisterEventHandlers();

            ResumeLayoutForComponents();
            ResumeLayoutForNewComponents();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Disables/Enable controls based on whether there is conflict or not
        /// </summary>
        private void controlActionAndBrowseBtns()
        {
            bool hasFileConflict = false, hasNameConflict = false;
            Conflict conflict = null;
            foreach (ListViewItem lvi in conflictsView.SelectedItems)
            {
                Conflicts conflicts = (Conflicts)lvi.Tag;
                if (null != conflicts.ServerConflict)
                    conflict = conflicts.ServerConflict;
                else if (null != conflicts.LocalConflict)
                    conflict = conflicts.LocalConflict;
                if(null != conflict)
                {
                    if (!conflict.IsNameConflict)
                    {
                        hasFileConflict = true;
                    }
                    else
                    {
                        hasNameConflict = true;
                    }
                }
            }

            if (conflictsView.SelectedItems.Count == 0)
            {
                actionSave.Enabled = browseBtn.Enabled = false;
                browseBtn.Enabled = false;
                pathTxtBox.Enabled = false;
                conflictBinEnable.Enabled = false;
            }
            else if (conflictsView.SelectedItems.Count == 1)
            {
                if (hasNameConflict)
                {
                    this.actionGroupBox.Enabled = false;
                }
                else
                {
                    this.actionGroupBox.Enabled = true;
                    actionSave.Enabled = browseBtn.Enabled = true;
                    conflictBinEnable.Enabled = true;
                    if (conflictBinEnable.Checked)
                    {
                        browseBtn.Enabled = true;
                        pathTxtBox.Enabled = true;
                    }
                    else
                    {
                        browseBtn.Enabled = false;
                        pathTxtBox.Enabled = false;
                    }
                }
            }
            else
            {
                if(!hasNameConflict && hasFileConflict)
                {
                    this.actionGroupBox.Enabled = true;
                    actionSave.Enabled = browseBtn.Enabled = true;
                    browseBtn.Enabled = true;
                    pathTxtBox.Enabled = true;
                    conflictBinEnable.Enabled = true;
                }
                else
                {
                    this.actionGroupBox.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Index changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enhancedConflictsView_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            conflictsView_SelectedIndexChanged(sender, e);
            controlActionAndBrowseBtns();
		}

        /// <summary>
        /// Function to resolve the conflict
        /// </summary>
        /// <param name="localCopy"></param>
        private void enhancedConflictResolve(bool localCopy)
        {
            string path = null;
            foreach (ListViewItem lvi in conflictsView.SelectedItems)
            {
                Conflicts conflicts = (Conflicts)lvi.Tag;
                if (!conflicts.ServerConflict.IsNameConflict)
                {
                    try
                    {
                        // Display a warning and return if the user has  not specified the conflict bin path
                        if (conflictBinEnable.Checked)
                        {
                            if( pathTxtBox.Text == "")
                            {
                                MyMessageBox mmb = new MyMessageBox(resources.GetString("emptyConflictBinMsg"), resources.GetString("emptyConflictBinTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                                mmb.ShowDialog();
                                return;
                            }
                            path = pathTxtBox.Text;
                            if (!path.EndsWith(resources.GetString("pathSeparator")))
                            {
                                path = path + resources.GetString("pathSeparator");
                            }
                            if (!Directory.Exists(path))
                            {
                                try
                                {
                                    Directory.CreateDirectory(path);
                                }
                                catch 
                                {
                                    MyMessageBox mmb = new MyMessageBox(resources.GetString("invalidConflictBinMsg"), resources.GetString("invalidConflictBinTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                                    mmb.ShowDialog();
                                    return;
                                }
                            }
                            // Check to make sure the user has rights to this directory.
                            if (!Win32Security.AccessAllowed(path))
                            {
                                MyMessageBox mmb = new MyMessageBox(resources.GetString("accessDenied"), resources.GetString("accessDeniedTitle"), string.Empty, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                                mmb.ShowDialog();
                                return;
                            }
                            else
                            {
                               ifWebService.ResolveEnhancedFileConflict(ifolder.ID, conflicts.ServerConflict.ConflictID, localCopy, path);
                            }

                            //ifWebService.ResolveEnhancedFileConflict(ifolder.ID, conflicts.ServerConflict.ConflictID, localCopy, path);
                        }
                        else
                        {
                            ifWebService.ResolveEnhancedFileConflict(ifolder.ID, conflicts.ServerConflict.ConflictID, localCopy, string.Empty);
                        }
                        lvi.Remove();
                    }
                    catch (Exception ex)
                    {
                        MyMessageBox mmb = new MyMessageBox(conflictErrorMsg, conflictErrorTitle, ex.Message, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
                        mmb.ShowDialog();
                    }
                }
            }

            if (conflictsView.Items.Count == 0)
            {
                // If all the conflicts have been resolved, fire the ConflictsResolved event.
                InvokeConflictResolvedEvent(this, new EventArgs());
            }
        }

        /// <summary>
        /// Save action clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actionSave_Click(object sender, System.EventArgs e)
        {
            if (iFolderLocalVersionRdBtn.Checked)
            {
                enhancedConflictResolve(true);
            }
            else
            {
                enhancedConflictResolve(false);
            }
        }

        /// <summary>
        /// Checked changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void conflictBinEnable_CheckStateChanged(object sender, System.EventArgs e)
        {
            if (conflictBinEnable.Checked)
            {
                pathTxtBox.Enabled = true;
                conflictBinLocalRdBtn.Enabled = true;
                conflictBinSvrRdBtn.Enabled = true;
                browseBtn.Enabled = true;
                if (iFolderLocalVersionRdBtn.Checked)
                {
                    conflictBinLocalRdBtn.Checked = false;
                    conflictBinSvrRdBtn.Checked = true;
                }
                else
                {
                    conflictBinLocalRdBtn.Checked = true;
                    conflictBinSvrRdBtn.Checked = false;
                }
            }
            else
            {
                pathTxtBox.Text = string.Empty;
                pathTxtBox.Enabled = false;
                conflictBinLocalRdBtn.Enabled = false;
                conflictBinSvrRdBtn.Enabled = false;
                browseBtn.Enabled = false;
            }
        }

        /// <summary>
        /// Checked changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveToiFolder_checkedChanged(object sender, System.EventArgs e)
        {
            if (iFolderLocalVersionRdBtn.Checked)
            {
                iFolderServerVersionRdBtn.Checked = false;
                if (conflictBinEnable.Checked)
                {
                    conflictBinLocalRdBtn.Checked = false;
                    conflictBinSvrRdBtn.Checked = true;
                }
                else
                {
                    conflictBinLocalRdBtn.Checked = false;
                    conflictBinSvrRdBtn.Checked = false;
                }
            }
            else
            {
                iFolderServerVersionRdBtn.Checked = true;
                if (conflictBinEnable.Checked)
                {
                    conflictBinLocalRdBtn.Checked = true;
                    conflictBinSvrRdBtn.Checked = false;
                }
                else
                {
                    conflictBinLocalRdBtn.Checked = false;
                    conflictBinSvrRdBtn.Checked = false;
                }
            }
        }

        /// <summary>
        /// Checked changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveToConflictBin_checkedChanged(object sender, System.EventArgs e)
        {
            if (conflictBinLocalRdBtn.Checked)
            {
                iFolderServerVersionRdBtn.Checked = true;
                iFolderLocalVersionRdBtn.Checked = false;
                conflictBinSvrRdBtn.Checked = false;
            }
            else
            {
                iFolderServerVersionRdBtn.Checked = false;
                iFolderLocalVersionRdBtn.Checked = true;
                conflictBinSvrRdBtn.Checked = true;
            }
        }

        /// <summary>
        /// Button cliecked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openLocal_Click(object sender, System.EventArgs e)
        {
            this.localName_DoubleClick(sender, e);
        }

        /// <summary>
        /// Button clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openServer_Click(object sender, System.EventArgs e)
        {
            this.serverName_DoubleClick(sender, e);
        }

        /// <summary>
        /// Text changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pathTxtBox_TextChanged(object sender, System.EventArgs e)
        {
            if (pathTxtBox.Text.Length > 0)
                actionSave.Enabled = true;
            else
                actionSave.Enabled = false;
        }

        /// <summary>
        /// Button cliecked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void browseBtn_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = this.resources.GetString("chooseFolder");
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                this.pathTxtBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

    }
}
