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
using Simias;
using Simias.Storage;
using Simias.POBox;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for MessageForm.
	/// </summary>
	public class MessageForm : System.Windows.Forms.Form
	{
		private POBox poBox = null;
		private Store store = null;
		private System.Windows.Forms.ListView inbound;
		private System.Windows.Forms.ListView outbound;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
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

			Configuration config = new Configuration();
			store = new Store(config);

			// TODO: pass in correct domain ... for now just use the default.
			poBox = POBox.GetPOBox(store, store.DefaultDomain);

			// Temporary code to populate the POBox.
/*			int n = 100;
			Simias.POBox.Message[] messages = new Simias.POBox.Message[100];

			while (--n >= 0)
			{
				string name = "MessageTest" + n.ToString();
				string type = n % 2 == 0 ? Simias.POBox.Message.InboundMessage : Simias.POBox.Message.OutboundMessage;
				Simias.POBox.Message msg = new Simias.POBox.Message(name, type);
				msg.ToIdentity = "IdentityTo" + n.ToString();
				msg.FromIdentity = "IdentityFrom" + n.ToString();
				msg.ToAddress = "AddressTo" + n.ToString();
				msg.FromAddress = "AddressFrom" + n.ToString();
				messages[n] = msg;
			}

			poBox.AddMessage(messages);
*/			// End - Temporary code.
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
			this.inbound = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.outbound = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// inbound
			// 
			this.inbound.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.columnHeader1});
			this.inbound.Location = new System.Drawing.Point(16, 112);
			this.inbound.Name = "inbound";
			this.inbound.Size = new System.Drawing.Size(480, 144);
			this.inbound.TabIndex = 0;
			this.inbound.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Name";
			this.columnHeader1.Width = 125;
			// 
			// outbound
			// 
			this.outbound.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					   this.columnHeader2});
			this.outbound.Location = new System.Drawing.Point(16, 288);
			this.outbound.Name = "outbound";
			this.outbound.Size = new System.Drawing.Size(480, 144);
			this.outbound.TabIndex = 1;
			this.outbound.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Name";
			this.columnHeader2.Width = 125;
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
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 96);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Inbox:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 272);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Outbox:";
			// 
			// MessageForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(512, 454);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.outbound);
			this.Controls.Add(this.inbound);
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
			try
			{
				string basePath = Path.Combine(Application.StartupPath, "res");
				outbound.SmallImageList = new ImageList();
				outbound.SmallImageList.Images.Add(Image.FromFile(Path.Combine(basePath, "mail_closed.ico")));
				outbound.SmallImageList.Images.Add(Image.FromFile(Path.Combine(basePath, "mail_opened.ico")));
				inbound.SmallImageList = outbound.SmallImageList;
			}
			catch {}

			ICSList msgList = this.poBox.GetMessagesByMessageType(Simias.POBox.Message.OutboundMessage);

			outbound.BeginUpdate();
			foreach (ShallowNode sn in msgList)
			{
				Simias.POBox.Message message = new Simias.POBox.Message(poBox, sn);
				ListViewItem lvi = new ListViewItem(message.Name, (int)message.State);
				lvi.Tag = message;
				outbound.Items.Add(lvi);
			}
			outbound.EndUpdate();

			msgList = poBox.GetMessagesByMessageType(Simias.POBox.Message.InboundMessage);

			inbound.BeginUpdate();
			foreach (ShallowNode sn in msgList)
			{
				Simias.POBox.Message message = new Simias.POBox.Message(poBox, sn);
				ListViewItem lvi = new ListViewItem(message.Name, (int)message.State);
				lvi.Tag = message;
				inbound.Items.Add(lvi);
			}
			inbound.EndUpdate();
		}
		#endregion
	}
}
