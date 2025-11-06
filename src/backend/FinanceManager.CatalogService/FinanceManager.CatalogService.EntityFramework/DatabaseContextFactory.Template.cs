
namespace FinanceManager.TransactionsService.EntityFramework;

/// <summary>
/// Используется EF Core для создания DatabaseContext на этапе design-time (например, при создании миграций).
/// </summary>


// public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
// {
//     public DatabaseContext CreateDbContext(string[] args)
//     {
//         var dbSettings = new DbSettings
//         {
//             Host = Environment.GetEnvironmentVariable("DB_HOST") ?? "",
//             Port = Environment.GetEnvironmentVariable("DB_PORT") ?? "",
//             Database = Environment.GetEnvironmentVariable("DB_NAME") ?? "",
//             Username = Environment.GetEnvironmentVariable("DB_USER") ?? "",
//             Password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? ""
//         };
//
//         var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
//         optionsBuilder.UseNpgsql(dbSettings.GetConnectionString());
//
//         return new DatabaseContext(optionsBuilder.Options);
//     }
//     
// }