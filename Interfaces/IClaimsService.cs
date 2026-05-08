using System.Collections.Generic;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IClaimsService
    {
        Task<List<LossTypeItem>> GetLossTypesAsync();
        Task<string?> CreateClaimAsync(ClaimCreateRequest request);
        Task<List<ClaimListItem>> GetClaimsAsync(string insuredNo = null);
        Task<List<ClaimDocument>> GetClaimDocumentsByCategoryAsync(string category);
    }
}
