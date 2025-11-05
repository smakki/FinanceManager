using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.Transactions;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с транзакциями.
/// Предоставляет методы фильтрации, получения по владельцу, проверки принадлежности.
/// </summary>
public class TransactionRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<Transaction, TransactionFilterDto>(context, logger),
      ITransactionRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Включает связанные сущности для транзакции.
    /// </summary>
    protected override IQueryable<Transaction> IncludeRelatedEntities(
        IQueryable<Transaction> query)
    {
        return query
            .Include(t => t.Account)
            .Include(t => t.Category);
    }

    /// <summary>
    /// Применяет фильтры к запросу транзакций.
    /// </summary>
    protected override IQueryable<Transaction> SetFilters(
        TransactionFilterDto filter,
        IQueryable<Transaction> query)
    {
        if (filter.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filter.AccountId);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId);
        
        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo);

        if (filter.AmountFrom.HasValue)
            query = query.Where(t => t.Amount >= filter.AmountFrom);

        if (filter.AmountTo.HasValue)
            query = query.Where(t => t.Amount <= filter.AmountTo);

        return query;
    }
    
    /// <summary>
    /// Получает общее количество транзакций по фильтру
    /// </summary>
    public async Task<int> GetCountAsync(
        TransactionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Получение количества транзакций по фильтру");

        var query = Entities.AsNoTracking();
        query = SetFilters(filter, query);

        var count = await query.CountAsync(cancellationToken);

        _logger.Information("Количество транзакций: {Count}", count);

        return count;
    }
}
