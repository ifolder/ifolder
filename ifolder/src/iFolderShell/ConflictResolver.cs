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
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for ConflictResolver.
	/// </summary>
	[ComVisible(false)]
	public class ConflictResolver : System.Windows.Forms.Form
	{
		#region Class Members
		private iFolderWebService ifWebService;
		private iFolder ifolder;
		private string loadPath;
		private bool initial = true;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label ifolderName;
		private System.Windows.Forms.Label ifolderPath;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label localName;
		private System.Windows.Forms.Label localDate;
		private System.Windows.Forms.Label localSize;
		private System.Windows.Forms.Button saveLocal;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button saveServer;
		private System.Windows.Forms.Label serverSize;
		private System.Windows.Forms.Label serverDate;
		private System.Windows.Forms.Label serverName;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.Button help;
		private System.Windows.Forms.ListView conflictsView;
		#endregion
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;

		public ConflictResolver()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.StartPosition = FormStartPosition.CenterParent;
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ifolderName = new System.Windows.Forms.Label();
			this.ifolderPath = new System.Windows.Forms.Label();
			this.conflictsView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.saveLocal = new System.Windows.Forms.Button();
			this.localSize = new System.Windows.Forms.Label();
			this.localDate = new System.Windows.Forms.Label();
			this.localName = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.saveServer = new System.Windows.Forms.Button();
			this.serverSize = new System.Windows.Forms.Label();
			this.serverDate = new System.Windows.Forms.Label();
			this.serverName = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.close = new System.Windows.Forms.Button();
			this.help = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(392, 40);
			this.label1.TabIndex = 0;
			this.label1.Text = "Select a conflict from the list below.  To resolve the conflict, save the local v" +
				"ersion or the server version.  The version you save will be synced to this iFold" +
				"er.";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 80);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "iFolder name:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(32, 104);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 16);
			this.label3.TabIndex = 2;
			this.label3.Text = "iFolder path:";
			// 
			// ifolderName
			// 
			this.ifolderName.Location = new System.Drawing.Point(112, 80);
			this.ifolderName.Name = "ifolderName";
			this.ifolderName.Size = new System.Drawing.Size(312, 16);
			this.ifolderName.TabIndex = 3;
			// 
			// ifolderPath
			// 
			this.ifolderPath.Location = new System.Drawing.Point(112, 104);
			this.ifolderPath.Name = "ifolderPath";
			this.ifolderPath.Size = new System.Drawing.Size(312, 16);
			this.ifolderPath.TabIndex = 4;
			this.ifolderPath.Paint += new System.Windows.Forms.PaintEventHandler(this.ifolderPath_Paint);
			// 
			// conflictsView
			// 
			this.conflictsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							this.columnHeader1});
			this.conflictsView.Location = new System.Drawing.Point(16, 128);
			this.conflictsView.Name = "conflictsView";
			this.conflictsView.Size = new System.Drawing.Size(408, 168);
			this.conflictsView.TabIndex = 5;
			this.conflictsView.View = System.Windows.Forms.View.Details;
			this.conflictsView.SelectedIndexChanged += new System.EventHandler(this.conflictsView_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "iFolder Conflicts";
			this.columnHeader1.Width = 404;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.saveLocal);
			this.groupBox1.Controls.Add(this.localSize);
			this.groupBox1.Controls.Add(this.localDate);
			this.groupBox1.Controls.Add(this.localName);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox1.Location = new System.Drawing.Point(16, 304);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(200, 128);
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Local version";
			// 
			// saveLocal
			// 
			this.saveLocal.Enabled = false;
			this.saveLocal.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveLocal.Location = new System.Drawing.Point(8, 96);
			this.saveLocal.Name = "saveLocal";
			this.saveLocal.Size = new System.Drawing.Size(64, 23);
			this.saveLocal.TabIndex = 6;
			this.saveLocal.Text = "Save";
			this.saveLocal.Click += new System.EventHandler(this.saveLocal_Click);
			// 
			// localSize
			// 
			this.localSize.Location = new System.Drawing.Point(64, 72);
			this.localSize.Name = "localSize";
			this.localSize.Size = new System.Drawing.Size(128, 16);
			this.localSize.TabIndex = 5;
			// 
			// localDate
			// 
			this.localDate.Location = new System.Drawing.Point(64, 48);
			this.localDate.Name = "localDate";
			this.localDate.Size = new System.Drawing.Size(128, 16);
			this.localDate.TabIndex = 4;
			// 
			// localName
			// 
			this.localName.Location = new System.Drawing.Point(64, 24);
			this.localName.Name = "localName";
			this.localName.Size = new System.Drawing.Size(128, 16);
			this.localName.TabIndex = 3;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 72);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 16);
			this.label6.TabIndex = 2;
			this.label6.Text = "Size:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 48);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 16);
			this.label5.TabIndex = 1;
			this.label5.Text = "Date:";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Name:";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.saveServer);
			this.groupBox2.Controls.Add(this.serverSize);
			this.groupBox2.Controls.Add(this.serverDate);
			this.groupBox2.Controls.Add(this.serverName);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.label12);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(224, 304);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(200, 128);
			this.groupBox2.TabIndex = 7;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Server version";
			// 
			// saveServer
			// 
			this.saveServer.Enabled = false;
			this.saveServer.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.saveServer.Location = new System.Drawing.Point(8, 96);
			this.saveServer.Name = "saveServer";
			this.saveServer.Size = new System.Drawing.Size(64, 23);
			this.saveServer.TabIndex = 6;
			this.saveServer.Text = "Save";
			this.saveServer.Click += new System.EventHandler(this.saveServer_Click);
			// 
			// serverSize
			// 
			this.serverSize.Location = new System.Drawing.Point(64, 72);
			this.serverSize.Name = "serverSize";
			this.serverSize.Size = new System.Drawing.Size(128, 16);
			this.serverSize.TabIndex = 5;
			// 
			// serverDate
			// 
			this.serverDate.Location = new System.Drawing.Point(64, 48);
			this.serverDate.Name = "serverDate";
			this.serverDate.Size = new System.Drawing.Size(128, 16);
			this.serverDate.TabIndex = 4;
			// 
			// serverName
			// 
			this.serverName.Location = new System.Drawing.Point(64, 24);
			this.serverName.Name = "serverName";
			this.serverName.Size = new System.Drawing.Size(128, 16);
			this.serverName.TabIndex = 3;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(8, 72);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(56, 16);
			this.label10.TabIndex = 2;
			this.label10.Text = "Size:";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(8, 48);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(56, 16);
			this.label11.TabIndex = 1;
			this.label11.Text = "Date:";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(8, 24);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(56, 16);
			this.label12.TabIndex = 0;
			this.label12.Text = "Name:";
			// 
			// close
			// 
			this.close.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.close.Location = new System.Drawing.Point(333, 440);
			this.close.Name = "close";
			this.close.TabIndex = 8;
			this.close.Text = "Close";
			this.close.Click += new System.EventHandler(this.close_Click);
			// 
			// help
			// 
			this.help.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.help.Location = new System.Drawing.Point(32, 440);
			this.help.Name = "help";
			this.help.TabIndex = 9;
			this.help.Text = "Help";
			this.help.Click += new System.EventHandler(this.help_Click);
			// 
			// ConflictResolver
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 470);
			this.Controls.Add(this.help);
			this.Controls.Add(this.close);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.conflictsView);
			this.Controls.Add(this.ifolderPath);
			this.Controls.Add(this.ifolderName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ConflictResolver";
			this.Text = "iFolder Conflict Resolver";
			this.Load += new System.EventHandler(this.ConflictResolver_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Methods
		private void resolveConflicts(bool localWins)
		{
			foreach (ListViewItem lvi in conflictsView.SelectedItems)
			{
				Conflict conflict = (Conflict)lvi.Tag;
				if (!conflict.IsNameConflict)
				{
					try
					{
						ifWebService.ResolveFileConflict(ifolder.ID, conflict.ConflictID, localWins);
						lvi.Remove();
					}
					catch (WebException ex)
					{
						MyMessageBox mmb = new MyMessageBox();
						// TODO: Localize
						mmb.Message = "An error was encountered while attempting to resolve the conflict.";
						mmb.ShowDialog();
					}
					catch (Exception ex)
					{
						MyMessageBox mmb = new MyMessageBox();
						// TODO: Localize
						mmb.Message = "An error was encountered while attempting to resolve the conflict.";
						mmb.ShowDialog();
					}
				}
				else if (!localWins)
				{
					/*try
					{
						// TODO: prompt for new name?
						ifWebService.ResolveNameConflict(ifolder.ID, conflict.ConflictID, newName);
						lvi.Remove();
					}
					catch (WebException e)
					{
						// TODO:
					}
					catch (Exception e)
					{
						// TODO:
					}*/
				}
			}

			if (conflictsView.Items.Count == 0 && ConflictsResolved != null)
			{
				// If all the conflicts have been resolved, fire the ConflictsResolved event.
				ConflictsResolved(this, new EventArgs());
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the iFolderWebService object to use.
		/// </summary>
		public iFolderWebService iFolderWebService
		{
			set { ifWebService = value; }
		}

		/// <summary>
		/// Sets the iFolder to resolve conflicts for.
		/// </summary>
		public iFolder iFolder
		{
			set { ifolder = value; }
		}

		/// <summary>
		/// Sets the load path of the assembly.
		/// </summary>
		public string LoadPath
		{
			set { loadPath = value; }
		}
		#endregion

		#region Events
		/// <summary>
		/// Delegate used when conflicts have been resolved.
		/// </summary>
		public delegate void ConflictsResolvedDelegate(object sender, EventArgs e);
		/// <summary>
		/// Occurs when all conflicts have been resolved.
		/// </summary>
		public event ConflictsResolvedDelegate ConflictsResolved;
		#endregion

		#region Event Handlers
		private void ConflictResolver_Load(object sender, System.EventArgs e)
		{
			try
			{
				string basePath = loadPath != null ? Path.Combine(loadPath, "res") : Path.Combine(Application.StartupPath, "res");
				this.Icon = new Icon(Path.Combine(basePath, "ifolderconflict.ico"));
			}
			catch
			{
				// Ignore
			}

			try
			{
				// Display the iFolder info.
				ifolderName.Text = ifolder.Name;

				// Put the conflicts in the listview.
				Conflict[] conflicts = ifWebService.GetiFolderConflicts(ifolder.ID);
				foreach (Conflict conflict in conflicts)
				{
					// TODO: what name to use ... and icon.
					ListViewItem lvi = new ListViewItem(conflict.LocalName);
					lvi.Tag = conflict;
					this.conflictsView.Items.Add(lvi);
				}
			}
			catch (WebException ex)
			{
				MyMessageBox mmb = new MyMessageBox();
				// TODO: Localize
				mmb.Message = "An error was encountered while reading the conflicts.";
				mmb.ShowDialog();
			}
			catch (Exception ex)
			{
				MyMessageBox mmb = new MyMessageBox();
				// TODO: Localize
				mmb.Message = "An error was encountered while reading the conflicts.";
				mmb.ShowDialog();
			}
		}

		private void conflictsView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// Enable/disable the save buttons.
			saveLocal.Enabled = saveServer.Enabled = conflictsView.SelectedItems.Count > 0;

			if (conflictsView.SelectedItems.Count == 1)
			{
				Conflict conflict = (Conflict)conflictsView.SelectedItems[0].Tag;

				// Fill in the server data.
				serverName.Text = conflict.ServerName;
				serverDate.Text = conflict.ServerDate;
				serverSize.Text = conflict.ServerSize;

				// For name conflicts there is not a local version.
				if (conflict.IsNameConflict)
				{
					saveLocal.Enabled = false;
					localName.Text = localDate.Text = localSize.Text = "";
				}
				else
				{
					localName.Text = conflict.LocalName;
					localDate.Text = conflict.LocalDate;
					localSize.Text = conflict.LocalSize;
				}
			}
			else
			{
				// Clear the data fields.
				localName.Text = serverName.Text =
					localDate.Text = serverDate.Text =
					localSize.Text = serverSize.Text = "";
			}
		}

		private void saveLocal_Click(object sender, System.EventArgs e)
		{
			resolveConflicts(true);
		}

		private void saveServer_Click(object sender, System.EventArgs e)
		{
			resolveConflicts(false);
		}

		private void help_Click(object sender, System.EventArgs e)
		{
			new iFolderComponent().ShowHelp(loadPath);
		}

		private void close_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void ifolderPath_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			// Only do this one time.
			if (initial)
			{
				initial = false;

				// Measure the string.
				SizeF stringSize = e.Graphics.MeasureString(ifolder.UnManagedPath, ifolderPath.Font);
				if (stringSize.Width > ifolderPath.Width)
				{
					// Calculate the length of the string that can be displayed ... this will get us in the
					// ballpark.
					int length = (int)(ifolderPath.Width * ifolder.UnManagedPath.Length / stringSize.Width);
					string tmp = String.Empty;

					// Remove one character at a time until we fit in the box.  This should only loop 3 or 4 times at most.
					while (stringSize.Width > ifolderPath.Width)
					{
						tmp = ifolder.UnManagedPath.Substring(0, length) + "...";
						stringSize = e.Graphics.MeasureString(tmp, ifolderPath.Font);
						length -= 1;
					}

					// Set the truncated string in the display.
					ifolderPath.Text = tmp;

					// Set up a tooltip to display the complete path.
					toolTip1.SetToolTip(ifolderPath, ifolder.UnManagedPath);
				}
				else
				{
					// The path fits ... no truncation needed.
					ifolderPath.Text = ifolder.UnManagedPath;
				}
			}
		}
		#endregion
	}
}
