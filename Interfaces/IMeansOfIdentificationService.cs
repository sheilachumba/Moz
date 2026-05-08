using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IMeansOfIdentificationService
    {
        Task<List<MeansOfIdentification>> GetMeansOfIdentificationAsync();
    }

    public class MeansOfIdentificationService : IMeansOfIdentificationService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<MeansOfIdentificationService> _logger;

        public MeansOfIdentificationService(IConfiguration configuration, ILogger<MeansOfIdentificationService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
        }

        public async Task<List<MeansOfIdentification>> GetMeansOfIdentificationAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();

                using (var client = new HttpClient(handler))
                {
                    _config.ConfigureHttpClient(client);

                    var endpoint = _config.GetEndpoint("MeansofIdentification");
                    var url = _config.BuildUrl(endpoint);

                    _logger.LogInformation($"Fetching means of identification from: {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Means of Identification Response Data: {jsonData}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var meansResponse = JsonSerializer.Deserialize<MeansOfIdentificationResponse>(jsonData, options);

                        _logger.LogInformation($"Successfully fetched {meansResponse?.Value?.Count ?? 0} means of identification from ERP");
                        return meansResponse?.Value ?? new List<MeansOfIdentification>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed Response: {response.StatusCode}, {errorContent}");
                        throw new Exception($"Failed to fetch means of identification: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error while fetching means of identification: {ex.Message}");
                return new List<MeansOfIdentification>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new List<MeansOfIdentification>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching means of identification: {ex.Message}");
                return new List<MeansOfIdentification>();
            }
        }
    }
}
