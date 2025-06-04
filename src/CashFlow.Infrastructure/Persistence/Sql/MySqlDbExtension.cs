using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.DapperTypeHandlers;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using CashFlow.Infrastructure.Persistence.Sql.Repository;
using CashFlow.Infrastructure.Settings;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure.Persistence.Sql;

public static class MySqlDbExtension
{
    public static IServiceCollection AddSqlWriteModelPersistence(this IServiceCollection services)
    {
        services.AddSqlPersistence();

        services
            .AddScoped<IAccountBalanceRepository, AccountBalanceRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>()
            .AddScoped<IConsolidatedReportRepository, ConsolidatedReportRepository>();

        return services;
    }

    public static IServiceCollection AddSqlReadModelPersistence(this IServiceCollection services)
    {
        services.AddSqlPersistence();
        services.AddScoped<IConsolidatedReportRepository, ConsolidatedReportRepository>();

        return services;
    }

    public static IServiceCollection AddSqlAuthModelPersistence(this IServiceCollection services)
    {
        services.AddSqlPersistence();
        
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }

    private static IServiceCollection AddSqlPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory>(provider =>
            new DbConnectionFactory(provider.GetRequiredService<IAppSettings>().DatabaseSettings.ConnectionString));

        SqlMapper.AddTypeHandler(new DapperGuidTypeHandler());
        SqlMapper.AddTypeHandler(new RoleTypesTypeHandler());
        
        return services;
    }
}