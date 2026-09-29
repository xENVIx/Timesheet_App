using System.Globalization;
using LibSqlLite;

namespace LibSqlLite.Tests;

public class DateOnlyTimeOnlyTests : SqliteStoreTestBase
{
    [Fact]
    public void EnsureTable_MapsDateOnlyAndTimeOnlyToText()
    {
        Store.EnsureTable<DateTimeOnlyModel>();

        var columns = Store.Query<PragmaColumn>("PRAGMA table_info([DateTimeOnlyModel]);")
            .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

        Assert.Equal("TEXT", columns["Date"].Type);
        Assert.Equal("TEXT", columns["Time"].Type);
        Assert.Equal(1, columns["Date"].NotNull);
        Assert.Equal(1, columns["Time"].NotNull);
        Assert.Equal(0, columns["NullableDate"].NotNull);
        Assert.Equal(0, columns["NullableTime"].NotNull);
    }

    [Fact]
    public void RoundTrip_ValuesAndNulls()
    {
        var item = new DateTimeOnlyModel
        {
            Date = new DateOnly(2026, 9, 29),
            Time = new TimeOnly(8, 30, 15).Add(TimeSpan.FromTicks(1234567)), // sub-second precision
            NullableDate = new DateOnly(2024, 2, 29),
            NullableTime = new TimeOnly(23, 59, 59, 999),
        };
        Store.Insert(item);

        var reloaded = Store.Get<DateTimeOnlyModel>(item.Id)!;
        Assert.Equal(item.Date, reloaded.Date);
        Assert.Equal(item.Time, reloaded.Time);
        Assert.Equal(item.NullableDate, reloaded.NullableDate);
        Assert.Equal(item.NullableTime, reloaded.NullableTime);

        reloaded.NullableDate = null;
        reloaded.NullableTime = null;
        Assert.True(Store.Update(reloaded));

        var cleared = Store.Get<DateTimeOnlyModel>(item.Id)!;
        Assert.Null(cleared.NullableDate);
        Assert.Null(cleared.NullableTime);
    }

    [Fact]
    public void RoundTrip_MinAndMaxValues()
    {
        var min = new DateTimeOnlyModel { Date = DateOnly.MinValue, Time = TimeOnly.MinValue };
        var max = new DateTimeOnlyModel { Date = DateOnly.MaxValue, Time = TimeOnly.MaxValue };
        Store.InsertMany([min, max]);

        Assert.Equal(DateOnly.MinValue, Store.Get<DateTimeOnlyModel>(min.Id)!.Date);
        Assert.Equal(TimeOnly.MinValue, Store.Get<DateTimeOnlyModel>(min.Id)!.Time);
        Assert.Equal(DateOnly.MaxValue, Store.Get<DateTimeOnlyModel>(max.Id)!.Date);
        Assert.Equal(TimeOnly.MaxValue, Store.Get<DateTimeOnlyModel>(max.Id)!.Time);
    }

    [Fact]
    public void StoredAsFixedWidthIsoText()
    {
        Store.Insert(new DateTimeOnlyModel { Date = new DateOnly(2026, 1, 5), Time = new TimeOnly(7, 5) });

        Assert.Equal("2026-01-05", Store.Query<string>("SELECT [Date] FROM [DateTimeOnlyModel];").Single());
        Assert.Equal("07:05:00.0000000", Store.Query<string>("SELECT [Time] FROM [DateTimeOnlyModel];").Single());
    }

    [Fact]
    public void SqlOrderByAndComparisons_AreChronological()
    {
        Store.InsertMany(new[]
        {
            new DateTimeOnlyModel { Date = new DateOnly(2026, 10, 1), Time = new TimeOnly(10, 0) },
            new DateTimeOnlyModel { Date = new DateOnly(2026, 9, 30), Time = new TimeOnly(9, 5) },
            new DateTimeOnlyModel { Date = new DateOnly(2026, 9, 30), Time = new TimeOnly(23, 59) },
            new DateTimeOnlyModel { Date = new DateOnly(2025, 12, 31), Time = new TimeOnly(0, 0) },
        });

        var ordered = Store.Query<DateTimeOnlyModel>(
            "SELECT * FROM [DateTimeOnlyModel] ORDER BY [Date], [Time];");
        Assert.Equal(
            new[] { new DateOnly(2025, 12, 31), new DateOnly(2026, 9, 30), new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 1) },
            ordered.Select(x => x.Date));
        Assert.Equal(new TimeOnly(9, 5), ordered[1].Time);
        Assert.Equal(new TimeOnly(23, 59), ordered[2].Time);

        var inRange = Store.Query<DateTimeOnlyModel>(
            "SELECT * FROM [DateTimeOnlyModel] WHERE [Date] BETWEEN @from AND @to;",
            new { from = new DateOnly(2026, 9, 1), to = new DateOnly(2026, 9, 30) });
        Assert.Equal(2, inRange.Count);
    }

    [Fact]
    public void SqliteTimeFunctions_UnderstandStoredFormat()
    {
        Store.Insert(new DateTimeOnlyModel
        {
            Date = new DateOnly(2026, 9, 29),
            Time = new TimeOnly(9, 0),
            NullableTime = new TimeOnly(17, 30),
        });

        var hours = Store.Query<double>(
            "SELECT (julianday([NullableTime]) - julianday([Time])) * 24 FROM [DateTimeOnlyModel];").Single();
        Assert.Equal(8.5, hours, precision: 6);
    }

    [Fact]
    public void Storage_IsIndependentOfCurrentCulture()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var item = new DateTimeOnlyModel { Date = new DateOnly(2026, 9, 29), Time = new TimeOnly(14, 45) };
            Store.Insert(item);

            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            var reloaded = Store.Get<DateTimeOnlyModel>(item.Id)!;
            Assert.Equal(item.Date, reloaded.Date);
            Assert.Equal(item.Time, reloaded.Time);
            Assert.Equal("2026-09-29", Store.Query<string>("SELECT [Date] FROM [DateTimeOnlyModel];").Single());
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void AddedNotNullTextColumns_ExistingRowsReadAsTypeDefaults()
    {
        Store.EnsureTable<TextDefaultsModelV1>();
        var original = new TextDefaultsModelV1 { Name = "Alice" };
        Store.Insert(original);

        Store.EnsureTable<TextDefaultsModelV2>();

        var reloaded = Store.Get<TextDefaultsModelV2>(original.Id)!;
        Assert.Equal("Alice", reloaded.Name);
        Assert.Equal(default, reloaded.Date);
        Assert.Equal(default, reloaded.Time);
        Assert.Equal(DateTime.MinValue, reloaded.DateTimeValue);
        Assert.Equal(DateTimeOffset.MinValue, reloaded.DateTimeOffsetValue);
        Assert.Equal(Guid.Empty, reloaded.GuidValue);
        Assert.Equal(0m, reloaded.DecimalValue);
    }

    [Fact]
    public void EmptyStringInTextBackedValueColumns_ReadsAsTypeDefault()
    {
        // Simulates a table migrated by an older library version, which used DEFAULT '' for all TEXT columns.
        Store.EnsureTable<TextDefaultsModelV1>();
        var original = new TextDefaultsModelV1 { Name = "Bob" };
        Store.Insert(original);
        foreach (var column in new[] { "Date", "Time", "DateTimeValue", "DateTimeOffsetValue", "GuidValue", "DecimalValue" })
        {
            Store.Execute($"ALTER TABLE [TextDefaultsModel] ADD COLUMN [{column}] TEXT NOT NULL DEFAULT '';");
        }

        var reloaded = Store.Get<TextDefaultsModelV2>(original.Id)!;
        Assert.Equal(default, reloaded.Date);
        Assert.Equal(default, reloaded.Time);
        Assert.Equal(default, reloaded.DateTimeValue);
        Assert.Equal(default, reloaded.DateTimeOffsetValue);
        Assert.Equal(Guid.Empty, reloaded.GuidValue);
        Assert.Equal(0m, reloaded.DecimalValue);
    }
}
