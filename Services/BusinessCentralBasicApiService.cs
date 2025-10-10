using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// This service handles Basic Authentication for Business Central on-premises API calls.
public class BusinessCentralBasicApiService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public BusinessCentralBasicApiService(IConfiguration config, IHttpClientFactory factory)
    {
        _config = config;
        _httpClient = factory.CreateClient();
    }

    // Configure the HttpClient with Basic Auth headers
    private void SetBasicAuthHeader()
    {
        var username = _config["BcApi:Username"];   // Correct key
        var password = _config["BcApi:Password"];   // Correct key
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
    }


    // Make a GET request to a BC endpoint
    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        SetBasicAuthHeader();
        return await _httpClient.GetAsync(url);
    }

    // Make a POST request to a BC endpoint
    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        SetBasicAuthHeader();
        return await _httpClient.PostAsync(url, content);
    }

    // Make a PATCH request to a BC endpoint
    public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
    {
        SetBasicAuthHeader();
        var method = new HttpMethod("PATCH");
        var request = new HttpRequestMessage(method, url) { Content = content };
        return await _httpClient.SendAsync(request);
    }
    public class Salutation
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public async Task<List<Salutation>> GetSalutationsAsync()
    {
        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/Salutations";

        SetBasicAuthHeader();
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch salutations: HTTP {(int)response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        var values = root.GetProperty("value");

        var result = new List<Salutation>();
        foreach (var item in values.EnumerateArray())
        {
            result.Add(new Salutation
            {
                Code = item.GetProperty("Code").GetString() ?? "",
                Description = item.GetProperty("Description").GetString() ?? ""
            });
        }
        return result;
    }
    public class CountryRegion
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        // You may add more fields if needed
    }

    public async Task<List<CountryRegion>> GetCountriesAsync()
    {
        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/CountriesAndRegions";

        SetBasicAuthHeader();
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch countries: HTTP {(int)response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        var values = root.GetProperty("value");

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
    public class PostalCodeVm
    {
        public string Code { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country_Region_Code { get; set; } = string.Empty;
    }

    public async Task<List<PostalCodeVm>> GetPostalCodesAsync()
    {
        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/PostalCodes";
        SetBasicAuthHeader();
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch postal codes: HTTP {(int)response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        var values = root.GetProperty("value");

        var result = new List<PostalCodeVm>();
        foreach (var item in values.EnumerateArray())
        {
            result.Add(new PostalCodeVm
            {
                Code = item.GetProperty("Code").GetString() ?? "",
                City = item.GetProperty("City").GetString() ?? "",
                Country_Region_Code = item.GetProperty("Country_Region_Code").GetString() ?? ""
            });
        }
        return result;
    }
    public class SourceOfFundsVm
    {
        public string Source { get; set; } = string.Empty;
    }

    public async Task<List<SourceOfFundsVm>> GetSourcesOfFundsAsync()
    {
        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/Insuredcard";
        SetBasicAuthHeader();
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to fetch sources of funds: HTTP {(int)response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        var values = root.GetProperty("value");

        var result = new HashSet<string>();
        foreach (var item in values.EnumerateArray())
        {
            var src = item.GetProperty("Source_of_Funds").GetString();
            if (!string.IsNullOrWhiteSpace(src))
                result.Add(src);
        }
        return result.Select(s => new SourceOfFundsVm { Source = s }).ToList();
    }

}
