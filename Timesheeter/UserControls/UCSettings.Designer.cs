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
            _tbFirstName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            _tbLastName = new TextBox();
            _btnSave = new Button();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(_btnSave);
            _pnlSideBar.Controls.Add(label2);
            _pnlSideBar.Controls.Add(_tbLastName);
            _pnlSideBar.Controls.Add(label1);
            _pnlSideBar.Controls.Add(_tbFirstName);
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
            _lblTheme.Location = new Point(15, 210);
            _lblTheme.Name = "_lblTheme";
            _lblTheme.Size = new Size(44, 15);
            _lblTheme.TabIndex = 0;
            _lblTheme.Text = "Theme";
            // 
            // _rbLight
            // 
            _rbLight.AutoSize = true;
            _rbLight.Location = new Point(15, 234);
            _rbLight.Name = "_rbLight";
            _rbLight.Size = new Size(52, 19);
            _rbLight.TabIndex = 1;
            _rbLight.Text = "Light";
            _rbLight.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _rbLightDarkNav
            // 
            _rbLightDarkNav.AutoSize = true;
            _rbLightDarkNav.Location = new Point(15, 259);
            _rbLightDarkNav.Name = "_rbLightDarkNav";
            _rbLightDarkNav.Size = new Size(163, 19);
            _rbLightDarkNav.TabIndex = 2;
            _rbLightDarkNav.Text = "Light with dark navigation";
            _rbLightDarkNav.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _rbDark
            // 
            _rbDark.AutoSize = true;
            _rbDark.Location = new Point(15, 284);
            _rbDark.Name = "_rbDark";
            _rbDark.Size = new Size(49, 19);
            _rbDark.TabIndex = 3;
            _rbDark.Text = "Dark";
            _rbDark.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _lblAccent
            // 
            _lblAccent.AutoSize = true;
            _lblAccent.Location = new Point(15, 322);
            _lblAccent.Name = "_lblAccent";
            _lblAccent.Size = new Size(81, 15);
            _lblAccent.TabIndex = 4;
            _lblAccent.Text = "Accent colour";
            // 
            // _flpAccents
            // 
            _flpAccents.Location = new Point(15, 344);
            _flpAccents.Name = "_flpAccents";
            _flpAccents.Size = new Size(266, 84);
            _flpAccents.TabIndex = 5;
            // 
            // _btnCustomAccent
            // 
            _btnCustomAccent.Location = new Point(15, 436);
            _btnCustomAccent.Name = "_btnCustomAccent";
            _btnCustomAccent.Size = new Size(130, 28);
            _btnCustomAccent.TabIndex = 6;
            _btnCustomAccent.Text = "Custom colour...";
            _btnCustomAccent.UseVisualStyleBackColor = true;
            _btnCustomAccent.Click += _btnCustomAccent_Click;
            // 
            // _tbFirstName
            // 
            _tbFirstName.Location = new Point(15, 32);
            _tbFirstName.Name = "_tbFirstName";
            _tbFirstName.Size = new Size(266, 23);
            _tbFirstName.TabIndex = 7;
            _tbFirstName.TextChanged += _tbFirstName_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(15, 14);
            label1.Name = "label1";
            label1.Size = new Size(64, 15);
            label1.TabIndex = 8;
            label1.Text = "First Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 58);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 10;
            label2.Text = "Last Name";
            // 
            // _tbLastName
            // 
            _tbLastName.Location = new Point(15, 76);
            _tbLastName.Name = "_tbLastName";
            _tbLastName.Size = new Size(266, 23);
            _tbLastName.TabIndex = 9;
            _tbLastName.TextChanged += _tbLastName_TextChanged;
            // 
            // _btnSave
            // 
            _btnSave.Location = new Point(15, 105);
            _btnSave.Name = "_btnSave";
            _btnSave.Size = new Size(130, 28);
            _btnSave.TabIndex = 11;
            _btnSave.Text = "Save";
            _btnSave.UseVisualStyleBackColor = true;
            _btnSave.Click += _btnSave_Click;
            // 
            // UCSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCSettings";
            _pnlElements.ResumeLayout(false);
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
        private Label label1;
        private TextBox _tbFirstName;
        private Label label2;
        private TextBox _tbLastName;
        private Button _btnSave;
    }
}
