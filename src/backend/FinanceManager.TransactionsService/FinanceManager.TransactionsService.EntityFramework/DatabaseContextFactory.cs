using FinanceManager.TransactionsService.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanceManager.TransactionsService.EntityFramework;

/// <summary>
/// Используется EF Core для создания DatabaseContext на этапе design-time (например, при создании миграций).
/// </summary>

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args)
    {
        var dbSettings = new DbSettings
        {
            Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost",
            Port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432",
            Database = Environment.GetEnvironmentVariable("DB_NAME") ?? "finance_db",
            Username = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres",
            Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "postgres"
        };

        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder.UseNpgsql(dbSettings.GetConnectionString());

        return new DatabaseContext(optionsBuilder.Options);
    }
    
}