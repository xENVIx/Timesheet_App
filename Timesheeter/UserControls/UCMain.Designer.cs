namespace Timesheeter.UserControls
{
    partial class UCMain
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
            _ucSelectionMenu = new UCSelectionMenu();
            SuspendLayout();
            // 
            // _ucSelectionMenu
            // 
            _ucSelectionMenu.Dock = DockStyle.Fill;
            _ucSelectionMenu.Location = new Point(0, 0);
            _ucSelectionMenu.Name = "_ucSelectionMenu";
            _ucSelectionMenu.Size = new Size(630, 418);
            _ucSelectionMenu.TabIndex = 0;
            // 
            // UCMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_ucSelectionMenu);
            Name = "UCMain";
            Size = new Size(630, 418);
            ResumeLayout(false);
        }

        #endregion

        private UCSelectionMenu _ucSelectionMenu;
    }
}
