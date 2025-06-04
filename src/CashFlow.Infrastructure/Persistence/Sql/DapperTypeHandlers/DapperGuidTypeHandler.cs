using System.Data;
using Dapper;

namespace CashFlow.Infrastructure.Persistence.Sql.DapperTypeHandlers;

public class DapperGuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return new Guid(value.ToString());
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }
}