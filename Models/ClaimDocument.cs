namespace MOZ_UPGRADE.Models
{
    public class ClaimDocument
    {
        public string Document_Type { get; set; } = string.Empty;
        public int Entry_No { get; set; }
        public string Document_No { get; set; } = string.Empty;
        public string Customer_No { get; set; } = string.Empty;
        public string Document_Name { get; set; } = string.Empty;
        public string Document_Path { get; set; } = string.Empty;
        public bool Enclosed { get; set; }
        public string Class_Category { get; set; } = string.Empty;
        public bool To_Follow { get; set; }
        public bool Required { get; set; }
        public bool Received { get; set; }
        public DateTime Date_Required { get; set; }
        public DateTime? Date_Received { get; set; }
    }
}
