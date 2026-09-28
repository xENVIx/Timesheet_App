using LibSqlLite;

namespace LibSqlLite.Tests;

public class KeyPrecedenceTests : SqliteStoreTestBase
{
    [Fact]
    public void PrimaryKeyAttribute_WinsOverIdNamedProperty()
    {
        Store.EnsureTable<IdAndPrimaryKeyModel>();

        var columns = Store.Query<PragmaColumn>("PRAGMA table_info([IdAndPrimaryKeyModel]);").ToList();

        Assert.Equal(1, columns.Single(c => c.Name == "Code").Pk);
        Assert.Equal(0, columns.Single(c => c.Name == "Id").Pk);
    }

    [Fact]
    public void PrimaryKeyAttribute_WinsOverIdNamedProperty_UsedAsRealKeyForCrud()
    {
        var item = new IdAndPrimaryKeyModel { Id = 42, Code = "abc", Name = "test" };
        Store.Insert(item);

        var reloaded = Store.Get<IdAndPrimaryKeyModel>("abc");
        Assert.NotNull(reloaded);
        Assert.Equal(42, reloaded!.Id);
        Assert.Equal("test", reloaded.Name);
    }

    [Fact]
    public void UnsupportedKeyType_ThrowsClearError()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Store.EnsureTable<UnsupportedKeyTypeModel>());
        Assert.Contains(nameof(UnsupportedKeyTypeModel), ex.Message);
    }
}
