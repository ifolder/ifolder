using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for EmailPrompt.
	/// </summary>
	public class EmailPrompt : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox email;
		private System.Windows.Forms.Button ok;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EmailPrompt()
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
			this.label1 = new System.Windows.Forms.Label();
			this.email = new System.Windows.Forms.TextBox();
			this.ok = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(368, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "Please enter your email address.  (Don\'t worry ... this is only temporary)";
			// 
			// email
			// 
			this.email.Location = new System.Drawing.Point(132, 64);
			this.email.Name = "email";
			this.email.Size = new System.Drawing.Size(216, 20);
			this.email.TabIndex = 1;
			this.email.Text = "";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(203, 112);
			this.ok.Name = "ok";
			this.ok.TabIndex = 2;
			this.ok.Text = "OK";
			// 
			// EmailPrompt
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(480, 150);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.email);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EmailPrompt";
			this.ShowInTaskbar = false;
			this.Text = "EmailPrompt";
			this.ResumeLayout(false);

		}
		#endregion

		public string Email
		{
			get { return email.Text; }
			set { email.Text = value; }
		}
	}
}
