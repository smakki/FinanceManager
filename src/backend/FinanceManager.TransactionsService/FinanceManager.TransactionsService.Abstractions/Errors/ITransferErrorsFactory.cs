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

    /// <summary>
    /// Создаёт ошибку, если счёт, участвующий в переводе, был мягко удалён
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>Экземпляр ошибки</returns>
    IError AccountIsSoftDeleted(Guid accountId);

    /// <summary>
    /// Создаёт ошибку, если счёт, участвующий в переводе, архивирован
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>Экземпляр ошибки</returns>
    IError AccountIsArchived(Guid accountId);
    
    /// <summary>
    /// Создаёт ошибку, если перевод не принадлежит указанному пользователю
    /// </summary>
    /// <param name="transactionId">Идентификатор перевода</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Экземпляр ошибки</returns>
    IError TransferNotBelongsToUser(Guid transactionId, Guid userId);
}