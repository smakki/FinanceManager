using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Implementations.Jobs;
using FinanceManager.TransactionsService.Implementations.Services;
using Quartz;
using Quartz.Spi;

namespace FinanceManager.TransactionsService.API.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollection AddTransactionDataLoaderJob(this IServiceCollection services)
    {
        services.AddScoped<IExternalDataLoaderService, ExternalDataLoaderService>();

        services.AddQuartz(q =>
        {
            //q.UseMicrosoftDependencyInjectionScopedJobFactory();

            var jobKey = new JobKey(nameof(ExternalDataLoaderJob));
            q.AddJob<ExternalDataLoaderJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(ExternalDataLoaderJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever()));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        return services;
    }
}