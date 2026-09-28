namespace Timesheeter.UserControls
{
    partial class UCSelectionMenu
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
            _btnCodes = new Button();
            _ucNewProjectCode = new UCNewProjectCode();
            SuspendLayout();
            // 
            // _btnCodes
            // 
            _btnCodes.Location = new Point(30, 29);
            _btnCodes.Name = "_btnCodes";
            _btnCodes.Size = new Size(93, 43);
            _btnCodes.TabIndex = 0;
            _btnCodes.Text = "Project Code Managment";
            _btnCodes.UseVisualStyleBackColor = true;
            _btnCodes.Click += _btnCodes_Click;
            // 
            // _ucNewProjectCode
            // 
            _ucNewProjectCode.Location = new Point(483, 431);
            _ucNewProjectCode.Name = "_ucNewProjectCode";
            _ucNewProjectCode.Size = new Size(577, 413);
            _ucNewProjectCode.TabIndex = 1;
            _ucNewProjectCode.Visible = false;
            // 
            // UCSelectionMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_ucNewProjectCode);
            Controls.Add(_btnCodes);
            Name = "UCSelectionMenu";
            Size = new Size(608, 477);
            ResumeLayout(false);
        }

        #endregion

        private Button _btnCodes;
        private UCNewProjectCode _ucNewProjectCode;
    }
}
