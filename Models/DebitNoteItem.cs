namespace MOZ_UPGRADE.Models
{
    public class DebitNoteItem
    {
        public string No { get; set; }
        public string Policy_Type { get; set; }
        public string Policy_Description { get; set; }
        public string Insured_No { get; set; }
        public string Insured_Name { get; set; }
        public string Insured_Address { get; set; }
        public string E_mail { get; set; }
        public string Endorsement_Type { get; set; }
        public string Action_Type { get; set; }
        public string Document_Date { get; set; }
        public string From_Date { get; set; }
        public string To_Date { get; set; }
        public string Cover_Start_Date { get; set; }
        public string Cover_End_Date { get; set; }
        public int No_Of_Instalments { get; set; }
        public string Policy_No { get; set; }
        public string Quotation_No { get; set; }
        public decimal Total_Premium_Amount { get; set; }
        public decimal Total_Tax { get; set; }
        public decimal Total_Sum_Insured { get; set; }
        public string Risk_Description { get; set; }
        public string Status { get; set; }
        public string Pay_Status { get; set; }
    }
}

