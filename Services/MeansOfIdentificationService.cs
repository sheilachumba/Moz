//using Microsoft.Extensions.Configuration;
//using System.Net;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Collections.Generic;

//public class MeansOfIdentificationService
//{
//    private readonly HttpClient _httpClient;

//    public MeansOfIdentificationService(IConfiguration config)
//    {
//        var handler = new HttpClientHandler
//        {
//            Credentials = new NetworkCredential(
//                config["BcApi:Administrator"],
//                config["BcApi:Insurance@2030#"])
//        };
//        _httpClient = new HttpClient(handler);
//    }

//    public class MeansOfIdentificationVm
//    {
//        public string Means_of_ID { get; set; } = string.Empty;
//        public string Description { get; set; } = string.Empty;
//    }

//    public async Task<List<MeansOfIdentificationVm>> GetMeansOfIdentificationAsync()
//    {
//        var url = "http://196.201.224.102:2048/BC260/ODataV4/Company%28%27STANDARD%20INSURANCE%27%29/MeansofIdentification";
//        var response = await _httpClient.GetAsync(url);

//        if (!response.IsSuccessStatusCode)
//            throw new Exception($"Failed to fetch means of identification: HTTP {(int)response.StatusCode}");

//        var json = await response.Content.ReadAsStringAsync();
//        using var doc = JsonDocument.Parse(json);
//        var values = doc.RootElement.GetProperty("value");

//        var result = new List<MeansOfIdentificationVm>();
//        foreach (var item in values.EnumerateArray())
//        {
//            result.Add(new MeansOfIdentificationVm
//            {
//                Means_of_ID = item.GetProperty("Means_of_ID").GetString() ?? "",
//                Description = item.GetProperty("Description").GetString() ?? ""
//            });
//        }
//        return result;
//    }
//}
