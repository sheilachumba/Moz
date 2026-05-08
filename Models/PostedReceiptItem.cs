namespace MOZ_UPGRADE.Models
{
    public class PostedReceiptItem
    {
        public string No { get; set; }
        public string Receipt_No { get; set; }
        public string Posting_Date { get; set; }
        public string Pay_Mode { get; set; }
        public string Cheque_No { get; set; }
        public string Received_From { get; set; }
        public string On_Behalf_Of { get; set; }
        public string Cheque_Date { get; set; }
        public string Currency_Code { get; set; }
        public string Bank_Code { get; set; }
        public string Cashier { get; set; }
        public bool Posted { get; set; }
        public string Posted_Date { get; set; }
        public string Posted_Time { get; set; }
        public string Posted_By { get; set; }
        public decimal Total_Amount { get; set; }
        public decimal Amount_LCY { get; set; }
        public string Cancellation_Reason { get; set; }
        public string Account_No { get; set; }
    }
}
