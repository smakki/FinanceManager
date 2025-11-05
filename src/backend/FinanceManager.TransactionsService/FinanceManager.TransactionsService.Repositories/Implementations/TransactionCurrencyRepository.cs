using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionCurrencies;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.EntityFramework;
using FinanceManager.TransactionsService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с валютами в системе транзакций.
/// </summary>
public class TransactionCurrencyRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<TransactionsCurrency, TransactionCurrencyFilterDto>(context, logger),
      ITransactionCurrencyRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Применяет фильтры к запросу валют.
    /// </summary>
    protected override IQueryable<TransactionsCurrency> SetFilters(
        TransactionCurrencyFilterDto filter,
        IQueryable<TransactionsCurrency> query)
    {
        if (!string.IsNullOrWhiteSpace(filter.NumCode))
            query = query.Where(c => c.NumCode.Contains(filter.NumCode));

        if (!string.IsNullOrWhiteSpace(filter.CharCode))
            query = query.Where(c => c.CharCode.Contains(filter.CharCode));

        return query;
    }

    /// <summary>
    /// Получает валюту по буквенному коду
    /// </summary>
    public async Task<TransactionsCurrency?> GetByCharCodeAsync(
        string charCode,
        CancellationToken cancellationToken = default)
    {
        _logger.Information("Получение валюты по коду {CharCode}", charCode);

        var currency = await Entities.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CharCode == charCode, cancellationToken);

        if (currency != null)
            _logger.Information("Найдена валюта {CurrencyId} с кодом {CharCode}", currency.Id, charCode);
        else
            _logger.Warning("Валюта с кодом {CharCode} не найдена", charCode);

        return currency;
    }

    /// <summary>
    /// Проверяет уникальность буквенного кода
    /// </summary>
    public async Task<bool> IsCharCodeUniqueAsync(
        string charCode,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Entities.AsNoTracking();

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        var exists = await query.AnyAsync(c => c.CharCode == charCode, cancellationToken);

        _logger.Information("Проверка уникальности кода {CharCode}: {IsUnique}",
            charCode, !exists ? "уникален" : "уже существует");

        return !exists;
    }
}
