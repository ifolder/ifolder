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
 *  Author: Rob
 *
 ***********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace ApplicationForms
{
	/// <summary>
	/// Remoting Sniffer Form
	/// </summary>
	public class RemotingSnifferForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView LogListView;
		private System.Windows.Forms.Panel controlPanel;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Constructor
		/// </summary>
		public RemotingSnifferForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			
			base.Dispose(disposing);
		}

		#region Windows Form Designer

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LogListView = new System.Windows.Forms.ListView();
			this.controlPanel = new System.Windows.Forms.Panel();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// LogListView
			// 
			this.LogListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.columnHeader1,
																						  this.columnHeader2});
			this.LogListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogListView.Location = new System.Drawing.Point(0, 0);
			this.LogListView.Name = "LogListView";
			this.LogListView.Size = new System.Drawing.Size(632, 446);
			this.LogListView.TabIndex = 0;
			this.LogListView.View = System.Windows.Forms.View.Details;
			// 
			// controlPanel
			// 
			this.controlPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.controlPanel.Location = new System.Drawing.Point(0, 310);
			this.controlPanel.Name = "controlPanel";
			this.controlPanel.Size = new System.Drawing.Size(632, 136);
			this.controlPanel.TabIndex = 1;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Message";
			this.columnHeader1.Width = 123;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Source";
			this.columnHeader2.Width = 227;
			// 
			// RemotingSnifferForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(632, 446);
			this.Controls.Add(this.controlPanel);
			this.Controls.Add(this.LogListView);
			this.Name = "RemotingSnifferForm";
			this.Text = "Remoting Sniffer";
			this.ResumeLayout(false);

		}
		
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new RemotingSnifferForm());
		}
	}
}
