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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace Simias
{
	/// <summary>
	/// MyTrace Trace Form
	/// </summary>
	public class MyTraceForm : Form
	{
		private const int TASKBAR_HEIGHT = 30;
		private const int SCROLLBAR_WIDTH = 21;
		private const int WM_QUERYENDSESSION = 0x0011;

		private Container components = null;

		private TreeView logTreeView;
		private ListView logListView;
		private ContextMenu logContextMenu;
		private MenuItem scrollLockMenuItem;
		private MenuItem copyMenuItem;
		private MenuItem clearMenuItem;
		private MyTraceFormListener traceListener;
		private ColumnHeader columnHeader2;
		private System.Windows.Forms.Splitter splitter;
		private System.Windows.Forms.Panel logListPanel;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.MenuItem limitMenuItem;
		private bool shutdown = false;

		/// <summary>
		/// Constructor
		/// </summary>
		public MyTraceForm()
		{
			InitializeComponent();

			// put the form in the bottom corner of the screen
			Rectangle screen = SystemInformation.VirtualScreen;
			Point start = new Point();

			start.X = screen.Width - this.Size.Width - 1;
			start.Y = screen.Height - this.Size.Height - TASKBAR_HEIGHT - 1;

			this.Location = start;

			// start listener
			traceListener = new MyTraceFormListener(logTreeView, logListView);
			Trace.Listeners.Add(traceListener);

			// set log list view column size
			MyTraceForm_SizeChanged(null, null);
		}

		/// <summary>
		/// Process window messages.
		/// </summary>
		/// <param name="m">The window message.</param>
		protected override void WndProc(ref Message m)
		{
			switch(m.Msg)
			{
				case WM_QUERYENDSESSION:
					shutdown = true;
					break;
			}

			base.WndProc(ref m);
		}

		#region IDispose Members

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

		#endregion

		#region Windows Form Designer
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.logTreeView = new System.Windows.Forms.TreeView();
			this.logListView = new System.Windows.Forms.ListView();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.logContextMenu = new System.Windows.Forms.ContextMenu();
			this.scrollLockMenuItem = new System.Windows.Forms.MenuItem();
			this.copyMenuItem = new System.Windows.Forms.MenuItem();
			this.clearMenuItem = new System.Windows.Forms.MenuItem();
			this.limitMenuItem = new System.Windows.Forms.MenuItem();
			this.splitter = new System.Windows.Forms.Splitter();
			this.logListPanel = new System.Windows.Forms.Panel();
			this.logListPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// logTreeView
			// 
			this.logTreeView.CheckBoxes = true;
			this.logTreeView.Dock = System.Windows.Forms.DockStyle.Right;
			this.logTreeView.ImageIndex = -1;
			this.logTreeView.Location = new System.Drawing.Point(352, 0);
			this.logTreeView.Name = "logTreeView";
			this.logTreeView.SelectedImageIndex = -1;
			this.logTreeView.Size = new System.Drawing.Size(280, 454);
			this.logTreeView.TabIndex = 1;
			this.logTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.logTreeView_AfterCheck);
			// 
			// logListView
			// 
			this.logListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader3,
																						  this.columnHeader2,
																						  this.columnHeader1});
			this.logListView.ContextMenu = this.logContextMenu;
			this.logListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logListView.FullRowSelect = true;
			this.logListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.logListView.Location = new System.Drawing.Point(0, 0);
			this.logListView.Name = "logListView";
			this.logListView.Size = new System.Drawing.Size(349, 454);
			this.logListView.TabIndex = 0;
			this.logListView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Message";
			this.columnHeader3.Width = 200;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Method";
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Time";
			// 
			// logContextMenu
			// 
			this.logContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						   this.scrollLockMenuItem,
																						   this.copyMenuItem,
																						   this.clearMenuItem,
																						   this.limitMenuItem});
			// 
			// scrollLockMenuItem
			// 
			this.scrollLockMenuItem.Index = 0;
			this.scrollLockMenuItem.Text = "Scroll Lock";
			this.scrollLockMenuItem.Click += new System.EventHandler(this.scrollLockMenuItem_Click);
			// 
			// copyMenuItem
			// 
			this.copyMenuItem.Index = 1;
			this.copyMenuItem.Text = "Copy Log";
			this.copyMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
			// 
			// clearMenuItem
			// 
			this.clearMenuItem.Index = 2;
			this.clearMenuItem.Text = "Clear All";
			this.clearMenuItem.Click += new System.EventHandler(this.clearMenuItem_Click);
			// 
			// limitMenuItem
			// 
			this.limitMenuItem.Index = 3;
			this.limitMenuItem.Text = "Limit Size";
			this.limitMenuItem.Click += new System.EventHandler(this.limitMenuItem_Click);
			// 
			// splitter
			// 
			this.splitter.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter.Location = new System.Drawing.Point(349, 0);
			this.splitter.Name = "splitter";
			this.splitter.Size = new System.Drawing.Size(3, 454);
			this.splitter.TabIndex = 2;
			this.splitter.TabStop = false;
			// 
			// logListPanel
			// 
			this.logListPanel.Controls.Add(this.logListView);
			this.logListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logListPanel.Location = new System.Drawing.Point(0, 0);
			this.logListPanel.Name = "logListPanel";
			this.logListPanel.Size = new System.Drawing.Size(349, 454);
			this.logListPanel.TabIndex = 3;
			// 
			// MyTraceForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 454);
			this.ControlBox = false;
			this.Controls.Add(this.logListPanel);
			this.Controls.Add(this.splitter);
			this.Controls.Add(this.logTreeView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "MyTraceForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "iFolder Trace Window";
			this.SizeChanged += new System.EventHandler(this.MyTraceForm_SizeChanged);
			this.logListPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		#region Event Handlers
		
		private void scrollLockMenuItem_Click(object sender, EventArgs e)
		{
			scrollLockMenuItem.Checked = !scrollLockMenuItem.Checked;
			traceListener.ScrollLock = scrollLockMenuItem.Checked;
		}

		private void copyMenuItem_Click(object sender, EventArgs e)
		{
			StringBuilder buffer = new StringBuilder();
			
			foreach (ListViewItem item in logListView.SelectedItems)
			{
				buffer.AppendFormat("{0}{1}", item.Text, Environment.NewLine);
			}

			Clipboard.SetDataObject(buffer.ToString(), true);
		}

		private void clearMenuItem_Click(object sender, EventArgs e)
		{
			logListView.Items.Clear();
		}

		private void limitMenuItem_Click(object sender, System.EventArgs e)
		{
			LimitSize limitSize = new LimitSize();
			limitSize.SizeLimit = traceListener.SizeLimit;
			if (limitSize.ShowDialog() == DialogResult.OK)
			{
				traceListener.SizeLimit = limitSize.SizeLimit;
			}		
		}

		private void MyTraceForm_SizeChanged(object sender, EventArgs e)
		{
			// update the width of the column
			logListView.Columns[0].Width = (logListView.Size.Width
				- logListView.Columns[1].Width - logListView.Columns[2].Width
				- SCROLLBAR_WIDTH - 1);
		}

		private void logTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
		}

		#endregion

		#region Properties
		
		/// <summary>
		/// Has this form received a shutdown message?
		/// </summary>
		public bool Shutdown
		{
			get { return shutdown; }
		}

		#endregion
	}
}
