using LibSqlLite;

namespace LibSqlLite.Tests;

public class ExtremeValueTests : SqliteStoreTestBase
{
    [Theory]
    [InlineData("79228162514264337593543950335")] // decimal.MaxValue
    [InlineData("-79228162514264337593543950335")] // decimal.MinValue
    [InlineData("0")]
    [InlineData("-0.00000001")]
    [InlineData("12345678901234567890.123456789")]
    public void RoundTrip_DecimalExtremes(string decimalText)
    {
        var value = decimal.Parse(decimalText, System.Globalization.CultureInfo.InvariantCulture);
        var item = new AllTypesModel { StringValue = "s", BlobValue = [], DecimalValue = value };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.Equal(value, reloaded!.DecimalValue);
    }

    [Fact]
    public void RoundTrip_IntAndLongExtremes()
    {
        var item = new AllTypesModel
        {
            StringValue = "s",
            BlobValue = [],
            LongValue = long.MaxValue,
            ShortValue = short.MinValue,
            ByteValue = byte.MaxValue,
        };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.Equal(long.MaxValue, reloaded!.LongValue);
        Assert.Equal(short.MinValue, reloaded.ShortValue);
        Assert.Equal(byte.MaxValue, reloaded.ByteValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(-5)]
    [InlineData(14)]
    [InlineData(-14)]
    public void RoundTrip_DateTimeOffset_PreservesOffset(int offsetHours)
    {
        var value = new DateTimeOffset(2024, 6, 15, 10, 0, 0, TimeSpan.FromHours(offsetHours));
        var item = new AllTypesModel { StringValue = "s", BlobValue = [], DateTimeOffsetValue = value };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.Equal(value, reloaded!.DateTimeOffsetValue);
        Assert.Equal(TimeSpan.FromHours(offsetHours), reloaded.DateTimeOffsetValue.Offset);
    }

    [Theory]
    [InlineData(SampleEnum.None)]
    [InlineData(SampleEnum.First)]
    [InlineData(SampleEnum.Second)]
    public void RoundTrip_EnumValues(SampleEnum value)
    {
        var item = new AllTypesModel { StringValue = "s", BlobValue = [], EnumValue = value };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.Equal(value, reloaded!.EnumValue);
    }

    [Fact]
    public void RoundTrip_EmptyByteArray()
    {
        var item = new AllTypesModel { StringValue = "s", BlobValue = [] };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.NotNull(reloaded!.BlobValue);
        Assert.Empty(reloaded.BlobValue);
    }

    [Fact]
    public void RoundTrip_LargeByteArray()
    {
        var bytes = new byte[64 * 1024];
        new Random(42).NextBytes(bytes);
        var item = new AllTypesModel { StringValue = "s", BlobValue = bytes };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.Equal(bytes, reloaded!.BlobValue);
    }

    [Fact]
    public void RoundTrip_UnicodeAndEmptyStrings()
    {
        var item = new AllTypesModel { StringValue = "héllo 世界 🎉 \n\t", BlobValue = [] };

        Store.Insert(item);
        var reloaded = Store.Get<AllTypesModel>(item.Id);

        Assert.Equal(item.StringValue, reloaded!.StringValue);

        var item2 = new AllTypesModel { StringValue = "", BlobValue = [] };
        Store.Insert(item2);
        var reloaded2 = Store.Get<AllTypesModel>(item2.Id);
        Assert.Equal("", reloaded2!.StringValue);
    }
}
