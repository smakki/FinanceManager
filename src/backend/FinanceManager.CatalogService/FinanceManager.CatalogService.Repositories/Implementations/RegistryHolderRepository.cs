using FinanceManager.CatalogService.Abstractions.Repositories;
using FinanceManager.CatalogService.Contracts.DTOs.RegistryHolders;
using FinanceManager.CatalogService.Domain.Entities;
using FinanceManager.CatalogService.EntityFramework;
using FinanceManager.CatalogService.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.CatalogService.Repositories.Implementations;

/// <summary>
/// Репозиторий для работы с владельцами справочников (RegistryHolder).
/// Предоставляет методы фильтрации, проверки уникальности TelegramId и возможности удаления.
/// </summary>
public class RegistryHolderRepository(DatabaseContext context, ILogger logger)
    : BaseRepository<RegistryHolder, RegistryHolderFilterDto>(context, logger), IRegistryHolderRepository
{
    private readonly DatabaseContext _context = context;
    private readonly ILogger _logger = logger;

    /// <summary>
/// Инициализирует репозиторий владельцев справочников набором сущностей, если они ещё не существуют.
/// </summary>
/// <param name="entities">Коллекция владельцев для инициализации.</param>
/// <param name="cancellationToken">Токен отмены операции.</param>
/// <returns>Количество добавленных записей.</returns>
public async Task<int> InitializeAsync(IEnumerable<RegistryHolder> entities, 
    CancellationToken cancellationToken = default)
{
    _logger.Information("Начинается инициализация владельцев реестров");

    var holdersList = entities as ICollection<RegistryHolder> ?? entities.ToList();
    _logger.Debug("Подготовлено {HoldersCount} владельцев для инициализации", holdersList.Count);

    if (!await Entities.AnyAsync(cancellationToken))
    {
        _logger.Debug("Таблица владельцев пуста, добавляем всех");
        await Entities.AddRangeAsync(holdersList, cancellationToken);
        var result = await _context.CommitAsync(cancellationToken);
        _logger.Information("Инициализация завершена, добавлено {AddedCount} владельцев", result);
        return result;
    }

    _logger.Debug("Таблица содержит данные, проверяем уникальность по TelegramId");
    var query = Entities.AsQueryable();
    var addedCount = 0;

    foreach (var holder in holdersList)
    {
        if (!await query.AnyAsync(
                rh => rh.TelegramId == holder.TelegramId, 
                cancellationToken))
        {
            await Entities.AddAsync(holder, cancellationToken);
            addedCount++;
            _logger.Debug("Добавлен владелец: TelegramId={TelegramId}, Role={Role}", 
                holder.TelegramId, holder.Role);
        }
        else
        {
            _logger.Debug("Владелец с TelegramId={TelegramId} уже существует, пропускаем", 
                holder.TelegramId);
        }
    }

    var commitResult = await _context.CommitAsync(cancellationToken);
    _logger.Information("Инициализация завершена, добавлено {AddedCount} новых владельцев из {TotalCount}", 
        addedCount, holdersList.Count);

    return commitResult;
}

    
    /// <summary>
    /// Применяет фильтры к запросу владельцев справочников.
    /// </summary>
    /// <param name="filter">Фильтр владельцев справочников.</param>
    /// <param name="query">Исходный запрос.</param>
    /// <returns>Запрос с применёнными фильтрами.</returns>
    private protected override IQueryable<RegistryHolder> SetFilters(RegistryHolderFilterDto filter,
        IQueryable<RegistryHolder> query)
    {
        if (filter.TelegramId.HasValue)
        {
            query = query.Where(rh => rh.TelegramId == filter.TelegramId.Value);
        }

        if (filter.Role.HasValue)
        {
            query = query.Where(rh => rh.Role == filter.Role.Value);
        }

        return query;
    }

    /// <summary>
    /// Проверяет уникальность TelegramId среди владельцев справочников.
    /// </summary>
    /// <param name="telegramId">Telegram ID для проверки.</param>
    /// <param name="excludeId">Идентификатор владельца, которого нужно исключить из проверки (опционально).</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>True, если TelegramId уникален, иначе false.</returns>
    public async Task<bool> IsTelegramIdUniqueAsync(long telegramId, Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var isUnique = await IsUniqueAsync(Entities.AsQueryable(),
            predicate: rh => rh.TelegramId == telegramId,
            excludeId, cancellationToken);

        _logger.Information("Проверка уникальности Telegram ID: {TelegramId} является {IsUnique}",
            telegramId, isUnique ? "уникальным" : "неуникальным");

        return isUnique;
    }

    /// <summary>
    /// Проверяет, может ли владелец справочника быть удалён (нет связанных категорий и счетов).
    /// </summary>
    /// <param name="id">Идентификатор владельца справочника.</param>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    /// <returns>True, если владелец может быть удалён, иначе false.</returns>
    public async Task<bool> CanBeDeletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hasCategories = await _context.Categories.AnyAsync(c => c.RegistryHolderId == id, cancellationToken);
        if (hasCategories)
        {
            _logger.Information(
                "Владелец справочника {RegistryHolderId} не может быть удален: имеет связанные категории", id);
            return false;
        }

        var hasAccounts = await _context.Accounts.AnyAsync(a => a.RegistryHolderId == id, cancellationToken);
        if (hasAccounts)
        {
            _logger.Information("Владелец справочника {RegistryHolderId} не может быть удален: имеет связанные счета", id);
            return false;
        }
        
        _logger.Information("Владелец справочника {RegistryHolderId} может быть удален", id);
        return true;
    }
}