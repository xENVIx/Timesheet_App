using LibSqlLite;

namespace LibSqlLite.Tests;

public class MigrationTests : SqliteStoreTestBase
{
    [Fact]
    public void EnsureTable_AddsNewColumn_PreservesExistingRows()
    {
        Store.EnsureTable<MigrationModelV1>();
        var original = new MigrationModelV1 { Name = "Alice" };
        Store.Insert(original);

        Store.EnsureTable<MigrationModelV2>();

        var columnNames = Store.Query<PragmaColumn>("PRAGMA table_info([MigrationModel]);")
            .Select(c => c.Name)
            .ToList();
        Assert.Contains("Extra", columnNames);

        var reloaded = Store.Get<MigrationModelV2>(original.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("Alice", reloaded!.Name);
        Assert.Null(reloaded.Extra);
    }

    [Fact]
    public void EnsureTable_AddsNonNullableColumn_ExistingRowsGetTypeDefault()
    {
        Store.EnsureTable<MigrationModel2V1>();
        var original = new MigrationModel2V1 { Name = "Alice" };
        Store.Insert(original);

        Store.EnsureTable<MigrationModel2V2>();

        var countColumn = Store.Query<PragmaColumn>("PRAGMA table_info([MigrationModel2]);")
            .Single(c => c.Name == "Count");
        Assert.Equal(1, countColumn.NotNull);

        var reloaded = Store.Get<MigrationModel2V2>(original.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(0, reloaded!.Count);
    }

    [Fact]
    public void ConcurrentEnsureTable_ForBrandNewType_CreatesTableExactlyOnce()
    {
        Parallel.For(0, 20, _ => Store.EnsureTable<IntKeyModel>());

        var tableCount = Store.Query<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'IntKeyModel';").Single();
        Assert.Equal(1, tableCount);
    }

    [Fact]
    public void EnsureTable_CalledTwice_IsIdempotent()
    {
        Store.EnsureTable<IntKeyModel>();
        Store.EnsureTable<IntKeyModel>();

        var columnNames = Store.Query<PragmaColumn>("PRAGMA table_info([IntKeyModel]);")
            .Select(c => c.Name)
            .ToList();
        Assert.Equal(2, columnNames.Count);
    }
}
