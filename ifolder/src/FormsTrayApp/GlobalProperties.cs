using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Simias;
using Simias.Sync;

namespace Novell.iFolder.FormsTrayApp
{
	/// <summary>
	/// Summary description for GlobalProperties.
	/// </summary>
	public class GlobalProperties : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown defaultInterval;
		private System.Windows.Forms.CheckBox displayConfirmation;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public GlobalProperties()
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
			this.label1 = new System.Windows.Forms.Label();
			this.defaultInterval = new System.Windows.Forms.NumericUpDown();
			this.displayConfirmation = new System.Windows.Forms.CheckBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
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
			this.defaultInterval.Location = new System.Drawing.Point(248, 16);
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
			this.defaultInterval.TabIndex = 1;
			// 
			// displayConfirmation
			// 
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
			// GlobalProperties
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(384, 382);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.displayConfirmation);
			this.Controls.Add(this.defaultInterval);
			this.Controls.Add(this.label1);
			this.Name = "GlobalProperties";
			this.Text = "Global iFolder Properties";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.GlobalProperties_Closing);
			this.Load += new System.EventHandler(this.GlobalProperties_Load);
			((System.ComponentModel.ISupportInitialize)(this.defaultInterval)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void GlobalProperties_Load(object sender, System.EventArgs e)
		{
			try
			{
				Configuration config = new Configuration();

				// Initialize defaultInterval.
				SyncProperties syncProperties = new SyncProperties(config);
				defaultInterval.Value = (decimal)syncProperties.Interval;

				// Initialize displayConfirmation.
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

		private void GlobalProperties_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				Configuration config = new Configuration();

				SyncProperties syncProperties = new SyncProperties(config);
				syncProperties.Interval = (int)defaultInterval.Value;

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
	}
}
