using System.Collections.Generic;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IDebitNoteService
    {
        Task<List<DebitNoteItem>> GetDebitNotesByQuotationAsync(string quotationNo);
        Task<List<DebitNoteItem>> GetDebitNotesByInsuredNoAsync(string insuredNo);
        Task<(bool Success, string? Error)> CreateDebitNotePaymentAsync(DebitNoteItem note, decimal amountPaid, string modeOfPayment, string paymentReferenceNo, DateTime paymentDate);
        Task<(bool HasLedger, decimal TaxAmount, decimal LedgerAmount)> GetPolicyPaymentAmountsAsync(string policyNo);
        Task<(bool Success, string? Error)> UpdateDebitNoteStatusAsync(string debitNoteNo, string status);
    }
}
