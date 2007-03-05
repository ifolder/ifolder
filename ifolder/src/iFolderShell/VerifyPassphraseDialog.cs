using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Simias.Client;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for VerifyPassphraseDialog.
	/// </summary>
	public class VerifyPassphraseDialog : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label lblPassphrase;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.CheckBox savePassphrase;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private SimiasWebService simws;
		private string DomainID;
		private bool status;
		private System.ComponentModel.Container components = null;

		public bool PassphraseStatus
		{
			get
			{
				return status;
			}
		}

		public VerifyPassphraseDialog(string domainID, SimiasWebService simws)
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
			this.lblPassphrase = new System.Windows.Forms.Label();
			this.Passphrase = new System.Windows.Forms.TextBox();
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
			// lblPassphrase
			// 
			this.lblPassphrase.Location = new System.Drawing.Point(16, 76);
			this.lblPassphrase.Name = "lblPassphrase";
			this.lblPassphrase.Size = new System.Drawing.Size(140, 20);
			this.lblPassphrase.TabIndex = 2;
			this.lblPassphrase.Text = "Enter Passphrase:";
			this.lblPassphrase.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// Passphrase
			// 
			this.Passphrase.Location = new System.Drawing.Point(156, 76);
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.Size = new System.Drawing.Size(240, 20);
			this.Passphrase.TabIndex = 3;
			this.Passphrase.Text = "";
			// 
			// savePassphrase
			// 
			this.savePassphrase.Location = new System.Drawing.Point(156, 104);
			this.savePassphrase.Name = "savePassphrase";
			this.savePassphrase.Size = new System.Drawing.Size(240, 20);
			this.savePassphrase.TabIndex = 4;
			this.savePassphrase.Text = "Remember Passphrase";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(239, 134);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(72, 24);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(319, 134);
			this.btnOk.Name = "btnOk";
			this.btnOk.TabIndex = 6;
			this.btnOk.Text = "&Ok";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// VerifyPassphraseDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(420, 178);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.savePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.lblPassphrase);
			this.Controls.Add(this.panel1);
			this.Name = "VerifyPassphraseDialog";
			this.Text = "VerifyPassphraseDialog";
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
			Status passPhraseStatus =  simws.ValidatePassPhrase(this.DomainID, this.Passphrase.Text);
			if( passPhraseStatus != null)
			{
				if( passPhraseStatus.statusCode == StatusCodes.PassPhraseInvalid)  // check for invalid passphrase
				{
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox("Passphrase Invalid", "Unable to validate the passphrase", "Please try again", MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
					mmb.ShowDialog();
					mmb.Dispose();	
				}
				else if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					try
					{
						simws.StorePassPhrase( DomainID, this.Passphrase.Text, CredentialType.Basic, this.savePassphrase.Checked);
						status = true;
						this.Dispose();
						this.Close();
					}
					catch(Exception ex) 
					{
						// TODO: Show error Messahe
						status = false;
					}
				}
			}
		}
	}
}
