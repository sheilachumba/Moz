using Moz.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace Moz.Services
{
    public interface IInsuranceProductService
    {
        Task<List<InsuranceProduct>> GetInsuranceProducts();
    }

    public class InsuranceProductService : IInsuranceProductService
    {
        private readonly HttpClient _httpClient;

        public InsuranceProductService()
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential("Administrator", "Insurance@2030#", "SIIBL-CIC-DEMO")
            };

            _httpClient = new HttpClient(handler);

            // Add Basic authentication header explicitly as double insurance
            //var byteArray = System.Text.Encoding.ASCII.GetBytes("Administrator:Insurance@2030#");
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<List<InsuranceProduct>> GetInsuranceProducts()
        {
            var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/productclasscard";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var productResponse = JsonSerializer.Deserialize<InsuranceProductResponse>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return productResponse?.Value ?? new List<InsuranceProduct>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch data: {response.StatusCode}, {errorContent}");
            }
        }
    }
}
