using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class CountryRegion
    {
        [JsonPropertyName("Code")]
        public string Code { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("ISO_Code")]
        public string IsoCode { get; set; }

        [JsonPropertyName("ISO_Numeric_Code")]
        public string IsoNumericCode { get; set; }

        [JsonPropertyName("Address_Format")]
        public string AddressFormat { get; set; }

        [JsonPropertyName("Contact_Address_Format")]
        public string ContactAddressFormat { get; set; }

        [JsonPropertyName("County_Name")]
        public string CountyName { get; set; }

        [JsonPropertyName("EU_Country_Region_Code")]
        public string EuCountryRegionCode { get; set; }

        [JsonPropertyName("Intrastat_Code")]
        public string IntrastatCode { get; set; }

        [JsonPropertyName("VAT_Scheme")]
        public string VatScheme { get; set; }

        [JsonPropertyName("Phone_Prefix")]
        public string PhonePrefix { get; set; }

        [JsonPropertyName("Number_of_Digits")]
        public int NumberOfDigits { get; set; }

        [JsonPropertyName("Nationality")]
        public string Nationality { get; set; }

        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }
    }

    public class CountryRegionResponse
    {
        [JsonPropertyName("value")]
        public List<CountryRegion> Value { get; set; } = new List<CountryRegion>();
    }
}
