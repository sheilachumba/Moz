using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Repositories.Services
{
    public class PostedReceiptService : IPostedReceiptService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<PostedReceiptService> _logger;

        public PostedReceiptService(IConfiguration configuration, ILogger<PostedReceiptService> logger)
        {
            _logger = logger;
            var cfg = new BusinessCentralConfig(configuration);
            var handler = cfg.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);

            var bcBaseUrl = configuration["BcApi:BaseUrl"]; // e.g. http://.../ODataV4/
            var companyPath = "Company('STANDARD%20INSURANCE')/";
            _baseUrl = $"{bcBaseUrl}{companyPath}PostedReceiptx";
        }

        public async Task<List<PostedReceiptItem>> GetPostedReceiptsByAccountAsync(string accountNo)
        {
            var items = new List<PostedReceiptItem>();
            try
            {
                if (string.IsNullOrWhiteSpace(accountNo)) return items;
                var filter = Uri.EscapeDataString($"Account_No eq '{accountNo}'");
                var select = Uri.EscapeDataString("No,Receipt_No,Posting_Date,Pay_Mode,Cheque_No,Received_From,On_Behalf_Of,Cheque_Date,Currency_Code,Bank_Code,Cashier,Posted,Posted_Date,Posted_Time,Posted_By,Total_Amount,Amount_LCY,Cancellation_Reason,Account_No");
                var url = $"{_baseUrl}?$filter={filter}&$select={select}";

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PostedReceiptx fetch failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return items;
                }

                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        items.Add(new PostedReceiptItem
                        {
                            No = GetString(el, "No"),
                            Receipt_No = GetString(el, "Receipt_No"),
                            Posting_Date = GetString(el, "Posting_Date"),
                            Pay_Mode = GetString(el, "Pay_Mode"),
                            Cheque_No = GetString(el, "Cheque_No"),
                            Received_From = GetString(el, "Received_From"),
                            On_Behalf_Of = GetString(el, "On_Behalf_Of"),
                            Cheque_Date = GetString(el, "Cheque_Date"),
                            Currency_Code = GetString(el, "Currency_Code"),
                            Bank_Code = GetString(el, "Bank_Code"),
                            Cashier = GetString(el, "Cashier"),
                            Posted = GetBool(el, "Posted"),
                            Posted_Date = GetString(el, "Posted_Date"),
                            Posted_Time = GetString(el, "Posted_Time"),
                            Posted_By = GetString(el, "Posted_By"),
                            Total_Amount = GetDecimal(el, "Total_Amount"),
                            Amount_LCY = GetDecimal(el, "Amount_LCY"),
                            Cancellation_Reason = GetString(el, "Cancellation_Reason"),
                            Account_No = GetString(el, "Account_No")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PostedReceiptx for account {AccountNo}", accountNo);
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
        private static bool GetBool(JsonElement el, string name)
        {
            if (el.TryGetProperty(name, out var prop))
            {
                try { return prop.GetBoolean(); } catch { }
                if (bool.TryParse(prop.ToString(), out var b)) return b;
            }
            return false;
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
