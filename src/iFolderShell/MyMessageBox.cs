/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for MyMessageBox.
	/// </summary>
	[ComVisible(false)]
	public class MyMessageBox : System.Windows.Forms.Form
	{
		private const float maxWidth = 400;
		private System.Windows.Forms.Button yes;
		private System.Windows.Forms.Button no;
		private System.Windows.Forms.Label message;
		private System.Windows.Forms.PictureBox messageIcon;
		private System.Windows.Forms.Button ok;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs a MyMessageBox object.
		/// </summary>
		public MyMessageBox()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.StartPosition = FormStartPosition.CenterParent;
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
			this.messageIcon = new System.Windows.Forms.PictureBox();
			this.ok = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// yes
			// 
			this.yes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			this.yes.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.yes.Location = new System.Drawing.Point(176, 64);
			this.yes.Name = "yes";
			this.yes.Size = new System.Drawing.Size(77, 23);
			this.yes.TabIndex = 0;
			this.yes.Text = "Yes";
			this.yes.Visible = false;
			// 
			// no
			// 
			this.no.DialogResult = System.Windows.Forms.DialogResult.No;
			this.no.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.no.Location = new System.Drawing.Point(256, 64);
			this.no.Name = "no";
			this.no.TabIndex = 1;
			this.no.Text = "No";
			this.no.Visible = false;
			// 
			// message
			// 
			this.message.Location = new System.Drawing.Point(64, 24);
			this.message.Name = "message";
			this.message.Size = new System.Drawing.Size(400, 16);
			this.message.TabIndex = 2;
			// 
			// messageIcon
			// 
			this.messageIcon.Location = new System.Drawing.Point(16, 24);
			this.messageIcon.Name = "messageIcon";
			this.messageIcon.Size = new System.Drawing.Size(32, 32);
			this.messageIcon.TabIndex = 3;
			this.messageIcon.TabStop = false;
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(96, 64);
			this.ok.Name = "ok";
			this.ok.TabIndex = 4;
			this.ok.Text = "OK";
			// 
			// MyMessageBox
			// 
			this.AcceptButton = this.yes;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.no;
			this.ClientSize = new System.Drawing.Size(490, 96);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.messageIcon);
			this.Controls.Add(this.message);
			this.Controls.Add(this.no);
			this.Controls.Add(this.yes);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MyMessageBox";
			this.ShowInTaskbar = false;
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.MyMessageBox_Paint);
			this.ResumeLayout(false);

		}
		#endregion

		private void MyMessageBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			SizeF size = e.Graphics.MeasureString(message.Text, message.Font);
			float width = (size.Width / maxWidth) > 1 ? maxWidth : size.Width;
			float height = (float)Math.Ceiling(size.Width / width) * size.Height + 2;
			message.Size = new Size((int)Math.Ceiling(width), (int)Math.Ceiling(height));
			this.Width = message.Right + message.Left + 4;

			ok.Left = (ClientRectangle.Width - ok.Width) / 2;
			ok.Top = message.Bottom + 12;

			yes.Left = (ClientRectangle.Width / 2) - (yes.Width + 4);
			no.Left = yes.Right + 4;
			yes.Top = no.Top = ok.Top;
			this.Height = ok.Bottom + (this.Height - ClientRectangle.Height) + 12;
		}

		/// <summary>
		/// Sets the string to display in the message box.
		/// </summary>
		public string Message
		{
			set
			{
				this.message.Text = value;
			}
		}

		/// <summary>
		/// Sets the string to display in the title bar.
		/// </summary>
		public string Caption
		{
			set
			{
				this.Text = value;
			}
		}

		/// <summary>
		/// Sets the icon to display in the message box.
		/// </summary>
		public Icon MessageIcon
		{
			set
			{
				this.messageIcon.Image = (Image)value.ToBitmap();
			}
		}

		/// <summary>
		/// Set to display Yes/No buttons instead of OK button.
		/// </summary>
		public bool YesNo
		{
			set
			{
				yes.Visible = no.Visible = value;
				ok.Visible = !value;
			}
		}
	}
}
