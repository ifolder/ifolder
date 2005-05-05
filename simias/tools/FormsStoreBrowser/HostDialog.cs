using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for HostDialog.
	/// </summary>
	public class HostDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.TextBox host;
		private System.Windows.Forms.Label usernameLbl;
		private System.Windows.Forms.Label passwordLbl;
		private System.Windows.Forms.TextBox usernameEdit;
		private System.Windows.Forms.TextBox passwordEdit;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public string HostName
		{
			get { return host.Text; }
			set { host.Text = value; }
		}

		public string UserName
		{
			get { return usernameEdit.Text; }
		}

		public string Password
		{
			get { return passwordEdit.Text; }
		}

		public HostDialog( string hostUri )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			host.Text = hostUri;
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
			this.host = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.usernameLbl = new System.Windows.Forms.Label();
			this.passwordLbl = new System.Windows.Forms.Label();
			this.usernameEdit = new System.Windows.Forms.TextBox();
			this.passwordEdit = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// host
			// 
			this.host.Location = new System.Drawing.Point(104, 16);
			this.host.Name = "host";
			this.host.Size = new System.Drawing.Size(208, 20);
			this.host.TabIndex = 0;
			this.host.Text = "http://localhost:8086/simias10/mlasky";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(24, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Host Uri:";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(152, 128);
			this.ok.Name = "ok";
			this.ok.TabIndex = 6;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(232, 128);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 7;
			this.cancel.Text = "Cancel";
			// 
			// usernameLbl
			// 
			this.usernameLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.usernameLbl.Location = new System.Drawing.Point(24, 48);
			this.usernameLbl.Name = "usernameLbl";
			this.usernameLbl.Size = new System.Drawing.Size(72, 16);
			this.usernameLbl.TabIndex = 2;
			this.usernameLbl.Text = "Username:";
			// 
			// passwordLbl
			// 
			this.passwordLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.passwordLbl.Location = new System.Drawing.Point(24, 80);
			this.passwordLbl.Name = "passwordLbl";
			this.passwordLbl.Size = new System.Drawing.Size(72, 16);
			this.passwordLbl.TabIndex = 3;
			this.passwordLbl.Text = "Password:";
			// 
			// usernameEdit
			// 
			this.usernameEdit.Location = new System.Drawing.Point(104, 48);
			this.usernameEdit.Name = "usernameEdit";
			this.usernameEdit.Size = new System.Drawing.Size(208, 20);
			this.usernameEdit.TabIndex = 4;
			this.usernameEdit.Text = "";
			// 
			// passwordEdit
			// 
			this.passwordEdit.Location = new System.Drawing.Point(104, 80);
			this.passwordEdit.Name = "passwordEdit";
			this.passwordEdit.PasswordChar = '*';
			this.passwordEdit.Size = new System.Drawing.Size(208, 20);
			this.passwordEdit.TabIndex = 5;
			this.passwordEdit.Text = "";
			this.passwordEdit.TextChanged += new System.EventHandler(this.passwordEdit_TextChanged);
			// 
			// HostDialog
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(328, 156);
			this.Controls.Add(this.passwordEdit);
			this.Controls.Add(this.usernameEdit);
			this.Controls.Add(this.host);
			this.Controls.Add(this.passwordLbl);
			this.Controls.Add(this.usernameLbl);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HostDialog";
			this.ShowInTaskbar = false;
			this.Text = "HostDialog";
			this.ResumeLayout(false);

		}
		#endregion

		private void passwordEdit_TextChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
