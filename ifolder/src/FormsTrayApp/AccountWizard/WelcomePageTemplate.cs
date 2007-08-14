/****************************************************************************
 |
 | Copyright (c) [2007] Novell, Inc.
 | All Rights Reserved.
 |
 | This program is free software; you can redistribute it and/or
 | modify it under the terms of version 2 of the GNU General Public License as
 | published by the Free Software Foundation.
 |
 | This program is distributed in the hope that it will be useful,
 | but WITHOUT ANY WARRANTY; without even the implied warranty of
 | MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 | GNU General Public License for more details.
 |
 | You should have received a copy of the GNU General Public License
 | along with this program; if not, contact Novell, Inc.
 |
 | To contact Novell about this file by physical or electronic mail,
 | you may find current contact information at www.novell.com 
 |
 | Author: Bruce Getter <bgetter@novell.com>
 |
 |***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Novell.FormsTrayApp;

namespace Novell.Wizard
{
	/// <summary>
	/// A class that is used as a template for exterior wizard pages.
	/// </summary>
	public class WelcomePageTemplate : Novell.Wizard.BaseWizardPage
	{
		#region Class Members
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label welcomeTitle;
		private System.Windows.Forms.Label descriptionText;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label actionText;
		private System.Windows.Forms.Label imageTitle1;
		private System.Windows.Forms.Label imageTitle2;
		private System.Windows.Forms.Label imageTitle3;

		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
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
			this.imageTitle1 = new System.Windows.Forms.Label();
			this.imageTitle2 = new System.Windows.Forms.Label();
			this.imageTitle3 = new System.Windows.Forms.Label();
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
			this.actionText.Text = "";
			// 
			// descriptionText
			// 
			this.descriptionText.Location = new System.Drawing.Point(16, 72);
			this.descriptionText.Name = "descriptionText";
			this.descriptionText.Size = new System.Drawing.Size(296, 184);
			this.descriptionText.TabIndex = 1;
			this.descriptionText.Text = Resource.GetString("CompletionPageDT");//"Description ...";
			// 
			// welcomeTitle
			// 
			this.welcomeTitle.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.welcomeTitle.Location = new System.Drawing.Point(16, 16);
			this.welcomeTitle.Name = "welcomeTitle";
			this.welcomeTitle.Size = new System.Drawing.Size(296, 40);
			this.welcomeTitle.TabIndex = 0;
			this.welcomeTitle.Text = Resource.GetString("WelcomePageTitle");//"Welcome to the <WIZARD> Wizard";
			// 
			// waterMark
			// 
			this.waterMark.Location = new System.Drawing.Point(0, 243);
			this.waterMark.Name = "waterMark";
			this.waterMark.Size = new System.Drawing.Size(168, 60);
			this.waterMark.TabIndex = 0;
			this.waterMark.TabStop = false;
			//
			// imageTitle1;
			//
			this.imageTitle1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.imageTitle1.Location = new System.Drawing.Point(35, 95);
			this.imageTitle1.Name = "imageTitle1";
			this.imageTitle1.Size = new System.Drawing.Size(125, 20);
			this.imageTitle1.TabIndex = 0;
			this.imageTitle1.TabStop = false;
			this.imageTitle1.Text = Resource.GetString("YourFiles"); //Your Files
			//
			// imageTitle2;
			//
			this.imageTitle2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.imageTitle2.Location = new System.Drawing.Point(65, 130);
			this.imageTitle2.Name = "imageTitle2";
			this.imageTitle2.Size = new System.Drawing.Size(150, 20);
			this.imageTitle2.TabIndex = 0;
			this.imageTitle2.TabStop = false;
			this.imageTitle2.Text = Resource.GetString("AnyTime"); //Any Time...
			//
			// imageTitle3;
			//
			this.imageTitle3.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.imageTitle3.Location = new System.Drawing.Point(35, 160);
			this.imageTitle3.Name = "imageTitle3";
			this.imageTitle3.Size = new System.Drawing.Size(150, 20);
			this.imageTitle3.TabIndex = 0;
			this.imageTitle3.TabStop = false;
			this.imageTitle3.Text = Resource.GetString("AnyWhere"); //AnyWhere...
			// 
			// WelcomePageTemplate
			// 
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.waterMark);
			this.Controls.Add(this.imageTitle1);
			this.Controls.Add(this.imageTitle2);
			this.Controls.Add(this.imageTitle3);
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
