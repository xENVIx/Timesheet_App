using LibSqlLite;

namespace LibSqlLite.Tests;

public class RoundTripTests : SqliteStoreTestBase
{
    [Fact]
    public void RoundTrip_AllSupportedTypes_WithValues()
    {
        var item = new AllTypesModel
        {
            LongValue = 123456789012345L,
            ShortValue = 42,
            ByteValue = 7,
            BoolValue = true,
            DoubleValue = 3.14159,
            FloatValue = 2.5f,
            DecimalValue = 12345.6789012345m,
            StringValue = "hello world",
            GuidValue = Guid.NewGuid(),
            DateTimeValue = new DateTime(2024, 1, 1, 12, 30, 0, DateTimeKind.Utc),
            DateTimeOffsetValue = new DateTimeOffset(2024, 1, 1, 12, 30, 0, TimeSpan.FromHours(-5)),
            BlobValue = [1, 2, 3, 4, 5],
            EnumValue = SampleEnum.Second,
            NullableInt = 99,
            NullableString = "not null",
            NullableGuid = Guid.NewGuid(),
            NullableDateTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            NullableDecimal = 1.5m,
        };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(item.LongValue, reloaded!.LongValue);
        Assert.Equal(item.ShortValue, reloaded.ShortValue);
        Assert.Equal(item.ByteValue, reloaded.ByteValue);
        Assert.Equal(item.BoolValue, reloaded.BoolValue);
        Assert.Equal(item.DoubleValue, reloaded.DoubleValue);
        Assert.Equal(item.FloatValue, reloaded.FloatValue);
        Assert.Equal(item.DecimalValue, reloaded.DecimalValue);
        Assert.Equal(item.StringValue, reloaded.StringValue);
        Assert.Equal(item.GuidValue, reloaded.GuidValue);
        Assert.Equal(item.DateTimeValue, reloaded.DateTimeValue);
        Assert.Equal(DateTimeKind.Utc, reloaded.DateTimeValue.Kind);
        Assert.Equal(item.DateTimeOffsetValue, reloaded.DateTimeOffsetValue);
        Assert.Equal(item.DateTimeOffsetValue.Offset, reloaded.DateTimeOffsetValue.Offset);
        Assert.Equal(item.BlobValue, reloaded.BlobValue);
        Assert.Equal(item.EnumValue, reloaded.EnumValue);
        Assert.Equal(item.NullableInt, reloaded.NullableInt);
        Assert.Equal(item.NullableString, reloaded.NullableString);
        Assert.Equal(item.NullableGuid, reloaded.NullableGuid);
        Assert.Equal(item.NullableDateTime, reloaded.NullableDateTime);
        Assert.Equal(item.NullableDecimal, reloaded.NullableDecimal);
    }

    [Fact]
    public void RoundTrip_NullableTypes_WithNulls()
    {
        var item = new AllTypesModel
        {
            StringValue = "s",
            BlobValue = [],
            NullableInt = null,
            NullableString = null,
            NullableGuid = null,
            NullableDateTime = null,
            NullableDecimal = null,
        };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.NotNull(reloaded);
        Assert.Null(reloaded!.NullableInt);
        Assert.Null(reloaded.NullableString);
        Assert.Null(reloaded.NullableGuid);
        Assert.Null(reloaded.NullableDateTime);
        Assert.Null(reloaded.NullableDecimal);
    }

    [Fact]
    public void RoundTrip_DateTime_ConvertsNonUtcToUtcOnWrite()
    {
        var local = new DateTime(2024, 3, 1, 8, 0, 0, DateTimeKind.Local);
        var item = new AllTypesModel { StringValue = "s", BlobValue = [], DateTimeValue = local };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(DateTimeKind.Utc, reloaded!.DateTimeValue.Kind);
        Assert.Equal(local.ToUniversalTime(), reloaded.DateTimeValue);
    }
}
