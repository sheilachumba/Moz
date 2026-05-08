using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class Salutation
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }
    }

    public class SalutationResponse
    {
        [JsonPropertyName("value")]
        public List<Salutation> Value { get; set; } = new List<Salutation>();
    }
}
