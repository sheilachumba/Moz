using System.Text.Json;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ICoverDetailService
    {
        Task<List<CoverDetail>> GetCoverDetailsByInsurerAsync(string insurerCode);
    }

    public class CoverDetailService : ICoverDetailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _coverDetailsBaseUrl;

        public CoverDetailService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            var baseUrl = _configuration["BcApi:BaseUrl"];
            _coverDetailsBaseUrl = $"{baseUrl}Company('STANDARD%20INSURANCE')/CoverDetails";
        }

        public async Task<List<CoverDetail>> GetCoverDetailsByInsurerAsync(string insurerCode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CoverDetailService] Fetching cover details for Insurer: {insurerCode}");

                var config = new BusinessCentralConfig(_configuration);
                var handler = config.CreateHttpClientHandler();
                var client = new HttpClient(handler);
                config.ConfigureHttpClient(client);
                
                // Filter: Insurer = insurerCode
                var filter = $"$filter=Insurer eq '{insurerCode}'";
                var url = $"{_coverDetailsBaseUrl}?{filter}";

                System.Diagnostics.Debug.WriteLine($"[CoverDetailService] URL: {url}");

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[CoverDetailService] Error: {response.StatusCode}");
                    return new List<CoverDetail>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[CoverDetailService] Response length: {content.Length}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<CoverDetail>>(content, options);

                if (oDataResponse?.Value == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CoverDetailService] No cover details found");
                    return new List<CoverDetail>();
                }

                var viewModels = oDataResponse.Value.Select(cd => new CoverDetail
                {
                    Description_Type = cd.Description_Type,
                    No = cd.No,
                    Description = cd.Description,
                    Value = cd.Value
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[CoverDetailService] Fetched {viewModels.Count} cover details");
                return viewModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CoverDetailService] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CoverDetailService] Stack trace: {ex.StackTrace}");
                return new List<CoverDetail>();
            }
        }
    }
}
