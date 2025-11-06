using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceManager.TransactionsService.Abstractions;
using FinanceManager.TransactionsService.Contracts.DTOs.TransactionHolders;
using FinanceManager.TransactionsService.Implementations.Errors;
using Microsoft.Extensions.Logging;

namespace FinanceManager.TransactionsService.Implementations;

public class CatalogApiClient(HttpClient httpClient, ILogger<CatalogApiClient> logger) : ICatalogApiClient
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<CatalogApiClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<TransactionHolderDto>> GetAllTransactionHoldersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Запрос всех RegistryHolders из внешнего API");

            // Используем query параметры для получения большого количества записей
            var response = await _httpClient.GetAsync(
                "/api/v1/RegistryHolder?ItemsPerPage=1000&Page=1", 
                cancellationToken);

            response.EnsureSuccessStatusCode();
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
            var holders = await response.Content.ReadFromJsonAsync<List<TransactionHolderDto>>(options,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Получено {Count} RegistryHolders", holders?.Count ?? 0);

            return holders ?? Enumerable.Empty<TransactionHolderDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при загрузке RegistryHolders");
            throw new ExternalApiException("Не удалось загрузить данные RegistryHolders", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неожиданная ошибка при загрузке RegistryHolders");
            throw;
        }
    }

    public async Task<TransactionHolderDto?> GetTransactionHolderByIdAsync(
        Guid id, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/v1/RegistryHolder/{id}", 
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TransactionHolderDto>(
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка HTTP при загрузке RegistryHolder {Id}", id);
            throw new ExternalApiException($"Не удалось загрузить RegistryHolder {id}", ex);
        }
    }
}
