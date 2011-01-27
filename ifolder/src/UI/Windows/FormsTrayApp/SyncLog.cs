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

using Microsoft.Win32;

using Simias.Client.Event;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for SyncLog.
	/// </summary>
	public class SyncLog : System.Windows.Forms.Form
	{
		#region Class Members
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(SyncLog));
		private const string syncLogX = "SyncLogX";
		private const string syncLogY = "SyncLogY";
		private const int maxMessages = 500;
		private bool shutdown = false;
		private bool initialPositionSet = false;
		private System.Windows.Forms.ListBox log;
		private Novell.FormsTrayApp.ToolBarEx toolBar1;
		private System.Windows.Forms.ToolBarButton toolBarSave;
		private System.Windows.Forms.ToolBarButton toolBarClear;
		//private System.ComponentModel.IContainer components;
		#endregion

		/// <summary>
		/// Constructs a SyncLog object.
		/// </summary>
		public SyncLog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));

				toolBar1.ImageList = new ImageList();
				toolBar1.ImageList.ImageSize = new Size(24, 24);
				toolBar1.ImageList.TransparentColor = Color.White;
				toolBar1.ImageList.Images.AddStrip(Image.FromFile(Path.Combine(Application.StartupPath, @"res\ltoolbar_nor.bmp")));

				toolBar1.DisabledImageList = new ImageList();
				toolBar1.DisabledImageList.ImageSize = new Size(24, 24);
				toolBar1.DisabledImageList.TransparentColor = Color.White;
				toolBar1.DisabledImageList.Images.AddStrip(Image.FromFile(Path.Combine(Application.StartupPath, @"res\ltoolbar_dis.bmp")));

				toolBar1.HotImageList = new ImageList();
				toolBar1.HotImageList.ImageSize = new Size(24, 24);
				toolBar1.HotImageList.TransparentColor = Color.White;
				toolBar1.HotImageList.Images.AddStrip(Image.FromFile(Path.Combine(Application.StartupPath, @"res\ltoolbar_hot.bmp")));
			}
			catch {} // Non-fatal ...
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				/*if(components != null)
				{
					components.Dispose();
				}*/
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SyncLog));
			this.log = new System.Windows.Forms.ListBox();
			this.toolBar1 = new Novell.FormsTrayApp.ToolBarEx();
			this.toolBarSave = new System.Windows.Forms.ToolBarButton();
			this.toolBarClear = new System.Windows.Forms.ToolBarButton();
			this.SuspendLayout();
			// 
			// log
			// 
			this.log.AccessibleDescription = resources.GetString("log.AccessibleDescription");
			this.log.AccessibleName = resources.GetString("log.AccessibleName");
			this.log.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("log.Anchor")));
			this.log.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("log.BackgroundImage")));
			this.log.ColumnWidth = ((int)(resources.GetObject("log.ColumnWidth")));
			this.log.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("log.Dock")));
			this.log.Enabled = ((bool)(resources.GetObject("log.Enabled")));
			this.log.Font = ((System.Drawing.Font)(resources.GetObject("log.Font")));
			this.log.HorizontalExtent = ((int)(resources.GetObject("log.HorizontalExtent")));
			this.log.HorizontalScrollbar = ((bool)(resources.GetObject("log.HorizontalScrollbar")));
			this.log.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("log.ImeMode")));
			this.log.IntegralHeight = ((bool)(resources.GetObject("log.IntegralHeight")));
			this.log.ItemHeight = ((int)(resources.GetObject("log.ItemHeight")));
			this.log.Location = ((System.Drawing.Point)(resources.GetObject("log.Location")));
			this.log.Name = "log";
			this.log.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("log.RightToLeft")));
			this.log.ScrollAlwaysVisible = ((bool)(resources.GetObject("log.ScrollAlwaysVisible")));
			this.log.Size = ((System.Drawing.Size)(resources.GetObject("log.Size")));
			this.log.TabIndex = ((int)(resources.GetObject("log.TabIndex")));
			this.log.Visible = ((bool)(resources.GetObject("log.Visible")));
			// 
			// toolBar1
			// 
			this.toolBar1.AccessibleDescription = resources.GetString("toolBar1.AccessibleDescription");
			this.toolBar1.AccessibleName = resources.GetString("toolBar1.AccessibleName");
			this.toolBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("toolBar1.Anchor")));
			this.toolBar1.Appearance = ((System.Windows.Forms.ToolBarAppearance)(resources.GetObject("toolBar1.Appearance")));
			this.toolBar1.AutoSize = ((bool)(resources.GetObject("toolBar1.AutoSize")));
			this.toolBar1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("toolBar1.BackgroundImage")));
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarSave,
																						this.toolBarClear});
			this.toolBar1.ButtonSize = ((System.Drawing.Size)(resources.GetObject("toolBar1.ButtonSize")));
			this.toolBar1.DisabledImageList = null;
			this.toolBar1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("toolBar1.Dock")));
			this.toolBar1.DropDownArrows = ((bool)(resources.GetObject("toolBar1.DropDownArrows")));
			this.toolBar1.Enabled = ((bool)(resources.GetObject("toolBar1.Enabled")));
			this.toolBar1.Font = ((System.Drawing.Font)(resources.GetObject("toolBar1.Font")));
			this.toolBar1.HotImageList = null;
			this.toolBar1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("toolBar1.ImeMode")));
			this.toolBar1.Location = ((System.Drawing.Point)(resources.GetObject("toolBar1.Location")));
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("toolBar1.RightToLeft")));
			this.toolBar1.ShowToolTips = ((bool)(resources.GetObject("toolBar1.ShowToolTips")));
			this.toolBar1.Size = ((System.Drawing.Size)(resources.GetObject("toolBar1.Size")));
			this.toolBar1.TabIndex = ((int)(resources.GetObject("toolBar1.TabIndex")));
			this.toolBar1.TabStop = true;
			this.toolBar1.TextAlign = ((System.Windows.Forms.ToolBarTextAlign)(resources.GetObject("toolBar1.TextAlign")));
			this.toolBar1.Visible = ((bool)(resources.GetObject("toolBar1.Visible")));
			this.toolBar1.Wrappable = ((bool)(resources.GetObject("toolBar1.Wrappable")));
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarSave
			// 
			this.toolBarSave.Enabled = ((bool)(resources.GetObject("toolBarSave.Enabled")));
			this.toolBarSave.ImageIndex = ((int)(resources.GetObject("toolBarSave.ImageIndex")));
			this.toolBarSave.Text = resources.GetString("toolBarSave.Text");
			this.toolBarSave.ToolTipText = resources.GetString("toolBarSave.ToolTipText");
			this.toolBarSave.Visible = ((bool)(resources.GetObject("toolBarSave.Visible")));
			// 
			// toolBarClear
			// 
			this.toolBarClear.Enabled = ((bool)(resources.GetObject("toolBarClear.Enabled")));
			this.toolBarClear.ImageIndex = ((int)(resources.GetObject("toolBarClear.ImageIndex")));
			this.toolBarClear.Text = resources.GetString("toolBarClear.Text");
			this.toolBarClear.ToolTipText = resources.GetString("toolBarClear.ToolTipText");
			this.toolBarClear.Visible = ((bool)(resources.GetObject("toolBarClear.Visible")));
			// 
			// SyncLog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.log);
			this.Controls.Add(this.toolBar1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "SyncLog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.SyncLog_Closing);
			this.Move += new System.EventHandler(this.SyncLog_Move);
			this.ResumeLayout(false);

		}
		#endregion

		#region Public Methods
        /// <summary>
        /// Add messages to Log
        /// </summary>
        /// <param name="dateTime">Time Stamp</param>
        /// <param name="message">String message that needs to be added</param>
		public void AddMessageToLog(DateTime dateTime, string message)
		{
			if (message != null)
			{
				log.Items.Add(dateTime.ToString() + " " + message);
				log.SelectedIndex = log.Items.Count - 1;
				toolBarSave.Enabled = toolBarClear.Enabled = true;
				
				log.HorizontalScrollbar = true;
				Graphics g = log.CreateGraphics();
				int hzSize = (int) g.MeasureString(dateTime.ToString() + " " + message, log.Font).Width;
				if( hzSize > log.HorizontalExtent )
					log.HorizontalExtent = hzSize;

				// This should only have to execute once.
				while (log.Items.Count > maxMessages)
				{
					log.Items.RemoveAt(0);
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
        /// Customize - For Filtered sync log
        /// </summary>
        /// <returns></returns>
        public bool Customize()
        {
            log.Items.Clear();
            //this.toolBarClear.Visible = false;
            this.Text = resourceManager.GetString("this.Text-Filtered");
            return true;
        }
		#endregion

		#region Private Methods
        /// <summary>
        /// Clear the log messages 
        /// </summary>
		private void clearLog()
		{
			log.Items.Clear();

			log.Items.Add(DateTime.Now.ToString() + " " + resourceManager.GetString("logEntriesCleared"));

			toolBarSave.Enabled = toolBarClear.Enabled = false;
		}

        /// <summary>
        /// Save the log messages to a File.
        /// </summary>
		private void saveLog()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "iFolder Synchronization Log.txt";
            saveFileDialog.Filter = "All Files (*.*)|*.*|Text Files (*.txt)|*.txt";
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				StreamWriter streamWriter = File.CreateText(saveFileDialog.FileName);
				foreach (string s in log.Items)
				{
					streamWriter.WriteLine(s);
				}

				streamWriter.Flush();
				streamWriter.Close();
			}
		}
		#endregion

		#region Event Handlers
        /// <summary>
        /// Event Handler for Sync log closing event
        /// </summary>
        private void SyncLog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// If we haven't received a shutdown event, hide this dialog and cancel the event.
			if (!shutdown)
			{
				e.Cancel = true;
				Hide();
			}
		}

        /// <summary>
        /// Event Handler for toolbar1 button click event
        /// </summary>
        private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (toolBar1.Buttons.IndexOf(e.Button))
			{
				case 0: // Save log
					saveLog();
					break;
				case 1: // Clear log
					clearLog();
					break;
			}
		}

        /// <summary>
        /// Event Handler for Sync log move event
        /// </summary>
        private void SyncLog_Move(object sender, System.EventArgs e)
		{
			if (initialPositionSet)
			{
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(Preferences.iFolderKey);

					// Set the location values.
					regKey.SetValue(syncLogX, Location.X);
					regKey.SetValue(syncLogY, Location.Y);
				}
				catch {}
			}
			else
			{
				try
				{
					// Create/open the iFolder key.
					RegistryKey regKey = Registry.CurrentUser.CreateSubKey(Preferences.iFolderKey);

					// Get the location values.
					int x = (int)regKey.GetValue(syncLogX);
					int y = (int)regKey.GetValue(syncLogY);

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
				{
#if DEBUG					
					MessageBox.Show("Shutdown msg got - synclog");
#endif
					this.shutdown = true;
					break;
				}
			}

			base.WndProc (ref m);
		}
	}
}
