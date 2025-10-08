using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
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

    // VMs you already use
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
        public int Id { get; set; }
        [Required] public string UserId { get; set; } = default!;

        [Required] public string FirstName { get; set; } = default!;
        public string? MiddleName { get; set; }
        [Required] public string LastName { get; set; } = default!;

        [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? Gender { get; set; }
        public string? MaritalStatus { get; set; }
        public string? IdType { get; set; }
        public string? IdNumber { get; set; }
        public string? NUIT { get; set; }

        [Phone] public string? Phone { get; set; }
        [EmailAddress] public string? Email { get; set; }

        public string? AddressLine { get; set; }
        public string? AddressProofType { get; set; }
        public string? IncomeProofType { get; set; }
        public string? Country { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Street { get; set; }
        public string? EmployerAddress { get; set; }
        public string? Profession { get; set; }

        public KycStatus Status { get; set; } = KycStatus.Draft;
        public DateTime? SubmittedAt { get; set; }
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
