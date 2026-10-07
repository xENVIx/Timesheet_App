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
            _dp = new Timesheeter.Lib.DatePicker();
            label1 = new Label();
            _cbProjects = new Timesheeter.Elements.ProjectCodeComboBox();
            label2 = new Label();
            _dgvTimeEntries = new Timesheeter.Elements.TimeEntriesDataGridView();
            _btnGenerate = new Button();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvTimeEntries).BeginInit();
            SuspendLayout();
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(_btnGenerate);
            _pnlSideBar.Controls.Add(label2);
            _pnlSideBar.Controls.Add(_cbProjects);
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(_dp);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(_dgvTimeEntries);
            // 
            // _dp
            // 
            _dp.Format = DateTimePickerFormat.Short;
            _dp.Location = new Point(3, 18);
            _dp.Name = "_dp";
            _dp.Size = new Size(289, 23);
            _dp.TabIndex = 0;
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
            // _cbProjects
            // 
            _cbProjects.DisplayMember = "Code";
            _cbProjects.FormattingEnabled = true;
            _cbProjects.Location = new Point(3, 62);
            _cbProjects.Name = "_cbProjects";
            _cbProjects.Size = new Size(289, 23);
            _cbProjects.TabIndex = 2;
            _cbProjects.ValueMember = "ID";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 44);
            label2.Name = "label2";
            label2.Size = new Size(78, 15);
            label2.TabIndex = 3;
            label2.Text = "Select Project";
            // 
            // _dgvTimeEntries
            // 
            _dgvTimeEntries.AllowUserToAddRows = false;
            _dgvTimeEntries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvTimeEntries.Dock = DockStyle.Fill;
            _dgvTimeEntries.Location = new Point(0, 0);
            _dgvTimeEntries.Name = "_dgvTimeEntries";
            _dgvTimeEntries.Size = new Size(672, 649);
            _dgvTimeEntries.TabIndex = 0;
            // 
            // _btnGenerate
            // 
            _btnGenerate.Location = new Point(3, 91);
            _btnGenerate.Name = "_btnGenerate";
            _btnGenerate.Size = new Size(289, 23);
            _btnGenerate.TabIndex = 4;
            _btnGenerate.Text = "Generate Timesheet";
            _btnGenerate.UseVisualStyleBackColor = true;
            _btnGenerate.Click += _btnGenerate_Click;
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
            ((System.ComponentModel.ISupportInitialize)_dgvTimeEntries).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Lib.DatePicker _dp;
        private Label label2;
        private Elements.ProjectCodeComboBox _cbProjects;
        private Elements.TimeEntriesDataGridView _dgvTimeEntries;
        private Button _btnGenerate;
    }
}
