using System.Security.Cryptography;
using System.Text;
using LibSqlLite;

namespace Timesheeter.Server
{
    /// <summary>
    /// The server's API keys. Each device gets its own named key; only a SHA-256 hash of it is
    /// stored, so the database (or a backup of it) can't be used to get in. Requests send the key in
    /// the <see cref="HeaderName"/> header.
    /// </summary>
    public sealed class ApiKeys
    {
        public const string HeaderName = "X-Api-Key";

        // Keys look like "tsk_" + 43 characters: 32 random bytes, base64url. Too long to guess, so a
        // plain (fast) hash is enough; a slow password hash is for short, human-chosen secrets.
        private const string Prefix = "tsk_";

        [Table("ApiKey")]
        public sealed class Key
        {
            [PrimaryKey] public Guid ID { get; set; }

            /// <summary>Which device or person it's for, e.g. "laptop".</summary>
            [Unique(IgnoreCase = true)] public string Name { get; set; } = string.Empty;

            /// <summary>SHA-256 of the key, lowercase hex.</summary>
            public string Hash { get; set; } = string.Empty;

            /// <summary>When it was created, UTC.</summary>
            public DateTime Created { get; set; }

            /// <summary>When it was revoked, UTC; null while it still works.</summary>
            public DateTime? Revoked { get; set; }
        }

        private readonly SqliteStore _store;

        public ApiKeys(SqliteStore store)
        {
            _store = store;
            _store.EnsureTable<Key>();
        }

        /// <summary>Keys that still work.</summary>
        public IReadOnlyList<Key> Active() => _store.All<Key>().Where(k => k.Revoked == null).ToList();

        /// <summary>All keys, revoked ones included.</summary>
        public IReadOnlyList<Key> All() => _store.All<Key>();

        /// <summary>
        /// Creates a key named <paramref name="name"/> and returns it. This is the only time the key
        /// itself is available; the server keeps just its hash.
        /// </summary>
        public string Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A key needs a name.", nameof(name));

            string key = Prefix + Base64Url(RandomNumberGenerator.GetBytes(32));
            _store.Insert(new Key
            {
                ID = Guid.NewGuid(),
                Name = name.Trim(),
                Hash = HashOf(key),
                Created = DateTime.UtcNow,
            });
            return key;
        }

        /// <summary>Stops the named key working. False if there's no such active key.</summary>
        public bool Revoke(string name)
        {
            var key = Active().FirstOrDefault(k => string.Equals(k.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (key == null) return false;

            key.Revoked = DateTime.UtcNow;
            return _store.Update(key);
        }

        /// <summary>
        /// The active key matching <paramref name="presented"/>, or null. Compares against every key
        /// in constant time, so how long it takes doesn't reveal how close a guess was.
        /// </summary>
        public Key? Find(string? presented)
        {
            if (string.IsNullOrEmpty(presented)) return null;

            byte[] hash = Convert.FromHexString(HashOf(presented.Trim()));
            Key? match = null;
            foreach (var key in Active())
            {
                if (CryptographicOperations.FixedTimeEquals(hash, Convert.FromHexString(key.Hash))) match = key;
            }
            return match;
        }

        private static string HashOf(string key) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key))).ToLowerInvariant();

        private static string Base64Url(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
