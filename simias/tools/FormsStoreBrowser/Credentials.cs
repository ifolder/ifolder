using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for Credentials.
	/// </summary>
	public class Credentials : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button Button2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox3;

		private IStoreBrowser browser;
		private bool checkAuthentication = false;
		private bool authenticated = false;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public string UserName
		{
			get { return this.textBox1.Text; }
			set { this.textBox1.Text = value; }
		}

		public string Password
		{
			get { return this.textBox2.Text; }
			set { this.textBox2.Text = value; }
		}

		public Credentials( string hostName, IStoreBrowser browser )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			this.browser = browser;
			this.textBox3.Text = hostName;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Credentials));
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.Button2 = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(24, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "User Name:";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(88, 48);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(256, 20);
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "";
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(24, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 23);
			this.label2.TabIndex = 7;
			this.label2.Text = "Password:";
			// 
			// textBox2
			// 
			this.textBox2.AcceptsReturn = true;
			this.textBox2.Location = new System.Drawing.Point(88, 80);
			this.textBox2.Name = "textBox2";
			this.textBox2.PasswordChar = '*';
			this.textBox2.Size = new System.Drawing.Size(256, 20);
			this.textBox2.TabIndex = 1;
			this.textBox2.Text = "";
			// 
			// button1
			// 
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.Enabled = false;
			this.button1.Location = new System.Drawing.Point(184, 120);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "OK";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// Button2
			// 
			this.Button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Button2.Location = new System.Drawing.Point(272, 120);
			this.Button2.Name = "Button2";
			this.Button2.TabIndex = 3;
			this.Button2.Text = "Cancel";
			this.Button2.Click += new System.EventHandler(this.Button2_Click);
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(24, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 16);
			this.label3.TabIndex = 4;
			this.label3.Text = "Host:";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(88, 16);
			this.textBox3.Name = "textBox3";
			this.textBox3.ReadOnly = true;
			this.textBox3.Size = new System.Drawing.Size(256, 20);
			this.textBox3.TabIndex = 5;
			this.textBox3.TabStop = false;
			this.textBox3.Text = "";
			// 
			// Credentials
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.Button2;
			this.ClientSize = new System.Drawing.Size(362, 158);
			this.Controls.Add(this.textBox3);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.Button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Credentials";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Enter Credentials";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Credentials_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		private void Credentials_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if ( ( this.checkAuthentication == true ) && ( this.authenticated == false ) )
			{
				MessageBox.Show( "The user name or password is invalid.", "Error", MessageBoxButtons.OK );
				e.Cancel = true;
			}
			else
			{
				e.Cancel = false;
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;

			// Verify that the user can be authenticated.
			this.authenticated = this.browser.ValidateCredentials( this.textBox1.Text, this.textBox2.Text );
			this.checkAuthentication = true;

			this.Cursor = Cursors.Default;
		}

		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
			this.button1.Enabled = !this.textBox1.Text.Equals( String.Empty );
		}

		private void Button2_Click(object sender, System.EventArgs e)
		{
			this.checkAuthentication = false;
		}
	}
}
