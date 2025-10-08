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

    public async Task<Result<TransferDto>> UpdateAsync(UpdateTransferDto updateDto, CancellationToken cancellationToken = default)
    {
        logger.Information("Updating transfer: {@UpdateDto}", updateDto);

        var transfer = await transferRepository.GetByIdAsync(updateDto.Id, cancellationToken: cancellationToken);
        if (transfer is null)
        {
            return Result.Fail(errorsFactory.NotFound(updateDto.Id));
        }

        var isNeedUpdate = false;

        if (updateDto.Date is not null && transfer.Date != updateDto.Date.Value)
        {
            transfer.Date = updateDto.Date.Value;
            isNeedUpdate = true;
        }

        if (updateDto.FromAccountId is not null && transfer.FromAccountId != updateDto.FromAccountId.Value)
        {
            var checkResult = await transactionAccountService.CheckAccountAsync(updateDto.FromAccountId.Value, cancellationToken);
            if (checkResult.IsFailed)
                return Result.Fail(checkResult.Errors);
            transfer.FromAccountId = updateDto.FromAccountId.Value;
            isNeedUpdate = true;
        }
        
        if (updateDto.ToAccountId is not null && transfer.ToAccountId != updateDto.ToAccountId.Value)
        {
            var checkResult = await transactionAccountService.CheckAccountAsync(updateDto.ToAccountId.Value, cancellationToken);
            if (checkResult.IsFailed)
                return Result.Fail(checkResult.Errors);
            transfer.ToAccountId = updateDto.ToAccountId.Value;
            isNeedUpdate = true;
        }

        if (updateDto.FromAmount is not null && transfer.FromAmount != updateDto.FromAmount.Value)
        {
            if (updateDto.FromAmount.Value <= 0)
            {
                return Result.Fail(errorsFactory.InvalidAmount());
            }
            transfer.FromAmount = updateDto.FromAmount.Value;
            isNeedUpdate = true;
        }
        
        if (updateDto.ToAmount is not null && transfer.ToAmount != updateDto.ToAmount.Value)
        {
            if (updateDto.ToAmount.Value <= 0)
            {
                return Result.Fail(errorsFactory.InvalidAmount());
            }
            transfer.ToAmount = updateDto.ToAmount.Value;
            isNeedUpdate = true;
        }

        if (updateDto.Description != transfer.Description)
        {
            transfer.Description = updateDto.Description;
            isNeedUpdate = true;
        }

        if (isNeedUpdate)
        {
            transferRepository.Update(transfer);
            await unitOfWork.CommitAsync(cancellationToken);
            logger.Information("Successfully updated transfer: {TransferId}", updateDto.Id);
        }
        else
        {
            logger.Information("No changes detected for transaction: {TransactionId}", updateDto.Id);
        }

        return Result.Ok(transfer.ToDto());

    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.Information("Deleting transfer: {TransferId}", id);

        var transfer = await transferRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (transfer is null)
        {
            return Result.Ok();
        }

        await transferRepository.DeleteAsync(id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        logger.Information("Successfully deleted transfer: {TransferId}", id);
        return Result.Ok();

    }

    public async Task<Result<int>> GetCountAsync(TransferFilterDto filter, CancellationToken cancellationToken = default)
    {
        logger.Information("Getting transfers count with filter: {@Filter}", filter);
        var count = await transferRepository.GetCountAsync(filter, cancellationToken);
        logger.Information("Successfully retrieved transfer count: {Count}", count);
        return Result.Ok(count);
    }
}