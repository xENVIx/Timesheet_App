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
            _btnAdd = new Button();
            _cbProjCodes = new Timesheeter.Elements.ProjectCodeComboBox();
            _dgvTimeEntries = new Timesheeter.Elements.TimeEntriesDataGridView();
            _dtpDate = new Timesheeter.Lib.DatePicker();
            _tpEnd = new Timesheeter.Lib.TimePicker();
            _tpStart = new Timesheeter.Lib.TimePicker();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            _tbComment = new TextBox();
            label5 = new Label();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvTimeEntries).BeginInit();
            SuspendLayout();
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(label5);
            _pnlSideBar.Controls.Add(_tbComment);
            _pnlSideBar.Controls.Add(label4);
            _pnlSideBar.Controls.Add(label3);
            _pnlSideBar.Controls.Add(label2);
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(_tpStart);
            _pnlSideBar.Controls.Add(_tpEnd);
            _pnlSideBar.Controls.Add(_dtpDate);
            _pnlSideBar.Controls.Add(_cbProjCodes);
            _pnlSideBar.Controls.Add(_btnAdd);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(_dgvTimeEntries);
            // 
            // _btnAdd
            // 
            _btnAdd.Location = new Point(3, 182);
            _btnAdd.Name = "_btnAdd";
            _btnAdd.Size = new Size(90, 23);
            _btnAdd.TabIndex = 6;
            _btnAdd.Text = "Add Entry";
            _btnAdd.UseVisualStyleBackColor = true;
            _btnAdd.Click += _btnAdd_Click;
            // 
            // _cbProjCodes
            // 
            _cbProjCodes.DisplayMember = "Code";
            _cbProjCodes.FormattingEnabled = true;
            _cbProjCodes.Location = new Point(3, 21);
            _cbProjCodes.Name = "_cbProjCodes";
            _cbProjCodes.Size = new Size(289, 23);
            _cbProjCodes.TabIndex = 1;
            _cbProjCodes.ValueMember = "ID";
            // 
            // _dgvTimeEntries
            // 
            _dgvTimeEntries.AllowUserToAddRows = false;
            _dgvTimeEntries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvTimeEntries.Dock = DockStyle.Fill;
            _dgvTimeEntries.Location = new Point(0, 0);
            _dgvTimeEntries.Name = "_dgvTimeEntries";
            _dgvTimeEntries.Size = new Size(672, 616);
            _dgvTimeEntries.TabIndex = 7;
            // 
            // _dtpDate
            // 
            _dtpDate.Format = DateTimePickerFormat.Short;
            _dtpDate.Location = new Point(3, 65);
            _dtpDate.MaxDate = new DateTime(3000, 12, 31, 0, 0, 0, 0);
            _dtpDate.MinDate = new DateTime(2000, 1, 1, 0, 0, 0, 0);
            _dtpDate.Name = "_dtpDate";
            _dtpDate.Size = new Size(289, 23);
            _dtpDate.TabIndex = 2;
            // 
            // _tpEnd
            // 
            _tpEnd.CustomFormat = "HH:mm";
            _tpEnd.Format = DateTimePickerFormat.Custom;
            _tpEnd.Location = new Point(160, 109);
            _tpEnd.Name = "_tpEnd";
            _tpEnd.ShowUpDown = true;
            _tpEnd.Size = new Size(132, 23);
            _tpEnd.TabIndex = 4;
            // 
            // _tpStart
            // 
            _tpStart.CustomFormat = "HH:mm";
            _tpStart.Format = DateTimePickerFormat.Custom;
            _tpStart.Location = new Point(3, 109);
            _tpStart.Name = "_tpStart";
            _tpStart.ShowUpDown = true;
            _tpStart.Size = new Size(132, 23);
            _tpStart.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(75, 15);
            label1.TabIndex = 14;
            label1.Text = "Project Code";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 47);
            label2.Name = "label2";
            label2.Size = new Size(31, 15);
            label2.TabIndex = 15;
            label2.Text = "Date";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 91);
            label3.Name = "label3";
            label3.Size = new Size(31, 15);
            label3.TabIndex = 16;
            label3.Text = "Start";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(160, 91);
            label4.Name = "label4";
            label4.Size = new Size(27, 15);
            label4.TabIndex = 17;
            label4.Text = "End";
            // 
            // _tbComment
            // 
            _tbComment.Location = new Point(3, 153);
            _tbComment.Name = "_tbComment";
            _tbComment.Size = new Size(289, 23);
            _tbComment.TabIndex = 5;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(3, 135);
            label5.Name = "label5";
            label5.Size = new Size(61, 15);
            label5.TabIndex = 19;
            label5.Text = "Comment";
            // 
            // UCTimeEntries
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCTimeEntries";
            _pnlElements.ResumeLayout(false);
            _pnlSideBar.ResumeLayout(false);
            _pnlSideBar.PerformLayout();
            _pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_dgvTimeEntries).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Elements.ProjectCodeComboBox _cbProjCodes;
        private Button _btnAdd;
        private Elements.TimeEntriesDataGridView _dgvTimeEntries;
        private Lib.DatePicker _dtpDate;
        private Lib.TimePicker _tpStart;
        private Lib.TimePicker _tpEnd;
        private Label label2;
        private Label label1;
        private Label label3;
        private Label label4;
        private Label label5;
        private TextBox _tbComment;
    }
}
