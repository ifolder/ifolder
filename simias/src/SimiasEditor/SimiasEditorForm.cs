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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace Simias.Editor
{
	/// <summary>
	/// Simias Editor Form
	/// </summary>
	public class SimiasEditorForm : Form
	{
		private System.Windows.Forms.FolderBrowserDialog storeBrowserDialog;
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.TreeView treeView;
		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem fileMenuItem;
		private System.Windows.Forms.MenuItem helpMenuItem;
		private System.Windows.Forms.MenuItem openMenuItem;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem exitMenuItem;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Panel panel;
		private System.Windows.Forms.Splitter splitter;
		private System.Windows.Forms.ContextMenu treeViewMenu;
		private System.Windows.Forms.ContextMenu listViewMenu;
		private System.Windows.Forms.MenuItem openTreeViewMenuItem;
		private System.Windows.Forms.MenuItem closeTreeViewMenuItem;
		private System.Windows.Forms.MenuItem deleteTreeViewMenuItem;
		private System.Windows.Forms.MenuItem addMenuItem;
		private System.Windows.Forms.MenuItem editMenuItem;
		private System.Windows.Forms.MenuItem removeMenuItem;
		private Container components = null;

		public SimiasEditorForm()
		{
			InitializeComponent();

			try
			{
				// add the default store
				treeView.Nodes.Add(new StoreTreeNode());
			}
			catch
			{
			}
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

		#region Windows Form Designer
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.storeBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.listView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.listViewMenu = new System.Windows.Forms.ContextMenu();
			this.addMenuItem = new System.Windows.Forms.MenuItem();
			this.editMenuItem = new System.Windows.Forms.MenuItem();
			this.removeMenuItem = new System.Windows.Forms.MenuItem();
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.treeView = new System.Windows.Forms.TreeView();
			this.treeViewMenu = new System.Windows.Forms.ContextMenu();
			this.openTreeViewMenuItem = new System.Windows.Forms.MenuItem();
			this.closeTreeViewMenuItem = new System.Windows.Forms.MenuItem();
			this.deleteTreeViewMenuItem = new System.Windows.Forms.MenuItem();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.fileMenuItem = new System.Windows.Forms.MenuItem();
			this.openMenuItem = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			this.helpMenuItem = new System.Windows.Forms.MenuItem();
			this.panel = new System.Windows.Forms.Panel();
			this.splitter = new System.Windows.Forms.Splitter();
			this.panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// storeBrowserDialog
			// 
			this.storeBrowserDialog.Description = "Store Browser";
			this.storeBrowserDialog.SelectedPath = "C:\\";
			// 
			// listView
			// 
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader1,
																					   this.columnHeader2});
			this.listView.ContextMenu = this.listViewMenu;
			this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView.FullRowSelect = true;
			this.listView.GridLines = true;
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView.Location = new System.Drawing.Point(200, 0);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(432, 424);
			this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listView.TabIndex = 7;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.DoubleClick += new System.EventHandler(this.listView_DoubleClick);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Property";
			this.columnHeader1.Width = 169;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Value";
			this.columnHeader2.Width = 227;
			// 
			// listViewMenu
			// 
			this.listViewMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.addMenuItem,
																						 this.editMenuItem,
																						 this.removeMenuItem});
			// 
			// addMenuItem
			// 
			this.addMenuItem.Index = 0;
			this.addMenuItem.Text = "Add...";
			// 
			// editMenuItem
			// 
			this.editMenuItem.Index = 1;
			this.editMenuItem.Text = "Edit...";
			// 
			// removeMenuItem
			// 
			this.removeMenuItem.Index = 2;
			this.removeMenuItem.Text = "Remove";
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 424);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(632, 22);
			this.statusBar.TabIndex = 2;
			this.statusBar.Text = "Ready";
			// 
			// treeView
			// 
			this.treeView.ContextMenu = this.treeViewMenu;
			this.treeView.Dock = System.Windows.Forms.DockStyle.Left;
			this.treeView.ImageIndex = -1;
			this.treeView.Location = new System.Drawing.Point(0, 0);
			this.treeView.Name = "treeView";
			this.treeView.SelectedImageIndex = -1;
			this.treeView.Size = new System.Drawing.Size(200, 424);
			this.treeView.TabIndex = 3;
			this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
			this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeExpand);
			// 
			// treeViewMenu
			// 
			this.treeViewMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.openTreeViewMenuItem,
																						 this.closeTreeViewMenuItem,
																						 this.deleteTreeViewMenuItem});
			// 
			// openTreeViewMenuItem
			// 
			this.openTreeViewMenuItem.Index = 0;
			this.openTreeViewMenuItem.Text = "Open";
			this.openTreeViewMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
			// 
			// closeTreeViewMenuItem
			// 
			this.closeTreeViewMenuItem.Index = 1;
			this.closeTreeViewMenuItem.Text = "Close";
			// 
			// deleteTreeViewMenuItem
			// 
			this.deleteTreeViewMenuItem.Index = 2;
			this.deleteTreeViewMenuItem.Text = "Delete";
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.fileMenuItem,
																					 this.helpMenuItem});
			// 
			// fileMenuItem
			// 
			this.fileMenuItem.Index = 0;
			this.fileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.openMenuItem,
																						 this.menuItem4,
																						 this.exitMenuItem});
			this.fileMenuItem.Text = "&File";
			// 
			// openMenuItem
			// 
			this.openMenuItem.Index = 0;
			this.openMenuItem.Text = "&Open";
			this.openMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 1;
			this.menuItem4.Text = "-";
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Index = 2;
			this.exitMenuItem.Text = "E&xit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			// 
			// helpMenuItem
			// 
			this.helpMenuItem.Index = 1;
			this.helpMenuItem.Text = "&Help";
			// 
			// panel
			// 
			this.panel.Controls.Add(this.splitter);
			this.panel.Controls.Add(this.listView);
			this.panel.Controls.Add(this.treeView);
			this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel.Location = new System.Drawing.Point(0, 0);
			this.panel.Name = "panel";
			this.panel.Size = new System.Drawing.Size(632, 424);
			this.panel.TabIndex = 8;
			// 
			// splitter
			// 
			this.splitter.Location = new System.Drawing.Point(200, 0);
			this.splitter.Name = "splitter";
			this.splitter.Size = new System.Drawing.Size(3, 424);
			this.splitter.TabIndex = 0;
			this.splitter.TabStop = false;
			// 
			// SimiasEditorForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 446);
			this.Controls.Add(this.panel);
			this.Controls.Add(this.statusBar);
			this.Menu = this.mainMenu;
			this.Name = "SimiasEditorForm";
			this.Text = "Simias Editor";
			this.panel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new SimiasEditorForm());
		}

		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void openMenuItem_Click(object sender, System.EventArgs e)
		{
			if (DialogResult.OK == storeBrowserDialog.ShowDialog(this))
			{
				treeView.Nodes.Add(new StoreTreeNode(storeBrowserDialog.SelectedPath));
			}
		}

		private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			if (typeof(BaseTreeNode).IsInstanceOfType(e.Node))
			{
				(e.Node as BaseTreeNode).Refresh();
			}
		}

		private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (typeof(BaseTreeNode).IsInstanceOfType(e.Node))
			{
				(e.Node as BaseTreeNode).Show(listView);
			}
		}

		private void listView_DoubleClick(object sender, System.EventArgs e)
		{
			string name = listView.SelectedItems[0].Text;
			
			if (typeof(NodeTreeNode).IsInstanceOfType(treeView.SelectedNode))
			{
				NodeTreeNode node = (NodeTreeNode)treeView.SelectedNode;

				PropertyForm pf = new PropertyForm(node.StoreCollection, node.StoreNode, name);

				pf.ShowDialog(this);
			}
		}
	}
}
