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

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for Credits.
	/// </summary>
	public class Credits : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.ListBox listBox2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Credits()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Credits));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.close = new System.Windows.Forms.Button();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.AccessibleDescription = resources.GetString("tabControl1.AccessibleDescription");
			this.tabControl1.AccessibleName = resources.GetString("tabControl1.AccessibleName");
			this.tabControl1.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tabControl1.Alignment")));
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabControl1.Anchor")));
			this.tabControl1.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tabControl1.Appearance")));
			this.tabControl1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabControl1.BackgroundImage")));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
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
			// close
			// 
			this.close.AccessibleDescription = resources.GetString("close.AccessibleDescription");
			this.close.AccessibleName = resources.GetString("close.AccessibleName");
			this.close.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("close.Anchor")));
			this.close.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("close.BackgroundImage")));
			this.close.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.close.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("close.Dock")));
			this.close.Enabled = ((bool)(resources.GetObject("close.Enabled")));
			this.close.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("close.FlatStyle")));
			this.close.Font = ((System.Drawing.Font)(resources.GetObject("close.Font")));
			this.close.Image = ((System.Drawing.Image)(resources.GetObject("close.Image")));
			this.close.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("close.ImageAlign")));
			this.close.ImageIndex = ((int)(resources.GetObject("close.ImageIndex")));
			this.close.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("close.ImeMode")));
			this.close.Location = ((System.Drawing.Point)(resources.GetObject("close.Location")));
			this.close.Name = "close";
			this.close.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("close.RightToLeft")));
			this.close.Size = ((System.Drawing.Size)(resources.GetObject("close.Size")));
			this.close.TabIndex = ((int)(resources.GetObject("close.TabIndex")));
			this.close.Text = resources.GetString("close.Text");
			this.close.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("close.TextAlign")));
			this.close.Visible = ((bool)(resources.GetObject("close.Visible")));
			// 
			// tabPage1
			// 
			this.tabPage1.AccessibleDescription = resources.GetString("tabPage1.AccessibleDescription");
			this.tabPage1.AccessibleName = resources.GetString("tabPage1.AccessibleName");
			this.tabPage1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tabPage1.Anchor")));
			this.tabPage1.AutoScroll = ((bool)(resources.GetObject("tabPage1.AutoScroll")));
			this.tabPage1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tabPage1.AutoScrollMargin")));
			this.tabPage1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tabPage1.AutoScrollMinSize")));
			this.tabPage1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage1.BackgroundImage")));
			this.tabPage1.Controls.Add(this.listBox1);
			this.tabPage1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tabPage1.Dock")));
			this.tabPage1.Enabled = ((bool)(resources.GetObject("tabPage1.Enabled")));
			this.tabPage1.Font = ((System.Drawing.Font)(resources.GetObject("tabPage1.Font")));
			this.tabPage1.ImageIndex = ((int)(resources.GetObject("tabPage1.ImageIndex")));
			this.tabPage1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tabPage1.ImeMode")));
			this.tabPage1.Location = ((System.Drawing.Point)(resources.GetObject("tabPage1.Location")));
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tabPage1.RightToLeft")));
			this.tabPage1.Size = ((System.Drawing.Size)(resources.GetObject("tabPage1.Size")));
			this.tabPage1.TabIndex = ((int)(resources.GetObject("tabPage1.TabIndex")));
			this.tabPage1.Text = resources.GetString("tabPage1.Text");
			this.tabPage1.ToolTipText = resources.GetString("tabPage1.ToolTipText");
			this.tabPage1.Visible = ((bool)(resources.GetObject("tabPage1.Visible")));
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
			this.tabPage2.Controls.Add(this.listBox2);
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
			// listBox1
			// 
			this.listBox1.AccessibleDescription = resources.GetString("listBox1.AccessibleDescription");
			this.listBox1.AccessibleName = resources.GetString("listBox1.AccessibleName");
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listBox1.Anchor")));
			this.listBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listBox1.BackgroundImage")));
			this.listBox1.ColumnWidth = ((int)(resources.GetObject("listBox1.ColumnWidth")));
			this.listBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listBox1.Dock")));
			this.listBox1.Enabled = ((bool)(resources.GetObject("listBox1.Enabled")));
			this.listBox1.Font = ((System.Drawing.Font)(resources.GetObject("listBox1.Font")));
			this.listBox1.HorizontalExtent = ((int)(resources.GetObject("listBox1.HorizontalExtent")));
			this.listBox1.HorizontalScrollbar = ((bool)(resources.GetObject("listBox1.HorizontalScrollbar")));
			this.listBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listBox1.ImeMode")));
			this.listBox1.IntegralHeight = ((bool)(resources.GetObject("listBox1.IntegralHeight")));
			this.listBox1.ItemHeight = ((int)(resources.GetObject("listBox1.ItemHeight")));
			this.listBox1.Items.AddRange(new object[] {
														  resources.GetString("listBox1.Items"),
														  resources.GetString("listBox1.Items1"),
														  resources.GetString("listBox1.Items2"),
														  resources.GetString("listBox1.Items3"),
														  resources.GetString("listBox1.Items4"),
														  resources.GetString("listBox1.Items5"),
														  resources.GetString("listBox1.Items6"),
														  resources.GetString("listBox1.Items7"),
														  resources.GetString("listBox1.Items8"),
														  resources.GetString("listBox1.Items9"),
														  resources.GetString("listBox1.Items10"),
														  resources.GetString("listBox1.Items11"),
														  resources.GetString("listBox1.Items12"),
														  resources.GetString("listBox1.Items13"),
														  resources.GetString("listBox1.Items14"),
														  resources.GetString("listBox1.Items15"),
														  resources.GetString("listBox1.Items16"),
														  resources.GetString("listBox1.Items17"),
														  resources.GetString("listBox1.Items18"),
														  resources.GetString("listBox1.Items19"),
														  resources.GetString("listBox1.Items20"),
														  resources.GetString("listBox1.Items21"),
														  resources.GetString("listBox1.Items22"),
														  resources.GetString("listBox1.Items23"),
														  resources.GetString("listBox1.Items24"),
														  resources.GetString("listBox1.Items25"),
														  resources.GetString("listBox1.Items26"),
														  resources.GetString("listBox1.Items27"),
														  resources.GetString("listBox1.Items28"),
														  resources.GetString("listBox1.Items29"),
														  resources.GetString("listBox1.Items30")});
			this.listBox1.Location = ((System.Drawing.Point)(resources.GetObject("listBox1.Location")));
			this.listBox1.Name = "listBox1";
			this.listBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listBox1.RightToLeft")));
			this.listBox1.ScrollAlwaysVisible = ((bool)(resources.GetObject("listBox1.ScrollAlwaysVisible")));
			this.listBox1.Size = ((System.Drawing.Size)(resources.GetObject("listBox1.Size")));
			this.listBox1.TabIndex = ((int)(resources.GetObject("listBox1.TabIndex")));
			this.listBox1.Visible = ((bool)(resources.GetObject("listBox1.Visible")));
			// 
			// listBox2
			// 
			this.listBox2.AccessibleDescription = resources.GetString("listBox2.AccessibleDescription");
			this.listBox2.AccessibleName = resources.GetString("listBox2.AccessibleName");
			this.listBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("listBox2.Anchor")));
			this.listBox2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("listBox2.BackgroundImage")));
			this.listBox2.ColumnWidth = ((int)(resources.GetObject("listBox2.ColumnWidth")));
			this.listBox2.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("listBox2.Dock")));
			this.listBox2.Enabled = ((bool)(resources.GetObject("listBox2.Enabled")));
			this.listBox2.Font = ((System.Drawing.Font)(resources.GetObject("listBox2.Font")));
			this.listBox2.HorizontalExtent = ((int)(resources.GetObject("listBox2.HorizontalExtent")));
			this.listBox2.HorizontalScrollbar = ((bool)(resources.GetObject("listBox2.HorizontalScrollbar")));
			this.listBox2.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("listBox2.ImeMode")));
			this.listBox2.IntegralHeight = ((bool)(resources.GetObject("listBox2.IntegralHeight")));
			this.listBox2.ItemHeight = ((int)(resources.GetObject("listBox2.ItemHeight")));
			this.listBox2.Items.AddRange(new object[] {
														  resources.GetString("listBox2.Items")});
			this.listBox2.Location = ((System.Drawing.Point)(resources.GetObject("listBox2.Location")));
			this.listBox2.Name = "listBox2";
			this.listBox2.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("listBox2.RightToLeft")));
			this.listBox2.ScrollAlwaysVisible = ((bool)(resources.GetObject("listBox2.ScrollAlwaysVisible")));
			this.listBox2.Size = ((System.Drawing.Size)(resources.GetObject("listBox2.Size")));
			this.listBox2.TabIndex = ((int)(resources.GetObject("listBox2.TabIndex")));
			this.listBox2.Visible = ((bool)(resources.GetObject("listBox2.Visible")));
			// 
			// Credits
			// 
			this.AcceptButton = this.close;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.close);
			this.Controls.Add(this.tabControl1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "Credits";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
