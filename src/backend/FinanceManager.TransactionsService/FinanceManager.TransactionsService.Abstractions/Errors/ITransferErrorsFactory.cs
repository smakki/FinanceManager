using FluentResults;

namespace FinanceManager.TransactionsService.Abstractions.Errors;

public interface ITransferErrorsFactory
{
    /// <summary>
    /// Создаёт ошибку, если перевод с указанным идентификатором не найден
    /// </summary>
    /// <param name="id">Идентификатор перевода</param>
    /// <returns>Экземпляр ошибки</returns>
    IError NotFound(Guid id);

    /// <summary>
    /// Создаёт ошибку, если сумма перевод недействителен (например, равен нулю)
    /// </summary>
    /// <returns>Экземпляр ошибки</returns>
    IError InvalidAmount();

    /// <summary>
    /// Создаёт ошибку, если счёт, участвующий в переводе, не найден
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>Экземпляр ошибки</returns>
    IError AccountNotFound(Guid accountId);

}