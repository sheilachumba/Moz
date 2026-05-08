using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;
using System.Text;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IShareholderService
    {
        Task<List<ShareholderDto>> GetShareholdersAsync(string insuredNo);
        Task<ShareholderDto> CreateShareholderAsync(ShareholderDto shareholder);
        Task<ShareholderDto> UpdateShareholderAsync(ShareholderDto shareholder);
        Task DeleteShareholderAsync(int shareholderNo, string insuredNo);
        Task<string> AttachShareholderDocumentAsync(string insuredNo, int shareholderNo, byte[] fileBytes, string fileName, string documentCode = "DOC022");
        Task<List<ShareholderDocumentDto>> GetShareholderDocumentsAsync(string insuredNo, int shareholderNo);
        Task<byte[]> GetShareholderDocumentAsync(string insuredNo, int shareholderNo, string fileName);
    }

    public class ShareholderDto
    {
        public int Shareholder_No { get; set; }
        public string Insured_No { get; set; }
        public string Name { get; set; }
        public decimal Shareholder_Percent { get; set; }
        public string Means_of_Identification { get; set; }
    }

    public class ShareholderDocumentDto
    {
        public int Shareholder_No { get; set; }
        public string Insured_No { get; set; }
        public string Document_Name { get; set; }
        public string File_Name { get; set; }
        public string Document_Code { get; set; }
        public DateTime Upload_Date { get; set; }
    }

    public class ShareholderService : IShareholderService
    {
        private readonly HttpClient _httpClient;
        private readonly BusinessCentralConfig _config;

        public ShareholderService(IConfiguration configuration)
        {
            _config = new BusinessCentralConfig(configuration);
            var handler = _config.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            _config.ConfigureHttpClient(_httpClient);
        }

        public async Task<List<ShareholderDto>> GetShareholdersAsync(string insuredNo)
        {
            try
            {
                var baseUrl = _config.BuildUrl("InsuredShareholders");
                var url = $"{baseUrl}?$filter=Insured_No eq '{insuredNo}'";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odata = JsonSerializer.Deserialize<ODataResponse<ShareholderDto>>(content, options);
                return odata?.Value ?? new List<ShareholderDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return new List<ShareholderDto>();
            }
        }

        public async Task<ShareholderDto> CreateShareholderAsync(ShareholderDto shareholder)
        {
            var json = JsonSerializer.Serialize(shareholder);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var baseUrl = _config.BuildUrl("InsuredShareholders");
            var response = await _httpClient.PostAsync(baseUrl, content);
            response.EnsureSuccessStatusCode();
            return shareholder;
        }

        public async Task<ShareholderDto> UpdateShareholderAsync(ShareholderDto shareholder)
        {
            var json = JsonSerializer.Serialize(shareholder);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var baseUrl = _config.BuildUrl("InsuredShareholders");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{baseUrl}({shareholder.Shareholder_No})") { Content = content };
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return shareholder;
        }

        public async Task DeleteShareholderAsync(int shareholderNo, string insuredNo)
        {
            var baseUrl = _config.BuildUrl("InsuredShareholders");
            var response = await _httpClient.DeleteAsync($"{baseUrl}({shareholderNo})");
            response.EnsureSuccessStatusCode();
        }

        public async Task<string> AttachShareholderDocumentAsync(string insuredNo, int shareholderNo, byte[] fileBytes, string fileName, string documentCode = "DOC022")
        {
            var base64 = Convert.ToBase64String(fileBytes);
            var payload = new { shareholderNo = shareholderNo.ToString(), insuredNo, base64Content = base64, fileName, documentCode };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = _config.BaseUrl + "PortalIntegration_AttachShareholderDocument?company=STANDARD%20INSURANCE";
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<ShareholderDocumentDto>> GetShareholderDocumentsAsync(string insuredNo, int shareholderNo)
        {
            try
            {
                var baseUrl = _config.BuildUrl("Shareholder_Documents");
                var url = $"{baseUrl}?$filter=Insured_No eq '{insuredNo}' and Shareholder_No eq {shareholderNo}";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odata = JsonSerializer.Deserialize<ODataResponse<ShareholderDocumentDto>>(content, options);
                return odata?.Value ?? new List<ShareholderDocumentDto>();
            }
            catch
            {
                return new List<ShareholderDocumentDto>();
            }
        }

        public async Task<byte[]> GetShareholderDocumentAsync(string insuredNo, int shareholderNo, string fileName)
        {
            var url = _config.BaseUrl + $"PortalIntegration_GetShareholderDocumentContent?shareholderNo={shareholderNo}&fileName={fileName}&company=STANDARD%20INSURANCE";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
