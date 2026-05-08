using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class SourceOfIncome
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }
    }

    public class SourceOfIncomeResponse
    {
        [JsonPropertyName("value")]
        public List<SourceOfIncome> Value { get; set; } = new List<SourceOfIncome>();
    }
}
