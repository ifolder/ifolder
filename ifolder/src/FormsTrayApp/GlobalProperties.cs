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
using Simias;
using Simias.Sync;
using Novell.iFolder;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(FormsTrayApp));

		private iFolderManager iFManager = null;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.PictureBox banner;
		private System.Windows.Forms.CheckBox autoStart;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView iFolderView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label amountToUpload;
		private System.Windows.Forms.Label filesToSync;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button syncNow;
		private System.Windows.Forms.ListBox log;
		private System.Windows.Forms.Button saveLog;
		private System.Windows.Forms.Button clearLog;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public GlobalProperties()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			iFManager = iFolderManager.Connect();
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
			this.label1 = new System.Windows.Forms.Label();
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.filesToSync = new System.Windows.Forms.Label();
			this.amountToUpload = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.iFolderView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.autoStart = new System.Windows.Forms.CheckBox();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.clearLog = new System.Windows.Forms.Button();
			this.saveLog = new System.Windows.Forms.Button();
			this.log = new System.Windows.Forms.ListBox();
			this.syncNow = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.banner = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 80);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Sync to host every:";
			// 
			// defaultInterval
			// 
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = new System.Drawing.Point(144, 80);
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
			this.defaultInterval.Size = new System.Drawing.Size(64, 20);
			this.defaultInterval.TabIndex = 2;
			// 
			// displayConfirmation
			// 
			this.displayConfirmation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.displayConfirmation.Location = new System.Drawing.Point(16, 48);
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.Size = new System.Drawing.Size(368, 24);
			this.displayConfirmation.TabIndex = 1;
			this.displayConfirmation.Text = "Display iFolder creation confirmation.";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(288, 432);
			this.ok.Name = "ok";
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(368, 432);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(216, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "seconds";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Location = new System.Drawing.Point(8, 65);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(434, 359);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.iFolderView);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(426, 333);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "iFolders";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.filesToSync);
			this.groupBox4.Controls.Add(this.amountToUpload);
			this.groupBox4.Controls.Add(this.label5);
			this.groupBox4.Controls.Add(this.label4);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(8, 240);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(408, 72);
			this.groupBox4.TabIndex = 1;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Synchronization";
			// 
			// filesToSync
			// 
			this.filesToSync.Location = new System.Drawing.Point(296, 48);
			this.filesToSync.Name = "filesToSync";
			this.filesToSync.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.filesToSync.Size = new System.Drawing.Size(100, 16);
			this.filesToSync.TabIndex = 3;
			// 
			// amountToUpload
			// 
			this.amountToUpload.Location = new System.Drawing.Point(296, 24);
			this.amountToUpload.Name = "amountToUpload";
			this.amountToUpload.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.amountToUpload.Size = new System.Drawing.Size(100, 16);
			this.amountToUpload.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(272, 16);
			this.label5.TabIndex = 2;
			this.label5.Text = "Files/folders to synchronize:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(272, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Amount to upload:";
			// 
			// iFolderView
			// 
			this.iFolderView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1});
			this.iFolderView.Location = new System.Drawing.Point(8, 16);
			this.iFolderView.Name = "iFolderView";
			this.iFolderView.Size = new System.Drawing.Size(408, 208);
			this.iFolderView.TabIndex = 0;
			this.iFolderView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "iFolders";
			this.columnHeader1.Width = 406;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Controls.Add(this.groupBox3);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(426, 333);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Preferences";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 128);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(398, 184);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.defaultInterval);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 48);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(366, 112);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Synchronize to host";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(336, 48);
			this.label3.TabIndex = 0;
			this.label3.Text = "This value sets the default value for how often the host of an iFolder will be co" +
				"ntacted  to sync files.";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 24);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(368, 16);
			this.autoSync.TabIndex = 0;
			this.autoSync.Text = "Automatic sync";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.autoStart);
			this.groupBox3.Controls.Add(this.displayConfirmation);
			this.groupBox3.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox3.Location = new System.Drawing.Point(16, 24);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(398, 80);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Application";
			// 
			// autoStart
			// 
			this.autoStart.Checked = true;
			this.autoStart.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoStart.Enabled = false;
			this.autoStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoStart.Location = new System.Drawing.Point(16, 20);
			this.autoStart.Name = "autoStart";
			this.autoStart.Size = new System.Drawing.Size(368, 24);
			this.autoStart.TabIndex = 0;
			this.autoStart.Text = "Startup iFolder at login.";
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.clearLog);
			this.tabPage3.Controls.Add(this.saveLog);
			this.tabPage3.Controls.Add(this.log);
			this.tabPage3.Controls.Add(this.syncNow);
			this.tabPage3.Controls.Add(this.label6);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(426, 333);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Log";
			// 
			// clearLog
			// 
			this.clearLog.Enabled = false;
			this.clearLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.clearLog.Location = new System.Drawing.Point(88, 288);
			this.clearLog.Name = "clearLog";
			this.clearLog.TabIndex = 4;
			this.clearLog.Text = "Clear";
			// 
			// saveLog
			// 
			this.saveLog.Enabled = false;
			this.saveLog.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveLog.Location = new System.Drawing.Point(8, 288);
			this.saveLog.Name = "saveLog";
			this.saveLog.TabIndex = 3;
			this.saveLog.Text = "Save...";
			// 
			// log
			// 
			this.log.HorizontalScrollbar = true;
			this.log.Location = new System.Drawing.Point(8, 48);
			this.log.Name = "log";
			this.log.ScrollAlwaysVisible = true;
			this.log.Size = new System.Drawing.Size(408, 225);
			this.log.TabIndex = 2;
			// 
			// syncNow
			// 
			this.syncNow.Enabled = false;
			this.syncNow.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.syncNow.Location = new System.Drawing.Point(320, 12);
			this.syncNow.Name = "syncNow";
			this.syncNow.Size = new System.Drawing.Size(96, 23);
			this.syncNow.TabIndex = 1;
			this.syncNow.Text = "Sync now";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(296, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "This log shows current iFolder activity.";
			// 
			// banner
			// 
			this.banner.Location = new System.Drawing.Point(0, 0);
			this.banner.Name = "banner";
			this.banner.Size = new System.Drawing.Size(450, 65);
			this.banner.TabIndex = 9;
			this.banner.TabStop = false;
			// 
			// GlobalProperties
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(450, 464);
			this.Controls.Add(this.banner);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GlobalProperties";
			this.Text = "Global iFolder Properties";
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void GlobalProperties_Load(object sender, System.EventArgs e)
		{
			// Load the application icon and banner image.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
				this.banner.Image = Image.FromFile(Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading graphics");
			}

			try
			{
				// Display the default sync interval.
				defaultInterval.Value = (decimal)iFManager.DefaultRefreshInterval;

				// Initialize displayConfirmation.
				Configuration config = new Configuration();
				string showWizard = config.Get("iFolderShell", "Show wizard", "true");
				displayConfirmation.Checked = showWizard == "true";
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Save the default sync interval.
				iFManager.DefaultRefreshInterval = (int)defaultInterval.Value;

				Configuration config = new Configuration();
				if (displayConfirmation.Checked)
				{
					config.Set("iFolderShell", "Show wizard", "true");
				}
				else
				{
					config.Set("iFolderShell", "Show wizard", "false");
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
			}
		}
		#endregion
	}
}
