using System.Text.Json.Serialization;

namespace Moz.Models
{
    public class Bank
    {
        [JsonPropertyName("@odata.etag")]
        public string OdataEtag { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

    }
    public class BankResponse
    {
        public List<Bank> Value { get; set; }
    }
}
