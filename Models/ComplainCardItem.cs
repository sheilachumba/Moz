using System;

namespace MOZ_UPGRADE.Models
{
    public class ComplainCardItem
    {
        public string Interact_Code { get; set; }
        public string Client_Type { get; set; }
        public string Client_No { get; set; }
        public string Client_Name { get; set; }
        public DateTime? Date_and_Time { get; set; }
        public DateTime? Last_Updated_Date_and_Time { get; set; }
        public string Interaction_Type { get; set; }
        public string Issue_Category { get; set; }
        public string Risk_Type { get; set; }
        public string Timeline { get; set; }
        public string Interaction_Resol_Desc { get; set; }
        public string Current_Status { get; set; }
        public DateTime? Escalation_Clock { get; set; }
        public decimal Overall_Level_Duration { get; set; }
        public string Reviewing_Officer_Remarks { get; set; }
        public string Reopening_Remarks { get; set; }
    }
}
