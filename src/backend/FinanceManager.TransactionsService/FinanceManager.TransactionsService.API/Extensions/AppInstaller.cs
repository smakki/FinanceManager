using System;
using FinanceManager.TransactionsService.Abstractions;
using FinanceManager.TransactionsService.Abstractions.Errors;
using FinanceManager.TransactionsService.Abstractions.Repositories;
using FinanceManager.TransactionsService.Abstractions.Services;
using FinanceManager.TransactionsService.Implementations;
using FinanceManager.TransactionsService.Implementations.Errors;
using FinanceManager.TransactionsService.Implementations.Services;
using FinanceManager.TransactionsService.Repositories.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FinanceManager.TransactionsService.API.Extensions;

public static class AppInstaller
{
    /// <summary>
    /// Добавляет и настраивает логирование Serilog в приложение.
    /// </summary>
    /// <param name="hostBuilder">Строитель хоста приложения.</param>
    /// <param name="configuration">Конфигурация приложения, содержащая настройки логирования.</param>
    /// <returns>Обновлённый <see cref="IHostBuilder"/> с настроенным логированием.</returns>
    public static IHostBuilder AddLogging(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        return hostBuilder.UseSerilog((context, loggerConfig) =>
            loggerConfig
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId());
    }
    
        /// <summary>
    /// Регистрирует все зависимости приложения, включая репозитории и сервисы.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddRepositories()
            .AddServices()
            .AddQuartzJobs(configuration);
    }
        
    private static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
        {
            var baseUrl = configuration["ExternalApis:CatalogService:Url"] ?? "http://localhost:8080";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }   
        
    /// <summary>
    /// Регистрирует репозитории в контейнер зависимостей со временем жизни Scoped.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<ITransactionAccountRepository, TransactionAccountRepository>()
            .AddScoped<ITransactionCategoryRepository, TransactionCategoryRepository>()
            .AddScoped<ITransactionCurrencyRepository, TransactionCurrencyRepository>()
            .AddScoped<IAccountTypeRepository, AccountTypeRepository>()
            .AddScoped<ITransactionHolderRepository, TransactionHolderRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>()
            .AddScoped<ITransferRepository, TransferRepository>();

        return services;
    }

    /// <summary>
    /// Регистрирует бизнес-сервисы в контейнер зависимостей со временем жизни Scoped.
    /// </summary>
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<ITransactionAccountService, TransactionAccountService>()
           // .AddScoped<ITransactionsCategoryService, TransactionCategoryService>()
            //.AddScoped<ITransactionsCurrencyService, TransactionsCurrencyService>()
            //.AddScoped<ITransactionsAccountTypeService, TransactionsAccountTypeService>()
            //.AddScoped<ITransactionHolderService, TransactionHolderService>()
            .AddScoped<ITransactionService, TransactionService>()
            .AddScoped<ITransferService, TransferService>();

        services.AddFluentResultsExceptionsFactories();

        return services;
    }

    /// <summary>
    /// Регистрирует фабрики ошибок для различных сущностей.
    /// </summary>
    private static IServiceCollection AddFluentResultsExceptionsFactories(this IServiceCollection services)
    {
        services
            .AddScoped<IErrorsFactory, ErrorsFactory>()
            .AddScoped<ITransactionAccountErrorsFactory, TransactionAccountErrorsFactory>()
            //.AddScoped<ITransactionCategoryErrorsFactory, TransactionCategoryErrorsFactory>()
            //.AddScoped<ITransactionCurrencyErrorsFactory, TransactionCurrencyErrorsFactory>()
            //.AddScoped<IAccountTypeErrorsFactory, AccountTypeErrorsFactory>()
            //.AddScoped<ITransactionHolderErrorsFactory, TransactionHolderErrorsFactory>()
            .AddScoped<ITransactionErrorsFactory, TransactionErrorsFactory>()
            .AddScoped<ITransferErrorsFactory, TransferErrorsFactory>();

        return services;
    }

}
