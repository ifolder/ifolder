/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
		private ListView listView;
		private Container components = null;
		private System.Windows.Forms.ContextMenu lvContextMenu;
		private System.Windows.Forms.MenuItem pauseMenu;
		private System.Windows.Forms.MenuItem clearAllMenu;
		private System.Windows.Forms.MenuItem copyMenu;
		private MyTraceListener traceListener;

		private const int WM_QUERYENDSESSION = 0x0011;
		private bool shutdown = false;

		/// <summary>
		/// Default Constructor
		/// </summary>
		public MyTraceForm()
		{
			InitializeComponent();

			// put the window in the bottom corner
			Rectangle screen = SystemInformation.VirtualScreen;
			Point start = new Point();

			start.X = screen.Width - this.Size.Width - 1;
			start.Y = screen.Height - this.Size.Height - 30 - 1;

			this.Location = start;

			this.listView.Columns.Add("Messages",  this.listView.Size.Width - 4, HorizontalAlignment.Left);

			// start listeners
			traceListener = new MyTraceListener(listView);
			Trace.Listeners.Add(traceListener);

			// Context menu for list view.
			pauseMenu = new MenuItem("Pause");
			pauseMenu.Click += new EventHandler(pauseMenu_Click);

			copyMenu = new MenuItem("Copy");
			copyMenu.Click += new EventHandler(copyMenu_Click);

			clearAllMenu = new MenuItem("Clear all entries");
			clearAllMenu.Click += new EventHandler(clearAllMenu_Click);

			lvContextMenu = new ContextMenu();
			lvContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {pauseMenu, copyMenu, clearAllMenu});
			listView.ContextMenu = lvContextMenu;

			this.SizeChanged += new EventHandler(MyTraceForm_SizeChanged);
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
			this.listView = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(352, 214);
			this.listView.TabIndex = 0;
			this.listView.View = System.Windows.Forms.View.Details;
			// 
			// MyTraceForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(352, 214);
			this.Controls.Add(this.listView);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "MyTraceForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Denali Trace Window";
			this.ResumeLayout(false);

		}

		#endregion

		#region Event Handlers
		private void pauseMenu_Click(object sender, EventArgs e)
		{
			traceListener.Pause = !pauseMenu.Checked;
			pauseMenu.Checked = !pauseMenu.Checked;
		}

		private void clearAllMenu_Click(object sender, EventArgs e)
		{
			listView.Items.Clear();
		}

		private void copyMenu_Click(object sender, EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			foreach (ListViewItem lvitem in listView.SelectedItems)
			{
				sb.AppendFormat("{0}\r\n", lvitem.Text);
			}

			Clipboard.SetDataObject(sb.ToString(), true);
		}

		private void MyTraceForm_SizeChanged(object sender, EventArgs e)
		{
			// When the form size is changed, change the width of the column.
			this.listView.Columns[0].Width = this.listView.Size.Width - 22;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets a value telling if the form received a shutdown notification from the operating system.
		/// </summary>
		public bool Shutdown
		{
			get
			{
				return this.shutdown;
			}
		}
		#endregion

		protected override void WndProc(ref Message m)
		{
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
