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
	/// A class that is used as a template for exterior wizard pages.
	/// </summary>
	public class WelcomePageTemplate : Novell.iFolder.InvitationWizard.BaseWizardPage
	{
		#region Class Members
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label welcomeTitle;
		private System.Windows.Forms.Label descriptionText;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label actionText;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs a WelcomePageTemplate object.
		/// </summary>
		public WelcomePageTemplate()
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.actionText = new System.Windows.Forms.Label();
			this.descriptionText = new System.Windows.Forms.Label();
			this.welcomeTitle = new System.Windows.Forms.Label();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.Controls.Add(this.actionText);
			this.panel1.Controls.Add(this.descriptionText);
			this.panel1.Controls.Add(this.welcomeTitle);
			this.panel1.Location = new System.Drawing.Point(168, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(328, 304);
			this.panel1.TabIndex = 1;
			// 
			// actionText
			// 
			this.actionText.Location = new System.Drawing.Point(16, 256);
			this.actionText.Name = "actionText";
			this.actionText.Size = new System.Drawing.Size(296, 40);
			this.actionText.TabIndex = 2;
			this.actionText.Text = "action...";
			// 
			// descriptionText
			// 
			this.descriptionText.Location = new System.Drawing.Point(16, 72);
			this.descriptionText.Name = "descriptionText";
			this.descriptionText.Size = new System.Drawing.Size(296, 184);
			this.descriptionText.TabIndex = 1;
			this.descriptionText.Text = "Description ...";
			// 
			// welcomeTitle
			// 
			this.welcomeTitle.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.welcomeTitle.Location = new System.Drawing.Point(16, 16);
			this.welcomeTitle.Name = "welcomeTitle";
			this.welcomeTitle.Size = new System.Drawing.Size(296, 40);
			this.welcomeTitle.TabIndex = 0;
			this.welcomeTitle.Text = "Welcome to the <WIZARD> Wizard";
			// 
			// waterMark
			// 
			this.waterMark.Location = new System.Drawing.Point(0, 0);
			this.waterMark.Name = "waterMark";
			this.waterMark.Size = new System.Drawing.Size(168, 304);
			this.waterMark.TabIndex = 0;
			this.waterMark.TabStop = false;
			// 
			// WelcomePageTemplate
			// 
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.waterMark);
			this.Name = "WelcomePageTemplate";
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets/sets the action text.
		/// </summary>
		public string ActionText
		{
			get
			{
				return actionText.Text;
			}
			
			set
			{
				actionText.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the welcome title.
		/// </summary>
		public string WelcomeTitle
		{
			get
			{
				return welcomeTitle.Text;
			}

			set
			{
				welcomeTitle.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the description text.
		/// </summary>
		public string DescriptionText
		{
			get
			{
				return descriptionText.Text;
			}

			set
			{
				descriptionText.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the watermark image.
		/// </summary>
		public Image Watermark
		{
			get
			{
				return waterMark.Image;
			}
			set
			{
				waterMark.Image = value;
			}
		}
		#endregion
	}
}
