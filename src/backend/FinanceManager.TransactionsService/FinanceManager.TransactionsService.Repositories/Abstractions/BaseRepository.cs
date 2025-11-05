using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Domain.Abstractions;
using FinanceManager.TransactionsService.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace FinanceManager.TransactionsService.Repositories.Abstractions;

/// <summary>
/// Абстрактный базовый репозиторий, предоставляющий стандартные операции CRUD и фильтрацию.
/// </summary>
/// <typeparam name="TEntity">Тип сущности</typeparam>
/// <typeparam name="TFilter">Тип DTO фильтра</typeparam>
public abstract class BaseRepository<TEntity, TFilter> : IBaseRepository<TEntity, TFilter>
    where TEntity : IdentityModel
    where TFilter : class
{
    protected readonly DatabaseContext Context;
    protected readonly ILogger Logger;
    protected readonly DbSet<TEntity> Entities;

    /// <summary>
    /// Конструктор базового репозитория
    /// </summary>
    protected BaseRepository(DatabaseContext context, ILogger logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Entities = context.Set<TEntity>();
    }

    /// <summary>
    /// Проверяет, существует ли сущность по ID
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Logger.Debug("Проверка существования сущности {EntityType} с ID {Id}", 
            typeof(TEntity).Name, id);

        var exists = await Entities.AsNoTracking()
            .AnyAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);

        Logger.Debug("Сущность {EntityType} с ID {Id}: {Exists}",
            typeof(TEntity).Name, id, exists ? "найдена" : "не найдена");

        return exists;
    }

    /// <summary>
    /// Получает сущность по ID
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id,
        bool includeRelated = true,
        bool disableTracking = false,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Получение сущности {EntityType} по ID {Id}", 
            typeof(TEntity).Name, id);

        var query = Entities.AsQueryable();

        if (disableTracking)
            query = query.AsNoTracking();

        if (includeRelated)
            query = IncludeRelatedEntities(query);

        var entity = await query
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);

        if (entity != null)
            Logger.Information("Сущность {EntityType} с ID {Id} найдена", 
                typeof(TEntity).Name, id);
        else
            Logger.Warning("Сущность {EntityType} с ID {Id} не найдена", 
                typeof(TEntity).Name, id);

        return entity;
    }

    /// <summary>
    /// Получает список сущностей с фильтрацией и пагинацией
    /// </summary>
    public virtual async Task<ICollection<TEntity>> GetPagedAsync(
        TFilter filter,
        bool includeRelated = true,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Получение страницы сущностей {EntityType}", typeof(TEntity).Name);

        var query = Entities.AsNoTracking();

        // Применяем фильтры
        query = SetFilters(filter, query);

        // Включаем связанные сущности
        if (includeRelated)
            query = IncludeRelatedEntities(query);

        // Применяем пагинацию если фильтр имеет свойства Page и ItemsPerPage
        var filterType = filter?.GetType();
        var pageProperty = filterType?.GetProperty("Page");
        var itemsPerPageProperty = filterType?.GetProperty("ItemsPerPage");

        if (pageProperty is not null && itemsPerPageProperty is not null)
        {
            var page = (int?)pageProperty.GetValue(filter) ?? 1;
            var itemsPerPage = (int?)itemsPerPageProperty.GetValue(filter) ?? 10;

            query = query
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage);
        }

        var entities = await query.ToListAsync(cancellationToken);

        Logger.Information("Получено {Count} сущностей {EntityType}",
            entities.Count, typeof(TEntity).Name);

        return entities;
    }

    /// <summary>
    /// Добавляет сущность
    /// </summary>
    public virtual async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Добавление новой сущности {EntityType}", typeof(TEntity).Name);

        await Entities.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Обновляет сущность (async версия)
    /// </summary>
    public virtual async Task<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Обновление сущности {EntityType}", typeof(TEntity).Name);

        // Убеждаемся, что сущность отслеживается контекстом
        Entities.Update(entity);

        // Возвращаем для асинхронности
        return await Task.FromResult(entity);
    }

    /// <summary>
    /// Удаляет сущность по ID
    /// </summary>
    public virtual async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Logger.Information("Удаление сущности {EntityType} с ID {Id}", 
            typeof(TEntity).Name, id);

        var entity = await Entities
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);

        if (entity == null)
        {
            Logger.Warning("Сущность {EntityType} с ID {Id} не найдена для удаления", 
                typeof(TEntity).Name, id);
            return false;
        }

        Entities.Remove(entity);
        Logger.Information("Сущность {EntityType} с ID {Id} помечена на удаление", 
            typeof(TEntity).Name, id);

        return true;
    }

    /// <summary>
    /// Применяет связанные сущности (Include) к запросу.
    /// Переопределите в производных классах для добавления Include.
    /// </summary>
    protected virtual IQueryable<TEntity> IncludeRelatedEntities(IQueryable<TEntity> query)
    {
        return query;
    }

    /// <summary>
    /// Применяет фильтры к запросу.
    /// Переопределите в производных классах для добавления фильтрации.
    /// </summary>
    protected virtual IQueryable<TEntity> SetFilters(TFilter filter, IQueryable<TEntity> query)
    {
        return query;
    }
}
