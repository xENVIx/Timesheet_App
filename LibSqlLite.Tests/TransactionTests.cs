using LibSqlLite;

namespace LibSqlLite.Tests;

public class TransactionTests : SqliteStoreTestBase
{
    [Fact]
    public void InTransaction_RollsBackAllChangesOnException()
    {
        Store.EnsureTable<IntKeyModel>();

        Assert.Throws<InvalidOperationException>(() =>
        {
            Store.InTransaction(store =>
            {
                store.Insert(new IntKeyModel { Name = "a" });
                store.Insert(new IntKeyModel { Name = "b" });
                throw new InvalidOperationException("boom");
            });
        });

        Assert.Empty(Store.All<IntKeyModel>());
    }

    [Fact]
    public void InTransaction_CommitsAllChangesOnSuccess()
    {
        Store.InTransaction(store =>
        {
            store.Insert(new IntKeyModel { Name = "a" });
            store.Insert(new IntKeyModel { Name = "b" });
        });

        Assert.Equal(2, Store.All<IntKeyModel>().Count);
    }

    [Fact]
    public async Task InTransactionAsync_RollsBackAllChangesOnException()
    {
        await Store.EnsureTableAsync<IntKeyModel>();

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Store.InTransactionAsync(async store =>
            {
                await store.InsertAsync(new IntKeyModel { Name = "a" });
                await store.InsertAsync(new IntKeyModel { Name = "b" });
                throw new InvalidOperationException("boom");
            });
        });

        Assert.Empty(await Store.AllAsync<IntKeyModel>());
    }
}
