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
    public class ClientServerSettings
    {
        [Table("ServerSettings")]
        public class Setting
        {
            [PrimaryKey] public long ID { get; set; }
            public bool UseServer { get; set; } = false;
            public String ServerAddress { get; set; } = "";
            public int ServerPort { get; set; }
            public String ServerAPISecret { get; set; } = "";


        }

        // The settings row always has this ID.
        private const int SettingsId = 1;

        private readonly SqliteStore _dataStore;
        private readonly Setting _setting;
        private ILoggerApp? _logger;
        public ClientServerSettings(SqliteStore dataStore, ILoggerApp? logger)
        {
            _dataStore = dataStore;
            _dataStore.EnsureTable<Setting>();
            _setting = _dataStore.Get<Setting>(SettingsId) ?? new Setting { ID = SettingsId };
            _logger = logger;
        }


        public bool UseServer { get { return _setting.UseServer; } set { _setting.UseServer = value; } }
        public (String address, int port, String secret) ServerDetails 
        { 
            get 
            {
                return (_setting.ServerAddress, _setting.ServerPort, _setting.ServerAPISecret);
            } 
            set
            {
                _setting.ServerAddress = value.address;
                _setting.ServerPort = value.port;
                _setting.ServerAPISecret = value.secret;
            }
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
