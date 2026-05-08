using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IReligionService
    {
        Task<List<Religion>> GetReligionsAsync();
    }

    public class ReligionService : IReligionService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<ReligionService> _logger;

        public ReligionService(IConfiguration configuration, ILogger<ReligionService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
        }

        public async Task<List<Religion>> GetReligionsAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();

                using (var client = new HttpClient(handler))
                {
                    _config.ConfigureHttpClient(client);

                    var endpoint = _config.GetEndpoint("ReligionList");
                    var url = _config.BuildUrl(endpoint);

                    _logger.LogInformation($"Fetching religions from: {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Religions Response Data: {jsonData}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var religionResponse = JsonSerializer.Deserialize<ReligionResponse>(jsonData, options);

                        _logger.LogInformation($"Successfully fetched {religionResponse?.Value?.Count ?? 0} religions from ERP");
                        return religionResponse?.Value ?? new List<Religion>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed Response: {response.StatusCode}, {errorContent}");
                        throw new Exception($"Failed to fetch religions: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error while fetching religions: {ex.Message}");
                return new List<Religion>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new List<Religion>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching religions: {ex.Message}");
                return new List<Religion>();
            }
        }
    }
}
