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
            textBox1 = new TextBox();
            customersDataGridView1 = new Timesheeter.Elements.CustomersDataGridView();
            _pnlElements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)customersDataGridView1).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(customersDataGridView1);
            _pnlElements.Controls.Add(textBox1);
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
            textBox1.Location = new Point(28, 35);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(149, 23);
            textBox1.TabIndex = 5;
            // 
            // customersDataGridView1
            // 
            customersDataGridView1.AllowUserToAddRows = false;
            customersDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            customersDataGridView1.Location = new Point(183, 35);
            customersDataGridView1.Name = "customersDataGridView1";
            customersDataGridView1.Size = new Size(567, 294);
            customersDataGridView1.TabIndex = 6;
            // 
            // UCCustomers
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCCustomers";
            Size = new Size(779, 403);
            _pnlElements.ResumeLayout(false);
            _pnlElements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)customersDataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TextBox textBox1;
        private Label label1;
        private Button _btnSave;
        private Elements.CustomersDataGridView customersDataGridView1;
    }
}
