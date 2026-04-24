using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.Models
{
    public class Report
    {
        public int Id { get; set; }

        public string ReporterId { get; set; } = string.Empty;
        public ApplicationUser? Reporter { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public string? ReportedUserId { get; set; }
        public ApplicationUser? ReportedUser { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}