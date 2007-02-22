using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
using Novell.Wizard;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for MigrationWindow.
	/// </summary>
	public class MigrationWindow : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnMigrate;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private bool windowActive;

		private iFolderWebService ifWebService; 
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MigrationWindow(iFolderWebService ifWebService)
		{
			windowActive = false;
			this.ifWebService = ifWebService;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnMigrate = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader2});
			this.listView1.FullRowSelect = true;
			this.listView1.Location = new System.Drawing.Point(12, 12);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(400, 339);
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.SelectedIndexChanged +=new EventHandler(listView1_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "User Name";
			this.columnHeader1.Width = 155;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Home Location";
			this.columnHeader2.Width = 241;
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(330, 367);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnMigrate
			// 
			this.btnMigrate.Location = new System.Drawing.Point(250, 367);
			this.btnMigrate.Name = "btnMigrate";
			this.btnMigrate.TabIndex = 2;
			this.btnMigrate.Text = "Migrate";
			this.btnMigrate.Click += new System.EventHandler(this.btnMigrate_Click);
			//this.btnMigrate.Enabled = false;
			// 
			// MigrationWindow
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(424, 398);
			this.Controls.Add(this.btnMigrate);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.listView1);
			this.Name = "MigrationWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Migration";
			this.Load += new System.EventHandler(this.MigrationWindow_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void MigrationWindow_Load(object sender, System.EventArgs e)
		{
			AddMigrationDetails();
		}
		public void AddMigrationDetails()
		{
			string iFolderRegistryKey = @"Software\Novell iFolder";
			RegistryKey iFolderKey = Registry.LocalMachine.OpenSubKey(iFolderRegistryKey);
			if(iFolderKey == null)
			{
				MessageBox.Show("iFolder2.x is not installed on this system", "iFolder2.x not present", MessageBoxButtons.OK);
				this.Dispose();
				return;
			}
			string[] AllKeys = new string[iFolderKey.SubKeyCount];
			string User;
			AllKeys = iFolderKey.GetSubKeyNames();
			this.listView1.Items.Clear();
		
			for(int i=0; i< AllKeys.Length; i++)
			{
				ListViewItem lvi;
				User = iFolderRegistryKey + "\\" + AllKeys[i];
				RegistryKey UserKey = Registry.LocalMachine.OpenSubKey(User);
				if (UserKey == null) 
					return;
				if( UserKey.GetValue("FolderPath") != null)
				{
					lvi = new ListViewItem( new string[]{AllKeys[i], (string)UserKey.GetValue("FolderPath")});
					listView1.Items.Add(lvi);
					lvi.Selected = true;
				}
				UserKey.Close();
				/*
				else
				{
					lvi = new ListViewItem( new string[]{AllKeys[i], "Not a user"});
					this.listView1.Items.Add(lvi);
				}
				*/
			}
			iFolderKey.Close();
			if( this.listView1.SelectedItems.Count <= 0)
			{
				if( !this.windowActive )
					MessageBox.Show("No iFolder2.x folders present for migration.", "Migration", MessageBoxButtons.OK);
				this.Dispose(true);
			}
			this.windowActive = true;
			//MessageBox mmb1 = new MyMessageBox("In AddMigrationDetails", "Migration details added", "nothing", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
			//mmb1.ShowDialog();
		}

		private void btnMigrate_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = this.listView1.SelectedItems[0];
			MigrationWizard migrationWizard = new MigrationWizard( lvi.SubItems[0].Text, lvi.SubItems[1].Text, ifWebService);
			//	accountWizard.EnterpriseConnect += new Novell.Wizard.AccountWizard.EnterpriseConnectDelegate(accountWizard_EnterpriseConnect);
			if ( migrationWizard.ShowDialog() == DialogResult.OK )
			{
				/*
				// Display the iFolders dialog.
				if ( DisplayiFolderDialog != null )
				{
					DisplayiFolderDialog( this, new EventArgs() );
				}
				*/
			}
			migrationWizard.Dispose();
			AddMigrationDetails();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Dispose(true);
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if ((this.listView1.SelectedItems.Count == 1) &&
					(this.listView1.Items.Count > 0))
				{
					ListViewItem lvi = this.listView1.SelectedItems[0];
					if (lvi != null)
					{
						this.btnMigrate.Enabled = true;
					}  
				}
				else
				{
					this.btnMigrate.Enabled = false;
				}
		
		}
	}
}
