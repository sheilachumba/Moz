using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ICountryRegionService
    {
        Task<List<CountryRegion>> GetCountriesAsync();
    }

    public class CountryRegionService : ICountryRegionService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<CountryRegionService> _logger;

        public CountryRegionService(IConfiguration configuration, ILogger<CountryRegionService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
        }

        public async Task<List<CountryRegion>> GetCountriesAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();

                using (var client = new HttpClient(handler))
                {
                    _config.ConfigureHttpClient(client);

                    var endpoint = _config.GetEndpoint("CountriesAndRegions");
                    var url = _config.BuildUrl(endpoint);

                    _logger.LogInformation($"Fetching countries from: {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Countries Response Data: {jsonData}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var countryResponse = JsonSerializer.Deserialize<CountryRegionResponse>(jsonData, options);

                        _logger.LogInformation($"Successfully fetched {countryResponse?.Value?.Count ?? 0} countries from ERP");
                        return countryResponse?.Value ?? new List<CountryRegion>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed Response: {response.StatusCode}, {errorContent}");
                        throw new Exception($"Failed to fetch countries: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error while fetching countries: {ex.Message}");
                return new List<CountryRegion>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new List<CountryRegion>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching countries: {ex.Message}");
                return new List<CountryRegion>();
            }
        }
    }
}
