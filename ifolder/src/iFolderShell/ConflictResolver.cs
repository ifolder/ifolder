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
using Novell.iFolder.Win32Util;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	[ComVisible(false)]
	public class ConflictResolver : System.Windows.Forms.Form
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(ConflictResolver));
		private const string applicationIcon = "ifolder_conflict_emb.ico";
		private iFolderManager manager;
		private iFolder ifolder;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButton3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private int labelDelta;
		private string loadPath;
		private bool conflictsFound = false;
		private System.Windows.Forms.ListView conflictFiles;
		private System.Windows.Forms.MenuItem menuFileOpen;
		private System.Windows.Forms.MenuItem menuView;
		private System.Windows.Forms.MenuItem menuViewRefresh;
		private System.Windows.Forms.MenuItem menuHelp;
		private System.Windows.Forms.MenuItem menuFileSeparator;
		private System.Windows.Forms.MenuItem menuFileExit;
		private System.Windows.Forms.ColumnHeader columnHeader6;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ListView localFiles;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
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

			labelDelta = label1.Left - splitter1.Left;
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
			this.localFiles = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.conflictFiles = new System.Windows.Forms.ListView();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton3 = new System.Windows.Forms.ToolBarButton();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.menuFileOpen = new System.Windows.Forms.MenuItem();
			this.menuFileSeparator = new System.Windows.Forms.MenuItem();
			this.menuFileExit = new System.Windows.Forms.MenuItem();
			this.menuView = new System.Windows.Forms.MenuItem();
			this.menuViewRefresh = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
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
			this.panel1.Controls.Add(this.localFiles);
			this.panel1.Controls.Add(this.splitter1);
			this.panel1.Controls.Add(this.conflictFiles);
			this.panel1.Location = new System.Drawing.Point(8, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(592, 416);
			this.panel1.TabIndex = 1;
			// 
			// localFiles
			// 
			this.localFiles.CheckBoxes = true;
			this.localFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						 this.columnHeader1,
																						 this.columnHeader2,
																						 this.columnHeader3});
			this.localFiles.ContextMenu = this.contextMenu1;
			this.localFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.localFiles.FullRowSelect = true;
			this.localFiles.HideSelection = false;
			this.localFiles.Location = new System.Drawing.Point(339, 0);
			this.localFiles.Name = "localFiles";
			this.localFiles.Size = new System.Drawing.Size(253, 416);
			this.localFiles.TabIndex = 4;
			this.localFiles.View = System.Windows.Forms.View.Details;
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
			this.splitter1.Location = new System.Drawing.Point(336, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 416);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			this.splitter1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter1_SplitterMoved);
			// 
			// conflictFiles
			// 
			this.conflictFiles.CheckBoxes = true;
			this.conflictFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader4,
																							this.columnHeader6,
																							this.columnHeader5});
			this.conflictFiles.ContextMenu = this.contextMenu1;
			this.conflictFiles.Dock = System.Windows.Forms.DockStyle.Left;
			this.conflictFiles.GridLines = true;
			this.conflictFiles.Location = new System.Drawing.Point(0, 0);
			this.conflictFiles.MultiSelect = false;
			this.conflictFiles.Name = "conflictFiles";
			this.conflictFiles.Size = new System.Drawing.Size(336, 416);
			this.conflictFiles.TabIndex = 2;
			this.conflictFiles.View = System.Windows.Forms.View.Details;
			this.conflictFiles.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.conflictFiles_AfterLabelEdit);
			this.conflictFiles.SelectedIndexChanged += new System.EventHandler(this.conflictFiles_SelectedIndexChanged);
			this.conflictFiles.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.conflictFiles_ItemCheck);
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Name";
			this.columnHeader4.Width = 78;
			// 
			// columnHeader6
			// 
			this.columnHeader6.Text = "Conflict Type";
			this.columnHeader6.Width = 77;
			// 
			// columnHeader5
			// 
			this.columnHeader5.Text = "Date Modified";
			this.columnHeader5.Width = 182;
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
			this.toolBar1.Size = new System.Drawing.Size(608, 42);
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
																					  this.menuView,
																					  this.menuHelp});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFileOpen,
																					 this.menuFileSeparator,
																					 this.menuFileExit});
			this.menuFile.Text = "&File";
			// 
			// menuFileOpen
			// 
			this.menuFileOpen.Index = 0;
			this.menuFileOpen.Text = "&Open";
			this.menuFileOpen.Click += new System.EventHandler(this.menuFileOpen_Click);
			// 
			// menuFileSeparator
			// 
			this.menuFileSeparator.Index = 1;
			this.menuFileSeparator.Text = "-";
			// 
			// menuFileExit
			// 
			this.menuFileExit.Index = 2;
			this.menuFileExit.Text = "E&xit";
			this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
			// 
			// menuView
			// 
			this.menuView.Index = 1;
			this.menuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuViewRefresh});
			this.menuView.Text = "&View";
			// 
			// menuViewRefresh
			// 
			this.menuViewRefresh.Index = 0;
			this.menuViewRefresh.Text = "&Refresh";
			this.menuViewRefresh.Click += new System.EventHandler(this.menuViewRefresh_Click);
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 2;
			this.menuHelp.Text = "&Help";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(352, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = "Local File:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Conflict Files:";
			// 
			// ConflictResolver
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(608, 539);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.toolBar1);
			this.Controls.Add(this.panel1);
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(552, 573);
			this.Name = "ConflictResolver";
			this.Text = "Conflict Resolver";
			this.Load += new System.EventHandler(this.ConflictResolver_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Gets the iFolder for a given node.
		/// </summary>
		/// <param name="node">The node to get the iFolder for.</param>
		/// <returns></returns>
		private iFolder GetiFolder(Node node)
		{
			return this.ifolder != null ? this.ifolder : manager.GetiFolderById(node.Properties.GetSingleProperty("CollectionId").ToString());
		}

		/// <summary>
		/// Opens the local file in the registered application.
		/// </summary>
		private void OpenLocalFile()
		{
			ListViewItem lvi = conflictFiles.SelectedItems[0];
			CollisionNode cn = (CollisionNode)lvi.Tag;
			iFolder ifolder = GetiFolder(cn.ConflictNode);
			string path = cn.LocalNode.GetFullPath(ifolder);

			try
			{
				Process.Start(path);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Opens the conflict file in the registered application.
		/// </summary>
		private void OpenConflictFile()
		{
			ListViewItem lvi = conflictFiles.SelectedItems[0];
			CollisionNode cn = (CollisionNode)lvi.Tag;

			try
			{
				Process.Start(cn.ConflictPath);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Populates the conflict files list view with collisions from the specified iFolder.
		/// </summary>
		/// <param name="ifolder">The iFolder to display conflicts from.</param>
		private void PutCollisionsInList(iFolder ifolder)
		{
			ICSList collisionList = ifolder.GetCollisions();
			foreach (ShallowNode sn in collisionList)
			{
				// Get the collision node.
				Node conflictNode = new Node(ifolder, sn);

				// Make sure it is a file node.
				if (ifolder.IsType(conflictNode, typeof(BaseFileNode).Name))
				{
					Node localNode = ifolder.GetNodeFromCollision(conflictNode);

					// fileNode will be null for name collisions.
					FileNode fileNode = null;
					if (localNode != null)
					{
						// Get the node that the collision occurred with.
						fileNode = new FileNode(localNode);
					}

					Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(ifolder, conflictNode);

					// Create the ListViewItem
					ListViewItem lvi = new ListViewItem(new string[] {
													  fileNode != null ? fileNode.GetFileName() : sn.Name,
													  conflict.IsFileNameConflict ? "Name" : "Update",
													  new FileNode(conflictNode).LastWriteTime.ToString()});

					// Create a CollisionNode to put on the ListViewItem.
					CollisionNode cNode = new CollisionNode();
					cNode.LocalNode = fileNode;
					cNode.ConflictNode =  conflictNode;
					cNode.ConflictPath = conflict.IsFileNameConflict ? conflict.FileNameConflictPath : conflict.UpdateConflictPath;
					lvi.Tag = cNode;

					// Set the state to unchecked.
					lvi.Checked = false;

					// Add the ListViewItem to the conflictFiles listview.
					conflictFiles.Items.Add(lvi);
				}
			}
		}

		/// <summary>
		/// Processes a ListViewItem.
		/// </summary>
		/// <param name="lviConflict">The conflict ListViewItem to process.</param>
		private void ProcessEntry(ListViewItem lviConflict)
		{
			try
			{
				// Get the CollisionNode from the ListViewItem.
				CollisionNode cn = (CollisionNode)lviConflict.Tag;

				// Get the iFolder for this node.
				iFolder ifolder = GetiFolder(cn.ConflictNode);

				// Instantiate a Conflict object for this node.
				Simias.Sync.Conflict conflict = new Simias.Sync.Conflict(ifolder, cn.ConflictNode);

				// If the item is checked then the conflict wins.
				if (lviConflict.Checked)
				{
					if (conflict.IsFileNameConflict)
					{
						// This is a filename conflict, resolve using the new name specified.
						conflict.Resolve(lviConflict.Text);
					}
					else
					{
						// This is an update conflict, commit the conflict file and throw away local changes.
						conflict.Resolve(false);
					}
				}
				else if (cn.Checked)
				{
					// This is an update conflict and the local changes win ... throw away the conflict file.
					conflict.Resolve(true);
				}

				if (!ifolder.HasCollisions())
				{
					// This iFolder no longer has conflicts, notify the shell so the overlay icon can be updated.
					Win32Window.ShChangeNotify(Win32Window.SHCNE_UPDATEITEM, Win32Window.SHCNF_PATHW, ifolder.LocalPath, IntPtr.Zero);
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

		/// <summary>
		/// Refreshes the conflictFiles listview.
		/// </summary>
		private void RefreshList()
		{
			Cursor.Current = Cursors.WaitCursor;

			// Clear the listviews.
			conflictFiles.Items.Clear();
			conflictFiles.SelectedItems.Clear();
			localFiles.Items.Clear();
			localFiles.SelectedItems.Clear();

			// Wait before redrawing the listview.
			conflictFiles.BeginUpdate();
			
			try
			{
				// Connect to the iFolderManager.
				manager = iFolderManager.Connect();

				if (this.ifolder != null)
				{
					// If an iFolder was passed in, then we will only display it's conflicts.
					PutCollisionsInList(this.ifolder);
				}
				else
				{
					// Display conflicts for all iFolders in the store.
					foreach (iFolder ifolder in manager)
					{
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

			// Redraw the listview.
			conflictFiles.EndUpdate();

			if (!conflictsFound && conflictFiles.Items.Count > 0)
				conflictsFound = true;

			if (!conflictsFound && conflictFiles.Items.Count == 0)
			{
				MessageBox.Show("There are no conflicts to resolve.");
			}

			if (conflictsFound && conflictFiles.Items.Count == 0)
			{
				MessageBox.Show("All conflicts have been resolved");
				conflictsFound = false;
			}

			Cursor.Current = Cursors.Default;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the iFolder to resolve conflicts for.
		/// </summary>
		public iFolder IFolder
		{
			set
			{
				ifolder = value;
			}
		}

		/// <summary>
		/// The path where the DLL is running from.
		/// </summary>
		public string LoadPath
		{
			get
			{
				return loadPath;
			}

			set
			{
				loadPath = value;
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used when conflicts have been resolved.
		/// </summary>
		public delegate void ConflictsResolvedDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when all conflicts have been resolved.
		/// </summary>
		public event ConflictsResolvedDelegate ConflictsResolved;
		#endregion

		#region Event Handlers
		private void conflictFiles_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (conflictFiles.SelectedItems.Count == 1)
			{
				// Get the selected ListViewItem.
				ListViewItem conflictLVI = conflictFiles.SelectedItems[0];

				// Are we dealing with name collisions?
				// TODO: we may want to use the Conflict object for this rather than doing a string compare
				bool nameCollision = conflictLVI.SubItems[1].Text.Equals("Name");
				conflictFiles.LabelEdit = nameCollision;

				if (!nameCollision)
				{
					// Get the CollisionNode.
					CollisionNode cn = (CollisionNode)conflictLVI.Tag;
	
					// Get the iFolder for this node.
					iFolder ifolder = GetiFolder(cn.ConflictNode);

					// Create the ListViewItem.
					ListViewItem localLVI = new ListViewItem(new string[] {
													  cn.LocalNode.Name, 
													  Path.GetDirectoryName(cn.LocalNode.GetFullPath(ifolder)), 
													  cn.LocalNode.LastWriteTime.ToString()});

					// Set the state based on the state of the CollisionNode.
					localLVI.Checked = cn.Checked;

					// Add the ListViewItem to the localFiles listview.
					localFiles.Items.Add(localLVI);
				}
				else
				{
					// This is a name collision, so there is nothing to display in the localFiles listview.
					localFiles.Items.Clear();
				}
			}
			else
			{
				// If multiple items are selected, disable label edit and clear the local files listview.
				conflictFiles.LabelEdit = false;
				localFiles.Items.Clear();
			}
		}

		private void conflictFiles_AfterLabelEdit(object sender, System.Windows.Forms.LabelEditEventArgs e)
		{
			// We only care if changes were actually made.
			if (e.Label != null)
			{
				// Get the ListViewItem.
				ListViewItem lvi = conflictFiles.Items[e.Item];

				// Get the CollisionNode.
				CollisionNode cn = (CollisionNode)lvi.Tag;

				// Get the iFolder.
				iFolder ifolder = this.GetiFolder(cn.ConflictNode);

				// Validate the name just entered.
				// 1. Make sure there are no illegal characters.
				// 2. Make sure there is not a name collision.
				string fileName = Path.Combine(ifolder.LocalPath, e.Label);
				try
				{
					// Test the name entered.
					FileInfo fi = new FileInfo(fileName);
					if (fi.Exists)
					{
						// Name collision.
						MessageBox.Show("The name conflicts with an existing file name.  Please specify a different name.");
						e.CancelEdit = true;
					}
				}
				catch (Exception ex)
				{
					// The name most likely contains illegal characters.
					MessageBox.Show(ex.Message);
					e.CancelEdit = true;
				}

				if (e.CancelEdit)
				{
					// The name was not valid ... let the user try again.
					cn.NameValidated = false;
					lvi.BeginEdit();
				}
				else
				{
					// The name was validated ... set the state to checked.
					cn.NameValidated = true;
					lvi.Checked = true;
				}
			}
		}

		private void localFiles_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			// Make sure that the user caused this event.
			Point point = localFiles.PointToClient(Control.MousePosition);
			ListViewItem tlvi = localFiles.GetItemAt(point.X, point.Y);
			if (tlvi == localFiles.Items[e.Index])
			{
				// Get the ListViewItem for the corresponding conflict.
				ListViewItem lvi = conflictFiles.SelectedItems[0];
				CollisionNode cn = (CollisionNode)lvi.Tag;

				switch (e.NewValue)
				{
					case CheckState.Checked:
						// Uncheck the conflict
						lvi.Checked = false;

						// Save the state in the CollisionNode.
						cn.Checked = true;
						break;
					case CheckState.Unchecked:
						// Save the state in the CollisionNode.
						cn.Checked = false;
						break;
					default:
						break;
				}
			}
		}

		private void conflictFiles_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			// Make sure the user set the state.
			Point point = conflictFiles.PointToClient(Control.MousePosition);
			ListViewItem tlvi = conflictFiles.GetItemAt(point.X, point.Y);
			if (tlvi == conflictFiles.Items[e.Index])
			{
				CollisionNode cn = (CollisionNode)tlvi.Tag;

				// TODO: may want to use Conflict object rather than name compare.
				if (tlvi.SubItems[1].Text.Equals("Name"))
				{
					// If this is a name conflict then allow the name to be edited.
					conflictFiles.LabelEdit = true;

					switch (e.NewValue)
					{
						case CheckState.Checked:
							// Make sure the name is validated.
							if (!cn.NameValidated)
							{
								MessageBox.Show("The name must be changed before the file can be selected for resolving.");

								// Uncheck the item.
								e.NewValue = CheckState.Unchecked;

								// Set the label to edit mode.
								tlvi.BeginEdit();
							}
							break;
						default:
							break;
					}
				}
				else
				{
					// This is an update conflict ...

					// Don't allow the name to be edited.
					conflictFiles.LabelEdit = false;

					// See if there is a corresponding local files item.
					ListViewItem lvi = null;
					if (conflictFiles.SelectedItems.Count == 1 &&
						conflictFiles.SelectedItems[0] == conflictFiles.Items[e.Index])
					{
						lvi = localFiles.Items[0];
					}

					switch (e.NewValue)
					{
						case CheckState.Checked:
							if (lvi != null)
							{
								// Uncheck the localFiles ListViewItem.
								lvi.Checked = false;
							}

							// Save state in the CollisionNode.
							cn.Checked = false;
							break;
						default:
							break;
					}
				}
			}
		}

		private void menuOpen_Click(object sender, System.EventArgs e)
		{
			// Open the file that has focus.
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
			// Enable the context menu if only one item is selected.
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
			label1.Left = e.X + labelDelta;
		}

		private void ConflictResolver_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				if (loadPath != null)
				{
					this.Icon = new Icon(Path.Combine(loadPath, applicationIcon));
				}
				else
				{
					this.Icon = new Icon(Path.Combine(Application.StartupPath, applicationIcon));
				}
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading icon");
			}

			RefreshList();
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch (e.Button.Text)
			{
				case "Open":
					menuOpen_Click(this, null);
					break;
				case "Resolve":
					Cursor.Current = Cursors.WaitCursor;

					// Process the list.
					foreach (ListViewItem lvi in conflictFiles.Items)
					{
						ProcessEntry(lvi);
					}

					Cursor.Current = Cursors.Default;

					// Update the listview.
					RefreshList();

					if (conflictFiles.Items.Count == 0 && ConflictsResolved != null)
					{
						// If all the conflicts have been resolved, fire the ConflictsResolved event.
						ConflictsResolved(this, new EventArgs());
					}
					break;
				case "Refresh":
					RefreshList();
					break;
				default:
					break;
			}
		}

		private void menuFileOpen_Click(object sender, System.EventArgs e)
		{
			menuOpen_Click(sender, e);
		}

		private void menuFileExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void menuViewRefresh_Click(object sender, System.EventArgs e)
		{
			RefreshList();
		}
		#endregion
	}
}
