using System.Collections.Generic;
using System.Threading.Tasks;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE.Interfaces
{
    public interface ISharePointDocumentService
    {
        Task<string> UploadKycDocumentAsync(string category, string customerOrContactNo, string documentCode, byte[] fileBytes, string originalFileName);
        Task<IReadOnlyList<SharePointListedDocument>> ListKycDocumentsAsync(string category, string customerOrContactNo);
    }
}
