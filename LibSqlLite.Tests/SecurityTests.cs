using LibSqlLite;

namespace LibSqlLite.Tests;

public class SecurityTests : SqliteStoreTestBase
{
    [Fact]
    public void Values_WithQuotesAndSqlLookingText_StoredAndReturnedVerbatim()
    {
        const string tricky = "Robert'); DROP TABLE IntKeyModel; -- \"quoted\" ' O'Brien";

        var item = new IntKeyModel { Name = tricky };
        Store.Insert(item);

        var reloaded = Store.Get<IntKeyModel>(item.Id);
        Assert.Equal(tricky, reloaded!.Name);

        // The table must still exist and contain exactly the one row inserted above.
        Assert.Single(Store.All<IntKeyModel>());
    }
}
