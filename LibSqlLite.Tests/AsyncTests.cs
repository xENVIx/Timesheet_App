using LibSqlLite;

namespace LibSqlLite.Tests;

public class AsyncTests : SqliteStoreTestBase
{
    [Fact]
    public async Task InsertAsync_GeneratesId_AndGetAsyncReturnsItem()
    {
        var item = new IntKeyModel { Name = "a" };
        await Store.InsertAsync(item);

        Assert.True(item.Id > 0);
        var reloaded = await Store.GetAsync<IntKeyModel>(item.Id);
        Assert.Equal("a", reloaded?.Name);
    }

    [Fact]
    public async Task UpdateAsync_MatchingRow_ReturnsTrue()
    {
        var item = new IntKeyModel { Name = "a" };
        await Store.InsertAsync(item);

        item.Name = "b";
        var result = await Store.UpdateAsync(item);

        Assert.True(result);
        Assert.Equal("b", (await Store.GetAsync<IntKeyModel>(item.Id))?.Name);
    }

    [Fact]
    public async Task DeleteAsync_MatchingRow_ReturnsTrue()
    {
        var item = new IntKeyModel { Name = "a" };
        await Store.InsertAsync(item);

        Assert.True(await Store.DeleteAsync<IntKeyModel>(item.Id));
        Assert.Null(await Store.GetAsync<IntKeyModel>(item.Id));
    }

    [Fact]
    public async Task UpsertAsync_InsertsThenUpdates()
    {
        var item = new IntKeyModel { Name = "first" };
        await Store.UpsertAsync(item);
        Assert.True(item.Id > 0);

        item.Name = "second";
        await Store.UpsertAsync(item);

        Assert.Equal("second", (await Store.GetAsync<IntKeyModel>(item.Id))?.Name);
        Assert.Single(await Store.AllAsync<IntKeyModel>());
    }

    [Fact]
    public async Task QueryAndExecuteAsync_RunRawSql()
    {
        await Store.InsertAsync(new IntKeyModel { Name = "a" });

        var affected = await Store.ExecuteAsync(
            "UPDATE [IntKeyModel] SET [Name] = @Name WHERE [Name] = 'a';", new { Name = "updated" });
        Assert.Equal(1, affected);

        var names = await Store.QueryAsync<string>("SELECT [Name] FROM [IntKeyModel];");
        Assert.Equal(["updated"], names);
    }

    [Fact]
    public async Task InsertAsync_GuidKey_GeneratesIdWhenEmpty()
    {
        var item = new GuidKeyModel { Name = "a" };
        await Store.InsertAsync(item);

        Assert.NotEqual(Guid.Empty, item.Id);
        var reloaded = await Store.GetAsync<GuidKeyModel>(item.Id);
        Assert.Equal("a", reloaded?.Name);
    }

    [Fact]
    public async Task InsertAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Store.InsertAsync(new IntKeyModel { Name = "a" }, cts.Token));
    }

    [Fact]
    public async Task AllAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        await Store.EnsureTableAsync<IntKeyModel>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Store.AllAsync<IntKeyModel>(cts.Token));
    }
}
