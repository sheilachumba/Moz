using System.Text.Json;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IDeductibleService
    {
        Task<List<Deductible>> GetDeductiblesByInsurerAsync(string insurerCode);
    }

    public class DeductibleService : IDeductibleService
    {
        private readonly IConfiguration _configuration;
        private readonly string _deductiblesBaseUrl;

        public DeductibleService(IConfiguration configuration)
        {
            _configuration = configuration;
            var baseUrl = _configuration["BcApi:BaseUrl"];
            _deductiblesBaseUrl = $"{baseUrl}Company('STANDARD%20INSURANCE')/Deductibles";
        }

        public async Task<List<Deductible>> GetDeductiblesByInsurerAsync(string insurerCode)
        {
            try
            {
                var config = new BusinessCentralConfig(_configuration);
                var handler = config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                config.ConfigureHttpClient(client);

                var filter = $"$filter=Insurer eq '{insurerCode}'";
                var url = $"{_deductiblesBaseUrl}?{filter}";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return new List<Deductible>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<Deductible>>(content, options);

                return oDataResponse?.Value?.ToList() ?? new List<Deductible>();
            }
            catch
            {
                return new List<Deductible>();
            }
        }
    }
}
