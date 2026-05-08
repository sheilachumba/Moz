namespace MOZ_UPGRADE.Models
{
    public class Plan
    {
        public string Undewriter_Product_Nos { get; set; }
        public string Description { get; set; }
        public string Underwriter_Code { get; set; }
        public string Regulator_Class { get; set; }
        public string Class_Name { get; set; }
        public string Class { get; set; }
        public string Sub_Class { get; set; }
        public string SubClass_Name { get; set; }
        public string Short_Code { get; set; }
        public string Short_Name { get; set; }
        public bool Non_Renewable { get; set; }
        public string Schedule_Line { get; set; }
        public string Premium_Calculation { get; set; }
        public string Default_Term { get; set; }
        public decimal Default_Premium_Payment { get; set; }
        public string Default_Area_Code { get; set; }
        public string Default_Excess { get; set; }
        public string Premium_Table { get; set; }
        public string Account_Type { get; set; }
        public string Commission_Income_Account_No { get; set; }
        public string Commission_Expense_Account { get; set; }
        public string Deferred_Commission_Income_A_C { get; set; }
        public string Deferred_Commission_Expense_A_C { get; set; }
        public decimal Commision_Percent_age { get; set; }
        public decimal Commission_Percentage_Agent { get; set; }
        public string VAT_Product_Group { get; set; }
        public string Period { get; set; }
        public string Start_Time { get; set; }
        public string End_Time { get; set; }
        public decimal First_Loss_Percent { get; set; }
        public bool Open_Cover { get; set; }
        public string Type { get; set; }
        public string Rating { get; set; }
        public decimal Premium_Rate { get; set; }
        public string Conveyance { get; set; }
        public string Claims_Validity_Period { get; set; }
        public string Certificate_Type { get; set; }
        public string Certificate_Type_Bus { get; set; }
        public decimal Bus_Seating_Capacity_Cut_off { get; set; }
        public decimal PPL_Cost_Per_PAX { get; set; }
        public bool Comprehensive { get; set; }
        public string Last_Policy_No { get; set; }
        public string Last_Endorsement_No { get; set; }
        public string Treaty_Code { get; set; }
        public decimal Addendum_No { get; set; }
        public string Premium_Payment_Options { get; set; }
        public string Commission_calculation_basis { get; set; }
        public bool Partial_maturity_x003F_ { get; set; }
    }

    public class PlanViewModel
    {
        public string Undewriter_Product_Nos { get; set; }
        public string Description { get; set; }
        public string Underwriter_Code { get; set; }
        public string Class { get; set; }
        public string Class_Name { get; set; }
        public string Sub_Class { get; set; }
        public string SubClass_Name { get; set; }
        public string Short_Code { get; set; }
        public string Short_Name { get; set; }
        public decimal Commision_Percent_age { get; set; }
        public decimal Premium_Rate { get; set; }
        public string Premium_Calculation { get; set; }
        public string Account_Type { get; set; }
        public string Schedule_Line { get; set; }
    }
}
