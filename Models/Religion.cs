using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class Religion
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }
    }

    public class ReligionResponse
    {
        [JsonPropertyName("value")]
        public List<Religion> Value { get; set; } = new List<Religion>();
    }
}
