using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceManager.TransactionsService.Abstractions;
using FinanceManager.TransactionsService.Contracts.DTOs.AccountTypes;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionAccounts;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionCurrencies;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionHolders;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionsCategories;
using FinanceManager.TransactionsService.Domain.Entities;
using FinanceManager.TransactionsService.Implementations.Errors;
using Microsoft.Extensions.Logging;

namespace FinanceManager.TransactionsService.Implementations;

public class CatalogApiClient(HttpClient httpClient, ILogger<CatalogApiClient> logger) : ICatalogApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<CatalogApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<IEnumerable<TransactionHolder>> GetAllTransactionHoldersAsync(CancellationToken cancellationToken = default)
    {
        return GetFromApiAsync<TransactionHolder>("/api/v1/RegistryHolder?ItemsPerPage=1000&Page=1", cancellationToken)
            .ContinueWith(t => (IEnumerable<TransactionHolder>)t.Result, cancellationToken);
    }
    
    public Task<TransactionHolder?> GetTransactionHolderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdFromApiAsync<TransactionHolder>($"/api/v1/RegistryHolder/{id}", cancellationToken);
    }
    
    public Task<IEnumerable<TransactionsAccount>> GetAllTransactionAccountsAsync(CancellationToken cancellationToken = default)
    {
        return GetFromApiAsync<TransactionsAccount>("/api/v1/Account?ItemsPerPage=1000&Page=1", cancellationToken)
            .ContinueWith(t => (IEnumerable<TransactionsAccount>)t.Result, cancellationToken);
    }
    
    public Task<TransactionsAccount?> GetTransactionAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdFromApiAsync<TransactionsAccount>($"/api/v1/Account/{id}", cancellationToken);
    }

    public Task<IEnumerable<TransactionsAccountType>> GetAllAccountTypesAsync(CancellationToken cancellationToken = default)
    {
        return GetFromApiAsync<TransactionsAccountType>("/api/v1/AccountType?ItemsPerPage=1000&Page=1", cancellationToken)
            .ContinueWith(t => (IEnumerable<TransactionsAccountType>)t.Result, cancellationToken);
    }

    public Task<TransactionsAccountType?> GetAccountTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdFromApiAsync<TransactionsAccountType>($"/api/v1/AccountType/{id}", cancellationToken);
    }

    public Task<IEnumerable<TransactionsCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return GetFromApiAsync<TransactionsCategory>("/api/v1/Category?ItemsPerPage=1000&Page=1", cancellationToken)
            .ContinueWith(t => (IEnumerable<TransactionsCategory>)t.Result, cancellationToken);
    }

    public Task<TransactionsCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdFromApiAsync<TransactionsCategory>($"/api/v1/Category/{id}", cancellationToken);
    }

    public Task<IEnumerable<TransactionsCurrency>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return GetFromApiAsync<TransactionsCurrency>("/api/v1/Currency?ItemsPerPage=1000&Page=1", cancellationToken)
            .ContinueWith(t => (IEnumerable<TransactionsCurrency>)t.Result, cancellationToken);
    }

    public Task<TransactionsCurrency?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return GetByIdFromApiAsync<TransactionsCurrency>($"/api/v1/Currency/{id}", cancellationToken);
    }
    
    private async Task<List<T>> GetFromApiAsync<T>(
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос {Url} из внешнего API", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };

            var result = await response.Content.ReadFromJsonAsync<List<T>>(options, cancellationToken: cancellationToken);
            _logger.LogInformation("Получено {Count} записей с {Url}", result?.Count ?? 0, url);

            return result ?? new List<T>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при загрузке {Url}", url);
            throw new ExternalApiException($"Не удалось загрузить данные с {url}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при загрузке {Url}", url);
            throw;
        }
    }
    private async Task<T?> GetByIdFromApiAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return default;

            response.EnsureSuccessStatusCode();

            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };

            return await response.Content.ReadFromJsonAsync<T>(options, cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при загрузке {Url}", url);
            throw new ExternalApiException($"Не удалось загрузить данные с {url}", ex);
        }
    }


}
