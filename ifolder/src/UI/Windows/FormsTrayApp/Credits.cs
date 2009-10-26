/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Bruce Getter <bgetter@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for Credits.
	/// </summary>
	public class Credits : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.ListBox listBox1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Credits()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Credits));
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            resources.ApplyResources(this.listBox1, "listBox1");
            this.listBox1.Items.AddRange(new object[] {
            resources.GetString("listBox1.Items"),
            resources.GetString("listBox1.Items1"),
            resources.GetString("listBox1.Items2"),
            resources.GetString("listBox1.Items3"),
            resources.GetString("listBox1.Items4"),
            resources.GetString("listBox1.Items5"),
            resources.GetString("listBox1.Items6"),
            resources.GetString("listBox1.Items7"),
            resources.GetString("listBox1.Items8"),
            resources.GetString("listBox1.Items9"),
            resources.GetString("listBox1.Items10"),
            resources.GetString("listBox1.Items11"),
            resources.GetString("listBox1.Items12"),
            resources.GetString("listBox1.Items13"),
            resources.GetString("listBox1.Items14"),
            resources.GetString("listBox1.Items15"),
            resources.GetString("listBox1.Items16"),
            resources.GetString("listBox1.Items17"),
            resources.GetString("listBox1.Items18"),
            resources.GetString("listBox1.Items19"),
            resources.GetString("listBox1.Items20"),
            resources.GetString("listBox1.Items21"),
            resources.GetString("listBox1.Items22"),
            resources.GetString("listBox1.Items23"),
            resources.GetString("listBox1.Items24"),
            resources.GetString("listBox1.Items25"),
            resources.GetString("listBox1.Items26"),
            resources.GetString("listBox1.Items27"),
            resources.GetString("listBox1.Items28"),
            resources.GetString("listBox1.Items29"),
            resources.GetString("listBox1.Items30"),
            resources.GetString("listBox1.Items31"),
            resources.GetString("listBox1.Items32"),
            resources.GetString("listBox1.Items33"),
            resources.GetString("listBox1.Items34"),
            resources.GetString("listBox1.Items35"),
            resources.GetString("listBox1.Items36"),
            resources.GetString("listBox1.Items37"),
            resources.GetString("listBox1.Items38"),
            resources.GetString("listBox1.Items39"),
            resources.GetString("listBox1.Items40"),
            resources.GetString("listBox1.Items41"),
            resources.GetString("listBox1.Items42"),
            resources.GetString("listBox1.Items43"),
            resources.GetString("listBox1.Items44"),
            resources.GetString("listBox1.Items45"),
            resources.GetString("listBox1.Items46"),
            resources.GetString("listBox1.Items47"),
            resources.GetString("listBox1.Items48"),
            resources.GetString("listBox1.Items49"),
            resources.GetString("listBox1.Items50"),
            resources.GetString("listBox1.Items51"),
            resources.GetString("listBox1.Items52"),
            resources.GetString("listBox1.Items53"),
            resources.GetString("listBox1.Items54"),
            resources.GetString("listBox1.Items55"),
            resources.GetString("listBox1.Items56"),
            resources.GetString("listBox1.Items57"),
            resources.GetString("listBox1.Items58"),
            resources.GetString("listBox1.Items59"),
            resources.GetString("listBox1.Items60"),
            resources.GetString("listBox1.Items61"),
            resources.GetString("listBox1.Items62"),
            resources.GetString("listBox1.Items63"),
            resources.GetString("listBox1.Items64"),
            resources.GetString("listBox1.Items65"),
            resources.GetString("listBox1.Items66"),
            resources.GetString("listBox1.Items67"),
            resources.GetString("listBox1.Items68"),
            resources.GetString("listBox1.Items69"),
            resources.GetString("listBox1.Items70"),
            resources.GetString("listBox1.Items71"),
            resources.GetString("listBox1.Items72"),
            resources.GetString("listBox1.Items73"),
            resources.GetString("listBox1.Items74"),
            resources.GetString("listBox1.Items75"),
            resources.GetString("listBox1.Items76"),
            resources.GetString("listBox1.Items77"),
            resources.GetString("listBox1.Items78"),
            resources.GetString("listBox1.Items79"),
            resources.GetString("listBox1.Items80")});
            this.listBox1.Name = "listBox1";
            // 
            // close
            // 
            this.close.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.close, "close");
            this.close.Name = "close";
            // 
            // Credits
            // 
            this.AcceptButton = this.close;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.close);
            this.Controls.Add(this.listBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Credits";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

		}
		#endregion
	}
}
