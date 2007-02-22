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
using System.Xml;
using System.IO;
using Novell.iFolderCom;

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the wizard page where the invitation is accepted or declined.
	/// </summary>
	public class MigrationIdentityPage : Novell.Wizard.MigrationInteriorPageTemplate
	{
		#region Class Members
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox encryptionCB;
		private System.Windows.Forms.CheckBox sslCB;
		private System.Windows.Forms.ComboBox Domains;
		private System.ComponentModel.IContainer components = null;

		private iFolderWebService ifws;
		private DomainItem selectedDomain;
		#endregion

		/// <summary>
		/// Constructs an IdentityPage object.
		/// </summary>
		public MigrationIdentityPage(iFolderWebService ifws)
		{
			// This call is required by the Windows Form Designer.
			this.ifws = ifws;
			InitializeComponent();

		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.Domains = new ComboBox();
			this.encryptionCB = new CheckBox();
			this.sslCB = new CheckBox();			
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(40, 96);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(416, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select the domain";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(80, 122);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(90, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = "Server Address:";

			///
			/// Domains
			/// 

			this.Domains.Location = new Point(170, 122);
			this.Domains.Name = "Domains";
			this.Domains.Size = new Size(260,16);
			this.Domains.SelectedIndexChanged +=new EventHandler(Domains_SelectedIndexChanged);

			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(40, 146);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(416, 16);
			this.label4.TabIndex = 3;
			this.label4.Text = "Select security options:";

			///
			/// encryptionCB
			/// 

			this.encryptionCB.Location = new Point(80, 172);
			this.encryptionCB.Name = "encryptionCB";
			this.encryptionCB.Text = "Encrypt the iFolder";
			this.encryptionCB.Size = new Size(390, 16);

			///
			/// sslCB
			/// 

			this.sslCB.Location = new Point(80, 198);
			this.sslCB.Name = "sslCB";
			this.sslCB.Text = "Use Secure channel for data transfer";
			this.sslCB.Size = new Size(390, 16);
			
			// 
			// IdentityPage
			// 
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.Domains);
			this.Controls.Add(this.encryptionCB);
			this.Controls.Add(this.sslCB);
			this.HeaderSubTitle = "HeaderSubTitle";
			this.HeaderTitle = "HeaderTitle";
			this.Name = "IdentityPage";
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label3, 0);
			//this.Controls.SetChildIndex(this.username, 0);
			this.Controls.SetChildIndex(this.label4, 0);
			this.Load += new EventHandler(MigrationIdentityPage_Load);
			//this.Controls.SetChildIndex(this.password, 0);
			//this.Controls.SetChildIndex(this.rememberPassword, 0);
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		/*
		private void username_TextChanged(object sender, System.EventArgs e)
		{
			// Enable the buttons.
			if (username.Text != "")
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Next | WizardButtons.Back | WizardButtons.Cancel;
			}
			else
			{
				((AccountWizard)this.Parent).WizardButtons = WizardButtons.Back | WizardButtons.Cancel;
			}
		}
	*/
		#endregion

		#region Overridden Methods

		internal override void ActivatePage(int previousIndex)
		{
			((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Next | MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
			base.ActivatePage (previousIndex);

			// Enable/disable the buttons
		//	username_TextChanged( this, null );
		//	username.Focus();
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
			// TODO:
			MigrationWizard wiz = (MigrationWizard)this.Parent;

			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the server address
		/// 

		public string DomainName
		{
			get
			{
				DomainItem domain = (DomainItem) Domains.SelectedItem;
				return domain.Name;
			}
		}
		public DomainItem domain
		{
			get
			{
				return (DomainItem)Domains.SelectedItem;
			}
		}

		public bool Encrypion
		{
			get
			{
				return this.encryptionCB.Checked;
			}
		}

		public bool SSL
		{
			get
			{
				return sslCB.Checked;
			}
		}

		/// <summary>
		/// Gets the value of the password entered.
		/// </summary>
		/*
		public string Password
		{
			get
			{
				return password.Text;
			}
		}

		/// <summary>
		/// Gets a value indicating if the remember password option is checked.
		/// </summary>
		public bool RememberPassword
		{
			get
			{
				return rememberPassword.Checked;
			}
		}

		/// <summary>
		/// Gets the value of the username entered.
		/// </summary>
		public string Username
		{
			get
			{
				return username.Text;
			}
		}
		*/
		#endregion

		private void MigrationIdentityPage_Load(object sender, EventArgs e)
		{
			if (Domains.Items.Count == 0)
			{
				try
				{
					XmlDocument DomainsDoc = new XmlDocument();
					DomainsDoc.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "domain.list"));

					XmlElement element = (XmlElement)DomainsDoc.SelectSingleNode("/domains");

					// Get the ID of the default domain.
					XmlElement defaultDomainElement = (XmlElement)DomainsDoc.SelectSingleNode("/domains/defaultDomain");
					string defaultID = defaultDomainElement.GetAttribute("ID");

					// Get the Domains.
					// Look for a domain with this ID.
					XmlNodeList nodeList = element.GetElementsByTagName("domain");
					foreach (XmlNode node in nodeList)
					{
						string name = ((XmlElement)node).GetAttribute("name");
						string id = ((XmlElement)node).GetAttribute("ID");

						DomainItem domainItem = new DomainItem(name, id);
						Domains.Items.Add(domainItem);

						if (id.Equals(defaultID))
						{
							selectedDomain = domainItem;
						}
					}

					if (selectedDomain != null)
						Domains.SelectedItem = selectedDomain;
					else
						Domains.SelectedIndex = 0;
				}
				catch
				{
				}
			}
			else
			{
				if (selectedDomain != null)
				{
					Domains.SelectedItem = selectedDomain;
				}
				else if (Domains.Items.Count > 0)
				{
					Domains.SelectedIndex = 0;
				}
			}
		}

		private void Domains_SelectedIndexChanged(object sender, EventArgs e)
		{
			DomainItem domain = (DomainItem) Domains.SelectedItem;
			int securityPolicy = ifws.GetSecurityPolicy(domain.ID);
			this.encryptionCB.Checked = this.sslCB.Checked = false;
			this.encryptionCB.Enabled = this.sslCB.Enabled = false;

			if( (securityPolicy & 0x0001) == 0x01)
			{
				if( (securityPolicy & 0x0010) == 0x0010)
					this.encryptionCB.Checked = true;
				else
					this.encryptionCB.Enabled = true;
			}
			if( (securityPolicy & 0x0100) == 0x0100)
			{
				if( (securityPolicy & 0x01000) == 0x01000)
					this.sslCB.Checked = true;
				else
					this.sslCB.Enabled = true;
			}
		}
	}
}

