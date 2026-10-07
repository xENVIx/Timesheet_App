namespace Timesheeter.Forms
{
    partial class FrmTimesheetUserInfo
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            _tbLastName = new TextBox();
            _tbFirstName = new TextBox();
            _btnOk = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(64, 15);
            label1.TabIndex = 0;
            label1.Text = "First Name";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 67);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 1;
            label2.Text = "Last Name";
            // 
            // _tbLastName
            // 
            _tbLastName.Location = new Point(12, 85);
            _tbLastName.Name = "_tbLastName";
            _tbLastName.Size = new Size(208, 23);
            _tbLastName.TabIndex = 1;
            // 
            // _tbFirstName
            // 
            _tbFirstName.Location = new Point(12, 27);
            _tbFirstName.Name = "_tbFirstName";
            _tbFirstName.Size = new Size(208, 23);
            _tbFirstName.TabIndex = 0;
            // 
            // _btnOk
            // 
            _btnOk.Location = new Point(12, 216);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new Size(208, 23);
            _btnOk.TabIndex = 2;
            _btnOk.Text = "Okay";
            _btnOk.UseVisualStyleBackColor = true;
            _btnOk.Click += _btnOk_Click;
            // 
            // FrmTimesheetUserInfo
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(264, 251);
            Controls.Add(_btnOk);
            Controls.Add(_tbFirstName);
            Controls.Add(_tbLastName);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Name = "FrmTimesheetUserInfo";
            Text = "FrmTimesheetUserInfo";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox _tbLastName;
        private TextBox _tbFirstName;
        private Button _btnOk;
    }
}