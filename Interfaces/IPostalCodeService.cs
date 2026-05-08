using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IPostalCodeService
    {
        Task<List<PostalCode>> GetPostalCodesAsync();
    }

    public class PostalCodeService : IPostalCodeService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<PostalCodeService> _logger;

        public PostalCodeService(IConfiguration configuration, ILogger<PostalCodeService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
        }

        public async Task<List<PostalCode>> GetPostalCodesAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();

                using (var client = new HttpClient(handler))
                {
                    _config.ConfigureHttpClient(client);

                    var endpoint = _config.GetEndpoint("PostalCodes");
                    var url = _config.BuildUrl(endpoint);

                    _logger.LogInformation($"Fetching postal codes from: {url}");

                    var response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonData = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation($"Postal Codes Response Data: {jsonData}");

                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var postalCodeResponse = JsonSerializer.Deserialize<PostalCodeResponse>(jsonData, options);

                        _logger.LogInformation($"Successfully fetched {postalCodeResponse?.Value?.Count ?? 0} postal codes from ERP");
                        return postalCodeResponse?.Value ?? new List<PostalCode>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed Response: {response.StatusCode}, {errorContent}");
                        throw new Exception($"Failed to fetch postal codes: {response.StatusCode}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error while fetching postal codes: {ex.Message}");
                return new List<PostalCode>();
            }
            catch (JsonException ex)
            {
                _logger.LogError($"JSON deserialization error: {ex.Message}");
                return new List<PostalCode>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching postal codes: {ex.Message}");
                return new List<PostalCode>();
            }
        }
    }
}
