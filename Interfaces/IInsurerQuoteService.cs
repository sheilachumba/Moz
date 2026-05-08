using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IInsurerQuoteService
    {
        Task<List<InsurerQuoteViewModel>> GetInsurerQuotesAsync(string? contactNo = null);
        Task<bool> SubmitAcknowledgementAsync(string contactNo, string customerName, string quoteId, string description = "");
        Task<bool> AcknowledgementExistsAsync(string contactNo, string quoteId);
        Task<List<InsurerQuoteViewModel>> GetAcknowledgedQuotesForContactAsync(string contactNo);
        Task<bool> SetSelectForQuoteAsync(int entryNo);
        Task<int> CleanupOtherRecommendationsAsync(string documentNo, string insuredNo, int exceptEntryNo);
        Task<bool> SelectProductAsync(int entryNo);
        Task<string?> GetDocumentNoForEntryAsync(int entryNo);
        Task<bool> CreatePolicyFromQuoteAsync(string documentNo, bool isRenewal = false);
        Task<string?> GetQuoteFinancialDetailsJsonAsync(string quoteNumber);
        Task<List<TermListItem>> GetTermListAsync();
    }

    public class InsurerQuoteService : IInsurerQuoteService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly BusinessCentralConfig _config;
        private readonly string _baseUrl;
        private readonly string _insurerQuoteListUrl;

        public InsurerQuoteService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _config = new BusinessCentralConfig(configuration);
            var bcBaseUrl = configuration["BcApi:BaseUrl"];
            _baseUrl = $"{bcBaseUrl}Company('STANDARD%20INSURANCE')/ProductSelection";
            _insurerQuoteListUrl = $"{bcBaseUrl}Company('STANDARD%20INSURANCE')/InsurerQuoteList";
        }

        public async Task<bool> SetSelectForQuoteAsync(int entryNo)
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var url = _baseUrl + $"(Entry_No={entryNo})";
                // Try to obtain current ETag
                string etag = null;
                try
                {
                    var getResp = await client.GetAsync(url);
                    if (getResp.IsSuccessStatusCode)
                    {
                        var body = await getResp.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var entity = System.Text.Json.JsonSerializer.Deserialize<ProductSelectionItem>(body, options);
                        etag = entity?.ODataEtag;
                    }
                }
                catch { }

                var payload = new { Select_for_Quote = true, Selected_by_Client = true };
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
                if (!string.IsNullOrWhiteSpace(etag))
                    request.Headers.TryAddWithoutValidation("If-Match", etag);
                else
                    request.Headers.TryAddWithoutValidation("If-Match", "*");
                var resp = await client.SendAsync(request);
                if (!resp.IsSuccessStatusCode) return false;
                try
                {
                    var verifyResp = await client.GetAsync(url);
                    if (verifyResp.IsSuccessStatusCode)
                    {
                        var body = await verifyResp.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var entity = System.Text.Json.JsonSerializer.Deserialize<ProductSelectionItem>(body, options);
                        if (entity?.Select_for_Quote == true && entity?.Selected_by_Client == true)
                            return true;
                    }
                }
                catch { }
                return false;
            }
            catch { return false; }
        }

        public async Task<int> CleanupOtherRecommendationsAsync(string documentNo, string insuredNo, int exceptEntryNo)
        {
            var deleted = 0;
            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var filter = $"?$filter=Document_No eq '{documentNo?.Replace("'", "''")}' and Insured_No eq '{insuredNo?.Replace("'", "''")}' and Entry_No ne {exceptEntryNo}";
                var listUrl = _baseUrl + filter;
                var listResp = await client.GetAsync(listUrl);
                if (!listResp.IsSuccessStatusCode) return 0;
                var content = await listResp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<ProductSelectionItem>>(content, options);
                if (oDataResponse?.Value == null || oDataResponse.Value.Count == 0) return 0;

                foreach (var item in oDataResponse.Value)
                {
                    try
                    {
                        var delUrl = _baseUrl + $"(Entry_No={item.Entry_No})";
                        var req = new HttpRequestMessage(HttpMethod.Delete, delUrl);
                        req.Headers.TryAddWithoutValidation("If-Match", "*");
                        var resp = await client.SendAsync(req);
                        if (resp.IsSuccessStatusCode) deleted++;
                    }
                    catch { }
                }
            }
            catch { }
            return deleted;
        }

        public async Task<bool> SelectProductAsync(int entryNo)
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var bcBaseUrl = _configuration["BcApi:BaseUrl"];
                // Function endpoint that will mark selected product and cleanup others in BC
                var url = $"{bcBaseUrl}PortalIntegration_SelectProduct?company=STANDARD%20INSURANCE";
                var payload = new { entryNo = entryNo };
                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync(url, content);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TermListItem>> GetTermListAsync()
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var bcBaseUrl = _configuration["BcApi:BaseUrl"];
                var url = $"{bcBaseUrl}Company('STANDARD%20INSURANCE')/TermList";

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<TermListItem>();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odata = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<TermListItem>>(content, options);
                return odata?.Value ?? new List<TermListItem>();
            }
            catch
            {
                return new List<TermListItem>();
            }
        }

        public async Task<string?> GetQuoteFinancialDetailsJsonAsync(string quoteNumber)
        {
            if (string.IsNullOrWhiteSpace(quoteNumber))
                return null;

            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var bcBaseUrl = _configuration["BcApi:BaseUrl"];
                var url = $"{bcBaseUrl}PortalIntegration_GetQuoteDetails?company=STANDARD%20INSURANCE";

                var payload = new { quoteNumber = quoteNumber };
                var jsonPayload = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync(url, content);
                if (!resp.IsSuccessStatusCode)
                    return null;

                var respBody = await resp.Content.ReadAsStringAsync();
                var outer = System.Text.Json.JsonSerializer.Deserialize<QuoteFinancialOuterDto>(respBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return outer?.Value;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreatePolicyFromQuoteAsync(string documentNo, bool isRenewal = false)
        {
            if (string.IsNullOrWhiteSpace(documentNo))
                return false;

            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var bcBaseUrl = _configuration["BcApi:BaseUrl"];
                var url = $"{bcBaseUrl}PortalIntegration_FnUpdateQuotePaymentStatus?company=STANDARD%20INSURANCE";

                var payload = new
                {
                    quoteNo = documentNo
                   
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync(url, content);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetDocumentNoForEntryAsync(int entryNo)
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                // Use a filtered list query instead of key syntax to be compatible with BC OData
                var url = _baseUrl + $"?$filter=Entry_No eq {entryNo}";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                    return null;

                var body = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var odata = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<ProductSelectionItem>>(body, options);
                var first = odata?.Value?.FirstOrDefault();
                return first?.Document_No;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<InsurerQuoteViewModel>> GetInsurerQuotesAsync(string? contactNo = null)
        {
            try
            {
                var url = _insurerQuoteListUrl;
                if (!string.IsNullOrWhiteSpace(contactNo))
                {
                    var escaped = contactNo.Replace("'", "''");
                    // InsurerQuoteList is keyed by Contact_No for the portal user
                    url = _insurerQuoteListUrl + $"?$filter=Contact_No eq '{escaped}'";
                }
                System.Diagnostics.Debug.WriteLine($"[InsurerQuoteService] Fetching quotes from InsurerQuoteList: {url}");

                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[InsurerQuoteService] Error: {response.StatusCode}");
                    return new List<InsurerQuoteViewModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[InsurerQuoteService] Response length: {content.Length}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<InsurerQuoteListItem>>(content, options);

                if (oDataResponse?.Value == null)
                {
                    System.Diagnostics.Debug.WriteLine("[InsurerQuoteService] No quotes found in InsurerQuoteList");
                    return new List<InsurerQuoteViewModel>();
                }

                var results = oDataResponse.Value.Select(x => new InsurerQuoteViewModel
                {
                    No = x.No,
                    Document_Type = x.Document_Type,
                    Contact_No = x.Contact_No,
                    Contact_Name = x.Contact_Name,
                    Document_Date = x.Document_Date,
                    Insured_No = x.Insured_No,
                    Insured_Name = x.Insured_Name,
                    Class_Name = x.Class_Name,
                    Risk_Description = x.Risk_Description,
                    Underwriter_Code = x.Underwriter_Code,
                    Underwriter_Name = x.Underwriter_Name,
                    Status = x.Payment_Status,
                    Total_Sum_Insured = x.Total_Sum_Insured,
                    Policy_No = x.Policy_No,
                    Selected_by_Client = x.Selected_by_Client,
                    ODataEtag = x.ODataEtag
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[InsurerQuoteService] Fetched {results.Count} insurer quotes from InsurerQuoteList");
                return results;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InsurerQuoteService] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[InsurerQuoteService] Stack trace: {ex.StackTrace}");
                return new List<InsurerQuoteViewModel>();
            }
        }

        public async Task<bool> SubmitAcknowledgementAsync(string contactNo, string customerName, string quoteId, string description = "")
        {
            // Pre-check: if already exists, treat as success
            if (await AcknowledgementExistsAsync(contactNo, quoteId))
                return true;

            var handler = _config.CreateHttpClientHandler();
            using var client = new HttpClient(handler);
            _config.ConfigureHttpClient(client);

            var endpoints = new List<string?>
            {
                _config.GetEndpoint("Acknowledgement"),
                "Acknowledgement",
                "Acknowledgements"
            };

            var attempts = new List<string>();
            // Try multiple endpoints; within each, retry a few times with different numeric No if duplicate key
            foreach (var ep in endpoints)
            {
                if (string.IsNullOrWhiteSpace(ep)) continue;
                var url = _config.BuildUrl(ep);

                for (int i = 0; i < 5; i++)
                {
                    // Generate a pseudo-unique 8- to 9-digit Int32 number
                    // Use GUID hash to avoid collisions from Random() across threads
                    var guid = Guid.NewGuid();
                    var hash = guid.GetHashCode();
                    var candidateNo = Math.Abs(hash % 900000000) + 100000000; // range 100,000,000 - 999,999,999

                    // Backend expects Quote_ID as Edm.String
                    var payload = new
                    {
                        No = candidateNo,
                        Contact_Number = contactNo ?? string.Empty,
                        Customer = customerName ?? string.Empty,
                        Description = description ?? string.Empty,
                        Quote_ID = quoteId ?? string.Empty
                    };

                    try
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(payload);
                        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        var resp = await client.PostAsync(url, content);
                        if (resp.IsSuccessStatusCode)
                        {
                            if (await AcknowledgementExistsAsync(contactNo, quoteId))
                                return true;
                            // If not immediately visible, break to next endpoint
                        }
                        var body = await resp.Content.ReadAsStringAsync();
                        if (body.Contains("EntityWithSameKeyExists", StringComparison.OrdinalIgnoreCase) ||
                            body.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                        {
                            // Duplicate No: retry with a different numeric No
                            continue;
                        }
                        attempts.Add($"{url} -> {(int)resp.StatusCode} {resp.ReasonPhrase} :: {body}");
                        // Non-duplicate error; break retry loop, try next endpoint
                        break;
                    }
                    catch (Exception ex)
                    {
                        attempts.Add($"{url} -> EX: {ex.Message}");
                        break;
                    }
                }
            }

            throw new Exception("Acknowledgement submit failed:\n" + string.Join("\n", attempts));
        }

        public async Task<bool> AcknowledgementExistsAsync(string contactNo, string quoteId)
        {
            try
            {
                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                var endpoints = new List<string?> { _config.GetEndpoint("Acknowledgement"), "Acknowledgement", "Acknowledgements" };
                foreach (var ep in endpoints)
                {
                    if (string.IsNullOrWhiteSpace(ep)) continue;
                    var baseUrl = _config.BuildUrl(ep);
                    // Backend expects Quote_ID as Edm.String
                    var filter = $"?$filter=Quote_ID eq '{quoteId?.Replace("'", "''")}' and Contact_Number eq '{contactNo?.Replace("'", "''")}'";
                    var url = baseUrl + filter;
                    try
                    {
                        var resp = await client.GetAsync(url);
                        if (!resp.IsSuccessStatusCode) continue;
                        var content = await resp.Content.ReadAsStringAsync();
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var result = System.Text.Json.JsonSerializer.Deserialize<ODataResponse<System.Text.Json.JsonElement>>(content, options);
                        if (result?.Value != null && result.Value.Count > 0)
                            return true;
                    }
                    catch { }
                }
            }
            catch { }
            return false;
        }

        public async Task<List<InsurerQuoteViewModel>> GetAcknowledgedQuotesForContactAsync(string contactNo)
        {
            var results = new List<InsurerQuoteViewModel>();
            try
            {
                if (string.IsNullOrWhiteSpace(contactNo)) return results;

                var handler = _config.CreateHttpClientHandler();
                using var client = new HttpClient(handler);
                _config.ConfigureHttpClient(client);

                // 1) Fetch acknowledgements for this contact from any likely endpoint
                var endpoints = new List<string?> { _config.GetEndpoint("Acknowledgement"), "Acknowledgement", "Acknowledgements" };
                var quoteIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var ep in endpoints)
                {
                    if (string.IsNullOrWhiteSpace(ep)) continue;
                    var baseUrl = _config.BuildUrl(ep);
                    var escapedContact = contactNo.Replace("'", "''");
                    var url = baseUrl + $"?$filter=Contact_Number eq '{escapedContact}'";
                    try
                    {
                        var resp = await client.GetAsync(url);
                        if (!resp.IsSuccessStatusCode) continue;
                        var content = await resp.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var ackResponse = JsonSerializer.Deserialize<ODataResponse<JsonElement>>(content, options);
                        if (ackResponse?.Value == null) continue;
                        foreach (var el in ackResponse.Value)
                        {
                            if (el.TryGetProperty("Quote_ID", out var qidProp))
                            {
                                var qid = qidProp.GetString();
                                if (!string.IsNullOrWhiteSpace(qid)) quoteIds.Add(qid);
                            }
                        }
                        if (quoteIds.Count > 0) break; // got data from a working endpoint
                    }
                    catch { }
                }

                if (quoteIds.Count == 0) return results;

                // 2) Fetch InsurerQuoteList entries where No matches any Quote_ID
                // OData doesn't support IN; chain with ORs and split if too long
                var idList = quoteIds.ToList();
                var batches = new List<List<string>>();
                const int batchSize = 15; // keep URL size reasonable
                for (int i = 0; i < idList.Count; i += batchSize)
                    batches.Add(idList.GetRange(i, Math.Min(batchSize, idList.Count - i)));

                foreach (var batch in batches)
                {
                    var parts = new List<string>();
                    foreach (var id in batch)
                    {
                        var safeId = id?.Replace("'", "''") ?? string.Empty;
                        // Quote_ID corresponds to No on InsurerQuoteList
                        parts.Add($"No eq '{safeId}'");
                    }
                    var filter = string.Join(" or ", parts);
                    var url = _insurerQuoteListUrl + $"?$filter={filter}";
                    try
                    {
                        var resp = await client.GetAsync(url);
                        if (!resp.IsSuccessStatusCode) continue;
                        var content = await resp.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var oDataResponse = JsonSerializer.Deserialize<ODataResponse<InsurerQuoteListItem>>(content, options);
                        if (oDataResponse?.Value != null && oDataResponse.Value.Count > 0)
                        {
                            // Map to InsurerQuoteViewModel for display (only Released status)
                            var releasedItems = oDataResponse.Value
                                .Where(x => x.Payment_Status != null && x.Payment_Status.Equals("Released", StringComparison.OrdinalIgnoreCase));

                            results.AddRange(releasedItems.Select(x => new InsurerQuoteViewModel
                            {
                                No = x.No,
                                Document_Type = x.Document_Type,
                                Contact_No = x.Contact_No,
                                Contact_Name = x.Contact_Name,
                                Document_Date = x.Document_Date,
                                Insured_No = x.Insured_No,
                                Insured_Name = x.Insured_Name,
                                Class_Name = x.Class_Name,
                                Risk_Description = x.Risk_Description,
                                Underwriter_Code = x.Underwriter_Code,
                                Underwriter_Name = x.Underwriter_Name,
                                Status = x.Payment_Status,
                                Total_Sum_Insured = x.Total_Sum_Insured,
                                Policy_No = x.Policy_No,
                                Selected_by_Client = x.Selected_by_Client,
                                ODataEtag = x.ODataEtag
                            }));
                        }
                    }
                    catch { }
                }
            }
            catch { }
            return results;
        }
    }

    public class ProductSelectionItem
    {
        public int Entry_No { get; set; }
        public string Document_Type { get; set; }
        public string Document_No { get; set; }
        public string UnderWriter { get; set; }
        public string Product_Plan { get; set; }
        public string Underwriter_Name { get; set; }
        public string Plan_Name { get; set; }
        public bool Select_for_Quote { get; set; }
        public bool Selected_by_Client { get; set; }
        public string Area_Of_Cover { get; set; }
        public string Select_Excess { get; set; }
        public string Client_Type { get; set; }
        public string Premium_Frequency { get; set; }
        public string Currency_Desired { get; set; }
        public decimal Exchange_Rate { get; set; }
        public string Area { get; set; }
        public string Cover_Type { get; set; }
        public string Module_Type { get; set; }
        public string No_Of_Employees_Range { get; set; }
        public string No_Of_Months { get; set; }
        public string Excess { get; set; }
        public string Insured_No { get; set; }
        public bool Visiblilty { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataEtag { get; set; }
    }

    public class InsurerQuoteListItem
    {
        public string Document_Type { get; set; }
        public string No { get; set; }
        public string Contact_No { get; set; }
        public string Contact_Name { get; set; }
        public string Document_Date { get; set; }
        public string Insured_No { get; set; }
        public string Insured_Name { get; set; }
        public string Class_Name { get; set; }
        public string Risk_Description { get; set; }
        public string Underwriter_Code { get; set; }
        public string Underwriter_Name { get; set; }
        [JsonPropertyName("Payment_Status")]
        public string Payment_Status { get; set; }
        public decimal Total_Sum_Insured { get; set; }
        public string Policy_No { get; set; }
        public bool Selected_by_Client { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataEtag { get; set; }
    }
    public class InsurerQuoteViewModel
    {
        public string No { get; set; }
        public string Document_Type { get; set; }
        public string Contact_No { get; set; }
        public string Contact_Name { get; set; }
        public string Document_Date { get; set; }
        public string Insured_No { get; set; }
        public string Insured_Name { get; set; }
        public string Class_Name { get; set; }
        public string Risk_Description { get; set; }
        public string Underwriter_Code { get; set; }
        public string Underwriter_Name { get; set; }
        public string Status { get; set; }
        public decimal Total_Sum_Insured { get; set; }
        public string Policy_No { get; set; }
        public bool Selected_by_Client { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataEtag { get; set; }
    }

    public class QuoteFinancialOuterDto
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}
