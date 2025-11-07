using FinanceManager.CatalogService.Abstractions.Repositories;
using FinanceManager.CatalogService.Domain.Entities;
using FinanceManager.CatalogService.EntityFramework.Seeding.Abstractions;
using Serilog;

namespace FinanceManager.CatalogService.EntityFramework.Seeding.Seeders;

/// <summary>
/// Сидер категорий. Загружает данные категорий из JSON-файла в репозиторий.
/// </summary>
public class CategoryFileSeeder(
    ICategoryRepository categoryRepository,
    ISeedingEntitiesProducer<Category> seedingProducer,
    ILogger logger)
    : FileDataSeederBase<Category>(seedingProducer, logger), IDataSeeder
{
    private const string CategorySeedingJsonFileName = "categories.json";

    /// <summary>
    /// Выполняет загрузку данных категорий.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedDataAsync(categoryRepository, CategorySeedingJsonFileName, cancellationToken);
    }
}