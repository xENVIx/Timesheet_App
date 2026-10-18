using LibLogging;
using LibSqlLite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Timesheeter.Core.Data;

namespace Timesheeter.Classes
{
    /// <summary>
    /// Whether and how the app connects to a Timesheeter server, saved in the app's database as a single row.
    /// Lives in the WinForms app rather than Core: the API key is protected with Windows DPAPI
    /// (ProtectedData), which only exists on Windows, and Core has to run on Linux too.
    /// </summary>
    public class ClientServerSettings : DataBase
    {
        [Table("ClientServerSettings")]
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

        private Classes.ServerConnect? _server;


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
                return (_setting.ServerAddress, _setting.ServerPort, ReadApiKey(), _setting.ServerAPISecret);
            } 
            set
            {
                _setting.ServerAddress = value.address;
                _setting.ServerPort = value.port;
                // No key is always stored as null, never as an encrypted empty string.
                _setting.ServerAPIKeyStored = String.IsNullOrEmpty(value.apiKey)
                    ? null
                    : ProtectedData.Protect(Encoding.UTF8.GetBytes(value.apiKey), null, DataProtectionScope.CurrentUser);
                _setting.ServerAPISecret = value.secret;
            }
        }

        /// <summary>The API key, or "" when none is saved or it can't be read here.</summary>
        private String ReadApiKey()
        {
            if (_setting.ServerAPIKeyStored == null || _setting.ServerAPIKeyStored.Length == 0) return "";
            try
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(_setting.ServerAPIKeyStored, null, DataProtectionScope.CurrentUser));
            }
            catch (CryptographicException ex)
            {
                // Saved by another Windows user or on another PC (e.g. a restored backup): it has to be entered again.
                _logger?.Error($"Stored API Key Can't Be Read Here", ex);
                return "";
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


        public ServerConnect.TestResults? TestConnection()
        {
            
            if (_server == null)
            {
                _server = new ServerConnect(this);
                return _server.TestConnection();
            }

            return null;


        }


    }


}
