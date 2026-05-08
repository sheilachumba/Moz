using System.Text.Json;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IPlanService
    {
        Task<List<PlanViewModel>> GetPlansByInsurerAndClassAsync(string insurerCode, string planClass);
        Task<List<PlanViewModel>> GetAllPlansAsync();
        Task<List<PlanViewModel>> GetPlansByClassNameAsync(string className);
    }

    public class PlanService : IPlanService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _plansBaseUrl;

        public PlanService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            var baseUrl = _configuration["BcApi:BaseUrl"];
            _plansBaseUrl = $"{baseUrl}Company('STANDARD%20INSURANCE')/plans";
        }

        public async Task<List<PlanViewModel>> GetPlansByInsurerAndClassAsync(string insurerCode, string planClass)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[PlanService] Fetching plans for Insurer: {insurerCode}, Class: {planClass}");

                var config = new BusinessCentralConfig(_configuration);
                var handler = config.CreateHttpClientHandler();
                var client = new HttpClient(handler);
                config.ConfigureHttpClient(client);
                
                // Filter: Class = planClass AND Underwriter_Code = insurerCode
                var filter = $"$filter=Class eq '{planClass}' and Underwriter_Code eq '{insurerCode}'";
                var url = $"{_plansBaseUrl}?{filter}";

                System.Diagnostics.Debug.WriteLine($"[PlanService] URL: {url}");

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[PlanService] Error: {response.StatusCode}");
                    return new List<PlanViewModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[PlanService] Response length: {content.Length}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<Plan>>(content, options);

                if (oDataResponse?.Value == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[PlanService] No plans found");
                    return new List<PlanViewModel>();
                }

                var viewModels = oDataResponse.Value.Select(p => new PlanViewModel
                {
                    Undewriter_Product_Nos = p.Undewriter_Product_Nos,
                    Description = p.Description,
                    Underwriter_Code = p.Underwriter_Code,
                    Class = p.Class,
                    Class_Name = p.Class_Name,
                    Sub_Class = p.Sub_Class,
                    SubClass_Name = p.SubClass_Name,
                    Short_Code = p.Short_Code,
                    Short_Name = p.Short_Name,
                    Commision_Percent_age = p.Commision_Percent_age,
                    Premium_Rate = p.Premium_Rate,
                    Account_Type = p.Account_Type,
                    Schedule_Line = p.Schedule_Line
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[PlanService] Fetched {viewModels.Count} plans");
                return viewModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PlanService] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[PlanService] Stack trace: {ex.StackTrace}");
                return new List<PlanViewModel>();
            }
        }

        public async Task<List<PlanViewModel>> GetAllPlansAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[PlanService] Fetching all plans");

                var config = new BusinessCentralConfig(_configuration);
                var handler = config.CreateHttpClientHandler();
                var client = new HttpClient(handler);
                config.ConfigureHttpClient(client);

                var url = _plansBaseUrl;
                System.Diagnostics.Debug.WriteLine($"[PlanService] URL: {url}");

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[PlanService] Error (GetAll): {response.StatusCode}");
                    return new List<PlanViewModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[PlanService] Response length (GetAll): {content.Length}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<Plan>>(content, options);

                if (oDataResponse?.Value == null)
                {
                    System.Diagnostics.Debug.WriteLine("[PlanService] No plans found (GetAll)");
                    return new List<PlanViewModel>();
                }

                var viewModels = oDataResponse.Value.Select(p => new PlanViewModel
                {
                    Undewriter_Product_Nos = p.Undewriter_Product_Nos,
                    Description = p.Description,
                    Underwriter_Code = p.Underwriter_Code,
                    Class = p.Class,
                    Class_Name = p.Class_Name,
                    Sub_Class = p.Sub_Class,
                    SubClass_Name = p.SubClass_Name,
                    Short_Code = p.Short_Code,
                    Short_Name = p.Short_Name,
                    Commision_Percent_age = p.Commision_Percent_age,
                    Premium_Rate = p.Premium_Rate,
                    Account_Type = p.Account_Type,
                    Schedule_Line = p.Schedule_Line
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[PlanService] Fetched {viewModels.Count} plans (GetAll)");
                return viewModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PlanService] Exception (GetAll): {ex.Message}");
                return new List<PlanViewModel>();
            }
        }

        public async Task<List<PlanViewModel>> GetPlansByClassNameAsync(string className)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(className))
                    return new List<PlanViewModel>();

                System.Diagnostics.Debug.WriteLine($"[PlanService] Fetching plans for Class_Name: {className}");

                var config = new BusinessCentralConfig(_configuration);
                var handler = config.CreateHttpClientHandler();
                var client = new HttpClient(handler);
                config.ConfigureHttpClient(client);

                // Escape single quotes for OData
                var safeClassName = className.Replace("'", "''");
                var filter = "$filter=" + $"Class_Name eq '{safeClassName}'";
                var url = $"{_plansBaseUrl}?{filter}";

                System.Diagnostics.Debug.WriteLine($"[PlanService] URL (ByClassName): {url}");

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[PlanService] Error (ByClassName): {response.StatusCode}");
                    return new List<PlanViewModel>();
                }

                var content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[PlanService] Response length (ByClassName): {content.Length}");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var oDataResponse = JsonSerializer.Deserialize<ODataResponse<Plan>>(content, options);

                if (oDataResponse?.Value == null)
                {
                    System.Diagnostics.Debug.WriteLine("[PlanService] No plans found (ByClassName)");
                    return new List<PlanViewModel>();
                }

                var viewModels = oDataResponse.Value.Select(p => new PlanViewModel
                {
                    Undewriter_Product_Nos = p.Undewriter_Product_Nos,
                    Description = p.Description,
                    Underwriter_Code = p.Underwriter_Code,
                    Class = p.Class,
                    Class_Name = p.Class_Name,
                    Sub_Class = p.Sub_Class,
                    SubClass_Name = p.SubClass_Name,
                    Short_Code = p.Short_Code,
                    Short_Name = p.Short_Name,
                    Commision_Percent_age = p.Commision_Percent_age,
                    Premium_Rate = p.Premium_Rate,
                    Account_Type = p.Account_Type,
                    Schedule_Line = p.Schedule_Line
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"[PlanService] Fetched {viewModels.Count} plans (ByClassName)");
                return viewModels;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PlanService] Exception (ByClassName): {ex.Message}");
                return new List<PlanViewModel>();
            }
        }
    }
}
