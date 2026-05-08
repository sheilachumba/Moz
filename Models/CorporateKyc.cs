using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class CorporateKyc
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string? UserId { get; set; }
       
        [JsonConverter(typeof(KycStatusConverter))]
        public KycStatus Application_Status { get; set; } = KycStatus.Open;
        [JsonIgnore]
        public DateTime? SubmittedAt { get; set; }
        [JsonPropertyName("@odata.etag")]
        public string? @odata_etag { get; set; }

        public string? No { get; set; }
        public string? Contact_Type { get; set; } = "Corporate";
        public string? Salesforce_ID { get; set; }
        // Title is not a property - it's always set to "Corporate" in the payload
        [Required]
        public string Name { get; set; }
        public string? Name_2 { get; set; }
        public string? Type { get; set; } = "Corporate";
        public string? Company_No { get; set; }
        public string? Company_Name { get; set; }
        public string? Company_Registration { get; set; }
        public string? Tax_ID { get; set; }
        public string? Date_of_Incorporation { get; set; }
        public string? Sector_Description { get; set; }
      
        public string? Segments { get; set; }
        public string? Subsegments { get; set; }
        public string? Company_Representative { get; set; }
        public string? Legal_Representative { get; set; }
        public string? Residency_Status { get; set; }
        public string? Legal_Representative_NUIT { get; set; }
        public string? Means_of_Identification { get; set; }
        public string? PEP_Status { get; set; }
        public string? Source_of_Funds { get; set; }
        public string? Approval_Status { get; set; } = "Open";
        public string? Customer_Category { get; set; }
        public string? High_Risk_Low_Risk { get; set; }
        public string? Stage { get; set; }
        public string? RM_Code { get; set; }
        public string? RM_Name { get; set; }
        public string? Officers_Name { get; set; }
        public string? Onboarding_Date { get; set; }
        public string? Job_Title { get; set; }
        public string? Contact_Business_Relation { get; set; }
        public string? IntegrationCustomerNo { get; set; }
        public string? Search_Name { get; set; }
        public string? Salesperson_Code { get; set; }
        public string? Salutation_Code { get; set; }
        public string? Organizational_Level_Code { get; set; }
        public string? LastDateTimeModified { get; set; }
        public string? Date_of_Last_Interaction { get; set; }
        public string? Last_Date_Attempted { get; set; }
        public string? Next_Task_Date { get; set; }
        public bool Exclude_from_Segment { get; set; }
        public bool Privacy_Blocked { get; set; }
        public string? Registration_Number { get; set; }
        [Required]
        public string Address { get; set; }
        public string? Address_2 { get; set; }
        [Required]
        public string Country_Region_Code { get; set; }
        public string? Post_Code { get; set; }
        public string? City { get; set; }
        public string? County { get; set; }
        [Required]
        public string Phone_No { get; set; }
        public string? E_Mail { get; set; }
        public string? E_Mail_2 { get; set; }
        public string? Fax_No { get; set; }
        public string? Home_Page { get; set; }
        public string? Correspondence_Type { get; set; }
        public string? Language_Code { get; set; }
        public string? Format_Region { get; set; }
        public string? Currency_Code { get; set; }
        public string? Territory_Code { get; set; }
        public string? VAT_Registration_No { get; set; }
        public string? BVN_No { get; set; }
        public string? Next_of_kin_Name { get; set; }
        public string? Next_of_kin_Email { get; set; }
        public string? Next_of_Kin_Phone_No { get; set; }
        public string? Insured_Number { get; set; }
        [JsonIgnore]
        public virtual Users? User { get; set; }
    }
}
