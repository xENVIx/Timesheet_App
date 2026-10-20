using System;
using System.Collections.Generic;
using System.Text;
using Timesheeter.Core.Data;
using Timesheeter.Core.Interfaces;

namespace Timesheeter.Classes
{
    /// <summary>
    /// Core's data plus the data only the WinForms app has (<see cref="ClientServerSettings"/>),
    /// so the pages keep asking for everything through <see cref="IDataFactory.GetData{T}"/>.
    /// </summary>
    internal class AppDataFactory : IDataFactory
    {
        private readonly DataFactory _core;
        private readonly ClientServerSettings _serverSettings;

        public AppDataFactory(DataFactory core, ClientServerSettings serverSettings)
        {
            _core = core;
            _serverSettings = serverSettings;
        }

        public T? GetData<T>()
        {
            if (typeof(T) == typeof(ClientServerSettings)) return (T)(object)_serverSettings;
            return _core.GetData<T>();
        }

        public Object? GetData(Type dataType)
        {
            if (dataType == typeof(ClientServerSettings)) return _serverSettings;
            return _core.GetData(dataType);
        }
    }
}
