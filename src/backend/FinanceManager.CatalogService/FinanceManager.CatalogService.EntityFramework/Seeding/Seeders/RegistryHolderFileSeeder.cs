using FinanceManager.CatalogService.Abstractions.Repositories;
using FinanceManager.CatalogService.Domain.Entities;
using FinanceManager.CatalogService.EntityFramework.Seeding.Abstractions;
using Serilog;

namespace FinanceManager.CatalogService.EntityFramework.Seeding.Seeders;


public class RegistryHolderFileSeeder(
    IRegistryHolderRepository registryHolderRepository,
    ISeedingEntitiesProducer<RegistryHolder> seedingProducer,
    ILogger logger) : FileDataSeederBase<RegistryHolder>(seedingProducer, logger), IDataSeeder
{
    private const string RegistryHolderSeedingJsonFileName = "holders.json";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedDataAsync(registryHolderRepository, RegistryHolderSeedingJsonFileName, cancellationToken);
    }
}