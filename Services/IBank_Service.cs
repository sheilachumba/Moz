using Moz.Models;
using System.Net;
using System.Text.Json;

namespace Moz.Services
{
    public interface IBank_interface
    {
       Task<List<Bank>> GetBanks();
      Bank GetBanksbyid();
    }

    public class Bankservice : IBank_interface
    {
        public async Task<List<Bank>> GetBanks()
        {

            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential("Administrator", "P3ns!0nUpgr@d3", "YourDomain")
            };

            var client = new HttpClient(handler);

            var url = "http://197.232.37.157:8153/BC230/ODataV4/Company('SBG%20Securities%20Uganda%20Limited')/Bank";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Data: {jsonData}");

                var accountTypeResponse = JsonSerializer.Deserialize<BankResponse>(jsonData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return accountTypeResponse?.Value ?? new List<Bank>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed Response: {response.StatusCode}, {errorContent}");
                throw new Exception($"Failed to fetch data: {response.StatusCode}");
            }
        }

        public Bank GetBanksbyid()
        {
            throw new NotImplementedException();
        }
    }
}
