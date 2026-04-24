namespace SecondHandMarketplaceAPI.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }

        public bool IsActive { get; set; } = true;
        public int UsageLimit { get; set; } = 0;
        public int UsedCount { get; set; } = 0;

        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}