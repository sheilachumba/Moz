using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ICorporateContactCardService
    {
        Task<string?> CreateOrUpdateContactCardAsync(CorporateKyc kyc);
        Task<CorporateKyc?> GetContactCardAsync(string contactNo);
    }

    public class CorporateContactCardService : ICorporateContactCardService
    {
        private readonly BusinessCentralConfig _config;
        private readonly HttpClient _httpClient;

        public CorporateContactCardService(IConfiguration configuration)
        {
            _config = new BusinessCentralConfig(configuration);
            
            // Create reusable HttpClient with connection pooling
            var handler = _config.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            _config.ConfigureHttpClient(_httpClient);
        }

        public async Task<string?> CreateOrUpdateContactCardAsync(CorporateKyc kyc)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(kyc.Name))
                {
                    throw new Exception("Name is required for contact card");
                }

                var endpoint = "ContactCard";
                var url = _config.BuildUrl(endpoint);

                // Build the payload for corporate
                // Note: Approval_Status is read-only in BC, so exclude it from updates
                var payload = new
                {
                    Contact_Type = kyc.Contact_Type ?? "Corporate",
                    // Title is omitted for corporate (not applicable for company contacts)
                    Name = kyc.Name,
                    Type = kyc.Type,
                    Company_Registration = kyc.Company_Registration,
                    Tax_ID = kyc.Tax_ID ?? "", // Provide empty string if null to satisfy BC requirement
                    Date_of_Incorporation = kyc.Date_of_Incorporation,
                    Sector_Description = kyc.Sector_Description,
                    Company_Representative = kyc.Company_Representative,
                    Legal_Representative = kyc.Legal_Representative,
                    Means_of_Identification = kyc.Means_of_Identification,
                    // Approval_Status is read-only in BC - do NOT include in payload
                    Customer_Category = kyc.Customer_Category ?? "Non-Staff",
                    High_Risk_Low_Risk = kyc.High_Risk_Low_Risk ?? "Low Risk",
                    PEP_Status = kyc.PEP_Status,
                    Source_of_Funds = kyc.Source_of_Funds,
                    Address = kyc.Address,
                    Address_2 = kyc.Address_2,
                    Country_Region_Code = kyc.Country_Region_Code,
                    Post_Code = kyc.Post_Code,
                    City = kyc.City,
                    County = kyc.County,
                    Phone_No = kyc.Phone_No,
                    E_Mail = kyc.E_Mail,
                    E_Mail_2 = kyc.E_Mail_2,
                    Fax_No = kyc.Fax_No,
                    Home_Page = kyc.Home_Page,
                    Language_Code = kyc.Language_Code,
                    Currency_Code = kyc.Currency_Code,
                    Territory_Code = kyc.Territory_Code,
                    VAT_Registration_No = kyc.VAT_Registration_No,
                    // Map Tax ID (NUIT) into BVN_No field on the ContactCard
                    BVN_No = kyc.VAT_Registration_No,
                    Next_of_kin_Name = kyc.Next_of_kin_Name,
                    Next_of_kin_Email = kyc.Next_of_kin_Email,
                    Next_of_Kin_Phone_No = kyc.Next_of_Kin_Phone_No,
                    Stage = "KYC",
                    Application_Status = MapKycStatusToBcString(kyc.Application_Status),
                };

                var content = JsonContent.Create(payload);

                HttpResponseMessage response;
                if (string.IsNullOrEmpty(kyc.No))
                {
                    // Create new
                    response = await _httpClient.PostAsync(url, content);
                }
                else
                {
                    // Update existing - need to include etag for concurrency
                    var updateUrl = $"{url}('{Uri.EscapeDataString(kyc.No)}')";
                    var request = new HttpRequestMessage(HttpMethod.Patch, updateUrl)
                    {
                        Content = content
                    };
                    
                    // Add etag header if available
                    if (!string.IsNullOrEmpty(kyc.@odata_etag))
                    {
                        request.Headers.Add("If-Match", kyc.@odata_etag);
                    }
                    else
                    {
                        // If no etag, use wildcard to allow any version
                        request.Headers.Add("If-Match", "*");
                    }
                    
                    response = await _httpClient.SendAsync(request);
                }

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var result = JsonSerializer.Deserialize<JsonElement>(responseContent, options);
                    
                    if (result.TryGetProperty("No", out var noProperty))
                    {
                        return noProperty.GetString();
                    }

                    return kyc.No;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to post contact card: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CorporateKyc?> GetContactCardAsync(string contactNo)
        {
            try
            {
                // Use OData filter to get by No - returns collection wrapped in "value"
                var baseUrl = _config.BuildUrl("ContactCard");
                var url = $"{baseUrl}?$filter=No eq '{Uri.EscapeDataString(contactNo)}'";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    
                    // Parse OData response with "value" wrapper
                    var odataResponse = await JsonSerializer.DeserializeAsync<ODataResponse<CorporateKyc>>(stream, options);

                    if (odataResponse?.Value != null && odataResponse.Value.Count > 0)
                    {
                        var kycRecord = odataResponse.Value.FirstOrDefault();
                        return kycRecord;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to get contact card: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // Helper class to map OData "value" wrapper
        private class ODataResponse<T>
        {
            public List<T> Value { get; set; } = new();
        }

        private static string MapKycStatusToBcString(KycStatus status)
        {
            // BC expects: Open, Pending Approval, Approved, Rejected
            return status switch
            {
                KycStatus.Open => "Open",
                KycStatus.Pending_Approval => "Pending Approval",
                KycStatus.Approved => "Approved",
                KycStatus.Rejected => "Rejected",
                KycStatus.Submitted => "Submitted",
                _ => "Open"
            };
        }
    }
}
