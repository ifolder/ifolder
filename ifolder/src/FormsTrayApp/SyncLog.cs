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

using Simias.Client.Event;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for SyncLog.
	/// </summary>
	public class SyncLog : System.Windows.Forms.Form
	{
		#region Class Members
		// Delegates used to marshal back to the control's creation thread.
		private delegate void SyncCollectionDelegate(CollectionSyncEventArgs syncEventArgs);
		private SyncCollectionDelegate syncCollectionDelegate;
		private delegate void SyncFileDelegate(FileSyncEventArgs syncEventArgs);
		private SyncFileDelegate syncFileDelegate;

		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(SyncLog));
		private const int maxMessages = 500;
		private IProcEventClient eventClient;
		private bool shutdown = false;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox log;
		private System.Windows.Forms.Button saveLog;
		private System.Windows.Forms.Button clearLog;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs a SyncLog object.
		/// </summary>
		/// <param name="eventClient">IProcEventClient object used to get events from simias.</param>
		public SyncLog(IProcEventClient eventClient)
		{
			syncCollectionDelegate = new SyncCollectionDelegate(syncCollection);
			syncFileDelegate = new SyncFileDelegate(syncFile);

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.eventClient = eventClient;

			// Set up the event handlers for sync events ... these need to be active here so that sync events can
			// be written to the log listbox.
			eventClient.SetEvent(IProcEventAction.AddCollectionSync, new IProcEventHandler(log_collectionSyncHandler));
			eventClient.SetEvent(IProcEventAction.AddFileSync, new IProcEventHandler(log_fileSyncHandler));
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SyncLog));
			this.log = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.saveLog = new System.Windows.Forms.Button();
			this.clearLog = new System.Windows.Forms.Button();
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
			// saveLog
			// 
			this.saveLog.AccessibleDescription = resources.GetString("saveLog.AccessibleDescription");
			this.saveLog.AccessibleName = resources.GetString("saveLog.AccessibleName");
			this.saveLog.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("saveLog.Anchor")));
			this.saveLog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("saveLog.BackgroundImage")));
			this.saveLog.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("saveLog.Dock")));
			this.saveLog.Enabled = ((bool)(resources.GetObject("saveLog.Enabled")));
			this.saveLog.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("saveLog.FlatStyle")));
			this.saveLog.Font = ((System.Drawing.Font)(resources.GetObject("saveLog.Font")));
			this.saveLog.Image = ((System.Drawing.Image)(resources.GetObject("saveLog.Image")));
			this.saveLog.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveLog.ImageAlign")));
			this.saveLog.ImageIndex = ((int)(resources.GetObject("saveLog.ImageIndex")));
			this.saveLog.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("saveLog.ImeMode")));
			this.saveLog.Location = ((System.Drawing.Point)(resources.GetObject("saveLog.Location")));
			this.saveLog.Name = "saveLog";
			this.saveLog.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("saveLog.RightToLeft")));
			this.saveLog.Size = ((System.Drawing.Size)(resources.GetObject("saveLog.Size")));
			this.saveLog.TabIndex = ((int)(resources.GetObject("saveLog.TabIndex")));
			this.saveLog.Text = resources.GetString("saveLog.Text");
			this.saveLog.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("saveLog.TextAlign")));
			this.saveLog.Visible = ((bool)(resources.GetObject("saveLog.Visible")));
			this.saveLog.Click += new System.EventHandler(this.saveLog_Click);
			// 
			// clearLog
			// 
			this.clearLog.AccessibleDescription = resources.GetString("clearLog.AccessibleDescription");
			this.clearLog.AccessibleName = resources.GetString("clearLog.AccessibleName");
			this.clearLog.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("clearLog.Anchor")));
			this.clearLog.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("clearLog.BackgroundImage")));
			this.clearLog.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("clearLog.Dock")));
			this.clearLog.Enabled = ((bool)(resources.GetObject("clearLog.Enabled")));
			this.clearLog.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("clearLog.FlatStyle")));
			this.clearLog.Font = ((System.Drawing.Font)(resources.GetObject("clearLog.Font")));
			this.clearLog.Image = ((System.Drawing.Image)(resources.GetObject("clearLog.Image")));
			this.clearLog.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("clearLog.ImageAlign")));
			this.clearLog.ImageIndex = ((int)(resources.GetObject("clearLog.ImageIndex")));
			this.clearLog.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("clearLog.ImeMode")));
			this.clearLog.Location = ((System.Drawing.Point)(resources.GetObject("clearLog.Location")));
			this.clearLog.Name = "clearLog";
			this.clearLog.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("clearLog.RightToLeft")));
			this.clearLog.Size = ((System.Drawing.Size)(resources.GetObject("clearLog.Size")));
			this.clearLog.TabIndex = ((int)(resources.GetObject("clearLog.TabIndex")));
			this.clearLog.Text = resources.GetString("clearLog.Text");
			this.clearLog.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("clearLog.TextAlign")));
			this.clearLog.Visible = ((bool)(resources.GetObject("clearLog.Visible")));
			this.clearLog.Click += new System.EventHandler(this.clearLog_Click);
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
			this.Controls.Add(this.clearLog);
			this.Controls.Add(this.saveLog);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.log);
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
			this.Load += new System.EventHandler(this.SyncLog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Sync Event Handlers
		private void log_collectionSyncHandler(SimiasEventArgs args)
		{
			try
			{
				CollectionSyncEventArgs syncEventArgs = args as CollectionSyncEventArgs;
				BeginInvoke(syncCollectionDelegate, new object[] {syncEventArgs});
			}
			catch {}
		}

		private void log_fileSyncHandler(SimiasEventArgs args)
		{
			try
			{
				FileSyncEventArgs syncEventArgs = args as FileSyncEventArgs;
				BeginInvoke(syncFileDelegate, new object[] {syncEventArgs});
			}
			catch {}
		}
		#endregion

		#region Private Methods
		private void addMessageToLog(DateTime dateTime, string message)
		{
			if (message != null)
			{
				log.Items.Add(dateTime.ToString() + " " + message);
				log.SelectedIndex = log.Items.Count - 1;
				saveLog.Enabled = clearLog.Enabled = true;

				// This should only have to execute once.
				while (log.Items.Count > maxMessages)
				{
					log.Items.RemoveAt(0);
				}
			}
		}

		private void syncCollection(CollectionSyncEventArgs syncEventArgs)
		{
			try
			{
				string message = null;
				switch (syncEventArgs.Action)
				{
					case Action.StartSync:
					{
						message = string.Format(resourceManager.GetString("synciFolder"), syncEventArgs.Name);
						break;
					}
					case Action.StopSync:
					{
						message = string.Format(resourceManager.GetString("syncComplete"), syncEventArgs.Name);
						break;
					}
				}

				// Add message to log.
				addMessageToLog(syncEventArgs.TimeStamp, message);
			}
			catch {}
		}

		private void syncFile(FileSyncEventArgs syncEventArgs)
		{
			try
			{
				if (syncEventArgs.SizeRemaining == syncEventArgs.SizeToSync)
				{
					string message = null;
					switch (syncEventArgs.ObjectType)
					{
						case ObjectType.File:
							message = syncEventArgs.Delete ? 
								string.Format(resourceManager.GetString("deleteClientFile"), syncEventArgs.Name) :
								string.Format(resourceManager.GetString(syncEventArgs.Direction == Direction.Uploading ? "uploadFile" : "downloadFile"), syncEventArgs.Name);
							break;
						case ObjectType.Directory:
							message = syncEventArgs.Delete ? 
								string.Format(resourceManager.GetString("deleteClientDir"), syncEventArgs.Name) :
								string.Format(resourceManager.GetString(syncEventArgs.Direction == Direction.Uploading ? "uploadDir" : "downloadDir"), syncEventArgs.Name);
							break;
						case ObjectType.Unknown:
							message = string.Format(resourceManager.GetString("deleteUnknown"), syncEventArgs.Name);
							break;
					}

					// Add message to log.
					addMessageToLog(syncEventArgs.TimeStamp, message);
				}
			}
			catch {}
		}
		#endregion

		#region Event Handlers
		private void SyncLog_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch {} // Non-fatal ...

		}

		private void SyncLog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// If we haven't received a shutdown event, hide this dialog and cancel the event.
			if (!shutdown)
			{
				e.Cancel = true;
				Hide();
			}
		}

		private void saveLog_Click(object sender, System.EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
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

		private void clearLog_Click(object sender, System.EventArgs e)
		{
			log.Items.Clear();

			log.Items.Add(DateTime.Now.ToString() + " " + resourceManager.GetString("logEntriesCleared"));

			saveLog.Enabled = clearLog.Enabled = false;
		}
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
