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
            _pnlElements = new Panel();
            _btnBack = new Button();
            _pnlTopBar.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlTopBar
            // 
            _pnlTopBar.Controls.Add(_btnBack);
            _pnlTopBar.Dock = DockStyle.Top;
            _pnlTopBar.Location = new Point(0, 0);
            _pnlTopBar.Name = "_pnlTopBar";
            _pnlTopBar.Size = new Size(627, 33);
            _pnlTopBar.TabIndex = 0;
            // 
            // _pnlElements
            // 
            _pnlElements.Dock = DockStyle.Fill;
            _pnlElements.Location = new Point(0, 33);
            _pnlElements.Name = "_pnlElements";
            _pnlElements.Size = new Size(627, 439);
            _pnlElements.TabIndex = 1;
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
            // UCSubPage
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_pnlElements);
            Controls.Add(_pnlTopBar);
            Name = "UCSubPage";
            Size = new Size(627, 472);
            _pnlTopBar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel _pnlTopBar;
        private Button _btnBack;
        protected Panel _pnlElements;
    }
}
