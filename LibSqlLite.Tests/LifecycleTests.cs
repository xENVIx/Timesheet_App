using LibSqlLite;

namespace LibSqlLite.Tests;

public class LifecycleTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespacePath_Throws(string? path)
    {
        Assert.ThrowsAny<ArgumentException>(() => new SqliteStore(path!));
    }

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var path = Path.Combine(Path.GetTempPath(), $"libsqllite-dispose-{Guid.NewGuid():N}.db");
        var store = new SqliteStore(path);

        store.Dispose();
        store.Dispose();

        TryDelete(path);
    }

    [Fact]
    public void UsingStoreAfterDispose_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), $"libsqllite-dispose-{Guid.NewGuid():N}.db");
        var store = new SqliteStore(path);
        store.Dispose();

        Assert.ThrowsAny<Exception>(() => store.Insert(new IntKeyModel { Name = "a" }));

        TryDelete(path);
    }

    private static void TryDelete(string path)
    {
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            try
            {
                if (File.Exists(path + suffix))
                {
                    File.Delete(path + suffix);
                }
            }
            catch (IOException)
            {
                // Best-effort cleanup.
            }
        }
    }
}
