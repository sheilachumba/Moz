using System;

namespace MOZ_UPGRADE.Models
{
    public class ClaimListItem
    {
        public string Claim_No { get; set; }
        public string Policy_No { get; set; }
        public string Name_of_Insured { get; set; }
        public decimal Premium_Balance { get; set; }
        public string Approval_Status { get; set; }
        public string Claim_Status { get; set; }
        public string Renewal_Date { get; set; }
        public string ID_Number { get; set; }
        public string Insured_Telephone_No { get; set; }
        public string Date_of_Birth { get; set; }
        public string Sex { get; set; }
        public string Insured_Address { get; set; }
        public string Occupation { get; set; }
        public string Loss_Type { get; set; }
        public string Loss_Type_Description { get; set; }
        public string Date_Notified { get; set; }
        public string Claim_Stage { get; set; }
        public string Claim_Stage_Description { get; set; }
        public string Class { get; set; }
        public string Class_Name { get; set; }
        public string Risk_Description { get; set; }
        public string Date_of_Occurence { get; set; }
        public decimal Estimated_Amount { get; set; }
        public decimal Offered_Amount { get; set; }
        public decimal Expected_Amount { get; set; }
        public decimal Amount_Settled { get; set; }
    }
}
