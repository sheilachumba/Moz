using ClientPortal.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KycSubmissionService
{
    public class KycSubmissionService
    {
        private readonly string _url = "http://196.201.224.102:2048/BC260/ODataV4/Company('STANDARD%20INSURANCE')/Insuredcard";
        private readonly string _username = "Administrator";
        private readonly string _password = "Insurance@2030#";

        // 1. Mapping Helper
        private object MapToBusinessCentralPayload(IndividualKyc kyc)
        {
            return new
            {
                Title = kyc.Title,
                First_name = kyc.FirstName,
                Surname = kyc.LastName,
                Other_Names = kyc.MiddleName,
                Name = kyc.FullName ?? $"{kyc.FirstName} {kyc.LastName}",
                Sex = kyc.Gender,
                Date_of_Birth = kyc.DateOfBirth?.ToString("yyyy-MM-dd"),
                Marital_Status = kyc.MaritalStatus,
                Occupation = kyc.Occupation,
                Nationality = kyc.Nationality,
                BVN_No = kyc.BvnNo,
                Means_of_Identification = kyc.IdType,
                ID_Number = kyc.IdNumber,
                ID_Expiry_Date = kyc.IdExpiryDate?.ToString("yyyy-MM-dd"),
                Religion_Code = kyc.ReligionCode,
                Religion_Name = kyc.ReligionName,
                Approval_Status = kyc.ApprovalStatus ?? "Open",
                SLA_Process = kyc.SlaProcess,
                Relationship_Manager = kyc.RelationshipManager,
                Relationship_Manager_Name = kyc.RelationshipManagerName,
                PEP_Status = kyc.PepStatus,
                High_Risk_Low_Risk = kyc.HighRiskLowRisk,
                Segments = kyc.Segments,
                Subsegments = kyc.Subsegments,
                Customer_Category = kyc.CustomerCategory,
                Source_of_Funds = kyc.SourceOfFunds,
                Physical_Address = kyc.PhysicalAddress,
                Postal_Address = kyc.PostalAddress,
                Work_Physical = kyc.WorkPhysical,
                Post_Code = kyc.PostCode,
                City = kyc.City,
                State = kyc.State,
                Country_Region_Code = kyc.CountryRegionCode,
                Primary_Phone_No = kyc.PrimaryPhoneNo,
                Phone_No = kyc.PhoneNo,
                Primary_Email = kyc.PrimaryEmail,
                E_Mail_2 = kyc.Email2,
                Privacy_Blocked = kyc.PrivacyBlocked,
                Accept_Marketing_Communication = kyc.AcceptMarketingCommunication,
                Accept_Renewal_Email = kyc.AcceptRenewalEmail,
                Accept_Renewal_SMS = kyc.AcceptRenewalSms,
                Data_Protection_Consent = kyc.DataProtectionConsent,
                KYC = kyc.KycFlag,
                Utility_Bill = kyc.UtilityBill,
                Address_Verification = kyc.AddressVerification,
                Next_of_Kin_Title = kyc.NextOfKinTitle,
                Next_of_kin_Name = kyc.NextOfKinName,
                Next_of_Kin_Gender = kyc.NextOfKinGender,
                Next_of_Kin_Email = kyc.NextOfKinEmail,
                Next_of_Kin_Phone_No = kyc.NextOfKinPhoneNo,
                Next_of_kin_address = kyc.NextOfKinAddress,
                Next_of_kin_DOB = kyc.NextOfKinDob?.ToString("yyyy-MM-dd"),
                Next_of_Kin_Relationship = kyc.NextOfKinRelationship,
                Officers_Name = kyc.OfficersName,
                Onboarding_Date = kyc.OnboardingDate?.ToString("yyyy-MM-dd"),
                Intermediary_No = kyc.IntermediaryNo,
                Intermediary_Name = kyc.IntermediaryName,
                Classification = kyc.Classification,
                Balance = kyc.Balance,
                Balance_LCY = kyc.BalanceLcy,
                Bill_to_Customer_No = kyc.BillToCustomerNo,
                Customer_Posting_Group = kyc.CustomerPostingGroup ?? "INSURED",
                Preferred_Bank_Account_Code = kyc.PreferredBankAccountCode,
                Blocked = kyc.Blocked,
                Customer_Status = kyc.CustomerStatus ?? "Active",
                Blocking_date = kyc.BlockingDate?.ToString("yyyy-MM-dd"),
                Unblocking_date = kyc.UnblockingDate?.ToString("yyyy-MM-dd"),
                Global_Dimension_1_Filter = kyc.GlobalDimension1Filter,
                Global_Dimension_2_Filter = kyc.GlobalDimension2Filter,
                Currency_Filter = kyc.CurrencyFilter
            };
        }

        // 2. POST Method
        public async Task<bool> SubmitKycToBusinessCentralAsync(IndividualKyc kyc)
        {
            var payload = BcPayload(kyc);
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { IgnoreNullValues = true });

            using var client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($"{_username}:{_password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_url, content);

            if (response.IsSuccessStatusCode)
            {
                // Success: handle as needed
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                // Log or throw error as needed
                throw new Exception($"Business Central POST failed: {response.StatusCode} - {error}");
            }
        }
    }

}