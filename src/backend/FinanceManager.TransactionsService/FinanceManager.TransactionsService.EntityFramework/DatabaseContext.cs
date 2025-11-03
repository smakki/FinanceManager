using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.TransactionsService.EntityFramework;

public class DatabaseContext(DbContextOptions<DatabaseContext> options):DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Счета.
    /// </summary>
    public DbSet<TransactionsAccount> Accounts { get; set; }
    
    /// <summary>
    /// Типы счетов.
    /// </summary>
    public DbSet<TransactionsAccountType> AccountTypes { get; set; }
    
    /// <summary>
    /// Категории.
    /// </summary>
    public DbSet<TransactionsCategory> Categories { get; set; }
    
    /// <summary>
    /// Валюты.
    /// </summary>
    public DbSet<TransactionsCurrency> Currencies { get; set; }
    
    /// <summary>
    /// Владельцы справочников.
    /// </summary>
    public DbSet<TransactionHolder> RegistryHolders { get; set; }

    /// <summary>
    /// Курсы валют.
    /// </summary>
    public DbSet<Transaction> Transactions { get; set; }
    
    /// <summary>
    /// Курсы валют.
    /// </summary>
    public DbSet<Transfer> Transfers { get; set; }
    
    /// <summary>
    /// Применяет конфигурации моделей при создании схемы базы данных.
    /// </summary>
    /// <param name="modelBuilder">Построитель моделей.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
    
    /// <summary>
    /// Сохраняет все изменения в базе данных в рамках текущей транзакции.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>Количество затронутых записей.</returns>
    public async Task<int> CommitAsync(CancellationToken cancellationToken) =>
        await SaveChangesAsync(cancellationToken);
}