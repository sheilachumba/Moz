using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MOZ_UPGRADE.Repositories.Services
{
    public class SharePointDocumentService : ISharePointDocumentService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private string? _cachedAccessToken;
        private DateTimeOffset _cachedAccessTokenExpiresAt;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

        public SharePointDocumentService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> UploadKycDocumentAsync(string category, string customerOrContactNo, string documentCode, byte[] fileBytes, string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category is required", nameof(category));
            if (string.IsNullOrWhiteSpace(customerOrContactNo)) throw new ArgumentException("customerOrContactNo is required", nameof(customerOrContactNo));
            if (string.IsNullOrWhiteSpace(documentCode)) throw new ArgumentException("documentCode is required", nameof(documentCode));
            if (fileBytes == null || fileBytes.Length == 0) throw new ArgumentException("fileBytes is empty", nameof(fileBytes));
            if (string.IsNullOrWhiteSpace(originalFileName)) originalFileName = "document.pdf";

            var driveId = GetRequired("SharePoint:Drive");
            var rootFolderSection = _configuration.GetSection("SharePoint:RootFolder");
            var rootFolder = rootFolderSection.Exists() ? (rootFolderSection.Value ?? string.Empty) : "KYC";

            var safeCategory = SanitizePathSegment(category);
            var safeCustomer = SanitizePathSegment(customerOrContactNo);
            var safeDoc = SanitizePathSegment(documentCode);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var safeFileName = SanitizeFileName(originalFileName);
            var finalFileName = $"{timestamp}_{safeFileName}";

            var folderPath = string.IsNullOrWhiteSpace(rootFolder)
                ? $"{safeCategory}/{safeCustomer}/{safeDoc}"
                : $"{rootFolder}/{safeCategory}/{safeCustomer}/{safeDoc}";
            await EnsureFolderPathAsync(driveId, folderPath);

            var filePath = $"{folderPath}/{finalFileName}";
            var webUrl = await UploadWithSessionAsync(driveId, filePath, fileBytes);
            return webUrl;
        }

        public async Task<IReadOnlyList<SharePointListedDocument>> ListKycDocumentsAsync(string category, string customerOrContactNo)
        {
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("category is required", nameof(category));
            if (string.IsNullOrWhiteSpace(customerOrContactNo)) throw new ArgumentException("customerOrContactNo is required", nameof(customerOrContactNo));

            var driveId = GetRequired("SharePoint:Drive");
            var rootFolderSection = _configuration.GetSection("SharePoint:RootFolder");
            var rootFolder = rootFolderSection.Exists() ? (rootFolderSection.Value ?? string.Empty) : "KYC";

            var safeCategory = SanitizePathSegment(category);
            var safeCustomer = SanitizePathSegment(customerOrContactNo);

            var basePath = string.IsNullOrWhiteSpace(rootFolder)
                ? $"{safeCategory}/{safeCustomer}"
                : $"{rootFolder}/{safeCategory}/{safeCustomer}";

            var exists = await PathExistsAsync(driveId, basePath);
            if (!exists)
            {
                return Array.Empty<SharePointListedDocument>();
            }

            var result = new List<SharePointListedDocument>();
            var docFolders = await ListChildrenByPathAsync(driveId, basePath);

            foreach (var folder in docFolders)
            {
                if (folder == null || !folder.IsFolder || string.IsNullOrWhiteSpace(folder.Name))
                {
                    continue;
                }

                var docCode = folder.Name;
                var docPath = $"{basePath}/{docCode}";

                var files = await ListChildrenByPathAsync(driveId, docPath);
                foreach (var file in files)
                {
                    if (file == null) continue;
                    if (file.IsFolder) continue;
                    if (string.IsNullOrWhiteSpace(file.Name) || string.IsNullOrWhiteSpace(file.WebUrl)) continue;

                    result.Add(new SharePointListedDocument
                    {
                        Category = category,
                        DocumentCode = docCode,
                        FileName = file.Name,
                        WebUrl = file.WebUrl,
                        LastModifiedDateTime = file.LastModifiedDateTime
                    });
                }
            }

            return result;
        }

        private async Task<bool> PathExistsAsync(string driveId, string folderPath)
        {
            var accessToken = await GetAccessTokenAsync();

            var getUrl = $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/root:/{EscapeGraphPath(folderPath)}";
            using var getReq = new HttpRequestMessage(HttpMethod.Get, getUrl);
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var getResp = await _httpClient.SendAsync(getReq);
            if (getResp.IsSuccessStatusCode)
            {
                return true;
            }

            if (getResp.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            var text = await getResp.Content.ReadAsStringAsync();
            throw new Exception($"Folder lookup failed: {(int)getResp.StatusCode} {getResp.ReasonPhrase} :: {text}");
        }

        private async Task<List<GraphDriveItem>> ListChildrenByPathAsync(string driveId, string folderPath)
        {
            var accessToken = await GetAccessTokenAsync();
            var url = $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/root:/{EscapeGraphPath(folderPath)}:/children?$top=999";

            var all = new List<GraphDriveItem>();
            while (!string.IsNullOrWhiteSpace(url))
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var resp = await _httpClient.SendAsync(req);
                var text = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        return all;
                    }
                    throw new Exception($"List folder children failed: {(int)resp.StatusCode} {resp.ReasonPhrase} :: {text}");
                }

                var parsed = JsonSerializer.Deserialize<GraphListResponse<GraphDriveItem>>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (parsed?.Value != null)
                {
                    all.AddRange(parsed.Value);
                }

                url = parsed?.NextLink;
            }

            return all;
        }

        private async Task<string> UploadWithSessionAsync(string driveId, string filePath, byte[] fileBytes)
        {
            var accessToken = await GetAccessTokenAsync();

            using var createReq = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/root:/{EscapeGraphPath(filePath)}:/createUploadSession");
            createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var bodyObj = new
            {
                item = new Dictionary<string, object>
                {
                    ["@microsoft.graph.conflictBehavior"] = "rename"
                }
            };
            createReq.Content = new StringContent(JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json");

            using var createResp = await _httpClient.SendAsync(createReq);
            var createText = await createResp.Content.ReadAsStringAsync();
            if (!createResp.IsSuccessStatusCode)
            {
                throw new Exception($"Create upload session failed: {(int)createResp.StatusCode} {createResp.ReasonPhrase} :: {createText}");
            }

            var session = JsonSerializer.Deserialize<UploadSessionResponse>(createText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (session == null || string.IsNullOrWhiteSpace(session.UploadUrl))
            {
                throw new Exception("Create upload session response did not include uploadUrl");
            }

            var uploadUrl = session.UploadUrl;

            const int chunkSize = 320 * 1024 * 5;
            long total = fileBytes.Length;
            long offset = 0;

            while (offset < total)
            {
                var remaining = total - offset;
                var currentChunkSize = (int)Math.Min(chunkSize, remaining);

                using var chunkContent = new ByteArrayContent(fileBytes, (int)offset, currentChunkSize);
                chunkContent.Headers.ContentLength = currentChunkSize;
                chunkContent.Headers.ContentRange = new ContentRangeHeaderValue(offset, offset + currentChunkSize - 1, total);

                using var putReq = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
                {
                    Content = chunkContent
                };

                using var putResp = await _httpClient.SendAsync(putReq);
                var putText = await putResp.Content.ReadAsStringAsync();

                if (putResp.StatusCode == HttpStatusCode.Accepted)
                {
                    offset += currentChunkSize;
                    continue;
                }

                if (putResp.IsSuccessStatusCode)
                {
                    var completed = JsonSerializer.Deserialize<DriveItemResponse>(putText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (completed != null && !string.IsNullOrWhiteSpace(completed.WebUrl))
                    {
                        return completed.WebUrl;
                    }
                    return string.Empty;
                }

                throw new Exception($"Upload chunk failed: {(int)putResp.StatusCode} {putResp.ReasonPhrase} :: {putText}");
            }

            return string.Empty;
        }

        private async Task EnsureFolderPathAsync(string driveId, string folderPath)
        {
            var accessToken = await GetAccessTokenAsync();
            var segments = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string parentId = "root";
            string currentPath = "";

            foreach (var rawSeg in segments)
            {
                var seg = SanitizePathSegment(rawSeg);
                currentPath = string.IsNullOrEmpty(currentPath) ? seg : $"{currentPath}/{seg}";

                var getUrl = $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/root:/{EscapeGraphPath(currentPath)}";
                using var getReq = new HttpRequestMessage(HttpMethod.Get, getUrl);
                getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var getResp = await _httpClient.SendAsync(getReq);
                if (getResp.IsSuccessStatusCode)
                {
                    var getText = await getResp.Content.ReadAsStringAsync();
                    var item = JsonSerializer.Deserialize<DriveItemResponse>(getText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                    {
                        parentId = item.Id;
                        continue;
                    }
                }

                if (getResp.StatusCode != HttpStatusCode.NotFound)
                {
                    var text = await getResp.Content.ReadAsStringAsync();
                    throw new Exception($"Folder lookup failed: {(int)getResp.StatusCode} {getResp.ReasonPhrase} :: {text}");
                }

                var createUrl = parentId == "root"
                    ? $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/root/children"
                    : $"https://graph.microsoft.com/v1.0/drives/{Uri.EscapeDataString(driveId)}/items/{Uri.EscapeDataString(parentId)}/children";

                using var createReq = new HttpRequestMessage(HttpMethod.Post, createUrl);
                createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var createBody = new Dictionary<string, object>
                {
                    ["name"] = seg,
                    ["folder"] = new Dictionary<string, object>(),
                    ["@microsoft.graph.conflictBehavior"] = "fail"
                };

                createReq.Content = new StringContent(JsonSerializer.Serialize(createBody), Encoding.UTF8, "application/json");

                using var createResp = await _httpClient.SendAsync(createReq);
                var createText = await createResp.Content.ReadAsStringAsync();

                if (createResp.IsSuccessStatusCode)
                {
                    var created = JsonSerializer.Deserialize<DriveItemResponse>(createText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (created != null && !string.IsNullOrWhiteSpace(created.Id))
                    {
                        parentId = created.Id;
                        continue;
                    }
                    throw new Exception("Folder created but no id returned");
                }

                if (createResp.StatusCode == HttpStatusCode.Conflict)
                {
                    using var retryReq = new HttpRequestMessage(HttpMethod.Get, getUrl);
                    retryReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    using var retryResp = await _httpClient.SendAsync(retryReq);
                    var retryText = await retryResp.Content.ReadAsStringAsync();
                    if (!retryResp.IsSuccessStatusCode)
                    {
                        throw new Exception($"Folder exists but could not fetch: {(int)retryResp.StatusCode} {retryResp.ReasonPhrase} :: {retryText}");
                    }
                    var item = JsonSerializer.Deserialize<DriveItemResponse>(retryText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (item != null && !string.IsNullOrWhiteSpace(item.Id))
                    {
                        parentId = item.Id;
                        continue;
                    }
                }

                throw new Exception($"Create folder failed: {(int)createResp.StatusCode} {createResp.ReasonPhrase} :: {createText}");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_cachedAccessToken) && _cachedAccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(2))
            {
                return _cachedAccessToken;
            }

            await _tokenLock.WaitAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(_cachedAccessToken) && _cachedAccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(2))
                {
                    return _cachedAccessToken;
                }

                var tenant = GetRequired("SharePoint:Tenant");
                var clientId = GetRequired("SharePoint:Client");
                var clientSecret = GetRequired("SharePoint:Client_Secret");
                var scope = _configuration["SharePoint:Graph_Scope"];
                if (string.IsNullOrWhiteSpace(scope)) scope = "https://graph.microsoft.com/.default";

                var tokenUrl = _configuration["SharePoint:Access_Url"];
                if (string.IsNullOrWhiteSpace(tokenUrl))
                {
                    tokenUrl = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
                }
                else
                {
                    tokenUrl = tokenUrl.Replace("%1", tenant, StringComparison.OrdinalIgnoreCase);
                }

                using var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
                req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["grant_type"] = "client_credentials",
                    ["scope"] = scope
                });

                using var resp = await _httpClient.SendAsync(req);
                var text = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    throw new Exception($"Token request failed: {(int)resp.StatusCode} {resp.ReasonPhrase} :: {text}");
                }

                var token = JsonSerializer.Deserialize<TokenResponse>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
                {
                    throw new Exception("Token response did not include access_token");
                }

                var expiresIn = token.ExpiresIn <= 0 ? 3600 : token.ExpiresIn;
                _cachedAccessToken = token.AccessToken;
                _cachedAccessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

                return _cachedAccessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private string GetRequired(string key)
        {
            var val = _configuration[key];
            if (string.IsNullOrWhiteSpace(val))
            {
                throw new Exception($"Missing configuration value: {key}");
            }
            return val;
        }

        private static string SanitizePathSegment(string value)
        {
            value = value.Trim();
            foreach (var c in new[] { '\\', ':', '*', '?', '"', '<', '>', '|' })
            {
                value = value.Replace(c.ToString(), "", StringComparison.OrdinalIgnoreCase);
            }
            value = value.Replace("..", ".", StringComparison.OrdinalIgnoreCase);
            return value;
        }

        private static string SanitizeFileName(string fileName)
        {
            fileName = fileName.Trim();
            foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), "", StringComparison.OrdinalIgnoreCase);
            }
            if (string.IsNullOrWhiteSpace(fileName)) return "document.pdf";
            return fileName;
        }

        private static string EscapeGraphPath(string path)
        {
            var segs = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < segs.Length; i++)
            {
                segs[i] = Uri.EscapeDataString(segs[i]);
            }
            return string.Join("/", segs);
        }

        private sealed class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string? AccessToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }
        }

        private sealed class UploadSessionResponse
        {
            public string? UploadUrl { get; set; }
        }

        private sealed class GraphListResponse<T>
        {
            [JsonPropertyName("value")]
            public List<T>? Value { get; set; }

            [JsonPropertyName("@odata.nextLink")]
            public string? NextLink { get; set; }
        }

        private sealed class GraphDriveItem
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }

            [JsonPropertyName("webUrl")]
            public string? WebUrl { get; set; }

            [JsonPropertyName("lastModifiedDateTime")]
            public DateTimeOffset? LastModifiedDateTime { get; set; }

            [JsonPropertyName("folder")]
            public JsonElement? Folder { get; set; }

            public bool IsFolder => Folder.HasValue && Folder.Value.ValueKind != JsonValueKind.Null && Folder.Value.ValueKind != JsonValueKind.Undefined;
        }

        private sealed class DriveItemResponse
        {
            public string? Id { get; set; }
            public string? WebUrl { get; set; }
        }
    }
}
