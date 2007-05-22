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
	/// Summary description for ImportKeysDialog.
	/// </summary>
	public class ImportKeysDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label filePathLabel;
		private System.Windows.Forms.Label oneTimePassphraseLabel;
		private System.Windows.Forms.Label passPhraseLabel;
		private System.Windows.Forms.Label retypePassphraseLabel;
		private System.Windows.Forms.TextBox oneTimePassphrase;
		private System.Windows.Forms.TextBox Passphrase;
		private System.Windows.Forms.TextBox retypePassphrase;
		private System.Windows.Forms.TextBox LocationEntry;
		private System.Windows.Forms.Button BrowseButton;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnImport;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox DomainComboBox;
		private DomainItem selectedDomain;
		private System.Windows.Forms.PictureBox pictureBox1;
		private SimiasWebService simiasWebService;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ImportKeysDialog(SimiasWebService simws)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.simiasWebService = simws;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ImportKeysDialog));
			this.panel1 = new System.Windows.Forms.Panel();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.filePathLabel = new System.Windows.Forms.Label();
			this.oneTimePassphraseLabel = new System.Windows.Forms.Label();
			this.passPhraseLabel = new System.Windows.Forms.Label();
			this.retypePassphraseLabel = new System.Windows.Forms.Label();
			this.oneTimePassphrase = new System.Windows.Forms.TextBox();
			this.Passphrase = new System.Windows.Forms.TextBox();
			this.retypePassphrase = new System.Windows.Forms.TextBox();
			this.LocationEntry = new System.Windows.Forms.TextBox();
			this.BrowseButton = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnImport = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.DomainComboBox = new System.Windows.Forms.ComboBox();
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
			// filePathLabel
			// 
			this.filePathLabel.AccessibleDescription = resources.GetString("filePathLabel.AccessibleDescription");
			this.filePathLabel.AccessibleName = resources.GetString("filePathLabel.AccessibleName");
			this.filePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("filePathLabel.Anchor")));
			this.filePathLabel.AutoSize = ((bool)(resources.GetObject("filePathLabel.AutoSize")));
			this.filePathLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("filePathLabel.Dock")));
			this.filePathLabel.Enabled = ((bool)(resources.GetObject("filePathLabel.Enabled")));
			this.filePathLabel.Font = ((System.Drawing.Font)(resources.GetObject("filePathLabel.Font")));
			this.filePathLabel.Image = ((System.Drawing.Image)(resources.GetObject("filePathLabel.Image")));
			this.filePathLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("filePathLabel.ImageAlign")));
			this.filePathLabel.ImageIndex = ((int)(resources.GetObject("filePathLabel.ImageIndex")));
			this.filePathLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("filePathLabel.ImeMode")));
			this.filePathLabel.Location = ((System.Drawing.Point)(resources.GetObject("filePathLabel.Location")));
			this.filePathLabel.Name = "filePathLabel";
			this.filePathLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("filePathLabel.RightToLeft")));
			this.filePathLabel.Size = ((System.Drawing.Size)(resources.GetObject("filePathLabel.Size")));
			this.filePathLabel.TabIndex = ((int)(resources.GetObject("filePathLabel.TabIndex")));
			this.filePathLabel.Text = resources.GetString("filePathLabel.Text");
			this.filePathLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("filePathLabel.TextAlign")));
			this.filePathLabel.Visible = ((bool)(resources.GetObject("filePathLabel.Visible")));
			// 
			// oneTimePassphraseLabel
			// 
			this.oneTimePassphraseLabel.AccessibleDescription = resources.GetString("oneTimePassphraseLabel.AccessibleDescription");
			this.oneTimePassphraseLabel.AccessibleName = resources.GetString("oneTimePassphraseLabel.AccessibleName");
			this.oneTimePassphraseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("oneTimePassphraseLabel.Anchor")));
			this.oneTimePassphraseLabel.AutoSize = ((bool)(resources.GetObject("oneTimePassphraseLabel.AutoSize")));
			this.oneTimePassphraseLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("oneTimePassphraseLabel.Dock")));
			this.oneTimePassphraseLabel.Enabled = ((bool)(resources.GetObject("oneTimePassphraseLabel.Enabled")));
			this.oneTimePassphraseLabel.Font = ((System.Drawing.Font)(resources.GetObject("oneTimePassphraseLabel.Font")));
			this.oneTimePassphraseLabel.Image = ((System.Drawing.Image)(resources.GetObject("oneTimePassphraseLabel.Image")));
			this.oneTimePassphraseLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("oneTimePassphraseLabel.ImageAlign")));
			this.oneTimePassphraseLabel.ImageIndex = ((int)(resources.GetObject("oneTimePassphraseLabel.ImageIndex")));
			this.oneTimePassphraseLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("oneTimePassphraseLabel.ImeMode")));
			this.oneTimePassphraseLabel.Location = ((System.Drawing.Point)(resources.GetObject("oneTimePassphraseLabel.Location")));
			this.oneTimePassphraseLabel.Name = "oneTimePassphraseLabel";
			this.oneTimePassphraseLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("oneTimePassphraseLabel.RightToLeft")));
			this.oneTimePassphraseLabel.Size = ((System.Drawing.Size)(resources.GetObject("oneTimePassphraseLabel.Size")));
			this.oneTimePassphraseLabel.TabIndex = ((int)(resources.GetObject("oneTimePassphraseLabel.TabIndex")));
			this.oneTimePassphraseLabel.Text = resources.GetString("oneTimePassphraseLabel.Text");
			this.oneTimePassphraseLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("oneTimePassphraseLabel.TextAlign")));
			this.oneTimePassphraseLabel.Visible = ((bool)(resources.GetObject("oneTimePassphraseLabel.Visible")));
			// 
			// passPhraseLabel
			// 
			this.passPhraseLabel.AccessibleDescription = resources.GetString("passPhraseLabel.AccessibleDescription");
			this.passPhraseLabel.AccessibleName = resources.GetString("passPhraseLabel.AccessibleName");
			this.passPhraseLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("passPhraseLabel.Anchor")));
			this.passPhraseLabel.AutoSize = ((bool)(resources.GetObject("passPhraseLabel.AutoSize")));
			this.passPhraseLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("passPhraseLabel.Dock")));
			this.passPhraseLabel.Enabled = ((bool)(resources.GetObject("passPhraseLabel.Enabled")));
			this.passPhraseLabel.Font = ((System.Drawing.Font)(resources.GetObject("passPhraseLabel.Font")));
			this.passPhraseLabel.Image = ((System.Drawing.Image)(resources.GetObject("passPhraseLabel.Image")));
			this.passPhraseLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passPhraseLabel.ImageAlign")));
			this.passPhraseLabel.ImageIndex = ((int)(resources.GetObject("passPhraseLabel.ImageIndex")));
			this.passPhraseLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("passPhraseLabel.ImeMode")));
			this.passPhraseLabel.Location = ((System.Drawing.Point)(resources.GetObject("passPhraseLabel.Location")));
			this.passPhraseLabel.Name = "passPhraseLabel";
			this.passPhraseLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("passPhraseLabel.RightToLeft")));
			this.passPhraseLabel.Size = ((System.Drawing.Size)(resources.GetObject("passPhraseLabel.Size")));
			this.passPhraseLabel.TabIndex = ((int)(resources.GetObject("passPhraseLabel.TabIndex")));
			this.passPhraseLabel.Text = resources.GetString("passPhraseLabel.Text");
			this.passPhraseLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("passPhraseLabel.TextAlign")));
			this.passPhraseLabel.Visible = ((bool)(resources.GetObject("passPhraseLabel.Visible")));
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
			// oneTimePassphrase
			// 
			this.oneTimePassphrase.AccessibleDescription = resources.GetString("oneTimePassphrase.AccessibleDescription");
			this.oneTimePassphrase.AccessibleName = resources.GetString("oneTimePassphrase.AccessibleName");
			this.oneTimePassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("oneTimePassphrase.Anchor")));
			this.oneTimePassphrase.AutoSize = ((bool)(resources.GetObject("oneTimePassphrase.AutoSize")));
			this.oneTimePassphrase.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("oneTimePassphrase.BackgroundImage")));
			this.oneTimePassphrase.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("oneTimePassphrase.Dock")));
			this.oneTimePassphrase.Enabled = ((bool)(resources.GetObject("oneTimePassphrase.Enabled")));
			this.oneTimePassphrase.Font = ((System.Drawing.Font)(resources.GetObject("oneTimePassphrase.Font")));
			this.oneTimePassphrase.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("oneTimePassphrase.ImeMode")));
			this.oneTimePassphrase.Location = ((System.Drawing.Point)(resources.GetObject("oneTimePassphrase.Location")));
			this.oneTimePassphrase.MaxLength = ((int)(resources.GetObject("oneTimePassphrase.MaxLength")));
			this.oneTimePassphrase.Multiline = ((bool)(resources.GetObject("oneTimePassphrase.Multiline")));
			this.oneTimePassphrase.Name = "oneTimePassphrase";
			this.oneTimePassphrase.PasswordChar = ((char)(resources.GetObject("oneTimePassphrase.PasswordChar")));
			this.oneTimePassphrase.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("oneTimePassphrase.RightToLeft")));
			this.oneTimePassphrase.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("oneTimePassphrase.ScrollBars")));
			this.oneTimePassphrase.Size = ((System.Drawing.Size)(resources.GetObject("oneTimePassphrase.Size")));
			this.oneTimePassphrase.TabIndex = ((int)(resources.GetObject("oneTimePassphrase.TabIndex")));
			this.oneTimePassphrase.Text = resources.GetString("oneTimePassphrase.Text");
			this.oneTimePassphrase.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("oneTimePassphrase.TextAlign")));
			this.oneTimePassphrase.Visible = ((bool)(resources.GetObject("oneTimePassphrase.Visible")));
			this.oneTimePassphrase.WordWrap = ((bool)(resources.GetObject("oneTimePassphrase.WordWrap")));
			this.oneTimePassphrase.TextChanged += new System.EventHandler(this.oneTimePassphrase_TextChanged);
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
			// LocationEntry
			// 
			this.LocationEntry.AccessibleDescription = resources.GetString("LocationEntry.AccessibleDescription");
			this.LocationEntry.AccessibleName = resources.GetString("LocationEntry.AccessibleName");
			this.LocationEntry.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LocationEntry.Anchor")));
			this.LocationEntry.AutoSize = ((bool)(resources.GetObject("LocationEntry.AutoSize")));
			this.LocationEntry.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LocationEntry.BackgroundImage")));
			this.LocationEntry.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LocationEntry.Dock")));
			this.LocationEntry.Enabled = ((bool)(resources.GetObject("LocationEntry.Enabled")));
			this.LocationEntry.Font = ((System.Drawing.Font)(resources.GetObject("LocationEntry.Font")));
			this.LocationEntry.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LocationEntry.ImeMode")));
			this.LocationEntry.Location = ((System.Drawing.Point)(resources.GetObject("LocationEntry.Location")));
			this.LocationEntry.MaxLength = ((int)(resources.GetObject("LocationEntry.MaxLength")));
			this.LocationEntry.Multiline = ((bool)(resources.GetObject("LocationEntry.Multiline")));
			this.LocationEntry.Name = "LocationEntry";
			this.LocationEntry.PasswordChar = ((char)(resources.GetObject("LocationEntry.PasswordChar")));
			this.LocationEntry.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LocationEntry.RightToLeft")));
			this.LocationEntry.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("LocationEntry.ScrollBars")));
			this.LocationEntry.Size = ((System.Drawing.Size)(resources.GetObject("LocationEntry.Size")));
			this.LocationEntry.TabIndex = ((int)(resources.GetObject("LocationEntry.TabIndex")));
			this.LocationEntry.Text = resources.GetString("LocationEntry.Text");
			this.LocationEntry.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("LocationEntry.TextAlign")));
			this.LocationEntry.Visible = ((bool)(resources.GetObject("LocationEntry.Visible")));
			this.LocationEntry.WordWrap = ((bool)(resources.GetObject("LocationEntry.WordWrap")));
			this.LocationEntry.TextChanged += new System.EventHandler(this.LocationEntry_TextChanged);
			// 
			// BrowseButton
			// 
			this.BrowseButton.AccessibleDescription = resources.GetString("BrowseButton.AccessibleDescription");
			this.BrowseButton.AccessibleName = resources.GetString("BrowseButton.AccessibleName");
			this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BrowseButton.Anchor")));
			this.BrowseButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BrowseButton.BackgroundImage")));
			this.BrowseButton.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BrowseButton.Dock")));
			this.BrowseButton.Enabled = ((bool)(resources.GetObject("BrowseButton.Enabled")));
			this.BrowseButton.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BrowseButton.FlatStyle")));
			this.BrowseButton.Font = ((System.Drawing.Font)(resources.GetObject("BrowseButton.Font")));
			this.BrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("BrowseButton.Image")));
			this.BrowseButton.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BrowseButton.ImageAlign")));
			this.BrowseButton.ImageIndex = ((int)(resources.GetObject("BrowseButton.ImageIndex")));
			this.BrowseButton.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BrowseButton.ImeMode")));
			this.BrowseButton.Location = ((System.Drawing.Point)(resources.GetObject("BrowseButton.Location")));
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BrowseButton.RightToLeft")));
			this.BrowseButton.Size = ((System.Drawing.Size)(resources.GetObject("BrowseButton.Size")));
			this.BrowseButton.TabIndex = ((int)(resources.GetObject("BrowseButton.TabIndex")));
			this.BrowseButton.Text = resources.GetString("BrowseButton.Text");
			this.BrowseButton.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BrowseButton.TextAlign")));
			this.BrowseButton.Visible = ((bool)(resources.GetObject("BrowseButton.Visible")));
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
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
			// btnImport
			// 
			this.btnImport.AccessibleDescription = resources.GetString("btnImport.AccessibleDescription");
			this.btnImport.AccessibleName = resources.GetString("btnImport.AccessibleName");
			this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnImport.Anchor")));
			this.btnImport.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnImport.BackgroundImage")));
			this.btnImport.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnImport.Dock")));
			this.btnImport.Enabled = ((bool)(resources.GetObject("btnImport.Enabled")));
			this.btnImport.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnImport.FlatStyle")));
			this.btnImport.Font = ((System.Drawing.Font)(resources.GetObject("btnImport.Font")));
			this.btnImport.Image = ((System.Drawing.Image)(resources.GetObject("btnImport.Image")));
			this.btnImport.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnImport.ImageAlign")));
			this.btnImport.ImageIndex = ((int)(resources.GetObject("btnImport.ImageIndex")));
			this.btnImport.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnImport.ImeMode")));
			this.btnImport.Location = ((System.Drawing.Point)(resources.GetObject("btnImport.Location")));
			this.btnImport.Name = "btnImport";
			this.btnImport.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnImport.RightToLeft")));
			this.btnImport.Size = ((System.Drawing.Size)(resources.GetObject("btnImport.Size")));
			this.btnImport.TabIndex = ((int)(resources.GetObject("btnImport.TabIndex")));
			this.btnImport.Text = resources.GetString("btnImport.Text");
			this.btnImport.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnImport.TextAlign")));
			this.btnImport.Visible = ((bool)(resources.GetObject("btnImport.Visible")));
			this.btnImport.Click += new EventHandler(btnImport_Click);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = resources.GetString("label1.AccessibleDescription");
			this.label1.AccessibleName = resources.GetString("label1.AccessibleName");
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("label1.Anchor")));
			this.label1.AutoSize = ((bool)(resources.GetObject("label1.AutoSize")));
			this.label1.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("label1.Dock")));
			this.label1.Enabled = ((bool)(resources.GetObject("label1.Enabled")));
			this.label1.Font = ((System.Drawing.Font)(resources.GetObject("label1.Font")));
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.ImageAlign")));
			this.label1.ImageIndex = ((int)(resources.GetObject("label1.ImageIndex")));
			this.label1.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("label1.ImeMode")));
			this.label1.Location = ((System.Drawing.Point)(resources.GetObject("label1.Location")));
			this.label1.Name = "label1";
			this.label1.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("label1.RightToLeft")));
			this.label1.Size = ((System.Drawing.Size)(resources.GetObject("label1.Size")));
			this.label1.TabIndex = ((int)(resources.GetObject("label1.TabIndex")));
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("label1.TextAlign")));
			this.label1.Visible = ((bool)(resources.GetObject("label1.Visible")));
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
			// ImportKeysDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.DomainComboBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnImport);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.LocationEntry);
			this.Controls.Add(this.retypePassphrase);
			this.Controls.Add(this.Passphrase);
			this.Controls.Add(this.oneTimePassphrase);
			this.Controls.Add(this.retypePassphraseLabel);
			this.Controls.Add(this.passPhraseLabel);
			this.Controls.Add(this.oneTimePassphraseLabel);
			this.Controls.Add(this.filePathLabel);
			this.Controls.Add(this.panel1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ImportKeysDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.ImportKeysDialog_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.Dispose();
			this.Close();
		}

		private void ImportKeysDialog_Load(object sender, System.EventArgs e)
		{
			this.Icon = new Icon(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder_16.ico"));
			//this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder48.png"));
			this.waterMark.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner.png"));
			this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			this.pictureBox1.Image = Image.FromFile(System.IO.Path.Combine(Application.StartupPath, @"res\ifolder-banner-scaler.png"));
			this.btnImport.Enabled = false;
			this.btnImport.Select();
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

		private void UpdateSensitivity()
		{
			if( this.Passphrase.Text.Length >0 && this.Passphrase.Text == this.retypePassphrase.Text )
				//if( this.oneTimePassphrase.Text.Length > 0)
					if( this.LocationEntry.Text.Length > 0 && this.DomainComboBox.SelectedIndex >= 0)
					{
						this.btnImport.Enabled = true;
						return;
					}
			this.btnImport.Enabled = false;
		}

		private void LocationEntry_TextChanged(object sender, EventArgs e)
		{
			UpdateSensitivity();
		}

		private void retypePassphrase_TextChanged(object sender, EventArgs e)
		{
			UpdateSensitivity();
		}

		private void Passphrase_TextChanged(object sender, EventArgs e)
		{
			UpdateSensitivity();
		}

		private void oneTimePassphrase_TextChanged(object sender, EventArgs e)
		{
			UpdateSensitivity();
		}

		private void BrowseButton_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog fileDlg = new OpenFileDialog();
			fileDlg.ReadOnlyChecked = true;
			fileDlg.ShowDialog();
			this.LocationEntry.Text = fileDlg.FileName;
		}

		private void btnImport_Click(object sender, EventArgs e)
		{
			DomainItem domainItem = (DomainItem)this.DomainComboBox.SelectedItem;
			try
			{
				string onetimepp;
				if( this.oneTimePassphrase != null)
					onetimepp = this.oneTimePassphrase.Text;
				else
					onetimepp = null;
				this.simiasWebService.ImportiFoldersCryptoKeys(domainItem.ID, this.Passphrase.Text,  onetimepp, this.LocationEntry.Text);
			}
			catch(Exception ex)
			{
				MessageBox.Show(string.Format("Error importing the keys. {0}", ex.Message));
			}
		}
	}
}
