namespace MOZ_UPGRADE.Models
{
    public class PolicyCardItem
    {
        public string No { get; set; }
        public string Policy_Status { get; set; }
        public string Quotation_No { get; set; }
        public string Underwriter { get; set; }
        public string Underwriter_Name { get; set; }
        public string Insured_No { get; set; }
        public string Insured_Name { get; set; }
        public string Policy_Type { get; set; }
        public string Policy_Description { get; set; }
        public string From_Date { get; set; }
        public string To_Date { get; set; }
        public string Cover_Start_Date { get; set; }
        public string Cover_End_Date { get; set; }
        public int Cover_Period { get; set; }
        public decimal Total_Sum_Insured { get; set; }
        public string Risk_Description { get; set; }
    }
}
