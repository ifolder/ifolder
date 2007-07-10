using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using Novell.iFolderCom;

namespace Novell.FormsTrayApp
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
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));
		private System.Windows.Forms.PictureBox pictureBox1;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ResetPassphrase));
			this.panel1 = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
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
			this.panel1.AccessibleDescription = resources.GetString("panel1.AccessibleDescription");
			this.panel1.AccessibleName = resources.GetString("panel1.AccessibleName");
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("panel1.Anchor")));
			this.panel1.AutoScroll = ((bool)(resources.GetObject("panel1.AutoScroll")));
			this.panel1.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMargin")));
			this.panel1.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("panel1.AutoScrollMinSize")));
			this.panel1.BackColor = System.Drawing.Color.Transparent;
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
			this.panel1.Controls.Add(this.pictureBox1);
			this.panel1.Controls.Add(this.waterMark);
			this.panel1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("panel1.Dock")));
			this.panel1.Enabled = ((bool)(resources.GetObject("panel1.Enabled")));
			this.panel1.Font = ((System.Drawing.Font)(resources.GetObject("panel1.Font")));
			this.panel1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("panel1.ImeMode")));
			this.panel1.Location = ((System.Drawing.Point)(resources.GetObject("panel1.Location")));
			this.panel1.Name = "panel1";
			this.panel1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("panel1.RightToLeft")));
			this.panel1.Size = ((System.Drawing.Size)(resources.GetObject("panel1.Size")));
			this.panel1.TabIndex = ((int)(resources.GetObject("panel1.TabIndex")));
			this.panel1.Text = resources.GetString("panel1.Text");
			this.panel1.Visible = ((bool)(resources.GetObject("panel1.Visible")));
			// 
			// pictureBox1
			// 
			this.pictureBox1.AccessibleDescription = resources.GetString("pictureBox1.AccessibleDescription");
			this.pictureBox1.AccessibleName = resources.GetString("pictureBox1.AccessibleName");
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("pictureBox1.Anchor")));
			this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
			this.pictureBox1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("pictureBox1.Dock")));
			this.pictureBox1.Enabled = ((bool)(resources.GetObject("pictureBox1.Enabled")));
			this.pictureBox1.Font = ((System.Drawing.Font)(resources.GetObject("pictureBox1.Font")));
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("pictureBox1.ImeMode")));
			this.pictureBox1.Location = ((System.Drawing.Point)(resources.GetObject("pictureBox1.Location")));
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("pictureBox1.RightToLeft")));
			this.pictureBox1.Size = ((System.Drawing.Size)(resources.GetObject("pictureBox1.Size")));
			this.pictureBox1.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("pictureBox1.SizeMode")));
			this.pictureBox1.TabIndex = ((int)(resources.GetObject("pictureBox1.TabIndex")));
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Text = resources.GetString("pictureBox1.Text");
			this.pictureBox1.Visible = ((bool)(resources.GetObject("pictureBox1.Visible")));
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// waterMark
			// 
			this.waterMark.AccessibleDescription = resources.GetString("waterMark.AccessibleDescription");
			this.waterMark.AccessibleName = resources.GetString("waterMark.AccessibleName");
			this.waterMark.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("waterMark.Anchor")));
			this.waterMark.BackColor = System.Drawing.Color.Transparent;
			this.waterMark.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("waterMark.BackgroundImage")));
			this.waterMark.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("waterMark.Dock")));
			this.waterMark.Enabled = ((bool)(resources.GetObject("waterMark.Enabled")));
			this.waterMark.Font = ((System.Drawing.Font)(resources.GetObject("waterMark.Font")));
			this.waterMark.Image = ((System.Drawing.Image)(resources.GetObject("waterMark.Image")));
			this.waterMark.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("waterMark.ImeMode")));
			this.waterMark.Location = ((System.Drawing.Point)(resources.GetObject("waterMark.Location")));
			this.waterMark.Name = "waterMark";
			this.waterMark.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("waterMark.RightToLeft")));
			this.waterMark.Size = ((System.Drawing.Size)(resources.GetObject("waterMark.Size")));
			this.waterMark.SizeMode = ((System.Windows.Forms.PictureBoxSizeMode)(resources.GetObject("waterMark.SizeMode")));
			this.waterMark.TabIndex = ((int)(resources.GetObject("waterMark.TabIndex")));
			this.waterMark.TabStop = false;
			this.waterMark.Text = resources.GetString("waterMark.Text");
			this.waterMark.Visible = ((bool)(resources.GetObject("waterMark.Visible")));
			// 
			// accountLabel
			// 
			this.accountLabel.AccessibleDescription = resources.GetString("accountLabel.AccessibleDescription");
			this.accountLabel.AccessibleName = resources.GetString("accountLabel.AccessibleName");
			this.accountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("accountLabel.Anchor")));
			this.accountLabel.AutoSize = ((bool)(resources.GetObject("accountLabel.AutoSize")));
			this.accountLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("accountLabel.Dock")));
			this.accountLabel.Enabled = ((bool)(resources.GetObject("accountLabel.Enabled")));
			this.accountLabel.Font = ((System.Drawing.Font)(resources.GetObject("accountLabel.Font")));
			this.accountLabel.Image = ((System.Drawing.Image)(resources.GetObject("accountLabel.Image")));
			this.accountLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("accountLabel.ImageAlign")));
			this.accountLabel.ImageIndex = ((int)(resources.GetObject("accountLabel.ImageIndex")));
			this.accountLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("accountLabel.ImeMode")));
			this.accountLabel.Location = ((System.Drawing.Point)(resources.GetObject("accountLabel.Location")));
			this.accountLabel.Name = "accountLabel";
			this.accountLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("accountLabel.RightToLeft")));
			this.accountLabel.Size = ((System.Drawing.Size)(resources.GetObject("accountLabel.Size")));
			this.accountLabel.TabIndex = ((int)(resources.GetObject("accountLabel.TabIndex")));
			this.accountLabel.Text = resources.GetString("accountLabel.Text");
			this.accountLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("accountLabel.TextAlign")));
			this.accountLabel.Visible = ((bool)(resources.GetObject("accountLabel.Visible")));
			// 
			// passphraseLabel
			// 
			this.passphraseLabel.AccessibleDescription = resources.GetString("passphraseLabel.AccessibleDescription");
			this.passphraseLabel.AccessibleName = resources.GetString("passphraseLabel.AccessibleName");
			this.passphraseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("passphraseLabel.Anchor")));
			this.passphraseLabel.AutoSize = ((bool)(resources.GetObject("passphraseLabel.AutoSize")));
			this.passphraseLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("passphraseLabel.Dock")));
			this.passphraseLabel.Enabled = ((bool)(resources.GetObject("passphraseLabel.Enabled")));
			this.passphraseLabel.Font = ((System.Drawing.Font)(resources.GetObject("passphraseLabel.Font")));
			this.passphraseLabel.Image = ((System.Drawing.Image)(resources.GetObject("passphraseLabel.Image")));
			this.passphraseLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passphraseLabel.ImageAlign")));
			this.passphraseLabel.ImageIndex = ((int)(resources.GetObject("passphraseLabel.ImageIndex")));
			this.passphraseLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("passphraseLabel.ImeMode")));
			this.passphraseLabel.Location = ((System.Drawing.Point)(resources.GetObject("passphraseLabel.Location")));
			this.passphraseLabel.Name = "passphraseLabel";
			this.passphraseLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("passphraseLabel.RightToLeft")));
			this.passphraseLabel.Size = ((System.Drawing.Size)(resources.GetObject("passphraseLabel.Size")));
			this.passphraseLabel.TabIndex = ((int)(resources.GetObject("passphraseLabel.TabIndex")));
			this.passphraseLabel.Text = resources.GetString("passphraseLabel.Text");
			this.passphraseLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passphraseLabel.TextAlign")));
			this.passphraseLabel.Visible = ((bool)(resources.GetObject("passphraseLabel.Visible")));
			// 
			// newPassphraseLabel
			// 
			this.newPassphraseLabel.AccessibleDescription = resources.GetString("newPassphraseLabel.AccessibleDescription");
			this.newPassphraseLabel.AccessibleName = resources.GetString("newPassphraseLabel.AccessibleName");
			this.newPassphraseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("newPassphraseLabel.Anchor")));
			this.newPassphraseLabel.AutoSize = ((bool)(resources.GetObject("newPassphraseLabel.AutoSize")));
			this.newPassphraseLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("newPassphraseLabel.Dock")));
			this.newPassphraseLabel.Enabled = ((bool)(resources.GetObject("newPassphraseLabel.Enabled")));
			this.newPassphraseLabel.Font = ((System.Drawing.Font)(resources.GetObject("newPassphraseLabel.Font")));
			this.newPassphraseLabel.Image = ((System.Drawing.Image)(resources.GetObject("newPassphraseLabel.Image")));
			this.newPassphraseLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("newPassphraseLabel.ImageAlign")));
			this.newPassphraseLabel.ImageIndex = ((int)(resources.GetObject("newPassphraseLabel.ImageIndex")));
			this.newPassphraseLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("newPassphraseLabel.ImeMode")));
			this.newPassphraseLabel.Location = ((System.Drawing.Point)(resources.GetObject("newPassphraseLabel.Location")));
			this.newPassphraseLabel.Name = "newPassphraseLabel";
			this.newPassphraseLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("newPassphraseLabel.RightToLeft")));
			this.newPassphraseLabel.Size = ((System.Drawing.Size)(resources.GetObject("newPassphraseLabel.Size")));
			this.newPassphraseLabel.TabIndex = ((int)(resources.GetObject("newPassphraseLabel.TabIndex")));
			this.newPassphraseLabel.Text = resources.GetString("newPassphraseLabel.Text");
			this.newPassphraseLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("newPassphraseLabel.TextAlign")));
			this.newPassphraseLabel.Visible = ((bool)(resources.GetObject("newPassphraseLabel.Visible")));
			this.newPassphraseLabel.Click += new System.EventHandler(this.newPassphraseLabel_Click);
			// 
			// retypePassphraseLabel
			// 
			this.retypePassphraseLabel.AccessibleDescription = resources.GetString("retypePassphraseLabel.AccessibleDescription");
			this.retypePassphraseLabel.AccessibleName = resources.GetString("retypePassphraseLabel.AccessibleName");
			this.retypePassphraseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("retypePassphraseLabel.Anchor")));
			this.retypePassphraseLabel.AutoSize = ((bool)(resources.GetObject("retypePassphraseLabel.AutoSize")));
			this.retypePassphraseLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("retypePassphraseLabel.Dock")));
			this.retypePassphraseLabel.Enabled = ((bool)(resources.GetObject("retypePassphraseLabel.Enabled")));
			this.retypePassphraseLabel.Font = ((System.Drawing.Font)(resources.GetObject("retypePassphraseLabel.Font")));
			this.retypePassphraseLabel.Image = ((System.Drawing.Image)(resources.GetObject("retypePassphraseLabel.Image")));
			this.retypePassphraseLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("retypePassphraseLabel.ImageAlign")));
			this.retypePassphraseLabel.ImageIndex = ((int)(resources.GetObject("retypePassphraseLabel.ImageIndex")));
			this.retypePassphraseLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("retypePassphraseLabel.ImeMode")));
			this.retypePassphraseLabel.Location = ((System.Drawing.Point)(resources.GetObject("retypePassphraseLabel.Location")));
			this.retypePassphraseLabel.Name = "retypePassphraseLabel";
			this.retypePassphraseLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("retypePassphraseLabel.RightToLeft")));
			this.retypePassphraseLabel.Size = ((System.Drawing.Size)(resources.GetObject("retypePassphraseLabel.Size")));
			this.retypePassphraseLabel.TabIndex = ((int)(resources.GetObject("retypePassphraseLabel.TabIndex")));
			this.retypePassphraseLabel.Text = resources.GetString("retypePassphraseLabel.Text");
			this.retypePassphraseLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("retypePassphraseLabel.TextAlign")));
			this.retypePassphraseLabel.Visible = ((bool)(resources.GetObject("retypePassphraseLabel.Visible")));
			// 
			// recoveryAgentLabel
			// 
			this.recoveryAgentLabel.AccessibleDescription = resources.GetString("recoveryAgentLabel.AccessibleDescription");
			this.recoveryAgentLabel.AccessibleName = resources.GetString("recoveryAgentLabel.AccessibleName");
			this.recoveryAgentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("recoveryAgentLabel.Anchor")));
			this.recoveryAgentLabel.AutoSize = ((bool)(resources.GetObject("recoveryAgentLabel.AutoSize")));
			this.recoveryAgentLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("recoveryAgentLabel.Dock")));
			this.recoveryAgentLabel.Enabled = ((bool)(resources.GetObject("recoveryAgentLabel.Enabled")));
			this.recoveryAgentLabel.Font = ((System.Drawing.Font)(resources.GetObject("recoveryAgentLabel.Font")));
			this.recoveryAgentLabel.Image = ((System.Drawing.Image)(resources.GetObject("recoveryAgentLabel.Image")));
			this.recoveryAgentLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("recoveryAgentLabel.ImageAlign")));
			this.recoveryAgentLabel.ImageIndex = ((int)(resources.GetObject("recoveryAgentLabel.ImageIndex")));
			this.recoveryAgentLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("recoveryAgentLabel.ImeMode")));
			this.recoveryAgentLabel.Location = ((System.Drawing.Point)(resources.GetObject("recoveryAgentLabel.Location")));
			this.recoveryAgentLabel.Name = "recoveryAgentLabel";
			this.recoveryAgentLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("recoveryAgentLabel.RightToLeft")));
			this.recoveryAgentLabel.Size = ((System.Drawing.Size)(resources.GetObject("recoveryAgentLabel.Size")));
			this.recoveryAgentLabel.TabIndex = ((int)(resources.GetObject("recoveryAgentLabel.TabIndex")));
			this.recoveryAgentLabel.Text = resources.GetString("recoveryAgentLabel.Text");
			this.recoveryAgentLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("recoveryAgentLabel.TextAlign")));
			this.recoveryAgentLabel.Visible = ((bool)(resources.GetObject("recoveryAgentLabel.Visible")));
			// 
			// DomainComboBox
			// 
			this.DomainComboBox.AccessibleDescription = resources.GetString("DomainComboBox.AccessibleDescription");
			this.DomainComboBox.AccessibleName = resources.GetString("DomainComboBox.AccessibleName");
			this.DomainComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("DomainComboBox.Anchor")));
			this.DomainComboBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("DomainComboBox.BackgroundImage")));
			this.DomainComboBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("DomainComboBox.Dock")));
			this.DomainComboBox.Enabled = ((bool)(resources.GetObject("DomainComboBox.Enabled")));
			this.DomainComboBox.Font = ((System.Drawing.Font)(resources.GetObject("DomainComboBox.Font")));
			this.DomainComboBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("DomainComboBox.ImeMode")));
			this.DomainComboBox.IntegralHeight = ((bool)(resources.GetObject("DomainComboBox.IntegralHeight")));
			this.DomainComboBox.ItemHeight = ((int)(resources.GetObject("DomainComboBox.ItemHeight")));
			this.DomainComboBox.Location = ((System.Drawing.Point)(resources.GetObject("DomainComboBox.Location")));
			this.DomainComboBox.MaxDropDownItems = ((int)(resources.GetObject("DomainComboBox.MaxDropDownItems")));
			this.DomainComboBox.MaxLength = ((int)(resources.GetObject("DomainComboBox.MaxLength")));
			this.DomainComboBox.Name = "DomainComboBox";
			this.DomainComboBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("DomainComboBox.RightToLeft")));
			this.DomainComboBox.Size = ((System.Drawing.Size)(resources.GetObject("DomainComboBox.Size")));
			this.DomainComboBox.TabIndex = ((int)(resources.GetObject("DomainComboBox.TabIndex")));
			this.DomainComboBox.Text = resources.GetString("DomainComboBox.Text");
			this.DomainComboBox.Visible = ((bool)(resources.GetObject("DomainComboBox.Visible")));
			this.DomainComboBox.SelectedIndexChanged += new System.EventHandler(this.DomainComboBox_SelectedIndexChanged);
			// 
			// passPhrase
			// 
			this.passPhrase.AccessibleDescription = resources.GetString("passPhrase.AccessibleDescription");
			this.passPhrase.AccessibleName = resources.GetString("passPhrase.AccessibleName");
			this.passPhrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("passPhrase.Anchor")));
			this.passPhrase.AutoSize = ((bool)(resources.GetObject("passPhrase.AutoSize")));
			this.passPhrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("passPhrase.BackgroundImage")));
			this.passPhrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("passPhrase.Dock")));
			this.passPhrase.Enabled = ((bool)(resources.GetObject("passPhrase.Enabled")));
			this.passPhrase.Font = ((System.Drawing.Font)(resources.GetObject("passPhrase.Font")));
			this.passPhrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("passPhrase.ImeMode")));
			this.passPhrase.Location = ((System.Drawing.Point)(resources.GetObject("passPhrase.Location")));
			this.passPhrase.MaxLength = ((int)(resources.GetObject("passPhrase.MaxLength")));
			this.passPhrase.Multiline = ((bool)(resources.GetObject("passPhrase.Multiline")));
			this.passPhrase.Name = "passPhrase";
			this.passPhrase.PasswordChar = ((char)(resources.GetObject("passPhrase.PasswordChar")));
			this.passPhrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("passPhrase.RightToLeft")));
			this.passPhrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("passPhrase.ScrollBars")));
			this.passPhrase.Size = ((System.Drawing.Size)(resources.GetObject("passPhrase.Size")));
			this.passPhrase.TabIndex = ((int)(resources.GetObject("passPhrase.TabIndex")));
			this.passPhrase.Text = resources.GetString("passPhrase.Text");
			this.passPhrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("passPhrase.TextAlign")));
			this.passPhrase.Visible = ((bool)(resources.GetObject("passPhrase.Visible")));
			this.passPhrase.WordWrap = ((bool)(resources.GetObject("passPhrase.WordWrap")));
			this.passPhrase.TextChanged += new System.EventHandler(this.passPhrase_TextChanged);
			// 
			// newPassphrase
			// 
			this.newPassphrase.AccessibleDescription = resources.GetString("newPassphrase.AccessibleDescription");
			this.newPassphrase.AccessibleName = resources.GetString("newPassphrase.AccessibleName");
			this.newPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("newPassphrase.Anchor")));
			this.newPassphrase.AutoSize = ((bool)(resources.GetObject("newPassphrase.AutoSize")));
			this.newPassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("newPassphrase.BackgroundImage")));
			this.newPassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("newPassphrase.Dock")));
			this.newPassphrase.Enabled = ((bool)(resources.GetObject("newPassphrase.Enabled")));
			this.newPassphrase.Font = ((System.Drawing.Font)(resources.GetObject("newPassphrase.Font")));
			this.newPassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("newPassphrase.ImeMode")));
			this.newPassphrase.Location = ((System.Drawing.Point)(resources.GetObject("newPassphrase.Location")));
			this.newPassphrase.MaxLength = ((int)(resources.GetObject("newPassphrase.MaxLength")));
			this.newPassphrase.Multiline = ((bool)(resources.GetObject("newPassphrase.Multiline")));
			this.newPassphrase.Name = "newPassphrase";
			this.newPassphrase.PasswordChar = ((char)(resources.GetObject("newPassphrase.PasswordChar")));
			this.newPassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("newPassphrase.RightToLeft")));
			this.newPassphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("newPassphrase.ScrollBars")));
			this.newPassphrase.Size = ((System.Drawing.Size)(resources.GetObject("newPassphrase.Size")));
			this.newPassphrase.TabIndex = ((int)(resources.GetObject("newPassphrase.TabIndex")));
			this.newPassphrase.Text = resources.GetString("newPassphrase.Text");
			this.newPassphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("newPassphrase.TextAlign")));
			this.newPassphrase.Visible = ((bool)(resources.GetObject("newPassphrase.Visible")));
			this.newPassphrase.WordWrap = ((bool)(resources.GetObject("newPassphrase.WordWrap")));
			this.newPassphrase.TextChanged += new System.EventHandler(this.newPassphrase_TextChanged);
			// 
			// retypePassphrase
			// 
			this.retypePassphrase.AccessibleDescription = resources.GetString("retypePassphrase.AccessibleDescription");
			this.retypePassphrase.AccessibleName = resources.GetString("retypePassphrase.AccessibleName");
			this.retypePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("retypePassphrase.Anchor")));
			this.retypePassphrase.AutoSize = ((bool)(resources.GetObject("retypePassphrase.AutoSize")));
			this.retypePassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("retypePassphrase.BackgroundImage")));
			this.retypePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("retypePassphrase.Dock")));
			this.retypePassphrase.Enabled = ((bool)(resources.GetObject("retypePassphrase.Enabled")));
			this.retypePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("retypePassphrase.Font")));
			this.retypePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("retypePassphrase.ImeMode")));
			this.retypePassphrase.Location = ((System.Drawing.Point)(resources.GetObject("retypePassphrase.Location")));
			this.retypePassphrase.MaxLength = ((int)(resources.GetObject("retypePassphrase.MaxLength")));
			this.retypePassphrase.Multiline = ((bool)(resources.GetObject("retypePassphrase.Multiline")));
			this.retypePassphrase.Name = "retypePassphrase";
			this.retypePassphrase.PasswordChar = ((char)(resources.GetObject("retypePassphrase.PasswordChar")));
			this.retypePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("retypePassphrase.RightToLeft")));
			this.retypePassphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("retypePassphrase.ScrollBars")));
			this.retypePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("retypePassphrase.Size")));
			this.retypePassphrase.TabIndex = ((int)(resources.GetObject("retypePassphrase.TabIndex")));
			this.retypePassphrase.Text = resources.GetString("retypePassphrase.Text");
			this.retypePassphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("retypePassphrase.TextAlign")));
			this.retypePassphrase.Visible = ((bool)(resources.GetObject("retypePassphrase.Visible")));
			this.retypePassphrase.WordWrap = ((bool)(resources.GetObject("retypePassphrase.WordWrap")));
			this.retypePassphrase.TextChanged += new System.EventHandler(this.retypePassphrase_TextChanged);
			// 
			// recoveryAgentCombo
			// 
			this.recoveryAgentCombo.AccessibleDescription = resources.GetString("recoveryAgentCombo.AccessibleDescription");
			this.recoveryAgentCombo.AccessibleName = resources.GetString("recoveryAgentCombo.AccessibleName");
			this.recoveryAgentCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("recoveryAgentCombo.Anchor")));
			this.recoveryAgentCombo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("recoveryAgentCombo.BackgroundImage")));
			this.recoveryAgentCombo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("recoveryAgentCombo.Dock")));
			this.recoveryAgentCombo.Enabled = ((bool)(resources.GetObject("recoveryAgentCombo.Enabled")));
			this.recoveryAgentCombo.Font = ((System.Drawing.Font)(resources.GetObject("recoveryAgentCombo.Font")));
			this.recoveryAgentCombo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("recoveryAgentCombo.ImeMode")));
			this.recoveryAgentCombo.IntegralHeight = ((bool)(resources.GetObject("recoveryAgentCombo.IntegralHeight")));
			this.recoveryAgentCombo.ItemHeight = ((int)(resources.GetObject("recoveryAgentCombo.ItemHeight")));
			this.recoveryAgentCombo.Location = ((System.Drawing.Point)(resources.GetObject("recoveryAgentCombo.Location")));
			this.recoveryAgentCombo.MaxDropDownItems = ((int)(resources.GetObject("recoveryAgentCombo.MaxDropDownItems")));
			this.recoveryAgentCombo.MaxLength = ((int)(resources.GetObject("recoveryAgentCombo.MaxLength")));
			this.recoveryAgentCombo.Name = "recoveryAgentCombo";
			this.recoveryAgentCombo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("recoveryAgentCombo.RightToLeft")));
			this.recoveryAgentCombo.Size = ((System.Drawing.Size)(resources.GetObject("recoveryAgentCombo.Size")));
			this.recoveryAgentCombo.TabIndex = ((int)(resources.GetObject("recoveryAgentCombo.TabIndex")));
			this.recoveryAgentCombo.Text = resources.GetString("recoveryAgentCombo.Text");
			this.recoveryAgentCombo.Visible = ((bool)(resources.GetObject("recoveryAgentCombo.Visible")));
			// 
			// rememberPassphrase
			// 
			this.rememberPassphrase.AccessibleDescription = resources.GetString("rememberPassphrase.AccessibleDescription");
			this.rememberPassphrase.AccessibleName = resources.GetString("rememberPassphrase.AccessibleName");
			this.rememberPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("rememberPassphrase.Anchor")));
			this.rememberPassphrase.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("rememberPassphrase.Appearance")));
			this.rememberPassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("rememberPassphrase.BackgroundImage")));
			this.rememberPassphrase.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassphrase.CheckAlign")));
			this.rememberPassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("rememberPassphrase.Dock")));
			this.rememberPassphrase.Enabled = ((bool)(resources.GetObject("rememberPassphrase.Enabled")));
			this.rememberPassphrase.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("rememberPassphrase.FlatStyle")));
			this.rememberPassphrase.Font = ((System.Drawing.Font)(resources.GetObject("rememberPassphrase.Font")));
			this.rememberPassphrase.Image = ((System.Drawing.Image)(resources.GetObject("rememberPassphrase.Image")));
			this.rememberPassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassphrase.ImageAlign")));
			this.rememberPassphrase.ImageIndex = ((int)(resources.GetObject("rememberPassphrase.ImageIndex")));
			this.rememberPassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("rememberPassphrase.ImeMode")));
			this.rememberPassphrase.Location = ((System.Drawing.Point)(resources.GetObject("rememberPassphrase.Location")));
			this.rememberPassphrase.Name = "rememberPassphrase";
			this.rememberPassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("rememberPassphrase.RightToLeft")));
			this.rememberPassphrase.Size = ((System.Drawing.Size)(resources.GetObject("rememberPassphrase.Size")));
			this.rememberPassphrase.TabIndex = ((int)(resources.GetObject("rememberPassphrase.TabIndex")));
			this.rememberPassphrase.Text = resources.GetString("rememberPassphrase.Text");
			this.rememberPassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("rememberPassphrase.TextAlign")));
			this.rememberPassphrase.Visible = ((bool)(resources.GetObject("rememberPassphrase.Visible")));
			// 
			// btnCancel
			// 
			this.btnCancel.AccessibleDescription = resources.GetString("btnCancel.AccessibleDescription");
			this.btnCancel.AccessibleName = resources.GetString("btnCancel.AccessibleName");
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnCancel.Anchor")));
			this.btnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCancel.BackgroundImage")));
			this.btnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnCancel.Dock")));
			this.btnCancel.Enabled = ((bool)(resources.GetObject("btnCancel.Enabled")));
			this.btnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnCancel.FlatStyle")));
			this.btnCancel.Font = ((System.Drawing.Font)(resources.GetObject("btnCancel.Font")));
			this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
			this.btnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.ImageAlign")));
			this.btnCancel.ImageIndex = ((int)(resources.GetObject("btnCancel.ImageIndex")));
			this.btnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnCancel.ImeMode")));
			this.btnCancel.Location = ((System.Drawing.Point)(resources.GetObject("btnCancel.Location")));
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnCancel.RightToLeft")));
			this.btnCancel.Size = ((System.Drawing.Size)(resources.GetObject("btnCancel.Size")));
			this.btnCancel.TabIndex = ((int)(resources.GetObject("btnCancel.TabIndex")));
			this.btnCancel.Text = resources.GetString("btnCancel.Text");
			this.btnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnCancel.TextAlign")));
			this.btnCancel.Visible = ((bool)(resources.GetObject("btnCancel.Visible")));
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnReset
			// 
			this.btnReset.AccessibleDescription = resources.GetString("btnReset.AccessibleDescription");
			this.btnReset.AccessibleName = resources.GetString("btnReset.AccessibleName");
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnReset.Anchor")));
			this.btnReset.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnReset.BackgroundImage")));
			this.btnReset.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnReset.Dock")));
			this.btnReset.Enabled = ((bool)(resources.GetObject("btnReset.Enabled")));
			this.btnReset.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnReset.FlatStyle")));
			this.btnReset.Font = ((System.Drawing.Font)(resources.GetObject("btnReset.Font")));
			this.btnReset.Image = ((System.Drawing.Image)(resources.GetObject("btnReset.Image")));
			this.btnReset.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnReset.ImageAlign")));
			this.btnReset.ImageIndex = ((int)(resources.GetObject("btnReset.ImageIndex")));
			this.btnReset.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnReset.ImeMode")));
			this.btnReset.Location = ((System.Drawing.Point)(resources.GetObject("btnReset.Location")));
			this.btnReset.Name = "btnReset";
			this.btnReset.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnReset.RightToLeft")));
			this.btnReset.Size = ((System.Drawing.Size)(resources.GetObject("btnReset.Size")));
			this.btnReset.TabIndex = ((int)(resources.GetObject("btnReset.TabIndex")));
			this.btnReset.Text = resources.GetString("btnReset.Text");
			this.btnReset.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnReset.TextAlign")));
			this.btnReset.Visible = ((bool)(resources.GetObject("btnReset.Visible")));
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// ResetPassphrase
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
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
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ResetPassphrase";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
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
			this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
			//this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
			this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			this.pictureBox1.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
			this.btnReset.Enabled = false;
			this.btnReset.Select();
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
			//MessageBox.Show("In reset passphrase", "Reset Passphrase");
			//return;
			try
			{
				DomainItem domainItem = (DomainItem)this.DomainComboBox.SelectedItem;
				this.domainID = domainItem.ID;
				//MessageBox.Show("In reset passphrase", "Reset Passphrase");
				//return;
				Status status = this.simws.ReSetPassPhrase(this.domainID, this.passPhrase.Text , this.newPassphrase.Text, "RAName", "Public Key");
				if( status.statusCode == StatusCodes.Success)
				{
					MessageBox.Show(Resource.GetString("ResetSuccess") /*"successfully reset passphrase"*/, Resource.GetString("ResetTitle"));
					this.success = true;
					this.Dispose();
					this.Close();
					this.simws.StorePassPhrase(this.domainID, this.newPassphrase.Text, CredentialType.Basic, this.rememberPassphrase.Checked);
					// successful..		
				}
				else
				{
					MessageBox.Show(Resource.GetString("ResetError")/*"Error resetting passphrase"*/ , Resource.GetString("ResetTitle")/*"reset error"*/ );
					this.success = false;
					// Unable to reset.
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(Resource.GetString("ResetError")/*"Error resetting passphrase"*/ , Resource.GetString("ResetTitle")/*"reset error"*/ );
				this.success = false;
			}
		}
	
		private void passPhrase_TextChanged(object sender, System.EventArgs e)
		{
			UpdateSensitivity();
		}

		private void retypePassphrase_TextChanged(object sender, System.EventArgs e)
		{
		//	MessageBox.Show(Resource.Test1, "In Reset passphrase");
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

		private void pictureBox1_Click(object sender, System.EventArgs e)
		{
		
		}

		private void DomainComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
