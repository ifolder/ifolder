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

		private Novell.AddressBook.Manager abManager;
		private Novell.AddressBook.AddressBook defaultAddressBook;
		private iFolder ifolder;
		private ArrayList removedList;
		private System.Windows.Forms.Button reinvite;
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

			this.apply.Enabled = false;
			this.remove.Enabled = false;
			this.reinvite.Enabled = false;

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
			this.reinvite = new System.Windows.Forms.Button();
			this.accessControlButtons = new System.Windows.Forms.GroupBox();
			this.accessReadOnly = new System.Windows.Forms.RadioButton();
			this.accessReadWrite = new System.Windows.Forms.RadioButton();
			this.accessFullControl = new System.Windows.Forms.RadioButton();
			this.add = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.shareWith = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
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
			this.tabSharing.Controls.Add(this.reinvite);
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
			// reinvite
			// 
			this.reinvite.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.helpProvider1.SetHelpString(this.reinvite, "Click to send another invitation to the selected contacts.");
			this.reinvite.Location = new System.Drawing.Point(8, 232);
			this.reinvite.Name = "reinvite";
			this.helpProvider1.SetShowHelp(this.reinvite, true);
			this.reinvite.TabIndex = 1;
			this.reinvite.Text = "R&e-invite";
			this.reinvite.Click += new System.EventHandler(this.reinvite_Click);
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
			this.columnHeader1.Width = 172;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Access";
			this.columnHeader2.Width = 144;
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
		private void ProcessChanges()
		{
			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			foreach (ListViewItem lvitem in this.shareWith.Items)
			{
				ShareListContact slContact = (ShareListContact)lvitem.Tag;

				// If the item is newly added or changed, then process it.
				if (slContact.Added || slContact.Changed)
				{
					// Get the rights for this contact.
					iFolder.Rights rights;
					switch (lvitem.SubItems[1].Text)
					{
						case "Full Control":
						{
							rights = iFolder.Rights.Admin;
							break;
						}
						case "Read/Write":
						{
							rights = iFolder.Rights.ReadWrite;
							break;
						}
						case "Read Only":
						{
							rights = iFolder.Rights.ReadOnly;
							break;
						}
						default:
						{
							rights = iFolder.Rights.Deny;
							break;
						}
					}

					bool accessSet = false;
					try
					{
						// Set the ACE.
						ifolder.SetRights(slContact.CurrentContact, rights);
						accessSet = true;

						// Reset the flags.
						slContact.Added = false;
						slContact.Changed = false;
					}
					catch (SimiasException e)
					{
						e.LogError();
						MessageBox.Show(slContact.CurrentContact.FN + "\nSetting access rights failed with the following exception: \n\n" + e.Message, "Set Rights Failure");
					}
					catch (Exception e)
					{
						// TODO
						logger.Debug(e, "Adding ACE");
						MessageBox.Show(slContact.CurrentContact.FN + "\nSetting access rights failed with the following exception: \n\n" + e.Message, "Set Rights Failure");
					}

					if (accessSet)
					{
						try
						{
							// Send the invitation.
							ifolder.Invite(slContact.CurrentContact);
						}
						catch (SimiasException e)
						{
							e.LogError();
							MessageBox.Show(slContact.CurrentContact.FN + "\nSending invitation failed with the following exception: \n\n" + e.Message, "Send Invitation Failure");
						}
						catch(Exception e)
						{
							// TODO
							logger.Debug(e, "Sending invitation");
							MessageBox.Show(slContact.CurrentContact.FN + "\nSending invitation failed with the following exception: \n\n" + e.Message, "Send Invitation Failure");
						}
					}
				}
			}

			// process the removedList
			if (this.removedList != null)
			{
				foreach (ShareListContact slContact in this.removedList)
				{
					try
					{
						// Remove the ACE and don't send an invitation.
						ifolder.RemoveRights(slContact.CurrentContact);
					}
					catch (SimiasException e)
					{
						e.LogError();
						MessageBox.Show("Remove failed with the following exception: \n\n" + e.Message, "Remove Failure");
					}
					catch (Exception e)
					{
						//TODO
						logger.Debug(e, "Removing ACE");
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
			if (this.shareWith.Items.Count == 1)
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

			return true;
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

				//Assign the ImageList objects to the books ListView.
				shareWith.SmallImageList = contactsImageList;

				pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
				pictureBox1.Image = (Image)SystemIcons.Information.ToBitmap();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading images");
			}

			defaultAddressBook = abManager.OpenDefaultAddressBook();

			// Enable/disable the Add button.
			this.add.Enabled = ifolder.Shareable;

			// Get the access control list for the collection.
			IFAccessControlList aclList = ifolder.GetAccessControlList();
			Contact contact = null;

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;

			foreach (IFAccessControlEntry ace in aclList)
			{
				string[] items = new string[2];
				try
				{
					if (ace.Contact.FN != null && ace.Contact.FN != String.Empty)
					{
						items[0] = ace.Contact.FN;
					}
					else
					{
						items[0] = ace.Contact.UserName;
					}
				}
				catch (Exception ex)
				{
                    logger.Debug(ex, "Unknown user");
					items[0] = "Unknown User";
				}

				int imageIndex;
				switch (ace.Rights)
				{
					case iFolder.Rights.Admin:
					{
						items[1] = "Full Control";
						imageIndex = 3;
						break;
					}
					case iFolder.Rights.ReadWrite:
					{
						items[1] = "Read/Write";
						imageIndex = 2;
						break;
					}
					case iFolder.Rights.ReadOnly:
					{
						items[1] = "Read Only";
						imageIndex = 1;
						break;
					}
					default:
					{
						items[1] = "Unknown";
						imageIndex = 4;
						break;
					}
				}

				try
				{
					if (ace.Contact.IsCurrentUser)
					{
						imageIndex = 0;
					}
				}
				catch{}

				ListViewItem lvitem = new ListViewItem(items, imageIndex);
				ShareListContact shareContact = new ShareListContact();
				shareContact.CurrentContact = ace.Contact;
				lvitem.Tag = shareContact;
				shareWith.Items.Add(lvitem);
			}

			// Select the first item in the list.
			shareWith.Items[0].Selected = true;

			// Restore the cursor.
			Cursor = Cursors.Default;
		}

		private void shareWith_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (shareWith.SelectedItems.Count == 0)
			{
				this.accessControlButtons.Enabled = false;
				this.remove.Enabled = false;
				this.reinvite.Enabled = false;
			}
			else if (shareWith.SelectedItems.Count > 1)
			{
				this.accessControlButtons.Enabled = ifolder.Shareable;
				this.accessReadOnly.Checked = false;
				this.accessReadWrite.Checked = false;
				this.accessFullControl.Checked = false;
			}
			else
			{
				this.reinvite.Enabled = ifolder.Shareable;

				ListViewItem item = shareWith.SelectedItems[0];

				if (ifolder.Shareable)
				{
					try
					{
						// If the current user has sufficient rights, enable the Remove button when
						// a different user is selected in the list.
						this.remove.Enabled = !((ShareListContact)item.Tag).CurrentContact.IsCurrentUser;

						// Enable the rights if the current user is not selected.
						this.accessControlButtons.Enabled = !((ShareListContact)item.Tag).CurrentContact.IsCurrentUser;
					}
					catch
					{
						remove.Enabled = false;
						accessControlButtons.Enabled = false;
						reinvite.Enabled = false;
					}
				}
				else
				{
					this.accessControlButtons.Enabled = false;
				}

				switch (item.SubItems[1].Text)
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
			string access;
			int imageIndex;

			if (this.accessFullControl.Checked)
			{
				access = "Full Control";
				imageIndex = 3;
			}
			else if ( this.accessReadWrite.Checked)
			{
				access = "Read/Write";
				imageIndex = 2;
			}
			else
			{
				access = "Read Only";
				imageIndex = 1;
			}

			foreach (ListViewItem item in this.shareWith.SelectedItems)
			{
				try
				{
					if (((ShareListContact)item.Tag).CurrentContact.IsCurrentUser)
					{
						// Don't allow current user to be modified.
					}
					else if (item.SubItems[1].Text != access)
					{
						// Change the subitem text.
						item.SubItems[1].Text = access;
						item.ImageIndex = imageIndex;

						// Mark this item as changed.
						((ShareListContact)item.Tag).Changed = true;

						// Enable the apply button.
						this.apply.Enabled = true;
					}
				}
				catch{}
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
				foreach (Contact c in contactList)
				{
					// Enable the apply button.
					this.apply.Enabled = true;

					// Initialize a listview item.
					string[] items = new string[2];
					items[0] = c.FN;
					items[1] = "Read/Write";
					ListViewItem lvitem = new ListViewItem(items, 2);

					ShareListContact shareContact = null;

					// Check to see if this contact was originally in the list.
					if (this.removedList != null)
					{
						ShareListContact slContactToRemove = null;

						foreach (ShareListContact slContact in removedList)
						{
							if (c.ID == slContact.CurrentContact.ID)
							{
								// The name may be different and we don't know what the rights used to be,
								// so create a new object to represent this item.
								shareContact = new ShareListContact();//(c, false, true);
								shareContact.CurrentContact = c;
								shareContact.Added = false;
								shareContact.Changed = true;
								slContactToRemove = slContact;
								break;
							}
						}

						if (slContactToRemove != null)
							removedList.Remove(slContactToRemove);
					}

					if (shareContact == null)
					{
						// The contact was not in the removed list, so create a new one.
						shareContact = new ShareListContact();//(c, true);
						shareContact.CurrentContact = c;
						shareContact.Added = true;
					}

					lvitem.Tag = shareContact;

					// Select the contacts that were just added.
					lvitem.Selected = true;

					this.shareWith.Items.Add(lvitem);
				}
			}
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvitem in this.shareWith.SelectedItems)
			{
				try
				{
					// Don't allow the current user to be removed.
					if (!((ShareListContact)lvitem.Tag).CurrentContact.IsCurrentUser)
					{
						// If this item is not newly added, we need to add it to the removedList.
						if (!((ShareListContact)lvitem.Tag).Added)
						{
							// Make sure the removed list is valid.
							if (this.removedList == null)
							{
								this.removedList = new ArrayList();
							}

							removedList.Add(lvitem.Tag);
						}

						lvitem.Remove();

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

		private void reinvite_Click(object sender, System.EventArgs e)
		{
			if (!IsCurrentUserValid())
				return;

			// Change the pointer to an hourglass.
			Cursor = Cursors.WaitCursor;
			
			foreach (ListViewItem lvitem in shareWith.SelectedItems)
			{
				ShareListContact slContact = (ShareListContact)lvitem.Tag;

				// Get the rights for this contact.
				iFolder.Rights rights;
				switch (lvitem.SubItems[1].Text)
				{
					case "Full Control":
					{
						rights = iFolder.Rights.Admin;
						break;
					}
					case "Read/Write":
					{
						rights = iFolder.Rights.ReadWrite;
						break;
					}
					case "Read Only":
					{
						rights = iFolder.Rights.ReadOnly;
						break;
					}
					default:
					{
						rights = iFolder.Rights.Deny;
						break;
					}
				}

				if (slContact.Added || slContact.Changed)
				{
					// If the share contact is newly added or has been changed,
					// we need to reset the ACE.
					bool accessSet = false;
					try
					{
						// Set the ACE.
						ifolder.SetRights(slContact.CurrentContact, rights);
						accessSet = true;

						// Reset the listview item since it has been committed.
						slContact.Added = false;
						slContact.Changed = false;
					}
					catch (SimiasException ex)
					{
						ex.LogError();
						MessageBox.Show(slContact.CurrentContact.FN + "\nSetting access rights failed with the following exception: \n\n" + ex.Message, "Set Access Failure");
					}
					catch (Exception ex)
					{
						// TODO
						logger.Debug(ex, "Setting ACE");
						MessageBox.Show(slContact.CurrentContact.FN + "\nSetting access rights failed with the following exception: \n\n" + ex.Message, "Set Access Failure");
					}

					if (accessSet)
					{
						try
						{
							// Send the invitation.
							ifolder.Invite(slContact.CurrentContact);
						}
						catch (SimiasException ex)
						{
							ex.LogError();
							MessageBox.Show(slContact.CurrentContact.FN + "\nSending invitation failed with the following exception: \n\n" + ex.Message, "Send Invitation Failure");
						}
						catch(Exception ex)
						{
							// TODO
							logger.Debug(ex, "Sending invitation");
							MessageBox.Show(slContact.CurrentContact.FN + "\nSending invitation failed with the following exception: \n\n" + ex.Message, "Send Invitation Failure");
						}
					}
				}
				else
				{
					// Just send the invitation.
					try
					{
						ifolder.Invite(slContact.CurrentContact);
					}
					catch (SimiasException ex)
					{
						ex.LogError();
						MessageBox.Show(lvitem.Text + "\nSending invitation failed with the following exception: \n\n" + ex.Message, "Send Invitation Failure");
					}
					catch(Exception ex)
					{
						// TODO
						logger.Debug(ex, "Sending invitation");
						MessageBox.Show(lvitem.Text + "\nSending invitation failed with the following exception: \n\n" + ex.Message, "Send Invitation Failure");
					}
				}
			}		

			// Disable the apply button.
			this.apply.Enabled = false;

			// Restore the cursor.
			Cursor = Cursors.Default;
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
	public class ShareListContact
	{
		private Contact contact;
		private bool added = false;
		private bool changed = false;

		#region Constructors
		public ShareListContact()
		{
		}

/*		public ShareListContact(Contact contact)
		{
			this.contact = contact;
		}

		public ShareListContact(Contact contact, bool added)
		{
			this.contact = contact;
			this.added = added;
		}

		public ShareListContact(Contact contact, bool added, bool changed)
		{
			this.contact = contact;
			this.added = added;
			this.changed = changed;
		}*/
		#endregion

		#region Properties
		/// <summary>
		/// Gets and Sets the Added flag.
		/// </summary>
		public bool Added
		{
			get
			{
				return this.added;
			}
			set
			{
				this.added = value;
			}
		}

		/// <summary>
		/// Gets and Sets the Changed flag.
		/// </summary>
		public bool Changed
		{
			get
			{
				return this.changed;
			}
			set
			{
				this.changed = value;
			}
		}

		/// <summary>
		/// Gets and Sets the current contact.
		/// </summary>
		public Contact CurrentContact
		{
			get
			{
				return this.contact;
			}
			set
			{
				this.contact = value;
			}
		}
		#endregion
	}
}
