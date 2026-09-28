using System.Data;
using Dapper;

namespace LibSqlLite.TypeHandlers;

/// <summary>Stores <see cref="Guid"/> values as lowercase "D"-format TEXT.</summary>
internal sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("D");
    }

    public override Guid Parse(object value) => value switch
    {
        string s => Guid.Parse(s),
        Guid g => g,
        _ => throw new InvalidCastException($"Cannot convert '{value}' ({value.GetType()}) to Guid."),
    };
}
