using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.Data;

using iFolder.Client;

namespace iFolderPPC
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class JoinForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label HostLabel;
		private System.Windows.Forms.TextBox HostEdit;
		private System.Windows.Forms.Label UserLabel;
		private System.Windows.Forms.TextBox UserEdit;
		private System.Windows.Forms.Label PwdLabel;
		private System.Windows.Forms.TextBox PwdEdit;
		private System.Windows.Forms.Button JoinButton;
		private System.Windows.Forms.Button Cancel;
	
		public JoinForm()
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
			base.Dispose( disposing );
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.HostLabel = new System.Windows.Forms.Label();
			this.HostEdit = new System.Windows.Forms.TextBox();
			this.UserLabel = new System.Windows.Forms.Label();
			this.UserEdit = new System.Windows.Forms.TextBox();
			this.PwdLabel = new System.Windows.Forms.Label();
			this.PwdEdit = new System.Windows.Forms.TextBox();
			this.JoinButton = new System.Windows.Forms.Button();
			this.Cancel = new System.Windows.Forms.Button();
			// 
			// HostLabel
			// 
			this.HostLabel.Location = new System.Drawing.Point(16, 24);
			this.HostLabel.Size = new System.Drawing.Size(56, 20);
			this.HostLabel.Text = "Host:";
			this.HostLabel.ParentChanged += new System.EventHandler(this.label1_ParentChanged);
			// 
			// HostEdit
			// 
			this.HostEdit.Location = new System.Drawing.Point(88, 16);
			this.HostEdit.Size = new System.Drawing.Size(176, 22);
			this.HostEdit.Text = "";
			// 
			// UserLabel
			// 
			this.UserLabel.Location = new System.Drawing.Point(16, 56);
			this.UserLabel.Size = new System.Drawing.Size(64, 20);
			this.UserLabel.Text = "Username:";
			// 
			// UserEdit
			// 
			this.UserEdit.Location = new System.Drawing.Point(88, 48);
			this.UserEdit.Size = new System.Drawing.Size(176, 22);
			this.UserEdit.Text = "";
			// 
			// PwdLabel
			// 
			this.PwdLabel.Location = new System.Drawing.Point(16, 88);
			this.PwdLabel.Size = new System.Drawing.Size(64, 20);
			this.PwdLabel.Text = "Password:";
			// 
			// PwdEdit
			// 
			this.PwdEdit.Location = new System.Drawing.Point(88, 80);
			this.PwdEdit.PasswordChar = '*';
			this.PwdEdit.Size = new System.Drawing.Size(176, 22);
			this.PwdEdit.Text = "";
			// 
			// JoinButton
			// 
			this.JoinButton.Location = new System.Drawing.Point(16, 136);
			this.JoinButton.Size = new System.Drawing.Size(72, 24);
			this.JoinButton.Text = "Join";
			this.JoinButton.Click += new System.EventHandler(this.JoinButton_Click);
			// 
			// Cancel
			// 
			this.Cancel.Location = new System.Drawing.Point(96, 136);
			this.Cancel.Size = new System.Drawing.Size(72, 24);
			this.Cancel.Text = "Cancel";
			this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
			// 
			// JoinForm
			// 
			this.ClientSize = new System.Drawing.Size(282, 184);
			this.Controls.Add(this.Cancel);
			this.Controls.Add(this.JoinButton);
			this.Controls.Add(this.PwdEdit);
			this.Controls.Add(this.PwdLabel);
			this.Controls.Add(this.UserEdit);
			this.Controls.Add(this.UserLabel);
			this.Controls.Add(this.HostEdit);
			this.Controls.Add(this.HostLabel);
			this.Text = "Join iFolder Domain";

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>

		static void Main() 
		{
			Application.Run(new JoinForm());
		}

		private void label1_ParentChanged(object sender, System.EventArgs e)
		{
		
		}

		private void JoinButton_Click(object sender, System.EventArgs e)
		{
			iFolder.Client.Host host = new iFolder.Client.Host( "192.168.1.99" );
			Domain ifolderDomain = iFolder.Client.Domain.Add( host, "banderso", "baci2002", true, true );

		
		}

		private void Cancel_Click(object sender, System.EventArgs e)
		{
		
		}
	}
}
