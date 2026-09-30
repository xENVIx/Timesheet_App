namespace Timesheeter.Lib
{
    partial class UCSubPage
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
            _pnlTopBar = new Panel();
            _btnBack = new Button();
            _pnlElements = new Panel();
            _pnlSideBar = new Panel();
            _pnlGrid = new Panel();
            _pnlTopBar.SuspendLayout();
            _pnlElements.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlTopBar
            // 
            _pnlTopBar.Controls.Add(_btnBack);
            _pnlTopBar.Dock = DockStyle.Top;
            _pnlTopBar.Location = new Point(0, 0);
            _pnlTopBar.Name = "_pnlTopBar";
            _pnlTopBar.Size = new Size(970, 33);
            _pnlTopBar.TabIndex = 0;
            // 
            // _btnBack
            // 
            _btnBack.Location = new Point(3, 7);
            _btnBack.Name = "_btnBack";
            _btnBack.Size = new Size(75, 23);
            _btnBack.TabIndex = 0;
            _btnBack.Text = "Back";
            _btnBack.UseVisualStyleBackColor = true;
            _btnBack.Click += _btnBack_Click;
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(_pnlGrid);
            _pnlElements.Controls.Add(_pnlSideBar);
            _pnlElements.Dock = DockStyle.Fill;
            _pnlElements.Location = new Point(0, 33);
            _pnlElements.Name = "_pnlElements";
            _pnlElements.Size = new Size(970, 616);
            _pnlElements.TabIndex = 1;
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Dock = DockStyle.Left;
            _pnlSideBar.Location = new Point(0, 0);
            _pnlSideBar.Name = "_pnlSideBar";
            _pnlSideBar.Size = new Size(298, 616);
            _pnlSideBar.TabIndex = 0;
            // 
            // _pnlGrid
            // 
            _pnlGrid.Dock = DockStyle.Fill;
            _pnlGrid.Location = new Point(298, 0);
            _pnlGrid.Name = "_pnlGrid";
            _pnlGrid.Size = new Size(672, 616);
            _pnlGrid.TabIndex = 1;
            // 
            // UCSubPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_pnlElements);
            Controls.Add(_pnlTopBar);
            Name = "UCSubPage";
            Size = new Size(970, 649);
            _pnlTopBar.ResumeLayout(false);
            _pnlElements.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel _pnlTopBar;
        private Button _btnBack;
        protected Panel _pnlElements;
        protected Panel _pnlSideBar;
        protected Panel _pnlGrid;
    }
}
