using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Collections;

namespace Novell.Wizard
{
	public partial class ImportKeyPage
	{
		#region Windows Form Designer generated code

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label filePathLabel;
        private System.Windows.Forms.TextBox LocationEntry;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.Label oneTimePassphraseLabel;
        private System.Windows.Forms.TextBox oneTimePassphrase;
        private System.Windows.Forms.Label passphraseLabel;
        private System.Windows.Forms.TextBox passphrase;
        private System.Windows.Forms.Label retypePassphraseLabel;
        private System.Windows.Forms.TextBox reTypePassphrase;
       

		
		private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.filePathLabel = new System.Windows.Forms.Label();
            this.LocationEntry = new System.Windows.Forms.TextBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.oneTimePassphraseLabel = new System.Windows.Forms.Label();
            this.oneTimePassphrase = new System.Windows.Forms.TextBox();
            this.passphraseLabel = new System.Windows.Forms.Label();
            this.passphrase = new System.Windows.Forms.TextBox();
            this.retypePassphraseLabel = new System.Windows.Forms.Label();
            this.reTypePassphrase = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.isEncrypted = new System.Windows.Forms.CheckBox();
            this.accountBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(40, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = TrayApp.Properties.Resources.iFolderAcc;
            // 
            // filePathLabel
            // 
            this.filePathLabel.AutoSize = true;
            this.filePathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filePathLabel.Location = new System.Drawing.Point(40, 125);
            this.filePathLabel.Name = "filePathLabel";
            this.filePathLabel.Size = new System.Drawing.Size(144, 13);
            this.filePathLabel.TabIndex = 2;
            this.filePathLabel.Text = TrayApp.Properties.Resources.importPageFilePathOne;
            // 
            // LocationEntry
            // 
            this.LocationEntry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LocationEntry.Location = new System.Drawing.Point(193, 118);
            this.LocationEntry.Name = "LocationEntry";
            this.LocationEntry.Size = new System.Drawing.Size(220, 20);
            this.LocationEntry.TabIndex = 3;
            this.LocationEntry.TextChanged += new System.EventHandler(this.LocationEntry_TextChanged);
            // 
            // BrowseButton
            // 
            this.BrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BrowseButton.Location = new System.Drawing.Point(419, 118);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(55, 20);
            this.BrowseButton.TabIndex = 4;
            this.BrowseButton.Text = global::TrayApp.Properties.Resources.browseText;
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // oneTimePassphraseLabel
            // 
            this.oneTimePassphraseLabel.AutoSize = true;
            this.oneTimePassphraseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.oneTimePassphraseLabel.Location = new System.Drawing.Point(40, 176);
            this.oneTimePassphraseLabel.Name = "oneTimePassphraseLabel";
            this.oneTimePassphraseLabel.Size = new System.Drawing.Size(106, 13);
            this.oneTimePassphraseLabel.TabIndex = 5;
            this.oneTimePassphraseLabel.Text = TrayApp.Properties.Resources.importPageOTP;
            // 
            // oneTimePassphrase
            // 
            this.oneTimePassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.oneTimePassphrase.Location = new System.Drawing.Point(193, 173);
            this.oneTimePassphrase.Name = "oneTimePassphrase";
            this.oneTimePassphrase.PasswordChar = '*';
            this.oneTimePassphrase.Size = new System.Drawing.Size(220, 20);
            this.oneTimePassphrase.TabIndex = 6;
            this.oneTimePassphrase.TextChanged += new System.EventHandler(this.oneTimePassphrase_TextChanged);
            // 
            // passphraseLabel
            // 
            this.passphraseLabel.AutoSize = true;
            this.passphraseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passphraseLabel.Location = new System.Drawing.Point(40, 211);
            this.passphraseLabel.Name = "passphraseLabel";
            this.passphraseLabel.Size = new System.Drawing.Size(89, 13);
            this.passphraseLabel.TabIndex = 7;
            this.passphraseLabel.Text = TrayApp.Properties.Resources.newPassphrase;
            // 
            // passphrase
            // 
            this.passphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passphrase.Location = new System.Drawing.Point(193, 204);
            this.passphrase.Name = "passphrase";
            this.passphrase.PasswordChar = '*';
            this.passphrase.Size = new System.Drawing.Size(220, 20);
            this.passphrase.TabIndex = 8;
            this.passphrase.TextChanged += new System.EventHandler(this.passphrase_TextChanged);
            // 
            // retypePassphraseLabel
            // 
            this.retypePassphraseLabel.AutoSize = true;
            this.retypePassphraseLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.retypePassphraseLabel.Location = new System.Drawing.Point(40, 247);
            this.retypePassphraseLabel.Name = "retypePassphraseLabel";
            this.retypePassphraseLabel.Size = new System.Drawing.Size(102, 13);
            this.retypePassphraseLabel.TabIndex = 9;
            this.retypePassphraseLabel.Text = TrayApp.Properties.Resources.confirmPassphrase;
            // 
            // reTypePassphrase
            // 
            this.reTypePassphrase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reTypePassphrase.Location = new System.Drawing.Point(193, 240);
            this.reTypePassphrase.Name = "reTypePassphrase";
            this.reTypePassphrase.PasswordChar = '*';
            this.reTypePassphrase.Size = new System.Drawing.Size(220, 20);
            this.reTypePassphrase.TabIndex = 10;
            this.reTypePassphrase.TextChanged += new System.EventHandler(this.reTypePassphrase_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(379, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = TrayApp.Properties.Resources.importPageFirstLine;
            // isEncrypted
            // 
            this.isEncrypted.AutoSize = true;
            this.isEncrypted.Location = new System.Drawing.Point(193, 150);
            this.isEncrypted.Name = "isEncrypted";
            this.isEncrypted.Size = new System.Drawing.Size(157, 17);
            this.isEncrypted.TabIndex = 13;
            this.isEncrypted.Text = global::TrayApp.Properties.Resources.importPageEncryCheck;
            this.isEncrypted.UseVisualStyleBackColor = true;
            this.isEncrypted.CheckedChanged += new System.EventHandler(this.isEncrypted_CheckedChanged);
            // 
            // accountBox
            // 
            this.accountBox.Location = new System.Drawing.Point(193, 88);
            this.accountBox.Name = "accountBox";
            this.accountBox.ReadOnly = true;
            this.accountBox.Size = new System.Drawing.Size(220, 20);
            this.accountBox.TabIndex = 14;
            // 
            // ImportKeyPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.accountBox);
            this.Controls.Add(this.isEncrypted);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.retypePassphraseLabel);
            this.Controls.Add(this.passphrase);
            this.Controls.Add(this.oneTimePassphrase);
            this.Controls.Add(this.passphraseLabel);
            this.Controls.Add(this.reTypePassphrase);
            this.Controls.Add(this.oneTimePassphraseLabel);
            this.Controls.Add(this.LocationEntry);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.filePathLabel);
            this.Name = "ImportKeyPage";
            this.Size = new System.Drawing.Size(513, 295);
            this.Controls.SetChildIndex(this.filePathLabel, 0);
            this.Controls.SetChildIndex(this.BrowseButton, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.LocationEntry, 0);
            this.Controls.SetChildIndex(this.oneTimePassphraseLabel, 0);
            this.Controls.SetChildIndex(this.reTypePassphrase, 0);
            this.Controls.SetChildIndex(this.passphraseLabel, 0);
            this.Controls.SetChildIndex(this.oneTimePassphrase, 0);
            this.Controls.SetChildIndex(this.passphrase, 0);
            this.Controls.SetChildIndex(this.retypePassphraseLabel, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.isEncrypted, 0);
            this.Controls.SetChildIndex(this.accountBox, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
		#endregion

		protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private CheckBox isEncrypted;
        private TextBox accountBox;
	}
}
