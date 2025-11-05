using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionsCategories;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с категориями транзакций.
/// Предоставляет методы фильтрации, проверки уникальности имени, проверки валидности смены родителя.
/// </summary>
public class TransactionCategoryRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<TransactionsCategory, TransactionCategoryFilterDto>(context, logger),
      ITransactionCategoryRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Включает связанные сущности для категории.
    /// </summary>
    protected override IQueryable<TransactionsCategory> IncludeRelatedEntities(
        IQueryable<TransactionsCategory> query)
    {
        return query
            .Include(c => c.Holder);
    }

    /// <summary>
    /// Применяет фильтры к запросу категорий.
    /// </summary>
    protected override IQueryable<TransactionsCategory> SetFilters(
        TransactionCategoryFilterDto filter,
        IQueryable<TransactionsCategory> query)
    {
        if (filter.TransactionHolderId.HasValue)
            query = query.Where(c => c.HolderId == filter.TransactionHolderId);
        
        if (filter.Income.HasValue)
            query = query.Where(c => c.Income == filter.Income.Value);

        if (filter.Expense.HasValue)
            query = query.Where(c => c.Expense == filter.Expense);

        return query;
    }

    /// <summary>
    /// Получает коллекцию категорий по идентификатору владельца.
    /// </summary>
    public async Task<ICollection<TransactionsCategory>> GetByHolderIdAsync(
        Guid holderId,
        bool includeRelated = true,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Получение категорий для владельца {HolderId}", holderId);

        var query = Entities.AsQueryable().Where(c => c.HolderId == holderId);

        if (includeRelated)
            query = IncludeRelatedEntities(query);

        var categories = await query.ToListAsync(cancellationToken);

        _logger.Information("Найдено {Count} категорий для владельца {HolderId}",
            categories.Count, holderId);

        return categories;
    }
    
}
