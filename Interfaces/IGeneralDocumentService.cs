using MOZ_UPGRADE.Utils;
using System.Text.Json;
using System.Text;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IGeneralDocumentService
    {
        Task<string> AttachCustomerDocumentAsync(string customerNo, byte[] fileBytes, string fileName, string documentCode);
        Task<string> AttachIndividualDocumentAsync(string contactNo, byte[] fileBytes, string fileName, string documentCode);
    }

    public class GeneralDocumentService : IGeneralDocumentService
    {
        private readonly HttpClient _httpClient;
        private readonly BusinessCentralConfig _config;

        public GeneralDocumentService(IConfiguration configuration)
        {
            _config = new BusinessCentralConfig(configuration);
            var handler = _config.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            _config.ConfigureHttpClient(_httpClient);
        }

        public async Task<string> AttachCustomerDocumentAsync(string customerNo, byte[] fileBytes, string fileName, string documentCode)
        {
            var base64 = Convert.ToBase64String(fileBytes);
            var configured = _config.GetEndpoint("AttachCustomerDocument");
            var candidates = new List<string?>
            {
                configured,
                // Common unbound action name
                "PortalIntegration_AttachCustomerDocument",
                // Some environments use Corporate naming
                "PortalIntegration_AttachCorporateDocument",
                // Raw fallbacks with explicit company query
                "raw:PortalIntegration_AttachCustomerDocument?company=STANDARD%20INSURANCE",
                "raw:PortalIntegration_AttachCorporateDocument?company=STANDARD%20INSURANCE",
                // Bound action variants (if exposed on Customer/ContactCard)
                "Customer('{customerNo}')/PortalIntegration_AttachCustomerDocument",
                "Customer('{customerNo}')/PortalIntegration_AttachCorporateDocument",
                "ContactCard('{customerNo}')/PortalIntegration_AttachCustomerDocument",
                "ContactCard('{customerNo}')/PortalIntegration_AttachCorporateDocument"
            };

            var attempts = new List<string>();
            foreach (var ep in candidates)
            {
                if (string.IsNullOrWhiteSpace(ep)) continue;
                var endpoint = ep;
                if (endpoint.Contains("{customerNo}", StringComparison.OrdinalIgnoreCase))
                {
                    endpoint = endpoint.Replace("{customerNo}", Uri.EscapeDataString(customerNo), StringComparison.OrdinalIgnoreCase);
                }

                var url = endpoint.StartsWith("raw:", StringComparison.OrdinalIgnoreCase)
                    ? _config.BaseUrl + endpoint.Substring(4)
                    : _config.BuildUrl(endpoint);

                try
                {
                    var payload = new { customerNo, base64Content = base64, fileName, documentCode };
                    var json = JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    var body = await response.Content.ReadAsStringAsync();
                    attempts.Add($"{url} -> {(int)response.StatusCode} {response.ReasonPhrase} :: {body}");
                }
                catch (Exception ex)
                {
                    attempts.Add($"{url} -> EX: {ex.Message}");
                }
            }

            throw new Exception("All upload endpoint attempts failed:\n" + string.Join("\n", attempts));
        }

        public async Task<string> AttachIndividualDocumentAsync(string contactNo, byte[] fileBytes, string fileName, string documentCode)
        {
            // Try endpoints in order: configured, unbound default, raw default, bound to ContactCard
            var configured = _config.GetEndpoint("AttachIndividualDocument");
            var candidates = new List<string?>
            {
                configured,
                "PortalIntegration_AttachIndividualDocument",
                "raw:PortalIntegration_AttachIndividualDocument?company=STANDARD%20INSURANCE",
                "ContactCard('{contactNo}')/PortalIntegration_AttachIndividualDocument",
                // Also try customer endpoint as some setups only expose that
                "PortalIntegration_AttachCustomerDocument",
                "raw:PortalIntegration_AttachCustomerDocument?company=STANDARD%20INSURANCE"
            };

            var attempts = new List<string>();
            foreach (var ep in candidates)
            {
                if (string.IsNullOrWhiteSpace(ep)) continue;
                var endpoint = ep;
                if (endpoint.Contains("{contactNo}", StringComparison.OrdinalIgnoreCase))
                {
                    endpoint = endpoint.Replace("{contactNo}", Uri.EscapeDataString(contactNo), StringComparison.OrdinalIgnoreCase);
                }
                var url = endpoint.StartsWith("raw:", StringComparison.OrdinalIgnoreCase)
                    ? _config.BaseUrl + endpoint.Substring(4)
                    : _config.BuildUrl(endpoint);

                try
                {
                    // Build payload depending on target action signature
                    var base64 = Convert.ToBase64String(fileBytes);
                    object payloadObj;
                    if (endpoint.IndexOf("AttachCustomerDocument", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        payloadObj = new { customerNo = contactNo, base64Content = base64, fileName, documentCode };
                    }
                    else
                    {
                        payloadObj = new { contactNo, base64Content = base64, fileName, documentCode };
                    }
                    var json = JsonSerializer.Serialize(payloadObj);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    var body = await response.Content.ReadAsStringAsync();
                    attempts.Add($"{url} -> {(int)response.StatusCode} {response.ReasonPhrase} :: {body}");
                }
                catch (Exception ex)
                {
                    attempts.Add($"{url} -> EX: {ex.Message}");
                }
            }

            throw new Exception("All upload endpoint attempts failed:\n" + string.Join("\n", attempts));
        }
    }
}
