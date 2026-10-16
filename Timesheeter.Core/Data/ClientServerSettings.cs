using LibLogging;
using LibSqlLite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;


using System.Security.Cryptography;


namespace Timesheeter.Core.Data
{
    /// <summary>
    /// Whether and how the app connects to a Timesheeter server, saved in the app's database as a single row.
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
            public byte[]? ServerAPIKeyStored { get; set; }


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
        public (String address, int port, String apiKey, String secret) ServerDetails 
        { 
            get 
            {
                String apiKey = Encoding.UTF8.GetString(ProtectedData.Unprotect(_setting.ServerAPIKeyStored, null, DataProtectionScope.CurrentUser));
                return (_setting.ServerAddress, _setting.ServerPort, apiKey, _setting.ServerAPISecret);
            } 
            set
            {
                _setting.ServerAddress = value.address;
                _setting.ServerPort = value.port;
                byte[] stored = ProtectedData.Protect(Encoding.UTF8.GetBytes(value.apiKey), null, DataProtectionScope.CurrentUser);
                _setting.ServerAPIKeyStored = stored;
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
                if (_logger != null) _logger.Error($"Exception Saving Server Settings", ex);
                else Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
