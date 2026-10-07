namespace Timesheeter.UserControls
{
    partial class UCCreateTimesheet
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
            datePicker1 = new Timesheeter.Lib.DatePicker();
            label1 = new Label();
            projectCodeComboBox1 = new Timesheeter.Elements.ProjectCodeComboBox();
            label2 = new Label();
            timeEntriesDataGridView1 = new Timesheeter.Elements.TimeEntriesDataGridView();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)timeEntriesDataGridView1).BeginInit();
            SuspendLayout();
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(label2);
            _pnlSideBar.Controls.Add(projectCodeComboBox1);
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(datePicker1);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(timeEntriesDataGridView1);
            // 
            // datePicker1
            // 
            datePicker1.Format = DateTimePickerFormat.Short;
            datePicker1.Location = new Point(3, 18);
            datePicker1.Name = "datePicker1";
            datePicker1.Size = new Size(289, 23);
            datePicker1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(70, 15);
            label1.TabIndex = 1;
            label1.Text = "Select Week";
            // 
            // projectCodeComboBox1
            // 
            projectCodeComboBox1.DisplayMember = "Code";
            projectCodeComboBox1.FormattingEnabled = true;
            projectCodeComboBox1.Location = new Point(3, 72);
            projectCodeComboBox1.Name = "projectCodeComboBox1";
            projectCodeComboBox1.Size = new Size(289, 23);
            projectCodeComboBox1.TabIndex = 2;
            projectCodeComboBox1.ValueMember = "ID";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 54);
            label2.Name = "label2";
            label2.Size = new Size(78, 15);
            label2.TabIndex = 3;
            label2.Text = "Select Project";
            // 
            // timeEntriesDataGridView1
            // 
            timeEntriesDataGridView1.AllowUserToAddRows = false;
            timeEntriesDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            timeEntriesDataGridView1.Dock = DockStyle.Fill;
            timeEntriesDataGridView1.Location = new Point(0, 0);
            timeEntriesDataGridView1.Name = "timeEntriesDataGridView1";
            timeEntriesDataGridView1.Size = new Size(672, 649);
            timeEntriesDataGridView1.TabIndex = 0;
            // 
            // UCCreateTimesheet
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCCreateTimesheet";
            _pnlElements.ResumeLayout(false);
            _pnlSideBar.ResumeLayout(false);
            _pnlSideBar.PerformLayout();
            _pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)timeEntriesDataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Lib.DatePicker datePicker1;
        private Label label2;
        private Elements.ProjectCodeComboBox projectCodeComboBox1;
        private Elements.TimeEntriesDataGridView timeEntriesDataGridView1;
    }
}
