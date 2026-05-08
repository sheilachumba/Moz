using System;
using System.Collections.Generic;

namespace MOZ_UPGRADE.Models
{
    public class ReportStats
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int Pending { get; set; }
        public int Open { get; set; }
        public int Closed { get; set; }
        public int Converted { get; set; }
        public int Expired { get; set; }
        public int ThisMonth { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ThisMonthAmount { get; set; }
    }

    public class ReportColumn
    {
        public string Property { get; set; }
        public string Header { get; set; }
        public string Format { get; set; }
    }

    public class PolicyReportItem
    {
        public string PolicyNumber { get; set; }
        public string PolicyType { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Premium { get; set; }
        public decimal SumInsured { get; set; }
    }

    public class PaymentReportItem
    {
        public string ReceiptNo { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
    }

    public class ReceiptReportItem
    {
        public string ReceiptNo { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
    }

    public class ClaimReportItem
    {
        public string ClaimNo { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime DateReported { get; set; }
        public string ClaimType { get; set; }
        public string Status { get; set; }
        public decimal AmountClaimed { get; set; }
        public decimal AmountPaid { get; set; }
    }

    public class QuoteReportItem
    {
        public string QuoteNo { get; set; }
        public string PolicyType { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Status { get; set; }
        public decimal Premium { get; set; }
        public decimal SumInsured { get; set; }
    }
}
