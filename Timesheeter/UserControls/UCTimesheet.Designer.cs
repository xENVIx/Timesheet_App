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
            _chartTimesheet = new Timesheeter.Elements.TimesheetChart();
            panel1 = new Panel();
            _rbDate = new RadioButton();
            _rbHours = new RadioButton();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvTimesheet).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Size = new Size(871, 539);
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(_chartTimesheet);
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(_datePicker);
            _pnlSideBar.Size = new Size(298, 539);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(_dgvTimesheet);
            _pnlGrid.Controls.Add(panel1);
            _pnlGrid.Size = new Size(573, 539);
            // 
            // _datePicker
            // 
            _datePicker.CalendarForeColor = Color.FromArgb(24, 24, 27);
            _datePicker.CalendarMonthBackground = Color.White;
            _datePicker.CalendarTitleBackColor = Color.FromArgb(37, 99, 235);
            _datePicker.CalendarTitleForeColor = Color.White;
            _datePicker.CalendarTrailingForeColor = Color.FromArgb(113, 113, 122);
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
            _dgvTimesheet.Size = new Size(573, 510);
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
            // _chartTimesheet
            // 
            _chartTimesheet.Dock = DockStyle.Bottom;
            _chartTimesheet.Location = new Point(0, 284);
            _chartTimesheet.Name = "_chartTimesheet";
            _chartTimesheet.Size = new Size(298, 255);
            _chartTimesheet.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(_rbDate);
            panel1.Controls.Add(_rbHours);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 510);
            panel1.Name = "panel1";
            panel1.Size = new Size(573, 29);
            panel1.TabIndex = 3;
            // 
            // _rbDate
            // 
            _rbDate.AutoSize = true;
            _rbDate.Location = new Point(73, 5);
            _rbDate.Name = "_rbDate";
            _rbDate.Size = new Size(52, 19);
            _rbDate.TabIndex = 1;
            _rbDate.Text = "Time";
            _rbDate.UseVisualStyleBackColor = true;
            // 
            // _rbHours
            // 
            _rbHours.AutoSize = true;
            _rbHours.Checked = true;
            _rbHours.Location = new Point(10, 5);
            _rbHours.Name = "_rbHours";
            _rbHours.Size = new Size(57, 19);
            _rbHours.TabIndex = 0;
            _rbHours.TabStop = true;
            _rbHours.Text = "Hours";
            _rbHours.UseVisualStyleBackColor = true;
            _rbHours.CheckedChanged += _rbHours_CheckedChanged;
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
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Lib.DatePicker _datePicker;
        private Elements.TimesheetDataGridView _dgvTimesheet;
        private Label label1;
        private Elements.TimesheetChart _chartTimesheet;
        private Panel panel1;
        private RadioButton _rbHours;
        private RadioButton _rbDate;
    }
}
