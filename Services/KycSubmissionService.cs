using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClientPortal.Models;

public class KycSubmissionService
{
    private readonly BusinessCentralBasicApiService _bcApi;

    public KycSubmissionService(BusinessCentralBasicApiService bcApi)
    {
        _bcApi = bcApi;
    }

    // Map your CompanyKyc model to the BC API JSON structure and submit
    public async Task<bool> SubmitCompanyKycAsync(CompanyKyc companyKyc)
    {
        // Map your CompanyKyc to the expected BC API JSON payload
        var bcPayload = new
        {
            name = companyKyc.CompanyName,
            registrationNo = companyKyc.RegistrationNo,
            countryOfRegistration = companyKyc.CountryOfReg,
            address = companyKyc.RegisteredAddress,
            activityDescription = companyKyc.ActivityDesc,
            fieldOfActivity = companyKyc.FieldOfActivity,
            nuit = companyKyc.NUIT,
            contactPhone = companyKyc.ContactPhone,
            contactEmail = companyKyc.ContactEmail,
            
        };

        var json = JsonSerializer.Serialize(bcPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        
        var bcEndpoint = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/Insuredcard";

        var response = await _bcApi.PostAsync(bcEndpoint, content);

        return response.IsSuccessStatusCode;
    }
}
