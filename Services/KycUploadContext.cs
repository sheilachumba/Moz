using System.Collections.Generic;

namespace MOZ_UPGRADE.Services
{
    public class KycUploadContext
    {
        public Dictionary<string, string> IndividualDocumentUrlsByCode { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> CorporateDocumentUrlsByCode { get; } = new Dictionary<string, string>();

        public void Clear()
        {
            IndividualDocumentUrlsByCode.Clear();
            CorporateDocumentUrlsByCode.Clear();
        }
    }
}
