using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ISalutationService
    {
        Task<List<Salutation>> GetSalutationsAsync();
    }

    public class SalutationService : ISalutationService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<SalutationService> _logger;

        public SalutationService(IConfiguration configuration, ILogger<SalutationService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
        }

        public async Task<List<Salutation>> GetSalutationsAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();

                using (var client = new HttpClient(handler))
                {
                    _config.ConfigureHttpClient(client);

                    var endpoint = _config.GetEndpoint("Salutations");
                    var url = _config.BuildUrl(endpoint);

                    _logger.LogInformation($"Fetching salutations from: {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Salutations Response Data: {jsonData}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var salutationResponse = JsonSerializer.Deserialize<SalutationResponse>(jsonData, options);

                        _logger.LogInformation($"Successfully fetched {salutationResponse?.Value?.Count ?? 0} salutations from ERP");
                        return salutationResponse?.Value ?? new List<Salutation>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed Response: {response.StatusCode}, {errorContent}");
                        throw new Exception($"Failed to fetch salutations: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error while fetching salutations: {ex.Message}");
                return new List<Salutation>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new List<Salutation>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching salutations: {ex.Message}");
                return new List<Salutation>();
            }
        }
    }
}
