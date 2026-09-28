using LibSqlLite;

namespace LibSqlLite.Tests;

public class PersistenceTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"libsqllite-persist-{Guid.NewGuid():N}.db");

    [Fact]
    public void Data_SurvivesAcrossStoreInstances_OnSameFile()
    {
        using (var first = new SqliteStore(_path))
        {
            first.Insert(new IntKeyModel { Name = "persisted" });
        }

        using var second = new SqliteStore(_path);
        var all = second.All<IntKeyModel>();

        Assert.Single(all);
        Assert.Equal("persisted", all[0].Name);
    }

    [Fact]
    public void Constructor_CreatesMissingDirectory()
    {
        var nestedPath = Path.Combine(
            Path.GetTempPath(), $"libsqllite-dir-{Guid.NewGuid():N}", "nested", "data.db");

        using (var store = new SqliteStore(nestedPath))
        {
            store.Insert(new IntKeyModel { Name = "a" });
            Assert.True(File.Exists(nestedPath));
        }

        Directory.Delete(Path.GetDirectoryName(Path.GetDirectoryName(nestedPath))!, recursive: true);
    }

    [Fact]
    public void Constructor_EnablesWalJournalModeAndForeignKeys()
    {
        using var store = new SqliteStore(_path);

        var journalMode = store.Query<string>("PRAGMA journal_mode;").Single();
        var foreignKeys = store.Query<long>("PRAGMA foreign_keys;").Single();

        Assert.Equal("wal", journalMode, StringComparer.OrdinalIgnoreCase);
        Assert.Equal(1, foreignKeys);
    }

    public void Dispose()
    {
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            try
            {
                if (File.Exists(_path + suffix))
                {
                    File.Delete(_path + suffix);
                }
            }
            catch (IOException)
            {
                // Best-effort cleanup.
            }
        }
    }
}
