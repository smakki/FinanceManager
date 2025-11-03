namespace FinanceManager.TransactionsService.Abstractions.Services;

public interface IExternalDataLoaderService
{
    Task LoadTransactionHoldersAsync(CancellationToken cancellationToken);
    Task LoadTransactionsAccountsAsync(CancellationToken cancellationToken);
    Task LoadAccountTypesAsync(CancellationToken cancellationToken);
    Task LoadCategoriesAsync(CancellationToken cancellationToken);
    Task LoadCurrenciesAsync(CancellationToken cancellationToken);
}