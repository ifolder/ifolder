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
using System.Runtime.InteropServices;
using System.IO;
using Novell.iFolder;
using Novell.AddressBook;
using Simias;
using Simias.Storage;
using Simias.Sync;
using Simias.POBox;
using Novell.iFolder.FormsBookLib;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// iFolder Advanced dialog.
	/// </summary>
	[ComVisible(false)]
	public class iFolderAdvanced : System.Windows.Forms.Form
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(iFolderAdvanced));
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Button apply;
		private System.Windows.Forms.ListView shareWith;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button remove;
		private System.Windows.Forms.Button add;
		private System.Windows.Forms.GroupBox accessControlButtons;
		private System.Windows.Forms.RadioButton accessFullControl;
		private System.Windows.Forms.RadioButton accessReadWrite;
		private System.Windows.Forms.RadioButton accessReadOnly;

		private POBox poBox;
		private Novell.AddressBook.Manager abManager;
		private Novell.AddressBook.AddressBook defaultAddressBook;
		private iFolder ifolder;
		private ArrayList removedList;
		private string loadPath;
//		private Control currentControl;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown syncInterval;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox autoSync;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Label objectCount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label byteCount;
		private System.Windows.Forms.LinkLabel conflicts;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TabPage tabSharing;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Button accept;
		private System.Windows.Forms.Button decline;
		private System.ComponentModel.IContainer components;
		#endregion

		public iFolderAdvanced()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Connect to the address book manager
			if (abManager == null)
			{
				abManager = Novell.AddressBook.Manager.Connect();
			}

			apply.Enabled = remove.Enabled = accept.Enabled = decline.Enabled = false;

			syncInterval.TextChanged += new EventHandler(syncInterval_ValueChanged);
//			currentControl = this;
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
			this.components = new System.ComponentModel.Container();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.conflicts = new System.Windows.Forms.LinkLabel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.syncInterval = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.autoSync = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.objectCount = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.byteCount = new System.Windows.Forms.Label();
			this.tabSharing = new System.Windows.Forms.TabPage();
			this.decline = new System.Windows.Forms.Button();
			this.accept = new System.Windows.Forms.Button();
			this.accessControlButtons = new System.Windows.Forms.GroupBox();
			this.accessReadOnly = new System.Windows.Forms.RadioButton();
			this.accessReadWrite = new System.Windows.Forms.RadioButton();
			this.accessFullControl = new System.Windows.Forms.RadioButton();
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.shareWith = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.apply = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			this.tabControl1.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.syncInterval)).BeginInit();
			this.groupBox4.SuspendLayout();
			this.tabSharing.SuspendLayout();
			this.accessControlButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabGeneral);
			this.tabControl1.Controls.Add(this.tabSharing);
			this.tabControl1.Location = new System.Drawing.Point(8, 16);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(344, 400);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabControl1_KeyDown);
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabGeneral
			// 
			this.tabGeneral.Controls.Add(this.conflicts);
			this.tabGeneral.Controls.Add(this.pictureBox1);
			this.tabGeneral.Controls.Add(this.groupBox1);
			this.tabGeneral.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(336, 374);
			this.tabGeneral.TabIndex = 1;
			this.tabGeneral.Text = "General";
			// 
			// conflicts
			// 
			this.conflicts.LinkArea = new System.Windows.Forms.LinkArea(44, 10);
			this.conflicts.Location = new System.Drawing.Point(44, 296);
			this.conflicts.Name = "conflicts";
			this.conflicts.Size = new System.Drawing.Size(280, 32);
			this.conflicts.TabIndex = 6;
			this.conflicts.TabStop = true;
			this.conflicts.Text = "This iFolder currently contains conflicts.  Click here to resolve the conflicts.";
			this.conflicts.Visible = false;
			this.conflicts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.conflicts_LinkClicked);
			// 
			// pictureBox1
			// 
			this.pictureBox1.Location = new System.Drawing.Point(12, 296);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(24, 24);
			this.pictureBox1.TabIndex = 7;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Visible = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.autoSync);
			this.groupBox1.Controls.Add(this.groupBox4);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(322, 256);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Synchronization";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.syncInterval);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(16, 48);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(296, 112);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Synchronize to host";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(16, 24);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(264, 48);
			this.label5.TabIndex = 0;
			this.label5.Text = "This value sets the default value for how often the host of an iFolder will be co" +
				"ntacted  to sync files.";
			// 
			// syncInterval
			// 
			this.syncInterval.Increment = new System.Decimal(new int[] {
																		   5,
																		   0,
																		   0,
																		   0});
			this.syncInterval.Location = new System.Drawing.Point(168, 80);
			this.syncInterval.Maximum = new System.Decimal(new int[] {
																		 86400,
																		 0,
																		 0,
																		 0});
			this.syncInterval.Minimum = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 -2147483648});
			this.syncInterval.Name = "syncInterval";
			this.syncInterval.Size = new System.Drawing.Size(64, 20);
			this.syncInterval.TabIndex = 2;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(240, 80);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(48, 16);
			this.label6.TabIndex = 3;
			this.label6.Text = "seconds";
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(140, 16);
			this.label7.TabIndex = 1;
			this.label7.Text = "Sync to host every:";
			// 
			// autoSync
			// 
			this.autoSync.Checked = true;
			this.autoSync.CheckState = System.Windows.Forms.CheckState.Checked;
			this.autoSync.Enabled = false;
			this.autoSync.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoSync.Location = new System.Drawing.Point(16, 24);
			this.autoSync.Name = "autoSync";
			this.autoSync.Size = new System.Drawing.Size(296, 16);
			this.autoSync.TabIndex = 0;
			this.autoSync.Text = "Automatic sync";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.objectCount);
			this.groupBox4.Controls.Add(this.label2);
			this.groupBox4.Controls.Add(this.label8);
			this.groupBox4.Controls.Add(this.byteCount);
			this.groupBox4.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox4.Location = new System.Drawing.Point(16, 168);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(296, 72);
			this.groupBox4.TabIndex = 9;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Statistics";
			// 
			// objectCount
			// 
			this.objectCount.Location = new System.Drawing.Point(208, 48);
			this.objectCount.Name = "objectCount";
			this.objectCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.objectCount.Size = new System.Drawing.Size(80, 16);
			this.objectCount.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Amount to upload:";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 48);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(184, 16);
			this.label8.TabIndex = 1;
			this.label8.Text = "Files/folders to synchronize:";
			// 
			// byteCount
			// 
			this.byteCount.Location = new System.Drawing.Point(208, 24);
			this.byteCount.Name = "byteCount";
			this.byteCount.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.byteCount.Size = new System.Drawing.Size(80, 16);
			this.byteCount.TabIndex = 7;
			// 
			// tabSharing
			// 
			this.tabSharing.Controls.Add(this.decline);
			this.tabSharing.Controls.Add(this.accept);
			this.tabSharing.Controls.Add(this.accessControlButtons);
			this.tabSharing.Controls.Add(this.add);
			this.tabSharing.Controls.Add(this.remove);
			this.tabSharing.Controls.Add(this.shareWith);
			this.tabSharing.Location = new System.Drawing.Point(4, 22);
			this.tabSharing.Name = "tabSharing";
			this.tabSharing.Size = new System.Drawing.Size(336, 374);
			this.tabSharing.TabIndex = 0;
			this.tabSharing.Text = "Sharing";
			// 
			// decline
			// 
			this.decline.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.decline.Location = new System.Drawing.Point(88, 232);
			this.decline.Name = "decline";
			this.decline.TabIndex = 5;
			this.decline.Text = "&Decline";
			this.decline.Click += new System.EventHandler(this.decline_Click);
			// 
			// accept
			// 
			this.accept.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.accept, "Click to send another invitation to the selected contacts.");
			this.accept.Location = new System.Drawing.Point(8, 232);
			this.accept.Name = "accept";
			this.helpProvider1.SetShowHelp(this.accept, true);
			this.accept.TabIndex = 1;
			this.accept.Text = "A&ccept";
			this.accept.Click += new System.EventHandler(this.accept_Click);
			// 
			// accessControlButtons
			// 
			this.accessControlButtons.Controls.Add(this.accessReadOnly);
			this.accessControlButtons.Controls.Add(this.accessReadWrite);
			this.accessControlButtons.Controls.Add(this.accessFullControl);
			this.accessControlButtons.Location = new System.Drawing.Point(8, 264);
			this.accessControlButtons.Name = "accessControlButtons";
			this.accessControlButtons.Size = new System.Drawing.Size(320, 100);
			this.accessControlButtons.TabIndex = 4;
			this.accessControlButtons.TabStop = false;
			this.accessControlButtons.Text = "Access";
			// 
			// accessReadOnly
			// 
			this.accessReadOnly.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.accessReadOnly, "Grants read only permission to the iFolder.");
			this.accessReadOnly.Location = new System.Drawing.Point(24, 72);
			this.accessReadOnly.Name = "accessReadOnly";
			this.helpProvider1.SetShowHelp(this.accessReadOnly, true);
			this.accessReadOnly.Size = new System.Drawing.Size(280, 16);
			this.accessReadOnly.TabIndex = 2;
			this.accessReadOnly.Text = "Read &Only";
			this.accessReadOnly.Click += new System.EventHandler(this.accessButton_Click);
			// 
			// accessReadWrite
			// 
			this.accessReadWrite.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.accessReadWrite, "Grants read/write permission to the iFolder.");
			this.accessReadWrite.Location = new System.Drawing.Point(24, 48);
			this.accessReadWrite.Name = "accessReadWrite";
			this.helpProvider1.SetShowHelp(this.accessReadWrite, true);
			this.accessReadWrite.Size = new System.Drawing.Size(280, 16);
			this.accessReadWrite.TabIndex = 1;
			this.accessReadWrite.Text = "Read/&Write";
			this.accessReadWrite.Click += new System.EventHandler(this.accessButton_Click);
			// 
			// accessFullControl
			// 
			this.accessFullControl.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.accessFullControl, "Grants read/write permission to the iFolder and allows a user to share this iFold" +
				"er with other users.");
			this.accessFullControl.Location = new System.Drawing.Point(24, 24);
			this.accessFullControl.Name = "accessFullControl";
			this.helpProvider1.SetShowHelp(this.accessFullControl, true);
			this.accessFullControl.Size = new System.Drawing.Size(280, 16);
			this.accessFullControl.TabIndex = 0;
			this.accessFullControl.Text = "&Full Control";
			this.accessFullControl.Click += new System.EventHandler(this.accessButton_Click);
			// 
			// add
			// 
			this.add.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.add, "Click to add contacts to share this iFolder with.");
			this.add.Location = new System.Drawing.Point(176, 232);
			this.add.Name = "add";
			this.helpProvider1.SetShowHelp(this.add, true);
			this.add.TabIndex = 2;
			this.add.Text = "A&dd...";
			this.add.Click += new System.EventHandler(this.add_Click);
			// 
			// remove
			// 
			this.remove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.remove, "Click to stop sharing this iFolder with the selected contact(s).");
			this.remove.Location = new System.Drawing.Point(256, 232);
			this.remove.Name = "remove";
			this.helpProvider1.SetShowHelp(this.remove, true);
			this.remove.TabIndex = 3;
			this.remove.Text = "&Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// shareWith
			// 
			this.shareWith.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1,
																						this.columnHeader3,
																						this.columnHeader2});
			this.helpProvider1.SetHelpString(this.shareWith, "Lists the contacts that this iFolder is currently being shared with.");
			this.shareWith.HideSelection = false;
			this.shareWith.Location = new System.Drawing.Point(8, 8);
			this.shareWith.Name = "shareWith";
			this.helpProvider1.SetShowHelp(this.shareWith, true);
			this.shareWith.Size = new System.Drawing.Size(320, 216);
			this.shareWith.TabIndex = 0;
			this.shareWith.View = System.Windows.Forms.View.Details;
			this.shareWith.SelectedIndexChanged += new System.EventHandler(this.shareWith_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Share with";
			this.columnHeader1.Width = 75;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Status";
			this.columnHeader3.Width = 171;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Access";
			this.columnHeader2.Width = 70;
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ok.Location = new System.Drawing.Point(112, 432);
			this.ok.Name = "ok";
			this.ok.TabIndex = 1;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancel.Location = new System.Drawing.Point(192, 432);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 2;
			this.cancel.Text = "Cancel";
			this.cancel.Click += new System.EventHandler(this.cancel_Click);
			// 
			// apply
			// 
			this.apply.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.apply.Location = new System.Drawing.Point(272, 432);
			this.apply.Name = "apply";
			this.apply.TabIndex = 3;
			this.apply.Text = "&Apply";
			this.apply.Click += new System.EventHandler(this.apply_Click);
			// 
			// iFolderAdvanced
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(360, 462);
			this.Controls.Add(this.apply);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "iFolderAdvanced";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.iFolderAdvanced_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.syncInterval)).EndInit();
			this.groupBox4.ResumeLayout(false);
			this.tabSharing.ResumeLayout(false);
			this.accessControlButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private string rightsToString(Access.Rights rights, out int imageIndex)
		{
			string rightsString = null;

			switch (rights)
			{
				case Access.Rights.Admin:
				{
					rightsString = "Full Control";
					imageIndex = 3;
					break;
				}
				case Access.Rights.ReadWrite:
				{
					rightsString = "Read/Write";
					imageIndex = 2;
					break;
				}
				case Access.Rights.ReadOnly:
				{
					rightsString = "Read Only";
					imageIndex = 1;
					break;
				}
				default:
				{
					rightsString = "Unknown";
					imageIndex = 4;
					break;
				}
			}

			return rightsString;
		}

		private Access.Rights stringToRights(string rightsString)
		{
			Access.Rights rights = Access.Rights.Deny;

			switch (rightsString)
			{
				case "Full Control":
				{
					rights = Access.Rights.Admin;
					break;
				}
				case "Read/Write":
				{
					rights = Access.Rights.ReadWrite;
					break;
				}
				case "Read Only":
				{
					rights = Access.Rights.ReadOnly;
					break;
				}
			}

			return rights;
		}

		private void ProcessChanges()
		{
			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			string sendersEmail = null;

			// Get the poBox for the current user.
			poBox = POBox.GetPOBox(ifolder.StoreReference, ifolder.StoreReference.DefaultDomain);

			foreach (ListViewItem lvitem in this.shareWith.Items)
			{
				ShareListMember slMember = (ShareListMember)lvitem.Tag;

				// Process added and changed members.
				if (slMember.Added)
				{
					// TODO: we'll get the email a different way in the future.
					if (ifolder.Domain.Equals(Domain.WorkGroupDomainID) &&
						(sendersEmail == null))
					{
						// TODO: check for an existing contact for the current user.
						EmailPrompt emailPrompt = new EmailPrompt();
						if (DialogResult.OK == emailPrompt.ShowDialog())
						{
							sendersEmail = emailPrompt.Email;
						}
					}

					// TODO: change this to use an array and add them all at once.
					Subscription subscr = poBox.CreateSubscription(ifolder, ifolder.GetCurrentMember(), typeof(iFolder).Name);

					// Add all of the other properties (ToAddress, FromAddress, etc.)
					subscr.FromAddress = sendersEmail;
					subscr.SubscriptionRights = slMember.Member.Rights;
					subscr.ToName = slMember.Member.Name;
					subscr.SubscriptionCollectionName = ifolder.Name;

					// Take the relationship off the ShareListMember and put it on the Subscription object.  This will
					// be used to hook up the Member and Contact objects after the subscription has been accepted.
					Property property = slMember.Member.Properties.GetSingleProperty("Contact");
					property.LocalProperty = true;
					subscr.Properties.AddProperty(property);
					Relationship relationship = (Relationship)property.Value;

					// Get the contact so that we can get the e-mail address from it.
					Novell.AddressBook.AddressBook ab = abManager.GetAddressBook(relationship.CollectionID);
					Contact contact = ab.GetContact(relationship.NodeID);
					subscr.ToAddress = contact.EMail;
					subscr.ToIdentity = contact.UserID;
					
					// Put the subscription in the POBox.
					poBox.AddMessage(subscr);

					// Update the state.
					slMember.Added = false;
				}
				else if (slMember.Changed)
				{
					// TODO: may need to save rights on subscription objects in the future.

					// Get the rights for this contact.
					slMember.Member.Rights = stringToRights(lvitem.SubItems[2].Text);
					ifolder.Commit(slMember.Member);

					// Reset the flags.
					slMember.Changed = false;
				}
			}

			// process the removedList
			if (this.removedList != null)
			{
				foreach (ShareListMember slMember in removedList)
				{
					try
					{
						if (slMember.IsMember)
						{
							// Delete the member.
							ifolder.Commit(ifolder.Delete(slMember.Member));
						}
						else
						{
							// Delete the subscription.
							poBox.Commit(poBox.Delete(slMember.Subscription));
						}
					}
					catch (SimiasException e)
					{
						e.LogError();
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
					catch (Exception e)
					{
						//TODO
						logger.Debug(e, "Removing member");
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
				}

				// Clear the list.
				removedList.Clear();
			}

			// Update the refresh interval.
			ifolder.RefreshInterval = (int)syncInterval.Value;

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private bool IsCurrentUserValid()
		{
			// TODO - may need to actually check the current user.
/*			if (this.shareWith.Items.Count == 1)
			{
				try
				{
					Contact currentContact = ((ShareListContact)this.shareWith.Items[0].Tag).CurrentContact;
					if (currentContact.FN == null ||
						currentContact.FN == String.Empty ||
						currentContact.EMail == null ||
						currentContact.EMail == String.Empty)
					{
						MyMessageBox mmb = new MyMessageBox();
						mmb.Text = "Incomplete Address Book Entry";
						mmb.Message = "Before you can share, you must add some data to your address book entry.  Do you want to add this information now?";
						mmb.MessageIcon = SystemIcons.Question.ToBitmap();
						DialogResult result = mmb.ShowDialog();
						//MessageBox.Show(this, , , MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
						if (result == DialogResult.Yes)
						{
							ContactEditor editor = new ContactEditor();
							editor.LoadPath = LoadPath;
							editor.CurrentContact = currentContact;
							editor.CurrentAddressBook = defaultAddressBook;
							if (editor.ShowDialog() != DialogResult.OK)
							{
								return false;
							}
						}
						else
						{
							return false;
						}
					}
				}
				catch
				{
					return false;
				}
			}
*/
			return true;
		}

		private void updateListViewItem(ListViewItem lvi, Access.Rights rights)
		{
			ShareListMember slMember = (ShareListMember)lvi.Tag;

			int imageIndex;
			string access = rightsToString(rights, out imageIndex);

			try
			{
				if (ifolder.GetCurrentMember().UserID == slMember.Member.UserID)
				{
					// Don't allow current user to be modified.
				}
				else
				{
					if (lvi.SubItems[2].Text != access)
					{
						// Mark this item as changed.
						slMember.Changed = true;

						// Change the subitem text.
						lvi.SubItems[2].Text = access;

						// Enable the apply button.
						this.apply.Enabled = true;
					}

					// Don't change the image if this item is not a member.
					if (slMember.IsMember)
					{
						lvi.ImageIndex = imageIndex;
						lvi.SubItems[1].Text = slMember.Member.IsOwner ? "Owner" : "";
					}
				}
			}
			catch{}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the current iFolder.
		/// </summary>
		public iFolder CurrentiFolder
		{
			set
			{
				this.ifolder = value;
			}
		}

		/// <summary>
		/// The path where the DLL is running from.
		/// </summary>
		public string LoadPath
		{
			get
			{
				return loadPath;
			}

			set
			{
				this.loadPath = value;
			}
		}

		public string ActiveTab
		{
			set
			{
				if (value != null)
				{
					switch (value)
					{
						case "share":
							tabControl1.SelectedTab = tabSharing;
							break;
						default:
							tabControl1.SelectedTab = tabGeneral;
							break;
					}
				}
				else
				{
					tabControl1.SelectedTab = tabGeneral;
				}
			}
		}
		#endregion

		#region Event Handlers
		private void iFolderAdvanced_Load(object sender, EventArgs e)
		{
			// TODO - use locale-specific path.
//			helpProvider1.HelpNamespace = Path.Combine(loadPath, @"help\en\doc\user\data\front.html");

			try
			{
				// Get the refresh interval.
				syncInterval.Value = (decimal)ifolder.RefreshInterval;
				Cursor.Current = Cursors.WaitCursor;

				// Show/hide the collision message.
				conflicts.Visible = pictureBox1.Visible = ifolder.HasCollisions();

				// Get the sync node and byte counts.
				uint nodeCount;
				ulong bytesToSend;
				SyncSize.CalculateSendSize(ifolder, out nodeCount, out bytesToSend);
				objectCount.Text = nodeCount.ToString();
				byteCount.Text = bytesToSend.ToString();

				Cursor.Current = Cursors.Default;
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "SyncSize.CalculateSendSize");
			}

			// Image list...
			try
			{
				// Create the ImageList object.
				ImageList contactsImageList = new ImageList();

				// Initialize the ImageList objects with icons.
				string basePath = loadPath != null ? Path.Combine(loadPath, "res") : Path.Combine(Application.StartupPath, "res");
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_me_card.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_read.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_read_write.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_full.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "ifolder_contact_card.ico")));

				// TODO: These are icons are temporary...
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "mail_closed.ico")));
				contactsImageList.Images.Add(new Icon(Path.Combine(basePath, "mail_opened.ico")));

				//Assign the ImageList objects to the books ListView.
				shareWith.SmallImageList = contactsImageList;

				pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
				pictureBox1.Image = (Image)SystemIcons.Information.ToBitmap();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading images");
			}

//			defaultAddressBook = abManager.OpenDefaultAddressBook();

			// Enable/disable the Add button.
//			this.add.Enabled = ifolder.Shareable;

			// Get the access control list for the collection.
//			IFAccessControlList aclList = ifolder.GetAccessControlList();

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;
			shareWith.BeginUpdate();

			try
			{
				poBox = POBox.GetPOBox(ifolder.StoreReference, ifolder.StoreReference.DefaultDomain);
				ICSList memberList = ifolder.GetMemberList();

				// Load the member list.
				foreach (ShallowNode shallowNode in memberList)
				{
					// TODO: We may want to reconstitute only when necessary ... for example, when the item comes into view
					Member member = new Member(ifolder, shallowNode);

					string[] items = new string[3];

					Contact contact = abManager.GetContact(member);
					if (contact != null)
					{
						items[0] = contact.FN;
					}
					else
					{
						items[0] = member.Name;
					}

					items[1] = member.IsOwner ? "Owner" : "";

					int imageIndex;

					// TODO: fix this to use rightsToString ... and maybe change the image index to line up with the rights enum.
					items[2] = rightsToString(member.Rights, out imageIndex);

					if (ifolder.GetCurrentMember().UserID == member.UserID)
					{
						imageIndex = 0;
					}

					ListViewItem lvitem = new ListViewItem(items, imageIndex);
					ShareListMember shareMember = new ShareListMember();
					shareMember.Member = member;
					shareMember.IsMember = true;
					lvitem.Tag = shareMember;

					shareWith.Items.Add(lvitem);
				}

				// TODO: Load the stuff from the POBox.
				ICSList messageList = poBox.Search(Subscription.SubscriptionCollectionIDProperty, ifolder.ID, SearchOp.Equal);
				foreach (ShallowNode shallowNode in messageList)
				{
					Subscription sub = new Subscription(poBox, shallowNode);
					ShareListMember shareMember = new ShareListMember();
					shareMember.Member = new Member(sub.ToName, Guid.NewGuid().ToString(), sub.SubscriptionRights);
					shareMember.Subscription = sub;

					string[] items = new string[3];
					items[0] = sub.ToName;
					items[1] = sub.SubscriptionState.ToString();
					int imageIndex;
					items[2] = rightsToString(sub.SubscriptionRights, out imageIndex);
					
					ListViewItem lvi = new ListViewItem(items, 5);
					lvi.Tag = shareMember;

					shareWith.Items.Add(lvi);
				}
			}
			catch (SimiasException ex)
			{
			}
			catch (Exception ex)
			{
			}

			// Select the first item in the list.
			shareWith.Items[0].Selected = true;

			shareWith.EndUpdate();

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private void shareWith_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ListViewItem lvi = null;

			if (shareWith.SelectedItems.Count == 1)
			{
				lvi = shareWith.SelectedItems[0];
				accept.Enabled = decline.Enabled = lvi.SubItems[1].Text.Equals(SubscriptionStates.Pending.ToString());
			}
			else
			{
				this.accessReadOnly.Checked = false;
				this.accessReadWrite.Checked = false;
				this.accessFullControl.Checked = false;
				accept.Enabled = decline.Enabled = false;
			}

			if (ifolder.GetCurrentMember().Rights == Access.Rights.Admin)
			{
				accessControlButtons.Enabled = shareWith.SelectedItems.Count != 0;

				remove.Enabled = true;

			}
			else
			{
				accessControlButtons.Enabled = add.Enabled = remove.Enabled = false;
			}

			if (lvi != null)
			{
				switch (lvi.SubItems[2].Text)
				{
					case "Full Control":
					{
						this.accessFullControl.Checked = true;
						break;
					}
					case "Read/Write":
					{
						this.accessReadWrite.Checked = true;
						break;
					}
					case "Read Only":
					{
						this.accessReadOnly.Checked = true;
						break;
					}
					default:
					{
						this.accessReadOnly.Checked = false;
						this.accessReadWrite.Checked = false;
						this.accessFullControl.Checked = false;
						break;
					}
				}
			}
		}

		private void accessButton_Click(object sender, EventArgs e)
		{
			Access.Rights rights;

			if (accessFullControl.Checked)
			{
				rights = Access.Rights.Admin;
			}
			else if (accessReadWrite.Checked)
			{
				rights = Access.Rights.ReadWrite;
			}
			else
			{
				rights = Access.Rights.ReadOnly;
			}

			foreach (ListViewItem lvi in shareWith.SelectedItems)
			{
				updateListViewItem(lvi, rights);
			}
		}

		private void add_Click(object sender, System.EventArgs e)
		{
			if (!IsCurrentUserValid())
				return;

			// TODO - Initialize the picker with the names that are already in the share list.
			ContactPicker picker = new ContactPicker();
			picker.CurrentManager = abManager;
			picker.LoadPath = loadPath;
			DialogResult result = picker.ShowDialog();
			if (result == DialogResult.OK)
			{
				// Unselect all items.
				shareWith.SelectedItems.Clear();

				// Get the list of added items from the picker.
				ArrayList contactList = picker.GetContactList;
				// Enable the apply button.
				if (contactList.Count > 0)
					this.apply.Enabled = true;

				foreach (Contact c in contactList)
				{
					// Initialize a listview item.
					string[] items = new string[3];
					items[0] = c.FN;
					items[1] = "Inviting";
					items[2] = "Read/Write";
					ListViewItem lvitem = new ListViewItem(items, 5);

					ShareListMember shareMember = null;

					// Check to see if this contact was originally in the list.
/*TODO:					if (this.removedList != null)
					{
						ShareListMember slMemberToRemove = null;

						foreach (ShareListMember slMember in removedList)
						{
							if (c.ID == slContact.CurrentContact.ID)
							{
								// The name may be different and we don't know what the rights used to be,
								// so create a new object to represent this item.
								shareMember = new ShareListContact();//(c, false, true);
								shareMember.CurrentContact = c;
								shareMember.IsMember = slMember.IsMember;
								shareMember.Added = false;
								shareMember.Changed = true;
								slMemberToRemove = slMember;
								break;
							}
						}

						if (slMemberToRemove != null)
							removedList.Remove(slMemberToRemove);
					}*/

					if (shareMember == null)
					{
						// TODO: this needs to be reworked after BETA1 ... for now we will only allow them to choose a contact that is linked to a member.
						if ((!ifolder.Domain.Equals(Domain.WorkGroupDomainID)) &&
							((c.UserID == null) || (c.UserID.Length == 0)))
						{
							MessageBox.Show("Unable to share with the following incomplete user:\n\n" + c.FN, "Share Failure");
						}
						else
						{
							// The contact was not in the removed list, so create a new one.
							shareMember = new ShareListMember();

							Member member;
							
							if (ifolder.Domain.Equals(Domain.WorkGroupDomainID))
							{
								// Create a place-holder member.
								member = new Member(c.FN, Guid.NewGuid().ToString(), Access.Rights.ReadWrite);

								// Create a relationship on the member ... this will be put on the Subscription object later on.
								string collectionId = (string)c.Properties.GetSingleProperty(BaseSchema.CollectionId).Value;
								member.Properties.AddProperty("Contact", new Relationship(collectionId, c.ID));
							}
							else
							{
								Novell.AddressBook.AddressBook ab = abManager.GetAddressBook(c.Properties.GetSingleProperty(BaseSchema.CollectionId).ToString());
								member = ab.GetMemberByID(c.UserID);
							}

							shareMember.Member = member;
							shareMember.Added = true;
						}
					}

					// If we have a valid shareMember, then add it to the listview.
					if (shareMember != null)
					{
						lvitem.Tag = shareMember;

						// Select the contacts that were just added.
						lvitem.Selected = true;

						shareWith.Items.Add(lvitem);
					}
				}
			}
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			// TODO: make this a member variable.
			string currentUser = ifolder.GetCurrentMember().UserID;

			foreach (ListViewItem lvi in shareWith.SelectedItems)
			{
				ShareListMember slMember = (ShareListMember)lvi.Tag;

				try
				{
					// Don't allow the current user to be removed.
					if (!currentUser.Equals(slMember.Member.UserID))
					{
						// If this item is not newly added, we need to add it to the removedList.
						if (!slMember.Added)
						{
							// Make sure the removed list is valid.
							if (this.removedList == null)
							{
								this.removedList = new ArrayList();
							}

							// Add this to the removed list.
							removedList.Add(slMember);
						}

						// Remove the item from the listview.
						lvi.Remove();

						// Enable the apply button.
						this.apply.Enabled = true;
					}
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Removing contacts");
				}
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			this.ProcessChanges();
			this.Close();
		}

		private void accept_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = this.shareWith.SelectedItems[0];
			ShareListMember slMember = (ShareListMember)lvi.Tag;
			slMember.Member = slMember.Subscription.Accept(ifolder.StoreReference, this.stringToRights(lvi.SubItems[2].Text));

			// Take the relationship off the Subscription object
			Property property = slMember.Subscription.Properties.GetSingleProperty("Contact");
			if (property != null)
			{
				Relationship relationship = (Relationship)property.Value;

				// Get the contact from the relationship.
				Novell.AddressBook.AddressBook ab = this.abManager.GetAddressBook(relationship.CollectionID);
				Contact contact = ab.GetContact(relationship.NodeID);

				// Put the Member userID into the Contact userID.
				contact.UserID = slMember.Member.UserID;
				ab.Commit(contact);
			}

			poBox.Commit(slMember.Subscription);
			
			// This entry is now a member.
			slMember.IsMember = true;

			updateListViewItem(lvi, slMember.Member.Rights);
		}

		private void decline_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = this.shareWith.SelectedItems[0];
			ShareListMember slMember = (ShareListMember)lvi.Tag;
			slMember.Subscription.Decline();
			poBox.Commit(slMember.Subscription);
			lvi.Remove();
		}

		private void apply_Click(object sender, System.EventArgs e)
		{
			this.ProcessChanges();
		
			// Disable the apply button.
			this.apply.Enabled = false;
		}

		private void cancel_Click(object sender, System.EventArgs e)
		{
			this.Close();	
		}

		private void syncInterval_ValueChanged(object sender, System.EventArgs e)
		{
			// Enable the apply button if the user changed the interval.
			if (syncInterval.Focused)
				apply.Enabled = true;		
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (tabControl1.SelectedTab == tabGeneral)
			{
				Cursor.Current = Cursors.WaitCursor;

				try
				{
					conflicts.Visible = pictureBox1.Visible = ifolder.HasCollisions();
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "HasCollisions");
				}

				Cursor.Current = Cursors.Default;
			}
		}

		private void conflicts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ConflictResolver conflictResolver = new ConflictResolver();
			conflictResolver.IFolder = this.ifolder;
			conflictResolver.LoadPath = loadPath;
			conflictResolver.ConflictsResolved += new Novell.iFolder.iFolderCom.ConflictResolver.ConflictsResolvedDelegate(conflictResolver_ConflictsResolved);
			conflictResolver.Show();		
		}

		private void conflictResolver_ConflictsResolved(object sender, EventArgs e)
		{
			conflicts.Visible = pictureBox1.Visible = false;
		}
		#endregion

		private void tabControl1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			// TODO - change focus when dialog is displayed non-modal
//			if (!this.Modal && e.KeyCode == Keys.Tab)
//			{
//				currentControl = this.GetNextControl(this, true);
//				bool focus = currentControl.Focus();// add.Focus();
//				focus = !focus;
//			}
		}
	}

	[ComVisible(false)]
	public class ShareListMember
	{
		private Subscription subscription;
		private Member member;
		private bool added = false;
		private bool changed = false;
		private bool isMember = false;

		#region Constructors
		public ShareListMember()
		{
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets and Sets the Added flag.
		/// </summary>
		public bool Added
		{
			get { return added; }
			set { added = value; }
		}

		/// <summary>
		/// Gets and Sets the Changed flag.
		/// </summary>
		public bool Changed
		{
			get { return changed; }
			set { changed = value; }
		}

		/// <summary>
		/// Gets and Sets the current contact.
		/// </summary>
		public Member Member
		{
			get { return member; }
			set { member = value; }
		}

		public bool IsMember
		{
			get { return isMember; }
			set { isMember = value; }
		}

		public Subscription Subscription
		{
			get { return subscription; }
			set { subscription = value; }
		}
		#endregion
	}
}
