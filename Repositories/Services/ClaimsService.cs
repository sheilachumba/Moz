using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Repositories.Services
{
    public class ClaimsService : IClaimsService
    {
        private readonly HttpClient _httpClient;
        private readonly BusinessCentralConfig _cfg;
        private readonly ILogger<ClaimsService> _logger;

        public ClaimsService(IConfiguration configuration, ILogger<ClaimsService> logger)
        {
            _logger = logger;
            _cfg = new BusinessCentralConfig(configuration);
            var handler = _cfg.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            _cfg.ConfigureHttpClient(_httpClient);
        }

        public async Task<List<LossTypeItem>> GetLossTypesAsync()
        {
            var url = _cfg.BuildUrl("LossTypeList");
            var items = new List<LossTypeItem>();
            try
            {
                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("LossTypeList fetch failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return items;
                }
                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        items.Add(new LossTypeItem
                        {
                            Code = GetString(el, "Code"),
                            Description = GetString(el, "Description")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching LossTypeList");
            }
            return items;
        }

        public async Task<string?> CreateClaimAsync(ClaimCreateRequest request)
        {
            var url = _cfg.BuildUrl("Claimscsrdpage");
            var payload = JsonSerializer.Serialize(request);
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var res = await _httpClient.PostAsync(url, content);
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateClaim failed: {Status} - {Reason}. Body: {Body}", (int)res.StatusCode, res.ReasonPhrase, body);
                throw new Exception($"Create claim failed: {(int)res.StatusCode} {res.ReasonPhrase} :: {body}");
            }

            // BC OData usually returns the created entity payload. Try to extract Claim_No.
            var responseBody = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                if (doc.RootElement.ValueKind == JsonValueKind.Object
                    && doc.RootElement.TryGetProperty("Claim_No", out var claimNoProp))
                {
                    var claimNo = claimNoProp.ValueKind == JsonValueKind.String ? claimNoProp.GetString() : claimNoProp.ToString();
                    return string.IsNullOrWhiteSpace(claimNo) ? null : claimNo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not parse CreateClaim response body: {Body}", responseBody);
            }

            return null;
        }

        public async Task<List<ClaimListItem>> GetClaimsAsync(string insuredNo = null)
        {
            var url = _cfg.BuildUrl("Claimscsrdpage");
            var list = new List<ClaimListItem>();
            try
            {
                // optional filter by InsuredNo if provided
                if (!string.IsNullOrWhiteSpace(insuredNo))
                {
                    var safe = insuredNo.Replace("'", "''");
                    var filter = Uri.EscapeDataString($"InsuredNo eq '{safe}'");
                    url += $"?$filter={filter}";
                }

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Claims list fetch failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return list;
                }
                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        list.Add(new ClaimListItem
                        {
                            Claim_No = GetString(el, "Claim_No"),
                            Policy_No = GetString(el, "Policy_No"),
                            Name_of_Insured = GetString(el, "Name_of_Insured"),
                            Premium_Balance = GetDecimal(el, "Premium_Balance"),
                            Approval_Status = GetString(el, "Approval_Status"),
                            Claim_Status = GetString(el, "Claim_Status"),
                            Renewal_Date = GetString(el, "Renewal_Date"),
                            ID_Number = GetString(el, "ID_Number"),
                            Insured_Telephone_No = GetString(el, "Insured_Telephone_No"),
                            Date_of_Birth = GetString(el, "Date_of_Birth"),
                            Sex = GetString(el, "Sex"),
                            Insured_Address = GetString(el, "Insured_Address"),
                            Occupation = GetString(el, "Occupation"),
                            Loss_Type = GetString(el, "Loss_Type"),
                            Loss_Type_Description = GetString(el, "Loss_Type_Description"),
                            Date_Notified = GetString(el, "Date_Notified"),
                            Claim_Stage = GetString(el, "Claim_Stage"),
                            Claim_Stage_Description = GetString(el, "Claim_Stage_Description"),
                            Class = GetString(el, "Class"),
                            Class_Name = GetString(el, "Class_Name"),
                            Risk_Description = GetString(el, "Risk_Description"),
                            Date_of_Occurence = GetString(el, "Date_of_Occurence"),
                            Estimated_Amount = GetDecimal(el, "Estimated_Amount"),
                            Offered_Amount = GetDecimal(el, "Offered_Amount"),
                            Expected_Amount = GetDecimal(el, "Expected_Amount"),
                            Amount_Settled = GetDecimal(el, "Amount_Settled")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claims list");
            }
            return list;
        }

        private static string GetString(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString(); } catch { }
            }
            return string.Empty;
        }
        private static decimal GetDecimal(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.GetDecimal(); } catch { }
                if (decimal.TryParse(prop.ToString(), out var d)) return d;
            }
            return 0m;
        }

        public async Task<List<ClaimDocument>> GetClaimDocumentsByCategoryAsync(string category)
        {
            var documents = new List<ClaimDocument>();
            try
            {
                // URL encode the category to handle special characters
                var encodedCategory = Uri.EscapeDataString(category);
                var url = $"{_cfg.BuildUrl("ClaimDocuments")}?$filter=Class_Category eq '{encodedCategory}' and Document_Type eq 'claim'";
                
                _logger.LogInformation("Fetching claim documents from: {Url}", url);
                
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch claim documents. Status: {StatusCode}, Reason: {Reason}", 
                        response.StatusCode, response.ReasonPhrase);
                    return documents;
                }

                var content = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                
                if (doc.RootElement.TryGetProperty("value", out var value) && value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in value.EnumerateArray())
                    {
                        documents.Add(new ClaimDocument
                        {
                            Document_Type = GetString(element, "Document_Type"),
                            Entry_No = element.TryGetProperty("Entry_No", out var entryNo) ? entryNo.GetInt32() : 0,
                            Document_No = GetString(element, "Document_No"),
                            Customer_No = GetString(element, "Customer_No"),
                            Document_Name = GetString(element, "Document_Name"),
                            Document_Path = GetString(element, "Document_Path"),
                            Enclosed = element.TryGetProperty("Enclosed", out var enclosed) && enclosed.GetBoolean(),
                            Class_Category = GetString(element, "Class_Category"),
                            To_Follow = element.TryGetProperty("To_Follow", out var toFollow) && toFollow.GetBoolean(),
                            Required = element.TryGetProperty("Required", out var required) && required.GetBoolean(),
                            Received = element.TryGetProperty("Received", out var received) && received.GetBoolean(),
                            Date_Required = element.TryGetProperty("Date_Required", out var dateReq) ? 
                                DateTime.Parse(dateReq.GetString()) : DateTime.MinValue,
                            Date_Received = element.TryGetProperty("Date_Received", out var dateRec) && 
                                           dateRec.ValueKind != JsonValueKind.Null ? 
                                           DateTime.Parse(dateRec.GetString()) : (DateTime?)null
                        });
                    }
                }
                
                _logger.LogInformation("Successfully fetched {Count} claim documents for category: {Category}", 
                    documents.Count, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching claim documents for category: {Category}", category);
            }
            
            return documents;
        }
    }
}
