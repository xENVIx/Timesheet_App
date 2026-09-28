using LibSqlLite;

namespace LibSqlLite.Tests;

public class ConcurrencyTests : SqliteStoreTestBase
{
    [Fact]
    public void ParallelSyncInserts_AllSucceedWithUniqueIds()
    {
        const int count = 200;

        Parallel.For(0, count, i => Store.Insert(new IntKeyModel { Name = $"item-{i}" }));

        var all = Store.All<IntKeyModel>();
        Assert.Equal(count, all.Count);
        Assert.Equal(count, all.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    public async Task ParallelAsyncInserts_AllSucceedWithUniqueIds()
    {
        const int count = 200;

        var tasks = Enumerable.Range(0, count)
            .Select(i => Store.InsertAsync(new IntKeyModel { Name = $"item-{i}" }));
        await Task.WhenAll(tasks);

        var all = await Store.AllAsync<IntKeyModel>();
        Assert.Equal(count, all.Count);
        Assert.Equal(count, all.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    public async Task MixedSyncAndAsyncInserts_FromDifferentThreads_DoNotCorruptState()
    {
        const int syncCount = 100;
        const int asyncCount = 100;

        var syncTask = Task.Run(() =>
        {
            for (var i = 0; i < syncCount; i++)
            {
                Store.Insert(new IntKeyModel { Name = $"sync-{i}" });
            }
        });

        var asyncTasks = Enumerable.Range(0, asyncCount)
            .Select(i => Store.InsertAsync(new IntKeyModel { Name = $"async-{i}" }));

        await Task.WhenAll(new[] { syncTask }.Concat(asyncTasks));

        var all = Store.All<IntKeyModel>();
        Assert.Equal(syncCount + asyncCount, all.Count);
        Assert.Equal(syncCount + asyncCount, all.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    public void NestedInTransaction_EnlistsInSameTransaction_CommitsOnce()
    {
        Store.InTransaction(outer =>
        {
            outer.Insert(new IntKeyModel { Name = "a" });

            outer.InTransaction(inner =>
            {
                inner.Insert(new IntKeyModel { Name = "b" });
            });

            outer.Insert(new IntKeyModel { Name = "c" });
        });

        Assert.Equal(3, Store.All<IntKeyModel>().Count);
    }

    [Fact]
    public void NestedInTransaction_InnerThrow_RollsBackEntireOuterTransaction()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            Store.InTransaction(outer =>
            {
                outer.Insert(new IntKeyModel { Name = "a" });

                outer.InTransaction(inner =>
                {
                    inner.Insert(new IntKeyModel { Name = "b" });
                    throw new InvalidOperationException("boom");
                });
            });
        });

        Assert.Empty(Store.All<IntKeyModel>());
    }

    [Fact]
    public async Task NestedInTransactionAsync_EnlistsInSameTransaction_CommitsOnce()
    {
        await Store.InTransactionAsync(async outer =>
        {
            await outer.InsertAsync(new IntKeyModel { Name = "a" });

            await outer.InTransactionAsync(async inner =>
            {
                await inner.InsertAsync(new IntKeyModel { Name = "b" });
            });

            await outer.InsertAsync(new IntKeyModel { Name = "c" });
        });

        Assert.Equal(3, (await Store.AllAsync<IntKeyModel>()).Count);
    }

    [Fact]
    public void InTransaction_SerializesConcurrentCallersFromOtherThreads()
    {
        Store.EnsureTable<IntKeyModel>();

        var barrierReached = new ManualResetEventSlim(false);
        var releaseWaiter = new ManualResetEventSlim(false);
        var waiterObservedCountBeforeCommit = -1;

        var writer = Task.Run(() =>
        {
            Store.InTransaction(store =>
            {
                store.Insert(new IntKeyModel { Name = "a" });
                barrierReached.Set();
                releaseWaiter.Wait(TimeSpan.FromSeconds(5));
            });
        });

        barrierReached.Wait(TimeSpan.FromSeconds(5));
        var reader = Task.Run(() =>
        {
            // This call must block until the writer's transaction commits or rolls back,
            // since both share the same connection-guarding lock.
            waiterObservedCountBeforeCommit = Store.All<IntKeyModel>().Count;
        });

        Thread.Sleep(100);
        releaseWaiter.Set();

        // Deliberately blocking: this test proves the *synchronous* API's lock blocks a real
        // OS thread across other threads, so an async rewrite would test nothing meaningful here.
#pragma warning disable xUnit1031
        Task.WaitAll(writer, reader);
#pragma warning restore xUnit1031

        Assert.Equal(1, waiterObservedCountBeforeCommit);
    }
}
