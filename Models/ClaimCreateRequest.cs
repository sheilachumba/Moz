using System;

namespace MOZ_UPGRADE.Models
{
    public class ClaimCreateRequest
    {
        public string Policy_No { get; set; }
        public string Loss_Type { get; set; }
        public string Loss_Type_Description { get; set; }
        public string Date_Notified { get; set; }
        public string Claim_Loss_Description { get; set; }
        public string Date_of_Occurence { get; set; }
    }
}
