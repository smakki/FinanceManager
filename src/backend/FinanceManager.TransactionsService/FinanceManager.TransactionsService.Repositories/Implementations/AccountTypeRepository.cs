using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.AccountTypes;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с типами счетов в системе транзакций.
/// </summary>
public class AccountTypeRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<TransactionsAccountType, AccountTypeFilterDto>(context, logger),
      IAccountTypeRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Применяет фильтры к запросу типов счетов.
    /// </summary>
    protected override IQueryable<TransactionsAccountType> SetFilters(
        AccountTypeFilterDto filter,
        IQueryable<TransactionsAccountType> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.DescriptionContains))
            query = query.Where(t => t.Description.Contains(filter.DescriptionContains));

        if (!string.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(t => t.Code.Contains(filter.Code));

        return query;
    }

    /// <summary>
    /// Проверяет уникальность кода типа счета
    /// </summary>
    public async Task<bool> IsCodeUniqueAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Entities.AsNoTracking();

        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);

        var exists = await query.AnyAsync(t => t.Code == code, cancellationToken);

        _logger.Information("Проверка уникальности кода типа счета {Code}: {IsUnique}",
            code, !exists ? "уникален" : "уже существует");

        return !exists;
    }
}
