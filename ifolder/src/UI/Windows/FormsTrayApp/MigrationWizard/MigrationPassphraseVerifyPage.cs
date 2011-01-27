/*****************************************************************************
*
* Copyright (c) [2009] Novell, Inc.
* All Rights Reserved.
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of version 2 of the GNU General Public License as
* published by the Free Software Foundation.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.   See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, contact Novell, Inc.
*
* To contact Novell about this file by physical or electronic mail,
* you may find current contact information at www.novell.com
*
*-----------------------------------------------------------------------------
*
*                 $Author: Ramesh Sunder <sramesh@novell.com>
*                 $Modified by: <Modifier>
*                 $Mod Date: <Date Modified>
*                 $Revision: 0.0
*-----------------------------------------------------------------------------
* This module is used to:
*        <Description of the functionality of the file >
*
*
*******************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Novell.Wizard
{
	/// <summary>
	/// Class for the wizard page where the invitation file is selected.
	/// </summary>
	public class MigrationPassphraseVerifyPage : Novell.Wizard.MigrationInteriorPageTemplate
	{
		#region Class Members

		private System.Windows.Forms.Label EnterPPLabel;
		private System.Windows.Forms.Label RememberPPLabel;
		private System.Windows.Forms.TextBox EnterPPText;
		private System.Windows.Forms.CheckBox RememberPPCheck;
		
		//private System.Windows.Forms.RadioButton removeFromServer;
		//private System.Windows.Forms.RadioButton copyToServer;

		//private System.Windows.Forms.CheckBox copyOption;
        
		//private System.Windows.Forms.TextBox iFolderLocation;
		//private System.Windows.Forms.Button browseButton;
		//private System.Windows.Forms.CheckBox defaultServer;
		//private System.Windows.Forms.Label defaultDescription;
		private System.ComponentModel.IContainer components = null;

		//private System.Windows.Forms.Label label1, label2;

		//private string homeLocation;
		//private string prevLoc;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(Novell.FormsTrayApp.FormsTrayApp));

		#endregion

		#region Constructor

		/// <summary>
		/// Constructs a ServerPage object.
		/// </summary>
		public MigrationPassphraseVerifyPage()
		{
			// This call is required by the Windows Form Designer.
			//this.homeLocation = "Home Location";
			InitializeComponent();
		}

		#endregion

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			EnterPPLabel = new Label();
			this.EnterPPText = new TextBox();
			RememberPPLabel = new Label();
			RememberPPCheck = new CheckBox();

			//EnterPPLabel
			EnterPPLabel.Text = Resource.GetString("EnterPassPhrase"); //Enter Passphrase:
			EnterPPLabel.Location = new System.Drawing.Point(60, 122);
			EnterPPLabel.Size = new System.Drawing.Size(120, 16);
			// EnterPPText
			this.EnterPPText.Text = "";
			this.EnterPPText.Location = new System.Drawing.Point(180, 122);
			this.EnterPPText.Size = new System.Drawing.Size(260, 16);
			//RememberPPCheck
			RememberPPCheck.Text = Resource.GetString("RememberPassPhrase"); //Remember Passphrase
			RememberPPCheck.Location = new System.Drawing.Point(180, 156);
			RememberPPCheck.Size = new System.Drawing.Size(350, 16);
			this.SuspendLayout();

			this.Controls.Add(this.EnterPPLabel);
			this.Controls.Add(this.RememberPPCheck);
			this.Controls.Add(this.EnterPPText);
			this.ResumeLayout(false);
		}
		#endregion

		#region Overridden Methods

        /// <summary>
        /// Activate Page
        /// </summary>
        /// <param name="previousIndex">previous index</param>
		internal override void ActivatePage(int previousIndex)
		{
			((MigrationWizard)this.Parent).MigrationWizardButtons = MigrationWizardButtons.Next | MigrationWizardButtons.Back | MigrationWizardButtons.Cancel;
			base.ActivatePage (previousIndex);
		}

        /// <summary>
        /// Deactivate Page
        /// </summary>
       	internal override int DeactivatePage()
		{
			// TODO - implement this...
			return base.DeactivatePage ();
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

        /// <summary>
        /// Validate PAGE
        /// </summary>
        /// <param name="currentIndex">current index</param>
        internal override int ValidatePage(int currentIndex)
		{
			// TODO:
			return base.ValidatePage (currentIndex);
		}

		#endregion

		#region Properties

        /// <summary>
        /// Gets PAssphrase
        /// </summary>
		public string Passphrase
		{
			get
			{
				return this.EnterPPText.Text;
			}
		}

        /// <summary>
        /// Gets Remember Passphrase
        /// </summary>
		public bool RememberPassphrase
		{
			get
			{
				return this.RememberPPCheck.Checked;
			}
		}

		#endregion

		private void copyToServer_CheckedChanged(object sender, EventArgs e)
		{
		}
	}	
}

