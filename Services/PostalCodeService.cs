//using Microsoft.Extensions.Configuration;
//using System.Net;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;

//public class PostalCodeService
//{
//    private readonly HttpClient _httpClient;

//    public PostalCodeService(IConfiguration config)
//    {
//        var handler = new HttpClientHandler
//        {
//            Credentials = new NetworkCredential(
//                config["BcApi:Administrator"],
//                config["BcApi:Insurance@2030#"])
//        };
//        _httpClient = new HttpClient(handler);
//    }

//    public class PostalCodeVm
//    {
//        public string Code { get; set; } = string.Empty;
//        public string City { get; set; } = string.Empty;
//        public string Country_Region_Code { get; set; } = string.Empty;
//    }

//    public async Task<List<PostalCodeVm>> GetPostalCodesAsync()
//    {
//        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/PostalCodes";
//        var response = await _httpClient.GetAsync(url);

//        if (!response.IsSuccessStatusCode)
//            throw new Exception($"Failed to fetch postal codes: HTTP {(int)response.StatusCode}");

//        var content = await response.Content.ReadAsStringAsync();
//        using var doc = JsonDocument.Parse(content);
//        var values = doc.RootElement.GetProperty("value");

//        var result = new List<PostalCodeVm>();
//        foreach (var item in values.EnumerateArray())
//        {
//            result.Add(new PostalCodeVm
//            {
//                Code = item.GetProperty("Code").GetString() ?? "",
//                City = item.GetProperty("City").GetString() ?? "",
//                Country_Region_Code = item.GetProperty("Country_Region_Code").GetString() ?? ""
//            });
//        }
//        return result;
//    }
//}
