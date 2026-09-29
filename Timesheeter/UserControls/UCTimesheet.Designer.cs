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
            label1 = new Label();
            _dgvTimesheet = new Timesheeter.Elements.TimesheetDataGridView();
            _pnlElements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvTimesheet).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(_dgvTimesheet);
            _pnlElements.Controls.Add(label1);
            _pnlElements.Controls.Add(_datePicker);
            _pnlElements.Size = new Size(871, 506);
            // 
            // _datePicker
            // 
            _datePicker.Format = DateTimePickerFormat.Short;
            _datePicker.Location = new Point(28, 39);
            _datePicker.Name = "_datePicker";
            _datePicker.Size = new Size(115, 23);
            _datePicker.TabIndex = 1;
            _datePicker.ValueChanged += _datePickerChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 21);
            label1.Name = "label1";
            label1.Size = new Size(63, 15);
            label1.TabIndex = 2;
            label1.Text = "Week Start";
            // 
            // _dgvTimesheet
            // 
            _dgvTimesheet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvTimesheet.Location = new Point(28, 68);
            _dgvTimesheet.Name = "_dgvTimesheet";
            _dgvTimesheet.Size = new Size(805, 356);
            _dgvTimesheet.TabIndex = 3;
            // 
            // UCTimesheet
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCTimesheet";
            Size = new Size(871, 539);
            _pnlElements.ResumeLayout(false);
            _pnlElements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvTimesheet).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Lib.DatePicker _datePicker;
        private Elements.TimesheetDataGridView _dgvTimesheet;
    }
}
