using FinanceManager.TransactionsService.Abstractions.Errors;
using FluentResults;
using Serilog;

namespace FinanceManager.TransactionsService.Implementations.Errors;

/// <summary>
/// Фабрика ошибок для сущности Transfer (перевод между счетами)
/// Предоставляет методы для генерации типовых ошибок, связанных с переводами
/// </summary>
public class TransferErrorsFactory(IErrorsFactory errorsFactory, ILogger logger) : ITransferErrorsFactory
{
    private const string EntityName = "Transfer";
    private const string AccountField = "Account";
    private const string AmountField = "Amount";

    /// <summary>
    /// Создаёт ошибку, если перевод с указанным идентификатором не найден
    /// </summary>
    /// <param name="id">Идентификатор перевода</param>
    /// <returns>Экземпляр ошибки</returns>
    public IError NotFound(Guid id)
    {
        logger.Warning("Transfer not found: {TransferId}", id);
        return errorsFactory.NotFound("TRANSFER_NOT_FOUND", EntityName, id);
    }

    /// <summary>
    /// Создаёт ошибку, если сумма перевода недействительна (например, равна нулю или отрицательна)
    /// </summary>
    /// <returns>Экземпляр ошибки</returns>
    public IError InvalidAmount()
    {
        logger.Warning("Invalid transfer amount");
        return errorsFactory.Required("TRANSFER_INVALID_AMOUNT", EntityName, AmountField);
    }

    /// <summary>
    /// Создаёт ошибку, если счёт, участвующий в переводе, не найден
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>Экземпляр ошибки</returns>
    public IError AccountNotFound(Guid accountId)
    {
        logger.Warning("Account not found for transfer: {AccountId}", accountId);
        return errorsFactory.NotFound("TRANSFER_ACCOUNT_NOT_FOUND", AccountField, accountId);
    }
}