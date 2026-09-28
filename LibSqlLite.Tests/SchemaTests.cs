using LibSqlLite;

namespace LibSqlLite.Tests;

public class SchemaTests : SqliteStoreTestBase
{
    [Fact]
    public void EnsureTable_CreatesColumnsWithCorrectTypes()
    {
        Store.EnsureTable<AllTypesModel>();

        var columns = Store.Query<PragmaColumn>("PRAGMA table_info([AllTypesModel]);")
            .ToDictionary(c => c.Name, c => c.Type, StringComparer.OrdinalIgnoreCase);

        Assert.Equal("INTEGER", columns["Id"]);
        Assert.Equal("INTEGER", columns["LongValue"]);
        Assert.Equal("INTEGER", columns["ShortValue"]);
        Assert.Equal("INTEGER", columns["ByteValue"]);
        Assert.Equal("INTEGER", columns["BoolValue"]);
        Assert.Equal("REAL", columns["DoubleValue"]);
        Assert.Equal("REAL", columns["FloatValue"]);
        Assert.Equal("TEXT", columns["DecimalValue"]);
        Assert.Equal("TEXT", columns["StringValue"]);
        Assert.Equal("TEXT", columns["GuidValue"]);
        Assert.Equal("TEXT", columns["DateTimeValue"]);
        Assert.Equal("TEXT", columns["DateTimeOffsetValue"]);
        Assert.Equal("BLOB", columns["BlobValue"]);
        Assert.Equal("INTEGER", columns["EnumValue"]);
    }

    [Fact]
    public void EnsureTable_ValueTypeColumnsAreNotNull_ReferenceTypeColumnsAreNullable()
    {
        Store.EnsureTable<AllTypesModel>();

        var columns = Store.Query<PragmaColumn>("PRAGMA table_info([AllTypesModel]);")
            .ToDictionary(c => c.Name, c => c.NotNull, StringComparer.OrdinalIgnoreCase);

        // Non-nullable value types get NOT NULL.
        Assert.Equal(1, columns["LongValue"]);
        Assert.Equal(1, columns["BoolValue"]);
        Assert.Equal(1, columns["EnumValue"]);

        // Reference types are always nullable columns, even with a non-null default in C#.
        Assert.Equal(0, columns["StringValue"]);
        Assert.Equal(0, columns["BlobValue"]);

        // Nullable<T> value types are nullable columns.
        Assert.Equal(0, columns["NullableInt"]);
        Assert.Equal(0, columns["NullableDecimal"]);
    }

    [Fact]
    public void TableAttribute_OverridesTableName()
    {
        Store.EnsureTable<TableAttributeModel>();

        var tableNames = Store.Query<string>(
            "SELECT name FROM sqlite_master WHERE type = 'table';");

        Assert.Contains("CustomTableName", tableNames);
        Assert.DoesNotContain(nameof(TableAttributeModel), tableNames);
    }

    [Fact]
    public void Ignore_ExcludesPropertyFromSchema()
    {
        Store.EnsureTable<TableAttributeModel>();

        var columnNames = Store.Query<PragmaColumn>("PRAGMA table_info([CustomTableName]);")
            .Select(c => c.Name)
            .ToList();

        Assert.DoesNotContain("NotPersisted", columnNames);
        Assert.Contains("Name", columnNames);
    }

    [Fact]
    public void PrimaryKeyAttribute_MarksKeyColumn()
    {
        Store.EnsureTable<TableAttributeModel>();

        var columns = Store.Query<PragmaColumn>("PRAGMA table_info([CustomTableName]);").ToList();

        Assert.Equal(1, columns.Single(c => c.Name == "Code").Pk);
        Assert.Equal(0, columns.Single(c => c.Name == "Name").Pk);
    }

    [Fact]
    public void EnsureTable_NoKeyProperty_ThrowsWithTypeName()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Store.EnsureTable<NoKeyModel>());
        Assert.Contains(nameof(NoKeyModel), ex.Message);
    }
}
