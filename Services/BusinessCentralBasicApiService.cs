using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
        var username = _config["BcApi:Administartor"];
        var password = _config["BcApi:Insurance@2030#"]; // Or use Web Service Access Key
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
}
