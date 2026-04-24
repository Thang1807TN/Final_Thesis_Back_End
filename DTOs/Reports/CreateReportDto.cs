using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Reports
{
    public class CreateReportDto
    {
        public int? ProductId { get; set; }
        public string? ReportedUserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
    }
}