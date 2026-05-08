using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class ProductClass
    {
        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }

        [JsonPropertyName("Product_Class_No")]
        public string ProductClassNo { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Product_Description")]
        public string ProductDescription { get; set; }

        // Updated to match new OData field name
        [JsonPropertyName("ShowOnline")]
        public bool ShowOnline { get; set; }

        [JsonPropertyName("Schedule_Line")]
        public string ScheduleLine { get; set; }

        [JsonPropertyName("Class_Group")]
        public string ClassGroup { get; set; }

        [JsonPropertyName("Regulator_Class")]
        public string RegulatorClass { get; set; }

        [JsonPropertyName("Renewable")]
        public bool Renewable { get; set; }

        // New field from updated endpoint
        [JsonPropertyName("Description1")]
        public string Description1 { get; set; }
    }
}
