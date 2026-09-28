using LibSqlLite;

namespace LibSqlLite.Tests;

public abstract class SqliteStoreTestBase : IDisposable
{
    private readonly string _path;

    protected SqliteStoreTestBase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"libsqllite-tests-{Guid.NewGuid():N}.db");
        Store = new SqliteStore(_path);
    }

    protected SqliteStore Store { get; }

    public void Dispose()
    {
        Store.Dispose();
        TryDelete(_path);
        TryDelete(_path + "-wal");
        TryDelete(_path + "-shm");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup; a lingering WAL/SHM file doesn't fail the test.
        }
    }
}
