using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for MyMessageBox.
	/// </summary>
	[ComVisible(false)]
	public class MyMessageBox : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button yes;
		private System.Windows.Forms.Button no;
		private System.Windows.Forms.Label message;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MyMessageBox()
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
			this.yes = new System.Windows.Forms.Button();
			this.no = new System.Windows.Forms.Button();
			this.message = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// yes
			// 
			this.yes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.yes.Location = new System.Drawing.Point(200, 104);
			this.yes.Name = "yes";
			this.yes.TabIndex = 0;
			this.yes.Text = "Yes";
			// 
			// no
			// 
			this.no.DialogResult = System.Windows.Forms.DialogResult.No;
			this.no.Location = new System.Drawing.Point(280, 104);
			this.no.Name = "no";
			this.no.TabIndex = 1;
			this.no.Text = "No";
			// 
			// message
			// 
			this.message.Location = new System.Drawing.Point(40, 32);
			this.message.Name = "message";
			this.message.Size = new System.Drawing.Size(480, 40);
			this.message.TabIndex = 2;
			// 
			// MyMessageBox
			// 
			this.AcceptButton = this.yes;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.no;
			this.ClientSize = new System.Drawing.Size(552, 136);
			this.Controls.Add(this.message);
			this.Controls.Add(this.no);
			this.Controls.Add(this.yes);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MyMessageBox";
			this.ShowInTaskbar = false;
			this.ResumeLayout(false);

		}
		#endregion

		public string Message
		{
			set
			{
				this.message.Text = value;
			}
		}
	}
}
