using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IContactCardService
    {
        Task<string?> CreateOrUpdateContactCardAsync(IndividualKyc kyc);
        Task<IndividualKyc?> GetContactCardAsync(string contactNo);
    }

    public class ContactCardService : IContactCardService
    {
        private readonly BusinessCentralConfig _config;
        private readonly ILogger<ContactCardService> _logger;
        private readonly HttpClient _httpClient;

        public ContactCardService(IConfiguration configuration, ILogger<ContactCardService> logger)
        {
            _config = new BusinessCentralConfig(configuration);
            _logger = logger;
            
            // Create reusable HttpClient with connection pooling
            var handler = _config.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            _config.ConfigureHttpClient(_httpClient);
        }

        private static string MapKycStatusToBcString(KycStatus status)
        {
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

        public async Task<string?> CreateOrUpdateContactCardAsync(IndividualKyc kyc)
        {
            try
            {
                var endpoint = "ContactCard";
                var url = _config.BuildUrl(endpoint);

                HttpResponseMessage response;
                var isCreate = string.IsNullOrEmpty(kyc.No);
                if (isCreate)
                {
                    // Create payload: include Approval_Status (defaults to Open)
                    var payloadCreate = new
                    {
                        Contact_Type = kyc.Contact_Type ?? "individual",
                        Title = kyc.Title,
                        First_name = kyc.First_name,
                        Last_Name = kyc.Last_Name,
                        Other_Name = kyc.Other_Name,
                        Sex = kyc.Sex,
                        Date_of_Birth = kyc.Date_of_Birth,
                        Marital_Status = kyc.Marital_Status,
                        Occupation = kyc.Occupation,
                        Nationality = kyc.Nationality,
                        BVN_No = kyc.BVN_No,
                        Means_of_Identification = kyc.Means_of_Identification,
                        ID_Number = kyc.ID_Number,
                        ID_Expiry_Date = kyc.ID_Expiry_Date,
                        Customer_Category = kyc.Customer_Category ?? "Non-Staff",
                        High_Risk_Low_Risk = kyc.High_Risk_Low_Risk ?? "Low Risk",
                        Accept_Marketing_Communication = kyc.Accept_Marketing_Communication,
                        Data_Protection_Consent = kyc.Data_Protection_Consent,
                        Next_of_Kin_Title = kyc.Next_of_Kin_Title,
                        Next_of_kin_Name = kyc.Next_of_kin_Name,
                        Next_of_Kin_Gender = kyc.Next_of_Kin_Gender,
                        Next_of_kin_Email = kyc.Next_of_kin_Email,
                        Next_of_Kin_Phone_No = kyc.Next_of_Kin_Phone_No,
                        Next_of_kin_address = kyc.Next_of_kin_address,
                        Next_of_kin_DOB = kyc.Next_of_kin_DOB,
                        Next_of_Kin_Relationship = kyc.Next_of_Kin_Relationship,
                        Address = kyc.Address,
                        Address_2 = kyc.Address_2,
                        Country_Region_Code = kyc.Country_Region_Code,
                        Post_Code = kyc.Post_Code,
                        City = kyc.City,
                        County = kyc.County,
                        Mobile_Phone_No = kyc.Mobile_Phone_No,
                        Phone_No = kyc.Phone_No,
                        E_Mail = kyc.E_Mail,
                        E_Mail_2 = kyc.E_Mail_2,
                        Fax_No = kyc.Fax_No,
                        Home_Page = kyc.Home_Page,
                        Language_Code = kyc.Language_Code,
                        Currency_Code = kyc.Currency_Code,
                        Territory_Code = kyc.Territory_Code,
                        VAT_Registration_No = kyc.VAT_Registration_No,
                        PEP_Status = kyc.PEP_Status,
                        Source_of_Funds = kyc.Source_of_Funds,
                        Employer_Name = kyc.Employer_Name,
                        Stage = "KYC",
                        Type = "Person",
                        Application_Status = MapKycStatusToBcString(kyc.Application_Status),
                    };
                    var content = JsonContent.Create(payloadCreate);
                    // Create new - fire and forget logging
                    response = await _httpClient.PostAsync(url, content);
                }
                else
                {
                    // Update existing
                    var updateUrl = $"{url}('{Uri.EscapeDataString(kyc.No)}')";
                    // Update payload: exclude Approval_Status (read-only) to avoid BadRequest
                    var payloadUpdate = new
                    {
                        Contact_Type = kyc.Contact_Type ?? "individual",
                        Title = kyc.Title,
                        First_name = kyc.First_name,
                        Last_Name = kyc.Last_Name,
                        Other_Name = kyc.Other_Name,
                        Sex = kyc.Sex,
                        Date_of_Birth = kyc.Date_of_Birth,
                        Marital_Status = kyc.Marital_Status,
                        Occupation = kyc.Occupation,
                        Nationality = kyc.Nationality,
                        BVN_No = kyc.BVN_No,
                        Means_of_Identification = kyc.Means_of_Identification,
                        ID_Number = kyc.ID_Number,
                        ID_Expiry_Date = kyc.ID_Expiry_Date,
                        Customer_Category = kyc.Customer_Category ?? "Non-Staff",
                        High_Risk_Low_Risk = kyc.High_Risk_Low_Risk ?? "Low Risk",
                        Accept_Marketing_Communication = kyc.Accept_Marketing_Communication,
                        Data_Protection_Consent = kyc.Data_Protection_Consent,
                        Next_of_Kin_Title = kyc.Next_of_Kin_Title,
                        Next_of_kin_Name = kyc.Next_of_kin_Name,
                        Next_of_Kin_Gender = kyc.Next_of_Kin_Gender,
                        Next_of_kin_Email = kyc.Next_of_kin_Email,
                        Next_of_Kin_Phone_No = kyc.Next_of_Kin_Phone_No,
                        Next_of_kin_address = kyc.Next_of_kin_address,
                        Next_of_kin_DOB = kyc.Next_of_kin_DOB,
                        Next_of_Kin_Relationship = kyc.Next_of_Kin_Relationship,
                        Address = kyc.Address,
                        Address_2 = kyc.Address_2,
                        Country_Region_Code = kyc.Country_Region_Code,
                        Post_Code = kyc.Post_Code,
                        City = kyc.City,
                        County = kyc.County,
                        Mobile_Phone_No = kyc.Mobile_Phone_No,
                        Phone_No = kyc.Phone_No,
                        E_Mail = kyc.E_Mail,
                        E_Mail_2 = kyc.E_Mail_2,
                        Fax_No = kyc.Fax_No,
                        Home_Page = kyc.Home_Page,
                        Language_Code = kyc.Language_Code,
                        Currency_Code = kyc.Currency_Code,
                        Territory_Code = kyc.Territory_Code,
                        VAT_Registration_No = kyc.VAT_Registration_No,
                        PEP_Status = kyc.PEP_Status,
                        Source_of_Funds = kyc.Source_of_Funds,
                        Employer_Name = kyc.Employer_Name,
                        Stage = "KYC",
                        Type = "Person",
                        Application_Status = MapKycStatusToBcString(kyc.Application_Status),
                    };
                    var content = JsonContent.Create(payloadUpdate);
                    using var request = new HttpRequestMessage(new HttpMethod("PATCH"), updateUrl)
                    {
                        Content = content
                    };
                    // Concurrency control: send If-Match with current etag or fallback to '*'
                    var etag = kyc.@odata_etag;
                    if (!string.IsNullOrWhiteSpace(etag))
                    {
                        request.Headers.TryAddWithoutValidation("If-Match", etag);
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation("If-Match", "*");
                    }
                    // Ask BC to return the representation including updated etag
                    request.Headers.TryAddWithoutValidation("Prefer", "return=representation");
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
                    _logger.LogError($"Failed to post contact card: {response.StatusCode}");
                    throw new Exception($"Failed to post contact card: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting contact card: {ex.Message}");
                throw;
            }
        }

        public async Task<IndividualKyc?> GetContactCardAsync(string contactNo)
        {
            try
            {
                var endpoint = $"ContactCard('{Uri.EscapeDataString(contactNo)}')";
                var url = _config.BuildUrl(endpoint);

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var kycRecord = JsonSerializer.Deserialize<IndividualKyc>(content, options);
                    return kycRecord;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get contact card: {response.StatusCode}");
                    throw new Exception($"Failed to get contact card: {response.StatusCode} - {errorContent}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting contact card: {ex.Message}");
                throw;
            }
        }
    }
}
