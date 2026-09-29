using System.Data;
using System.Globalization;
using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>Stores <see cref="decimal"/> values as invariant-culture TEXT to preserve precision.</summary>
internal sealed class DecimalTypeHandler : SqlMapper.TypeHandler<decimal>
{
    public override void SetValue(IDbDataParameter parameter, decimal value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString(CultureInfo.InvariantCulture);
    }

    public override decimal Parse(object value) => value switch
    {
        // Rows from older schema versions may hold '' in a column added later as NOT NULL.
        string { Length: 0 } => default,
        string s => decimal.Parse(s, CultureInfo.InvariantCulture),
        decimal d => d,
        double d => (decimal)d,
        _ => throw new InvalidCastException($"Cannot convert '{value}' ({value.GetType()}) to decimal."),
    };
}
