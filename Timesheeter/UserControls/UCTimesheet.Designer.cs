namespace Timesheeter.UserControls
{
    partial class UCTimesheet
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
            _datePicker = new Timesheeter.Lib.DatePicker();
            _dgvTimesheet = new Timesheeter.Elements.TimesheetDataGridView();
            label1 = new Label();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvTimesheet).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Size = new Size(871, 539);
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(_datePicker);
            _pnlSideBar.Size = new Size(298, 539);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(_dgvTimesheet);
            _pnlGrid.Size = new Size(573, 539);
            // 
            // _datePicker
            // 
            _datePicker.CalendarMonthBackground = Color.White;
            _datePicker.Format = DateTimePickerFormat.Short;
            _datePicker.Location = new Point(3, 21);
            _datePicker.Name = "_datePicker";
            _datePicker.Size = new Size(115, 23);
            _datePicker.TabIndex = 1;
            _datePicker.ValueChanged += _datePickerChanged;
            // 
            // _dgvTimesheet
            // 
            _dgvTimesheet.AllowUserToAddRows = false;
            _dgvTimesheet.AllowUserToDeleteRows = false;
            _dgvTimesheet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvTimesheet.Dock = DockStyle.Fill;
            _dgvTimesheet.Location = new Point(0, 0);
            _dgvTimesheet.Name = "_dgvTimesheet";
            _dgvTimesheet.ReadOnly = true;
            _dgvTimesheet.RowHeadersVisible = false;
            _dgvTimesheet.Size = new Size(573, 539);
            _dgvTimesheet.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(63, 15);
            label1.TabIndex = 3;
            label1.Text = "Week Start";
            // 
            // UCTimesheet
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCTimesheet";
            Size = new Size(871, 539);
            _pnlElements.ResumeLayout(false);
            _pnlSideBar.ResumeLayout(false);
            _pnlSideBar.PerformLayout();
            _pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_dgvTimesheet).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Lib.DatePicker _datePicker;
        private Elements.TimesheetDataGridView _dgvTimesheet;
        private Label label1;
    }
}
