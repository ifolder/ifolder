using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Simias;
using Simias.Sync;
using Novell.iFolder;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
	{
		#region Class Members
		private static readonly ISimiasLog logger = SimiasLogManager.GetLogger(typeof(FormsTrayApp));

		private iFolderManager iFManager = null;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button ok;
		private System.Windows.Forms.Button cancel;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		public GlobalProperties()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			iFManager = iFolderManager.Connect();
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
			this.label1 = new System.Windows.Forms.Label();
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.ok = new System.Windows.Forms.Button();
			this.cancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(184, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Default refresh interval:";
			// 
			// defaultInterval
			// 
			this.defaultInterval.Increment = new System.Decimal(new int[] {
																			  5,
																			  0,
																			  0,
																			  0});
			this.defaultInterval.Location = new System.Drawing.Point(200, 16);
			this.defaultInterval.Maximum = new System.Decimal(new int[] {
																			86400,
																			0,
																			0,
																			0});
			this.defaultInterval.Minimum = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			-2147483648});
			this.defaultInterval.Name = "defaultInterval";
			this.defaultInterval.Size = new System.Drawing.Size(80, 20);
			this.defaultInterval.TabIndex = 1;
			// 
			// displayConfirmation
			// 
			this.displayConfirmation.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.displayConfirmation.Location = new System.Drawing.Point(8, 64);
			this.displayConfirmation.Name = "displayConfirmation";
			this.displayConfirmation.Size = new System.Drawing.Size(360, 24);
			this.displayConfirmation.TabIndex = 2;
			this.displayConfirmation.Text = "Display iFolder creation confirmation.";
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(8, 48);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(360, 4);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Location = new System.Drawing.Point(8, 96);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(360, 4);
			this.groupBox2.TabIndex = 4;
			this.groupBox2.TabStop = false;
			// 
			// ok
			// 
			this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ok.Location = new System.Drawing.Point(216, 352);
			this.ok.Name = "ok";
			this.ok.TabIndex = 5;
			this.ok.Text = "OK";
			this.ok.Click += new System.EventHandler(this.ok_Click);
			// 
			// cancel
			// 
			this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancel.Location = new System.Drawing.Point(296, 352);
			this.cancel.Name = "cancel";
			this.cancel.TabIndex = 6;
			this.cancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(288, 18);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "seconds";
			// 
			// GlobalProperties
			// 
			this.AcceptButton = this.ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancel;
			this.ClientSize = new System.Drawing.Size(376, 382);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cancel);
			this.Controls.Add(this.ok);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.displayConfirmation);
			this.Controls.Add(this.defaultInterval);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GlobalProperties";
			this.Text = "Global iFolder Properties";
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void GlobalProperties_Load(object sender, System.EventArgs e)
		{
			// Load the application icon.
			try
			{
				this.Icon = new Icon(Path.Combine(Application.StartupPath, @"res\ifolder_loaded.ico"));
			}
			catch (Exception ex)
			{
				logger.Debug(ex, "Loading icon");
			}

			try
			{
				// Display the default sync interval.
				defaultInterval.Value = (decimal)iFManager.DefaultRefreshInterval;

				// Initialize displayConfirmation.
				Configuration config = new Configuration();
				string showWizard = config.Get("iFolderShell", "Show wizard", "true");
				displayConfirmation.Checked = showWizard == "true";
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
			}
		}

		private void ok_Click(object sender, System.EventArgs e)
		{
			try
			{
				// Save the default sync interval.
				iFManager.DefaultRefreshInterval = (int)defaultInterval.Value;

				Configuration config = new Configuration();
				if (displayConfirmation.Checked)
				{
					config.Set("iFolderShell", "Show wizard", "true");
				}
				else
				{
					config.Set("iFolderShell", "Show wizard", "false");
				}
			}
			catch (SimiasException ex)
			{
				ex.LogError();
			}
			catch (Exception ex)
			{
			}
		}
		#endregion
	}
}
