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

            /// <summary>The Timesheeter server to sync with, e.g. https://nas:7045; empty for none.</summary>
            public String ServerUrl { get; set; } = "";

            /// <summary>
            /// The server's API key, encrypted by the app (never stored as plain text). Core only keeps
            /// it; how it's protected is up to the app (Windows uses DPAPI for the current user).
            /// </summary>
            public String ServerApiKeyProtected { get; set; } = "";
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
            


        /// <summary>The sync server's address, trimmed, without a trailing slash; empty for none.</summary>
        public String ServerUrl
        {
            get => _setting.ServerUrl;
            set => _setting.ServerUrl = (value ?? "").Trim().TrimEnd('/');
        }

        /// <summary>The sync server's API key as the app encrypted it (see <see cref="Setting.ServerApiKeyProtected"/>).</summary>
        public String ServerApiKeyProtected
        {
            get => _setting.ServerApiKeyProtected;
            set => _setting.ServerApiKeyProtected = value ?? "";
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
