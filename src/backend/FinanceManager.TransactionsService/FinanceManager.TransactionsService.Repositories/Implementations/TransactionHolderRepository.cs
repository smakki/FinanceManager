using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionHolders;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с владельцами транзакций.
/// </summary>
public class TransactionHolderRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<TransactionHolder, TransactionHolderFilterDto>(context, logger),
        ITransactionHolderRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Применяет фильтры к запросу владельцев.
    /// </summary>
    protected override IQueryable<TransactionHolder> SetFilters(
        TransactionHolderFilterDto filter,
        IQueryable<TransactionHolder> query)
    {
        if (filter.TelegramId.HasValue)
            query = query.Where(h => h.TelegramId == filter.TelegramId);

        if (filter.Role.HasValue)
            query = query.Where(h => h.Role == filter.Role);

        return query;
    }
}