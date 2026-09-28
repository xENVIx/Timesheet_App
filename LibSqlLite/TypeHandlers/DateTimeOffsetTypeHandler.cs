using System.Data;
using System.Globalization;
using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>Stores <see cref="DateTimeOffset"/> values as ISO 8601 round-trip ("O") TEXT.</summary>
internal sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("O", CultureInfo.InvariantCulture);
    }

    public override DateTimeOffset Parse(object value) => value switch
    {
        string s => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        DateTimeOffset dto => dto,
        _ => throw new InvalidCastException($"Cannot convert '{value}' ({value.GetType()}) to DateTimeOffset."),
    };
}
