namespace Timesheeter.UserControls
{
    partial class UCTimeEntries
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
            label1 = new Label();
            projectCodeComboBox1 = new Timesheeter.Elements.ProjectCodeComboBox();
            _dtpDate = new DateTimePicker();
            label2 = new Label();
            _pnlElements.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(label2);
            _pnlElements.Controls.Add(_dtpDate);
            _pnlElements.Controls.Add(projectCodeComboBox1);
            _pnlElements.Controls.Add(label1);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 19);
            label1.Name = "label1";
            label1.Size = new Size(75, 15);
            label1.TabIndex = 0;
            label1.Text = "Project Code";
            // 
            // projectCodeComboBox1
            // 
            projectCodeComboBox1.DisplayMember = "Name";
            projectCodeComboBox1.FormattingEnabled = true;
            projectCodeComboBox1.Location = new Point(16, 37);
            projectCodeComboBox1.Name = "projectCodeComboBox1";
            projectCodeComboBox1.Size = new Size(182, 23);
            projectCodeComboBox1.TabIndex = 1;
            projectCodeComboBox1.ValueMember = "ID";
            // 
            // _dtpDate
            // 
            _dtpDate.Location = new Point(16, 88);
            _dtpDate.MaxDate = new DateTime(3000, 12, 31, 0, 0, 0, 0);
            _dtpDate.MinDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);
            _dtpDate.Name = "_dtpDate";
            _dtpDate.Size = new Size(200, 23);
            _dtpDate.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(16, 70);
            label2.Name = "label2";
            label2.Size = new Size(31, 15);
            label2.TabIndex = 3;
            label2.Text = "Date";
            // 
            // UCTimeEntries
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCTimeEntries";
            _pnlElements.ResumeLayout(false);
            _pnlElements.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Elements.ProjectCodeComboBox projectCodeComboBox1;
        private Label label1;
        private Label label2;
        private DateTimePicker _dtpDate;
    }
}
