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
 *  Author: Bruce
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Simias
{
	/// <summary>
	/// Summary description for LimitSize.
	/// </summary>
	public class LimitSize : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown sizeLimit;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LimitSize()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.label1 = new System.Windows.Forms.Label();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.sizeLimit = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.sizeLimit)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Number of entries:";
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(40, 72);
			this.ok.Name = "ok";
			this.ok.TabIndex = 2;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(120, 72);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 3;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(112, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "(enter -1 for no limit)";
			// 
			// sizeLimit
			// 
			this.sizeLimit.Location = new System.Drawing.Point(104, 24);
			this.sizeLimit.Maximum = new System.Decimal(new int[] {
																	  500,
																	  0,
																	  0,
																	  0});
			this.sizeLimit.Minimum = new System.Decimal(new int[] {
																	  1,
																	  0,
																	  0,
																	  -2147483648});
			this.sizeLimit.Name = "sizeLimit";
			this.sizeLimit.Size = new System.Drawing.Size(88, 20);
			this.sizeLimit.TabIndex = 5;
			// 
			// LimitSize
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(202, 104);
			this.Controls.Add(this.sizeLimit);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "LimitSize";
			this.Text = "Number of Entries in ListView";
			((System.ComponentModel.ISupportInitialize)(this.sizeLimit)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Sets/Gets the size limit value.
		/// </summary>
		public decimal SizeLimit
		{
			get { return sizeLimit.Value; }
			set { sizeLimit.Value = value; }
		}
	}
}
