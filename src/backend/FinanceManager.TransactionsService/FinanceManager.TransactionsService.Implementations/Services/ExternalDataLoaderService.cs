using FinanceManager.TransactionsService.Abstractions;
using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Abstractions.Services;
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

    // Примитивы синхронизации для предотвращения параллельных вызовов
    private readonly SemaphoreSlim _holderLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _accountLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _accountTypeLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _categoryLoadLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _currencyLoadLock = new SemaphoreSlim(1, 1);
    public async Task LoadTransactionHoldersAsync(CancellationToken cancellationToken)
    {
             // Используем SemaphoreSlim для предотвращения одновременной загрузки
        // Если другой поток уже выполняет загрузку, текущий поток будет ожидать
        logger.Information("Начало загрузки TransactionHolders (RegistryHolders)");

        // WaitAsync позволяет асинхронно ожидать освобождения семафора
        await _holderLoadLock.WaitAsync(cancellationToken);
        
        try
        {
            logger.Debug("Семафор захвачен, начинается загрузка данных");

            // Получаем данные из внешнего API
            var holders = await catalogApiClient.GetAllTransactionHoldersAsync(cancellationToken);

            if (holders == null || !holders.Any())
            {
                logger.Warning("Внешний API не вернул данные RegistryHolders");
                return;
            }

            logger.Information("Получено {Count} записей RegistryHolders для сохранения", holders.Count());

            // Маппинг DTO в доменную модель и сохранение
            foreach (var holderDto in holders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Проверяем, существует ли уже запись
                var existingHolder = await holderRepository.GetByIdAsync(holderDto.Id, cancellationToken :cancellationToken);

                if (existingHolder == null)
                {
                    // Создаем новую сущность
                    var newHolder = new TransactionHolder(
                        role: holderDto.Role,
                        telegramId: holderDto.TelegramId)
                    {
                        Id = holderDto.Id,
                        CreatedAt = DateTime.UtcNow  // Не забудьте добавить дату создания
                    };

                    await holderRepository.AddAsync(newHolder, cancellationToken);
                    logger.Debug("Добавлен новый TransactionHolder: {Id}", holderDto.Id);
                }
                else
                {
                    // Обновляем существующую сущность
                    existingHolder.TelegramId = holderDto.TelegramId;
                    existingHolder.Role = holderDto.Role;

                    await holderRepository.UpdateAsync(existingHolder, cancellationToken);
                    logger.Debug("Обновлен TransactionHolder: {Id}", holderDto.Id);
                }
            }

            // Сохраняем изменения в базе данных
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
            // КРИТИЧЕСКИ ВАЖНО: освобождаем семафор в блоке finally
            // чтобы гарантировать освобождение даже при исключениях
            _holderLoadLock.Release();
            logger.Debug("Семафор освобожден");
        }

    }

    public async Task LoadTransactionsAccountsAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadAccountTypesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadCategoriesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadCurrenciesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    
    
    private string MapRole(Role role)
    {
        return role switch
        {
            Role.User => "User",
            Role.Administrator => "Administrator",
            _ => throw new ArgumentOutOfRangeException(nameof(role), $"Неизвестная роль: {role}")
        };
    }

}