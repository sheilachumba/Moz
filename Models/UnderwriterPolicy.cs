using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class UnderwriterPolicy
    {
        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }

        [JsonPropertyName("No")]
        public int No { get; set; }

        [JsonPropertyName("Insurer")]
        public string Insurer { get; set; }

        [JsonPropertyName("Plans")]
        public string Plans { get; set; }

        [JsonPropertyName("Policy_Plan")]
        public string PolicyPlan { get; set; }
    }

    public class UnderwriterPolicyViewModel
    {
        public int No { get; set; }
        public string Insurer { get; set; }
        public string InsurerName { get; set; }
        public string Plans { get; set; }
        public string ProductClassName { get; set; }
        public string PolicyPlan { get; set; }
    }
}
