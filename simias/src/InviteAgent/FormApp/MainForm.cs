/***********************************************************************
 *  $RCSfile$
 * 
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Library General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this library; if not, write to the Free
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
using System.IO;

namespace Simias.Agent
{
	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button acceptButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.TextBox sharedByTextBox;
		private System.Windows.Forms.TextBox rightsTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox locationTextBox;
		private System.Windows.Forms.Button browseButton;
		internal System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private string pathname;

		private Invitation invitation;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="args">Arguments</param>
		public MainForm(string[] args)
		{
			InitializeComponent();

			if (args.Length == 1)
			{
				pathname = args[0];
			}
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
			this.acceptButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.sharedByTextBox = new System.Windows.Forms.TextBox();
			this.rightsTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.locationTextBox = new System.Windows.Forms.TextBox();
			this.browseButton = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// acceptButton
			// 
			this.acceptButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.acceptButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.acceptButton.Location = new System.Drawing.Point(152, 184);
			this.acceptButton.Name = "acceptButton";
			this.acceptButton.TabIndex = 0;
			this.acceptButton.Text = "Accept";
			this.acceptButton.Click += new System.EventHandler(this.acceptButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cancelButton.Location = new System.Drawing.Point(232, 184);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// label1
			// 
			this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 24);
			this.label1.TabIndex = 2;
			this.label1.Text = "Share Name:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label2.Location = new System.Drawing.Point(8, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 24);
			this.label2.TabIndex = 4;
			this.label2.Text = "Shared by:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// nameTextBox
			// 
			this.nameTextBox.Location = new System.Drawing.Point(88, 8);
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.ReadOnly = true;
			this.nameTextBox.Size = new System.Drawing.Size(216, 20);
			this.nameTextBox.TabIndex = 3;
			this.nameTextBox.Text = "";
			// 
			// sharedByTextBox
			// 
			this.sharedByTextBox.Location = new System.Drawing.Point(88, 40);
			this.sharedByTextBox.Name = "sharedByTextBox";
			this.sharedByTextBox.ReadOnly = true;
			this.sharedByTextBox.Size = new System.Drawing.Size(216, 20);
			this.sharedByTextBox.TabIndex = 5;
			this.sharedByTextBox.Text = "";
			// 
			// rightsTextBox
			// 
			this.rightsTextBox.Location = new System.Drawing.Point(88, 72);
			this.rightsTextBox.Name = "rightsTextBox";
			this.rightsTextBox.ReadOnly = true;
			this.rightsTextBox.Size = new System.Drawing.Size(216, 20);
			this.rightsTextBox.TabIndex = 7;
			this.rightsTextBox.Text = "";
			// 
			// label3
			// 
			this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label3.Location = new System.Drawing.Point(9, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(71, 24);
			this.label3.TabIndex = 6;
			this.label3.Text = "Rights:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label4.Location = new System.Drawing.Point(8, 104);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 8;
			this.label4.Text = "Location:";
			// 
			// locationTextBox
			// 
			this.locationTextBox.Location = new System.Drawing.Point(8, 120);
			this.locationTextBox.Multiline = true;
			this.locationTextBox.Name = "locationTextBox";
			this.locationTextBox.Size = new System.Drawing.Size(256, 56);
			this.locationTextBox.TabIndex = 9;
			this.locationTextBox.Text = "";
			// 
			// browseButton
			// 
			this.browseButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.browseButton.Location = new System.Drawing.Point(272, 136);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(32, 23);
			this.browseButton.TabIndex = 10;
			this.browseButton.Text = "...";
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
			// 
			// MainForm
			// 
			this.AcceptButton = this.acceptButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(314, 216);
			this.Controls.Add(this.browseButton);
			this.Controls.Add(this.locationTextBox);
			this.Controls.Add(this.rightsTextBox);
			this.Controls.Add(this.sharedByTextBox);
			this.Controls.Add(this.nameTextBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.acceptButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.HelpButton = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.Text = "Simias Invitation";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);
		}
		
		#endregion Windows Form Designer

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			Application.Run(new MainForm(args));
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void acceptButton_Click(object sender, System.EventArgs e)
		{
			invitation.RootPath = locationTextBox.Text;
			
			(new AgentFactory()).GetInviteAgent().Accept(invitation);

			MessageBox.Show(this, "Invitation Successfully Accepted!",
				"Simias Invitation");

			this.Close();
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			folderBrowserDialog.SelectedPath = locationTextBox.Text;

			DialogResult result = folderBrowserDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				locationTextBox.Text = folderBrowserDialog.SelectedPath;
			}
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
			if (pathname == null)
			{
				MessageBox.Show(this, "An Simias invitation file is required.",
					"Simias Invitation");

				this.Close();
			}
			else
			{
				invitation = new Invitation();
				
				invitation.Load(pathname);

				nameTextBox.Text = invitation.CollectionName;
				sharedByTextBox.Text = invitation.FromName;
				rightsTextBox.Text = invitation.CollectionRights;
				locationTextBox.Text = Invitation.DefaultRootPath;
			}
		}
	}
}
