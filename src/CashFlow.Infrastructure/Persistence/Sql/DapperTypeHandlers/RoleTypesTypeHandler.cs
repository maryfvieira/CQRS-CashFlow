using Dapper;
using System;
using System.Data;
using CashFlow.CrossCutting;

namespace CashFlow.Infrastructure.Persistence.Sql.DapperTypeHandlers;

public class RoleTypesTypeHandler : SqlMapper.TypeHandler<RoleTypes>
{
    public override RoleTypes Parse(object value)
    {
        // Converte o valor do banco (string ou int) para o enum
        return value switch
        {
            string stringValue => Enum.Parse<RoleTypes>(stringValue, ignoreCase: true),
            int intValue => (RoleTypes)intValue,
            _ => throw new NotSupportedException($"Tipo {value.GetType()} não suportado")
        };
    }

    public override void SetValue(IDbDataParameter parameter, RoleTypes value)
    {
        // Define como o enum será salvo no banco (opcional se você só faz leitura)
        parameter.Value = value.ToString();
    }
}