using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class PostalCode
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("City")]
        public string City { get; set; }

        [JsonPropertyName("Country_Region_Code")]
        public string CountryRegionCode { get; set; }

        [JsonPropertyName("County")]
        public string County { get; set; }

        [JsonPropertyName("State")]
        public string? State { get; set; }

        [JsonPropertyName("TimeZone")]
        public string TimeZone { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }
    }

    public class PostalCodeResponse
    {
        [JsonPropertyName("value")]
        public List<PostalCode> Value { get; set; } = new List<PostalCode>();
    }
}
