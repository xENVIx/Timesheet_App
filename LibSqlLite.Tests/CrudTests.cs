using LibSqlLite;

namespace LibSqlLite.Tests;

public class CrudTests : SqliteStoreTestBase
{
    [Fact]
    public void Update_NoMatchingRow_ReturnsFalse()
    {
        var result = Store.Update(new IntKeyModel { Id = 9999, Name = "x" });
        Assert.False(result);
    }

    [Fact]
    public void Update_MatchingRow_UpdatesAndReturnsTrue()
    {
        var item = new IntKeyModel { Name = "first" };
        Store.Insert(item);

        item.Name = "second";
        var result = Store.Update(item);

        Assert.True(result);
        Assert.Equal("second", Store.Get<IntKeyModel>(item.Id)?.Name);
    }

    [Fact]
    public void Delete_NoMatchingRow_ReturnsFalse()
    {
        Store.EnsureTable<IntKeyModel>();
        Assert.False(Store.Delete<IntKeyModel>(9999));
    }

    [Fact]
    public void Delete_MatchingRow_RemovesRowAndReturnsTrue()
    {
        var item = new IntKeyModel { Name = "a" };
        Store.Insert(item);

        Assert.True(Store.Delete<IntKeyModel>(item.Id));
        Assert.Null(Store.Get<IntKeyModel>(item.Id));
    }

    [Fact]
    public void Upsert_InsertsThenUpdates()
    {
        var item = new IntKeyModel { Name = "first" };
        Store.Upsert(item);
        Assert.True(item.Id > 0);

        item.Name = "second";
        Store.Upsert(item);

        Assert.Equal("second", Store.Get<IntKeyModel>(item.Id)?.Name);
        Assert.Single(Store.All<IntKeyModel>());
    }

    [Fact]
    public void DeleteAll_RemovesAllRowsAndReturnsCount()
    {
        Store.InsertMany(new[]
        {
            new IntKeyModel { Name = "a" },
            new IntKeyModel { Name = "b" },
            new IntKeyModel { Name = "c" },
        });

        var removed = Store.DeleteAll<IntKeyModel>();

        Assert.Equal(3, removed);
        Assert.Empty(Store.All<IntKeyModel>());
    }

    [Fact]
    public void InsertMany_InsertsAllItemsInOneTransaction()
    {
        var items = new[]
        {
            new IntKeyModel { Name = "a" },
            new IntKeyModel { Name = "b" },
        };

        Store.InsertMany(items);

        Assert.Equal(2, Store.All<IntKeyModel>().Count);
        Assert.All(items, i => Assert.True(i.Id > 0));
    }

    [Fact]
    public void InsertMany_ConstraintViolationPartwayThrough_RollsBackEntireBatch()
    {
        var items = new[]
        {
            new StringKeyModel { Id = "dup", Name = "first" },
            new StringKeyModel { Id = "dup", Name = "second" }, // duplicate key -> constraint violation
        };

        Assert.ThrowsAny<Exception>(() => Store.InsertMany(items));

        Assert.Empty(Store.All<StringKeyModel>());
    }

    [Fact]
    public void Upsert_GuidKey_SecondCallWithGeneratedIdUpdatesSameRow()
    {
        var item = new GuidKeyModel { Name = "first" };
        Store.Upsert(item); // Guid.Empty -> generates a new key
        var generatedId = item.Id;
        Assert.NotEqual(Guid.Empty, generatedId);

        item.Name = "second";
        Store.Upsert(item); // now has a non-empty key -> must update, not insert

        Assert.Equal(generatedId, item.Id);
        Assert.Single(Store.All<GuidKeyModel>());
        Assert.Equal("second", Store.Get<GuidKeyModel>(generatedId)?.Name);
    }

    [Fact]
    public void Upsert_StringKey_SecondCallWithSameKeyUpdatesSameRow()
    {
        var item = new StringKeyModel { Id = "fixed-key", Name = "first" };
        Store.Upsert(item);

        item.Name = "second";
        Store.Upsert(item);

        Assert.Single(Store.All<StringKeyModel>());
        Assert.Equal("second", Store.Get<StringKeyModel>("fixed-key")?.Name);
    }

    [Fact]
    public void Upsert_LongKey_InsertsThenUpdates()
    {
        var item = new LongKeyModel { Name = "first" };
        Store.Upsert(item);
        Assert.True(item.Id > 0);

        item.Name = "second";
        Store.Upsert(item);

        Assert.Single(Store.All<LongKeyModel>());
        Assert.Equal("second", Store.Get<LongKeyModel>(item.Id)?.Name);
    }

    [Fact]
    public void Upsert_KeyOnlyModel_InsertsThenIsNoOpOnConflict()
    {
        var item = new KeyOnlyModel { Code = "fixed-key" };
        Store.Upsert(item);
        Store.Upsert(item); // no non-key columns to reassign - must not throw

        Assert.Single(Store.All<KeyOnlyModel>());
        Assert.Equal("fixed-key", Store.Get<KeyOnlyModel>("fixed-key")?.Code);
    }
}
