using LibSqlLite;

namespace LibSqlLite.Tests;

public class KeyTests : SqliteStoreTestBase
{
    [Fact]
    public void Insert_IntKey_GeneratesAndWritesBackId()
    {
        var item = new IntKeyModel { Name = "a" };
        Store.Insert(item);

        Assert.True(item.Id > 0);
        Assert.Equal("a", Store.Get<IntKeyModel>(item.Id)?.Name);
    }

    [Fact]
    public void Insert_LongKey_GeneratesAndWritesBackId()
    {
        var item = new LongKeyModel { Name = "a" };
        Store.Insert(item);

        Assert.True(item.Id > 0);
        Assert.Equal("a", Store.Get<LongKeyModel>(item.Id)?.Name);
    }

    [Fact]
    public void Insert_GuidKey_GeneratesIdWhenEmpty()
    {
        var item = new GuidKeyModel { Name = "a" };
        Store.Insert(item);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal("a", Store.Get<GuidKeyModel>(item.Id)?.Name);
    }

    [Fact]
    public void Insert_GuidKey_PreservesCallerSuppliedValue()
    {
        var id = Guid.NewGuid();
        var item = new GuidKeyModel { Id = id, Name = "a" };
        Store.Insert(item);

        Assert.Equal(id, item.Id);
    }

    [Fact]
    public void Insert_StringKey_UsesCallerSuppliedValue()
    {
        var item = new StringKeyModel { Id = "custom-key", Name = "a" };
        Store.Insert(item);

        var reloaded = Store.Get<StringKeyModel>("custom-key");
        Assert.NotNull(reloaded);
        Assert.Equal("a", reloaded!.Name);
    }
}
