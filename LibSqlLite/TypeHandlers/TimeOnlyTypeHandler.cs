using System.Data;
using System.Globalization;
using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>
/// Stores <see cref="TimeOnly"/> values as fixed-width TEXT ("HH:mm:ss.fffffff"): full tick precision,
/// sorts chronologically, and is understood by SQLite's time functions.
/// </summary>
internal sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public const string Format = "HH:mm:ss.fffffff";

    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString(Format, CultureInfo.InvariantCulture);
    }

    public override TimeOnly Parse(object value) => value switch
    {
        // Rows from older schema versions may hold '' in a column added later as NOT NULL.
        string { Length: 0 } => default,
        string s => TimeOnly.ParseExact(s, Format, CultureInfo.InvariantCulture),
        TimeOnly t => t,
        _ => throw new InvalidCastException($"Cannot convert '{value}' ({value.GetType()}) to TimeOnly."),
    };
}
