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
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace NUnit.UiKit
{
	/// <summary>
	/// Summary description for AssemblyPathDialog.
	/// </summary>
	public class AssemblyPathDialog : System.Windows.Forms.Form
	{
		private string path;

		private System.Windows.Forms.TextBox assemblyPathTextBox;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button browseButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AssemblyPathDialog( string path )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//

			this.path = path;
		}

		public string Path
		{
			get { return path; }
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AssemblyPathDialog));
			this.assemblyPathTextBox = new System.Windows.Forms.TextBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.browseButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// assemblyPathTextBox
			// 
			this.assemblyPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.assemblyPathTextBox.Location = new System.Drawing.Point(8, 16);
			this.assemblyPathTextBox.Name = "assemblyPathTextBox";
			this.assemblyPathTextBox.Size = new System.Drawing.Size(440, 22);
			this.assemblyPathTextBox.TabIndex = 0;
			this.assemblyPathTextBox.Text = "";
			// 
			// okButton
			// 
			this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.okButton.Location = new System.Drawing.Point(160, 48);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(256, 48);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			// 
			// browseButton
			// 
			this.browseButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.browseButton.Image = ((System.Drawing.Bitmap)(resources.GetObject("browseButton.Image")));
			this.browseButton.Location = new System.Drawing.Point(456, 16);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(24, 24);
			this.browseButton.TabIndex = 4;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// AssemblyPathDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(490, 78);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.browseButton,
																		  this.cancelButton,
																		  this.okButton,
																		  this.assemblyPathTextBox});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "AssemblyPathDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Assembly Path";
			this.Load += new System.EventHandler(this.AssemblyPathDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void okButton_Click(object sender, System.EventArgs e)
		{
			string path = assemblyPathTextBox.Text;

			try
			{
				FileInfo info = new FileInfo( path );

				if ( !info.Exists )
				{
					DialogResult answer = UserMessage.Ask( string.Format( 
						"The path {0} does not exist. Do you want to use it anyway?", path ) );
					if ( answer != DialogResult.Yes )
						return;
				}

				DialogResult = DialogResult.OK;
				this.path = path;
				this.Close();
			}
			catch( System.Exception exception )
			{
				assemblyPathTextBox.SelectAll();
				UserMessage.DisplayFailure( exception, "Invalid Entry" );
			}	
		}

		private void AssemblyPathDialog_Load(object sender, System.EventArgs e)
		{
			assemblyPathTextBox.Text = path;
		}

		private void browseButton_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Select Assembly";
			
			dlg.Filter =
				"Assemblies (*.dll,*.exe)|*.dll;*.exe|" +
				"All Files (*.*)|*.*";

			dlg.InitialDirectory = System.IO.Path.GetDirectoryName( path );
			dlg.FilterIndex = 1;
			dlg.FileName = "";

			if ( dlg.ShowDialog( this ) == DialogResult.OK ) 
			{
				assemblyPathTextBox.Text = dlg.FileName;
			}
		}
	}
}
