using System.Text.Json.Serialization;  // For controlling JSON property names during serialization

namespace Moz.Models  // Namespace where all models are defined
{
    // Represents an insurance product
    public class InsuranceProduct
    {
        [JsonPropertyName("@odata.etag")] // Maps the JSON '@odata.etag' property to this field
        public string OdataEtag { get; set; } = string.Empty;  // Stores the ETag value for concurrency

        public string Code { get; set; } = string.Empty;  // Product's unique code from the API

        public string Name { get; set; } = string.Empty;  // Product's name as provided by the API

        public string Product_Description { get; set; } = string.Empty;  // Additional description of the product

        public bool Show_Online { get; set; }  // Determines if the product should be shown online

        public string Class_Group { get; set; } = string.Empty;  // The product's classification group

        public string Regulator_Class { get; set; } = string.Empty;  // Regulatory class code from the API
    }

    // Wrapper for deserializing the full API response payload
    public class InsuranceProductResponse
    {
        public List<InsuranceProduct> Value { get; set; }  // List of insurance products from the API
    }
}
