using System.Text.Json.Serialization;

namespace MOZ_UPGRADE.Models
{
    public class InsurerList
    {
        [JsonPropertyName("@odata.etag")]
        public string ODataETag { get; set; }

        [JsonPropertyName("No")]
        public string No { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Responsibility_Center")]
        public string ResponsibilityCenter { get; set; }

        [JsonPropertyName("Location_Code")]
        public string LocationCode { get; set; }

        [JsonPropertyName("Post_Code")]
        public string PostCode { get; set; }

        [JsonPropertyName("Country_Region_Code")]
        public string CountryRegionCode { get; set; }

        [JsonPropertyName("Phone_No")]
        public string PhoneNo { get; set; }

        [JsonPropertyName("Fax_No")]
        public string FaxNo { get; set; }

        [JsonPropertyName("Contact")]
        public string Contact { get; set; }

        [JsonPropertyName("Search_Name")]
        public string SearchName { get; set; }

        [JsonPropertyName("Last_Date_Modified")]
        public string LastDateModified { get; set; }
    }
}
