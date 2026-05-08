using System.Collections.Generic;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IComplainService
    {
        Task<List<ComplainCardItem>> GetComplaintsAsync(string clientNo);
        Task<(bool Success, string? Error)> CreateComplaintAsync(ComplainCardItem item);
    }
}
