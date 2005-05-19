using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for PropertyForm.
	/// </summary>
	public class PropertyForm : System.Windows.Forms.Form
	{
		private BrowserService browser;
		private DisplayNode node;
		private DisplayProperty property;
		private System.Windows.Forms.Button bOK;
		private System.Windows.Forms.Button bCancel;
		public StoreBrowser.PropertyEditor propertyEditor;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PropertyForm(BrowserService browser, DisplayNode node, DisplayProperty property)
		{
			this.browser = browser;
			this.node = node;
			this.property = property;
			
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			if (property != null)
			{
				propertyEditor.Name = property.Name;
				propertyEditor.tbName.Enabled = false;
				propertyEditor.Type = property.Type;
				propertyEditor.Value = property.Value;
				propertyEditor.cbType.Enabled = false;
				propertyEditor.Local = property.IsLocal;
				propertyEditor.MultiValued = property.IsMultiValued;
			}
			else
			{
				propertyEditor.tbName.Enabled = true;
				propertyEditor.Name = "";
				propertyEditor.cbType.Enabled = true;
				propertyEditor.Type = "String";
				propertyEditor.Value = "";
			}
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(PropertyForm));
			this.propertyEditor = new StoreBrowser.PropertyEditor();
			this.bOK = new System.Windows.Forms.Button();
			this.bCancel = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// propertyEditor
			// 
			this.propertyEditor.Local = false;
			this.propertyEditor.Location = new System.Drawing.Point(8, 24);
			this.propertyEditor.MultiValued = false;
			this.propertyEditor.Name = "propertyEditor";
			this.propertyEditor.Size = new System.Drawing.Size(400, 176);
			this.propertyEditor.TabIndex = 0;
			this.propertyEditor.Type = "";
			this.propertyEditor.Value = "";
			// 
			// bOK
			// 
			this.bOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bOK.Location = new System.Drawing.Point(8, 208);
			this.bOK.Name = "bOK";
			this.bOK.TabIndex = 1;
			this.bOK.Text = "OK";
			this.bOK.Click += new System.EventHandler(this.bOK_Click);
			// 
			// bCancel
			// 
			this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.bCancel.Location = new System.Drawing.Point(96, 208);
			this.bCancel.Name = "bCancel";
			this.bCancel.TabIndex = 2;
			this.bCancel.Text = "Cancel";
			this.bCancel.Click += new System.EventHandler(this.bCancel_Click);
			// 
			// PropertyForm
			// 
			this.AcceptButton = this.bOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.bCancel;
			this.ClientSize = new System.Drawing.Size(416, 246);
			this.Controls.Add(this.bCancel);
			this.Controls.Add(this.bOK);
			this.Controls.Add(this.propertyEditor);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PropertyForm";
			this.Text = "PropertyForm";
			this.ResumeLayout(false);

		}
		#endregion

		private void bOK_Click(object sender, System.EventArgs e)
		{
			string pType = (property == null) ? propertyEditor.Type : property.Type;
			if (propertyEditor.Value != null)
			{
				uint flags = 0;
				flags += propertyEditor.Local ? ( uint )0x00020000 : 0;
				flags += propertyEditor.MultiValued ? ( uint )0x00040000 : 0;

				if (property == null)
				{
					browser.AddProperty(node.CollectionID, node.ID, propertyEditor.Name, pType, propertyEditor.Value, flags);
				}
				else
				{
					browser.ModifyProperty(node.CollectionID, node.ID, property.Name, property.Type, property.Value, propertyEditor.Value, flags);
				}
			}

			Close();
		}

		private void bCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
