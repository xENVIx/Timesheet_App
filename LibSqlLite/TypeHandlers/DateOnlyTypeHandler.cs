using System.Data;
using System.Globalization;
using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>
/// Stores <see cref="DateOnly"/> values as fixed-width ISO 8601 TEXT ("yyyy-MM-dd"), which sorts
/// chronologically and is understood by SQLite's date functions.
/// </summary>
internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public const string Format = "yyyy-MM-dd";

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString(Format, CultureInfo.InvariantCulture);
    }

    public override DateOnly Parse(object value) => value switch
    {
        // Rows from older schema versions may hold '' in a column added later as NOT NULL.
        string { Length: 0 } => default,
        string s => DateOnly.ParseExact(s, Format, CultureInfo.InvariantCulture),
        DateOnly d => d,
        _ => throw new InvalidCastException($"Cannot convert '{value}' ({value.GetType()}) to DateOnly."),
    };
}
