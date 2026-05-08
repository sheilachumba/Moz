using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Utils
{
    public class SelectedProductService : ISelectedProductService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SelectedProductService> _logger;

        public SelectedProductService(HttpClient client, IConfiguration configuration, ILogger<SelectedProductService> logger)
        {
            _client = client;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<(bool Success, string? Error, string? ResponseBody)> CreateSelectedProductAsync(SelectedProduct product, CancellationToken cancellationToken = default)
        {
            try
            {
                // Align with KYC pattern via BusinessCentralConfig
                var bc = new BusinessCentralConfig(_configuration);
                var url = bc.BuildUrl("SelectedProduct");

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var payload = new
                {
                    Contact_No = product.Contact_No,
                    Policy_No = product.Policy_No,
                    Class_Name = product.Class_Name,
                    Description = product.Description,
                    Underwriter_Code = product.Underwriter_Code,
                    Policy_Start_Date = product.Policy_Start_Date.ToString("yyyy-MM-dd"),
                    Policy_End_Date = product.Policy_End_Date.ToString("yyyy-MM-dd"),
                    First_Name = product.First_Name,
                    Family_Name = product.Family_Name,
                    Relationship_to_Applicant = product.Relationship_to_Applicant,
                    Age = product.Age,
                    Chronic_Status = product.Chronic_Status,
                    Occupation = product.Occupation,
                    Session_ID = product.Session_ID,
                    Selected = product.Selected
                };

                var json = JsonSerializer.Serialize(payload, options);
                _logger.LogInformation("[SelectedProductService] POST {Url} Payload: {Payload}", url, json);

                // Use the same auth strategy as KYC services
                var handler = bc.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                bc.ConfigureHttpClient(client);

                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                request.Headers.TryAddWithoutValidation("OData-Version", "4.0");
                request.Headers.TryAddWithoutValidation("OData-MaxVersion", "4.0");

                var response = await client.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null, body);
                }
                _logger.LogError("[SelectedProductService] Failed: {Status} {Body}", response.StatusCode, body);
                return (false, response.StatusCode.ToString(), body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SelectedProductService] Exception");
                return (false, ex.Message, null);
            }
        }
    }
}
