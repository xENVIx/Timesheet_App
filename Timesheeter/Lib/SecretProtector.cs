using System;
using System.Security.Cryptography;
using System.Text;

namespace Timesheeter.Lib
{
    /// <summary>
    /// Encrypts secrets (e.g. the sync server's API key) with Windows DPAPI for the current user, so
    /// they're stored in the database as unreadable text: only this Windows account on this PC can
    /// decrypt them. A copied database file is useless to anyone else.
    /// </summary>
    public static class SecretProtector
    {
        // Ties the encrypted text to this use, so another app's DPAPI data can't be swapped in.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("Timesheeter.ServerApiKey");

        /// <summary>The secret encrypted, as base64; empty for an empty secret.</summary>
        public static string Protect(string secret)
        {
            if (string.IsNullOrEmpty(secret)) return "";
            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(secret), Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// The secret decrypted; empty if there is none, or it can't be decrypted (e.g. the database
        /// came from another PC or Windows account, so the key has to be entered again).
        /// </summary>
        public static string Unprotect(string protectedSecret)
        {
            if (string.IsNullOrEmpty(protectedSecret)) return "";
            try
            {
                byte[] decrypted = ProtectedData.Unprotect(Convert.FromBase64String(protectedSecret), Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex) when (ex is CryptographicException or FormatException)
            {
                return "";
            }
        }
    }
}
