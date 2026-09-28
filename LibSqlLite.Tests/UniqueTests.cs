using LibSqlLite;
using Microsoft.Data.Sqlite;

namespace LibSqlLite.Tests;

public class UniqueTests : SqliteStoreTestBase
{
    [Fact]
    public void EnsureTable_CreatesUniqueIndexPerUniqueProperty()
    {
        Store.EnsureTable<UniqueModel>();

        var indexes = Store.Query<string>(
            "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'UniqueModel';");

        Assert.Contains("UX_UniqueModel_Name", indexes);
        Assert.Contains("UX_UniqueModel_Code", indexes);
    }

    [Fact]
    public void Unique_IgnoreCase_RejectsValueDifferingOnlyByCase()
    {
        Store.Insert(new UniqueModel { Name = "Acme", Code = "a" });

        var ex = Assert.Throws<SqliteException>(() => Store.Insert(new UniqueModel { Name = "ACME", Code = "b" }));
        Assert.Contains("UNIQUE", ex.Message);
        Assert.Single(Store.All<UniqueModel>());
    }

    [Fact]
    public void Unique_CaseSensitive_AllowsValueDifferingOnlyByCase()
    {
        Store.Insert(new UniqueModel { Name = "One", Code = "abc" });
        Store.Insert(new UniqueModel { Name = "Two", Code = "ABC" });

        Assert.Equal(2, Store.All<UniqueModel>().Count);
        Assert.Throws<SqliteException>(() => Store.Insert(new UniqueModel { Name = "Three", Code = "abc" }));
    }

    [Fact]
    public void Unique_UpdateToOwnValueSucceeds_UpdateToAnotherRowsValueThrows()
    {
        var first = new UniqueModel { Name = "Acme", Code = "a" };
        var second = new UniqueModel { Name = "Globex", Code = "b" };
        Store.Insert(first);
        Store.Insert(second);

        first.Name = "ACME";
        Assert.True(Store.Update(first));

        second.Name = "acme";
        Assert.Throws<SqliteException>(() => Store.Update(second));
    }

    [Fact]
    public void Unique_AddedToExistingTable_CreatesIndex()
    {
        Store.EnsureTable<UniqueMigrationModelV1>();
        Store.Insert(new UniqueMigrationModelV1 { Name = "Acme" });

        Store.EnsureTable<UniqueMigrationModelV2>();

        Assert.Throws<SqliteException>(() => Store.Insert(new UniqueMigrationModelV2 { Name = "acme" }));
    }

    [Fact]
    public void Unique_OnPrimaryKey_CreatesNoExtraIndex()
    {
        Store.EnsureTable<UniqueKeyModel>();

        var indexes = Store.Query<string>(
            "SELECT name FROM sqlite_master WHERE type = 'index' AND tbl_name = 'UniqueKeyModel';");

        Assert.DoesNotContain(indexes, n => n.StartsWith("UX_"));
    }
}
