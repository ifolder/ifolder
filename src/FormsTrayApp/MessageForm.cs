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
using System.IO;
using System.Diagnostics;
using Simias;
using Simias.Storage;
using Simias.POBox;
using Novell.AddressBook;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for MessageForm.
	/// </summary>
	public class MessageForm : System.Windows.Forms.Form
	{
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(MessageForm));
		private POBox poBox = null;
		private Store store = null;
		private Configuration config;
		private Manager abManager;
		private EventSubscriber subscriber;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ListView messages;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button accept;
		private System.Windows.Forms.Button decline;
		private System.Windows.Forms.Button remove;
		private Hashtable ht;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MessageForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			try
			{
				config = new Configuration();
				abManager = Manager.Connect(config);

				store = new Store(config);

				// TODO: pass in correct domain ... for now just use the default.
				poBox = POBox.GetPOBox(store, store.DefaultDomain);
			}
			catch (SimiasException e)
			{
				e.LogFatal();
			}
			catch (Exception e)
			{
				logger.Fatal(e, "Initializing");
			}

			ht = new Hashtable();
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
			this.messages = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.accept = new System.Windows.Forms.Button();
			this.decline = new System.Windows.Forms.Button();
			this.remove = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// messages
			// 
			this.messages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.messages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader1,
																					   this.columnHeader2});
			this.messages.Location = new System.Drawing.Point(16, 112);
			this.messages.Name = "messages";
			this.messages.Size = new System.Drawing.Size(424, 280);
			this.messages.TabIndex = 0;
			this.messages.View = System.Windows.Forms.View.Details;
			this.messages.SelectedIndexChanged += new System.EventHandler(this.messages_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 125;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "State";
			this.columnHeader2.Width = 92;
			// 
			// comboBox1
			// 
			this.comboBox1.Enabled = false;
			this.comboBox1.Location = new System.Drawing.Point(72, 56);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(176, 21);
			this.comboBox1.TabIndex = 2;
			this.comboBox1.Text = "Workgroup";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 56);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 3;
			this.label1.Text = "Domain:";
			// 
			// accept
			// 
			this.accept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.accept.Enabled = false;
			this.accept.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.accept.Location = new System.Drawing.Point(16, 400);
			this.accept.Name = "accept";
			this.accept.TabIndex = 4;
			this.accept.Text = "Accept";
			this.accept.Click += new System.EventHandler(this.accept_Click);
			// 
			// decline
			// 
			this.decline.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.decline.Enabled = false;
			this.decline.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.decline.Location = new System.Drawing.Point(96, 400);
			this.decline.Name = "decline";
			this.decline.TabIndex = 5;
			this.decline.Text = "Decline";
			this.decline.Click += new System.EventHandler(this.decline_Click);
			// 
			// remove
			// 
			this.remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.remove.Enabled = false;
			this.remove.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.remove.Location = new System.Drawing.Point(365, 400);
			this.remove.Name = "remove";
			this.remove.TabIndex = 6;
			this.remove.Text = "Remove";
			this.remove.Click += new System.EventHandler(this.remove_Click);
			// 
			// MessageForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 430);
			this.Controls.Add(this.remove);
			this.Controls.Add(this.decline);
			this.Controls.Add(this.accept);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.messages);
			this.MinimumSize = new System.Drawing.Size(336, 368);
			this.Name = "MessageForm";
			this.Text = "Messages";
			this.Load += new System.EventHandler(this.MessageForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		#endregion

		#region Event Handlers
		private void MessageForm_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"Invitation.ico"));

				messages.SmallImageList = new ImageList();
				messages.SmallImageList.Images.Add(Image.FromFile(Path.Combine(Application.StartupPath, "Invitation.ico")));
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading graphics");
			}

			subscriber = new EventSubscriber(config);

			// TODO: Will need to change this when a different POBox is selectable.
			subscriber.CollectionId = poBox.ID;
			subscriber.NodeChanged += new NodeEventHandler(subscriber_NodeChanged);
			subscriber.NodeCreated += new NodeEventHandler(subscriber_NodeCreated);
			subscriber.NodeDeleted += new NodeEventHandler(subscriber_NodeDeleted);

			messages.BeginUpdate();

			try
			{
				ICSList msgList = poBox.GetNodesByType(typeof(Subscription).Name);

				foreach (ShallowNode sn in msgList)
				{
					Subscription sub = new Subscription(poBox, sn);
					string[] items = new string[]{sub.Name, sub.SubscriptionState.ToString()};
					ListViewItem lvi = new ListViewItem(items, 0);
					lvi.Tag = sub;
					messages.Items.Add(lvi);
					ht.Add(sub.ID, lvi);
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "In Load");
			}

			messages.EndUpdate();
		}

		private void messages_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// TODO: for now, we only care about the accept and remove buttons.
			if (messages.SelectedItems.Count > 0)
			{
				remove.Enabled = true;

				if (messages.SelectedItems.Count == 1)
				{
					Subscription sub = (Subscription)messages.SelectedItems[0].Tag;
					if ((sub.SubscriptionState == SubscriptionStates.Received) ||
						(sub.SubscriptionState == SubscriptionStates.Pending))
					{
						accept.Enabled = true;
					}
				}
				else
				{
					accept.Enabled = false;
				}
			}
			else
			{
				accept.Enabled = decline.Enabled = remove.Enabled = false;
			}
		}

		private void accept_Click(object sender, System.EventArgs e)
		{
			ListViewItem lvi = messages.SelectedItems[0];
			Subscription sub = (Subscription)lvi.Tag;
			if (sub.SubscriptionState == SubscriptionStates.Received)
			{
				Process.Start(Path.Combine(Application.StartupPath, "InvitationWizard.exe"), "/ID=" + sub.ID + ":" + sub.DomainID);
			}
			else
			{
				try
				{
					Member member = sub.Accept(store, sub.SubscriptionRights);

					if (sub.DomainID == Domain.WorkGroupDomainID)
					{
						// Take the relationship off the Subsription object
						Property property = sub.Properties.GetSingleProperty("Contact");
						if (property != null)
						{
							Relationship relationship = (Relationship)property.Value;

							// Get the contact from the relationship.
							Novell.AddressBook.AddressBook ab = this.abManager.GetAddressBook(relationship.CollectionID);
							Contact contact = ab.GetContact(relationship.NodeID);

							// Put the Member userID into the Contact userID.
							contact.UserID = member.UserID;
							ab.Commit(contact);
						}
					}

					poBox.Commit(sub);

					lvi.SubItems[1].Text = sub.SubscriptionState.ToString();
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Accepting");
				}
			}

			accept.Enabled = false;
		}

		private void decline_Click(object sender, System.EventArgs e)
		{
		}

		private void remove_Click(object sender, System.EventArgs e)
		{
			foreach (ListViewItem lvi in messages.SelectedItems)
			{
				Subscription sub = (Subscription)lvi.Tag;
				string nodeID = sub.ID;

				try
				{
					poBox.Commit(poBox.Delete(sub));
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "Removing subscription");
				}

				ht.Remove(nodeID);
				lvi.Remove();
			}
		}

		private void subscriber_NodeCreated(NodeEventArgs args)
		{
			try
			{
				Node node = poBox.GetNodeByID(args.ID);
				if (node != null)
				{
					Subscription sub = new Subscription(node);

					string[] items = new string[]{sub.Name, sub.SubscriptionState.ToString()};
					ListViewItem lvi = new ListViewItem(items, 0);
					lvi.Tag = sub;
					messages.Items.Add(lvi);
					ht.Add(sub.ID, lvi);
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "OnNodeCreated");
			}
		}

		private void subscriber_NodeDeleted(NodeEventArgs args)
		{
			ListViewItem lvi = (ListViewItem)ht[args.Node];
			if (lvi != null)
			{
				lvi.Remove();
				ht.Remove(args.Node);
			}
		}

		private void subscriber_NodeChanged(NodeEventArgs args)
		{
			ListViewItem lvi = (ListViewItem)ht[args.Node];
			if (lvi != null)
			{
				try
				{
					Node node = poBox.GetNodeByID(args.ID);
					if (node != null)
					{
						Subscription sub = new Subscription(node);
						lvi.SubItems[1].Text = sub.SubscriptionState.ToString();
					}
				}
				catch (SimiasException ex)
				{
					ex.LogError();
				}
				catch (Exception ex)
				{
					logger.Debug(ex, "OnNodeChanged");
				}
			}
		}
		#endregion
	}
}
