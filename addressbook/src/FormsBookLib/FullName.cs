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

namespace Novell.iFolder.FormsBookLib
{
	/// <summary>
	/// Summary description for FullName.
	/// </summary>
	public class FullName : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.TextBox title;
		private System.Windows.Forms.TextBox firstName;
		private System.Windows.Forms.TextBox middleName;
		private System.Windows.Forms.TextBox lastName;
		private System.Windows.Forms.TextBox suffix;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FullName()
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
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.title = new System.Windows.Forms.TextBox();
			this.firstName = new System.Windows.Forms.TextBox();
			this.middleName = new System.Windows.Forms.TextBox();
			this.lastName = new System.Windows.Forms.TextBox();
			this.suffix = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(56, 192);
			this.ok.Name = "ok";
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(136, 192);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			// 
			// title
			// 
			this.title.Location = new System.Drawing.Point(48, 16);
			this.title.Name = "title";
			this.title.Size = new System.Drawing.Size(160, 20);
			this.title.TabIndex = 0;
			this.title.Text = "";
			// 
			// firstName
			// 
			this.firstName.Location = new System.Drawing.Point(48, 48);
			this.firstName.Name = "firstName";
			this.firstName.Size = new System.Drawing.Size(160, 20);
			this.firstName.TabIndex = 1;
			this.firstName.Text = "";
			// 
			// middleName
			// 
			this.middleName.Location = new System.Drawing.Point(48, 80);
			this.middleName.Name = "middleName";
			this.middleName.Size = new System.Drawing.Size(160, 20);
			this.middleName.TabIndex = 2;
			this.middleName.Text = "";
			// 
			// lastName
			// 
			this.lastName.Location = new System.Drawing.Point(48, 112);
			this.lastName.Name = "lastName";
			this.lastName.Size = new System.Drawing.Size(160, 20);
			this.lastName.TabIndex = 3;
			this.lastName.Text = "";
			// 
			// suffix
			// 
			this.suffix.Location = new System.Drawing.Point(48, 144);
			this.suffix.Name = "suffix";
			this.suffix.Size = new System.Drawing.Size(160, 20);
			this.suffix.TabIndex = 4;
			this.suffix.Text = "";
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(6, 176);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(208, 4);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(19, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 16);
			this.label1.TabIndex = 8;
			this.label1.Text = "Title:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(20, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 16);
			this.label2.TabIndex = 9;
			this.label2.Text = "First:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 82);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 16);
			this.label3.TabIndex = 10;
			this.label3.Text = "Middle:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(18, 114);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 16);
			this.label4.TabIndex = 11;
			this.label4.Text = "Last:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(12, 146);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(48, 16);
			this.label5.TabIndex = 12;
			this.label5.Text = "Suffix:";
			// 
			// FullName
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(218, 224);
			this.Controls.Add(this.suffix);
			this.Controls.Add(this.lastName);
			this.Controls.Add(this.middleName);
			this.Controls.Add(this.firstName);
			this.Controls.Add(this.title);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FullName";
			this.ShowInTaskbar = false;
			this.Text = "Full Name";
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public string Title
		{
			get
			{
				return title.Text.Trim();
			}
			set
			{
				title.Text = value;
			}
		}

		public string FirstName
		{
			get
			{
				return firstName.Text.Trim();
			}
			set
			{
				firstName.Text = value;
			}
		}

		public string MiddleName
		{
			get
			{
				return middleName.Text.Trim();
			}
			set
			{
				middleName.Text = value;
			}
		}

		public string LastName
		{
			get
			{
				return lastName.Text.Trim();
			}
			set
			{
				lastName.Text = value;
			}
		}

		public string Suffix
		{
			get
			{
				return suffix.Text.Trim();
			}
			set
			{
				suffix.Text = value;
			}
		}
		#endregion
	}
}
