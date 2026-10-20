using System.Security.Cryptography;
using System.Text;

namespace Timesheeter.Server.Classes
{
    public static class ApiKeyUtility
    {
        public static string GenerateApiKey()
        {
            byte[] keyBytes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(keyBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static byte[] HashApiKey(string apiKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(apiKey);
            return SHA256.HashData(keyBytes);
        }

        public static bool ValidateApiKey(
            string suppliedApiKey,
            byte[] storedHash)
        {
            byte[] suppliedHash = HashApiKey(suppliedApiKey);

            return CryptographicOperations.FixedTimeEquals(
                suppliedHash,
                storedHash
            );
        }
    }
}
