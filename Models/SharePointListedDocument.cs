using System;

namespace MOZ_UPGRADE.Models
{
    public class SharePointListedDocument
    {
        public string Category { get; set; } = string.Empty;
        public string DocumentCode { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string WebUrl { get; set; } = string.Empty;
        public DateTimeOffset? LastModifiedDateTime { get; set; }
    }
}
