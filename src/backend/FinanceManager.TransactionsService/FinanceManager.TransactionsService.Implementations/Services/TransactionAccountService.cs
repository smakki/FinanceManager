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

    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> BelongsToUserAsync(Guid accountId, Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> IsWithinCreditLimitAsync(Guid accountId, decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<AccountTypeDto>> GetAccountTypeAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}