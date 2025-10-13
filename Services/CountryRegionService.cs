using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CountryRegionService
{
    private readonly HttpClient _httpClient;

    public CountryRegionService(IConfiguration config)
    {
        var handler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(
                config["BcApi:Administrator"],
                config["BcApi:Insurance@2030#"])
        };
        _httpClient = new HttpClient(handler);
    }

    public class CountryRegion
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public async Task<List<CountryRegion>> GetCountriesAsync()
    {
        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/CountriesAndRegions";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch countries: HTTP {(int)response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var values = doc.RootElement.GetProperty("value");

        var result = new List<CountryRegion>();
        foreach (var item in values.EnumerateArray())
        {
            result.Add(new CountryRegion
            {
                Code = item.GetProperty("Code").GetString() ?? "",
                Name = item.GetProperty("Name").GetString() ?? ""
            });
        }
        return result;
    }
}
