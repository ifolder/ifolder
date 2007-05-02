using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Novell.iFolderCom;

using Simias.Client;

namespace Novell.FormsTrayApp 
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
		private static System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(EnterPassphraseDialog));
		private System.Windows.Forms.PictureBox pictureBox1;
		private static System.Resources.ResourceManager Resource = new System.Resources.ResourceManager(typeof(FormsTrayApp));

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(EnterPassphraseDialog));
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
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
			// RecoveryAgentCombo
			// 
			this.RecoveryAgentCombo.AccessibleDescription = resources.GetString("RecoveryAgentCombo.AccessibleDescription");
			this.RecoveryAgentCombo.AccessibleName = resources.GetString("RecoveryAgentCombo.AccessibleName");
			this.RecoveryAgentCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RecoveryAgentCombo.Anchor")));
			this.RecoveryAgentCombo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RecoveryAgentCombo.BackgroundImage")));
			this.RecoveryAgentCombo.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RecoveryAgentCombo.Dock")));
			this.RecoveryAgentCombo.Enabled = ((bool)(resources.GetObject("RecoveryAgentCombo.Enabled")));
			this.RecoveryAgentCombo.Font = ((System.Drawing.Font)(resources.GetObject("RecoveryAgentCombo.Font")));
			this.RecoveryAgentCombo.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RecoveryAgentCombo.ImeMode")));
			this.RecoveryAgentCombo.IntegralHeight = ((bool)(resources.GetObject("RecoveryAgentCombo.IntegralHeight")));
			this.RecoveryAgentCombo.ItemHeight = ((int)(resources.GetObject("RecoveryAgentCombo.ItemHeight")));
			this.RecoveryAgentCombo.Location = ((System.Drawing.Point)(resources.GetObject("RecoveryAgentCombo.Location")));
			this.RecoveryAgentCombo.MaxDropDownItems = ((int)(resources.GetObject("RecoveryAgentCombo.MaxDropDownItems")));
			this.RecoveryAgentCombo.MaxLength = ((int)(resources.GetObject("RecoveryAgentCombo.MaxLength")));
			this.RecoveryAgentCombo.Name = "RecoveryAgentCombo";
			this.RecoveryAgentCombo.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RecoveryAgentCombo.RightToLeft")));
			this.RecoveryAgentCombo.Size = ((System.Drawing.Size)(resources.GetObject("RecoveryAgentCombo.Size")));
			this.RecoveryAgentCombo.TabIndex = ((int)(resources.GetObject("RecoveryAgentCombo.TabIndex")));
			this.RecoveryAgentCombo.Text = resources.GetString("RecoveryAgentCombo.Text");
			this.RecoveryAgentCombo.Visible = ((bool)(resources.GetObject("RecoveryAgentCombo.Visible")));
			// 
			// lblRecoveryAgent
			// 
			this.lblRecoveryAgent.AccessibleDescription = resources.GetString("lblRecoveryAgent.AccessibleDescription");
			this.lblRecoveryAgent.AccessibleName = resources.GetString("lblRecoveryAgent.AccessibleName");
			this.lblRecoveryAgent.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblRecoveryAgent.Anchor")));
			this.lblRecoveryAgent.AutoSize = ((bool)(resources.GetObject("lblRecoveryAgent.AutoSize")));
			this.lblRecoveryAgent.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblRecoveryAgent.Dock")));
			this.lblRecoveryAgent.Enabled = ((bool)(resources.GetObject("lblRecoveryAgent.Enabled")));
			this.lblRecoveryAgent.Font = ((System.Drawing.Font)(resources.GetObject("lblRecoveryAgent.Font")));
			this.lblRecoveryAgent.Image = ((System.Drawing.Image)(resources.GetObject("lblRecoveryAgent.Image")));
			this.lblRecoveryAgent.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRecoveryAgent.ImageAlign")));
			this.lblRecoveryAgent.ImageIndex = ((int)(resources.GetObject("lblRecoveryAgent.ImageIndex")));
			this.lblRecoveryAgent.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblRecoveryAgent.ImeMode")));
			this.lblRecoveryAgent.Location = ((System.Drawing.Point)(resources.GetObject("lblRecoveryAgent.Location")));
			this.lblRecoveryAgent.Name = "lblRecoveryAgent";
			this.lblRecoveryAgent.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblRecoveryAgent.RightToLeft")));
			this.lblRecoveryAgent.Size = ((System.Drawing.Size)(resources.GetObject("lblRecoveryAgent.Size")));
			this.lblRecoveryAgent.TabIndex = ((int)(resources.GetObject("lblRecoveryAgent.TabIndex")));
			this.lblRecoveryAgent.Text = resources.GetString("lblRecoveryAgent.Text");
			this.lblRecoveryAgent.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRecoveryAgent.TextAlign")));
			this.lblRecoveryAgent.Visible = ((bool)(resources.GetObject("lblRecoveryAgent.Visible")));
			// 
			// Passphrase
			// 
			this.Passphrase.AccessibleDescription = resources.GetString("Passphrase.AccessibleDescription");
			this.Passphrase.AccessibleName = resources.GetString("Passphrase.AccessibleName");
			this.Passphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("Passphrase.Anchor")));
			this.Passphrase.AutoSize = ((bool)(resources.GetObject("Passphrase.AutoSize")));
			this.Passphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Passphrase.BackgroundImage")));
			this.Passphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("Passphrase.Dock")));
			this.Passphrase.Enabled = ((bool)(resources.GetObject("Passphrase.Enabled")));
			this.Passphrase.Font = ((System.Drawing.Font)(resources.GetObject("Passphrase.Font")));
			this.Passphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("Passphrase.ImeMode")));
			this.Passphrase.Location = ((System.Drawing.Point)(resources.GetObject("Passphrase.Location")));
			this.Passphrase.MaxLength = ((int)(resources.GetObject("Passphrase.MaxLength")));
			this.Passphrase.Multiline = ((bool)(resources.GetObject("Passphrase.Multiline")));
			this.Passphrase.Name = "Passphrase";
			this.Passphrase.PasswordChar = ((char)(resources.GetObject("Passphrase.PasswordChar")));
			this.Passphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("Passphrase.RightToLeft")));
			this.Passphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("Passphrase.ScrollBars")));
			this.Passphrase.Size = ((System.Drawing.Size)(resources.GetObject("Passphrase.Size")));
			this.Passphrase.TabIndex = ((int)(resources.GetObject("Passphrase.TabIndex")));
			this.Passphrase.Text = resources.GetString("Passphrase.Text");
			this.Passphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("Passphrase.TextAlign")));
			this.Passphrase.Visible = ((bool)(resources.GetObject("Passphrase.Visible")));
			this.Passphrase.WordWrap = ((bool)(resources.GetObject("Passphrase.WordWrap")));
			this.Passphrase.TextChanged += new System.EventHandler(this.Passphrase_TextChanged);
			// 
			// RetypePassphrase
			// 
			this.RetypePassphrase.AccessibleDescription = resources.GetString("RetypePassphrase.AccessibleDescription");
			this.RetypePassphrase.AccessibleName = resources.GetString("RetypePassphrase.AccessibleName");
			this.RetypePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("RetypePassphrase.Anchor")));
			this.RetypePassphrase.AutoSize = ((bool)(resources.GetObject("RetypePassphrase.AutoSize")));
			this.RetypePassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RetypePassphrase.BackgroundImage")));
			this.RetypePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("RetypePassphrase.Dock")));
			this.RetypePassphrase.Enabled = ((bool)(resources.GetObject("RetypePassphrase.Enabled")));
			this.RetypePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("RetypePassphrase.Font")));
			this.RetypePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("RetypePassphrase.ImeMode")));
			this.RetypePassphrase.Location = ((System.Drawing.Point)(resources.GetObject("RetypePassphrase.Location")));
			this.RetypePassphrase.MaxLength = ((int)(resources.GetObject("RetypePassphrase.MaxLength")));
			this.RetypePassphrase.Multiline = ((bool)(resources.GetObject("RetypePassphrase.Multiline")));
			this.RetypePassphrase.Name = "RetypePassphrase";
			this.RetypePassphrase.PasswordChar = ((char)(resources.GetObject("RetypePassphrase.PasswordChar")));
			this.RetypePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("RetypePassphrase.RightToLeft")));
			this.RetypePassphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("RetypePassphrase.ScrollBars")));
			this.RetypePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("RetypePassphrase.Size")));
			this.RetypePassphrase.TabIndex = ((int)(resources.GetObject("RetypePassphrase.TabIndex")));
			this.RetypePassphrase.Text = resources.GetString("RetypePassphrase.Text");
			this.RetypePassphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("RetypePassphrase.TextAlign")));
			this.RetypePassphrase.Visible = ((bool)(resources.GetObject("RetypePassphrase.Visible")));
			this.RetypePassphrase.WordWrap = ((bool)(resources.GetObject("RetypePassphrase.WordWrap")));
			this.RetypePassphrase.TextChanged += new System.EventHandler(this.RetypePassphrase_TextChanged);
			// 
			// lblPassphrase
			// 
			this.lblPassphrase.AccessibleDescription = resources.GetString("lblPassphrase.AccessibleDescription");
			this.lblPassphrase.AccessibleName = resources.GetString("lblPassphrase.AccessibleName");
			this.lblPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblPassphrase.Anchor")));
			this.lblPassphrase.AutoSize = ((bool)(resources.GetObject("lblPassphrase.AutoSize")));
			this.lblPassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblPassphrase.Dock")));
			this.lblPassphrase.Enabled = ((bool)(resources.GetObject("lblPassphrase.Enabled")));
			this.lblPassphrase.Font = ((System.Drawing.Font)(resources.GetObject("lblPassphrase.Font")));
			this.lblPassphrase.Image = ((System.Drawing.Image)(resources.GetObject("lblPassphrase.Image")));
			this.lblPassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPassphrase.ImageAlign")));
			this.lblPassphrase.ImageIndex = ((int)(resources.GetObject("lblPassphrase.ImageIndex")));
			this.lblPassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblPassphrase.ImeMode")));
			this.lblPassphrase.Location = ((System.Drawing.Point)(resources.GetObject("lblPassphrase.Location")));
			this.lblPassphrase.Name = "lblPassphrase";
			this.lblPassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblPassphrase.RightToLeft")));
			this.lblPassphrase.Size = ((System.Drawing.Size)(resources.GetObject("lblPassphrase.Size")));
			this.lblPassphrase.TabIndex = ((int)(resources.GetObject("lblPassphrase.TabIndex")));
			this.lblPassphrase.Text = resources.GetString("lblPassphrase.Text");
			this.lblPassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblPassphrase.TextAlign")));
			this.lblPassphrase.Visible = ((bool)(resources.GetObject("lblPassphrase.Visible")));
			// 
			// lblRetypePassphrase
			// 
			this.lblRetypePassphrase.AccessibleDescription = resources.GetString("lblRetypePassphrase.AccessibleDescription");
			this.lblRetypePassphrase.AccessibleName = resources.GetString("lblRetypePassphrase.AccessibleName");
			this.lblRetypePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lblRetypePassphrase.Anchor")));
			this.lblRetypePassphrase.AutoSize = ((bool)(resources.GetObject("lblRetypePassphrase.AutoSize")));
			this.lblRetypePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lblRetypePassphrase.Dock")));
			this.lblRetypePassphrase.Enabled = ((bool)(resources.GetObject("lblRetypePassphrase.Enabled")));
			this.lblRetypePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("lblRetypePassphrase.Font")));
			this.lblRetypePassphrase.Image = ((System.Drawing.Image)(resources.GetObject("lblRetypePassphrase.Image")));
			this.lblRetypePassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRetypePassphrase.ImageAlign")));
			this.lblRetypePassphrase.ImageIndex = ((int)(resources.GetObject("lblRetypePassphrase.ImageIndex")));
			this.lblRetypePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lblRetypePassphrase.ImeMode")));
			this.lblRetypePassphrase.Location = ((System.Drawing.Point)(resources.GetObject("lblRetypePassphrase.Location")));
			this.lblRetypePassphrase.Name = "lblRetypePassphrase";
			this.lblRetypePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lblRetypePassphrase.RightToLeft")));
			this.lblRetypePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("lblRetypePassphrase.Size")));
			this.lblRetypePassphrase.TabIndex = ((int)(resources.GetObject("lblRetypePassphrase.TabIndex")));
			this.lblRetypePassphrase.Text = resources.GetString("lblRetypePassphrase.Text");
			this.lblRetypePassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lblRetypePassphrase.TextAlign")));
			this.lblRetypePassphrase.Visible = ((bool)(resources.GetObject("lblRetypePassphrase.Visible")));
			// 
			// savePassphrase
			// 
			this.savePassphrase.AccessibleDescription = resources.GetString("savePassphrase.AccessibleDescription");
			this.savePassphrase.AccessibleName = resources.GetString("savePassphrase.AccessibleName");
			this.savePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("savePassphrase.Anchor")));
			this.savePassphrase.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("savePassphrase.Appearance")));
			this.savePassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("savePassphrase.BackgroundImage")));
			this.savePassphrase.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("savePassphrase.CheckAlign")));
			this.savePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("savePassphrase.Dock")));
			this.savePassphrase.Enabled = ((bool)(resources.GetObject("savePassphrase.Enabled")));
			this.savePassphrase.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("savePassphrase.FlatStyle")));
			this.savePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("savePassphrase.Font")));
			this.savePassphrase.Image = ((System.Drawing.Image)(resources.GetObject("savePassphrase.Image")));
			this.savePassphrase.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("savePassphrase.ImageAlign")));
			this.savePassphrase.ImageIndex = ((int)(resources.GetObject("savePassphrase.ImageIndex")));
			this.savePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("savePassphrase.ImeMode")));
			this.savePassphrase.Location = ((System.Drawing.Point)(resources.GetObject("savePassphrase.Location")));
			this.savePassphrase.Name = "savePassphrase";
			this.savePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("savePassphrase.RightToLeft")));
			this.savePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("savePassphrase.Size")));
			this.savePassphrase.TabIndex = ((int)(resources.GetObject("savePassphrase.TabIndex")));
			this.savePassphrase.Text = resources.GetString("savePassphrase.Text");
			this.savePassphrase.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("savePassphrase.TextAlign")));
			this.savePassphrase.Visible = ((bool)(resources.GetObject("savePassphrase.Visible")));
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
			// btnOk
			// 
			this.btnOk.AccessibleDescription = resources.GetString("btnOk.AccessibleDescription");
			this.btnOk.AccessibleName = resources.GetString("btnOk.AccessibleName");
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnOk.Anchor")));
			this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
			this.btnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnOk.Dock")));
			this.btnOk.Enabled = ((bool)(resources.GetObject("btnOk.Enabled")));
			this.btnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnOk.FlatStyle")));
			this.btnOk.Font = ((System.Drawing.Font)(resources.GetObject("btnOk.Font")));
			this.btnOk.Image = ((System.Drawing.Image)(resources.GetObject("btnOk.Image")));
			this.btnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.ImageAlign")));
			this.btnOk.ImageIndex = ((int)(resources.GetObject("btnOk.ImageIndex")));
			this.btnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnOk.ImeMode")));
			this.btnOk.Location = ((System.Drawing.Point)(resources.GetObject("btnOk.Location")));
			this.btnOk.Name = "btnOk";
			this.btnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnOk.RightToLeft")));
			this.btnOk.Size = ((System.Drawing.Size)(resources.GetObject("btnOk.Size")));
			this.btnOk.TabIndex = ((int)(resources.GetObject("btnOk.TabIndex")));
			this.btnOk.Text = resources.GetString("btnOk.Text");
			this.btnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnOk.TextAlign")));
			this.btnOk.Visible = ((bool)(resources.GetObject("btnOk.Visible")));
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
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
			// 
			// EnterPassphraseDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
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
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "EnterPassphraseDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
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
				string ragent = null;
				if( this.RecoveryAgentCombo.SelectedItem != null && this.RecoveryAgentCombo.SelectedText != "None")
				{
					// Show the certificate.....
					byte[] CertificateObj = this.simws.GetRACertificateOnClient(this.DomainID, this.RecoveryAgentCombo.SelectedText);
					System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate(CertificateObj);
					MyMessageBox mmb = new MyMessageBox( "Verify Certificate", "Verify Certificate", cert.ToString(true), MyMessageBoxButtons.YesNo, MyMessageBoxIcon.Question, MyMessageBoxDefaultButton.Button2 );
					DialogResult messageDialogResult = mmb.ShowDialog();
					mmb.Dispose();
					mmb.Close();
					if( messageDialogResult != DialogResult.OK )
						return;
					else
					{
						ragent = this.RecoveryAgentCombo.SelectedText;
						publicKey = cert.GetPublicKeyString();
					}
					//return;
				}
				
				Status passPhraseStatus = null;
				try
				{
					passPhraseStatus = simws.SetPassPhrase( DomainID, this.Passphrase.Text, ragent, publicKey);
				}
				catch(Exception ex)
				{
					//MessageBox.Show("Unable to set Passphrase. "+ ex.Message);
					MessageBox.Show( Resource.GetString("IsPassphraseSetException")+ex.Message);
				}
				if(passPhraseStatus.statusCode == StatusCodes.Success)
				{
					// Validating Passphrase
					//passPhraseStatus = simws.ValidatePassPhrase( DomainID, PadString(this.Passphrase.Text, 16));
					//if(passPhraseStatus.statusCode != StatusCodes.Success)
					//	MessageBox.Show("Passphrase not validated");
					simws.StorePassPhrase( DomainID, this.Passphrase.Text, CredentialType.Basic, this.savePassphrase.Checked);
					string passphr = simws.GetPassPhrase(DomainID);
					//MessageBox.Show("Passphrase is set & stored", passphr, MessageBoxButtons.OK);
					this.status= simws.IsPassPhraseSet(DomainID);
					if( status == true)
					{
						Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("SetPassphraseSuccess")/*"Successfully set the passphrase"*/, resourceManager.GetString("$this.Text"), "", MyMessageBoxButtons.OK, MyMessageBoxIcon.None);
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
					Novell.iFolderCom.MyMessageBox mmb = new MyMessageBox(Resource.GetString("IsPassphraseSetException")/*"Unable to set the passphrase"*/, resourceManager.GetString("$this.Text")/*"Error setting the passphrase"*/, Resource.GetString("TryAgain")/*"Please try again"*/, MyMessageBoxButtons.OK, MyMessageBoxIcon.Error);
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
			this.btnOk.Enabled = false;
			this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
			this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			//this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
			this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			this.pictureBox1.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
			string[] rAgents= this.simws.GetRAListOnClient(DomainID);
			foreach( string rAgent in rAgents)
			{
				this.RecoveryAgentCombo.Items.Add( rAgent ); 
				//MessageBox.Show(String.Format("Adding {0}", rAgent));
			}
			// Needs to be changed
			//string[] ralist = this.simws.GetRAList(this.DomainID);
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
