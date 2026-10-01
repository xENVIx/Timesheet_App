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
            _btnCustomers = new Button();
            _ucCustomers = new UCCustomers();
            _btnTimeEntries = new Button();
            _ucTimeEntries = new UCTimeEntries();
            _btnTimeSheet = new Button();
            _ucTimesheet = new UCTimesheet();
            _btnSettings = new Button();
            _ucSettings = new UCSettings();
            panel1 = new Panel();
            _pnlControls = new Panel();
            panel1.SuspendLayout();
            _pnlControls.SuspendLayout();
            SuspendLayout();
            // 
            // _btnCodes
            // 
            _btnCodes.Location = new Point(3, 52);
            _btnCodes.Name = "_btnCodes";
            _btnCodes.Size = new Size(93, 43);
            _btnCodes.TabIndex = 2;
            _btnCodes.Text = "Project Code Managment";
            _btnCodes.UseVisualStyleBackColor = true;
            _btnCodes.Click += _btnCodes_Click;
            // 
            // _ucNewProjectCode
            // 
            _ucNewProjectCode.Location = new Point(387, 536);
            _ucNewProjectCode.Name = "_ucNewProjectCode";
            _ucNewProjectCode.Size = new Size(577, 413);
            _ucNewProjectCode.TabIndex = 1;
            _ucNewProjectCode.Visible = false;
            // 
            // _btnCustomers
            // 
            _btnCustomers.Location = new Point(3, 3);
            _btnCustomers.Name = "_btnCustomers";
            _btnCustomers.Size = new Size(93, 43);
            _btnCustomers.TabIndex = 1;
            _btnCustomers.Text = "Customer Managment";
            _btnCustomers.UseVisualStyleBackColor = true;
            _btnCustomers.Click += _btnCustomers_Click;
            // 
            // _ucCustomers
            // 
            _ucCustomers.Location = new Point(460, 542);
            _ucCustomers.Name = "_ucCustomers";
            _ucCustomers.Size = new Size(779, 403);
            _ucCustomers.TabIndex = 3;
            _ucCustomers.Visible = false;
            // 
            // _btnTimeEntries
            // 
            _btnTimeEntries.Location = new Point(3, 101);
            _btnTimeEntries.Name = "_btnTimeEntries";
            _btnTimeEntries.Size = new Size(93, 43);
            _btnTimeEntries.TabIndex = 3;
            _btnTimeEntries.Text = "Time Entry Managment";
            _btnTimeEntries.UseVisualStyleBackColor = true;
            _btnTimeEntries.Click += _btnTimeEntries_Click;
            // 
            // _ucTimeEntries
            // 
            _ucTimeEntries.Location = new Point(454, 543);
            _ucTimeEntries.Name = "_ucTimeEntries";
            _ucTimeEntries.Size = new Size(628, 480);
            _ucTimeEntries.TabIndex = 5;
            _ucTimeEntries.Visible = false;
            // 
            // _btnTimeSheet
            // 
            _btnTimeSheet.Location = new Point(3, 150);
            _btnTimeSheet.Name = "_btnTimeSheet";
            _btnTimeSheet.Size = new Size(93, 43);
            _btnTimeSheet.TabIndex = 4;
            _btnTimeSheet.Text = "View Timesheet";
            _btnTimeSheet.UseVisualStyleBackColor = true;
            _btnTimeSheet.Click += _btnTimeSheet_Click;
            // 
            // _ucTimesheet
            // 
            _ucTimesheet.Location = new Point(471, 538);
            _ucTimesheet.Name = "_ucTimesheet";
            _ucTimesheet.Size = new Size(871, 539);
            _ucTimesheet.TabIndex = 7;
            _ucTimesheet.Visible = false;
            // 
            // _btnSettings
            // 
            _btnSettings.Location = new Point(3, 199);
            _btnSettings.Name = "_btnSettings";
            _btnSettings.Size = new Size(93, 43);
            _btnSettings.TabIndex = 5;
            _btnSettings.Text = "Settings";
            _btnSettings.UseVisualStyleBackColor = true;
            _btnSettings.Click += _btnSettings_Click;
            // 
            // _ucSettings
            // 
            _ucSettings.Location = new Point(471, 538);
            _ucSettings.Name = "_ucSettings";
            _ucSettings.Size = new Size(871, 539);
            _ucSettings.TabIndex = 8;
            _ucSettings.Visible = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(_btnSettings);
            panel1.Controls.Add(_btnCustomers);
            panel1.Controls.Add(_btnTimeSheet);
            panel1.Controls.Add(_btnTimeEntries);
            panel1.Controls.Add(_btnCodes);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(104, 477);
            panel1.TabIndex = 8;
            // 
            // _pnlControls
            // 
            _pnlControls.Controls.Add(_ucSettings);
            _pnlControls.Controls.Add(_ucCustomers);
            _pnlControls.Controls.Add(_ucNewProjectCode);
            _pnlControls.Controls.Add(_ucTimesheet);
            _pnlControls.Controls.Add(_ucTimeEntries);
            _pnlControls.Dock = DockStyle.Fill;
            _pnlControls.Location = new Point(104, 0);
            _pnlControls.Name = "_pnlControls";
            _pnlControls.Size = new Size(504, 477);
            _pnlControls.TabIndex = 9;
            // 
            // UCSelectionMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_pnlControls);
            Controls.Add(panel1);
            Name = "UCSelectionMenu";
            Size = new Size(608, 477);
            panel1.ResumeLayout(false);
            _pnlControls.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button _btnCodes;
        private UCNewProjectCode _ucNewProjectCode;
        private Button _btnCustomers;
        private UCCustomers _ucCustomers;
        private Button _btnTimeEntries;
        private UCTimeEntries _ucTimeEntries;
        private Button _btnTimeSheet;
        private UCTimesheet _ucTimesheet;
        private Button _btnSettings;
        private UCSettings _ucSettings;
        private Panel panel1;
        private Panel _pnlControls;
    }
}
