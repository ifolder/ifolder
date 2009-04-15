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
*                 $Modified by: Satya       <ssutapalli@novell.com>
*                 $Mod Date: 04/09/2007
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
using System.Windows.Forms;
using System.IO;
using Novell.FormsTrayApp;

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the wizard page where the invitation file is selected.
	/// </summary>
	public class ServerPage : Novell.Wizard.InteriorPageTemplate
	{
		#region Class Members

//		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(SelectInvitationPage));
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox serverAddress;
		private System.Windows.Forms.CheckBox defaultServer;
		private System.Windows.Forms.Label defaultDescription;
		private System.ComponentModel.IContainer components = null;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));
		private int defaultTextXPos =50;
		private int defaultTextYPos = 110;
		private int defaultSpacing = 16;
		private int maxTextWidth = 420;
		private int defaultTextWidth = 90;

        private SizeF strSize;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a ServerPage object.
		/// </summary>
		public ServerPage( bool makeDefaultAccount )
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			defaultDescription.Visible = defaultServer.Visible = !makeDefaultAccount;
			defaultServer.Checked = makeDefaultAccount;
		}

        /// <summary>
        /// Disbale Server Entry
        /// </summary>
        public void DisableServerEntry()
        {
            this.serverAddress.ReadOnly = true;
        }

		#endregion

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serverAddress = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.defaultDescription = new System.Windows.Forms.Label();
			this.defaultServer = new System.Windows.Forms.CheckBox();
            this.strSize = new SizeF();
			this.SuspendLayout();
            Graphics graphics = CreateGraphics();
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point( this.defaultTextXPos-10, this.defaultTextYPos );
			this.label2.Name = "label2";			
			this.label2.TabIndex = 0;
			this.label2.Text = Resource.GetString("ServerPagelbl2txt");//"Enter the name of your iFolder server (for example, "ifolder.example.net").";			
            this.strSize = graphics.MeasureString(this.label2.Text, this.label2.Font);
			this.label2.Size = new System.Drawing.Size( this.maxTextWidth , ((int)this.strSize.Width/this.maxTextWidth+1)*16 );			
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point( this.defaultTextXPos, this.label2.Location.Y+this.label2.Size.Height+this.defaultSpacing );
			this.label1.Name = "label1";			
			this.label1.TabIndex = 1;
            this.label1.Text = Resource.GetString("ServerNameTextbox") + ":";//"Server address:";
            this.strSize = graphics.MeasureString(this.label1.Text, this.label1.Font);
			this.label1.Size = new System.Drawing.Size( this.defaultTextWidth, ((int)this.strSize.Width/this.defaultTextWidth+1)*16 );
			// 
			// serverAddress
			// 
			this.serverAddress.Location = new System.Drawing.Point( 145, this.label1.Location.Y-2);
			this.serverAddress.Name = "serverAddress";
			this.serverAddress.Size = new System.Drawing.Size(320, 20);
			this.serverAddress.TabIndex = 2;
			this.serverAddress.Text = "";
			this.serverAddress.TextChanged += new System.EventHandler(this.serverAddress_TextChanged);			
			// 
			// defaultDescription
			// 
			this.defaultDescription.Location = new System.Drawing.Point( this.defaultTextXPos-10, this.label1.Location.Y+this.label1.Size.Height+this.defaultSpacing );
			this.defaultDescription.Name = "defaultDescription";			
			this.defaultDescription.TabIndex = 3;
			this.defaultDescription.Text = Resource.GetString("ServerPageDesc");//"Setting this iFolder server as your default server will allow iFolder to automatically select this server when adding new iFolders.";
            this.strSize = graphics.MeasureString(this.defaultDescription.Text, this.defaultDescription.Font);
			this.defaultDescription.Size = new System.Drawing.Size( this.maxTextWidth , ((int)this.strSize.Width/this.maxTextWidth+1)*16 );			
			// 
			// defaultServer
			// 
			this.defaultServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.defaultServer.Location = new System.Drawing.Point( this.defaultTextXPos, this.defaultDescription.Location.Y+this.defaultDescription.Size.Height+this.defaultSpacing );
			this.defaultServer.Name = "defaultServer";
			this.defaultServer.Size = new System.Drawing.Size(406, 20);
			this.defaultServer.TabIndex = 4;
			this.defaultServer.Text = Resource.GetString("DefaultServerCheckbox");//"Make this my &default server.";

            graphics.Dispose();
			// 
			// ServerPage
			// 
			this.Controls.Add(this.defaultServer);
			this.Controls.Add(this.serverAddress);
			this.Controls.Add(this.defaultDescription);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.HeaderTitle = "";
			this.Name = "ServerPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.defaultDescription, 0);
			this.Controls.SetChildIndex(this.serverAddress, 0);
			this.Controls.SetChildIndex(this.defaultServer, 0);
			this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region Event Handlers

        /// <summary>
        /// Event Handler for serverAddress text changed event
        /// </summary>
        private void serverAddress_TextChanged(object sender, System.EventArgs e)
		{
			// Enable the buttons.
			if (serverAddress.Text != "")
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
			}
			else
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Back | WizardButtons.Cancel;
			}
		}

		#endregion

		#region Overridden Methods
        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous Page</param>
		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons.
			serverAddress_TextChanged(this, null);

			serverAddress.Focus();
		}

        /// <summary>
        /// Deactivate page
        /// </summary>
        internal override int DeactivatePage()
		{
			// TODO - implement this...
			return base.DeactivatePage ();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

        /// <summary>
        /// Validate page
        /// </summary>
        /// <param name="currentIndex">current index</param>
        internal override int ValidatePage(int currentIndex)
		{
			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the server address entered.
		/// </summary>
		public string ServerAddress
		{
			get { return serverAddress.Text; }
            set { serverAddress.Text = value; }
		}

		/// <summary>
		/// Gets a value indicating if this server should be the default server.
		/// </summary>
		public bool DefaultServer
		{
			get { return defaultServer.Checked; }
		}

		#endregion
	}
}

