using LibSqlLite;

namespace LibSqlLite.Tests;

public class ReflectionExclusionTests : SqliteStoreTestBase
{
    [Fact]
    public void EnsureTable_OnlyIncludesEligibleProperties()
    {
        Store.EnsureTable<ReflectionExclusionModel>();

        var columnNames = Store.Query<PragmaColumn>("PRAGMA table_info([ReflectionExclusionModel]);")
            .Select(c => c.Name)
            .ToList();

        Assert.Equal(["Id", "Name"], columnNames.OrderBy(n => n));
    }

    [Fact]
    public void Insert_IgnoresExcludedProperties_AndRoundTripsEligibleOnes()
    {
        var item = new ReflectionExclusionModel { Name = "test" };
        Store.Insert(item);

        var reloaded = Store.Get<ReflectionExclusionModel>(item.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("test", reloaded!.Name);
    }

    [Fact]
    public void EnsureTable_ExcludesNonScalarCollectionProperty()
    {
        Store.EnsureTable<ListPropertyModel>();

        var columnNames = Store.Query<PragmaColumn>("PRAGMA table_info([ListPropertyModel]);")
            .Select(c => c.Name)
            .ToList();

        Assert.Equal(["Id", "Name"], columnNames.OrderBy(n => n));
    }
}
