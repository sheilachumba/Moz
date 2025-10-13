using ClientPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class KycSubmissionService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _companyEndpoint;
    private readonly string _insuredCardEndpoint;

    public KycSubmissionService(IConfiguration config)
    {
        _baseUrl = config["BcApi:BaseUrl"] ?? "http://196.201.224.102:2048/BC260/ODataV4/";
        _companyEndpoint = config["BcApi:CompanyEndpoint"] ?? "Company('STANDARD%20INSURANCE')/Customercard";
        _insuredCardEndpoint = config["BcApi:InsuredCardEndpoint"] ?? "Company('STANDARD%20INSURANCE')/Insuredcard";

        var handler = new HttpClientHandler
        {
            Credentials = new NetworkCredential(
                config["BcApi:Administrator"] ?? "Administrator",
                config["BcApi:Password"] ?? "Insurance@2030#")
        };
        _httpClient = new HttpClient(handler);
    }

    public async Task<string> GetCustomersAsync()
    {
        var response = await _httpClient.GetAsync(_baseUrl + _companyEndpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> SubmitIndividualKycAsync(IndividualKyc individualKyc)
    {
        var bcPayload = new
        {
            Title = individualKyc.Title,
            First_name = individualKyc.FirstName,
            Surname = individualKyc.LastName,
            Other_Names = individualKyc.MiddleName,
            Name = $"{individualKyc.FirstName} {individualKyc.LastName}".Trim(),
            Sex = individualKyc.Gender,
            Date_of_Birth = individualKyc.DateOfBirth?.ToString("yyyy-MM-dd"),
            Marital_Status = individualKyc.MaritalStatus,
            Occupation = individualKyc.Profession,
            Nationality = individualKyc.Nationality,
            Physical_Address = individualKyc.AddressLine,
            Postal_Address = individualKyc.PostalAddress,
            Post_Code = individualKyc.PostCode,
            City = individualKyc.City,
            State = individualKyc.State,
            Country_Region_Code = individualKyc.CountryCode,
            Primary_Phone_No = individualKyc.Phone,
            Primary_Email = individualKyc.Email,
            NUIT = individualKyc.NUIT,
            Approval_Status = "Open",
            Customer_Status = "Active",
            Customer_Posting_Group = "INSURED",
            BVN_No = "",
            Means_of_Identification = individualKyc.IdentityType,
            ID_Number = individualKyc.IdentityNumber,
            ID_Expiry_Date = individualKyc.IdentityExpiryDate?.ToString("yyyy-MM-dd"),
            Religion_Code = "",
            Religion_Name = "",
            SLA_Process = "",
            Relationship_Manager = "",
            Relationship_Manager_Name = "",
            PEP_Status = "",
            High_Risk_Low_Risk = "0",
            Segments = "",
            Subsegments = "",
            Customer_Category = "0",
            Source_of_Funds = individualKyc.SourceOfFunds,
            Work_Physical = "",
            E_Mail_2 = "",
            Privacy_Blocked = false,
            Accept_Marketing_Communication = false,
            Accept_Renewal_Email = true,
            Accept_Renewal_SMS = true,
            Data_Protection_Consent = false,
            KYC = false,
            Utility_Bill = false,
            Address_Verification = false,
            Next_of_Kin_Title = individualKyc.NextOfKinTitle,
            Next_of_kin_Name = individualKyc.NextOfKinName,
            Next_of_Kin_Gender = individualKyc.NextOfKinGender,
            Next_of_kin_Email = individualKyc.NextOfKinEmail,
            Next_of_Kin_Phone_No = individualKyc.NextOfKinPhone,
            Next_of_kin_address = individualKyc.NextOfKinAddress,
            Next_of_kin_DOB = individualKyc.NextOfKinDOB?.ToString("yyyy-MM-dd"),
            Next_of_Kin_Relationship = individualKyc.NextOfKinRelationship,
            Officers_Name = "SIIBL-CIC-DEMO\\ADMINISTRATOR",
            Onboarding_Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Intermediary_No = "",
            Intermediary_Name = "",
            Classification = "",
            Balance = 0,
            Balance_LCY = 0,
            Bill_to_Customer_No = "",
            Preferred_Bank_Account_Code = "",
            Blocked = "",
            Blocking_date = "0001-01-01",
            Unblocking_date = "0001-01-01",
            Global_Dimension_1_Filter = "",
            Global_Dimension_2_Filter = "",
            Currency_Filter = ""
        };

        var json = JsonSerializer.Serialize(bcPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_baseUrl + _insuredCardEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to submit KYC. Status: {response.StatusCode}, Details: {errorContent}");
        }
        return true;
    }

    public async Task<bool> SubmitCompanyKycAsync(CompanyKyc companyKyc)
    {
        // Temporarily use an empty payload or only properties that exist in your model
        var bcPayload = new { };

        var json = JsonSerializer.Serialize(bcPayload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_baseUrl + _insuredCardEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to submit company KYC. Status: {response.StatusCode}, Details: {errorContent}");
        }
        return true;
    }


    public async Task<bool> UploadKycDocumentAsync(string insuredCardNo, string filePath, string fileName)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File path '{filePath}' does not exist.");

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);
        var endpoint = $"{_baseUrl}Company('STANDARD%20INSURANCE')/Insuredcard('{insuredCardNo}')/attachments";
        var response = await _httpClient.PostAsync(endpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload KYC document. Status: {response.StatusCode}, Details: {errorContent}");
        }
        return true;
    }
    //public async Task<ActionResult> Index()
    //{
    //    var salutations = await _service.GetSalutationsAsync();
    //    ViewBag.Salutations = new SelectList(salutations, "Code", "Description");
    //    return View();
    //}
}
