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
using Simias.Storage;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for TestForm.
	/// </summary>
	public class UserProperties : System.Windows.Forms.Form
	{
		private System.Windows.Forms.GroupBox accessButtons;
		private System.Windows.Forms.RadioButton readOnly;
		private System.Windows.Forms.RadioButton readWrite;
		private System.Windows.Forms.RadioButton fullControl;
		private System.Windows.Forms.CheckBox owner;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private bool ownerCanBeSet;
		private bool canBeOwner;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructs a UserProperties object.
		/// </summary>
		public UserProperties()
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
			this.accessButtons = new System.Windows.Forms.GroupBox();
			this.readOnly = new System.Windows.Forms.RadioButton();
			this.readWrite = new System.Windows.Forms.RadioButton();
			this.fullControl = new System.Windows.Forms.RadioButton();
			this.owner = new System.Windows.Forms.CheckBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.accessButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// accessButtons
			// 
			this.accessButtons.Controls.Add(this.readOnly);
			this.accessButtons.Controls.Add(this.readWrite);
			this.accessButtons.Controls.Add(this.fullControl);
			this.accessButtons.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.accessButtons.Location = new System.Drawing.Point(14, 16);
			this.accessButtons.Name = "accessButtons";
			this.accessButtons.Size = new System.Drawing.Size(264, 104);
			this.accessButtons.TabIndex = 2;
			this.accessButtons.TabStop = false;
			this.accessButtons.Text = "Access";
			// 
			// readOnly
			// 
			this.readOnly.Location = new System.Drawing.Point(16, 72);
			this.readOnly.Name = "readOnly";
			this.readOnly.Size = new System.Drawing.Size(232, 16);
			this.readOnly.TabIndex = 2;
			this.readOnly.Text = "Read Only";
			// 
			// readWrite
			// 
			this.readWrite.Location = new System.Drawing.Point(16, 48);
			this.readWrite.Name = "readWrite";
			this.readWrite.Size = new System.Drawing.Size(232, 16);
			this.readWrite.TabIndex = 1;
			this.readWrite.Text = "Read/Write";
			// 
			// fullControl
			// 
			this.fullControl.Location = new System.Drawing.Point(16, 24);
			this.fullControl.Name = "fullControl";
			this.fullControl.Size = new System.Drawing.Size(232, 16);
			this.fullControl.TabIndex = 0;
			this.fullControl.Text = "Full Control";
			// 
			// owner
			// 
			this.owner.Location = new System.Drawing.Point(14, 152);
			this.owner.Name = "owner";
			this.owner.Size = new System.Drawing.Size(264, 16);
			this.owner.TabIndex = 3;
			this.owner.Text = "Make this user the owner of the collection.";
			this.owner.CheckedChanged += new System.EventHandler(this.owner_CheckedChanged);
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(128, 184);
			this.ok.Name = "ok";
			this.ok.TabIndex = 4;
			this.ok.Text = "OK";
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(208, 184);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 5;
			this.cancel.Text = "Cancel";
			// 
			// UserProperties
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 214);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.accessButtons);
			this.Controls.Add(this.owner);
			this.Name = "UserProperties";
			this.Text = "TestForm";
			this.Load += new System.EventHandler(this.TestForm_Load);
			this.accessButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void TestForm_Load(object sender, System.EventArgs e)
		{
			owner.Enabled = canBeOwner && ownerCanBeSet && !owner.Checked;
		}

		private void owner_CheckedChanged(object sender, System.EventArgs e)
		{
			accessButtons.Enabled = !owner.Checked;

			if (owner.Checked)
			{
				fullControl.Checked = true;
			}
		}

		#region Properties
		/// <summary>
		/// Sets the string that will be displayed in the title bar.
		/// </summary>
		public string Title
		{
			set
			{
				this.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the value of the rights displayed in the access control buttons.
		/// </summary>
		public Simias.Storage.Access.Rights Rights
		{
			get
			{
				if (fullControl.Checked)
				{
					return Access.Rights.Admin;
				}
				else if (readWrite.Checked)
				{
					return Access.Rights.ReadWrite;
				}
				else
				{
					return Access.Rights.ReadOnly;
				}
			}
			set 
			{
				switch (value)
				{
					case Access.Rights.Admin:
						fullControl.Checked = true;
						break;
					case Access.Rights.ReadWrite:
						readWrite.Checked = true;
						break;
					default:
						readOnly.Checked = true;
						break;
				}
			}
		}

		/// <summary>
		/// Sets a value indicating that the selected user can become the owner.
		/// </summary>
		public bool CanBeOwner
		{
			set { canBeOwner = value; }
		}

		/// <summary>
		/// Gets/sets a value indicating that the selected user is the owner.
		/// </summary>
		public bool Owner
		{
			get { return owner.Checked; }
			set { owner.Checked = value; }
		}

		/// <summary>
		/// Sets a value indicating that a single user is selected.
		/// </summary>
		public bool OwnerCanBeSet
		{
			set { ownerCanBeSet = value; }
		}
		#endregion
	}
}
