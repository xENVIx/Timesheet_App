using LibSqlLite;

namespace LibSqlLite.Tests;

public class EmptyResultTests : SqliteStoreTestBase
{
    [Fact]
    public void All_OnEmptyTable_ReturnsEmptyList()
    {
        Store.EnsureTable<IntKeyModel>();
        Assert.Empty(Store.All<IntKeyModel>());
    }

    [Fact]
    public void Get_MissingId_ReturnsNull()
    {
        Store.EnsureTable<IntKeyModel>();
        Assert.Null(Store.Get<IntKeyModel>(12345));
    }

    [Fact]
    public void DeleteAll_OnEmptyTable_ReturnsZero()
    {
        Store.EnsureTable<IntKeyModel>();
        Assert.Equal(0, Store.DeleteAll<IntKeyModel>());
    }

    [Fact]
    public void InsertMany_EmptyCollection_IsNoOp()
    {
        Store.InsertMany(Array.Empty<IntKeyModel>());
        Assert.Empty(Store.All<IntKeyModel>());
    }

    [Fact]
    public void Query_WithNoMatchingRows_ReturnsEmptyList()
    {
        Store.EnsureTable<IntKeyModel>();
        var result = Store.Query<IntKeyModel>("SELECT * FROM [IntKeyModel] WHERE [Name] = @Name;", new { Name = "nope" });
        Assert.Empty(result);
    }
}

public class ForAppTests : IDisposable
{
    private readonly string _appName = $"LibSqlLiteTestApp-{Guid.NewGuid():N}";

    [Fact]
    public void ForApp_CreatesDatabaseUnderAppDataAppNameFolder()
    {
        using var store = SqliteStore.ForApp(_appName);
        store.Insert(new IntKeyModel { Name = "a" });

        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appName, "data.db");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public void ForApp_CustomFileName_UsesGivenFileName()
    {
        using var store = SqliteStore.ForApp(_appName, "custom.db");

        var expectedPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appName, "custom.db");
        Assert.True(File.Exists(expectedPath));
    }

    public void Dispose()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appName);
        try
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }
        catch (IOException)
        {
            // Best-effort cleanup.
        }
    }
}
