using FinanceManager.TransactionsService.Abstractions.Services;
using Quartz;

namespace FinanceManager.TransactionsService.Implementations.Jobs;

public class ExternalDataLoaderJob(IExternalDataLoaderService loader) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await loader.LoadTransactionHoldersAsync(context.CancellationToken);
        await loader.LoadAccountTypesAsync(context.CancellationToken);
        await loader.LoadCurrenciesAsync(context.CancellationToken);
        await loader.LoadTransactionsAccountsAsync(context.CancellationToken);
        await loader.LoadCategoriesAsync(context.CancellationToken);

    }
}