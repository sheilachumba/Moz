using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientPortal.Models
{
    public class RegisterVm
    {
        [Required] public AccountType AccountType { get; set; }

        // Individual
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }

        // Company
        public string? CompanyName { get; set; }

        // Sole Proprietor
        public string? SoleProprietorName { get; set; }

        // Association
        public string? AssociationName { get; set; }

        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required, Phone] public string Phone { get; set; } = default!;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = default!;
        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;
    }

    public class LoginVm
    {
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = default!;
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordVm
    {
        [Required, EmailAddress] public string Email { get; set; } = default!;
    }

    public class ResetPasswordVm
    {
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required] public string Token { get; set; } = default!;
        [Required, DataType(DataType.Password)] public string NewPassword { get; set; } = default!;
        [DataType(DataType.Password), Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = default!;
    }

    // =====================
    // KYC Checklist ViewModels
    // =====================
    public record ChecklistRequirement(string Key, string DisplayName, bool Required, string? Note = null);

    public record ChecklistVm(
        KycType Type,
        IEnumerable<ChecklistRequirement> Requirements,
        bool Acknowledged,
        string CurrentKycStatus = "NotStarted",
        bool IsKycCompleted = false,
        string CompanyId = "",
        DateTime? SubmittedDate = null,
        string ScreeningReference = ""
    );

    // =====================
    // CLAIM CREATION VIEWMODEL
    // =====================
    public class ClaimCreateVm
    {
        public List<string>? PolicyNumbers { get; set; }

        [Required, Display(Name = "Policy No")]
        public string? PolicyNumber { get; set; }

        [Required, Display(Name = "Loss Type")]
        public string? LossType { get; set; }

        [Display(Name = "Loss Type Description")]
        public string? LossTypeDescription { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of Notification")]
        public DateTime? DateOfNotification { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of Occurrence")]
        public DateTime? DateOfOccurrence { get; set; }

        [Display(Name = "Loss Description")]
        public string? LossDescription { get; set; }

        public string? Occupation { get; set; }

        [Display(Name = "Risk Description")]
        public string? RiskDescription { get; set; }

        [Display(Name = "Branch Name")]
        public string? BranchName { get; set; }

        [Range(0, double.MaxValue), Display(Name = "Claim Amount")]
        public decimal? Amount { get; set; }
    }

    // =====================
    // PRODUCTS – GROUPED VIEWMODELS
    // (No DB change; we derive MainClass in code)
    // =====================
    public class ProductCardVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Class { get; set; } = "";     // Subclass from DB (e.g., LIFE, HEALTH, MOTOR, PROPERTY, LIABILITY, TRANSPORT, TRAVEL)
        public string MainClass { get; set; } = ""; // Derived: "Life & Health" | "Motor" | "Property" | "Liability"

        // For image path: /images/products/{slug}.jpg
        public string ImageSlug
        {
            get
            {
                var slug = (Name ?? string.Empty).ToLowerInvariant()
                    .Replace(" & ", " and ")
                    .Replace("/", " ")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(".", "")
                    .Replace("-", " ")
                    .Trim();
                while (slug.Contains("  ")) slug = slug.Replace("  ", " ");
                return slug.Replace(' ', '-');
            }
        }
    }

    public class ProductSubGroupVm
    {
        public string SubClass { get; set; } = "";      // e.g., LIFE, HEALTH, MOTOR, PROPERTY, LIABILITY, TRANSPORT, TRAVEL
        public List<ProductCardVm> Items { get; set; } = new();
    }

    public class ProductGroupVm
    {
        public string MainClass { get; set; } = "";     // "Life & Health" | "Motor" | "Property" | "Liability"
        public List<ProductSubGroupVm> SubGroups { get; set; } = new();
    }
}
