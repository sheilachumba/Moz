using System;
using System.Collections.Generic;

namespace MOZ_UPGRADE.Interfaces
{
    public class QuoteRiskLine
    {
        public string Family_Name { get; set; } = string.Empty;
        public string First_Names { get; set; } = string.Empty;
        public string Other_Names { get; set; } = string.Empty;
        public int Ages { get; set; }
        public int GenderToken { get; set; }
        public string Relationship_to_Applicant { get; set; } = string.Empty;
        public string Cover_Description { get; set; } = string.Empty;
        public decimal Sum_Insured { get; set; }
        public decimal Gross_Premium { get; set; }

        // Core cover-style fields (mirroring TestPage's RiskFormModel)
        public decimal RatePercentage { get; set; }
        public string RateType { get; set; } = string.Empty;
        public decimal RateDiscount { get; set; }

        // Motor / Vehicle
        public string RegistrationNo { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Colour { get; set; } = string.Empty;
        public string ChassisNo { get; set; } = string.Empty;
        public string EngineNo { get; set; } = string.Empty;
        public int CubicCapacity { get; set; }
        public int CarryingCapacity { get; set; }
        public string VehicleUsage { get; set; } = string.Empty;

        // Property
        public string PropertyCategory { get; set; } = string.Empty;
        public string PropertyDetails { get; set; } = string.Empty;
        public decimal PropertyValue { get; set; }
        public string PropertyPermanent { get; set; } = string.Empty;
        public string PropertyOccupied { get; set; } = string.Empty;
        public string BusinessOnPremise { get; set; } = string.Empty;

        // Travel
        public string TravelCategory { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public string CountryZone { get; set; } = string.Empty;
        public string PurposeOfTravel { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string PassengerPolicyHolder { get; set; } = string.Empty;

        // Marine
        public string MarineCategory { get; set; } = string.Empty;
        public string PortOfOrigin { get; set; } = string.Empty;
        public string PortOfDestination { get; set; } = string.Empty;
        public decimal InvoiceValue { get; set; }
        public string TaxIdentificationNo { get; set; } = string.Empty;
        public string CargoType { get; set; } = string.Empty;
        public string CharterType { get; set; } = string.Empty;
        public string Conditions { get; set; } = string.Empty;
    }

    public class QuoteContext
    {
        public string PlanNo { get; set; } = string.Empty;
        public string UnderwriterCode { get; set; } = string.Empty;
        public string PlanClass { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PremiumRate { get; set; }
        public string ScheduleLine { get; set; } = string.Empty;

        public DateTime StartingDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public int SourceOfBusiness { get; set; }
        public string InsuredAddress { get; set; } = string.Empty;

        public List<QuoteRiskLine> RiskLines { get; set; } = new List<QuoteRiskLine>();
    }

    public interface IQuoteContextService
    {
        void SetQuote(QuoteContext context);
        QuoteContext? GetQuote();
        void Clear();
    }
}
