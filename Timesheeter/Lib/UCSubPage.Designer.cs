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
            _pnlElements = new Panel();
            splitter1 = new Splitter();
            _pnlGrid = new Panel();
            _pnlSideBar = new Panel();
            _pnlElements.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Controls.Add(splitter1);
            _pnlElements.Controls.Add(_pnlGrid);
            _pnlElements.Controls.Add(_pnlSideBar);
            _pnlElements.Dock = DockStyle.Fill;
            _pnlElements.Location = new Point(0, 0);
            _pnlElements.Name = "_pnlElements";
            _pnlElements.Size = new Size(970, 649);
            _pnlElements.TabIndex = 1;
            // 
            // splitter1
            // 
            splitter1.Location = new Point(298, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(3, 649);
            splitter1.TabIndex = 0;
            splitter1.TabStop = false;
            splitter1.SplitterMoving += splitter1_SplitterMoving;
            splitter1.SplitterMoved += splitter1_SplitterMoved;
            // 
            // _pnlGrid
            // 
            _pnlGrid.Dock = DockStyle.Fill;
            _pnlGrid.Location = new Point(298, 0);
            _pnlGrid.Name = "_pnlGrid";
            _pnlGrid.Size = new Size(672, 649);
            _pnlGrid.TabIndex = 1;
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Dock = DockStyle.Left;
            _pnlSideBar.Location = new Point(0, 0);
            _pnlSideBar.Name = "_pnlSideBar";
            _pnlSideBar.Size = new Size(298, 649);
            _pnlSideBar.TabIndex = 0;
            // 
            // UCSubPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_pnlElements);
            Name = "UCSubPage";
            Size = new Size(970, 649);
            _pnlElements.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        protected Panel _pnlElements;
        protected Panel _pnlSideBar;
        protected Panel _pnlGrid;
        private Splitter splitter1;
    }
}
