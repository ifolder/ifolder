using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using Simias;
using Win32Util;

namespace Novell.iFolder.iFolderCom
{
	/// <summary>
	/// Summary description for NewiFolder.
	/// </summary>
	[ComVisible(false)]
	public class NewiFolder : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button close;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel iFolderProperties;
		private System.Windows.Forms.PictureBox iFolderEmblem;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox dontAsk;
		private string folderName;
		private string loadPath;
		private const int SHOP_FILEPATH = 0x2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public NewiFolder()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Center the window.
			this.StartPosition = FormStartPosition.CenterScreen;
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
			this.close = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.iFolderProperties = new System.Windows.Forms.LinkLabel();
			this.iFolderEmblem = new System.Windows.Forms.PictureBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.dontAsk = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// close
			// 
			this.close.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.close.Location = new System.Drawing.Point(376, 160);
			this.close.Name = "close";
			this.close.TabIndex = 0;
			this.close.Text = "Close";
			this.close.Click += new System.EventHandler(this.close_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(424, 24);
			this.label1.TabIndex = 1;
			this.label1.Text = "Congratulations, you\'ve just converted this folder into an iFolder!  ...";
			// 
			// iFolderProperties
			// 
			this.iFolderProperties.Location = new System.Drawing.Point(352, 125);
			this.iFolderProperties.Name = "iFolderProperties";
			this.iFolderProperties.Size = new System.Drawing.Size(136, 16);
			this.iFolderProperties.TabIndex = 2;
			this.iFolderProperties.TabStop = true;
			this.iFolderProperties.Text = "by clicking here.";
			this.iFolderProperties.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.iFolderProperties_LinkClicked);
			// 
			// iFolderEmblem
			// 
			this.iFolderEmblem.Location = new System.Drawing.Point(16, 52);
			this.iFolderEmblem.Name = "iFolderEmblem";
			this.iFolderEmblem.Size = new System.Drawing.Size(48, 48);
			this.iFolderEmblem.TabIndex = 3;
			this.iFolderEmblem.TabStop = false;
			this.iFolderEmblem.Paint += new System.Windows.Forms.PaintEventHandler(this.iFolderEmblem_Paint);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(72, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(368, 32);
			this.label2.TabIndex = 4;
			this.label2.Text = "iFolders are identified by the Novell iFolder emblem being placed on the folder a" +
				"s shown.";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 112);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(424, 32);
			this.label3.TabIndex = 5;
			this.label3.Text = "In order to fully utilize your new iFolder, you need to share it.  You can share " +
				"the iFolder by right-clicking the folder and accessing the iFolder menu or";
			// 
			// dontAsk
			// 
			this.dontAsk.Location = new System.Drawing.Point(16, 160);
			this.dontAsk.Name = "dontAsk";
			this.dontAsk.Size = new System.Drawing.Size(304, 16);
			this.dontAsk.TabIndex = 6;
			this.dontAsk.Text = "Please don\'t show me this again.";
			// 
			// NewiFolder
			// 
			this.AcceptButton = this.close;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(456, 192);
			this.Controls.Add(this.dontAsk);
			this.Controls.Add(this.iFolderProperties);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.iFolderEmblem);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.close);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "NewiFolder";
			this.Text = "iFolder Introduction";
			this.ResumeLayout(false);

		}
		#endregion

		#region Properties
		public string FolderName
		{
			get
			{
				return folderName;
			}
			set
			{
				folderName = value;
			}
		}

		public string LoadPath
		{
			get
			{
				return loadPath;
			}

			set
			{
				loadPath = value;
			}
		}
		#endregion

		#region Event Handlers
		private void iFolderProperties_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			// Invoke the iFolder properties dialog.
//			Win32Window.ShObjectProperties(IntPtr.Zero, SHOP_FILEPATH, FolderName, "iFolder");
			iFolderComponent ifCom = new iFolderComponent();
			ifCom.InvokeAdvancedDlg(LoadPath, FolderName, false);
		}

		private void close_Click(object sender, System.EventArgs e)
		{
			if (dontAsk.Checked)
			{
				new Configuration().Set("iFolderShell", "Show wizard", "false");
			}

			this.Close();
		}

		private void iFolderEmblem_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			try
			{
				IFSHFILEINFO fi;
				IntPtr ret = Win32Window.ShGetFileInfo(
					FolderName, 
					Win32Window.FILE_ATTRIBUTE_DIRECTORY,
					out fi,
					342,
					Win32Window.SHGFI_ICON | Win32Window.SHGFI_USEFILEATTRIBUTES);

				if (ret != IntPtr.Zero)
				{
					Bitmap bmap = Bitmap.FromHicon(fi.hIcon);
					e.Graphics.DrawImage(bmap, 0, 0);

					IntPtr hIcon = Win32Window.LoadImageFromFile(
						0,
						Path.Combine(loadPath, "ifolder_emblem.ico"),
						Win32Window.IMAGE_ICON,
						32,
						32,
						Win32Window.LR_LOADFROMFILE);

					bmap = Bitmap.FromHicon(hIcon);
					e.Graphics.DrawImage(bmap, 0, 0);
				}
			}
			catch{}
		}
		#endregion
	}
}
