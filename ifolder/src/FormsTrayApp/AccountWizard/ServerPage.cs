/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004-2006 Novell, Inc.
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
			this.SuspendLayout();
			// 
			// serverAddress
			// 
			this.serverAddress.Location = new System.Drawing.Point(136, 120);
			this.serverAddress.Name = "serverAddress";
			this.serverAddress.Size = new System.Drawing.Size(320, 20);
			this.serverAddress.TabIndex = 2;
			this.serverAddress.Text = "";
			this.serverAddress.TextChanged += new System.EventHandler(this.serverAddress_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(50, 122);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = Resource.GetString("ServerNameText");//"Server address:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(416, 24);
			this.label2.TabIndex = 0;
			this.label2.Text = Resource.GetString("ServerPagelbl2txt");//"Enter the name of your iFolder server (for example, ifolder.example.net).";
			// 
			// defaultDescription
			// 
			this.defaultDescription.Location = new System.Drawing.Point(40, 168);
			this.defaultDescription.Name = "defaultDescription";
			this.defaultDescription.Size = new System.Drawing.Size(416, 32);
			this.defaultDescription.TabIndex = 3;
			this.defaultDescription.Text = Resource.GetString("ServerPageDesc");//"Setting this iFolder server as your default server will allow iFolder to automatically select this server when adding new iFolders.";
			// 
			// defaultServer
			// 
			this.defaultServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.defaultServer.Location = new System.Drawing.Point(50, 208);
			this.defaultServer.Name = "defaultServer";
			this.defaultServer.Size = new System.Drawing.Size(406, 24);
			this.defaultServer.TabIndex = 4;
			this.defaultServer.Text = Resource.GetString("DefaultServer");//"Make this my &default server.";
			// 
			// ServerPage
			// 
			this.Controls.Add(this.defaultServer);
			this.Controls.Add(this.serverAddress);
			this.Controls.Add(this.defaultDescription);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.HeaderSubTitle = "";
			this.HeaderTitle = "";
			this.Name = "ServerPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.defaultDescription, 0);
			this.Controls.SetChildIndex(this.serverAddress, 0);
			this.Controls.SetChildIndex(this.defaultServer, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers

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

		internal override void ActivatePage(int previousIndex)
		{
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons.
			serverAddress_TextChanged(this, null);

			serverAddress.Focus();
		}

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

