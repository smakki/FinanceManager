using FinanceManager.TransactionsService.Abstractions;
using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Domain.Abstractions;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.Domain.Enums;
using FinanceManager.TransactionsService.Implementations.Errors;
using Serilog;

namespace FinanceManager.TransactionsService.Implementations.Services;

public class ExternalDataLoaderService(
    ITransactionHolderRepository holderRepository,
    ITransactionAccountRepository accountRepository,
    IAccountTypeRepository accountTypeRepository,
    ITransactionCategoryRepository categoryRepository,
    ITransactionCurrencyRepository currencyRepository,
    ICatalogApiClient catalogApiClient,
    IUnitOfWork unitOfWork,
    ILogger logger)
    : IExternalDataLoaderService
{
    private readonly SemaphoreSlim _holderLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _accountLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _accountTypeLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _categoryLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _currencyLoadLock = new SemaphoreSlim(1, 1);
    
    public async Task LoadTransactionHoldersAsync(CancellationToken cancellationToken)
    {
        await LoadEntitiesAsync(
            semaphore: _holderLoadLock,
            fetchFromApiAsync: catalogApiClient.GetAllTransactionHoldersAsync,
            repository: holderRepository,
            getId: dto => dto.Id,
            createEntity: dto => new TransactionHolder(dto.Role, dto.TelegramId)
            {
                Id = dto.Id,
                CreatedAt = DateTime.UtcNow
            },
            updateEntity: (entity, dto) =>
            {
                entity.Role = dto.Role;
                entity.TelegramId = dto.TelegramId;
            },
            entityName: "TransactionHolder",
            cancellationToken: cancellationToken
        );
    }

    public async Task LoadTransactionsAccountsAsync(CancellationToken cancellationToken)
    {
        await LoadEntitiesAsync(
            semaphore: _accountLoadLock,
            fetchFromApiAsync: catalogApiClient.GetAllTransactionAccountsAsync,
            repository: accountRepository,
            getId: dto => dto.Id,
            createEntity: dto => new TransactionsAccount(
                accountTypeId: dto.AccountTypeId,
                currencyId: dto.CurrencyId,
                holderId: dto.HolderId,
                creditLimit: dto.CreditLimit)
            {
                Id = dto.Id,
                CreatedAt = DateTime.UtcNow
            },
            updateEntity: (entity, dto) =>
            {
                entity.AccountTypeId = dto.AccountTypeId;
                entity.CurrencyId = dto.CurrencyId;
                entity.HolderId = dto.HolderId;
                entity.IsArchived = dto.IsArchived;
                entity.CreditLimit = dto.CreditLimit;
            },
            entityName: "TransactionAccount",
            cancellationToken: cancellationToken
        );
    }


    public async Task LoadAccountTypesAsync(CancellationToken cancellationToken)
    {
        await LoadEntitiesAsync(
            semaphore: _accountTypeLoadLock,
            fetchFromApiAsync: catalogApiClient.GetAllAccountTypesAsync,
            repository: accountTypeRepository,
            getId: dto => dto.Id,
            createEntity: dto => new TransactionsAccountType(dto.Code, dto.Description)
            {
                Id = dto.Id,
                CreatedAt = DateTime.UtcNow
            },
            updateEntity: (entity, dto) =>
            {
                entity.Code = dto.Code;
                entity.Description = dto.Description;
            },
            entityName: "TransactionsAccountType",
            cancellationToken: cancellationToken
        );
    }

    public async Task LoadCategoriesAsync(CancellationToken cancellationToken)
    {
        await LoadEntitiesAsync(
            semaphore: _categoryLoadLock,
            fetchFromApiAsync: catalogApiClient.GetAllCategoriesAsync,
            repository: categoryRepository,
            getId: dto => dto.Id,
            createEntity: dto => new TransactionsCategory(dto.HolderId, dto.Income, dto.Expense)
            {
                Id = dto.Id,
                CreatedAt = DateTime.UtcNow
            },
            updateEntity: (entity, dto) =>
            {
                entity.HolderId = dto.HolderId;
                entity.Income = dto.Income;
                entity.Expense = dto.Expense;
            },
            entityName: "TransactionCategory",
            cancellationToken: cancellationToken
        );
    }


    public async Task LoadCurrenciesAsync(CancellationToken cancellationToken)
    {
        await LoadEntitiesAsync(
            semaphore: _currencyLoadLock,
            fetchFromApiAsync: catalogApiClient.GetAllCurrenciesAsync,
            repository: currencyRepository,
            getId: dto => dto.Id,
            createEntity: dto => new TransactionsCurrency(dto.CharCode, dto.NumCode)
            {
                Id = dto.Id,
                CreatedAt = DateTime.UtcNow
            },
            updateEntity: (entity, dto) =>
            {
                entity.CharCode = dto.CharCode;
                entity.NumCode = dto.NumCode;
            },
            entityName: "TransactionCurrency",
            cancellationToken: cancellationToken
        );
    }


    
private async Task LoadEntitiesAsync<TDto, TEntity, TFilter>(
    SemaphoreSlim semaphore,
    Func<CancellationToken, Task<IEnumerable<TDto>>> fetchFromApiAsync,
    IBaseRepository<TEntity, TFilter> repository,
    Func<TDto, Guid> getId,
    Func<TDto, TEntity> createEntity,
    Action<TEntity, TDto> updateEntity,
    string entityName,
    CancellationToken cancellationToken)
    where TEntity : IdentityModel
{
    logger.Information("Начало загрузки {EntityName}", entityName);
    await semaphore.WaitAsync(cancellationToken);

    try
    {
        logger.Debug("Семафор захвачен для {EntityName}", entityName);

        var dtos = await fetchFromApiAsync(cancellationToken);

        if (dtos == null || !dtos.Any())
        {
            logger.Warning("Внешний API не вернул данные для {EntityName}", entityName);
            return;
        }

        logger.Information("Получено {Count} записей для {EntityName}", dtos.Count(), entityName);

        foreach (var dto in dtos)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var id = getId(dto);
            var existing = await repository.GetByIdAsync(id, cancellationToken: cancellationToken, includeRelated: false);

            if (existing == null)
            {
                var entity = createEntity(dto);
                await repository.AddAsync(entity, cancellationToken);
                logger.Debug("Добавлен новый {EntityName}: {Id}", entityName, id);
            }
            else
            {
                updateEntity(existing, dto);
                await repository.UpdateAsync(existing, cancellationToken);
                logger.Debug("Обновлен {EntityName}: {Id}", entityName, id);
            }
        }

        await unitOfWork.CommitAsync(cancellationToken);
        logger.Information("Успешно загружено и сохранено {Count} {EntityName}", dtos.Count(), entityName);
    }
    catch (OperationCanceledException)
    {
        logger.Warning("Загрузка {EntityName} была отменена", entityName);
        throw;
    }
    catch (ExternalApiException ex)
    {
        logger.Error(ex, "Ошибка при обращении к внешнему API {EntityName}", entityName);
        throw;
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Неожиданная ошибка при загрузке {EntityName}", entityName);
        throw;
    }
    finally
    {
        semaphore.Release();
        logger.Debug("Семафор освобожден для {EntityName}", entityName);
    }
}

    
}