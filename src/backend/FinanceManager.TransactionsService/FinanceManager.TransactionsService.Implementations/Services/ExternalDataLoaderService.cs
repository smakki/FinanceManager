using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Abstractions.Services;

namespace FinanceManager.TransactionsService.Implementations.Services;

public class ExternalDataLoaderService(
    ITransactionHolderRepository holderRepository,
    ITransactionAccountRepository accountRepository,
    IAccountTypeRepository accountTypeRepository,
    ITransactionCategoryRepository categoryRepository,
    ITransactionCurrencyRepository currencyRepository)
    : IExternalDataLoaderService
{
    private readonly ITransactionHolderRepository _holderRepository = holderRepository;
    private readonly ITransactionAccountRepository _accountRepository = accountRepository;
    private readonly IAccountTypeRepository _accountTypeRepository = accountTypeRepository;
    private readonly ITransactionCategoryRepository _categoryRepository = categoryRepository;
    private readonly ITransactionCurrencyRepository _currencyRepository = currencyRepository;
    
    public async Task LoadTransactionHoldersAsync(CancellationToken cancellationToken)
    {
        // Здесь вызываем внешний API, получаем данные и сохраняем через репозиторий
    }

    public async Task LoadTransactionsAccountsAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadAccountTypesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadCategoriesAsync(CancellationToken cancellationToken) { /* аналогично */ }
    public async Task LoadCurrenciesAsync(CancellationToken cancellationToken) { /* аналогично */ }

}