using MOZ_UPGRADE.Utils;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IPepService
    {
        Task<List<PepDto>> GetPepsAsync(string insuredNo);
        Task<string> CreatePepAsync(string insuredNo, PepDto pep);
        Task<string> UpdatePepAsync(string insuredNo, int pepNo, PepDto pep);
        Task DeletePepAsync(string insuredNo, int pepNo);
        Task<string> AttachPepDocumentAsync(
            string insuredNo,
            int pepNo,
            byte[] fileBytes,
            string fileName,
            string documentCode = "DOC022");
        Task<byte[]> GetPepDocumentContentAsync(
            string insuredNo,
            int pepNo,
            int documentId);
    }

    public class PepDto
    {
        public int No { get; set; }
        public string Pep_No { get; set; }
        public string Name { get; set; }
        public string Office_Held { get; set; }
        public string Relationship { get; set; }
    }

    public class PepService : IPepService
    {
        private readonly HttpClient _httpClient;
        private readonly BusinessCentralConfig _config;
        private readonly string _baseUrl;
        private readonly PortalDocumentSoapService _soapService;

        public PepService(IConfiguration configuration)
        {
            _config = new BusinessCentralConfig(configuration);
            var handler = _config.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            
            // Initialize SOAP service for document uploads
            var soapEndpoint = configuration["BcSoap:Endpoint"]
                ?? "http://sicds:2048/BC260/WS/STANDARD%20INSURANCE/Codeunit/PortalIntegration";
            var username = configuration["BcSoap:Username"] ?? configuration["BcApi:Username"] ?? @"SBICMZ01\EC919681T1";
            var password = configuration["BcSoap:Password"] ?? configuration["BcApi:Password"] ?? "modD9ehki\\}s";
            var cred = new NetworkCredential(username, password);
            _soapService = new PortalDocumentSoapService(soapEndpoint, cred);
            
            // Build base URL from configuration
            var bcBaseUrl = configuration["BcApi:BaseUrl"];
            var endpoint = configuration["BcApi:Endpoints:PepCard"] ?? "PepCard";
            _baseUrl = $"{bcBaseUrl}{endpoint}";
        }

        public async Task<List<PepDto>> GetPepsAsync(string insuredNo)
        {
            try
            {
                var url = $"{_baseUrl}?$filter=Pep_No eq '{insuredNo}'&company=STANDARD%20INSURANCE";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var content = JsonSerializer.Deserialize<ODataResponse<PepDto>>(jsonContent, options);
                
                var peps = content?.Value ?? new List<PepDto>();
                return peps;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving PEPs: {ex.Message}");
                throw;
            }
        }

        public async Task<string> CreatePepAsync(string insuredNo, PepDto pep)
        {
            try
            {
                var payload = new
                {
                    Pep_No = insuredNo,
                    Name = pep.Name,
                    Office_Held = pep.Office_Held
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}?company=STANDARD%20INSURANCE";

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"PEP creation failed: {response.StatusCode} - {error}");
                }

                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating PEP: {ex.Message}");
                throw;
            }
        }

        public async Task<string> UpdatePepAsync(string insuredNo, int pepNo, PepDto pep)
        {
            try
            {
                var payload = new
                {
                    Pep_No = insuredNo,
                    Name = pep.Name,
                    Office_Held = pep.Office_Held
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}({pepNo})?company=STANDARD%20INSURANCE";

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"PEP update failed: {response.StatusCode} - {error}");
                }

                var result = await response.Content.ReadAsStringAsync();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating PEP: {ex.Message}");
                throw;
            }
        }

        public async Task DeletePepAsync(string insuredNo, int pepNo)
        {
            try
            {
                var url = $"{_baseUrl}({pepNo})?company=STANDARD%20INSURANCE";
                var response = await _httpClient.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"PEP deletion failed: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting PEP: {ex.Message}");
                throw;
            }
        }

        public async Task<string> AttachPepDocumentAsync(
            string insuredNo,
            int pepNo,
            byte[] fileBytes,
            string fileName,
            string documentCode = "DOC022")
        {
            var base64Content = Convert.ToBase64String(fileBytes);
            var payload = new
            {
                pepRecordNo = pepNo.ToString(),
                pepNo = insuredNo,
                base64Content = base64Content,
                fileName = fileName,
                documentCode = documentCode
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = _config.BaseUrl + "PortalIntegration_AttachPepDocument?company=STANDARD%20INSURANCE";

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OData PEP document upload failed: {response.StatusCode} - {error}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<byte[]> GetPepDocumentContentAsync(
            string insuredNo,
            int pepNo,
            int documentId)
        {
            try
            {
                var url = _config.BaseUrl + $"PortalIntegration_GetPepDocumentContent?pepNo={pepNo}&documentId={documentId}&company=STANDARD%20INSURANCE";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to retrieve PEP document: {response.StatusCode} - {error}");
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                return content;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving PEP document content: {ex.Message}");
                throw;
            }
        }
    }
}
