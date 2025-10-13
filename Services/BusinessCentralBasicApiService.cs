using ClientPortal.Models;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class BusinessCentralBasicApiService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public BusinessCentralBasicApiService(IConfiguration config)
    {
        _config = config;
        var handler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(
                _config["BcApi:Administrator"],
                _config["BcApi:Insurance@2030#"],
                _config["BcApi:SIIBL-CIC-DEMO"]) 
        };
        _httpClient = new HttpClient(handler);
    }

    // Make a GET request to a BC endpoint
    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await _httpClient.GetAsync(url);
    }

    // Make a POST request to a BC endpoint
    public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
    {
        return await _httpClient.PostAsync(url, content);
    }

    // Make a PATCH request to a BC endpoint
    public async Task<HttpResponseMessage> PatchAsync(string url, HttpContent content)
    {
        var method = new HttpMethod("PATCH");
        var request = new HttpRequestMessage(method, url) { Content = content };
        return await _httpClient.SendAsync(request);
    }

    //public class Salutation
    //{
    //    public string Code { get; set; } = string.Empty;
    //    public string Description { get; set; } = string.Empty;
    //}

    //public async Task<List<Salutation>> GetSalutationsAsync()
    //{
    //    var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/Insuredcard"; // Fully encoded URL
    //    var response = await _httpClient.GetAsync(url);

    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Failed to fetch salutations: HTTP {(int)response.StatusCode}");

    //    var content = await response.Content.ReadAsStringAsync();

    //    using var doc = JsonDocument.Parse(content);
    //    var root = doc.RootElement;

    //    var values = root.GetProperty("value");

    //    var result = new List<Salutation>();
    //    foreach (var item in values.EnumerateArray())
    //    {
    //        result.Add(new Salutation
    //        {
    //            Code = item.GetProperty("Code").GetString() ?? "",
    //            Description = item.GetProperty("Description").GetString() ?? ""
    //        });
    //    }
    //    return result;
    //}

    //public class CountryRegion
    //{
    //    public string Code { get; set; } = string.Empty;
    //    public string Name { get; set; } = string.Empty;
    //}

    //public async Task<List<CountryRegion>> GetCountriesAsync()
    //{
    //    var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/CountriesAndRegions";
    //    var response = await _httpClient.GetAsync(url);

    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Failed to fetch countries: HTTP {(int)response.StatusCode}");

    //    var content = await response.Content.ReadAsStringAsync();

    //    using var doc = JsonDocument.Parse(content);
    //    var root = doc.RootElement;
    //    var values = root.GetProperty("value");

    //    var result = new List<CountryRegion>();
    //    foreach (var item in values.EnumerateArray())
    //    {
    //        result.Add(new CountryRegion
    //        {
    //            Code = item.GetProperty("Code").GetString() ?? "",
    //            Name = item.GetProperty("Name").GetString() ?? ""
    //        });
    //    }
    //    return result;
    //}

    //public class PostalCodeVm
    //{
    //    public string Code { get; set; } = string.Empty;
    //    public string City { get; set; } = string.Empty;
    //    public string Country_Region_Code { get; set; } = string.Empty;
    //}

    //public async Task<List<PostalCodeVm>> GetPostalCodesAsync()
    //{
    //    var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/PostalCodes";
    //    var response = await _httpClient.GetAsync(url);

    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Failed to fetch postal codes: HTTP {(int)response.StatusCode}");

    //    var content = await response.Content.ReadAsStringAsync();

    //    using var doc = JsonDocument.Parse(content);
    //    var root = doc.RootElement;
    //    var values = root.GetProperty("value");

    //    var result = new List<PostalCodeVm>();
    //    foreach (var item in values.EnumerateArray())
    //    {
    //        result.Add(new PostalCodeVm
    //        {
    //            Code = item.GetProperty("Code").GetString() ?? "",
    //            City = item.GetProperty("City").GetString() ?? "",
    //            Country_Region_Code = item.GetProperty("Country_Region_Code").GetString() ?? ""
    //        });
    //    }
    //    return result;
    //}

    //public class SourceOfFundsVm
    //{
    //    public string Source { get; set; } = string.Empty;
    //}

    //public async Task<List<SourceOfFundsVm>> GetSourcesOfFundsAsync()
    //{
    //    var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/Insuredcard";
    //    var response = await _httpClient.GetAsync(url);

    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Failed to fetch sources of funds: HTTP {(int)response.StatusCode}");

    //    var content = await response.Content.ReadAsStringAsync();

    //    using var doc = JsonDocument.Parse(content);
    //    var root = doc.RootElement;
    //    var values = root.GetProperty("value");

    //    var result = new HashSet<string>();
    //    foreach (var item in values.EnumerateArray())
    //    {
    //        var src = item.GetProperty("Source_of_Funds").GetString();
    //        if (!string.IsNullOrWhiteSpace(src))
    //            result.Add(src);
    //    }
    //    return result.Select(s => new SourceOfFundsVm { Source = s }).ToList();
    //}

    //public class MeansOfIdentificationVm
    //{
    //    public string Means_of_ID { get; set; } = string.Empty;
    //    public string Description { get; set; } = string.Empty;
    //}

    //public async Task<List<MeansOfIdentificationVm>> GetMeansOfIdentificationAsync()
    //{
    //    var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/MeansofIdentification";
    //    var response = await _httpClient.GetAsync(url);
    //    if (!response.IsSuccessStatusCode)
    //        throw new Exception($"Failed to fetch means of identification: HTTP {(int)response.StatusCode}");

    //    var json = await response.Content.ReadAsStringAsync();
    //    using var doc = JsonDocument.Parse(json);
    //    var values = doc.RootElement.GetProperty("value");

    //    var result = new List<MeansOfIdentificationVm>();
    //    foreach (var item in values.EnumerateArray())
    //    {
    //        result.Add(new MeansOfIdentificationVm
    //        {
    //            Means_of_ID = item.GetProperty("Means_of_ID").GetString() ?? "",
    //            Description = item.GetProperty("Description").GetString() ?? ""
    //        });

    //    }
    //    return result;
    //}

    public async Task<IndividualKyc?> GetIndividualKycStatusAsync(string userId)
    {
        var url = $"http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/Insuredcard";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var kyc = JsonSerializer.Deserialize<IndividualKyc>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return kyc;
    }
}
