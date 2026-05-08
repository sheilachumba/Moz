using System.Text.Json;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Repositories.Services
{
    public class DebitNoteService : IDebitNoteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _paymentsUrl;
        private readonly string _updatePaymentStatusUrl;
        private readonly ILogger<DebitNoteService> _logger;
        private readonly string _postedLinesUrl;
        private readonly string _ledgersUrl;

        public DebitNoteService(IConfiguration configuration, ILogger<DebitNoteService> logger)
        {
            _logger = logger;
            var cfg = new BusinessCentralConfig(configuration);
            var handler = cfg.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);

            var bcBaseUrl = configuration["BcApi:BaseUrl"]; // e.g. http://.../ODataV4/
            var companyPath = "Company('STANDARD%20INSURANCE')/";
            _baseUrl = $"{bcBaseUrl}{companyPath}DebitNoteCard";
            _paymentsUrl = $"{bcBaseUrl}{companyPath}DebitNotePayments";
            // Function endpoint to let BC update payment status server-side
            _updatePaymentStatusUrl = "http://196.201.224.102:2048/BC260/ODataV4/PortalIntegration_UpdatePaymentStatus?company=STANDARD%20INSURANCE";
            _postedLinesUrl = $"{bcBaseUrl}{companyPath}PostedDebitNoteLines";
            _ledgersUrl = $"{bcBaseUrl}{companyPath}Ledgers";
        }

        public async Task<List<DebitNoteItem>> GetDebitNotesByQuotationAsync(string quotationNo)
        {
            var items = new List<DebitNoteItem>();
            try
            {
                if (string.IsNullOrWhiteSpace(quotationNo)) return items;
                var filter = Uri.EscapeDataString($"Quotation_No eq '{quotationNo}'");
                var select = Uri.EscapeDataString("No,Policy_Type,Policy_Description,Insured_No,Insured_Name,Insured_Address,E_mail,Endorsement_Type,Action_Type,Document_Date,From_Date,To_Date,Cover_Start_Date,Cover_End_Date,No_Of_Instalments,Policy_No,Quotation_No,Total_Premium_Amount,Total_Tax,Total_Sum_Insured,Risk_Description,Status,Pay_Status");
                var url = $"{_baseUrl}?$filter={filter}&$select={select}";

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DebitNote fetch failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return items;
                }

                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        items.Add(new DebitNoteItem
                        {
                            No = GetString(el, "No"),
                            Policy_Type = GetString(el, "Policy_Type"),
                            Policy_Description = GetString(el, "Policy_Description"),
                            Insured_No = GetString(el, "Insured_No"),
                            Insured_Name = GetString(el, "Insured_Name"),
                            Insured_Address = GetString(el, "Insured_Address"),
                            E_mail = GetString(el, "E_mail"),
                            Endorsement_Type = GetString(el, "Endorsement_Type"),
                            Action_Type = GetString(el, "Action_Type"),
                            Document_Date = GetString(el, "Document_Date"),
                            From_Date = GetString(el, "From_Date"),
                            To_Date = GetString(el, "To_Date"),
                            Cover_Start_Date = GetString(el, "Cover_Start_Date"),
                            Cover_End_Date = GetString(el, "Cover_End_Date"),
                            No_Of_Instalments = GetInt(el, "No_Of_Instalments"),
                            Policy_No = GetString(el, "Policy_No"),
                            Quotation_No = GetString(el, "Quotation_No"),
                            Total_Premium_Amount = GetDecimal(el, "Total_Premium_Amount"),
                            Total_Tax = GetDecimal(el, "Total_Tax"),
                            Total_Sum_Insured = GetDecimal(el, "Total_Sum_Insured"),
                            Risk_Description = GetString(el, "Risk_Description"),
                            Status = GetString(el, "Status"),
                            Pay_Status = GetString(el, "Pay_Status")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching DebitNote by quotation {QuotationNo}", quotationNo);
            }
            return items;
        }

        public async Task<(bool HasLedger, decimal TaxAmount, decimal LedgerAmount)> GetPolicyPaymentAmountsAsync(string policyNo)
        {
            if (string.IsNullOrWhiteSpace(policyNo))
            {
                return (false, 0m, 0m);
            }

            try
            {
                var safePolicy = policyNo.Replace("'", "''");
                var filterDebit = Uri.EscapeDataString($"Policy_No eq '{safePolicy}'");
                var selectDebit = Uri.EscapeDataString("No,Policy_No,Total_Tax");
                var debitUrl = $"{_baseUrl}?$filter={filterDebit}&$select={selectDebit}";

                string debitNo = null;
                decimal taxAmountFromHeader = 0m;

                var debitRes = await _httpClient.GetAsync(debitUrl);
                if (debitRes.IsSuccessStatusCode)
                {
                    var content = await debitRes.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                    {
                        var first = arr.EnumerateArray().FirstOrDefault();
                        if (first.ValueKind == JsonValueKind.Object)
                        {
                            debitNo = GetString(first, "No");
                            taxAmountFromHeader = GetDecimal(first, "Total_Tax");
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(debitNo))
                {
                    return (false, 0m, 0m);
                }

                var safeDoc = debitNo.Replace("'", "''");

                decimal taxAmount = 0m;
                try
                {
                    var filterLines = Uri.EscapeDataString($"Document_No eq '{safeDoc}' and Description eq 'Tax'");
                    var selectLines = Uri.EscapeDataString("Document_No,Amount,Description");
                    var linesUrl = $"{_postedLinesUrl}?$filter={filterLines}&$select={selectLines}";

                    var linesRes = await _httpClient.GetAsync(linesUrl);
                    if (linesRes.IsSuccessStatusCode)
                    {
                        var linesContent = await linesRes.Content.ReadAsStringAsync();
                        using var linesDoc = JsonDocument.Parse(linesContent);
                        if (linesDoc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                        {
                            var firstLine = arr.EnumerateArray().FirstOrDefault();
                            if (firstLine.ValueKind == JsonValueKind.Object)
                            {
                                taxAmount = GetDecimal(firstLine, "Amount");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching PostedDebitNoteLines for policy {PolicyNo}", policyNo);
                }

                if (taxAmount == 0m && taxAmountFromHeader > 0m)
                {
                    taxAmount = taxAmountFromHeader;
                }

                decimal ledgerAmount = 0m;
                bool hasLedger = false;

                try
                {
                    var filterLedger = Uri.EscapeDataString($"Document_No eq '{safeDoc}'");
                    var selectLedger = Uri.EscapeDataString("Document_No,Amount");
                    var ledgerUrl = $"{_ledgersUrl}?$filter={filterLedger}&$select={selectLedger}";

                    var ledgerRes = await _httpClient.GetAsync(ledgerUrl);
                    if (ledgerRes.IsSuccessStatusCode)
                    {
                        var ledgerContent = await ledgerRes.Content.ReadAsStringAsync();
                        using var ledgerDoc = JsonDocument.Parse(ledgerContent);
                        if (ledgerDoc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                        {
                            var firstLedger = arr.EnumerateArray().FirstOrDefault();
                            if (firstLedger.ValueKind == JsonValueKind.Object)
                            {
                                ledgerAmount = GetDecimal(firstLedger, "Amount");
                                hasLedger = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Ledgers for policy {PolicyNo}", policyNo);
                }

                return (hasLedger, taxAmount, ledgerAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing payment amounts for policy {PolicyNo}", policyNo);
                return (false, 0m, 0m);
            }
        }

        public async Task<(bool Success, string? Error)> CreateDebitNotePaymentAsync(DebitNoteItem note, decimal amountPaid, string modeOfPayment, string paymentReferenceNo, DateTime paymentDate)
        {
            if (note == null || string.IsNullOrWhiteSpace(note.No))
                return (false, "Invalid debit note.");

            try
            {
                var payload = new
                {
                    Debit_Note_No = note.No,
                    Insured_Name = note.Insured_Name,
                    Payment_Date = paymentDate.ToString("yyyy-MM-dd"),
                    Amount_Paid = amountPaid,
                    Balance_Amount = 0m,
                    Mode_of_Payment = modeOfPayment ?? string.Empty,
                    Payment_Reference_No = paymentReferenceNo ?? string.Empty,
                    Payment_Status = "Pending",
                    Posted = false
                };

                var content = JsonContent.Create(payload);
                var res = await _httpClient.PostAsync(_paymentsUrl, content);

                if (!res.IsSuccessStatusCode)
                {
                    var err = await res.Content.ReadAsStringAsync();
                    _logger.LogWarning("DebitNote payment post failed: {Status} - {Reason} - {Body}", (int)res.StatusCode, res.ReasonPhrase, err);
                    return (false, err);
                }

                // After successfully creating the payment, update the debit note status in BC
                var statusResult = await UpdateDebitNoteStatusAsync(note.No, "Pending");
                if (!statusResult.Success)
                {
                    // Propagate BC error so the caller can show it in the UI
                    return (false, statusResult.Error ?? "Failed to update debit note status to Pending Payment.");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for DebitNote {DebitNoteNo}", note.No);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateDebitNoteStatusAsync(string debitNoteNo, string status)
        {
            if (string.IsNullOrWhiteSpace(debitNoteNo) || string.IsNullOrWhiteSpace(status))
                return (false, "Invalid debit note or status.");

            try
            {
                // Use the BC function PortalIntegration_UpdatePaymentStatus to update status server-side
                var payload = new
                {
                    no = debitNoteNo
                };

                var res = await _httpClient.PostAsync(_updatePaymentStatusUrl, JsonContent.Create(payload));
                if (!res.IsSuccessStatusCode)
                {
                    var body = await res.Content.ReadAsStringAsync();
                    _logger.LogWarning("DebitNote status update via PortalIntegration_UpdatePaymentStatus failed: {Status} - {Reason} - {Body}", (int)res.StatusCode, res.ReasonPhrase, body);
                    return (false, body);
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating DebitNote status for {DebitNoteNo}", debitNoteNo);
                return (false, ex.Message);
            }
        }

        public async Task<List<DebitNoteItem>> GetDebitNotesByInsuredNoAsync(string insuredNo)
        {
            var items = new List<DebitNoteItem>();
            try
            {
                if (string.IsNullOrWhiteSpace(insuredNo)) return items;
                var filter = Uri.EscapeDataString($"Insured_No eq '{insuredNo}'");
                var select = Uri.EscapeDataString("No,Policy_Type,Policy_Description,Insured_No,Insured_Name,Insured_Address,E_mail,Endorsement_Type,Action_Type,Document_Date,From_Date,To_Date,Cover_Start_Date,Cover_End_Date,No_Of_Instalments,Policy_No,Quotation_No,Total_Premium_Amount,Total_Tax,Total_Sum_Insured,Risk_Description,Status,Pay_Status");
                var url = $"{_baseUrl}?$filter={filter}&$select={select}";

                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("DebitNote fetch by insured failed: {Status} - {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                    return items;
                }

                var content = await res.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("value", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in arr.EnumerateArray())
                    {
                        items.Add(new DebitNoteItem
                        {
                            No = GetString(el, "No"),
                            Policy_Type = GetString(el, "Policy_Type"),
                            Policy_Description = GetString(el, "Policy_Description"),
                            Insured_No = GetString(el, "Insured_No"),
                            Insured_Name = GetString(el, "Insured_Name"),
                            Insured_Address = GetString(el, "Insured_Address"),
                            E_mail = GetString(el, "E_mail"),
                            Endorsement_Type = GetString(el, "Endorsement_Type"),
                            Action_Type = GetString(el, "Action_Type"),
                            Document_Date = GetString(el, "Document_Date"),
                            From_Date = GetString(el, "From_Date"),
                            To_Date = GetString(el, "To_Date"),
                            Cover_Start_Date = GetString(el, "Cover_Start_Date"),
                            Cover_End_Date = GetString(el, "Cover_End_Date"),
                            No_Of_Instalments = GetInt(el, "No_Of_Instalments"),
                            Policy_No = GetString(el, "Policy_No"),
                            Quotation_No = GetString(el, "Quotation_No"),
                            Total_Premium_Amount = GetDecimal(el, "Total_Premium_Amount"),
                            Total_Tax = GetDecimal(el, "Total_Tax"),
                            Total_Sum_Insured = GetDecimal(el, "Total_Sum_Insured"),
                            Risk_Description = GetString(el, "Risk_Description"),
                            Status = GetString(el, "Status"),
                            Pay_Status = GetString(el, "Pay_Status")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching DebitNote by insured number {InsuredNo}", insuredNo);
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
