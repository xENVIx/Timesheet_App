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
            _accordionPanelName = new Timesheeter.Elements.AccordionPanel();
            _accordionPanelTheme = new Timesheeter.Elements.AccordionPanel();
            _accordionPanelSettings = new Timesheeter.Elements.AccordionPanel();
            _btnTest = new Button();
            _btnSaveServer = new Button();
            _tbSecret = new TextBox();
            label6 = new Label();
            _tbApi = new TextBox();
            label5 = new Label();
            _tbPort = new TextBox();
            label4 = new Label();
            _tbServer = new TextBox();
            label3 = new Label();
            _chkbUseServer = new CheckBox();
            _pnlElements.SuspendLayout();
            _pnlSideBar.SuspendLayout();
            _accordionPanelName.SuspendLayout();
            _accordionPanelTheme.SuspendLayout();
            _accordionPanelSettings.SuspendLayout();
            SuspendLayout();
            // 
            // _pnlElements
            // 
            _pnlElements.Size = new Size(970, 865);
            // 
            // _pnlSideBar
            // 
            _pnlSideBar.Controls.Add(_accordionPanelSettings);
            _pnlSideBar.Controls.Add(_accordionPanelTheme);
            _pnlSideBar.Controls.Add(_accordionPanelName);
            _pnlSideBar.Size = new Size(298, 865);
            // 
            // _pnlGrid
            // 
            _pnlGrid.Size = new Size(672, 865);
            // 
            // _lblTheme
            // 
            _lblTheme.AutoSize = true;
            _lblTheme.Location = new Point(4, 32);
            _lblTheme.Name = "_lblTheme";
            _lblTheme.Size = new Size(44, 15);
            _lblTheme.TabIndex = 0;
            _lblTheme.Text = "Theme";
            // 
            // _rbLight
            // 
            _rbLight.AutoSize = true;
            _rbLight.Location = new Point(4, 56);
            _rbLight.Name = "_rbLight";
            _rbLight.Size = new Size(52, 19);
            _rbLight.TabIndex = 1;
            _rbLight.Text = "Light";
            _rbLight.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _rbLightDarkNav
            // 
            _rbLightDarkNav.AutoSize = true;
            _rbLightDarkNav.Location = new Point(4, 81);
            _rbLightDarkNav.Name = "_rbLightDarkNav";
            _rbLightDarkNav.Size = new Size(163, 19);
            _rbLightDarkNav.TabIndex = 2;
            _rbLightDarkNav.Text = "Light with dark navigation";
            _rbLightDarkNav.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _rbDark
            // 
            _rbDark.AutoSize = true;
            _rbDark.Location = new Point(4, 106);
            _rbDark.Name = "_rbDark";
            _rbDark.Size = new Size(49, 19);
            _rbDark.TabIndex = 3;
            _rbDark.Text = "Dark";
            _rbDark.CheckedChanged += _themeMode_CheckedChanged;
            // 
            // _lblAccent
            // 
            _lblAccent.AutoSize = true;
            _lblAccent.Location = new Point(4, 144);
            _lblAccent.Name = "_lblAccent";
            _lblAccent.Size = new Size(74, 15);
            _lblAccent.TabIndex = 4;
            _lblAccent.Text = "Accent color";
            // 
            // _flpAccents
            // 
            _flpAccents.Location = new Point(4, 166);
            _flpAccents.Name = "_flpAccents";
            _flpAccents.Size = new Size(266, 84);
            _flpAccents.TabIndex = 5;
            // 
            // _btnCustomAccent
            // 
            _btnCustomAccent.Location = new Point(4, 256);
            _btnCustomAccent.Name = "_btnCustomAccent";
            _btnCustomAccent.Size = new Size(130, 28);
            _btnCustomAccent.TabIndex = 6;
            _btnCustomAccent.Text = "Custom color...";
            _btnCustomAccent.UseVisualStyleBackColor = true;
            _btnCustomAccent.Click += _btnCustomAccent_Click;
            // 
            // _tbFirstName
            // 
            _tbFirstName.Location = new Point(3, 50);
            _tbFirstName.Name = "_tbFirstName";
            _tbFirstName.Size = new Size(266, 23);
            _tbFirstName.TabIndex = 7;
            _tbFirstName.TextChanged += _tbFirstName_TextChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 32);
            label1.Name = "label1";
            label1.Size = new Size(64, 15);
            label1.TabIndex = 8;
            label1.Text = "First Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(3, 76);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 10;
            label2.Text = "Last Name";
            // 
            // _tbLastName
            // 
            _tbLastName.Location = new Point(3, 94);
            _tbLastName.Name = "_tbLastName";
            _tbLastName.Size = new Size(266, 23);
            _tbLastName.TabIndex = 9;
            _tbLastName.TextChanged += _tbLastName_TextChanged;
            // 
            // _btnSave
            // 
            _btnSave.Location = new Point(3, 123);
            _btnSave.Name = "_btnSave";
            _btnSave.Size = new Size(130, 28);
            _btnSave.TabIndex = 11;
            _btnSave.Text = "Save";
            _btnSave.UseVisualStyleBackColor = true;
            _btnSave.Click += _btnSave_Click;
            // 
            // _accordionPanelName
            // 
            _accordionPanelName.Controls.Add(label1);
            _accordionPanelName.Controls.Add(_btnSave);
            _accordionPanelName.Controls.Add(_tbLastName);
            _accordionPanelName.Controls.Add(_tbFirstName);
            _accordionPanelName.Controls.Add(label2);
            _accordionPanelName.Dock = DockStyle.Top;
            _accordionPanelName.ExpandedHeight = 162;
            _accordionPanelName.Location = new Point(0, 0);
            _accordionPanelName.Name = "_accordionPanelName";
            _accordionPanelName.Padding = new Padding(0, 32, 0, 0);
            _accordionPanelName.Size = new Size(298, 162);
            _accordionPanelName.TabIndex = 12;
            _accordionPanelName.TabStop = true;
            _accordionPanelName.Title = "Name";
            // 
            // _accordionPanelTheme
            // 
            _accordionPanelTheme.Controls.Add(_lblTheme);
            _accordionPanelTheme.Controls.Add(_rbLight);
            _accordionPanelTheme.Controls.Add(_btnCustomAccent);
            _accordionPanelTheme.Controls.Add(_rbLightDarkNav);
            _accordionPanelTheme.Controls.Add(_flpAccents);
            _accordionPanelTheme.Controls.Add(_rbDark);
            _accordionPanelTheme.Controls.Add(_lblAccent);
            _accordionPanelTheme.Dock = DockStyle.Top;
            _accordionPanelTheme.ExpandedHeight = 294;
            _accordionPanelTheme.Location = new Point(0, 162);
            _accordionPanelTheme.Name = "_accordionPanelTheme";
            _accordionPanelTheme.Padding = new Padding(0, 32, 0, 0);
            _accordionPanelTheme.Size = new Size(298, 294);
            _accordionPanelTheme.TabIndex = 13;
            _accordionPanelTheme.TabStop = true;
            _accordionPanelTheme.Title = "Theme";
            // 
            // _accordionPanelSettings
            // 
            _accordionPanelSettings.Controls.Add(_btnTest);
            _accordionPanelSettings.Controls.Add(_btnSaveServer);
            _accordionPanelSettings.Controls.Add(_tbSecret);
            _accordionPanelSettings.Controls.Add(label6);
            _accordionPanelSettings.Controls.Add(_tbApi);
            _accordionPanelSettings.Controls.Add(label5);
            _accordionPanelSettings.Controls.Add(_tbPort);
            _accordionPanelSettings.Controls.Add(label4);
            _accordionPanelSettings.Controls.Add(_tbServer);
            _accordionPanelSettings.Controls.Add(label3);
            _accordionPanelSettings.Controls.Add(_chkbUseServer);
            _accordionPanelSettings.Dock = DockStyle.Top;
            _accordionPanelSettings.ExpandedHeight = 230;
            _accordionPanelSettings.Location = new Point(0, 456);
            _accordionPanelSettings.Name = "_accordionPanelSettings";
            _accordionPanelSettings.Padding = new Padding(0, 32, 0, 0);
            _accordionPanelSettings.Size = new Size(298, 230);
            _accordionPanelSettings.TabIndex = 14;
            _accordionPanelSettings.TabStop = true;
            _accordionPanelSettings.Title = "Server";
            // 
            // _btnTest
            // 
            _btnTest.Location = new Point(163, 192);
            _btnTest.Name = "_btnTest";
            _btnTest.Size = new Size(129, 28);
            _btnTest.TabIndex = 10;
            _btnTest.Text = "Test Connection...";
            _btnTest.UseVisualStyleBackColor = true;
            _btnTest.Click += _btnTest_Click;
            // 
            // _btnSaveServer
            // 
            _btnSaveServer.Location = new Point(5, 192);
            _btnSaveServer.Name = "_btnSaveServer";
            _btnSaveServer.Size = new Size(129, 28);
            _btnSaveServer.TabIndex = 9;
            _btnSaveServer.Text = "Save";
            _btnSaveServer.UseVisualStyleBackColor = true;
            _btnSaveServer.Click += _btnSaveServer_Click;
            // 
            // _tbSecret
            // 
            _tbSecret.Location = new Point(4, 163);
            _tbSecret.Name = "_tbSecret";
            _tbSecret.Size = new Size(288, 23);
            _tbSecret.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(4, 145);
            label6.Name = "label6";
            label6.Size = new Size(60, 15);
            label6.TabIndex = 7;
            label6.Text = "API Secret";
            // 
            // _tbApi
            // 
            _tbApi.Location = new Point(4, 119);
            _tbApi.Name = "_tbApi";
            _tbApi.Size = new Size(288, 23);
            _tbApi.TabIndex = 6;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 101);
            label5.Name = "label5";
            label5.Size = new Size(47, 15);
            label5.TabIndex = 5;
            label5.Text = "API Key";
            // 
            // _tbPort
            // 
            _tbPort.Location = new Point(173, 75);
            _tbPort.Name = "_tbPort";
            _tbPort.Size = new Size(119, 23);
            _tbPort.TabIndex = 4;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(173, 57);
            label4.Name = "label4";
            label4.Size = new Size(29, 15);
            label4.TabIndex = 3;
            label4.Text = "Port";
            // 
            // _tbServer
            // 
            _tbServer.Location = new Point(4, 75);
            _tbServer.Name = "_tbServer";
            _tbServer.Size = new Size(163, 23);
            _tbServer.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(3, 57);
            label3.Name = "label3";
            label3.Size = new Size(84, 15);
            label3.TabIndex = 1;
            label3.Text = "Server Address";
            // 
            // _chkbUseServer
            // 
            _chkbUseServer.AutoSize = true;
            _chkbUseServer.Location = new Point(4, 35);
            _chkbUseServer.Name = "_chkbUseServer";
            _chkbUseServer.Size = new Size(80, 19);
            _chkbUseServer.TabIndex = 0;
            _chkbUseServer.Text = "Use Server";
            _chkbUseServer.UseVisualStyleBackColor = true;
            _chkbUseServer.CheckedChanged += _chkbUseServer_CheckedChanged;
            // 
            // UCSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "UCSettings";
            Size = new Size(970, 865);
            _pnlElements.ResumeLayout(false);
            _pnlSideBar.ResumeLayout(false);
            _accordionPanelName.ResumeLayout(false);
            _accordionPanelName.PerformLayout();
            _accordionPanelTheme.ResumeLayout(false);
            _accordionPanelTheme.PerformLayout();
            _accordionPanelSettings.ResumeLayout(false);
            _accordionPanelSettings.PerformLayout();
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
        private Elements.AccordionPanel _accordionPanelName;
        private Elements.AccordionPanel _accordionPanelTheme;
        private Elements.AccordionPanel _accordionPanelSettings;
        private CheckBox _chkbUseServer;
        private Label label3;
        private TextBox _tbServer;
        private TextBox _tbPort;
        private Label label4;
        private TextBox _tbApi;
        private Label label5;
        private TextBox _tbSecret;
        private Label label6;
        private Button _btnSaveServer;
        private Button _btnTest;
    }
}
