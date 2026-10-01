namespace Timesheeter.UserControls
{
    partial class UCSettings
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
            components = new System.ComponentModel.Container();
            _lblTheme = new Label();
            _rbLight = new RadioButton();
            _rbLightDarkNav = new RadioButton();
            _rbDark = new RadioButton();
            _lblAccent = new Label();
            _flpAccents = new FlowLayoutPanel();
            _btnCustomAccent = new Button();
            _toolTip = new ToolTip(components);
            _pnlSideBar.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(_btnCustomAccent);
            _pnlSideBar.Controls.Add(_flpAccents);
            _pnlSideBar.Controls.Add(_lblAccent);
            _pnlSideBar.Controls.Add(_rbDark);
            _pnlSideBar.Controls.Add(_rbLightDarkNav);
            _pnlSideBar.Controls.Add(_rbLight);
            _pnlSideBar.Controls.Add(_lblTheme);
            // 
            // _lblTheme
            // 
            _lblTheme.AutoSize = true;
            _lblTheme.Location = new Point(16, 16);
            _lblTheme.Name = "_lblTheme";
            _lblTheme.Size = new Size(44, 15);
            _lblTheme.TabIndex = 0;
            _lblTheme.Text = "Theme";
            // 
            // _rbLight
            // 
            _rbLight.AutoSize = true;
            _rbLight.Location = new Point(16, 40);
            _rbLight.Name = "_rbLight";
            _rbLight.Size = new Size(52, 19);
            _rbLight.TabIndex = 1;
            _rbLight.Text = "Light";
            _rbLight.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _rbLightDarkNav
            // 
            _rbLightDarkNav.AutoSize = true;
            _rbLightDarkNav.Location = new Point(16, 65);
            _rbLightDarkNav.Name = "_rbLightDarkNav";
            _rbLightDarkNav.Size = new Size(174, 19);
            _rbLightDarkNav.TabIndex = 2;
            _rbLightDarkNav.Text = "Light with dark navigation";
            _rbLightDarkNav.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _rbDark
            // 
            _rbDark.AutoSize = true;
            _rbDark.Location = new Point(16, 90);
            _rbDark.Name = "_rbDark";
            _rbDark.Size = new Size(49, 19);
            _rbDark.TabIndex = 3;
            _rbDark.Text = "Dark";
            _rbDark.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _lblAccent
            // 
            _lblAccent.AutoSize = true;
            _lblAccent.Location = new Point(16, 128);
            _lblAccent.Name = "_lblAccent";
            _lblAccent.Size = new Size(79, 15);
            _lblAccent.TabIndex = 4;
            _lblAccent.Text = "Accent colour";
            // 
            // _flpAccents
            // 
            _flpAccents.Location = new Point(16, 150);
            _flpAccents.Name = "_flpAccents";
            _flpAccents.Size = new Size(266, 84);
            _flpAccents.TabIndex = 5;
            // 
            // _btnCustomAccent
            // 
            _btnCustomAccent.Location = new Point(16, 242);
            _btnCustomAccent.Name = "_btnCustomAccent";
            _btnCustomAccent.Size = new Size(130, 28);
            _btnCustomAccent.TabIndex = 6;
            _btnCustomAccent.Text = "Custom colour...";
            _btnCustomAccent.UseVisualStyleBackColor = true;
            _btnCustomAccent.Click += _btnCustomAccent_Click;
            // 
            // UCSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCSettings";
            _pnlSideBar.ResumeLayout(false);
            _pnlSideBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label _lblTheme;
        private RadioButton _rbLight;
        private RadioButton _rbLightDarkNav;
        private RadioButton _rbDark;
        private Label _lblAccent;
        private FlowLayoutPanel _flpAccents;
        private Button _btnCustomAccent;
        private ToolTip _toolTip;
    }
}
