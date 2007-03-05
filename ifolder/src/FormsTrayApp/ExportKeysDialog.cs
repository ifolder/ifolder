using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TrayApp
{
	/// <summary>
	/// Summary description for ExportKeysDialog.
	/// </summary>
	public class ExportKeysDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label domainLabel;
		private System.Windows.Forms.Label filePathLabel;
		private System.Windows.Forms.Label recoveryAgentLabel;
		private System.Windows.Forms.Label emailLabel;
		private System.Windows.Forms.ComboBox domainComboBox;
		private System.Windows.Forms.TextBox filePath;
		private System.Windows.Forms.TextBox recoveryAgent;
		private System.Windows.Forms.TextBox emailID;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnExport;
		private System.Windows.Forms.Button BrowseButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ExportKeysDialog()
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
			this.domainLabel = new System.Windows.Forms.Label();
			this.filePathLabel = new System.Windows.Forms.Label();
			this.recoveryAgentLabel = new System.Windows.Forms.Label();
			this.emailLabel = new System.Windows.Forms.Label();
			this.domainComboBox = new System.Windows.Forms.ComboBox();
			this.filePath = new System.Windows.Forms.TextBox();
			this.recoveryAgent = new System.Windows.Forms.TextBox();
			this.emailID = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnExport = new System.Windows.Forms.Button();
			this.BrowseButton = new System.Windows.Forms.Button();
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
			// domainLabel
			// 
			this.domainLabel.Location = new System.Drawing.Point(16, 76);
			this.domainLabel.Name = "domainLabel";
			this.domainLabel.Size = new System.Drawing.Size(120, 20);
			this.domainLabel.TabIndex = 0;
			this.domainLabel.Text = "iFolder Account:";
			this.domainLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// filePathLabel
			// 
			this.filePathLabel.Location = new System.Drawing.Point(16, 104);
			this.filePathLabel.Name = "filePathLabel";
			this.filePathLabel.Size = new System.Drawing.Size(120, 20);
			this.filePathLabel.TabIndex = 1;
			this.filePathLabel.Text = "FilePath:";
			this.filePathLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// recoveryAgentLabel
			// 
			this.recoveryAgentLabel.Location = new System.Drawing.Point(16, 132);
			this.recoveryAgentLabel.Name = "recoveryAgentLabel";
			this.recoveryAgentLabel.Size = new System.Drawing.Size(120, 20);
			this.recoveryAgentLabel.TabIndex = 2;
			this.recoveryAgentLabel.Text = "Recovery Agent:";
			this.recoveryAgentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// emailLabel
			// 
			this.emailLabel.Location = new System.Drawing.Point(16, 160);
			this.emailLabel.Name = "emailLabel";
			this.emailLabel.Size = new System.Drawing.Size(120, 20);
			this.emailLabel.TabIndex = 3;
			this.emailLabel.Text = "E-Mail ID:";
			this.emailLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// domainComboBox
			// 
			this.domainComboBox.Location = new System.Drawing.Point(156, 76);
			this.domainComboBox.Name = "domainComboBox";
			this.domainComboBox.Size = new System.Drawing.Size(240, 21);
			this.domainComboBox.TabIndex = 4;
			// 
			// filePath
			// 
			this.filePath.Location = new System.Drawing.Point(156, 104);
			this.filePath.Name = "filePath";
			this.filePath.Size = new System.Drawing.Size(172, 20);
			this.filePath.TabIndex = 5;
			this.filePath.Text = "";
			this.filePath.TextChanged += new System.EventHandler(this.filePath_TextChanged);
			// 
			// recoveryAgent
			// 
			this.recoveryAgent.Location = new System.Drawing.Point(156, 132);
			this.recoveryAgent.Name = "recoveryAgent";
			this.recoveryAgent.Size = new System.Drawing.Size(240, 20);
			this.recoveryAgent.TabIndex = 6;
			this.recoveryAgent.Text = "";
			// 
			// emailID
			// 
			this.emailID.Location = new System.Drawing.Point(156, 160);
			this.emailID.Name = "emailID";
			this.emailID.Size = new System.Drawing.Size(240, 20);
			this.emailID.TabIndex = 7;
			this.emailID.Text = "";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(249, 200);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 8;
			this.btnCancel.Text = "&Cancel";
			// 
			// btnExport
			// 
			this.btnExport.Location = new System.Drawing.Point(329, 200);
			this.btnExport.Name = "btnExport";
			this.btnExport.TabIndex = 9;
			this.btnExport.Text = "&Export";
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = new System.Drawing.Point(336, 104);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = new System.Drawing.Size(60, 20);
			this.BrowseButton.TabIndex = 10;
			this.BrowseButton.Text = "&Browse";
			// 
			// ExportKeysDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(420, 242);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.btnExport);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.emailID);
			this.Controls.Add(this.recoveryAgent);
			this.Controls.Add(this.filePath);
			this.Controls.Add(this.domainComboBox);
			this.Controls.Add(this.emailLabel);
			this.Controls.Add(this.recoveryAgentLabel);
			this.Controls.Add(this.filePathLabel);
			this.Controls.Add(this.domainLabel);
			this.Controls.Add(this.panel1);
			this.Name = "ExportKeysDialog";
			this.Text = "ExportKeysDialog";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void filePath_TextChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
