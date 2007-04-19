using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Novell.FormsTrayApp
{
	/// <summary>
	/// Summary description for ExportKeysDialog.
	/// </summary>
	public class ExportKeysDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.PictureBox waterMark;
		private System.Windows.Forms.Label domainLabel;
		private System.Windows.Forms.Label filePathLabel;
		private System.Windows.Forms.Label recoveryAgentLabel;
		private System.Windows.Forms.Label emailLabel;
		private System.Windows.Forms.ComboBox domainComboBox;
		private System.Windows.Forms.TextBox filePath;
		private System.Windows.Forms.TextBox recoveryAgent;
		private System.Windows.Forms.TextBox emailID;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnExport;
		private System.Windows.Forms.Button BrowseButton;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ExportKeysDialog()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExportKeysDialog));
			this.panel1 = new System.Windows.Forms.Panel();
			this.waterMark = new System.Windows.Forms.PictureBox();
			this.domainLabel = new System.Windows.Forms.Label();
			this.filePathLabel = new System.Windows.Forms.Label();
			this.recoveryAgentLabel = new System.Windows.Forms.Label();
			this.emailLabel = new System.Windows.Forms.Label();
			this.domainComboBox = new System.Windows.Forms.ComboBox();
			this.filePath = new System.Windows.Forms.TextBox();
			this.recoveryAgent = new System.Windows.Forms.TextBox();
			this.emailID = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnExport = new System.Windows.Forms.Button();
			this.BrowseButton = new System.Windows.Forms.Button();
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
			this.panel1.BackColor = System.Drawing.Color.Blue;
			this.panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("panel1.BackgroundImage")));
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
			this.waterMark.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(163)), ((System.Byte)(237)));
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
			// domainLabel
			// 
			this.domainLabel.AccessibleDescription = resources.GetString("domainLabel.AccessibleDescription");
			this.domainLabel.AccessibleName = resources.GetString("domainLabel.AccessibleName");
			this.domainLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("domainLabel.Anchor")));
			this.domainLabel.AutoSize = ((bool)(resources.GetObject("domainLabel.AutoSize")));
			this.domainLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("domainLabel.Dock")));
			this.domainLabel.Enabled = ((bool)(resources.GetObject("domainLabel.Enabled")));
			this.domainLabel.Font = ((System.Drawing.Font)(resources.GetObject("domainLabel.Font")));
			this.domainLabel.Image = ((System.Drawing.Image)(resources.GetObject("domainLabel.Image")));
			this.domainLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("domainLabel.ImageAlign")));
			this.domainLabel.ImageIndex = ((int)(resources.GetObject("domainLabel.ImageIndex")));
			this.domainLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("domainLabel.ImeMode")));
			this.domainLabel.Location = ((System.Drawing.Point)(resources.GetObject("domainLabel.Location")));
			this.domainLabel.Name = "domainLabel";
			this.domainLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("domainLabel.RightToLeft")));
			this.domainLabel.Size = ((System.Drawing.Size)(resources.GetObject("domainLabel.Size")));
			this.domainLabel.TabIndex = ((int)(resources.GetObject("domainLabel.TabIndex")));
			this.domainLabel.Text = resources.GetString("domainLabel.Text");
			this.domainLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("domainLabel.TextAlign")));
			this.domainLabel.Visible = ((bool)(resources.GetObject("domainLabel.Visible")));
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
			// emailLabel
			// 
			this.emailLabel.AccessibleDescription = resources.GetString("emailLabel.AccessibleDescription");
			this.emailLabel.AccessibleName = resources.GetString("emailLabel.AccessibleName");
			this.emailLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("emailLabel.Anchor")));
			this.emailLabel.AutoSize = ((bool)(resources.GetObject("emailLabel.AutoSize")));
			this.emailLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("emailLabel.Dock")));
			this.emailLabel.Enabled = ((bool)(resources.GetObject("emailLabel.Enabled")));
			this.emailLabel.Font = ((System.Drawing.Font)(resources.GetObject("emailLabel.Font")));
			this.emailLabel.Image = ((System.Drawing.Image)(resources.GetObject("emailLabel.Image")));
			this.emailLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("emailLabel.ImageAlign")));
			this.emailLabel.ImageIndex = ((int)(resources.GetObject("emailLabel.ImageIndex")));
			this.emailLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("emailLabel.ImeMode")));
			this.emailLabel.Location = ((System.Drawing.Point)(resources.GetObject("emailLabel.Location")));
			this.emailLabel.Name = "emailLabel";
			this.emailLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("emailLabel.RightToLeft")));
			this.emailLabel.Size = ((System.Drawing.Size)(resources.GetObject("emailLabel.Size")));
			this.emailLabel.TabIndex = ((int)(resources.GetObject("emailLabel.TabIndex")));
			this.emailLabel.Text = resources.GetString("emailLabel.Text");
			this.emailLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("emailLabel.TextAlign")));
			this.emailLabel.Visible = ((bool)(resources.GetObject("emailLabel.Visible")));
			// 
			// domainComboBox
			// 
			this.domainComboBox.AccessibleDescription = resources.GetString("domainComboBox.AccessibleDescription");
			this.domainComboBox.AccessibleName = resources.GetString("domainComboBox.AccessibleName");
			this.domainComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("domainComboBox.Anchor")));
			this.domainComboBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("domainComboBox.BackgroundImage")));
			this.domainComboBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("domainComboBox.Dock")));
			this.domainComboBox.Enabled = ((bool)(resources.GetObject("domainComboBox.Enabled")));
			this.domainComboBox.Font = ((System.Drawing.Font)(resources.GetObject("domainComboBox.Font")));
			this.domainComboBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("domainComboBox.ImeMode")));
			this.domainComboBox.IntegralHeight = ((bool)(resources.GetObject("domainComboBox.IntegralHeight")));
			this.domainComboBox.ItemHeight = ((int)(resources.GetObject("domainComboBox.ItemHeight")));
			this.domainComboBox.Location = ((System.Drawing.Point)(resources.GetObject("domainComboBox.Location")));
			this.domainComboBox.MaxDropDownItems = ((int)(resources.GetObject("domainComboBox.MaxDropDownItems")));
			this.domainComboBox.MaxLength = ((int)(resources.GetObject("domainComboBox.MaxLength")));
			this.domainComboBox.Name = "domainComboBox";
			this.domainComboBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("domainComboBox.RightToLeft")));
			this.domainComboBox.Size = ((System.Drawing.Size)(resources.GetObject("domainComboBox.Size")));
			this.domainComboBox.TabIndex = ((int)(resources.GetObject("domainComboBox.TabIndex")));
			this.domainComboBox.Text = resources.GetString("domainComboBox.Text");
			this.domainComboBox.Visible = ((bool)(resources.GetObject("domainComboBox.Visible")));
			// 
			// filePath
			// 
			this.filePath.AccessibleDescription = resources.GetString("filePath.AccessibleDescription");
			this.filePath.AccessibleName = resources.GetString("filePath.AccessibleName");
			this.filePath.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("filePath.Anchor")));
			this.filePath.AutoSize = ((bool)(resources.GetObject("filePath.AutoSize")));
			this.filePath.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("filePath.BackgroundImage")));
			this.filePath.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("filePath.Dock")));
			this.filePath.Enabled = ((bool)(resources.GetObject("filePath.Enabled")));
			this.filePath.Font = ((System.Drawing.Font)(resources.GetObject("filePath.Font")));
			this.filePath.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("filePath.ImeMode")));
			this.filePath.Location = ((System.Drawing.Point)(resources.GetObject("filePath.Location")));
			this.filePath.MaxLength = ((int)(resources.GetObject("filePath.MaxLength")));
			this.filePath.Multiline = ((bool)(resources.GetObject("filePath.Multiline")));
			this.filePath.Name = "filePath";
			this.filePath.PasswordChar = ((char)(resources.GetObject("filePath.PasswordChar")));
			this.filePath.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("filePath.RightToLeft")));
			this.filePath.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("filePath.ScrollBars")));
			this.filePath.Size = ((System.Drawing.Size)(resources.GetObject("filePath.Size")));
			this.filePath.TabIndex = ((int)(resources.GetObject("filePath.TabIndex")));
			this.filePath.Text = resources.GetString("filePath.Text");
			this.filePath.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("filePath.TextAlign")));
			this.filePath.Visible = ((bool)(resources.GetObject("filePath.Visible")));
			this.filePath.WordWrap = ((bool)(resources.GetObject("filePath.WordWrap")));
			this.filePath.TextChanged += new System.EventHandler(this.filePath_TextChanged);
			// 
			// recoveryAgent
			// 
			this.recoveryAgent.AccessibleDescription = resources.GetString("recoveryAgent.AccessibleDescription");
			this.recoveryAgent.AccessibleName = resources.GetString("recoveryAgent.AccessibleName");
			this.recoveryAgent.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("recoveryAgent.Anchor")));
			this.recoveryAgent.AutoSize = ((bool)(resources.GetObject("recoveryAgent.AutoSize")));
			this.recoveryAgent.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("recoveryAgent.BackgroundImage")));
			this.recoveryAgent.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("recoveryAgent.Dock")));
			this.recoveryAgent.Enabled = ((bool)(resources.GetObject("recoveryAgent.Enabled")));
			this.recoveryAgent.Font = ((System.Drawing.Font)(resources.GetObject("recoveryAgent.Font")));
			this.recoveryAgent.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("recoveryAgent.ImeMode")));
			this.recoveryAgent.Location = ((System.Drawing.Point)(resources.GetObject("recoveryAgent.Location")));
			this.recoveryAgent.MaxLength = ((int)(resources.GetObject("recoveryAgent.MaxLength")));
			this.recoveryAgent.Multiline = ((bool)(resources.GetObject("recoveryAgent.Multiline")));
			this.recoveryAgent.Name = "recoveryAgent";
			this.recoveryAgent.PasswordChar = ((char)(resources.GetObject("recoveryAgent.PasswordChar")));
			this.recoveryAgent.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("recoveryAgent.RightToLeft")));
			this.recoveryAgent.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("recoveryAgent.ScrollBars")));
			this.recoveryAgent.Size = ((System.Drawing.Size)(resources.GetObject("recoveryAgent.Size")));
			this.recoveryAgent.TabIndex = ((int)(resources.GetObject("recoveryAgent.TabIndex")));
			this.recoveryAgent.Text = resources.GetString("recoveryAgent.Text");
			this.recoveryAgent.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("recoveryAgent.TextAlign")));
			this.recoveryAgent.Visible = ((bool)(resources.GetObject("recoveryAgent.Visible")));
			this.recoveryAgent.WordWrap = ((bool)(resources.GetObject("recoveryAgent.WordWrap")));
			// 
			// emailID
			// 
			this.emailID.AccessibleDescription = resources.GetString("emailID.AccessibleDescription");
			this.emailID.AccessibleName = resources.GetString("emailID.AccessibleName");
			this.emailID.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("emailID.Anchor")));
			this.emailID.AutoSize = ((bool)(resources.GetObject("emailID.AutoSize")));
			this.emailID.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("emailID.BackgroundImage")));
			this.emailID.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("emailID.Dock")));
			this.emailID.Enabled = ((bool)(resources.GetObject("emailID.Enabled")));
			this.emailID.Font = ((System.Drawing.Font)(resources.GetObject("emailID.Font")));
			this.emailID.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("emailID.ImeMode")));
			this.emailID.Location = ((System.Drawing.Point)(resources.GetObject("emailID.Location")));
			this.emailID.MaxLength = ((int)(resources.GetObject("emailID.MaxLength")));
			this.emailID.Multiline = ((bool)(resources.GetObject("emailID.Multiline")));
			this.emailID.Name = "emailID";
			this.emailID.PasswordChar = ((char)(resources.GetObject("emailID.PasswordChar")));
			this.emailID.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("emailID.RightToLeft")));
			this.emailID.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("emailID.ScrollBars")));
			this.emailID.Size = ((System.Drawing.Size)(resources.GetObject("emailID.Size")));
			this.emailID.TabIndex = ((int)(resources.GetObject("emailID.TabIndex")));
			this.emailID.Text = resources.GetString("emailID.Text");
			this.emailID.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("emailID.TextAlign")));
			this.emailID.Visible = ((bool)(resources.GetObject("emailID.Visible")));
			this.emailID.WordWrap = ((bool)(resources.GetObject("emailID.WordWrap")));
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
			// 
			// btnExport
			// 
			this.btnExport.AccessibleDescription = resources.GetString("btnExport.AccessibleDescription");
			this.btnExport.AccessibleName = resources.GetString("btnExport.AccessibleName");
			this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btnExport.Anchor")));
			this.btnExport.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnExport.BackgroundImage")));
			this.btnExport.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btnExport.Dock")));
			this.btnExport.Enabled = ((bool)(resources.GetObject("btnExport.Enabled")));
			this.btnExport.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btnExport.FlatStyle")));
			this.btnExport.Font = ((System.Drawing.Font)(resources.GetObject("btnExport.Font")));
			this.btnExport.Image = ((System.Drawing.Image)(resources.GetObject("btnExport.Image")));
			this.btnExport.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnExport.ImageAlign")));
			this.btnExport.ImageIndex = ((int)(resources.GetObject("btnExport.ImageIndex")));
			this.btnExport.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btnExport.ImeMode")));
			this.btnExport.Location = ((System.Drawing.Point)(resources.GetObject("btnExport.Location")));
			this.btnExport.Name = "btnExport";
			this.btnExport.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btnExport.RightToLeft")));
			this.btnExport.Size = ((System.Drawing.Size)(resources.GetObject("btnExport.Size")));
			this.btnExport.TabIndex = ((int)(resources.GetObject("btnExport.TabIndex")));
			this.btnExport.Text = resources.GetString("btnExport.Text");
			this.btnExport.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btnExport.TextAlign")));
			this.btnExport.Visible = ((bool)(resources.GetObject("btnExport.Visible")));
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
			// 
			// ExportKeysDialog
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.btnExport);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.emailID);
			this.Controls.Add(this.recoveryAgent);
			this.Controls.Add(this.filePath);
			this.Controls.Add(this.domainComboBox);
			this.Controls.Add(this.emailLabel);
			this.Controls.Add(this.recoveryAgentLabel);
			this.Controls.Add(this.filePathLabel);
			this.Controls.Add(this.domainLabel);
			this.Controls.Add(this.panel1);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ExportKeysDialog";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void filePath_TextChanged(object sender, System.EventArgs e)
		{
		
		}
	}
}
