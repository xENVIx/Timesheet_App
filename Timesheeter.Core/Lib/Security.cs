using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Timesheeter.Core.Lib
{
    public static class Security
    {

        public static string ComputeSignature(string apiKey, string apiSecret)
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(apiSecret);
            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);

            using HMACSHA256 hmac = new HMACSHA256(secretBytes);

            byte[] signatureBytes = hmac.ComputeHash(apiKeyBytes);

            return Convert.ToBase64String(signatureBytes);
        }

    }
}
