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

        public enum EServerTestResults
        {
            SUCCESS,
            FAILURE,
            EXCEPTION
        }

        public ServerConnect(String webAddr, int port, String key, String secret)
        {
            //var details = svrSettings.ServerDetails;
            apiKey = key;
            apiSecret = secret;
            serverAddress = webAddr;
            serverPort = port;


        }


        public TestResults TestConnection()
        {

            EServerTestResults status = EServerTestResults.FAILURE;
            Exception? retExc = null;
            String message = String.Empty;

            try
            {




            }
            catch (Exception ex)
            {
                retExc = ex;
                status = EServerTestResults.EXCEPTION;
            }


            return new TestResults(status, message, retExc);


        }


        public class TestResults
        {



            public EServerTestResults ResultsStatus { get; private set; }
            public String Message { get; private set; } = String.Empty;
            public Exception? Ex { get; private set; } = null;


            public TestResults(EServerTestResults results, String message, Exception? ex = null)
            {

                ResultsStatus = results;
                Message = message;
                Ex = ex;

            }



        }



    }
}
