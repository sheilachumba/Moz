using System.Text.Json;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ILimitDetailService
    {
        Task<List<LimitDetail>> GetLimitDetailsByInsurerAsync(string insurerCode);
    }

    public class LimitDetailService : ILimitDetailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _limitDetailsBaseUrl;

        public LimitDetailService(IConfiguration configuration)
        {
            _configuration = configuration;
            var baseUrl = _configuration["BcApi:BaseUrl"];
            _limitDetailsBaseUrl = $"{baseUrl}Company('STANDARD%20INSURANCE')/LimitDetails";
        }

        public async Task<List<LimitDetail>> GetLimitDetailsByInsurerAsync(string insurerCode)
        {
            try
            {
                var config = new BusinessCentralConfig(_configuration);
                var handler = config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                config.ConfigureHttpClient(client);

                var filter = $"$filter=Insurer eq '{insurerCode}'";
                var url = $"{_limitDetailsBaseUrl}?{filter}";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return new List<LimitDetail>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<LimitDetail>>(content, options);

                return oDataResponse?.Value?.ToList() ?? new List<LimitDetail>();
            }
            catch
            {
                return new List<LimitDetail>();
            }
        }
    }
}
