using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Text.Json;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IUnderwriterPolicyService
    {
        Task<List<UnderwriterPolicyViewModel>> GetUnderwriterPoliciesAsync();
        Task<List<ProductClass>> GetProductClassesAsync();
        Task<List<InsurerList>> GetInsurersAsync();
    }

    public class UnderwriterPolicyService : IUnderwriterPolicyService
    {
        private readonly HttpClient _httpClient;
        private readonly BusinessCentralConfig _config;
        private readonly string _policiesBaseUrl;
        private readonly string _insurersBaseUrl;
        private readonly string _productClassesBaseUrl;

        public UnderwriterPolicyService(IConfiguration configuration)
        {
            _config = new BusinessCentralConfig(configuration);
            var handler = _config.CreateHttpClientHandler();
            _httpClient = new HttpClient(handler);
            
            var bcBaseUrl = configuration["BcApi:BaseUrl"];
            var companyPath = "Company('STANDARD%20INSURANCE')/";
            _policiesBaseUrl = $"{bcBaseUrl}{companyPath}underwriterpolicy";
            _insurersBaseUrl = $"{bcBaseUrl}{companyPath}InsurerList";
            _productClassesBaseUrl = $"{bcBaseUrl}{companyPath}ProductClass";
        }

        public async Task<List<UnderwriterPolicyViewModel>> GetUnderwriterPoliciesAsync()
        {
            try
            {
                // Fetch policies, insurers, and product classes in parallel
                var policiesTask = FetchUnderwriterPoliciesAsync();
                var insurersTask = FetchInsurersAsync();
                var productClassesTask = FetchProductClassesAsync();

                await Task.WhenAll(policiesTask, insurersTask, productClassesTask);

                var policies = await policiesTask;
                var insurers = await insurersTask;
                var productClasses = await productClassesTask;

                System.Diagnostics.Debug.WriteLine($"Fetched {policies.Count} policies, {insurers.Count} insurers, and {productClasses.Count} product classes");

                // Create lookup dictionaries
                var insurerLookup = insurers.ToDictionary(i => i.No, i => i.Name);
                var productClassLookup = productClasses.ToDictionary(p => p.ProductClassNo, p => p.Name);

                // Map to view models
                var viewModels = policies.Select(p => new UnderwriterPolicyViewModel
                {
                    No = p.No,
                    Insurer = p.Insurer,
                    InsurerName = insurerLookup.ContainsKey(p.Insurer) ? insurerLookup[p.Insurer] : p.Insurer,
                    Plans = p.Plans,
                    ProductClassName = productClassLookup.ContainsKey(p.Plans) ? productClassLookup[p.Plans] : p.Plans,
                    PolicyPlan = p.PolicyPlan
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"Returning {viewModels.Count} view models");
                return viewModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching underwriter policies: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<UnderwriterPolicyViewModel>();
            }
        }

        public async Task<List<ProductClass>> GetProductClassesAsync()
        {
            return await FetchProductClassesAsync();
        }

        public async Task<List<InsurerList>> GetInsurersAsync()
        {
            return await FetchInsurersAsync();
        }

        private async Task<List<UnderwriterPolicy>> FetchUnderwriterPoliciesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching policies from: {_policiesBaseUrl}");
                var response = await _httpClient.GetAsync(_policiesBaseUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error response: {response.StatusCode} - {response.ReasonPhrase}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error content: {errorContent}");
                    return new List<UnderwriterPolicy>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Policies response length: {content.Length}");
                System.Diagnostics.Debug.WriteLine($"Policies response: {(content.Length > 500 ? content.Substring(0, 500) + "..." : content)}");
                
                var jsonDoc = JsonDocument.Parse(content);
                var policies = new List<UnderwriterPolicy>();

                if (jsonDoc.RootElement.TryGetProperty("value", out var valueElement))
                {
                    System.Diagnostics.Debug.WriteLine($"Found 'value' property with {valueElement.GetArrayLength()} items");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        try
                        {
                            var policy = JsonSerializer.Deserialize<UnderwriterPolicy>(item.GetRawText(), options);
                            if (policy != null)
                            {
                                policies.Add(policy);
                            }
                        }
                        catch (Exception deserializeEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deserializing policy: {deserializeEx.Message}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No 'value' property found in response");
                    System.Diagnostics.Debug.WriteLine($"Root element: {jsonDoc.RootElement.ValueKind}");
                }

                System.Diagnostics.Debug.WriteLine($"Fetched {policies.Count} policies");
                return policies;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching underwriter policies: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<UnderwriterPolicy>();
            }
        }

        private async Task<List<InsurerList>> FetchInsurersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching insurers from: {_insurersBaseUrl}");
                var response = await _httpClient.GetAsync(_insurersBaseUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error response: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<InsurerList>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Insurers response length: {content.Length}");
                
                var jsonDoc = JsonDocument.Parse(content);
                var insurers = new List<InsurerList>();

                if (jsonDoc.RootElement.TryGetProperty("value", out var valueElement))
                {
                    System.Diagnostics.Debug.WriteLine($"Found 'value' property with {valueElement.GetArrayLength()} insurers");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        try
                        {
                            var insurer = JsonSerializer.Deserialize<InsurerList>(item.GetRawText(), options);
                            if (insurer != null)
                            {
                                insurers.Add(insurer);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deserializing insurer: {ex.Message}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Fetched {insurers.Count} insurers");
                return insurers;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching insurers: {ex.Message}");
                return new List<InsurerList>();
            }
        }

        private async Task<List<ProductClass>> FetchProductClassesAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Fetching product classes from: {_productClassesBaseUrl}");
                var response = await _httpClient.GetAsync(_productClassesBaseUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error response: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<ProductClass>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Product classes response length: {content.Length}");
                
                var jsonDoc = JsonDocument.Parse(content);
                var productClasses = new List<ProductClass>();

                if (jsonDoc.RootElement.TryGetProperty("value", out var valueElement))
                {
                    System.Diagnostics.Debug.WriteLine($"Found 'value' property with {valueElement.GetArrayLength()} product classes");
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    foreach (var item in valueElement.EnumerateArray())
                    {
                        try
                        {
                            var productClass = JsonSerializer.Deserialize<ProductClass>(item.GetRawText(), options);
                            if (productClass != null)
                            {
                                productClasses.Add(productClass);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error deserializing product class: {ex.Message}");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Fetched {productClasses.Count} product classes");
                return productClasses;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching product classes: {ex.Message}");
                return new List<ProductClass>();
            }
        }
    }
}
