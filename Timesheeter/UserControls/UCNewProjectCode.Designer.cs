namespace Timesheeter.UserControls
{
    partial class UCNewProjectCode
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
            _cbCustomer = new Timesheeter.Elements.CustomerComboBox();
            label1 = new Label();
            _tbLocation = new TextBox();
            label2 = new Label();
            _tbProjectCode = new TextBox();
            label3 = new Label();
            _btnAdd = new Button();
            _projCodesDgv = new Timesheeter.Elements.ProjectCodesDataGridView();
            label4 = new Label();
            _tbDescription = new TextBox();
            _pnlElements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_projCodesDgv).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(label4);
            _pnlElements.Controls.Add(_tbDescription);
            _pnlElements.Controls.Add(_projCodesDgv);
            _pnlElements.Controls.Add(_btnAdd);
            _pnlElements.Controls.Add(label3);
            _pnlElements.Controls.Add(_tbProjectCode);
            _pnlElements.Controls.Add(label2);
            _pnlElements.Controls.Add(_tbLocation);
            _pnlElements.Controls.Add(label1);
            _pnlElements.Controls.Add(_cbCustomer);
            _pnlElements.Size = new Size(931, 506);
            // 
            // _cbCustomer
            // 
            _cbCustomer.DisplayMember = "Name";
            _cbCustomer.FormattingEnabled = true;
            _cbCustomer.Location = new Point(25, 72);
            _cbCustomer.Name = "_cbCustomer";
            _cbCustomer.Size = new Size(244, 23);
            _cbCustomer.TabIndex = 5;
            _cbCustomer.ValueMember = "ID";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(25, 54);
            label1.Name = "label1";
            label1.Size = new Size(59, 15);
            label1.TabIndex = 6;
            label1.Text = "Customer";
            // 
            // _tbLocation
            // 
            _tbLocation.Location = new Point(25, 160);
            _tbLocation.Name = "_tbLocation";
            _tbLocation.Size = new Size(244, 23);
            _tbLocation.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(25, 142);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 8;
            label2.Text = "Location";
            // 
            // _tbProjectCode
            // 
            _tbProjectCode.Location = new Point(25, 29);
            _tbProjectCode.Name = "_tbProjectCode";
            _tbProjectCode.Size = new Size(244, 23);
            _tbProjectCode.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(25, 11);
            label3.Name = "label3";
            label3.Size = new Size(75, 15);
            label3.TabIndex = 10;
            label3.Text = "Project Code";
            // 
            // _btnAdd
            // 
            _btnAdd.Location = new Point(25, 189);
            _btnAdd.Name = "_btnAdd";
            _btnAdd.Size = new Size(121, 23);
            _btnAdd.TabIndex = 11;
            _btnAdd.Text = "Add Code";
            _btnAdd.UseVisualStyleBackColor = true;
            _btnAdd.Click += _btnAdd_Click;
            // 
            // _projCodesDgv
            // 
            _projCodesDgv.AllowUserToAddRows = false;
            _projCodesDgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _projCodesDgv.Location = new Point(275, 29);
            _projCodesDgv.Name = "_projCodesDgv";
            _projCodesDgv.Size = new Size(653, 474);
            _projCodesDgv.TabIndex = 12;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(25, 98);
            label4.Name = "label4";
            label4.Size = new Size(67, 15);
            label4.TabIndex = 14;
            label4.Text = "Description";
            // 
            // _tbDescription
            // 
            _tbDescription.Location = new Point(25, 116);
            _tbDescription.Name = "_tbDescription";
            _tbDescription.Size = new Size(244, 23);
            _tbDescription.TabIndex = 13;
            // 
            // UCNewProjectCode
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCNewProjectCode";
            Size = new Size(931, 539);
            _pnlElements.ResumeLayout(false);
            _pnlElements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_projCodesDgv).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label label3;
        private TextBox _tbProjectCode;
        private Label label2;
        private TextBox _tbLocation;
        private Label label1;
        private Elements.CustomerComboBox _cbCustomer;
        private Button _btnAdd;
        private Elements.ProjectCodesDataGridView _projCodesDgv;
        private Label label4;
        private TextBox _tbDescription;
    }
}
