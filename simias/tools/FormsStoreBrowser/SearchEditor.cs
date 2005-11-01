using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace StoreBrowser
{
	/// <summary>
	/// Summary description for SearchEditor.
	/// </summary>
	public class SearchEditor : System.Windows.Forms.UserControl
	{
		private IStoreBrowser browser;
//		private Search search;
		private TreeNode tNode;
		private System.Windows.Forms.TextBox target;
		private System.Windows.Forms.TextBox propertyName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox propertyType;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox propertyValue;
		private System.Windows.Forms.ComboBox operation;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button searchButton;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SearchEditor()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		#region Properties
		public Button SearchButton
		{
			get
			{
				return searchButton;
			}
		}

		public IStoreBrowser Browser
		{
			set
			{
				browser = value;
			}
		}

		public TreeNode SearchNode
		{
			set
			{
				tNode = value;

				SearchExpression search = (SearchExpression)tNode.Tag;
				if ( search != null && search.Target != null )
				{
					target.Text = search.Target.Name;
					propertyName.Text = search.PropertyName;
					propertyType.Text = search.PropertyType;
					propertyValue.Text = search.PropertyValue;
					operation.Text = search.Operation;
				}
				else
				{
					target.Text = "Store";

					propertyName.Text = search.PropertyName;
					propertyType.Text = search.PropertyType;
					propertyValue.Text = search.PropertyValue;
					operation.Text = search.Operation;
				}
			}
		}
		#endregion

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
			this.target = new System.Windows.Forms.TextBox();
			this.propertyName = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.propertyValue = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.propertyType = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.operation = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.searchButton = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// target
			// 
			this.target.Location = new System.Drawing.Point(96, 16);
			this.target.Name = "target";
			this.target.ReadOnly = true;
			this.target.Size = new System.Drawing.Size(288, 20);
			this.target.TabIndex = 0;
			this.target.Text = "";
			// 
			// propertyName
			// 
			this.propertyName.Location = new System.Drawing.Point(56, 24);
			this.propertyName.Name = "propertyName";
			this.propertyName.Size = new System.Drawing.Size(216, 20);
			this.propertyName.TabIndex = 1;
			this.propertyName.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 16);
			this.label1.TabIndex = 2;
			this.label1.Text = "Name:";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.propertyValue);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.propertyType);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.propertyName);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(96, 51);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(288, 120);
			this.groupBox1.TabIndex = 3;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Property";
			// 
			// propertyValue
			// 
			this.propertyValue.Location = new System.Drawing.Point(56, 88);
			this.propertyValue.Name = "propertyValue";
			this.propertyValue.Size = new System.Drawing.Size(216, 20);
			this.propertyValue.TabIndex = 6;
			this.propertyValue.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 90);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(48, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Value:";
			// 
			// propertyType
			// 
			this.propertyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.propertyType.Items.AddRange(new object[] {
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
			this.propertyType.Location = new System.Drawing.Point(56, 56);
			this.propertyType.Name = "propertyType";
			this.propertyType.Size = new System.Drawing.Size(216, 21);
			this.propertyType.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(56, 16);
			this.label2.TabIndex = 3;
			this.label2.Text = "Type:";
			// 
			// operation
			// 
			this.operation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.operation.Items.AddRange(new object[] {
														   "Equal",
														   "Not_Equal",
														   "Begins",
														   "Ends",
														   "Contains",
														   "Greater",
														   "Less",
														   "Greater_Equal",
														   "Less_Equal",
														   "Exists",
														   "CaseEqual"});
			this.operation.Location = new System.Drawing.Point(96, 186);
			this.operation.Name = "operation";
			this.operation.Size = new System.Drawing.Size(288, 21);
			this.operation.TabIndex = 4;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 188);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(100, 16);
			this.label4.TabIndex = 5;
			this.label4.Text = "Search operator:";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 18);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 16);
			this.label5.TabIndex = 7;
			this.label5.Text = "Search target:";
			// 
			// searchButton
			// 
			this.searchButton.Location = new System.Drawing.Point(296, 222);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(88, 23);
			this.searchButton.TabIndex = 8;
			this.searchButton.Text = "&Search";
			this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
			// 
			// SearchEditor
			// 
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.target);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.operation);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.groupBox1);
			this.Name = "SearchEditor";
			this.Size = new System.Drawing.Size(416, 264);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void searchButton_Click(object sender, System.EventArgs e)
		{
			SearchExpression searchExpression = (SearchExpression)tNode.Tag;

			searchExpression.PropertyName = propertyName.Text;
			searchExpression.PropertyType = propertyType.Text;
			searchExpression.PropertyValue = propertyValue.Text;
			searchExpression.Operation = operation.Text;

			tNode.Tag = searchExpression;
			
			browser.Search( searchExpression );
		}
	}
}
