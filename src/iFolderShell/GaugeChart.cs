/***********************************************************************
 *  $RCSfile$
 *
 *  Copyright (C) 2004 Novell, Inc.
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public
 *  License as published by the Free Software Foundation; either
 *  version 2 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public
 *  License along with this program; if not, write to the Free
 *  Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 *
 *  Author: Bruce Getter <bgetter@novell.com>
 *
 ***********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Novell.iFolderCom
{
	/// <summary>
	/// Summary description for GaugeChart.
	/// </summary>
	[ComVisible(false)]
	public class GaugeChart : System.Windows.Forms.UserControl
	{
		#region Class Members
		private System.Windows.Forms.Label gauge;
		private double maxValue;
		private double resolution;
		private double used;
		private Color barColor = Color.Red;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion

		/// <summary>
		/// Constructs a GaugeChart object.
		/// </summary>
		public GaugeChart()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(GaugeChart));
			this.gauge = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// gauge
			// 
			this.gauge.AccessibleDescription = resources.GetString("gauge.AccessibleDescription");
			this.gauge.AccessibleName = resources.GetString("gauge.AccessibleName");
			this.gauge.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gauge.Anchor")));
			this.gauge.AutoSize = ((bool)(resources.GetObject("gauge.AutoSize")));
			this.gauge.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.gauge.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("gauge.Dock")));
			this.gauge.Enabled = ((bool)(resources.GetObject("gauge.Enabled")));
			this.gauge.Font = ((System.Drawing.Font)(resources.GetObject("gauge.Font")));
			this.gauge.Image = ((System.Drawing.Image)(resources.GetObject("gauge.Image")));
			this.gauge.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("gauge.ImageAlign")));
			this.gauge.ImageIndex = ((int)(resources.GetObject("gauge.ImageIndex")));
			this.gauge.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gauge.ImeMode")));
			this.gauge.Location = ((System.Drawing.Point)(resources.GetObject("gauge.Location")));
			this.gauge.Name = "gauge";
			this.gauge.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("gauge.RightToLeft")));
			this.gauge.Size = ((System.Drawing.Size)(resources.GetObject("gauge.Size")));
			this.gauge.TabIndex = ((int)(resources.GetObject("gauge.TabIndex")));
			this.gauge.Text = resources.GetString("gauge.Text");
			this.gauge.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("gauge.TextAlign")));
			this.gauge.Visible = ((bool)(resources.GetObject("gauge.Visible")));
			this.gauge.Paint += new System.Windows.Forms.PaintEventHandler(this.gauge_Paint);
			// 
			// GaugeChart
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.Controls.Add(this.gauge);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.Name = "GaugeChart";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.Size = ((System.Drawing.Size)(resources.GetObject("$this.Size")));
			this.ResumeLayout(false);

		}
		#endregion

		#region Event Handlers
		private void gauge_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			e.Graphics.Clear(Label.DefaultBackColor);
			Pen pen = new Pen(Color.Black);
			e.Graphics.DrawRectangle(pen, gauge.ClientRectangle);
			if (maxValue != 0)
			{
				int fillLevel = (int)((used/maxValue) * gauge.Size.Height);

				SolidBrush brush = new SolidBrush(barColor);
				Rectangle rect = new Rectangle(0, gauge.ClientSize.Height - fillLevel, gauge.ClientSize.Width, fillLevel);
				rect.Intersect(e.ClipRectangle);
				e.Graphics.FillRectangle(brush, rect);

				// Draw a black line across the top ... I like it better without.
//				int y = spaceChart.ClientSize.Height - height;
//				e.Graphics.DrawLine(pen, 0, y, spaceChart.ClientRectangle.Width, y);
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Sets the maximum value.
		/// </summary>
		public double MaxValue
		{
			set 
			{ 
				maxValue = value;
			}
		}

		/// <summary>
		/// Sets the color of the bar in the gauge.
		/// </summary>
		public Color BarColor
		{
			set { barColor = value; }
		}

		/// <summary>
		/// Sets the amount used.
		/// </summary>
		public double Used
		{
			set 
			{
				used = value;
			}
		}
		#endregion
	}
}
