using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Novell.iFolder;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	[ComVisible(false)]
	public class ConflictResolver : System.Windows.Forms.Form
	{
		#region Class Members
		private iFolderManager manager;
		private iFolder ifolder;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private int labelDelta;
		private System.Windows.Forms.ListView localFiles;
		private System.Windows.Forms.ListView conflictFiles;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public ConflictResolver()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			labelDelta = label2.Left - splitter1.Left;
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.conflictFiles = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.localFiles = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.Controls.Add(this.conflictFiles);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.localFiles);
			this.panel1.Location = new System.Drawing.Point(8, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(528, 416);
			this.panel1.TabIndex = 1;
			// 
			// conflictFiles
			// 
			this.conflictFiles.CheckBoxes = true;
			this.conflictFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader4,
																							this.columnHeader5});
			this.conflictFiles.ContextMenu = this.contextMenu1;
			this.conflictFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.conflictFiles.Location = new System.Drawing.Point(251, 0);
			this.conflictFiles.Name = "conflictFiles";
			this.conflictFiles.Size = new System.Drawing.Size(277, 416);
			this.conflictFiles.TabIndex = 2;
			this.conflictFiles.View = System.Windows.Forms.View.Details;
			this.conflictFiles.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.conflictFiles_ItemCheck);
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Name";
			this.columnHeader4.Width = 91;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Date Modified";
			this.columnHeader5.Width = 182;
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuOpen});
			this.contextMenu1.Popup += new System.EventHandler(this.contextMenu1_Popup);
			// 
			// menuOpen
			// 
			this.menuOpen.Index = 0;
			this.menuOpen.Text = "Open";
			this.menuOpen.Click += new System.EventHandler(this.menuOpen_Click);
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(248, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 416);
			this.splitter1.TabIndex = 1;
			this.splitter1.TabStop = false;
			this.splitter1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter1_SplitterMoved);
			// 
			// localFiles
			// 
			this.localFiles.CheckBoxes = true;
			this.localFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						 this.columnHeader1,
																						 this.columnHeader2,
																						 this.columnHeader3});
			this.localFiles.ContextMenu = this.contextMenu1;
			this.localFiles.Dock = System.Windows.Forms.DockStyle.Left;
			this.localFiles.FullRowSelect = true;
			this.localFiles.GridLines = true;
			this.localFiles.HideSelection = false;
			this.localFiles.Location = new System.Drawing.Point(0, 0);
			this.localFiles.Name = "localFiles";
			this.localFiles.Size = new System.Drawing.Size(248, 416);
			this.localFiles.TabIndex = 0;
			this.localFiles.View = System.Windows.Forms.View.Details;
			this.localFiles.DoubleClick += new System.EventHandler(this.localFiles_DoubleClick);
			this.localFiles.SelectedIndexChanged += new System.EventHandler(this.localFiles_SelectedIndexChanged);
			this.localFiles.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.localFiles_ItemCheck);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Location";
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Date Modified";
			this.columnHeader3.Width = 124;
			// 
			// toolBar1
			// 
			this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						this.toolBarButton1,
																						this.toolBarButton2,
																						this.toolBarButton3});
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(544, 42);
			this.toolBar1.TabIndex = 2;
			this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Text = "Open";
			this.toolBarButton1.ToolTipText = "Open the selected file.";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Text = "Resolve";
			this.toolBarButton2.ToolTipText = "Process the list of files.";
			// 
			// toolBarButton3
			// 
			this.toolBarButton3.Text = "Refresh";
			this.toolBarButton3.ToolTipText = "Update the list of files.";
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuFile,
																					  this.menuItem1,
																					  this.menuItem5});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItem2,
																					 this.menuItem3});
			this.menuFile.Text = "&File";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 0;
			this.menuItem2.Text = "Open";
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "";
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 1;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem4});
			this.menuItem1.Text = "&View";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 0;
			this.menuItem4.Text = "&Refresh";
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 2;
			this.menuItem5.Text = "&Help";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = "Local Files:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(264, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Conflict File:";
			// 
			// ConflictResolver
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(544, 518);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.toolBar1);
			this.Controls.Add(this.panel1);
			this.Menu = this.mainMenu1;
			this.Name = "ConflictResolver";
			this.Text = "Conflict Resolver";
			this.Load += new System.EventHandler(this.ConflictResolver_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private void OpenLocalFile()
		{
			ListViewItem lvi = localFiles.SelectedItems[0];
			CollisionNode cn = (CollisionNode)lvi.Tag;
			iFolder ifolder = manager.GetiFolderById(cn.LocalNode.Properties.GetSingleProperty("CollectionId").ToString());
			string path = cn.LocalNode.GetFullPath(ifolder);
			//			string path = Path.Combine(lvi.SubItems[1].Text, lvi.SubItems[0].Text);
			try
			{
				Process.Start(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void OpenConflictFile()
		{
			ListViewItem lvi = conflictFiles.SelectedItems[0];

			try
			{
				Process.Start(lvi.Text);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		private void PutCollisionsInList(iFolder ifolder)
		{
			ICSList collisionList = ifolder.GetCollisions();
			foreach (ShallowNode sn in collisionList)
			{
				// Get the collision node.
				Node conflictNode = new Node(ifolder, sn);
				if (ifolder.IsType(conflictNode, typeof(BaseFileNode).Name))
				{
					// Get the node that the collision occurred with.
					FileNode fileNode = new FileNode(ifolder.GetNodeFromCollision(conflictNode));
					//string collectionID = fileNode.Properties.GetSingleProperty("CollectionId").ToString();

					string path = Path.GetDirectoryName(fileNode.GetFullPath(ifolder));
					string name = fileNode.GetFileName();
					string[] items = new string[] {name, path, fileNode.LastWriteTime.ToString()};

					Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(ifolder, fileNode);

					ListViewItem lvi = new ListViewItem(items);
					CollisionNode cNode = new CollisionNode();
					cNode.LocalNode = fileNode;
					cNode.ConflictNode =  sn;
					cNode.ConflictPath = conflict.UpdateConflictPath;
					lvi.Tag = cNode;
					lvi.StateImageIndex = 0;
					this.localFiles.Items.Add(lvi);
				}
			}
		}

		private void ProcessEntry(ListViewItem lviLocal)
		{
			try
			{
				CollisionNode cn = (CollisionNode)lviLocal.Tag;
				iFolder ifolder = manager.GetiFolderById(cn.LocalNode.Properties.GetSingleProperty("CollectionId").ToString());
				Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(ifolder, cn.LocalNode);

				if (lviLocal.Checked)
				{
					conflict.Resolve(true);
				}
				else if (cn.Checked)
				{
					conflict.Resolve(false);
				}
			}
			catch (SimiasException e)
			{
				e.LogError();
			}
			catch (Exception e)
			{
				// TODO - log message.
			}
		}

		private void RefreshList()
		{
			Cursor.Current = Cursors.WaitCursor;
			localFiles.Clear();
			localFiles.BeginUpdate();
			
			try
			{
				manager = iFolderManager.Connect();

				if (this.ifolder != null)
				{
					PutCollisionsInList(this.ifolder);
				}
				else
				{
					foreach (iFolder ifolder in manager)
					{
						// Temporary code to cause a collision.
/*						ICSList list = ifolder.GetNodesByType(NodeTypes.FileNodeType);
						foreach (ShallowNode sn in list)
						{
							bool b = false;
							if (b)
							{
								FileNode fileNode = new FileNode(ifolder, sn);
								Node colNode = ifolder.CreateCollision(fileNode);
								ifolder.Commit(colNode);
							}
						}
*/						// End Temporary code.

						if (ifolder.HasCollisions())
						{
							PutCollisionsInList(ifolder);
						}
					}
				}
			}
			catch (SimiasException ex)
			{
				ex.LogFatal();
				Application.Exit();
			}

			localFiles.EndUpdate();
			Cursor.Current = Cursors.Default;
		}
		#endregion

		#region Properties
		public iFolder IFolder
		{
			set
			{
				ifolder = value;
			}
		}
		#endregion

		#region Event Handlers
		private void localFiles_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (localFiles.SelectedItems.Count == 1)
			{
				ListViewItem llvi = localFiles.SelectedItems[0];
				CollisionNode cn = (CollisionNode)llvi.Tag;

				ListViewItem clvi = new ListViewItem(cn.ConflictPath);
				clvi.StateImageIndex = cn.Checked ? 1 : 0;
				conflictFiles.Items.Add(clvi);
			}
			else
			{
				conflictFiles.Items.Clear();
			}
		}

		private void localFiles_DoubleClick(object sender, System.EventArgs e)
		{
//			OpenLocalFile();
		}

		private void localFiles_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			Point point = localFiles.PointToClient(Control.MousePosition);
			ListViewItem tlvi = localFiles.GetItemAt(point.X, point.Y);
			if (localFiles.SelectedItems.Count == 1 &&
				localFiles.SelectedItems[0] == localFiles.Items[e.Index] &&
				tlvi == localFiles.Items[e.Index])
			{
				ListViewItem lvi = conflictFiles.Items[0];

				switch (e.NewValue)
				{
					case CheckState.Checked:
						lvi.StateImageIndex = 0;
						break;
//					case CheckState.Unchecked:
//						lvi.StateImageIndex = 1;
//						break;
					default:
						break;
				}
			}
		}

		private void conflictFiles_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			Point point = conflictFiles.PointToClient(Control.MousePosition);
			ListViewItem tlvi = conflictFiles.GetItemAt(point.X, point.Y);
			if (tlvi == conflictFiles.Items[e.Index])
			{
				ListViewItem lvi = localFiles.SelectedItems[0];
				CollisionNode cn = (CollisionNode)lvi.Tag;

				switch (e.NewValue)
				{
					case CheckState.Checked:
						lvi.StateImageIndex = 0;
						cn.Checked = true;
						break;
					case CheckState.Unchecked:
//						lvi.StateImageIndex = 1;
						cn.Checked = false;
						break;
					default:
						break;
				}
			}
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			if (localFiles.Focused)
			{
				OpenLocalFile();
			}
			else
			{
				OpenConflictFile();
			}
		}

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			if (localFiles.Focused)
			{
				menuOpen.Enabled = localFiles.SelectedItems.Count == 1;
			}
			else
			{
				menuOpen.Enabled = conflictFiles.SelectedItems.Count == 1;
			}
		}

		private void splitter1_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
		{
			label2.Left = e.X + labelDelta;
		}

		private void ConflictResolver_Load(object sender, System.EventArgs e)
		{
			RefreshList();
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.Text)
			{
				case "Resolve":
					Cursor.Current = Cursors.WaitCursor;

					// Process the list.
					foreach (ListViewItem lvi in localFiles.Items)
					{
						ProcessEntry(lvi);
					}

					Cursor.Current = Cursors.Default;

					// Update the listview.
					RefreshList();
					break;
				default:
					break;
			}
		}
		#endregion
	}
}
