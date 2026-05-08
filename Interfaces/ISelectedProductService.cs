using System.Threading;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ISelectedProductService
    {
        Task<(bool Success, string? Error, string? ResponseBody)> CreateSelectedProductAsync(SelectedProduct product, CancellationToken cancellationToken = default);
    }
}
