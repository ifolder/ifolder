using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Text;

using Simias.Client;
using Simias.Storage;


namespace EventLogReaderII
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.MenuItem EditMenu;
		private System.Windows.Forms.MenuItem SelectAllMenuItem;
		private System.Windows.Forms.MenuItem CopyMenuItem;
		private System.Windows.Forms.MenuItem FileMenu;
		private System.Windows.Forms.MenuItem OpenMenuItem;
		private System.Windows.Forms.MenuItem CloseMenuItem;
		private System.Windows.Forms.MenuItem ExitMenuItem;
		private System.Windows.Forms.ContextMenu CopyContextMenu;
		private System.Windows.Forms.MenuItem CopyContextMenuItem;
		private System.Windows.Forms.MenuItem SelectAllContextMenuItem;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private void DisplayHeader( FileStream fs )
		{
			// Position the file pointer to the beginning of the file.
			fs.Position = 0;

			// Read the data.
			byte[] buffer = new byte[ LogFileHeader.RecordSize ];
			int bytesRead = fs.Read( buffer, 0, buffer.Length );
			if ( bytesRead == buffer.Length )
			{
				LogFileHeader header = new LogFileHeader( buffer );

				listBox1.Items.Add( String.Format( "*===== Log File Header for File {0} =====*", fs.Name ) );
				listBox1.Items.Add( String.Format( "\tHeader length = {0}", header.Length ) );
				listBox1.Items.Add( String.Format( "\tLog File ID = {0}", header.LogFileID ) );
				listBox1.Items.Add( String.Format( "\tMaximum records = {0}", header.MaxLogRecords ) );
				listBox1.Items.Add( String.Format( "\tHint next record ID = {0}", header.LastRecord ) );
				listBox1.Items.Add( String.Format( "\tHint next write location = {0}", header.RecordLocation ) );
				listBox1.Items.Add( String.Format( "\tChange log record size = {0}", ChangeLogRecord.RecordSize ) );
				listBox1.Items.Add( "*===== End Log File Header =====*" );
				listBox1.Items.Add( "" );
			}
		}

		private void DisplayLog( FileStream fs )
		{
			listBox1.BeginUpdate();

			// Display the LogFileHeader record.
			DisplayHeader( fs );

			listBox1.Items.Add( "*===== Change Log Records =====*" );

			// Allocate a buffer to hold the records that are read.
			byte[] buffer = new byte[ ChangeLogRecord.RecordSize * 1000 ];

			// Skip over the file header.
			fs.Position = LogFileHeader.RecordSize;

			// Read a bunch of records.
			int bytesRead = fs.Read( buffer, 0, buffer.Length );
			while ( bytesRead > 0 )
			{
				int index = 0;
				while ( ( index + ChangeLogRecord.RecordSize ) <= bytesRead )
				{
					// Instantiate the next record so the id's can be compared.
					ChangeLogRecord record = new ChangeLogRecord( buffer, index );

					StringBuilder recStr = new StringBuilder( 
						String.Format( "\tRecord {0} - {1}: Operation = {2}, Node ID = {3}, Node Type = {4}, Flags = {5}, MasterRev = {6}, SlaveRev = {7}, FileLength = {8}", 
							record.RecordID, 
							record.Epoch.ToString(), 
							Enum.GetName( typeof( ChangeLogRecord.ChangeLogOp ), record.Operation ), 
							record.EventID,
							Enum.GetName( typeof( NodeTypes.NodeTypeEnum ), record.Type ),
							record.Flags,
							record.MasterRev,
							record.SlaveRev,
							record.FileLength ) );

					if ( record.Flags != 0 )
					{
						recStr.Append( " [" );

						if ( ( record.Flags & 1 ) == 1 )
						{
							recStr.Append( "Local" );
						}

						recStr.Append( "]" );
					}

					listBox1.Items.Add( recStr.ToString() );  
					index += ChangeLogRecord.RecordSize;
				}

				bytesRead = fs.Read( buffer, 0, buffer.Length );
			}

			listBox1.Items.Add( "*===== End of Change Log Records =====*" );
			listBox1.EndUpdate();
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
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.FileMenu = new System.Windows.Forms.MenuItem();
			this.OpenMenuItem = new System.Windows.Forms.MenuItem();
			this.CloseMenuItem = new System.Windows.Forms.MenuItem();
			this.ExitMenuItem = new System.Windows.Forms.MenuItem();
			this.EditMenu = new System.Windows.Forms.MenuItem();
			this.CopyMenuItem = new System.Windows.Forms.MenuItem();
			this.SelectAllMenuItem = new System.Windows.Forms.MenuItem();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.CopyContextMenu = new System.Windows.Forms.ContextMenu();
			this.CopyContextMenuItem = new System.Windows.Forms.MenuItem();
			this.SelectAllContextMenuItem = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.FileMenu,
																					  this.EditMenu});
			// 
			// FileMenu
			// 
			this.FileMenu.Index = 0;
			this.FileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.OpenMenuItem,
																					 this.CloseMenuItem,
																					 this.ExitMenuItem});
			this.FileMenu.Text = "&File";
			// 
			// OpenMenuItem
			// 
			this.OpenMenuItem.Index = 0;
			this.OpenMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.OpenMenuItem.Text = "&Open";
			this.OpenMenuItem.Click += new System.EventHandler(this.OpenMenuItem_Click);
			// 
			// CloseMenuItem
			// 
			this.CloseMenuItem.Index = 1;
			this.CloseMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.CloseMenuItem.Text = "&Close";
			this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
			// 
			// ExitMenuItem
			// 
			this.ExitMenuItem.Index = 2;
			this.ExitMenuItem.Shortcut = System.Windows.Forms.Shortcut.AltF4;
			this.ExitMenuItem.Text = "E&xit";
			this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
			// 
			// EditMenu
			// 
			this.EditMenu.Index = 1;
			this.EditMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.CopyMenuItem,
																					 this.SelectAllMenuItem});
			this.EditMenu.Text = "&Edit";
			this.EditMenu.Popup += new System.EventHandler(this.EditMenuItem_Popup);
			// 
			// CopyMenuItem
			// 
			this.CopyMenuItem.Enabled = false;
			this.CopyMenuItem.Index = 0;
			this.CopyMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.CopyMenuItem.Text = "&Copy";
			this.CopyMenuItem.Click += new System.EventHandler(this.CopyMenuItem_Click);
			// 
			// SelectAllMenuItem
			// 
			this.SelectAllMenuItem.Index = 1;
			this.SelectAllMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
			this.SelectAllMenuItem.Text = "Select &All";
			this.SelectAllMenuItem.Click += new System.EventHandler(this.SelectAllMenuItem_Click);
			// 
			// listBox1
			// 
			this.listBox1.ContextMenu = this.CopyContextMenu;
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.Location = new System.Drawing.Point(0, 0);
			this.listBox1.Name = "listBox1";
			this.listBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listBox1.Size = new System.Drawing.Size(664, 277);
			this.listBox1.TabIndex = 0;
			// 
			// CopyContextMenu
			// 
			this.CopyContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.CopyContextMenuItem,
																							this.SelectAllContextMenuItem});
			this.CopyContextMenu.Popup += new System.EventHandler(this.EditMenuItem_Popup);
			// 
			// CopyContextMenuItem
			// 
			this.CopyContextMenuItem.Index = 0;
			this.CopyContextMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
			this.CopyContextMenuItem.Text = "Copy";
			this.CopyContextMenuItem.Click += new System.EventHandler(this.CopyMenuItem_Click);
			// 
			// SelectAllContextMenuItem
			// 
			this.SelectAllContextMenuItem.Index = 1;
			this.SelectAllContextMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
			this.SelectAllContextMenuItem.Text = "Select All";
			this.SelectAllContextMenuItem.Click += new System.EventHandler(this.SelectAllMenuItem_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(664, 286);
			this.Controls.Add(this.listBox1);
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.Text = "EventLogReader";
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

		private void OpenMenuItem_Click(object sender, System.EventArgs e)
		{
			listBox1.Items.Clear();

			OpenFileDialog ofd = new OpenFileDialog();
			ofd.CheckFileExists = true;
			ofd.Filter = "Event Log files (*.changelog)|*.changelog|All files (*.*)|*.*";
			ofd.InitialDirectory = Directory.GetCurrentDirectory();
			DialogResult result = ofd.ShowDialog();

			if ( result == DialogResult.OK )
			{
				FileStream fs = new FileStream( ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.Read );
				try
				{
					DisplayLog( fs );
				}
				finally
				{
					fs.Close();
				}
			}
		}

		private void CloseMenuItem_Click(object sender, System.EventArgs e)
		{
			listBox1.Items.Clear();
		}

		private void ExitMenuItem_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		private void SelectAllMenuItem_Click(object sender, System.EventArgs e)
		{
			for ( int i = 0; i < listBox1.Items.Count; ++i  )
			{
				listBox1.SetSelected( i, true );
			}
		}

		private void EditMenuItem_Popup(object sender, System.EventArgs e)
		{
			CopyMenuItem.Enabled = ( listBox1.SelectedItems.Count > 0 );
		}

		private void CopyMenuItem_Click(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();

			foreach( String s in listBox1.SelectedItems )
			{
				sb.Append( s + "\r\n" );
			}

			Clipboard.SetDataObject( sb.ToString(), true );
		}
	}
}
