using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public class ComplainService : IComplainService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<ComplainService> _logger;

        public ComplainService(IConfiguration configuration, ILogger<ComplainService> logger)
        {
            _logger = logger;
            var cfg = new BusinessCentralConfig(configuration);
            var handler = cfg.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);

            var bcBaseUrl = configuration["BcApi:BaseUrl"];
            var companyPath = "Company('STANDARD%20INSURANCE')/";
            _baseUrl = $"{bcBaseUrl}{companyPath}ComplainCard";
        }

        public async Task<List<ComplainCardItem>> GetComplaintsAsync(string clientNo)
        {
            var list = new List<ComplainCardItem>();
            try
            {
                var url = _baseUrl;
                if (!string.IsNullOrWhiteSpace(clientNo))
                {
                    var safe = clientNo.Replace("'", "''");
                    var filter = Uri.EscapeDataString($"Client_No eq '{safe}'");
                    url += $"?$filter={filter}";
                }

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("ComplainCard fetch failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return list;
                }

                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        list.Add(new ComplainCardItem
                        {
                            Interact_Code = GetString(el, "Interact_Code"),
                            Client_Type = GetString(el, "Client_Type"),
                            Client_No = GetString(el, "Client_No"),
                            Client_Name = GetString(el, "Client_Name"),
                            Date_and_Time = GetDateTime(el, "Date_and_Time"),
                            Last_Updated_Date_and_Time = GetDateTime(el, "Last_Updated_Date_and_Time"),
                            Interaction_Type = GetString(el, "Interaction_Type"),
                            Issue_Category = GetString(el, "Issue_Category"),
                            Risk_Type = GetString(el, "Risk_Type"),
                            Timeline = GetString(el, "Timeline"),
                            Interaction_Resol_Desc = GetString(el, "Interaction_Resol_Desc"),
                            Current_Status = GetString(el, "Current_Status"),
                            Escalation_Clock = GetDateTime(el, "Escalation_Clock"),
                            Overall_Level_Duration = GetDecimal(el, "Overall_Level_Duration"),
                            Reviewing_Officer_Remarks = GetString(el, "Reviewing_Officer_Remarks"),
                            Reopening_Remarks = GetString(el, "Reopening_Remarks")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ComplainCard list");
            }

            return list;
        }

        public async Task<(bool Success, string? Error)> CreateComplaintAsync(ComplainCardItem item)
        {
            if (item == null)
                return (false, "Invalid complaint payload.");

            try
            {
                var payload = new
                {
                    Interact_Code = item.Interact_Code,
                    Client_Type = item.Client_Type,
                    Client_No = item.Client_No,
                    // Client_Name is read-only in BC; let BC derive it from Client_No
                    Interaction_Type = item.Interaction_Type,
                    Issue_Category = item.Issue_Category,
                    Risk_Type = item.Risk_Type,
                    Timeline = item.Timeline,
                    Interaction_Resol_Desc = item.Interaction_Resol_Desc,
                    Current_Status = item.Current_Status,
                    Overall_Level_Duration = item.Overall_Level_Duration
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _httpClient.PostAsync(_baseUrl, content);

                if (!res.IsSuccessStatusCode)
                {
                    var body = await res.Content.ReadAsStringAsync();
                    _logger.LogWarning("CreateComplaint failed: {Status} - {Reason}. Body: {Body}", (int)res.StatusCode, res.ReasonPhrase, body);
                    var error = string.IsNullOrWhiteSpace(body) ? res.ReasonPhrase : body;
                    return (false, error);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating complaint");
                return (false, ex.Message);
            }
        }

        private static string GetString(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.ToString(); } catch { }
            }
            return string.Empty;
        }

        private static DateTime? GetDateTime(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.GetDateTime(); } catch { }
                if (DateTime.TryParse(prop.ToString(), out var d)) return d;
            }
            return null;
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
