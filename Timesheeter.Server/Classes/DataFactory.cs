using LibInterfaces;

namespace Timesheeter.Server.Classes
{
    public class DataFactory : IDataFactory
    {


        private APIKeys _apiKeys;

        IDataStore _store;
        ILoggerApp? _logger;

        public DataFactory(IDataStore store, ILoggerApp? logger)
        {

            _store = store;
            _logger = logger;



            _apiKeys = new APIKeys(store, _logger);


        }




        public Object? GetData(Type type)
        {

            if (type == typeof(APIKeys))
            {
                return _apiKeys;
            }

            return null;

        }

        public T? GetData<T>()
        {
            if (typeof(T) ==  typeof(APIKeys))
            {
                return (T)(Object)_apiKeys;
            }


            return default(T);
        }


    }
}
