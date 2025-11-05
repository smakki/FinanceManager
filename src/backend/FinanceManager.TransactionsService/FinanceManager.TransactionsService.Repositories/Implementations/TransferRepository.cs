using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.Transfers;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с переводами между счетами.
/// Предоставляет методы фильтрации, получения по владельцу, проверки принадлежности.
/// </summary>
public class TransferRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<Transfer, TransferFilterDto>(context, logger),
      ITransferRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Включает связанные сущности для перевода.
    /// </summary>
    protected override IQueryable<Transfer> IncludeRelatedEntities(
        IQueryable<Transfer> query)
    {
        return query
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .Include(t => t.FromAccount.Holder);
    }

    /// <summary>
    /// Применяет фильтры к запросу переводов.
    /// </summary>
    protected override IQueryable<Transfer> SetFilters(
        TransferFilterDto filter,
        IQueryable<Transfer> query)
    {
        if (filter.FromAccountId.HasValue)
            query = query.Where(t => t.FromAccountId == filter.FromAccountId);

        if (filter.ToAccountId.HasValue)
            query = query.Where(t => t.ToAccountId == filter.ToAccountId);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= filter.DateFrom);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.CreatedAt <= filter.DateTo);

        if (filter.FromAmountFrom.HasValue)
            query = query.Where(t => t.FromAmount >= filter.FromAmountFrom);
        if (filter.FromAmountTo.HasValue)
            query = query.Where(t => t.FromAmount <= filter.FromAmountTo);
        if (filter.ToAmountFrom.HasValue)
            query = query.Where(t => t.ToAmount >= filter.ToAmountFrom);
        if (filter.ToAmountTo.HasValue)
            query = query.Where(t => t.ToAmount <= filter.ToAmountTo);
        
        if (!string.IsNullOrWhiteSpace(filter.DescriptionContains))
            query = query.Where(t => t.Description.Contains(filter.DescriptionContains));

        return query;
    }

    /// <summary>
    /// Получает список переводов по владельцу (через счёт)
    /// </summary>
    public async Task<ICollection<Transfer>> GetByHolderIdAsync(
        Guid holderId,
        bool includeRelated = true,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Получение переводов для владельца {HolderId}", holderId);

        var query = Entities.AsQueryable()
            .Where(t => t.FromAccount.HolderId == holderId);

        if (includeRelated)
            query = IncludeRelatedEntities(query);

        var transfers = await query.ToListAsync(cancellationToken);

        _logger.Information("Найдено {Count} переводов для владельца {HolderId}",
            transfers.Count, holderId);

        return transfers;
    }

    /// <summary>
    /// Проверяет, принадлежит ли перевод указанному пользователю
    /// </summary>
    public async Task<bool> BelongsToUserAsync(
        Guid transferId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Проверка принадлежности перевода {TransferId} пользователю {UserId}",
            transferId, userId);

        var belongs = await Entities.AsNoTracking()
            .AnyAsync(t => t.Id == transferId && t.FromAccount.HolderId == userId, cancellationToken);

        _logger.Information("Перевод {TransferId} пользователю {UserId}: {Belongs}",
            transferId, userId, belongs ? "принадлежит" : "не принадлежит");

        return belongs;
    }

    /// <summary>
    /// Получает общее количество переводов по фильтру
    /// </summary>
    public async Task<int> GetCountAsync(
        TransferFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Получение количества переводов по фильтру");

        var query = Entities.AsNoTracking();
        query = SetFilters(filter, query);

        var count = await query.CountAsync(cancellationToken);

        _logger.Information("Количество переводов: {Count}", count);

        return count;
    }
}
