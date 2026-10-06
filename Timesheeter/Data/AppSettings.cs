using System;
using System.Collections.Generic;
using System.Text;

using LibSqlLite;
using Timesheeter.Lib;

namespace Timesheeter.Data
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
            public ThemeMode ThemeMode { get; set; } = ThemeMode.LightWithDarkNavigation;
            public int AccentArgb { get; set; } = Theme.AccentPresets[0].Color.ToArgb();
        }

        // The settings row always has this ID.
        private const int SettingsId = 1;

        private readonly SqliteStore _dataStore;
        private readonly Setting _setting;

        public AppSettings(SqliteStore dataStore)
        {
            _dataStore = dataStore;
            _dataStore.EnsureTable<Setting>();
            _setting = _dataStore.Get<Setting>(SettingsId) ?? new Setting { Id = SettingsId };
        }

        public ThemeMode ThemeMode
        {
            get => Enum.IsDefined(_setting.ThemeMode) ? _setting.ThemeMode : ThemeMode.LightWithDarkNavigation;
            set => _setting.ThemeMode = value;
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
