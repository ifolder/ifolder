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
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public string HostName
		{
			get { return host.Text; }
			set { host.Text = value; }
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
			this.SuspendLayout();
			// 
			// host
			// 
			this.host.Location = new System.Drawing.Point(96, 16);
			this.host.Name = "host";
			this.host.Size = new System.Drawing.Size(208, 20);
			this.host.TabIndex = 0;
			this.host.Text = "http://localhost:8086/simias10/mlasky";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "Host Uri:";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(152, 56);
			this.ok.Name = "ok";
			this.ok.TabIndex = 2;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(232, 56);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			// 
			// HostDialog
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(328, 94);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.host);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HostDialog";
			this.ShowInTaskbar = false;
			this.Text = "HostDialog";
			this.ResumeLayout(false);

		}
		#endregion
	}
}
