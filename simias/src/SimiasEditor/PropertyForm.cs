using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using Simias;
using Simias.Storage;

namespace Simias.Editor
{
	/// <summary>
	/// Property Form
	/// </summary>
	public class PropertyForm : Form
	{
		private System.Windows.Forms.ComboBox typeComboBox;
		private System.Windows.Forms.TextBox NameTextBox;
		private System.Windows.Forms.TextBox valuesTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button acceptButton;
		private System.Windows.Forms.Button cancelButton;
		private System.ComponentModel.Container components = null;

		public PropertyForm(Collection collection, Node node)
			: this(collection, node, null)
		{
		}

		public PropertyForm(Collection collection, Node node, string name)
		{
			InitializeComponent();
			
			foreach(string item in Enum.GetNames(typeof(Syntax)))
			{
				typeComboBox.Items.Add(item);
			}

			typeComboBox.Text = Syntax.String.ToString();

			if (name != null)
			{
				NameTextBox.Text = name;

				MultiValuedList mvl = node.Properties.GetProperties(name);
				
				foreach(Property p in mvl)
				{
					valuesTextBox.Text += p.ToString() + Environment.NewLine; 
				
					typeComboBox.Text = Enum.GetName(typeof(Syntax), p.Type);
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			
			base.Dispose(disposing);
		}

		#region Windows Form Designer
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.typeComboBox = new System.Windows.Forms.ComboBox();
			this.NameTextBox = new System.Windows.Forms.TextBox();
			this.valuesTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.acceptButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// typeComboBox
			// 
			this.typeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.typeComboBox.Location = new System.Drawing.Point(80, 8);
			this.typeComboBox.Name = "typeComboBox";
			this.typeComboBox.Size = new System.Drawing.Size(248, 21);
			this.typeComboBox.TabIndex = 0;
			// 
			// NameTextBox
			// 
			this.NameTextBox.Location = new System.Drawing.Point(80, 40);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = new System.Drawing.Size(248, 20);
			this.NameTextBox.TabIndex = 1;
			this.NameTextBox.Text = "";
			// 
			// valuesTextBox
			// 
			this.valuesTextBox.Location = new System.Drawing.Point(80, 72);
			this.valuesTextBox.Multiline = true;
			this.valuesTextBox.Name = "valuesTextBox";
			this.valuesTextBox.Size = new System.Drawing.Size(248, 88);
			this.valuesTextBox.TabIndex = 2;
			this.valuesTextBox.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Type:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Name:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 72);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Value(s):";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// acceptButton
			// 
			this.acceptButton.Location = new System.Drawing.Point(184, 176);
			this.acceptButton.Name = "acceptButton";
			this.acceptButton.TabIndex = 6;
			this.acceptButton.Text = "&Accept";
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(264, 176);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "&Cancel";
			// 
			// PropertyForm
			// 
			this.AcceptButton = this.acceptButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(346, 208);
			this.ControlBox = false;
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.acceptButton);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.valuesTextBox);
			this.Controls.Add(this.NameTextBox);
			this.Controls.Add(this.typeComboBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "PropertyForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Property";
			this.ResumeLayout(false);

		}
		
		#endregion
	}
}
