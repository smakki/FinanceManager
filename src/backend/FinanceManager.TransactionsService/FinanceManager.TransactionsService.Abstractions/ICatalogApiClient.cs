using FinanceManager.TransactionsService.Contracts.DTOs.AccountTypes;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionAccounts;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionCurrencies;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionHolders;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionsCategories;
using FinanceManager.TransactionsService.Domain.Entities;

namespace FinanceManager.TransactionsService.Abstractions;

public interface ICatalogApiClient
{
    // TransactionHolders
    Task<IEnumerable<TransactionHolder>> GetAllTransactionHoldersAsync(CancellationToken cancellationToken = default);
    Task<TransactionHolder?> GetTransactionHolderByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // TransactionAccounts
    Task<IEnumerable<TransactionsAccount>> GetAllTransactionAccountsAsync(CancellationToken cancellationToken = default);
    Task<TransactionsAccount?> GetTransactionAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // AccountTypes
    Task<IEnumerable<TransactionsAccountType>> GetAllAccountTypesAsync(CancellationToken cancellationToken = default);
    Task<TransactionsAccountType?> GetAccountTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // TransactionCategories
    Task<IEnumerable<TransactionsCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<TransactionsCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // TransactionCurrencies
    Task<IEnumerable<TransactionsCurrency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<TransactionsCurrency?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken = default);
}