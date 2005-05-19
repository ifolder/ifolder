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
 *  Author: Russ Young
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Reflection;
using System.Xml;

using Simias.Client;


namespace StoreBrowser
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		Hashtable recentStores = new Hashtable();

		string hostName;
		string username;
		string password;
		IStoreBrowser browser = null;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.TreeView tView;
		private System.Windows.Forms.MenuItem MI_OpenStore;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader Value;
		private System.Windows.Forms.ColumnHeader Type;
		private System.Windows.Forms.ColumnHeader Flags;
		private System.Windows.Forms.ColumnHeader CName;
		private System.Windows.Forms.ContextMenu NodeMenu;
		private System.Windows.Forms.MenuItem cmDelete;
		private System.Windows.Forms.ContextMenu PropertyMenu;
		private System.Windows.Forms.MenuItem pcmDelete;
		private System.Windows.Forms.MenuItem pcmNew;
		private System.Windows.Forms.MenuItem pcmEdit;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem MI_RecentS;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItemProperty;
		private System.Windows.Forms.MenuItem menuItemXml;
		private System.Windows.Forms.MenuItem menuItemExit;
		private System.Windows.Forms.MenuItem menuItem10;

		private CertPolicy certPolicy = new CertPolicy();

		public bool IsXmlView
		{
			get { return menuItemXml.Checked; }
		}

		public Form1()
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
				if (components != null) 
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Form1));
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.MI_OpenStore = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuItemExit = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.MI_RecentS = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.menuItemProperty = new System.Windows.Forms.MenuItem();
			this.menuItemXml = new System.Windows.Forms.MenuItem();
			this.tView = new System.Windows.Forms.TreeView();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.listView1 = new System.Windows.Forms.ListView();
			this.CName = new System.Windows.Forms.ColumnHeader();
			this.Value = new System.Windows.Forms.ColumnHeader();
			this.Type = new System.Windows.Forms.ColumnHeader();
			this.Flags = new System.Windows.Forms.ColumnHeader();
			this.NodeMenu = new System.Windows.Forms.ContextMenu();
			this.cmDelete = new System.Windows.Forms.MenuItem();
			this.PropertyMenu = new System.Windows.Forms.ContextMenu();
			this.pcmDelete = new System.Windows.Forms.MenuItem();
			this.pcmNew = new System.Windows.Forms.MenuItem();
			this.pcmEdit = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem9});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.MI_OpenStore,
																					  this.menuItem7,
																					  this.menuItem5,
																					  this.menuItem6,
																					  this.menuItem10,
																					  this.menuItemExit,
																					  this.menuItem2,
																					  this.MI_RecentS});
			this.menuItem1.Text = "&File";
			// 
			// MI_OpenStore
			// 
			this.MI_OpenStore.Index = 0;
			this.MI_OpenStore.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.MI_OpenStore.Text = "&Open Store";
			this.MI_OpenStore.Click += new System.EventHandler(this.MI_Open_Store_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.Text = "-";
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 2;
			this.menuItem5.Text = "&Export";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 3;
			this.menuItem6.Text = "&Import";
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 4;
			this.menuItem10.Text = "-";
			// 
			// menuItemExit
			// 
			this.menuItemExit.Index = 5;
			this.menuItemExit.Text = "E&xit";
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 6;
			this.menuItem2.Text = "-";
			// 
			// MI_RecentS
			// 
			this.MI_RecentS.Index = 7;
			this.MI_RecentS.Text = "&Recent Stores";
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 1;
			this.menuItem9.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItemProperty,
																					  this.menuItemXml});
			this.menuItem9.Text = "&View";
			// 
			// menuItemProperty
			// 
			this.menuItemProperty.Checked = true;
			this.menuItemProperty.Index = 0;
			this.menuItemProperty.Text = "&Property";
			this.menuItemProperty.Click += new System.EventHandler(this.menuItemProperty_Click);
			// 
			// menuItemXml
			// 
			this.menuItemXml.Index = 1;
			this.menuItemXml.Text = "&Xml";
			this.menuItemXml.Click += new System.EventHandler(this.menuItemXml_Click);
			// 
			// tView
			// 
			this.tView.Dock = System.Windows.Forms.DockStyle.Left;
			this.tView.ImageIndex = -1;
			this.tView.Location = new System.Drawing.Point(0, 0);
			this.tView.Name = "tView";
			this.tView.SelectedImageIndex = -1;
			this.tView.Size = new System.Drawing.Size(296, 574);
			this.tView.Sorted = true;
			this.tView.TabIndex = 0;
			this.tView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.tView_AfterCollapse);
			this.tView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tView_MouseUp);
			this.tView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tView_AfterSelect);
			this.tView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tView_BeforeExpand);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(296, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 574);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// richTextBox1
			// 
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Location = new System.Drawing.Point(299, 0);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(613, 574);
			this.richTextBox1.TabIndex = 3;
			this.richTextBox1.Text = "";
			this.richTextBox1.Visible = false;
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// listView1
			// 
			this.listView1.AllowColumnReorder = true;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.CName,
																						this.Value,
																						this.Type,
																						this.Flags});
			this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.Location = new System.Drawing.Point(299, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(613, 574);
			this.listView1.TabIndex = 4;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.Resize += new System.EventHandler(this.listView1_Resize);
			this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView1_MouseUp);
			// 
			// CName
			// 
			this.CName.Text = "Name";
			this.CName.Width = 40;
			// 
			// Value
			// 
			this.Value.Text = "Value";
			this.Value.Width = 40;
			// 
			// Type
			// 
			this.Type.Text = "Type";
			this.Type.Width = 40;
			// 
			// Flags
			// 
			this.Flags.Text = "Flags";
			this.Flags.Width = 40;
			// 
			// NodeMenu
			// 
			this.NodeMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.cmDelete});
			// 
			// cmDelete
			// 
			this.cmDelete.Index = 0;
			this.cmDelete.Text = "Delete";
			this.cmDelete.Click += new System.EventHandler(this.cmDelete_Click);
			// 
			// PropertyMenu
			// 
			this.PropertyMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.pcmDelete,
																						 this.pcmNew,
																						 this.pcmEdit});
			// 
			// pcmDelete
			// 
			this.pcmDelete.Index = 0;
			this.pcmDelete.Text = "Delete";
			this.pcmDelete.Click += new System.EventHandler(this.pcmDelete_Click);
			// 
			// pcmNew
			// 
			this.pcmNew.Index = 1;
			this.pcmNew.Text = "New";
			this.pcmNew.Click += new System.EventHandler(this.pcmNew_Click);
			// 
			// pcmEdit
			// 
			this.pcmEdit.Index = 2;
			this.pcmEdit.Text = "Edit";
			this.pcmEdit.Click += new System.EventHandler(this.pcmEdit_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(912, 574);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.tView);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.Text = "Store Browser";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void On_RecentS_Click(object sender, System.EventArgs e)
		{
			bool isLocal = false;
			hostName = ((MenuItem)sender).Text;
			if ( recentStores.ContainsKey( hostName ) )
			{
				RecentStore rs = recentStores[ hostName ] as RecentStore;
				username = rs.UserName;
				password = rs.Password;
				isLocal = rs.Local;
			}
			else
			{
				username = null;
				password = null;
			}

			// Save off the old browser.
			IStoreBrowser oldBrowser = browser;

			if ( GetBrowserWithCredentials( isLocal ) )
			{
				this.Text = "Store Browser : " + hostName;

				listView1.Items.Clear();
				tView.Nodes.Clear();
				richTextBox1.Clear();

				browser.Show();
				AddRecentMI( new RecentStore( hostName, username, password, isLocal ) );
			}
			else
			{
				browser = oldBrowser;
				username = browser.UserName;
				password = browser.Password;
			}
		}

		private void AddRecentMI( RecentStore rs )
		{
			bool found = false;
			foreach (MenuItem item in MI_RecentS.MenuItems)
			{
				if (item.Text == rs.Host)
				{
					found = true;
					break;
				}
			}

			if (!found)
			{
				MI_RecentS.MenuItems.Add(new MenuItem(rs.Host, new EventHandler(On_RecentS_Click)));
			}

			// Always set this as new passwords may need to be set.
			recentStores[ rs.Host ] = rs;
		}

		private void MI_Open_Store_Click(object sender, System.EventArgs e)
		{
			HostDialog hDiag = new HostDialog(hostName);
			while ( true )
			{
				if (hDiag.ShowDialog() == DialogResult.OK)
				{
					bool isLocal = false;
					hostName = hDiag.HostName;
					if ( recentStores.ContainsKey( hostName ) )
					{
						RecentStore rs = recentStores[ hostName ] as RecentStore;
						username = rs.UserName;
						password = rs.Password;
						isLocal = rs.Local;
					}
					else
					{
						username = null;
						password = null;
					}

					tView.Nodes.Clear();
					richTextBox1.Clear();

					if ( GetBrowserWithCredentials( isLocal ) )
					{
						browser.Show();
						AddRecentMI( new RecentStore( hostName, username, password, isLocal ) );
						break;
					}
				}
				else
				{
					break;
				}
			}
		}

		private void tView_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			browser.ShowNode(e.Node);
		}

		private void tView_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			e.Node.Nodes.Clear();
			browser.AddChildren(e.Node);	
		}

		private void tView_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			if (e.Node.Tag != null)
			{
				e.Node.Nodes.Clear();
				e.Node.Nodes.Add("Temp");
			}
			else
			{
				browser.Show();
			}
		}

		private void listView1_Resize(object sender, System.EventArgs e)
		{
			int cwidth = listView1.Width / 4;
			listView1.Columns[0].Width = listView1.Columns[1].Width = listView1.Columns[2].Width = listView1.Columns[3].Width = cwidth;
		}

		private void tView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				TreeNode tn = tView.GetNodeAt(e.X, e.Y);
				tView.SelectedNode = tn;
				NodeMenu.Show(tView, new Point(e.X, e.Y));
			}
		}

		private void cmDelete_Click(object sender, System.EventArgs e)
		{
			tView.BeginUpdate();
			TreeNode tn = tView.SelectedNode;
			DisplayNode dspNode = (DisplayNode)tn.Tag;
			if (dspNode != null)
			{
				if (dspNode.IsCollection)
				{
					try
					{
						DialogResult result = MessageBox.Show("Do you really want to delete this collection?", "Delete collection", MessageBoxButtons.YesNo);
						if (result == DialogResult.Yes)
						{
							browser.StoreBrowser.DeleteCollection(dspNode.ID);
							tn.Remove();
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message, "Error deleting collection");
					}
				}
				else
				{
					try
					{
						DialogResult result = MessageBox.Show("Do you really want to delete this node?", "Delete node", MessageBoxButtons.YesNo);
						if (result == DialogResult.Yes)
						{
							browser.StoreBrowser.DeleteNode(dspNode.CollectionID, dspNode.ID);
							tn.Remove();
						}
					}
					catch(Exception ex)
					{
						MessageBox.Show(ex.Message, "Error deleting node");
					}
				}
			}

			tView.EndUpdate();
			tView.Update();
		}

		private void CmNew_Click(object sender, System.EventArgs e)
		{
		}

		private void listView1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				ListViewItem lvi = listView1.GetItemAt(e.X, e.Y);
				if (lvi != null)
				{
					lvi.Selected = true;
					pcmDelete.Enabled = true;
				}
				else
				{
					pcmDelete.Enabled = false;
				}
				PropertyMenu.Show(listView1, new Point(e.X, e.Y));
			}
		}

		private void pcmEdit_Click(object sender, System.EventArgs e)
		{
			DisplayNode dspNode = (DisplayNode)tView.SelectedNode.Tag;
			if (dspNode != null)
			{
				ListView.SelectedListViewItemCollection itemList = listView1.SelectedItems;
				if (itemList.Count == 1)
				{
					ListViewItem.ListViewSubItemCollection items = itemList[0].SubItems;
					foreach(DisplayProperty p in dspNode)
					{
						if ((p.Name == items[0].Text) && (p.Value == items[1].Text))
						{
							try
							{
								new PropertyForm(browser.StoreBrowser, dspNode, p).ShowDialog();
							}
							catch(Exception ex)
							{
								MessageBox.Show(ex.Message, "Error editing node property");
							}
							break;
						}
					}
				}

				// Get a refreshed node.
				BrowserNode bn = browser.StoreBrowser.GetNodeByID(dspNode.CollectionID, dspNode.ID);
				if (bn != null)
				{
					if ( dspNode is DisplayShallowNode )
					{
						tView.SelectedNode.Tag = new DisplayShallowNode(bn);
					}
					else
					{
						tView.SelectedNode.Tag = new DisplayNode(bn);
					}

					browser.ShowNode(tView.SelectedNode);
					listView1.Refresh();
				}
			}
		}

		private void pcmNew_Click(object sender, System.EventArgs e)
		{
			DisplayNode dspNode = (DisplayNode)tView.SelectedNode.Tag;
			if (dspNode != null)
			{
				new PropertyForm(browser.StoreBrowser, dspNode, null).ShowDialog();
			}

			// Get a refreshed node.
			BrowserNode bn = browser.StoreBrowser.GetNodeByID(dspNode.CollectionID, dspNode.ID);
			if (bn != null)
			{
				if ( dspNode is DisplayShallowNode )
				{
					tView.SelectedNode.Tag = new DisplayShallowNode(bn);
				}
				else
				{
					tView.SelectedNode.Tag = new DisplayNode(bn);
				}

				browser.ShowNode(tView.SelectedNode);
				listView1.Refresh();
			}
		}

		private void pcmDelete_Click(object sender, System.EventArgs e)
		{
			DisplayNode dspNode = (DisplayNode)tView.SelectedNode.Tag;
			if (dspNode != null)
			{
				ListView.SelectedListViewItemCollection itemList = listView1.SelectedItems;
				foreach (ListViewItem item in itemList)
				{
					ListViewItem.ListViewSubItemCollection subItems = item.SubItems;
					foreach(DisplayProperty p in dspNode)
					{
						if ((p.Name == subItems[0].Text) && (p.Value == subItems[1].Text))
						{
							try
							{
								string type = subItems[2].Text;
								string val = ( type == "DateTime" ) ? new DateTime( Convert.ToInt64( subItems[1].Text ) ).ToString() : subItems[1].Text;
								browser.StoreBrowser.DeleteProperty(dspNode.CollectionID, dspNode.ID, subItems[0].Text, type, val);
							}
							catch(Exception ex)
							{
								MessageBox.Show(ex.Message, "Error deleting node property");
							}

							break;
						}
					}
				}

				// Get a refreshed node.
				BrowserNode bn = browser.StoreBrowser.GetNodeByID(dspNode.CollectionID, dspNode.ID);
				if (bn != null)
				{
					if ( dspNode is DisplayShallowNode )
					{
						tView.SelectedNode.Tag = new DisplayShallowNode(bn);
					}
					else
					{
						tView.SelectedNode.Tag = new DisplayNode(bn);
					}

					browser.ShowNode(tView.SelectedNode);
					listView1.Refresh();
				}
			}
		}

		private void Form1_Load(object sender, System.EventArgs e)
		{
			this.listView1.Hide();
			tView.ImageList = imageList1;
			tView.Dock = DockStyle.Fill;

			LoadRecentStores();

			Uri uri = Manager.LocalServiceUrl;
			if ( uri != null )
			{
				hostName = uri.AbsoluteUri;
				if ( GetBrowserWithCredentials( true ) )
				{
					this.Text = "Store Browser : " + hostName;
					browser.Show();
					AddRecentMI( new RecentStore( hostName ) );
				}
			}
			else
			{
				hostName = "https://ifoldertest218.provo.novell.com/simias10";
				HostDialog hDiag = new HostDialog(hostName);
				while ( true )
				{
					if (hDiag.ShowDialog() == DialogResult.OK)
					{
						hostName = hDiag.HostName;
						if ( GetBrowserWithCredentials( false ) )
						{
							this.Text = "Store Browser : " + hostName;
							browser.Show();
							AddRecentMI( new RecentStore( hostName, username, password ) );
							break;
						}
					}
					else
					{
						SaveRecentStores();
						Application.Exit();
						break;
					}
				}
			}
		}

		private bool GetBrowserWithCredentials( bool local )
		{
			bool result = false;

			try
			{
				browser = new NodeBrowser2(this, tView, listView1, richTextBox1, hostName);

				this.Cursor = Cursors.WaitCursor;
				bool needsAuth = browser.NeedsAuthentication();
				this.Cursor = Cursors.Default;

				if ( needsAuth )
				{
					if ( local )
					{
						this.Cursor = Cursors.WaitCursor;
						result = browser.ValidateCredentials();
						this.Cursor = Cursors.Default;
					}
					else
					{
						this.Cursor = Cursors.WaitCursor;
						result = browser.ValidateCredentials( username, password );
						this.Cursor = Cursors.Default;

						if ( result == false )
						{
							Credentials credDiag = new Credentials( hostName, browser );
							if ( credDiag.ShowDialog() == DialogResult.OK )
							{
								username = browser.UserName;
								password = browser.Password;
								result = true;
							}
						}
					}
				}
			}
			catch ( System.Web.Services.Protocols.SoapHeaderException )
			{
				browser = new NodeBrowser(this, tView, listView1, richTextBox1, hostName);

				this.Cursor = Cursors.WaitCursor;
				bool needsAuth = browser.NeedsAuthentication();
				this.Cursor = Cursors.Default;

				if ( needsAuth )
				{
					if ( local )
					{
						this.Cursor = Cursors.WaitCursor;
						result = browser.ValidateCredentials();
						this.Cursor = Cursors.Default;
					}
					else
					{
						this.Cursor = Cursors.WaitCursor;
						result = browser.ValidateCredentials( username, password );
						this.Cursor = Cursors.Default;

						if ( result == false )
						{
							Credentials credDiag = new Credentials( hostName, browser );
							if ( credDiag.ShowDialog() == DialogResult.OK )
							{
								username = browser.UserName;
								password = browser.Password;
								result = true;
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
				MessageBox.Show( ex.Message, "Error", MessageBoxButtons.OK );
			}

			return result;
		}

		private void menuItemXml_Click(object sender, System.EventArgs e)
		{
			menuItemProperty.Checked = false;
			menuItemXml.Checked = true;
			browser.ShowNode(tView.SelectedNode);
		}

		private void menuItemProperty_Click(object sender, System.EventArgs e)
		{
			menuItemProperty.Checked = true;
			menuItemXml.Checked = false;
			browser.ShowNode(tView.SelectedNode);
		}

		private void menuItemExit_Click(object sender, System.EventArgs e)
		{
			SaveRecentStores();
			Application.Exit();
		}

		private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SaveRecentStores();
		}

		private void SaveRecentStores()
		{
			string path = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "StoreBrowser.cfg" );
			if ( File.Exists( path ) )
			{
				File.Delete( path );
			}

			using ( StreamWriter sw = new StreamWriter( path ) )
			{
				foreach ( RecentStore rs in recentStores.Values )
				{
					sw.WriteLine( rs.ToString() );
				}
			}
		}

		private void LoadRecentStores()
		{
			string path = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "StoreBrowser.cfg" );
			if ( File.Exists( path ) )
			{
				using ( StreamReader sr = new StreamReader( path ) )
				{
					string line = sr.ReadLine();
					while ( line != null )
					{
						RecentStore rs = new RecentStore();
						rs.FromString( line );
						AddRecentMI( rs );
						line = sr.ReadLine();
					}
				}
			}
		}
	}

	public interface IStoreBrowser : IDisposable
	{
		void Show();
		void ShowNode(TreeNode node);
		void AddChildren(TreeNode tNode);
		bool NeedsAuthentication();
		bool ValidateCredentials();
		bool ValidateCredentials( string userName, string password );
		BrowserService StoreBrowser { get; }
		string UserName { get; }
		string Password { get; }
	}

	public class RecentStore
	{
		private string host;
		private string userName;
		private string password;
		private bool local;

		public string Host
		{
			get { return host; }
			set { host = value; }
		}

		public string UserName
		{
			get { return userName; }
			set { userName = value; }
		}

		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		public bool Local
		{
			get { return local; }
			set { local = value; }
		}

		public RecentStore()
		{
			this.host = null;
			this.userName = null;
			this.password = null;
			this.local = false;
		}

		public RecentStore( string host, string userName, string password, bool local )
		{
			this.host = host;
			this.userName = userName;
			this.password = password;
			this.local = local;
		}

		public RecentStore( string host ) :
			this ( host, null, null, true )
		{
		}

		public RecentStore( string host, string userName, string password ) : 
			this( host, userName, password, false )
		{
		}

		public void FromString( string fileString )
		{
			string[] list = fileString.Split( new Char[] { ' ' } );
			host = list[ 1 ];
			userName = null;
			password = null;
			local = Convert.ToBoolean( list[ 0 ] );
		}

		public override string ToString()
		{
			return String.Format( "{0} {1}", local, host );
		}
	}
}
