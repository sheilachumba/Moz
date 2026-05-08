using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class MeansOfIdentification
    {
        [JsonPropertyName("Means_of_ID")]
        public string MeansOfId { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("Account_Type")]
        public string AccountType { get; set; }

        [JsonPropertyName("Expiration_Date_Required")]
        public bool ExpirationDateRequired { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }
    }

    public class MeansOfIdentificationResponse
    {
        [JsonPropertyName("value")]
        public List<MeansOfIdentification> Value { get; set; } = new List<MeansOfIdentification>();
    }
}
