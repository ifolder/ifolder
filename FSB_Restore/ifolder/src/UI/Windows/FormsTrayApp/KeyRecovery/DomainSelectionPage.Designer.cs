namespace Novell.Wizard
{
    partial class DomainSelectionPage
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.domainSelectionFirst = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.domainComboBox = new System.Windows.Forms.ComboBox();
            this.recoveryAgent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // domainSelectionFirst
            // 
            this.domainSelectionFirst.AutoSize = true;
            this.domainSelectionFirst.Location = new System.Drawing.Point(25, 88);
            this.domainSelectionFirst.Name = "domainSelectionFirst";
            this.domainSelectionFirst.Size = new System.Drawing.Size(299, 13);
            this.domainSelectionFirst.TabIndex = 2;
            this.domainSelectionFirst.Text = TrayApp.Properties.Resources.domainSelectionFirst;// "Select the account for which the passphrase must to be reset.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(72, 146);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = TrayApp.Properties.Resources.iFolderAcc;// "iFolder account :";
            // 
            // domainComboBox
            // 
            this.domainComboBox.FormattingEnabled = true;
            this.domainComboBox.Location = new System.Drawing.Point(205, 138);
            this.domainComboBox.Name = "domainComboBox";
            this.domainComboBox.Size = new System.Drawing.Size(194, 21);
            this.domainComboBox.TabIndex = 4;
            // 
            // recoveryAgent
            // 
            this.recoveryAgent.AutoSize = true;
            this.recoveryAgent.Location = new System.Drawing.Point(25, 202);
            this.recoveryAgent.Name = "recoveryAgent";
            this.recoveryAgent.Size = new System.Drawing.Size(112, 13);
            this.recoveryAgent.TabIndex = 6;
            this.recoveryAgent.Text = TrayApp.Properties.Resources.domainSelectionPageSecond;
            // 
            // DomainSelectionPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.recoveryAgent);
            this.Controls.Add(this.domainComboBox);
            this.Controls.Add(this.domainSelectionFirst);
            this.Controls.Add(this.label2);
            this.Name = "DomainSelectionPage";
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.domainSelectionFirst, 0);
            this.Controls.SetChildIndex(this.domainComboBox, 0);
            this.Controls.SetChildIndex(this.recoveryAgent, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label domainSelectionFirst;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox domainComboBox;
        private System.Windows.Forms.Label recoveryAgent;
    }
}
