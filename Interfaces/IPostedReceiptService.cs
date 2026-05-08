using System.Collections.Generic;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IPostedReceiptService
    {
        Task<List<PostedReceiptItem>> GetPostedReceiptsByAccountAsync(string accountNo);
    }
}
