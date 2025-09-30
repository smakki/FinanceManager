using FinanceManager.TransactionsService.Abstractions.Errors;
using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Contracts.DTOs.AccountTypes;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionAccounts;
using FluentResults;
using Serilog;

namespace FinanceManager.TransactionsService.Implementations.Services;

public class TransactionAccountService(
    ITransactionAccountRepository accountRepository,
    ITransactionAccountErrorsFactory errorsFactory,
    IUnitOfWork unitOfWork,
    ILogger logger) : ITransactionAccountService
{
    /// <summary>
    /// Получает счет по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор счета</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO счета или ошибка, если не найдена</returns>
    public async Task<Result<TransactionAccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.Information("Получение счета по идентификатору {id}", id);
        var account = await accountRepository.GetByIdAsync(id, disableTracking: true, cancellationToken: cancellationToken);
        if (account is null)
        {
            logger.Warning("Счет с идентификатором {AccountId} не найден", id);
            return Result.Fail(errorsFactory.NotFound(id));
        }
        
        logger.Information("Счет {AccountId} успешно получен", id);
        return Result.Ok(account.ToDto());
    }

    public async Task<Result<ICollection<TransactionAccountDto>>> GetPagedAsync(TransactionAccountFilterDto filter, CancellationToken cancellationToken = default)
    {
        logger.Information("Получение списка счетов с фильтрацией: {Filter}", filter);

        var accounts = await accountRepository.GetPagedAsync(filter, cancellationToken: cancellationToken);
        var accountsDto = accounts.ToDto();

        logger.Information("Получено {Count} счетов", accountsDto.Count);
        return Result.Ok(accountsDto);
    }

    public async Task<Result<TransactionAccountDto>> CreateAsync(CreateTransactionAccountDto createDto, CancellationToken cancellationToken = default)
    {
        logger.Information("Создание нового счета: {@CreateDto}", createDto);

        var account = await accountRepository.AddAsync(createDto.ToTransactionAccount(), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.Information("Счет {AccountId} успешно создан", account.Id);
        return Result.Ok(account.ToDto());
    }

    public Task<Result<TransactionAccountDto>> UpdateAsync(UpdateTransactionAccountDto updateDto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.Information("Жесткое удаление счета: {AccountId}", id);

        var account = await accountRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (account is null)
        {
            logger.Information("Счет {AccountId} не найден, удаление не требуется", id);
            return Result.Ok();
        }
        
        await accountRepository.DeleteAsync(id, cancellationToken);
        var affectedRows = await unitOfWork.CommitAsync(cancellationToken);

        if (affectedRows > 0)
        {
            logger.Information("Счет {AccountId} успешно удален", id);
        }
        
        return Result.Ok();
    }
}