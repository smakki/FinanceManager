using FinanceManager.CatalogService.Abstractions.Repositories;
using FinanceManager.CatalogService.Domain.Entities;
using FinanceManager.CatalogService.EntityFramework.Seeding.Abstractions;
using Serilog;

namespace FinanceManager.CatalogService.EntityFramework.Seeding.Seeders;

/// <summary>
/// Сидер счетов. Загружает данные счетов из JSON-файла в репозиторий.
/// </summary>
public class AccountFileSeeder(
    IAccountRepository accountRepository,
    ISeedingEntitiesProducer<Account> seedingProducer,
    ILogger logger)
    : FileDataSeederBase<Account>(seedingProducer, logger), IDataSeeder
{
    private const string AccountSeedingJsonFileName = "accounts.json";

    /// <summary>
    /// Выполняет загрузку данных счетов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedDataAsync(accountRepository, AccountSeedingJsonFileName, cancellationToken);
    }
}