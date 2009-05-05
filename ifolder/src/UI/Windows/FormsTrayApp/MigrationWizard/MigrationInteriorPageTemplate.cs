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
*                 $Author: Ramesh Sunder <sramesh@novell.com>
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Novell.Wizard
{
	/// <summary>
	/// A class that is a template for interior pages of a wizard.
	/// </summary>
	public class MigrationInteriorPageTemplate : Novell.Wizard.MigrationBaseWizardPage
	{
		#region Class Members
		private System.Windows.Forms.Panel headerPanel;
		private System.Windows.Forms.PictureBox left_image;
		private System.Windows.Forms.PictureBox right_image;
		private System.Windows.Forms.Label headerTitle;
		private System.Windows.Forms.Label headerSubTitle;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs an InteriorPageTemplate object.
		/// </summary>
		public MigrationInteriorPageTemplate()
		{
			// This call is required by the Windows.Forms Form Designer.
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.headerPanel = new System.Windows.Forms.Panel();
			this.left_image = new System.Windows.Forms.PictureBox();
			this.right_image = new System.Windows.Forms.PictureBox();
			this.headerSubTitle = new System.Windows.Forms.Label();
			this.headerTitle = new System.Windows.Forms.Label();
			this.headerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// headerPanel
			// 
			this.headerPanel.Controls.Add(this.left_image);
			this.headerPanel.Controls.Add(this.right_image);
			this.headerPanel.Location = new System.Drawing.Point(0, 0);
			this.headerPanel.Name = "headerPanel";
			this.headerPanel.Size = new System.Drawing.Size(496, 65);
			this.headerPanel.TabIndex = 0;
			// 
			// left_image  for adding the image
			// 
			this.left_image.Location = new System.Drawing.Point(0, 0);
			this.left_image.Name = "left_image";
			this.left_image.Size = new System.Drawing.Size(159,65);
			this.left_image.TabIndex = 2;
			this.left_image.TabStop = false;
			this.left_image.Image = System.Drawing.Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			// 
			// right_image  for adding the image
			// 
			this.right_image.Location = new System.Drawing.Point(157,0);
			this.right_image.Name = "right_image";
			this.right_image.Size = new System.Drawing.Size(342,65);
			this.right_image.TabIndex = 3;
			this.right_image.TabStop = false;
			this.right_image.SizeMode = PictureBoxSizeMode.StretchImage;
			this.right_image.Image = System.Drawing.Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
			// 
			// headerTitle
			// 
			this.headerTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.headerTitle.Location = new System.Drawing.Point(16,70);
			this.headerTitle.Name = "headerTitle";
			this.headerTitle.Size = new System.Drawing.Size(424, 16);
			this.headerTitle.TabIndex = 0;
			this.headerTitle.Text = "";
			// 
			// InteriorPageTemplate
			// 
			this.Controls.Add(this.headerPanel);
			this.Controls.Add(this.headerTitle);
			this.Name = "InteriorPageTemplate";
			this.Size = new System.Drawing.Size(496, 314);
			this.headerPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the text of the header title.
		/// </summary>
		public string HeaderTitle
		{
			get	{ return headerTitle.Text; }
			set	{ headerTitle.Text = value;	}
		}

		/// <summary>
		/// Gets/sets the text of the header subtitle.
		/// </summary>
		public string HeaderSubTitle
		{
			get { return headerSubTitle.Text; }
			set	{ headerSubTitle.Text = value; }
		}

		/// <summary>
		/// Gets/sets the image used for the thumbnail
		/// </summary>
		public Image Thumbnail
		{
			get { return left_image.Image;	}
			set	{ left_image.Image = value; }
		}
		#endregion
	}
}

