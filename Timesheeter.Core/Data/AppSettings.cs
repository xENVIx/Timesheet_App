using LibLogging;
using LibSqlLite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Timesheeter.Core.Data
{
    /// <summary>
    /// User preferences, saved in the app's database as a single row.
    /// </summary>
    public class AppSettings
    {
        [Table("AppSettings")]
        public class Setting
        {
            public int Id { get; set; }
            public String FirstName { get; set; } = "";
            public String LastName { get; set; } = "";
            public ThemeMode ThemeMode { get; set; } = ThemeMode.LightWithDarkNavigation;
            public int AccentArgb { get; set; } = DefaultAccentArgb;
        }

        // Theme.AccentPresets[0] ("Blue"); the theme itself lives in the WinForms app.
        public static readonly int DefaultAccentArgb = Color.FromArgb(37, 99, 235).ToArgb();

        // The settings row always has this ID.
        private const int SettingsId = 1;

        private readonly SqliteStore _dataStore;
        private readonly Setting _setting;
        private ILoggerApp? _logger;
        public AppSettings(SqliteStore dataStore, ILoggerApp? logger)
        {
            _dataStore = dataStore;
            _dataStore.EnsureTable<Setting>();
            _setting = _dataStore.Get<Setting>(SettingsId) ?? new Setting { Id = SettingsId };
            _logger = logger;
        }

        public ThemeMode ThemeMode
        {
            get => Enum.IsDefined(_setting.ThemeMode) ? _setting.ThemeMode : ThemeMode.LightWithDarkNavigation;
            set => _setting.ThemeMode = value;
        }

        public (String FirstName, String LastName) Name 
        { 
            get 
            { 
                return (_setting.FirstName, _setting.LastName); 
            }
            set
            {
                _setting.FirstName = value.FirstName;
                _setting.LastName = value.LastName;
            }
        }
            


        public Color Accent
        {
            get => Color.FromArgb(_setting.AccentArgb);
            set => _setting.AccentArgb = value.ToArgb();
        }

        public bool Save()
        {
            try
            {
                _dataStore.Upsert(_setting);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
