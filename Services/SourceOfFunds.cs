//using Microsoft.Extensions.Configuration;
//using System.Net;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using System.Linq;

//public class SourceOfFundsService
//{
//    private readonly HttpClient _httpClient;

//    public SourceOfFundsService(IConfiguration config)
//    {
//        var handler = new HttpClientHandler
//        {
//            Credentials = new NetworkCredential(
//                config["BcApi:Administrator"],
//                config["BcApi:Insurance@2030#"])
//        };
//        _httpClient = new HttpClient(handler);
//    }

//    public class SourceOfFundsVm
//    {
//        public string Source { get; set; } = string.Empty;
//    }

//    public async Task<List<SourceOfFundsVm>> GetSourcesOfFundsAsync()
//    {
//        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/Insuredcard";
//        var response = await _httpClient.GetAsync(url);

//        if (!response.IsSuccessStatusCode)
//            throw new Exception($"Failed to fetch sources of funds: HTTP {(int)response.StatusCode}");

//        var content = await response.Content.ReadAsStringAsync();
//        using var doc = JsonDocument.Parse(content);
//        var values = doc.RootElement.GetProperty("value");

//        var result = new HashSet<string>();
//        foreach (var item in values.EnumerateArray())
//        {
//            var src = item.GetProperty("Source_of_Funds").GetString();
//            if (!string.IsNullOrWhiteSpace(src))
//                result.Add(src);
//        }
//        return result.Select(s => new SourceOfFundsVm { Source = s }).ToList();
//    }
//}
