using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.DataBase;

public static class DatabaseMigrator
{
    public static void Migrate(string connectionString)
    {
        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddMySql5() // Ou outro provedor
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(DatabaseMigrator).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        
        if (runner.HasMigrationsToApplyUp())
        {
            runner.MigrateUp();
        }
    }
}