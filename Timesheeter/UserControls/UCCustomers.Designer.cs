namespace Timesheeter.UserControls
{
    partial class UCCustomers
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
            _btnSave = new Button();
            label1 = new Label();
            _tbCustName = new TextBox();
            _custDgv = new Timesheeter.Elements.CustomersDataGridView();
            _pnlElements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_custDgv).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(_custDgv);
            _pnlElements.Controls.Add(_tbCustName);
            _pnlElements.Controls.Add(label1);
            _pnlElements.Controls.Add(_btnSave);
            _pnlElements.Size = new Size(779, 370);
            // 
            // _btnSave
            // 
            _btnSave.Location = new Point(28, 64);
            _btnSave.Name = "_btnSave";
            _btnSave.Size = new Size(75, 23);
            _btnSave.TabIndex = 3;
            _btnSave.Text = "Add";
            _btnSave.UseVisualStyleBackColor = true;
            _btnSave.Click += _btnSave_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 17);
            label1.Name = "label1";
            label1.Size = new Size(94, 15);
            label1.TabIndex = 4;
            label1.Text = "Customer Name";
            // 
            // textBox1
            // 
            _tbCustName.Location = new Point(28, 35);
            _tbCustName.Name = "textBox1";
            _tbCustName.Size = new Size(149, 23);
            _tbCustName.TabIndex = 5;
            // 
            // _custDgv
            // 
            _custDgv.AllowUserToAddRows = false;
            _custDgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _custDgv.Location = new Point(183, 35);
            _custDgv.Name = "_custDgv";
            _custDgv.Size = new Size(567, 294);
            _custDgv.TabIndex = 6;
            // 
            // UCCustomers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCCustomers";
            Size = new Size(779, 403);
            _pnlElements.ResumeLayout(false);
            _pnlElements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_custDgv).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TextBox _tbCustName;
        private Label label1;
        private Button _btnSave;
        private Elements.CustomersDataGridView _custDgv;
    }
}
