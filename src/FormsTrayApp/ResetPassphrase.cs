using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using Novell.iFolderCom;

namespace TrayApp
{
	/// <summary>
	/// Summary description for ResetPassphrase.
	/// </summary>
	public class ResetPassphrase : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label accountLabel;
		private System.Windows.Forms.Label passphraseLabel;
		private System.Windows.Forms.Label newPassphraseLabel;
		private System.Windows.Forms.Label retypePassphraseLabel;
		private System.Windows.Forms.Label recoveryAgentLabel;
		private System.Windows.Forms.ComboBox DomainComboBox;
		private System.Windows.Forms.TextBox passPhrase;
		private System.Windows.Forms.TextBox newPassphrase;
		private System.Windows.Forms.TextBox retypePassphrase;
		private System.Windows.Forms.ComboBox recoveryAgentCombo;
		private System.Windows.Forms.CheckBox rememberPassphrase;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnReset;
		private SimiasWebService simws;
		private string domainID;
		private DomainItem selectedDomain;
		private bool success;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SimiasWebService simiasWebservice
		{
			set
			{
				this.simws = value;
			}
		}

		public bool Success 
		{
			get
			{
				return this.success;
			}
		}

		public ResetPassphrase()
		{
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
			this.accountLabel = new System.Windows.Forms.Label();
			this.passphraseLabel = new System.Windows.Forms.Label();
			this.newPassphraseLabel = new System.Windows.Forms.Label();
			this.retypePassphraseLabel = new System.Windows.Forms.Label();
			this.recoveryAgentLabel = new System.Windows.Forms.Label();
			this.DomainComboBox = new System.Windows.Forms.ComboBox();
			this.passPhrase = new System.Windows.Forms.TextBox();
			this.newPassphrase = new System.Windows.Forms.TextBox();
			this.retypePassphrase = new System.Windows.Forms.TextBox();
			this.recoveryAgentCombo = new System.Windows.Forms.ComboBox();
			this.rememberPassphrase = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
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
			// accountLabel
			// 
			this.accountLabel.Location = new System.Drawing.Point(16, 76);
			this.accountLabel.Name = "accountLabel";
			this.accountLabel.Size = new System.Drawing.Size(140, 20);
			this.accountLabel.TabIndex = 0;
			this.accountLabel.Text = "iFolder Account:";
			this.accountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// passphraseLabel
			// 
			this.passphraseLabel.Location = new System.Drawing.Point(16, 104);
			this.passphraseLabel.Name = "passphraseLabel";
			this.passphraseLabel.Size = new System.Drawing.Size(140, 20);
			this.passphraseLabel.TabIndex = 1;
			this.passphraseLabel.Text = "Enter Passphrase:";
			this.passphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// newPassphraseLabel
			// 
			this.newPassphraseLabel.Location = new System.Drawing.Point(16, 132);
			this.newPassphraseLabel.Name = "newPassphraseLabel";
			this.newPassphraseLabel.Size = new System.Drawing.Size(140, 20);
			this.newPassphraseLabel.TabIndex = 2;
			this.newPassphraseLabel.Text = "Enter New Passphrase:";
			this.newPassphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.newPassphraseLabel.Click += new System.EventHandler(this.newPassphraseLabel_Click);
			// 
			// retypePassphraseLabel
			// 
			this.retypePassphraseLabel.Location = new System.Drawing.Point(16, 160);
			this.retypePassphraseLabel.Name = "retypePassphraseLabel";
			this.retypePassphraseLabel.Size = new System.Drawing.Size(140, 20);
			this.retypePassphraseLabel.TabIndex = 3;
			this.retypePassphraseLabel.Text = "Retype Passphrase:";
			this.retypePassphraseLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// recoveryAgentLabel
			// 
			this.recoveryAgentLabel.Location = new System.Drawing.Point(16, 188);
			this.recoveryAgentLabel.Name = "recoveryAgentLabel";
			this.recoveryAgentLabel.Size = new System.Drawing.Size(140, 20);
			this.recoveryAgentLabel.TabIndex = 4;
			this.recoveryAgentLabel.Text = "Recovery Agent:";
			this.recoveryAgentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DomainComboBox
			// 
			this.DomainComboBox.Location = new System.Drawing.Point(176, 76);
			this.DomainComboBox.Name = "DomainComboBox";
			this.DomainComboBox.Size = new System.Drawing.Size(240, 21);
			this.DomainComboBox.TabIndex = 5;
			// 
			// passPhrase
			// 
			this.passPhrase.Location = new System.Drawing.Point(176, 104);
			this.passPhrase.Name = "passPhrase";
			this.passPhrase.Size = new System.Drawing.Size(240, 20);
			this.passPhrase.TabIndex = 6;
			this.passPhrase.Text = "";
			this.passPhrase.TextChanged += new System.EventHandler(this.passPhrase_TextChanged);
			// 
			// newPassphrase
			// 
			this.newPassphrase.Location = new System.Drawing.Point(176, 132);
			this.newPassphrase.Name = "newPassphrase";
			this.newPassphrase.Size = new System.Drawing.Size(240, 20);
			this.newPassphrase.TabIndex = 7;
			this.newPassphrase.Text = "";
			this.newPassphrase.TextChanged += new System.EventHandler(this.newPassphrase_TextChanged);
			// 
			// retypePassphrase
			// 
			this.retypePassphrase.Location = new System.Drawing.Point(176, 160);
			this.retypePassphrase.Name = "retypePassphrase";
			this.retypePassphrase.Size = new System.Drawing.Size(240, 20);
			this.retypePassphrase.TabIndex = 8;
			this.retypePassphrase.Text = "";
			this.retypePassphrase.TextChanged += new System.EventHandler(this.retypePassphrase_TextChanged);
			// 
			// recoveryAgentCombo
			// 
			this.recoveryAgentCombo.Location = new System.Drawing.Point(176, 188);
			this.recoveryAgentCombo.Name = "recoveryAgentCombo";
			this.recoveryAgentCombo.Size = new System.Drawing.Size(240, 21);
			this.recoveryAgentCombo.TabIndex = 9;
			// 
			// rememberPassphrase
			// 
			this.rememberPassphrase.Location = new System.Drawing.Point(176, 216);
			this.rememberPassphrase.Name = "rememberPassphrase";
			this.rememberPassphrase.Size = new System.Drawing.Size(180, 20);
			this.rememberPassphrase.TabIndex = 10;
			this.rememberPassphrase.Text = "Remember Passphrase";
			// 
			// btnCancel
			// 
			this.btnCancel.Location = new System.Drawing.Point(252, 256);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnReset
			// 
			this.btnReset.Location = new System.Drawing.Point(344, 256);
			this.btnReset.Name = "btnReset";
			this.btnReset.TabIndex = 12;
			this.btnReset.Text = "&Reset";
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// ResetPassphrase
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(440, 290);
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.rememberPassphrase);
			this.Controls.Add(this.recoveryAgentCombo);
			this.Controls.Add(this.retypePassphrase);
			this.Controls.Add(this.newPassphrase);
			this.Controls.Add(this.passPhrase);
			this.Controls.Add(this.DomainComboBox);
			this.Controls.Add(this.recoveryAgentLabel);
			this.Controls.Add(this.retypePassphraseLabel);
			this.Controls.Add(this.newPassphraseLabel);
			this.Controls.Add(this.passphraseLabel);
			this.Controls.Add(this.accountLabel);
			this.Controls.Add(this.panel1);
			this.Name = "ResetPassphrase";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Reset Passphrase";
			this.Load += new System.EventHandler(this.ResetPassphrase_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void newPassphrase_TextChanged(object sender, System.EventArgs e)
		{			
			UpdateSensitivity();			
		}

		private void newPassphraseLabel_Click(object sender, System.EventArgs e)
		{
		
		}

		private void ResetPassphrase_Load(object sender, System.EventArgs e)
		{
			this.btnReset.Enabled = false;
			if (this.DomainComboBox.Items.Count == 0)
			{
				try
				{
					XmlDocument domainsDoc = new XmlDocument();
					domainsDoc.Load(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "domain.list"));

					XmlElement element = (XmlElement)domainsDoc.SelectSingleNode("/domains");

					// Get the ID of the default domain.
					XmlElement defaultDomainElement = (XmlElement)domainsDoc.SelectSingleNode("/domains/defaultDomain");
					string defaultID = defaultDomainElement.GetAttribute("ID");

					// Get the domains.
					// Look for a domain with this ID.
					XmlNodeList nodeList = element.GetElementsByTagName("domain");
					foreach (XmlNode node in nodeList)
					{
						string name = ((XmlElement)node).GetAttribute("name");
						string id = ((XmlElement)node).GetAttribute("ID");

						DomainItem domainItem = new DomainItem(name, id);
						this.DomainComboBox.Items.Add(domainItem);
						if (id.Equals(defaultID))
						{
							selectedDomain = domainItem;
						}
					}

					if (selectedDomain != null)
					{
						this.DomainComboBox.SelectedItem = selectedDomain;
					}
					else
						this.DomainComboBox.SelectedIndex = 0;
				}
				catch
				{
				}
			}
			else
			{
				if (selectedDomain != null)
				{
					this.DomainComboBox.SelectedItem = selectedDomain;
				}
				else if (this.DomainComboBox.Items.Count > 0)
				{
					this.DomainComboBox.SelectedIndex = 0;
				}
			}
		}

		private void btnReset_Click(object sender, System.EventArgs e)
		{
			// assign domain id.
			DomainItem domainItem = (DomainItem)this.DomainComboBox.SelectedItem;
			this.domainID = domainItem.ID;
			Status status = this.simws.ReSetPassPhrase(this.domainID, this.passPhrase.Text , this.newPassphrase.Text, "RAName", "Public Key");
			if( status.statusCode == StatusCodes.Success)
			{
				this.simws.StorePassPhrase(this.domainID, this.newPassphrase.Text, CredentialType.Basic, this.rememberPassphrase.Checked);
				// successful..
				this.success = true;
				this.Dispose();
				this.Close();
			}
			else
			{
				this.success = false;
				// Unable to reset.
			}
			this.Show();
		}

		private void passPhrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void retypePassphrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.success = false;
			this.Dispose();
			this.Close();
		}

		private void UpdateSensitivity()
		{
			if( this.passPhrase.Text.Length > 0 &&
				this.newPassphrase.Text.Length > 0 && 
				this.newPassphrase.Text == this.retypePassphrase.Text)
				this.btnReset.Enabled = true;
			else
				this.btnReset.Enabled = false;

		}
	}
}
