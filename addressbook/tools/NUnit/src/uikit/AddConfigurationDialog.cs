#region Copyright (c) 2002-2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using NUnit.Util;

namespace NUnit.UiKit
{
	/// <summary>
	/// Summary description for AssemblyNameDialog.
	/// </summary>
	public class AddConfigurationDialog : System.Windows.Forms.Form
	{
		#region Instance variables

		private NUnitProject project;
		private string configurationName;
		private string copyConfigurationName;

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox configurationNameTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox configurationComboBox;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#endregion

		#region Construction and Disposal

		public AddConfigurationDialog( NUnitProject project )
		{ 
			InitializeComponent();
			this.project = project;
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

		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.configurationNameTextBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.configurationComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// configurationNameTextBox
			// 
			this.configurationNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.configurationNameTextBox.Location = new System.Drawing.Point(16, 24);
			this.configurationNameTextBox.Name = "configurationNameTextBox";
			this.configurationNameTextBox.Size = new System.Drawing.Size(254, 22);
			this.configurationNameTextBox.TabIndex = 0;
			this.configurationNameTextBox.Text = "";
			// 
			// okButton
			// 
			this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.okButton.Location = new System.Drawing.Point(51, 120);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(155, 120);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			// 
			// configurationComboBox
			// 
			this.configurationComboBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.configurationComboBox.Location = new System.Drawing.Point(16, 80);
			this.configurationComboBox.Name = "configurationComboBox";
			this.configurationComboBox.Size = new System.Drawing.Size(256, 24);
			this.configurationComboBox.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(248, 16);
			this.label1.TabIndex = 4;
			this.label1.Text = "Configuration Name:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(240, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Copy Settings From:";
			// 
			// AddConfigurationDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(282, 150);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.label2,
																		  this.label1,
																		  this.configurationComboBox,
																		  this.cancelButton,
																		  this.okButton,
																		  this.configurationNameTextBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AddConfigurationDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "New Configuration";
			this.Load += new System.EventHandler(this.ConfigurationNameDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties

		public string ConfigurationName
		{
			get { return configurationName; }
		}

		public string CopyConfigurationName
		{
			get { return copyConfigurationName; }
		}

		#endregion

		#region Methods

		private void ConfigurationNameDialog_Load(object sender, System.EventArgs e)
		{
			configurationComboBox.Items.Add( "<none>" );
			configurationComboBox.SelectedIndex = 0;

			foreach( ProjectConfig config in project.Configs )
			{
				int index = configurationComboBox.Items.Add( config.Name );
				if ( config.Name == project.ActiveConfigName )
					configurationComboBox.SelectedIndex = index;
			}
		}

		private void okButton_Click(object sender, System.EventArgs e)
		{
			configurationName = configurationNameTextBox.Text;

			if ( configurationName == string.Empty )
			{
				UserMessage.Display( "No configuration name provided", "Configuration Name Error" );
				return;
			}

			if ( project.Configs.Contains( configurationName ) )
			{
				// TODO: Need general error message display
				UserMessage.Display( "A configuration with that name already exists", "Configuration Name Error" );
				return;
			}

			// ToDo: Move more of this to project
			ProjectConfig newConfig = new ProjectConfig( configurationName );
				
			copyConfigurationName = null;
			if ( configurationComboBox.SelectedIndex > 0 )
			{		
				copyConfigurationName = (string)configurationComboBox.SelectedItem;
				ProjectConfig copyConfig = project.Configs[copyConfigurationName];
				if ( copyConfig != null )
					foreach( AssemblyListItem item in copyConfig.Assemblies )
						newConfig.Assemblies.Add( item.FullPath, item.HasTests );
			}

			project.Configs.Add( newConfig );
			DialogResult = DialogResult.OK;

			Close();
		}

		#endregion
	}
}
