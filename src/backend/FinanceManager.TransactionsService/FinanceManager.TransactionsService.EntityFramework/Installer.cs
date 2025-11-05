using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FinanceManager.TransactionsService.EntityFramework;


/// <summary>
/// Предоставляет методы для регистрации сервисов и настроек базы данных в DI-контейнере.
/// </summary>
public static class Installer
{
    /// <summary>
    /// Регистрирует контекст базы данных и связанные настройки в контейнере зависимостей.
    /// Позволяет опционально включить логирование чувствительных данных (EnableSensitiveDataLogging).
    /// </summary>
    /// <param name="services">Коллекция сервисов для регистрации.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <param name="enableSensitiveLogging">Включить логирование чувствительных данных (по умолчанию false).</param>
    /// <returns>Коллекция сервисов с добавленными зависимостями для работы с базой данных.</returns>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration,
        bool enableSensitiveLogging = false)
    {
        services.Configure<DbSettings>(configuration.GetSection(nameof(DbSettings)));
        services.AddDbContext<IUnitOfWork, DatabaseContext>((provider, options) =>
        {
            var dbSettings = provider.GetRequiredService<IOptions<DbSettings>>().Value;
            options.UseNpgsql(dbSettings.GetConnectionString());

            if (enableSensitiveLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
    
    /// <summary>
    /// Применяет все ожидающие миграции базы данных для контекста DatabaseContext
    /// </summary>
    /// <param name="host">Экземпляр IHost, содержащий конфигурацию приложения и сервисы</param>
    /// <returns>
    /// Возвращает тот же экземпляр IHost для поддержки цепочки вызовов.
    /// Задача завершается после применения всех миграций.
    /// </returns>
    public static async Task<IHost> UseMigrationAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await dbContext.Database.MigrateAsync();
        return host;
    }
}