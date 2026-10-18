using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheeter.Classes
{
    public sealed class ServerConnect
    {

        private HttpClient? _httpClient;

        private readonly string apiKey;
        private readonly string apiSecret;
        private readonly string serverAddress;
        private readonly int serverPort;

        public ServerConnect(ClientServerSettings svrSettings)
        {
            var details = svrSettings.ServerDetails;
            apiKey = details.apiKey;
            apiSecret = details.secret;
            serverAddress = details.address;
            serverPort = details.port;

        }


        public void TestConnection()
        {

            



        }



    }
}
