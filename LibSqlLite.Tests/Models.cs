using LibSqlLite;

namespace LibSqlLite.Tests;

public enum SampleEnum
{
    None = 0,
    First = 1,
    Second = 2,
}

public class AllTypesModel
{
    public int Id { get; set; }
    public long LongValue { get; set; }
    public short ShortValue { get; set; }
    public byte ByteValue { get; set; }
    public bool BoolValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public decimal DecimalValue { get; set; }
    public string StringValue { get; set; } = "";
    public Guid GuidValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public byte[] BlobValue { get; set; } = [];
    public SampleEnum EnumValue { get; set; }

    public int? NullableInt { get; set; }
    public string? NullableString { get; set; }
    public Guid? NullableGuid { get; set; }
    public DateTime? NullableDateTime { get; set; }
    public decimal? NullableDecimal { get; set; }
}

[Table("CustomTableName")]
public class TableAttributeModel
{
    [PrimaryKey]
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    [Ignore]
    public string NotPersisted { get; set; } = "";
}

public class NoKeyModel
{
    public string Name { get; set; } = "";
}

[Table("MigrationModel")]
public class MigrationModelV1
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

[Table("MigrationModel")]
public class MigrationModelV2
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Extra { get; set; }
}

[Table("MigrationModel2")]
public class MigrationModel2V1
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

[Table("MigrationModel2")]
public class MigrationModel2V2
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public class ListPropertyModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<string> Tags { get; set; } = [];
}

public class IntKeyModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class LongKeyModel
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
}

public class GuidKeyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}

public class StringKeyModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

public class KeyOnlyModel
{
    [PrimaryKey]
    public string Code { get; set; } = "";
}

public class IdAndPrimaryKeyModel
{
    // Named "Id", but [PrimaryKey] on Code must win as the actual key.
    public int Id { get; set; }

    [PrimaryKey]
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";
}

public class UnsupportedKeyTypeModel
{
    public double Id { get; set; }
    public string Name { get; set; } = "";
}

public class ReflectionExclusionModel
{
    private readonly Dictionary<int, string> _items = new();

    public int Id { get; set; }
    public string Name { get; set; } = "";

    public string ReadOnlyProp => "computed";
    public string PrivateSetterProp { get; private set; } = "";
    public static string StaticProp { get; set; } = "";
    public object? UnsupportedTypeProp { get; set; }

    public string this[int index]
    {
        get => _items.TryGetValue(index, out var v) ? v : "";
        set => _items[index] = value;
    }
}

public class UniqueModel
{
    public int Id { get; set; }

    [Unique(IgnoreCase = true)]
    public string Name { get; set; } = "";

    [Unique]
    public string Code { get; set; } = "";
}

[Table("UniqueMigrationModel")]
public class UniqueMigrationModelV1
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

[Table("UniqueMigrationModel")]
public class UniqueMigrationModelV2
{
    public int Id { get; set; }

    [Unique(IgnoreCase = true)]
    public string Name { get; set; } = "";
}

public class UniqueKeyModel
{
    [PrimaryKey, Unique]
    public string Code { get; set; } = "";
}

internal sealed class PragmaColumn
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public int NotNull { get; set; }
    public int Pk { get; set; }
}
