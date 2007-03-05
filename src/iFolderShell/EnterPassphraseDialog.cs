using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Simias.Client;

namespace Novell.iFolderCom 
{
	/// <summary>
	/// Summary description for EnterPassphraseDialog.
	/// </summary>
	public class EnterPassphraseDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.ComboBox RecoveryAgentCombo;
		private System.Windows.Forms.Label lblRecoveryAgent;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.TextBox RetypePassphrase;
		private System.Windows.Forms.Label lblPassphrase;
		private System.Windows.Forms.Label lblRetypePassphrase;
		private System.Windows.Forms.CheckBox savePassphrase;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.ComponentModel.Container components = null;
		private SimiasWebService simws;
		private string DomainID;
		private bool	status;

		public bool PassphraseStatus
		{
			get
			{
				return status;
			}
		}

		public EnterPassphraseDialog(string domainID, SimiasWebService simws)
		{
			this.DomainID = domainID;
			this.simws = simws;
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.panel1 = new System.Windows.Forms.Panel();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.RecoveryAgentCombo = new System.Windows.Forms.ComboBox();
			this.lblRecoveryAgent = new System.Windows.Forms.Label();
			this.Passphrase = new System.Windows.Forms.TextBox();
			this.RetypePassphrase = new System.Windows.Forms.TextBox();
			this.lblPassphrase = new System.Windows.Forms.Label();
			this.lblRetypePassphrase = new System.Windows.Forms.Label();
			this.savePassphrase = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.Blue;
			this.panel1.Controls.Add(this.waterMark);
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(448, 56);
			this.panel1.TabIndex = 1;
			// 
			// waterMark
			// 
			this.waterMark.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(163)), ((System.Byte)(237)));
			this.waterMark.Location = new System.Drawing.Point(16, 8);
			this.waterMark.Name = "waterMark";
			this.waterMark.Size = new System.Drawing.Size(48, 48);
			this.waterMark.TabIndex = 0;
			this.waterMark.TabStop = false;
			// 
			// RecoveryAgentCombo
			// 
			this.RecoveryAgentCombo.Location = new System.Drawing.Point(156, 76);
			this.RecoveryAgentCombo.Name = "RecoveryAgentCombo";
			this.RecoveryAgentCombo.Size = new System.Drawing.Size(240, 21);
			this.RecoveryAgentCombo.TabIndex = 2;
			// 
			// lblRecoveryAgent
			// 
			this.lblRecoveryAgent.Location = new System.Drawing.Point(16, 76);
			this.lblRecoveryAgent.Name = "lblRecoveryAgent";
			this.lblRecoveryAgent.Size = new System.Drawing.Size(140, 20);
			this.lblRecoveryAgent.TabIndex = 3;
			this.lblRecoveryAgent.Text = "Recovery Agent";
			this.lblRecoveryAgent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Passphrase
			// 
			this.Passphrase.Location = new System.Drawing.Point(156, 104);
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.Size = new System.Drawing.Size(240, 20);
			this.Passphrase.TabIndex = 4;
			this.Passphrase.Text = "";
			this.Passphrase.TextChanged += new System.EventHandler(this.Passphrase_TextChanged);
			// 
			// RetypePassphrase
			// 
			this.RetypePassphrase.Location = new System.Drawing.Point(156, 132);
			this.RetypePassphrase.Name = "RetypePassphrase";
			this.RetypePassphrase.Size = new System.Drawing.Size(240, 20);
			this.RetypePassphrase.TabIndex = 5;
			this.RetypePassphrase.Text = "";
			this.RetypePassphrase.TextChanged += new System.EventHandler(this.RetypePassphrase_TextChanged);
			// 
			// lblPassphrase
			// 
			this.lblPassphrase.Location = new System.Drawing.Point(16, 104);
			this.lblPassphrase.Name = "lblPassphrase";
			this.lblPassphrase.Size = new System.Drawing.Size(120, 20);
			this.lblPassphrase.TabIndex = 6;
			this.lblPassphrase.Text = "Enter Passphrase";
			// 
			// lblRetypePassphrase
			// 
			this.lblRetypePassphrase.Location = new System.Drawing.Point(16, 132);
			this.lblRetypePassphrase.Name = "lblRetypePassphrase";
			this.lblRetypePassphrase.Size = new System.Drawing.Size(120, 20);
			this.lblRetypePassphrase.TabIndex = 7;
			this.lblRetypePassphrase.Text = "Retype Passphrase";
			// 
			// savePassphrase
			// 
			this.savePassphrase.Location = new System.Drawing.Point(156, 160);
			this.savePassphrase.Name = "savePassphrase";
			this.savePassphrase.Size = new System.Drawing.Size(240, 20);
			this.savePassphrase.TabIndex = 8;
			this.savePassphrase.Text = "Remember Passphrase";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(239, 200);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(319, 200);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 10;
			this.btnOk.Text = "&Ok";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// EnterPassphraseDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(420, 242);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.savePassphrase);
			this.Controls.Add(this.lblRetypePassphrase);
			this.Controls.Add(this.lblPassphrase);
			this.Controls.Add(this.RetypePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.lblRecoveryAgent);
			this.Controls.Add(this.RecoveryAgentCombo);
			this.Controls.Add(this.panel1);
			this.Name = "EnterPassphraseDialog";
			this.Text = "EnterPassphraseDialog";
			this.Load += new System.EventHandler(this.EnterPassphraseDialog_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			simws.StorePassPhrase(DomainID, "", CredentialType.None, false);
			status = false;
			this.Dispose();
			this.Close();
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			// Check for passphrase
			/*	If passphrase is valid try setting the passphrase.
			 *	if successful try storing on local place
			 *	if success close dialog.
			 *	else show dialog again..
			 * 
			 */
			if( this.Passphrase.Text == this.RetypePassphrase.Text)
			{
				string publicKey = "";
				Status passPhraseStatus = simws.SetPassPhrase( DomainID, this.Passphrase.Text, null, publicKey);
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					simws.StorePassPhrase( DomainID, this.Passphrase.Text, CredentialType.Basic, this.savePassphrase.Checked);
					string passphr = simws.GetPassPhrase(DomainID);
					MessageBox.Show("Passphrase is set & stored", passphr, MessageBoxButtons.OK);
					this.status= simws.IsPassPhraseSet(DomainID);
					if( status == true)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox("Enter passphrase", "Successfully set the passphrase", "", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
						mmb.ShowDialog();
						mmb.Dispose();
						this.Dispose();
						this.Close();
					}
				}
				else 
				{
					// Unable to set the passphrase
					status = false;
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox("Error setting the passphrase", "Unable to set the passphrase", "Please try again", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();
				}
			}
			else
			{
				status = false;
			}
		}

		private void EnterPassphraseDialog_Load(object sender, System.EventArgs e)
		{
			
		}

		private void Passphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void RetypePassphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void UpdateSensitivity()
		{
			if( this.Passphrase.Text.Length > 0 && this.Passphrase.Text == this.RetypePassphrase.Text)
				this.btnOk.Enabled = true;
			else
				this.btnOk.Enabled = false;
		}
	}
}
