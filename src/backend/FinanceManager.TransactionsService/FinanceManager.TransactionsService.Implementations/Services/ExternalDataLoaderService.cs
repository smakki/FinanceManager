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
    private readonly ITransactionAccountRepository _accountRepository = accountRepository;
    private readonly IAccountTypeRepository _accountTypeRepository = accountTypeRepository;
    private readonly ITransactionCategoryRepository _categoryRepository = categoryRepository;
    private readonly ITransactionCurrencyRepository _currencyRepository = currencyRepository;
    
    private readonly SemaphoreSlim _holderLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _accountLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _accountTypeLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _categoryLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _currencyLoadLock = new SemaphoreSlim(1, 1);
    public async Task LoadTransactionHoldersAsync(CancellationToken cancellationToken)
    {
        logger.Information("Начало загрузки TransactionHolders (RegistryHolders)");
        await _holderLoadLock.WaitAsync(cancellationToken);
        
        try
        {
            logger.Debug("Семафор захвачен, начинается загрузка данных");
            
            var holders = await catalogApiClient.GetAllTransactionHoldersAsync(cancellationToken);

            if (holders == null || !holders.Any())
            {
                logger.Warning("Внешний API не вернул данные RegistryHolders");
                return;
            }

            logger.Information("Получено {Count} записей RegistryHolders для сохранения", holders.Count());
            
            foreach (var holderDto in holders)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var existingHolder = await holderRepository.GetByIdAsync(holderDto.Id, cancellationToken :cancellationToken);

                if (existingHolder == null)
                {
                    var newHolder = new TransactionHolder(
                        role: holderDto.Role,
                        telegramId: holderDto.TelegramId)
                    {
                        Id = holderDto.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await holderRepository.AddAsync(newHolder, cancellationToken);
                    logger.Debug("Добавлен новый TransactionHolder: {Id}", holderDto.Id);
                }
                else
                {
                    existingHolder.TelegramId = holderDto.TelegramId;
                    existingHolder.Role = holderDto.Role;

                    await holderRepository.UpdateAsync(existingHolder, cancellationToken);
                    logger.Debug("Обновлен TransactionHolder: {Id}", holderDto.Id);
                }
            }
            await unitOfWork.CommitAsync(cancellationToken);
            logger.Information("Успешно загружено и сохранено {Count} TransactionHolders", holders.Count());
        }
        catch (OperationCanceledException)
        {
            logger.Warning("Загрузка TransactionHolders была отменена");
            throw;
        }
        catch (ExternalApiException ex)
        {
            logger.Error(ex, "Ошибка при обращении к внешнему API");
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Неожиданная ошибка при загрузке TransactionHolders");
            throw;
        }
        finally
        {
            _holderLoadLock.Release();
            logger.Debug("Семафор освобожден");
        }

    }

    public async Task LoadTransactionsAccountsAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadAccountTypesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadCategoriesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadCurrenciesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    
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
            var existing = await repository.GetByIdAsync(id, cancellationToken: cancellationToken);

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