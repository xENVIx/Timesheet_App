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
            _custDgv = new Timesheeter.Elements.CustomersDataGridView();
            _tbCustName = new TextBox();
            label1 = new Label();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_custDgv).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Size = new Size(779, 370);
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(_tbCustName);
            _pnlSideBar.Controls.Add(_btnSave);
            _pnlSideBar.Size = new Size(298, 370);
            _pnlSideBar.TabIndex = 99;
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(_custDgv);
            _pnlGrid.Size = new Size(481, 370);
            // 
            // _btnSave
            // 
            _btnSave.Location = new Point(3, 50);
            _btnSave.Name = "_btnSave";
            _btnSave.Size = new Size(75, 23);
            _btnSave.TabIndex = 1;
            _btnSave.Text = "Add";
            _btnSave.UseVisualStyleBackColor = true;
            _btnSave.Click += _btnSave_Click;
            // 
            // _custDgv
            // 
            _custDgv.AllowUserToAddRows = false;
            _custDgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _custDgv.Dock = DockStyle.Fill;
            _custDgv.Location = new Point(0, 0);
            _custDgv.Name = "_custDgv";
            _custDgv.Size = new Size(481, 370);
            _custDgv.TabIndex = 3;
            // 
            // _tbCustName
            // 
            _tbCustName.Location = new Point(3, 21);
            _tbCustName.Name = "_tbCustName";
            _tbCustName.Size = new Size(149, 23);
            _tbCustName.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(94, 15);
            label1.TabIndex = 7;
            label1.Text = "Customer Name";
            // 
            // UCCustomers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCCustomers";
            Size = new Size(779, 403);
            _pnlElements.ResumeLayout(false);
            _pnlSideBar.ResumeLayout(false);
            _pnlSideBar.PerformLayout();
            _pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_custDgv).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button _btnSave;
        private Elements.CustomersDataGridView _custDgv;
        private Label label1;
        private TextBox _tbCustName;
    }
}
