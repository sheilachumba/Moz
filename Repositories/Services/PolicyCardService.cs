using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Utils;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Repositories.Services
{
    public class PolicyCardService : IPolicyCardService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _renewalUrl;
        private readonly BusinessCentralConfig _bcConfig;
        private readonly ILogger<PolicyCardService> _logger;

        public PolicyCardService(IConfiguration configuration, ILogger<PolicyCardService> logger)
        {
            _logger = logger;
            var cfg = new BusinessCentralConfig(configuration);
            var handler = cfg.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);

            _bcConfig = new BusinessCentralConfig(configuration);
            _baseUrl = _bcConfig.BuildUrl("PolicyCard");
            _renewalUrl = _bcConfig.BuildUrl("PortalIntegration_FnSubmitPolicyRenewal");
        }

        public async Task<List<PolicyCardItem>> GetPoliciesByQuotationAsync(string quotationNo)
        {
            var items = new List<PolicyCardItem>();
            try
            {
                if (string.IsNullOrWhiteSpace(quotationNo)) return items;
                string filterExpr;
                if (int.TryParse(quotationNo, out var numericQuotation))
                {
                    // Numeric field: do not wrap in quotes
                    filterExpr = $"Quotation_No eq {numericQuotation}";
                }
                else
                {
                    // String field: wrap in quotes and escape single quotes
                    var safe = (quotationNo ?? string.Empty).Replace("'", "''");
                    filterExpr = $"Quotation_No eq '{safe}'";
                }

                var filter = Uri.EscapeDataString(filterExpr);
                var select = Uri.EscapeDataString("No,Policy_Status,Quotation_No,Underwriter,Underwriter_Name,Insured_No,Insured_Name,Policy_Type,Policy_Description,From_Date,To_Date,Cover_Start_Date,Cover_End_Date,Cover_Period,Total_Sum_Insured,Risk_Description");
                var url = $"{_baseUrl}?$filter={filter}&$select={select}";

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PolicyCard fetch failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return items;
                }

                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        items.Add(new PolicyCardItem
                        {
                            No = GetString(el, "No"),
                            Policy_Status = GetString(el, "Policy_Status"),
                            Quotation_No = GetString(el, "Quotation_No"),
                            Underwriter = GetString(el, "Underwriter"),
                            Underwriter_Name = GetString(el, "Underwriter_Name"),
                            Insured_No = GetString(el, "Insured_No"),
                            Insured_Name = GetString(el, "Insured_Name"),
                            Policy_Type = GetString(el, "Policy_Type"),
                            Policy_Description = GetString(el, "Policy_Description"),
                            From_Date = GetString(el, "From_Date"),
                            To_Date = GetString(el, "To_Date"),
                            Cover_Start_Date = GetString(el, "Cover_Start_Date"),
                            Cover_End_Date = GetString(el, "Cover_End_Date"),
                            Cover_Period = GetInt(el, "Cover_Period"),
                            Total_Sum_Insured = GetDecimal(el, "Total_Sum_Insured"),
                            Risk_Description = GetString(el, "Risk_Description")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PolicyCard by quotation {QuotationNo}", quotationNo);
            }
            return items;
        }

        public async Task<(bool Success, string ErrorMessage)> SubmitPolicyRenewalAsync(PolicyCardItem policy)
        {
            if (policy == null)
                return (false, "Policy information is missing");

            try
            {
                // Business Central does not allow backdated policy start dates.
                // Derive a safe start/end date based on the existing period but
                // ensure the start is at least today.
                var today = DateTime.Today;
                DateTime? from = null;
                DateTime? to = null;

                if (DateTime.TryParse(policy.From_Date, out var fromParsed))
                    from = fromParsed.Date;
                if (DateTime.TryParse(policy.To_Date, out var toParsed))
                    to = toParsed.Date;

                var safeStart = from ?? today;
                if (safeStart < today)
                    safeStart = today;

                DateTime safeEnd;
                if (from.HasValue && to.HasValue && to.Value > from.Value)
                {
                    var span = to.Value - from.Value;
                    safeEnd = safeStart.Add(span);
                }
                else
                {
                    // Fallback: one-year period
                    safeEnd = safeStart.AddYears(1);
                }

                var payload = new
                {
                    quote_Noa46 = policy.Quotation_No ?? string.Empty,
                    insured_Noa46 = policy.Insured_No ?? string.Empty,
                    name = policy.Insured_Name ?? string.Empty,
                    policy_Type = policy.Policy_Type ?? string.Empty,
                    policy_Description = policy.Policy_Description ?? string.Empty,
                    agenta39s_Name = "SYSTEM",
                    starting_Date = safeStart.ToString("yyyy-MM-dd"),
                    ending_Date = safeEnd.ToString("yyyy-MM-dd"),
                    discount = 0m,
                    premium = 0m,
                    sum_Insured = policy.Total_Sum_Insured,
                    insurance_Cover_Typea46 = 2,
                    payment_mode = "BANK_",
                    policy_Number = policy.No ?? string.Empty
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _httpClient.PostAsync(_renewalUrl, content);
                var responseContent = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode)
                {
                    var errorMessage = $"Policy renewal failed with status code: {res.StatusCode} - {res.ReasonPhrase}";
                    try
                    {
                        // Try to parse the error response for more details
                        using var errorDoc = JsonDocument.Parse(responseContent);
                        if (errorDoc.RootElement.TryGetProperty("error", out var errorObj) &&
                            errorObj.TryGetProperty("message", out var message) && 
                            message.ValueKind == JsonValueKind.Object &&
                            message.TryGetProperty("value", out var value))
                        {
                            errorMessage = value.GetString() ?? errorMessage;
                        }
                    }
                    catch (JsonException)
                    {
                        // If we can't parse the error as JSON, include the raw response
                        if (!string.IsNullOrWhiteSpace(responseContent))
                        {
                            errorMessage = $"{errorMessage}. Response: {responseContent}";
                        }
                    }
                    
                    _logger.LogWarning("Policy renewal submit failed: {Message}", errorMessage);
                    return (false, errorMessage);
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An unexpected error occurred while processing your renewal request: {ex.Message}";
                _logger.LogError(ex, "Error submitting policy renewal for policy {PolicyNo}", policy?.No ?? "unknown");
                return (false, errorMessage);
            }
        }

        public async Task<List<PolicyCardItem>> GetPoliciesByInsuredNoAsync(string insuredNo)
        {
            var items = new List<PolicyCardItem>();
            try
            {
                if (string.IsNullOrWhiteSpace(insuredNo)) return items;
                var filterExpr = $"Insured_No eq '{(insuredNo ?? string.Empty).Replace("'", "''")}'";
                var filter = Uri.EscapeDataString(filterExpr);
                var select = Uri.EscapeDataString("No,Policy_Status,Quotation_No,Underwriter,Underwriter_Name,Insured_No,Insured_Name,Policy_Type,Policy_Description,From_Date,To_Date,Cover_Start_Date,Cover_End_Date,Cover_Period,Total_Sum_Insured,Risk_Description");
                var url = $"{_baseUrl}?$filter={filter}&$select={select}";

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PolicyCard fetch by insured failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return items;
                }

                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        items.Add(new PolicyCardItem
                        {
                            No = GetString(el, "No"),
                            Policy_Status = GetString(el, "Policy_Status"),
                            Quotation_No = GetString(el, "Quotation_No"),
                            Underwriter = GetString(el, "Underwriter"),
                            Underwriter_Name = GetString(el, "Underwriter_Name"),
                            Insured_No = GetString(el, "Insured_No"),
                            Insured_Name = GetString(el, "Insured_Name"),
                            Policy_Type = GetString(el, "Policy_Type"),
                            Policy_Description = GetString(el, "Policy_Description"),
                            From_Date = GetString(el, "From_Date"),
                            To_Date = GetString(el, "To_Date"),
                            Cover_Start_Date = GetString(el, "Cover_Start_Date"),
                            Cover_End_Date = GetString(el, "Cover_End_Date"),
                            Cover_Period = GetInt(el, "Cover_Period"),
                            Total_Sum_Insured = GetDecimal(el, "Total_Sum_Insured"),
                            Risk_Description = GetString(el, "Risk_Description")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PolicyCard by insured number {InsuredNo}", insuredNo);
            }
            return items;
        }

        private static string GetString(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString(); } catch { }
            }
            return string.Empty;
        }
        private static int GetInt(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.GetInt32(); } catch { }
                if (int.TryParse(prop.ToString(), out var i)) return i;
            }
            return 0;
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
    }

    
}
