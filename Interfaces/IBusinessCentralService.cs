using MOZ_UPGRADE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IBusinessCentralService
    {
        Task<IEnumerable<ClaimDocument>> GetClaimDocumentsAsync();
        Task<IEnumerable<TermListItem>> GetTermListAsync();
    }
}
