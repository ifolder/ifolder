/***********************************************************************
 *  AddBook.cs - A dialog to add an address book implemented 
 *	using Windows.Forms
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
	/// Summary description for AddBook.
	/// </summary>
	public class AddBook : System.Windows.Forms.Form
	{
		#region Class Members
		private System.Windows.Forms.TextBox bookName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public AddBook()
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
			this.bookName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// bookName
			// 
			this.bookName.Location = new System.Drawing.Point(72, 48);
			this.bookName.Name = "bookName";
			this.bookName.Size = new System.Drawing.Size(200, 20);
			this.bookName.TabIndex = 0;
			this.bookName.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 48);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Name:";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(116, 88);
			this.ok.Name = "ok";
			this.ok.TabIndex = 2;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(200, 88);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			// 
			// AddBook
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(292, 126);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.bookName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AddBook";
			this.ShowInTaskbar = false;
			this.Text = "Create Address Book";
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public new string Name
		{
			get
			{
				return bookName.Text;
			}

			set
			{
				bookName.Text = value;
			}
		}
		#endregion
	}
}
