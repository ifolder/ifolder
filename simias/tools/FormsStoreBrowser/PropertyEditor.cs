using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for UserControl1.
	/// </summary>
	public class PropertyEditor : System.Windows.Forms.UserControl
	{
		public System.Windows.Forms.ComboBox cbType;
		public System.Windows.Forms.TextBox tbName;
		private System.Windows.Forms.Label lName;
		private System.Windows.Forms.Label lType;
		private System.Windows.Forms.Label lValue;
		private System.Windows.Forms.TextBox tbValue;
		private System.Windows.Forms.CheckBox cbLocal;
		private System.Windows.Forms.CheckBox cbMultiValued;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PropertyEditor()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		public new string Name
		{
			get
			{
				return tbName.Text;
			}

			set
			{
				tbName.Text = value;
			}
		}

		public string Type
		{
			get
			{
				return cbType.Text;
			}
			set
			{
				cbType.Text = value;
				//cbType.SelectedText = value;
				//cbType.Refresh();
			}
		}

		public string Value
		{
			get
			{
				return tbValue.Text;
			}
			set
			{
				tbValue.Text = value;
			}
		}

		public bool Local
		{
			get
			{
				return cbLocal.Checked;
			}
			set
			{
				cbLocal.Checked = value;
			}
		}

		public bool MultiValued
		{
			get
			{
				return cbMultiValued.Checked;
			}
			set
			{
				cbMultiValued.Checked = value;
			}
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cbType = new System.Windows.Forms.ComboBox();
			this.tbName = new System.Windows.Forms.TextBox();
			this.lName = new System.Windows.Forms.Label();
			this.lType = new System.Windows.Forms.Label();
			this.lValue = new System.Windows.Forms.Label();
			this.tbValue = new System.Windows.Forms.TextBox();
			this.cbLocal = new System.Windows.Forms.CheckBox();
			this.cbMultiValued = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// cbType
			// 
			this.cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbType.Items.AddRange(new object[] {
														"Boolean",
														"Byte",
														"Char",
														"DateTime",
														"Int16",
														"Int32",
														"Int64",
														"Relationship",
														"SByte",
														"Single",
														"String",
														"TimeSpan",
														"UInt16",
														"UInt32",
														"UInt64",
														"Uri",
														"XmlDocument"});
			this.cbType.Location = new System.Drawing.Point(72, 56);
			this.cbType.MaxDropDownItems = 17;
			this.cbType.Name = "cbType";
			this.cbType.Size = new System.Drawing.Size(296, 21);
			this.cbType.Sorted = true;
			this.cbType.TabIndex = 3;
			// 
			// tbName
			// 
			this.tbName.Location = new System.Drawing.Point(72, 24);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(296, 20);
			this.tbName.TabIndex = 1;
			this.tbName.Text = "";
			// 
			// lName
			// 
			this.lName.Location = new System.Drawing.Point(8, 24);
			this.lName.Name = "lName";
			this.lName.Size = new System.Drawing.Size(40, 23);
			this.lName.TabIndex = 0;
			this.lName.Text = "Name";
			// 
			// lType
			// 
			this.lType.Location = new System.Drawing.Point(8, 56);
			this.lType.Name = "lType";
			this.lType.Size = new System.Drawing.Size(40, 23);
			this.lType.TabIndex = 2;
			this.lType.Text = "Type";
			// 
			// lValue
			// 
			this.lValue.Location = new System.Drawing.Point(8, 88);
			this.lValue.Name = "lValue";
			this.lValue.Size = new System.Drawing.Size(40, 23);
			this.lValue.TabIndex = 4;
			this.lValue.Text = "Value";
			// 
			// tbValue
			// 
			this.tbValue.Location = new System.Drawing.Point(72, 88);
			this.tbValue.Name = "tbValue";
			this.tbValue.Size = new System.Drawing.Size(296, 20);
			this.tbValue.TabIndex = 5;
			this.tbValue.Text = "";
			// 
			// cbLocal
			// 
			this.cbLocal.Location = new System.Drawing.Point(72, 120);
			this.cbLocal.Name = "cbLocal";
			this.cbLocal.TabIndex = 6;
			this.cbLocal.Text = "Local";
			// 
			// cbMultiValued
			// 
			this.cbMultiValued.Location = new System.Drawing.Point(72, 144);
			this.cbMultiValued.Name = "cbMultiValued";
			this.cbMultiValued.TabIndex = 7;
			this.cbMultiValued.Text = "Multi Valued";
			// 
			// PropertyEditor
			// 
			this.Controls.Add(this.cbMultiValued);
			this.Controls.Add(this.cbLocal);
			this.Controls.Add(this.tbValue);
			this.Controls.Add(this.lValue);
			this.Controls.Add(this.lType);
			this.Controls.Add(this.lName);
			this.Controls.Add(this.tbName);
			this.Controls.Add(this.cbType);
			this.Name = "PropertyEditor";
			this.Size = new System.Drawing.Size(400, 176);
			this.ResumeLayout(false);

		}
		#endregion

		private void cbFlags_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		private void lFlags_Click(object sender, System.EventArgs e)
		{
		
		}
		
	}
}
