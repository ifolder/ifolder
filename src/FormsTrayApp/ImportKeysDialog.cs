using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TrayApp
{
	/// <summary>
	/// Summary description for ImportKeysDialog.
	/// </summary>
	public class ImportKeysDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label filePathLabel;
		private System.Windows.Forms.Label oneTimePassphraseLabel;
		private System.Windows.Forms.Label passPhraseLabel;
		private System.Windows.Forms.Label retypePassphraseLabel;
		private System.Windows.Forms.TextBox oneTimePassphrase;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.TextBox retypePassphrase;
		private System.Windows.Forms.TextBox LocationEntry;
		private System.Windows.Forms.Button BrowseButton;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnImport;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ImportKeysDialog()
		{
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.filePathLabel = new System.Windows.Forms.Label();
			this.oneTimePassphraseLabel = new System.Windows.Forms.Label();
			this.passPhraseLabel = new System.Windows.Forms.Label();
			this.retypePassphraseLabel = new System.Windows.Forms.Label();
			this.oneTimePassphrase = new System.Windows.Forms.TextBox();
			this.Passphrase = new System.Windows.Forms.TextBox();
			this.retypePassphrase = new System.Windows.Forms.TextBox();
			this.LocationEntry = new System.Windows.Forms.TextBox();
			this.BrowseButton = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnImport = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Blue;
			this.panel1.Controls.Add(this.waterMark);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(428, 56);
			this.panel1.TabIndex = 1;
			// 
			// waterMark
			// 
			this.waterMark.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(163)), ((System.Byte)(237)));
			this.waterMark.Location = new System.Drawing.Point(16, 8);
			this.waterMark.Name = "waterMark";
			this.waterMark.Size = new System.Drawing.Size(48, 48);
			this.waterMark.TabIndex = 0;
			this.waterMark.TabStop = false;
			// 
			// filePathLabel
			// 
			this.filePathLabel.Location = new System.Drawing.Point(16, 76);
			this.filePathLabel.Name = "filePathLabel";
			this.filePathLabel.Size = new System.Drawing.Size(120, 20);
			this.filePathLabel.TabIndex = 0;
			this.filePathLabel.Text = "File Path:";
			this.filePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// oneTimePassphraseLabel
			// 
			this.oneTimePassphraseLabel.Location = new System.Drawing.Point(16, 104);
			this.oneTimePassphraseLabel.Name = "oneTimePassphraseLabel";
			this.oneTimePassphraseLabel.Size = new System.Drawing.Size(120, 20);
			this.oneTimePassphraseLabel.TabIndex = 1;
			this.oneTimePassphraseLabel.Text = "One Time Passphrase:";
			this.oneTimePassphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// passPhraseLabel
			// 
			this.passPhraseLabel.Location = new System.Drawing.Point(16, 132);
			this.passPhraseLabel.Name = "passPhraseLabel";
			this.passPhraseLabel.Size = new System.Drawing.Size(120, 20);
			this.passPhraseLabel.TabIndex = 2;
			this.passPhraseLabel.Text = "New Passphrase:";
			this.passPhraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// retypePassphraseLabel
			// 
			this.retypePassphraseLabel.Location = new System.Drawing.Point(16, 160);
			this.retypePassphraseLabel.Name = "retypePassphraseLabel";
			this.retypePassphraseLabel.Size = new System.Drawing.Size(120, 20);
			this.retypePassphraseLabel.TabIndex = 3;
			this.retypePassphraseLabel.Text = "Retype Passphrase";
			this.retypePassphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// oneTimePassphrase
			// 
			this.oneTimePassphrase.Location = new System.Drawing.Point(156, 104);
			this.oneTimePassphrase.Name = "oneTimePassphrase";
			this.oneTimePassphrase.Size = new System.Drawing.Size(240, 20);
			this.oneTimePassphrase.TabIndex = 4;
			this.oneTimePassphrase.Text = "";
			// 
			// Passphrase
			// 
			this.Passphrase.Location = new System.Drawing.Point(156, 132);
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.Size = new System.Drawing.Size(240, 20);
			this.Passphrase.TabIndex = 5;
			this.Passphrase.Text = "";
			// 
			// retypePassphrase
			// 
			this.retypePassphrase.Location = new System.Drawing.Point(156, 160);
			this.retypePassphrase.Name = "retypePassphrase";
			this.retypePassphrase.Size = new System.Drawing.Size(240, 20);
			this.retypePassphrase.TabIndex = 6;
			this.retypePassphrase.Text = "";
			// 
			// LocationEntry
			// 
			this.LocationEntry.Location = new System.Drawing.Point(156, 76);
			this.LocationEntry.Name = "LocationEntry";
			this.LocationEntry.Size = new System.Drawing.Size(172, 20);
			this.LocationEntry.TabIndex = 7;
			this.LocationEntry.Text = "";
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = new System.Drawing.Point(336, 76);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = new System.Drawing.Size(60, 20);
			this.BrowseButton.TabIndex = 8;
			this.BrowseButton.Text = "&Browse";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(240, 200);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "&Cancel";
			// 
			// btnImport
			// 
			this.btnImport.Enabled = false;
			this.btnImport.Location = new System.Drawing.Point(320, 200);
			this.btnImport.Name = "btnImport";
			this.btnImport.TabIndex = 10;
			this.btnImport.Text = "&Import";
			// 
			// ImportKeysDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(420, 242);
			this.Controls.Add(this.btnImport);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.LocationEntry);
			this.Controls.Add(this.retypePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.oneTimePassphrase);
			this.Controls.Add(this.retypePassphraseLabel);
			this.Controls.Add(this.passPhraseLabel);
			this.Controls.Add(this.oneTimePassphraseLabel);
			this.Controls.Add(this.filePathLabel);
			this.Controls.Add(this.panel1);
			this.Name = "ImportKeysDialog";
			this.Text = "Import Keys";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
