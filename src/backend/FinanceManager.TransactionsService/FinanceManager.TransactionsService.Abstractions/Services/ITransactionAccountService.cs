using FinanceManager.TransactionsService.Contracts.DTOs.TransactionAccounts;
using FluentResults;

namespace FinanceManager.TransactionsService.Abstractions.Services;

/// <summary>
/// Интерфейс сервиса для работы со счётами пользователей
/// </summary>
public interface ITransactionAccountService
{
    /// <summary>
    /// Получает счёт по идентификатору
    /// </summary>
    Task<Result<TransactionAccountDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает список счетов с фильтрацией и пагинацией
    /// </summary>
    Task<Result<ICollection<TransactionAccountDto>>> GetPagedAsync(
        TransactionAccountFilterDto filter,
        CancellationToken cancellationToken = default);
    

    /// <summary>
    /// Создаёт новый счёт
    /// </summary>
    Task<Result<TransactionAccountDto>> CreateAsync(
        CreateTransactionAccountDto createDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет существующий счёт
    /// </summary>
    Task<Result<TransactionAccountDto>> UpdateAsync(
        UpdateTransactionAccountDto updateDto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет счёт
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет возможность использования счета
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> CheckAccountAsync(Guid accountId, CancellationToken cancellationToken);
}