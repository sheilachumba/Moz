using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class TermListItem
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("Term")]
        public string Term { get; set; }

        [JsonPropertyName("No_Of_Periods")]
        public int No_Of_Periods { get; set; }
    }
}
