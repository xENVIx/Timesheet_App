using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Timesheeter.Data;
using Timesheeter.Lib;

namespace Timesheeter.UserControls
{
    /// <summary>Lets the user pick the theme mode and accent colour; changes apply and save immediately.</summary>
    public partial class UCSettings : UCSubPage
    {
        private AppSettings? _settings;

        // True while the controls are being set from the saved settings, so that doesn't count as a change.
        private bool _loading;

        public UCSettings() : base()
        {
            InitializeComponent();

            // The swatches show their own colours, so the theme leaves them alone.
            Theme.SetRole(_flpAccents, ThemeRole.Ignore);

            foreach (var (name, color) in Theme.AccentPresets)
            {
                var swatch = new Button()
                {
                    Size = new Size(30, 30),
                    Margin = new Padding(0, 0, 8, 8),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = color,
                    Tag = color,
                    Cursor = Cursors.Hand,
                    TabStop = false,
                };
                swatch.FlatAppearance.MouseOverBackColor = Theme.Blend(color, Color.White, 0.2);
                swatch.Click += (s, e) => ChangeAccent(color);
                _toolTip.SetToolTip(swatch, name);
                _flpAccents.Controls.Add(swatch);
            }

            _btnSave.Enabled = false;

            Theme.Changed += (s, e) => MarkSelectedSwatch();
        }

        protected override void PostInit()
        {
            if (_factory == null) throw new ArgumentNullException(nameof(_factory));

            _settings = _factory.GetData<AppSettings>();
            if (_settings == null) throw new ArgumentNullException(nameof(_settings));

            _loading = true;
            try
            {
                _rbLight.Checked = Theme.Mode == ThemeMode.Light;
                _rbLightDarkNav.Checked = Theme.Mode == ThemeMode.LightWithDarkNavigation;
                _rbDark.Checked = Theme.Mode == ThemeMode.Dark;
            }
            finally
            {
                _loading = false;
            }

            MarkSelectedSwatch();


            _tbFirstName.Text = _settings.Name.FirstName;
            _tbLastName.Text = _settings.Name.LastName;


            if (_tbFirstName.Text.Length <= 0 || _tbLastName.Text.Length <= 0)
            {
                MessageBox.Show(this, $"First Name And / Or Last Name Not Entered - Head Over to the Settings Page to Enter Them!");
            }
        }

        private void _themeMode_CheckedChanged(object? sender, EventArgs e)
        {
            // Fires for the button being unchecked too; only act on the newly checked one.
            if (_loading || sender is not RadioButton { Checked: true } button) return;

            var mode = button == _rbLight ? ThemeMode.Light
                : button == _rbDark ? ThemeMode.Dark
                : ThemeMode.LightWithDarkNavigation;

            SaveAndApply(mode, Theme.Accent);
        }

        private void _btnCustomAccent_Click(object? sender, EventArgs e)
        {
            using var dialog = new ColorDialog()
            {
                Color = Theme.Accent,
                FullOpen = true,
                AnyColor = true,
            };

            if (dialog.ShowDialog(this) == DialogResult.OK) ChangeAccent(dialog.Color);
        }

        private void ChangeAccent(Color accent) => SaveAndApply(Theme.Mode, accent);

        private void SaveAndApply(ThemeMode mode, Color accent)
        {
            if (_settings != null)
            {
                _settings.ThemeMode = mode;
                _settings.Accent = accent;
                if (!_settings.Save()) MessageBox.Show("Could not save the theme settings.");
            }

            Theme.Set(mode, accent);
        }

        /// <summary>Outlines the swatch matching the current accent, if it's one of the presets.</summary>
        private void MarkSelectedSwatch()
        {
            foreach (Button swatch in _flpAccents.Controls.OfType<Button>())
            {
                bool selected = swatch.Tag is Color color && color.ToArgb() == Theme.Accent.ToArgb();
                swatch.FlatAppearance.BorderSize = selected ? 3 : 0;
                swatch.FlatAppearance.BorderColor = Theme.Current.Text;
            }
        }

        private void CheckNamesEnableSave()
        {
            if (_settings == null) return;
            if ((_settings.Name.FirstName != _tbFirstName.Text || _settings.Name.LastName != _tbLastName.Text) && 
                _tbFirstName.Text.Length > 0 && 
                _tbLastName.Text.Length > 0)
            {
                _btnSave.Enabled = true;
            }
            else
            {
                _btnSave.Enabled = false;
            }


        }

        private void _tbFirstName_TextChanged(object sender, EventArgs e)
        {
            if (_settings == null) return;
            CheckNamesEnableSave();
            
        }

        private void _tbLastName_TextChanged(object sender, EventArgs e)
        {
            if (_settings == null) return;
            CheckNamesEnableSave();

        }

        private void _btnSave_Click(object sender, EventArgs e)
        {

            if (_settings == null) return;

            _settings.Name = (_tbFirstName.Text, _tbLastName.Text);


            if (!_settings.Save())
            {
                MessageBox.Show("Could not save the name settings.");
                return;
            }

            _btnSave.Enabled = false;



        }
    }
}
