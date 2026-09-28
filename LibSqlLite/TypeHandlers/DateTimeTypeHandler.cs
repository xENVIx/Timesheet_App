using System.Data;
using System.Globalization;
using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>Stores <see cref="DateTime"/> values as UTC, ISO 8601 round-trip ("O") TEXT.</summary>
internal sealed class DateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }

    public override DateTime Parse(object value) => value switch
    {
        string s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
        DateTime dt => dt,
        _ => throw new InvalidCastException($"Cannot convert '{value}' ({value.GetType()}) to DateTime."),
    };
}
