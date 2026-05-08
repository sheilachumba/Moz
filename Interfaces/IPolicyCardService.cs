using System.Threading.Tasks;
using System.Collections.Generic;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IPolicyCardService
    {
        Task<List<MOZ_UPGRADE.Models.PolicyCardItem>> GetPoliciesByQuotationAsync(string quotationNo);
        Task<List<MOZ_UPGRADE.Models.PolicyCardItem>> GetPoliciesByInsuredNoAsync(string insuredNo);
        Task<(bool Success, string ErrorMessage)> SubmitPolicyRenewalAsync(MOZ_UPGRADE.Models.PolicyCardItem policy);
    }
}
