using FinanceManager.CatalogService.Abstractions.Repositories;
using FinanceManager.CatalogService.Domain.Entities;
using FinanceManager.CatalogService.EntityFramework.Seeding.Abstractions;
using Serilog;

namespace FinanceManager.CatalogService.EntityFramework.Seeding.Seeders;

/// <summary>
/// Сидер типов счетов. Загружает данные типов счетов из JSON-файла в репозиторий.
/// </summary>
public class AccountTypeFileSeeder(
    IAccountTypeRepository accountTypeRepository,
    ISeedingEntitiesProducer<AccountType> seedingProducer,
    ILogger logger)
    : FileDataSeederBase<AccountType>(seedingProducer, logger), IDataSeeder
{
    private const string AccountTypeSeedingJsonFileName = "account_types.json";

    /// <summary>
    /// Выполняет загрузку данных типов счетов.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedDataAsync(accountTypeRepository, AccountTypeSeedingJsonFileName, cancellationToken);
    }
}