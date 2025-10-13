using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
//using ClientPortal.Models;
namespace ClientPortal.Models
{
    public enum AccountType { Individual = 1, Company = 2, SoleProprietor = 3, Association = 4 }
    public enum KycStatus { NotStarted, Draft, Submitted, Verified }
    public enum KycType { Individual, Company, SoleProprietor, Association }
    public enum ScreeningStatus { Pending, InProgress, Completed, Failed }
    public enum NotificationType { Info, Warning, Success, Error, KycUpdate, PolicyUpdate, ClaimUpdate }

    public class ApplicationUser : IdentityUser
    {
        public AccountType AccountType { get; set; }
        public KycStatus KycStatus { get; set; } = KycStatus.NotStarted;
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public KycType KycType { get; set; }
        public bool KycCompleted { get; set; }
        public DateTime? KycSubmittedDate { get; set; }
        public string? ScreeningReference { get; set; }

        // Dashboard denormalized values (optional)
        public int ActivePoliciesCount { get; set; }
        public int ExpiredPoliciesCount { get; set; }
        public int ActiveRenewalsCount { get; set; }
        public int PendingClaimsCount { get; set; }
        public int SubscribedServicesCount { get; set; }
        public int TotalProductsCount { get; set; }
        public decimal AccountBalance { get; set; }
        public string Currency { get; set; } = "MZN";
        public int UnreadNotificationsCount { get; set; }
        public int NewPoliciesThisMonth { get; set; }
        public int AverageClaimProcessingDays { get; set; }

        public virtual ICollection<Policy> Policies { get; set; } = new List<Policy>();
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
        public virtual ICollection<Renewal> Renewals { get; set; } = new List<Renewal>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<KycScreening> KycScreenings { get; set; } = new List<KycScreening>();
        public virtual ICollection<Interaction> Interactions { get; set; } = new List<Interaction>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    }

    public class KycScreening
    {
        public int Id { get; set; }

        [Required]
        public string CompanyId { get; set; } = string.Empty;

        [Required]
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedDate { get; set; }

        [Required]
        public ScreeningStatus Status { get; set; } = ScreeningStatus.Pending;

        [Required, MaxLength(100)]
        public string ScreeningId { get; set; } = string.Empty;

        public string? Results { get; set; }
        public string? Notes { get; set; }

        public virtual ApplicationUser Company { get; set; } = null!;
    }

    public class Interaction
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Subject { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public string Type { get; set; } = "Call";

        // Use this as the scheduled/actual time of the interaction
        public DateTime InteractionDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class Notification
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;

        [MaxLength(500)] public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public NotificationType Type { get; set; } = NotificationType.Info;
        public string? LinkUrl { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }

    public class Policy
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        [Required] public string PolicyNumber { get; set; } = default!;
        [Required] public string PolicyType { get; set; } = default!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required] public string Status { get; set; } = default!;
        public decimal Premium { get; set; }

        public virtual ApplicationUser User { get; set; } = default!;

        // Convenience for queries
        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
    }

    public class Claim
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        [Required] public string ClaimNumber { get; set; } = default!;
        [Required] public string PolicyNumber { get; set; } = default!;

        [Required] public string Status { get; set; } = default!;
        public DateTime ClaimDate { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public decimal Amount { get; set; }
        [Required] public string Description { get; set; } = default!;

        public virtual ApplicationUser User { get; set; } = default!;
    }

    public class Renewal
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        [Required] public string PolicyNumber { get; set; } = default!;

        public DateTime RenewalDate { get; set; }

        [Required] public string Status { get; set; } = default!; // Pending/Completed

        public virtual ApplicationUser User { get; set; } = default!;
    }

    public class Document
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        [Required] public string DocumentType { get; set; } = default!;
        [Required] public string FileName { get; set; } = default!;
        [Required] public string FilePath { get; set; } = default!;

        public DateTime UploadDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public virtual ApplicationUser User { get; set; } = default!;

        // Helper for UI urgency pill
        public string UrgencyClass
        {
            get
            {
                if (!ExpiryDate.HasValue) return "ok";
                var days = (ExpiryDate.Value - DateTime.UtcNow).TotalDays;
                if (days <= 0) return "danger";
                if (days <= 14) return "warning";
                return "ok";
            }
        }
    }

    // NEW: Quotation entity (used by dashboard recent items & menu)
    public class Quotation
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        [Required, MaxLength(50)] public string Reference { get; set; } = default!; // e.g., QTN-2001

        [Required] public string Status { get; set; } = "Draft"; // Draft/Sent/Accepted/Declined
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public decimal? Amount { get; set; }

        public virtual ApplicationUser User { get; set; } = default!;
    }

    // NEW: Product catalog (for Products & Services page)
    public class Product
    {
        public int Id { get; set; }

        [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
        [MaxLength(60)] public string Class { get; set; } = string.Empty; // Motor/HealthLife/Travel/Property/Other
        public bool Active { get; set; } = true;
        [MaxLength(300)] public string Description { get; set; } = string.Empty;
    }

    public class ClaimQuotationVm
    {
        public string Type { get; set; } = default!;
        public string Reference { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string StatusClass { get; set; } = default!;
        public DateTime UpdatedDate { get; set; }
    }

    public class ExpiringDocumentVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime ExpiryDate { get; set; }
        public string UrgencyClass { get; set; } = default!;
    }

    public class ChecklistAck
    {
        public int Id { get; set; }
        [Required] public string UserId { get; set; } = default!;
        public KycType Type { get; set; }
        public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
    }

public class IndividualKyc

    {
        // ========= Your local/domain fields =========
        [JsonPropertyName("@odata.etag")]
        public string OdataEtag { get; set; } = string.Empty;

        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        public KycStatus Status { get; set; } = KycStatus.Draft;
        public DateTime? SubmittedAt { get; set; }

        // ========= Business Central / OData fields =========
        [JsonPropertyName("No")]
        public string? No { get; set; }

        [JsonPropertyName("Salesforce_ID")]
        public string? SalesforceId { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        // Keep your original name fields aligned to BC keys
        [JsonPropertyName("First_name")]
        [Required]
        public string? FirstName { get; set; }

        [JsonPropertyName("Other_Names")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("Surname")]
        [Required]
        public string? LastName { get; set; }

        [JsonPropertyName("Name")]
        public string? FullName { get; set; }

        [JsonPropertyName("Sex")]
        public string? Gender { get; set; }

        [JsonPropertyName("Date_of_Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("Marital_Status")]
        public string? MaritalStatus { get; set; }

        [JsonPropertyName("Occupation")]
        public string? Occupation { get; set; }

        [JsonPropertyName("Nationality")]
        public string? Nationality { get; set; }

        [JsonPropertyName("BVN_No")]
        public string? BvnNo { get; set; }

        [JsonPropertyName("Means_of_Identification")]
        public string? IdType { get; set; }

        //[JsonIgnore]
        //public string? SelectedSalutation { get; set; }

        public List<SelectListItem>? Salutations { get; set; }
        [NotMapped]
        public string SelectedSalutation { get; set; }

        [JsonPropertyName("ID_Number")]
        public string? IdNumber { get; set; }

        [JsonPropertyName("ID_Expiry_Date")]
        [DataType(DataType.Date)]
        public DateTime? IdExpiryDate { get; set; }

        [JsonPropertyName("Religion_Code")]
        public string? ReligionCode { get; set; }

        [JsonPropertyName("Religion_Name")]
        public string? ReligionName { get; set; }

        [JsonPropertyName("Approval_Status")]
        public string? ApprovalStatus { get; set; }

        [JsonPropertyName("SLA_Process")]
        public string? SlaProcess { get; set; }

        [JsonPropertyName("Relationship_Manager")]
        public string? RelationshipManager { get; set; }

        [JsonPropertyName("Relationship_Manager_Name")]
        public string? RelationshipManagerName { get; set; }

        public string? PepStatus { get; set; }

        public string? HighRiskLowRisk { get; set; }

        public string? Segments { get; set; }

        public string? Subsegments { get; set; }

        public string? CustomerCategory { get; set; }

        public string? SourceOfFunds { get; set; }

        public string? PhysicalAddress { get; set; }

        public string? PostalAddress { get; set; }

        public string? WorkPhysical { get; set; }

        public string? PostCode { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? CountryRegionCode { get; set; }

        public string? PrimaryPhoneNo { get; set; }

        public string? PhoneNo { get; set; }

        public string? PrimaryEmail { get; set; }

        public string? Email2 { get; set; }

        public bool PrivacyBlocked { get; set; }

        public bool AcceptMarketingCommunication { get; set; }

        public bool AcceptRenewalEmail { get; set; }

        public bool AcceptRenewalSms { get; set; }

        public bool DataProtectionConsent { get; set; }

        public bool KycFlag { get; set; }

        public bool UtilityBill { get; set; }

        public bool AddressVerification { get; set; }

        public string? NextOfKinTitle { get; set; }

        public string? NextOfKinName { get; set; }

        public string? NextOfKinGender { get; set; }

        public string? NextOfKinEmail { get; set; }

        public string? NextOfKinPhoneNo { get; set; }

        public string? NextOfKinAddress { get; set; }

        public DateTime? NextOfKinDob { get; set; }

        public string? NextOfKinRelationship { get; set; }

        public string? OfficersName { get; set; }

        public DateTime? OnboardingDate { get; set; }

        public string? IntermediaryNo { get; set; }

        public string? IntermediaryName { get; set; }

        public string? Classification { get; set; }

        public decimal Balance { get; set; }

        public decimal BalanceLcy { get; set; }

        public string? BillToCustomerNo { get; set; }

        public string? CustomerPostingGroup { get; set; }

        public string? PreferredBankAccountCode { get; set; }

        public string? Blocked { get; set; }

        public string? CustomerStatus { get; set; }

        public DateTime? BlockingDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? UnblockingDate { get; set; }

        public string? GlobalDimension1Filter { get; set; }

        public string? GlobalDimension2Filter { get; set; }

        public string? CurrencyFilter { get; set; }
        // ==== Back-compat shims for existing code (Views + Controller) ====
        // These keep your current bindings compiling without changing the razor/controller code.

        // UI expected: Email -> map to BC "Primary_Email"
        [JsonIgnore]
        public string? Email
        {
            get => PrimaryEmail;
            set => PrimaryEmail = value;
        }

        // UI expected: Phone -> map to BC "Primary_Phone_No"
        [JsonIgnore]
        public string? Phone
        {
            get => PrimaryPhoneNo;
            set => PrimaryPhoneNo = value;
        }

        // UI expected: AddressLine -> map to BC "Physical_Address"
        [JsonIgnore]
        public string? AddressLine
        {
            get => PhysicalAddress;
            set => PhysicalAddress = value;
        }

        // UI expected: NUIT (Moz tax id). Not in the BC individual payload provided.
        // Keep as domain-only so your forms compile; map to BC later if needed.
        [JsonIgnore]
        public string? NUIT { get; set; }

        // UI expected: AddressProofType, IncomeProofType (document-type selections).
        // Domain-only (not part of BC fields); keep for forms/workflow.
        [JsonIgnore]
        public string? AddressProofType { get; set; }

        [JsonIgnore]
        public string? IncomeProofType { get; set; }


    }
    public class MeansOfIdentificationVm
    {
        public string Means_of_ID { get; set; }
        public string Description { get; set; }
    }

    public class CompanyKyc
    {
        public int Id { get; set; }
        [Required] public string UserId { get; set; } = default!;

        [Required] public string CompanyName { get; set; } = default!;
        public string? DocType { get; set; }
        public string? DocNumber { get; set; }
        public string? RegistrationNo { get; set; }
        public string? CountryOfReg { get; set; }
        public string? RegisteredAddress { get; set; }
        public string? ActivityDesc { get; set; }
        public string? FieldOfActivity { get; set; }
        public string? NUIT { get; set; }

        [Phone] public string? ContactPhone { get; set; }
        [EmailAddress] public string? ContactEmail { get; set; }

        public KycStatus Status { get; set; } = KycStatus.Draft;
        public DateTime? SubmittedAt { get; set; }
    }

    public class KycDocument
    {
        public int Id { get; set; }

        [Required] public string UserId { get; set; } = default!;
        public KycType KycType { get; set; }

        [Required] public string DocTypeKey { get; set; } = default!;
        [Required] public string FilePath { get; set; } = default!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public bool IsRequired { get; set; }
        public bool Verified { get; set; }
    }
}
