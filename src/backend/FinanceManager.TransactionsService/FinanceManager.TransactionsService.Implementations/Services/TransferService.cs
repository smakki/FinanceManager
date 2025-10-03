using FinanceManager.TransactionsService.Abstractions.Errors;
using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Contracts.DTOs.Transfers;
using FluentResults;
using Serilog;

namespace FinanceManager.TransactionsService.Implementations.Services;

public class TransferService(IUnitOfWork unitOfWork,
    ITransactionAccountService transactionAccountService,
    ITransferRepository transferRepository,
    ITransactionAccountRepository accountRepository,
    ITransferErrorsFactory errorsFactory,
    ILogger logger) : ITransferService
{
    /// <summary>
    /// Получает перевод по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор перевода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>DTO перевода или ошибка, если не найдена</returns>
    public async Task<Result<TransferDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.Information("Getting transfer by id: {TransferId}", id);
        var transfer = await transferRepository.GetByIdAsync(id, disableTracking: true, cancellationToken: cancellationToken);
        if (transfer is null)
        {
            return Result.Fail(errorsFactory.NotFound(id));
        }

        logger.Information("Successfully retrieved transfer: {TransferId}", id);
        return Result.Ok(transfer.ToDto());
    }

    /// <summary>
    /// Получает список переводов с фильтрацией и пагинацией
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат со списком переводов или ошибкой</returns>
    public async Task<Result<ICollection<TransferDto>>> GetPagedAsync(TransferFilterDto filter, CancellationToken cancellationToken = default)
    {
        logger.Information("Getting paged transfers with filter: {@Filter}", filter);
        var transfers = await transferRepository.GetPagedAsync(filter, cancellationToken: cancellationToken);
        var transfersDto = transfers.ToDto();
        logger.Information("Successfully retrieved {Count} transfers", transfersDto.Count);
        return Result.Ok(transfersDto);
    }

    /// <summary>
    /// Создаёт новую перевод
    /// </summary>
    /// <param name="createDto">Данные для создания перевода</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат с созданным переводом или ошибкой</returns>
    public async Task<Result<TransferDto>> CreateAsync(CreateTransferDto createDto, CancellationToken cancellationToken = default)
    {
        logger.Information("Creating transaction: {@CreateDto}", createDto);
        
        if (createDto.FromAmount == 0 || createDto.ToAmount == 0)
        {
            return Result.Fail(errorsFactory.InvalidAmount());
        }

        var checkResult = await transactionAccountService.CheckAccountAsync(createDto.FromAccountId, cancellationToken);
        if (checkResult.IsFailed)
            return Result.Fail(checkResult.Errors);
        
        checkResult = await transactionAccountService.CheckAccountAsync(createDto.ToAccountId, cancellationToken);
        if (checkResult.IsFailed)
            return Result.Fail(checkResult.Errors);
        
        var transaction = await transferRepository.AddAsync(createDto.ToTransfer(), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        logger.Information("Successfully created transaction: {TransactionId}", transaction.Id);
        return Result.Ok(transaction.ToDto());
    }

    public Task<Result<TransferDto>> UpdateAsync(UpdateTransferDto updateDto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<int>> GetCountAsync(TransferFilterDto filter, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}