using System.Net;
using System.Net.Http.Headers;

namespace MOZ_UPGRADE.Utils
{
    public class BusinessCentralConfig
    {
        private readonly IConfiguration _configuration;

        public BusinessCentralConfig(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string BaseUrl
        {
            get
            {
                var baseUrl = (_configuration["BcApi:BaseUrl"] ?? "http://pmoz-iberp1t.mz.sbicdirectory.com:7048/BC260/ODataV4/Company('STRD%20Insurance%20MOZ')").Trim();
                if (!baseUrl.EndsWith("/"))
                {
                    baseUrl += "/";
                }

                return baseUrl;
            }
        }
        public string Username => _configuration["BcApi:Username"] ?? @"SBICMZ01\EC919681T1";
        public string Password => _configuration["BcApi:Password"] ?? "modD9ehki\\}s";
        public string Domain => _configuration["BcApi:Domain"] ?? "";
        public string Company => _configuration["BcApi:Company"] ?? "STANDARD INSURANCE";
        public int Timeout => int.Parse(_configuration["BcApi:Timeout"] ?? "30");
        public string GetEndpoint(string endpointKey)
        {
            return _configuration[$"BcApi:Endpoints:{endpointKey}"] ?? endpointKey;
        }

        public HttpClientHandler CreateHttpClientHandler()
        {
            var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(Username, Password, Domain)
            };

            return handler;
        }

        public string BuildUrl(string endpoint)
        {
            var encodedCompany = Uri.EscapeDataString(Company);
            if (BaseUrl.Contains("Company(", StringComparison.OrdinalIgnoreCase))
            {
                return $"{BaseUrl.TrimEnd('/')}/{endpoint}";
            }

            return $"{BaseUrl}Company('{encodedCompany}')/{endpoint}";
        }

        public void ConfigureHttpClient(HttpClient client)
        {
            client.Timeout = TimeSpan.FromSeconds(Timeout);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
