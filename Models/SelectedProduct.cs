using System;
using System.ComponentModel.DataAnnotations;

namespace MOZ_UPGRADE.Models
{
    public class SelectedProduct
    {
        public string? ODataEtag { get; set; } // "@odata.etag"
        public int? No { get; set; }
        public string? Contact_No { get; set; }

        [Required]
        public string Policy_No { get; set; }

        public string? Class_Name { get; set; }
        public string? Description { get; set; }
        public string? Underwriter_Code { get; set; }

        [Required]
        public DateTime Policy_Start_Date { get; set; } = DateTime.Now;

        [Required]
        public DateTime Policy_End_Date { get; set; } = DateTime.Now;

        [Required]
        public int Policy_Term { get; set; } = 1;

        [Required]
        public string? First_Name { get; set; }

        [Required]
        public string? Family_Name { get; set; }

        [Required]
        public string? Relationship_to_Applicant { get; set; }

        [Required]
        public int Age { get; set; }

        public string? Chronic_Status { get; set; }

        [Required]
        public string Occupation { get; set; }

        public string? Session_ID { get; set; }
        public bool? Selected { get; set; }
    }
}
