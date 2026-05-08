using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ISourceOfIncomeService
    {
        Task<List<SourceOfIncome>> GetSourcesOfIncomeAsync();
    }

    public class SourceOfIncomeService : ISourceOfIncomeService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<SourceOfIncomeService> _logger;

        public SourceOfIncomeService(IConfiguration configuration, ILogger<SourceOfIncomeService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
        }

        public async Task<List<SourceOfIncome>> GetSourcesOfIncomeAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();

                using (var client = new HttpClient(handler))
                {
                    _config.ConfigureHttpClient(client);

                    var endpoint = _config.GetEndpoint("SourcesofFunds");
                    var url = _config.BuildUrl(endpoint);

                    _logger.LogInformation($"Fetching sources of income from: {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Sources of Income Response Data: {jsonData}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var sourceResponse = JsonSerializer.Deserialize<SourceOfIncomeResponse>(jsonData, options);

                        _logger.LogInformation($"Successfully fetched {sourceResponse?.Value?.Count ?? 0} sources of income from ERP");
                        return sourceResponse?.Value ?? new List<SourceOfIncome>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed Response: {response.StatusCode}, {errorContent}");
                        throw new Exception($"Failed to fetch sources of income: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error while fetching sources of income: {ex.Message}");
                return new List<SourceOfIncome>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new List<SourceOfIncome>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching sources of income: {ex.Message}");
                return new List<SourceOfIncome>();
            }
        }
    }
}
