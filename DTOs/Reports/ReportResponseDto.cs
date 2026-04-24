namespace SecondHandMarketplaceAPI.DTOs.Reports
{
    public class ReportResponseDto
    {
        public int Id { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ProductTitle { get; set; } = string.Empty;
        public string ReportedUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}