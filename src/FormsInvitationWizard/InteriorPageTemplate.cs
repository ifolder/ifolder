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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Novell.iFolder.InvitationWizard
{
	/// <summary>
	/// Summary description for InteriorPageTemplate.
	/// </summary>
	public class InteriorPageTemplate : Novell.iFolder.InvitationWizard.BaseWizardPage
	{
		#region Class Members
		private System.Windows.Forms.Panel headerPanel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label headerTitle;
		private System.Windows.Forms.Label headerSubTitle;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public InteriorPageTemplate()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.headerPanel = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.headerSubTitle = new System.Windows.Forms.Label();
			this.headerTitle = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.headerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// headerPanel
			// 
			this.headerPanel.BackColor = System.Drawing.Color.White;
			this.headerPanel.Controls.Add(this.pictureBox1);
			this.headerPanel.Controls.Add(this.headerSubTitle);
			this.headerPanel.Controls.Add(this.headerTitle);
			this.headerPanel.Location = new System.Drawing.Point(0, 0);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = new System.Drawing.Size(496, 56);
			this.headerPanel.TabIndex = 0;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(448, 8);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(40, 40);
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// headerSubTitle
			// 
			this.headerSubTitle.Location = new System.Drawing.Point(40, 32);
			this.headerSubTitle.Name = "headerSubTitle";
			this.headerSubTitle.Size = new System.Drawing.Size(384, 16);
			this.headerSubTitle.TabIndex = 1;
			this.headerSubTitle.Text = "Header subtitle";
			// 
			// headerTitle
			// 
			this.headerTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.headerTitle.Location = new System.Drawing.Point(24, 16);
			this.headerTitle.Name = "headerTitle";
			this.headerTitle.Size = new System.Drawing.Size(400, 16);
			this.headerTitle.TabIndex = 0;
			this.headerTitle.Text = "Header Title";
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(0, 56);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(496, 4);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// InteriorPageTemplate
			// 
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.headerPanel);
			this.Name = "InteriorPageTemplate";
			this.Size = new System.Drawing.Size(496, 314);
			this.headerPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public string HeaderTitle
		{
			get
			{
				return headerTitle.Text;
			}

			set
			{
				headerTitle.Text = value;
			}
		}

		public string HeaderSubTitle
		{
			get
			{
				return headerSubTitle.Text;
			}

			set
			{
				headerSubTitle.Text = value;
			}
		}
		#endregion
	}
}
