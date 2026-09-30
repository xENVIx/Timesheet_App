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
            _btnAdd = new Button();
            _cbCustomer = new Timesheeter.Elements.CustomerComboBox();
            _projCodesDgv = new Timesheeter.Elements.ProjectCodesDataGridView();
            _tbDescription = new TextBox();
            _tbLocation = new TextBox();
            _tbProjectCode = new TextBox();
            _lblCustomer = new Label();
            _lblLocation = new Label();
            _lblProjCode = new Label();
            _lblDescription = new Label();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _pnlGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_projCodesDgv).BeginInit();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Size = new Size(931, 506);
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(_lblDescription);
            _pnlSideBar.Controls.Add(_lblProjCode);
            _pnlSideBar.Controls.Add(_lblLocation);
            _pnlSideBar.Controls.Add(_lblCustomer);
            _pnlSideBar.Controls.Add(_tbProjectCode);
            _pnlSideBar.Controls.Add(_tbLocation);
            _pnlSideBar.Controls.Add(_tbDescription);
            _pnlSideBar.Controls.Add(_cbCustomer);
            _pnlSideBar.Controls.Add(_btnAdd);
            _pnlSideBar.Size = new Size(298, 506);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Controls.Add(_projCodesDgv);
            _pnlGrid.Size = new Size(633, 506);
            // 
            // _btnAdd
            // 
            _btnAdd.Location = new Point(3, 207);
            _btnAdd.Name = "_btnAdd";
            _btnAdd.Size = new Size(121, 23);
            _btnAdd.TabIndex = 12;
            _btnAdd.Text = "Add Code";
            _btnAdd.UseVisualStyleBackColor = true;
            // 
            // _cbCustomer
            // 
            _cbCustomer.DisplayMember = "Name";
            _cbCustomer.FormattingEnabled = true;
            _cbCustomer.Location = new Point(3, 80);
            _cbCustomer.Name = "_cbCustomer";
            _cbCustomer.Size = new Size(244, 23);
            _cbCustomer.TabIndex = 13;
            _cbCustomer.ValueMember = "ID";
            // 
            // _projCodesDgv
            // 
            _projCodesDgv.AllowUserToAddRows = false;
            _projCodesDgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _projCodesDgv.Dock = DockStyle.Fill;
            _projCodesDgv.Location = new Point(0, 0);
            _projCodesDgv.Name = "_projCodesDgv";
            _projCodesDgv.Size = new Size(633, 506);
            _projCodesDgv.TabIndex = 13;
            // 
            // _tbDescription
            // 
            _tbDescription.Location = new Point(3, 124);
            _tbDescription.Name = "_tbDescription";
            _tbDescription.Size = new Size(244, 23);
            _tbDescription.TabIndex = 14;
            // 
            // _tbLocation
            // 
            _tbLocation.Location = new Point(3, 168);
            _tbLocation.Name = "_tbLocation";
            _tbLocation.Size = new Size(244, 23);
            _tbLocation.TabIndex = 15;
            // 
            // _tbProjectCode
            // 
            _tbProjectCode.Location = new Point(3, 36);
            _tbProjectCode.Name = "_tbProjectCode";
            _tbProjectCode.Size = new Size(244, 23);
            _tbProjectCode.TabIndex = 16;
            // 
            // _lblCustomer
            // 
            _lblCustomer.AutoSize = true;
            _lblCustomer.Location = new Point(3, 62);
            _lblCustomer.Name = "_lblCustomer";
            _lblCustomer.Size = new Size(59, 15);
            _lblCustomer.TabIndex = 17;
            _lblCustomer.Text = "Customer";
            // 
            // _lblLocation
            // 
            _lblLocation.AutoSize = true;
            _lblLocation.Location = new Point(3, 150);
            _lblLocation.Name = "_lblLocation";
            _lblLocation.Size = new Size(53, 15);
            _lblLocation.TabIndex = 18;
            _lblLocation.Text = "Location";
            // 
            // _lblProjCode
            // 
            _lblProjCode.AutoSize = true;
            _lblProjCode.Location = new Point(3, 18);
            _lblProjCode.Name = "_lblProjCode";
            _lblProjCode.Size = new Size(75, 15);
            _lblProjCode.TabIndex = 19;
            _lblProjCode.Text = "Project Code";
            // 
            // _lblDescription
            // 
            _lblDescription.AutoSize = true;
            _lblDescription.Location = new Point(3, 106);
            _lblDescription.Name = "_lblDescription";
            _lblDescription.Size = new Size(67, 15);
            _lblDescription.TabIndex = 20;
            _lblDescription.Text = "Description";
            // 
            // UCNewProjectCode
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCNewProjectCode";
            Size = new Size(931, 539);
            _pnlElements.ResumeLayout(false);
            _pnlSideBar.ResumeLayout(false);
            _pnlSideBar.PerformLayout();
            _pnlGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_projCodesDgv).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Elements.CustomerComboBox _cbCustomer;
        private Button _btnAdd;
        private Elements.ProjectCodesDataGridView _projCodesDgv;
        private TextBox _tbProjectCode;
        private TextBox _tbLocation;
        private TextBox _tbDescription;
        private Label _lblCustomer;
        private Label _lblLocation;
        private Label _lblProjCode;
        private Label _lblDescription;
    }
}
