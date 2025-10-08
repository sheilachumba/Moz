using ClientPortal.Models; 
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class KycSubmissionService
{
    private static readonly string baseUrl = "http://196.201.224.102:2048/BC260/ODataV4/";
    private static readonly string companyEndpoint = "Company('STANDARD%20INSURANCE')/Customercard";
    private static readonly string insuredCardEndpoint = "Company('STANDARD%20INSURANCE')/Insuredcard";

    public async Task<string> GetCustomersAsync()
    {
        using var client = new HttpClient();
        var byteArray = Encoding.ASCII.GetBytes("Administrator:Insurance@2030#");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response = await client.GetAsync(baseUrl + companyEndpoint);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<bool> SubmitIndividualKycAsync(IndividualKyc individualKyc)
    {
        // TODO: Map and serialize individualKyc to BC payload
        //using var client = new HttpClient();
        //var byteArray = Encoding.ASCII.GetBytes("Administrator:Insurance@2030#");
        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        //var json = "{var bcPayload = new\r\n{\r\n    Title = individualKyc.Title, // e.g., \"MR\"\r\n    First_name = individualKyc.FirstName,\r\n    Surname = individualKyc.LastName,\r\n    Other_Names = individualKyc.MiddleName,\r\n    Name = $\"{individualKyc.FirstName} {individualKyc.LastName}\".Trim(),\r\n    Sex = individualKyc.Gender,\r\n    Date_of_Birth = individualKyc.DateOfBirth?.ToString(\"yyyy-MM-dd\"),\r\n    Marital_Status = individualKyc.MaritalStatus,\r\n    Occupation = individualKyc.Profession,\r\n    Nationality = individualKyc.Nationality,\r\n    Physical_Address = individualKyc.AddressLine,\r\n    Postal_Address = individualKyc.PostalAddress,\r\n    Post_Code = individualKyc.PostCode,\r\n    City = individualKyc.City,\r\n    State = individualKyc.State,\r\n    Country_Region_Code = individualKyc.CountryCode,\r\n    Primary_Phone_No = individualKyc.Phone,\r\n    Primary_Email = individualKyc.Email,\r\n    NUIT = individualKyc.NUIT,\r\n    Approval_Status = \"Open\",\r\n    Customer_Status = \"Active\",\r\n    Customer_Posting_Group = \"INSURED\",\r\n    // Add other fields as needed, using defaults or empty strings if not collected\r\n    BVN_No = \"\",\r\n    Means_of_Identification = individualKyc.IdentityType,\r\n    ID_Number = individualKyc.IdentityNumber,\r\n    ID_Expiry_Date = individualKyc.IdentityExpiryDate?.ToString(\"yyyy-MM-dd\"),\r\n    Religion_Code = \"\",\r\n    Religion_Name = \"\",\r\n    SLA_Process = \"\",\r\n    Relationship_Manager = \"\",\r\n    Relationship_Manager_Name = \"\",\r\n    PEP_Status = \"\",\r\n    High_Risk_Low_Risk = \"0\",\r\n    Segments = \"\",\r\n    Subsegments = \"\",\r\n    Customer_Category = \"0\",\r\n    Source_of_Funds = \"\",\r\n    Work_Physical = \"\",\r\n    E_Mail_2 = \"\",\r\n    Privacy_Blocked = false,\r\n    Accept_Marketing_Communication = false,\r\n    Accept_Renewal_Email = true,\r\n    Accept_Renewal_SMS = true,\r\n    Data_Protection_Consent = false,\r\n    KYC = false,\r\n    Utility_Bill = false,\r\n    Address_Verification = false,\r\n    Next_of_Kin_Title = individualKyc.NextOfKinTitle,\r\n    Next_of_kin_Name = individualKyc.NextOfKinName,\r\n    Next_of_Kin_Gender = individualKyc.NextOfKinGender,\r\n    Next_of_kin_Email = individualKyc.NextOfKinEmail,\r\n    Next_of_Kin_Phone_No = individualKyc.NextOfKinPhone,\r\n    Next_of_kin_address = individualKyc.NextOfKinAddress,\r\n    Next_of_kin_DOB = individualKyc.NextOfKinDOB?.ToString(\"yyyy-MM-dd\"),\r\n    Next_of_Kin_Relationship = individualKyc.NextOfKinRelationship,\r\n    Officers_Name = \"SIIBL-CIC-DEMO\\\\ADMINISTRATOR\",\r\n    Onboarding_Date = DateTime.UtcNow.ToString(\"yyyy-MM-dd\"),\r\n    Intermediary_No = \"\",\r\n    Intermediary_Name = \"\",\r\n    Classification = \"\",\r\n    Balance = 0,\r\n    Balance_LCY = 0,\r\n    Bill_to_Customer_No = \"\",\r\n    Preferred_Bank_Account_Code = \"\",\r\n    Blocked = \"\",\r\n    Blocking_date = \"0001-01-01\",\r\n    Unblocking_date = \"0001-01-01\",\r\n    Global_Dimension_1_Filter = \"\",\r\n    Global_Dimension_2_Filter = \"\",\r\n    Currency_Filter = \"\"\r\n};\r\n\r\nvar json = JsonSerializer.Serialize(bcPayload);\r\n}";


        //var content = new StringContent(json, Encoding.UTF8, "application/json");
        //var response = await client.PostAsync(baseUrl + insuredCardEndpoint, content);
        //return response.IsSuccessStatusCode;


        using var handler = new HttpClientHandler
        {
            Credentials = new NetworkCredential("Administrator", "Insurance@2030#", "YourDomain")
        };

        using var client = new HttpClient(handler);

        string url = baseUrl + insuredCardEndpoint;

        // Serialize the accountApplication object to JSON
        var jsonData = JsonSerializer.Serialize(individualKyc);
        var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

        try
        {
            // Send the POST request
            var response = await client.PostAsync(url, content);

            // Check if the response is successful
            if (response.IsSuccessStatusCode)
            {
                // Deserialize the response content into the same type of object
                var responseData = await response.Content.ReadAsStringAsync();
                var createdObject = JsonSerializer.Deserialize<IndividualKyc>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (createdObject == null)
                {
                    throw new Exception("The response content is null or could not be deserialized.");
                }
                return response.IsSuccessStatusCode;
            }
            else
            {
                // Get error content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to post data. StatusCode: {response.StatusCode}, Error: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            throw new Exception($"An error occurred while creating Account Application: {ex.Message}", ex);
        }
    }

    public async Task<bool> SubmitCompanyKycAsync(CompanyKyc companyKyc)
    {
        // TODO: Map and serialize companyKyc to BC payload
        using var client = new HttpClient();
        var byteArray = Encoding.ASCII.GetBytes("Administrator:Insurance@2030#");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        var json = "{}"; 
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(baseUrl + insuredCardEndpoint, content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UploadKycDocumentAsync(string insuredCardNo, string filePath, string fileName)
    {
        using var client = new HttpClient();
        var byteArray = Encoding.ASCII.GetBytes("Administrator:Insurance@2030#");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(filePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);
        var endpoint = $"{baseUrl}Company('STANDARD%20INSURANCE')/Insuredcard('{insuredCardNo}')/attachments";
        var response = await client.PostAsync(endpoint, content);
        return response.IsSuccessStatusCode;
    }

}
