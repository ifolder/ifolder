using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Simias.Storage;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for PropertyForm.
	/// </summary>
	public class PropertyForm : System.Windows.Forms.Form
	{
		private Collection collection;
		private Node node;
		private Property property;
		private System.Windows.Forms.Button bOK;
		private System.Windows.Forms.Button bCancel;
		public StoreBrowser.PropertyEditor propertyEditor;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PropertyForm(Collection col, Node node, Property property)
		{
			this.collection = col;
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
				propertyEditor.Type = property.Type.ToString();
				propertyEditor.Value = property.ToString();
				propertyEditor.cbType.Enabled = false;
				propertyEditor.Local = property.LocalProperty;
				propertyEditor.MultiValued = property.MultiValuedProperty;
			}
			else
			{
				propertyEditor.tbName.Enabled = true;
				propertyEditor.Name = "";
				propertyEditor.cbType.Enabled = true;
				propertyEditor.Type = Syntax.String.ToString();
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
			this.Name = "PropertyForm";
			this.Text = "PropertyForm";
			this.ResumeLayout(false);

		}
		#endregion

		private void bOK_Click(object sender, System.EventArgs e)
		{
			Syntax pType;
			object pValue = null;
			pType = property == null ? (Syntax)Enum.Parse(typeof(Syntax), propertyEditor.Type) : property.Type;
			
			switch (pType)
			{
				case Syntax.Boolean:
					pValue = Boolean.Parse(propertyEditor.Value);
					break;
				case Syntax.Byte:
					pValue = Byte.Parse(propertyEditor.Value);
					break;
				case Syntax.Char:
					pValue = Char.Parse(propertyEditor.Value);
					break;
				case Syntax.DateTime:
					pValue = DateTime.Parse(propertyEditor.Value);
					break;
				case Syntax.Int16:
					pValue = Int16.Parse(propertyEditor.Value);
					break;
				case Syntax.Int32:
					pValue = Int32.Parse(propertyEditor.Value);
					break;
				case Syntax.Int64:
					pValue = Int64.Parse(propertyEditor.Value);
					break;
				case Syntax.Relationship:
					break;
				case Syntax.SByte:
					pValue = SByte.Parse(propertyEditor.Value);
					break;
				case Syntax.Single:
					pValue = Single.Parse(propertyEditor.Value);
					break;
				case Syntax.String:
					pValue = propertyEditor.Value;
					break;
				case Syntax.TimeSpan:
					pValue = TimeSpan.Parse(propertyEditor.Value);
					break;
				case Syntax.UInt16:
					pValue = UInt16.Parse(propertyEditor.Value);
					break;
				case Syntax.UInt32:
					pValue = UInt32.Parse(propertyEditor.Value);
					break;
				case Syntax.UInt64:
					pValue = UInt64.Parse(propertyEditor.Value);
					break;
				case Syntax.Uri:
					pValue = new Uri(propertyEditor.Value);
					break;
				case Syntax.XmlDocument:
					System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
					doc.LoadXml(propertyEditor.Value);
					pValue = doc;
					break;
			}
			if (pValue != null)
			{
				if (property == null)
				{
					property = new Property(propertyEditor.Name, pValue);
				}
				else
				{
					property.SetValue(pValue);
				}
				property.LocalProperty = propertyEditor.Local;
				property.MultiValuedProperty = propertyEditor.MultiValued;
				node.Properties.ModifyProperty(property);
				collection.Commit(node);
			}
			Close();
		}

		private void bCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
