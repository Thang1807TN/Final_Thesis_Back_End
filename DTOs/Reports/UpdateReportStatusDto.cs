using System.ComponentModel.DataAnnotations;

namespace SecondHandMarketplaceAPI.DTOs.Reports
{
    public class UpdateReportStatusDto
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Reviewed";
    }
}