using FinanceManager.TransactionsService.Contracts.DTOs.TransactionHolders;

namespace FinanceManager.TransactionsService.Abstractions;

public interface ICatalogApiClient
{
    Task<IEnumerable<TransactionHolderDto>> GetAllTransactionHoldersAsync(CancellationToken cancellationToken = default);
    Task<TransactionHolderDto?> GetTransactionHolderByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
