namespace Timesheeter
{
    partial class UserInterface
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _ucMain = new Timesheeter.UserControls.UCMain();
            SuspendLayout();
            // 
            // _ucMain
            // 
            _ucMain.Dock = DockStyle.Fill;
            _ucMain.Location = new Point(0, 0);
            _ucMain.Name = "_ucMain";
            _ucMain.Size = new Size(1511, 782);
            _ucMain.TabIndex = 0;
            // 
            // UserInterface
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1511, 782);
            Controls.Add(_ucMain);
            Name = "UserInterface";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Timesheeter";
            ResumeLayout(false);
        }

        #endregion

        private UserControls.UCMain _ucMain;
    }
}
