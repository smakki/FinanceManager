using FinanceManager.TransactionsService.Abstractions.Repositories.Common;
using FinanceManager.TransactionsService.Contracts.DTOs.Transactions;
using FinanceManager.TransactionsService.Domain.Entities;

namespace FinanceManager.TransactionsService.Abstractions.Repositories;

/// <summary>
/// Интерфейс репозитория для работы с транзакциями
/// </summary>
public interface ITransactionRepository: IBaseRepository<Transaction, TransactionFilterDto>
{
    /// <summary>
    /// Получает общее количество транзакций по фильтру
    /// </summary>
    Task<int> GetCountAsync(TransactionFilterDto filter, CancellationToken cancellationToken = default);
}