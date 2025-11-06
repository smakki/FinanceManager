using FinanceManager.CatalogService.Abstractions.Repositories;
using FinanceManager.CatalogService.Contracts.DTOs.AccountTypes;
using FinanceManager.CatalogService.Domain.Entities;
using FinanceManager.CatalogService.EntityFramework;
using FinanceManager.CatalogService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.CatalogService.Repositories.Implementations;

/// <summary>
/// Репозиторий для управления сущностями <see cref="AccountType"/>.
/// Предоставляет методы для фильтрации, получения всех типов счетов, проверки уникальности кода, проверки существования по коду и проверки возможности удаления.
/// </summary>
public class AccountTypeRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<AccountType, AccountTypeFilterDto>(context, logger), IAccountTypeRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
/// Инициализирует репозиторий набором типов счетов, если он пуст или частично заполнен.
/// </summary>
/// <param name="entities">Коллекция типов счетов для инициализации.</param>
/// <param name="cancellationToken">Токен отмены операции.</param>
/// <returns>Количество добавленных записей.</returns>
public async Task<int> InitializeAsync(IEnumerable<AccountType> entities,
    CancellationToken cancellationToken = default)
{
    _logger.Information("Начинается инициализация типов счетов");

    var accountTypesList = entities as ICollection<AccountType> ?? entities.ToList();
    _logger.Debug("Подготовлено {AccountTypeCount} типов счетов для инициализации", accountTypesList.Count);

    // Если таблица пуста — просто добавляем всё
    if (!await Entities.AnyAsync(cancellationToken))
    {
        _logger.Debug("Таблица типов счетов пуста, добавляем все типы");
        await Entities.AddRangeAsync(accountTypesList, cancellationToken);
        var result = await _context.CommitAsync(cancellationToken);
        _logger.Information("Инициализация завершена, добавлено {AddedCount} типов счетов", result);
        return result;
    }

    _logger.Debug("Таблица типов счетов содержит данные, проверяем уникальность кодов");
    var query = Entities.AsQueryable();
    var addedCount = 0;

    foreach (var entity in accountTypesList)
    {
        var exists = await query.AnyAsync(
            a => string.Equals(a.Code, entity.Code, StringComparison.InvariantCultureIgnoreCase),
            cancellationToken);

        if (!exists)
        {
            await Entities.AddAsync(entity, cancellationToken);
            addedCount++;
            _logger.Debug("Добавлен тип счёта: {AccountTypeCode} ({Description})",
                entity.Code, entity.Description);
        }
        else
        {
            _logger.Debug("Тип счёта {AccountTypeCode} уже существует, пропускаем",
                entity.Code);
        }
    }

    var commitResult = await _context.CommitAsync(cancellationToken);
    _logger.Information("Инициализация завершена, добавлено {AddedCount} новых типов счетов из {TotalCount}",
        addedCount, accountTypesList.Count);

    return commitResult;
}

    
    /// <summary>
    /// Применяет фильтры к запросу <see cref="AccountType"/> на основе переданного <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">DTO фильтра с критериями фильтрации.</param>
    /// <param name="query">Исходный запрос для применения фильтров.</param>
    /// <returns>Отфильтрованный <see cref="IQueryable{AccountType}"/>.</returns>
    private protected override IQueryable<AccountType> SetFilters(AccountTypeFilterDto filter,
        IQueryable<AccountType> query)
    {
        if (filter.Code != null)
        {
            query = filter.Code.Length > 0
                ? query.Where(a => a.Code.Contains(filter.Code))
                : query.Where(a => string.Equals(a.Code, string.Empty));
        }

        if (filter.DescriptionContains != null)
        {
            query = filter.DescriptionContains.Length > 0
                ? query.Where(a => a.Description.Contains(filter.DescriptionContains))
                : query.Where(a => string.Equals(a.Description, string.Empty));
        }

        return query;
    }

    /// <summary>
    /// Получает все сущности <see cref="AccountType"/>.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Коллекция типов счетов.</returns>
    public async Task<ICollection<AccountType>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Получение всех типов счетов");

        var accountTypes = await Entities.AsNoTracking().ToListAsync(cancellationToken);

        _logger.Information("Получено {AccountTypesCount} типов счетов", accountTypes.Count);

        return accountTypes;
    }

    /// <summary>
    /// Проверяет уникальность кода типа счета.
    /// </summary>
    /// <param name="code">Код для проверки.</param>
    /// <param name="excludeId">Необязательный идентификатор типа счета, который нужно исключить из проверки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>True, если код уникален; иначе — false.</returns>
    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Проверка уникальности кода типа счёта '{Code}', исключая тип {ExcludeId}",
            code, excludeId);

        var isUnique = await IsUniqueAsync(Entities.AsQueryable(),
            predicate: a => string.Equals(a.Code, code, StringComparison.InvariantCultureIgnoreCase),
            excludeId, cancellationToken);

        _logger.Debug("Код типа счёта '{Code}' {UniqueResult}",
            code, isUnique ? "уникален" : "не уникален");

        return isUnique;
    }

    /// <summary>
    /// Проверяет существование типа счета по коду.
    /// </summary>
    /// <param name="code">Код типа счета.</param>
    /// <param name="includeDeleted">Включать ли удалённые типы счетов.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>True, если тип счета существует; иначе — false.</returns>
    public async Task<bool> ExistsByCodeAsync(string code, bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Проверка существования типа счёта по коду '{Code}'. Включить удалённые: {IncludeDeleted}",
            code, includeDeleted);

        var query = Entities.Where(a => a.Code == code);
        if (!includeDeleted)
            query = query.Where(a => !a.IsDeleted);
        
        var exists = await query.AnyAsync(cancellationToken);
        
        _logger.Debug("Тип счёта с кодом '{Code}' {ExistsResult}", 
            code, exists ? "существует" : "не существует");

        return exists;
    }

    /// <summary>
    /// Определяет, может ли тип счета быть удалён (т.е. не используется ни одним счетом).
    /// </summary>
    /// <param name="id">Идентификатор типа счета для проверки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>True, если тип счета можно удалить; иначе — false.</returns>
    public async Task<bool> CanBeDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.Information("Проверка возможности удаления типа счёта {AccountTypeId}", id);
        
        var canBeDeleted = !await _context.Accounts.AnyAsync(a => a.AccountTypeId == id, cancellationToken);
        
        _logger.Information("Тип счёта {AccountTypeId} {DeletionResult}", 
            id, canBeDeleted ? "может быть удалён" : "не может быть удалён (используется в счетах)");
        
        return canBeDeleted;
    }
}